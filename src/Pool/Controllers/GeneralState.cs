//-----------------------------------------------------------------------
// <copyright file="GeneralState.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Pool.Control;
using Pool.Hardware;
using System.Linq;

namespace Pool.Controllers
{
    /// <summary>
    /// Holds the system state
    /// </summary>
    public class GeneralState
    {
        /// <summary>
        /// Work in summer mode.
        /// </summary>
        public bool SummerMode { get; set; }

        /// <summary>
        /// Get text status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Water temperature without any filtering, even when pump is stopped
        /// </summary>
        public double WaterTemperature { get; set; }

        /// <summary>
        /// The current temperature of the pool, value is updated only if pump is running
        /// </summary>
        public double PoolTemperature { get; set; }

        /// <summary>
        /// Exterior temperature
        /// </summary>
        public double AirTemperature { get; set; }

        /// <summary>
        /// Temperature value used to calculate running time
        /// </summary>
        public double PoolTemperatureDecision { get; set; }

        /// <summary>
        /// Pumping time per day
        /// </summary>
        public double PumpingDurationPerDayInHours { get; set; }

        /// <summary>
        /// Pump running.
        /// </summary>
        public bool Pump { get; set; }

        /// <summary>
        /// Pump force on.
        /// </summary>
        public bool PumpForceOn { get; set; }

        /// <summary>
        /// Pump force off.
        /// </summary>
        public bool PumpForceOff { get; set; }

        /// <summary>
        /// Chlorine inhibition.
        /// </summary>
        public bool ChlorineInhibition { get; set; }

        /// <summary>
        /// PH Regulation Inhibition
        /// </summary>
        public bool PhRegulationInhibition { get; set; }

        /// <summary>
        /// Watering running.
        /// </summary>
        public bool Watering { get; set; }

        /// <summary>
        /// Swimming pool ligth on.
        /// </summary>
        public bool SwimmingPoolLigth { get; set; }

        /// <summary>
        /// Deck ligth on.
        /// </summary>
        public bool DeckLight { get; set; }

        public static GeneralState ConvertFromPoolState(PoolControlInfo state)
        {
            var systemState = state.SystemState;
            var outputs = state.Outputs;

            var result = new GeneralState()
            {
                SummerMode = state.PoolSettings.WorkingMode == Control.Store.PoolWorkingMode.Summer,
                WaterTemperature = systemState.WaterTemperature.Value,
                PoolTemperature = systemState.PoolTemperature.Value,
                AirTemperature = systemState.AirTemperature.Value,
                PoolTemperatureDecision = systemState.PoolTemperatureDecision.Value,
                PumpingDurationPerDayInHours = systemState.PumpingDurationPerDayInHours.Value,
                Pump = outputs.Single(o => o.Output == PinName.Pump).State,
                PumpForceOff = systemState.PumpForceOff.Value,
                PumpForceOn = systemState.PumpForceOn.Value,
                ChlorineInhibition = outputs.Single(o => o.Output == PinName.ChlorineInhibition).State,
                PhRegulationInhibition = outputs.Single(o => o.Output == PinName.PhRegulationInhibition).State,
                SwimmingPoolLigth = outputs.Single(o => o.Output == PinName.SwimmingPoolLigth).State,
                DeckLight = outputs.Single(o => o.Output == PinName.DeckLight).State,
                Watering = outputs.Single(o => o.Output == PinName.Watering).State,
            };

            if (result.PumpForceOn | result.PumpForceOff)
            {
                result.Status = "Mode manuel";
            }
            else
            {
                var pumpCycle = state.PumpCycles.FirstOrDefault();
                if (pumpCycle != null)
                {
                    if (result.Pump)
                    {
                        result.Status = string.Format("En cours jusqu'a {0:HH:mm}", pumpCycle.EndTime);
                    }
                    else
                    {
                        result.Status = string.Format("Prochain cycle de {0:HH:mm} a {1:HH:mm}", pumpCycle.StartTime, pumpCycle.EndTime);
                    }
                }
            }

            return result;
        }
    }
}
