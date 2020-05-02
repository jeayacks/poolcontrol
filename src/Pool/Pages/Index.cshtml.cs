using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Highsoft.Web.Mvc.Charts;
using Highsoft.Web.Mvc.Charts.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Pool.Control;
using Pool.Control.Store;
using Pool.Controllers;
using Pool.Hardware;

namespace Pool.Pages
{
    public class IndexModel : PageModel
    {

        public IndexModel(PoolControl poolControl)
        {
            this.State = poolControl.GetPoolControlInformation();
            this.GeneralState = GeneralState.ConvertFromPoolState(poolControl.GetPoolControlInformation());
            this.TemperatureRunTime = this.State.PoolSettings.TemperatureRunTime.ToArray();
            this.PumpingCycles = this.State.PoolSettings.WorkingMode == PoolWorkingMode.Summer
                ? this.State.PoolSettings.SummerPumpingCycles
                : this.State.PoolSettings.WinterPumpingCycles;


            var chartOptions = new Highcharts
            {
                Title = new Title
                {
                    Text = "Temps de pompage"
                },
                XAxis = new List<XAxis>() { new XAxis() { Title = new XAxisTitle() { Text = "Température" } } },
                YAxis = new List<YAxis>() { new YAxis() { Title = new YAxisTitle() { Text = "heures" } } },
                Series = new List<Series>()
                {
                    new AreaSeries
                    {
                        Name = "Temps",
                        Data = this.State.PoolSettings.TemperatureRunTime
                            .Select(d=> new AreaSeriesData(){X = d.Temperature, Y=d.RunTimeHours})
                            .ToList(),
                    }
                }
            };

            chartOptions.ID = "chart";
            this.ChartRenderer = new HighchartsRenderer(chartOptions);
        }

        public GeneralState GeneralState { get; set; }

        public PoolControlInfo State { get; set; }

        public TemperatureRunTime[] TemperatureRunTime { get; set; }

        public List<PumpCycleGroupSetting> PumpingCycles { get; set; }

        public HighchartsRenderer ChartRenderer { get; set; }

        public void OnGet()
        {
        }
    }
}
