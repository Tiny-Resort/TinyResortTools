using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Discovery
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Network/NetworkDiscoveryHUD")]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-discovery")]
	[RequireComponent(typeof(NetworkDiscovery))]
	public class NetworkDiscoveryHUD : MonoBehaviour
	{
		private readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

		private Vector2 scrollViewPos = Vector2.zero;

		public NetworkDiscovery networkDiscovery;

		private void OnGUI()
		{
			if (!(NetworkManager.singleton == null))
			{
				if (!NetworkClient.isConnected && !NetworkServer.active && !NetworkClient.active)
				{
					DrawGUI();
				}
				if (NetworkServer.active || NetworkClient.active)
				{
					StopButtons();
				}
			}
		}

		private void DrawGUI()
		{
			GUILayout.BeginArea(new Rect(10f, 10f, 300f, 500f));
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Find Servers"))
			{
				discoveredServers.Clear();
				networkDiscovery.StartDiscovery();
			}
			if (GUILayout.Button("Start Host"))
			{
				discoveredServers.Clear();
				NetworkManager.singleton.StartHost();
				networkDiscovery.AdvertiseServer();
			}
			if (GUILayout.Button("Start Server"))
			{
				discoveredServers.Clear();
				NetworkManager.singleton.StartServer();
				networkDiscovery.AdvertiseServer();
			}
			GUILayout.EndHorizontal();
			GUILayout.Label(string.Format("Discovered Servers [{0}]:", discoveredServers.Count));
			scrollViewPos = GUILayout.BeginScrollView(scrollViewPos);
			foreach (ServerResponse value in discoveredServers.Values)
			{
				if (GUILayout.Button(value.EndPoint.Address.ToString()))
				{
					Connect(value);
				}
			}
			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		private void StopButtons()
		{
			GUILayout.BeginArea(new Rect(10f, 40f, 100f, 25f));
			if (NetworkServer.active && NetworkClient.isConnected)
			{
				if (GUILayout.Button("Stop Host"))
				{
					NetworkManager.singleton.StopHost();
					networkDiscovery.StopDiscovery();
				}
			}
			else if (NetworkClient.isConnected)
			{
				if (GUILayout.Button("Stop Client"))
				{
					NetworkManager.singleton.StopClient();
					networkDiscovery.StopDiscovery();
				}
			}
			else if (NetworkServer.active && GUILayout.Button("Stop Server"))
			{
				NetworkManager.singleton.StopServer();
				networkDiscovery.StopDiscovery();
			}
			GUILayout.EndArea();
		}

		private void Connect(ServerResponse info)
		{
			networkDiscovery.StopDiscovery();
			NetworkManager.singleton.StartClient(info.uri);
		}

		public void OnDiscoveredServer(ServerResponse info)
		{
			discoveredServers[info.serverId] = info;
		}
	}
}
