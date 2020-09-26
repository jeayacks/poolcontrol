//-----------------------------------------------------------------------
// <copyright file="PoolControlPump.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Pool.Control.Store;
    using Pool.Hardware;

    /// <summary>
    /// Manages the pump functionning time
    /// and acquire temperatures.
    /// </summary>
    public class PoolControlPump
    {
        /// <summary>
        /// Interval to read temperature.
        /// </summary>
        public const int TemperatureReadingInterval = 60;

        /// <summary>
        /// Interval to read temperature.
        /// </summary>
        public const int PumpRunTimeToAcquirePoolTemperature = 900;

        /// <summary>
        /// Delay used to reset forcing states
        /// </summary>
        private readonly TimeSpan resetForcingStatesDelay = TimeSpan.FromDays(1.5);

        /// <summary>
        /// The general settings
        /// </summary>
        private PoolSettings poolSettings;

        /// <summary>
        /// The current states
        /// </summary>
        private SystemState systemState;

        /// <summary>
        /// Hardware layer.
        /// </summary>
        private IHardwareManager hardwareManager;

        /// <summary>
        /// Current pump cycle if pump is running.
        /// </summary>
        private PumpCycle pumpCycle;

        /// <summary>
        /// Current pump cycle if pump is running.
        /// </summary>
        private List<PumpCycle> nextCycles = new List<PumpCycle>();

        /// <summary>
        /// The current pump status.
        /// </summary>
        private bool currentPumpStatus = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PoolControlPump"/> class.
        /// </summary>
        public PoolControlPump(
            PoolSettings poolSettings,
            SystemState systemState,
            IHardwareManager hardwareManager)
        {
            this.poolSettings = poolSettings;
            this.systemState = systemState;
            this.hardwareManager = hardwareManager;
        }

        /// <summary>
        /// Process temperatures and the pump
        /// </summary>
        public void Process()
        {
            this.ResetForcingStates();

            this.TemperatureAcquisition();

            bool pumpOn;

            // Process forcing options first
            if (this.systemState.PumpForceOff.Value)
            {
                pumpOn = false;
            }
            else if (this.systemState.PumpForceOn.Value)
            {
                pumpOn = true;
            }
            else
            {
                if (this.pumpCycle != null)
                {
                    // Pump must be on, reassign the value after forcing change
                    pumpOn = true;

                    if (this.pumpCycle.EndTime < SystemTime.Now)
                    {
                        // Stop the pump
                        pumpOn = false;
                        this.pumpCycle = null;

                        // Update next cycles
                        this.UpdateNextPumpCycles();
                    }
                }
                else
                {
                    // Pump must be off, reassign the value after forcing change
                    pumpOn = false;
                }

                // Now check if a cycle must be started
                // New cycle could start just after previous cycle
                if (pumpOn == false)
                {
                    // This should not append because cycles are updated when daily temperature is updated
                    // Except when the system starts
                    if (this.nextCycles.Count < 3)
                    {
                        this.UpdateNextPumpCycles();
                    }

                    // Check next execution
                    var list = this.nextCycles.ToList();
                    foreach (var cycle in list)
                    {
                        if (cycle.EndTime <= SystemTime.Now)
                        {
                            // Expired cycle, remove it (this is should not append)
                            this.nextCycles.Remove(cycle);
                        }
                        else if (cycle.StartTime <= SystemTime.Now)
                        {
                            // Start a new cycle
                            this.nextCycles.Remove(cycle);

                            this.pumpCycle = cycle;
                            pumpOn = true;
                            break;
                        }
                    }
                }

                // Winter mode, frost protection
                if (this.poolSettings.WorkingMode == PoolWorkingMode.Winter && pumpOn == false)
                {
                    // If water temperature of the pipe < 7°, inject a new cycle
                    if (this.systemState.WaterTemperature.Value < this.poolSettings.FrostProtection.TemperatureActivation)
                    {
                        // Add pumping cycle of 15 minutes
                        this.pumpCycle = new PumpCycle(
                            SystemTime.Now,
                            SystemTime.Now.AddMinutes(this.poolSettings.FrostProtection.RecyclingDurationMinutes),
                            false,
                            false);

                        pumpOn = true;
                    }
                }
            }

            if (pumpOn != this.currentPumpStatus)
            {
                this.SwitchPump(pumpOn);
            }
        }

        /// <summary>
        /// Gets all cycles including current.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PumpCycle> GetCyclesInfo()
        {
            var list = this.nextCycles.ToList();
            if (this.pumpCycle != null)
            {
                list.Insert(0, this.pumpCycle);
            }

            return list;
        }

        /// <summary>
        /// Used to reset the precalculated cycles
        /// </summary>
        public void ResetSettings(PoolSettings poolSettings)
        {
            this.poolSettings = poolSettings;

            this.nextCycles.Clear();
        }

        /// <summary>
        /// Reset pump forcing modes
        /// </summary>
        private void ResetForcingStates()
        {
            var time = SystemTime.Now.Subtract(this.resetForcingStatesDelay);

            if (this.systemState.PumpForceOff.Value && this.systemState.PumpForceOff.Time < time)
            {
                this.systemState.PumpForceOff.UpdateValue(false);
            }

            if (this.systemState.PumpForceOn.Value && this.systemState.PumpForceOn.Time < time)
            {
                this.systemState.PumpForceOn.UpdateValue(false);
            }
        }

        /// <summary>
        /// Acquire temperatures
        /// </summary>
        /// <returns>True if state has changed.</returns>
        private void TemperatureAcquisition()
        {
            // Exterior temperature
            if (this.systemState.AirTemperature.Time.AddSeconds(TemperatureReadingInterval) <= SystemTime.Now)
            {
                this.systemState.AirTemperature.UpdateValue(hardwareManager.ReadTemperatureValue(TemperatureSensorName.AirTemperature));
            }

            // Water temperature
            if (this.systemState.WaterTemperature.Time.AddSeconds(TemperatureReadingInterval) <= SystemTime.Now)
            {
                var temperature = hardwareManager.ReadTemperatureValue(TemperatureSensorName.WaterTemperature);
                this.systemState.WaterTemperature.UpdateValue(temperature);

                // if pump is running for more than the required functionning time, copy the temperature
                if (this.systemState.Pump.Value == true && this.systemState.Pump.Time.AddSeconds(PumpRunTimeToAcquirePoolTemperature) <= SystemTime.Now)
                {
                    this.systemState.PoolTemperature.UpdateValue(temperature);

                    // Update the min of the day
                    if (temperature < this.systemState.PoolTemperatureMinOfTheDay.Value || this.systemState.PoolTemperatureMinOfTheDay.Value <= 0)
                    {
                        // Change the value but not the time
                        this.systemState.PoolTemperatureMinOfTheDay.UpdateValueOnly(temperature);
                    }

                    // Update the max of the day
                    if (temperature > this.systemState.PoolTemperatureMaxOfTheDay.Value)
                    {
                        // Change the value but not the time
                        this.systemState.PoolTemperatureMaxOfTheDay.UpdateValueOnly(temperature);
                    }
                }
            }

            // Copy the pool max temperature to "decision temperature" once a day at midnight
            if (this.systemState.PoolTemperatureMaxOfTheDay.Time.Date < SystemTime.Now.Date)
            {
                // Check value (second security if temperature not assigned)
                if (this.systemState.PoolTemperatureMinOfTheDay.Value > 0
                    && this.systemState.PoolTemperatureMinOfTheDay.Value < 40
                    && this.systemState.PoolTemperatureMaxOfTheDay.Value > 0
                    && this.systemState.PoolTemperatureMaxOfTheDay.Value < 40)
                {
                    double decision = (this.systemState.PoolTemperatureMaxOfTheDay.Value + this.systemState.PoolTemperatureMinOfTheDay.Value) / 2;
                    decision = Math.Round(decision, 1);
                    this.systemState.PoolTemperatureDecision.UpdateValue(decision);

                    var pumpingDuration = this.poolSettings.GetHoursPumpingTimePerDay(decision);
                    this.systemState.PumpingDurationPerDayInHours.UpdateValue(pumpingDuration);

                    // Reset min and max temperature
                    this.systemState.PoolTemperatureMinOfTheDay = new SampleValue<double>(SystemTime.Now.Date, this.systemState.PoolTemperature.Value);
                    this.systemState.PoolTemperatureMaxOfTheDay = new SampleValue<double>(SystemTime.Now.Date, this.systemState.PoolTemperature.Value);

                    // Calculate cycles
                    this.UpdateNextPumpCycles();
                }
            }
        }

        private void UpdateNextPumpCycles()
        {
            var temperature = this.systemState.PoolTemperatureDecision.Value;
            var durationPerDay = TimeSpan.FromHours(this.systemState.PumpingDurationPerDayInHours.Value);

            this.nextCycles = this.poolSettings
                .GetNextPumpCycles(SystemTime.Now, temperature, durationPerDay, 3)
                .ToList();
        }

        private void SwitchPump(bool state)
        {
            if (state)
            {
                // Change inihbition first 
                // Next cycle can be null when manual forcing
                if (this.pumpCycle != null)
                {
                    this.hardwareManager.Write(PinName.ChlorineInhibition, this.pumpCycle.ChlorineInhibition);
                    this.hardwareManager.Write(PinName.PhRegulationInhibition, this.pumpCycle.PhRegulationInhibition);
                    Thread.Sleep(200);
                }

                // Switch on pumps
                this.hardwareManager.Write(PinName.Pump, state);
            }
            else
            {
                // Stop first, then release inhibitions
                this.hardwareManager.Write(PinName.Pump, false);

                Thread.Sleep(200);
                this.hardwareManager.Write(PinName.ChlorineInhibition, false);
                this.hardwareManager.Write(PinName.PhRegulationInhibition, false);
            }

            this.currentPumpStatus = state;

            this.systemState.Pump.UpdateValue(state);
        }

    }
}