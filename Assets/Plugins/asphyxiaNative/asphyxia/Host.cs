//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER || GODOT
using System;
using System.Collections.Generic;
#endif
using System.Net.Sockets;
using System.Security.Cryptography;
using NanoSockets;
using static asphyxia.Settings;
using static asphyxia.Time;
using static asphyxia.PacketFlag;
using static System.Runtime.InteropServices.Marshal;
using static KCP.KCPBASIC;

#pragma warning disable CA1816
#pragma warning disable CS0162
#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604
#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable CS8632

// ReSharper disable RedundantIfElseBlock
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable PossibleNullReferenceException
// ReSharper disable CommentTypo

namespace asphyxia
{
    /// <summary>
    ///     Host
    /// </summary>
    public sealed unsafe class Host : IDisposable
    {
        /// <summary>
        ///     Socket
        /// </summary>
        private readonly NanoSocket? _socket = new();

        /// <summary>
        ///     Unmanaged buffer
        /// </summary>
        private byte* _unmanagedBuffer;

        /// <summary>
        ///     Max peers
        /// </summary>
        private int _maxPeers;

        /// <summary>
        ///     Id
        /// </summary>
        private uint _id;

        /// <summary>
        ///     Id pool
        /// </summary>
        private readonly Queue<uint> _idPool = new();

        /// <summary>
        ///     Sentinel
        /// </summary>
        private Peer? _sentinel;

        /// <summary>
        ///     Peers
        /// </summary>
        private readonly Dictionary<int, Peer> _peers = new();

        /// <summary>
        ///     NetworkEvents
        /// </summary>
        private readonly Queue<NetworkEvent> _networkEvents = new();

        /// <summary>
        ///     Remote endPoint
        /// </summary>
        private NanoIPEndPoint _remoteEndPoint;

        /// <summary>
        ///     Peer
        /// </summary>
        private Peer? _peer;

        /// <summary>
        ///     Service timestamp
        /// </summary>
        private uint _serviceTimestamp;

        /// <summary>
        ///     Flush timestamp
        /// </summary>
        private uint _flushTimestamp;

        /// <summary>
        ///     State lock
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsSet => _socket.IsSet;

        /// <summary>
        ///     LocalEndPoint
        /// </summary>
        public NanoIPEndPoint LocalEndPoint => _socket.LocalEndPoint;

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                if (!IsSet)
                    return;
                _socket.Close();
                FreeHGlobal((nint)_unmanagedBuffer);
                _maxPeers = 0;
                _id = 0;
                _idPool.Clear();
                _peers.Clear();
                _sentinel = null;
                while (_networkEvents.TryDequeue(out var networkEvent))
                {
                    if (networkEvent.EventType != NetworkEventType.Data)
                        continue;
                    networkEvent.Packet.Dispose();
                }

