using System.Collections;
using System.Runtime.InteropServices;
using I2.Loc;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class CharPickUp : NetworkBehaviour
{
	public LayerMask pickUpLayerMask;

	public LayerMask vehicleMask;

	public CharInteract myInteract;

	public GameObject[] hideStuff;

	private CharTalkUse myTalkTo;

	private CharMovement myChar;

	public bool sitting;

	private int sittingInSeat;

	public int sittingXpos;

	public int sittingYPos;

	public Transform sittingPosition;

	public bool drivingVehicle;

	private uint drivingVehicleId;

	[SyncVar(hook = "onCarryChanged")]
	private uint carrying;

	public EquipItemToChar myEquip;

	[SyncVar(hook = "onSittingChanged")]
	public Vector3 sittingPos;

	public bool holdingPickUp;

	public NetworkFishingRod netRod;

	public Vehicle currentlyDriving;

	private bool justSat;

	private AnimalCarryBox carriedAnimal;

	private TrappedAnimal trappedAnimal;

	private bool sittingInHairDresserSeat;

	private Vector3 lastLayedDownPos;

	private HouseDetails sleepingInside;

	private int sittingLayingOrStanding;

	public uint Networkcarrying
	{
		get
		{
			return carrying;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref carrying))
			{
				uint oldCarry = carrying;
				SetSyncVar(value, ref carrying, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onCarryChanged(oldCarry, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public Vector3 NetworksittingPos
	{
		get
		{
			return sittingPos;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref sittingPos))
			{
				Vector3 old = sittingPos;
				SetSyncVar(value, ref sittingPos, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, true);
					onSittingChanged(old, value);
					setSyncVarHookGuard(2uL, false);
				}
			}
		}
	}

	private void Start()
	{
		myChar = GetComponent<CharMovement>();
		myTalkTo = GetComponent<CharTalkUse>();
		myInteract = GetComponent<CharInteract>();
		myEquip = GetComponent<EquipItemToChar>();
		netRod = GetComponent<NetworkFishingRod>();
		hideStuff[3] = StatusManager.manage.statusWindow.gameObject;
		hideStuff[2] = Inventory.inv.quickSlotBar.gameObject;
	}

	public override void OnStopClient()
	{
		if (base.isServer)
		{
			if (carrying != 0)
			{
				NetworkIdentity.spawned[carrying].GetComponent<PickUpAndCarry>().NetworkbeingCarriedBy = 0u;
			}
			if (sittingInHairDresserSeat)
			{
				CmdGetUpFromHairDresserSeat();
			}
			else if (sitting)
			{
				CmdGetUp(sittingInSeat, sittingXpos, sittingYPos);
			}
			if (drivingVehicle)
			{
				CmdStopDriving(drivingVehicleId);
			}
		}
	}

	private IEnumerator justSatDelay()
	{
		yield return new WaitForSeconds(0.5f);
		justSat = false;
	}

	private void Update()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (!Inventory.inv.canMoveChar())
		{
			if (GiveNPC.give.giveWindowOpen || Inventory.inv.invOpen)
			{
				if (GiveNPC.give.giveWindowOpen)
				{
					NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.InGiveMenu);
				}
				else if (Inventory.inv.dragSlot.itemNo > -1 && Inventory.inv.allItems[Inventory.inv.dragSlot.itemNo].checkIfStackable())
				{
					NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.InChestWhileHoldingItem);
				}
				else
				{
					NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.InChest);
				}
			}
			else
			{
				NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.None);
			}
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if (myChar.swimming && !myChar.underWater)
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.Dive);
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if (netRod.lineIsCasted)
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.Fishing);
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if (carrying != 0)
		{
			if ((!myEquip.isInside() && (bool)carriedAnimal) || (!myEquip.isInside() && (bool)trappedAnimal))
			{
				NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.CarryingAnimal);
			}
			else
			{
				NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.CarryingItem);
			}
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if (myInteract.placingDeed)
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.multiTiledPlacing);
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if (drivingVehicle)
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.StopDriving);
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if (sitting)
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.GetUp);
			NotificationManager.manage.hideButtonPrompt();
			return;
		}
		if (myInteract.myEquip.currentlyHoldingSinglePlaceableItem() && myInteract.myEquip.currentlyHolding.placeable.getsRotationFromMap())
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.singleTiledPlacing);
		}
		else if (myInteract.canTileBePickedUp())
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.PickUp);
		}
		else
		{
			NotificationManager.manage.hintWindowOpen(NotificationManager.toolTipType.None);
		}
		RaycastHit hitInfo;
		int xPos;
		int yPos;
		if (Physics.Raycast(base.transform.position + base.transform.forward * 1.5f + Vector3.up * 3f, Vector3.down, out hitInfo, 3.1f, pickUpLayerMask))
		{
			DroppedItem component = hitInfo.transform.GetComponent<DroppedItem>();
			if ((bool)component)
			{
				if (holdingPickUp)
				{
					if (Inventory.inv.addItemToInventory(component.myItemId, component.stackAmount))
					{
						SoundManager.manage.play2DSound(SoundManager.manage.pickUpItem);
						bool isServer2 = base.isServer;
						CmdPickUp(component.netId);
						component.pickUpLocal();
					}
					else
					{
						NotificationManager.manage.turnOnPocketsFullNotification(holdingPickUp);
					}
					NotificationManager.manage.showButtonPrompt(Inventory.inv.allItems[component.myItemId].getInvItemName(), "B", hitInfo.transform.position);
				}
				else if (component.stackAmount > 1 && !Inventory.inv.allItems[component.myItemId].hasFuel && !Inventory.inv.allItems[component.myItemId].hasColourVariation)
				{
					NotificationManager.manage.showButtonPrompt(Inventory.inv.allItems[component.myItemId].getInvItemName() + " X " + component.stackAmount, "B", hitInfo.transform.position);
				}
				else
				{
					NotificationManager.manage.showButtonPrompt(Inventory.inv.allItems[component.myItemId].getInvItemName(), "B", hitInfo.transform.position);
				}
				return;
			}
			if (NetworkMapSharer.share.localChar.underWater)
			{
				BugTypes componentInParent = hitInfo.transform.GetComponentInParent<BugTypes>();
				if ((bool)componentInParent && componentInParent.isUnderwaterCreature)
				{
					NotificationManager.manage.showButtonPrompt("Catch " + Inventory.inv.allItems[componentInParent.getBugTypeId()].getInvItemName(), "B", hitInfo.transform.position);
				}
				return;
			}
			InteractableObject interactableObject = hitInfo.transform.GetComponentInParent<InteractableObject>();
			if ((bool)interactableObject && hitInfo.collider.tag != "Wheelbarrow")
			{
				if (hitInfo.collider.tag == "Multiple")
				{
					interactableObject = hitInfo.collider.GetComponent<InteractableObject>();
				}
				interactableObject.showingToolTip(hitInfo.transform, this);
			}
		}
		else if (myTalkTo.npcInRange != -1)
		{
			if (((bool)myTalkTo.npcTryToTalk && myTalkTo.npcTryToTalk.canBeTalkTo()) || ((bool)myTalkTo.npcTryToTalk && myTalkTo.npcTryToTalk.canBeTalkedToFollowing()))
			{
				NotificationManager.manage.showButtonPrompt(string.Concat((LocalizedString)"ToolTips/Tip_TalkTo", " ", NPCManager.manage.NPCDetails[myTalkTo.npcInRange].NPCName), "B", myTalkTo.npcTryToTalk.transform.position + Vector3.up);
			}
			else
			{
				NotificationManager.manage.showButtonPrompt(NPCManager.manage.NPCDetails[myTalkTo.npcInRange].NPCName + " " + (LocalizedString)"ToolTips/Tip_IsBusy", "no", myTalkTo.npcTryToTalk.transform.position + Vector3.up);
			}
		}
		else if (!myInteract.isInside() && myInteract.tileCloseNeedsPrompt(out xPos, out yPos) != 0)
		{
			NotificationManager.manage.showButtonPrompt(myInteract.getPromptStringTile(myInteract.tileCloseNeedsPrompt(out xPos, out yPos), xPos, yPos), "B", new Vector3(xPos * 2, base.transform.position.y, yPos * 2));
		}
		else
		{
			NotificationManager.manage.hideButtonPrompt();
		}
	}

	public void pressX()
	{
		if (carrying != 0)
		{
			if ((!myEquip.isInside() && (bool)carriedAnimal) || (!myEquip.isInside() && (bool)trappedAnimal))
			{
				if (carrying != 0 && myEquip.isCarrying())
				{
					RaycastHit hitInfo;
					if (Physics.Raycast(base.transform.position + Vector3.up * 2f + base.transform.forward * 2.5f, Vector3.down, out hitInfo, 15f, GetComponent<CharMovement>().jumpLayers))
					{
						CmdDropAndRelase(hitInfo.point.y);
						myEquip.setCarrying(false);
					}
					else
					{
						SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
					}
				}
			}
			else if ((myEquip.isInside() && (bool)carriedAnimal) || (myEquip.isInside() && (bool)trappedAnimal))
			{
				SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
			}
			return;
		}
		RaycastHit hitInfo2;
		if (Physics.Raycast(base.transform.position + base.transform.forward * 1.5f + Vector3.up * 3f, Vector3.down, out hitInfo2, 3f, vehicleMask))
		{
			VehicleHitBox componentInParent = hitInfo2.transform.GetComponentInParent<VehicleHitBox>();
			if ((bool)componentInParent && componentInParent.connectedTo.canBePainted && myEquip.currentlyHoldingNo > 0 && (bool)Inventory.inv.allItems[myEquip.currentlyHoldingNo].GetComponent<PaintCan>())
			{
				CmdPaintVehicle(componentInParent.connectedTo.netId, (int)Inventory.inv.allItems[myEquip.currentlyHoldingNo].GetComponent<PaintCan>().colourId);
				Inventory.inv.consumeItemInHand();
			}
		}
		if (!Physics.Raycast(base.transform.position + base.transform.forward * 1.5f + Vector3.up * 3f, Vector3.down, out hitInfo2, 3f, pickUpLayerMask))
		{
			return;
		}
		ItemDepositAndChanger componentInParent2 = hitInfo2.transform.GetComponentInParent<ItemDepositAndChanger>();
		if ((bool)componentInParent2 && (bool)myInteract.myEquip.currentlyHolding && (bool)myInteract.myEquip.currentlyHolding == componentInParent2.canDepositThisItem(myInteract.myEquip.currentlyHolding, myInteract.insideHouseDetails))
		{
			myInteract.insertItemInTo(componentInParent2);
		}
		TileObjectGrowthStages componentInParent3 = hitInfo2.transform.GetComponentInParent<TileObjectGrowthStages>();
		if ((bool)componentInParent3 && (bool)myInteract.myEquip.currentlyHolding)
		{
			for (int i = 0; i < componentInParent3.itemsToPlace.Length; i++)
			{
				if (myInteract.myEquip.currentlyHolding == componentInParent3.itemsToPlace[i])
				{
					TileObject componentInParent4 = componentInParent3.GetComponentInParent<TileObject>();
					if (WorldManager.manageWorld.onTileStatusMap[componentInParent4.xPos, componentInParent4.yPos] < componentInParent3.maxStageToReachByPlacing)
					{
						Inventory.inv.consumeItemInHand();
						CmdChangeStatus(componentInParent4.xPos, componentInParent4.yPos, WorldManager.manageWorld.onTileStatusMap[componentInParent4.xPos, componentInParent4.yPos] + 1);
					}
				}
			}
		}
		Wheelbarrow componentInParent5 = hitInfo2.transform.GetComponentInParent<Wheelbarrow>();
		if ((bool)componentInParent5 && (bool)myInteract.myEquip.currentlyHolding)
		{
			if ((myInteract.myEquip.currentlyHolding == componentInParent5.emptyShovel && componentInParent5.totalDirt <= 0) || (componentInParent5.isHoldingAShovel(myInteract.myEquip.currentlyHolding) && componentInParent5.totalDirt >= 10))
			{
				GetComponent<Animator>().SetTrigger("Clang");
			}
			else if (myInteract.myEquip.currentlyHolding == componentInParent5.emptyShovel && componentInParent5.totalDirt > 0)
			{
				myEquip.currentlyHolding.changeToWhenUsed = WorldManager.manageWorld.tileTypes[componentInParent5.topDirtId].uniqueShovel;
				GetComponent<Animator>().SetTrigger("WheelBarrow");
				Inventory.inv.changeItemInHand();
				CmdRemoveFromBarrow(componentInParent5.netId);
			}
			else if (componentInParent5.isHoldingAShovel(myInteract.myEquip.currentlyHolding) && componentInParent5.totalDirt < 10)
			{
				CmdAddToBarrow(componentInParent5.netId, myInteract.myEquip.currentlyHolding.resultingTileType[0]);
				GetComponent<Animator>().SetTrigger("WheelBarrow");
				Inventory.inv.changeItemInHand();
			}
		}
		FarmAnimal componentInParent6 = hitInfo2.transform.GetComponentInParent<FarmAnimal>();
		if ((bool)componentInParent6 && (bool)componentInParent6.canBeHarvested && componentInParent6.canBeHarvested.checkIfCanHarvest(myEquip.currentlyHolding))
		{
			if (componentInParent6.canBeHarvested.taskWhenHarvested != 0)
			{
				DailyTaskGenerator.generate.doATask(componentInParent6.canBeHarvested.taskWhenHarvested);
			}
			if (componentInParent6.canBeHarvested.harvestToInv)
			{
				CmdHarvestAnimalToInv(componentInParent6.netId);
			}
			else
			{
				CmdHarvestAnimal(componentInParent6.netId);
			}
			return;
		}
		ItemSign componentInParent7 = hitInfo2.transform.GetComponentInParent<ItemSign>();
		if ((bool)componentInParent7)
		{
			if (componentInParent7.isSilo)
			{
				if (myEquip.currentlyHoldingNo == Inventory.inv.getInvItemId(componentInParent7.itemCanPlaceIn))
				{
					TileObject componentInParent8 = componentInParent7.GetComponentInParent<TileObject>();
					if (WorldManager.manageWorld.onTileStatusMap[componentInParent8.xPos, componentInParent8.yPos] < 200)
					{
						Inventory.inv.consumeItemInHand();
						myInteract.CmdPlayPlaceableAnimation();
						CmdAddToSilo(componentInParent8.xPos, componentInParent8.yPos);
					}
				}
			}
			else if ((bool)myEquip.currentlyHolding)
			{
				TileObject component = componentInParent7.GetComponent<TileObject>();
				CmdChangeSignItem(myEquip.currentlyHoldingNo, component.xPos, component.yPos);
			}
		}
		AnimalAI componentInParent9 = hitInfo2.transform.GetComponentInParent<AnimalAI>();
		if ((bool)componentInParent9 && myEquip.currentlyHoldingNo > 0 && (bool)Inventory.inv.allItems[myEquip.currentlyHoldingNo].GetComponent<PlaceOnAnimal>() && componentInParent9.animalId == Inventory.inv.allItems[myEquip.currentlyHoldingNo].GetComponent<PlaceOnAnimal>().toPlaceOn.animalId)
		{
			CmdPlaceOntoAnimal(myEquip.currentlyHoldingNo, componentInParent9.netId);
			Inventory.inv.consumeItemInHand();
		}
	}

	public bool isCarryingSomething()
	{
		if (carrying != 0)
		{
			return true;
		}
		return false;
	}

	public void pressY()
	{
		if (!Inventory.inv.canMoveChar())
		{
			return;
		}
		if (drivingVehicle)
		{
			GetComponent<CharMovement>().getOutVehicle();
			drivingVehicle = false;
			CmdStopDriving(drivingVehicleId);
			myEquip.setDriving(false);
		}
		else if (sitting && !justSat)
		{
			MonoBehaviour.print("Pressing Y while seated");
			MonoBehaviour.print(sitting + " " + justSat);
			if (sittingInHairDresserSeat && !ConversationManager.manage.inConversation)
			{
				StopCoroutine("talkToHairDresserOnceInSeat");
				CmdGetUpFromHairDresserSeat();
				sitting = false;
			}
			else if (!sittingInHairDresserSeat)
			{
				myEquip.setLayDown(false);
				CmdGetUp(sittingInSeat, sittingXpos, sittingYPos);
				sitting = false;
			}
			if (!sitting)
			{
				GetComponent<Rigidbody>().isKinematic = false;
				GetComponent<Animator>().SetTrigger("Standing");
				GetComponent<Animator>().SetBool("SittingOrLaying", false);
				base.transform.position = base.transform.position + base.transform.forward;
			}
			justSat = true;
			StartCoroutine(justSatDelay());
		}
	}

	[ClientRpc]
	public void RpcStopDrivingFromServer()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CharPickUp), "RpcStopDrivingFromServer", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	public bool pickUp()
	{
		if (!MenuButtonsTop.menu.closed || !Inventory.inv.canMoveChar())
		{
			return true;
		}
		if (carrying != 0 && myEquip.isCarrying())
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(base.transform.position + Vector3.up * 2f + base.transform.forward * 1.6f, Vector3.down, out hitInfo, 15f, pickUpLayerMask) && (bool)hitInfo.transform && hitInfo.transform.tag == "DropOffSpot")
			{
				CmdPutDownObjectInDropPoint(hitInfo.transform.position);
				myEquip.setCarrying(false);
			}
			else if (Physics.Raycast(base.transform.position + Vector3.up * 2f + base.transform.forward * 1.6f, Vector3.down, out hitInfo, 15f, GetComponent<CharMovement>().jumpLayers))
			{
				RaycastHit hitInfo2;
				Physics.Raycast(base.transform.position + Vector3.up * 15f + base.transform.forward * 1.6f, Vector3.down, out hitInfo2, 30f, GetComponent<CharMovement>().jumpLayers);
				if (hitInfo2.transform.gameObject.layer == LayerMask.NameToLayer("Building") || hitInfo2.transform.gameObject.tag == "Walls")
				{
					SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
				}
				else
				{
					if (hitInfo.transform.tag == "DropOffSpot")
					{
						CmdPutDownObjectInDropPoint(hitInfo.transform.position);
					}
					else
					{
						CmdPutDownObject(hitInfo.point.y);
					}
					myEquip.setCarrying(false);
				}
			}
			else
			{
				SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
			}
			return true;
		}
		if (drivingVehicle)
		{
			return true;
		}
		if (sitting && !justSat)
		{
			return true;
		}
		RaycastHit hitInfo3;
		if (Physics.Raycast(base.transform.position + base.transform.forward * 1.5f + Vector3.up * 3f, Vector3.down, out hitInfo3, 3.1f, pickUpLayerMask))
		{
			if (hitInfo3.collider.tag == "Multiple")
			{
				MonoBehaviour.print(hitInfo3.collider.name);
				InteractableObject component = hitInfo3.collider.GetComponent<InteractableObject>();
				if ((bool)component.isVehicle)
				{
					if (!drivingVehicle)
					{
						drivingVehicle = true;
						drivingVehicleId = component.isVehicle.netId;
						GetComponent<CharMovement>().getInVehicle(component.isVehicle);
						CmdStartDriving(component.isVehicle.netId);
						InputMaster.input.connectRumbleToVehicle(component.isVehicle.GetComponent<VehicleMakeParticles>());
						component.isVehicle.transform.position = component.isVehicle.transform.position;
						myEquip.setDriving(true);
						currentlyDriving = component.isVehicle;
					}
					return true;
				}
				if ((bool)component.isFarmAnimal)
				{
					if (component.isFarmAnimal.canPat)
					{
						if (!component.isFarmAnimal.hasBeenPatted)
						{
							DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.PetAnimals);
						}
						CmdPetAnimal(component.isFarmAnimal.netId);
					}
					return true;
				}
			}
			DroppedItem component2 = hitInfo3.transform.GetComponent<DroppedItem>();
			if ((bool)component2)
			{
				if (Inventory.inv.addItemToInventory(component2.myItemId, component2.stackAmount))
				{
					SoundManager.manage.play2DSound(SoundManager.manage.pickUpItem);
					CmdPickUp(component2.netId);
					component2.pickUpLocal();
				}
				else
				{
					NotificationManager.manage.turnOnPocketsFullNotification();
				}
				return true;
			}
			TileObjectAnimalHouse componentInParent = hitInfo3.transform.GetComponentInParent<TileObjectAnimalHouse>();
			if ((bool)componentInParent)
			{
				Inventory.inv.quickSlotBar.gameObject.SetActive(false);
				componentInParent.showHouseDetails();
				return true;
			}
			PickUpAndCarry componentInParent2 = hitInfo3.transform.GetComponentInParent<PickUpAndCarry>();
			if ((bool)componentInParent2 && componentInParent2.canBePickedUp)
			{
				CmdPickUpObject(componentInParent2.netId);
				myEquip.setCarrying(true);
				return true;
			}
			ShopBuyDrop componentInParent3 = hitInfo3.transform.GetComponentInParent<ShopBuyDrop>();
			if ((bool)componentInParent3)
			{
				if (componentInParent3.canTalkToKeeper() && !componentInParent3.isKeeperWorking())
				{
					if (componentInParent3.canTalkToKeeper() && (bool)componentInParent3.closedConversation)
					{
						componentInParent3.TryAndBuyItem();
					}
				}
				else
				{
					componentInParent3.TryAndBuyItem();
				}
				return true;
			}
			ChestPlaceable componentInParent4 = hitInfo3.transform.GetComponentInParent<ChestPlaceable>();
			if ((bool)componentInParent4)
			{
				if (componentInParent4.isStash)
				{
					ContainerManager.manage.openStash(0);
					CmdOpenStash(0);
				}
				else
				{
					CmdOpenChest(componentInParent4.myXPos(), componentInParent4.myYPos());
				}
				return true;
			}
			ReadableSign component3 = hitInfo3.transform.GetComponent<ReadableSign>();
			if ((bool)component3)
			{
				Inventory.inv.quickSlotBar.gameObject.SetActive(false);
				component3.readSign();
				return true;
			}
			if ((bool)hitInfo3.transform.GetComponentInParent<MailBoxShowsMail>())
			{
				MailManager.manage.openMailWindow();
				return true;
			}
			if ((bool)hitInfo3.transform.GetComponentInParent<BulletinBoardShowNewMessage>())
			{
				if (!base.isServer && !BulletinBoard.board.clientLoaded)
				{
					BulletinBoard.board.clientLoaded = true;
					Inventory.inv.quickSlotBar.gameObject.SetActive(false);
					CmdFillBulletinBoard();
					return true;
				}
				Inventory.inv.quickSlotBar.gameObject.SetActive(false);
				BulletinBoard.board.openWindow();
				return true;
			}
			WorkTable componentInParent5 = hitInfo3.transform.GetComponentInParent<WorkTable>();
			if ((bool)componentInParent5)
			{
				if ((bool)componentInParent5.tableText)
				{
					componentInParent5.checkForItemAndChangeText();
					Inventory.inv.quickSlotBar.gameObject.SetActive(false);
					componentInParent5.tableText.readSign();
				}
				else
				{
					Inventory.inv.quickSlotBar.gameObject.SetActive(false);
					CraftingManager.manage.openCloseCraftMenu(true, componentInParent5.typeOfCrafting);
				}
				return true;
			}
			FurnitureStatus componentInParent6 = hitInfo3.transform.GetComponentInParent<FurnitureStatus>();
			if ((bool)componentInParent6 && !justSat)
			{
				if (hitInfo3.transform == componentInParent6.seatPosition1.transform)
				{
					sittingInSeat = 1;
				}
				else if (hitInfo3.transform == componentInParent6.seatPosition2.transform)
				{
					sittingInSeat = 2;
				}
				sitting = true;
				sittingPosition = hitInfo3.transform;
				sittingXpos = componentInParent6.showingX;
				sittingYPos = componentInParent6.showingY;
				GetComponent<Rigidbody>().isKinematic = true;
				if ((bool)componentInParent6.GetComponent<HairDresserSeat>())
				{
					sittingInHairDresserSeat = true;
					StartCoroutine("talkToHairDresserOnceInSeat");
					sitting = true;
					CmdSitInHairDresserSeat(sittingPosition.position);
				}
				else
				{
					int num = -1;
					if (myInteract.insidePlayerHouse)
					{
						CmdSitDown(sittingInSeat, componentInParent6.showingX, componentInParent6.showingY, sittingPosition.position, myInteract.insideHouseDetails.xPos, myInteract.insideHouseDetails.yPos);
						num = myInteract.insideHouseDetails.houseMapOnTile[componentInParent6.showingX, componentInParent6.showingY];
						if (!componentInParent6.isSeat)
						{
							myEquip.setLayDown(true);
							WorldManager.manageWorld.confirmSleepSign.signSays = WorldManager.manageWorld.getSleepText();
							WorldManager.manageWorld.confirmSleepSign.readSign();
						}
					}
					else
					{
						CmdSitDown(sittingInSeat, componentInParent6.showingX, componentInParent6.showingY, sittingPosition.position, -1, -1);
						num = WorldManager.manageWorld.onTileMap[componentInParent6.showingX, componentInParent6.showingY];
						if (!componentInParent6.isSeat)
						{
							WorldManager.manageWorld.confirmSleepSign.signSays = WorldManager.manageWorld.getSleepText();
							WorldManager.manageWorld.confirmSleepSign.readSign();
						}
					}
					if (num >= 0 && (bool)WorldManager.manageWorld.allObjects[num].tileObjectFurniture && !WorldManager.manageWorld.allObjects[num].tileObjectFurniture.isSeat)
					{
						myEquip.setLayDown(true);
						lastLayedDownPos = base.transform.position;
						sleepingInside = myInteract.insideHouseDetails;
					}
				}
				justSat = true;
				StartCoroutine(justSatDelay());
				return true;
			}
			MineControls component4 = hitInfo3.transform.GetComponent<MineControls>();
			if ((bool)component4)
			{
				component4.useControls();
				return true;
			}
			Vehicle componentInParent7 = hitInfo3.transform.GetComponentInParent<Vehicle>();
			if ((bool)componentInParent7 && hitInfo3.collider.tag != "Wheelbarrow")
			{
				if (LicenceManager.manage.allLicences[7].getCurrentLevel() < componentInParent7.requiresLicenceLevel)
				{
					NotificationManager.manage.pocketsFull.showNoLicence(LicenceManager.LicenceTypes.Vehicle);
				}
				else if (!drivingVehicle)
				{
					drivingVehicle = true;
					drivingVehicleId = componentInParent7.netId;
					GetComponent<CharMovement>().getInVehicle(componentInParent7);
					CmdStartDriving(componentInParent7.netId);
					InputMaster.input.connectRumbleToVehicle(componentInParent7.GetComponent<VehicleMakeParticles>());
					componentInParent7.transform.position = componentInParent7.transform.position;
					myEquip.setDriving(true);
					currentlyDriving = componentInParent7;
				}
				return true;
			}
			FarmAnimal componentInParent8 = hitInfo3.transform.GetComponentInParent<FarmAnimal>();
			if ((bool)componentInParent8)
			{
				if (componentInParent8.canPat)
				{
					if (!componentInParent8.hasBeenPatted)
					{
						DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.PetAnimals);
					}
					CmdPetAnimal(componentInParent8.netId);
				}
				return true;
			}
			MuseumPainting componentInParent9 = hitInfo3.transform.GetComponentInParent<MuseumPainting>();
			if ((bool)componentInParent9)
			{
				if (componentInParent9.checkIfMuseumKeeperCanBeTalkedTo() && componentInParent9.checkIfMuseumKeeperIsAtWork())
				{
					componentInParent9.askAboutPainting();
				}
				return true;
			}
			if (NetworkMapSharer.share.localChar.underWater)
			{
				BugTypes componentInParent10 = hitInfo3.transform.GetComponentInParent<BugTypes>();
				if ((bool)componentInParent10 && componentInParent10.isUnderwaterCreature)
				{
					if (Inventory.inv.addItemToInventory(componentInParent10.getBugTypeId(), 1))
					{
						CharLevelManager.manage.addToDayTally(componentInParent10.getBugTypeId(), 1, 3);
						CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Fishing, 4);
						SoundManager.manage.play2DSound(SoundManager.manage.pickUpUnderwaterCreature);
						PediaManager.manage.addCaughtToList(componentInParent10.getBugTypeId());
						CmdCatchUnderwater(componentInParent10.netId);
					}
					else
					{
						NotificationManager.manage.turnOnPocketsFullNotification();
					}
				}
			}
			CharMovement componentInParent11 = hitInfo3.transform.GetComponentInParent<CharMovement>();
			if ((bool)componentInParent11)
			{
				componentInParent11.reviveBox.SetActive(false);
				CmdReviveOtherChar(componentInParent11.netId);
				return true;
			}
			return false;
		}
		return myTalkTo.talkOrUse();
	}

	private void onCarryChanged(uint oldCarry, uint newCarry)
	{
		if (base.isLocalPlayer)
		{
			carriedAnimal = null;
			trappedAnimal = null;
			if (newCarry != 0)
			{
				carriedAnimal = NetworkIdentity.spawned[newCarry].GetComponent<AnimalCarryBox>();
				trappedAnimal = NetworkIdentity.spawned[newCarry].GetComponent<TrappedAnimal>();
			}
		}
		Networkcarrying = newCarry;
	}

	private void onSittingChanged(Vector3 old, Vector3 newSittingPos)
	{
		NetworksittingPos = newSittingPos;
	}

	[Command]
	public void CmdChangeSignItem(int itemId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharPickUp), "CmdChangeSignItem", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdAddToSilo(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharPickUp), "CmdAddToSilo", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdHarvestAnimalToInv(uint animalToHarvest)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(animalToHarvest);
		SendCommandInternal(typeof(CharPickUp), "CmdHarvestAnimalToInv", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdHarvestAnimal(uint animalToHarvest)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(animalToHarvest);
		SendCommandInternal(typeof(CharPickUp), "CmdHarvestAnimal", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPetAnimal(uint animalToPet)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(animalToPet);
		SendCommandInternal(typeof(CharPickUp), "CmdPetAnimal", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdReviveOtherChar(uint reviveId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(reviveId);
		SendCommandInternal(typeof(CharPickUp), "CmdReviveOtherChar", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator reviveDelayServer(uint reviveId)
	{
		yield return new WaitForSeconds(1f);
		NetworkIdentity.spawned[reviveId].GetComponent<Damageable>().Networkhealth = 5;
		NetworkIdentity.spawned[reviveId].GetComponent<CharMovement>().Networkstamina = 5;
		NetworkMapSharer.share.TargetGiveStamina(NetworkIdentity.spawned[reviveId].connectionToClient);
	}

	private IEnumerator reviveDelayClient(CharMovement myChar)
	{
		myChar.reviveBox.SetActive(false);
		StartCoroutine(playPetAnimation());
		yield return new WaitForSeconds(1f);
		myChar.myAnim.SetBool("Fainted", false);
	}

	[ClientRpc]
	public void RpcReviveDelay(uint reviveId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(reviveId);
		SendRPCInternal(typeof(CharPickUp), "RpcReviveDelay", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdSitInHairDresserSeat(Vector3 newSittingPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(newSittingPos);
		SendCommandInternal(typeof(CharPickUp), "CmdSitInHairDresserSeat", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdGetUpFromHairDresserSeat()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharPickUp), "CmdGetUpFromHairDresserSeat", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdSitDown(int seatNo, int xPos, int yPos, Vector3 newSittingPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(seatNo);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteVector3(newSittingPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendCommandInternal(typeof(CharPickUp), "CmdSitDown", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void confirmSleep()
	{
		TownManager.manage.lastSleptPos = lastLayedDownPos;
		TownManager.manage.sleepInsideHouse = sleepingInside;
		CmdConfirmSleep();
	}

	[Command]
	private void CmdConfirmSleep()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharPickUp), "CmdConfirmSleep", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void SetReadyToSleep(bool ready)
	{
		if (ready)
		{
			NetworkNavMesh.nav.addSleepingChar(base.transform);
		}
		else
		{
			NetworkNavMesh.nav.removeSleepingChar(base.transform);
		}
	}

	[ClientRpc]
	private void RpcCharPetAnimation()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CharPickUp), "RpcCharPetAnimation", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator playPetAnimation()
	{
		GetComponent<Animator>().SetTrigger("Pet");
		if (base.isLocalPlayer)
		{
			myEquip.setPetting(true);
			CharMovement myMove = GetComponent<CharMovement>();
			myMove.attackLockOn(true);
			yield return new WaitForSeconds(1.8f);
			myMove.attackLockOn(false);
			myEquip.setPetting(false);
		}
	}

	[ClientRpc]
	private void RpcSittingLayingOrStanding(int stat)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(stat);
		SendRPCInternal(typeof(CharPickUp), "RpcSittingLayingOrStanding", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	public bool isLayingDown()
	{
		return sittingLayingOrStanding == 2;
	}

	[Command]
	private void CmdGetUp(int seatNo, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(seatNo);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharPickUp), "CmdGetUp", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPickUp(uint pickUpId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(pickUpId);
		SendCommandInternal(typeof(CharPickUp), "CmdPickUp", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdOpenChest(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharPickUp), "CmdOpenChest", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdOpenStash(int stashId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(stashId);
		SendCommandInternal(typeof(CharPickUp), "CmdOpenStash", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcPlayOpenStashSound()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CharPickUp), "RpcPlayOpenStashSound", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeOneInChest(int xPos, int yPos, int slotNo, int newSlotId, int newStackNo)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(slotNo);
		writer.WriteInt(newSlotId);
		writer.WriteInt(newStackNo);
		SendCommandInternal(typeof(CharPickUp), "CmdChangeOneInChest", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdStartDriving(uint vehicleToDrive)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(vehicleToDrive);
		SendCommandInternal(typeof(CharPickUp), "CmdStartDriving", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdStopDriving(uint vehicleToDrive)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(vehicleToDrive);
		SendCommandInternal(typeof(CharPickUp), "CmdStopDriving", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcDropCarriedItem()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CharPickUp), "RpcDropCarriedItem", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPickUpObject(uint pickUpObject)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(pickUpObject);
		SendCommandInternal(typeof(CharPickUp), "CmdPickUpObject", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPutDownObject(float heightDroppedAt)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteFloat(heightDroppedAt);
		SendCommandInternal(typeof(CharPickUp), "CmdPutDownObject", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPutDownObjectInDropPoint(Vector3 dropPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(dropPos);
		SendCommandInternal(typeof(CharPickUp), "CmdPutDownObjectInDropPoint", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdDropAndRelase(float heightDroppedAt)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteFloat(heightDroppedAt);
		SendCommandInternal(typeof(CharPickUp), "CmdDropAndRelase", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeStatus(int xPos, int yPos, int newStatus)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(newStatus);
		SendCommandInternal(typeof(CharPickUp), "CmdChangeStatus", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPlaceOntoAnimal(int itemPlacingOn, uint animalNetId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemPlacingOn);
		writer.WriteUInt(animalNetId);
		SendCommandInternal(typeof(CharPickUp), "CmdPlaceOntoAnimal", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdFillBulletinBoard()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharPickUp), "CmdFillBulletinBoard", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdAddToBarrow(uint barrowId, int tileTypeToAdd)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(barrowId);
		writer.WriteInt(tileTypeToAdd);
		SendCommandInternal(typeof(CharPickUp), "CmdAddToBarrow", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdRemoveFromBarrow(uint barrowId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(barrowId);
		SendCommandInternal(typeof(CharPickUp), "CmdRemoveFromBarrow", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPaintVehicle(uint vehicleId, int colourId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(vehicleId);
		writer.WriteInt(colourId);
		SendCommandInternal(typeof(CharPickUp), "CmdPaintVehicle", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcPaintVehicle(uint vehicle, int colourId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(vehicle);
		writer.WriteInt(colourId);
		SendRPCInternal(typeof(CharPickUp), "RpcPaintVehicle", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator talkToHairDresserOnceInSeat()
	{
		yield return new WaitForSeconds(0.25f);
		while (sittingInHairDresserSeat)
		{
			if ((bool)NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Hair_Dresser) && NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Hair_Dresser).isAtWork() && NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Hair_Dresser).canBeTalkTo())
			{
				ConversationManager.manage.talkToNPC(NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Hair_Dresser), HairDresserSeat.seat.hairDresserConversation);
				break;
			}
			yield return null;
		}
	}

	public void CmdCatchUnderwater(uint idToCatch)
	{
		NetworkNavMesh.nav.UnSpawnAnAnimal(NetworkIdentity.spawned[idToCatch].GetComponent<AnimalAI>(), false);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcStopDrivingFromServer()
	{
		GetComponent<CharMovement>().getOutVehicle();
		drivingVehicle = false;
		drivingVehicleId = 0u;
		myEquip.setDriving(false);
	}

	protected static void InvokeUserCode_RpcStopDrivingFromServer(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopDrivingFromServer called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcStopDrivingFromServer();
		}
	}

	protected void UserCode_CmdChangeSignItem(int itemId, int xPos, int yPos)
	{
		NetworkMapSharer.share.RpcGiveOnTileStatus(itemId, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdChangeSignItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeSignItem called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdChangeSignItem(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdAddToSilo(int xPos, int yPos)
	{
		NetworkMapSharer.share.RpcGiveOnTileStatus(WorldManager.manageWorld.onTileStatusMap[xPos, yPos] + 1, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdAddToSilo(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddToSilo called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdAddToSilo(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdHarvestAnimalToInv(uint animalToHarvest)
	{
		NetworkIdentity.spawned[animalToHarvest].GetComponent<FarmAnimalHarvest>().harvestFromServer();
		NetworkIdentity.spawned[animalToHarvest].GetComponent<FarmAnimalHarvest>().TargetGiveItemToNonLocal(base.connectionToClient, NetworkIdentity.spawned[animalToHarvest].GetComponent<FarmAnimalHarvest>().getHarvestedItem().getItemId());
	}

	protected static void InvokeUserCode_CmdHarvestAnimalToInv(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHarvestAnimalToInv called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdHarvestAnimalToInv(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdHarvestAnimal(uint animalToHarvest)
	{
		FarmAnimalHarvest component = NetworkIdentity.spawned[animalToHarvest].GetComponent<FarmAnimalHarvest>();
		component.harvestFromServer();
		NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(component.getHarvestedItem()), 1, component.transform.position + Vector3.up);
	}

	protected static void InvokeUserCode_CmdHarvestAnimal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHarvestAnimal called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdHarvestAnimal(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdPetAnimal(uint animalToPet)
	{
		RpcCharPetAnimation();
		if (NetworkIdentity.spawned.ContainsKey(animalToPet))
		{
			NetworkIdentity.spawned[animalToPet].GetComponent<FarmAnimal>().checkEffectOnPet(base.netId);
		}
	}

	protected static void InvokeUserCode_CmdPetAnimal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPetAnimal called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdPetAnimal(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdReviveOtherChar(uint reviveId)
	{
		RpcReviveDelay(reviveId);
		StartCoroutine(reviveDelayServer(reviveId));
	}

	protected static void InvokeUserCode_CmdReviveOtherChar(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdReviveOtherChar called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdReviveOtherChar(reader.ReadUInt());
		}
	}

	protected void UserCode_RpcReviveDelay(uint reviveId)
	{
		StartCoroutine(reviveDelayClient(NetworkIdentity.spawned[reviveId].GetComponent<CharMovement>()));
	}

	protected static void InvokeUserCode_RpcReviveDelay(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReviveDelay called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcReviveDelay(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdSitInHairDresserSeat(Vector3 newSittingPos)
	{
		sittingInHairDresserSeat = true;
		RpcSittingLayingOrStanding(1);
		NetworkMapSharer.share.NetworkhairDresserSeatOccupied = true;
	}

	protected static void InvokeUserCode_CmdSitInHairDresserSeat(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSitInHairDresserSeat called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdSitInHairDresserSeat(reader.ReadVector3());
		}
	}

	protected void UserCode_CmdGetUpFromHairDresserSeat()
	{
		sittingInHairDresserSeat = false;
		RpcSittingLayingOrStanding(0);
		NetworkMapSharer.share.NetworkhairDresserSeatOccupied = false;
	}

	protected static void InvokeUserCode_CmdGetUpFromHairDresserSeat(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGetUpFromHairDresserSeat called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdGetUpFromHairDresserSeat();
		}
	}

	protected void UserCode_CmdSitDown(int seatNo, int xPos, int yPos, Vector3 newSittingPos, int houseX, int houseY)
	{
		sitting = true;
		sittingInSeat = seatNo;
		sittingXpos = xPos;
		sittingYPos = yPos;
		NetworksittingPos = newSittingPos;
		HouseDetails houseDetails = null;
		if (houseX != -1 && houseY != -1)
		{
			houseDetails = HouseManager.manage.getHouseInfo(houseX, houseY);
		}
		if (houseX != -1 && houseY != -1)
		{
			int newSitPosition = houseDetails.houseMapOnTileStatus[xPos, yPos] + seatNo;
			NetworkMapSharer.share.RpcSitDown(newSitPosition, xPos, yPos, houseX, houseY);
		}
		else
		{
			int newSitPosition2 = WorldManager.manageWorld.onTileStatusMap[xPos, yPos] + seatNo;
			NetworkMapSharer.share.RpcSitDown(newSitPosition2, xPos, yPos, -1, -1);
		}
		bool flag = false;
		if ((houseDetails == null && WorldManager.manageWorld.onTileMap[xPos, yPos] > -1 && WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectFurniture.isSeat) || (houseDetails != null && houseDetails.houseMapOnTile[xPos, yPos] > -1 && WorldManager.manageWorld.allObjects[houseDetails.houseMapOnTile[xPos, yPos]].tileObjectFurniture.isSeat))
		{
			flag = true;
		}
		if (flag)
		{
			RpcSittingLayingOrStanding(1);
		}
		else
		{
			RpcSittingLayingOrStanding(2);
		}
	}

	protected static void InvokeUserCode_CmdSitDown(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSitDown called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdSitDown(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadVector3(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdConfirmSleep()
	{
		SetReadyToSleep(true);
	}

	protected static void InvokeUserCode_CmdConfirmSleep(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdConfirmSleep called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdConfirmSleep();
		}
	}

	protected void UserCode_RpcCharPetAnimation()
	{
		StartCoroutine(playPetAnimation());
	}

	protected static void InvokeUserCode_RpcCharPetAnimation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCharPetAnimation called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcCharPetAnimation();
		}
	}

	protected void UserCode_RpcSittingLayingOrStanding(int stat)
	{
		switch (stat)
		{
		case 0:
			GetComponent<Animator>().SetTrigger("Standing");
			GetComponent<Animator>().SetBool("SittingOrLaying", false);
			break;
		case 1:
			GetComponent<Animator>().SetTrigger("Sitting");
			GetComponent<Animator>().SetBool("SittingOrLaying", true);
			break;
		case 2:
			GetComponent<Animator>().SetTrigger("Laying");
			GetComponent<Animator>().SetBool("SittingOrLaying", true);
			break;
		}
		sittingLayingOrStanding = stat;
	}

	protected static void InvokeUserCode_RpcSittingLayingOrStanding(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSittingLayingOrStanding called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcSittingLayingOrStanding(reader.ReadInt());
		}
	}

	protected void UserCode_CmdGetUp(int seatNo, int xPos, int yPos)
	{
		NetworksittingPos = Vector3.zero;
		if (myInteract.insideHouseDetails != null)
		{
			int sitPosition = Mathf.Clamp(myInteract.insideHouseDetails.houseMapOnTileStatus[xPos, yPos] - seatNo, 0, 3);
			NetworkMapSharer.share.RpcGetUp(sitPosition, xPos, yPos, myInteract.insideHouseDetails.xPos, myInteract.insideHouseDetails.yPos);
		}
		else
		{
			int sitPosition2 = Mathf.Clamp(WorldManager.manageWorld.onTileStatusMap[xPos, yPos] - seatNo, 0, 3);
			NetworkMapSharer.share.RpcGetUp(sitPosition2, xPos, yPos, -1, -1);
		}
		SetReadyToSleep(false);
		RpcSittingLayingOrStanding(0);
	}

	protected static void InvokeUserCode_CmdGetUp(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGetUp called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdGetUp(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPickUp(uint pickUpId)
	{
		NetworkIdentity.spawned[pickUpId].GetComponent<DroppedItem>().pickUp();
		NetworkIdentity.spawned[pickUpId].GetComponent<DroppedItem>().RpcMoveTowardsPickedUpBy(base.netId);
	}

	protected static void InvokeUserCode_CmdPickUp(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUp called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdPickUp(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdOpenChest(int xPos, int yPos)
	{
		ContainerManager.manage.openChestFromServer(base.connectionToClient, xPos, yPos, myInteract.insideHouseDetails);
	}

	protected static void InvokeUserCode_CmdOpenChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOpenChest called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdOpenChest(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdOpenStash(int stashId)
	{
		RpcPlayOpenStashSound();
	}

	protected static void InvokeUserCode_CmdOpenStash(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOpenStash called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdOpenStash(reader.ReadInt());
		}
	}

	protected void UserCode_RpcPlayOpenStashSound()
	{
	}

	protected static void InvokeUserCode_RpcPlayOpenStashSound(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayOpenStashSound called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcPlayOpenStashSound();
		}
	}

	protected void UserCode_CmdChangeOneInChest(int xPos, int yPos, int slotNo, int newSlotId, int newStackNo)
	{
		ContainerManager.manage.changeSlotInChest(xPos, yPos, slotNo, newSlotId, newStackNo, myInteract.insideHouseDetails);
	}

	protected static void InvokeUserCode_CmdChangeOneInChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeOneInChest called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdChangeOneInChest(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdStartDriving(uint vehicleToDrive)
	{
		drivingVehicle = true;
		drivingVehicleId = vehicleToDrive;
		NetworkIdentity component = NetworkIdentity.spawned[vehicleToDrive].GetComponent<NetworkIdentity>();
		if (component.connectionToClient != base.connectionToClient)
		{
			if (component.connectionToClient != null)
			{
				component.RemoveClientAuthority();
			}
			component.AssignClientAuthority(base.connectionToClient);
		}
		component.GetComponent<Vehicle>().startDriving(base.netId);
		drivingVehicleId = vehicleToDrive;
	}

	protected static void InvokeUserCode_CmdStartDriving(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStartDriving called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdStartDriving(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdStopDriving(uint vehicleToDrive)
	{
		drivingVehicle = false;
		drivingVehicleId = 0u;
		NetworkIdentity.spawned[vehicleToDrive].GetComponent<Vehicle>().stopDriving();
	}

	protected static void InvokeUserCode_CmdStopDriving(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStopDriving called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdStopDriving(reader.ReadUInt());
		}
	}

	protected void UserCode_RpcDropCarriedItem()
	{
		if (base.isLocalPlayer)
		{
			myEquip.setCarrying(false);
		}
		Networkcarrying = 0u;
	}

	protected static void InvokeUserCode_RpcDropCarriedItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDropCarriedItem called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcDropCarriedItem();
		}
	}

	protected void UserCode_CmdPickUpObject(uint pickUpObject)
	{
		Networkcarrying = pickUpObject;
		NetworkIdentity.spawned[pickUpObject].GetComponent<PickUpAndCarry>().NetworkbeingCarriedBy = base.netId;
	}

	protected static void InvokeUserCode_CmdPickUpObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpObject called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdPickUpObject(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdPutDownObject(float heightDroppedAt)
	{
		NetworkIdentity.spawned[carrying].GetComponent<PickUpAndCarry>().dropAndPlaceAtPos(heightDroppedAt);
		Networkcarrying = 0u;
	}

	protected static void InvokeUserCode_CmdPutDownObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPutDownObject called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdPutDownObject(reader.ReadFloat());
		}
	}

	protected void UserCode_CmdPutDownObjectInDropPoint(Vector3 dropPos)
	{
		NetworkIdentity.spawned[carrying].GetComponent<PickUpAndCarry>().dropAndPlaceAtDropPos(dropPos);
		Networkcarrying = 0u;
	}

	protected static void InvokeUserCode_CmdPutDownObjectInDropPoint(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPutDownObjectInDropPoint called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdPutDownObjectInDropPoint(reader.ReadVector3());
		}
	}

	protected void UserCode_CmdDropAndRelase(float heightDroppedAt)
	{
		PickUpAndCarry component = NetworkIdentity.spawned[carrying].GetComponent<PickUpAndCarry>();
		component.dropAndPlaceAtPos(heightDroppedAt);
		component.NetworkcanBePickedUp = false;
		if ((bool)NetworkIdentity.spawned[carrying].GetComponent<TrappedAnimal>())
		{
			NetworkIdentity.spawned[carrying].GetComponent<TrappedAnimal>().OpenOnServerWhenOnFloor();
		}
		if ((bool)NetworkIdentity.spawned[carrying].GetComponent<AnimalCarryBox>())
		{
			NetworkIdentity.spawned[carrying].GetComponent<AnimalCarryBox>().OpenOnServerWhenOnFloor();
		}
		Networkcarrying = 0u;
	}

	protected static void InvokeUserCode_CmdDropAndRelase(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDropAndRelase called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdDropAndRelase(reader.ReadFloat());
		}
	}

	protected void UserCode_CmdChangeStatus(int xPos, int yPos, int newStatus)
	{
		NetworkMapSharer.share.RpcGiveOnTileStatus(newStatus, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdChangeStatus(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeStatus called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdChangeStatus(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceOntoAnimal(int itemPlacingOn, uint animalNetId)
	{
		PlaceOnAnimal component = Inventory.inv.allItems[itemPlacingOn].GetComponent<PlaceOnAnimal>();
		AnimalAI component2 = NetworkIdentity.spawned[animalNetId].GetComponent<AnimalAI>();
		NetworkNavMesh.nav.UnSpawnAnAnimal(component2, false);
		if ((bool)component.becomePet)
		{
			FarmAnimalManager.manage.spawnNewFarmAnimalWithDetails(component.becomePet.animalId, component2.getVariationNo(), "Doggo", component2.transform.position);
		}
		if ((bool)component.becomeVehicle)
		{
			NetworkServer.Spawn(Object.Instantiate(component.becomeVehicle, component2.transform.position, component2.transform.rotation).gameObject);
		}
	}

	protected static void InvokeUserCode_CmdPlaceOntoAnimal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceOntoAnimal called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdPlaceOntoAnimal(reader.ReadInt(), reader.ReadUInt());
		}
	}

	protected void UserCode_CmdFillBulletinBoard()
	{
		NetworkMapSharer.share.getBulletinBoardAndSend(base.connectionToClient);
	}

	protected static void InvokeUserCode_CmdFillBulletinBoard(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFillBulletinBoard called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdFillBulletinBoard();
		}
	}

	protected void UserCode_CmdAddToBarrow(uint barrowId, int tileTypeToAdd)
	{
		NetworkIdentity.spawned[barrowId].GetComponent<Wheelbarrow>().insertDirt(tileTypeToAdd);
	}

	protected static void InvokeUserCode_CmdAddToBarrow(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddToBarrow called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdAddToBarrow(reader.ReadUInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRemoveFromBarrow(uint barrowId)
	{
		NetworkIdentity.spawned[barrowId].GetComponent<Wheelbarrow>().removeDirt();
	}

	protected static void InvokeUserCode_CmdRemoveFromBarrow(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRemoveFromBarrow called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdRemoveFromBarrow(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdPaintVehicle(uint vehicleId, int colourId)
	{
		NetworkIdentity.spawned[vehicleId].GetComponent<Vehicle>().setVariation(colourId);
		RpcPaintVehicle(vehicleId, colourId);
	}

	protected static void InvokeUserCode_CmdPaintVehicle(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPaintVehicle called on client.");
		}
		else
		{
			((CharPickUp)obj).UserCode_CmdPaintVehicle(reader.ReadUInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcPaintVehicle(uint vehicle, int colourId)
	{
		Vehicle component = NetworkIdentity.spawned[vehicle].GetComponent<Vehicle>();
		ParticleManager.manage.paintVehicle.GetComponent<ParticleSystemRenderer>().sharedMaterial = EquipWindow.equip.vehicleColours[colourId];
		SoundManager.manage.playASoundAtPoint(SoundManager.manage.paintSound, component.transform.position);
		if (component.meshToChangeColours.Length != 0)
		{
			for (int i = 0; i < component.meshToChangeColours.Length; i++)
			{
				if (component.meshToChangeColours[i].gameObject.activeInHierarchy)
				{
					ParticleSystem.ShapeModule shape = ParticleManager.manage.paintVehicle.shape;
					shape.enabled = true;
					shape.shapeType = ParticleSystemShapeType.Mesh;
					shape.mesh = component.meshToChangeColours[i].GetComponent<MeshFilter>().mesh;
					ParticleManager.manage.paintVehicle.transform.position = component.meshToChangeColours[i].transform.position;
					ParticleManager.manage.paintVehicle.transform.rotation = Quaternion.Euler(-90f, component.meshToChangeColours[i].transform.rotation.eulerAngles.y, 0f);
					ParticleManager.manage.paintVehicle.Emit(50);
				}
			}
		}
		else
		{
			if (component.meshRenderersToTintColours.Length == 0)
			{
				return;
			}
			for (int j = 0; j < component.meshRenderersToTintColours.Length; j++)
			{
				if (component.meshRenderersToTintColours[j].gameObject.activeInHierarchy)
				{
					ParticleSystem.ShapeModule shape2 = ParticleManager.manage.paintVehicle.shape;
					shape2.enabled = true;
					shape2.shapeType = ParticleSystemShapeType.Mesh;
					shape2.mesh = component.meshRenderersToTintColours[j].GetComponent<MeshFilter>().mesh;
					ParticleManager.manage.paintVehicle.transform.position = component.meshRenderersToTintColours[j].transform.position;
					ParticleManager.manage.paintVehicle.transform.rotation = Quaternion.Euler(-90f, component.meshRenderersToTintColours[j].transform.rotation.eulerAngles.y, 0f);
					ParticleManager.manage.paintVehicle.Emit(50);
				}
			}
		}
	}

	protected static void InvokeUserCode_RpcPaintVehicle(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPaintVehicle called on server.");
		}
		else
		{
			((CharPickUp)obj).UserCode_RpcPaintVehicle(reader.ReadUInt(), reader.ReadInt());
		}
	}

	static CharPickUp()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdChangeSignItem", InvokeUserCode_CmdChangeSignItem, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdAddToSilo", InvokeUserCode_CmdAddToSilo, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdHarvestAnimalToInv", InvokeUserCode_CmdHarvestAnimalToInv, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdHarvestAnimal", InvokeUserCode_CmdHarvestAnimal, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdPetAnimal", InvokeUserCode_CmdPetAnimal, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdReviveOtherChar", InvokeUserCode_CmdReviveOtherChar, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdSitInHairDresserSeat", InvokeUserCode_CmdSitInHairDresserSeat, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdGetUpFromHairDresserSeat", InvokeUserCode_CmdGetUpFromHairDresserSeat, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdSitDown", InvokeUserCode_CmdSitDown, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdConfirmSleep", InvokeUserCode_CmdConfirmSleep, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdGetUp", InvokeUserCode_CmdGetUp, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdPickUp", InvokeUserCode_CmdPickUp, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdOpenChest", InvokeUserCode_CmdOpenChest, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdOpenStash", InvokeUserCode_CmdOpenStash, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdChangeOneInChest", InvokeUserCode_CmdChangeOneInChest, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdStartDriving", InvokeUserCode_CmdStartDriving, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdStopDriving", InvokeUserCode_CmdStopDriving, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdPickUpObject", InvokeUserCode_CmdPickUpObject, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdPutDownObject", InvokeUserCode_CmdPutDownObject, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdPutDownObjectInDropPoint", InvokeUserCode_CmdPutDownObjectInDropPoint, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdDropAndRelase", InvokeUserCode_CmdDropAndRelase, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdChangeStatus", InvokeUserCode_CmdChangeStatus, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdPlaceOntoAnimal", InvokeUserCode_CmdPlaceOntoAnimal, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdFillBulletinBoard", InvokeUserCode_CmdFillBulletinBoard, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdAddToBarrow", InvokeUserCode_CmdAddToBarrow, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdRemoveFromBarrow", InvokeUserCode_CmdRemoveFromBarrow, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharPickUp), "CmdPaintVehicle", InvokeUserCode_CmdPaintVehicle, true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcStopDrivingFromServer", InvokeUserCode_RpcStopDrivingFromServer);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcReviveDelay", InvokeUserCode_RpcReviveDelay);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcCharPetAnimation", InvokeUserCode_RpcCharPetAnimation);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcSittingLayingOrStanding", InvokeUserCode_RpcSittingLayingOrStanding);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcPlayOpenStashSound", InvokeUserCode_RpcPlayOpenStashSound);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcDropCarriedItem", InvokeUserCode_RpcDropCarriedItem);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharPickUp), "RpcPaintVehicle", InvokeUserCode_RpcPaintVehicle);
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteUInt(carrying);
			writer.WriteVector3(sittingPos);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteUInt(carrying);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteVector3(sittingPos);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			uint num = carrying;
			Networkcarrying = reader.ReadUInt();
			if (!SyncVarEqual(num, ref carrying))
			{
				onCarryChanged(num, carrying);
			}
			Vector3 vector = sittingPos;
			NetworksittingPos = reader.ReadVector3();
			if (!SyncVarEqual(vector, ref sittingPos))
			{
				onSittingChanged(vector, sittingPos);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			uint num3 = carrying;
			Networkcarrying = reader.ReadUInt();
			if (!SyncVarEqual(num3, ref carrying))
			{
				onCarryChanged(num3, carrying);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			Vector3 vector2 = sittingPos;
			NetworksittingPos = reader.ReadVector3();
			if (!SyncVarEqual(vector2, ref sittingPos))
			{
				onSittingChanged(vector2, sittingPos);
			}
		}
	}
}
