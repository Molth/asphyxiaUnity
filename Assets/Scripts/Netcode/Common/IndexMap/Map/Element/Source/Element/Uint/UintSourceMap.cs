//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     Mapping
    /// </summary>
    public sealed class UintSourceMap<TValue> : SourceMap<uint, TValue> where TValue : notnull
    {
        /// <summary>
        ///     Index Pool
        /// </summary>
        private readonly UintIndexPool _indexPool;

        /// <summary>
        ///     Structure
        /// </summary>
        public UintSourceMap() => _indexPool = new UintIndexPool();

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public UintSourceMap(int capacity) : base(capacity) => _indexPool = new UintIndexPool(capacity);

        /// <summary>
        ///     Index Pool
        /// </summary>
        protected override IIndexPool<uint> IndexPool => _indexPool;
    }
}