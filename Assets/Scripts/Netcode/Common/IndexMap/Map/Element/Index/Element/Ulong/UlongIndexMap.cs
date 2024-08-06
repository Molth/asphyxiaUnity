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
    public sealed class UlongIndexMap<TKey> : IndexMap<TKey, ulong> where TKey : notnull
    {
        /// <summary>
        ///     Index
        /// </summary>
        private readonly Map<TKey, ulong> _map;

        /// <summary>
        ///     Index Pool
        /// </summary>
        private readonly UlongIndexPool _indexPool;

        /// <summary>
        ///     Structure
        /// </summary>
        public UlongIndexMap()
        {
            _map = new Map<TKey, ulong>();
            _indexPool = new UlongIndexPool();
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public UlongIndexMap(int capacity)
        {
            _map = new Map<TKey, ulong>(capacity);
            _indexPool = new UlongIndexPool(capacity);
        }

        /// <summary>
        ///     Index mapping
        /// </summary>
        protected override IDuplicateMap<TKey, ulong> Map => _map;

        /// <summary>
        ///     Index Pool
        /// </summary>
        protected override IIndexPool<ulong> IndexPool => _indexPool;
    }
}