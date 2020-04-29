//-----------------------------------------------------------------------
// <copyright file="HardwareTemperatureSensorConfiguration.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;

namespace Pool.Hardware
{
    /// <summary>
    /// Pin configuration.
    /// </summary>
    public class HardwareTemperatureSensorConfiguration
    {
        public HardwareTemperatureSensorConfiguration()
        {
        }

        public HardwareTemperatureSensorConfiguration(string name, string deviceId)
        {
            this.Name = name;
            this.DeviceId = deviceId;
        }

        /// <summary>
        /// Gets or sets the output logical name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the device UID.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the descrition
        /// </summary>
        public string Description { get; set; }
    }
}
