//-----------------------------------------------------------------------
// <copyright file="Startup.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.HttpsPolicy;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Pool.Control;
    using Pool.Hardware;
    using Serilog;
    using Microsoft.OpenApi.Models;
    using Microsoft.AspNetCore.Mvc.Razor;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // The configuration
            services.Configure<HardwareConfiguration>(this.Configuration.GetSection("Hardware"));

            // Services
            services.AddSingleton<IHardwareManager, HardwareManager>();
            services.AddSingleton<Control.Store.IStoreService, Control.Store.StoreService>();
            services.AddSingleton<PoolControl>();
            services.AddSingleton<IHostedService, MainBackgroundService>();

#if DEBUG
            // Driver
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddTransient<IHardwareDriver, MockHardwareDriver>();
            }
            else
            {
                services.AddTransient<IHardwareDriver, RaspberryDriver>();
            }
#else
                services.AddTransient<IHardwareDriver, RaspberryDriver>();
#endif
            // Web site
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddRazorPages()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            services.AddControllers();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.IncludeXmlComments(this.GetType().Assembly);
                c.IncludeXmlComments(typeof(HardwareManager).Assembly);
                c.IncludeXmlComments(typeof(PoolControl).Assembly);

                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Swimming pool APIs by JeYacks", Version = "v1" });

#if DEBUG
                // In debug (for Api proxy generation), sets the method name as operation id
                c.CustomOperationIds(e => e.ActionDescriptor.RouteValues["action"].Replace("Async", string.Empty));

                c.SwaggerDoc(
                    "client",
                    new OpenApiInfo
                    {
                        Title = "Internal APIs - Client",
                        Description = "APIs of the client application, Swagger definitions enabled in debug mode only",
                        Version = "v1",
                    });
#endif
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseSerilogRequestLogging();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            var supportedCultures = new[] { "en", "fr" };
            var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            app.UseRequestLocalization(localizationOptions);

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "api";
#if DEBUG
                c.SwaggerEndpoint("../swagger/client/swagger.json", "Internal APIs - Client");
#endif
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "APIs");
                c.DisplayOperationId();
            });
        }
    }
}