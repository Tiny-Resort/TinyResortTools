using System;
using System.Collections.Generic;
using Mirror.RemoteCalls;
using UnityEngine;

namespace Mirror
{
	[AddComponentMenu("")]
	[RequireComponent(typeof(NetworkIdentity))]
	[HelpURL("https://mirror-networking.gitbook.io/docs/guides/networkbehaviour")]
	public abstract class NetworkBehaviour : MonoBehaviour
	{
		public struct NetworkBehaviourSyncVar : IEquatable<NetworkBehaviourSyncVar>
		{
			public uint netId;

			public byte componentIndex;

			public NetworkBehaviourSyncVar(uint netId, int componentIndex)
			{
				this = default(NetworkBehaviourSyncVar);
				this.netId = netId;
				this.componentIndex = (byte)componentIndex;
			}

			public bool Equals(NetworkBehaviourSyncVar other)
			{
				if (other.netId == netId)
				{
					return other.componentIndex == componentIndex;
				}
				return false;
			}

			public bool Equals(uint netId, int componentIndex)
			{
				if (this.netId == netId)
				{
					return this.componentIndex == componentIndex;
				}
				return false;
			}

			public override string ToString()
			{
				return string.Format("[netId:{0} compIndex:{1}]", netId, componentIndex);
			}
		}

		[Tooltip("By default synced data is sent from the server to all Observers of the object.\nChange this to Owner to only have the server update the client that has ownership authority for this object")]
		[HideInInspector]
		public SyncMode syncMode;

		[Tooltip("Time in seconds until next change is synchronized to the client. '0' means send immediately if changed. '0.5' means only send changes every 500ms.\n(This is for state synchronization like SyncVars, SyncLists, OnSerialize. Not for Cmds, Rpcs, etc.)")]
		[Range(0f, 2f)]
		[HideInInspector]
		public float syncInterval = 0.1f;

		internal double lastSyncTime;

		private ulong syncVarHookGuard;

		protected readonly List<SyncObject> syncObjects = new List<SyncObject>();

		public bool isServer
		{
			get
			{
				return netIdentity.isServer;
			}
		}

		public bool isClient
		{
			get
			{
				return netIdentity.isClient;
			}
		}

		public bool isLocalPlayer
		{
			get
			{
				return netIdentity.isLocalPlayer;
			}
		}

		public bool isServerOnly
		{
			get
			{
				return netIdentity.isServerOnly;
			}
		}

		public bool isClientOnly
		{
			get
			{
				return netIdentity.isClientOnly;
			}
		}

		public bool hasAuthority
		{
			get
			{
				return netIdentity.hasAuthority;
			}
		}

		public uint netId
		{
			get
			{
				return netIdentity.netId;
			}
		}

		public NetworkConnection connectionToServer
		{
			get
			{
				return netIdentity.connectionToServer;
			}
		}

		public NetworkConnection connectionToClient
		{
			get
			{
				return netIdentity.connectionToClient;
			}
		}

		protected ulong syncVarDirtyBits { get; private set; }

		public NetworkIdentity netIdentity { get; internal set; }

		public int ComponentIndex { get; internal set; }

		protected bool getSyncVarHookGuard(ulong dirtyBit)
		{
			return (syncVarHookGuard & dirtyBit) != 0;
		}

		protected void setSyncVarHookGuard(ulong dirtyBit, bool value)
		{
			if (value)
			{
				syncVarHookGuard |= dirtyBit;
			}
			else
			{
				syncVarHookGuard &= ~dirtyBit;
			}
		}

		protected void InitSyncObject(SyncObject syncObject)
		{
			if (syncObject == null)
			{
				Debug.LogError("Uninitialized SyncObject. Manually call the constructor on your SyncList, SyncSet or SyncDictionary");
			}
			else
			{
				syncObjects.Add(syncObject);
			}
		}

