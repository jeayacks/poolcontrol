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
            this.SummerPumpingCycles = new List<PumpCycleSetting>();
            this.WinterPumpingCycles = new List<PumpCycleSetting>();
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
        public List<PumpCycleSetting> SummerPumpingCycles { get; set; }

        /// <summary>
        /// Gets or sets the winter cycles
        /// </summary>
        public List<PumpCycleSetting> WinterPumpingCycles { get; set; }

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
                result = this.TemperatureRunTime.First().RunTimeHours;
            }

            foreach (var item in this.TemperatureRunTime)
            {
                if (temperature >= item.Temperature)
                {
                    result = item.RunTimeHours;
                }
            }

            return result;
        }

        public IEnumerable<PumpCycle> GetNextPumpCycles(DateTime currentTime, TimeSpan pumpingDurationPerDay, int numberOfDays)
        {
            return this.WorkingMode == PoolWorkingMode.Summer
                ? GetNextPumpCycles(this.SummerPumpingCycles, currentTime, pumpingDurationPerDay, numberOfDays)
                : GetNextPumpCycles(this.WinterPumpingCycles, currentTime, pumpingDurationPerDay, numberOfDays);
        }

        private static IEnumerable<PumpCycle> GetNextPumpCycles(
            List<PumpCycleSetting> settings,
            DateTime currentTime,
            TimeSpan durationPerDay,
            int numberOfDays)
        {
            if (settings.Count != 0)
            {

                var durationInMinutes = Math.Round(durationPerDay.TotalMinutes / settings.Count);
                var duration = TimeSpan.FromMinutes(durationInMinutes);

                for (int i = 0; i < numberOfDays; i++)
                {
                    var time = currentTime.Date.AddDays(i);

                    foreach (var cycle in settings)
                    {
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
