//-----------------------------------------------------------------------
// <copyright file="PoolControlPump.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control
{
    using Microsoft.Extensions.Logging;
    using Pool.Control.Store;
    using Pool.Hardware;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

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
        public const int PumpRunTimeToAcquirePoolTemperature = 300;

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
            else if (this.pumpCycle != null)
            {
                // Pump must be on, reassign the value after forcing change
                pumpOn = true;

                if (this.pumpCycle.EndTime < SystemTime.Now)
                {
                    // Stop the pump
                    pumpOn = false;
                    this.pumpCycle = null;

                    // Update next cycles
                    this.UpdateNextPumpCycles(false);
                }
            }
            else
            {
                // Pump must be off, reassign the value after forcing change
                pumpOn = false;

                // This should not append because cycles are updated when daily temperature is updated
                // Except when the system starts
                if (this.nextCycles.Count < 3)
                {
                    this.UpdateNextPumpCycles(true);
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
        public void ResetSettings()
        {
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

                    // Update the max of the day
                    if (temperature > this.systemState.PoolTemperatureMaxOfTheDay.Value)
                    {
                        // Change the value but not the time
                        this.systemState.PoolTemperatureMaxOfTheDay.UpdateValueOnly(this.systemState.PoolTemperature.Value);
                    }
                }
            }

            // Copy the pool max temperature to "decision temperature" once a day at midnight
            if (this.systemState.PoolTemperatureMaxOfTheDay.Time.Date < SystemTime.Now.Date)
            {
                // Check value (second security if temperature not assigned)
                if (this.systemState.PoolTemperatureMaxOfTheDay.Value > 0)
                {
                    double decistionTemperature = this.systemState.PoolTemperatureMaxOfTheDay.Value;
                    this.systemState.PoolTemperatureDecision.UpdateValue(decistionTemperature);

                    var pumpingDuration = this.poolSettings.GetHoursPumpingTimePerDay(decistionTemperature);
                    this.systemState.PumpingDurationPerDayInHours.UpdateValue(pumpingDuration);

                    // Reset max temperature
                    this.systemState.PoolTemperatureMaxOfTheDay = new SampleValue<double>(
                        SystemTime.Now.Date,
                        -1);

                    // Calculate cycles
                    this.UpdateNextPumpCycles(true);
                }
            }
        }

        private void UpdateNextPumpCycles(bool includeCurrent)
        {
            var temperature = this.systemState.PoolTemperatureDecision.Value;
            var durationPerDay = TimeSpan.FromHours(this.systemState.PumpingDurationPerDayInHours.Value);

            var results = this.poolSettings
                .GetNextPumpCycles(SystemTime.Now, temperature, durationPerDay, 3)
                .ToList();

            if (includeCurrent)
            {
                this.nextCycles = results;
            }
            else
            {
                this.nextCycles = results
                    .Where(d => d.StartTime >= SystemTime.Now)
                    .ToList();
            }
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