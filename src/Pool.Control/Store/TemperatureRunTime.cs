//-----------------------------------------------------------------------
// <copyright file="TemperatureRunTime.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control.Store
{
    /// <summary>
    /// Value used to determine daily pumping time
    /// </summary>
    public class TemperatureRunTime
    {
        /// <summary>
        /// Gets or sets the temperature
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// Gets or sets the pumping time
        /// </summary>
        public double RunTimeHours { get; set; }
    }
}
