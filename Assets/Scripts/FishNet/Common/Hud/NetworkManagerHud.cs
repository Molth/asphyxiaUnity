//------------------------------------------------------------
// Erinn Framework
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

namespace FishNet
{
    /// <summary>
    ///     Network ManagerHud
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkManager))]
    public sealed class NetworkManagerHud : MonoBehaviour
    {
        /// <summary>
        ///     x offset
        /// </summary>
        [SerializeField] private int _offsetX;

        /// <summary>
        ///     y offset
        /// </summary>
        [SerializeField] private int _offsetY;

        /// <summary>
        ///     Address
        /// </summary>
        public string Address = "127.0.0.1";

        /// <summary>
        ///     Port
        /// </summary>
        public ushort Port = 7777;

        /// <summary>
        ///     Network Manager
        /// </summary>
        private NetworkManager _manager;

        /// <summary>
        ///     Transport
        /// </summary>
        private Transport _transport;

        /// <summary>
        ///     Port
        /// </summary>
        private string _port;

        /// <summary>
        ///     Call on load
        /// </summary>
        private void Awake()
        {
            _manager = GetComponent<NetworkManager>();
            _port = Port.ToString();
            _transport = GetComponent<Transport>();
        }

        /// <summary>
        ///     Displayed onGUI
        /// </summary>
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10 + _offsetX, 40 + _offsetY, 250, 400));
            if (!_manager.IsServerStarted && !_manager.IsClientStarted)
                StartButtons();
            else
                StatusLabels();
            GUILayout.EndArea();
        }

        /// <summary>
        ///     Start button
        /// </summary>
        private void StartButtons()
        {
            if (_manager.IsClientOnlyStarted)
            {
                GUILayout.Label($"Connecting {Address}");
                if (GUILayout.Button("Cancel connection attempt"))
                    _manager.ClientManager.StopConnection();
            }
            else
            {
                if (GUILayout.Button("Host"))
                {
                    _transport.SetPort(Port);
                    _transport.SetServerBindAddress("0.0.0.0", IPAddressType.IPv4);
                    _transport.SetClientAddress(Address);
                    _manager.ServerManager.StartConnection();
                    _manager.ClientManager.StartConnection();
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Client"))
                {
                    _transport.SetPort(Port);
                    _transport.SetClientAddress(Address);
                    _manager.ClientManager.StartConnection();
                }

                Address = GUILayout.TextField(Address);
                if (ushort.TryParse(GUILayout.TextField(_port), out var port))
                {
                    var portString = port.ToString();
                    if (_port != portString)
                    {
                        _port = portString;
                        Port = port;
                    }
                }

                GUILayout.EndHorizontal();
                if (GUILayout.Button("Server only"))
                {
                    _transport.SetPort(Port);
                    _transport.SetServerBindAddress("0.0.0.0", IPAddressType.IPv4);
                    _manager.ServerManager.StartConnection();
                }
            }
        }

        /// <summary>
        ///     Status bar
        /// </summary>
        private void StatusLabels()
        {
            if (_manager.IsServerStarted && _manager.IsClientStarted)
            {
                GUILayout.Label("<b>Host</b>");
                if (GUILayout.Button("Stop host"))
                {
                    _manager.ClientManager.StopConnection();
                    _manager.ServerManager.StopConnection(true);
                }
            }
            else if (_manager.IsClientStarted)
            {
                GUILayout.Label($"<b>Client</b>: {Address}:{Port}");
                if (GUILayout.Button("Stop client"))
                    _manager.ClientManager.StopConnection();
            }
            else if (_manager.IsServerStarted)
            {
                GUILayout.Label("<b>Server</b>");
                if (GUILayout.Button("Stop server"))
                    _manager.ServerManager.StopConnection(true);
            }
        }
    }
}