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

                settings.SummerPumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(5), PumpCycleType = PumpCycleType.StopAt });
                settings.SummerPumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(11), PumpCycleType = PumpCycleType.StartAt });
                settings.SummerPumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(18), PumpCycleType = PumpCycleType.StopAt });

                settings.WinterPumpingCycles.Add(new PumpCycleSetting() { DecisionTime = TimeSpan.FromHours(4), PumpCycleType = PumpCycleType.StartAt });

                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 15, RunTimeHours = 1 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 18, RunTimeHours = 2 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 20, RunTimeHours = 3 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 21, RunTimeHours = 4 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 22, RunTimeHours = 5 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 23, RunTimeHours = 8 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 25, RunTimeHours = 10 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 26, RunTimeHours = 12 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 28, RunTimeHours = 14 });
                settings.TemperatureRunTime.Add(new TemperatureRunTime() { Temperature = 30, RunTimeHours = 16 });

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
