using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pool.Hardware.Tests
{
    [TestClass]
    public class HardwareManagerTests
    {
        [TestMethod]
        public void InitializeConfiguration()
        {
            var driver = new Mock<IHardwareDriver>();
            var manager = CreateHardwareManagerWithFullConfiguration(driver.Object);
            manager.OpenConfiguration();

            manager.Write(PinName.ChlorineInhibition, false);

            driver.Verify(d => d.OpenPins(It.IsAny<HardwareConfiguration>()));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConfigurationMissingInputs()
        {
            var configuration = new HardwareConfiguration();

            // Add sensors
            foreach (var n in Enum.GetNames(typeof(TemperatureSensorName)))
            {
                configuration.TemperatureSensors.Add(new HardwareTemperatureSensorConfiguration(n, n));
            }

            CreateHardwareManager(configuration, Mock.Of<IHardwareDriver>())
                .OpenConfiguration();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConfigurationMissingSensors()
        {
            var configuration = new HardwareConfiguration();

            // Add all pins
            int pinId = 0;
            foreach (var pinName in Enum.GetNames(typeof(PinName)))
            {
                configuration.Pins.Add(new HardwarePinConfiguration(pinName, pinId++, HardwarePinConfigurationMode.Input));
            }

            CreateHardwareManager(configuration, Mock.Of<IHardwareDriver>())
                .OpenConfiguration();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConfigurationDuplicatePinsInputs()
        {
            var configuration = new HardwareConfiguration();
            foreach (var pinName in Enum.GetNames(typeof(PinName)))
            {
                configuration.Pins.Add(new HardwarePinConfiguration(pinName, 1, HardwarePinConfigurationMode.Input));
            }

            // Add sensors
            foreach (var n in Enum.GetNames(typeof(TemperatureSensorName)))
            {
                configuration.TemperatureSensors.Add(new HardwareTemperatureSensorConfiguration(n, n));
            }


            CreateHardwareManager(configuration, Mock.Of<IHardwareDriver>())
                .OpenConfiguration();
        }

        [TestMethod]
        public void WriteOuput()
        {
            var driver = new Mock<IHardwareDriver>();
            driver.Setup(d => d.OpenPins(It.IsAny<HardwareConfiguration>()));
            driver.Setup(d => d.ClosePins());
            driver.Setup(d => d.Write(1, true));

            var manager = CreateHardwareManagerWithFullConfiguration(driver.Object);

            manager.OpenConfiguration();
            manager.Write(PinName.Pump, true);
            manager.CloseConfiguration();

            driver.VerifyAll();

            Assert.IsTrue(manager.GetOutputs().First().State);
        }

        [TestMethod]
        public void ReadInput()
        {
            var driver = new Mock<IHardwareDriver>();
            BooleanInputChangeEventArgs eventRaised = null;

            var manager = CreateHardwareManagerWithFullConfiguration(driver.Object);
            manager.OpenConfiguration();
            manager.BooleanInputChanged += (s, e) =>
            {
                eventRaised = e;
            };

            driver.Raise(d => d.InputBooleanChanged += null, new HardwarePinChangeEventArgs(1, true));

            manager.CloseConfiguration();

            Assert.IsNotNull(eventRaised);
            Assert.AreEqual(PinName.Pump, eventRaised.Pin);
            Assert.IsTrue(eventRaised.State);
        }

        [TestMethod]
        public void ReadTemperature()
        {
            var driver = new Mock<IHardwareDriver>();
            driver.Setup(d => d.ReadTemperatureValue("AirTemperature")).Returns(27.8);

            var manager = CreateHardwareManagerWithFullConfiguration(driver.Object);
            manager.OpenConfiguration();

            var value = manager.ReadTemperatureValue(TemperatureSensorName.AirTemperature);
            Assert.AreEqual(27.8, value);
        }

        private HardwareManager CreateHardwareManager(HardwareConfiguration configuration, IHardwareDriver driver)
        {
            return new HardwareManager(
                Options.Create(configuration),
                Mock.Of<ILogger<HardwareManager>>(),
                driver);
        }

        private HardwareManager CreateHardwareManagerWithFullConfiguration(IHardwareDriver driver)
        {
            var configuration = new HardwareConfiguration();

            // Add all pins
            int pinId = 1;
            foreach (var pinName in Enum.GetNames(typeof(PinName)))
            {
                configuration.Pins.Add(new HardwarePinConfiguration(pinName, pinId++, HardwarePinConfigurationMode.Output));
            }

            // Add sensors
            foreach (var n in Enum.GetNames(typeof(TemperatureSensorName)))
            {
                configuration.TemperatureSensors.Add(new HardwareTemperatureSensorConfiguration(n, n));
            }

            return new HardwareManager(
                Options.Create(configuration),
                Mock.Of<ILogger<HardwareManager>>(),
                driver);
        }
    }
}
