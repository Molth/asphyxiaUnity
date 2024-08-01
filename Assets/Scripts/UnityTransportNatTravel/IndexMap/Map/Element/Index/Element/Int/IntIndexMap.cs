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
    public sealed class IntIndexMap<TKey> : IndexMap<TKey, int> where TKey : notnull
    {
        /// <summary>
        ///     Index
        /// </summary>
        private readonly Map<TKey, int> _map;

        /// <summary>
        ///     Index Pool
        /// </summary>
        private readonly IntIndexPool _indexPool;

        /// <summary>
        ///     Structure
        /// </summary>
        public IntIndexMap()
        {
            _map = new Map<TKey, int>();
            _indexPool = new IntIndexPool();
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public IntIndexMap(int capacity)
        {
            _map = new Map<TKey, int>(capacity);
            _indexPool = new IntIndexPool(capacity);
        }

        /// <summary>
        ///     Index mapping
        /// </summary>
        protected override IDuplicateMap<TKey, int> Map => _map;

        /// <summary>
        ///     Index Pool
        /// </summary>
        protected override IIndexPool<int> IndexPool => _indexPool;
    }
}