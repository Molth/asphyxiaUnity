//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     Index mapping
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    public sealed class UintIndexMap<TKey> : IndexMap<TKey, uint> where TKey : notnull
    {
        /// <summary>
        ///     Index
        /// </summary>
        private readonly Map<TKey, uint> _map;

        /// <summary>
        ///     Index Pool
        /// </summary>
        private readonly UintIndexPool _indexPool;

        /// <summary>
        ///     Structure
        /// </summary>
        public UintIndexMap()
        {
            _map = new Map<TKey, uint>();
            _indexPool = new UintIndexPool();
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public UintIndexMap(int capacity)
        {
            _map = new Map<TKey, uint>(capacity);
            _indexPool = new UintIndexPool(capacity);
        }

        /// <summary>
        ///     Index mapping
        /// </summary>
        protected override IDuplicateMap<TKey, uint> Map => _map;

        /// <summary>
        ///     Index Pool
        /// </summary>
        protected override IIndexPool<uint> IndexPool => _indexPool;
    }
}