using System.Runtime.InteropServices;
using Mirror.RemoteCalls;
using UnityEngine;

namespace Mirror.Experimental
{
	[AddComponentMenu("Network/Experimental/NetworkRigidbody2D")]
	public class NetworkRigidbody2D : NetworkBehaviour
	{
		public class ClientSyncState
		{
			public float nextSyncTime;

			public Vector2 velocity;

			public float angularVelocity;

			public bool isKinematic;

			public float gravityScale;

			public float drag;

			public float angularDrag;
		}

		[Header("Settings")]
		[SerializeField]
		internal Rigidbody2D target;

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
		private Vector2 velocity;

		[SyncVar(hook = "OnAngularVelocityChanged")]
		private float angularVelocity;

		[SyncVar(hook = "OnIsKinematicChanged")]
		private bool isKinematic;

		[SyncVar(hook = "OnGravityScaleChanged")]
		private float gravityScale;

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

		public Vector2 Networkvelocity
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
					Vector2 _ = velocity;
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

		public float NetworkangularVelocity
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
					float _ = angularVelocity;
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

		public float NetworkgravityScale
		{
			get
			{
				return gravityScale;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref gravityScale))
				{
					float _ = gravityScale;
					SetSyncVar(value, ref gravityScale, 8uL);
					if (NetworkServer.localClientActive && !getSyncVarHookGuard(8uL))
					{
						setSyncVarHookGuard(8uL, true);
						OnGravityScaleChanged(_, value);
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
				target = GetComponent<Rigidbody2D>();
			}
		}

		private void OnVelocityChanged(Vector2 _, Vector2 newValue)
		{
			if (!IgnoreSync)
			{
				target.velocity = newValue;
			}
		}

		private void OnAngularVelocityChanged(float _, float newValue)
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

		private void OnGravityScaleChanged(float _, float newValue)
		{
			if (!IgnoreSync)
			{
				target.gravityScale = newValue;
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
				target.angularVelocity = 0f;
			}
			if (clearVelocity && !syncVelocity)
			{
				target.velocity = Vector2.zero;
			}
		}

