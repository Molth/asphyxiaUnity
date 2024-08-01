//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable CS8601

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable UseCollectionExpression

namespace asphyxia
{
    /// <summary>
    ///     Queue
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public sealed class Queue<T>
    {
        /// <summary>
        ///     Array
        /// </summary>
        private T[] _array;

        /// <summary>
        ///     Head
        /// </summary>
        private int _head;

        /// <summary>
        ///     Tail
        /// </summary>
        private int _tail;

        /// <summary>
        ///     Size
        /// </summary>
        private int _size;

        /// <summary>
        ///     Structure
        /// </summary>
        public Queue() => _array = Array.Empty<T>();

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        public Queue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException();
            _array = new T[capacity];
        }

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _size;

        /// <summary>
        ///     Clear
        /// </summary>
        public void Clear()
        {
            if (_size != 0)
            {
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
                    if (_head < _tail)
                    {
                        Array.Clear(_array, _head, _size);
                    }
                    else
                    {
                        Array.Clear(_array, _head, _array.Length - _head);
                        Array.Clear(_array, 0, _tail);
                    }
                }

                _size = 0;
            }

            _head = 0;
            _tail = 0;
        }

        /// <summary>
        ///     Enqueue
        /// </summary>
        /// <param name="item">Item</param>
        public void Enqueue(T item)
        {
            if (_size == _array.Length)
                Grow(_size + 1);
            _array[_tail] = item;
            MoveNext(ref _tail);
            _size++;
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Result</param>
        /// <returns>Dequeued</returns>
        public bool TryDequeue([MaybeNullWhen(false)] out T result)
        {
            var head = _head;
            var array = _array;
            if (_size == 0)
            {
                result = default;
                return false;
            }

            result = array[head];
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                array[head] = default;
            MoveNext(ref _head);
            _size--;
            return true;
        }

        /// <summary>
        ///     Set capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        private void SetCapacity(int capacity)
        {
            var newArray = new T[capacity];
            if (_size > 0)
            {
                if (_head < _tail)
                {
                    Array.Copy(_array, _head, newArray, 0, _size);
                }
                else
                {
                    Array.Copy(_array, _head, newArray, 0, _array.Length - _head);
                    Array.Copy(_array, 0, newArray, _array.Length - _head, _tail);
                }
            }

            _array = newArray;
            _head = 0;
            _tail = _size == capacity ? 0 : _size;
        }

        /// <summary>
        ///     Move next
        /// </summary>
        /// <param name="index">Index</param>
        private void MoveNext(ref int index)
        {
            var tmp = index + 1;
            if (tmp == _array.Length)
                tmp = 0;
            index = tmp;
        }

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Capacity</returns>
        public int EnsureCapacity(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException();
            if (_array.Length < capacity)
                Grow(capacity);
            return _array.Length;
        }

        /// <summary>
        ///     Grow
        /// </summary>
        /// <param name="capacity">Capacity</param>
        private void Grow(int capacity)
        {
            var newCapacity = 2 * _array.Length;
            if ((uint)newCapacity > 2147483591)
                newCapacity = 2147483591;
            newCapacity = Math.Max(newCapacity, _array.Length + 4);
            if (newCapacity < capacity)
                newCapacity = capacity;
            SetCapacity(newCapacity);
        }
    }
}