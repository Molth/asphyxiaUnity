using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using asphyxia;
using NanoSockets;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using asphyxiaNetworkEvent = asphyxia.NetworkEvent;
using NetcodeNetworkEvent = Unity.Netcode.NetworkEvent;

namespace Unity.Netcode.Transports.Native
{
    public sealed class KcpNatTravelTransport : UnityTransport
    {
        public string ServiceIPAddress;
        public ushort ServicePort;
        public int MaxPeers = 100;
        private NanoIPEndPoint _serviceIPEndPoint;
        private NanoIPEndPoint _serverIPEndPoint;
        private string _localEndPoint;
        private bool _isServer;
        private Host _host;
        private ConcurrentDictionary<ulong, Peer> _peers;
        private Peer _servicePeer;
        private Peer _peer;
        private ConcurrentQueue<asphyxiaNetworkEvent> _networkEvents;
        private ConcurrentQueue<Peer> _disconnectPeers;
        private ConcurrentQueue<NetworkOutgoing> _outgoings;
        private int _running;
        private byte[] _receiveBuffer;
        public override ulong ServerClientId => _isServer ? 0UL : 2UL;

        private void Start()
        {
            _host = new Host();
            _peers = new ConcurrentDictionary<ulong, Peer>();
            _networkEvents = new ConcurrentQueue<asphyxiaNetworkEvent>();
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

        public override void Send(ulong clientId, ArraySegment<byte> payload, NetworkDelivery networkDelivery)
        {
            if (!_isServer)
            {
                if (_peer != null)
                {
                    var flag = networkDelivery switch
                    {
                        NetworkDelivery.Unreliable => PacketFlag.Unreliable,
                        NetworkDelivery.UnreliableSequenced => PacketFlag.Sequenced,
                        _ => PacketFlag.Reliable
                    };
                    _outgoings.Enqueue(NetworkOutgoing.Create(_peer, payload, flag));
                }

                return;
            }

            if (_peers.TryGetValue(clientId - 1, out var peer))
            {
                var flag = networkDelivery switch
                {
                    NetworkDelivery.Unreliable => PacketFlag.Unreliable,
                    NetworkDelivery.UnreliableSequenced => PacketFlag.Sequenced,
                    _ => PacketFlag.Reliable
                };
                _outgoings.Enqueue(NetworkOutgoing.Create(peer, payload, flag));
            }
        }

        public override NetcodeNetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
        {
            if (!_host.IsSet)
            {
                clientId = default;
                payload = default;
                receiveTime = Time.realtimeSinceStartup;
                return NetcodeNetworkEvent.Nothing;
            }

            while (_networkEvents.TryDequeue(out var networkEvent))
            {
                var id = networkEvent.Peer.Id;
                clientId = id + 1;
                receiveTime = Time.realtimeSinceStartup;
                switch (networkEvent.EventType)
                {
                    case NetworkEventType.Connect:
                        if (_isServer)
                            _peers[id] = networkEvent.Peer;
                        payload = default;
                        return NetcodeNetworkEvent.Connect;
                    case NetworkEventType.Data:
                        var packet = networkEvent.Packet;
                        try
                        {
                            packet.CopyTo(_receiveBuffer);
                            payload = new ArraySegment<byte>(_receiveBuffer, 0, packet.Length);
                        }
                        finally
                        {
                            packet.Dispose();
                        }

                        return NetcodeNetworkEvent.Data;
                    case NetworkEventType.Disconnect:
                    case NetworkEventType.Timeout:
                        _peers.TryRemove(id, out _);
                        payload = default;
                        return NetcodeNetworkEvent.Disconnect;
                    case NetworkEventType.None:
                        break;
                }
            }

            clientId = default;
            payload = default;
            receiveTime = Time.realtimeSinceStartup;
            return NetcodeNetworkEvent.Nothing;
        }

        public override bool StartClient()
        {
            if (_host.IsSet)
                return false;
            _serviceIPEndPoint = NanoIPEndPoint.Create(ServiceIPAddress, ServicePort);
            if (ConnectionData.Address == "localhost")
                ConnectionData.Address = "127.0.0.1";
            _serverIPEndPoint = NanoIPEndPoint.Create(ConnectionData.Address, ConnectionData.Port);
            _isServer = false;
            _host.Create(2);
            _servicePeer = _host.Connect(_serviceIPEndPoint);
            _peer = _host.Connect(_serverIPEndPoint);
            new Thread(Service) { IsBackground = true }.Start();
            return true;
        }

        public override bool StartServer()
        {
            if (_host.IsSet)
                return false;
            _serviceIPEndPoint = NanoIPEndPoint.Create(ServiceIPAddress, ServicePort);
            _isServer = true;
            _host.Create(MaxPeers, ConnectionData.Port);
            _servicePeer = _host.Connect(_serviceIPEndPoint);
            new Thread(Service) { IsBackground = true }.Start();
            return true;
        }

        public override void DisconnectRemoteClient(ulong clientId)
        {
            if (_peers.TryGetValue((uint)(clientId - 1), out var peer))
                _disconnectPeers.Enqueue(peer);
        }

        public override void DisconnectLocalClient()
        {
        }

        public override ulong GetCurrentRtt(ulong clientId) => _peers.TryGetValue((uint)(clientId - 1), out var peer) ? peer.RoundTripTime : 0UL;

        public override void Shutdown() => Interlocked.Exchange(ref _running, 0);

        public override void Initialize(NetworkManager networkManager = null)
        {
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
                                    if (!NetworkManager.Singleton.IsConnectedClient)
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
                                        NanoIPAddress address;
                                        try
                                        {
                                            address = new NanoIPAddress(span[..^4]);
                                        }
                                        catch
                                        {
                                            break;
                                        }

                                        var port = Unsafe.ReadUnaligned<int>(ref span[^4]);
                                        var ipEndPoint = new NanoIPEndPoint(address, (ushort)port);
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
                                        NanoIPAddress address;
                                        try
                                        {
                                            address = new NanoIPAddress(span[..^4]);
                                        }
                                        catch
                                        {
                                            break;
                                        }

                                        var port = Unsafe.ReadUnaligned<int>(ref span[^4]);
                                        var ipEndPoint = new NanoIPEndPoint(address, (ushort)port);
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