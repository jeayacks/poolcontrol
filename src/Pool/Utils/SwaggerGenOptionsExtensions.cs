//-----------------------------------------------------------------------
// <copyright file="SwaggerGenOptionsExtensions.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool
{
    using System;
    using System.IO;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// Swagger options extensions
    /// </summary>
    public static class SwaggerGenOptionsExtensions
    {
        /// <summary>
        /// Include XML comments of the specified assembly if file exists
        /// </summary>
        /// <param name="options">Swagger gen options</param>
        /// <param name="assembly">Assembly</param>
        public static void IncludeXmlComments(this SwaggerGenOptions options, Assembly assembly)
        {
            var commenFile = assembly.Location.Replace(".dll", ".xml", StringComparison.OrdinalIgnoreCase);
            if (File.Exists(commenFile))
            {
                options.IncludeXmlComments(commenFile);
            }
        }
    }
}