		protected void SendCommandInternal(Type invokeClass, string cmdName, NetworkWriter writer, int channelId, bool requiresAuthority = true)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("Command Function " + cmdName + " called without an active client.");
				return;
			}
			if (requiresAuthority && !isLocalPlayer && !hasAuthority)
			{
				Debug.LogWarning(string.Format("Trying to send command for object without authority. {0}.{1}", invokeClass, cmdName));
				return;
			}
			if (!NetworkClient.ready)
			{
				Debug.LogError("Send command attempted while NetworkClient is not ready.");
				return;
			}
			if (NetworkClient.connection == null)
			{
				Debug.LogError("Send command attempted with no client running.");
				return;
			}
			CommandMessage commandMessage = default(CommandMessage);
			commandMessage.netId = netId;
			commandMessage.componentIndex = ComponentIndex;
			commandMessage.functionHash = RemoteCallHelper.GetMethodHash(invokeClass, cmdName);
			commandMessage.payload = writer.ToArraySegment();
			CommandMessage message = commandMessage;
			NetworkClient.connection.Send(message, channelId);
		}

		protected void SendRPCInternal(Type invokeClass, string rpcName, NetworkWriter writer, int channelId, bool includeOwner)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("RPC Function " + rpcName + " called on Client.");
				return;
			}
			if (!isServer)
			{
				Debug.LogWarning("ClientRpc " + rpcName + " called on un-spawned object: " + base.name);
				return;
			}
			RpcMessage rpcMessage = default(RpcMessage);
			rpcMessage.netId = netId;
			rpcMessage.componentIndex = ComponentIndex;
			rpcMessage.functionHash = RemoteCallHelper.GetMethodHash(invokeClass, rpcName);
			rpcMessage.payload = writer.ToArraySegment();
			RpcMessage message = rpcMessage;
			NetworkServer.SendToReady(netIdentity, message, includeOwner, channelId);
		}

		protected void SendTargetRPCInternal(NetworkConnection conn, Type invokeClass, string rpcName, NetworkWriter writer, int channelId)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("TargetRPC " + rpcName + " called when server not active");
				return;
			}
			if (!isServer)
			{
				Debug.LogWarning("TargetRpc " + rpcName + " called on " + base.name + " but that object has not been spawned or has been unspawned");
				return;
			}
			if (conn == null)
			{
				conn = connectionToClient;
			}
			if (conn == null)
			{
				Debug.LogError("TargetRPC " + rpcName + " was given a null connection, make sure the object has an owner or you pass in the target connection");
				return;
			}
			if (!(conn is NetworkConnectionToClient))
			{
				Debug.LogError("TargetRPC " + rpcName + " requires a NetworkConnectionToClient but was given " + conn.GetType().Name);
				return;
			}
			RpcMessage rpcMessage = default(RpcMessage);
			rpcMessage.netId = netId;
			rpcMessage.componentIndex = ComponentIndex;
			rpcMessage.functionHash = RemoteCallHelper.GetMethodHash(invokeClass, rpcName);
			rpcMessage.payload = writer.ToArraySegment();
			RpcMessage message = rpcMessage;
			conn.Send(message, channelId);
		}

		protected bool SyncVarGameObjectEqual(GameObject newGameObject, uint netIdField)
		{
			uint num = 0u;
			if (newGameObject != null)
			{
				NetworkIdentity component = newGameObject.GetComponent<NetworkIdentity>();
				if (component != null)
				{
					num = component.netId;
					if (num == 0)
					{
						Debug.LogWarning("SetSyncVarGameObject GameObject " + (((object)newGameObject != null) ? newGameObject.ToString() : null) + " has a zero netId. Maybe it is not spawned yet?");
					}
				}
			}
			return num == netIdField;
		}

		protected void SetSyncVarGameObject(GameObject newGameObject, ref GameObject gameObjectField, ulong dirtyBit, ref uint netIdField)
		{
			if (getSyncVarHookGuard(dirtyBit))
			{
				return;
			}
			uint num = 0u;
			if (newGameObject != null)
			{
				NetworkIdentity component = newGameObject.GetComponent<NetworkIdentity>();
				if (component != null)
				{
					num = component.netId;
					if (num == 0)
					{
						Debug.LogWarning("SetSyncVarGameObject GameObject " + (((object)newGameObject != null) ? newGameObject.ToString() : null) + " has a zero netId. Maybe it is not spawned yet?");
					}
				}
			}
			SetDirtyBit(dirtyBit);
			gameObjectField = newGameObject;
			netIdField = num;
		}

		protected GameObject GetSyncVarGameObject(uint netId, ref GameObject gameObjectField)
		{
			if (isServer)
			{
				return gameObjectField;
			}
			NetworkIdentity value;
			if (NetworkIdentity.spawned.TryGetValue(netId, out value) && value != null)
			{
				return gameObjectField = value.gameObject;
			}
			return null;
		}

		protected bool SyncVarNetworkIdentityEqual(NetworkIdentity newIdentity, uint netIdField)
		{
			uint num = 0u;
			if (newIdentity != null)
			{
				num = newIdentity.netId;
				if (num == 0)
				{
					Debug.LogWarning("SetSyncVarNetworkIdentity NetworkIdentity " + (((object)newIdentity != null) ? newIdentity.ToString() : null) + " has a zero netId. Maybe it is not spawned yet?");
				}
			}
			return num == netIdField;
		}

		protected void SetSyncVarNetworkIdentity(NetworkIdentity newIdentity, ref NetworkIdentity identityField, ulong dirtyBit, ref uint netIdField)
		{
			if (getSyncVarHookGuard(dirtyBit))
			{
				return;
			}
			uint num = 0u;
			if (newIdentity != null)
			{
				num = newIdentity.netId;
				if (num == 0)
				{
					Debug.LogWarning("SetSyncVarNetworkIdentity NetworkIdentity " + (((object)newIdentity != null) ? newIdentity.ToString() : null) + " has a zero netId. Maybe it is not spawned yet?");
				}
			}
			SetDirtyBit(dirtyBit);
			netIdField = num;
			identityField = newIdentity;
		}

		protected NetworkIdentity GetSyncVarNetworkIdentity(uint netId, ref NetworkIdentity identityField)
		{
			if (isServer)
			{
				return identityField;
			}
			NetworkIdentity.spawned.TryGetValue(netId, out identityField);
			return identityField;
		}

		protected bool SyncVarNetworkBehaviourEqual<T>(T newBehaviour, NetworkBehaviourSyncVar syncField) where T : NetworkBehaviour
		{
			uint num = 0u;
			int componentIndex = 0;
			if ((UnityEngine.Object)newBehaviour != (UnityEngine.Object)null)
			{
				num = newBehaviour.netId;
				componentIndex = newBehaviour.ComponentIndex;
				if (num == 0)
				{
					Debug.LogWarning("SetSyncVarNetworkIdentity NetworkIdentity " + ((newBehaviour != null) ? newBehaviour.ToString() : null) + " has a zero netId. Maybe it is not spawned yet?");
				}
			}
			return syncField.Equals(num, componentIndex);
		}

		protected void SetSyncVarNetworkBehaviour<T>(T newBehaviour, ref T behaviourField, ulong dirtyBit, ref NetworkBehaviourSyncVar syncField) where T : NetworkBehaviour
		{
			if (getSyncVarHookGuard(dirtyBit))
			{
				return;
			}
			uint num = 0u;
			int componentIndex = 0;
			if ((UnityEngine.Object)newBehaviour != (UnityEngine.Object)null)
			{
				num = newBehaviour.netId;
				componentIndex = newBehaviour.ComponentIndex;
				if (num == 0)
				{
					Debug.LogWarning("SetSyncVarNetworkBehaviour NetworkIdentity " + ((newBehaviour != null) ? newBehaviour.ToString() : null) + " has a zero netId. Maybe it is not spawned yet?");
				}
			}
			syncField = new NetworkBehaviourSyncVar(num, componentIndex);
			SetDirtyBit(dirtyBit);
			behaviourField = newBehaviour;
		}

		protected T GetSyncVarNetworkBehaviour<T>(NetworkBehaviourSyncVar syncNetBehaviour, ref T behaviourField) where T : NetworkBehaviour
		{
			if (isServer)
			{
				return behaviourField;
			}
			NetworkIdentity value;
			if (!NetworkIdentity.spawned.TryGetValue(syncNetBehaviour.netId, out value))
			{
				return null;
			}
			behaviourField = value.NetworkBehaviours[syncNetBehaviour.componentIndex] as T;
			return behaviourField;
		}

		protected bool SyncVarEqual<T>(T value, ref T fieldValue)
		{
			return EqualityComparer<T>.Default.Equals(value, fieldValue);
		}

		protected void SetSyncVar<T>(T value, ref T fieldValue, ulong dirtyBit)
		{
			SetDirtyBit(dirtyBit);
			fieldValue = value;
		}

		public void SetDirtyBit(ulong dirtyBit)
		{
			syncVarDirtyBits |= dirtyBit;
		}

		public void ClearAllDirtyBits()
		{
			lastSyncTime = NetworkTime.localTime;
			syncVarDirtyBits = 0uL;
			for (int i = 0; i < syncObjects.Count; i++)
			{
				syncObjects[i].Flush();
			}
		}

		private bool AnySyncObjectDirty()
		{
			for (int i = 0; i < syncObjects.Count; i++)
			{
				if (syncObjects[i].IsDirty)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsDirty()
		{
			if (NetworkTime.localTime - lastSyncTime >= (double)syncInterval)
			{
				if (syncVarDirtyBits == 0L)
				{
					return AnySyncObjectDirty();
				}
				return true;
			}
			return false;
		}

		public virtual bool OnSerialize(NetworkWriter writer, bool initialState)
		{
			bool flag = false;
			flag = ((!initialState) ? SerializeObjectsDelta(writer) : SerializeObjectsAll(writer));
			bool flag2 = SerializeSyncVars(writer, initialState);
			return flag || flag2;
		}

		public virtual void OnDeserialize(NetworkReader reader, bool initialState)
		{
			if (initialState)
			{
				DeSerializeObjectsAll(reader);
			}
			else
			{
				DeSerializeObjectsDelta(reader);
			}
			DeserializeSyncVars(reader, initialState);
		}

		protected virtual bool SerializeSyncVars(NetworkWriter writer, bool initialState)
		{
			return false;
		}

		protected virtual void DeserializeSyncVars(NetworkReader reader, bool initialState)
		{
		}

		internal ulong DirtyObjectBits()
		{
			ulong num = 0uL;
			for (int i = 0; i < syncObjects.Count; i++)
			{
				if (syncObjects[i].IsDirty)
				{
					num |= (ulong)(1L << i);
				}
			}
			return num;
		}

		public bool SerializeObjectsAll(NetworkWriter writer)
		{
			bool result = false;
			for (int i = 0; i < syncObjects.Count; i++)
			{
				syncObjects[i].OnSerializeAll(writer);
				result = true;
			}
			return result;
		}

		public bool SerializeObjectsDelta(NetworkWriter writer)
		{
			bool result = false;
			writer.WriteULong(DirtyObjectBits());
			for (int i = 0; i < syncObjects.Count; i++)
			{
				SyncObject syncObject = syncObjects[i];
				if (syncObject.IsDirty)
				{
					syncObject.OnSerializeDelta(writer);
					result = true;
				}
			}
			return result;
		}

		internal void DeSerializeObjectsAll(NetworkReader reader)
		{
			for (int i = 0; i < syncObjects.Count; i++)
			{
				syncObjects[i].OnDeserializeAll(reader);
			}
		}

		internal void DeSerializeObjectsDelta(NetworkReader reader)
		{
			ulong num = reader.ReadULong();
			for (int i = 0; i < syncObjects.Count; i++)
			{
				SyncObject syncObject = syncObjects[i];
				if ((num & (ulong)(1L << i)) != 0L)
				{
					syncObject.OnDeserializeDelta(reader);
				}
			}
		}

		internal void ResetSyncObjects()
		{
			foreach (SyncObject syncObject in syncObjects)
			{
				syncObject.Reset();
			}
		}

		public virtual void OnStartServer()
		{
		}

		public virtual void OnStopServer()
		{
		}

		public virtual void OnStartClient()
		{
		}

		public virtual void OnStopClient()
		{
		}

		public virtual void OnStartLocalPlayer()
		{
		}

		public virtual void OnStartAuthority()
		{
		}

		public virtual void OnStopAuthority()
		{
		}
	}
}
