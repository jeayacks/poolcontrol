//-----------------------------------------------------------------------
// <copyright file="PumpCycle.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control.Store
{
    using System;

    /// <summary>
    /// Defines a pump cycle
    /// </summary>
    public class PumpCycle : Cycle
    {
        public PumpCycle(DateTime startTime, DateTime endTime, bool chlorineInhibition, bool phRegulationInhibition)
            : base(startTime, endTime)
        {
            this.ChlorineInhibition = chlorineInhibition;
            this.PhRegulationInhibition = phRegulationInhibition;
        }

        /// <summary>
        /// True to stop chlorine
        /// </summary>
        public bool ChlorineInhibition { get; }

        /// <summary>
        /// True to stop PH
        /// </summary>
        public bool PhRegulationInhibition { get; }
    }
}