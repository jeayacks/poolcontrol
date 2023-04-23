using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc.RazorPages;

using Pool.Control;
using Pool.Hardware;

namespace Pool.Pages
{
    public class VariablesModel : PageModel
    {
        public VariablesModel(PoolControl poolControl, IHardwareManager hardwareManager)
        {
            var state = poolControl.GetPoolControlInformation().SystemState;
            this.Outputs = hardwareManager.GetOutputs().ToArray();

            this.Variables = new List<VariableItem>();
            this.Variables.Add(VariableItem.Create("Température extérieure", state.AirTemperature));
            this.Variables.Add(VariableItem.Create("Température eau brute (sonde)", state.WaterTemperature));
            this.Variables.Add(VariableItem.Create("Température min jour", state.PoolTemperatureMinOfTheDay));
            this.Variables.Add(VariableItem.Create("Température max jour", state.PoolTemperatureMaxOfTheDay));
            this.Variables.Add(VariableItem.Create("Température de consigne", state.PoolTemperatureDecision));
            this.Variables.Add(VariableItem.Create("Température du bassin", state.PoolTemperature));
            this.Variables.Add(VariableItem.Create("Durée de filtration/jour", state.PumpingDurationPerDayInHours));
            this.Variables.Add(VariableItem.Create("Pompe", state.Pump));
            this.Variables.Add(VariableItem.Create("Arosage automatique", state.WateringScheduleEnabled));
            this.Variables.Add(VariableItem.Create("Durée d'arosage automatique", state.WateringScheduleDuration));
            this.Variables.Add(VariableItem.Create("Arosage manuel", state.WateringManualOn));
            this.Variables.Add(VariableItem.Create("Durée d'arosage manuel", state.WateringManualDuration));
        }

        public HardwareOutputState[] Outputs { get; set; }

        public List<VariableItem> Variables { get; set; }

        public void OnGet()
        {
        }

        public class VariableItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Date { get; set; }

            public static VariableItem Create(string name, SampleValue<double> value)
            {
                return new VariableItem() { Name = name, Value = value.Value.ToString(), Date = value.Time.ToString() };
            }

            public static VariableItem Create(string name, SampleValue<int> value)
            {
                return new VariableItem() { Name = name, Value = value.Value.ToString(), Date = value.Time.ToString() };
            }

            public static VariableItem Create(string name, SampleValue<bool> value)
            {
                return new VariableItem() { Name = name, Value = value.Value ? "On" : "Off", Date = value.Time.ToString() };
            }
        }
    }
}