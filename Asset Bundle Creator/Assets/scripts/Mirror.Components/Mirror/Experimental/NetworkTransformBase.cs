using System;
using System.Runtime.InteropServices;
using Mirror.RemoteCalls;
using UnityEngine;

namespace Mirror.Experimental
{
	public abstract class NetworkTransformBase : NetworkBehaviour
	{
		[Serializable]
		public struct DataPoint
		{
			public float timeStamp;

			public Vector3 localPosition;

			public Quaternion localRotation;

			public Vector3 localScale;

			public float movementSpeed;

			public bool isValid
			{
				get
				{
					return timeStamp != 0f;
				}
			}
		}

		[Header("Authority")]
		[Tooltip("Set to true if moves come from owner client, set to false if moves always come from server")]
		[SyncVar]
		public bool clientAuthority;

		[Tooltip("Set to true if updates from server should be ignored by owner")]
		[SyncVar]
		public bool excludeOwnerUpdate = true;

		[Header("Synchronization")]
		[Tooltip("Set to true if position should be synchronized")]
		[SyncVar]
		public bool syncPosition = true;

		[Tooltip("Set to true if rotation should be synchronized")]
		[SyncVar]
		public bool syncRotation = true;

		[Tooltip("Set to true if scale should be synchronized")]
		[SyncVar]
		public bool syncScale = true;

		[Header("Interpolation")]
		[Tooltip("Set to true if position should be interpolated")]
		[SyncVar]
		public bool interpolatePosition = true;

		[Tooltip("Set to true if rotation should be interpolated")]
		[SyncVar]
		public bool interpolateRotation = true;

		[Tooltip("Set to true if scale should be interpolated")]
		[SyncVar]
		public bool interpolateScale = true;

		[Header("Sensitivity")]
		[Tooltip("Changes to the transform must exceed these values to be transmitted on the network.")]
		[SyncVar]
		public float localPositionSensitivity = 0.01f;

		[Tooltip("If rotation exceeds this angle, it will be transmitted on the network")]
		[SyncVar]
		public float localRotationSensitivity = 0.01f;

		[Tooltip("Changes to the transform must exceed these values to be transmitted on the network.")]
		[SyncVar]
		public float localScaleSensitivity = 0.01f;

		[Header("Diagnostics")]
		public Vector3 lastPosition;

		public Quaternion lastRotation;

		public Vector3 lastScale;

		public DataPoint start;

		public DataPoint goal;

		private bool clientAuthorityBeforeTeleport;

		protected abstract Transform targetTransform { get; }

		private bool IsOwnerWithClientAuthority
		{
			get
			{
				if (base.hasAuthority)
				{
					return clientAuthority;
				}
				return false;
			}
		}

		private bool HasMoved
		{
			get
			{
				if (syncPosition)
				{
					return Vector3.SqrMagnitude(lastPosition - targetTransform.localPosition) > localPositionSensitivity * localPositionSensitivity;
				}
				return false;
			}
		}

		private bool HasRotated
		{
			get
			{
				if (syncRotation)
				{
					return Quaternion.Angle(lastRotation, targetTransform.localRotation) > localRotationSensitivity;
				}
				return false;
			}
		}

		private bool HasScaled
		{
			get
			{
				if (syncScale)
				{
					return Vector3.SqrMagnitude(lastScale - targetTransform.localScale) > localScaleSensitivity * localScaleSensitivity;
				}
				return false;
			}
		}

