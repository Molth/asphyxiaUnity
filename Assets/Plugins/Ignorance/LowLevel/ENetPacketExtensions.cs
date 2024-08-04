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
    ///     ENetPacket extensions
    /// </summary>
    public static unsafe class ENetPacketExtensions
    {
        /// <summary>
        ///     AsSpan
        /// </summary>
        /// <param name="packet">Packet</param>
        /// <returns>Span</returns>
        public static Span<byte> AsSpan(this Packet packet) => new((byte*)packet.Data, packet.Length);
    }
}