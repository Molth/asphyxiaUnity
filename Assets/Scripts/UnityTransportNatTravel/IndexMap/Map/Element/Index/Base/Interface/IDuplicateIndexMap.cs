//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     Mapping Interface
    /// </summary>
    public interface IDuplicateIndexMap<TKey, TValue> where TKey : notnull where TValue : notnull
    {
        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="id">Index</param>
        void TryRemove(TKey key, out TValue id);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="id">Index</param>
        /// <param name="key">Key</param>
        void TryRemove(TValue id, out TKey key);

        /// <summary>
        ///     Attempt to obtain value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="id">Index</param>
        /// <returns>Successfully obtained</returns>
        bool TryGet(TKey key, out TValue id);

        /// <summary>
        ///     Attempt to obtain value
        /// </summary>
        /// <param name="key">Add</param>
        /// <param name="id">Index</param>
        /// <returns>Successfully obtained</returns>
        bool TryGet(TValue id, out TKey key);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        bool Remove(TKey key);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="id">Index</param>
        bool Remove(TValue id);
    }
}