﻿//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

namespace LiteNetLib
{
    /// <summary>
    ///     NetworkEvent
    /// </summary>
    public struct NetworkEvent
    {
        /// <summary>
        ///     EventType
        /// </summary>
        public NetworkEventType EventType;

        /// <summary>
        ///     Peer
        /// </summary>
        public NetPeer Peer;

        /// <summary>
        ///     Packet
        /// </summary>
        public DataPacket Packet;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsSet => Peer != null;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="eventType">EventType</param>
        /// <param name="peer">Peer</param>
        public NetworkEvent(NetworkEventType eventType, NetPeer peer) : this(eventType, peer, new DataPacket())
        {
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="eventType">EventType</param>
        /// <param name="peer">Peer</param>
        /// <param name="packet">Packet</param>
        public NetworkEvent(NetworkEventType eventType, NetPeer peer, DataPacket packet)
        {
            EventType = eventType;
            Peer = peer;
            Packet = packet;
        }
    }
}