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
    public class PumpCycle
    {
        public PumpCycle(DateTime startTime, DateTime endTime, bool chlorineInhibition, bool phRegulationInhibition) =>
            (StartTime, EndTime, ChlorineInhibition, PhRegulationInhibition) = (startTime, endTime, chlorineInhibition, phRegulationInhibition);

        /// <summary>
        /// Gets the time of start or end cycle
        /// </summary>
        public DateTime StartTime { get;  }

        /// <summary>
        /// Gets the time of end or end cycle
        /// </summary>
        public DateTime EndTime { get;  }

        /// <summary>
        /// True to stop chlorine
        /// </summary>
        public bool ChlorineInhibition { get; }

        /// <summary>
        /// True to stop PH
        /// </summary>
        public bool PhRegulationInhibition { get;  }
    }
}
