//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     Mapping
    /// </summary>
    public sealed class ConcurrentUlongSourceMap<TValue> : ConcurrentSourceMap<ulong, TValue> where TValue : notnull
    {
        /// <summary>
        ///     Index Pool
        /// </summary>
        private readonly ConcurrentUlongIndexPool _indexPool;

        /// <summary>
        ///     Structure
        /// </summary>
        public ConcurrentUlongSourceMap() => _indexPool = new ConcurrentUlongIndexPool();

        /// <summary>
        ///     Index Pool
        /// </summary>
        protected override IIndexPool<ulong> IndexPool => _indexPool;
    }
}