//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     IndexListInterface
    /// </summary>
    public interface IIndexPool<T> where T : unmanaged
    {
        /// <summary>
        ///     Distribution
        /// </summary>
        T Allocate();

        /// <summary>
        ///     Rent index
        /// </summary>
        /// <returns>New index obtained</returns>
        T Rent();

        /// <summary>
        ///     Return Index
        /// </summary>
        /// <param name="index">Index to be pushed</param>
        void Return(T index);

        /// <summary>
        ///     Clear index
        /// </summary>
        void Clear();
    }
}