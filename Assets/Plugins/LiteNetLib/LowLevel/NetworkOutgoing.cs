//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif

namespace LiteNetLib
{
    /// <summary>
    ///     Network outgoing
    /// </summary>
    public struct NetworkOutgoing : IDisposable
    {
        /// <summary>
        ///     Peer
        /// </summary>
        public NetPeer Peer;

        /// <summary>
        ///     DataPacket
        /// </summary>
        public DataPacket Packet;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="peer">Peer</param>
        /// <param name="data">DataPacket</param>
        public NetworkOutgoing(NetPeer peer, DataPacket data)
        {
            Peer = peer;
            Packet = data;
        }

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="peer">Peer</param>
        /// <param name="data">DataPacket</param>
        /// <param name="flag">Flag</param>
        /// <returns>NetworkOutgoing</returns>
        public static NetworkOutgoing Create(NetPeer peer, Span<byte> data, DeliveryMethod flag) => new(peer, DataPacket.Create(data, flag));

        /// <summary>
        ///     Send
        /// </summary>
        public void Send()
        {
            try
            {
                Peer.Send(Packet.AsSpan(), Packet.Flags);
            }
            finally
            {
                Packet.Dispose();
            }
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose() => Packet.Dispose();
    }
}