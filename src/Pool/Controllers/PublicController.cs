//-----------------------------------------------------------------------
// <copyright file="PublicController.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Pool.Control;
    using Pool.Hardware;
    using System;
    using System.Linq;

    [ApiController]
    [Route("api/v1")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class PublicController : ControllerBase
    {
        private ILogger<PublicController> logger;

        private PoolControl poolControl;

        private IHardwareManager hardwareManager;

        public PublicController(
            ILogger<PublicController> logger,
            PoolControl poolControl,
            IHardwareManager hardwareManager)
        {
            this.logger = logger;
            this.poolControl = poolControl;
            this.hardwareManager = hardwareManager;
        }

        [HttpGet("status")]
        public GeneralState Get()
        {
            this.logger.LogDebug("GET api/v1/status");

            return GeneralState.ConvertFromPoolState(this.poolControl.GetPoolControlInformation());
        }

        [HttpGet("status/details")]
        public PoolControlInfo GetDetails()
        {
            this.logger.LogDebug("GET api/v1/status/details");

            return this.poolControl.GetPoolControlInformation();
        }

        [HttpGet("io/{name}")]
        public ActionResult<SwitchState> GetSwitchState([FromRoute] string name)
        {
            this.logger.LogDebug($"GET api/v1/io/{name}");

            // Return pins
            var output = this.hardwareManager
                .GetOutputs()
                .FirstOrDefault(o => o.PinName.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (output != null)
            {
                return new SwitchState() { Active = output.State };
            }

            var systemState = this.poolControl.GetPoolControlInformation().SystemState;

            switch (name)
            {
                case "PumpForceOff":
                    return new SwitchState() { Active = systemState.PumpForceOff.Value };
                case "PumpForceOn":
                    return new SwitchState() { Active = systemState.PumpForceOn.Value };
            }

            return this.BadRequest("Switch not found");
        }

        [HttpPost("io/{name}")]
        public ActionResult ChangeSwitchState([FromRoute] string name, [FromBody]SwitchState state)
        {
            this.logger.LogDebug($"POST api/v1/io/{name} state={state.Active}");

            var systemState = this.poolControl.GetPoolControlInformation().SystemState;
            var output = this.hardwareManager
                .GetOutputs()
                .FirstOrDefault(o => o.PinName.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (output != null)
            {
                switch (output.Output)
                {
                    case PinName.DeckLight:
                    case PinName.SwimmingPoolLigth:
                    case PinName.Watering:
                        this.hardwareManager.Write(output.Output, state.Active);
                        return this.Ok();
                }
            }

            switch (name)
            {
                case "PumpForceOff":
                    if (systemState.PumpForceOff.Value != state.Active)
                    {
                        systemState.PumpForceOff.UpdateValue(state.Active);
                        systemState.PumpForceOn.UpdateValue(false);
                    }
                    return this.Ok();

                case "PumpForceOn":
                    if (systemState.PumpForceOn.Value != state.Active)
                    {
                        systemState.PumpForceOn.UpdateValue(state.Active);
                        systemState.PumpForceOff.UpdateValue(false);
                    }
                    return this.Ok();
            }

            return this.BadRequest("Switch not found");
        }

        [HttpPost("action/{name}")]
        public ActionResult ExecuteAction([FromRoute] string name)
        {
            this.logger.LogDebug($"POST api/v1/action/{name}");

            switch (name)
            {
                case "OpenCover":
                    this.poolControl.OpenCover();
                    break;

                case "CloseCover":
                    this.poolControl.CloseCover();
                    break;
                case "StopCover":
                    this.poolControl.StopCover();
                    break;

                default:
                    return this.BadRequest("Command not found");
            }

            return this.Ok();
        }
    }
}
