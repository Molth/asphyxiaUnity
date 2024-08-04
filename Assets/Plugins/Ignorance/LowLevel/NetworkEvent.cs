//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

namespace ENet
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
        public Peer Peer;

        /// <summary>
        ///     Packet
        /// </summary>
        public Packet Packet;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsSet => Peer.IsSet;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="eventType">EventType</param>
        /// <param name="peer">Peer</param>
        public NetworkEvent(NetworkEventType eventType, Peer peer) : this(eventType, peer, new Packet())
        {
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="eventType">EventType</param>
        /// <param name="peer">Peer</param>
        /// <param name="packet">Packet</param>
        public NetworkEvent(NetworkEventType eventType, Peer peer, Packet packet)
        {
            EventType = eventType;
            Peer = peer;
            Packet = packet;
        }
    }
}