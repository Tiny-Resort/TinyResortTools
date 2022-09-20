using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using kcp2k;

namespace Mirror
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Network/NetworkManager")]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-manager")]
	public class NetworkManager : MonoBehaviour
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static Func<KeyValuePair<int, NetworkConnectionToClient>, bool> _003C_003E9__24_0;

			public static Func<GameObject, bool> _003C_003E9__55_0;

			public static Func<Transform, int> _003C_003E9__72_0;

			public static Predicate<Transform> _003C_003E9__74_0;

			internal bool _003Cget_numPlayers_003Eb__24_0(KeyValuePair<int, NetworkConnectionToClient> kv)
			{
				return kv.Value.identity != null;
			}

			internal bool _003CRegisterClientMessages_003Eb__55_0(GameObject t)
			{
				return t != null;
			}

			internal int _003CRegisterStartPosition_003Eb__72_0(Transform transform)
			{
				return transform.GetSiblingIndex();
			}

			internal bool _003CGetStartPosition_003Eb__74_0(Transform t)
			{
				return t == null;
			}
		}

		[Header("Configuration")]
		[FormerlySerializedAs("m_DontDestroyOnLoad")]
		[Tooltip("Should the Network Manager object be persisted through scene changes?")]
		public bool dontDestroyOnLoad = true;

		[Obsolete("This was added temporarily and will be removed in a future release.")]
		[Tooltip("Should the Network Manager object be persisted through scene change to the offline scene?")]
		public bool PersistNetworkManagerToOfflineScene;

		[FormerlySerializedAs("m_RunInBackground")]
		[Tooltip("Multiplayer games should always run in the background so the network doesn't time out.")]
		public bool runInBackground = true;

		[Tooltip("Should the server auto-start when 'Server Build' is checked in build settings")]
		[FormerlySerializedAs("startOnHeadless")]
		public bool autoStartServerBuild = true;

		[Tooltip("Server Update frequency, per second. Use around 60Hz for fast paced games like Counter-Strike to minimize latency. Use around 30Hz for games like WoW to minimize computations. Use around 1-10Hz for slow paced games like EVE.")]
		public int serverTickRate = 30;

		[Header("Scene Management")]
		[Scene]
		[FormerlySerializedAs("m_OfflineScene")]
		[Tooltip("Scene that Mirror will switch to when the client or server is stopped")]
		public string offlineScene = "";

		[Scene]
		[FormerlySerializedAs("m_OnlineScene")]
		[Tooltip("Scene that Mirror will switch to when the server is started. Clients will recieve a Scene Message to load the server's current scene when they connect.")]
		public string onlineScene = "";

		[Header("Network Info")]
		[Tooltip("Transport component attached to this object that server and client will use to connect")]
		[SerializeField]
		protected Transport transport;

		[FormerlySerializedAs("m_NetworkAddress")]
		[Tooltip("Network Address where the client should connect to the server. Server does not use this for anything.")]
		public string networkAddress = "localhost";

		[FormerlySerializedAs("m_MaxConnections")]
		[Tooltip("Maximum number of concurrent connections.")]
		public int maxConnections = 100;

		[Obsolete("Transport is responsible for timeouts.")]
		public bool disconnectInactiveConnections;

		[Obsolete("Transport is responsible for timeouts. Configure the Transport's timeout setting instead.")]
		public float disconnectInactiveTimeout = 60f;

		[Header("Authentication")]
		[Tooltip("Authentication component attached to this object")]
		public NetworkAuthenticator authenticator;

		[Header("Player Object")]
		[FormerlySerializedAs("m_PlayerPrefab")]
		[Tooltip("Prefab of the player object. Prefab must have a Network Identity component. May be an empty game object or a full avatar.")]
		public GameObject playerPrefab;

		[FormerlySerializedAs("m_AutoCreatePlayer")]
		[Tooltip("Should Mirror automatically spawn the player after scene change?")]
		public bool autoCreatePlayer = true;

		[FormerlySerializedAs("m_PlayerSpawnMethod")]
		[Tooltip("Round Robin or Random order of Start Position selection")]
		public PlayerSpawnMethod playerSpawnMethod;

		[FormerlySerializedAs("m_SpawnPrefabs")]
		[HideInInspector]
		public List<GameObject> spawnPrefabs = new List<GameObject>();

		public static List<Transform> startPositions = new List<Transform>();

		public static int startPositionIndex;

		private static NetworkConnection clientReadyConnection;

		[NonSerialized]
		public bool clientLoadedScene;

		private bool finishStartHostPending;

		public static AsyncOperation loadingSceneAsync;

		private SceneOperation clientSceneOperation;

		public static NetworkManager singleton { get; private set; }

		public int numPlayers
		{
			get
			{
				return NetworkServer.connections.Count(_003C_003Ec._003C_003E9__24_0 ?? (_003C_003Ec._003C_003E9__24_0 = _003C_003Ec._003C_003E9._003Cget_numPlayers_003Eb__24_0));
			}
		}

		public bool isNetworkActive
		{
			get
			{
				if (!NetworkServer.active)
				{
					return NetworkClient.active;
				}
				return true;
			}
		}

		public NetworkManagerMode mode { get; private set; }

		public static string networkSceneName { get; protected set; } = "";


		public virtual void OnValidate()
		{
			if (transport == null)
			{
				transport = GetComponent<Transport>();
				if (transport == null)
				{
					transport = base.gameObject.AddComponent<KcpTransport>();
					Debug.Log("NetworkManager: added default Transport because there was none yet.");
				}
			}
			maxConnections = Mathf.Max(maxConnections, 0);
			if (playerPrefab != null && playerPrefab.GetComponent<NetworkIdentity>() == null)
			{
				Debug.LogError("NetworkManager - Player Prefab must have a NetworkIdentity.");
				playerPrefab = null;
			}
			if (playerPrefab != null && spawnPrefabs.Contains(playerPrefab))
			{
				Debug.LogWarning("NetworkManager - Player Prefab should not be added to Registered Spawnable Prefabs list...removed it.");
				spawnPrefabs.Remove(playerPrefab);
			}
		}

		public virtual void Awake()
		{
			if (InitializeSingleton())
			{
				networkSceneName = offlineScene;
				SceneManager.sceneLoaded += OnSceneLoaded;
			}
		}

		public virtual void Start()
		{
		}

		public virtual void LateUpdate()
		{
			UpdateScene();
		}

		private bool IsServerOnlineSceneChangeNeeded()
		{
			if (!string.IsNullOrEmpty(onlineScene) && !IsSceneActive(onlineScene))
			{
				return onlineScene != offlineScene;
			}
			return false;
		}

		public static bool IsSceneActive(string scene)
		{
			Scene activeScene = SceneManager.GetActiveScene();
			if (!(activeScene.path == scene))
			{
				return activeScene.name == scene;
			}
			return true;
		}

		private void SetupServer()
		{
			InitializeSingleton();
			if (runInBackground)
			{
				Application.runInBackground = true;
			}
			if (authenticator != null)
			{
				authenticator.OnStartServer();
				authenticator.OnServerAuthenticated.AddListener(OnServerAuthenticated);
			}
			ConfigureHeadlessFrameRate();
			NetworkServer.disconnectInactiveTimeout = disconnectInactiveTimeout;
			NetworkServer.disconnectInactiveConnections = disconnectInactiveConnections;
			NetworkServer.Listen(maxConnections);
			OnStartServer();
			RegisterServerMessages();
		}

		public void StartServer()
		{
			if (NetworkServer.active)
			{
				Debug.LogWarning("Server already started.");
				return;
			}
			mode = NetworkManagerMode.ServerOnly;
			SetupServer();
			if (IsServerOnlineSceneChangeNeeded())
			{
				ServerChangeScene(onlineScene);
			}
			else
			{
				NetworkServer.SpawnObjects();
			}
		}

		public void StartClient()
		{
			if (NetworkClient.active)
			{
				Debug.LogWarning("Client already started.");
				return;
			}
			mode = NetworkManagerMode.ClientOnly;
			InitializeSingleton();
			if (runInBackground)
			{
				Application.runInBackground = true;
			}
			if (authenticator != null)
			{
				authenticator.OnStartClient();
				authenticator.OnClientAuthenticated.AddListener(OnClientAuthenticated);
			}
			ConfigureHeadlessFrameRate();
			RegisterClientMessages();
			if (string.IsNullOrEmpty(networkAddress))
			{
				Debug.LogError("Must set the Network Address field in the manager");
				return;
			}
			NetworkClient.Connect(networkAddress);
			OnStartClient();
		}

		public void StartClient(Uri uri)
		{
			if (NetworkClient.active)
			{
				Debug.LogWarning("Client already started.");
				return;
			}
			mode = NetworkManagerMode.ClientOnly;
			InitializeSingleton();
			if (runInBackground)
			{
				Application.runInBackground = true;
			}
			if (authenticator != null)
			{
				authenticator.OnStartClient();
				authenticator.OnClientAuthenticated.AddListener(OnClientAuthenticated);
			}
			RegisterClientMessages();
			networkAddress = uri.Host;
			NetworkClient.Connect(uri);
			OnStartClient();
		}

		public void StartHost()
		{
			if (NetworkServer.active || NetworkClient.active)
			{
				Debug.LogWarning("Server or Client already started.");
				return;
			}
			mode = NetworkManagerMode.Host;
			SetupServer();
			OnStartHost();
			if (IsServerOnlineSceneChangeNeeded())
			{
				finishStartHostPending = true;
				ServerChangeScene(onlineScene);
			}
			else
			{
				FinishStartHost();
			}
		}

		private void FinishStartHost()
		{
			NetworkClient.ConnectHost();
			NetworkServer.SpawnObjects();
			StartHostClient();
		}

		private void StartHostClient()
		{
			if (authenticator != null)
			{
				authenticator.OnStartClient();
				authenticator.OnClientAuthenticated.AddListener(OnClientAuthenticated);
			}
			networkAddress = "localhost";
			NetworkServer.ActivateHostScene();
			RegisterClientMessages();
			NetworkClient.ConnectLocalServer();
			OnStartClient();
		}

		public void StopHost()
		{
			OnStopHost();
			NetworkServer.OnTransportDisconnected(0);
			StopClient();
			StopServer();
		}

		public void StopServer()
		{
			if (NetworkServer.active)
			{
				if (authenticator != null)
				{
					authenticator.OnServerAuthenticated.RemoveListener(OnServerAuthenticated);
					authenticator.OnStopServer();
				}
				if (base.gameObject != null && !PersistNetworkManagerToOfflineScene && base.gameObject.scene.name == "DontDestroyOnLoad" && !string.IsNullOrEmpty(offlineScene) && SceneManager.GetActiveScene().path != offlineScene)
				{
					SceneManager.MoveGameObjectToScene(base.gameObject, SceneManager.GetActiveScene());
				}
				OnStopServer();
				NetworkServer.Shutdown();
				mode = NetworkManagerMode.Offline;
				if (!string.IsNullOrEmpty(offlineScene))
				{
					ServerChangeScene(offlineScene);
				}
				startPositionIndex = 0;
				networkSceneName = "";
			}
		}

		public void StopClient()
		{
			if (authenticator != null)
			{
				authenticator.OnClientAuthenticated.RemoveListener(OnClientAuthenticated);
				authenticator.OnStopClient();
			}
			if (base.gameObject != null && !PersistNetworkManagerToOfflineScene && base.gameObject.scene.name == "DontDestroyOnLoad" && !string.IsNullOrEmpty(offlineScene) && SceneManager.GetActiveScene().path != offlineScene)
			{
				SceneManager.MoveGameObjectToScene(base.gameObject, SceneManager.GetActiveScene());
			}
			OnStopClient();
			NetworkClient.Disconnect();
			NetworkClient.Shutdown();
			mode = NetworkManagerMode.Offline;
			if (!string.IsNullOrEmpty(offlineScene) && !IsSceneActive(offlineScene) && loadingSceneAsync == null && !NetworkServer.active)
			{
				ClientChangeScene(offlineScene);
			}
			networkSceneName = "";
		}

		public virtual void OnApplicationQuit()
		{
			if (NetworkClient.isConnected)
			{
				StopClient();
			}
			if (NetworkServer.active)
			{
				StopServer();
			}
		}

		[Obsolete("Renamed to ConfigureHeadlessFrameRate()")]
		public virtual void ConfigureServerFrameRate()
		{
		}

		public virtual void ConfigureHeadlessFrameRate()
		{
			ConfigureServerFrameRate();
		}

		private bool InitializeSingleton()
		{
			if (singleton != null && singleton == this)
			{
				return true;
			}
			if (dontDestroyOnLoad)
			{
				if (singleton != null)
				{
					Debug.LogWarning("Multiple NetworkManagers detected in the scene. Only one NetworkManager can exist at a time. The duplicate NetworkManager will be destroyed.");
					UnityEngine.Object.Destroy(base.gameObject);
					return false;
				}
				singleton = this;
				if (Application.isPlaying)
				{
					UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
				}
			}
			else
			{
				singleton = this;
			}
			Transport.activeTransport = transport;
			return true;
		}

		private void RegisterServerMessages()
		{
			NetworkServer.OnConnectedEvent = OnServerConnectInternal;
			NetworkServer.OnDisconnectedEvent = OnServerDisconnect;
			NetworkServer.OnErrorEvent = OnServerError;
			NetworkServer.RegisterHandler<AddPlayerMessage>(OnServerAddPlayerInternal);
			NetworkServer.ReplaceHandler<ReadyMessage>(OnServerReadyMessageInternal);
		}

		private void RegisterClientMessages()
		{
			NetworkClient.OnConnectedEvent = OnClientConnectInternal;
			NetworkClient.OnDisconnectedEvent = OnClientDisconnectInternal;
			NetworkClient.OnErrorEvent = OnClientError;
			NetworkClient.RegisterHandler<NotReadyMessage>(OnClientNotReadyMessageInternal);
			NetworkClient.RegisterHandler<SceneMessage>(OnClientSceneInternal, false);
			if (playerPrefab != null)
			{
				NetworkClient.RegisterPrefab(playerPrefab);
			}
			foreach (GameObject item in spawnPrefabs.Where(_003C_003Ec._003C_003E9__55_0 ?? (_003C_003Ec._003C_003E9__55_0 = _003C_003Ec._003C_003E9._003CRegisterClientMessages_003Eb__55_0)))
			{
				NetworkClient.RegisterPrefab(item);
			}
		}

		public static void Shutdown()
		{
			if (!(singleton == null))
			{
				startPositions.Clear();
				startPositionIndex = 0;
				clientReadyConnection = null;
				singleton.StopHost();
				singleton = null;
			}
		}

		public virtual void OnDestroy()
		{
		}

		public virtual void ServerChangeScene(string newSceneName)
		{
			if (string.IsNullOrEmpty(newSceneName))
			{
				Debug.LogError("ServerChangeScene empty scene name");
				return;
			}
			NetworkServer.SetAllClientsNotReady();
			networkSceneName = newSceneName;
			OnServerChangeScene(newSceneName);
			NetworkServer.isLoadingScene = true;
			loadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName);
			if (NetworkServer.active)
			{
				SceneMessage message = default(SceneMessage);
				message.sceneName = newSceneName;
				NetworkServer.SendToAll(message);
			}
			startPositionIndex = 0;
			startPositions.Clear();
		}

		internal void ClientChangeScene(string newSceneName, SceneOperation sceneOperation = SceneOperation.Normal, bool customHandling = false)
		{
			if (string.IsNullOrEmpty(newSceneName))
			{
				Debug.LogError("ClientChangeScene empty scene name");
				return;
			}
			OnClientChangeScene(newSceneName, sceneOperation, customHandling);
			if (NetworkServer.active)
			{
				return;
			}
			NetworkClient.isLoadingScene = true;
			clientSceneOperation = sceneOperation;
			if (customHandling)
			{
				return;
			}
			switch (sceneOperation)
			{
			case SceneOperation.Normal:
				loadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName);
				break;
			case SceneOperation.LoadAdditive:
				if (!SceneManager.GetSceneByName(newSceneName).IsValid() && !SceneManager.GetSceneByPath(newSceneName).IsValid())
				{
					loadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
					break;
				}
				Debug.LogWarning("Scene " + newSceneName + " is already loaded");
				NetworkClient.isLoadingScene = false;
				break;
			case SceneOperation.UnloadAdditive:
				if (SceneManager.GetSceneByName(newSceneName).IsValid() || SceneManager.GetSceneByPath(newSceneName).IsValid())
				{
					loadingSceneAsync = SceneManager.UnloadSceneAsync(newSceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
					break;
				}
				Debug.LogWarning("Cannot unload " + newSceneName + " with UnloadAdditive operation");
				NetworkClient.isLoadingScene = false;
				break;
			}
			if (sceneOperation == SceneOperation.Normal)
			{
				networkSceneName = newSceneName;
			}
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (mode == LoadSceneMode.Additive)
			{
				if (NetworkServer.active)
				{
					NetworkServer.SpawnObjects();
				}
				if (NetworkClient.active)
				{
					NetworkClient.PrepareToSpawnSceneObjects();
				}
			}
		}

		private void UpdateScene()
		{
			if (loadingSceneAsync != null && loadingSceneAsync.isDone)
			{
				try
				{
					FinishLoadScene();
				}
				finally
				{
					loadingSceneAsync.allowSceneActivation = true;
					loadingSceneAsync = null;
				}
			}
		}

		protected void FinishLoadScene()
		{
			Debug.Log("FinishLoadScene: resuming handlers after scene was loading.");
			NetworkServer.isLoadingScene = false;
			NetworkClient.isLoadingScene = false;
			if (mode == NetworkManagerMode.Host)
			{
				FinishLoadSceneHost();
			}
			else if (mode == NetworkManagerMode.ServerOnly)
			{
				FinishLoadSceneServerOnly();
			}
			else if (mode == NetworkManagerMode.ClientOnly)
			{
				FinishLoadSceneClientOnly();
			}
		}

		private void FinishLoadSceneHost()
		{
			Debug.Log("Finished loading scene in host mode.");
			if (clientReadyConnection != null)
			{
				OnClientConnect(clientReadyConnection);
				clientLoadedScene = true;
				clientReadyConnection = null;
			}
			if (finishStartHostPending)
			{
				finishStartHostPending = false;
				FinishStartHost();
				OnServerSceneChanged(networkSceneName);
				return;
			}
			NetworkServer.SpawnObjects();
			OnServerSceneChanged(networkSceneName);
			if (NetworkClient.isConnected)
			{
				OnClientSceneChanged(NetworkClient.connection);
			}
		}

		private void FinishLoadSceneServerOnly()
		{
			Debug.Log("Finished loading scene in server-only mode.");
			NetworkServer.SpawnObjects();
			OnServerSceneChanged(networkSceneName);
		}

		private void FinishLoadSceneClientOnly()
		{
			Debug.Log("Finished loading scene in client-only mode.");
			if (clientReadyConnection != null)
			{
				OnClientConnect(clientReadyConnection);
				clientLoadedScene = true;
				clientReadyConnection = null;
			}
			if (NetworkClient.isConnected)
			{
				OnClientSceneChanged(NetworkClient.connection);
			}
		}

		public static void RegisterStartPosition(Transform start)
		{
			startPositions.Add(start);
			startPositions = startPositions.OrderBy(_003C_003Ec._003C_003E9__72_0 ?? (_003C_003Ec._003C_003E9__72_0 = _003C_003Ec._003C_003E9._003CRegisterStartPosition_003Eb__72_0)).ToList();
		}

		public static void UnRegisterStartPosition(Transform start)
		{
			startPositions.Remove(start);
		}

		public Transform GetStartPosition()
		{
			startPositions.RemoveAll(_003C_003Ec._003C_003E9__74_0 ?? (_003C_003Ec._003C_003E9__74_0 = _003C_003Ec._003C_003E9._003CGetStartPosition_003Eb__74_0));
			if (startPositions.Count == 0)
			{
				return null;
			}
			if (playerSpawnMethod == PlayerSpawnMethod.Random)
			{
				return startPositions[UnityEngine.Random.Range(0, startPositions.Count)];
			}
			Transform result = startPositions[startPositionIndex];
			startPositionIndex = (startPositionIndex + 1) % startPositions.Count;
			return result;
		}

		private void OnServerConnectInternal(NetworkConnection conn)
		{
			if (authenticator != null)
			{
				authenticator.OnServerAuthenticate(conn);
			}
			else
			{
				OnServerAuthenticated(conn);
			}
		}

		private void OnServerAuthenticated(NetworkConnection conn)
		{
			conn.isAuthenticated = true;
			if (networkSceneName != "" && networkSceneName != offlineScene)
			{
				SceneMessage sceneMessage = default(SceneMessage);
				sceneMessage.sceneName = networkSceneName;
				SceneMessage message = sceneMessage;
				conn.Send(message);
			}
			OnServerConnect(conn);
		}

		private void OnServerReadyMessageInternal(NetworkConnection conn, ReadyMessage msg)
		{
			OnServerReady(conn);
		}

		private void OnServerAddPlayerInternal(NetworkConnection conn, AddPlayerMessage msg)
		{
			if (autoCreatePlayer && playerPrefab == null)
			{
				Debug.LogError("The PlayerPrefab is empty on the NetworkManager. Please setup a PlayerPrefab object.");
			}
			else if (autoCreatePlayer && playerPrefab.GetComponent<NetworkIdentity>() == null)
			{
				Debug.LogError("The PlayerPrefab does not have a NetworkIdentity. Please add a NetworkIdentity to the player prefab.");
			}
			else if (conn.identity != null)
			{
				Debug.LogError("There is already a player for this connection.");
			}
			else
			{
				OnServerAddPlayer(conn);
			}
		}

		private void OnClientConnectInternal()
		{
			if (authenticator != null)
			{
				authenticator.OnClientAuthenticate();
			}
			else
			{
				OnClientAuthenticated(NetworkClient.connection);
			}
		}

		private void OnClientAuthenticated(NetworkConnection conn)
		{
			conn.isAuthenticated = true;
			if (string.IsNullOrEmpty(onlineScene) || onlineScene == offlineScene || IsSceneActive(onlineScene))
			{
				clientLoadedScene = false;
				OnClientConnect(conn);
			}
			else
			{
				clientLoadedScene = true;
				clientReadyConnection = conn;
			}
		}

		private void OnClientDisconnectInternal()
		{
			OnClientDisconnect(NetworkClient.connection);
		}

		private void OnClientNotReadyMessageInternal(NotReadyMessage msg)
		{
			NetworkClient.ready = false;
			OnClientNotReady(NetworkClient.connection);
		}

		private void OnClientSceneInternal(SceneMessage msg)
		{
			if (NetworkClient.isConnected)
			{
				ClientChangeScene(msg.sceneName, msg.sceneOperation, msg.customHandling);
			}
		}

		public virtual void OnServerConnect(NetworkConnection conn)
		{
		}

		public virtual void OnServerDisconnect(NetworkConnection conn)
		{
			NetworkServer.DestroyPlayerForConnection(conn);
		}

		public virtual void OnServerReady(NetworkConnection conn)
		{
			bool flag = conn.identity == null;
			NetworkServer.SetClientReady(conn);
		}

		public virtual void OnServerAddPlayer(NetworkConnection conn)
		{
			Transform startPosition = GetStartPosition();
			GameObject gameObject = ((startPosition != null) ? UnityEngine.Object.Instantiate(playerPrefab, startPosition.position, startPosition.rotation) : UnityEngine.Object.Instantiate(playerPrefab));
			gameObject.name = string.Format("{0} [connId={1}]", playerPrefab.name, conn.connectionId);
			NetworkServer.AddPlayerForConnection(conn, gameObject);
		}

		[Obsolete("OnServerError was removed because it hasn't been used in a long time.")]
		public virtual void OnServerError(NetworkConnection conn, int errorCode)
		{
		}

		public virtual void OnServerError(NetworkConnection conn, Exception exception)
		{
		}

		public virtual void OnServerChangeScene(string newSceneName)
		{
		}

		public virtual void OnServerSceneChanged(string sceneName)
		{
		}

		public virtual void OnClientConnect(NetworkConnection conn)
		{
			if (!clientLoadedScene)
			{
				if (!NetworkClient.ready)
				{
					NetworkClient.Ready();
				}
				if (autoCreatePlayer)
				{
					NetworkClient.AddPlayer();
				}
			}
		}

		public virtual void OnClientDisconnect(NetworkConnection conn)
		{
			StopClient();
		}

		[Obsolete("OnClientError was removed because it hasn't been used in a long time.")]
		public virtual void OnClientError(NetworkConnection conn, int errorCode)
		{
		}

		public virtual void OnClientError(Exception exception)
		{
		}

		public virtual void OnClientNotReady(NetworkConnection conn)
		{
		}

		public virtual void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
		{
		}

		public virtual void OnClientSceneChanged(NetworkConnection conn)
		{
			if (!NetworkClient.ready)
			{
				NetworkClient.Ready();
			}
			if (clientSceneOperation == SceneOperation.Normal && autoCreatePlayer && NetworkClient.localPlayer == null)
			{
				NetworkClient.AddPlayer();
			}
		}

		public virtual void OnStartHost()
		{
		}

		public virtual void OnStartServer()
		{
		}

		public virtual void OnStartClient()
		{
		}

		public virtual void OnStopServer()
		{
		}

		public virtual void OnStopClient()
		{
		}

		public virtual void OnStopHost()
		{
		}
	}
}
