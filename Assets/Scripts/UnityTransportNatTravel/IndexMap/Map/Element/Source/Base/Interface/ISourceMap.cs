//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     Mapping Interface
    /// </summary>
    public interface ISourceMap<out TKey, in TValue> where TKey : notnull where TValue : notnull
    {
        /// <summary>
        ///     Distribution
        /// </summary>
        TKey Allocate();

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="value">Value</param>
        TKey Add(TValue value);
    }
}