using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pool.Control.Store;
using System;

namespace Pool.Control.Tests
{
    [TestClass]
    public class StoreServiceTests
    {
        [TestMethod]
        public void ReadWritePoolSettings()
        {
            var storeService = new StoreService();

            var poolSettings = new PoolSettings();

            poolSettings.WorkingMode = PoolWorkingMode.Winter;
            poolSettings.CoverCylcleDurationInSeconds = 100;

            poolSettings.SummerPumpingCycles.Add(new PumpCycleGroupSetting() { MinimumTemperature = 0 });
            poolSettings.SummerPumpingCycles[0].PumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(1), PumpCycleType = PumpCycleType.StartAt });
            poolSettings.SummerPumpingCycles[0].PumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(12), PumpCycleType = PumpCycleType.StopAt });


            poolSettings.WinterPumpingCycles.Add(new PumpCycleGroupSetting() { MinimumTemperature = 0 });
            poolSettings.WinterPumpingCycles[0].PumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(15), PumpCycleType = PumpCycleType.StopAt });

            poolSettings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 15, RunTimeHours = 1 });
            poolSettings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 20, RunTimeHours = 4 });

            storeService.WritePoolSettings(poolSettings, "poolsettings.json");

            var results = storeService.ReadPoolSettings("poolsettings.json");
            Assert.AreEqual(PoolWorkingMode.Winter, results.WorkingMode);
            Assert.AreEqual(100, results.CoverCylcleDurationInSeconds);
            Assert.AreEqual(1, results.SummerPumpingCycles.Count);
            Assert.AreEqual(2, results.SummerPumpingCycles[0].PumpingCycles.Count);
            Assert.AreEqual(PumpCycleType.StartAt, results.SummerPumpingCycles[0].PumpingCycles[0].PumpCycleType);
            Assert.AreEqual(TimeSpan.FromHours(12), results.SummerPumpingCycles[0].PumpingCycles[1].DecisionTime);
            Assert.AreEqual(PumpCycleType.StopAt, results.SummerPumpingCycles[0].PumpingCycles[1].PumpCycleType);
        }

        [TestMethod]
        public void ReadEmptyPoolSettings()
        {
            var results = new StoreService().ReadPoolSettings("nofile");
            Assert.AreEqual(PoolWorkingMode.Summer, results.WorkingMode);
            Assert.AreEqual(90, results.CoverCylcleDurationInSeconds);
            Assert.AreEqual(4, results.SummerPumpingCycles.Count);
            Assert.AreEqual(1, results.WinterPumpingCycles.Count);
        }

        [TestMethod]
        public void ReadWriteSystemState()
        {
            var storeService = new StoreService();

            var time = DateTime.UtcNow;
            var fileName = "poolsettings.json";

            var state = new SystemState();
            state.AirTemperature = new SampleValue<double>(time, 28.3);
            state.PoolTemperature = new SampleValue<double>(time, 23.6);
            state.PumpingDurationPerDayInHours = new SampleValue<double>(time, 1);
            state.WaterTemperature = new SampleValue<double>(time, 27.2);
            state.PoolTemperatureDecision = new SampleValue<double>(time, 24.1);
            state.Pump = new SampleValue<bool>(time, true);

            storeService.WriteSystemState(state, fileName);

            var result = storeService.ReadSystemState(fileName);
            Assert.AreEqual(time, result.PoolTemperature.Time);
            Assert.AreEqual(23.6, result.PoolTemperature.Value);
            Assert.AreEqual(1, result.PumpingDurationPerDayInHours.Value);
            Assert.AreEqual(24.1, result.PoolTemperatureDecision.Value);

            // Not persisted
            Assert.AreEqual(23, result.AirTemperature.Value);
            Assert.AreEqual(23, result.WaterTemperature.Value);
            Assert.IsFalse(result.Pump.Value);
        }

        [TestMethod]
        public void ReadEmptySystemState()
        {
            var time = new DateTime(2000, 1, 1);

            var result = new StoreService().ReadSystemState("nofile");
            Assert.AreEqual(time, result.AirTemperature.Time);
            Assert.AreEqual(23, result.AirTemperature.Value);
            Assert.AreEqual(23, result.PoolTemperature.Value);
            Assert.AreEqual(1, result.PumpingDurationPerDayInHours.Value);
            Assert.AreEqual(23, result.WaterTemperature.Value);
            Assert.AreEqual(23, result.PoolTemperatureDecision.Value);
        }
    }
}