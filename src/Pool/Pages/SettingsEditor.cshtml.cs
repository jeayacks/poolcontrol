using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pool.Control;
using Pool.Control.Store;
using Pool.Hardware;

namespace Pool.Pages
{
    public class SettingsEditorModel : PageModel
    {
        private PoolControl poolControl;
        public SettingsEditorModel(PoolControl poolControl)
        {
            this.poolControl = poolControl;
        }

        public bool SummerMode { get; set; }

        public string JsonSettings { get; set; }

        public void OnGet()
        {
            var settings = this.poolControl.GetPoolSettings();

            this.JsonSettings = Newtonsoft.Json.JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
        }


        public void OnPost()
        {
            this.SaveSettings();
        }


        public void SaveSettings()
        {
            string value = Request.Form[nameof(JsonSettings)];

            var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<PoolSettings>(value);

            this.poolControl.SavePoolSettings(settings);

            this.Redirect("/Settings");
        }

    }
}