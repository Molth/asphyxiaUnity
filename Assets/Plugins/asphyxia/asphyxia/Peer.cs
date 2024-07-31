//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

using System.Net;
using KCP;
using static asphyxia.Settings;
using static asphyxia.PacketFlag;
using static asphyxia.PeerState;
using static asphyxia.Header;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS8602
#pragma warning disable CS8632

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable PossibleNullReferenceException

namespace asphyxia
{
    /// <summary>
    ///     Peer
    /// </summary>
    public sealed unsafe class Peer
    {
        /// <summary>
        ///     Previous
        /// </summary>
        internal Peer? Previous;

        /// <summary>
        ///     Next
        /// </summary>
        internal Peer? Next;

        /// <summary>
        ///     Host
        /// </summary>
        private readonly Host _host;

        /// <summary>
        ///     Id
        /// </summary>
        public readonly uint Id;

        /// <summary>
        ///     IPEndPoint
        /// </summary>
        public readonly IPEndPoint IPEndPoint;

#if NET8_0_OR_GREATER
        /// <summary>
        ///     SocketAddress
        /// </summary>
        private readonly SocketAddress _socketAddress;
#endif

        /// <summary>
        ///     Remote endPoint
        /// </summary>
        private
#if NET8_0_OR_GREATER
            SocketAddress EndPoint => _socketAddress;
#else
            EndPoint EndPoint => IPEndPoint;
#endif

        /// <summary>
        ///     HashCode
        /// </summary>
        private readonly int _hashCode;

        /// <summary>
        ///     Session id
        /// </summary>
        private readonly byte _sessionId;

        /// <summary>
        ///     Kcp
        /// </summary>
        private readonly Kcp _kcp;

        /// <summary>
        ///     Managed buffer
        /// </summary>
        private readonly byte[] _managedBuffer;

        /// <summary>
        ///     Unmanaged buffer
        /// </summary>
        private readonly byte* _unmanagedBuffer;

        /// <summary>
        ///     Last send sequence number
        /// </summary>
        private ushort _lastSendSequence;

        /// <summary>
        ///     Last receive sequence number
        /// </summary>
        private ushort _lastReceiveSequence = ushort.MaxValue;

        /// <summary>
        ///     Last send timestamp
        /// </summary>
        private uint _lastSendTimestamp;

        /// <summary>
        ///     Last receive timestamp
        /// </summary>
        private uint _lastReceiveTimestamp;

        /// <summary>
        ///     Last throttle timestamp
        /// </summary>
        private uint _lastThrottleTimestamp;

        /// <summary>
        ///     Peer state
        /// </summary>
        private PeerState _state;

        /// <summary>
        ///     Disconnecting
        /// </summary>
        private bool _disconnecting;

#if NET8_0_OR_GREATER
        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="sessionId">SessionId</param>
        /// <param name="host">Host</param>
        /// <param name="id">Id</param>
        /// <param name="ipEndPoint">IPEndPoint</param>
        /// <param name="socketAddress">SocketAddress</param>
        /// <param name="managedBuffer">Managed buffer</param>
        /// <param name="unmanagedBuffer">Unmanaged buffer</param>
        /// <param name="current">Timestamp</param>
        /// <param name="state">State</param>
#else
        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="sessionId">SessionId</param>
        /// <param name="host">Host</param>
        /// <param name="id">Id</param>
        /// <param name="ipEndPoint">IPEndPoint</param>
        /// <param name="managedBuffer">Managed buffer</param>
        /// <param name="unmanagedBuffer">Unmanaged buffer</param>
        /// <param name="current">Timestamp</param>
        /// <param name="state">State</param>
#endif
        internal Peer(byte sessionId, Host host, uint id, EndPoint ipEndPoint,
#if NET8_0_OR_GREATER
            SocketAddress socketAddress,
#endif
            byte[] managedBuffer, byte* unmanagedBuffer, uint current, PeerState state = PeerState.None)
        {
            _host = host;
            Id = id;
            IPEndPoint = (IPEndPoint)ipEndPoint;
#if NET8_0_OR_GREATER
            _socketAddress = socketAddress;
#endif
            _hashCode = EndPoint.GetHashCode();
            _sessionId = sessionId;
            _managedBuffer = managedBuffer;
            _unmanagedBuffer = unmanagedBuffer;
            _state = state;
            _kcp = new Kcp(this);
            _kcp.SetNoDelay(KCP_NO_DELAY, KCP_FLUSH_INTERVAL_MIN, KCP_FAST_RESEND, KCP_NO_CONGESTION_WINDOW);
            _kcp.SetWindowSize(KCP_WINDOW_SIZE_MIN, KCP_WINDOW_SIZE_MIN);
            _kcp.SetMtu(KCP_MAXIMUM_TRANSMISSION_UNIT);
            _kcp.SetMinrto(KCP_RTO_MIN);
            _lastSendTimestamp = current + PEER_PING_INTERVAL;
            _lastReceiveTimestamp = current + PEER_RECEIVE_TIMEOUT;
        }

