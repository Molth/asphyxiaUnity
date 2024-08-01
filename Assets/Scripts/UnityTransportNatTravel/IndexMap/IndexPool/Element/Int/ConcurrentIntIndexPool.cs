//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER
using System.Threading;
#endif

namespace Erinn
{
    /// <summary>
    ///     IndexList
    /// </summary>
    public sealed class ConcurrentIntIndexPool : ConcurrentIndexPool<int>
    {
        /// <summary>
        ///     Current index
        /// </summary>
        private int _index;

        /// <summary>
        ///     Pop up index
        /// </summary>
        /// <returns>New index obtained</returns>
        protected override int OnRent()
        {
            var index = _index;
            Interlocked.Add(ref _index, 1);
            return index;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        protected override void OnClear() => _index = 0;
    }
}