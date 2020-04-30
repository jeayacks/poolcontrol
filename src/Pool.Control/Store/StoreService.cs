//-----------------------------------------------------------------------
// <copyright file="StoreService.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control.Store
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>
    /// Read and write settings to disk.
    /// </summary>
    public class StoreService : IStoreService
    {
        public PoolSettings ReadPoolSettings(string fileName)
        {
            if (File.Exists(fileName))
            {
                return JsonConvert.DeserializeObject<PoolSettings>(File.ReadAllText(fileName));
            }
            else
            {
                var settings = new PoolSettings();

                var summerGroup1 = new PumpCycleGroupSetting() { MinimumTemperature = 0 };
                summerGroup1.PumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(13.3), PumpCycleType = PumpCycleType.StopAt });

                var summerGroup2 = new PumpCycleGroupSetting() { MinimumTemperature = 17 };
                summerGroup2.PumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(13.5), PumpCycleType = PumpCycleType.StartAt });
                summerGroup2.PumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(2), PumpCycleType = PumpCycleType.StartAt });

                var summerGroup3 = new PumpCycleGroupSetting() { MinimumTemperature = 20 };
                summerGroup3.PumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(5), PumpCycleType = PumpCycleType.StopAt });
                summerGroup3.PumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(13.5), PumpCycleType = PumpCycleType.StartAt });
                summerGroup3.PumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(20), PumpCycleType = PumpCycleType.StartAt });

                // > 23°
                var summerGroup4 = new PumpCycleGroupSetting() { MinimumTemperature = 23 };
                summerGroup4.PumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(5), PumpCycleType = PumpCycleType.StartAt });
                summerGroup4.PumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(11), PumpCycleType = PumpCycleType.StartAt });
                summerGroup4.PumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(17), PumpCycleType = PumpCycleType.StartAt });
                summerGroup4.PumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(23), PumpCycleType = PumpCycleType.StartAt });

                settings.SummerPumpingCycles.Add(summerGroup1);
                settings.SummerPumpingCycles.Add(summerGroup2);
                settings.SummerPumpingCycles.Add(summerGroup3);
                settings.SummerPumpingCycles.Add(summerGroup4);

                settings.WinterPumpingCycles.Add(new PumpCycleGroupSetting() { MinimumTemperature = 0 });
                settings.WinterPumpingCycles[0].PumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(5), PumpCycleType = PumpCycleType.StartAt });

                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 15, RunTimeHours = 1 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 18, RunTimeHours = 3 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 20, RunTimeHours = 4 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 21, RunTimeHours = 5 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 22, RunTimeHours = 6 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 23, RunTimeHours = 8 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 24, RunTimeHours = 9 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 25, RunTimeHours = 10 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 26, RunTimeHours = 12 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 28, RunTimeHours = 14 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 29, RunTimeHours = 16 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 30, RunTimeHours = 20 });

                return settings;
            }
        }

        public void WritePoolSettings(PoolSettings settings, string fileName)
        {
            File.WriteAllText(
                fileName,
                JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public SystemState ReadSystemState(string fileName)
        {
            if (File.Exists(fileName))
            {
                var value = JsonConvert.DeserializeObject<SystemState>(File.ReadAllText(fileName));

                // Reset volatile values (persisted in case of, but it is better to put with default)
                var defaultValue = new SystemState();
                value.AirTemperature = defaultValue.AirTemperature;
                value.WaterTemperature = defaultValue.WaterTemperature;
                value.Pump = defaultValue.Pump;

                return value;
            }
            else
            {
                return new SystemState();
            }
        }

        public void WriteSystemState(SystemState state, string fileName)
        {
            File.WriteAllText(
                fileName,
                JsonConvert.SerializeObject(state, Formatting.Indented));
        }
    }
}
