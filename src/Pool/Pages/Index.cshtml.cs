using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Pool.Control;
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


        }

        public GeneralState GeneralState { get; set; }

        public PoolControlInfo State { get; set; }

        public void OnGet()
        {
        }
    }
}
