using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.RemoteCalls
{
	public static class RemoteCallHelper
	{
		private static readonly Dictionary<int, Invoker> cmdHandlerDelegates = new Dictionary<int, Invoker>();

		internal static int GetMethodHash(Type invokeClass, string methodName)
		{
			return invokeClass.FullName.GetStableHashCode() * 503 + methodName.GetStableHashCode();
		}

		internal static int RegisterDelegate(Type invokeClass, string cmdName, MirrorInvokeType invokerType, CmdDelegate func, bool cmdRequiresAuthority = true)
		{
			int methodHash = GetMethodHash(invokeClass, cmdName);
			if (CheckIfDeligateExists(invokeClass, invokerType, func, methodHash))
			{
				return methodHash;
			}
			Invoker value = new Invoker
			{
				invokeType = invokerType,
				invokeClass = invokeClass,
				invokeFunction = func,
				cmdRequiresAuthority = cmdRequiresAuthority
			};
			cmdHandlerDelegates[methodHash] = value;
			return methodHash;
		}

		private static bool CheckIfDeligateExists(Type invokeClass, MirrorInvokeType invokerType, CmdDelegate func, int cmdHash)
		{
			if (cmdHandlerDelegates.ContainsKey(cmdHash))
			{
				Invoker invoker = cmdHandlerDelegates[cmdHash];
				if (invoker.AreEqual(invokeClass, invokerType, func))
				{
					return true;
				}
				Debug.LogError(string.Format("Function {0}.{1} and {2}.{3} have the same hash.  Please rename one of them", invoker.invokeClass, invoker.invokeFunction.GetMethodName(), invokeClass, func.GetMethodName()));
			}
			return false;
		}

		public static void RegisterCommandDelegate(Type invokeClass, string cmdName, CmdDelegate func, bool requiresAuthority)
		{
			RegisterDelegate(invokeClass, cmdName, MirrorInvokeType.Command, func, requiresAuthority);
		}

		public static void RegisterRpcDelegate(Type invokeClass, string rpcName, CmdDelegate func)
		{
			RegisterDelegate(invokeClass, rpcName, MirrorInvokeType.ClientRpc, func);
		}

		internal static void RemoveDelegate(int hash)
		{
			cmdHandlerDelegates.Remove(hash);
		}

		private static bool GetInvokerForHash(int cmdHash, MirrorInvokeType invokeType, out Invoker invoker)
		{
			if (cmdHandlerDelegates.TryGetValue(cmdHash, out invoker) && invoker != null && invoker.invokeType == invokeType)
			{
				return true;
			}
			return false;
		}

		internal static bool InvokeHandlerDelegate(int cmdHash, MirrorInvokeType invokeType, NetworkReader reader, NetworkBehaviour invokingType, NetworkConnectionToClient senderConnection = null)
		{
			Invoker invoker;
			if (GetInvokerForHash(cmdHash, invokeType, out invoker) && invoker.invokeClass.IsInstanceOfType(invokingType))
			{
				invoker.invokeFunction(invokingType, reader, senderConnection);
				return true;
			}
			return false;
		}

		internal static CommandInfo GetCommandInfo(int cmdHash, NetworkBehaviour invokingType)
		{
			Invoker invoker;
			if (GetInvokerForHash(cmdHash, MirrorInvokeType.Command, out invoker) && invoker.invokeClass.IsInstanceOfType(invokingType))
			{
				CommandInfo result = default(CommandInfo);
				result.requiresAuthority = invoker.cmdRequiresAuthority;
				return result;
			}
			return default(CommandInfo);
		}

		public static CmdDelegate GetDelegate(int cmdHash)
		{
			Invoker value;
			if (cmdHandlerDelegates.TryGetValue(cmdHash, out value))
			{
				return value.invokeFunction;
			}
			return null;
		}
	}
}
