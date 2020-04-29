//-----------------------------------------------------------------------
// <copyright file="SystemTime.cs" company="JeYacks">
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
    /// Used for getting DateTime.Now(), time is changeable for unit testing
    /// </summary>
    public static class SystemTime
    {
        private static DateTime? mockValue = null;

        /// <summary> Normally this is a pass-through to DateTime.Now, but it can be overridden with SetDateTime( .. ) for testing or debugging.
        /// </summary>
        public static DateTime Now
        {
            get
            {
                return mockValue.HasValue ? mockValue.Value : DateTime.Now;
            }
        }

        /// <summary> Set time to return when SystemTime.Now() is called.
        /// </summary>
        public static void SetDateTime(DateTime dateTimeNow)
        {
            mockValue= dateTimeNow;
        }

        /// <summary> Resets SystemTime.Now() to return DateTime.Now.
        /// </summary>
        public static void ResetDateTime()
        {
            mockValue = null;
        }
    }
}