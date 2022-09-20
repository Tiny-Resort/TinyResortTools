using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirror
{
	public static class NetworkClient
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static Action<NetworkPongMessage> _003C_003E9__37_0;

			public static Action<ObjectSpawnStartedMessage> _003C_003E9__37_1;

			public static Action<ObjectSpawnFinishedMessage> _003C_003E9__37_2;

			public static Action<EntityStateMessage> _003C_003E9__37_3;

			public static Func<NetworkIdentity, uint> _003C_003E9__83_0;

			internal void _003CRegisterSystemHandlers_003Eb__37_0(NetworkPongMessage msg)
			{
			}

			internal void _003CRegisterSystemHandlers_003Eb__37_1(ObjectSpawnStartedMessage msg)
			{
			}

			internal void _003CRegisterSystemHandlers_003Eb__37_2(ObjectSpawnFinishedMessage msg)
			{
			}

			internal void _003CRegisterSystemHandlers_003Eb__37_3(EntityStateMessage msg)
			{
			}

			internal uint _003COnObjectSpawnFinished_003Eb__83_0(NetworkIdentity uv)
			{
				return uv.netId;
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass51_0<T> where T : struct, NetworkMessage
		{
			public Action<T> handler;

			internal void _003CRegisterHandler_003Eg__HandlerWrapped_007C0(NetworkConnection _, T value)
			{
				handler(value);
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass53_0<T> where T : struct, NetworkMessage
		{
			public Action<T> handler;

			internal void _003CReplaceHandler_003Eb__0(NetworkConnection _, T value)
			{
				handler(value);
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass59_0
		{
			public SpawnDelegate spawnHandler;

			internal GameObject _003CRegisterPrefab_003Eb__0(SpawnMessage msg)
			{
				return spawnHandler(msg.position, msg.assetId);
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass60_0
		{
			public SpawnDelegate spawnHandler;

			internal GameObject _003CRegisterPrefab_003Eb__0(SpawnMessage msg)
			{
				return spawnHandler(msg.position, msg.assetId);
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass64_0
		{
			public SpawnDelegate spawnHandler;

			internal GameObject _003CRegisterSpawnHandler_003Eb__0(SpawnMessage msg)
			{
				return spawnHandler(msg.position, msg.assetId);
			}
		}

		internal static readonly Dictionary<ushort, NetworkMessageDelegate> handlers = new Dictionary<ushort, NetworkMessageDelegate>();

		public static bool ready;

		internal static ConnectState connectState = ConnectState.None;

		public static Action OnConnectedEvent;

		public static Action OnDisconnectedEvent;

		public static Action<Exception> OnErrorEvent;

		public static readonly Dictionary<Guid, GameObject> prefabs = new Dictionary<Guid, GameObject>();

		internal static readonly Dictionary<Guid, SpawnHandlerDelegate> spawnHandlers = new Dictionary<Guid, SpawnHandlerDelegate>();

		internal static readonly Dictionary<Guid, UnSpawnDelegate> unspawnHandlers = new Dictionary<Guid, UnSpawnDelegate>();

		private static bool isSpawnFinished;

		internal static readonly Dictionary<ulong, NetworkIdentity> spawnableObjects = new Dictionary<ulong, NetworkIdentity>();

		private static Unbatcher unbatcher = new Unbatcher();

		public static InterestManagement aoi;

		public static bool isLoadingScene;

		private static readonly List<uint> removeFromSpawned = new List<uint>();

		public static NetworkConnection connection { get; internal set; }

		[Obsolete("NetworkClient.readyConnection is redundant. Use NetworkClient.connection and use NetworkClient.ready to check if it's ready.")]
		public static NetworkConnection readyConnection
		{
			get
			{
				if (!ready)
				{
					return null;
				}
				return connection;
			}
		}

		public static NetworkIdentity localPlayer { get; internal set; }

		public static string serverIp
		{
			get
			{
				return connection.address;
			}
		}

		public static bool active
		{
			get
			{
				if (connectState != ConnectState.Connecting)
				{
					return connectState == ConnectState.Connected;
				}
				return true;
			}
		}

		public static bool isConnecting
		{
			get
			{
				return connectState == ConnectState.Connecting;
			}
		}

		public static bool isConnected
		{
			get
			{
				return connectState == ConnectState.Connected;
			}
		}

		public static bool isHostClient
		{
			get
			{
				return connection is LocalConnectionToServer;
			}
		}

		[Obsolete("isLocalClient was renamed to isHostClient because that's what it actually means.")]
		public static bool isLocalClient
		{
			get
			{
				return isHostClient;
			}
		}

		private static void AddTransportHandlers()
		{
			Transport.activeTransport.OnClientConnected = OnTransportConnected;
			Transport.activeTransport.OnClientDataReceived = OnTransportData;
			Transport.activeTransport.OnClientDisconnected = OnTransportDisconnected;
			Transport.activeTransport.OnClientError = OnError;
		}

		internal static void RegisterSystemHandlers(bool hostMode)
		{
			if (hostMode)
			{
				RegisterHandler<ObjectDestroyMessage>(OnHostClientObjectDestroy);
				RegisterHandler<ObjectHideMessage>(OnHostClientObjectHide);
				RegisterHandler(_003C_003Ec._003C_003E9__37_0 ?? (_003C_003Ec._003C_003E9__37_0 = _003C_003Ec._003C_003E9._003CRegisterSystemHandlers_003Eb__37_0), false);
				RegisterHandler<SpawnMessage>(OnHostClientSpawn);
				RegisterHandler(_003C_003Ec._003C_003E9__37_1 ?? (_003C_003Ec._003C_003E9__37_1 = _003C_003Ec._003C_003E9._003CRegisterSystemHandlers_003Eb__37_1));
				RegisterHandler(_003C_003Ec._003C_003E9__37_2 ?? (_003C_003Ec._003C_003E9__37_2 = _003C_003Ec._003C_003E9._003CRegisterSystemHandlers_003Eb__37_2));
				RegisterHandler(_003C_003Ec._003C_003E9__37_3 ?? (_003C_003Ec._003C_003E9__37_3 = _003C_003Ec._003C_003E9._003CRegisterSystemHandlers_003Eb__37_3));
			}
			else
			{
				RegisterHandler<ObjectDestroyMessage>(OnObjectDestroy);
				RegisterHandler<ObjectHideMessage>(OnObjectHide);
				RegisterHandler<NetworkPongMessage>(NetworkTime.OnClientPong, false);
				RegisterHandler<SpawnMessage>(OnSpawn);
				RegisterHandler<ObjectSpawnStartedMessage>(OnObjectSpawnStarted);
				RegisterHandler<ObjectSpawnFinishedMessage>(OnObjectSpawnFinished);
				RegisterHandler<EntityStateMessage>(OnEntityStateMessage);
			}
			RegisterHandler<RpcMessage>(OnRPCMessage);
		}

		public static void Connect(string address)
		{
			RegisterSystemHandlers(false);
			Transport.activeTransport.enabled = true;
			AddTransportHandlers();
			connectState = ConnectState.Connecting;
			Transport.activeTransport.ClientConnect(address);
			connection = new NetworkConnectionToServer();
		}

		public static void Connect(Uri uri)
		{
			RegisterSystemHandlers(false);
			Transport.activeTransport.enabled = true;
			AddTransportHandlers();
			connectState = ConnectState.Connecting;
			Transport.activeTransport.ClientConnect(uri);
			connection = new NetworkConnectionToServer();
		}

		public static void ConnectHost()
		{
			RegisterSystemHandlers(true);
			connectState = ConnectState.Connected;
			LocalConnectionToServer localConnectionToServer = new LocalConnectionToServer();
			LocalConnectionToClient localConnectionToClient = (localConnectionToServer.connectionToClient = new LocalConnectionToClient());
			localConnectionToClient.connectionToServer = localConnectionToServer;
			connection = localConnectionToServer;
			NetworkServer.SetLocalConnection(localConnectionToClient);
		}

		public static void ConnectLocalServer()
		{
			NetworkServer.OnConnected(NetworkServer.localConnection);
			((LocalConnectionToServer)connection).QueueConnectedEvent();
		}

		public static void Disconnect()
		{
			if (connectState == ConnectState.Connecting || connectState == ConnectState.Connected)
			{
				connectState = ConnectState.Disconnecting;
				ready = false;
				NetworkConnection networkConnection = connection;
				if (networkConnection != null)
				{
					networkConnection.Disconnect();
				}
			}
		}

		[Obsolete("Call NetworkClient.Disconnect() instead. Nobody should use DisconnectLocalServer.")]
		public static void DisconnectLocalServer()
		{
			if (NetworkServer.localConnection != null)
			{
				NetworkServer.OnTransportDisconnected(NetworkServer.localConnection.connectionId);
			}
		}

		private static void OnTransportConnected()
		{
			if (connection != null)
			{
				NetworkTime.Reset();
				unbatcher = new Unbatcher();
				connectState = ConnectState.Connected;
				NetworkTime.UpdateClient();
				Action onConnectedEvent = OnConnectedEvent;
				if (onConnectedEvent != null)
				{
					onConnectedEvent();
				}
			}
			else
			{
				Debug.LogError("Skipped Connect message handling because connection is null.");
			}
		}

		private static bool UnpackAndInvoke(NetworkReader reader, int channelId)
		{
			ushort msgType;
			if (MessagePacking.Unpack(reader, out msgType))
			{
				NetworkMessageDelegate value;
				if (handlers.TryGetValue(msgType, out value))
				{
					value(connection, reader, channelId);
					if (connection != null)
					{
						connection.lastMessageTime = Time.time;
					}
					return true;
				}
				return false;
			}
			NetworkConnection networkConnection = connection;
			Debug.LogError("Closed connection: " + ((networkConnection != null) ? networkConnection.ToString() : null) + ". Invalid message header.");
			connection.Disconnect();
			return false;
		}

		internal static void OnTransportData(ArraySegment<byte> data, int channelId)
		{
			if (connection != null)
			{
				if (!unbatcher.AddBatch(data))
				{
					Debug.LogWarning("NetworkClient: failed to add batch, disconnecting.");
					connection.Disconnect();
					return;
				}
				NetworkReader message;
				double remoteTimeStamp;
				while (!isLoadingScene && unbatcher.GetNextMessage(out message, out remoteTimeStamp))
				{
					if (message.Remaining >= 2)
					{
						connection.remoteTimeStamp = remoteTimeStamp;
						if (!UnpackAndInvoke(message, channelId))
						{
							break;
						}
						continue;
					}
					Debug.LogError("NetworkClient: received Message was too short (messages should start with message id)");
					connection.Disconnect();
					break;
				}
			}
			else
			{
				Debug.LogError("Skipped Data message handling because connection is null.");
			}
		}

		internal static void OnTransportDisconnected()
		{
			if (connectState == ConnectState.Disconnected)
			{
				return;
			}
			if (connection != null)
			{
				Action onDisconnectedEvent = OnDisconnectedEvent;
				if (onDisconnectedEvent != null)
				{
					onDisconnectedEvent();
				}
			}
			connectState = ConnectState.Disconnected;
			ready = false;
			connection = null;
		}

		private static void OnError(Exception exception)
		{
			Debug.LogException(exception);
			Action<Exception> onErrorEvent = OnErrorEvent;
			if (onErrorEvent != null)
			{
				onErrorEvent(exception);
			}
		}

		public static void Send<T>(T message, int channelId = 0) where T : struct, NetworkMessage
		{
			if (connection != null)
			{
				if (connectState == ConnectState.Connected)
				{
					connection.Send(message, channelId);
				}
				else
				{
					Debug.LogError("NetworkClient Send when not connected to a server");
				}
			}
			else
			{
				Debug.LogError("NetworkClient Send with no connection");
			}
		}

		[Obsolete("Use RegisterHandler<T> version without NetworkConnection parameter. It always points to NetworkClient.connection anyway.")]
		public static void RegisterHandler<T>(Action<NetworkConnection, T> handler, bool requireAuthentication = true) where T : struct, NetworkMessage
		{
			ushort id = MessagePacking.GetId<T>();
			if (handlers.ContainsKey(id))
			{
				Debug.LogWarning(string.Format("NetworkClient.RegisterHandler replacing handler for {0}, id={1}. If replacement is intentional, use ReplaceHandler instead to avoid this warning.", typeof(T).FullName, id));
			}
			handlers[id] = MessagePacking.WrapHandler(handler, requireAuthentication);
		}

		public static void RegisterHandler<T>(Action<T> handler, bool requireAuthentication = true) where T : struct, NetworkMessage
		{
			_003C_003Ec__DisplayClass51_0<T> _003C_003Ec__DisplayClass51_ = new _003C_003Ec__DisplayClass51_0<T>();
			_003C_003Ec__DisplayClass51_.handler = handler;
			ushort id = MessagePacking.GetId<T>();
			if (handlers.ContainsKey(id))
			{
				Debug.LogWarning(string.Format("NetworkClient.RegisterHandler replacing handler for {0}, id={1}. If replacement is intentional, use ReplaceHandler instead to avoid this warning.", typeof(T).FullName, id));
			}
			handlers[id] = MessagePacking.WrapHandler<T, NetworkConnection>(_003C_003Ec__DisplayClass51_._003CRegisterHandler_003Eg__HandlerWrapped_007C0, requireAuthentication);
		}

		public static void ReplaceHandler<T>(Action<NetworkConnection, T> handler, bool requireAuthentication = true) where T : struct, NetworkMessage
		{
			ushort id = MessagePacking.GetId<T>();
			handlers[id] = MessagePacking.WrapHandler(handler, requireAuthentication);
		}

		public static void ReplaceHandler<T>(Action<T> handler, bool requireAuthentication = true) where T : struct, NetworkMessage
		{
			_003C_003Ec__DisplayClass53_0<T> _003C_003Ec__DisplayClass53_ = new _003C_003Ec__DisplayClass53_0<T>();
			_003C_003Ec__DisplayClass53_.handler = handler;
			ReplaceHandler<T>(_003C_003Ec__DisplayClass53_._003CReplaceHandler_003Eb__0, requireAuthentication);
		}

		public static bool UnregisterHandler<T>() where T : struct, NetworkMessage
		{
			ushort id = MessagePacking.GetId<T>();
			return handlers.Remove(id);
		}

		public static bool GetPrefab(Guid assetId, out GameObject prefab)
		{
			prefab = null;
			if (assetId != Guid.Empty && prefabs.TryGetValue(assetId, out prefab))
			{
				return prefab != null;
			}
			return false;
		}

		private static void RegisterPrefabIdentity(NetworkIdentity prefab)
		{
			if (prefab.assetId == Guid.Empty)
			{
				Debug.LogError("Can not Register '" + prefab.name + "' because it had empty assetid. If this is a scene Object use RegisterSpawnHandler instead");
				return;
			}
			if (prefab.sceneId != 0L)
			{
				Debug.LogError("Can not Register '" + prefab.name + "' because it has a sceneId, make sure you are passing in the original prefab and not an instance in the scene.");
				return;
			}
			if (prefab.GetComponentsInChildren<NetworkIdentity>().Length > 1)
			{
				Debug.LogError("Prefab '" + prefab.name + "' has multiple NetworkIdentity components. There should only be one NetworkIdentity on a prefab, and it must be on the root object.");
			}
			if (prefabs.ContainsKey(prefab.assetId))
			{
				GameObject gameObject = prefabs[prefab.assetId];
				Debug.LogWarning(string.Format("Replacing existing prefab with assetId '{0}'. Old prefab '{1}', New prefab '{2}'", prefab.assetId, gameObject.name, prefab.name));
			}
			if (spawnHandlers.ContainsKey(prefab.assetId) || unspawnHandlers.ContainsKey(prefab.assetId))
			{
				Debug.LogWarning(string.Format("Adding prefab '{0}' with assetId '{1}' when spawnHandlers with same assetId already exists.", prefab.name, prefab.assetId));
			}
			prefabs[prefab.assetId] = prefab.gameObject;
		}

		public static void RegisterPrefab(GameObject prefab, Guid newAssetId)
		{
			if (prefab == null)
			{
				Debug.LogError("Could not register prefab because it was null");
				return;
			}
			if (newAssetId == Guid.Empty)
			{
				Debug.LogError("Could not register '" + prefab.name + "' with new assetId because the new assetId was empty");
				return;
			}
			NetworkIdentity component = prefab.GetComponent<NetworkIdentity>();
			if (component == null)
			{
				Debug.LogError("Could not register '" + prefab.name + "' since it contains no NetworkIdentity component");
				return;
			}
			if (component.assetId != Guid.Empty && component.assetId != newAssetId)
			{
				Debug.LogError(string.Format("Could not register '{0}' to {1} because it already had an AssetId, Existing assetId {2}", prefab.name, newAssetId, component.assetId));
				return;
			}
			component.assetId = newAssetId;
			RegisterPrefabIdentity(component);
		}

		public static void RegisterPrefab(GameObject prefab)
		{
			if (prefab == null)
			{
				Debug.LogError("Could not register prefab because it was null");
				return;
			}
			NetworkIdentity component = prefab.GetComponent<NetworkIdentity>();
			if (component == null)
			{
				Debug.LogError("Could not register '" + prefab.name + "' since it contains no NetworkIdentity component");
			}
			else
			{
				RegisterPrefabIdentity(component);
			}
		}

		public static void RegisterPrefab(GameObject prefab, Guid newAssetId, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
		{
			_003C_003Ec__DisplayClass59_0 _003C_003Ec__DisplayClass59_ = new _003C_003Ec__DisplayClass59_0();
			_003C_003Ec__DisplayClass59_.spawnHandler = spawnHandler;
			if (_003C_003Ec__DisplayClass59_.spawnHandler == null)
			{
				Debug.LogError(string.Format("Can not Register null SpawnHandler for {0}", newAssetId));
			}
			else
			{
				RegisterPrefab(prefab, newAssetId, _003C_003Ec__DisplayClass59_._003CRegisterPrefab_003Eb__0, unspawnHandler);
			}
		}

		public static void RegisterPrefab(GameObject prefab, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
		{
			_003C_003Ec__DisplayClass60_0 _003C_003Ec__DisplayClass60_ = new _003C_003Ec__DisplayClass60_0();
			_003C_003Ec__DisplayClass60_.spawnHandler = spawnHandler;
			if (prefab == null)
			{
				Debug.LogError("Could not register handler for prefab because the prefab was null");
				return;
			}
			NetworkIdentity component = prefab.GetComponent<NetworkIdentity>();
			if (component == null)
			{
				Debug.LogError("Could not register handler for '" + prefab.name + "' since it contains no NetworkIdentity component");
				return;
			}
			if (component.sceneId != 0L)
			{
				Debug.LogError("Can not Register '" + prefab.name + "' because it has a sceneId, make sure you are passing in the original prefab and not an instance in the scene.");
				return;
			}
			Guid assetId = component.assetId;
			if (assetId == Guid.Empty)
			{
				Debug.LogError("Can not Register handler for '" + prefab.name + "' because it had empty assetid. If this is a scene Object use RegisterSpawnHandler instead");
			}
			else if (_003C_003Ec__DisplayClass60_.spawnHandler == null)
			{
				Debug.LogError(string.Format("Can not Register null SpawnHandler for {0}", assetId));
			}
			else
			{
				RegisterPrefab(prefab, _003C_003Ec__DisplayClass60_._003CRegisterPrefab_003Eb__0, unspawnHandler);
			}
		}

		public static void RegisterPrefab(GameObject prefab, Guid newAssetId, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
		{
			if (newAssetId == Guid.Empty)
			{
				Debug.LogError("Could not register handler for '" + prefab.name + "' with new assetId because the new assetId was empty");
				return;
			}
			if (prefab == null)
			{
				Debug.LogError("Could not register handler for prefab because the prefab was null");
				return;
			}
			NetworkIdentity component = prefab.GetComponent<NetworkIdentity>();
			if (component == null)
			{
				Debug.LogError("Could not register handler for '" + prefab.name + "' since it contains no NetworkIdentity component");
				return;
			}
			if (component.assetId != Guid.Empty && component.assetId != newAssetId)
			{
				Debug.LogError(string.Format("Could not register Handler for '{0}' to {1} because it already had an AssetId, Existing assetId {2}", prefab.name, newAssetId, component.assetId));
				return;
			}
			if (component.sceneId != 0L)
			{
				Debug.LogError("Can not Register '" + prefab.name + "' because it has a sceneId, make sure you are passing in the original prefab and not an instance in the scene.");
				return;
			}
			component.assetId = newAssetId;
			Guid assetId = component.assetId;
			if (spawnHandler == null)
			{
				Debug.LogError(string.Format("Can not Register null SpawnHandler for {0}", assetId));
				return;
			}
			if (unspawnHandler == null)
			{
				Debug.LogError(string.Format("Can not Register null UnSpawnHandler for {0}", assetId));
				return;
			}
			if (spawnHandlers.ContainsKey(assetId) || unspawnHandlers.ContainsKey(assetId))
			{
				Debug.LogWarning(string.Format("Replacing existing spawnHandlers for prefab '{0}' with assetId '{1}'", prefab.name, assetId));
			}
			if (prefabs.ContainsKey(assetId))
			{
				Debug.LogError(string.Format("assetId '{0}' is already used by prefab '{1}', unregister the prefab first before trying to add handler", assetId, prefabs[assetId].name));
			}
			if (prefab.GetComponentsInChildren<NetworkIdentity>().Length > 1)
			{
				Debug.LogError("Prefab '" + prefab.name + "' has multiple NetworkIdentity components. There should only be one NetworkIdentity on a prefab, and it must be on the root object.");
			}
			spawnHandlers[assetId] = spawnHandler;
			unspawnHandlers[assetId] = unspawnHandler;
		}

		public static void RegisterPrefab(GameObject prefab, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
		{
			if (prefab == null)
			{
				Debug.LogError("Could not register handler for prefab because the prefab was null");
				return;
			}
			NetworkIdentity component = prefab.GetComponent<NetworkIdentity>();
			if (component == null)
			{
				Debug.LogError("Could not register handler for '" + prefab.name + "' since it contains no NetworkIdentity component");
				return;
			}
			if (component.sceneId != 0L)
			{
				Debug.LogError("Can not Register '" + prefab.name + "' because it has a sceneId, make sure you are passing in the original prefab and not an instance in the scene.");
				return;
			}
			Guid assetId = component.assetId;
			if (assetId == Guid.Empty)
			{
				Debug.LogError("Can not Register handler for '" + prefab.name + "' because it had empty assetid. If this is a scene Object use RegisterSpawnHandler instead");
				return;
			}
			if (spawnHandler == null)
			{
				Debug.LogError(string.Format("Can not Register null SpawnHandler for {0}", assetId));
				return;
			}
			if (unspawnHandler == null)
			{
				Debug.LogError(string.Format("Can not Register null UnSpawnHandler for {0}", assetId));
				return;
			}
			if (spawnHandlers.ContainsKey(assetId) || unspawnHandlers.ContainsKey(assetId))
			{
				Debug.LogWarning(string.Format("Replacing existing spawnHandlers for prefab '{0}' with assetId '{1}'", prefab.name, assetId));
			}
			if (prefabs.ContainsKey(assetId))
			{
				Debug.LogError(string.Format("assetId '{0}' is already used by prefab '{1}', unregister the prefab first before trying to add handler", assetId, prefabs[assetId].name));
			}
			if (prefab.GetComponentsInChildren<NetworkIdentity>().Length > 1)
			{
				Debug.LogError("Prefab '" + prefab.name + "' has multiple NetworkIdentity components. There should only be one NetworkIdentity on a prefab, and it must be on the root object.");
			}
			spawnHandlers[assetId] = spawnHandler;
			unspawnHandlers[assetId] = unspawnHandler;
		}

		public static void UnregisterPrefab(GameObject prefab)
		{
			if (prefab == null)
			{
				Debug.LogError("Could not unregister prefab because it was null");
				return;
			}
			NetworkIdentity component = prefab.GetComponent<NetworkIdentity>();
			if (component == null)
			{
				Debug.LogError("Could not unregister '" + prefab.name + "' since it contains no NetworkIdentity component");
				return;
			}
			Guid assetId = component.assetId;
			prefabs.Remove(assetId);
			spawnHandlers.Remove(assetId);
			unspawnHandlers.Remove(assetId);
		}

		public static void RegisterSpawnHandler(Guid assetId, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
		{
			_003C_003Ec__DisplayClass64_0 _003C_003Ec__DisplayClass64_ = new _003C_003Ec__DisplayClass64_0();
			_003C_003Ec__DisplayClass64_.spawnHandler = spawnHandler;
			if (_003C_003Ec__DisplayClass64_.spawnHandler == null)
			{
				Debug.LogError(string.Format("Can not Register null SpawnHandler for {0}", assetId));
			}
			else
			{
				RegisterSpawnHandler(assetId, _003C_003Ec__DisplayClass64_._003CRegisterSpawnHandler_003Eb__0, unspawnHandler);
			}
		}

		public static void RegisterSpawnHandler(Guid assetId, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
		{
			if (spawnHandler == null)
			{
				Debug.LogError(string.Format("Can not Register null SpawnHandler for {0}", assetId));
				return;
			}
			if (unspawnHandler == null)
			{
				Debug.LogError(string.Format("Can not Register null UnSpawnHandler for {0}", assetId));
				return;
			}
			if (assetId == Guid.Empty)
			{
				Debug.LogError("Can not Register SpawnHandler for empty Guid");
				return;
			}
			if (spawnHandlers.ContainsKey(assetId) || unspawnHandlers.ContainsKey(assetId))
			{
				Debug.LogWarning(string.Format("Replacing existing spawnHandlers for {0}", assetId));
			}
			if (prefabs.ContainsKey(assetId))
			{
				Debug.LogError(string.Format("assetId '{0}' is already used by prefab '{1}'", assetId, prefabs[assetId].name));
			}
			spawnHandlers[assetId] = spawnHandler;
			unspawnHandlers[assetId] = unspawnHandler;
		}

		public static void UnregisterSpawnHandler(Guid assetId)
		{
			spawnHandlers.Remove(assetId);
			unspawnHandlers.Remove(assetId);
		}

		public static void ClearSpawners()
		{
			prefabs.Clear();
			spawnHandlers.Clear();
			unspawnHandlers.Clear();
		}

		internal static bool InvokeUnSpawnHandler(Guid assetId, GameObject obj)
		{
			UnSpawnDelegate value;
			if (unspawnHandlers.TryGetValue(assetId, out value) && value != null)
			{
				value(obj);
				return true;
			}
			return false;
		}

		public static bool Ready()
		{
			if (ready)
			{
				Debug.LogError("NetworkClient is already ready. It shouldn't be called twice.");
				return false;
			}
			if (connection == null)
			{
				Debug.LogError("Ready() called with invalid connection object: conn=null");
				return false;
			}
			ready = true;
			connection.isReady = true;
			connection.Send(default(ReadyMessage));
			return true;
		}

		[Obsolete("NetworkClient.Ready doesn't need a NetworkConnection parameter anymore. It always uses NetworkClient.connection anyway.")]
		public static bool Ready(NetworkConnection conn)
		{
			return Ready();
		}

		internal static void InternalAddPlayer(NetworkIdentity identity)
		{
			localPlayer = identity;
			if (ready && connection != null)
			{
				connection.identity = identity;
			}
			else
			{
				Debug.LogWarning("No ready connection found for setting player controller during InternalAddPlayer");
			}
		}

		public static bool AddPlayer()
		{
			if (connection == null)
			{
				Debug.LogError("AddPlayer requires a valid NetworkClient.connection.");
				return false;
			}
			if (!ready)
			{
				Debug.LogError("AddPlayer requires a ready NetworkClient.");
				return false;
			}
			if (connection.identity != null)
			{
				Debug.LogError("NetworkClient.AddPlayer: a PlayerController was already added. Did you call AddPlayer twice?");
				return false;
			}
			connection.Send(default(AddPlayerMessage));
			return true;
		}

		[Obsolete("NetworkClient.AddPlayer doesn't need a NetworkConnection parameter anymore. It always uses NetworkClient.connection anyway.")]
		public static bool AddPlayer(NetworkConnection readyConn)
		{
			return AddPlayer();
		}

		internal static void ApplySpawnPayload(NetworkIdentity identity, SpawnMessage message)
		{
			if (message.assetId != Guid.Empty)
			{
				identity.assetId = message.assetId;
			}
			if (!identity.gameObject.activeSelf)
			{
				identity.gameObject.SetActive(true);
			}
			identity.transform.localPosition = message.position;
			identity.transform.localRotation = message.rotation;
			identity.transform.localScale = message.scale;
			identity.hasAuthority = message.isOwner;
			identity.netId = message.netId;
			if (message.isLocalPlayer)
			{
				InternalAddPlayer(identity);
			}
			if (message.payload.Count > 0)
			{
				using (PooledNetworkReader reader = NetworkReaderPool.GetReader(message.payload))
				{
					identity.OnDeserializeAllSafely(reader, true);
				}
			}
			NetworkIdentity.spawned[message.netId] = identity;
			if (isSpawnFinished)
			{
				identity.NotifyAuthority();
				identity.OnStartClient();
				CheckForLocalPlayer(identity);
			}
		}

		internal static bool FindOrSpawnObject(SpawnMessage message, out NetworkIdentity identity)
		{
			identity = GetExistingObject(message.netId);
			if (identity != null)
			{
				return true;
			}
			if (message.assetId == Guid.Empty && message.sceneId == 0L)
			{
				Debug.LogError(string.Format("OnSpawn message with netId '{0}' has no AssetId or sceneId", message.netId));
				return false;
			}
			identity = ((message.sceneId == 0L) ? SpawnPrefab(message) : SpawnSceneObject(message));
			if (identity == null)
			{
				Debug.LogError(string.Format("Could not spawn assetId={0} scene={1:X} netId={2}", message.assetId, message.sceneId, message.netId));
				return false;
			}
			return true;
		}

		private static NetworkIdentity GetExistingObject(uint netid)
		{
			NetworkIdentity value;
			NetworkIdentity.spawned.TryGetValue(netid, out value);
			return value;
		}

		private static NetworkIdentity SpawnPrefab(SpawnMessage message)
		{
			GameObject prefab;
			if (GetPrefab(message.assetId, out prefab))
			{
				return UnityEngine.Object.Instantiate(prefab, message.position, message.rotation).GetComponent<NetworkIdentity>();
			}
			SpawnHandlerDelegate value;
			if (spawnHandlers.TryGetValue(message.assetId, out value))
			{
				GameObject gameObject = value(message);
				if (gameObject == null)
				{
					Debug.LogError(string.Format("Spawn Handler returned null, Handler assetId '{0}'", message.assetId));
					return null;
				}
				NetworkIdentity component = gameObject.GetComponent<NetworkIdentity>();
				if (component == null)
				{
					Debug.LogError(string.Format("Object Spawned by handler did not have a NetworkIdentity, Handler assetId '{0}'", message.assetId));
					return null;
				}
				return component;
			}
			Debug.LogError(string.Format("Failed to spawn server object, did you forget to add it to the NetworkManager? assetId={0} netId={1}", message.assetId, message.netId));
			return null;
		}

		private static NetworkIdentity SpawnSceneObject(SpawnMessage message)
		{
			NetworkIdentity andRemoveSceneObject = GetAndRemoveSceneObject(message.sceneId);
			if (andRemoveSceneObject == null)
			{
				Debug.LogError(string.Format("Spawn scene object not found for {0:X}. Make sure that client and server use exactly the same project. This only happens if the hierarchy gets out of sync.", message.sceneId));
			}
			return andRemoveSceneObject;
		}

		private static NetworkIdentity GetAndRemoveSceneObject(ulong sceneId)
		{
			NetworkIdentity value;
			if (spawnableObjects.TryGetValue(sceneId, out value))
			{
				spawnableObjects.Remove(sceneId);
				return value;
			}
			return null;
		}

		private static bool ConsiderForSpawning(NetworkIdentity identity)
		{
			if (!identity.gameObject.activeSelf && identity.gameObject.hideFlags != HideFlags.NotEditable && identity.gameObject.hideFlags != HideFlags.HideAndDontSave)
			{
				return identity.sceneId != 0;
			}
			return false;
		}

		public static void PrepareToSpawnSceneObjects()
		{
			spawnableObjects.Clear();
			NetworkIdentity[] array = Resources.FindObjectsOfTypeAll<NetworkIdentity>();
			foreach (NetworkIdentity networkIdentity in array)
			{
				if (ConsiderForSpawning(networkIdentity))
				{
					spawnableObjects.Add(networkIdentity.sceneId, networkIdentity);
				}
			}
		}

		internal static void OnObjectSpawnStarted(ObjectSpawnStartedMessage _)
		{
			PrepareToSpawnSceneObjects();
			isSpawnFinished = false;
		}

		internal static void OnObjectSpawnFinished(ObjectSpawnFinishedMessage _)
		{
			ClearNullFromSpawned();
			foreach (NetworkIdentity item in NetworkIdentity.spawned.Values.OrderBy(_003C_003Ec._003C_003E9__83_0 ?? (_003C_003Ec._003C_003E9__83_0 = _003C_003Ec._003C_003E9._003COnObjectSpawnFinished_003Eb__83_0)))
			{
				item.NotifyAuthority();
				item.OnStartClient();
				CheckForLocalPlayer(item);
			}
			isSpawnFinished = true;
		}

		private static void ClearNullFromSpawned()
		{
			foreach (KeyValuePair<uint, NetworkIdentity> item in NetworkIdentity.spawned)
			{
				if (item.Value == null)
				{
					removeFromSpawned.Add(item.Key);
				}
			}
			foreach (uint item2 in removeFromSpawned)
			{
				NetworkIdentity.spawned.Remove(item2);
			}
			removeFromSpawned.Clear();
		}

		private static void OnHostClientObjectDestroy(ObjectDestroyMessage message)
		{
			NetworkIdentity.spawned.Remove(message.netId);
		}

		private static void OnHostClientObjectHide(ObjectHideMessage message)
		{
			NetworkIdentity value;
			if (NetworkIdentity.spawned.TryGetValue(message.netId, out value) && value != null)
			{
				if (value.visibility != null)
				{
					value.visibility.OnSetHostVisibility(false);
				}
				else if (aoi != null)
				{
					aoi.SetHostVisibility(value, false);
				}
			}
		}

		internal static void OnHostClientSpawn(SpawnMessage message)
		{
			NetworkIdentity value;
			if (NetworkIdentity.spawned.TryGetValue(message.netId, out value) && value != null)
			{
				if (message.isLocalPlayer)
				{
					InternalAddPlayer(value);
				}
				value.hasAuthority = message.isOwner;
				value.NotifyAuthority();
				value.OnStartClient();
				if (value.visibility != null)
				{
					value.visibility.OnSetHostVisibility(true);
				}
				else if (aoi != null)
				{
					aoi.SetHostVisibility(value, true);
				}
				CheckForLocalPlayer(value);
			}
		}

		private static void OnEntityStateMessage(EntityStateMessage message)
		{
			NetworkIdentity value;
			if (NetworkIdentity.spawned.TryGetValue(message.netId, out value) && value != null)
			{
				using (PooledNetworkReader reader = NetworkReaderPool.GetReader(message.payload))
				{
					value.OnDeserializeAllSafely(reader, false);
					return;
				}
			}
			Debug.LogWarning("Did not find target for sync message for " + message.netId + " . Note: this can be completely normal because UDP messages may arrive out of order, so this message might have arrived after a Destroy message.");
		}

		private static void OnRPCMessage(RpcMessage message)
		{
			NetworkIdentity value;
			if (NetworkIdentity.spawned.TryGetValue(message.netId, out value))
			{
				using (PooledNetworkReader reader = NetworkReaderPool.GetReader(message.payload))
				{
					value.HandleRemoteCall(message.componentIndex, message.functionHash, MirrorInvokeType.ClientRpc, reader);
				}
			}
		}

		private static void OnObjectHide(ObjectHideMessage message)
		{
			DestroyObject(message.netId);
		}

		internal static void OnObjectDestroy(ObjectDestroyMessage message)
		{
			DestroyObject(message.netId);
		}

		internal static void OnSpawn(SpawnMessage message)
		{
			NetworkIdentity identity;
			if (FindOrSpawnObject(message, out identity))
			{
				ApplySpawnPayload(identity, message);
			}
		}

		internal static void CheckForLocalPlayer(NetworkIdentity identity)
		{
			if (identity == localPlayer)
			{
				identity.connectionToServer = connection;
				identity.OnStartLocalPlayer();
			}
		}

		private static void DestroyObject(uint netId)
		{
			NetworkIdentity value;
			if (NetworkIdentity.spawned.TryGetValue(netId, out value) && value != null)
			{
				value.OnStopClient();
				if (InvokeUnSpawnHandler(value.assetId, value.gameObject))
				{
					value.Reset();
				}
				else if (value.sceneId == 0L)
				{
					UnityEngine.Object.Destroy(value.gameObject);
				}
				else
				{
					value.gameObject.SetActive(false);
					spawnableObjects[value.sceneId] = value;
					value.Reset();
				}
				NetworkIdentity.spawned.Remove(netId);
			}
		}

		internal static void NetworkEarlyUpdate()
		{
			if (Transport.activeTransport != null)
			{
				Transport.activeTransport.ClientEarlyUpdate();
			}
		}

		internal static void NetworkLateUpdate()
		{
			LocalConnectionToServer localConnectionToServer = connection as LocalConnectionToServer;
			if (localConnectionToServer != null)
			{
				localConnectionToServer.Update();
			}
			else
			{
				NetworkConnectionToServer networkConnectionToServer = connection as NetworkConnectionToServer;
				if (networkConnectionToServer != null && active && connectState == ConnectState.Connected)
				{
					NetworkTime.UpdateClient();
					networkConnectionToServer.Update();
				}
			}
			if (Transport.activeTransport != null)
			{
				Transport.activeTransport.ClientLateUpdate();
			}
		}

		[Obsolete("NetworkClient.Update is now called internally from our custom update loop. No need to call Update manually anymore.")]
		public static void Update()
		{
			NetworkLateUpdate();
		}

		public static void DestroyAllClientObjects()
		{
			try
			{
				foreach (NetworkIdentity value in NetworkIdentity.spawned.Values)
				{
					if (!(value != null) || !(value.gameObject != null))
					{
						continue;
					}
					value.OnStopClient();
					if (!InvokeUnSpawnHandler(value.assetId, value.gameObject))
					{
						if (value.sceneId != 0L)
						{
							value.Reset();
							value.gameObject.SetActive(false);
						}
						else
						{
							UnityEngine.Object.Destroy(value.gameObject);
						}
					}
				}
				NetworkIdentity.spawned.Clear();
			}
			catch (InvalidOperationException exception)
			{
				Debug.LogException(exception);
				Debug.LogError("Could not DestroyAllClientObjects because spawned list was modified during loop, make sure you are not modifying NetworkIdentity.spawned by calling NetworkServer.Destroy or NetworkServer.Spawn in OnDestroy or OnDisable.");
			}
		}

		public static void Shutdown()
		{
			ClearSpawners();
			spawnableObjects.Clear();
			ready = false;
			isSpawnFinished = false;
			DestroyAllClientObjects();
			connectState = ConnectState.None;
			handlers.Clear();
			if (Transport.activeTransport != null)
			{
				Transport.activeTransport.ClientDisconnect();
			}
			connection = null;
			OnConnectedEvent = null;
			OnDisconnectedEvent = null;
		}
	}
}
