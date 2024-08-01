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
    public sealed class ConcurrentUintIndexMap<TKey> : IndexMap<TKey, uint> where TKey : notnull
    {
        /// <summary>
        ///     Index
        /// </summary>
        private readonly ConcurrentMap<TKey, uint> _map = new();

        /// <summary>
        ///     Index Pool
        /// </summary>
        private readonly ConcurrentUintIndexPool _indexPool = new();

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