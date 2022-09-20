using System.Runtime.InteropServices;
using Mirror.RemoteCalls;
using UnityEngine;

namespace Mirror.Experimental
{
	[AddComponentMenu("Network/Experimental/NetworkRigidbody")]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-rigidbody")]
	public class NetworkRigidbody : NetworkBehaviour
	{
		public class ClientSyncState
		{
			public float nextSyncTime;

			public Vector3 velocity;

			public Vector3 angularVelocity;

			public bool isKinematic;

			public bool useGravity;

			public float drag;

			public float angularDrag;
		}

		[Header("Settings")]
		[SerializeField]
		internal Rigidbody target;

		[Tooltip("Set to true if moves come from owner client, set to false if moves always come from server")]
		public bool clientAuthority;

		[Header("Velocity")]
		[Tooltip("Syncs Velocity every SyncInterval")]
		[SerializeField]
		private bool syncVelocity = true;

		[Tooltip("Set velocity to 0 each frame (only works if syncVelocity is false")]
		[SerializeField]
		private bool clearVelocity;

		[Tooltip("Only Syncs Value if distance between previous and current is great than sensitivity")]
		[SerializeField]
		private float velocitySensitivity = 0.1f;

		[Header("Angular Velocity")]
		[Tooltip("Syncs AngularVelocity every SyncInterval")]
		[SerializeField]
		private bool syncAngularVelocity = true;

		[Tooltip("Set angularVelocity to 0 each frame (only works if syncAngularVelocity is false")]
		[SerializeField]
		private bool clearAngularVelocity;

		[Tooltip("Only Syncs Value if distance between previous and current is great than sensitivity")]
		[SerializeField]
		private float angularVelocitySensitivity = 0.1f;

		private readonly ClientSyncState previousValue = new ClientSyncState();

		[SyncVar(hook = "OnVelocityChanged")]
		private Vector3 velocity;

		[SyncVar(hook = "OnAngularVelocityChanged")]
		private Vector3 angularVelocity;

		[SyncVar(hook = "OnIsKinematicChanged")]
		private bool isKinematic;

		[SyncVar(hook = "OnUseGravityChanged")]
		private bool useGravity;

		[SyncVar(hook = "OnuDragChanged")]
		private float drag;

		[SyncVar(hook = "OnAngularDragChanged")]
		private float angularDrag;

		private bool IgnoreSync
		{
			get
			{
				if (!base.isServer)
				{
					return ClientWithAuthority;
				}
				return true;
			}
		}

		private bool ClientWithAuthority
		{
			get
			{
				if (clientAuthority)
				{
					return base.hasAuthority;
				}
				return false;
			}
		}

