using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
	public static BuildingManager manage;

	public bool windowOpen;

	public GameObject moveBuilingButtonPrefab;

	public Transform moveBuildingWindow;

	public Transform buttonScrollArea;

	private List<MoveableBuilding> moveableBuildings = new List<MoveableBuilding>();

	public int currentlyMoving = -1;

	[Header("Conversations---------")]
	public Conversation alreadyMovingABuilding;

	public Conversation houseIsBeingUpgraded;

	public Conversation noRoomInInv;

	public Conversation wantToMoveBuilding;

	public Conversation beingUpgradedConvo;

	public Conversation alreadyInDebt;

	public Conversation wantToMoveBuildingNonLocal;

	public Conversation wantToMovePlayerHouse;

	public Conversation wantToMovePlayerHouseNotEnoughMoney;

	public List<GameObject> buttons = new List<GameObject>();

	public InventoryItem houseMoveDeed;

	private bool movingHouse;

	private int talkingAboutMovingBuilding = -1;

	private void Awake()
	{
		manage = this;
	}

	public void openWindow()
	{
		movingHouse = false;
		if (!stopAtStartConvoChecks())
		{
			TownManager.manage.openTownManager(TownManager.windowType.Move);
			windowOpen = true;
			moveBuildingWindow.gameObject.SetActive(true);
			fillListWithButtons();
			DeedManager.manage.closeDeedWindow();
		}
	}

	public void fillListWithButtons()
	{
		for (int i = 0; i < moveableBuildings.Count; i++)
		{
			InventoryItem inventoryItem = findDeedForBuilding(moveableBuildings[i].getBuildingId());
			if (inventoryItem != null && !inventoryItem.placeable.GetComponent<DisplayPlayerHouseTiles>())
			{
				DeedButton component = Object.Instantiate(moveBuilingButtonPrefab, buttonScrollArea).GetComponent<DeedButton>();
				component.moveABuildingButton = true;
				component.setUpBuildingButton(Inventory.inv.getInvItemId(inventoryItem), i);
				buttons.Add(component.gameObject);
			}
		}
	}

	public int findPlayersHouseNumberInMovableBuildings()
	{
		for (int i = 0; i < moveableBuildings.Count; i++)
		{
			if ((bool)WorldManager.manageWorld.allObjects[moveableBuildings[i].getBuildingId()].displayPlayerHouseTiles && WorldManager.manageWorld.allObjects[moveableBuildings[i].getBuildingId()].displayPlayerHouseTiles.isPlayerHouse)
			{
				return i;
			}
		}
		return -1;
	}

	public void closeWindow()
	{
		moveBuildingWindow.gameObject.SetActive(false);
		windowOpen = false;
		for (int i = 0; i < buttons.Count; i++)
		{
			Object.Destroy(buttons[i]);
		}
		buttons.Clear();
	}

	public void addBuildingToMoveList(int xPos, int yPos)
	{
		if (findBuildingToMoveById(WorldManager.manageWorld.onTileMap[xPos, yPos]) == null)
		{
			moveableBuildings.Add(new MoveableBuilding(xPos, yPos));
		}
	}

	public void askToMoveBuilding(int buildingNo)
	{
		if (moveableBuildings[buildingNo].isBeingUpgraded())
		{
			TownManager.manage.closeTownManager();
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, beingUpgradedConvo);
			return;
		}
		wantToMoveBuilding.responesAlt[0].talkingAboutItem = findDeedForBuilding(moveableBuildings[buildingNo].getBuildingId());
		talkingAboutMovingBuilding = moveableBuildings[buildingNo].getBuildingId();
		TownManager.manage.closeTownManager();
		ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, wantToMoveBuilding);
	}

	public string getTalkingAboutBuildingName()
	{
		return getBuildingName(talkingAboutMovingBuilding);
	}

	public string getBuildingName(int buildingId)
	{
		if (buildingId < -1)
		{
			return "no building";
		}
		if ((bool)WorldManager.manageWorld.allObjectSettings[buildingId].tileObjectLoadInside)
		{
			return WorldManager.manageWorld.allObjectSettings[buildingId].tileObjectLoadInside.buildingName;
		}
		if ((bool)WorldManager.manageWorld.allObjects[buildingId].displayPlayerHouseTiles)
		{
			if (WorldManager.manageWorld.allObjects[buildingId].displayPlayerHouseTiles.isPlayerHouse)
			{
				return "Player House";
			}
			return "House";
		}
		return WorldManager.manageWorld.allObjects[buildingId].name;
	}

	public void confirmWantToMoveBuilding()
	{
		if (!movingHouse)
		{
			NetworkMapSharer share = NetworkMapSharer.share;
			share.NetworktownDebt = share.townDebt + 25000;
			currentlyMoving = talkingAboutMovingBuilding;
			NetworkMapSharer.share.NetworkmovingBuilding = talkingAboutMovingBuilding;
			giveDeedForBuildingToBeMoved(talkingAboutMovingBuilding);
		}
		else
		{
			Inventory.inv.changeWallet(-25000);
			giveDeedForHouseToMove();
			currentlyMoving = talkingAboutMovingBuilding;
			NetworkMapSharer.share.NetworkmovingBuilding = talkingAboutMovingBuilding;
		}
		movingHouse = false;
	}

	private InventoryItem findDeedForBuilding(int movingBuildingId)
	{
		for (int i = 0; i < DeedManager.manage.allDeeds.Count; i++)
		{
			if ((bool)DeedManager.manage.allDeeds[i].placeable && (bool)DeedManager.manage.allDeeds[i].placeable.tileObjectGrowthStages && DeedManager.manage.allDeeds[i].placeable.tileObjectGrowthStages.changeToWhenGrown.tileObjectId == movingBuildingId)
			{
				return DeedManager.manage.allDeeds[i];
			}
		}
		return null;
	}

	public void giveDeedForBuildingToBeMoved(int movingBuildingId)
	{
		GiftedItemWindow.gifted.addToListToBeGiven(Inventory.inv.getInvItemId(findDeedForBuilding(movingBuildingId)), 1);
		GiftedItemWindow.gifted.openWindowAndGiveItems();
	}

	public void giveDeedForHouseToMove()
	{
		int itemId = houseMoveDeed.getItemId();
		talkingAboutMovingBuilding = moveableBuildings[findPlayersHouseNumberInMovableBuildings()].getBuildingId();
		Inventory.inv.allItems[itemId].placeable.tileObjectGrowthStages.changeToWhenGrown = WorldManager.manageWorld.allObjects[talkingAboutMovingBuilding];
		GiftedItemWindow.gifted.addToListToBeGiven(itemId, 1);
		GiftedItemWindow.gifted.openWindowAndGiveItems();
	}

	public void moveBuildingToNewSite(int movingBuildingId, int newXPos, int newYPos)
	{
		for (int i = 0; i < TownManager.manage.allShopFloors.Length; i++)
		{
			if ((bool)TownManager.manage.allShopFloors[i] && TownManager.manage.allShopFloors[i].connectedToBuilingId == movingBuildingId)
			{
				Object.Destroy(TownManager.manage.allShopFloors[i]);
				TownManager.manage.allShopFloors[i] = null;
			}
		}
		MoveableBuilding moveableBuilding = findBuildingToMoveById(movingBuildingId);
		if (moveableBuilding != null)
		{
			moveableBuilding.moveBuildingToNewPos(newXPos, newYPos);
		}
	}

	private MoveableBuilding findBuildingToMoveById(int id)
	{
		for (int i = 0; i < moveableBuildings.Count; i++)
		{
			if (moveableBuildings[i].getBuildingId() == id)
			{
				return moveableBuildings[i];
			}
		}
		return null;
	}

	public void getWantToMovePlayerHouseConvo()
	{
		if (TownManager.manage.checkIfHouseIsBeingUpgraded())
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, houseIsBeingUpgraded);
			return;
		}
		if (currentlyMoving != -1)
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, alreadyMovingABuilding);
			return;
		}
		if (!Inventory.inv.checkIfItemCanFit(0, 1))
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, noRoomInInv);
			return;
		}
		if (Inventory.inv.wallet < 25000)
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, wantToMovePlayerHouseNotEnoughMoney);
			return;
		}
		movingHouse = true;
		ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, wantToMovePlayerHouse);
	}

	public bool stopAtStartConvoChecks()
	{
		if (NetworkMapSharer.share.isServer)
		{
			if (currentlyMoving != -1)
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, alreadyMovingABuilding);
				return true;
			}
			if (!Inventory.inv.checkIfItemCanFit(0, 1))
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, noRoomInInv);
				return true;
			}
			if (NetworkMapSharer.share.townDebt > 0)
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, alreadyInDebt);
				return true;
			}
			return false;
		}
		ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, wantToMoveBuildingNonLocal);
		return true;
	}

	public void loadCurrentlyMoving(int newCurrentlyMoving)
	{
		currentlyMoving = newCurrentlyMoving;
		NetworkMapSharer.share.NetworkmovingBuilding = newCurrentlyMoving;
		if ((bool)WorldManager.manageWorld.allObjects[currentlyMoving].displayPlayerHouseTiles && WorldManager.manageWorld.allObjects[currentlyMoving].displayPlayerHouseTiles.isPlayerHouse)
		{
			houseMoveDeed.placeable.tileObjectGrowthStages.changeToWhenGrown = WorldManager.manageWorld.allObjects[currentlyMoving];
		}
	}
}
