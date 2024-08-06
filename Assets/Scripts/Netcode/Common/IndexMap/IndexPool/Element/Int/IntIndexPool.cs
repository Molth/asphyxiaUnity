//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     IndexList
    /// </summary>
    public sealed class IntIndexPool : IndexPool<int>
    {
        /// <summary>
        ///     Current index
        /// </summary>
        private int _index;

        /// <summary>
        ///     Structure
        /// </summary>
        public IntIndexPool()
        {
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public IntIndexPool(int capacity) : base(capacity)
        {
        }

        /// <summary>
        ///     Pop up index
        /// </summary>
        /// <returns>New index obtained</returns>
        protected override int OnRent() => _index++;

        /// <summary>
        ///     Empty
        /// </summary>
        protected override void OnClear() => _index = 0;
    }
}