		[Server]
		private void SyncToClients()
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void Mirror.Experimental.NetworkRigidbody2D::SyncToClients()' called when server was not active");
				return;
			}
			Vector2 vector = (syncVelocity ? target.velocity : default(Vector2));
			float num = (syncAngularVelocity ? target.angularVelocity : 0f);
			bool num2 = syncVelocity && (previousValue.velocity - vector).sqrMagnitude > velocitySensitivity * velocitySensitivity;
			bool flag = syncAngularVelocity && previousValue.angularVelocity - num > angularVelocitySensitivity;
			if (num2)
			{
				Networkvelocity = vector;
				previousValue.velocity = vector;
			}
			if (flag)
			{
				NetworkangularVelocity = num;
				previousValue.angularVelocity = num;
			}
			NetworkisKinematic = target.isKinematic;
			NetworkgravityScale = target.gravityScale;
			Networkdrag = target.drag;
			NetworkangularDrag = target.angularDrag;
		}

		[Client]
		private void SendToServer()
		{
			if (!NetworkClient.active)
			{
				Debug.LogWarning("[Client] function 'System.Void Mirror.Experimental.NetworkRigidbody2D::SendToServer()' called when client was not active");
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
				Debug.LogWarning("[Client] function 'System.Void Mirror.Experimental.NetworkRigidbody2D::SendVelocity()' called when client was not active");
				return;
			}
			float time = Time.time;
			if (time < previousValue.nextSyncTime)
			{
				return;
			}
			Vector2 vector = (syncVelocity ? target.velocity : default(Vector2));
			float num = (syncAngularVelocity ? target.angularVelocity : 0f);
			bool flag = syncVelocity && (previousValue.velocity - vector).sqrMagnitude > velocitySensitivity * velocitySensitivity;
			int num2;
			if (syncAngularVelocity)
			{
				num2 = ((previousValue.angularVelocity != num) ? 1 : 0);
				if (num2 != 0)
				{
					CmdSendVelocityAndAngular(vector, num);
					previousValue.velocity = vector;
					previousValue.angularVelocity = num;
					goto IL_00f1;
				}
			}
			else
			{
				num2 = 0;
			}
			if (flag)
			{
				CmdSendVelocity(vector);
				previousValue.velocity = vector;
			}
			goto IL_00f1;
			IL_00f1:
			if (((uint)num2 | (flag ? 1u : 0u)) != 0)
			{
				previousValue.nextSyncTime = time + syncInterval;
			}
		}

		[Client]
		private void SendRigidBodySettings()
		{
			if (!NetworkClient.active)
			{
				Debug.LogWarning("[Client] function 'System.Void Mirror.Experimental.NetworkRigidbody2D::SendRigidBodySettings()' called when client was not active");
				return;
			}
			if (previousValue.isKinematic != target.isKinematic)
			{
				CmdSendIsKinematic(target.isKinematic);
				previousValue.isKinematic = target.isKinematic;
			}
			if (previousValue.gravityScale != target.gravityScale)
			{
				CmdChangeGravityScale(target.gravityScale);
				previousValue.gravityScale = target.gravityScale;
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
		private void CmdSendVelocity(Vector2 velocity)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteVector2(velocity);
			SendCommandInternal(typeof(NetworkRigidbody2D), "CmdSendVelocity", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		private void CmdSendVelocityAndAngular(Vector2 velocity, float angularVelocity)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteVector2(velocity);
			writer.WriteFloat(angularVelocity);
			SendCommandInternal(typeof(NetworkRigidbody2D), "CmdSendVelocityAndAngular", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		private void CmdSendIsKinematic(bool isKinematic)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteBool(isKinematic);
			SendCommandInternal(typeof(NetworkRigidbody2D), "CmdSendIsKinematic", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		private void CmdChangeGravityScale(float gravityScale)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteFloat(gravityScale);
			SendCommandInternal(typeof(NetworkRigidbody2D), "CmdChangeGravityScale", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		private void CmdSendDrag(float drag)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteFloat(drag);
			SendCommandInternal(typeof(NetworkRigidbody2D), "CmdSendDrag", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		private void CmdSendAngularDrag(float angularDrag)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteFloat(angularDrag);
			SendCommandInternal(typeof(NetworkRigidbody2D), "CmdSendAngularDrag", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		private void MirrorProcessed()
		{
		}

		protected void UserCode_CmdSendVelocity(Vector2 velocity)
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
				((NetworkRigidbody2D)obj).UserCode_CmdSendVelocity(reader.ReadVector2());
			}
		}

		protected void UserCode_CmdSendVelocityAndAngular(Vector2 velocity, float angularVelocity)
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
				((NetworkRigidbody2D)obj).UserCode_CmdSendVelocityAndAngular(reader.ReadVector2(), reader.ReadFloat());
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
				((NetworkRigidbody2D)obj).UserCode_CmdSendIsKinematic(reader.ReadBool());
			}
		}

		protected void UserCode_CmdChangeGravityScale(float gravityScale)
		{
			if (clientAuthority)
			{
				NetworkgravityScale = gravityScale;
				target.gravityScale = gravityScale;
			}
		}

		protected static void InvokeUserCode_CmdChangeGravityScale(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdChangeGravityScale called on client.");
			}
			else
			{
				((NetworkRigidbody2D)obj).UserCode_CmdChangeGravityScale(reader.ReadFloat());
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
				((NetworkRigidbody2D)obj).UserCode_CmdSendDrag(reader.ReadFloat());
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
				((NetworkRigidbody2D)obj).UserCode_CmdSendAngularDrag(reader.ReadFloat());
			}
		}

		static NetworkRigidbody2D()
		{
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkRigidbody2D), "CmdSendVelocity", InvokeUserCode_CmdSendVelocity, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkRigidbody2D), "CmdSendVelocityAndAngular", InvokeUserCode_CmdSendVelocityAndAngular, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkRigidbody2D), "CmdSendIsKinematic", InvokeUserCode_CmdSendIsKinematic, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkRigidbody2D), "CmdChangeGravityScale", InvokeUserCode_CmdChangeGravityScale, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkRigidbody2D), "CmdSendDrag", InvokeUserCode_CmdSendDrag, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkRigidbody2D), "CmdSendAngularDrag", InvokeUserCode_CmdSendAngularDrag, true);
		}

		protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
		{
			bool result = base.SerializeSyncVars(writer, forceAll);
			if (forceAll)
			{
				writer.WriteVector2(velocity);
				writer.WriteFloat(angularVelocity);
				writer.WriteBool(isKinematic);
				writer.WriteFloat(gravityScale);
				writer.WriteFloat(drag);
				writer.WriteFloat(angularDrag);
				return true;
			}
			writer.WriteULong(base.syncVarDirtyBits);
			if ((base.syncVarDirtyBits & 1L) != 0L)
			{
				writer.WriteVector2(velocity);
				result = true;
			}
			if ((base.syncVarDirtyBits & 2L) != 0L)
			{
				writer.WriteFloat(angularVelocity);
				result = true;
			}
			if ((base.syncVarDirtyBits & 4L) != 0L)
			{
				writer.WriteBool(isKinematic);
				result = true;
			}
			if ((base.syncVarDirtyBits & 8L) != 0L)
			{
				writer.WriteFloat(gravityScale);
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
				Vector2 vector = velocity;
				Networkvelocity = reader.ReadVector2();
				if (!SyncVarEqual(vector, ref velocity))
				{
					OnVelocityChanged(vector, velocity);
				}
				float num = angularVelocity;
				NetworkangularVelocity = reader.ReadFloat();
				if (!SyncVarEqual(num, ref angularVelocity))
				{
					OnAngularVelocityChanged(num, angularVelocity);
				}
				bool flag = isKinematic;
				NetworkisKinematic = reader.ReadBool();
				if (!SyncVarEqual(flag, ref isKinematic))
				{
					OnIsKinematicChanged(flag, isKinematic);
				}
				float num2 = gravityScale;
				NetworkgravityScale = reader.ReadFloat();
				if (!SyncVarEqual(num2, ref gravityScale))
				{
					OnGravityScaleChanged(num2, gravityScale);
				}
				float num3 = drag;
				Networkdrag = reader.ReadFloat();
				if (!SyncVarEqual(num3, ref drag))
				{
					OnuDragChanged(num3, drag);
				}
				float num4 = angularDrag;
				NetworkangularDrag = reader.ReadFloat();
				if (!SyncVarEqual(num4, ref angularDrag))
				{
					OnAngularDragChanged(num4, angularDrag);
				}
				return;
			}
			long num5 = (long)reader.ReadULong();
			if ((num5 & 1L) != 0L)
			{
				Vector2 vector2 = velocity;
				Networkvelocity = reader.ReadVector2();
				if (!SyncVarEqual(vector2, ref velocity))
				{
					OnVelocityChanged(vector2, velocity);
				}
			}
			if ((num5 & 2L) != 0L)
			{
				float num6 = angularVelocity;
				NetworkangularVelocity = reader.ReadFloat();
				if (!SyncVarEqual(num6, ref angularVelocity))
				{
					OnAngularVelocityChanged(num6, angularVelocity);
				}
			}
			if ((num5 & 4L) != 0L)
			{
				bool flag2 = isKinematic;
				NetworkisKinematic = reader.ReadBool();
				if (!SyncVarEqual(flag2, ref isKinematic))
				{
					OnIsKinematicChanged(flag2, isKinematic);
				}
			}
			if ((num5 & 8L) != 0L)
			{
				float num7 = gravityScale;
				NetworkgravityScale = reader.ReadFloat();
				if (!SyncVarEqual(num7, ref gravityScale))
				{
					OnGravityScaleChanged(num7, gravityScale);
				}
			}
			if ((num5 & 0x10L) != 0L)
			{
				float num8 = drag;
				Networkdrag = reader.ReadFloat();
				if (!SyncVarEqual(num8, ref drag))
				{
					OnuDragChanged(num8, drag);
				}
			}
			if ((num5 & 0x20L) != 0L)
			{
				float num9 = angularDrag;
				NetworkangularDrag = reader.ReadFloat();
				if (!SyncVarEqual(num9, ref angularDrag))
				{
					OnAngularDragChanged(num9, angularDrag);
				}
			}
		}
	}
}
