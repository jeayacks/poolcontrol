//-----------------------------------------------------------------------
// <copyright file="InputBooleanChangeArgs.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Hardware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Changes about the input.
    /// </summary>
    public class HardwarePinChangeEventArgs :EventArgs
    {
        public HardwarePinChangeEventArgs(int pin, bool state)
        {
            this.Pin = pin;
            this.State = state;
        }

        /// <summary>
        /// The logical pin number.
        /// </summary>
        public int Pin { get; private set; }

        /// <summary>
        /// The pin state.
        /// </summary>
        public bool State { get; private set; }
    }
}
