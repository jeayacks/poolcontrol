//-----------------------------------------------------------------------
// <copyright file="PoolControlCover.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control
{
    using System;
    using System.Threading;
    using Pool.Control.Store;
    using Pool.Hardware;

    /// <summary>
    /// Control the cover.
    /// </summary>
    public class PoolControlCover
    {
        /// <summary>
        /// The general settings
        /// </summary>
        private PoolSettings poolSettings;

        /// <summary>
        /// The current states
        /// </summary>
        private SystemState systemState;

        /// <summary>
        /// Hardware layer.
        /// </summary>
        private IHardwareManager hardwareManager;

        /// <summary>
        /// The last cover action
        /// </summary>
        private DateTime? lastAction;

        /// <summary>
        /// True if cover is moving.
        /// </summary>
        private bool coverActionInProgress = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PoolControlCover"/> class.
        /// </summary>
        public PoolControlCover(
            PoolSettings poolSettings,
            SystemState systemState,
            IHardwareManager hardwareManager)
        {
            this.poolSettings = poolSettings;
            this.systemState = systemState;
            this.hardwareManager = hardwareManager;
        }

        /// <summary>
        /// Stop actions if required.
        /// </summary>
        public void Idle()
        {
            if (this.lastAction != null)
            {
                if (this.lastAction.Value.AddSeconds(this.poolSettings.CoverCylcleDurationInSeconds) < SystemTime.Now)
                {
                    this.StopCover();
                }
            }
        }

        public void OpenCover(bool stopIfInProgress)
        {
            if (this.coverActionInProgress && stopIfInProgress)
            {
                this.StopCover();
            }
            else
            {
                if (this.coverActionInProgress)
                {
                    this.StopCover();
                    Thread.Sleep(800);
                }

                this.coverActionInProgress = true;
                this.lastAction = SystemTime.Now;

                this.hardwareManager.Write(PinName.CoverPowerInverter, false);
                Thread.Sleep(200);
                this.hardwareManager.Write(PinName.CoverPowerSupply, true);
                Thread.Sleep(500);
            }
        }

        public void CloseCover(bool stopIfInProgress)
        {
            if (this.coverActionInProgress && stopIfInProgress)
            {
                this.StopCover();
            }
            else
            {
                if (this.coverActionInProgress)
                {
                    this.StopCover();
                    Thread.Sleep(800);
                }

                this.coverActionInProgress = true;
                this.lastAction = SystemTime.Now;

                this.hardwareManager.Write(PinName.CoverPowerInverter, true);
                Thread.Sleep(200);
                this.hardwareManager.Write(PinName.CoverPowerSupply, true);
                Thread.Sleep(500);
            }
        }

        public void StopCover()
        {
            this.coverActionInProgress = false;
            this.lastAction = null;

            this.hardwareManager.Write(PinName.CoverPowerSupply, false);
            Thread.Sleep(300);
            this.hardwareManager.Write(PinName.CoverPowerInverter, false);
        }
    }
}