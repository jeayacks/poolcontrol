//-----------------------------------------------------------------------
// <copyright file="FrostProtectionSetting.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control.Store
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Frost protection settings
    /// </summary>
    public class FrostProtectionSetting
    {
        public FrostProtectionSetting()
        {
            this.TemperatureActivation = 7;
            this.RecyclingDurationMinutes = 15;
        }

        /// <summary>
        /// Gets or sets the temperature threshold.
        /// </summary>
        public double TemperatureActivation { get; set; }

        /// <summary>
        /// Gets or sets the recycling duration of the pump.
        /// </summary>
        public double RecyclingDurationMinutes { get; set; }
    }
}