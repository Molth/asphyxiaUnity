//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     Mapping
    /// </summary>
    public sealed class IntSourceMap<TValue> : SourceMap<int, TValue> where TValue : notnull
    {
        /// <summary>
        ///     Index Pool
        /// </summary>
        private readonly IntIndexPool _indexPool;

        /// <summary>
        ///     Structure
        /// </summary>
        public IntSourceMap() => _indexPool = new IntIndexPool();

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public IntSourceMap(int capacity) : base(capacity) => _indexPool = new IntIndexPool(capacity);

        /// <summary>
        ///     Index Pool
        /// </summary>
        protected override IIndexPool<int> IndexPool => _indexPool;
    }
}