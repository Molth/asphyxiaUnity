//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

namespace Erinn
{
    /// <summary>
    ///     Index mapping
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    public abstract class IndexMap<TKey, TValue> : IIndexMap<TKey, TValue>, IDuplicateIndexMap<TKey, TValue> where TKey : notnull where TValue : unmanaged
    {
        /// <summary>
        ///     Index mapping
        /// </summary>
        protected abstract IDuplicateMap<TKey, TValue> Map { get; }

        /// <summary>
        ///     Index Pool
        /// </summary>
        protected abstract IIndexPool<TValue> IndexPool { get; }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="id">Index</param>
        public void TryRemove(TKey key, out TValue id) => TryRemoveKey(key, out id);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="id">Index</param>
        /// <param name="key">Key</param>
        public void TryRemove(TValue id, out TKey key) => TryRemoveValue(id, out key);

        /// <summary>
        ///     Attempt to obtain value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="id">Index</param>
        /// <returns>Successfully obtained</returns>
        public bool TryGet(TKey key, out TValue id) => TryGetValue(key, out id);

        /// <summary>
        ///     Attempt to obtain value
        /// </summary>
        /// <param name="key">Add</param>
        /// <param name="id">Index</param>
        /// <returns>Successfully obtained</returns>
        public bool TryGet(TValue id, out TKey key) => TryGetKey(id, out key);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        public bool Remove(TKey key) => RemoveKey(key);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="id">Index</param>
        public bool Remove(TValue id) => RemoveValue(id);

        /// <summary>
        ///     Current quantity
        /// </summary>
        public int Count => Map.Count;

        /// <summary>
        ///     Distribution
        /// </summary>
        public TValue Allocate() => IndexPool.Allocate();

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="key">Key</param>
        public TValue Add(TKey key)
        {
            var id = IndexPool.Rent();
            Map[key] = id;
            return id;
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        public bool RemoveKey(TKey key)
        {
            if (!Map.TryRemove(key, out var id))
                return false;
            IndexPool.Return(id);
            return true;
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="id">Index</param>
        public bool RemoveValue(TValue id)
        {
            if (!Map.Remove(id))
                return false;
            IndexPool.Return(id);
            return true;
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="id">Index</param>
        public bool TryRemoveKey(TKey key, out TValue id)
        {
            if (!Map.TryRemove(key, out id))
                return false;
            IndexPool.Return(id);
            return true;
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="id">Index</param>
        /// <param name="key">Key</param>
        public bool TryRemoveValue(TValue id, out TKey key)
        {
            if (!Map.TryRemove(id, out key))
                return false;
            IndexPool.Return(id);
            return true;
        }

        /// <summary>
        ///     Attempt to obtain value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="id">Index</param>
        /// <returns>Successfully obtained</returns>
        public bool TryGetValue(TKey key, out TValue id) => Map.TryGetValue(key, out id);

        /// <summary>
        ///     Attempt to obtain value
        /// </summary>
        /// <param name="key">Add</param>
        /// <param name="id">Index</param>
        /// <returns>Successfully obtained</returns>
        public bool TryGetKey(TValue id, out TKey key) => Map.TryGetKey(id, out key);

        /// <summary>
        ///     Get or addId
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Index</returns>
        public TValue GetOrAddValue(TKey key)
        {
            if (Map.TryGetValue(key, out var id))
                return id;
            id = IndexPool.Rent();
            Map[key] = id;
            return id;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public void Clear()
        {
            Map.Clear();
            IndexPool.Clear();
        }
    }
}