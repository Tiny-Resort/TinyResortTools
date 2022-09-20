using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirror
{
	public static class NetworkServer
	{
		private enum DestroyMode
		{
			Destroy = 0,
			Reset = 1
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass48_0<T> where T : struct, NetworkMessage
		{
			public Action<T> handler;

			internal void _003CRegisterHandler_003Eb__0(NetworkConnection _, T value)
			{
				handler(value);
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass50_0<T> where T : struct, NetworkMessage
		{
			public Action<T> handler;

			internal void _003CReplaceHandler_003Eb__0(NetworkConnection _, T value)
			{
				handler(value);
			}
		}

		private static bool initialized;

		public static int maxConnections;

		public static Dictionary<int, NetworkConnectionToClient> connections = new Dictionary<int, NetworkConnectionToClient>();

		internal static Dictionary<ushort, NetworkMessageDelegate> handlers = new Dictionary<ushort, NetworkMessageDelegate>();

		public static bool dontListen;

		public static bool isLoadingScene;

		public static InterestManagement aoi;

		[Obsolete("Transport is responsible for timeouts.")]
		public static bool disconnectInactiveConnections;

		[Obsolete("Transport is responsible for timeouts. Configure the Transport's timeout setting instead.")]
		public static float disconnectInactiveTimeout = 60f;

		public static Action<NetworkConnection> OnConnectedEvent;

		public static Action<NetworkConnection> OnDisconnectedEvent;

		public static Action<NetworkConnection, Exception> OnErrorEvent;

		private static readonly HashSet<NetworkConnection> newObservers = new HashSet<NetworkConnection>();

		private static readonly List<NetworkConnectionToClient> connectionsCopy = new List<NetworkConnectionToClient>();

		public static NetworkConnectionToClient localConnection { get; private set; }

		public static bool localClientActive
		{
			get
			{
				return localConnection != null;
			}
		}

		public static bool active { get; internal set; }

		private static void Initialize()
		{
			if (!initialized)
			{
				initialized = true;
				connections.Clear();
				NetworkTime.Reset();
				AddTransportHandlers();
			}
		}

		private static void AddTransportHandlers()
		{
			Transport.activeTransport.OnServerConnected = OnTransportConnected;
			Transport.activeTransport.OnServerDataReceived = OnTransportData;
			Transport.activeTransport.OnServerDisconnected = OnTransportDisconnected;
			Transport.activeTransport.OnServerError = OnError;
		}

		public static void ActivateHostScene()
		{
			foreach (NetworkIdentity value in NetworkIdentity.spawned.Values)
			{
				if (!value.isClient)
				{
					value.OnStartClient();
				}
			}
		}

		internal static void RegisterMessageHandlers()
		{
			RegisterHandler<ReadyMessage>(OnClientReadyMessage);
			RegisterHandler<CommandMessage>(OnCommandMessage);
			RegisterHandler<NetworkPingMessage>(NetworkTime.OnServerPing, false);
		}

		public static void Listen(int maxConns)
		{
			Initialize();
			maxConnections = maxConns;
			if (!dontListen)
			{
				Transport.activeTransport.ServerStart();
				Debug.Log("Server started listening");
			}
			active = true;
			RegisterMessageHandlers();
		}

		private static void CleanupNetworkIdentities()
		{
			foreach (NetworkIdentity item in NetworkIdentity.spawned.Values.ToList())
			{
				if (item != null)
				{
					if (item.sceneId != 0L)
					{
						DestroyObject(item, DestroyMode.Reset);
						item.gameObject.SetActive(false);
					}
					else
					{
						DestroyObject(item, DestroyMode.Destroy);
					}
				}
			}
			NetworkIdentity.spawned.Clear();
		}

		public static void Shutdown()
		{
			if (initialized)
			{
				DisconnectAll();
				Transport.activeTransport.ServerStop();
				initialized = false;
			}
			dontListen = false;
			active = false;
			handlers.Clear();
			CleanupNetworkIdentities();
			NetworkIdentity.ResetNextNetworkId();
			OnConnectedEvent = null;
			OnDisconnectedEvent = null;
		}

		public static bool AddConnection(NetworkConnectionToClient conn)
		{
			if (!connections.ContainsKey(conn.connectionId))
			{
				connections[conn.connectionId] = conn;
				return true;
			}
			return false;
		}

		public static bool RemoveConnection(int connectionId)
		{
			return connections.Remove(connectionId);
		}

		internal static void SetLocalConnection(LocalConnectionToClient conn)
		{
			if (localConnection != null)
			{
				Debug.LogError("Local Connection already exists");
			}
			else
			{
				localConnection = conn;
			}
		}

		internal static void RemoveLocalConnection()
		{
			if (localConnection != null)
			{
				localConnection.Disconnect();
				localConnection = null;
			}
			RemoveConnection(0);
		}

		public static bool NoExternalConnections()
		{
			if (connections.Count != 0)
			{
				if (connections.Count == 1)
				{
					return localConnection != null;
				}
				return false;
			}
			return true;
		}

		[Obsolete("NoConnections was renamed to NoExternalConnections because that's what it checks for.")]
		public static bool NoConnections()
		{
			return NoExternalConnections();
		}

		public static void SendToAll<T>(T message, int channelId = 0, bool sendToReadyOnly = false) where T : struct, NetworkMessage
		{
			if (!active)
			{
				Debug.LogWarning("Can not send using NetworkServer.SendToAll<T>(T msg) because NetworkServer is not active");
				return;
			}
			using (PooledNetworkWriter pooledNetworkWriter = NetworkWriterPool.GetWriter())
			{
				MessagePacking.Pack(message, pooledNetworkWriter);
				ArraySegment<byte> segment = pooledNetworkWriter.ToArraySegment();
				int num = 0;
				foreach (NetworkConnectionToClient value in connections.Values)
				{
					if (!sendToReadyOnly || value.isReady)
					{
						num++;
						value.Send(segment, channelId);
					}
				}
				NetworkDiagnostics.OnSend(message, channelId, segment.Count, num);
			}
		}

		public static void SendToReady<T>(T message, int channelId = 0) where T : struct, NetworkMessage
		{
			if (!active)
			{
				Debug.LogWarning("Can not send using NetworkServer.SendToReady<T>(T msg) because NetworkServer is not active");
			}
			else
			{
				SendToAll(message, channelId, true);
			}
		}

		public static void SendToReady<T>(NetworkIdentity identity, T message, bool includeOwner = true, int channelId = 0) where T : struct, NetworkMessage
		{
			if (identity == null || identity.observers == null || identity.observers.Count == 0)
			{
				return;
			}
			using (PooledNetworkWriter pooledNetworkWriter = NetworkWriterPool.GetWriter())
			{
				MessagePacking.Pack(message, pooledNetworkWriter);
				ArraySegment<byte> segment = pooledNetworkWriter.ToArraySegment();
				int num = 0;
				foreach (NetworkConnection value in identity.observers.Values)
				{
					if ((value != identity.connectionToClient || includeOwner) && value.isReady)
					{
						num++;
						value.Send(segment, channelId);
					}
				}
				NetworkDiagnostics.OnSend(message, channelId, segment.Count, num);
			}
		}

		public static void SendToReady<T>(NetworkIdentity identity, T message, int channelId) where T : struct, NetworkMessage
		{
			SendToReady(identity, message, true, channelId);
		}

		private static void SendToObservers<T>(NetworkIdentity identity, T message, int channelId = 0) where T : struct, NetworkMessage
		{
			if (identity == null || identity.observers == null || identity.observers.Count == 0)
			{
				return;
			}
			using (PooledNetworkWriter pooledNetworkWriter = NetworkWriterPool.GetWriter())
			{
				MessagePacking.Pack(message, pooledNetworkWriter);
				ArraySegment<byte> segment = pooledNetworkWriter.ToArraySegment();
				foreach (NetworkConnection value in identity.observers.Values)
				{
					value.Send(segment, channelId);
				}
				NetworkDiagnostics.OnSend(message, channelId, segment.Count, identity.observers.Count);
			}
		}

		[Obsolete("Use identity.connectionToClient.Send() instead! Previously Mirror needed this function internally, but not anymore.")]
		public static void SendToClientOfPlayer<T>(NetworkIdentity identity, T msg, int channelId = 0) where T : struct, NetworkMessage
		{
			if (identity != null)
			{
				identity.connectionToClient.Send(msg, channelId);
			}
			else
			{
				Debug.LogError("SendToClientOfPlayer: player has no NetworkIdentity: " + (((object)identity != null) ? identity.ToString() : null));
			}
		}

		private static void OnTransportConnected(int connectionId)
		{
			if (connectionId == 0)
			{
				Debug.LogError("Server.HandleConnect: invalid connectionId: " + connectionId + " . Needs to be != 0, because 0 is reserved for local player.");
				Transport.activeTransport.ServerDisconnect(connectionId);
			}
			else if (connections.ContainsKey(connectionId))
			{
				Transport.activeTransport.ServerDisconnect(connectionId);
			}
			else if (connections.Count < maxConnections)
			{
				OnConnected(new NetworkConnectionToClient(connectionId));
			}
			else
			{
				Transport.activeTransport.ServerDisconnect(connectionId);
			}
		}

		internal static void OnConnected(NetworkConnectionToClient conn)
		{
			AddConnection(conn);
			Action<NetworkConnection> onConnectedEvent = OnConnectedEvent;
			if (onConnectedEvent != null)
			{
				onConnectedEvent(conn);
			}
		}

		private static bool UnpackAndInvoke(NetworkConnectionToClient connection, NetworkReader reader, int channelId)
		{
			ushort msgType;
			if (MessagePacking.Unpack(reader, out msgType))
			{
				NetworkMessageDelegate value;
				if (handlers.TryGetValue(msgType, out value))
				{
					value(connection, reader, channelId);
					connection.lastMessageTime = Time.time;
					return true;
				}
				return false;
			}
			Debug.LogError("Closed connection: " + ((connection != null) ? connection.ToString() : null) + ". Invalid message header.");
			connection.Disconnect();
			return false;
		}

		internal static void OnTransportData(int connectionId, ArraySegment<byte> data, int channelId)
		{
			NetworkConnectionToClient value;
			if (connections.TryGetValue(connectionId, out value))
			{
				if (!value.unbatcher.AddBatch(data))
				{
					Debug.LogWarning("NetworkServer: received Message was too short (messages should start with message id)");
					value.Disconnect();
					return;
				}
				NetworkReader message;
				double remoteTimeStamp;
				while (!isLoadingScene && value.unbatcher.GetNextMessage(out message, out remoteTimeStamp))
				{
					if (message.Remaining >= 2)
					{
						value.remoteTimeStamp = remoteTimeStamp;
						if (!UnpackAndInvoke(value, message, channelId))
						{
							break;
						}
						continue;
					}
					Debug.LogError(string.Format("NetworkServer: received Message was too short (messages should start with message id). Disconnecting {0}", connectionId));
					value.Disconnect();
					break;
				}
			}
			else
			{
				Debug.LogError("HandleData Unknown connectionId:" + connectionId);
			}
		}

		internal static void OnTransportDisconnected(int connectionId)
		{
			NetworkConnectionToClient value;
			if (connections.TryGetValue(connectionId, out value))
			{
				RemoveConnection(connectionId);
				if (OnDisconnectedEvent != null)
				{
					OnDisconnectedEvent(value);
				}
				else
				{
					DestroyPlayerForConnection(value);
				}
			}
		}

		private static void OnError(int connectionId, Exception exception)
		{
			Debug.LogException(exception);
			NetworkConnectionToClient value;
			connections.TryGetValue(connectionId, out value);
			Action<NetworkConnection, Exception> onErrorEvent = OnErrorEvent;
			if (onErrorEvent != null)
			{
				onErrorEvent(value, exception);
			}
		}

		public static void RegisterHandler<T>(Action<NetworkConnection, T> handler, bool requireAuthentication = true) where T : struct, NetworkMessage
		{
			ushort id = MessagePacking.GetId<T>();
			if (handlers.ContainsKey(id))
			{
				Debug.LogWarning(string.Format("NetworkServer.RegisterHandler replacing handler for {0}, id={1}. If replacement is intentional, use ReplaceHandler instead to avoid this warning.", typeof(T).FullName, id));
			}
			handlers[id] = MessagePacking.WrapHandler(handler, requireAuthentication);
		}

		[Obsolete("Use RegisterHandler(Action<NetworkConnection, T), requireAuthentication instead.")]
		public static void RegisterHandler<T>(Action<T> handler, bool requireAuthentication = true) where T : struct, NetworkMessage
		{
			_003C_003Ec__DisplayClass48_0<T> _003C_003Ec__DisplayClass48_ = new _003C_003Ec__DisplayClass48_0<T>();
			_003C_003Ec__DisplayClass48_.handler = handler;
			RegisterHandler<T>(_003C_003Ec__DisplayClass48_._003CRegisterHandler_003Eb__0, requireAuthentication);
		}

		public static void ReplaceHandler<T>(Action<NetworkConnection, T> handler, bool requireAuthentication = true) where T : struct, NetworkMessage
		{
			ushort id = MessagePacking.GetId<T>();
			handlers[id] = MessagePacking.WrapHandler(handler, requireAuthentication);
		}

		public static void ReplaceHandler<T>(Action<T> handler, bool requireAuthentication = true) where T : struct, NetworkMessage
		{
			_003C_003Ec__DisplayClass50_0<T> _003C_003Ec__DisplayClass50_ = new _003C_003Ec__DisplayClass50_0<T>();
			_003C_003Ec__DisplayClass50_.handler = handler;
			ReplaceHandler<T>(_003C_003Ec__DisplayClass50_._003CReplaceHandler_003Eb__0, requireAuthentication);
		}

		public static void UnregisterHandler<T>() where T : struct, NetworkMessage
		{
			ushort id = MessagePacking.GetId<T>();
			handlers.Remove(id);
		}

		public static void ClearHandlers()
		{
			handlers.Clear();
		}

		internal static bool GetNetworkIdentity(GameObject go, out NetworkIdentity identity)
		{
			identity = go.GetComponent<NetworkIdentity>();
			if (identity == null)
			{
				Debug.LogError("GameObject " + go.name + " doesn't have NetworkIdentity.");
				return false;
			}
			return true;
		}

		public static void DisconnectAll()
		{
			foreach (NetworkConnectionToClient item in connections.Values.ToList())
			{
				item.Disconnect();
				if (item.connectionId != 0)
				{
					OnTransportDisconnected(item.connectionId);
				}
			}
			connections.Clear();
			localConnection = null;
			active = false;
		}

		[Obsolete("Call NetworkClient.DisconnectAll() instead")]
		public static void DisconnectAllExternalConnections()
		{
			DisconnectAll();
		}

		[Obsolete("Call NetworkClient.DisconnectAll() instead")]
		public static void DisconnectAllConnections()
		{
			DisconnectAll();
		}

		public static bool AddPlayerForConnection(NetworkConnection conn, GameObject player)
		{
			NetworkIdentity component = player.GetComponent<NetworkIdentity>();
			if (component == null)
			{
				Debug.LogWarning("AddPlayer: playerGameObject has no NetworkIdentity. Please add a NetworkIdentity to " + (((object)player != null) ? player.ToString() : null));
				return false;
			}
			if (conn.identity != null)
			{
				Debug.Log("AddPlayer: player object already exists");
				return false;
			}
			conn.identity = component;
			component.SetClientOwner(conn);
			if (conn is LocalConnectionToClient)
			{
				component.hasAuthority = true;
				NetworkClient.InternalAddPlayer(component);
			}
			SetClientReady(conn);
			Respawn(component);
			return true;
		}

		public static bool AddPlayerForConnection(NetworkConnection conn, GameObject player, Guid assetId)
		{
			NetworkIdentity identity;
			if (GetNetworkIdentity(player, out identity))
			{
				identity.assetId = assetId;
			}
			return AddPlayerForConnection(conn, player);
		}

		public static bool ReplacePlayerForConnection(NetworkConnection conn, GameObject player, bool keepAuthority = false)
		{
			NetworkIdentity component = player.GetComponent<NetworkIdentity>();
			if (component == null)
			{
				Debug.LogError("ReplacePlayer: playerGameObject has no NetworkIdentity. Please add a NetworkIdentity to " + (((object)player != null) ? player.ToString() : null));
				return false;
			}
			if (component.connectionToClient != null && component.connectionToClient != conn)
			{
				Debug.LogError("Cannot replace player for connection. New player is already owned by a different connection" + (((object)player != null) ? player.ToString() : null));
				return false;
			}
			NetworkIdentity identity = conn.identity;
			conn.identity = component;
			component.SetClientOwner(conn);
			if (conn is LocalConnectionToClient)
			{
				component.hasAuthority = true;
				NetworkClient.InternalAddPlayer(component);
			}
			SpawnObserversForConnection(conn);
			Respawn(component);
			if (!keepAuthority)
			{
				identity.RemoveClientAuthority();
			}
			return true;
		}

		public static bool ReplacePlayerForConnection(NetworkConnection conn, GameObject player, Guid assetId, bool keepAuthority = false)
		{
			NetworkIdentity identity;
			if (GetNetworkIdentity(player, out identity))
			{
				identity.assetId = assetId;
			}
			return ReplacePlayerForConnection(conn, player, keepAuthority);
		}

		public static void SetClientReady(NetworkConnection conn)
		{
			conn.isReady = true;
			if (conn.identity != null)
			{
				SpawnObserversForConnection(conn);
			}
		}

		public static void SetClientNotReady(NetworkConnection conn)
		{
			if (conn.isReady)
			{
				conn.isReady = false;
				conn.RemoveFromObservingsObservers();
				conn.Send(default(NotReadyMessage));
			}
		}

		public static void SetAllClientsNotReady()
		{
			foreach (NetworkConnectionToClient value in connections.Values)
			{
				SetClientNotReady(value);
			}
		}

		private static void OnClientReadyMessage(NetworkConnection conn, ReadyMessage msg)
		{
			SetClientReady(conn);
		}

		internal static void ShowForConnection(NetworkIdentity identity, NetworkConnection conn)
		{
			if (conn.isReady)
			{
				SendSpawnMessage(identity, conn);
			}
		}

		internal static void HideForConnection(NetworkIdentity identity, NetworkConnection conn)
		{
			ObjectHideMessage objectHideMessage = default(ObjectHideMessage);
			objectHideMessage.netId = identity.netId;
			ObjectHideMessage message = objectHideMessage;
			conn.Send(message);
		}

		public static void RemovePlayerForConnection(NetworkConnection conn, bool destroyServerObject)
		{
			if (conn.identity != null)
			{
				if (destroyServerObject)
				{
					Destroy(conn.identity.gameObject);
				}
				else
				{
					UnSpawn(conn.identity.gameObject);
				}
				conn.identity = null;
			}
		}

		private static void OnCommandMessage(NetworkConnection conn, CommandMessage msg)
		{
			NetworkIdentity value;
			if (!NetworkIdentity.spawned.TryGetValue(msg.netId, out value))
			{
				Debug.LogWarning("Spawned object not found when handling Command message [netId=" + msg.netId + "]");
				return;
			}
			if (value.GetCommandInfo(msg.componentIndex, msg.functionHash).requiresAuthority && value.connectionToClient != conn)
			{
				Debug.LogWarning("Command for object without authority [netId=" + msg.netId + "]");
				return;
			}
			using (PooledNetworkReader reader = NetworkReaderPool.GetReader(msg.payload))
			{
				value.HandleRemoteCall(msg.componentIndex, msg.functionHash, MirrorInvokeType.Command, reader, conn as NetworkConnectionToClient);
			}
		}

		private static ArraySegment<byte> CreateSpawnMessagePayload(bool isOwner, NetworkIdentity identity, PooledNetworkWriter ownerWriter, PooledNetworkWriter observersWriter)
		{
			if (identity.NetworkBehaviours.Length == 0)
			{
				return default(ArraySegment<byte>);
			}
			identity.OnSerializeAllSafely(true, ownerWriter, observersWriter);
			ArraySegment<byte> result = ownerWriter.ToArraySegment();
			ArraySegment<byte> result2 = observersWriter.ToArraySegment();
			if (!isOwner)
			{
				return result2;
			}
			return result;
		}

		internal static void SendSpawnMessage(NetworkIdentity identity, NetworkConnection conn)
		{
			if (identity.serverOnly)
			{
				return;
			}
			using (PooledNetworkWriter ownerWriter = NetworkWriterPool.GetWriter())
			{
				using (PooledNetworkWriter observersWriter = NetworkWriterPool.GetWriter())
				{
					bool isOwner = identity.connectionToClient == conn;
					ArraySegment<byte> payload = CreateSpawnMessagePayload(isOwner, identity, ownerWriter, observersWriter);
					SpawnMessage spawnMessage = default(SpawnMessage);
					spawnMessage.netId = identity.netId;
					spawnMessage.isLocalPlayer = conn.identity == identity;
					spawnMessage.isOwner = isOwner;
					spawnMessage.sceneId = identity.sceneId;
					spawnMessage.assetId = identity.assetId;
					spawnMessage.position = identity.transform.localPosition;
					spawnMessage.rotation = identity.transform.localRotation;
					spawnMessage.scale = identity.transform.localScale;
					spawnMessage.payload = payload;
					SpawnMessage message = spawnMessage;
					conn.Send(message);
				}
			}
		}

		private static void SpawnObject(GameObject obj, NetworkConnection ownerConnection)
		{
			if (Utils.IsPrefab(obj))
			{
				Debug.LogError("GameObject " + obj.name + " is a prefab, it can't be spawned. Instantiate it first.");
				return;
			}
			if (!active)
			{
				Debug.LogError("SpawnObject for " + (((object)obj != null) ? obj.ToString() : null) + ", NetworkServer is not active. Cannot spawn objects without an active server.");
				return;
			}
			NetworkIdentity component = obj.GetComponent<NetworkIdentity>();
			if (component == null)
			{
				Debug.LogError("SpawnObject " + (((object)obj != null) ? obj.ToString() : null) + " has no NetworkIdentity. Please add a NetworkIdentity to " + (((object)obj != null) ? obj.ToString() : null));
			}
			else
			{
				if (component.SpawnedFromInstantiate)
				{
					return;
				}
				component.connectionToClient = (NetworkConnectionToClient)ownerConnection;
				if (ownerConnection is LocalConnectionToClient)
				{
					component.hasAuthority = true;
				}
				component.OnStartServer();
				if ((bool)aoi)
				{
					try
					{
						aoi.OnSpawned(component);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
				RebuildObservers(component, true);
			}
		}

		public static void Spawn(GameObject obj, NetworkConnection ownerConnection = null)
		{
			SpawnObject(obj, ownerConnection);
		}

		public static void Spawn(GameObject obj, GameObject ownerPlayer)
		{
			NetworkIdentity component = ownerPlayer.GetComponent<NetworkIdentity>();
			if (component == null)
			{
				Debug.LogError("Player object has no NetworkIdentity");
			}
			else if (component.connectionToClient == null)
			{
				Debug.LogError("Player object is not a player.");
			}
			else
			{
				Spawn(obj, component.connectionToClient);
			}
		}

		public static void Spawn(GameObject obj, Guid assetId, NetworkConnection ownerConnection = null)
		{
			NetworkIdentity identity;
			if (GetNetworkIdentity(obj, out identity))
			{
				identity.assetId = assetId;
			}
			SpawnObject(obj, ownerConnection);
		}

		internal static bool ValidateSceneObject(NetworkIdentity identity)
		{
			if (identity.gameObject.hideFlags == HideFlags.NotEditable || identity.gameObject.hideFlags == HideFlags.HideAndDontSave)
			{
				return false;
			}
			return identity.sceneId != 0;
		}

		public static bool SpawnObjects()
		{
			if (!active)
			{
				return false;
			}
			NetworkIdentity[] array = Resources.FindObjectsOfTypeAll<NetworkIdentity>();
			NetworkIdentity[] array2 = array;
			foreach (NetworkIdentity networkIdentity in array2)
			{
				if (ValidateSceneObject(networkIdentity))
				{
					networkIdentity.gameObject.SetActive(true);
					if (!networkIdentity.gameObject.activeInHierarchy)
					{
						networkIdentity.Awake();
					}
				}
			}
			array2 = array;
			foreach (NetworkIdentity networkIdentity2 in array2)
			{
				if (ValidateSceneObject(networkIdentity2))
				{
					Spawn(networkIdentity2.gameObject);
				}
			}
			return true;
		}

		private static void Respawn(NetworkIdentity identity)
		{
			if (identity.netId == 0)
			{
				Spawn(identity.gameObject, identity.connectionToClient);
			}
			else
			{
				SendSpawnMessage(identity, identity.connectionToClient);
			}
		}

		private static void SpawnObserversForConnection(NetworkConnection conn)
		{
			if (!conn.isReady)
			{
				return;
			}
			conn.Send(default(ObjectSpawnStartedMessage));
			foreach (NetworkIdentity value in NetworkIdentity.spawned.Values)
			{
				if (!value.gameObject.activeSelf)
				{
					continue;
				}
				if (value.visible == Visibility.ForceShown)
				{
					value.AddObserver(conn);
				}
				else
				{
					if (value.visible == Visibility.ForceHidden || value.visible != 0)
					{
						continue;
					}
					if (value.visibility != null)
					{
						if (value.visibility.OnCheckObserver(conn))
						{
							value.AddObserver(conn);
						}
					}
					else if (aoi != null)
					{
						if (aoi.OnCheckObserver(value, conn))
						{
							value.AddObserver(conn);
						}
					}
					else
					{
						value.AddObserver(conn);
					}
				}
			}
			conn.Send(default(ObjectSpawnFinishedMessage));
		}

		public static void UnSpawn(GameObject obj)
		{
			DestroyObject(obj, DestroyMode.Reset);
		}

		public static void DestroyPlayerForConnection(NetworkConnection conn)
		{
			conn.DestroyOwnedObjects();
			conn.RemoveFromObservingsObservers();
			conn.identity = null;
		}

		private static void DestroyObject(NetworkIdentity identity, DestroyMode mode)
		{
			if ((bool)aoi)
			{
				try
				{
					aoi.OnDestroyed(identity);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			NetworkIdentity.spawned.Remove(identity.netId);
			NetworkConnectionToClient connectionToClient = identity.connectionToClient;
			if (connectionToClient != null)
			{
				connectionToClient.RemoveOwnedObject(identity);
			}
			SendToObservers(identity, new ObjectDestroyMessage
			{
				netId = identity.netId
			});
			identity.ClearObservers();
			if (NetworkClient.active && localClientActive)
			{
				identity.OnStopClient();
				identity.hasAuthority = false;
				identity.NotifyAuthority();
			}
			identity.OnStopServer();
			switch (mode)
			{
			case DestroyMode.Destroy:
				identity.destroyCalled = true;
				UnityEngine.Object.Destroy(identity.gameObject);
				break;
			case DestroyMode.Reset:
				identity.Reset();
				break;
			}
		}

		private static void DestroyObject(GameObject obj, DestroyMode mode)
		{
			NetworkIdentity identity;
			if (obj == null)
			{
				Debug.Log("NetworkServer DestroyObject is null");
			}
			else if (GetNetworkIdentity(obj, out identity))
			{
				DestroyObject(identity, mode);
			}
		}

		public static void Destroy(GameObject obj)
		{
			DestroyObject(obj, DestroyMode.Destroy);
		}

		internal static void AddAllReadyServerConnectionsToObservers(NetworkIdentity identity)
		{
			foreach (NetworkConnectionToClient value in connections.Values)
			{
				if (value.isReady)
				{
					identity.AddObserver(value);
				}
			}
			if (localConnection != null && localConnection.isReady)
			{
				identity.AddObserver(localConnection);
			}
		}

		private static void RebuildObserversDefault(NetworkIdentity identity, bool initialize)
		{
			if (initialize && identity.visible != Visibility.ForceHidden)
			{
				AddAllReadyServerConnectionsToObservers(identity);
			}
		}

		private static void RebuildObserversCustom(NetworkIdentity identity, bool initialize)
		{
			newObservers.Clear();
			if (identity.visible != Visibility.ForceHidden)
			{
				if (identity.visibility != null)
				{
					identity.visibility.OnRebuildObservers(newObservers, initialize);
				}
				else
				{
					aoi.OnRebuildObservers(identity, newObservers, initialize);
				}
			}
			if (identity.connectionToClient != null)
			{
				newObservers.Add(identity.connectionToClient);
			}
			bool flag = false;
			foreach (NetworkConnection newObserver in newObservers)
			{
				if (newObserver != null && newObserver.isReady && (initialize || !identity.observers.ContainsKey(newObserver.connectionId)))
				{
					newObserver.AddToObserving(identity);
					flag = true;
				}
			}
			foreach (NetworkConnection value in identity.observers.Values)
			{
				if (!newObservers.Contains(value))
				{
					value.RemoveFromObserving(identity, false);
					flag = true;
				}
			}
			if (flag)
			{
				identity.observers.Clear();
				foreach (NetworkConnection newObserver2 in newObservers)
				{
					if (newObserver2 != null && newObserver2.isReady)
					{
						identity.observers.Add(newObserver2.connectionId, newObserver2);
					}
				}
			}
			if (initialize && !newObservers.Contains(localConnection))
			{
				if (identity.visibility != null)
				{
					identity.visibility.OnSetHostVisibility(false);
				}
				else if (aoi != null)
				{
					aoi.SetHostVisibility(identity, false);
				}
			}
		}

		public static void RebuildObservers(NetworkIdentity identity, bool initialize)
		{
			if (identity.observers != null)
			{
				if ((aoi != null) & (identity.visibility != null))
				{
					Debug.LogError(string.Format("RebuildObservers: {0} has {1} component but there is also a global {2} component. Can't use both systems at the same time!", identity.name, identity.visibility.GetType(), aoi.GetType()));
				}
				else if ((aoi == null && identity.visibility == null) || identity.visible == Visibility.ForceShown)
				{
					RebuildObserversDefault(identity, initialize);
				}
				else
				{
					RebuildObserversCustom(identity, initialize);
				}
			}
		}

		private static NetworkWriter GetEntitySerializationForConnection(NetworkIdentity identity, NetworkConnectionToClient connection)
		{
			NetworkIdentitySerialization serializationAtTick = identity.GetSerializationAtTick(Time.frameCount);
			if (identity.connectionToClient == connection)
			{
				if (serializationAtTick.ownerWriter.Position > 0)
				{
					return serializationAtTick.ownerWriter;
				}
			}
			else if (serializationAtTick.observersWriter.Position > 0)
			{
				return serializationAtTick.observersWriter;
			}
			return null;
		}

		private static void ClearSpawnedDirtyBits()
		{
			foreach (NetworkIdentity value in NetworkIdentity.spawned.Values)
			{
				if (value.observers == null || value.observers.Count == 0)
				{
					value.ClearAllComponentsDirtyBits();
				}
			}
		}

		private static void BroadcastToConnection(NetworkConnectionToClient connection)
		{
			foreach (NetworkIdentity item in connection.observing)
			{
				if (item != null)
				{
					NetworkWriter entitySerializationForConnection = GetEntitySerializationForConnection(item, connection);
					if (entitySerializationForConnection != null)
					{
						EntityStateMessage entityStateMessage = default(EntityStateMessage);
						entityStateMessage.netId = item.netId;
						entityStateMessage.payload = entitySerializationForConnection.ToArraySegment();
						EntityStateMessage message = entityStateMessage;
						connection.Send(message);
					}
					item.ClearDirtyComponentsDirtyBits();
				}
				else
				{
					Debug.LogWarning("Found 'null' entry in observing list for connectionId=" + connection.connectionId + ". Please call NetworkServer.Destroy to destroy networked objects. Don't use GameObject.Destroy.");
				}
			}
		}

		private static bool DisconnectIfInactive(NetworkConnectionToClient connection)
		{
			if (disconnectInactiveConnections && !connection.IsAlive(disconnectInactiveTimeout))
			{
				Debug.LogWarning(string.Format("Disconnecting {0} for inactivity!", connection));
				connection.Disconnect();
				return true;
			}
			return false;
		}

		private static void Broadcast()
		{
			connectionsCopy.Clear();
			connections.Values.CopyTo(connectionsCopy);
			foreach (NetworkConnectionToClient item in connectionsCopy)
			{
				if (!DisconnectIfInactive(item))
				{
					if (item.isReady)
					{
						BroadcastToConnection(item);
					}
					item.Update();
				}
			}
			ClearSpawnedDirtyBits();
		}

		internal static void NetworkEarlyUpdate()
		{
			if (Transport.activeTransport != null)
			{
				Transport.activeTransport.ServerEarlyUpdate();
			}
		}

		internal static void NetworkLateUpdate()
		{
			if (active)
			{
				Broadcast();
			}
			if (Transport.activeTransport != null)
			{
				Transport.activeTransport.ServerLateUpdate();
			}
		}

		[Obsolete("NetworkServer.Update is now called internally from our custom update loop. No need to call Update manually anymore.")]
		public static void Update()
		{
			NetworkLateUpdate();
		}
	}
}
