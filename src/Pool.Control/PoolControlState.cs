//-----------------------------------------------------------------------
// <copyright file="PoolControlState.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool.Control
{
    using Pool.Control.Store;
    using Pool.Hardware;

    /// <summary>
    /// Manages the pump.
    /// </summary>
    public class PoolControlInfo
    {
        public SystemState SystemState { get; set; }
        public PoolSettings PoolSettings { get; set; }
        public HardwareOutputState[] Outputs { get; set; }
        public PumpCycle[] PumpCycles { get; set; }
        public Cycle[] WateringCycles { get; set; }
    }
}