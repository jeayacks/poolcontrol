//-----------------------------------------------------------------------
// <copyright file="BooleanInputChangeEventArgs.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Hardware
{
    using System;

    /// <summary>
    /// Changes about the input.
    /// </summary>
    public class BooleanInputChangeEventArgs : EventArgs
    {
        public BooleanInputChangeEventArgs(PinName pin, bool state)
        {
            this.Pin = pin;
            this.State = state;
        }

        /// <summary>
        /// The logical pin number.
        /// </summary>
        public PinName Pin { get; private set; }

        /// <summary>
        /// The pin state.
        /// </summary>
        public bool State { get; private set; }
    }
}
