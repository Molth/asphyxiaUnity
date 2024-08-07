//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif
using System.Net;
using System.Runtime.InteropServices;

#pragma warning disable CS8632

// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace NanoSockets
{
    /// <summary>
    ///     IPAddress
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public unsafe struct NanoIPAddress
    {
        /// <summary>
        ///     High address
        /// </summary>
        [FieldOffset(0)] private ulong _high;

        /// <summary>
        ///     Low address
        /// </summary>
        [FieldOffset(8)] private ulong _low;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsSet => _high != 0UL || _low != 0UL;

        /// <summary>
        ///     High address
        /// </summary>
        public ulong High => _high;

        /// <summary>
        ///     Low address
        /// </summary>
        public ulong Low => _low;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="high">High</param>
        /// <param name="low">Low</param>
        public NanoIPAddress(ulong high, ulong low)
        {
            _high = high;
            _low = low;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="src">Buffer</param>
        public NanoIPAddress(Span<byte> src)
        {
            if (src.Length < 16)
                throw new ArgumentOutOfRangeException(src.Length.ToString());
            fixed (byte* ptr = &src[0])
            {
                _high = *(ulong*)ptr;
                _low = *(ulong*)(ptr + 8);
            }
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="src">Buffer</param>
        public NanoIPAddress(ReadOnlySpan<byte> src)
        {
            if (src.Length < 16)
                throw new ArgumentOutOfRangeException(src.Length.ToString());
            var span = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(src), src.Length);
            fixed (byte* ptr = &span[0])
            {
                _high = *(ulong*)ptr;
                _low = *(ulong*)(ptr + 8);
            }
        }

        /// <summary>
        ///     Returns the hash code for this instance
        /// </summary>
        /// <returns>A hash code for the current</returns>
        public override int GetHashCode() => ((16337 + (int)_high) ^ ((int)(_high >> 32) * 31 + (int)_low) ^ (int)(_low >> 32)) * 31;

        /// <summary>
        ///     Converts the value of this instance to its equivalent string representation
        /// </summary>
        /// <returns>Represents the boolean value as a string</returns>
        public override string ToString() => _high + ":" + _low;

        /// <summary>
        ///     Create IPAddress
        /// </summary>
        /// <returns>IPAddress</returns>
        public IPAddress? CreateIPAddress()
        {
            if (!IsSet)
                return null;
            var dst = stackalloc byte[16];
            *(ulong*)dst = _high;
            *(ulong*)(dst + 8) = _low;
            return new IPAddress(new Span<byte>(dst, 16));
        }

        /// <summary>
        ///     Copy to destination
        /// </summary>
        /// <param name="destination">Destination</param>
        /// <returns>Copied</returns>
        public void WriteBytes(Span<byte> destination)
        {
            fixed (byte* ptr = &destination[0])
            {
                *(ulong*)ptr = _high;
                *(ulong*)(ptr + 8) = _low;
            }
        }
    }
}