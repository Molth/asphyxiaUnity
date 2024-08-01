//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     Mapping Interface
    /// </summary>
    public interface IIndexMap<TKey, TValue> where TKey : notnull where TValue : notnull
    {
        /// <summary>
        ///     Current quantity
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     Distribution
        /// </summary>
        TValue Allocate();

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="key">Key</param>
        TValue Add(TKey key);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        bool RemoveKey(TKey key);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="id">Index</param>
        bool RemoveValue(TValue id);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="id">Index</param>
        bool TryRemoveKey(TKey key, out TValue id);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="id">Index</param>
        /// <param name="key">Key</param>
        bool TryRemoveValue(TValue id, out TKey key);

        /// <summary>
        ///     Attempt to obtain value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="id">Index</param>
        /// <returns>Successfully obtained</returns>
        bool TryGetValue(TKey key, out TValue id);

        /// <summary>
        ///     Attempt to obtain value
        /// </summary>
        /// <param name="key">Add</param>
        /// <param name="id">Index</param>
        /// <returns>Successfully obtained</returns>
        bool TryGetKey(TValue id, out TKey key);

        /// <summary>
        ///     Get or addId
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Index</returns>
        TValue GetOrAddValue(TKey key);

        /// <summary>
        ///     Empty
        /// </summary>
        void Clear();
    }
}