//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif

namespace ENet
{
    /// <summary>
    ///     IPEndPoint extensions
    /// </summary>
    public static unsafe class IPEndPointExtensions
    {
        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="ipEndPoint">IPEndPoint</param>
        /// <param name="flag">Flag</param>
        /// <returns>DataPacket</returns>
        public static Packet CreateDataPacket(this ENetIPEndPoint ipEndPoint, PacketFlags flag)
        {
            var buffer = stackalloc byte[64];
            ipEndPoint.WriteBytes(new Span<byte>(buffer, 64), out var bytesWritten);
            var packet = new Packet();
            packet.Create((nint)buffer, bytesWritten, flag);
            return packet;
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="ipEndPoint">IPEndPoint</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="flag">Flag</param>
        /// <returns>DataPacket</returns>
        public static Packet CreateDataPacket(this ENetIPEndPoint ipEndPoint, byte* buffer, PacketFlags flag)
        {
            ipEndPoint.WriteBytes(new Span<byte>(buffer, 64), out var bytesWritten);
            var packet = new Packet();
            packet.Create((nint)buffer, bytesWritten, flag);
            return packet;
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="ipEndPoint">IPEndPoint</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="first">First</param>
        /// <param name="flag">Flag</param>
        /// <returns>DataPacket</returns>
        public static Packet CreateDataPacket(this ENetIPEndPoint ipEndPoint, byte* buffer, byte first, PacketFlags flag)
        {
            buffer[0] = first;
            buffer += 1;
            ipEndPoint.WriteBytes(new Span<byte>(buffer, 64), out var bytesWritten);
            var packet = new Packet();
            packet.Create((nint)(buffer - 1), 1 + bytesWritten, flag);
            return packet;
        }
    }
}