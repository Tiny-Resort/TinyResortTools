using System;
using System.Collections.Generic;
using Mirror.RemoteCalls;
using UnityEngine;

namespace Mirror
{
	public abstract class NetworkTransformBase : NetworkBehaviour
	{
		[Header("Authority")]
		[Tooltip("Set to true if moves come from owner client, set to false if moves always come from server")]
		public bool clientAuthority;

		[Header("Synchronization")]
		[Range(0f, 1f)]
		public float sendInterval = 0.05f;

		public bool syncPosition = true;

		public bool syncRotation = true;

		public bool syncScale;

		private double lastClientSendTime;

		private double lastServerSendTime;

		[Header("Interpolation")]
		public bool interpolatePosition = true;

		public bool interpolateRotation = true;

		public bool interpolateScale = true;

		[Header("Buffering")]
		[Tooltip("Snapshots are buffered for sendInterval * multiplier seconds. At 2-5% packet loss, 3x supposedly works best.")]
		public int bufferTimeMultiplier = 3;

		[Tooltip("Buffer size limit to avoid ever growing list memory consumption attacks.")]
		public int bufferSizeLimit = 64;

		[Tooltip("Start to accelerate interpolation if buffer size is >= threshold. Needs to be larger than bufferTimeMultiplier.")]
		public int catchupThreshold = 6;

		[Tooltip("Once buffer is larger catchupThreshold, accelerate by multiplier % per excess entry.")]
		[Range(0f, 1f)]
		public float catchupMultiplier = 0.1f;

		internal SortedList<double, NTSnapshot> serverBuffer = new SortedList<double, NTSnapshot>();

		internal SortedList<double, NTSnapshot> clientBuffer = new SortedList<double, NTSnapshot>();

		private double serverInterpolationTime;

		private double clientInterpolationTime;

		private Func<NTSnapshot, NTSnapshot, double, NTSnapshot> Interpolate = NTSnapshot.Interpolate;

		[Header("Debug")]
		public bool showGizmos;

		public bool showOverlay;

		public Color overlayColor = new Color(0f, 0f, 0f, 0.5f);

		private bool IsClientWithAuthority
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

		protected abstract Transform targetComponent { get; }

		public float bufferTime
		{
			get
			{
				return sendInterval * (float)bufferTimeMultiplier;
			}
		}

		protected virtual NTSnapshot ConstructSnapshot()
		{
			return new NTSnapshot(NetworkTime.localTime, 0.0, targetComponent.localPosition, targetComponent.localRotation, targetComponent.localScale);
		}

		protected virtual void ApplySnapshot(NTSnapshot start, NTSnapshot goal, NTSnapshot interpolated)
		{
			if (syncPosition)
			{
				targetComponent.localPosition = (interpolatePosition ? interpolated.position : goal.position);
			}
			if (syncRotation)
			{
				targetComponent.localRotation = (interpolateRotation ? interpolated.rotation : goal.rotation);
			}
			if (syncScale)
			{
				targetComponent.localScale = (interpolateScale ? interpolated.scale : goal.scale);
			}
		}

		[Command(channel = 1)]
		private void CmdClientToServerSync(Vector3? position, Quaternion? rotation, Vector3? scale)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteVector3Nullable(position);
			writer.WriteQuaternionNullable(rotation);
			writer.WriteVector3Nullable(scale);
			SendCommandInternal(typeof(NetworkTransformBase), "CmdClientToServerSync", writer, 1);
			NetworkWriterPool.Recycle(writer);
		}

		protected virtual void OnClientToServerSync(Vector3? position, Quaternion? rotation, Vector3? scale)
		{
			if (clientAuthority && serverBuffer.Count < bufferSizeLimit)
			{
				double remoteTimeStamp = base.connectionToClient.remoteTimeStamp;
				if (!position.HasValue)
				{
					position = targetComponent.localPosition;
				}
				if (!rotation.HasValue)
				{
					rotation = targetComponent.localRotation;
				}
				if (!scale.HasValue)
				{
					scale = targetComponent.localScale;
				}
				SnapshotInterpolation.InsertIfNewEnough(new NTSnapshot(remoteTimeStamp, NetworkTime.localTime, position.Value, rotation.Value, scale.Value), serverBuffer);
			}
		}

