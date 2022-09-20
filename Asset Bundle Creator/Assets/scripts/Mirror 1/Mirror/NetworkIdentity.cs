using System;
using System.Collections.Generic;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirror
{
	[DisallowMultipleComponent]
	[DefaultExecutionOrder(-1)]
	[AddComponentMenu("Network/NetworkIdentity")]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-identity")]
	public sealed class NetworkIdentity : MonoBehaviour
	{
		public delegate void ClientAuthorityCallback(NetworkConnection conn, NetworkIdentity identity, bool authorityState);

		public Dictionary<int, NetworkConnection> observers;

		[FormerlySerializedAs("m_SceneId")]
		[HideInInspector]
		public ulong sceneId;

		[FormerlySerializedAs("m_ServerOnly")]
		[Tooltip("Prevents this object from being spawned / enabled on clients")]
		public bool serverOnly;

		internal bool destroyCalled;

		private NetworkConnectionToClient _connectionToClient;

		public static readonly Dictionary<uint, NetworkIdentity> spawned = new Dictionary<uint, NetworkIdentity>();

		[Tooltip("Visibility can overwrite interest management. ForceHidden can be useful to hide monsters while they respawn. ForceShown can be useful for score NetworkIdentities that should always broadcast to everyone in the world.")]
		public Visibility visible;

		private NetworkIdentitySerialization lastSerialization = new NetworkIdentitySerialization
		{
			ownerWriter = new NetworkWriter(),
			observersWriter = new NetworkWriter()
		};

		[SerializeField]
		[HideInInspector]
		private string m_AssetId;

		private static readonly Dictionary<ulong, NetworkIdentity> sceneIds = new Dictionary<ulong, NetworkIdentity>();

		private static uint nextNetworkId = 1u;

		[SerializeField]
		[HideInInspector]
		private bool hasSpawned;

		private bool clientStarted;

		private static NetworkIdentity previousLocalPlayer = null;

		private bool hadAuthority;

		public bool isClient { get; internal set; }

		public bool isServer { get; internal set; }

		public bool isLocalPlayer { get; internal set; }

		public bool isServerOnly
		{
			get
			{
				if (isServer)
				{
					return !isClient;
				}
				return false;
			}
		}

		public bool isClientOnly
		{
			get
			{
				if (isClient)
				{
					return !isServer;
				}
				return false;
			}
		}

		public bool hasAuthority { get; internal set; }

		public uint netId { get; internal set; }

		public NetworkConnection connectionToServer { get; internal set; }

		public NetworkConnectionToClient connectionToClient
		{
			get
			{
				return _connectionToClient;
			}
			internal set
			{
				NetworkConnectionToClient networkConnectionToClient = _connectionToClient;
				if (networkConnectionToClient != null)
				{
					networkConnectionToClient.RemoveOwnedObject(this);
				}
				_connectionToClient = value;
				NetworkConnectionToClient networkConnectionToClient2 = _connectionToClient;
				if (networkConnectionToClient2 != null)
				{
					networkConnectionToClient2.AddOwnedObject(this);
				}
			}
		}

		public NetworkBehaviour[] NetworkBehaviours { get; private set; }

		[Obsolete("Per-NetworkIdentity Interest Management is being replaced by global Interest Management.\n\nWe already converted some components to the new system. For those, please remove Proximity checkers from NetworkIdentity prefabs and add one global InterestManagement component to your NetworkManager instead. If we didn't convert this one yet, then simply wait. See our Benchmark example and our Mirror/Components/InterestManagement for available implementations.\n\nIf you need to port a custom solution, move your code into a new class that inherits from InterestManagement and add one global update method instead of using NetworkBehaviour.Update.\n\nDon't panic. The whole change mostly moved code from NetworkVisibility components into one global place on NetworkManager. Allows for Spatial Hashing which is ~30x faster.\n\n(╯°□°)╯︵ ┻━┻")]
		public NetworkVisibility visibility { get; private set; }

		public Guid assetId
		{
			get
			{
				if (!string.IsNullOrEmpty(m_AssetId))
				{
					return new Guid(m_AssetId);
				}
				return Guid.Empty;
			}
			internal set
			{
				string text = ((value == Guid.Empty) ? string.Empty : value.ToString("N"));
				string text2 = m_AssetId;
				if (!(text2 == text))
				{
					if (string.IsNullOrEmpty(text))
					{
						Debug.LogError("Can not set AssetId to empty guid on NetworkIdentity '" + base.name + "', old assetId '" + text2 + "'");
					}
					else if (!string.IsNullOrEmpty(text2))
					{
						Debug.LogError("Can not Set AssetId on NetworkIdentity '" + base.name + "' because it already had an assetId, current assetId '" + text2 + "', attempted new assetId '" + text + "'");
					}
					else
					{
						m_AssetId = text;
					}
				}
			}
		}

		public bool SpawnedFromInstantiate { get; private set; }

		public static event ClientAuthorityCallback clientAuthorityCallback;

		public static NetworkIdentity GetSceneIdentity(ulong id)
		{
			return sceneIds[id];
		}

		internal void SetClientOwner(NetworkConnection conn)
		{
			if (connectionToClient != null && conn != connectionToClient)
			{
				Debug.LogError(string.Format("Object {0} netId={1} already has an owner. Use RemoveClientAuthority() first", this, netId), this);
			}
			else
			{
				connectionToClient = (NetworkConnectionToClient)conn;
			}
		}

		internal static uint GetNextNetworkId()
		{
			return nextNetworkId++;
		}

		public static void ResetNextNetworkId()
		{
			nextNetworkId = 1u;
		}

		internal void RemoveObserverInternal(NetworkConnection conn)
		{
			Dictionary<int, NetworkConnection> dictionary = observers;
			if (dictionary != null)
			{
				dictionary.Remove(conn.connectionId);
			}
		}

		internal void InitializeNetworkBehaviours()
		{
			NetworkBehaviours = GetComponents<NetworkBehaviour>();
			if (NetworkBehaviours.Length > 255)
			{
				Debug.LogError(string.Format("Only {0} NetworkBehaviour components are allowed for NetworkIdentity: {1} because we send the index as byte.", byte.MaxValue, base.name), this);
			}
			for (int i = 0; i < NetworkBehaviours.Length; i++)
			{
				NetworkBehaviour obj = NetworkBehaviours[i];
				obj.netIdentity = this;
				obj.ComponentIndex = i;
			}
		}

		internal void Awake()
		{
			InitializeNetworkBehaviours();
			visibility = GetComponent<NetworkVisibility>();
			if (hasSpawned)
			{
				Debug.LogError(base.name + " has already spawned. Don't call Instantiate for NetworkIdentities that were in the scene since the beginning (aka scene objects).  Otherwise the client won't know which object to use for a SpawnSceneObject message.");
				SpawnedFromInstantiate = true;
				UnityEngine.Object.Destroy(base.gameObject);
			}
			hasSpawned = true;
		}

		private void OnValidate()
		{
			hasSpawned = false;
		}

		private void OnDestroy()
		{
			if (!SpawnedFromInstantiate)
			{
				if (isServer && !destroyCalled)
				{
					NetworkServer.Destroy(base.gameObject);
				}
				if (isLocalPlayer && NetworkClient.localPlayer == this)
				{
					NetworkClient.localPlayer = null;
				}
			}
		}

		internal void OnStartServer()
		{
			if (isServer)
			{
				return;
			}
			isServer = true;
			if (NetworkClient.localPlayer == this)
			{
				isLocalPlayer = true;
			}
			if (netId != 0)
			{
				return;
			}
			netId = GetNextNetworkId();
			observers = new Dictionary<int, NetworkConnection>();
			spawned[netId] = this;
			if (NetworkClient.active)
			{
				isClient = true;
			}
			NetworkBehaviour[] networkBehaviours = NetworkBehaviours;
			foreach (NetworkBehaviour networkBehaviour in networkBehaviours)
			{
				try
				{
					networkBehaviour.OnStartServer();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, networkBehaviour);
				}
			}
		}

		internal void OnStopServer()
		{
			NetworkBehaviour[] networkBehaviours = NetworkBehaviours;
			foreach (NetworkBehaviour networkBehaviour in networkBehaviours)
			{
				try
				{
					networkBehaviour.OnStopServer();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, networkBehaviour);
				}
			}
		}

		internal void OnStartClient()
		{
			if (clientStarted)
			{
				return;
			}
			clientStarted = true;
			isClient = true;
			if (NetworkClient.localPlayer == this)
			{
				isLocalPlayer = true;
			}
			NetworkBehaviour[] networkBehaviours = NetworkBehaviours;
			foreach (NetworkBehaviour networkBehaviour in networkBehaviours)
			{
				try
				{
					networkBehaviour.OnStartClient();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, networkBehaviour);
				}
			}
		}

		internal void OnStopClient()
		{
			NetworkBehaviour[] networkBehaviours = NetworkBehaviours;
			foreach (NetworkBehaviour networkBehaviour in networkBehaviours)
			{
				try
				{
					networkBehaviour.OnStopClient();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, networkBehaviour);
				}
			}
		}

		internal void OnStartLocalPlayer()
		{
			if (previousLocalPlayer == this)
			{
				return;
			}
			previousLocalPlayer = this;
			isLocalPlayer = true;
			NetworkBehaviour[] networkBehaviours = NetworkBehaviours;
			foreach (NetworkBehaviour networkBehaviour in networkBehaviours)
			{
				try
				{
					networkBehaviour.OnStartLocalPlayer();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, networkBehaviour);
				}
			}
		}

		internal void NotifyAuthority()
		{
			if (!hadAuthority && hasAuthority)
			{
				OnStartAuthority();
			}
			if (hadAuthority && !hasAuthority)
			{
				OnStopAuthority();
			}
			hadAuthority = hasAuthority;
		}

		internal void OnStartAuthority()
		{
			NetworkBehaviour[] networkBehaviours = NetworkBehaviours;
			foreach (NetworkBehaviour networkBehaviour in networkBehaviours)
			{
				try
				{
					networkBehaviour.OnStartAuthority();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, networkBehaviour);
				}
			}
		}

		internal void OnStopAuthority()
		{
			NetworkBehaviour[] networkBehaviours = NetworkBehaviours;
			foreach (NetworkBehaviour networkBehaviour in networkBehaviours)
			{
				try
				{
					networkBehaviour.OnStopAuthority();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, networkBehaviour);
				}
			}
		}

		[Obsolete("Use NetworkServer.RebuildObservers(identity, initialize) instead.")]
		public void RebuildObservers(bool initialize)
		{
			NetworkServer.RebuildObservers(this, initialize);
		}

		private bool OnSerializeSafely(NetworkBehaviour comp, NetworkWriter writer, bool initialState)
		{
			int position = writer.Position;
			writer.WriteInt(0);
			int position2 = writer.Position;
			bool result = false;
			try
			{
				result = comp.OnSerialize(writer, initialState);
			}
			catch (Exception ex)
			{
				string[] obj = new string[8] { "OnSerialize failed for: object=", base.name, " component=", null, null, null, null, null };
				Type type = comp.GetType();
				obj[3] = (((object)type != null) ? type.ToString() : null);
				obj[4] = " sceneId=";
				obj[5] = sceneId.ToString("X");
				obj[6] = "\n\n";
				obj[7] = ((ex != null) ? ex.ToString() : null);
				Debug.LogError(string.Concat(obj));
			}
			int position3 = writer.Position;
			writer.Position = position;
			writer.WriteInt(position3 - position2);
			writer.Position = position3;
			return result;
		}

		internal void OnSerializeAllSafely(bool initialState, NetworkWriter ownerWriter, NetworkWriter observersWriter)
		{
			NetworkBehaviour[] networkBehaviours = NetworkBehaviours;
			if (networkBehaviours.Length > 255)
			{
				throw new IndexOutOfRangeException(string.Format("{0} has more than {1} components. This is not supported.", base.name, byte.MaxValue));
			}
			for (int i = 0; i < networkBehaviours.Length; i++)
			{
				NetworkBehaviour networkBehaviour = networkBehaviours[i];
				if (initialState || networkBehaviour.IsDirty())
				{
					int position = ownerWriter.Position;
					ownerWriter.WriteByte((byte)i);
					OnSerializeSafely(networkBehaviour, ownerWriter, initialState);
					if (networkBehaviour.syncMode == SyncMode.Observers)
					{
						ArraySegment<byte> arraySegment = ownerWriter.ToArraySegment();
						int count = ownerWriter.Position - position;
						observersWriter.WriteBytes(arraySegment.Array, position, count);
					}
				}
			}
		}

		internal NetworkIdentitySerialization GetSerializationAtTick(int tick)
		{
			if (lastSerialization.tick != tick)
			{
				lastSerialization.ownerWriter.Position = 0;
				lastSerialization.observersWriter.Position = 0;
				OnSerializeAllSafely(false, lastSerialization.ownerWriter, lastSerialization.observersWriter);
				lastSerialization.tick = tick;
			}
			return lastSerialization;
		}

		private void OnDeserializeSafely(NetworkBehaviour comp, NetworkReader reader, bool initialState)
		{
			int num = reader.ReadInt();
			int position = reader.Position;
			int num2 = reader.Position + num;
			try
			{
				comp.OnDeserialize(reader, initialState);
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Format("OnDeserialize failed Exception={0} (see below) object={1} component={2} sceneId={3:X} length={4}. Possible Reasons:\n", ex.GetType(), base.name, comp.GetType(), sceneId, num) + string.Format("  * Do {0}'s OnSerialize and OnDeserialize calls write the same amount of data({1} bytes)? \n", comp.GetType(), num) + string.Format("  * Was there an exception in {0}'s OnSerialize/OnDeserialize code?\n", comp.GetType()) + "  * Are the server and client the exact same project?\n  * Maybe this OnDeserialize call was meant for another GameObject? The sceneIds can easily get out of sync if the Hierarchy was modified only in the client OR the server. Try rebuilding both.\n\n" + string.Format("Exception {0}", ex));
			}
			if (reader.Position != num2)
			{
				int num3 = reader.Position - position;
				string[] obj = new string[11]
				{
					"OnDeserialize was expected to read ",
					num.ToString(),
					" instead of ",
					num3.ToString(),
					" bytes for object:",
					base.name,
					" component=",
					null,
					null,
					null,
					null
				};
				Type type = comp.GetType();
				obj[7] = (((object)type != null) ? type.ToString() : null);
				obj[8] = " sceneId=";
				obj[9] = sceneId.ToString("X");
				obj[10] = ". Make sure that OnSerialize and OnDeserialize write/read the same amount of data in all cases.";
				Debug.LogWarning(string.Concat(obj));
				reader.Position = num2;
			}
		}

		internal void OnDeserializeAllSafely(NetworkReader reader, bool initialState)
		{
			if (NetworkBehaviours == null)
			{
				Debug.LogError("NetworkBehaviours array is null on " + base.gameObject.name + "!\nTypically this can happen when a networked object is a child of a non-networked parent that's disabled, preventing Awake on the networked object from being invoked, where the NetworkBehaviours array is initialized.", base.gameObject);
				return;
			}
			NetworkBehaviour[] networkBehaviours = NetworkBehaviours;
			while (reader.Remaining > 0)
			{
				byte b = reader.ReadByte();
				if (b < networkBehaviours.Length)
				{
					OnDeserializeSafely(networkBehaviours[b], reader, initialState);
				}
			}
		}

		internal void HandleRemoteCall(int componentIndex, int functionHash, MirrorInvokeType invokeType, NetworkReader reader, NetworkConnectionToClient senderConnection = null)
		{
			if (this == null)
			{
				Debug.LogWarning(string.Format("{0} [{1}] received for deleted object [netId={2}]", invokeType, functionHash, netId));
				return;
			}
			if (componentIndex < 0 || componentIndex >= NetworkBehaviours.Length)
			{
				Debug.LogWarning(string.Format("Component [{0}] not found for [netId={1}]", componentIndex, netId));
				return;
			}
			NetworkBehaviour invokingType = NetworkBehaviours[componentIndex];
			if (!RemoteCallHelper.InvokeHandlerDelegate(functionHash, invokeType, reader, invokingType, senderConnection))
			{
				Debug.LogError(string.Format("Found no receiver for incoming {0} [{1}] on {2}, the server and client should have the same NetworkBehaviour instances [netId={3}].", invokeType, functionHash, base.gameObject.name, netId));
			}
		}

		internal CommandInfo GetCommandInfo(int componentIndex, int cmdHash)
		{
			if (this == null)
			{
				return default(CommandInfo);
			}
			if (0 <= componentIndex && componentIndex < NetworkBehaviours.Length)
			{
				NetworkBehaviour invokingType = NetworkBehaviours[componentIndex];
				return RemoteCallHelper.GetCommandInfo(cmdHash, invokingType);
			}
			return default(CommandInfo);
		}

		internal void ClearObservers()
		{
			if (observers == null)
			{
				return;
			}
			foreach (NetworkConnection value in observers.Values)
			{
				value.RemoveFromObserving(this, true);
			}
			observers.Clear();
		}

		internal void AddObserver(NetworkConnection conn)
		{
			if (observers == null)
			{
				GameObject obj = base.gameObject;
				Debug.LogError("AddObserver for " + (((object)obj != null) ? obj.ToString() : null) + " observer list is null");
			}
			else if (!observers.ContainsKey(conn.connectionId))
			{
				observers[conn.connectionId] = conn;
				conn.AddToObserving(this);
			}
		}

		public bool AssignClientAuthority(NetworkConnection conn)
		{
			if (!isServer)
			{
				Debug.LogError("AssignClientAuthority can only be called on the server for spawned objects.");
				return false;
			}
			if (conn == null)
			{
				GameObject obj = base.gameObject;
				Debug.LogError("AssignClientAuthority for " + (((object)obj != null) ? obj.ToString() : null) + " owner cannot be null. Use RemoveClientAuthority() instead.");
				return false;
			}
			if (connectionToClient != null && conn != connectionToClient)
			{
				GameObject obj2 = base.gameObject;
				Debug.LogError("AssignClientAuthority for " + (((object)obj2 != null) ? obj2.ToString() : null) + " already has an owner. Use RemoveClientAuthority() first.");
				return false;
			}
			SetClientOwner(conn);
			NetworkServer.SendSpawnMessage(this, conn);
			ClientAuthorityCallback obj3 = NetworkIdentity.clientAuthorityCallback;
			if (obj3 != null)
			{
				obj3(conn, this, true);
			}
			return true;
		}

		public void RemoveClientAuthority()
		{
			if (!isServer)
			{
				Debug.LogError("RemoveClientAuthority can only be called on the server for spawned objects.");
				return;
			}
			NetworkConnectionToClient networkConnectionToClient = connectionToClient;
			if (((networkConnectionToClient != null) ? networkConnectionToClient.identity : null) == this)
			{
				Debug.LogError("RemoveClientAuthority cannot remove authority for a player object");
			}
			else if (connectionToClient != null)
			{
				ClientAuthorityCallback obj = NetworkIdentity.clientAuthorityCallback;
				if (obj != null)
				{
					obj(connectionToClient, this, false);
				}
				NetworkConnectionToClient conn = connectionToClient;
				connectionToClient = null;
				NetworkServer.SendSpawnMessage(this, conn);
				connectionToClient = null;
			}
		}

		internal void Reset()
		{
			ResetSyncObjects();
			hasSpawned = false;
			clientStarted = false;
			isClient = false;
			isServer = false;
			netId = 0u;
			connectionToServer = null;
			connectionToClient = null;
			ClearObservers();
			if (isLocalPlayer && NetworkClient.localPlayer == this)
			{
				NetworkClient.localPlayer = null;
			}
			isLocalPlayer = false;
		}

		internal void ClearAllComponentsDirtyBits()
		{
			NetworkBehaviour[] networkBehaviours = NetworkBehaviours;
			for (int i = 0; i < networkBehaviours.Length; i++)
			{
				networkBehaviours[i].ClearAllDirtyBits();
			}
		}

		internal void ClearDirtyComponentsDirtyBits()
		{
			NetworkBehaviour[] networkBehaviours = NetworkBehaviours;
			foreach (NetworkBehaviour networkBehaviour in networkBehaviours)
			{
				if (networkBehaviour.IsDirty())
				{
					networkBehaviour.ClearAllDirtyBits();
				}
			}
		}

		private void ResetSyncObjects()
		{
			if (NetworkBehaviours != null)
			{
				NetworkBehaviour[] networkBehaviours = NetworkBehaviours;
				for (int i = 0; i < networkBehaviours.Length; i++)
				{
					networkBehaviours[i].ResetSyncObjects();
				}
			}
		}
	}
}