		public Vector3 Networkvelocity
		{
			get
			{
				return velocity;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref velocity))
				{
					Vector3 _ = velocity;
					SetSyncVar(value, ref velocity, 1uL);
					if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
					{
						setSyncVarHookGuard(1uL, true);
						OnVelocityChanged(_, value);
						setSyncVarHookGuard(1uL, false);
					}
				}
			}
		}

		public Vector3 NetworkangularVelocity
		{
			get
			{
				return angularVelocity;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref angularVelocity))
				{
					Vector3 _ = angularVelocity;
					SetSyncVar(value, ref angularVelocity, 2uL);
					if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
					{
						setSyncVarHookGuard(2uL, true);
						OnAngularVelocityChanged(_, value);
						setSyncVarHookGuard(2uL, false);
					}
				}
			}
		}

		public bool NetworkisKinematic
		{
			get
			{
				return isKinematic;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref isKinematic))
				{
					bool _ = isKinematic;
					SetSyncVar(value, ref isKinematic, 4uL);
					if (NetworkServer.localClientActive && !getSyncVarHookGuard(4uL))
					{
						setSyncVarHookGuard(4uL, true);
						OnIsKinematicChanged(_, value);
						setSyncVarHookGuard(4uL, false);
					}
				}
			}
		}

		public bool NetworkuseGravity
		{
			get
			{
				return useGravity;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref useGravity))
				{
					bool _ = useGravity;
					SetSyncVar(value, ref useGravity, 8uL);
					if (NetworkServer.localClientActive && !getSyncVarHookGuard(8uL))
					{
						setSyncVarHookGuard(8uL, true);
						OnUseGravityChanged(_, value);
						setSyncVarHookGuard(8uL, false);
					}
				}
			}
		}

		public float Networkdrag
		{
			get
			{
				return drag;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref drag))
				{
					float _ = drag;
					SetSyncVar(value, ref drag, 16uL);
					if (NetworkServer.localClientActive && !getSyncVarHookGuard(16uL))
					{
						setSyncVarHookGuard(16uL, true);
						OnuDragChanged(_, value);
						setSyncVarHookGuard(16uL, false);
					}
				}
			}
		}

		public float NetworkangularDrag
		{
			get
			{
				return angularDrag;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref angularDrag))
				{
					float _ = angularDrag;
					SetSyncVar(value, ref angularDrag, 32uL);
					if (NetworkServer.localClientActive && !getSyncVarHookGuard(32uL))
					{
						setSyncVarHookGuard(32uL, true);
						OnAngularDragChanged(_, value);
						setSyncVarHookGuard(32uL, false);
					}
				}
			}
		}

		private void OnValidate()
		{
			if (target == null)
			{
				target = GetComponent<Rigidbody>();
			}
		}

		private void OnVelocityChanged(Vector3 _, Vector3 newValue)
		{
			if (!IgnoreSync)
			{
				target.velocity = newValue;
			}
		}

		private void OnAngularVelocityChanged(Vector3 _, Vector3 newValue)
		{
			if (!IgnoreSync)
			{
				target.angularVelocity = newValue;
			}
		}

		private void OnIsKinematicChanged(bool _, bool newValue)
		{
			if (!IgnoreSync)
			{
				target.isKinematic = newValue;
			}
		}

		private void OnUseGravityChanged(bool _, bool newValue)
		{
			if (!IgnoreSync)
			{
				target.useGravity = newValue;
			}
		}

		private void OnuDragChanged(float _, float newValue)
		{
			if (!IgnoreSync)
			{
				target.drag = newValue;
			}
		}

		private void OnAngularDragChanged(float _, float newValue)
		{
			if (!IgnoreSync)
			{
				target.angularDrag = newValue;
			}
		}

		internal void Update()
		{
			if (base.isServer)
			{
				SyncToClients();
			}
			else if (ClientWithAuthority)
			{
				SendToServer();
			}
		}

		internal void FixedUpdate()
		{
			if (clearAngularVelocity && !syncAngularVelocity)
			{
				target.angularVelocity = Vector3.zero;
			}
			if (clearVelocity && !syncVelocity)
			{
				target.velocity = Vector3.zero;
			}
		}

		[Server]
		private void SyncToClients()
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void Mirror.Experimental.NetworkRigidbody::SyncToClients()' called when server was not active");
				return;
			}
			Vector3 vector = (syncVelocity ? target.velocity : default(Vector3));
			Vector3 vector2 = (syncAngularVelocity ? target.angularVelocity : default(Vector3));
			bool num = syncVelocity && (previousValue.velocity - vector).sqrMagnitude > velocitySensitivity * velocitySensitivity;
			bool flag = syncAngularVelocity && (previousValue.angularVelocity - vector2).sqrMagnitude > angularVelocitySensitivity * angularVelocitySensitivity;
			if (num)
			{
				Networkvelocity = vector;
				previousValue.velocity = vector;
			}
			if (flag)
			{
				NetworkangularVelocity = vector2;
				previousValue.angularVelocity = vector2;
			}
			NetworkisKinematic = target.isKinematic;
			NetworkuseGravity = target.useGravity;
			Networkdrag = target.drag;
			NetworkangularDrag = target.angularDrag;
		}

		[Client]
		private void SendToServer()
		{
			if (!NetworkClient.active)
			{
				Debug.LogWarning("[Client] function 'System.Void Mirror.Experimental.NetworkRigidbody::SendToServer()' called when client was not active");
				return;
			}
			if (!base.hasAuthority)
			{
				Debug.LogWarning("SendToServer called without authority");
				return;
			}
			SendVelocity();
			SendRigidBodySettings();
		}

		[Client]
		private void SendVelocity()
		{
			if (!NetworkClient.active)
			{
				Debug.LogWarning("[Client] function 'System.Void Mirror.Experimental.NetworkRigidbody::SendVelocity()' called when client was not active");
				return;
			}
			float time = Time.time;
			if (time < previousValue.nextSyncTime)
			{
				return;
			}
			Vector3 vector = (syncVelocity ? target.velocity : default(Vector3));
			Vector3 vector2 = (syncAngularVelocity ? target.angularVelocity : default(Vector3));
			bool flag = syncVelocity && (previousValue.velocity - vector).sqrMagnitude > velocitySensitivity * velocitySensitivity;
			int num;
			if (syncAngularVelocity)
			{
				num = (((previousValue.angularVelocity - vector2).sqrMagnitude > angularVelocitySensitivity * angularVelocitySensitivity) ? 1 : 0);
				if (num != 0)
				{
					CmdSendVelocityAndAngular(vector, vector2);
					previousValue.velocity = vector;
					previousValue.angularVelocity = vector2;
					goto IL_010e;
				}
			}
			else
			{
				num = 0;
			}
			if (flag)
			{
				CmdSendVelocity(vector);
				previousValue.velocity = vector;
			}
			goto IL_010e;
			IL_010e:
			if (((uint)num | (flag ? 1u : 0u)) != 0)
			{
				previousValue.nextSyncTime = time + syncInterval;
			}
		}

		[Client]
		private void SendRigidBodySettings()
		{
			if (!NetworkClient.active)
			{
				Debug.LogWarning("[Client] function 'System.Void Mirror.Experimental.NetworkRigidbody::SendRigidBodySettings()' called when client was not active");
				return;
			}
			if (previousValue.isKinematic != target.isKinematic)
			{
				CmdSendIsKinematic(target.isKinematic);
				previousValue.isKinematic = target.isKinematic;
			}
			if (previousValue.useGravity != target.useGravity)
			{
				CmdSendUseGravity(target.useGravity);
				previousValue.useGravity = target.useGravity;
			}
			if (previousValue.drag != target.drag)
			{
				CmdSendDrag(target.drag);
				previousValue.drag = target.drag;
			}
			if (previousValue.angularDrag != target.angularDrag)
			{
				CmdSendAngularDrag(target.angularDrag);
				previousValue.angularDrag = target.angularDrag;
			}
		}

		[Command]
		private void CmdSendVelocity(Vector3 velocity)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteVector3(velocity);
			SendCommandInternal(typeof(NetworkRigidbody), "CmdSendVelocity", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		private void CmdSendVelocityAndAngular(Vector3 velocity, Vector3 angularVelocity)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteVector3(velocity);
			writer.WriteVector3(angularVelocity);
			SendCommandInternal(typeof(NetworkRigidbody), "CmdSendVelocityAndAngular", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		private void CmdSendIsKinematic(bool isKinematic)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteBool(isKinematic);
			SendCommandInternal(typeof(NetworkRigidbody), "CmdSendIsKinematic", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		private void CmdSendUseGravity(bool useGravity)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteBool(useGravity);
			SendCommandInternal(typeof(NetworkRigidbody), "CmdSendUseGravity", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		private void CmdSendDrag(float drag)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteFloat(drag);
			SendCommandInternal(typeof(NetworkRigidbody), "CmdSendDrag", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		private void CmdSendAngularDrag(float angularDrag)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteFloat(angularDrag);
			SendCommandInternal(typeof(NetworkRigidbody), "CmdSendAngularDrag", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		private void MirrorProcessed()
		{
		}

		protected void UserCode_CmdSendVelocity(Vector3 velocity)
		{
			if (clientAuthority)
			{
				Networkvelocity = velocity;
				target.velocity = velocity;
			}
		}

		protected static void InvokeUserCode_CmdSendVelocity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdSendVelocity called on client.");
			}
			else
			{
				((NetworkRigidbody)obj).UserCode_CmdSendVelocity(reader.ReadVector3());
			}
		}

		protected void UserCode_CmdSendVelocityAndAngular(Vector3 velocity, Vector3 angularVelocity)
		{
			if (clientAuthority)
			{
				if (syncVelocity)
				{
					Networkvelocity = velocity;
					target.velocity = velocity;
				}
				NetworkangularVelocity = angularVelocity;
				target.angularVelocity = angularVelocity;
			}
		}

		protected static void InvokeUserCode_CmdSendVelocityAndAngular(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdSendVelocityAndAngular called on client.");
			}
			else
			{
				((NetworkRigidbody)obj).UserCode_CmdSendVelocityAndAngular(reader.ReadVector3(), reader.ReadVector3());
			}
		}

		protected void UserCode_CmdSendIsKinematic(bool isKinematic)
		{
			if (clientAuthority)
			{
				NetworkisKinematic = isKinematic;
				target.isKinematic = isKinematic;
			}
		}

		protected static void InvokeUserCode_CmdSendIsKinematic(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdSendIsKinematic called on client.");
			}
			else
			{
				((NetworkRigidbody)obj).UserCode_CmdSendIsKinematic(reader.ReadBool());
			}
		}

		protected void UserCode_CmdSendUseGravity(bool useGravity)
		{
			if (clientAuthority)
			{
				NetworkuseGravity = useGravity;
				target.useGravity = useGravity;
			}
		}

		protected static void InvokeUserCode_CmdSendUseGravity(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdSendUseGravity called on client.");
			}
			else
			{
				((NetworkRigidbody)obj).UserCode_CmdSendUseGravity(reader.ReadBool());
			}
		}

		protected void UserCode_CmdSendDrag(float drag)
		{
			if (clientAuthority)
			{
				Networkdrag = drag;
				target.drag = drag;
			}
		}

		protected static void InvokeUserCode_CmdSendDrag(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdSendDrag called on client.");
			}
			else
			{
				((NetworkRigidbody)obj).UserCode_CmdSendDrag(reader.ReadFloat());
			}
		}

		protected void UserCode_CmdSendAngularDrag(float angularDrag)
		{
			if (clientAuthority)
			{
				NetworkangularDrag = angularDrag;
				target.angularDrag = angularDrag;
			}
		}

		protected static void InvokeUserCode_CmdSendAngularDrag(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdSendAngularDrag called on client.");
			}
			else
			{
				((NetworkRigidbody)obj).UserCode_CmdSendAngularDrag(reader.ReadFloat());
			}
		}

		static NetworkRigidbody()
		{
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkRigidbody), "CmdSendVelocity", InvokeUserCode_CmdSendVelocity, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkRigidbody), "CmdSendVelocityAndAngular", InvokeUserCode_CmdSendVelocityAndAngular, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkRigidbody), "CmdSendIsKinematic", InvokeUserCode_CmdSendIsKinematic, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkRigidbody), "CmdSendUseGravity", InvokeUserCode_CmdSendUseGravity, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkRigidbody), "CmdSendDrag", InvokeUserCode_CmdSendDrag, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkRigidbody), "CmdSendAngularDrag", InvokeUserCode_CmdSendAngularDrag, true);
		}

		protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
		{
			bool result = base.SerializeSyncVars(writer, forceAll);
			if (forceAll)
			{
				writer.WriteVector3(velocity);
				writer.WriteVector3(angularVelocity);
				writer.WriteBool(isKinematic);
				writer.WriteBool(useGravity);
				writer.WriteFloat(drag);
				writer.WriteFloat(angularDrag);
				return true;
			}
			writer.WriteULong(base.syncVarDirtyBits);
			if ((base.syncVarDirtyBits & 1L) != 0L)
			{
				writer.WriteVector3(velocity);
				result = true;
			}
			if ((base.syncVarDirtyBits & 2L) != 0L)
			{
				writer.WriteVector3(angularVelocity);
				result = true;
			}
			if ((base.syncVarDirtyBits & 4L) != 0L)
			{
				writer.WriteBool(isKinematic);
				result = true;
			}
			if ((base.syncVarDirtyBits & 8L) != 0L)
			{
				writer.WriteBool(useGravity);
				result = true;
			}
			if ((base.syncVarDirtyBits & 0x10L) != 0L)
			{
				writer.WriteFloat(drag);
				result = true;
			}
			if ((base.syncVarDirtyBits & 0x20L) != 0L)
			{
				writer.WriteFloat(angularDrag);
				result = true;
			}
			return result;
		}

		protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
		{
			base.DeserializeSyncVars(reader, initialState);
			if (initialState)
			{
				Vector3 vector = velocity;
				Networkvelocity = reader.ReadVector3();
				if (!SyncVarEqual(vector, ref velocity))
				{
					OnVelocityChanged(vector, velocity);
				}
				Vector3 vector2 = angularVelocity;
				NetworkangularVelocity = reader.ReadVector3();
				if (!SyncVarEqual(vector2, ref angularVelocity))
				{
					OnAngularVelocityChanged(vector2, angularVelocity);
				}
				bool flag = isKinematic;
				NetworkisKinematic = reader.ReadBool();
				if (!SyncVarEqual(flag, ref isKinematic))
				{
					OnIsKinematicChanged(flag, isKinematic);
				}
				bool flag2 = useGravity;
				NetworkuseGravity = reader.ReadBool();
				if (!SyncVarEqual(flag2, ref useGravity))
				{
					OnUseGravityChanged(flag2, useGravity);
				}
				float num = drag;
				Networkdrag = reader.ReadFloat();
				if (!SyncVarEqual(num, ref drag))
				{
					OnuDragChanged(num, drag);
				}
				float num2 = angularDrag;
				NetworkangularDrag = reader.ReadFloat();
				if (!SyncVarEqual(num2, ref angularDrag))
				{
					OnAngularDragChanged(num2, angularDrag);
				}
				return;
			}
			long num3 = (long)reader.ReadULong();
			if ((num3 & 1L) != 0L)
			{
				Vector3 vector3 = velocity;
				Networkvelocity = reader.ReadVector3();
				if (!SyncVarEqual(vector3, ref velocity))
				{
					OnVelocityChanged(vector3, velocity);
				}
			}
			if ((num3 & 2L) != 0L)
			{
				Vector3 vector4 = angularVelocity;
				NetworkangularVelocity = reader.ReadVector3();
				if (!SyncVarEqual(vector4, ref angularVelocity))
				{
					OnAngularVelocityChanged(vector4, angularVelocity);
				}
			}
			if ((num3 & 4L) != 0L)
			{
				bool flag3 = isKinematic;
				NetworkisKinematic = reader.ReadBool();
				if (!SyncVarEqual(flag3, ref isKinematic))
				{
					OnIsKinematicChanged(flag3, isKinematic);
				}
			}
			if ((num3 & 8L) != 0L)
			{
				bool flag4 = useGravity;
				NetworkuseGravity = reader.ReadBool();
				if (!SyncVarEqual(flag4, ref useGravity))
				{
					OnUseGravityChanged(flag4, useGravity);
				}
			}
			if ((num3 & 0x10L) != 0L)
			{
				float num4 = drag;
				Networkdrag = reader.ReadFloat();
				if (!SyncVarEqual(num4, ref drag))
				{
					OnuDragChanged(num4, drag);
				}
			}
			if ((num3 & 0x20L) != 0L)
			{
				float num5 = angularDrag;
				NetworkangularDrag = reader.ReadFloat();
				if (!SyncVarEqual(num5, ref angularDrag))
				{
					OnAngularDragChanged(num5, angularDrag);
				}
			}
		}
	}
}
