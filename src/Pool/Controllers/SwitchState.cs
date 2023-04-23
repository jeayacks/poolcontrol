//-----------------------------------------------------------------------
// <copyright file="SwitchState.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Controllers
{
    /// <summary>
    /// Represents a switch state
    /// </summary>
    public class SwitchState
    {
        /// <summary>
        /// True if the switch is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// The optional int value
        /// </summary>
        public int Value { get; set; }
    }
}