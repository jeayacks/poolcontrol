using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Pool.Control;
using Pool.Hardware;

namespace Pool.Pages
{
    public class SettingsModel : PageModel
    {
        private PoolControl poolControl;

        public SettingsModel(PoolControl poolControl, IHardwareManager hardwareManager)
        {
            this.poolControl = poolControl;

            this.LoadData();
        }

        public bool SummerMode { get; set; }

        public void ChangeMode()
        {
            var settings = this.poolControl.GetPoolSettings();

            if (settings.WorkingMode == Control.Store.PoolWorkingMode.Summer)
            {
                settings.WorkingMode = Control.Store.PoolWorkingMode.Winter;
            }
            else
            {
                settings.WorkingMode = Control.Store.PoolWorkingMode.Summer;
            }

            this.poolControl.SavePoolSettings(settings);

            this.LoadData();
        }

        private void LoadData()
        {
            var settings = this.poolControl.GetPoolSettings();

            this.SummerMode = settings.WorkingMode == Control.Store.PoolWorkingMode.Summer;
        }

        public void OnPost()
        {
            this.ChangeMode();
        }
    }
}