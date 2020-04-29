//-----------------------------------------------------------------------
// <copyright file="IHardwareManager.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Hardware
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Hardware manager in charge of interacting with the physical hardware
    /// </summary>
    public interface IHardwareManager
    {
        /// <summary>
        /// Raise each time a input changed.
        /// </summary>
        event EventHandler<BooleanInputChangeEventArgs> BooleanInputChanged;

        /// <summary>
        /// Initialize the physical driver.
        /// </summary>
        void CloseConfiguration();

        /// <summary>
        /// Free the physical driver.
        /// </summary>
        void OpenConfiguration();

        /// <summary>
        /// Change output value.
        /// </summary>
        /// <param name="output">The logical output name.</param>
        /// <param name="value">The value.</param>
        void Write(PinName output, bool value);

        /// <summary>
        /// Read output states
        /// </summary>
        /// <param name="pinName">Pin to read</param>
        /// <returns>The output status</returns>
        HardwareOutputState GetOutput(PinName pinName);

        /// <summary>
        /// Read output states
        /// </summary>
        /// <returns>The output status</returns>
        IReadOnlyList<HardwareOutputState> GetOutputs();

        /// <summary>
        /// Read temperature value
        /// </summary>
        /// <param name="sensor">Device identifier</param>
        /// <returns>Temperature value in degrees</returns>
        double ReadTemperatureValue(TemperatureSensorName sensor);
    }
}