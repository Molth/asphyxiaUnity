//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER || GODOT
using System;
using System.Collections.Generic;
#endif
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
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
        private Socket? _socket;

        /// <summary>
        ///     Managed buffer
        /// </summary>
        private readonly byte[] _managedBuffer = new byte[KCP_FLUSH_BUFFER_SIZE];

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
        private
#if NET8_0_OR_GREATER
            SocketAddress
#else
            EndPoint
#endif
            _remoteEndPoint;

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
        public bool IsSet => _socket != null;

        /// <summary>
        ///     LocalEndPoint
        /// </summary>
        public IPEndPoint LocalEndPoint => (IPEndPoint)_socket.LocalEndPoint;

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
                _socket = null;
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
                IPEndPoint localEndPoint;
                if (ipv6)
                {
                    _socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                    _socket.DualMode = true;
                    localEndPoint = new IPEndPoint(IPAddress.IPv6Any, port);
                }
                else
                {
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    localEndPoint = new IPEndPoint(IPAddress.Any, port);
                }

                try
                {
                    _socket.Bind(localEndPoint);
                }
                catch
                {
                    _socket.Dispose();
                    _socket = null;
                    return SocketError.AddressAlreadyInUse;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    _socket.IOControl(-1744830452, new byte[1], null);
#if NET8_0_OR_GREATER
                if (_remoteEndPoint == null || _remoteEndPoint.Family != _socket.AddressFamily)
                    _remoteEndPoint = ((IPEndPoint)_socket.LocalEndPoint).Serialize();
#else
                if (_remoteEndPoint == null || _remoteEndPoint.AddressFamily != _socket.AddressFamily)
                    _remoteEndPoint = new IPEndPoint(((IPEndPoint)_socket.LocalEndPoint).Address, 0);
#endif
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
        public Peer? Connect(string ipAddress, ushort port) => !IsSet ? null : ConnectInternal(new IPEndPoint(IPAddress.Parse(ipAddress), port));

        /// <summary>
        ///     Connect
        /// </summary>
        /// <param name="remoteEndPoint">Remote endPoint</param>
        public Peer? Connect(IPEndPoint remoteEndPoint) => !IsSet ? null : ConnectInternal(remoteEndPoint);

        /// <summary>
        ///     Connect
        /// </summary>
        /// <param name="remoteEndPoint">Remote endPoint</param>
        private Peer? ConnectInternal(IPEndPoint remoteEndPoint)
        {
            if (remoteEndPoint.AddressFamily != _socket.AddressFamily && !_socket.DualMode)
                return null;
#if NET8_0_OR_GREATER
            var socketAddress = remoteEndPoint.Serialize();
            var hashCode = socketAddress.GetHashCode();
#else
            var hashCode = remoteEndPoint.GetHashCode();
#endif
            if (_peers.TryGetValue(hashCode, out var peer))
                return peer;
            if (_peers.Count >= _maxPeers)
                return null;
            var buffer = stackalloc byte[1];
            RandomNumberGenerator.Fill(new Span<byte>(buffer, 1));
            var sessionId = *buffer;
#if NET8_0_OR_GREATER
            peer = new Peer(sessionId, this, _idPool.TryDequeue(out var id) ? id : _id++, remoteEndPoint, socketAddress, _managedBuffer, _unmanagedBuffer, Current, PeerState.Connecting);
#else
            peer = new Peer(sessionId, this, _idPool.TryDequeue(out var id) ? id : _id++, remoteEndPoint, _managedBuffer, _unmanagedBuffer, Current, PeerState.Connecting);
#endif
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
            PingInternal(new IPEndPoint(IPAddress.Parse(ipAddress), port));
        }

        /// <summary>
        ///     Ping
        /// </summary>
        /// <param name="remoteEndPoint">Remote endPoint</param>
        public void Ping(IPEndPoint remoteEndPoint)
        {
            if (!IsSet)
                return;
            PingInternal(remoteEndPoint);
        }

        /// <summary>
        ///     Ping
        /// </summary>
        /// <param name="remoteEndPoint">Remote endPoint</param>
        private void PingInternal(IPEndPoint remoteEndPoint)
        {
            if (remoteEndPoint.AddressFamily != _socket.AddressFamily && !_socket.DualMode)
                return;
            _managedBuffer[0] = (byte)Header.Ping;
            Insert(remoteEndPoint
#if NET8_0_OR_GREATER
                    .Serialize()
#endif
                , 1);
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
            if (_socket.Poll(0, SelectMode.SelectRead))
            {
#if NET8_0_OR_GREATER
                var buffer = _managedBuffer.AsSpan(0, SOCKET_BUFFER_SIZE);
#endif
                var remoteEndPoint = _remoteEndPoint.GetHashCode();
                do
                {
                    int count;
                    try
                    {
#if NET8_0_OR_GREATER
                        count = _socket.ReceiveFrom(buffer, SocketFlags.None, _remoteEndPoint);
#else
                        count = _socket.ReceiveFrom(_managedBuffer, 0, SOCKET_BUFFER_SIZE, SocketFlags.None, ref _remoteEndPoint);
#endif
                    }
                    catch
                    {
                        continue;
                    }

                    var hashCode = _remoteEndPoint.GetHashCode();
                    try
                    {
                        count--;
                        int flags = _managedBuffer[count];
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
                                if (count == 3 && _managedBuffer[0] == (byte)Header.Disconnect && _managedBuffer[1] == (byte)Header.DisconnectAcknowledge)
                                {
                                    if ((_peer == null || hashCode != remoteEndPoint) && !_peers.TryGetValue(hashCode, out _peer))
                                        continue;
                                    _peer.TryDisconnectNow(_managedBuffer[2]);
                                }

                                continue;
                            }

                            if ((_peer == null || hashCode != remoteEndPoint) && !_peers.TryGetValue(hashCode, out _peer))
                            {
                                if (count != 22 || _managedBuffer[21] != (byte)Header.Connect || _peers.Count >= _maxPeers)
                                    continue;
#if NET8_0_OR_GREATER
                                var ipEndPoint = _remoteEndPoint.CreateIPEndPoint();
                                var socketAddress = ipEndPoint.Serialize();
                                _peer = new Peer(_managedBuffer[0], this, _idPool.TryDequeue(out var id) ? id : _id++, ipEndPoint, socketAddress, _managedBuffer, _unmanagedBuffer, current);
#else
                                _peer = new Peer(_managedBuffer[0], this, _idPool.TryDequeue(out var id) ? id : _id++, _remoteEndPoint, _managedBuffer, _unmanagedBuffer, current);
#endif
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
                } while (_socket.Poll(0, SelectMode.SelectRead));
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
        internal void Insert(
#if NET8_0_OR_GREATER
            SocketAddress
#else
            EndPoint
#endif
                endPoint, int length)
        {
            try
            {
                _socket.SendTo(_managedBuffer, 0, length, SocketFlags.None, endPoint);
            }
            catch
            {
                _socket.Poll(0, SelectMode.SelectWrite);
            }
        }

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