//-----------------------------------------------------------------------
// <copyright file="SystemState.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>
    /// Store the current states.
    /// </summary>
    public class SystemState
    {
        public SystemState()
        {
            var time = new DateTime(2000, 1, 1);

            this.WaterTemperature = new SampleValue<double>(time, 23);
            this.PoolTemperature = new SampleValue<double>(time, 23);
            this.AirTemperature = new SampleValue<double>(time, 23);
            this.PoolTemperatureDecision = new SampleValue<double>(time, 23);
            this.PoolTemperatureMaxOfTheDay = new SampleValue<double>(time, 0);
            this.PumpingDurationPerDayInHours = new SampleValue<double>(time, 1);
            this.Pump = new SampleValue<bool>(time, false);
            this.PumpForceOff = new SampleValue<bool>(time, false);
            this.PumpForceOn = new SampleValue<bool>(time, false);
        }

        /// <summary>
        /// Water temperature without any filtering, even when pump is stopped
        /// </summary>
        public SampleValue<double> WaterTemperature { get; set; }

        /// <summary>
        /// The current temperature of the pool, value is updated only if pump is running
        /// </summary>
        public SampleValue<double> PoolTemperature { get; set; }

        /// <summary>
        /// Exterior temperature
        /// </summary>
        public SampleValue<double> AirTemperature { get; set; }

        /// <summary>
        /// Temperature value used to calculate running time
        /// </summary>
        public SampleValue<double> PoolTemperatureDecision { get; set; }

        /// <summary>
        /// Temperature value used to calculate running time
        /// </summary>
        public SampleValue<double> PoolTemperatureMaxOfTheDay { get; set; }

        /// <summary>
        /// Pumping time per day
        /// </summary>
        public SampleValue<double> PumpingDurationPerDayInHours { get; set; }

        /// <summary>
        /// The pump state
        /// </summary>
        public SampleValue<bool> Pump { get; set; }

        /// <summary>
        /// The pump state
        /// </summary>
        public SampleValue<bool> PumpForceOff { get; set; }

        /// <summary>
        /// The pump state
        /// </summary>
        public SampleValue<bool> PumpForceOn { get; set; }
    }
}
