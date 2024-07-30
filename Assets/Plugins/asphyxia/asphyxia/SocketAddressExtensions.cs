//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif

#pragma warning disable CS8632

namespace asphyxia
{
    /// <summary>
    ///     SocketAddress extensions
    /// </summary>
    public static class SocketAddressExtensions
    {
        /// <summary>
        ///     Serialize SocketAddress
        /// </summary>
        /// <param name="socketAddress">SocketAddress</param>
        /// <param name="source">Source</param>
        /// <returns>Serialized</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Serialize(this SocketAddress socketAddress, Span<byte> source)
        {
            var size = socketAddress.Size;
            if (source.Length != size)
                return false;
#if NET8_0_OR_GREATER
            Unsafe.CopyBlock(ref socketAddress.Buffer.Span[0], ref source[0], (uint)size);
#else
            for (var i = 0; i < size; ++i)
                socketAddress[i] = source[i];
#endif
            return true;
        }

        /// <summary>
        ///     Copy SocketAddress's buffer
        /// </summary>
        /// <param name="socketAddress">SocketAddress</param>
        /// <param name="destination">Destination</param>
        /// <returns>Count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CopyTo(this SocketAddress socketAddress, Span<byte> destination)
        {
            var size = socketAddress.Size;
            if (destination.Length < size)
                return -1;
#if NET8_0_OR_GREATER
            Unsafe.CopyBlock(ref destination[0], ref socketAddress.Buffer.Span[0], (uint)size);
#else
            for (var i = 0; i < size; ++i)
                destination[i] = socketAddress[i];
#endif
            return size;
        }

        /// <summary>
        ///     Copy SocketAddress's buffer
        /// </summary>
        /// <param name="socketAddress">SocketAddress</param>
        /// <param name="destination">Destination</param>
        /// <returns>Count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CopyTo(this SocketAddress socketAddress, SocketAddress destination)
        {
            var size = socketAddress.Size;
            if (destination.Size != size)
                return -1;
#if NET8_0_OR_GREATER
            var span = destination.Buffer.Span;
            ref var dst = ref span[0];
            span = socketAddress.Buffer.Span;
            ref var src = ref span[0];
            Unsafe.CopyBlock(ref dst, ref src, (uint)size);
#else
            for (var i = 0; i < size; ++i)
                destination[i] = socketAddress[i];
#endif
            return size;
        }

        /// <summary>
        ///     Create SocketAddress
        /// </summary>
        /// <param name="socketAddress">SocketAddress</param>
        /// <returns>SocketAddress</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketAddress Create(this SocketAddress socketAddress)
        {
            var size = socketAddress.Size;
            var newSocketAddress = new SocketAddress(socketAddress.CreateAddressFamily(), size);
#if NET8_0_OR_GREATER
            var span = newSocketAddress.Buffer.Span;
            ref var dst = ref span[0];
            span = socketAddress.Buffer.Span;
            ref var src = ref span[0];
            Unsafe.CopyBlock(ref dst, ref src, (uint)size);
#else
            for (var i = 0; i < size; ++i)
                newSocketAddress[i] = socketAddress[i];
#endif
            return newSocketAddress;
        }

        /// <summary>
        ///     Create AddressFamily
        /// </summary>
        /// <param name="socketAddress">SocketAddress</param>
        /// <returns>AddressFamily</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static
#if NET8_0_OR_GREATER
            unsafe
#endif
            AddressFamily CreateAddressFamily(this SocketAddress socketAddress)
        {
#if NET8_0_OR_GREATER
            var socketAddressBuffer = socketAddress.Buffer.Span;
            var result = Unsafe.Read<ushort>(Unsafe.AsPointer(ref socketAddressBuffer[0]));
            return (AddressFamily)result;
#else
            return socketAddress.Family;
#endif
        }

        /// <summary>
        ///     Create IPAddress
        /// </summary>
        /// <param name="socketAddress">SocketAddress</param>
        /// <returns>IPAddress</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IPAddress? CreateIPAddress(this SocketAddress socketAddress)
        {
#if NET8_0_OR_GREATER
            var socketAddressBuffer = socketAddress.Buffer.Span;
#else
            var size = socketAddress.Size;
            Span<byte> socketAddressBuffer = stackalloc byte[size];
            for (var i = 0; i < size; ++i)
                socketAddressBuffer[i] = socketAddress[i];
#endif
            switch (socketAddress.CreateAddressFamily())
            {
                case AddressFamily.InterNetwork:
                    return new IPAddress(BinaryPrimitives.ReadUInt32LittleEndian(socketAddressBuffer.Slice(4, 12)) & 4294967295L);
                case AddressFamily.InterNetworkV6:
                    var address = socketAddressBuffer.Slice(8, 16);
                    var scope = BinaryPrimitives.ReadUInt32LittleEndian(socketAddressBuffer.Slice(24, 4));
                    return new IPAddress(address, address[0] != 254 || (address[1] & 192) != 128 ? 0L : Unsafe.As<uint, long>(ref scope));
                default:
                    return null;
            }
        }

        /// <summary>
        ///     Create Port
        /// </summary>
        /// <param name="socketAddress">SocketAddress</param>
        /// <returns>Port</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort CreatePort(this SocketAddress socketAddress)
        {
#if NET8_0_OR_GREATER
            var socketAddressBuffer = socketAddress.Buffer.Span;
#else
            var size = socketAddress.Size;
            Span<byte> socketAddressBuffer = stackalloc byte[size];
            for (var i = 0; i < size; ++i)
                socketAddressBuffer[i] = socketAddress[i];
#endif
            return BinaryPrimitives.ReadUInt16BigEndian(socketAddressBuffer.Slice(2, 4));
        }

        /// <summary>
        ///     Create IPEndPoint
        /// </summary>
        /// <param name="socketAddress">SocketAddress</param>
        /// <returns>IPEndPoint</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IPEndPoint? CreateIPEndPoint(this SocketAddress socketAddress)
        {
#if NET8_0_OR_GREATER
            var socketAddressBuffer = socketAddress.Buffer.Span;
#else
            var size = socketAddress.Size;
            Span<byte> socketAddressBuffer = stackalloc byte[size];
            for (var i = 0; i < size; ++i)
                socketAddressBuffer[i] = socketAddress[i];
#endif
            switch (socketAddress.CreateAddressFamily())
            {
                case AddressFamily.InterNetwork:
                    return new IPEndPoint(new IPAddress(BinaryPrimitives.ReadUInt32LittleEndian(socketAddressBuffer.Slice(4, 12)) & 4294967295L), BinaryPrimitives.ReadUInt16BigEndian(socketAddressBuffer.Slice(2, 4)));
                case AddressFamily.InterNetworkV6:
                    var address = socketAddressBuffer.Slice(8, 16);
                    var scope = BinaryPrimitives.ReadUInt32LittleEndian(socketAddressBuffer.Slice(24, 4));
                    return new IPEndPoint(new IPAddress(address, address[0] != 254 || (address[1] & 192) != 128 ? 0L : Unsafe.As<uint, long>(ref scope)), BinaryPrimitives.ReadUInt16BigEndian(socketAddressBuffer.Slice(2, 4)));
                default:
                    return null;
            }
        }
    }
}