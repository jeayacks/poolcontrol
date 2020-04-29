//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Pool.Hardware;
    using Serilog;
    using Serilog.Events;

    public class Program
    {
        public static int Main(string[] args)
        {
            Console.WriteLine($"Starting...");

            DisplayLogo();

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version.ToString();
            Console.WriteLine("Pool control system by JeYacks");
            Console.WriteLine($"Version {version}");
            Console.WriteLine($"Starting application");
            Console.WriteLine("---------------------------------------------------------");


            Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
              .Enrich.FromLogContext()
              .WriteTo.Console()
              .CreateLogger();

            Log.Information("Starting web host");

            var parser = CommandLineParser.FromArguments(args);

            if (parser.Commands.Contains("run"))
            {
                // Start the web host
                Host.CreateDefaultBuilder(args)
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddSerilog();
                    })
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                    })
                    .UseSerilog()
                    .UseConsoleLifetime()
                    .Build()
                    .Run();

                return 0;
            }
            else if (parser.Flags.Contains("diag"))
            {
                RaspberryDiagnostic.RunAndDisplay();
                return 0;
            }

            PrintUsage();
            return -1;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("pool run: Run the pool server.");
            Console.WriteLine("pool --help : Display this message.");
            Console.WriteLine("pool --diag : Execute a diagnostic and return.");
        }

        /// <summary>
        /// Display logo in ASCII mode
        /// </summary>
        /// <param name="value"></param>
        private static void DisplayLogo()
        {
            var text = @"

   ___       __   __              _         
  |_  |      \ \ / /             | |        
    | |  ___  \ V /   __ _   ___ | | __ ___ 
    | | / _ \  \ /   / _` | / __|| |/ // __|
/\__/ /|  __/  | |  | (_| || (__ |   < \__ \
\____/  \___|  \_/   \__,_| \___||_|\_\|___/
                                                      
";
            var lines = text.Split('\n');
            foreach (var l in lines)
            {
                Console.WriteLine(l);
            }
        }
    }
}
