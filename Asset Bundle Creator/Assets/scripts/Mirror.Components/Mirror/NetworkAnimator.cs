using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirror
{
	[AddComponentMenu("Network/NetworkAnimator")]
	[RequireComponent(typeof(NetworkIdentity))]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-animator")]
	public class NetworkAnimator : NetworkBehaviour
	{
		[Header("Authority")]
		[Tooltip("Set to true if animations come from owner client,  set to false if animations always come from server")]
		public bool clientAuthority;

		[FormerlySerializedAs("m_Animator")]
		[Header("Animator")]
		[Tooltip("Animator that will have parameters synchronized")]
		public Animator animator;

		[SyncVar(hook = "OnAnimatorSpeedChanged")]
		private float animatorSpeed;

		private float previousSpeed;

		private int[] lastIntParameters;

		private float[] lastFloatParameters;

		private bool[] lastBoolParameters;

		private AnimatorControllerParameter[] parameters;

		private int[] animationHash;

		private int[] transitionHash;

		private float[] layerWeight;

		private double nextSendTime;

		private bool SendMessagesAllowed
		{
			get
			{
				if (base.isServer)
				{
					if (!clientAuthority)
					{
						return true;
					}
					if (base.netIdentity != null && base.netIdentity.connectionToClient == null)
					{
						return true;
					}
				}
				if (base.hasAuthority)
				{
					return clientAuthority;
				}
				return false;
			}
		}

		public float NetworkanimatorSpeed
		{
			get
			{
				return animatorSpeed;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref animatorSpeed))
				{
					float _ = animatorSpeed;
					SetSyncVar(value, ref animatorSpeed, 1uL);
					if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
					{
						setSyncVarHookGuard(1uL, true);
						OnAnimatorSpeedChanged(_, value);
						setSyncVarHookGuard(1uL, false);
					}
				}
			}
		}

		private void Awake()
		{
			parameters = animator.parameters.Where(_003CAwake_003Eb__14_0).ToArray();
			lastIntParameters = new int[parameters.Length];
			lastFloatParameters = new float[parameters.Length];
			lastBoolParameters = new bool[parameters.Length];
			animationHash = new int[animator.layerCount];
			transitionHash = new int[animator.layerCount];
			layerWeight = new float[animator.layerCount];
		}

		private void FixedUpdate()
		{
			if (!SendMessagesAllowed || !animator.enabled)
			{
				return;
			}
			CheckSendRate();
			for (int i = 0; i < animator.layerCount; i++)
			{
				int stateHash;
				float normalizedTime;
				if (CheckAnimStateChanged(out stateHash, out normalizedTime, i))
				{
					using (PooledNetworkWriter pooledNetworkWriter = NetworkWriterPool.GetWriter())
					{
						WriteParameters(pooledNetworkWriter);
						SendAnimationMessage(stateHash, normalizedTime, i, layerWeight[i], pooledNetworkWriter.ToArray());
					}
				}
			}
			CheckSpeed();
		}

		private void CheckSpeed()
		{
			float speed = animator.speed;
			if (Mathf.Abs(previousSpeed - speed) > 0.001f)
			{
				previousSpeed = speed;
				if (base.isServer)
				{
					NetworkanimatorSpeed = speed;
				}
				else if (base.isClient)
				{
					CmdSetAnimatorSpeed(speed);
				}
			}
		}

		private void OnAnimatorSpeedChanged(float _, float value)
		{
			if (!base.isServer && (!base.hasAuthority || !clientAuthority))
			{
				animator.speed = value;
			}
		}

		private bool CheckAnimStateChanged(out int stateHash, out float normalizedTime, int layerId)
		{
			bool result = false;
			stateHash = 0;
			normalizedTime = 0f;
			float num = animator.GetLayerWeight(layerId);
			if (Mathf.Abs(num - layerWeight[layerId]) > 0.001f)
			{
				layerWeight[layerId] = num;
				result = true;
			}
			if (animator.IsInTransition(layerId))
			{
				AnimatorTransitionInfo animatorTransitionInfo = animator.GetAnimatorTransitionInfo(layerId);
				if (animatorTransitionInfo.fullPathHash != transitionHash[layerId])
				{
					transitionHash[layerId] = animatorTransitionInfo.fullPathHash;
					animationHash[layerId] = 0;
					return true;
				}
				return result;
			}
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(layerId);
			if (currentAnimatorStateInfo.fullPathHash != animationHash[layerId])
			{
				if (animationHash[layerId] != 0)
				{
					stateHash = currentAnimatorStateInfo.fullPathHash;
					normalizedTime = currentAnimatorStateInfo.normalizedTime;
				}
				transitionHash[layerId] = 0;
				animationHash[layerId] = currentAnimatorStateInfo.fullPathHash;
				return true;
			}
			return result;
		}

		private void CheckSendRate()
		{
			double localTime = NetworkTime.localTime;
			if (!SendMessagesAllowed || !(syncInterval >= 0f) || !(localTime > nextSendTime))
			{
				return;
			}
			nextSendTime = localTime + (double)syncInterval;
			using (PooledNetworkWriter pooledNetworkWriter = NetworkWriterPool.GetWriter())
			{
				if (WriteParameters(pooledNetworkWriter))
				{
					SendAnimationParametersMessage(pooledNetworkWriter.ToArray());
				}
			}
		}

		private void SendAnimationMessage(int stateHash, float normalizedTime, int layerId, float weight, byte[] parameters)
		{
			if (base.isServer)
			{
				RpcOnAnimationClientMessage(stateHash, normalizedTime, layerId, weight, parameters);
			}
			else if (base.isClient)
			{
				CmdOnAnimationServerMessage(stateHash, normalizedTime, layerId, weight, parameters);
			}
		}

		private void SendAnimationParametersMessage(byte[] parameters)
		{
			if (base.isServer)
			{
				RpcOnAnimationParametersClientMessage(parameters);
			}
			else if (base.isClient)
			{
				CmdOnAnimationParametersServerMessage(parameters);
			}
		}

		private void HandleAnimMsg(int stateHash, float normalizedTime, int layerId, float weight, NetworkReader reader)
		{
			if (!base.hasAuthority || !clientAuthority)
			{
				if (stateHash != 0 && animator.enabled)
				{
					animator.Play(stateHash, layerId, normalizedTime);
				}
				animator.SetLayerWeight(layerId, weight);
				ReadParameters(reader);
			}
		}

		private void HandleAnimParamsMsg(NetworkReader reader)
		{
			if (!base.hasAuthority || !clientAuthority)
			{
				ReadParameters(reader);
			}
		}

		private void HandleAnimTriggerMsg(int hash)
		{
			if (animator.enabled)
			{
				animator.SetTrigger(hash);
			}
		}

		private void HandleAnimResetTriggerMsg(int hash)
		{
			if (animator.enabled)
			{
				animator.ResetTrigger(hash);
			}
		}

		private ulong NextDirtyBits()
		{
			ulong num = 0uL;
			for (int i = 0; i < parameters.Length; i++)
			{
				AnimatorControllerParameter animatorControllerParameter = parameters[i];
				bool flag = false;
				if (animatorControllerParameter.type == AnimatorControllerParameterType.Int)
				{
					int integer = animator.GetInteger(animatorControllerParameter.nameHash);
					flag = integer != lastIntParameters[i];
					if (flag)
					{
						lastIntParameters[i] = integer;
					}
				}
				else if (animatorControllerParameter.type == AnimatorControllerParameterType.Float)
				{
					float @float = animator.GetFloat(animatorControllerParameter.nameHash);
					flag = Mathf.Abs(@float - lastFloatParameters[i]) > 0.001f;
					if (flag)
					{
						lastFloatParameters[i] = @float;
					}
				}
				else if (animatorControllerParameter.type == AnimatorControllerParameterType.Bool)
				{
					bool @bool = animator.GetBool(animatorControllerParameter.nameHash);
					flag = @bool != lastBoolParameters[i];
					if (flag)
					{
						lastBoolParameters[i] = @bool;
					}
				}
				if (flag)
				{
					num |= (ulong)(1L << i);
				}
			}
			return num;
		}

		private bool WriteParameters(NetworkWriter writer, bool forceAll = false)
		{
			ulong num = (forceAll ? ulong.MaxValue : NextDirtyBits());
			writer.WriteULong(num);
			for (int i = 0; i < parameters.Length; i++)
			{
				if ((num & (ulong)(1L << i)) != 0L)
				{
					AnimatorControllerParameter animatorControllerParameter = parameters[i];
					if (animatorControllerParameter.type == AnimatorControllerParameterType.Int)
					{
						int integer = animator.GetInteger(animatorControllerParameter.nameHash);
						writer.WriteInt(integer);
					}
					else if (animatorControllerParameter.type == AnimatorControllerParameterType.Float)
					{
						float @float = animator.GetFloat(animatorControllerParameter.nameHash);
						writer.WriteFloat(@float);
					}
					else if (animatorControllerParameter.type == AnimatorControllerParameterType.Bool)
					{
						bool @bool = animator.GetBool(animatorControllerParameter.nameHash);
						writer.WriteBool(@bool);
					}
				}
			}
			return num != 0;
		}

		private void ReadParameters(NetworkReader reader)
		{
			bool flag = animator.enabled;
			ulong num = reader.ReadULong();
			for (int i = 0; i < parameters.Length; i++)
			{
				if ((num & (ulong)(1L << i)) == 0L)
				{
					continue;
				}
				AnimatorControllerParameter animatorControllerParameter = parameters[i];
				if (animatorControllerParameter.type == AnimatorControllerParameterType.Int)
				{
					int value = reader.ReadInt();
					if (flag)
					{
						animator.SetInteger(animatorControllerParameter.nameHash, value);
					}
				}
				else if (animatorControllerParameter.type == AnimatorControllerParameterType.Float)
				{
					float value2 = reader.ReadFloat();
					if (flag)
					{
						animator.SetFloat(animatorControllerParameter.nameHash, value2);
					}
				}
				else if (animatorControllerParameter.type == AnimatorControllerParameterType.Bool)
				{
					bool value3 = reader.ReadBool();
					if (flag)
					{
						animator.SetBool(animatorControllerParameter.nameHash, value3);
					}
				}
			}
		}

		public override bool OnSerialize(NetworkWriter writer, bool initialState)
		{
			bool result = base.OnSerialize(writer, initialState);
			if (initialState)
			{
				for (int i = 0; i < animator.layerCount; i++)
				{
					if (animator.IsInTransition(i))
					{
						AnimatorStateInfo nextAnimatorStateInfo = animator.GetNextAnimatorStateInfo(i);
						writer.WriteInt(nextAnimatorStateInfo.fullPathHash);
						writer.WriteFloat(nextAnimatorStateInfo.normalizedTime);
					}
					else
					{
						AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(i);
						writer.WriteInt(currentAnimatorStateInfo.fullPathHash);
						writer.WriteFloat(currentAnimatorStateInfo.normalizedTime);
					}
					writer.WriteFloat(animator.GetLayerWeight(i));
				}
				WriteParameters(writer, initialState);
				return true;
			}
			return result;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			base.OnDeserialize(reader, initialState);
			if (initialState)
			{
				for (int i = 0; i < animator.layerCount; i++)
				{
					int stateNameHash = reader.ReadInt();
					float normalizedTime = reader.ReadFloat();
					animator.SetLayerWeight(i, reader.ReadFloat());
					animator.Play(stateNameHash, i, normalizedTime);
				}
				ReadParameters(reader);
			}
		}

		public void SetTrigger(string triggerName)
		{
			SetTrigger(Animator.StringToHash(triggerName));
		}

		public void SetTrigger(int hash)
		{
			if (clientAuthority)
			{
				if (!base.isClient)
				{
					Debug.LogWarning("Tried to set animation in the server for a client-controlled animator");
					return;
				}
				if (!base.hasAuthority)
				{
					Debug.LogWarning("Only the client with authority can set animations");
					return;
				}
				if (base.isClient)
				{
					CmdOnAnimationTriggerServerMessage(hash);
				}
				HandleAnimTriggerMsg(hash);
			}
			else if (!base.isServer)
			{
				Debug.LogWarning("Tried to set animation in the client for a server-controlled animator");
			}
			else
			{
				HandleAnimTriggerMsg(hash);
				RpcOnAnimationTriggerClientMessage(hash);
			}
		}

		public void ResetTrigger(string triggerName)
		{
			ResetTrigger(Animator.StringToHash(triggerName));
		}

		public void ResetTrigger(int hash)
		{
			if (clientAuthority)
			{
				if (!base.isClient)
				{
					Debug.LogWarning("Tried to reset animation in the server for a client-controlled animator");
					return;
				}
				if (!base.hasAuthority)
				{
					Debug.LogWarning("Only the client with authority can reset animations");
					return;
				}
				if (base.isClient)
				{
					CmdOnAnimationResetTriggerServerMessage(hash);
				}
				HandleAnimResetTriggerMsg(hash);
			}
			else if (!base.isServer)
			{
				Debug.LogWarning("Tried to reset animation in the client for a server-controlled animator");
			}
			else
			{
				HandleAnimResetTriggerMsg(hash);
				RpcOnAnimationResetTriggerClientMessage(hash);
			}
		}

		[Command]
		private void CmdOnAnimationServerMessage(int stateHash, float normalizedTime, int layerId, float weight, byte[] parameters)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteInt(stateHash);
			writer.WriteFloat(normalizedTime);
			writer.WriteInt(layerId);
			writer.WriteFloat(weight);
			writer.WriteBytesAndSize(parameters);
			SendCommandInternal(typeof(NetworkAnimator), "CmdOnAnimationServerMessage", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		private void CmdOnAnimationParametersServerMessage(byte[] parameters)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteBytesAndSize(parameters);
			SendCommandInternal(typeof(NetworkAnimator), "CmdOnAnimationParametersServerMessage", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		private void CmdOnAnimationTriggerServerMessage(int hash)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteInt(hash);
			SendCommandInternal(typeof(NetworkAnimator), "CmdOnAnimationTriggerServerMessage", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		private void CmdOnAnimationResetTriggerServerMessage(int hash)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteInt(hash);
			SendCommandInternal(typeof(NetworkAnimator), "CmdOnAnimationResetTriggerServerMessage", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[Command]
		private void CmdSetAnimatorSpeed(float newSpeed)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteFloat(newSpeed);
			SendCommandInternal(typeof(NetworkAnimator), "CmdSetAnimatorSpeed", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		[ClientRpc]
		private void RpcOnAnimationClientMessage(int stateHash, float normalizedTime, int layerId, float weight, byte[] parameters)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteInt(stateHash);
			writer.WriteFloat(normalizedTime);
			writer.WriteInt(layerId);
			writer.WriteFloat(weight);
			writer.WriteBytesAndSize(parameters);
			SendRPCInternal(typeof(NetworkAnimator), "RpcOnAnimationClientMessage", writer, 0, true);
			NetworkWriterPool.Recycle(writer);
		}

		[ClientRpc]
		private void RpcOnAnimationParametersClientMessage(byte[] parameters)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteBytesAndSize(parameters);
			SendRPCInternal(typeof(NetworkAnimator), "RpcOnAnimationParametersClientMessage", writer, 0, true);
			NetworkWriterPool.Recycle(writer);
		}

		[ClientRpc]
		private void RpcOnAnimationTriggerClientMessage(int hash)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteInt(hash);
			SendRPCInternal(typeof(NetworkAnimator), "RpcOnAnimationTriggerClientMessage", writer, 0, true);
			NetworkWriterPool.Recycle(writer);
		}

		[ClientRpc]
		private void RpcOnAnimationResetTriggerClientMessage(int hash)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteInt(hash);
			SendRPCInternal(typeof(NetworkAnimator), "RpcOnAnimationResetTriggerClientMessage", writer, 0, true);
			NetworkWriterPool.Recycle(writer);
		}

		[CompilerGenerated]
		private bool _003CAwake_003Eb__14_0(AnimatorControllerParameter par)
		{
			return !animator.IsParameterControlledByCurve(par.nameHash);
		}

		private void MirrorProcessed()
		{
		}

		protected void UserCode_CmdOnAnimationServerMessage(int stateHash, float normalizedTime, int layerId, float weight, byte[] parameters)
		{
			if (!clientAuthority)
			{
				return;
			}
			using (PooledNetworkReader reader = NetworkReaderPool.GetReader(parameters))
			{
				HandleAnimMsg(stateHash, normalizedTime, layerId, weight, reader);
				RpcOnAnimationClientMessage(stateHash, normalizedTime, layerId, weight, parameters);
			}
		}

		protected static void InvokeUserCode_CmdOnAnimationServerMessage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdOnAnimationServerMessage called on client.");
			}
			else
			{
				((NetworkAnimator)obj).UserCode_CmdOnAnimationServerMessage(reader.ReadInt(), reader.ReadFloat(), reader.ReadInt(), reader.ReadFloat(), reader.ReadBytesAndSize());
			}
		}

		protected void UserCode_CmdOnAnimationParametersServerMessage(byte[] parameters)
		{
			if (!clientAuthority)
			{
				return;
			}
			using (PooledNetworkReader reader = NetworkReaderPool.GetReader(parameters))
			{
				HandleAnimParamsMsg(reader);
				RpcOnAnimationParametersClientMessage(parameters);
			}
		}

		protected static void InvokeUserCode_CmdOnAnimationParametersServerMessage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdOnAnimationParametersServerMessage called on client.");
			}
			else
			{
				((NetworkAnimator)obj).UserCode_CmdOnAnimationParametersServerMessage(reader.ReadBytesAndSize());
			}
		}

		protected void UserCode_CmdOnAnimationTriggerServerMessage(int hash)
		{
			if (clientAuthority)
			{
				if (!base.isClient || !base.hasAuthority)
				{
					HandleAnimTriggerMsg(hash);
				}
				RpcOnAnimationTriggerClientMessage(hash);
			}
		}

		protected static void InvokeUserCode_CmdOnAnimationTriggerServerMessage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdOnAnimationTriggerServerMessage called on client.");
			}
			else
			{
				((NetworkAnimator)obj).UserCode_CmdOnAnimationTriggerServerMessage(reader.ReadInt());
			}
		}

		protected void UserCode_CmdOnAnimationResetTriggerServerMessage(int hash)
		{
			if (clientAuthority)
			{
				if (!base.isClient || !base.hasAuthority)
				{
					HandleAnimResetTriggerMsg(hash);
				}
				RpcOnAnimationResetTriggerClientMessage(hash);
			}
		}

		protected static void InvokeUserCode_CmdOnAnimationResetTriggerServerMessage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdOnAnimationResetTriggerServerMessage called on client.");
			}
			else
			{
				((NetworkAnimator)obj).UserCode_CmdOnAnimationResetTriggerServerMessage(reader.ReadInt());
			}
		}

		protected void UserCode_CmdSetAnimatorSpeed(float newSpeed)
		{
			animator.speed = newSpeed;
			NetworkanimatorSpeed = newSpeed;
		}

		protected static void InvokeUserCode_CmdSetAnimatorSpeed(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdSetAnimatorSpeed called on client.");
			}
			else
			{
				((NetworkAnimator)obj).UserCode_CmdSetAnimatorSpeed(reader.ReadFloat());
			}
		}

		protected void UserCode_RpcOnAnimationClientMessage(int stateHash, float normalizedTime, int layerId, float weight, byte[] parameters)
		{
			using (PooledNetworkReader reader = NetworkReaderPool.GetReader(parameters))
			{
				HandleAnimMsg(stateHash, normalizedTime, layerId, weight, reader);
			}
		}

		protected static void InvokeUserCode_RpcOnAnimationClientMessage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("RPC RpcOnAnimationClientMessage called on server.");
			}
			else
			{
				((NetworkAnimator)obj).UserCode_RpcOnAnimationClientMessage(reader.ReadInt(), reader.ReadFloat(), reader.ReadInt(), reader.ReadFloat(), reader.ReadBytesAndSize());
			}
		}

		protected void UserCode_RpcOnAnimationParametersClientMessage(byte[] parameters)
		{
			using (PooledNetworkReader reader = NetworkReaderPool.GetReader(parameters))
			{
				HandleAnimParamsMsg(reader);
			}
		}

		protected static void InvokeUserCode_RpcOnAnimationParametersClientMessage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("RPC RpcOnAnimationParametersClientMessage called on server.");
			}
			else
			{
				((NetworkAnimator)obj).UserCode_RpcOnAnimationParametersClientMessage(reader.ReadBytesAndSize());
			}
		}

		protected void UserCode_RpcOnAnimationTriggerClientMessage(int hash)
		{
			if (!base.isServer && (!clientAuthority || !base.hasAuthority))
			{
				HandleAnimTriggerMsg(hash);
			}
		}

		protected static void InvokeUserCode_RpcOnAnimationTriggerClientMessage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("RPC RpcOnAnimationTriggerClientMessage called on server.");
			}
			else
			{
				((NetworkAnimator)obj).UserCode_RpcOnAnimationTriggerClientMessage(reader.ReadInt());
			}
		}

		protected void UserCode_RpcOnAnimationResetTriggerClientMessage(int hash)
		{
			if (!base.isServer && (!clientAuthority || !base.hasAuthority))
			{
				HandleAnimResetTriggerMsg(hash);
			}
		}

		protected static void InvokeUserCode_RpcOnAnimationResetTriggerClientMessage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("RPC RpcOnAnimationResetTriggerClientMessage called on server.");
			}
			else
			{
				((NetworkAnimator)obj).UserCode_RpcOnAnimationResetTriggerClientMessage(reader.ReadInt());
			}
		}

		static NetworkAnimator()
		{
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkAnimator), "CmdOnAnimationServerMessage", InvokeUserCode_CmdOnAnimationServerMessage, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkAnimator), "CmdOnAnimationParametersServerMessage", InvokeUserCode_CmdOnAnimationParametersServerMessage, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkAnimator), "CmdOnAnimationTriggerServerMessage", InvokeUserCode_CmdOnAnimationTriggerServerMessage, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkAnimator), "CmdOnAnimationResetTriggerServerMessage", InvokeUserCode_CmdOnAnimationResetTriggerServerMessage, true);
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkAnimator), "CmdSetAnimatorSpeed", InvokeUserCode_CmdSetAnimatorSpeed, true);
			RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkAnimator), "RpcOnAnimationClientMessage", InvokeUserCode_RpcOnAnimationClientMessage);
			RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkAnimator), "RpcOnAnimationParametersClientMessage", InvokeUserCode_RpcOnAnimationParametersClientMessage);
			RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkAnimator), "RpcOnAnimationTriggerClientMessage", InvokeUserCode_RpcOnAnimationTriggerClientMessage);
			RemoteCallHelper.RegisterRpcDelegate(typeof(NetworkAnimator), "RpcOnAnimationResetTriggerClientMessage", InvokeUserCode_RpcOnAnimationResetTriggerClientMessage);
		}

		protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
		{
			bool result = base.SerializeSyncVars(writer, forceAll);
			if (forceAll)
			{
				writer.WriteFloat(animatorSpeed);
				return true;
			}
			writer.WriteULong(base.syncVarDirtyBits);
			if ((base.syncVarDirtyBits & 1L) != 0L)
			{
				writer.WriteFloat(animatorSpeed);
				result = true;
			}
			return result;
		}

		protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
		{
			base.DeserializeSyncVars(reader, initialState);
			if (initialState)
			{
				float num = animatorSpeed;
				NetworkanimatorSpeed = reader.ReadFloat();
				if (!SyncVarEqual(num, ref animatorSpeed))
				{
					OnAnimatorSpeedChanged(num, animatorSpeed);
				}
				return;
			}
			long num2 = (long)reader.ReadULong();
			if ((num2 & 1L) != 0L)
			{
				float num3 = animatorSpeed;
				NetworkanimatorSpeed = reader.ReadFloat();
				if (!SyncVarEqual(num3, ref animatorSpeed))
				{
					OnAnimatorSpeedChanged(num3, animatorSpeed);
				}
			}
		}
	}
}
