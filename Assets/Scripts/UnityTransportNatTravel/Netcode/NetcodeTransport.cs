using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using UnityEngine;
using NetworkEvent = Unity.Netcode.NetworkEvent;

// ReSharper disable RedundantExplicitArraySize
// ReSharper disable InconsistentNaming
// ReSharper disable PossibleNullReferenceException

namespace Netcode
{
    public sealed class NetcodeTransport : UnityTransport
    {
        public ulong ServiceId;
        public ConnectionAddressData ServiceData;
        public string LocalEndPoint;
        private FieldInfo _driverFieldInfo;
        private FieldInfo _stateFieldInfo;
        private NetworkDriver _driver => (NetworkDriver)_driverFieldInfo.GetValue(this);

        private void Start()
        {
            _driverFieldInfo = typeof(UnityTransport).GetField("m_Driver", BindingFlags.Instance | BindingFlags.NonPublic);
            _stateFieldInfo = typeof(UnityTransport).GetField("m_State", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(LocalEndPoint))
                return;
            GUILayout.BeginArea(new Rect(10, 120, 300, 9999));
            GUILayout.Label($"<b>Local</b>: {LocalEndPoint}");
            GUILayout.EndArea();
        }

        public event TransportEventDelegate OnUserTransportEvent;

        public override void Initialize(NetworkManager networkManager = null)
        {
            base.Initialize(networkManager);
            if (networkManager != null)
            {
                var connectionManager = (NetworkConnectionManager)typeof(NetworkManager).GetField("ConnectionManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(networkManager);
                var methodInfo = typeof(NetworkConnectionManager).GetMethod("HandleNetworkEvent", BindingFlags.Instance | BindingFlags.NonPublic);
                OnUserTransportEvent = (TransportEventDelegate)Delegate.CreateDelegate(typeof(TransportEventDelegate), connectionManager, methodInfo);
            }

            var eventInfo = typeof(NetworkTransport).GetEvent("OnTransportEvent", BindingFlags.Instance | BindingFlags.Public);
            var fieldInfo = typeof(NetworkTransport).GetField(eventInfo.Name, BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo.SetValue(this, null);
            OnTransportEvent += HandleNetworkEvent;
        }

        public override void Shutdown()
        {
            ServiceId = 0UL;
            LocalEndPoint = null;
            base.Shutdown();
            OnUserTransportEvent = null;
            OnTransportEvent -= HandleNetworkEvent;
        }

        private void HandleNetworkEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
        {
            if (clientId == ServiceId)
            {
                switch (eventType)
                {
                    case NetworkEvent.Data:
                        var address = Encoding.UTF8.GetString(payload.AsSpan(1, payload.Count - 1));
                        if (payload[0] == 0)
                        {
                            LocalEndPoint = address;
                        }
                        else if (ServerClientId == 0)
                        {
                            var split = address.Split(':');
                            var connection = _driver.Connect(NetworkEndPoint.Parse(split[0], ushort.Parse(split[1])));
                            _driver.Disconnect(connection);
                            _stateFieldInfo.SetValue(this, 1);
                        }

                        break;
                    case NetworkEvent.Connect:
                        if (ServerClientId != 0)
                        {
                            var serverEndPoint = ConnectionData.ServerEndPoint;
                            var serverAddress = serverEndPoint.Address;
                            var buffer = Encoding.UTF8.GetBytes(serverAddress);
                            Send(clientId, new ArraySegment<byte>(buffer), NetworkDelivery.Reliable);
                        }

                        break;
                    case NetworkEvent.Disconnect:
                        break;
                    case NetworkEvent.TransportFailure:
                        break;
                    case NetworkEvent.Nothing:
                        break;
                }

                return;
            }

            OnUserTransportEvent?.Invoke(eventType, clientId, payload, receiveTime);
        }

        private static ulong ParseClientId(NetworkConnection utpConnectionId) => Unsafe.As<NetworkConnection, ulong>(ref utpConnectionId);

        public override bool StartServer()
        {
            if (_driver.IsCreated)
                return false;
            ConnectionData.Address = "0.0.0.0";
            var succeeded = ServerBindAndListen(ConnectionData.ListenEndPoint);
            if (!succeeded && _driver.IsCreated)
                _driver.Dispose();
            return succeeded;
        }

        private bool ServerBindAndListen(NetworkEndPoint endPoint)
        {
            if (endPoint.Family == NetworkFamily.Invalid)
            {
                Debug.LogError($"Network listen address ({ConnectionData.Address}) is {nameof(NetworkFamily.Invalid)}!");
                return false;
            }

            typeof(UnityTransport).GetMethod("InitDriver", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, null);
            var result = _driver.Bind(endPoint);
            if (result != 0)
            {
                Debug.LogError("Server failed to bind. This is usually caused by another process being bound to the same port.");
                return false;
            }

            var serviceConnection = _driver.Connect(ServiceData.ServerEndPoint);
            result = _driver.Listen();
            if (result != 0)
            {
                Debug.LogError("Server failed to listen.");
                return false;
            }

            ServiceId = ParseClientId(serviceConnection);
            _stateFieldInfo.SetValue(this, 1);
            return true;
        }

        public override bool StartClient()
        {
            if (_driver.IsCreated)
                return false;
            var succeeded = ClientBindAndConnect();
            if (!succeeded && _driver.IsCreated)
                _driver.Dispose();
            return succeeded;
        }

        private bool ClientBindAndConnect()
        {
            var serverEndpoint = ConnectionData.ServerEndPoint;
            if (serverEndpoint.Family == NetworkFamily.Invalid)
            {
                Debug.LogError($"Target server network address ({ConnectionData.Address}) is {nameof(NetworkFamily.Invalid)}!");
                return false;
            }

            typeof(UnityTransport).GetMethod("InitDriver", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, null);
            var bindEndpoint = serverEndpoint.Family == NetworkFamily.Ipv6 ? NetworkEndPoint.AnyIpv6 : NetworkEndPoint.AnyIpv4;
            var result = _driver.Bind(bindEndpoint);
            if (result != 0)
            {
                Debug.LogError("Client failed to bind");
                return false;
            }

            var serviceConnection = _driver.Connect(ServiceData.ServerEndPoint);
            var serverConnection = _driver.Connect(serverEndpoint);
            var fieldInfo = typeof(UnityTransport).GetField("m_ServerClientId", BindingFlags.Instance | BindingFlags.NonPublic);
            ServiceId = ParseClientId(serviceConnection);
            fieldInfo.SetValue(this, ParseClientId(serverConnection));
            return true;
        }
    }
}