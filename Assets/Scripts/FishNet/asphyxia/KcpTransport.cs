using System;
using System.Collections.Concurrent;
using System.Threading;
using asphyxia;
using FishNet.Transporting;

namespace FishNet
{
    public sealed class KcpTransport : Transport
    {
        public string Address;
        public ushort Port;
        public IPAddressType IPAddressType;
        public int MaxPeers = 100;
        private bool _isServer;
        private Host _host;
        private ConcurrentDictionary<uint, Peer> _peers;
        private Peer _peer;
        private ConcurrentQueue<NetworkEvent> _networkEvents;
        private ConcurrentQueue<Peer> _disconnectPeers;
        private ConcurrentQueue<NetworkOutgoing> _outgoings;
        private int _running;
        private byte[] _receiveBuffer;

        private void Start()
        {
            _host = new Host();
            _peers = new ConcurrentDictionary<uint, Peer>();
            _networkEvents = new ConcurrentQueue<NetworkEvent>();
            _disconnectPeers = new ConcurrentQueue<Peer>();
            _outgoings = new ConcurrentQueue<NetworkOutgoing>();
            _receiveBuffer = new byte[2048];
        }

        private void Service()
        {
            var spinWait = new SpinWait();
            Interlocked.Exchange(ref _running, 1);
            while (_running == 1)
            {
                while (_disconnectPeers.TryDequeue(out var peer))
                    peer.DisconnectNow();
                while (_outgoings.TryDequeue(out var outgoing))
                    outgoing.Send();
                _host.Service();
                while (_host.CheckEvents(out var networkEvent))
                    _networkEvents.Enqueue(networkEvent);
                _host.Flush();
                spinWait.SpinOnce();
            }

            foreach (var peer in _peers.Values)
                peer.DisconnectNow();
            _peers.Clear();
            _host.Flush();
            _host.Dispose();
            Shutdown();
        }

        public override string GetConnectionAddress(int connectionId) => null;

        public override event Action<ClientConnectionStateArgs> OnClientConnectionState;

        public override event Action<ServerConnectionStateArgs> OnServerConnectionState;

        public override event Action<RemoteConnectionStateArgs> OnRemoteConnectionState;

        public override void HandleClientConnectionState(ClientConnectionStateArgs connectionStateArgs) => OnClientConnectionState?.Invoke(connectionStateArgs);

        public override void HandleServerConnectionState(ServerConnectionStateArgs connectionStateArgs) => OnServerConnectionState?.Invoke(connectionStateArgs);

        public override void HandleRemoteConnectionState(RemoteConnectionStateArgs connectionStateArgs) => OnRemoteConnectionState?.Invoke(connectionStateArgs);

        public override LocalConnectionState GetConnectionState(bool server)
        {
            if (server)
                return _host.IsSet ? LocalConnectionState.Started : LocalConnectionState.Stopped;
            if (_isServer)
                return LocalConnectionState.Started;
            if (_peer != null)
                return LocalConnectionState.Started;
            return LocalConnectionState.Stopped;
        }

        public override RemoteConnectionState GetConnectionState(int connectionId) => connectionId == 0 || _peers.ContainsKey((uint)(connectionId - 1)) ? RemoteConnectionState.Started : RemoteConnectionState.Stopped;

        public override void SendToServer(byte channelId, ArraySegment<byte> segment)
        {
            if (_isServer)
            {
                HandleServerReceivedDataArgs(new ServerReceivedDataArgs(segment, channelId == 0 ? Channel.Reliable : Channel.Unreliable, 0, Index));
                return;
            }

            if (_peer != null)
            {
                var flag = channelId == 0 ? PacketFlag.Reliable : PacketFlag.Sequenced;
                _outgoings.Enqueue(NetworkOutgoing.Create(_peer, segment, flag));
            }
        }

        public override void SendToClient(byte channelId, ArraySegment<byte> segment, int connectionId)
        {
            if (connectionId == 0)
            {
                HandleClientReceivedDataArgs(new ClientReceivedDataArgs(segment, channelId == 0 ? Channel.Reliable : Channel.Unreliable, Index));
                return;
            }

            if (_peers.TryGetValue((uint)(connectionId - 1), out var peer))
            {
                var flag = channelId == 0 ? PacketFlag.Reliable : PacketFlag.Sequenced;
                _outgoings.Enqueue(NetworkOutgoing.Create(peer, segment, flag));
            }
        }

        public override event Action<ClientReceivedDataArgs> OnClientReceivedData;

        public override void HandleClientReceivedDataArgs(ClientReceivedDataArgs receivedDataArgs) => OnClientReceivedData?.Invoke(receivedDataArgs);

        public override event Action<ServerReceivedDataArgs> OnServerReceivedData;

