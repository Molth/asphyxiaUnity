//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     Mapping Interface
    /// </summary>
    public interface IManualMap<in TKey, in TValue> where TKey : notnull where TValue : notnull
    {
        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        void Add(TKey key, TValue value);
    }
}