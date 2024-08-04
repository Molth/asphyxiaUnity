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
    ///     Network outgoing
    /// </summary>
    public unsafe struct NetworkOutgoing : IDisposable
    {
        /// <summary>
        ///     Peer
        /// </summary>
        public Peer Peer;

        /// <summary>
        ///     DataPacket
        /// </summary>
        public Packet Packet;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="peer">Peer</param>
        /// <param name="data">DataPacket</param>
        public NetworkOutgoing(Peer peer, Packet data)
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
        public static NetworkOutgoing Create(Peer peer, Span<byte> data, PacketFlags flag)
        {
            var packet = new Packet();
            fixed (byte* ptr = &data[0])
            {
                packet.Create((nint)ptr, data.Length, flag);
            }

            return new NetworkOutgoing(peer, packet);
        }

        /// <summary>
        ///     Send
        /// </summary>
        public void Send()
        {
            try
            {
                Peer.Send(0, ref Packet);
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