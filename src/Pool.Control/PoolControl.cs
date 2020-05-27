//-----------------------------------------------------------------------
// <copyright file="PoolControl.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control
{
    using System;
    using System.Linq;
    using System.Threading;
    using Microsoft.Extensions.Logging;
    using Pool.Control.Store;
    using Pool.Hardware;

    /// <summary>
    /// Execute the main loop.
    /// </summary>
    public class PoolControl
    {
        /// <summary>
        /// General settings file
        /// </summary>
        private const string settingsFileName = "settings.json";

        /// <summary>
        /// Store the current values
        /// </summary>
        private const string currentStateFileName = "system-states.json";

        /// <summary>
        /// The logger
        /// </summary>
        private ILogger<PoolControl> logger;

        /// <summary>
        /// Hardware layer.
        /// </summary>
        private IHardwareManager hardwareManager;

        /// <summary>
        /// The setting store.
        /// </summary>
        private IStoreService storeService;
        /// <summary>
        /// Loop delay
        /// </summary>
        private int loopDelay = 1000;

        /// <summary>
        /// The general settings
        /// </summary>
        private PoolSettings poolSettings;

        /// <summary>
        /// The current states
        /// </summary>
        private SystemState systemState;

        /// <summary>
        /// Use to execute actions.
        /// </summary>
        private PoolControlPump poolControlLoop;

        /// <summary>
        /// The cover control.
        /// </summary>
        private PoolControlCover coverControl;

        /// <summary>
        /// Time of last save.
        /// </summary>
        private DateTime lastSystemStateSave = DateTime.Now;

        /// <summary>
        /// Initializes a new instance of the <see cref="PoolControl"/> class.
        /// </summary>
        public PoolControl(
            ILogger<PoolControl> logger,
            IHardwareManager hardwareManager,
            IStoreService storeService,
            int loopDelay = 1000)
        {
            this.logger = logger;
            this.hardwareManager = hardwareManager;
            this.storeService = storeService;
            this.loopDelay = loopDelay;
        }

        /// <summary>
        /// Gets the current state with output values.
        /// </summary>
        /// <returns></returns>
        public PoolControlInfo GetPoolControlInformation()
        {
            return new PoolControlInfo()
            {
                SystemState = this.systemState,
                Outputs = this.hardwareManager.GetOutputs().ToArray(),
                PumpCycles = this.poolControlLoop.GetCyclesInfo().ToArray(),
                PoolSettings = this.poolSettings,
            };
        }

        /// <summary>
        /// Gets the current settings.
        /// </summary>
        /// <returns></returns>
        public PoolSettings GetPoolSettings()
        {
            return this.poolSettings;
        }

        /// <summary>
        /// Change the settings.
        /// </summary>
        /// <returns></returns>
        public void SavePoolSettings(PoolSettings settings)
        {
            if (settings.SummerPumpingCycles.Count == 0 || settings.WinterPumpingCycles.Count == 0)
            {
                throw new ArgumentException("Invalid cycles");
            }

            this.poolSettings.CoverCylcleDurationInSeconds = settings.CoverCylcleDurationInSeconds;
            this.poolSettings.SummerPumpingCycles = settings.SummerPumpingCycles;
            this.poolSettings.WinterPumpingCycles = settings.WinterPumpingCycles;
            this.poolSettings.WorkingMode = settings.WorkingMode;
            this.poolSettings.TemperatureRunTime = settings.TemperatureRunTime;
            this.storeService.WritePoolSettings(this.poolSettings, settingsFileName);

            this.poolControlLoop.ResetSettings();
        }

        /// <summary>
        /// Open cover.
        /// </summary>
        public void OpenCover()
        {
            this.coverControl.OpenCover(false);
        }

        /// <summary>
        /// Close cover.
        /// </summary>
        public void CloseCover()
        {
            this.coverControl.CloseCover(false);
        }

        /// <summary>
        /// Stop cover.
        /// </summary>
        public void StopCover()
        {
            this.coverControl.StopCover();
        }

        /// <summary>
        /// Execute an infinite loop for the control.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void Execute(CancellationToken cancellationToken)
        {
            this.LoadSettingsAndStates();

            this.poolControlLoop = new PoolControlPump(this.poolSettings, this.systemState, this.hardwareManager);
            this.coverControl = new PoolControlCover(this.poolSettings, this.systemState, this.hardwareManager);

            this.hardwareManager.OpenConfiguration();
            this.hardwareManager.BooleanInputChanged += HardwareManager_BooleanInputChanged;

            this.logger.LogInformation("Starting control loop...");
            this.logger.LogInformation($"Using loop delay = {this.loopDelay} ms");

            // Loop until exit asked
            while (!cancellationToken.WaitHandle.WaitOne(this.loopDelay))
            {
                lock (this.poolControlLoop)
                {
                    // Pump and temperature processing
                    this.poolControlLoop.Process();
                }

                // Covers control
                lock (this.coverControl)
                {
                    this.coverControl.Idle();
                }

                // Persist current values every hour
                if (this.lastSystemStateSave.AddHours(1) < DateTime.Now)
                {
                    this.PersistSystemSate();
                }
            }

            // Close hardware
            this.hardwareManager.BooleanInputChanged -= HardwareManager_BooleanInputChanged;
            this.hardwareManager.CloseConfiguration();

            // Persist changes of system state
            this.PersistSystemSate();

            this.logger.LogInformation("Control loop ended.");
        }


        private void HardwareManager_BooleanInputChanged(object sender, BooleanInputChangeEventArgs e)
        {
            this.logger.LogDebug($"Input received {e.Pin} = {e.State}");

            lock (this.coverControl)
            {
                if (e.Pin == PinName.CoverButtonUp && e.State == true)
                {
                    this.coverControl.OpenCover(true);
                }

                if (e.Pin == PinName.CoverButtonDown && e.State == true)
                {
                    this.coverControl.CloseCover(true);
                }
            }
        }

        /// <summary>
        /// Load settings and state from local files
        /// </summary>
        private void LoadSettingsAndStates()
        {
            this.logger.LogDebug($"Reading '{settingsFileName}' file");
            this.poolSettings = this.storeService.ReadPoolSettings(settingsFileName);

            this.logger.LogDebug($"Reading '{currentStateFileName}' file");
            this.systemState = this.storeService.ReadSystemState(currentStateFileName);
        }

        private void PersistSystemSate()
        {
            this.logger.LogDebug($"Saving '{currentStateFileName}' file");
            this.storeService.WriteSystemState(this.systemState, currentStateFileName);
            this.lastSystemStateSave = DateTime.Now;
        }
    }
}