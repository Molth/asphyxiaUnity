//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

using System.Collections.Concurrent;
#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
#endif

#pragma warning disable CS8601

namespace Erinn
{
    /// <summary>
    ///     Mapping
    /// </summary>
    public sealed class ConcurrentMap<TKey, TValue> : IDuplicateMap<TKey, TValue>, IManualMap<TKey, TValue> where TKey : notnull where TValue : notnull
    {
        /// <summary>
        ///     Key
        /// </summary>
        private readonly ConcurrentDictionary<TKey, TValue> _keys = new();

        /// <summary>
        ///     Value
        /// </summary>
        private readonly ConcurrentDictionary<TValue, TKey> _values = new();

        /// <summary>
        ///     Key
        /// </summary>
        public ICollection<TKey> Keys => _keys.Keys;

        /// <summary>
        ///     Value
        /// </summary>
        public ICollection<TValue> Values => _values.Keys;

        /// <summary>
        ///     Empty
        /// </summary>
        public bool IsEmpty => _keys.IsEmpty;

        /// <summary>
        ///     Current quantity
        /// </summary>
        public int Count => _keys.Count;

        /// <summary>
        ///     Get value
        /// </summary>
        public TValue this[TKey tKey]
        {
            get => _keys[tKey];
            set
            {
                _keys[tKey] = value;
                _values[value] = tKey;
            }
        }

        /// <summary>
        ///     Get Key
        /// </summary>
        public TKey this[TValue tValue]
        {
            get => _values[tValue];
            set
            {
                _values[tValue] = value;
                _keys[value] = tValue;
            }
        }

        /// <summary>
        ///     Remove key
        /// </summary>
        /// <param name="key">Key</param>
        public bool Remove(TKey key)
        {
            if (!_keys.TryRemove(key, out var value))
                return false;
            _values.TryRemove(value, out _);
            return true;
        }

        /// <summary>
        ///     Remove value
        /// </summary>
        /// <param name="value">Value</param>
        public bool Remove(TValue value)
        {
            if (!_values.TryRemove(value, out var key))
                return false;
            _keys.TryRemove(key, out _);
            return true;
        }

        /// <summary>
        ///     Remove key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public bool TryRemove(TKey key, out TValue value)
        {
            if (!_keys.TryRemove(key, out value))
                return false;
            _values.TryRemove(value, out _);
            return true;
        }

        /// <summary>
        ///     Remove value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="key">Key</param>
        public bool TryRemove(TValue value, out TKey key)
        {
            if (!_values.TryRemove(value, out key))
                return false;
            _keys.TryRemove(key, out _);
            return true;
        }

        /// <summary>
        ///     Remove key
        /// </summary>
        /// <param name="key">Key</param>
        public bool RemoveKey(TKey key)
        {
            if (!_keys.TryRemove(key, out var value))
                return false;
            _values.TryRemove(value, out _);
            return true;
        }

        /// <summary>
        ///     Remove value
        /// </summary>
        /// <param name="value">Value</param>
        public bool RemoveValue(TValue value)
        {
            if (!_values.TryRemove(value, out var key))
                return false;
            _keys.TryRemove(key, out _);
            return true;
        }

        /// <summary>
        ///     Remove key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public bool TryRemoveKey(TKey key, out TValue value)
        {
            if (!_keys.TryRemove(key, out value))
                return false;
            _values.TryRemove(value, out _);
            return true;
        }

        /// <summary>
        ///     Remove value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="key">Key</param>
        public bool TryRemoveValue(TValue value, out TKey key)
        {
            if (!_values.TryRemove(value, out key))
                return false;
            _keys.TryRemove(key, out _);
            return true;
        }

        /// <summary>
        ///     Get value
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public TValue Get(TKey key) => _keys[key];

        /// <summary>
        ///     Get Key
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Key</returns>
        public TKey Get(TValue value) => _values[value];

        /// <summary>
        ///     Get value
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public TValue GetValue(TKey key) => _keys[key];

        /// <summary>
        ///     Get Key
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Key</returns>
        public TKey GetKey(TValue value) => _values[value];

        /// <summary>
        ///     Get value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Successfully obtained</returns>
        public bool TryGet(TKey key, out TValue value) => _keys.TryGetValue(key, out value);

        /// <summary>
        ///     Get Key
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="key">Key</param>
        /// <returns>Successfully obtained</returns>
        public bool TryGet(TValue value, out TKey key) => _values.TryGetValue(value, out key);

        /// <summary>
        ///     Get value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Successfully obtained</returns>
        public bool TryGetValue(TKey key, out TValue value) => _keys.TryGetValue(key, out value);

        /// <summary>
        ///     Get Key
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="key">Key</param>
        /// <returns>Successfully obtained</returns>
        public bool TryGetKey(TValue value, out TKey key) => _values.TryGetValue(value, out key);

        /// <summary>
        ///     Containing keys
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Containing keys</returns>
        public bool ContainsKey(TKey key) => _keys.ContainsKey(key);

        /// <summary>
        ///     Containing value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public bool ContainsValue(TValue value) => _values.ContainsKey(value);

        /// <summary>
        ///     Contains specified key value pairs
        /// </summary>
        /// <param name="tKey">Key</param>
        /// <param name="tValue">Value</param>
        /// <returns>Does it contain specified key value pairs</returns>
        public bool Contains(TKey tKey, TValue tValue) => _keys.TryGetValue(tKey, out var value) && _values.TryGetValue(tValue, out var key) && value.Equals(tValue) && key.Equals(tKey);

        /// <summary>
        ///     Empty
        /// </summary>
        public void Clear()
        {
            _keys.Clear();
            _values.Clear();
        }

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void Add(TKey key, TValue value)
        {
            _keys[key] = value;
            _values[value] = key;
        }
    }
}