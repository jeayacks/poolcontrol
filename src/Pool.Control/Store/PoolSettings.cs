//-----------------------------------------------------------------------
// <copyright file="PoolSettings.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control.Store
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Host all pool settings
    /// </summary>
    public class PoolSettings
    {
        public PoolSettings()
        {
            this.WorkingMode = PoolWorkingMode.Summer;
            this.CoverCylcleDurationInSeconds = 90;
            this.SummerPumpingCycles = new List<PumpCycleGroupSetting>();
            this.WinterPumpingCycles = new List<PumpCycleGroupSetting>();
            this.FrostProtection = new FrostProtectionSetting();
            this.TemperatureRunTime = new List<TemperatureRunTime>();
        }

        /// <summary>
        /// Gets or sets the working mode
        /// </summary>
        public PoolWorkingMode WorkingMode { get; set; }

        /// <summary>
        /// Gets or sets the duration of cover operation 
        /// </summary>
        public int CoverCylcleDurationInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the summer cycles
        /// </summary>
        public List<PumpCycleGroupSetting> SummerPumpingCycles { get; set; }

        /// <summary>
        /// Gets or sets the winter cycles
        /// </summary>
        public List<PumpCycleGroupSetting> WinterPumpingCycles { get; set; }

        /// <summary>
        /// Gets or sets frost settings.
        /// </summary>
        public FrostProtectionSetting FrostProtection { get; set; }

        /// <summary>
        /// Gets or sets the puming time values
        /// </summary>
        public List<TemperatureRunTime> TemperatureRunTime { get; set; }


        /// <summary>
        /// Gets the number of pumping hours per day using temperature
        /// </summary>
        /// <param name="temperature">Temperature value.</param>
        /// <returns>Pumping duration</returns>
        public double GetHoursPumpingTimePerDay(double temperature)
        {
            double result = 0;

            if (this.TemperatureRunTime.Count > 0)
            {
                // Take the first
                result = this.TemperatureRunTime.First().RunTimeHours;

                // Find the higher temp item
                foreach (var item in this.TemperatureRunTime)
                {
                    if (temperature >= item.Temperature)
                    {
                        result = item.RunTimeHours;
                    }
                }
            }

            return result;
        }

        public IEnumerable<PumpCycle> GetNextPumpCycles(DateTime currentTime, double temperature, TimeSpan pumpingDurationPerDay, int numberOfDays)
        {
            return this.WorkingMode == PoolWorkingMode.Summer
                ? GetNextPumpCycles(this.SummerPumpingCycles, currentTime, temperature, pumpingDurationPerDay, numberOfDays)
                : GetNextPumpCycles(this.WinterPumpingCycles, currentTime, temperature, pumpingDurationPerDay, numberOfDays);
        }

        private static IEnumerable<PumpCycle> GetNextPumpCycles(
            List<PumpCycleGroupSetting> settings,
            DateTime currentTime,
            double temperature,
            TimeSpan durationPerDay,
            int numberOfDays)
        {
            if (settings.Count != 0)
            {
                // Find the group
                var groups = settings.OrderBy(g => g.MinimumTemperature).ToList();
                PumpCycleGroupSetting group = groups.LastOrDefault(g => g.MinimumTemperature <= temperature);
                if (group == null)
                {
                    group = groups.First();
                }

                if (group != null)
                {
                    // Correct ratio if required
                    foreach (var cycle in group.PumpingCycles)
                    {
                        if (cycle.Ratio < 0.5 || cycle.Ratio > 4)
                        {
                            cycle.Ratio = 1;
                        }
                    }

                    double totalRatio = group.PumpingCycles.Sum(g => g.Ratio);

                    for (int i = 0; i < numberOfDays; i++)
                    {
                        var time = currentTime.Date.AddDays(i);

                        foreach (var cycle in group.PumpingCycles)
                        {
                            var duration = TimeSpan.FromMinutes(Math.Round(durationPerDay.TotalMinutes * cycle.Ratio / totalRatio));

                            DateTime startTime = cycle.PumpCycleType == PumpCycleType.StartAt
                                ? time.Add(cycle.DecisionTime)
                                : time.Add(cycle.DecisionTime - duration);

                            DateTime endTime = cycle.PumpCycleType == PumpCycleType.StartAt
                                ? time.Add(cycle.DecisionTime + duration)
                                : time.Add(cycle.DecisionTime);

                            if (endTime > currentTime)
                            {
                                yield return new PumpCycle(
                                    startTime,
                                    endTime,
                                    cycle.ChlorineInhibition,
                                    cycle.PhRegulationInhibition);
                            }
                        }
                    }
                }
            }
        }
    }
}
