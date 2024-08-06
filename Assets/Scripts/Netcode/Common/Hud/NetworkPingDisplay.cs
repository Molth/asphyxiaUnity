using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Unity.Netcode
{
    /// <summary>
    ///     Component that will display the clients ping in milliseconds
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NetworkPingDisplay : MonoBehaviour
    {
        public int padding = 2;
        public int width = 150;
        public int height = 25;

        private void OnGUI()
        {
            if (NetworkManager.Singleton.IsServer || !NetworkManager.Singleton.IsClient)
                return;
            GUI.color = Color.white;
            var rect = new Rect(Screen.width - width - padding, Screen.height - height - padding, width, height);
            GUILayout.BeginArea(rect);
            var style = GUI.skin.GetStyle("Label");
            style.alignment = TextAnchor.MiddleRight;
            GUILayout.BeginHorizontal(style);
            GUILayout.Label($"RTT: {((UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport).GetCurrentRtt(1)}ms");
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.color = Color.white;
        }
    }
}