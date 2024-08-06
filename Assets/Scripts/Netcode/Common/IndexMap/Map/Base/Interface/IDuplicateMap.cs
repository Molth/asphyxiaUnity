//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     Mapping Interface
    /// </summary>
    public interface IDuplicateMap<TKey, TValue> : IMap<TKey, TValue> where TKey : notnull where TValue : notnull
    {
        /// <summary>
        ///     Remove key
        /// </summary>
        /// <param name="key">Key</param>
        bool Remove(TKey key);

        /// <summary>
        ///     Remove value
        /// </summary>
        /// <param name="value">Value</param>
        bool Remove(TValue value);

        /// <summary>
        ///     Remove key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        bool TryRemove(TKey key, out TValue value);

        /// <summary>
        ///     Remove value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="key">Key</param>
        bool TryRemove(TValue value, out TKey key);

        /// <summary>
        ///     Get value
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        TValue Get(TKey key);

        /// <summary>
        ///     Get Key
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Key</returns>
        TKey Get(TValue value);

        /// <summary>
        ///     Get value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Successfully obtained</returns>
        bool TryGet(TKey key, out TValue value);

        /// <summary>
        ///     Get Key
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="key">Key</param>
        /// <returns>Successfully obtained</returns>
        bool TryGet(TValue value, out TKey key);
    }
}