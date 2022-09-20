using System.Runtime.InteropServices;
using Mirror.RemoteCalls;
using UnityEngine;

namespace Mirror.Experimental
{
	[AddComponentMenu("Network/Experimental/NetworkLerpRigidbody")]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-lerp-rigidbody")]
	public class NetworkLerpRigidbody : NetworkBehaviour
	{
		[Header("Settings")]
		[SerializeField]
		internal Rigidbody target;

		[Tooltip("How quickly current velocity approaches target velocity")]
		[SerializeField]
		private float lerpVelocityAmount = 0.5f;

		[Tooltip("How quickly current position approaches target position")]
		[SerializeField]
		private float lerpPositionAmount = 0.5f;

		[Tooltip("Set to true if moves come from owner client, set to false if moves always come from server")]
		[SerializeField]
		private bool clientAuthority;

		private float nextSyncTime;

		[SyncVar]
		private Vector3 targetVelocity;

		[SyncVar]
		private Vector3 targetPosition;

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

		public Vector3 NetworktargetVelocity
		{
			get
			{
				return targetVelocity;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref targetVelocity))
				{
					Vector3 vector = targetVelocity;
					SetSyncVar(value, ref targetVelocity, 1uL);
				}
			}
		}

		public Vector3 NetworktargetPosition
		{
			get
			{
				return targetPosition;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref targetPosition))
				{
					Vector3 vector = targetPosition;
					SetSyncVar(value, ref targetPosition, 2uL);
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

		private void Update()
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

		private void SyncToClients()
		{
			NetworktargetVelocity = target.velocity;
			NetworktargetPosition = target.position;
		}

		private void SendToServer()
		{
			float time = Time.time;
			if (time > nextSyncTime)
			{
				nextSyncTime = time + syncInterval;
				CmdSendState(target.velocity, target.position);
			}
		}

		[Command]
		private void CmdSendState(Vector3 velocity, Vector3 position)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteVector3(velocity);
			writer.WriteVector3(position);
			SendCommandInternal(typeof(NetworkLerpRigidbody), "CmdSendState", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		private void FixedUpdate()
		{
			if (!IgnoreSync)
			{
				target.velocity = Vector3.Lerp(target.velocity, targetVelocity, lerpVelocityAmount);
				target.position = Vector3.Lerp(target.position, targetPosition, lerpPositionAmount);
				NetworktargetPosition = targetPosition + target.velocity * Time.fixedDeltaTime;
			}
		}

		private void MirrorProcessed()
		{
		}

		protected void UserCode_CmdSendState(Vector3 velocity, Vector3 position)
		{
			target.velocity = velocity;
			target.position = position;
			NetworktargetVelocity = velocity;
			NetworktargetPosition = position;
		}

		protected static void InvokeUserCode_CmdSendState(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdSendState called on client.");
			}
			else
			{
				((NetworkLerpRigidbody)obj).UserCode_CmdSendState(reader.ReadVector3(), reader.ReadVector3());
			}
		}

		static NetworkLerpRigidbody()
		{
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkLerpRigidbody), "CmdSendState", InvokeUserCode_CmdSendState, true);
		}

		protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
		{
			bool result = base.SerializeSyncVars(writer, forceAll);
			if (forceAll)
			{
				writer.WriteVector3(targetVelocity);
				writer.WriteVector3(targetPosition);
				return true;
			}
			writer.WriteULong(base.syncVarDirtyBits);
			if ((base.syncVarDirtyBits & 1L) != 0L)
			{
				writer.WriteVector3(targetVelocity);
				result = true;
			}
			if ((base.syncVarDirtyBits & 2L) != 0L)
			{
				writer.WriteVector3(targetPosition);
				result = true;
			}
			return result;
		}

		protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
		{
			base.DeserializeSyncVars(reader, initialState);
			if (initialState)
			{
				Vector3 vector = targetVelocity;
				NetworktargetVelocity = reader.ReadVector3();
				Vector3 vector2 = targetPosition;
				NetworktargetPosition = reader.ReadVector3();
				return;
			}
			long num = (long)reader.ReadULong();
			if ((num & 1L) != 0L)
			{
				Vector3 vector3 = targetVelocity;
				NetworktargetVelocity = reader.ReadVector3();
			}
			if ((num & 2L) != 0L)
			{
				Vector3 vector4 = targetPosition;
				NetworktargetPosition = reader.ReadVector3();
			}
		}
	}
}
