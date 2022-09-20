using System.Runtime.InteropServices;
using Mirror;

public class NPCJob : NetworkBehaviour
{
	[SyncVar(hook = "onChangeWork")]
	public bool atWork;

	[SyncVar(hook = "onVendorChange")]
	public int vendorId;

	private NPCAI myAi;

	private NPCIdentity myIdenity;

	public bool NetworkatWork
	{
		get
		{
			return atWork;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref atWork))
			{
				bool old = atWork;
				SetSyncVar(value, ref atWork, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onChangeWork(old, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public int NetworkvendorId
	{
		get
		{
			return vendorId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref vendorId))
			{
				int old = vendorId;
				SetSyncVar(value, ref vendorId, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, true);
					onVendorChange(old, value);
					setSyncVarHookGuard(2uL, false);
				}
			}
		}
	}

	private void Awake()
	{
		myIdenity = GetComponent<NPCIdentity>();
		myAi = GetComponent<NPCAI>();
	}

	public override void OnStartClient()
	{
		onChangeWork(atWork, atWork);
		onVendorChange(vendorId, vendorId);
	}

	private void OnDisable()
	{
		if (base.isServer)
		{
			NetworkatWork = false;
			if (vendorId != 0)
			{
				NPCManager.manage.vendorNPCs[vendorId] = null;
				NetworkvendorId = 0;
			}
		}
	}

	public override void OnStartServer()
	{
		NetworkvendorId = (int)NPCManager.manage.NPCDetails[myIdenity.NPCNo].workLocation;
		if (myIdenity.NPCNo == 5)
		{
			NetworkatWork = true;
		}
		onChangeWork(atWork, atWork);
	}

	public bool isRunningLate()
	{
		if (atWork || myIdenity.NPCNo == 6)
		{
			return false;
		}
		return NPCManager.manage.NPCDetails[myIdenity.NPCNo].mySchedual.checkIfLate();
	}

	public void onChangeWork(bool old, bool newAtWork)
	{
		NetworkatWork = newAtWork;
	}

	public void onVendorChange(int old, int newId)
	{
		NetworkvendorId = newId;
		if (newId != 0)
		{
			NPCManager.manage.vendorNPCs[newId] = myAi;
		}
	}

	private void MirrorProcessed()
	{
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(atWork);
			writer.WriteInt(vendorId);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(atWork);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(vendorId);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = atWork;
			NetworkatWork = reader.ReadBool();
			if (!SyncVarEqual(flag, ref atWork))
			{
				onChangeWork(flag, atWork);
			}
			int num = vendorId;
			NetworkvendorId = reader.ReadInt();
			if (!SyncVarEqual(num, ref vendorId))
			{
				onVendorChange(num, vendorId);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			bool flag2 = atWork;
			NetworkatWork = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref atWork))
			{
				onChangeWork(flag2, atWork);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			int num3 = vendorId;
			NetworkvendorId = reader.ReadInt();
			if (!SyncVarEqual(num3, ref vendorId))
			{
				onVendorChange(num3, vendorId);
			}
		}
	}
}
