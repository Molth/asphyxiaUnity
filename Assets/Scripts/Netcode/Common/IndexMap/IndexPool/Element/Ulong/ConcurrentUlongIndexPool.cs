//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

using System.Runtime.CompilerServices;
#if UNITY_2021_3_OR_NEWER
using System.Threading;
#endif

namespace Erinn
{
    /// <summary>
    ///     IndexList
    /// </summary>
    public sealed class ConcurrentUlongIndexPool : ConcurrentIndexPool<ulong>
    {
        /// <summary>
        ///     Current index
        /// </summary>
        private ulong _index;

        /// <summary>
        ///     Pop up index
        /// </summary>
        /// <returns>New index obtained</returns>
        protected override ulong OnRent()
        {
            var index = _index;
            Interlocked.Add(ref Unsafe.As<ulong, long>(ref _index), 1L);
            return index;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        protected override void OnClear() => _index = 0UL;
    }
}