using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using asphyxia;
using NanoSockets;
using Netick.Unity;
using UnityEngine;

namespace Netick.Transport
{
    [CreateAssetMenu(fileName = "AsphyxiaTransportProvider", menuName = "Netick/Transport/AsphyxiaTransportProvider", order = 1)]
    public sealed class AsphyxiaTransportProvider : NetworkTransportProvider
    {
        public override NetworkTransport MakeTransportInstance() => new AsphyxiaTransport();
    }

    public struct AsphyxiaEndPoint : IEndPoint
    {
        public string IPAddress { get; }
        public int Port { get; }

        public AsphyxiaEndPoint(NanoIPEndPoint ipEndPoint)
        {
            IPAddress = ipEndPoint.ToString().Split(':')[0];
            Port = ipEndPoint.Port;
        }
    }

    internal sealed class AsphyxiaConnection : TransportConnection
    {
        private readonly AsphyxiaTransport _transport;
        public readonly Peer Peer;
        public readonly AsphyxiaEndPoint IPEndPoint;

        public AsphyxiaConnection(AsphyxiaTransport transport, Peer peer)
        {
            _transport = transport;
            Peer = peer;
            IPEndPoint = new AsphyxiaEndPoint(peer.IPEndPoint);
        }

        public override IEndPoint EndPoint => IPEndPoint;

        public override int Mtu => Settings.KCP_MAXIMUM_TRANSMISSION_UNIT;

        public override void Send(IntPtr ptr, int length) => _transport.Send(Peer, ptr, length, PacketFlag.Unreliable);

        public override void SendUserData(IntPtr ptr, int length, TransportDeliveryMethod transportDeliveryMethod) => _transport.Send(Peer, ptr, length, transportDeliveryMethod == TransportDeliveryMethod.Reliable ? PacketFlag.Reliable : PacketFlag.Unreliable);
    }

    public sealed unsafe class AsphyxiaTransport : NetworkTransport
    {
        private readonly Host _host = new();
        private readonly Dictionary<uint, AsphyxiaConnection> _peers = new();
        private readonly ConcurrentQueue<NetworkEvent> _networkEvents = new();
        private readonly ConcurrentQueue<Peer> _disconnectPeers = new();
        private readonly ConcurrentQueue<NetworkOutgoing> _outgoings = new();
        private int _running;
        private BitBuffer _buffer;

        internal void Send(Peer peer, IntPtr ptr, int length, PacketFlag flags) => _outgoings.Enqueue(new NetworkOutgoing(peer, DataPacket.Create((byte*)ptr, length, flags)));

        public override void Init() => _buffer = new BitBuffer(createChunks: false);

        public override void Connect(string address, int port, byte[] connectionData, int connectionDataLength) => _host.Connect(address, (ushort)port);

        public override void Disconnect(TransportConnection connection) => _disconnectPeers.Enqueue(((AsphyxiaConnection)connection).Peer);

        public override void Run(RunMode mode, int port)
        {
            if (_host.IsSet)
                return;
            switch (mode)
            {
                case RunMode.Server:
                    _host.Create(Engine.Config.MaxPlayers, (ushort)port);
                    break;
                case RunMode.Client:
                    _host.Create(1);
                    break;
            }

            new Thread(Service) { IsBackground = true }.Start();
        }

        public override void Shutdown() => Interlocked.Exchange(ref _running, 0);

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
                _host.Flush();
                _host.Service();
                while (_host.CheckEvents(out var networkEvent))
                    _networkEvents.Enqueue(networkEvent);
                spinWait.SpinOnce();
            }

            foreach (var peer in _peers.Values)
                peer.Peer.DisconnectNow();
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

        public override void PollEvents()
        {
            if (!_host.IsSet)
                return;
            while (_networkEvents.TryDequeue(out var networkEvent))
            {
                var id = networkEvent.Peer.Id;
                AsphyxiaConnection connection;
                switch (networkEvent.EventType)
                {
                    case NetworkEventType.Connect:
                        connection = new AsphyxiaConnection(this, networkEvent.Peer);
                        _peers[id] = connection;
                        NetworkPeer.OnConnected(connection);
                        break;
                    case NetworkEventType.Data:
                        var packet = networkEvent.Packet;
                        try
                        {
                            if (_peers.TryGetValue(id, out connection))
                            {
                                _buffer.SetFrom(packet.Data, packet.Length, packet.Length);
                                NetworkPeer.Receive(connection, _buffer);
                            }
                        }
                        finally
                        {
                            packet.Dispose();
                        }

                        break;
                    case NetworkEventType.Timeout:
                    case NetworkEventType.Disconnect:
                        if (_peers.Remove(id, out connection))
                            NetworkPeer.OnDisconnected(connection, TransportDisconnectReason.Shutdown);
                        break;
                    case NetworkEventType.None:
                        break;
                }
            }
        }
    }
}