		public bool NetworkclientAuthority
		{
			get
			{
				return clientAuthority;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref clientAuthority))
				{
					bool flag = clientAuthority;
					SetSyncVar(value, ref clientAuthority, 1uL);
				}
			}
		}

		public bool NetworkexcludeOwnerUpdate
		{
			get
			{
				return excludeOwnerUpdate;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref excludeOwnerUpdate))
				{
					bool flag = excludeOwnerUpdate;
					SetSyncVar(value, ref excludeOwnerUpdate, 2uL);
				}
			}
		}

		public bool NetworksyncPosition
		{
			get
			{
				return syncPosition;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref syncPosition))
				{
					bool flag = syncPosition;
					SetSyncVar(value, ref syncPosition, 4uL);
				}
			}
		}

		public bool NetworksyncRotation
		{
			get
			{
				return syncRotation;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref syncRotation))
				{
					bool flag = syncRotation;
					SetSyncVar(value, ref syncRotation, 8uL);
				}
			}
		}

		public bool NetworksyncScale
		{
			get
			{
				return syncScale;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref syncScale))
				{
					bool flag = syncScale;
					SetSyncVar(value, ref syncScale, 16uL);
				}
			}
		}

		public bool NetworkinterpolatePosition
		{
			get
			{
				return interpolatePosition;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref interpolatePosition))
				{
					bool flag = interpolatePosition;
					SetSyncVar(value, ref interpolatePosition, 32uL);
				}
			}
		}

		public bool NetworkinterpolateRotation
		{
			get
			{
				return interpolateRotation;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref interpolateRotation))
				{
					bool flag = interpolateRotation;
					SetSyncVar(value, ref interpolateRotation, 64uL);
				}
			}
		}

		public bool NetworkinterpolateScale
		{
			get
			{
				return interpolateScale;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref interpolateScale))
				{
					bool flag = interpolateScale;
					SetSyncVar(value, ref interpolateScale, 128uL);
				}
			}
		}

		public float NetworklocalPositionSensitivity
		{
			get
			{
				return localPositionSensitivity;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref localPositionSensitivity))
				{
					float num = localPositionSensitivity;
					SetSyncVar(value, ref localPositionSensitivity, 256uL);
				}
			}
		}

		public float NetworklocalRotationSensitivity
		{
			get
			{
				return localRotationSensitivity;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref localRotationSensitivity))
				{
					float num = localRotationSensitivity;
					SetSyncVar(value, ref localRotationSensitivity, 512uL);
				}
			}
		}

		public float NetworklocalScaleSensitivity
		{
			get
			{
				return localScaleSensitivity;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref localScaleSensitivity))
				{
					float num = localScaleSensitivity;
					SetSyncVar(value, ref localScaleSensitivity, 1024uL);
				}
			}
		}

		private void FixedUpdate()
		{
			if (base.isServer && HasEitherMovedRotatedScaled())
			{
				ServerUpdate();
			}
			if (base.isClient)
			{
				if (IsOwnerWithClientAuthority)
				{
					ClientAuthorityUpdate();
				}
				else if (goal.isValid)
				{
					ClientRemoteUpdate();
				}
			}
		}

		private void ServerUpdate()
		{
			RpcMove(targetTransform.localPosition, Compression.CompressQuaternion(targetTransform.localRotation), targetTransform.localScale);
		}

		private void ClientAuthorityUpdate()
		{
			if (!base.isServer && HasEitherMovedRotatedScaled())
			{
				CmdClientToServerSync(targetTransform.localPosition, Compression.CompressQuaternion(targetTransform.localRotation), targetTransform.localScale);
			}
		}

		private void ClientRemoteUpdate()
		{
			if (NeedsTeleport())
			{
				ApplyPositionRotationScale(goal.localPosition, goal.localRotation, goal.localScale);
				start = default(DataPoint);
				goal = default(DataPoint);
			}
			else
			{
				ApplyPositionRotationScale(InterpolatePosition(start, goal, targetTransform.localPosition), InterpolateRotation(start, goal, targetTransform.localRotation), InterpolateScale(start, goal, targetTransform.localScale));
			}
		}

		private bool HasEitherMovedRotatedScaled()
		{
			int num;
			if (!HasMoved && !HasRotated)
			{
				num = (HasScaled ? 1 : 0);
				if (num == 0)
				{
					goto IL_0067;
				}
			}
			else
			{
				num = 1;
			}
			if (syncPosition)
			{
				lastPosition = targetTransform.localPosition;
			}
			if (syncRotation)
			{
				lastRotation = targetTransform.localRotation;
			}
			if (syncScale)
			{
				lastScale = targetTransform.localScale;
			}
			goto IL_0067;
			IL_0067:
			return (byte)num != 0;
		}

		private bool NeedsTeleport()
		{
			float num = (start.isValid ? start.timeStamp : (Time.time - Time.fixedDeltaTime));
			float num2 = (goal.isValid ? goal.timeStamp : Time.time);
			float num3 = num2 - num;
			return Time.time - num2 > num3 * 5f;
		}

		[Command(channel = 1)]
		private void CmdClientToServerSync(Vector3 position, uint packedRotation, Vector3 scale)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteVector3(position);
			writer.WriteUInt(packedRotation);
			writer.WriteVector3(scale);
			SendCommandInternal(typeof(NetworkTransformBase), "CmdClientToServerSync", writer, 1);
			NetworkWriterPool.Recycle(writer);
		}

		[ClientRpc(channel = 1)]
		private void RpcMove(Vector3 position, uint packedRotation, Vector3 scale)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteVector3(position);
			writer.WriteUInt(packedRotation);
			writer.WriteVector3(scale);
			SendRPCInternal(typeof(NetworkTransformBase), "RpcMove", writer, 1, true);
			NetworkWriterPool.Recycle(writer);
		}

		private void SetGoal(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			DataPoint dataPoint = default(DataPoint);
			dataPoint.localPosition = position;
			dataPoint.localRotation = rotation;
			dataPoint.localScale = scale;
			dataPoint.timeStamp = Time.time;
			DataPoint to = dataPoint;
			to.movementSpeed = EstimateMovementSpeed(goal, to, targetTransform, Time.fixedDeltaTime);
			if (start.timeStamp == 0f)
			{
				start = new DataPoint
				{
					timeStamp = Time.time - Time.fixedDeltaTime,
					localPosition = targetTransform.localPosition,
					localRotation = targetTransform.localRotation,
					localScale = targetTransform.localScale,
					movementSpeed = to.movementSpeed
				};
			}
			else
			{
				float num = Vector3.Distance(start.localPosition, goal.localPosition);
				float num2 = Vector3.Distance(goal.localPosition, to.localPosition);
				start = goal;
				if (Vector3.Distance(targetTransform.localPosition, start.localPosition) < num + num2)
				{
					start.localPosition = targetTransform.localPosition;
					start.localRotation = targetTransform.localRotation;
					start.localScale = targetTransform.localScale;
				}
			}
			goal = to;
		}

		private static float EstimateMovementSpeed(DataPoint from, DataPoint to, Transform transform, float sendInterval)
		{
			Vector3 vector = to.localPosition - ((from.localPosition != transform.localPosition) ? from.localPosition : transform.localPosition);
			float num = (from.isValid ? (to.timeStamp - from.timeStamp) : sendInterval);
			if (!(num > 0f))
			{
				return 0f;
			}
			return vector.magnitude / num;
		}

		private void ApplyPositionRotationScale(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			if (syncPosition)
			{
				targetTransform.localPosition = position;
			}
			if (syncRotation)
			{
				targetTransform.localRotation = rotation;
			}
			if (syncScale)
			{
				targetTransform.localScale = scale;
			}
		}

		private Vector3 InterpolatePosition(DataPoint start, DataPoint goal, Vector3 currentPosition)
		{
			if (!interpolatePosition)
			{
				return currentPosition;
			}
			if (start.movementSpeed != 0f)
			{
				float num = Mathf.Max(start.movementSpeed, goal.movementSpeed);
				return Vector3.MoveTowards(currentPosition, goal.localPosition, num * Time.deltaTime);
			}
			return currentPosition;
		}

		private Quaternion InterpolateRotation(DataPoint start, DataPoint goal, Quaternion defaultRotation)
		{
			if (!interpolateRotation)
			{
				return defaultRotation;
			}
			if (start.localRotation != goal.localRotation)
			{
				float t = CurrentInterpolationFactor(start, goal);
				return Quaternion.Slerp(start.localRotation, goal.localRotation, t);
			}
			return defaultRotation;
		}

		private Vector3 InterpolateScale(DataPoint start, DataPoint goal, Vector3 currentScale)
		{
			if (!interpolateScale)
			{
				return currentScale;
			}
			if (start.localScale != goal.localScale)
			{
				float t = CurrentInterpolationFactor(start, goal);
				return Vector3.Lerp(start.localScale, goal.localScale, t);
			}
			return currentScale;
		}

		private static float CurrentInterpolationFactor(DataPoint start, DataPoint goal)
		{
			if (start.isValid)
			{
				float num = goal.timeStamp - start.timeStamp;
				float num2 = Time.time - goal.timeStamp;
				if (!(num > 0f))
				{
					return 1f;
				}
				return num2 / num;
			}
			return 1f;
		}

		[Server]
		public void ServerTeleport(Vector3 localPosition)
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void Mirror.Experimental.NetworkTransformBase::ServerTeleport(UnityEngine.Vector3)' called when server was not active");
				return;
			}
			Quaternion localRotation = targetTransform.localRotation;
			ServerTeleport(localPosition, localRotation);
		}

		[Server]
		public void ServerTeleport(Vector3 localPosition, Quaternion localRotation)
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void Mirror.Experimental.NetworkTransformBase::ServerTeleport(UnityEngine.Vector3,UnityEngine.Quaternion)' called when server was not active");
				return;
			}
			clientAuthorityBeforeTeleport = clientAuthority || clientAuthorityBeforeTeleport;
			NetworkclientAuthority = false;
			DoTeleport(localPosition, localRotation);
			RpcTeleport(localPosition, Compression.CompressQuaternion(localRotation), clientAuthorityBeforeTeleport);
		}

		private void DoTeleport(Vector3 newLocalPosition, Quaternion newLocalRotation)
		{
			targetTransform.localPosition = newLocalPosition;
			targetTransform.localRotation = newLocalRotation;
			goal = default(DataPoint);
			start = default(DataPoint);
			lastPosition = newLocalPosition;
			lastRotation = newLocalRotation;
		}

		[ClientRpc(channel = 1)]
		private void RpcTeleport(Vector3 newPosition, uint newPackedRotation, bool isClientAuthority)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteVector3(newPosition);
			writer.WriteUInt(newPackedRotation);
			writer.WriteBool(isClientAuthority);
			SendRPCInternal(typeof(NetworkTransformBase), "RpcTeleport", writer, 1, true);
			NetworkWriterPool.Recycle(writer);
		}

		[Command(channel = 1)]
		private void CmdTeleportFinished()
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			SendCommandInternal(typeof(NetworkTransformBase), "CmdTeleportFinished", writer, 1);
			NetworkWriterPool.Recycle(writer);
		}

		private void OnDrawGizmos()
		{
			if (start.localPosition != goal.localPosition)
			{
				DrawDataPointGizmo(start, Color.yellow);
				DrawDataPointGizmo(goal, Color.green);
				DrawLineBetweenDataPoints(start, goal, Color.cyan);
			}
		}

		private static void DrawDataPointGizmo(DataPoint data, Color color)
		{
			Vector3 vector = Vector3.up * 0.01f;
			Gizmos.color = color;
			Gizmos.DrawSphere(data.localPosition + vector, 0.5f);
			Gizmos.color = Color.blue;
			Gizmos.DrawRay(data.localPosition + vector, data.localRotation * Vector3.forward);
			Gizmos.color = Color.green;
			Gizmos.DrawRay(data.localPosition + vector, data.localRotation * Vector3.up);
		}

		private static void DrawLineBetweenDataPoints(DataPoint data1, DataPoint data2, Color color)
		{
			Gizmos.color = color;
			Gizmos.DrawLine(data1.localPosition, data2.localPosition);
		}

		private void MirrorProcessed()
		{
		}

		protected void UserCode_CmdClientToServerSync(Vector3 position, uint packedRotation, Vector3 scale)
		{
			if (clientAuthority)
			{
				SetGoal(position, Compression.DecompressQuaternion(packedRotation), scale);
				if (base.isServer && !base.isClient)
				{
					ApplyPositionRotationScale(goal.localPosition, goal.localRotation, goal.localScale);
				}
				RpcMove(position, packedRotation, scale);
			}
		}

		protected static void InvokeUserCode_CmdClientToServerSync(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdClientToServerSync called on client.");
			}
			else
			{
				((NetworkTransformBase)obj).UserCode_CmdClientToServerSync(reader.ReadVector3(), reader.ReadUInt(), reader.ReadVector3());
			}
		}

		protected void UserCode_RpcMove(Vector3 position, uint packedRotation, Vector3 scale)
		{
			if ((!base.hasAuthority || !excludeOwnerUpdate) && !base.isServer)
			{
				SetGoal(position, Compression.DecompressQuaternion(packedRotation), scale);
			}
		}

		protected static void InvokeUserCode_RpcMove(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("RPC RpcMove called on server.");
			}
			else
			{
				((NetworkTransformBase)obj).UserCode_RpcMove(reader.ReadVector3(), reader.ReadUInt(), reader.ReadVector3());
			}
		}

		protected void UserCode_RpcTeleport(Vector3 newPosition, uint newPackedRotation, bool isClientAuthority)
		{
			DoTeleport(newPosition, Compression.DecompressQuaternion(newPackedRotation));
			if (base.hasAuthority && isClientAuthority)
			{
				CmdTeleportFinished();
			}
		}

		protected static void InvokeUserCode_RpcTeleport(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("RPC RpcTeleport called on server.");
			}
			else
			{
				((NetworkTransformBase)obj).UserCode_RpcTeleport(reader.ReadVector3(), reader.ReadUInt(), reader.ReadBool());
			}
		}

		protected void UserCode_CmdTeleportFinished()
		{
			if (clientAuthorityBeforeTeleport)
			{
				NetworkclientAuthority = true;
				clientAuthorityBeforeTeleport = false;
			}
			else
			{
				Debug.LogWarning("Client called TeleportFinished when clientAuthority was false on server", this);
			}
		}

		protected static void InvokeUserCode_CmdTeleportFinished(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdTeleportFinished called on client.");
			}
			else
			{
				((NetworkTransformBase)obj).UserCode_CmdTeleportFinished();
			}
		}

		static NetworkTransformBase()
		{
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkTransformBase), "CmdClientToServerSync", InvokeUserCode_CmdClientToServerSync, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkTransformBase), "CmdTeleportFinished", InvokeUserCode_CmdTeleportFinished, true);
			RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkTransformBase), "RpcMove", InvokeUserCode_RpcMove);
			RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkTransformBase), "RpcTeleport", InvokeUserCode_RpcTeleport);
		}

		protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
		{
			bool result = base.SerializeSyncVars(writer, forceAll);
			if (forceAll)
			{
				writer.WriteBool(clientAuthority);
				writer.WriteBool(excludeOwnerUpdate);
				writer.WriteBool(syncPosition);
				writer.WriteBool(syncRotation);
				writer.WriteBool(syncScale);
				writer.WriteBool(interpolatePosition);
				writer.WriteBool(interpolateRotation);
				writer.WriteBool(interpolateScale);
				writer.WriteFloat(localPositionSensitivity);
				writer.WriteFloat(localRotationSensitivity);
				writer.WriteFloat(localScaleSensitivity);
				return true;
			}
			writer.WriteULong(base.syncVarDirtyBits);
			if ((base.syncVarDirtyBits & 1L) != 0L)
			{
				writer.WriteBool(clientAuthority);
				result = true;
			}
			if ((base.syncVarDirtyBits & 2L) != 0L)
			{
				writer.WriteBool(excludeOwnerUpdate);
				result = true;
			}
			if ((base.syncVarDirtyBits & 4L) != 0L)
			{
				writer.WriteBool(syncPosition);
				result = true;
			}
			if ((base.syncVarDirtyBits & 8L) != 0L)
			{
				writer.WriteBool(syncRotation);
				result = true;
			}
			if ((base.syncVarDirtyBits & 0x10L) != 0L)
			{
				writer.WriteBool(syncScale);
				result = true;
			}
			if ((base.syncVarDirtyBits & 0x20L) != 0L)
			{
				writer.WriteBool(interpolatePosition);
				result = true;
			}
			if ((base.syncVarDirtyBits & 0x40L) != 0L)
			{
				writer.WriteBool(interpolateRotation);
				result = true;
			}
			if ((base.syncVarDirtyBits & 0x80L) != 0L)
			{
				writer.WriteBool(interpolateScale);
				result = true;
			}
			if ((base.syncVarDirtyBits & 0x100L) != 0L)
			{
				writer.WriteFloat(localPositionSensitivity);
				result = true;
			}
			if ((base.syncVarDirtyBits & 0x200L) != 0L)
			{
				writer.WriteFloat(localRotationSensitivity);
				result = true;
			}
			if ((base.syncVarDirtyBits & 0x400L) != 0L)
			{
				writer.WriteFloat(localScaleSensitivity);
				result = true;
			}
			return result;
		}

		protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
		{
			base.DeserializeSyncVars(reader, initialState);
			if (initialState)
			{
				bool flag = clientAuthority;
				NetworkclientAuthority = reader.ReadBool();
				bool flag2 = excludeOwnerUpdate;
				NetworkexcludeOwnerUpdate = reader.ReadBool();
				bool flag3 = syncPosition;
				NetworksyncPosition = reader.ReadBool();
				bool flag4 = syncRotation;
				NetworksyncRotation = reader.ReadBool();
				bool flag5 = syncScale;
				NetworksyncScale = reader.ReadBool();
				bool flag6 = interpolatePosition;
				NetworkinterpolatePosition = reader.ReadBool();
				bool flag7 = interpolateRotation;
				NetworkinterpolateRotation = reader.ReadBool();
				bool flag8 = interpolateScale;
				NetworkinterpolateScale = reader.ReadBool();
				float num = localPositionSensitivity;
				NetworklocalPositionSensitivity = reader.ReadFloat();
				float num2 = localRotationSensitivity;
				NetworklocalRotationSensitivity = reader.ReadFloat();
				float num3 = localScaleSensitivity;
				NetworklocalScaleSensitivity = reader.ReadFloat();
				return;
			}
			long num4 = (long)reader.ReadULong();
			if ((num4 & 1L) != 0L)
			{
				bool flag9 = clientAuthority;
				NetworkclientAuthority = reader.ReadBool();
			}
			if ((num4 & 2L) != 0L)
			{
				bool flag10 = excludeOwnerUpdate;
				NetworkexcludeOwnerUpdate = reader.ReadBool();
			}
			if ((num4 & 4L) != 0L)
			{
				bool flag11 = syncPosition;
				NetworksyncPosition = reader.ReadBool();
			}
			if ((num4 & 8L) != 0L)
			{
				bool flag12 = syncRotation;
				NetworksyncRotation = reader.ReadBool();
			}
			if ((num4 & 0x10L) != 0L)
			{
				bool flag13 = syncScale;
				NetworksyncScale = reader.ReadBool();
			}
			if ((num4 & 0x20L) != 0L)
			{
				bool flag14 = interpolatePosition;
				NetworkinterpolatePosition = reader.ReadBool();
			}
			if ((num4 & 0x40L) != 0L)
			{
				bool flag15 = interpolateRotation;
				NetworkinterpolateRotation = reader.ReadBool();
			}
			if ((num4 & 0x80L) != 0L)
			{
				bool flag16 = interpolateScale;
				NetworkinterpolateScale = reader.ReadBool();
			}
			if ((num4 & 0x100L) != 0L)
			{
				float num5 = localPositionSensitivity;
				NetworklocalPositionSensitivity = reader.ReadFloat();
			}
			if ((num4 & 0x200L) != 0L)
			{
				float num6 = localRotationSensitivity;
				NetworklocalRotationSensitivity = reader.ReadFloat();
			}
			if ((num4 & 0x400L) != 0L)
			{
				float num7 = localScaleSensitivity;
				NetworklocalScaleSensitivity = reader.ReadFloat();
			}
		}
	}
}
