using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class FarmAnimalHarvest : NetworkBehaviour
{
	[SyncVar(hook = "onAnimalHarvest")]
	private bool harvestReady;

	public InventoryItem aquireOnHarvest;

	public InventoryItem higherQualityHarvest;

	public InventoryItem requiredToHarvest;

	public string harvestVerb = "Milk";

	private FarmAnimal myAnimal;

	public bool harvestToInv = true;

	public ASound harvestSound;

	public GameObject showWhenReadyToHarvest;

	public DailyTaskGenerator.genericTaskType taskWhenHarvested;

	public bool NetworkharvestReady
	{
		get
		{
			return harvestReady;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref harvestReady))
			{
				bool old = harvestReady;
				SetSyncVar(value, ref harvestReady, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onAnimalHarvest(old, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	private void Awake()
	{
		myAnimal = GetComponent<FarmAnimal>();
		onAnimalHarvest(harvestReady, harvestReady);
	}

	public bool checkIfCanHarvest(InventoryItem itemInHand)
	{
		if (harvestReady && (!requiredToHarvest || requiredToHarvest == itemInHand))
		{
			return true;
		}
		return false;
	}

	public void setHarvestReadyServer()
	{
		NetworkharvestReady = true;
	}

	public void harvestFromServer()
	{
		NetworkharvestReady = false;
	}

	public InventoryItem getHarvestedItem()
	{
		if (!myAnimal || myAnimal.getDetails() == null)
		{
			return aquireOnHarvest;
		}
		if (!higherQualityHarvest)
		{
			return aquireOnHarvest;
		}
		if (myAnimal.getDetails().animalRelationShip >= Random.Range(60, 85))
		{
			return higherQualityHarvest;
		}
		return aquireOnHarvest;
	}

	[TargetRpc]
	public void TargetGiveItemToNonLocal(NetworkConnection con, int itemId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		SendTargetRPCInternal(con, typeof(FarmAnimalHarvest), "TargetGiveItemToNonLocal", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public string getToolTip(InventoryItem itemInHand)
	{
		if (harvestReady && (!requiredToHarvest || requiredToHarvest == itemInHand))
		{
			return harvestVerb + " " + myAnimal.getAnimalName();
		}
		return "";
	}

	public void onAnimalHarvest(bool old, bool newHarvest)
	{
		NetworkharvestReady = newHarvest;
		if (!newHarvest)
		{
			if ((bool)harvestSound)
			{
				SoundManager.manage.playASoundAtPoint(harvestSound, base.transform.position);
			}
			myAnimal.harvestAnimalAnimation();
		}
		if ((bool)showWhenReadyToHarvest)
		{
			showWhenReadyToHarvest.SetActive(newHarvest);
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_TargetGiveItemToNonLocal(NetworkConnection con, int itemId)
	{
		if (!Inventory.inv.addItemToInventory(itemId, 1))
		{
			NetworkMapSharer.share.localChar.CmdDropItem(itemId, 1, base.transform.position, base.transform.position);
		}
	}

	protected static void InvokeUserCode_TargetGiveItemToNonLocal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetGiveItemToNonLocal called on server.");
		}
		else
		{
			((FarmAnimalHarvest)obj).UserCode_TargetGiveItemToNonLocal(NetworkClient.readyConnection, reader.ReadInt());
		}
	}

	static FarmAnimalHarvest()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(FarmAnimalHarvest), "TargetGiveItemToNonLocal", InvokeUserCode_TargetGiveItemToNonLocal);
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(harvestReady);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(harvestReady);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = harvestReady;
			NetworkharvestReady = reader.ReadBool();
			if (!SyncVarEqual(flag, ref harvestReady))
			{
				onAnimalHarvest(flag, harvestReady);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool flag2 = harvestReady;
			NetworkharvestReady = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref harvestReady))
			{
				onAnimalHarvest(flag2, harvestReady);
			}
		}
	}
}
