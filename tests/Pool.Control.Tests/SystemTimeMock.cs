//-----------------------------------------------------------------------
// <copyright file="SystemTimeMock.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Newtonsoft.Json;

    /// <summary>
    /// Used for getting DateTime.Now(), time is changeable for unit testing
    /// </summary>
    public class SystemTimeMock : IDisposable
    {
        FieldInfo field;

        public SystemTimeMock()
        {
            this.field = typeof(SystemTime).GetField("mockValue", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public void Set(DateTime value)
        {
            this.field.SetValue(null, value);
        }

        public void Dispose()
        {
            this.field.SetValue(null, null);
            SystemTime.ResetDateTime();
        }
    }
}