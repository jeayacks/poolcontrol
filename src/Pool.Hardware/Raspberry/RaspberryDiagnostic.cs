//-----------------------------------------------------------------------
// <copyright file="RaspberryDiagnostic.cs" company="JeYacks">
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
    /// Driver implementation
    /// </summary>
    public static class RaspberryDiagnostic
    {
        public static void RunAndDisplay()
        {
            using var controller = new GpioController(PinNumberingScheme.Logical);

            for (int i = 0; i < controller.PinCount; i++)
            {
                Console.WriteLine(string.Concat(
                    $"Pin {i:000}",
                    $" | Input = {controller.IsPinModeSupported(i, PinMode.Input)}",
                    $" | InputPullDown = {controller.IsPinModeSupported(i, PinMode.InputPullDown)}",
                    $" | InputPullUp = {controller.IsPinModeSupported(i, PinMode.InputPullUp)}",
                    $" | Output = {controller.IsPinModeSupported(i, PinMode.Output)}"
                ));
            }

            // Quick and simple way to find a thermometer and print the temperature
            Console.WriteLine("Enumerating temperature sensors...");
            foreach (var dev in OneWireThermometerDevice.EnumerateDevices())
            {
                var temperature = dev.ReadTemperature().Celsius;
                Console.WriteLine($"Temperature reported by '{dev.DeviceId}':  {temperature}°C");
            }
        }
    }
}