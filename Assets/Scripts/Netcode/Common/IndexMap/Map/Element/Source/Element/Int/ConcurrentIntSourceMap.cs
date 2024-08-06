//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     Mapping
    /// </summary>
    public sealed class ConcurrentIntSourceMap<TValue> : ConcurrentSourceMap<int, TValue> where TValue : notnull
    {
        /// <summary>
        ///     Index Pool
        /// </summary>
        private readonly ConcurrentIntIndexPool _indexPool;

        /// <summary>
        ///     Structure
        /// </summary>
        public ConcurrentIntSourceMap() => _indexPool = new ConcurrentIntIndexPool();

        /// <summary>
        ///     Index Pool
        /// </summary>
        protected override IIndexPool<int> IndexPool => _indexPool;
    }
}