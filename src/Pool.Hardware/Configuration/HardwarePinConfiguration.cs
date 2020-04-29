//-----------------------------------------------------------------------
// <copyright file="HardwarePinConfiguration.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;

namespace Pool.Hardware
{
    /// <summary>
    /// Pin configuration.
    /// </summary>
    public class HardwarePinConfiguration
    {
        public HardwarePinConfiguration()
        {
        }

        public HardwarePinConfiguration(string name, int logicalPin, HardwarePinConfigurationMode mode)
        {
            this.Name = name;
            this.LogicalPin = logicalPin;
            this.Mode = mode;
        }

        /// <summary>
        /// Gets or sets the output logical name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets corresponding logical pin.
        /// </summary>
        public int LogicalPin { get; set; }

        /// <summary>
        /// Gets or sets corresponding the pin mode.
        /// </summary>
        public HardwarePinConfigurationMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the descrition
        /// </summary>
        public string Description { get; set; }
    }
}
