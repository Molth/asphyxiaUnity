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
    public sealed class ConcurrentIntIndexMap<TKey> : IndexMap<TKey, int> where TKey : notnull
    {
        /// <summary>
        ///     Index
        /// </summary>
        private readonly ConcurrentMap<TKey, int> _map = new();

        /// <summary>
        ///     Index Pool
        /// </summary>
        private readonly ConcurrentIntIndexPool _indexPool = new();

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