                _peer = null;
            }
        }

        /// <summary>
        ///     Destructure
        /// </summary>
        ~Host() => Dispose();

        /// <summary>
        ///     Create
        /// </summary>
        /// <param name="maxPeers">Max peers</param>
        /// <param name="port">Port</param>
        /// <param name="ipv6">IPv6</param>
        public SocketError Create(int maxPeers, ushort port = 0, bool ipv6 = false)
        {
            lock (_lock)
            {
                if (IsSet)
                    return SocketError.InvalidArgument;
                if (ipv6 && !Socket.OSSupportsIPv6)
                    return SocketError.SocketNotSupported;
                _socket.Create(0, 0);
                var localEndPoint = ipv6 ? NanoIPEndPoint.Any(port) : NanoIPEndPoint.IPv6Any(port);
                try
                {
                    _socket.Bind(ref localEndPoint);
                }
                catch
                {
                    _socket.Dispose();
                    return SocketError.AddressAlreadyInUse;
                }

                if (maxPeers <= 0)
                    maxPeers = 1;
                var socketBufferSize = maxPeers * SOCKET_BUFFER_SIZE;
                if (socketBufferSize < 16777216)
                    socketBufferSize = 16777216;
                _socket.SendBufferSize = socketBufferSize;
                _socket.ReceiveBufferSize = socketBufferSize;
                _socket.Blocking = false;
                _idPool.EnsureCapacity(maxPeers);
                _peers.EnsureCapacity(maxPeers);
                var maxReceiveEvents = maxPeers << 1;
                _networkEvents.EnsureCapacity(maxReceiveEvents);
                _unmanagedBuffer = (byte*)AllocHGlobal(KCP_FLUSH_BUFFER_SIZE);
                _maxPeers = maxPeers;
                return SocketError.Success;
            }
        }

        /// <summary>
        ///     Check events
        /// </summary>
        /// <param name="networkEvent">NetworkEvent</param>
        /// <returns>Checked</returns>
        public bool CheckEvents(out NetworkEvent networkEvent) => _networkEvents.TryDequeue(out networkEvent);

        /// <summary>
        ///     Connect
        /// </summary>
        /// <param name="ipAddress">IPAddress</param>
        /// <param name="port">Port</param>
        public Peer? Connect(string ipAddress, ushort port) => !IsSet ? null : ConnectInternal(NanoIPEndPoint.Create(ipAddress, port));

        /// <summary>
        ///     Connect
        /// </summary>
        /// <param name="remoteEndPoint">Remote endPoint</param>
        public Peer? Connect(NanoIPEndPoint remoteEndPoint) => !IsSet ? null : ConnectInternal(remoteEndPoint);

        /// <summary>
        ///     Connect
        /// </summary>
        /// <param name="remoteEndPoint">Remote endPoint</param>
        private Peer? ConnectInternal(NanoIPEndPoint remoteEndPoint)
        {
            var hashCode = remoteEndPoint.GetHashCode();
            if (_peers.TryGetValue(hashCode, out var peer))
                return peer;
            if (_peers.Count >= _maxPeers)
                return null;
            var buffer = stackalloc byte[1];
            RandomNumberGenerator.Fill(new Span<byte>(buffer, 1));
            var sessionId = *buffer;
            peer = new Peer(sessionId, this, _idPool.TryDequeue(out var id) ? id : _id++, remoteEndPoint, _unmanagedBuffer, Current, PeerState.Connecting);
            _peers[hashCode] = peer;
            _peer ??= peer;
            if (_sentinel == null)
            {
                _sentinel = peer;
            }
            else
            {
                _sentinel.Previous = peer;
                peer.Next = _sentinel;
                _sentinel = peer;
            }

            buffer[0] = (byte)Header.Connect;
            peer.KcpSend(buffer, 1);
            return peer;
        }

        /// <summary>
        ///     Ping
        /// </summary>
        /// <param name="ipAddress">IPAddress</param>
        /// <param name="port">Port</param>
        public void Ping(string ipAddress, ushort port)
        {
            if (!IsSet)
                return;
            PingInternal(NanoIPEndPoint.Create(ipAddress, port));
        }

        /// <summary>
        ///     Ping
        /// </summary>
        /// <param name="remoteEndPoint">Remote endPoint</param>
        public void Ping(NanoIPEndPoint remoteEndPoint)
        {
            if (!IsSet)
                return;
            PingInternal(remoteEndPoint);
        }

        /// <summary>
        ///     Ping
        /// </summary>
        /// <param name="remoteEndPoint">Remote endPoint</param>
        private void PingInternal(NanoIPEndPoint remoteEndPoint)
        {
            _unmanagedBuffer[0] = (byte)Header.Ping;
            Insert(remoteEndPoint, 1);
        }

        /// <summary>
        ///     Service
        /// </summary>
        public void Service()
        {
            var current = Current;
            if (current == _serviceTimestamp)
                return;
            _serviceTimestamp = current;
            if (_socket.Poll(0))
            {
                var remoteEndPoint = _remoteEndPoint.GetHashCode();
                do
                {
                    int count;
                    try
                    {
                        count = _socket.Receive(_unmanagedBuffer, SOCKET_BUFFER_SIZE, ref _remoteEndPoint);
                    }
                    catch
                    {
                        continue;
                    }

                    var hashCode = _remoteEndPoint.GetHashCode();
                    try
                    {
                        count--;
                        int flags = _unmanagedBuffer[count];
                        if ((flags & (int)Unreliable) != 0)
                        {
                            if (count <= 1 || ((_peer == null || hashCode != remoteEndPoint) && !_peers.TryGetValue(hashCode, out _peer)))
                                continue;
                            _peer.ReceiveUnreliable(count);
                            continue;
                        }

                        if ((flags & (int)Sequenced) != 0)
                        {
                            if (count <= 3 || ((_peer == null || hashCode != remoteEndPoint) && !_peers.TryGetValue(hashCode, out _peer)))
                                continue;
                            _peer.ReceiveSequenced(count);
                            continue;
                        }

                        if ((flags & (int)Reliable) != 0)
                        {
                            if (count < (int)REVERSED_HEAD + (int)OVERHEAD)
                            {
                                if (count == 3 && _unmanagedBuffer[0] == (byte)Header.Disconnect && _unmanagedBuffer[1] == (byte)Header.DisconnectAcknowledge)
                                {
                                    if ((_peer == null || hashCode != remoteEndPoint) && !_peers.TryGetValue(hashCode, out _peer))
                                        continue;
                                    _peer.TryDisconnectNow(_unmanagedBuffer[2]);
                                }

                                continue;
                            }

                            if ((_peer == null || hashCode != remoteEndPoint) && !_peers.TryGetValue(hashCode, out _peer))
                            {
                                if (count != 22 || _unmanagedBuffer[21] != (byte)Header.Connect || _peers.Count >= _maxPeers)
                                    continue;
                                _peer = new Peer(_unmanagedBuffer[0], this, _idPool.TryDequeue(out var id) ? id : _id++, _remoteEndPoint, _unmanagedBuffer, current);
                                _peers[hashCode] = _peer;
                                if (_sentinel == null)
                                {
                                    _sentinel = _peer;
                                }
                                else
                                {
                                    _sentinel.Previous = _peer;
                                    _peer.Next = _sentinel;
                                    _sentinel = _peer;
                                }
                            }

                            _peer.ReceiveReliable(count, current);
                        }
                    }
                    finally
                    {
                        remoteEndPoint = hashCode;
                    }
                } while (_socket.Poll(0));
            }

            var node = _sentinel;
            while (node != null)
            {
                var temp = node;
                node = node.Next;
                temp.Service(current);
            }
        }

        /// <summary>
        ///     Flush
        /// </summary>
        public void Flush()
        {
            var current = Current;
            if (current == _flushTimestamp)
                return;
            _flushTimestamp = current;
            var node = _sentinel;
            while (node != null)
            {
                var temp = node;
                node = node.Next;
                temp.Update(current);
            }
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="networkEvent">NetworkEvent</param>
        internal void Insert(in NetworkEvent networkEvent) => _networkEvents.Enqueue(networkEvent);

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="endPoint">Remote endPoint</param>
        /// <param name="length">Length</param>
        internal void Insert(NanoIPEndPoint endPoint, int length) => _socket.Send(_unmanagedBuffer, length, &endPoint);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="hashCode">HashCode</param>
        /// <param name="peer">Peer</param>
        internal void Remove(int hashCode, Peer peer)
        {
            if (_peer == peer)
                _peer = null;
            _idPool.Enqueue(peer.Id);
            _peers.Remove(hashCode);
            if (peer.Previous != null)
                peer.Previous.Next = peer.Next;
            else
                _sentinel = peer.Next;
            if (peer.Next != null)
                peer.Next.Previous = peer.Previous;
        }
    }
}