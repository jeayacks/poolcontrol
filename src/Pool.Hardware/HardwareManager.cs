//-----------------------------------------------------------------------
// <copyright file="HardwareManager.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pool.Hardware
{
    /// <summary>
    /// Hardware manager in charge of interacting with the physical hardware
    /// </summary>
    public class HardwareManager : IHardwareManager
    {
        /// <summary>
        /// The physical driver used to interract with the hardware.
        /// </summary>
        private IHardwareDriver driver;

        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger<HardwareManager> logger;

        /// <summary>
        /// The hardware settings.
        /// </summary>
        private HardwareConfiguration configuration;

        /// <summary>
        /// Association between logical names and pins.
        /// </summary>
        private Dictionary<PinName, int> pins = new Dictionary<PinName, int>();

        /// <summary>
        /// Association between logical names and pins.
        /// </summary>
        private Dictionary<TemperatureSensorName, string> temperatureSensors = new Dictionary<TemperatureSensorName, string>();

        /// <summary>
        /// Output statuses.
        /// </summary>
        private Dictionary<PinName, HardwareOutputState> outputsStates = new Dictionary<PinName, HardwareOutputState>();


        /// <summary>
        /// Initializes a new instance of the <see cref="HardwareManager"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="driver">The driver used to access the hardware.</param>
        public HardwareManager(IOptions<HardwareConfiguration> configuration, ILogger<HardwareManager> logger, IHardwareDriver driver)
        {
            this.driver = driver;
            this.logger = logger;
            this.configuration = configuration.Value;
        }

        /// <summary>
        /// Raise each time a input changed.
        /// </summary>
        public event EventHandler<BooleanInputChangeEventArgs> BooleanInputChanged;

        /// <inheritdoc />
        public void OpenConfiguration()
        {
            this.PrintHardwarePins();

            this.CheckConfiguration();

            var pinsWithName = this.configuration.Pins
                .Select(p => new
                {
                    PinName = (PinName)Enum.Parse(typeof(PinName), p.Name),
                    Pin = p
                })
                .ToList();

            // Associate each name with pin output
            this.pins = pinsWithName.ToDictionary(
                p => p.PinName,
                p => p.Pin.LogicalPin);

            this.outputsStates = pinsWithName
                .Where(p => p.Pin.Mode == HardwarePinConfigurationMode.Output)
                .ToDictionary(
                    p => p.PinName,
                    p => new HardwareOutputState(p.PinName, p.Pin.Description));

            this.temperatureSensors = this.configuration.TemperatureSensors.ToDictionary(
                t => (TemperatureSensorName)Enum.Parse(typeof(TemperatureSensorName), t.Name),
                t => t.DeviceId);

            // Set hardware outputs
            this.logger.LogInformation("Initializing hardware...");
            this.driver.OpenPins(this.configuration);
            this.driver.InputBooleanChanged += this.Driver_InputBooleanChanged;
            this.logger.LogInformation("Hardware initialization ended.");
        }

        /// <inheritdoc />
        public void CloseConfiguration()
        {
            // Set hardware outputs
            this.logger.LogInformation("Stopping hardware...");
            this.driver.InputBooleanChanged -= this.Driver_InputBooleanChanged;
            this.driver.ClosePins();
            this.logger.LogInformation("Hardware stopped.");
        }

        /// <inheritdoc />
        public void Write(PinName output, bool value)
        {
            this.logger.LogInformation($"> [{output.ToString()}] = '{value}'");

            int pin = this.pins[output];

            this.driver.Write(pin, value);

            this.outputsStates[output].State = value;
        }

        /// <inheritdoc />
        public double ReadTemperatureValue(TemperatureSensorName sensor)
        {
            return Math.Round(this.driver.ReadTemperatureValue(this.temperatureSensors[sensor]), 1);
        }

        /// <inheritdoc />
        public IReadOnlyList<HardwareOutputState> GetOutputs()
        {
            return this.outputsStates.Values.ToList();
        }

        /// <inheritdoc />
        public HardwareOutputState GetOutput(PinName pinName)
        {
            return this.outputsStates[pinName];
        }

        /// <summary>
        /// Just display all configured pins
        /// </summary>
        private void PrintHardwarePins()
        {
            string pintformat = "{0,3} | {1,-22} | {2,-8} | {3}";

            this.logger.LogInformation("-------------------------------------------------------------------");
            this.logger.LogInformation(pintformat, "Pin", "Name", "Mode", "Description");
            this.logger.LogInformation("-------------------------------------------------------------------");

            foreach (var pin in this.configuration.Pins)
            {
                this.logger.LogInformation(
                    pintformat,
                    pin.LogicalPin,
                    pin.Name,
                    pin.Mode,
                    pin.Description);
            }

            string sensorFormat = "{0,-19} | {1,-17} | {2}";

            this.logger.LogInformation("-------------------------------------------------------------------");
            this.logger.LogInformation(sensorFormat, "Name", "DeviceID", "Description");
            this.logger.LogInformation("-------------------------------------------------------------------");

            foreach (var sensor in this.configuration.TemperatureSensors)
            {
                this.logger.LogInformation(
                    sensorFormat,
                    sensor.Name,
                    sensor.DeviceId,
                    sensor.Description);
            }

            this.logger.LogInformation("-------------------------------------------------------------------");
        }

        /// <summary>
        /// Check that the configuation is complete.
        /// </summary>
        private void CheckConfiguration()
        {
            // Check that all pins are configured
            var expectedPinNames = Enum.GetNames(typeof(PinName));
            var configuredPins = this.configuration.Pins.Select(o => o.Name).ToArray();
            var nonConfiguredPins = expectedPinNames.Where(n => configuredPins.Contains(n) == false).ToList();

            if (nonConfiguredPins.Count > 0)
            {
                throw new ArgumentException($"All pins are not configured, please add : {string.Join(", ", nonConfiguredPins)}");
            }

            // Check duplicates
            var duplicates = this.configuration.Pins.GroupBy(p => p.LogicalPin)
                .Select(g => new { Pin = g.Key, Count = g.Count() })
                .Where(p => p.Count > 1)
                .ToList();
            if (duplicates.Count > 0)
            {
                throw new ArgumentException("Duplicate pin numbers in configuration");
            }

            // Check temperature sensors
            var expectedNames = Enum.GetNames(typeof(TemperatureSensorName));
            var configuredTemperatureSensors = this.configuration.TemperatureSensors.Select(o => o.Name).ToArray();
            var nonConfiguredTemperatureSensors = expectedNames.Where(n => configuredTemperatureSensors.Contains(n) == false).ToList();

            if (nonConfiguredTemperatureSensors.Count > 0)
            {
                throw new ArgumentException($"All temperature sensors are not configured, please add : {string.Join(", ", nonConfiguredTemperatureSensors)}");
            }
        }

        /// <summary>
        /// Called when pin change
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Pin status</param>
        private void Driver_InputBooleanChanged(object sender, HardwarePinChangeEventArgs e)
        {
            if (this.BooleanInputChanged != null)
            {
                // Convert the pin to name
                var pinName = this.pins.Single(p => p.Value == e.Pin).Key;

                // And raise the event
                this.BooleanInputChanged(this, new BooleanInputChangeEventArgs(pinName, e.State));
            }
        }
    }
}
