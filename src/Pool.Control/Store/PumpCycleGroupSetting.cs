//-----------------------------------------------------------------------
// <copyright file="PumpCycleGroupSetting.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control.Store
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines a pump cycle
    /// </summary>
    public class PumpCycleGroupSetting
    {
        public PumpCycleGroupSetting()
        {
            this.MinimumTemperature = 0;
            this.PumpingCycles = new List<PumpCycleSetting>();
        }

        /// <summary>
        /// Gets or sets the minimum temperature to use this group
        /// </summary>
        public double MinimumTemperature { get; set; }

        /// <summary>
        /// Gets or sets the pump cycles
        /// </summary>
        public List<PumpCycleSetting> PumpingCycles { get; set; }
    }
}
