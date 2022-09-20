using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirror
{
	[AddComponentMenu("Network/NetworkRoomManager")]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-room-manager")]
	public class NetworkRoomManager : NetworkManager
	{
		public struct PendingPlayer
		{
			public NetworkConnection conn;

			public GameObject roomPlayer;
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static Func<KeyValuePair<int, NetworkConnectionToClient>, bool> _003C_003E9__16_0;

			internal bool _003CCheckReadyToBegin_003Eb__16_0(KeyValuePair<int, NetworkConnectionToClient> conn)
			{
				if (conn.Value != null)
				{
					return conn.Value.identity.gameObject.GetComponent<NetworkRoomPlayer>().readyToBegin;
				}
				return false;
			}
		}

		[Header("Room Settings")]
		[FormerlySerializedAs("m_ShowRoomGUI")]
		[SerializeField]
		[Tooltip("This flag controls whether the default UI is shown for the room")]
		public bool showRoomGUI = true;

		[FormerlySerializedAs("m_MinPlayers")]
		[SerializeField]
		[Tooltip("Minimum number of players to auto-start the game")]
		public int minPlayers = 1;

		[FormerlySerializedAs("m_RoomPlayerPrefab")]
		[SerializeField]
		[Tooltip("Prefab to use for the Room Player")]
		public NetworkRoomPlayer roomPlayerPrefab;

		[Scene]
		public string RoomScene;

		[Scene]
		public string GameplayScene;

		[FormerlySerializedAs("m_PendingPlayers")]
		public List<PendingPlayer> pendingPlayers = new List<PendingPlayer>();

		[Header("Diagnostics")]
		[Tooltip("Diagnostic flag indicating all players are ready to play")]
		[FormerlySerializedAs("allPlayersReady")]
		[SerializeField]
		private bool _allPlayersReady;

		[Tooltip("List of Room Player objects")]
		public List<NetworkRoomPlayer> roomSlots = new List<NetworkRoomPlayer>();

		public int clientIndex;

		public bool allPlayersReady
		{
			get
			{
				return _allPlayersReady;
			}
			set
			{
				bool num = _allPlayersReady;
				bool flag = value;
				if (num != flag)
				{
					_allPlayersReady = value;
					if (flag)
					{
						OnRoomServerPlayersReady();
					}
					else
					{
						OnRoomServerPlayersNotReady();
					}
				}
			}
		}

		public override void OnValidate()
		{
			maxConnections = Mathf.Max(maxConnections, 0);
			minPlayers = Mathf.Min(minPlayers, maxConnections);
			minPlayers = Mathf.Max(minPlayers, 0);
			if (roomPlayerPrefab != null && roomPlayerPrefab.GetComponent<NetworkIdentity>() == null)
			{
				roomPlayerPrefab = null;
				Debug.LogError("RoomPlayer prefab must have a NetworkIdentity component.");
			}
			base.OnValidate();
		}

		public void ReadyStatusChanged()
		{
			int num = 0;
			int num2 = 0;
			foreach (NetworkRoomPlayer roomSlot in roomSlots)
			{
				if (roomSlot != null)
				{
					num++;
					if (roomSlot.readyToBegin)
					{
						num2++;
					}
				}
			}
			if (num == num2)
			{
				CheckReadyToBegin();
			}
			else
			{
				allPlayersReady = false;
			}
		}

		public override void OnServerReady(NetworkConnection conn)
		{
			Debug.Log("NetworkRoomManager OnServerReady");
			base.OnServerReady(conn);
			if (conn != null && conn.identity != null)
			{
				GameObject gameObject = conn.identity.gameObject;
				if (gameObject != null && gameObject.GetComponent<NetworkRoomPlayer>() != null)
				{
					SceneLoadedForPlayer(conn, gameObject);
				}
			}
		}

		private void SceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer)
		{
			if (NetworkManager.IsSceneActive(RoomScene))
			{
				PendingPlayer item = default(PendingPlayer);
				item.conn = conn;
				item.roomPlayer = roomPlayer;
				pendingPlayers.Add(item);
				return;
			}
			GameObject gameObject = OnRoomServerCreateGamePlayer(conn, roomPlayer);
			if (gameObject == null)
			{
				Transform startPosition = GetStartPosition();
				gameObject = ((startPosition != null) ? UnityEngine.Object.Instantiate(playerPrefab, startPosition.position, startPosition.rotation) : UnityEngine.Object.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity));
			}
			if (OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gameObject))
			{
				NetworkServer.ReplacePlayerForConnection(conn, gameObject, true);
			}
		}

		public void CheckReadyToBegin()
		{
			if (NetworkManager.IsSceneActive(RoomScene))
			{
				int num = NetworkServer.connections.Count(_003C_003Ec._003C_003E9__16_0 ?? (_003C_003Ec._003C_003E9__16_0 = _003C_003Ec._003C_003E9._003CCheckReadyToBegin_003Eb__16_0));
				if (minPlayers <= 0 || num >= minPlayers)
				{
					pendingPlayers.Clear();
					allPlayersReady = true;
				}
				else
				{
					allPlayersReady = false;
				}
			}
		}

		internal void CallOnClientEnterRoom()
		{
			OnRoomClientEnter();
			foreach (NetworkRoomPlayer roomSlot in roomSlots)
			{
				if (roomSlot != null)
				{
					roomSlot.OnClientEnterRoom();
				}
			}
		}

		internal void CallOnClientExitRoom()
		{
			OnRoomClientExit();
			foreach (NetworkRoomPlayer roomSlot in roomSlots)
			{
				if (roomSlot != null)
				{
					roomSlot.OnClientExitRoom();
				}
			}
		}

		public override void OnServerConnect(NetworkConnection conn)
		{
			if (base.numPlayers >= maxConnections)
			{
				conn.Disconnect();
				return;
			}
			if (!NetworkManager.IsSceneActive(RoomScene))
			{
				conn.Disconnect();
				return;
			}
			base.OnServerConnect(conn);
			OnRoomServerConnect(conn);
		}

		public override void OnServerDisconnect(NetworkConnection conn)
		{
			if (conn.identity != null)
			{
				NetworkRoomPlayer component = conn.identity.GetComponent<NetworkRoomPlayer>();
				if (component != null)
				{
					roomSlots.Remove(component);
				}
				foreach (NetworkIdentity clientOwnedObject in conn.clientOwnedObjects)
				{
					component = clientOwnedObject.GetComponent<NetworkRoomPlayer>();
					if (component != null)
					{
						roomSlots.Remove(component);
					}
				}
			}
			allPlayersReady = false;
			foreach (NetworkRoomPlayer roomSlot in roomSlots)
			{
				if (roomSlot != null)
				{
					roomSlot.GetComponent<NetworkRoomPlayer>().NetworkreadyToBegin = false;
				}
			}
			if (NetworkManager.IsSceneActive(RoomScene))
			{
				RecalculateRoomPlayerIndices();
			}
			OnRoomServerDisconnect(conn);
			base.OnServerDisconnect(conn);
		}

		public override void OnServerAddPlayer(NetworkConnection conn)
		{
			clientIndex++;
			if (NetworkManager.IsSceneActive(RoomScene))
			{
				if (roomSlots.Count != maxConnections)
				{
					allPlayersReady = false;
					GameObject gameObject = OnRoomServerCreateRoomPlayer(conn);
					if (gameObject == null)
					{
						gameObject = UnityEngine.Object.Instantiate(roomPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
					}
					NetworkServer.AddPlayerForConnection(conn, gameObject);
				}
			}
			else
			{
				OnRoomServerAddPlayer(conn);
			}
		}

		[Server]
		public void RecalculateRoomPlayerIndices()
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void Mirror.NetworkRoomManager::RecalculateRoomPlayerIndices()' called when server was not active");
			}
			else if (roomSlots.Count > 0)
			{
				for (int i = 0; i < roomSlots.Count; i++)
				{
					roomSlots[i].Networkindex = i;
				}
			}
		}

		public override void ServerChangeScene(string newSceneName)
		{
			if (newSceneName == RoomScene)
			{
				foreach (NetworkRoomPlayer roomSlot in roomSlots)
				{
					if (!(roomSlot == null))
					{
						NetworkIdentity component = roomSlot.GetComponent<NetworkIdentity>();
						if (NetworkServer.active)
						{
							roomSlot.GetComponent<NetworkRoomPlayer>().NetworkreadyToBegin = false;
							NetworkServer.ReplacePlayerForConnection(component.connectionToClient, roomSlot.gameObject);
						}
					}
				}
				allPlayersReady = false;
			}
			base.ServerChangeScene(newSceneName);
		}

		public override void OnServerSceneChanged(string sceneName)
		{
			if (sceneName != RoomScene)
			{
				foreach (PendingPlayer pendingPlayer in pendingPlayers)
				{
					SceneLoadedForPlayer(pendingPlayer.conn, pendingPlayer.roomPlayer);
				}
				pendingPlayers.Clear();
			}
			OnRoomServerSceneChanged(sceneName);
		}

		public override void OnStartServer()
		{
			if (string.IsNullOrEmpty(RoomScene))
			{
				Debug.LogError("NetworkRoomManager RoomScene is empty. Set the RoomScene in the inspector for the NetworkRoomManager");
			}
			else if (string.IsNullOrEmpty(GameplayScene))
			{
				Debug.LogError("NetworkRoomManager PlayScene is empty. Set the PlayScene in the inspector for the NetworkRoomManager");
			}
			else
			{
				OnRoomStartServer();
			}
		}

		public override void OnStartHost()
		{
			OnRoomStartHost();
		}

		public override void OnStopServer()
		{
			roomSlots.Clear();
			OnRoomStopServer();
		}

		public override void OnStopHost()
		{
			OnRoomStopHost();
		}

		public override void OnStartClient()
		{
			if (roomPlayerPrefab == null || roomPlayerPrefab.gameObject == null)
			{
				Debug.LogError("NetworkRoomManager no RoomPlayer prefab is registered. Please add a RoomPlayer prefab.");
			}
			else
			{
				NetworkClient.RegisterPrefab(roomPlayerPrefab.gameObject);
			}
			if (playerPrefab == null)
			{
				Debug.LogError("NetworkRoomManager no GamePlayer prefab is registered. Please add a GamePlayer prefab.");
			}
			OnRoomStartClient();
		}

		public override void OnClientConnect(NetworkConnection conn)
		{
			OnRoomClientConnect(conn);
			base.OnClientConnect(conn);
		}

		public override void OnClientDisconnect(NetworkConnection conn)
		{
			OnRoomClientDisconnect(conn);
			base.OnClientDisconnect(conn);
		}

		public override void OnStopClient()
		{
			OnRoomStopClient();
			CallOnClientExitRoom();
			roomSlots.Clear();
		}

		public override void OnClientSceneChanged(NetworkConnection conn)
		{
			if (NetworkManager.IsSceneActive(RoomScene))
			{
				if (NetworkClient.isConnected)
				{
					CallOnClientEnterRoom();
				}
			}
			else
			{
				CallOnClientExitRoom();
			}
			base.OnClientSceneChanged(conn);
			OnRoomClientSceneChanged(conn);
		}

		public virtual void OnRoomStartHost()
		{
		}

		public virtual void OnRoomStopHost()
		{
		}

		public virtual void OnRoomStartServer()
		{
		}

		public virtual void OnRoomStopServer()
		{
		}

		public virtual void OnRoomServerConnect(NetworkConnection conn)
		{
		}

		public virtual void OnRoomServerDisconnect(NetworkConnection conn)
		{
		}

		public virtual void OnRoomServerSceneChanged(string sceneName)
		{
		}

		public virtual GameObject OnRoomServerCreateRoomPlayer(NetworkConnection conn)
		{
			return null;
		}

		public virtual GameObject OnRoomServerCreateGamePlayer(NetworkConnection conn, GameObject roomPlayer)
		{
			return null;
		}

		public virtual void OnRoomServerAddPlayer(NetworkConnection conn)
		{
			base.OnServerAddPlayer(conn);
		}

		public virtual bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
		{
			return true;
		}

		public virtual void OnRoomServerPlayersReady()
		{
			ServerChangeScene(GameplayScene);
		}

		public virtual void OnRoomServerPlayersNotReady()
		{
		}

		public virtual void OnRoomClientEnter()
		{
		}

		public virtual void OnRoomClientExit()
		{
		}

		public virtual void OnRoomClientConnect(NetworkConnection conn)
		{
		}

		public virtual void OnRoomClientDisconnect(NetworkConnection conn)
		{
		}

		public virtual void OnRoomStartClient()
		{
		}

		public virtual void OnRoomStopClient()
		{
		}

		public virtual void OnRoomClientSceneChanged(NetworkConnection conn)
		{
		}

		public virtual void OnRoomClientAddPlayerFailed()
		{
		}

		public virtual void OnGUI()
		{
			if (!showRoomGUI)
			{
				return;
			}
			if (NetworkServer.active && NetworkManager.IsSceneActive(GameplayScene))
			{
				GUILayout.BeginArea(new Rect((float)Screen.width - 150f, 10f, 140f, 30f));
				if (GUILayout.Button("Return to Room"))
				{
					ServerChangeScene(RoomScene);
				}
				GUILayout.EndArea();
			}
			if (NetworkManager.IsSceneActive(RoomScene))
			{
				GUI.Box(new Rect(10f, 180f, 520f, 150f), "PLAYERS");
			}
		}
	}
}
