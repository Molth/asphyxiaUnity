//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
#endif

namespace Erinn
{
    /// <summary>
    ///     Index Pool
    /// </summary>
    public abstract class IndexPool<T> : IIndexPool<T> where T : unmanaged
    {
        /// <summary>
        ///     Idle index
        /// </summary>
        private readonly Queue<T> _idlePool;

        /// <summary>
        ///     Structure
        /// </summary>
        protected IndexPool() => _idlePool = new Queue<T>();

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        protected IndexPool(int capacity) => _idlePool = new Queue<T>(capacity);

        /// <summary>
        ///     Distribution
        /// </summary>
        public T Allocate() => OnRent();

        /// <summary>
        ///     Rent index
        /// </summary>
        /// <returns>New index obtained</returns>
        public T Rent() => _idlePool.TryDequeue(out var index) ? index : OnRent();

        /// <summary>
        ///     Return Index
        /// </summary>
        /// <param name="index">Index to be pushed</param>
        public void Return(T index) => _idlePool.Enqueue(index);

        /// <summary>
        ///     Clear index
        /// </summary>
        public void Clear()
        {
            _idlePool.Clear();
            OnClear();
        }

        /// <summary>
        ///     Rent index
        /// </summary>
        /// <returns>New index obtained</returns>
        protected abstract T OnRent();

        /// <summary>
        ///     Clear index
        /// </summary>
        protected abstract void OnClear();
    }
}