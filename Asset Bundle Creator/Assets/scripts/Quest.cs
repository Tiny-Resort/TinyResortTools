using UnityEngine;

public class Quest : MonoBehaviour
{
	public string QuestName;

	[TextArea]
	public string QuestDescription;

	[Header("Offered By --------------")]
	public int offeredByNpc;

	public bool townHallQuest;

	public bool attractResidentsQuest;

	public int relationshipLevelNeeded;

	public float townBeautyLevelRequired;

	public float townEcconnomyLevelRequired;

	[Header("Conversations --------------")]
	public Conversation questConversation;

	public Conversation questAcceptedConversation;

	public Conversation completedConversation;

	[Header("Items Given On Quest Accepted ------")]
	public InventoryItem[] giveItemsOnAccept;

	public int[] amountGivenOnAccept;

	public InventoryItem deedUnlockedOnAccept;

	[Header("Items Given As Reward ------")]
	public InventoryItem[] rewardOnComplete;

	public int[] rewardStacksGiven;

	public InventoryItem[] unlockRecipeOnComplete;

	[Header("Buildings Required by Quest ------")]
	public TileObject[] requiredBuilding;

	public InventoryItem placeableToPlace;

	[Header("Items Required by Quest ------")]
	public InventoryItem[] requiredItems;

	public int[] requiredStacks;

	[Header("NPC Required by Quest ------")]
	public NPCDetails npcToConvinceToMoveAIn;

	[Header("Other ------")]
	public DailyTaskGenerator.genericTaskType doTaskOnComplete;

	public bool questForFood;

	public TileObject changerItem;

	public int acceptQuestOnComplete = -1;

	public bool questToUseChanger;

	public bool autoCompletesOnDate;

	public bool placeOrHaveItem;

	public bool teleporterQuest;

	public int[] dateCompleted = new int[4];

	public InventoryItem deedToApplyFor;

	public bool checkIfComplete()
	{
		if (autoCompletesOnDate)
		{
			return isPastDate();
		}
		if (placeOrHaveItem)
		{
			return checkIfHasInInvOrHasBeenPlaced();
		}
		if (npcToConvinceToMoveAIn != null)
		{
			for (int i = 0; i < NPCManager.manage.NPCDetails.Length; i++)
			{
				if (NPCManager.manage.NPCDetails[i] == npcToConvinceToMoveAIn)
				{
					return NPCManager.manage.npcStatus[i].checkIfHasMovedIn();
				}
			}
		}
		if (requiredItems.Length != 0)
		{
			return checkIfHasAllRequiredItems();
		}
		if (requiredBuilding.Length != 0)
		{
			return checkIfHasBeenPlaced();
		}
		if (questForFood)
		{
			return checkIfFoodInInv();
		}
		return false;
	}

