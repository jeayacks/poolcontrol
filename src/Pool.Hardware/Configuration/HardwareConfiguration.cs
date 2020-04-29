//-----------------------------------------------------------------------
// <copyright file="HardwareConfiguration.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pool.Hardware
{
    /// <summary>
    /// Store the hardware configuration.
    /// </summary>
    public class HardwareConfiguration
    {
        public HardwareConfiguration()
        {
            this.Pins = new Collection<HardwarePinConfiguration>();
            this.TemperatureSensors = new Collection<HardwareTemperatureSensorConfiguration>();
        }

        /// <summary>
        /// Gets or sets the pins.
        /// </summary>
        public Collection<HardwarePinConfiguration> Pins { get; set; }


        /// <summary>
        /// Gets or sets the pins.
        /// </summary>
        public Collection<HardwareTemperatureSensorConfiguration> TemperatureSensors { get; set; }
    }
}
