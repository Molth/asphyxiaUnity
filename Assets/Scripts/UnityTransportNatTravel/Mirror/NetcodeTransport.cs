//------------------------------------------------------------
// Erinn Network
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

using System;
using Erinn;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Mirror
{
    /// <summary>
    ///     Unity Transport
    /// </summary>
    public sealed class NetcodeTransport : Transport, PortTransport
    {
        /// <summary>
        ///     Netcode for gameObjects Unity Transport
        /// </summary>
        public UnityTransport Utp;

        /// <summary>
        ///     Client Connection
        /// </summary>
        private bool _clientConnected;

        /// <summary>
        ///     Server valid
        /// </summary>
        private bool _serverActive;

        /// <summary>
        ///     Registered
        /// </summary>
        private bool _register;

        /// <summary>
        ///     Server side index
        /// </summary>
        private ulong _serverId;

        /// <summary>
        ///     Client Index
        /// </summary>
        private IntIndexMap<ulong> _clientIdMap;

        /// <summary>
        ///     Port
        /// </summary>
        public ushort Port
        {
            get => Utp.ConnectionData.Port;
            set => Utp.ConnectionData.Port = value;
        }

        /// <summary>
        ///     Attempting to register
        /// </summary>
        private void TryRegister(bool isServer)
        {
            if (_register)
                return;
            _register = true;
            if (Utp is Netcode.NetcodeTransport udp)
            {
                if (!isServer)
                    udp.OnUserTransportEvent += HandleClientEvent;
                else
                    udp.OnUserTransportEvent += HandleServerEvent;
                return;
            }

            if (!isServer)
                Utp.OnTransportEvent += HandleClientEvent;
            else
                Utp.OnTransportEvent += HandleServerEvent;
        }

        /// <summary>
        ///     Cancel registration
        /// </summary>
        private void Unregister()
        {
            _register = false;
            Utp.OnTransportEvent -= HandleClientEvent;
            Utp.OnTransportEvent -= HandleServerEvent;
        }

        /// <summary>
        ///     Execute network events
        /// </summary>
        /// <param name="networkEvent">Event type</param>
        /// <param name="transportClientId">TransmissionId</param>
        /// <param name="payload">Data</param>
        /// <param name="receiveTime">Receive timestamp</param>
        private void HandleClientEvent(NetworkEvent networkEvent, ulong transportClientId, ArraySegment<byte> payload, float receiveTime)
        {
            switch (networkEvent)
            {
                case NetworkEvent.Data:
                    OnClientDataReceived.Invoke(payload, 0);
                    break;
                case NetworkEvent.Connect:
                    _serverId = transportClientId;
                    _clientConnected = true;
                    OnClientConnected.Invoke();
                    break;
                case NetworkEvent.Disconnect:
                    OnClientDisconnected.Invoke();
                    break;
                case NetworkEvent.TransportFailure:
                    break;
                case NetworkEvent.Nothing:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(networkEvent), networkEvent, null);
            }
        }

        /// <summary>
        ///     Execute network events
        /// </summary>
        /// <param name="networkEvent">Event type</param>
        /// <param name="transportClientId">TransmissionId</param>
        /// <param name="payload">Data</param>
        /// <param name="receiveTime">Receive timestamp</param>
        private void HandleServerEvent(NetworkEvent networkEvent, ulong transportClientId, ArraySegment<byte> payload, float receiveTime)
        {
            int clientId;
            switch (networkEvent)
            {
                case NetworkEvent.Data:
                    if (!_clientIdMap.TryGetValue(transportClientId, out clientId))
                        break;
                    OnServerDataReceived.Invoke(clientId, payload, 0);
                    break;
                case NetworkEvent.Connect:
                    clientId = _clientIdMap.Add(transportClientId);
                    OnServerConnected.Invoke(clientId);
                    break;
                case NetworkEvent.Disconnect:
                    if (!_clientIdMap.TryRemoveKey(transportClientId, out clientId))
                        break;
                    OnServerDisconnected.Invoke(clientId);
                    break;
                case NetworkEvent.TransportFailure:
                    break;
                case NetworkEvent.Nothing:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(networkEvent), networkEvent, null);
            }
        }

        /// <summary>
        ///     Is it supported
        /// </summary>
        public override bool Available() => Application.platform != RuntimePlatform.WebGLPlayer;

        /// <summary>
        ///     Client Connection
        /// </summary>
        /// <returns>Client Connection</returns>
        public override bool ClientConnected() => _clientConnected;

        /// <summary>
        ///     Start client
        /// </summary>
        public override void ClientConnect(string address)
        {
            TryRegister(false);
            if (address == "localhost")
                address = "127.0.0.1";
            Utp.ConnectionData.Address = address;
            Utp.Initialize();
            var succeeded = Utp.StartClient();
            if (succeeded)
                return;
            OnShutdown();
        }

        /// <summary>
        ///     SendRelayData
        /// </summary>
        /// <param name="payload">Payload</param>
        /// <param name="channelId">Passageway</param>
        public override void ClientSend(ArraySegment<byte> payload, int channelId = Channels.Reliable) => Utp.Send(_serverId, payload, NetworkDelivery.Reliable);

        /// <summary>
        ///     Disconnect local client
        /// </summary>
        public override void ClientDisconnect()
        {
            _clientConnected = false;
            Utp.DisconnectLocalClient();
            if (_serverActive)
                return;
            Shutdown();
        }

        /// <summary>
        ///     ServerUri
        /// </summary>
        /// <returns>ServerUri</returns>
        public override Uri ServerUri() => null;

        /// <summary>
        ///     Server valid
        /// </summary>
        /// <returns>Server valid</returns>
        public override bool ServerActive() => _serverActive;

        /// <summary>
        ///     Start the server
        /// </summary>
        public override void ServerStart()
        {
            TryRegister(true);
            _clientIdMap = new IntIndexMap<ulong>();
            _clientIdMap.Allocate();
            Utp.ConnectionData.Address = "0.0.0.0";
            Utp.Initialize();
            _serverActive = Utp.StartServer();
            if (_serverActive)
                return;
            OnShutdown();
        }

        /// <summary>
        ///     SendRelayData
        /// </summary>
        /// <param name="connectionId">ClientId</param>
        /// <param name="payload">Payload</param>
        /// <param name="channelId">Passageway</param>
        public override void ServerSend(int connectionId, ArraySegment<byte> payload, int channelId = Channels.Reliable)
        {
            if (!_clientIdMap.TryGetKey(connectionId, out var clientId))
                return;
            Utp.Send(clientId, payload, NetworkDelivery.Reliable);
        }

        /// <summary>
        ///     Disconnect remote client
        /// </summary>
        /// <param name="connectionId">ClientId</param>
        public override void ServerDisconnect(int connectionId)
        {
            if (!_clientIdMap.TryGetKey(connectionId, out var clientId))
                return;
            Utp.DisconnectRemoteClient(clientId);
        }

        /// <summary>
        ///     Server obtains client address
        /// </summary>
        /// <param name="connectionId">ClientId</param>
        /// <returns>The client address obtained by the server</returns>
        public override string ServerGetClientAddress(int connectionId) => null;

        /// <summary>
        ///     Stop server
        /// </summary>
        public override void ServerStop()
        {
            _serverActive = false;
            Shutdown();
        }

        /// <summary>
        ///     Get the maximum packet capacity
        /// </summary>
        /// <param name="channelId">Passageway</param>
        /// <returns>Maximum packet capacity obtained</returns>
        public override int GetMaxPacketSize(int channelId = Channels.Reliable) => 1024;

        /// <summary>
        ///     Cease
        /// </summary>
        public override void Shutdown()
        {
            Utp.Shutdown();
            Unregister();
        }

        /// <summary>
        ///     Cease
        /// </summary>
        private void OnShutdown()
        {
            NetworkManager.singleton.StopClient();
            NetworkManager.singleton.StopServer();
        }
    }
}