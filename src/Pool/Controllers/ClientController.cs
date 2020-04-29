//-----------------------------------------------------------------------
// <copyright file="Startup.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Pool.Control;
    using Pool.Control.Store;
    using Pool.Hardware;
    using System;
    using System.Linq;

    [ApiController]
    [Route("api/client")]
    [ApiExplorerSettings(GroupName = "client")]
    public class ClientController : ControllerBase
    {
        private PoolControl poolControl;

        private IHardwareManager hardwareManager;

        public ClientController(PoolControl poolControl, IHardwareManager hardwareManager)
        {
            this.poolControl = poolControl;
            this.hardwareManager = hardwareManager;
        }

        [HttpGet("settings")]
        public PoolSettings GetPoolSettings()
        {
            return this.poolControl.GetPoolSettings();
        }

        [HttpPost("settings")]
        public void SavePoolSettings([FromBody]PoolSettings settings)
        {
            this.poolControl.SavePoolSettings(settings);
        }

        [HttpGet("states")]
        public GeneralState GetStates()
        {
            return GeneralState.ConvertFromPoolState(this.poolControl.GetPoolControlInformation());
        }
    }
}