		[ClientRpc(channel = 1)]
		private void RpcServerToClientSync(Vector3? position, Quaternion? rotation, Vector3? scale)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteVector3Nullable(position);
			writer.WriteQuaternionNullable(rotation);
			writer.WriteVector3Nullable(scale);
			SendRPCInternal(typeof(NetworkTransformBase), "RpcServerToClientSync", writer, 1, true);
			NetworkWriterPool.Recycle(writer);
		}

		protected virtual void OnServerToClientSync(Vector3? position, Quaternion? rotation, Vector3? scale)
		{
			if (!base.isServer && !IsClientWithAuthority && clientBuffer.Count < bufferSizeLimit)
			{
				double remoteTimeStamp = NetworkClient.connection.remoteTimeStamp;
				if (!position.HasValue)
				{
					position = targetComponent.localPosition;
				}
				if (!rotation.HasValue)
				{
					rotation = targetComponent.localRotation;
				}
				if (!scale.HasValue)
				{
					scale = targetComponent.localScale;
				}
				SnapshotInterpolation.InsertIfNewEnough(new NTSnapshot(remoteTimeStamp, NetworkTime.localTime, position.Value, rotation.Value, scale.Value), clientBuffer);
			}
		}

		private void UpdateServer()
		{
			if (NetworkTime.localTime >= lastServerSendTime + (double)sendInterval)
			{
				NTSnapshot nTSnapshot = ConstructSnapshot();
				RpcServerToClientSync(syncPosition ? new Vector3?(nTSnapshot.position) : null, syncRotation ? new Quaternion?(nTSnapshot.rotation) : null, syncScale ? new Vector3?(nTSnapshot.scale) : null);
				lastServerSendTime = NetworkTime.localTime;
			}
			NTSnapshot computed;
			if (clientAuthority && !base.hasAuthority && SnapshotInterpolation.Compute(NetworkTime.localTime, Time.deltaTime, ref serverInterpolationTime, bufferTime, serverBuffer, catchupThreshold, catchupMultiplier, Interpolate, out computed))
			{
				NTSnapshot start = serverBuffer.Values[0];
				NTSnapshot goal = serverBuffer.Values[1];
				ApplySnapshot(start, goal, computed);
			}
		}

		private void UpdateClient()
		{
			NTSnapshot computed;
			if (IsClientWithAuthority)
			{
				if (NetworkTime.localTime >= lastClientSendTime + (double)sendInterval)
				{
					NTSnapshot nTSnapshot = ConstructSnapshot();
					CmdClientToServerSync(syncPosition ? new Vector3?(nTSnapshot.position) : null, syncRotation ? new Quaternion?(nTSnapshot.rotation) : null, syncScale ? new Vector3?(nTSnapshot.scale) : null);
					lastClientSendTime = NetworkTime.localTime;
				}
			}
			else if (SnapshotInterpolation.Compute(NetworkTime.localTime, Time.deltaTime, ref clientInterpolationTime, bufferTime, clientBuffer, catchupThreshold, catchupMultiplier, Interpolate, out computed))
			{
				NTSnapshot start = clientBuffer.Values[0];
				NTSnapshot goal = clientBuffer.Values[1];
				ApplySnapshot(start, goal, computed);
			}
		}

		private void Update()
		{
			if (base.isServer)
			{
				UpdateServer();
			}
			else if (base.isClient)
			{
				UpdateClient();
			}
		}

		protected virtual void OnTeleport(Vector3 destination)
		{
			Reset();
			targetComponent.position = destination;
		}

		public void resetObjectsInterpolation()
		{
			Reset();
		}

		[ClientRpc]
		public void RpcTeleport(Vector3 destination)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteVector3(destination);
			SendRPCInternal(typeof(NetworkTransformBase), "RpcTeleport", writer, 0, true);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		public void CmdTeleport(Vector3 destination)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteVector3(destination);
			SendCommandInternal(typeof(NetworkTransformBase), "CmdTeleport", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		protected virtual void Reset()
		{
			serverBuffer.Clear();
			clientBuffer.Clear();
			serverInterpolationTime = 0.0;
			clientInterpolationTime = 0.0;
		}

		protected virtual void OnDisable()
		{
			Reset();
		}

		protected virtual void OnEnable()
		{
			Reset();
		}

		protected virtual void OnValidate()
		{
			catchupThreshold = Mathf.Max(bufferTimeMultiplier + 1, catchupThreshold);
			bufferSizeLimit = Mathf.Max(bufferTimeMultiplier, bufferSizeLimit);
		}

		protected virtual void OnGUI()
		{
			if (!showOverlay || !Debug.isDebugBuild)
			{
				return;
			}
			Vector3 vector = Camera.main.WorldToScreenPoint(targetComponent.position);
			if (vector.z >= 0f && Utils.IsPointInScreen(vector))
			{
				int num = Mathf.Max(serverBuffer.Count - catchupThreshold, 0);
				int num2 = Mathf.Max(clientBuffer.Count - catchupThreshold, 0);
				float num3 = (float)num * catchupMultiplier;
				float num4 = (float)num2 * catchupMultiplier;
				GUI.color = overlayColor;
				GUILayout.BeginArea(new Rect(vector.x, (float)Screen.height - vector.y, 200f, 100f));
				GUILayout.Label(string.Format("Server Buffer:{0}", serverBuffer.Count));
				if (num3 > 0f)
				{
					GUILayout.Label(string.Format("Server Catchup:{0:F2}%", num3 * 100f));
				}
				GUILayout.Label(string.Format("Client Buffer:{0}", clientBuffer.Count));
				if (num4 > 0f)
				{
					GUILayout.Label(string.Format("Client Catchup:{0:F2}%", num4 * 100f));
				}
				GUILayout.EndArea();
				GUI.color = Color.white;
			}
		}

		protected virtual void DrawGizmos(SortedList<double, NTSnapshot> buffer)
		{
			if (buffer.Count >= 2)
			{
				double num = NetworkTime.localTime - (double)bufferTime;
				Color color = new Color(0f, 1f, 0f, 0.5f);
				Color color2 = new Color(0.5f, 0.5f, 0.5f, 0.3f);
				for (int i = 0; i < buffer.Count; i++)
				{
					NTSnapshot nTSnapshot = buffer.Values[i];
					Gizmos.color = ((nTSnapshot.localTimestamp <= num) ? color : color2);
					Gizmos.DrawCube(nTSnapshot.position, Vector3.one);
				}
				Gizmos.color = Color.green;
				Gizmos.DrawLine(buffer.Values[0].position, targetComponent.position);
				Gizmos.color = Color.white;
				Gizmos.DrawLine(targetComponent.position, buffer.Values[1].position);
			}
		}

		protected virtual void OnDrawGizmos()
		{
			if (showGizmos)
			{
				if (base.isServer)
				{
					DrawGizmos(serverBuffer);
				}
				if (base.isClient)
				{
					DrawGizmos(clientBuffer);
				}
			}
		}

		private void MirrorProcessed()
		{
		}

		protected void UserCode_CmdClientToServerSync(Vector3? position, Quaternion? rotation, Vector3? scale)
		{
			OnClientToServerSync(position, rotation, scale);
		}

		protected static void InvokeUserCode_CmdClientToServerSync(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdClientToServerSync called on client.");
			}
			else
			{
				((NetworkTransformBase)obj).UserCode_CmdClientToServerSync(reader.ReadVector3Nullable(), reader.ReadQuaternionNullable(), reader.ReadVector3Nullable());
			}
		}

		protected void UserCode_RpcServerToClientSync(Vector3? position, Quaternion? rotation, Vector3? scale)
		{
			OnServerToClientSync(position, rotation, scale);
		}

		protected static void InvokeUserCode_RpcServerToClientSync(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("RPC RpcServerToClientSync called on server.");
			}
			else
			{
				((NetworkTransformBase)obj).UserCode_RpcServerToClientSync(reader.ReadVector3Nullable(), reader.ReadQuaternionNullable(), reader.ReadVector3Nullable());
			}
		}

		protected void UserCode_RpcTeleport(Vector3 destination)
		{
			OnTeleport(destination);
		}

		protected static void InvokeUserCode_RpcTeleport(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("RPC RpcTeleport called on server.");
			}
			else
			{
				((NetworkTransformBase)obj).UserCode_RpcTeleport(reader.ReadVector3());
			}
		}

		protected void UserCode_CmdTeleport(Vector3 destination)
		{
			if (clientAuthority)
			{
				OnTeleport(destination);
				RpcTeleport(destination);
			}
		}

		protected static void InvokeUserCode_CmdTeleport(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdTeleport called on client.");
			}
			else
			{
				((NetworkTransformBase)obj).UserCode_CmdTeleport(reader.ReadVector3());
			}
		}

		static NetworkTransformBase()
		{
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkTransformBase), "CmdClientToServerSync", InvokeUserCode_CmdClientToServerSync, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkTransformBase), "CmdTeleport", InvokeUserCode_CmdTeleport, true);
			RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkTransformBase), "RpcServerToClientSync", InvokeUserCode_RpcServerToClientSync);
			RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkTransformBase), "RpcTeleport", InvokeUserCode_RpcTeleport);
		}
	}
}
