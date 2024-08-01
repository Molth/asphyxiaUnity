using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LiteNetLib;
using UnityEngine;

namespace Mirror
{
    public sealed class LiteNetLibTransport : Transport, PortTransport, INetEventListener
    {
        public int MaxPeers = 100;
        private bool _isServer;
        private NetManager _host;
        private ConcurrentDictionary<int, NetPeer> _peers;
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

        public void OnPeerConnected(NetPeer peer) => _networkEvents.Enqueue(new NetworkEvent(NetworkEventType.Connect, peer));

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) => _networkEvents.Enqueue(new NetworkEvent(NetworkEventType.Disconnect, peer));

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
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
            if (address == "localhost")
                address = "127.0.0.1";
            _isServer = false;
            _host.Start();
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
            _isServer = true;
            _host.Start(Port);
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