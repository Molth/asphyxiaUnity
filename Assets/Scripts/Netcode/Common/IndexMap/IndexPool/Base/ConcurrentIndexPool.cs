//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

using System.Collections.Concurrent;

namespace Erinn
{
    /// <summary>
    ///     Index Pool
    /// </summary>
    public abstract class ConcurrentIndexPool<T> : IIndexPool<T> where T : unmanaged
    {
        /// <summary>
        ///     Idle index
        /// </summary>
        private readonly ConcurrentQueue<T> _idlePool;

        /// <summary>
        ///     Structure
        /// </summary>
        protected ConcurrentIndexPool() => _idlePool = new ConcurrentQueue<T>();

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