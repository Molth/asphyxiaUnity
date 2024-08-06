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
    public sealed class ConcurrentUintIndexPool : ConcurrentIndexPool<uint>
    {
        /// <summary>
        ///     Current index
        /// </summary>
        private uint _index;

        /// <summary>
        ///     Pop up index
        /// </summary>
        /// <returns>New index obtained</returns>
        protected override uint OnRent()
        {
            var index = _index;
            Interlocked.Add(ref Unsafe.As<uint, int>(ref _index), 1);
            return index;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        protected override void OnClear() => _index = 0U;
    }
}