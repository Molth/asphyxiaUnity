//------------------------------------------------------------
// Erinn Framework
// Copyright © 2024 Molth Nevin. All rights reserved.
//------------------------------------------------------------

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Erinn.Native
{
    /// <summary>
    ///     Network ManagerHud
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkManager))]
    public sealed class NetworkManagerHud : MonoBehaviour
    {
        /// <summary>
        ///     xDeviation
        /// </summary>
        [SerializeField] private int _offsetX;

        /// <summary>
        ///     yDeviation
        /// </summary>
        [SerializeField] private int _offsetY;

        /// <summary>
        ///     Network Manager
        /// </summary>
        private NetworkManager _manager;

        /// <summary>
        ///     Port
        /// </summary>
        private string _port;

        /// <summary>
        ///     Transmission
        /// </summary>
        private UnityTransport _transport;

        /// <summary>
        ///     Call on load
        /// </summary>
        private void Awake()
        {
            _manager = GetComponent<NetworkManager>();
            _transport = (UnityTransport)_manager.NetworkConfig.NetworkTransport;
            _port = _transport.ConnectionData.Port.ToString();
        }

        /// <summary>
        ///     Displayed onGUI
        /// </summary>
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10 + _offsetX, 40 + _offsetY, 250, 400));
            if (!_manager.IsServer && !_manager.IsConnectedClient)
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
            if (_manager.IsClient)
            {
                GUILayout.Label($"Connecting {_transport.ConnectionData.Address}");
                if (GUILayout.Button("Cancel connection attempt"))
                    _manager.Shutdown();
            }
            else
            {
                if (GUILayout.Button("Host"))
                    _manager.StartHost();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Client"))
                    _manager.StartClient();
                _transport.ConnectionData.Address = GUILayout.TextField(_transport.ConnectionData.Address);
                if (ushort.TryParse(GUILayout.TextField(_port), out var port))
                {
                    var portString = port.ToString();
                    if (_port != portString)
                    {
                        _port = portString;
                        _transport.ConnectionData.Port = port;
                    }
                }

                GUILayout.EndHorizontal();
                if (GUILayout.Button("Server only"))
                    _manager.StartServer();
            }
        }

        /// <summary>
        ///     Status bar
        /// </summary>
        private void StatusLabels()
        {
            if (_manager.IsServer && _manager.IsConnectedClient)
            {
                GUILayout.Label("<b>Host</b>");
                if (GUILayout.Button("Stop host"))
                    _manager.Shutdown();
            }
            else if (_manager.IsConnectedClient)
            {
                GUILayout.Label($"<b>Client</b>: {_transport.ConnectionData.Address}:{_transport.ConnectionData.Port}");
                if (GUILayout.Button("Stop client"))
                    _manager.Shutdown();
            }
            else if (_manager.IsServer)
            {
                GUILayout.Label("<b>Server</b>");
                if (GUILayout.Button("Stop server"))
                    _manager.Shutdown();
            }
        }
    }
}