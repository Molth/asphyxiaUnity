using System;
using System.Collections.Concurrent;
using System.Threading;
using asphyxia;
using NanoSockets;
using UnityEngine;

namespace Mirror.Native
{
    public sealed class KcpNatTravelTransport : Transport, PortTransport
    {
        public string ServiceIPAddress;
        public ushort ServicePort;
        public int MaxPeers = 100;
        private NanoIPEndPoint _serviceIPEndPoint;
        private NanoIPEndPoint _serverIPEndPoint;
        private string _localEndPoint;
        private bool _isServer;
        private Host _host;
        private ConcurrentDictionary<uint, Peer> _peers;
        private Peer _servicePeer;
        private Peer _peer;
        private ConcurrentQueue<NetworkEvent> _networkEvents;
        private ConcurrentQueue<Peer> _disconnectPeers;
        private ConcurrentQueue<NetworkOutgoing> _outgoings;
        private int _running;
        private byte[] _receiveBuffer;

        private void Start()
        {
            if (Port == 0)
                Port = 7777;
            _host = new Host();
            _peers = new ConcurrentDictionary<uint, Peer>();
            _networkEvents = new ConcurrentQueue<NetworkEvent>();
            _disconnectPeers = new ConcurrentQueue<Peer>();
            _outgoings = new ConcurrentQueue<NetworkOutgoing>();
            _receiveBuffer = new byte[2048];
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(_localEndPoint))
                return;
            GUILayout.BeginArea(new Rect(10, 120, 300, 9999));
            GUILayout.Label($"<b>Local</b>: {_localEndPoint}");
            GUILayout.EndArea();
        }

        public ushort Port { get; set; }

        public override void ServerEarlyUpdate()
        {
            if (!_host.IsSet)
                return;
            if (!_isServer)
            {
                while (_networkEvents.TryDequeue(out var networkEvent))
                {
                    switch (networkEvent.EventType)
                    {
                        case NetworkEventType.Connect:
                            _peer = networkEvent.Peer;
                            _peers[0] = _peer;
                            OnClientConnected();
                            break;
                        case NetworkEventType.Data:
                            var packet = networkEvent.Packet;
                            try
                            {
                                packet.CopyTo(_receiveBuffer);
                                var channelId = packet.Flags == PacketFlag.Reliable ? Channels.Reliable : Channels.Unreliable;
                                OnClientDataReceived(new ArraySegment<byte>(_receiveBuffer, 0, packet.Length), channelId);
                            }
                            finally
                            {
                                packet.Dispose();
                            }

                            break;
                        case NetworkEventType.Timeout:
                        case NetworkEventType.Disconnect:
                            OnClientDisconnected();
                            _peers.TryRemove(0, out _);
                            _peer = null;
                            break;
                        case NetworkEventType.None:
                            break;
                    }
                }
            }
            else
            {
                while (_networkEvents.TryDequeue(out var networkEvent))
                {
                    var id = networkEvent.Peer.Id;
                    switch (networkEvent.EventType)
                    {
                        case NetworkEventType.Connect:
                            _peers[id] = networkEvent.Peer;
                            OnServerConnected((int)(id + 1));
                            break;
                        case NetworkEventType.Data:
                            var packet = networkEvent.Packet;
                            try
                            {
                                packet.CopyTo(_receiveBuffer);
                                var channelId = packet.Flags == PacketFlag.Reliable ? Channels.Reliable : Channels.Unreliable;
                                OnServerDataReceived((int)(id + 1), new ArraySegment<byte>(_receiveBuffer, 0, packet.Length), channelId);
                            }
                            finally
                            {
                                packet.Dispose();
                            }

                            break;
                        case NetworkEventType.Timeout:
                        case NetworkEventType.Disconnect:
                            OnServerDisconnected((int)(id + 1));
                            _peers.TryRemove(id, out _);
                            break;
                        case NetworkEventType.None:
                            break;
                    }
                }
            }
        }