        /// <summary>
        ///     Packets lost
        /// </summary>
        public uint PacketsLost => _kcp.SendNext - _kcp.SendUna - _kcp.AckCount;

        /// <summary>
        ///     Packet loss
        /// </summary>
        public float PacketLoss
        {
            get
            {
                var totalSentPackets = _kcp.SendNext - _kcp.SendUna;
                return totalSentPackets == 0 ? 0f : (float)(totalSentPackets - _kcp.AckCount) / totalSentPackets;
            }
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsSet => _kcp.IsSet;

        /// <summary>
        ///     Peer state
        /// </summary>
        public PeerState State => _state;

        /// <summary>
        ///     Session id
        /// </summary>
        public byte SessionId => _sessionId;

        /// <summary>
        ///     Smoothed round-trip time
        /// </summary>
        public uint RoundTripTime => (uint)_kcp.RxSrtt;

        /// <summary>
        ///     Output
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="current">Timestamp</param>
        internal void Output(int length, uint current)
        {
            _managedBuffer[0] = _sessionId;
            _lastSendTimestamp = current + PEER_PING_INTERVAL;
            _managedBuffer[length] = (byte)Reliable;
            _host.Insert(EndPoint, length + 1);
            if (_disconnecting && _kcp.SendQueueCount == 0)
            {
                _host.Insert(new NetworkEvent(NetworkEventType.Disconnect, this));
                _state = Disconnected;
                _kcp.Dispose();
                _host.Remove(_hashCode, this);
            }
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="current">Timestamp</param>
        internal void ReceiveReliable(int length, uint current)
        {
            if (_managedBuffer[0] != _sessionId || _kcp.Input(_managedBuffer, length) != 0 || _state != Connected)
                return;
            _lastReceiveTimestamp = current + PEER_RECEIVE_TIMEOUT;
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="length">Length</param>
        internal void ReceiveSequenced(int length)
        {
            if (_state != Connected || _managedBuffer[0] != _sessionId)
                return;
            var sendSequence = As<byte, ushort>(ref _managedBuffer[1]);
            if (sendSequence <= _lastReceiveSequence && _lastReceiveSequence - sendSequence <= 32767)
                return;
            _lastReceiveSequence = sendSequence;
            _host.Insert(new NetworkEvent(NetworkEventType.Data, this, DataPacket.Create(_managedBuffer, 3, length - 3, Sequenced)));
        }

        /// <summary>
        ///     Receive
        /// </summary>
        /// <param name="length">Length</param>
        internal void ReceiveUnreliable(int length)
        {
            if (_state != Connected || _managedBuffer[0] != _sessionId)
                return;
            _host.Insert(new NetworkEvent(NetworkEventType.Data, this, DataPacket.Create(_managedBuffer, 1, length - 1, Unreliable)));
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Sent bytes</returns>
        internal int KcpSend(byte* buffer, int length) => _kcp.Send(buffer, length);

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Sent bytes</returns>
        internal int SendReliable(byte* buffer, int length)
        {
            _unmanagedBuffer[0] = (byte)Data;
            CopyBlock(_unmanagedBuffer + 1, buffer, (uint)length);
            return KcpSend(_unmanagedBuffer, length + 1);
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        internal int SendSequenced(byte* buffer, int length)
        {
            _managedBuffer[0] = _sessionId;
            As<byte, ushort>(ref _managedBuffer[1]) = _lastSendSequence++;
            CopyBlock(ref _managedBuffer[3], ref *buffer, (uint)length);
            length += 4;
            _managedBuffer[length - 1] = (byte)Sequenced;
            _host.Insert(EndPoint, length);
            return length;
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        internal int SendUnreliable(byte* buffer, int length)
        {
            _managedBuffer[0] = _sessionId;
            CopyBlock(ref _managedBuffer[1], ref *buffer, (uint)length);
            length += 2;
            _managedBuffer[length - 1] = (byte)Unreliable;
            _host.Insert(EndPoint, length);
            return length;
        }

        /// <summary>
        ///     Send
        /// </summary>
        /// <param name="packet">DataPacket</param>
        /// <returns>Sent bytes</returns>
        public int Send(DataPacket packet)
        {
            if (_state != Connected)
                return -1;
            var flags = (int)packet.Flags;
            if ((flags & (int)Reliable) != 0)
                return SendReliable(packet.Data, packet.Length);
            if ((flags & (int)Sequenced) != 0)
                return SendSequenced(packet.Data, packet.Length);
            if ((flags & (int)Unreliable) != 0)
                return SendUnreliable(packet.Data, packet.Length);
            return -1;
        }

        /// <summary>
        ///     Timeout
        /// </summary>
        private void Timeout()
        {
            if (_state == Connected || _state == Disconnecting)
                _host.Insert(new NetworkEvent(NetworkEventType.Disconnect, this));
            else if (_state == Connecting)
                _host.Insert(new NetworkEvent(NetworkEventType.Timeout, this));
            _state = Disconnected;
            _kcp.Dispose();
            _host.Remove(_hashCode, this);
        }

        /// <summary>
        ///     Try disconnect now
        /// </summary>
        internal void TryDisconnectNow(byte sessionId)
        {
            if (_sessionId != sessionId || _state == Disconnected)
                return;
            if (_state == Connected)
                _host.Insert(new NetworkEvent(NetworkEventType.Disconnect, this));
            _state = Disconnected;
            _kcp.Dispose();
            _host.Remove(_hashCode, this);
        }

        /// <summary>
        ///     Disconnect
        /// </summary>
        private void DisconnectInternal()
        {
            if (_state == Disconnected)
                return;
            if (_state == Connected)
                _host.Insert(new NetworkEvent(NetworkEventType.Disconnect, this));
            _state = Disconnected;
            _kcp.Dispose();
            _host.Remove(_hashCode, this);
        }

        /// <summary>
        ///     Send disconnect now
        /// </summary>
        private void SendDisconnectNow()
        {
            _state = Disconnected;
            _kcp.Flush(_managedBuffer);
            _kcp.Dispose();
            _managedBuffer[0] = (byte)Header.Disconnect;
            _managedBuffer[1] = (byte)DisconnectAcknowledge;
            _managedBuffer[2] = _sessionId;
            _managedBuffer[3] = (byte)Reliable;
            _host.Insert(EndPoint, 4);
            _host.Remove(_hashCode, this);
        }

        /// <summary>
        ///     Disconnect
        /// </summary>
        public void Disconnect()
        {
            if (_state == Connected)
            {
                _state = Disconnecting;
                _unmanagedBuffer[0] = (byte)Header.Disconnect;
                KcpSend(_unmanagedBuffer, 1);
                return;
            }

            if (_state == Disconnecting || _state == Disconnected)
                return;
            SendDisconnectNow();
        }

        /// <summary>
        ///     Disconnect now
        /// </summary>
        public void DisconnectNow()
        {
            if (_state == Disconnecting || _state == Disconnected)
                return;
            if (_state == Connected)
                _host.Insert(new NetworkEvent(NetworkEventType.Disconnect, this));
            SendDisconnectNow();
        }

        /// <summary>
        ///     Disconnect later
        /// </summary>
        public void DisconnectLater()
        {
            if (_state != Connected)
                return;
            _state = Disconnecting;
            _unmanagedBuffer[0] = (byte)Header.Disconnect;
            KcpSend(_unmanagedBuffer, 1);
        }

        /// <summary>
        ///     Service
        /// </summary>
        /// <param name="current">Timestamp</param>
        internal void Service(uint current)
        {
            if (_lastReceiveTimestamp <= current)
            {
                Timeout();
                return;
            }

            while (true)
            {
                var received = _kcp.Receive(_unmanagedBuffer, KCP_MESSAGE_SIZE);
                if (received < 0)
                {
                    if (received != -1)
                    {
                        DisconnectInternal();
                        return;
                    }

                    break;
                }

                var header = _unmanagedBuffer[0];
                switch (header)
                {
                    case (byte)Ping:
                        if (_state != Connected)
                            goto error;
                        continue;
                    case (byte)Connect:
                        if (_state != PeerState.None)
                            goto error;
                        _state = ConnectAcknowledging;
                        _unmanagedBuffer[0] = (byte)ConnectAcknowledge;
                        KcpSend(_unmanagedBuffer, 1);
                        continue;
                    case (byte)ConnectAcknowledge:
                        if (_state != Connecting)
                            goto error;
                        _state = Connected;
                        _lastThrottleTimestamp = current + PEER_THROTTLE_INTERVAL;
                        _host.Insert(new NetworkEvent(NetworkEventType.Connect, this));
                        _unmanagedBuffer[0] = (byte)ConnectEstablish;
                        KcpSend(_unmanagedBuffer, 1);
                        continue;
                    case (byte)ConnectEstablish:
                        if (_state != ConnectAcknowledging)
                            goto error;
                        _state = Connected;
                        _lastThrottleTimestamp = current + PEER_THROTTLE_INTERVAL;
                        _host.Insert(new NetworkEvent(NetworkEventType.Connect, this));
                        continue;
                    case (byte)Data:
                        if (_state != Connected && _state != Disconnecting)
                            goto error;
                        _host.Insert(new NetworkEvent(NetworkEventType.Data, this, DataPacket.Create(_unmanagedBuffer + 1, received - 1, Reliable)));
                        continue;
                    case (byte)Header.Disconnect:
                        if (_state != Connected)
                            goto error;
                        _state = Disconnected;
                        _disconnecting = true;
                        _unmanagedBuffer[0] = (byte)DisconnectAcknowledge;
                        KcpSend(_unmanagedBuffer, 1);
                        continue;
                    case (byte)DisconnectAcknowledge:
                        if (_state != Disconnecting)
                            goto error;
                        _host.Insert(new NetworkEvent(NetworkEventType.Disconnect, this));
                        _kcp.Dispose();
                        _host.Remove(_hashCode, this);
                        return;
                    default:
                        error:
                        DisconnectInternal();
                        return;
                }
            }

            if (_state == Connected)
            {
                if (_lastSendTimestamp <= current)
                {
                    _lastSendTimestamp = current + PEER_PING_INTERVAL;
                    _unmanagedBuffer[0] = (byte)Ping;
                    KcpSend(_unmanagedBuffer, 1);
                }

                if (_lastThrottleTimestamp <= current)
                {
                    _lastThrottleTimestamp = current + PEER_THROTTLE_INTERVAL;
                    var rtt = RoundTripTime;
                    var loss = PacketLoss;
                    var rto = (int)(1.25f * rtt * (1.0f + loss));
                    if (rto < KCP_RTO_MIN)
                        rto = KCP_RTO_MIN;
                    else if (rto > KCP_RTO_MAX)
                        rto = KCP_RTO_MAX;
                    var variance = rtt / (100.0f + rtt) + loss;
                    var interval = (int)(KCP_FLUSH_INTERVAL_MIN * (1.0f + variance));
                    if (interval > KCP_FLUSH_INTERVAL_MAX)
                        interval = KCP_FLUSH_INTERVAL_MAX;
                    var windowSize = (int)(KCP_WINDOW_SIZE_MAX * (1.0f - variance));
                    if (windowSize < KCP_WINDOW_SIZE_MIN)
                        windowSize = KCP_WINDOW_SIZE_MIN;
                    _kcp.SetMinrto(rto);
                    _kcp.SetInterval(interval);
                    _kcp.SetWindowSize(windowSize, windowSize);
                }
            }
        }

        /// <summary>
        ///     Update
        /// </summary>
        internal void Update(uint current) => _kcp.Update(current, _managedBuffer);
    }
}