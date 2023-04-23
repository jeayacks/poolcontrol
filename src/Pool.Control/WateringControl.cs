//-----------------------------------------------------------------------
// <copyright file="WateringControl.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control
{
    using System;
    using System.Collections.Generic;

    using Pool.Control.Store;
    using Pool.Hardware;

    /// <summary>
    /// Control the cover.
    /// </summary>
    public class WateringControl
    {
        /// <summary>
        /// Delay used to reset to off
        /// </summary>
        private readonly TimeSpan resetForcingStatesDelay = TimeSpan.FromHours(1);

        private PoolSettings poolSettings;

        private SystemState systemState;

        private IHardwareManager hardwareManager;

        private TimeSpan schedule;

        private DateTime nextSchedule;
        private DateTime? lastSwitchOnTime;
        private DateTime? nextSwitchOffTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="PoolControlCover"/> class.
        /// </summary>
        public WateringControl(
            PoolSettings poolSettings,
            SystemState systemState,
            IHardwareManager hardwareManager)
        {
            this.poolSettings = poolSettings;
            this.systemState = systemState;
            this.hardwareManager = hardwareManager;

            this.InitializeNextCycle();
        }

        public DateTime NextSchedule => this.nextSchedule;

        public DateTime? NextSwitchOffTime => this.nextSwitchOffTime;

        public bool AutomaticWatering { get; private set; }

        public void Process()
        {
            var wateringOutput = this.hardwareManager.GetOutput(PinName.Watering);
            bool watering = wateringOutput.State;

            if (this.lastSwitchOnTime.HasValue && this.lastSwitchOnTime < SystemTime.Now.Subtract(this.resetForcingStatesDelay))
            {
                watering = false;
            }

            if (this.systemState.WateringScheduleEnabled.Value)
            {
                if (SystemTime.Now >= this.nextSchedule)
                {
                    watering = true;
                    this.lastSwitchOnTime = SystemTime.Now;
                    this.nextSwitchOffTime = SystemTime.Now.Add(this.GetValidDuration(this.systemState.WateringScheduleDuration.Value));
                    this.nextSchedule = SystemTime.Now.Date.AddDays(1).Add(this.schedule);
                }
            }

            if (this.systemState.WateringManualOn.Value && !watering)
            {
                watering = true;
                this.lastSwitchOnTime = SystemTime.Now;
                this.nextSwitchOffTime = SystemTime.Now.Add(this.GetValidDuration(this.systemState.WateringManualDuration.Value));
            }

            if (!this.systemState.WateringManualOn.Value && watering)
            {
                watering = false;
            }

            if (this.nextSwitchOffTime.HasValue && this.nextSwitchOffTime.Value < SystemTime.Now)
            {
                watering = false;
            }

            if (watering != wateringOutput.State)
            {
                this.hardwareManager.Write(PinName.Watering, watering);

                if (!watering)
                {
                    this.nextSwitchOffTime = null;
                    this.systemState.WateringManualOn.UpdateValue(false);
                }
            }
        }

        public void ResetSettings(PoolSettings poolSettings)
        {
            this.poolSettings = poolSettings;

            this.InitializeNextCycle();
        }

        public IEnumerable<Cycle> GetCyclesInfo()
        {
            if (this.nextSwitchOffTime.HasValue)
            {
                yield return new Cycle(this.nextSwitchOffTime.Value, this.nextSwitchOffTime.Value);
            }
            else if (this.systemState.WateringScheduleEnabled.Value)
            {
                yield return new Cycle(
                    this.nextSchedule,
                    this.nextSchedule.Add(this.GetValidDuration(this.systemState.WateringScheduleDuration.Value)));
            }
        }

        private void InitializeNextCycle()
        {
            this.schedule = this.poolSettings.WateringScheduleTime;

            if (SystemTime.Now.TimeOfDay > this.schedule)
            {
                this.nextSchedule = SystemTime.Now.Date.AddDays(1).Add(this.schedule);
            }
            else
            {
                this.nextSchedule = SystemTime.Now.Date.Add(this.schedule);
            }
        }

        private TimeSpan GetValidDuration(int durationInMinutes)
        {
            if (durationInMinutes > 0 && durationInMinutes < 60)
            {
                return TimeSpan.FromMinutes(durationInMinutes);
            }

            return TimeSpan.FromMinutes(5);
        }
    }
}