        private void Service()
        {
            var spinWait = new SpinWait();
            Interlocked.Exchange(ref _running, 1);
            while (_running == 1)
            {
                _host.Service();
                while (_host.CheckEvents(out var networkEvent))
                {
                    if (networkEvent.Peer.IPEndPoint.Equals(_serviceIPEndPoint))
                    {
                        if (!_isServer)
                        {
                            switch (networkEvent.EventType)
                            {
                                case NetworkEventType.Connect:
                                    if (!NetworkClient.isConnected)
                                    {
                                        var dataPacket = _serverIPEndPoint.CreateDataPacket();
                                        try
                                        {
                                            _servicePeer.Send(dataPacket);
                                        }
                                        finally
                                        {
                                            dataPacket.Dispose();
                                        }
                                    }

                                    break;
                                case NetworkEventType.Data:
                                    var packet = networkEvent.Packet;
                                    try
                                    {
                                        var span = packet.AsSpan();
                                        var isLocalEndPoint = span[0] == 0;
                                        span = span[1..];
                                        var ipEndPoint = new NanoIPEndPoint(span);
                                        if (isLocalEndPoint)
                                            _localEndPoint = ipEndPoint.ToString();
                                    }
                                    finally
                                    {
                                        packet.Dispose();
                                    }

                                    break;
                                case NetworkEventType.Timeout:
                                    break;
                                case NetworkEventType.Disconnect:
                                    break;
                                case NetworkEventType.None:
                                    break;
                            }
                        }
                        else
                        {
                            switch (networkEvent.EventType)
                            {
                                case NetworkEventType.Connect:
                                    break;
                                case NetworkEventType.Data:
                                    var packet = networkEvent.Packet;
                                    try
                                    {
                                        var span = packet.AsSpan();
                                        var isLocalEndPoint = span[0] == 0;
                                        span = span[1..];
                                        var ipEndPoint = new NanoIPEndPoint(span);
                                        if (isLocalEndPoint)
                                            _localEndPoint = ipEndPoint.ToString();
                                        else
                                            _host.Ping(ipEndPoint);
                                    }
                                    finally
                                    {
                                        packet.Dispose();
                                    }

                                    break;
                                case NetworkEventType.Timeout:
                                    break;
                                case NetworkEventType.Disconnect:
                                    break;
                                case NetworkEventType.None:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        _networkEvents.Enqueue(networkEvent);
                    }
                }

                while (_disconnectPeers.TryDequeue(out var peer))
                    peer.DisconnectNow();
                while (_outgoings.TryDequeue(out var outgoing))
                    outgoing.Send();
                _host.Flush();
                spinWait.SpinOnce();
            }

            _servicePeer?.DisconnectNow();
            _servicePeer = null;
            _peer?.DisconnectNow();
            _peer = null;
            foreach (var peer in _peers.Values)
                peer.DisconnectNow();
            _peers.Clear();
            _host.Flush();
            _host.Dispose();
            _localEndPoint = null;
            Shutdown();
        }

        public override bool Available() => Application.platform != RuntimePlatform.WebGLPlayer;

        public override bool ClientConnected() => _peer != null;

        public override void ClientConnect(string address)
        {
            _serviceIPEndPoint = NanoIPEndPoint.Create(ServiceIPAddress, ServicePort);
            if (address == "localhost")
                address = "127.0.0.1";
            _serverIPEndPoint = NanoIPEndPoint.Create(address, Port);
            _isServer = false;
            _host.Create(2);
            _servicePeer = _host.Connect(_serviceIPEndPoint);
            _peer = _host.Connect(_serverIPEndPoint);
            new Thread(Service) { IsBackground = true }.Start();
        }

        public override void ClientSend(ArraySegment<byte> segment, int channelId = Channels.Reliable)
        {
            if (_peer != null)
            {
                var flag = channelId == Channels.Reliable ? PacketFlag.Reliable : PacketFlag.Sequenced;
                _outgoings.Enqueue(NetworkOutgoing.Create(_peer, segment, flag));
            }
        }

        public override void ClientDisconnect() => Interlocked.Exchange(ref _running, 0);

        public override Uri ServerUri() => null;

        public override bool ServerActive() => _host.IsSet;

        public override void ServerStart()
        {
            _serviceIPEndPoint = NanoIPEndPoint.Create(ServiceIPAddress, ServicePort);
            _isServer = true;
            _host.Create(MaxPeers, Port);
            _servicePeer = _host.Connect(_serviceIPEndPoint);
            new Thread(Service) { IsBackground = true }.Start();
        }

        public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId = Channels.Reliable)
        {
            if (_peers.TryGetValue((uint)(connectionId - 1), out var peer))
            {
                var flag = channelId == Channels.Reliable ? PacketFlag.Reliable : PacketFlag.Sequenced;
                _outgoings.Enqueue(NetworkOutgoing.Create(peer, segment, flag));
            }
        }

        public override void ServerDisconnect(int connectionId)
        {
            if (_peers.TryGetValue((uint)(connectionId - 1), out var peer))
                _disconnectPeers.Enqueue(peer);
        }

        public override string ServerGetClientAddress(int connectionId) => null;

        public override void ServerStop() => Interlocked.Exchange(ref _running, 0);

        public override int GetMaxPacketSize(int channelId = Channels.Reliable) => 1024;

        public override void Shutdown()
        {
            while (_networkEvents.TryDequeue(out var networkEvent))
            {
                if (networkEvent.EventType != NetworkEventType.Data)
                    continue;
                networkEvent.Packet.Dispose();
            }

            while (_outgoings.TryDequeue(out var outgoing))
                outgoing.Dispose();
        }
    }
}