//-----------------------------------------------------------------------
// <copyright file="PumpCycleSetting.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control.Store
{
    using System;

    /// <summary>
    /// Defines a pump cycle
    /// </summary>
    public class PumpCycleSetting
    {
        /// <summary>
        /// Gets or sets the time of start or end cycle
        /// </summary>
        public TimeSpan DecisionTime { get; set; }

        /// <summary>
        /// Gets or sets the type of cycle
        /// </summary>
        public PumpCycleType PumpCycleType { get; set; }

        /// <summary>
        /// True to stop chlorine
        /// </summary>
        public bool ChlorineInhibition { get; set; }

        /// <summary>
        /// True to stop PH
        /// </summary>
        public bool PhRegulationInhibition { get; set; }
    }
}
