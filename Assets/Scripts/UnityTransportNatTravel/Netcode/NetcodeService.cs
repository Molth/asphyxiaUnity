using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Erinn;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using NetworkEvent = Unity.Netcode.NetworkEvent;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable InconsistentNaming

namespace Netcode
{
    public sealed class NetcodeService : UnityTransport
    {
        private Map<ulong, string> _endPoints;
        private FieldInfo _driverFieldInfo;
        private byte[] _bytes;
        private NetworkDriver _driver => (NetworkDriver)_driverFieldInfo.GetValue(this);

        private void Start()
        {
            _endPoints = new Map<ulong, string>();
            _bytes = new byte[1024];
            _driverFieldInfo = typeof(UnityTransport).GetField("m_Driver", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public override void Initialize(NetworkManager networkManager = null)
        {
            base.Initialize(networkManager);
            var eventInfo = typeof(NetworkTransport).GetEvent("OnTransportEvent", BindingFlags.Instance | BindingFlags.Public);
            var fieldInfo = typeof(NetworkTransport).GetField(eventInfo.Name, BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo.SetValue(this, null);
            OnTransportEvent += HandleNetworkEvent;
        }

        public override void Shutdown()
        {
            base.Shutdown();
            OnTransportEvent -= HandleNetworkEvent;
        }

        public override bool StartServer()
        {
            ConnectionData.Address = "0.0.0.0";
            return base.StartServer();
        }

        private void HandleNetworkEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
        {
            switch (eventType)
            {
                case NetworkEvent.Data:
                    var toAddress = Encoding.UTF8.GetString(payload);
                    if (_endPoints.TryGetKey(toAddress, out var toId) && _endPoints.TryGetValue(clientId, out var fromAddress))
                    {
                        _bytes[0] = 1;
                        var bytes = Encoding.UTF8.GetBytes(fromAddress, _bytes.AsSpan(1));
                        Send(toId, new ArraySegment<byte>(_bytes, 0, 1 + bytes), NetworkDelivery.Reliable);
                    }

                    break;
                case NetworkEvent.Connect:
                    var endPoint = _driver.RemoteEndPoint(Unsafe.As<ulong, NetworkConnection>(ref clientId));
                    _endPoints[clientId] = endPoint.Address;
                    _bytes[0] = 0;
                    var count = Encoding.UTF8.GetBytes(endPoint.Address, _bytes.AsSpan(1));
                    Send(clientId, new ArraySegment<byte>(_bytes, 0, 1 + count), NetworkDelivery.Reliable);
                    break;
                case NetworkEvent.Disconnect:
                    _endPoints.Remove(clientId);
                    break;
                case NetworkEvent.TransportFailure:
                    break;
                case NetworkEvent.Nothing:
                    break;
            }
        }
    }
}