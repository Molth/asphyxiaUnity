using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using LiteNetLib;
using UnityEngine;

namespace Mirror
{
    public sealed class LiteNetLibNatTravelTransport : Transport, PortTransport, INetEventListener
    {
        public string ServiceIPAddress;
        public ushort ServicePort;
        public int MaxPeers = 100;
        private IPEndPoint _serviceIPEndPoint;
        private IPEndPoint _serverIPEndPoint;
        private IPEndPoint _localEndPoint;
        private bool _isServer;
        private NetManager _host;
        private ConcurrentDictionary<int, NetPeer> _peers;
        private NetPeer _servicePeer;
        private NetPeer _peer;
        private ConcurrentQueue<NetworkEvent> _networkEvents;
        private ConcurrentQueue<NetPeer> _disconnectPeers;
        private ConcurrentQueue<NetworkOutgoing> _outgoings;
        private int _running;
        private byte[] _receiveBuffer;

        private void Start()
        {
            if (Port == 0)
                Port = 7777;
            _host = new NetManager(this);
            _peers = new ConcurrentDictionary<int, NetPeer>();
            _networkEvents = new ConcurrentQueue<NetworkEvent>();
            _disconnectPeers = new ConcurrentQueue<NetPeer>();
            _outgoings = new ConcurrentQueue<NetworkOutgoing>();
            _receiveBuffer = new byte[2048];
        }

        private void OnGUI()
        {
            if (_localEndPoint == null)
                return;
            GUILayout.BeginArea(new Rect(10, 120, 300, 9999));
            GUILayout.Label($"<b>Local</b>: {_localEndPoint}");
            GUILayout.EndArea();
        }

        public void OnPeerConnected(NetPeer peer)
        {
            if (peer.Equals(_serviceIPEndPoint))
            {
                if (!_isServer)
                {
                    if (!NetworkClient.isConnected)
                    {
                        var dataPacket = _serverIPEndPoint.CreateDataPacket();
                        try
                        {
                            _servicePeer.Send(dataPacket.AsSpan(), dataPacket.Flags);
                        }
                        finally
                        {
                            dataPacket.Dispose();
                        }
                    }
                }
            }

            _networkEvents.Enqueue(new NetworkEvent(NetworkEventType.Connect, peer));
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) => _networkEvents.Enqueue(new NetworkEvent(NetworkEventType.Disconnect, peer));

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            if (peer.Equals(_serviceIPEndPoint))
            {
                var span = reader.RawData.AsSpan(reader.Position, reader.AvailableBytes);
                if (!_isServer)
                {
                    var isLocalEndPoint = span[0] == 0;
                    span = span[1..];
                    IPAddress address;
                    try
                    {
                        address = new IPAddress(span[..^4]);
                    }
                    catch
                    {
                        reader.Recycle();
                        return;
                    }

                    var port = Unsafe.ReadUnaligned<int>(ref span[^4]);
                    var ipEndPoint = new IPEndPoint(address, port);
                    if (isLocalEndPoint)
                        _localEndPoint = ipEndPoint;
                }
                else
                {
                    var isLocalEndPoint = span[0] == 0;
                    span = span[1..];
                    IPAddress address;
                    try
                    {
                        address = new IPAddress(span[..^4]);
                    }
                    catch
                    {
                        reader.Recycle();
                        return;
                    }

                    var port = Unsafe.ReadUnaligned<int>(ref span[^4]);
                    var ipEndPoint = new IPEndPoint(address, port);
                    if (isLocalEndPoint)
                        _localEndPoint = ipEndPoint;
                    else
                        _host.SendUnconnectedMessage(span[..1], ipEndPoint);
                }

                reader.Recycle();
                return;
            }

            _networkEvents.Enqueue(new NetworkEvent(NetworkEventType.Data, peer, DataPacket.Create(reader.RawData.AsSpan(reader.Position, reader.AvailableBytes), deliveryMethod)));
            reader.Recycle();
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            if (_host.ConnectedPeersCount < MaxPeers)
                request.Accept();
            else
                request.Reject();
        }

        public ushort Port { get; set; } = 7777;

        public override void ServerEarlyUpdate()
        {
            if (!_host.IsRunning)
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
                            packet.CopyTo(_receiveBuffer);
                            OnClientDataReceived(new ArraySegment<byte>(_receiveBuffer, 0, packet.Length), 0);
                            packet.Dispose();
                            break;
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
                            OnServerConnected(id + 1);
                            break;
                        case NetworkEventType.Data:
                            var packet = networkEvent.Packet;
                            packet.CopyTo(_receiveBuffer);
                            OnServerDataReceived(id + 1, new ArraySegment<byte>(_receiveBuffer, 0, packet.Length), 0);
                            packet.Dispose();
                            break;
                        case NetworkEventType.Disconnect:
                            OnServerDisconnected(id + 1);
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
                _host.PollEvents();
                while (_disconnectPeers.TryDequeue(out var peer))
                    peer.Disconnect();
                while (_outgoings.TryDequeue(out var outgoing))
                    outgoing.Send();
                spinWait.SpinOnce();
            }

            _host.DisconnectAll();
            _peers.Clear();
            _host.Stop();
            Shutdown();
        }

        public override bool Available() => Application.platform != RuntimePlatform.WebGLPlayer;

        public override bool ClientConnected() => _peer != null;

        public override void ClientConnect(string address)
        {
            _serviceIPEndPoint = new IPEndPoint(IPAddress.Parse(ServiceIPAddress), ServicePort);
            if (address == "localhost")
                address = "127.0.0.1";
            _serverIPEndPoint = new IPEndPoint(IPAddress.Parse(address), Port);
            _isServer = false;
            _host.Start();
            _servicePeer = _host.Connect(_serviceIPEndPoint, "");
            _host.Connect(new IPEndPoint(IPAddress.Parse(address), Port), "");
            new Thread(Service) { IsBackground = true }.Start();
        }

        public override void ClientSend(ArraySegment<byte> segment, int channelId = Channels.Reliable)
        {
            if (_peer != null)
            {
                var flag = channelId == Channels.Reliable ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable;
                _outgoings.Enqueue(NetworkOutgoing.Create(_peer, segment, flag));
            }
        }

        public override void ClientDisconnect() => Interlocked.Exchange(ref _running, 0);

        public override Uri ServerUri() => null;

        public override bool ServerActive() => _host.IsRunning;

        public override void ServerStart()
        {
            _serviceIPEndPoint = new IPEndPoint(IPAddress.Parse(ServiceIPAddress), ServicePort);
            _isServer = true;
            _host.Start(Port);
            _servicePeer = _host.Connect(_serviceIPEndPoint, "");
            new Thread(Service) { IsBackground = true }.Start();
        }

        public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId = Channels.Reliable)
        {
            if (_peers.TryGetValue(connectionId - 1, out var peer))
            {
                var flag = channelId == Channels.Reliable ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable;
                _outgoings.Enqueue(NetworkOutgoing.Create(peer, segment, flag));
            }
        }

        public override void ServerDisconnect(int connectionId)
        {
            if (_peers.TryGetValue(connectionId - 1, out var peer))
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