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
    public sealed class ConcurrentUlongIndexMap<TKey> : IndexMap<TKey, ulong> where TKey : notnull
    {
        /// <summary>
        ///     Index
        /// </summary>
        private readonly ConcurrentMap<TKey, ulong> _map = new();

        /// <summary>
        ///     Index Pool
        /// </summary>
        private readonly ConcurrentUlongIndexPool _indexPool = new();

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