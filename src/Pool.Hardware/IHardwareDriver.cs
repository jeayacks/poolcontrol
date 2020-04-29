//-----------------------------------------------------------------------
// <copyright file="IHardwareDriver.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Hardware
{
using System;
using System.Collections.Generic;

    /// <summary>
    /// Represents the inner driver used to read/write data.
    /// </summary>
    public interface IHardwareDriver
    {
        /// <summary>
        /// Open pins.
        /// </summary>
        /// <param name="configuration">The configuration</param>
        void OpenPins(HardwareConfiguration configuration);
        
        /// <summary>
        /// Close the pins.
        /// </summary>
        void ClosePins();

        /// <summary>
        /// Change output value.
        /// </summary>
        /// <param name="pin">The logical output name.</param>
        /// <param name="value">The value.</param>
        void Write(int pin, bool value);

        /// <summary>
        /// Read temperature value
        /// </summary>
        /// <param name="device">Device identifier</param>
        /// <returns>Temperature value in degrees</returns>
        double ReadTemperatureValue(string device);

        /// <summary>
        /// Raised when an input change.
        /// </summary>
        event EventHandler<HardwarePinChangeEventArgs> InputBooleanChanged;
    }
}