        public override void HandleServerReceivedDataArgs(ServerReceivedDataArgs receivedDataArgs) => OnServerReceivedData?.Invoke(receivedDataArgs);

        public override void IterateIncoming(bool server)
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
                            HandleClientConnectionState(new ClientConnectionStateArgs(LocalConnectionState.Started, Index));
                            break;
                        case NetworkEventType.Data:
                            var packet = networkEvent.Packet;
                            packet.CopyTo(_receiveBuffer);
                            var channelId = packet.Flags == PacketFlag.Reliable ? Channel.Reliable : Channel.Unreliable;
                            HandleClientReceivedDataArgs(new ClientReceivedDataArgs(new ArraySegment<byte>(_receiveBuffer, 0, packet.Length), channelId, Index));
                            packet.Dispose();
                            break;
                        case NetworkEventType.Timeout:
                        case NetworkEventType.Disconnect:
                            HandleClientConnectionState(new ClientConnectionStateArgs(LocalConnectionState.Stopped, Index));
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
                            HandleRemoteConnectionState(new RemoteConnectionStateArgs(RemoteConnectionState.Started, (int)(id + 1), Index));
                            break;
                        case NetworkEventType.Data:
                            var packet = networkEvent.Packet;
                            packet.CopyTo(_receiveBuffer);
                            var channelId = packet.Flags == PacketFlag.Reliable ? Channel.Reliable : Channel.Unreliable;
                            HandleServerReceivedDataArgs(new ServerReceivedDataArgs(new ArraySegment<byte>(_receiveBuffer, 0, packet.Length), channelId, (int)(id + 1), Index));
                            packet.Dispose();
                            break;
                        case NetworkEventType.Timeout:
                        case NetworkEventType.Disconnect:
                            HandleRemoteConnectionState(new RemoteConnectionStateArgs(RemoteConnectionState.Stopped, (int)(id + 1), Index));
                            _peers.TryRemove(id, out _);
                            break;
                        case NetworkEventType.None:
                            break;
                    }
                }
            }
        }

        public override void IterateOutgoing(bool server)
        {
        }

        public override bool StartConnection(bool server)
        {
            if (_host.IsSet)
            {
                if (_isServer)
                {
                    HandleClientConnectionState(new ClientConnectionStateArgs(LocalConnectionState.Starting, Index));
                    HandleClientConnectionState(new ClientConnectionStateArgs(LocalConnectionState.Started, Index));
                    HandleRemoteConnectionState(new RemoteConnectionStateArgs(RemoteConnectionState.Started, 0, Index));
                    return true;
                }

                return false;
            }

            if (server)
            {
                HandleServerConnectionState(new ServerConnectionStateArgs(LocalConnectionState.Starting, Index));
                _isServer = true;
                _host.Create(MaxPeers, Port, IPAddressType == IPAddressType.IPv6);
                new Thread(Service) { IsBackground = true }.Start();
                HandleServerConnectionState(new ServerConnectionStateArgs(LocalConnectionState.Started, Index));
            }
            else
            {
                if (Address == "localhost")
                    Address = "127.0.0.1";
                _isServer = false;
                _host.Create(1);
                _host.Connect(Address, Port);
                new Thread(Service) { IsBackground = true }.Start();
                HandleClientConnectionState(new ClientConnectionStateArgs(LocalConnectionState.Starting, Index));
            }

            return true;
        }

        public override void SetServerBindAddress(string address, IPAddressType addressType) => IPAddressType = addressType;

        public override void SetClientAddress(string address) => Address = address;

        public override void SetPort(ushort port) => Port = port;

        public override ushort GetPort() => Port;

        public override bool StopConnection(bool server)
        {
            if (server)
            {
                HandleServerConnectionState(new ServerConnectionStateArgs(LocalConnectionState.Stopping, Index));
                HandleServerConnectionState(new ServerConnectionStateArgs(LocalConnectionState.Stopped, Index));
            }
            else
            {
                HandleClientConnectionState(new ClientConnectionStateArgs(LocalConnectionState.Stopping, Index));
                HandleClientConnectionState(new ClientConnectionStateArgs(LocalConnectionState.Stopped, Index));
            }

            Interlocked.Exchange(ref _running, 0);
            Shutdown();
            return true;
        }

        public override bool StopConnection(int connectionId, bool immediately)
        {
            if (connectionId == 0)
            {
                HandleClientConnectionState(new ClientConnectionStateArgs(LocalConnectionState.Stopped, Index));
                return true;
            }

            if (_peers.TryGetValue((uint)(connectionId - 1), out var peer))
            {
                _disconnectPeers.Enqueue(peer);
                return true;
            }

            return false;
        }

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

        public override int GetMTU(byte channel) => Settings.KCP_MAXIMUM_TRANSMISSION_UNIT;
    }
}