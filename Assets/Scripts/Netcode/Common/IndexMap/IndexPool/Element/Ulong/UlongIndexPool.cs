//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     IndexList
    /// </summary>
    public sealed class UlongIndexPool : IndexPool<ulong>
    {
        /// <summary>
        ///     Current index
        /// </summary>
        private ulong _index;

        /// <summary>
        ///     Structure
        /// </summary>
        public UlongIndexPool()
        {
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public UlongIndexPool(int capacity) : base(capacity)
        {
        }

        /// <summary>
        ///     Pop up index
        /// </summary>
        /// <returns>New index obtained</returns>
        protected override ulong OnRent() => _index++;

        /// <summary>
        ///     Empty
        /// </summary>
        protected override void OnClear() => _index = 0UL;
    }
}