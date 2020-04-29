//-----------------------------------------------------------------------
// <copyright file="MockHardwareDriver.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Hardware
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Display diagnotics
    /// </summary>
    public class MockHardwareDriver : IHardwareDriver
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger<MockHardwareDriver> logger;

        public MockHardwareDriver(ILogger<MockHardwareDriver> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public event EventHandler<HardwarePinChangeEventArgs> InputBooleanChanged;

        /// <inheritdoc />
        public void OpenPins(HardwareConfiguration configuration)
        {
        }

        /// <inheritdoc />
        public void ClosePins()
        {
        }

        /// <inheritdoc />
        public void Write(int pin, bool value)
        {
            this.logger.LogInformation($"Write pin value {pin} = {(value ? "On" : "Off")}");
        }

        /// <inheritdoc />
        public double ReadTemperatureValue(string device)
        {
            return 27.8;
        }
    }
}