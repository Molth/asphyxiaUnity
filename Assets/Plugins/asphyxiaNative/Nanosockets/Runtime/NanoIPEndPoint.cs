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
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable InconsistentNaming

namespace NanoSockets
{
    /// <summary>
    ///     IPEndPoint
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public unsafe struct NanoIPEndPoint : IEquatable<NanoIPEndPoint>
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
        ///     Address
        /// </summary>
        public NanoIPAddress Address => new(_high, _low);

        /// <summary>
        ///     Port
        /// </summary>
        [FieldOffset(16)] private ushort _port;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsSet => _high != 0UL || _low != 0UL || _port != 0;

        /// <summary>
        ///     High address
        /// </summary>
        public ulong High => _high;

        /// <summary>
        ///     Low address
        /// </summary>
        public ulong Low => _low;

        /// <summary>
        ///     Port
        /// </summary>
        public ushort Port => _port;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="address">IPAddress</param>
        /// <param name="port">Port</param>
        public NanoIPEndPoint(NanoIPAddress address, ushort port)
        {
            _high = address.High;
            _low = address.Low;
            _port = port;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">IPEndPoint</param>
        /// <returns>summary</returns>
        public bool Equals(NanoIPEndPoint other) => _high == other._high && _low == other._low && _port == other._port;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NanoIPEndPoint other && Equals(other);

        /// <summary>
        ///     Returns the hash code for this instance
        /// </summary>
        /// <returns>A hash code for the current</returns>
        public override int GetHashCode() => ((16337 + (int)_high) ^ ((int)(_high >> 32) * 31 + (int)_low) ^ (int)(_low >> 32)) * 31 + _port;

        /// <summary>
        ///     Converts the value of this instance to its equivalent string representation
        /// </summary>
        /// <returns>Represents the boolean value as a string</returns>
        public override string ToString()
        {
            var stringBuffer = stackalloc byte[64];
            _ = NanoUdp.GetIP(ref this, stringBuffer, 64);
            var ipAddress = new string((sbyte*)stringBuffer);
            return ipAddress + ":" + _port;
        }

        /// <summary>
        ///     Create IPEndPoint
        /// </summary>
        /// <returns>IPEndPoint</returns>
        public IPEndPoint? CreateIPEndPoint()
        {
            if (!IsSet)
                return null;
            var dst = stackalloc byte[16];
            *(ulong*)dst = _high;
            *(ulong*)(dst + 8) = _low;
            return new IPEndPoint(new IPAddress(new Span<byte>(dst, 16)), _port);
        }

        /// <summary>
        ///     Create IPEndPoint
        /// </summary>
        /// <param name="port">Port</param>
        /// <returns>IPEndPoint</returns>
        public static NanoIPEndPoint Any(ushort port = 0) => Create("0.0.0.0", port);

        /// <summary>
        ///     Create IPEndPoint
        /// </summary>
        /// <param name="port">Port</param>
        /// <returns>IPEndPoint</returns>
        public static NanoIPEndPoint IPv6Any(ushort port = 0) => Create("::0", port);

        /// <summary>
        ///     Create IPEndPoint
        /// </summary>
        /// <param name="ipAddress">IPAddress</param>
        /// <param name="port">Port</param>
        /// <returns>IPEndPoint</returns>
        public static NanoIPEndPoint Create(string ipAddress, ushort port)
        {
            var ipEndPoint = new NanoIPEndPoint();
            if (NanoUdp.SetIP(ref ipEndPoint, ipAddress) != 0)
                throw new InvalidOperationException(nameof(ipAddress));
            ipEndPoint._port = port;
            return ipEndPoint;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NanoIPEndPoint left, NanoIPEndPoint right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NanoIPEndPoint left, NanoIPEndPoint right) => !left.Equals(right);
    }
}