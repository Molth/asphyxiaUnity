using System;
using System.Collections.Concurrent;
using System.Threading;
using asphyxia;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using asphyxiaNetworkEvent = asphyxia.NetworkEvent;
using NetcodeNetworkEvent = Unity.Netcode.NetworkEvent;

namespace Unity.Netcode.Transports.Native
{
    public sealed class KcpTransport : UnityTransport
    {
        public int MaxPeers = 100;
        private bool _isServer;
        private Host _host;
        private ConcurrentDictionary<ulong, Peer> _peers;
        private ConcurrentQueue<asphyxiaNetworkEvent> _networkEvents;
        private ConcurrentQueue<Peer> _disconnectPeers;
        private ConcurrentQueue<NetworkOutgoing> _outgoings;
        private int _running;
        private byte[] _receiveBuffer;
        public override ulong ServerClientId => _isServer ? 0UL : 1UL;

        private void Start()
        {
            _host = new Host();
            _peers = new ConcurrentDictionary<ulong, Peer>();
            _networkEvents = new ConcurrentQueue<asphyxiaNetworkEvent>();
            _disconnectPeers = new ConcurrentQueue<Peer>();
            _receiveBuffer = new byte[2048];
            _outgoings = new ConcurrentQueue<NetworkOutgoing>();
        }

        public override void Send(ulong clientId, ArraySegment<byte> payload, NetworkDelivery networkDelivery)
        {
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
                        _peers[id] = networkEvent.Peer;
                        payload = default;
                        return NetcodeNetworkEvent.Connect;
                    case NetworkEventType.Data:
                        var packet = networkEvent.Packet;
                        packet.CopyTo(_receiveBuffer);
                        payload = new ArraySegment<byte>(_receiveBuffer, 0, packet.Length);
                        packet.Dispose();
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
            _isServer = false;
            _host.Create(1);
            _host.Connect(ConnectionData.Address, ConnectionData.Port);
            new Thread(Service) { IsBackground = true }.Start();
            return true;
        }

        public override bool StartServer()
        {
            if (_host.IsSet)
                return false;
            _isServer = true;
            _host.Create(MaxPeers, ConnectionData.Port);
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
                    _networkEvents.Enqueue(networkEvent);
                while (_disconnectPeers.TryDequeue(out var peer))
                    peer.DisconnectNow();
                while (_outgoings.TryDequeue(out var outgoing))
                    outgoing.Send();
                _host.Flush();
                spinWait.SpinOnce();
            }

            foreach (var peer in _peers.Values)
                peer.DisconnectNow();
            _peers.Clear();
            _host.Flush();
            _host.Dispose();
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