	public bool checkIfFoodInInv()
	{
		InventorySlot[] invSlots = Inventory.inv.invSlots;
		foreach (InventorySlot inventorySlot in invSlots)
		{
			if (inventorySlot.itemNo == -1)
			{
				continue;
			}
			if ((bool)Inventory.inv.allItems[inventorySlot.itemNo].consumeable)
			{
				return true;
			}
			if ((bool)Inventory.inv.allItems[inventorySlot.itemNo].itemChange)
			{
				int changerResultId = Inventory.inv.allItems[inventorySlot.itemNo].itemChange.getChangerResultId(changerItem.tileObjectId);
				if (changerResultId != -1 && (bool)Inventory.inv.allItems[changerResultId].consumeable)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool checkIfHasBeenPlaced()
	{
		if (requiredBuilding.Length != 0)
		{
			for (int i = 0; i < WorldManager.manageWorld.getMapSize() / 10; i++)
			{
				for (int j = 0; j < WorldManager.manageWorld.getMapSize() / 10; j++)
				{
					if (!WorldManager.manageWorld.chunkChangedMap[j, i])
					{
						continue;
					}
					for (int k = i * 10; k < i * 10 + 10; k++)
					{
						for (int l = j * 10; l < j * 10 + 10; l++)
						{
							if (WorldManager.manageWorld.onTileMap[l, k] == -1)
							{
								continue;
							}
							for (int m = 0; m < requiredBuilding.Length; m++)
							{
								if (requiredBuilding[m].tileObjectId == WorldManager.manageWorld.onTileMap[l, k])
								{
									return true;
								}
							}
						}
					}
				}
			}
		}
		return false;
	}

	public bool upgradeBaseTent()
	{
		MonoBehaviour.print("Looking for base tent");
		if (requiredBuilding.Length != 0)
		{
			MonoBehaviour.print("Looking for base tent");
			for (int i = 0; i < WorldManager.manageWorld.getMapSize() / 10; i++)
			{
				for (int j = 0; j < WorldManager.manageWorld.getMapSize() / 10; j++)
				{
					if (!WorldManager.manageWorld.chunkChangedMap[j, i])
					{
						continue;
					}
					for (int k = i * 10; k < i * 10 + 10; k++)
					{
						for (int l = j * 10; l < j * 10 + 10; l++)
						{
							if (WorldManager.manageWorld.onTileMap[l, k] == -1)
							{
								continue;
							}
							for (int m = 0; m < requiredBuilding.Length; m++)
							{
								if (requiredBuilding[m].tileObjectId == WorldManager.manageWorld.onTileMap[l, k])
								{
									MonoBehaviour.print("Found the tent and set status");
									NetworkMapSharer.share.RpcGiveOnTileStatus(1, l, k);
									return true;
								}
							}
						}
					}
				}
			}
		}
		return false;
	}

	public bool checkIfHasAllRequiredItems()
	{
		for (int i = 0; i < requiredItems.Length; i++)
		{
			if (Inventory.inv.getAmountOfItemInAllSlots(Inventory.inv.getInvItemId(requiredItems[i])) < requiredStacks[i])
			{
				return false;
			}
		}
		return true;
	}

	public bool isPastDate()
	{
		MonoBehaviour.print(WorldManager.manageWorld.year >= dateCompleted[3]);
		MonoBehaviour.print(WorldManager.manageWorld.month >= dateCompleted[2]);
		MonoBehaviour.print(WorldManager.manageWorld.week >= dateCompleted[1]);
		MonoBehaviour.print(WorldManager.manageWorld.day >= dateCompleted[0]);
		if (WorldManager.manageWorld.day >= dateCompleted[0])
		{
			return true;
		}
		return false;
	}

	public bool checkIfHasInInvOrHasBeenPlaced()
	{
		if (requiredItems.Length != 0 && checkIfHasAllRequiredItems())
		{
			return true;
		}
		if (requiredBuilding.Length != 0)
		{
			return checkIfHasBeenPlaced();
		}
		return false;
	}

	public string getMissionObjText()
	{
		if (attractResidentsQuest)
		{
			if (NPCManager.manage.getNoOfNPCsMovedIn() < 5)
			{
				return "<sprite=12> Attract a total of 5 permanent residents to move to " + Inventory.inv.islandName + " [ " + NPCManager.manage.getNoOfNPCsMovedIn() + "/5]";
			}
			if (BuildingManager.manage.currentlyMoving == requiredBuilding[0].tileObjectId)
			{
				return "<sprite=12> Talk to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName + " once the Base Tent has been moved";
			}
			return "<sprite=12> Talk to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName;
		}
		if (questToUseChanger)
		{
			if (checkIfHasAllRequiredItems())
			{
				return "<sprite=13> Craft a " + placeableToPlace.getInvItemName() + "at the crafting table in the Base Tent.\n<sprite=13> Place the  " + placeableToPlace.getInvItemName() + " down outside.\n<sprite=13> Place Tin Ore into " + placeableToPlace.getInvItemName() + " and wait for it to become " + requiredItems[0].getInvItemName() + ".\n<sprite=12> Take the " + requiredItems[0].getInvItemName() + " to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName;
			}
			if (checkIfHasBeenPlaced())
			{
				return "<sprite=13> Craft a " + placeableToPlace.getInvItemName() + "at the crafting table in the Base Tent.\n<sprite=13> Place the  " + placeableToPlace.getInvItemName() + " down outside.\n<sprite=12> Place Tin Ore into " + placeableToPlace.getInvItemName() + " and wait for it to become " + requiredItems[0].getInvItemName() + ".\n<sprite=12> Take the " + requiredItems[0].getInvItemName() + " to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName;
			}
			if (Inventory.inv.getAmountOfItemInAllSlots(Inventory.inv.getInvItemId(placeableToPlace)) >= 1)
			{
				return "<sprite=13> Craft a " + placeableToPlace.getInvItemName() + "at the crafting table in the Base Tent.\n<sprite=12> Place the  " + placeableToPlace.getInvItemName() + " down outside.\n<sprite=12> Place Tin Ore into " + placeableToPlace.getInvItemName() + " and wait for it to become " + requiredItems[0].getInvItemName() + ".\n<sprite=12> Take the " + requiredItems[0].getInvItemName() + " to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName;
			}
			return "<sprite=12> Craft a " + placeableToPlace.getInvItemName() + "at the crafting table in the Base Tent.\n<sprite=12> Place the  " + placeableToPlace.getInvItemName() + " down outside.\n<sprite=12> Place Tin Ore into " + placeableToPlace.getInvItemName() + " and wait for it to become " + requiredItems[0].getInvItemName() + ".\n<sprite=12> Take the " + requiredItems[0].getInvItemName() + " to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName;
		}
		if (placeOrHaveItem)
		{
			if (checkIfHasInInvOrHasBeenPlaced())
			{
				return "<sprite=13> Buy the " + requiredItems[0].getInvItemName() + "\n<sprite=12> Talk to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName;
			}
			return "<sprite=12> Buy the " + requiredItems[0].getInvItemName() + "\n<sprite=12> Talk to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName;
		}
		if (autoCompletesOnDate)
		{
			if (!isPastDate())
			{
				return "[Optional] Complete Daily tasks\n<sprite=12> Place sleeping bag and get some rest.";
			}
			return "<sprite=12> Talk to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName;
		}
		if (questForFood)
		{
			if (checkIfFoodInInv())
			{
				return "<sprite=13> Find something to eat.\n<sprite=12> Talk to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName;
			}
			return "<sprite=12> Find something to eat.\n<sprite=12> Talk to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName;
		}
		if (requiredItems.Length != 0)
		{
			string text = "\t";
			for (int i = 0; i < requiredItems.Length; i++)
			{
				text = text + "\n[" + Inventory.inv.getAmountOfItemInAllSlots(Inventory.inv.getInvItemId(requiredItems[i])) + "/" + requiredStacks[i] + "] " + requiredItems[i].getInvItemName();
			}
			if (checkIfHasAllRequiredItems())
			{
				return "<sprite=13> Collect the requested items.\n<sprite=12> Bring items to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName;
			}
			return "<sprite=12> Collect the requested items." + text + "\n<sprite=12> Bring items to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName;
		}
		if ((bool)deedToApplyFor)
		{
			if (npcToConvinceToMoveAIn != null)
			{
				for (int j = 0; j < NPCManager.manage.NPCDetails.Length; j++)
				{
					if (NPCManager.manage.NPCDetails[j] == npcToConvinceToMoveAIn && !NPCManager.manage.npcStatus[j].hasAskedToMoveIn)
					{
						string text2 = "";
						text2 = ((NPCManager.manage.npcStatus[j].relationshipLevel >= NPCManager.manage.NPCDetails[j].relationshipBeforeMove) ? (text2 + "<sprite=13> Do some favours for John") : (text2 + "<sprite=12> Do some favours for John"));
						text2 = ((NPCManager.manage.npcStatus[j].moneySpentAtStore >= NPCManager.manage.NPCDetails[j].spendBeforeMoveIn) ? (text2 + "\n<sprite=13> Spend money or sell items in John's store") : (text2 + "\n<sprite=12> Spend money or sell items in John's store"));
						return text2 + "\n<sprite=12> Convince John to move in.";
					}
				}
			}
			if (!DeedManager.manage.checkIfDeedHasBeenBought(deedToApplyFor))
			{
				return "<sprite=12> Ask " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName + " about the town to apply for the " + deedToApplyFor.getInvItemName();
			}
			if (!checkIfHasBeenPlaced())
			{
				return "<sprite=12> Place the " + deedToApplyFor.getInvItemName();
			}
			if (DeedManager.manage.checkIfDeedMaterialsComplete(deedToApplyFor))
			{
				return "<sprite=12> Wait for construction of the  " + deedToApplyFor.getInvItemName() + " to be completed";
			}
			return "<sprite=12> Place the required items into the construction box at the deed site";
		}
		if (requiredBuilding != null && placeableToPlace != null)
		{
			if (!checkIfHasBeenPlaced())
			{
				return "<sprite=12> Place " + placeableToPlace.getInvItemName() + "\n<sprite=12> Talk to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName;
			}
			return "<sprite=13> Place " + placeableToPlace.getInvItemName() + "\n<sprite=12> Talk to " + NPCManager.manage.NPCDetails[offeredByNpc].NPCName;
		}
		return "";
	}
}
