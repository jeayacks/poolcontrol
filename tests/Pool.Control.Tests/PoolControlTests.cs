using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pool.Control.Store;
using Pool.Hardware;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pool.Control.Tests
{
    [TestClass]
    public class PoolControlTests
    {
        [TestMethod]
        public void StartStopPoolControl()
        {
            var context = Context.Create();
            context.PoolControl.Execute(new CancellationToken(true));

            context.StoreService.Verify(s => s.ReadPoolSettings("settings.json"));
            //context.StoreService.Verify(s => s.WritePoolSettings(It.IsAny<PoolSettings>(), "settings.json"));
            context.StoreService.Verify(s => s.ReadSystemState("system-states.json"));
            context.StoreService.Verify(s => s.WriteSystemState(It.IsAny<SystemState>(), "system-states.json"));
            context.HardwareManager.Verify(s => s.OpenConfiguration());
            context.HardwareManager.Verify(s => s.CloseConfiguration());
        }

        [TestMethod]
        public void TypicalControlLoop()
        {
            var context = Context.Create();

            var cancellation = new CancellationTokenSource();
            Task.Run(() =>
            {
                Thread.Sleep(100);
                cancellation.Cancel();
            });

            context.PoolControl.Execute(cancellation.Token);

            // Check temperature values
            var states = context.PoolControl.GetPoolControlInformation().SystemState;
            Assert.AreEqual(27.2, states.AirTemperature.Value);
            Assert.AreEqual(27.2, states.WaterTemperature.Value);
        }

        [TestMethod]
        public void ChangeSettings()
        {
            var context = Context.Create();

            context.PoolControl.Execute(new CancellationToken(true));
            var settings = context.PoolControl.GetPoolSettings();

            settings.WorkingMode = PoolWorkingMode.Winter;
            settings.CoverCylcleDurationInSeconds = 30;
            settings.WinterPumpingCycles.Add(new PumpCycleGroupSetting());
            settings.TemperatureRunTime.Add(new TemperatureRunTime());
            settings.SummerPumpingCycles.Add(new PumpCycleGroupSetting());

            context.PoolControl.SavePoolSettings(settings);

            settings = context.PoolControl.GetPoolSettings();

            Assert.AreEqual(PoolWorkingMode.Winter, settings.WorkingMode);
            Assert.AreEqual(30, settings.CoverCylcleDurationInSeconds);
            Assert.AreEqual(1, settings.WinterPumpingCycles.Count);
            Assert.AreEqual(2, settings.SummerPumpingCycles.Count);
            Assert.AreEqual(5, settings.TemperatureRunTime.Count);
        }

        private class Context
        {
            public Mock<IHardwareManager> HardwareManager { get; private set; }
            public Mock<IStoreService> StoreService { get; private set; }
            public PoolControl PoolControl { get; private set; }

            public static Context Create()
            {
                var context = new Context()
                {
                    HardwareManager = new Mock<IHardwareManager>(),
                    StoreService = new Mock<IStoreService>(),
                };

                context.HardwareManager.Setup(s => s.GetOutputs()).Returns(new List<HardwareOutputState>());
                context.HardwareManager.Setup(s => s.ReadTemperatureValue(It.IsAny<TemperatureSensorName>())).Returns(27.2);

                var poolSettings = GetPoolSettings();
                context.StoreService.Setup(s => s.ReadPoolSettings(It.IsAny<string>())).Returns(poolSettings);

                context.StoreService.Setup(s => s.ReadSystemState(It.IsAny<string>())).Returns(new SystemState());

                context.PoolControl = new PoolControl(
                    Mock.Of<ILogger<PoolControl>>(),
                    context.HardwareManager.Object,
                    context.StoreService.Object,
                    1);

                return context;
            }

            private static PoolSettings GetPoolSettings()
            {
                var settings = new PoolSettings();
                settings.SummerPumpingCycles.Clear();
                settings.SummerPumpingCycles.Add(new PumpCycleGroupSetting());

                settings.SummerPumpingCycles[0].PumpingCycles.Add(new PumpCycleSetting()
                {
                    DecisionTime = TimeSpan.FromHours(8),
                    PumpCycleType = PumpCycleType.StartAt,
                });

                settings.SummerPumpingCycles[0].PumpingCycles.Add(new PumpCycleSetting()
                {
                    DecisionTime = TimeSpan.FromHours(16),
                    PumpCycleType = PumpCycleType.StartAt,
                });

                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 15, RunTimeHours = 1 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 20, RunTimeHours = 4 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 25, RunTimeHours = 8 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 30, RunTimeHours = 12 });

                return settings;
            }
        }
    }
}