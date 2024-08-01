//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     Mapping Interface
    /// </summary>
    public interface IMap<TKey, TValue> where TKey : notnull where TValue : notnull
    {
        /// <summary>
        ///     Current quantity
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     Get value
        /// </summary>
        TValue this[TKey tKey] { get; set; }

        /// <summary>
        ///     Get Key
        /// </summary>
        TKey this[TValue tValue] { get; set; }

        /// <summary>
        ///     Remove key
        /// </summary>
        /// <param name="key">Key</param>
        bool RemoveKey(TKey key);

        /// <summary>
        ///     Remove value
        /// </summary>
        /// <param name="value">Value</param>
        bool RemoveValue(TValue value);

        /// <summary>
        ///     Remove key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        bool TryRemoveKey(TKey key, out TValue value);

        /// <summary>
        ///     Remove value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="key">Key</param>
        bool TryRemoveValue(TValue value, out TKey key);

        /// <summary>
        ///     Get value
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        TValue GetValue(TKey key);

        /// <summary>
        ///     Get Key
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Key</returns>
        TKey GetKey(TValue value);

        /// <summary>
        ///     Get value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Successfully obtained</returns>
        bool TryGetValue(TKey key, out TValue value);

        /// <summary>
        ///     Get Key
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="key">Key</param>
        /// <returns>Successfully obtained</returns>
        bool TryGetKey(TValue value, out TKey key);

        /// <summary>
        ///     Containing keys
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Containing keys</returns>
        bool ContainsKey(TKey key);

        /// <summary>
        ///     Containing value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        bool ContainsValue(TValue value);

        /// <summary>
        ///     Contains specified key value pairs
        /// </summary>
        /// <param name="tKey">Key</param>
        /// <param name="tValue">Value</param>
        /// <returns>Does it contain specified key value pairs</returns>
        bool Contains(TKey tKey, TValue tValue);

        /// <summary>
        ///     Empty
        /// </summary>
        void Clear();
    }
}