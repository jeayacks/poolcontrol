//-----------------------------------------------------------------------
// <copyright file="RaspberryDriver.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Hardware
{
    using Iot.Device.OneWire;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Device.Gpio;
    using System.Linq;

    /// <summary>
    /// Display diagnotics
    /// </summary>
    public class RaspberryDriver : IHardwareDriver
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger<RaspberryDriver> logger;

        /// <summary>
        /// The controller.
        /// </summary>
        private GpioController controller;

        /// <summary>
        /// List of open pins
        /// </summary>
        private List<HardwarePinConfiguration> pins = new List<HardwarePinConfiguration>();

        /// <summary>
        /// List of sensors
        /// </summary>
        private List<OneWireThermometerDevice> devices = new List<OneWireThermometerDevice>();

        public RaspberryDriver(ILogger<RaspberryDriver> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public event EventHandler<HardwarePinChangeEventArgs> InputBooleanChanged;

        /// <inheritdoc />
        public void OpenPins(HardwareConfiguration configuration)
        {
            this.controller = new GpioController(PinNumberingScheme.Logical);

            foreach (var pin in configuration.Pins)
            {

                if (pin.Mode == HardwarePinConfigurationMode.Input)
                {
                    this.controller.OpenPin(pin.LogicalPin);

                    this.controller.RegisterCallbackForPinValueChangedEvent(
                        pin.LogicalPin,
                        PinEventTypes.Falling | PinEventTypes.Rising | PinEventTypes.None,
                        this.OnPinValueChanged);

                }
                else
                {
                    this.controller.OpenPin(pin.LogicalPin, PinMode.Output);
                    this.controller.Write(pin.LogicalPin, PinValue.High);
                }
            }

            this.pins = configuration.Pins.ToList();

            this.devices = OneWireThermometerDevice.EnumerateDevices().ToList();
            foreach (var device in this.devices)
            {
                this.logger.LogDebug($"Found device bus={device.BusId}, id={device.DeviceId}, type={device.Family.ToString()}");
            }
        }

        /// <inheritdoc />
        public void ClosePins()
        {
            foreach (var pin in this.pins)
            {
                if (pin.Mode == HardwarePinConfigurationMode.Input)
                {
                    this.controller.UnregisterCallbackForPinValueChangedEvent(
                        pin.LogicalPin,
                        this.OnPinValueChanged);
                }
                else
                {
                    // Set to OFF
                    this.controller.Write(pin.LogicalPin, PinValue.High);
                }

                this.controller.ClosePin(pin.LogicalPin);
            }

            this.pins = null;
        }

        /// <inheritdoc />
        public void Write(int pin, bool value)
        {
            PinValue pinValue = value ? PinValue.Low : PinValue.High;

            this.logger.LogDebug($"Write pin {pin} = {pinValue.ToString()}");

            this.controller.Write(pin, pinValue);
        }

        /// <inheritdoc />
        public double ReadTemperatureValue(string id)
        {
            var device = this.devices.FirstOrDefault(d => d.DeviceId == id);

            if (device!= null)
            {
                return device.ReadTemperature().Celsius;
            }

            this.logger.LogError($"Temperature device {id} not found");
            return 0;
        }


        private void OnPinValueChanged(object sender, PinValueChangedEventArgs e)
        {
            this.logger.LogDebug($"Pin {e.PinNumber} change = {e.ChangeType.ToString()}");

            if (this.InputBooleanChanged != null)
            {
                this.InputBooleanChanged(
                    this,
                    new HardwarePinChangeEventArgs(e.PinNumber, e.ChangeType == PinEventTypes.Falling ? true : false));
            }
        }
    }
}