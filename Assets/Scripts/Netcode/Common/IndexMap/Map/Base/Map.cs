//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
#endif

#pragma warning disable CS8601

namespace Erinn
{
    /// <summary>
    ///     Mapping
    /// </summary>
    public sealed class Map<TKey, TValue> : IDuplicateMap<TKey, TValue>, IManualMap<TKey, TValue> where TKey : notnull where TValue : notnull
    {
        /// <summary>
        ///     Key
        /// </summary>
        private readonly Dictionary<TKey, TValue> _keys;

        /// <summary>
        ///     Value
        /// </summary>
        private readonly Dictionary<TValue, TKey> _values;

        /// <summary>
        ///     Structure
        /// </summary>
        public Map()
        {
            _keys = new Dictionary<TKey, TValue>();
            _values = new Dictionary<TValue, TKey>();
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public Map(int capacity)
        {
            _keys = new Dictionary<TKey, TValue>(capacity);
            _values = new Dictionary<TValue, TKey>(capacity);
        }

        /// <summary>
        ///     Key
        /// </summary>
        public Dictionary<TKey, TValue>.KeyCollection Keys => _keys.Keys;

        /// <summary>
        ///     Value
        /// </summary>
        public Dictionary<TValue, TKey>.KeyCollection Values => _values.Keys;

        /// <summary>
        ///     Empty
        /// </summary>
        public bool IsEmpty => _keys.Count == 0;

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
            if (!_keys.TryGetValue(key, out var value))
                return false;
            _values.Remove(value);
            _keys.Remove(key);
            return true;
        }

        /// <summary>
        ///     Remove value
        /// </summary>
        /// <param name="value">Value</param>
        public bool Remove(TValue value)
        {
            if (!_values.TryGetValue(value, out var key))
                return false;
            _keys.Remove(key);
            _values.Remove(value);
            return true;
        }

        /// <summary>
        ///     Remove key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public bool TryRemove(TKey key, out TValue value)
        {
            if (!_keys.TryGetValue(key, out value))
                return false;
            _values.Remove(value);
            _keys.Remove(key);
            return true;
        }

        /// <summary>
        ///     Remove value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="key">Key</param>
        public bool TryRemove(TValue value, out TKey key)
        {
            if (!_values.TryGetValue(value, out key))
                return false;
            _keys.Remove(key);
            _values.Remove(value);
            return true;
        }

        /// <summary>
        ///     Remove key
        /// </summary>
        /// <param name="key">Key</param>
        public bool RemoveKey(TKey key)
        {
            if (!_keys.TryGetValue(key, out var value))
                return false;
            _values.Remove(value);
            _keys.Remove(key);
            return true;
        }

        /// <summary>
        ///     Remove value
        /// </summary>
        /// <param name="value">Value</param>
        public bool RemoveValue(TValue value)
        {
            if (!_values.TryGetValue(value, out var key))
                return false;
            _keys.Remove(key);
            _values.Remove(value);
            return true;
        }

        /// <summary>
        ///     Remove key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public bool TryRemoveKey(TKey key, out TValue value)
        {
            if (!_keys.TryGetValue(key, out value))
                return false;
            _values.Remove(value);
            _keys.Remove(key);
            return true;
        }

        /// <summary>
        ///     Remove value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="key">Key</param>
        public bool TryRemoveValue(TValue value, out TKey key)
        {
            if (!_values.TryGetValue(value, out key))
                return false;
            _keys.Remove(key);
            _values.Remove(value);
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

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public void EnsureCapacity(int capacity)
        {
            _keys.EnsureCapacity(capacity);
            _values.EnsureCapacity(capacity);
        }

        /// <summary>
        ///     Reduce capacity
        /// </summary>
        public void TrimExcess()
        {
            _keys.TrimExcess();
            _values.TrimExcess();
        }

        /// <summary>
        ///     Reduce capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public void TrimExcess(int capacity)
        {
            _keys.TrimExcess(capacity);
            _values.TrimExcess(capacity);
        }
    }
}