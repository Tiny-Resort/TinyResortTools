using System;
using UnityEngine;

namespace Mirror
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Network/NetworkManagerHUD")]
	[RequireComponent(typeof(NetworkManager))]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-manager-hud")]
	public class NetworkManagerHUD : MonoBehaviour
	{
		private NetworkManager manager;

		[Obsolete("showGUI will be removed unless someone has a valid use case. Simply use or don't use the HUD component.")]
		public bool showGUI = true;

		public int offsetX;

		public int offsetY;

		private void Awake()
		{
			manager = GetComponent<NetworkManager>();
		}

		private void OnGUI()
		{
			if (!showGUI)
			{
				return;
			}
			GUILayout.BeginArea(new Rect(10 + offsetX, 40 + offsetY, 215f, 9999f));
			if (!NetworkClient.isConnected && !NetworkServer.active)
			{
				StartButtons();
			}
			else
			{
				StatusLabels();
			}
			if (NetworkClient.isConnected && !NetworkClient.ready && GUILayout.Button("Client Ready"))
			{
				NetworkClient.Ready();
				if (NetworkClient.localPlayer == null)
				{
					NetworkClient.AddPlayer();
				}
			}
			StopButtons();
			GUILayout.EndArea();
		}

		private void StartButtons()
		{
			if (!NetworkClient.active)
			{
				if (Application.platform != RuntimePlatform.WebGLPlayer && GUILayout.Button("Host (Server + Client)"))
				{
					manager.StartHost();
				}
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Client"))
				{
					manager.StartClient();
				}
				manager.networkAddress = GUILayout.TextField(manager.networkAddress);
				GUILayout.EndHorizontal();
				if (Application.platform == RuntimePlatform.WebGLPlayer)
				{
					GUILayout.Box("(  WebGL cannot be server  )");
				}
				else if (GUILayout.Button("Server Only"))
				{
					manager.StartServer();
				}
			}
			else
			{
				GUILayout.Label("Connecting to " + manager.networkAddress + "..");
				if (GUILayout.Button("Cancel Connection Attempt"))
				{
					manager.StopClient();
				}
			}
		}

		private void StatusLabels()
		{
			if (NetworkServer.active && NetworkClient.active)
			{
				GUILayout.Label(string.Format("<b>Host</b>: running via {0}", Transport.activeTransport));
			}
			else if (NetworkServer.active)
			{
				GUILayout.Label(string.Format("<b>Server</b>: running via {0}", Transport.activeTransport));
			}
			else if (NetworkClient.isConnected)
			{
				GUILayout.Label(string.Format("<b>Client</b>: connected to {0} via {1}", manager.networkAddress, Transport.activeTransport));
			}
		}

		private void StopButtons()
		{
			if (NetworkServer.active && NetworkClient.isConnected)
			{
				if (GUILayout.Button("Stop Host"))
				{
					manager.StopHost();
				}
			}
			else if (NetworkClient.isConnected)
			{
				if (GUILayout.Button("Stop Client"))
				{
					manager.StopClient();
				}
			}
			else if (NetworkServer.active && GUILayout.Button("Stop Server"))
			{
				manager.StopServer();
			}
		}
	}
}
