//-----------------------------------------------------------------------
// <copyright file="SampleValue.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a sample
    /// </summary>
    public class SampleValue<TValue>
    {
        public SampleValue(DateTime time, TValue value)
        {
            this.Time = time;
            this.Value = value;
        }

        public DateTime Time { get; set; }

        public TValue Value { get; set; }

        public void UpdateValue(TValue value)
        {
            this.Time = SystemTime.Now;
            this.Value = value;
        }

        public void UpdateValueOnly(TValue value)
        {
            this.Value = value;
        }
    }
}