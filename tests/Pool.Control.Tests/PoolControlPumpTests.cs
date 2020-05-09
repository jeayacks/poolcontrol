using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pool.Control.Store;
using Pool.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pool.Control.Tests
{
    [TestClass]
    public class PoolControlPumpTests
    {
        [TestMethod]
        public void TemperatureAcquisition()
        {
            using (var systemTime = new SystemTimeMock())
            {
                var time = DateTime.Now.Date;
                systemTime.Set(time);

                var context = Context.Create();

                context.WaterTemperature = 21;

                context.PoolControlPump.Process();

                Assert.AreEqual(time, context.SystemState.WaterTemperature.Time);
                Assert.AreEqual(21, context.SystemState.WaterTemperature.Value);

                // Default values are not yet modified, require more than the delay
                Assert.AreEqual(23, context.SystemState.PoolTemperature.Value);
                Assert.AreEqual(23, context.SystemState.PoolTemperatureDecision.Value);
                Assert.AreEqual(0, context.SystemState.PoolTemperatureMaxOfTheDay.Value);
                Assert.AreEqual(1, context.SystemState.PumpingDurationPerDayInHours.Value);

                // Set pump ON
                context.SystemState.Pump.UpdateValue(true);

                // Move significant time after pump start
                time = time.AddSeconds(PoolControlPump.PumpRunTimeToAcquirePoolTemperature);
                systemTime.Set(time);
                context.WaterTemperature = 22;
                context.PoolControlPump.Process();

                // Pool temperature must be assigned as well temperature of the day
                Assert.AreEqual(time, context.SystemState.WaterTemperature.Time);
                Assert.AreEqual(22, context.SystemState.WaterTemperature.Value);
                Assert.AreEqual(22, context.SystemState.PoolTemperature.Value);
                Assert.AreEqual(-1, context.SystemState.PoolTemperatureMaxOfTheDay.Value); // Value reseted
                Assert.AreEqual(22, context.SystemState.PoolTemperatureDecision.Value);
                Assert.AreEqual(4, context.SystemState.PumpingDurationPerDayInHours.Value);

                // Max reassigned
                time = time.AddSeconds(PoolControlPump.TemperatureReadingInterval);
                systemTime.Set(time);
                context.PoolControlPump.Process();
                Assert.AreEqual(22, context.SystemState.PoolTemperatureMaxOfTheDay.Value);

                // Increase temp, pump still running
                time = time.AddSeconds(PoolControlPump.TemperatureReadingInterval);
                systemTime.Set(time);
                context.WaterTemperature = 24;
                context.PoolControlPump.Process();
                Assert.AreEqual(24, context.SystemState.WaterTemperature.Value);
                Assert.AreEqual(24, context.SystemState.PoolTemperature.Value);
                Assert.AreEqual(24, context.SystemState.PoolTemperatureMaxOfTheDay.Value);

                // Decrease temp, pump still running
                time = time.AddSeconds(PoolControlPump.TemperatureReadingInterval);
                systemTime.Set(time);
                context.WaterTemperature = 21;
                context.PoolControlPump.Process();
                Assert.AreEqual(21, context.SystemState.WaterTemperature.Value);
                Assert.AreEqual(21, context.SystemState.PoolTemperature.Value);
                Assert.AreEqual(24, context.SystemState.PoolTemperatureMaxOfTheDay.Value);

                // Increase temp, pump still running
                // Check temperature decision not changed
                time = time.AddSeconds(PoolControlPump.TemperatureReadingInterval);
                systemTime.Set(time);
                context.WaterTemperature = 28;
                context.PoolControlPump.Process();
                Assert.AreEqual(28, context.SystemState.WaterTemperature.Value);
                Assert.AreEqual(28, context.SystemState.PoolTemperature.Value);
                Assert.AreEqual(28, context.SystemState.PoolTemperatureMaxOfTheDay.Value);
                Assert.AreEqual(22, context.SystemState.PoolTemperatureDecision.Value);
                Assert.AreEqual(4, context.SystemState.PumpingDurationPerDayInHours.Value);

                // Stop pump
                context.SystemState.Pump.UpdateValue(false);

                // Move to next day
                time = DateTime.Now.Date.AddDays(1);
                systemTime.Set(time);
                context.WaterTemperature = 18;
                context.PoolControlPump.Process();
                Assert.AreEqual(18, context.SystemState.WaterTemperature.Value);
                Assert.AreEqual(28, context.SystemState.PoolTemperature.Value);
                Assert.AreEqual(-1, context.SystemState.PoolTemperatureMaxOfTheDay.Value); // Max reseted
                Assert.AreEqual(28, context.SystemState.PoolTemperatureDecision.Value);
                Assert.AreEqual(8, context.SystemState.PumpingDurationPerDayInHours.Value);

                // Start the pump
                context.SystemState.Pump.UpdateValue(true);

                time = time.AddSeconds(PoolControlPump.PumpRunTimeToAcquirePoolTemperature);
                systemTime.Set(time);
                context.WaterTemperature = 17;
                context.PoolControlPump.Process();
                Assert.AreEqual(17, context.SystemState.WaterTemperature.Value);
                Assert.AreEqual(17, context.SystemState.PoolTemperature.Value);
                Assert.AreEqual(17, context.SystemState.PoolTemperatureMaxOfTheDay.Value);
                Assert.AreEqual(28, context.SystemState.PoolTemperatureDecision.Value);
                Assert.AreEqual(8, context.SystemState.PumpingDurationPerDayInHours.Value);

                // Move to next day
                time = DateTime.Now.Date.AddDays(2);
                systemTime.Set(time);
                context.WaterTemperature = 17;
                context.PoolControlPump.Process();
                Assert.AreEqual(17, context.SystemState.WaterTemperature.Value);
                Assert.AreEqual(17, context.SystemState.PoolTemperature.Value);
                Assert.AreEqual(-1, context.SystemState.PoolTemperatureMaxOfTheDay.Value);
                Assert.AreEqual(17, context.SystemState.PoolTemperatureDecision.Value);
                Assert.AreEqual(1, context.SystemState.PumpingDurationPerDayInHours.Value);
            }
        }

        [TestMethod]
        public void PumpCycles()
        {
            using (var systemTime = new SystemTimeMock())
            {
                var time = DateTime.Now.Date;
                systemTime.Set(time);

                var context = Context.Create();

                // 21� = 4h
                // 2cycles = 2h/cycle
                //     8h----10h       16h----18h 
                context.WaterTemperature = 21;
                context.SystemState.PumpingDurationPerDayInHours.UpdateValue(4);
                context.PoolControlPump.Process();

                systemTime.Set(time.AddHours(8));
                context.PoolControlPump.Process();
                Assert.IsTrue(context.SystemState.Pump.Value);
                Assert.IsTrue(context.PinStatus[PinName.Pump]);
                Assert.IsTrue(context.PinStatus[PinName.ChlorineInhibition]);
                Assert.IsFalse(context.PinStatus[PinName.PhRegulationInhibition]);

                systemTime.Set(time.AddHours(10));
                context.PoolControlPump.Process();
                Assert.IsTrue(context.SystemState.Pump.Value);
                Assert.IsTrue(context.PinStatus[PinName.Pump]);
                Assert.IsTrue(context.PinStatus[PinName.ChlorineInhibition]);
                Assert.IsFalse(context.PinStatus[PinName.PhRegulationInhibition]);

                systemTime.Set(time.AddHours(10).AddSeconds(1));
                context.PoolControlPump.Process();
                Assert.IsFalse(context.SystemState.Pump.Value);
                Assert.IsFalse(context.PinStatus[PinName.Pump]);
                Assert.IsFalse(context.PinStatus[PinName.ChlorineInhibition]);
                Assert.IsFalse(context.PinStatus[PinName.PhRegulationInhibition]);

                systemTime.Set(time.AddHours(16));
                context.PoolControlPump.Process();
                Assert.IsTrue(context.SystemState.Pump.Value);
                Assert.IsTrue(context.PinStatus[PinName.Pump]);
                Assert.IsFalse(context.PinStatus[PinName.ChlorineInhibition]);
                Assert.IsTrue(context.PinStatus[PinName.PhRegulationInhibition]);

                // Increase pumping duration to pump must stay on
                context.SystemState.PumpingDurationPerDayInHours.UpdateValue(8);
                systemTime.Set(time.AddHours(18).AddSeconds(1));
                context.PoolControlPump.Process();
                Assert.IsTrue(context.SystemState.Pump.Value);
                Assert.IsTrue(context.PinStatus[PinName.Pump]);

                systemTime.Set(time.AddHours(22).AddSeconds(1));
                context.PoolControlPump.Process();
                Assert.IsFalse(context.SystemState.Pump.Value);
                Assert.IsFalse(context.PinStatus[PinName.Pump]);

                // Check next cycle
                var cycles = context.PoolControlPump.GetCyclesInfo().ToList();
                Assert.AreEqual(time.AddDays(1).AddHours(8), cycles[0].StartTime);
                Assert.AreEqual(time.AddDays(1).AddHours(12), cycles[0].EndTime);
            }
        }


        [TestMethod]
        public void PumpWinterCycles()
        {
            using (var systemTime = new SystemTimeMock())
            {
                var time = DateTime.Now.Date;
                systemTime.Set(time);

                var context = Context.Create();

                context.WaterTemperature = 12;
                context.PoolSettings.WorkingMode = PoolWorkingMode.Winter;
                context.PoolSettings.WinterPumpingCycles.Clear();
                context.PoolSettings.WinterPumpingCycles.Add(new PumpCycleGroupSetting());
                context.PoolSettings.WinterPumpingCycles[0].PumpingCycles.Add(new PumpCycleSetting()
                {
                    DecisionTime = TimeSpan.FromHours(5),
                    PumpCycleType = PumpCycleType.StartAt,
                });

                context.SystemState.PumpingDurationPerDayInHours.UpdateValue(1);
                context.PoolControlPump.Process();

                systemTime.Set(time.AddHours(5));
                context.PoolControlPump.Process();
                Assert.IsTrue(context.PinStatus[PinName.Pump]);

                systemTime.Set(time.AddHours(6).AddSeconds(1));
                context.PoolControlPump.Process();
                Assert.IsFalse(context.PinStatus[PinName.Pump]);
            }
        }

        [TestMethod]
        public void PumpForcingStates()
        {
            using (var systemTime = new SystemTimeMock())
            {
                var time = DateTime.Now.Date;
                systemTime.Set(time);

                var context = Context.Create();

                // 21� = 4h
                // 2cycles = 2h/cycle
                //     8h----10h       16h----18h 
                context.WaterTemperature = 21;
                context.SystemState.PumpingDurationPerDayInHours.UpdateValue(4);
                context.PoolControlPump.Process();
                Assert.IsFalse(context.PinStatus[PinName.Pump]); // Pump is currently off

                // Force ON when pump if OFF
                systemTime.Set(time.AddHours(1));
                context.SystemState.PumpForceOn.UpdateValue(true);
                context.PoolControlPump.Process();
                Assert.IsTrue(context.PinStatus[PinName.Pump]);

                // Disable force ON
                systemTime.Set(time.AddHours(2));
                context.SystemState.PumpForceOn.UpdateValue(false);
                context.PoolControlPump.Process();
                Assert.IsFalse(context.PinStatus[PinName.Pump]);

                // Force OFF during pumping time
                systemTime.Set(time.AddHours(8));
                context.PoolControlPump.Process();
                Assert.IsTrue(context.PinStatus[PinName.Pump]);
                context.SystemState.PumpForceOff.UpdateValue(true);
                context.PoolControlPump.Process();
                Assert.IsFalse(context.PinStatus[PinName.Pump]);

                // Disable force OFF during pumpin cycle
                systemTime.Set(time.AddHours(9));
                context.SystemState.PumpForceOff.UpdateValue(false);
                context.PoolControlPump.Process();
                Assert.IsTrue(context.PinStatus[PinName.Pump]);
            }
        }

        [TestMethod]
        public void PumpDisableForcingStates()
        {
            using (var systemTime = new SystemTimeMock())
            {
                var time = DateTime.Now.Date;
                systemTime.Set(time);

                var context = Context.Create();

                context.PoolControlPump.Process();

                context.SystemState.PumpForceOn.UpdateValue(true);
                context.PoolControlPump.Process();
                systemTime.Set(time.AddDays(2));
                context.PoolControlPump.Process();
                Assert.IsFalse(context.SystemState.PumpForceOn.Value);

                context.SystemState.PumpForceOff.UpdateValue(true);
                context.PoolControlPump.Process();
                systemTime.Set(time.AddDays(4));
                context.PoolControlPump.Process();
                Assert.IsFalse(context.SystemState.PumpForceOff.Value);
            }
        }

        private class Context
        {
            public Mock<IHardwareManager> HardwareManager { get; private set; }
            public PoolSettings PoolSettings { get; private set; }
            public SystemState SystemState { get; private set; }
            public PoolControlPump PoolControlPump { get; private set; }
            public double WaterTemperature { get; set; }

            public Dictionary<PinName, bool> PinStatus { get; private set; }

            public static Context Create()
            {
                var context = new Context()
                {
                    HardwareManager = new Mock<IHardwareManager>(),
                    PoolSettings = GetPoolSettings(),
                    SystemState = new SystemState(),
                    PinStatus = new Dictionary<PinName, bool>(),
                };

                context.PinStatus.Add(PinName.Pump, false);
                context.PinStatus.Add(PinName.ChlorineInhibition, false);
                context.PinStatus.Add(PinName.PhRegulationInhibition, false);

                context.HardwareManager.Setup(s => s.ReadTemperatureValue(It.IsAny<TemperatureSensorName>())).Returns(() =>
                {
                    return context.WaterTemperature;
                });

                context.HardwareManager.Setup(s => s.Write(It.IsAny<PinName>(), It.IsAny<bool>())).Callback<PinName, bool>((pin, state) =>
                {
                    context.PinStatus[pin] = state;
                });

                context.HardwareManager.Setup(s => s.GetOutput(It.IsAny<PinName>())).Returns<PinName>((pin) =>
                {
                    return new HardwareOutputState(pin, pin.ToString())
                    {
                        State = context.PinStatus[pin],
                    };
                });

                context.PoolControlPump = new PoolControlPump(
                    context.PoolSettings,
                    context.SystemState,
                    context.HardwareManager.Object);

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
                    ChlorineInhibition = true,
                    PhRegulationInhibition = false,
                });

                settings.SummerPumpingCycles[0].PumpingCycles.Add(new PumpCycleSetting()
                {
                    DecisionTime = TimeSpan.FromHours(16),
                    PumpCycleType = PumpCycleType.StartAt,
                    ChlorineInhibition = false,
                    PhRegulationInhibition = true,
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