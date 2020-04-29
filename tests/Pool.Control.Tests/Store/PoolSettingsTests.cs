using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Pool.Control.Store;
using System;
using System.Linq;

namespace Pool.Control.Tests
{
    [TestClass]
    public class PoolSettingsTests
    {
        [TestMethod]
        public void JsonSerialiation()
        {
            var settings = new PoolSettings();
            settings.TemperatureRunTime.Clear();
            settings.SummerPumpingCycles.Clear();
            settings.WinterPumpingCycles.Clear();

            settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 15, RunTimeHours = 1 });
            settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 20, RunTimeHours = 3 });
            settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 25, RunTimeHours = 10 });

            settings.SummerPumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(8), PumpCycleType = PumpCycleType.StopAt });
            settings.SummerPumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(13), PumpCycleType = PumpCycleType.StartAt });

            settings.WinterPumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(13), PumpCycleType = PumpCycleType.StartAt });

            var result = JsonConvert.DeserializeObject<PoolSettings>(JsonConvert.SerializeObject(settings));
            Assert.AreEqual(3, settings.TemperatureRunTime.Count);
            Assert.AreEqual(2, settings.SummerPumpingCycles.Count);
            Assert.AreEqual(1, settings.WinterPumpingCycles.Count);
        }

        [TestMethod]
        public void ConvertTemperatureToPumpDuration()
        {
            var settings = new PoolSettings();
            settings.TemperatureRunTime.Clear();
            settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 15, RunTimeHours = 1 });
            settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 20, RunTimeHours = 3 });
            settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 25, RunTimeHours = 10 });
            settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 30, RunTimeHours = 16 });

            Assert.AreEqual(1, settings.GetHoursPumpingTimePerDay(10));
            Assert.AreEqual(1, settings.GetHoursPumpingTimePerDay(15));
            Assert.AreEqual(1, settings.GetHoursPumpingTimePerDay(15.2));
            Assert.AreEqual(3, settings.GetHoursPumpingTimePerDay(20));
            Assert.AreEqual(10, settings.GetHoursPumpingTimePerDay(29.9));
            Assert.AreEqual(16, settings.GetHoursPumpingTimePerDay(30));
            Assert.AreEqual(16, settings.GetHoursPumpingTimePerDay(35));
        }


        [TestMethod]
        public void CalculateCycles()
        {
            var settings = new PoolSettings();
            settings.SummerPumpingCycles.Clear();

            settings.SummerPumpingCycles.Add(new PumpCycleSetting()
            {
                DecisionTime = TimeSpan.FromHours(8),
                PumpCycleType = PumpCycleType.StopAt,
                ChlorineInhibition = true,
                PhRegulationInhibition = true,
            });

            settings.SummerPumpingCycles.Add(new PumpCycleSetting()
            {
                DecisionTime = TimeSpan.FromHours(13),
                PumpCycleType = PumpCycleType.StartAt,
                ChlorineInhibition = false,
                PhRegulationInhibition = true,
            });

            settings.SummerPumpingCycles.Add(new PumpCycleSetting()
            {
                DecisionTime = TimeSpan.FromHours(22),
                PumpCycleType = PumpCycleType.StartAt,
                ChlorineInhibition = true,
                PhRegulationInhibition = false,
            });

            settings.WinterPumpingCycles.Add(new PumpCycleSetting()
            {
                DecisionTime = TimeSpan.FromHours(5),
                PumpCycleType = PumpCycleType.StopAt,
                ChlorineInhibition = false,
                PhRegulationInhibition = false,
            });

            // ======== SUMMER MODE ============
            settings.WorkingMode = PoolWorkingMode.Summer;
            var time = DateTime.Now.Date;
            var result = settings.GetNextPumpCycles(time, TimeSpan.FromHours(3), 3).ToList();
            Assert.AreEqual(9, result.Count);
            Assert.AreEqual(time.AddHours(7), result[0].StartTime);
            Assert.AreEqual(time.AddHours(8), result[0].EndTime);
            Assert.IsTrue(result[0].ChlorineInhibition);
            Assert.IsTrue(result[0].PhRegulationInhibition);

            Assert.AreEqual(time.AddHours(13), result[1].StartTime);
            Assert.AreEqual(time.AddHours(14), result[1].EndTime);
            Assert.IsFalse(result[1].ChlorineInhibition);
            Assert.IsTrue(result[1].PhRegulationInhibition);

            Assert.AreEqual(time.AddHours(22), result[2].StartTime);
            Assert.AreEqual(time.AddHours(23), result[2].EndTime);
            Assert.IsTrue(result[2].ChlorineInhibition);
            Assert.IsFalse(result[2].PhRegulationInhibition);

            // Query during 1st cycle
            result = settings.GetNextPumpCycles(time.AddHours(7), TimeSpan.FromHours(3), 3).ToList();
            Assert.AreEqual(9, result.Count);
            Assert.AreEqual(time.AddHours(7), result[0].StartTime);
            Assert.AreEqual(time.AddHours(8), result[0].EndTime);

            // Query between 1st and 2nd cycle
            result = settings.GetNextPumpCycles(time.AddHours(12), TimeSpan.FromHours(3), 3).ToList();
            Assert.AreEqual(8, result.Count);
            Assert.AreEqual(time.AddHours(13), result[0].StartTime);
            Assert.AreEqual(time.AddHours(14), result[0].EndTime);

            // Query between 2nd and 3rd cycle
            result = settings.GetNextPumpCycles(time.AddHours(15), TimeSpan.FromHours(3), 3).ToList();
            Assert.AreEqual(7, result.Count);
            Assert.AreEqual(time.AddHours(22), result[0].StartTime);
            Assert.AreEqual(time.AddHours(23), result[0].EndTime);

            // Query after 3rd cycle
            result = settings.GetNextPumpCycles(time.AddHours(23.5), TimeSpan.FromHours(3), 3).ToList();
            Assert.AreEqual(6, result.Count);
            Assert.AreEqual(time.AddDays(1).AddHours(7), result[0].StartTime);
            Assert.AreEqual(time.AddDays(1).AddHours(8), result[0].EndTime);

            // ======== WINTER MODE ============
            settings.WorkingMode = PoolWorkingMode.Winter;
            result = settings.GetNextPumpCycles(time, TimeSpan.FromHours(1), 3).ToList();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(time.AddHours(4), result[0].StartTime);
            Assert.AreEqual(time.AddHours(5), result[0].EndTime);
            Assert.IsFalse(result[0].ChlorineInhibition);
            Assert.IsFalse(result[0].PhRegulationInhibition);

            Assert.AreEqual(time.AddDays(1).AddHours(4), result[1].StartTime);
            Assert.AreEqual(time.AddDays(1).AddHours(5), result[1].EndTime);

            Assert.AreEqual(time.AddDays(2).AddHours(4), result[2].StartTime);
            Assert.AreEqual(time.AddDays(2).AddHours(5), result[2].EndTime);
        }
    }
}