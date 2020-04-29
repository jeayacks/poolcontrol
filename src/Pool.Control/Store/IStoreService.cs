//-----------------------------------------------------------------------
// <copyright file="IStoreService.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control.Store
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>
    /// Read and write settings to disk.
    /// </summary>
    public interface IStoreService
    {
        /// <summary>
        /// Read settings
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        PoolSettings ReadPoolSettings(string fileName);

        /// <summary>
        /// Write the pool settings
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="fileName">File name</param>
        void WritePoolSettings(PoolSettings settings, string fileName);

        /// <summary>
        /// Read system state
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        SystemState ReadSystemState(string fileName);

        /// <summary>
        /// Write the system state
        /// </summary>
        /// <param name="state">Settings</param>
        /// <param name="fileName">File name</param>
        void WriteSystemState(SystemState state, string fileName);
    }
}