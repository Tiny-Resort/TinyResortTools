using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Tractor : NetworkBehaviour
{
	private enum TractorAttachment
	{
		None = 0,
		Harvester = 1,
		Cultivator = 2,
		Planter = 3
	}

	public Vehicle connectedVehicle;

	public InventoryItem fertilizer;

	public ControlVehicle control;

	public Transform[] harvestableSpot;

	public LayerMask dropLayer;

	public Animator attachmentAnimator;

	public AudioSource attachmentAudio;

	[SyncVar(hook = "onChangeAttachment")]
	public int attached = 1;

	public GameObject attachmentHolder;

	public GameObject[] attachmentsToShow;

	[SyncVar]
	public bool usingAttachment;

	private float baseSpeed;

	private float turningSpeed;

	public Transform wallCheckerAttachment;

	public Transform wallCheckerNoAttachment;

	public ASound changeAttachmentSound;

	public TractorLight[] lights;

	private float animationSpeed;

	public int Networkattached
	{
		get
		{
			return attached;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref attached))
			{
				int oldAttach = attached;
				SetSyncVar(value, ref attached, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onChangeAttachment(oldAttach, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public bool NetworkusingAttachment
	{
		get
		{
			return usingAttachment;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref usingAttachment))
			{
				bool flag = usingAttachment;
				SetSyncVar(value, ref usingAttachment, 2uL);
			}
		}
	}

	public override void OnStartClient()
	{
		StartCoroutine(harvester());
		StartCoroutine(animateAttachments());
		baseSpeed = control.maxSpeed;
		turningSpeed = control.turnSpeed;
		onChangeAttachment(attached, attached);
	}

	public void onChangeAttachment(int oldAttach, int newAttach)
	{
		animationSpeed = 0f;
		Networkattached = newAttach;
		for (int i = 0; i < attachmentsToShow.Length; i++)
		{
			attachmentsToShow[i].SetActive(false);
		}
		attachmentHolder.SetActive(attached > 0);
		attachmentsToShow[newAttach].SetActive(true);
		attachmentAnimator.SetTrigger("SwapAttachment");
		if (newAttach == 0)
		{
			control.wallChecker = wallCheckerNoAttachment;
		}
		else
		{
			control.wallChecker = wallCheckerAttachment;
		}
		SoundManager.manage.playASoundAtPoint(changeAttachmentSound, base.transform.position);
		for (int j = 0; j < lights.Length; j++)
		{
			lights[j].updateLight(attached);
		}
	}

	public override void OnStartAuthority()
	{
		CmdSwapAttachments(0);
	}

	private IEnumerator harvester()
	{
		while (true)
		{
			yield return null;
			if (!connectedVehicle.hasDriver())
			{
				while (!connectedVehicle.hasDriver())
				{
					yield return null;
				}
				yield return new WaitForSeconds(0.5f);
			}
			if (base.hasAuthority && connectedVehicle.hasDriver() && InputMaster.input.VehicleUseHeld() && Inventory.inv.canMoveChar())
			{
				if (!usingAttachment)
				{
					NetworkusingAttachment = true;
					CmdChangeUsing(true);
				}
				if (attached == 1)
				{
					for (int i = 0; i < harvestableSpot.Length; i++)
					{
						if (checkIfTileIsHarvestable(Mathf.RoundToInt(harvestableSpot[i].position.x / 2f), Mathf.RoundToInt(harvestableSpot[i].position.z / 2f)))
						{
							harvestTile(Mathf.RoundToInt(harvestableSpot[i].position.x / 2f), Mathf.RoundToInt(harvestableSpot[i].position.z / 2f));
						}
					}
					if (!Inventory.inv.invOpen && Physics.CheckSphere(base.transform.position + base.transform.forward * 2f, 5f, dropLayer))
					{
						Collider[] array = Physics.OverlapSphere(base.transform.position + base.transform.forward * 2f, 5f, dropLayer);
						for (int j = 0; j < array.Length; j++)
						{
							DroppedItem component = array[j].GetComponent<DroppedItem>();
							if (((bool)component && (bool)Inventory.inv.allItems[component.myItemId].consumeable && Inventory.inv.allItems[component.myItemId].consumeable.isVegitable) || ((bool)component && (bool)Inventory.inv.allItems[component.myItemId].consumeable && Inventory.inv.allItems[component.myItemId].consumeable.isFruit))
							{
								if (Inventory.inv.addItemToInventory(component.myItemId, component.stackAmount))
								{
									SoundManager.manage.play2DSound(SoundManager.manage.pickUpItem);
									component.pickUpLocal();
									NetworkMapSharer.share.localChar.myPickUp.CmdPickUp(component.netId);
								}
								else
								{
									NotificationManager.manage.turnOnPocketsFullNotification(true);
								}
							}
						}
					}
				}
				else if (attached == 2)
				{
					for (int k = 0; k < harvestableSpot.Length; k++)
					{
						if (checkIfTileCanBeHoed(Mathf.RoundToInt(harvestableSpot[k].position.x / 2f), Mathf.RoundToInt(harvestableSpot[k].position.z / 2f)))
						{
							CmdHoeTile(Mathf.RoundToInt(harvestableSpot[k].position.x / 2f), Mathf.RoundToInt(harvestableSpot[k].position.z / 2f));
							WorldManager.manageWorld.lockTileClient(Mathf.RoundToInt(harvestableSpot[k].position.x / 2f), Mathf.RoundToInt(harvestableSpot[k].position.z / 2f));
						}
					}
				}
				else if (attached == 3)
				{
					if (!Inventory.inv.invOpen && Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemNo > 1 && (bool)Inventory.inv.allItems[Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemNo].placeable && (bool)Inventory.inv.allItems[Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemNo].placeable.tileObjectGrowthStages && Inventory.inv.allItems[Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemNo].placeable.tileObjectGrowthStages.needsTilledSoil)
					{
						for (int l = 0; l < harvestableSpot.Length; l++)
						{
							if (Inventory.inv.invSlots[Inventory.inv.selectedSlot].stack > 0 && checkIfCanBePlantedHere(Mathf.RoundToInt(harvestableSpot[l].position.x / 2f), Mathf.RoundToInt(harvestableSpot[l].position.z / 2f)))
							{
								CmdPlantItem(Inventory.inv.allItems[Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemNo].placeable.tileObjectId, Mathf.RoundToInt(harvestableSpot[l].position.x / 2f), Mathf.RoundToInt(harvestableSpot[l].position.z / 2f));
								WorldManager.manageWorld.lockTileClient(Mathf.RoundToInt(harvestableSpot[l].position.x / 2f), Mathf.RoundToInt(harvestableSpot[l].position.z / 2f));
								Inventory.inv.invSlots[Inventory.inv.selectedSlot].updateSlotContentsAndRefresh(Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemNo, Inventory.inv.invSlots[Inventory.inv.selectedSlot].stack - 1);
							}
						}
					}
					else if (!Inventory.inv.invOpen && Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemInSlot == fertilizer)
					{
						for (int m = 0; m < harvestableSpot.Length; m++)
						{
							if (Inventory.inv.invSlots[Inventory.inv.selectedSlot].stack > 0 && checkIfCanBeFertilized(Mathf.RoundToInt(harvestableSpot[m].position.x / 2f), Mathf.RoundToInt(harvestableSpot[m].position.z / 2f)))
							{
								CmdPlaceFertilizer(Mathf.RoundToInt(harvestableSpot[m].position.x / 2f), Mathf.RoundToInt(harvestableSpot[m].position.z / 2f));
								WorldManager.manageWorld.lockTileClient(Mathf.RoundToInt(harvestableSpot[m].position.x / 2f), Mathf.RoundToInt(harvestableSpot[m].position.z / 2f));
								Inventory.inv.invSlots[Inventory.inv.selectedSlot].updateSlotContentsAndRefresh(Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemNo, Inventory.inv.invSlots[Inventory.inv.selectedSlot].stack - 1);
							}
						}
					}
				}
			}
			else if (usingAttachment)
			{
				NetworkusingAttachment = false;
				CmdChangeUsing(false);
			}
			if (base.hasAuthority && connectedVehicle.hasDriver() && InputMaster.input.VehicleInteract() && Inventory.inv.canMoveChar())
			{
				CmdSwapAttachments(LicenceManager.manage.allLicences[18].getCurrentLevel());
				NetworkusingAttachment = false;
				yield return new WaitForSeconds(0.65f);
			}
		}
	}

	public bool checkIfTileIsHarvestable(int xPos, int yPos)
	{
		if (!WorldManager.manageWorld.checkTileClientLock(xPos, yPos) && WorldManager.manageWorld.onTileMap[xPos, yPos] > 0 && (bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages)
		{
			if (Mathf.Abs(base.transform.position.y - (float)WorldManager.manageWorld.heightMap[xPos, yPos]) > 1.5f)
			{
				return false;
			}
			if (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.needsTilledSoil || ((bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectConnect && (bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectConnect.canConnectTo[0].tileObjectGrowthStages && WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectConnect.canConnectTo[0].tileObjectGrowthStages.needsTilledSoil))
			{
				if (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.canTractorHarvest(WorldManager.manageWorld.onTileStatusMap[xPos, yPos]))
				{
					return true;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	public bool checkIfTileCanBeHoed(int xPos, int yPos)
	{
		if (Mathf.Abs(base.transform.position.y - (float)WorldManager.manageWorld.heightMap[xPos, yPos]) > 1.5f)
		{
			return false;
		}
		if (WorldManager.manageWorld.checkTileClientLock(xPos, yPos) || (WorldManager.manageWorld.onTileMap[xPos, yPos] > -1 && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].isGrass && WorldManager.manageWorld.onTileMap[xPos, yPos] != 30))
		{
			return false;
		}
		if (WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 7 || WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 8 || WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 12 || WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 13 || (bool)WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos, yPos]].dropOnChange)
		{
			return false;
		}
		return true;
	}

	public bool checkIfCanBePlantedHere(int xPos, int yPos)
	{
		if (Mathf.Abs(base.transform.position.y - (float)WorldManager.manageWorld.heightMap[xPos, yPos]) > 1.5f)
		{
			return false;
		}
		if (WorldManager.manageWorld.checkTileClientLock(xPos, yPos))
		{
			return false;
		}
		if ((WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 7 || WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 8 || WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 12 || WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 13) && (WorldManager.manageWorld.onTileMap[xPos, yPos] == -1 || WorldManager.manageWorld.onTileMap[xPos, yPos] == 30))
		{
			return true;
		}
		return false;
	}

	public bool checkIfCanBeFertilized(int xPos, int yPos)
	{
		if (Mathf.Abs(base.transform.position.y - (float)WorldManager.manageWorld.heightMap[xPos, yPos]) > 1.5f)
		{
			return false;
		}
		if (WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 7 || WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 8)
		{
			return true;
		}
		return false;
	}

	private void harvestTile(int xPos, int yPos)
	{
		if (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.harvestableByHand)
		{
			NetworkMapSharer.share.localChar.myInteract.CmdHarvestOnTile(xPos, yPos, false);
		}
		else if (!WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.steamsOutInto)
		{
			CmdDestroyObject(xPos, yPos);
		}
		WorldManager.manageWorld.lockTileClient(xPos, yPos);
	}

	[Command]
	private void CmdDestroyObject(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(Tractor), "CmdDestroyObject", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPlantItem(int plantId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(plantId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(Tractor), "CmdPlantItem", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator animateAttachments()
	{
		while (true)
		{
			bool hasAuthority2 = base.hasAuthority;
			if (attached != 0)
			{
				if (usingAttachment)
				{
					animationSpeed = Mathf.Lerp(animationSpeed, 1f, Time.deltaTime);
				}
				else if (animationSpeed > 0f)
				{
					animationSpeed = Mathf.Lerp(animationSpeed, 0f, Time.deltaTime);
					if (animationSpeed < 0.05f)
					{
						animationSpeed = 0f;
					}
				}
				attachmentAudio.volume = animationSpeed * 0.35f * SoundManager.manage.getSoundVolume();
				attachmentAudio.pitch = 0.85f + animationSpeed / 2f;
				attachmentAnimator.SetFloat("AnimationSpeed", animationSpeed);
				attachmentAnimator.SetBool("UsingAttachment", usingAttachment);
			}
			yield return null;
		}
	}

	[Command]
	private void CmdChangeUsing(bool newUsing)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(newUsing);
		SendCommandInternal(typeof(Tractor), "CmdChangeUsing", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdHoeTile(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(Tractor), "CmdHoeTile", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdSwapAttachments(int licenceLevel)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(licenceLevel);
		SendCommandInternal(typeof(Tractor), "CmdSwapAttachments", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPlaceFertilizer(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(Tractor), "CmdPlaceFertilizer", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdDestroyObject(int xPos, int yPos)
	{
		if (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.milestoneOnHarvest != 0)
		{
			NetworkMapSharer.share.TargetGiveHarvestMilestone(base.connectionToClient, WorldManager.manageWorld.onTileMap[xPos, yPos]);
		}
		NetworkMapSharer.share.RpcHarvestObject(-1, xPos, yPos, true);
	}

	protected static void InvokeUserCode_CmdDestroyObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDestroyObject called on client.");
		}
		else
		{
			((Tractor)obj).UserCode_CmdDestroyObject(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlantItem(int plantId, int xPos, int yPos)
	{
		NetworkMapSharer.share.RpcUpdateOnTileObject(plantId, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdPlantItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlantItem called on client.");
		}
		else
		{
			((Tractor)obj).UserCode_CmdPlantItem(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeUsing(bool newUsing)
	{
		NetworkusingAttachment = newUsing;
	}

	protected static void InvokeUserCode_CmdChangeUsing(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeUsing called on client.");
		}
		else
		{
			((Tractor)obj).UserCode_CmdChangeUsing(reader.ReadBool());
		}
	}

	protected void UserCode_CmdHoeTile(int xPos, int yPos)
	{
		NetworkMapSharer.share.RpcUpdateTileType(7, xPos, yPos);
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] > -1 && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].isGrass)
		{
			NetworkMapSharer.share.RpcUpdateOnTileObject(-1, xPos, yPos);
		}
	}

	protected static void InvokeUserCode_CmdHoeTile(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHoeTile called on client.");
		}
		else
		{
			((Tractor)obj).UserCode_CmdHoeTile(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSwapAttachments(int licenceLevel)
	{
		NetworkusingAttachment = false;
		int num = attached + 1;
		if (num > licenceLevel)
		{
			Networkattached = 0;
		}
		else
		{
			Networkattached = num;
		}
	}

	protected static void InvokeUserCode_CmdSwapAttachments(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSwapAttachments called on client.");
		}
		else
		{
			((Tractor)obj).UserCode_CmdSwapAttachments(reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceFertilizer(int xPos, int yPos)
	{
		NetworkMapSharer.share.RpcUpdateTileType(fertilizer.getResultingPlaceableTileType(WorldManager.manageWorld.tileTypeMap[xPos, yPos]), xPos, yPos);
	}

	protected static void InvokeUserCode_CmdPlaceFertilizer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceFertilizer called on client.");
		}
		else
		{
			((Tractor)obj).UserCode_CmdPlaceFertilizer(reader.ReadInt(), reader.ReadInt());
		}
	}

	static Tractor()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(Tractor), "CmdDestroyObject", InvokeUserCode_CmdDestroyObject, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Tractor), "CmdPlantItem", InvokeUserCode_CmdPlantItem, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Tractor), "CmdChangeUsing", InvokeUserCode_CmdChangeUsing, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Tractor), "CmdHoeTile", InvokeUserCode_CmdHoeTile, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Tractor), "CmdSwapAttachments", InvokeUserCode_CmdSwapAttachments, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Tractor), "CmdPlaceFertilizer", InvokeUserCode_CmdPlaceFertilizer, true);
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(attached);
			writer.WriteBool(usingAttachment);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(attached);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(usingAttachment);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = attached;
			Networkattached = reader.ReadInt();
			if (!SyncVarEqual(num, ref attached))
			{
				onChangeAttachment(num, attached);
			}
			bool flag = usingAttachment;
			NetworkusingAttachment = reader.ReadBool();
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			int num3 = attached;
			Networkattached = reader.ReadInt();
			if (!SyncVarEqual(num3, ref attached))
			{
				onChangeAttachment(num3, attached);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			bool flag2 = usingAttachment;
			NetworkusingAttachment = reader.ReadBool();
		}
	}
}
