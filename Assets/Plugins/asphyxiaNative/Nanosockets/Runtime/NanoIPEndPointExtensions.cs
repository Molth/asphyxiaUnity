//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif
using NanoSockets;

namespace asphyxia
{
    /// <summary>
    ///     IPEndPoint extensions
    /// </summary>
    public static unsafe class NanoIPEndPointExtensions
    {
        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="ipEndPoint">IPEndPoint</param>
        /// <param name="flag">Flag</param>
        /// <returns>DataPacket</returns>
        public static DataPacket CreateDataPacket(this NanoIPEndPoint ipEndPoint, PacketFlag flag = PacketFlag.Reliable)
        {
            var buffer = stackalloc byte[18];
            ipEndPoint.WriteBytes(new Span<byte>(buffer, 18));
            return DataPacket.Create(buffer, 18, flag);
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="ipEndPoint">IPEndPoint</param>
        /// <param name="space">Space</param>
        /// <param name="flag">Flag</param>
        /// <returns>DataPacket</returns>
        public static DataPacket CreateDataPacket(this NanoIPEndPoint ipEndPoint, int space, PacketFlag flag = PacketFlag.Reliable)
        {
            var buffer = stackalloc byte[space + 18];
            buffer += space;
            ipEndPoint.WriteBytes(new Span<byte>(buffer, 18));
            return DataPacket.Create(buffer - space, space + 18, flag);
        }
    }
}