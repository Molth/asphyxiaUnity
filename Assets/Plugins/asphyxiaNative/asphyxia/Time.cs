//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

using System.Diagnostics;

namespace asphyxia
{
    /// <summary>
    ///     Time
    /// </summary>
    internal static class Time
    {
        /// <summary>
        ///     Current
        /// </summary>
        public static uint Current => (uint)(Stopwatch.GetTimestamp() / 10000L);
    }
}