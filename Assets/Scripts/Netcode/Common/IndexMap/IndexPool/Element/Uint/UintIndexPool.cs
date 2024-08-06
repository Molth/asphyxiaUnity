//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     IndexList
    /// </summary>
    public sealed class UintIndexPool : IndexPool<uint>
    {
        /// <summary>
        ///     Current index
        /// </summary>
        private uint _index;

        /// <summary>
        ///     Structure
        /// </summary>
        public UintIndexPool()
        {
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public UintIndexPool(int capacity) : base(capacity)
        {
        }

        /// <summary>
        ///     Pop up index
        /// </summary>
        /// <returns>New index obtained</returns>
        protected override uint OnRent() => _index++;

        /// <summary>
        ///     Empty
        /// </summary>
        protected override void OnClear() => _index = 0U;
    }
}