using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Mirror
{
	internal static class NetworkLoop
	{
		internal enum AddMode
		{
			Beginning = 0,
			End = 1
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass1_0
		{
			public PlayerLoopSystem.UpdateFunction function;

			internal bool _003CFindPlayerLoopEntryIndex_003Eb__0(PlayerLoopSystem elem)
			{
				return elem.updateDelegate == function;
			}
		}

		internal static int FindPlayerLoopEntryIndex(PlayerLoopSystem.UpdateFunction function, PlayerLoopSystem playerLoop, Type playerLoopSystemType)
		{
			_003C_003Ec__DisplayClass1_0 _003C_003Ec__DisplayClass1_ = new _003C_003Ec__DisplayClass1_0();
			_003C_003Ec__DisplayClass1_.function = function;
			if (playerLoop.type == playerLoopSystemType)
			{
				return Array.FindIndex(playerLoop.subSystemList, _003C_003Ec__DisplayClass1_._003CFindPlayerLoopEntryIndex_003Eb__0);
			}
			if (playerLoop.subSystemList != null)
			{
				for (int i = 0; i < playerLoop.subSystemList.Length; i++)
				{
					int num = FindPlayerLoopEntryIndex(_003C_003Ec__DisplayClass1_.function, playerLoop.subSystemList[i], playerLoopSystemType);
					if (num != -1)
					{
						return num;
					}
				}
			}
			return -1;
		}

		internal static bool AddToPlayerLoop(PlayerLoopSystem.UpdateFunction function, Type ownerType, ref PlayerLoopSystem playerLoop, Type playerLoopSystemType, AddMode addMode)
		{
			if (playerLoop.type == playerLoopSystemType)
			{
				int num = ((playerLoop.subSystemList != null) ? playerLoop.subSystemList.Length : 0);
				Array.Resize(ref playerLoop.subSystemList, num + 1);
				PlayerLoopSystem playerLoopSystem = default(PlayerLoopSystem);
				playerLoopSystem.type = ownerType;
				playerLoopSystem.updateDelegate = function;
				PlayerLoopSystem playerLoopSystem2 = playerLoopSystem;
				switch (addMode)
				{
				case AddMode.Beginning:
					Array.Copy(playerLoop.subSystemList, 0, playerLoop.subSystemList, 1, playerLoop.subSystemList.Length - 1);
					playerLoop.subSystemList[0] = playerLoopSystem2;
					break;
				case AddMode.End:
					playerLoop.subSystemList[num] = playerLoopSystem2;
					break;
				}
				return true;
			}
			if (playerLoop.subSystemList != null)
			{
				for (int i = 0; i < playerLoop.subSystemList.Length; i++)
				{
					if (AddToPlayerLoop(function, ownerType, ref playerLoop.subSystemList[i], playerLoopSystemType, addMode))
					{
						return true;
					}
				}
			}
			return false;
		}

		[RuntimeInitializeOnLoadMethod]
		private static void RuntimeInitializeOnLoad()
		{
			PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
			AddToPlayerLoop(NetworkEarlyUpdate, typeof(NetworkLoop), ref playerLoop, typeof(EarlyUpdate), AddMode.End);
			AddToPlayerLoop(NetworkLateUpdate, typeof(NetworkLoop), ref playerLoop, typeof(PreLateUpdate), AddMode.End);
			PlayerLoop.SetPlayerLoop(playerLoop);
		}

		private static void NetworkEarlyUpdate()
		{
			NetworkServer.NetworkEarlyUpdate();
			NetworkClient.NetworkEarlyUpdate();
		}

		private static void NetworkLateUpdate()
		{
			NetworkServer.NetworkLateUpdate();
			NetworkClient.NetworkLateUpdate();
		}
	}
}
