//-----------------------------------------------------------------------
// <copyright file="HardwareOutputState.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Hardware
{
    using System;

    /// <summary>
    /// Output status
    /// </summary>
    public class HardwareOutputState 
    {
        public HardwareOutputState(PinName pin, string description)
        {
            this.Output = pin;
            this.PinName = pin.ToString();
            this.Description = description;
            this.State = false;
        }

        /// <summary>
        /// The logical pin name.
        /// </summary>
        public PinName Output { get; private set; }

        /// <summary>
        /// The logical pin name.
        /// </summary>
        public string PinName { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The pin state.
        /// </summary>
        public bool State { get;  set; }
    }
}
