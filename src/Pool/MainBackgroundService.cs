//-----------------------------------------------------------------------
// <copyright file="MainBackgroundService.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Pool.Control;

    /// <summary>
    /// Background task, just call the control loop
    /// </summary>
    public class MainBackgroundService : BackgroundService
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private ILogger<MainBackgroundService> logger;

        /// <summary>
        /// The main loop control.
        /// </summary>
        private PoolControl control;

        public MainBackgroundService(ILogger<MainBackgroundService> logger, PoolControl control)
        {
            this.logger = logger;
            this.control = control;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    this.control.Execute(cancellationToken);

                }
                catch (Exception ex)
                {
                    this.logger.LogCritical(ex, "Fatal error");

                    // Exit program
                    Process.GetCurrentProcess().Kill();

                }
            });
        }
    }
}
