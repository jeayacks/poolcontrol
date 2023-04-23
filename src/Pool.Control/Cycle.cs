//-----------------------------------------------------------------------
// <copyright file="PumpCycle.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control.Store
{
    using System;

    public class Cycle
    {
        public Cycle(DateTime startTime, DateTime endTime) =>
            (StartTime, EndTime) = (startTime, endTime);

        /// <summary>
        /// Gets the time of start or end cycle
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// Gets the time of end or end cycle
        /// </summary>
        public DateTime EndTime { get; }
    }
}