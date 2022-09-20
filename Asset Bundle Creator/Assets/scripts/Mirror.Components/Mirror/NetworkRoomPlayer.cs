using System.Runtime.InteropServices;
using Mirror.RemoteCalls;
using UnityEngine;

namespace Mirror
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Network/NetworkRoomPlayer")]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-room-player")]
	public class NetworkRoomPlayer : NetworkBehaviour
	{
		[Tooltip("This flag controls whether the default UI is shown for the room player")]
		public bool showRoomGUI = true;

		[Header("Diagnostics")]
		[Tooltip("Diagnostic flag indicating whether this player is ready for the game to begin")]
		[SyncVar(hook = "ReadyStateChanged")]
		public bool readyToBegin;

		[Tooltip("Diagnostic index of the player, e.g. Player1, Player2, etc.")]
		[SyncVar(hook = "IndexChanged")]
		public int index;

		public bool NetworkreadyToBegin
		{
			get
			{
				return readyToBegin;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref readyToBegin))
				{
					bool oldReadyState = readyToBegin;
					SetSyncVar(value, ref readyToBegin, 1uL);
					if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
					{
						setSyncVarHookGuard(1uL, true);
						ReadyStateChanged(oldReadyState, value);
						setSyncVarHookGuard(1uL, false);
					}
				}
			}
		}

		public int Networkindex
		{
			get
			{
				return index;
			}
			[param: In]
			set
			{
				if (!SyncVarEqual(value, ref index))
				{
					int oldIndex = index;
					SetSyncVar(value, ref index, 2uL);
					if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
					{
						setSyncVarHookGuard(2uL, true);
						IndexChanged(oldIndex, value);
						setSyncVarHookGuard(2uL, false);
					}
				}
			}
		}

		public void Start()
		{
			NetworkRoomManager networkRoomManager = NetworkManager.singleton as NetworkRoomManager;
			if ((object)networkRoomManager != null)
			{
				if (networkRoomManager.dontDestroyOnLoad)
				{
					Object.DontDestroyOnLoad(base.gameObject);
				}
				networkRoomManager.roomSlots.Add(this);
				if (NetworkServer.active)
				{
					networkRoomManager.RecalculateRoomPlayerIndices();
				}
				if (NetworkClient.active)
				{
					networkRoomManager.CallOnClientEnterRoom();
				}
			}
			else
			{
				Debug.LogError("RoomPlayer could not find a NetworkRoomManager. The RoomPlayer requires a NetworkRoomManager object to function. Make sure that there is one in the scene.");
			}
		}

		public virtual void OnDisable()
		{
			if (NetworkClient.active)
			{
				NetworkRoomManager networkRoomManager = NetworkManager.singleton as NetworkRoomManager;
				if ((object)networkRoomManager != null)
				{
					networkRoomManager.roomSlots.Remove(this);
					networkRoomManager.CallOnClientExitRoom();
				}
			}
		}

		[Command]
		public void CmdChangeReadyState(bool readyState)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteBool(readyState);
			SendCommandInternal(typeof(NetworkRoomPlayer), "CmdChangeReadyState", writer, 0);
			NetworkWriterPool.Recycle(writer);
		}

		public virtual void IndexChanged(int oldIndex, int newIndex)
		{
		}

		public virtual void ReadyStateChanged(bool oldReadyState, bool newReadyState)
		{
		}

		public virtual void OnClientEnterRoom()
		{
		}

		public virtual void OnClientExitRoom()
		{
		}

		public virtual void OnGUI()
		{
			if (showRoomGUI)
			{
				NetworkRoomManager networkRoomManager = NetworkManager.singleton as NetworkRoomManager;
				if ((bool)networkRoomManager && networkRoomManager.showRoomGUI && NetworkManager.IsSceneActive(networkRoomManager.RoomScene))
				{
					DrawPlayerReadyState();
					DrawPlayerReadyButton();
				}
			}
		}

		private void DrawPlayerReadyState()
		{
			GUILayout.BeginArea(new Rect(20f + (float)(index * 100), 200f, 90f, 130f));
			GUILayout.Label(string.Format("Player [{0}]", index + 1));
			if (readyToBegin)
			{
				GUILayout.Label("Ready");
			}
			else
			{
				GUILayout.Label("Not Ready");
			}
			if (((base.isServer && index > 0) || base.isServerOnly) && GUILayout.Button("REMOVE"))
			{
				GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
			}
			GUILayout.EndArea();
		}

		private void DrawPlayerReadyButton()
		{
			if (!NetworkClient.active || !base.isLocalPlayer)
			{
				return;
			}
			GUILayout.BeginArea(new Rect(20f, 300f, 120f, 20f));
			if (readyToBegin)
			{
				if (GUILayout.Button("Cancel"))
				{
					CmdChangeReadyState(false);
				}
			}
			else if (GUILayout.Button("Ready"))
			{
				CmdChangeReadyState(true);
			}
			GUILayout.EndArea();
		}

		private void MirrorProcessed()
		{
		}

		protected void UserCode_CmdChangeReadyState(bool readyState)
		{
			NetworkreadyToBegin = readyState;
			NetworkRoomManager networkRoomManager = NetworkManager.singleton as NetworkRoomManager;
			if (networkRoomManager != null)
			{
				networkRoomManager.ReadyStatusChanged();
			}
		}

		protected static void InvokeUserCode_CmdChangeReadyState(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdChangeReadyState called on client.");
			}
			else
			{
				((NetworkRoomPlayer)obj).UserCode_CmdChangeReadyState(reader.ReadBool());
			}
		}

		static NetworkRoomPlayer()
		{
			RemoteCallHelper.RegisterCommandDelegate(typeof(NetworkRoomPlayer), "CmdChangeReadyState", InvokeUserCode_CmdChangeReadyState, true);
		}

		protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
		{
			bool result = base.SerializeSyncVars(writer, forceAll);
			if (forceAll)
			{
				writer.WriteBool(readyToBegin);
				writer.WriteInt(index);
				return true;
			}
			writer.WriteULong(base.syncVarDirtyBits);
			if ((base.syncVarDirtyBits & 1L) != 0L)
			{
				writer.WriteBool(readyToBegin);
				result = true;
			}
			if ((base.syncVarDirtyBits & 2L) != 0L)
			{
				writer.WriteInt(index);
				result = true;
			}
			return result;
		}

		protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
		{
			base.DeserializeSyncVars(reader, initialState);
			if (initialState)
			{
				bool flag = readyToBegin;
				NetworkreadyToBegin = reader.ReadBool();
				if (!SyncVarEqual(flag, ref readyToBegin))
				{
					ReadyStateChanged(flag, readyToBegin);
				}
				int num = index;
				Networkindex = reader.ReadInt();
				if (!SyncVarEqual(num, ref index))
				{
					IndexChanged(num, index);
				}
				return;
			}
			long num2 = (long)reader.ReadULong();
			if ((num2 & 1L) != 0L)
			{
				bool flag2 = readyToBegin;
				NetworkreadyToBegin = reader.ReadBool();
				if (!SyncVarEqual(flag2, ref readyToBegin))
				{
					ReadyStateChanged(flag2, readyToBegin);
				}
			}
			if ((num2 & 2L) != 0L)
			{
				int num3 = index;
				Networkindex = reader.ReadInt();
				if (!SyncVarEqual(num3, ref index))
				{
					IndexChanged(num3, index);
				}
			}
		}
	}
}
