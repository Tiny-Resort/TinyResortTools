using TMPro;
using UnityEngine;

public class ChestWindow : MonoBehaviour
{
	public static ChestWindow chests;

	public GameObject inventorySlotPrefab;

	public Transform chestWindow;

	public Transform slotWindow;

	public InventorySlot[] chestSlots = new InventorySlot[24];

	private Chest currentlyOpenedChest;

	public bool chestWindowOpen;

	private int[] arrayForCheckingItems = new int[24];

	private int[] arrayForCheckingStacks = new int[24];

	public bool localChangeMade = true;

	public bool isStash;

	public TextMeshProUGUI titleText;

	private void Awake()
	{
		chests = this;
	}

	private void Start()
	{
		for (int i = 0; i < 24; i++)
		{
			chestSlots[i] = Object.Instantiate(inventorySlotPrefab, slotWindow).GetComponent<InventorySlot>();
			chestSlots[i].chestSlotNo = i;
		}
	}

	public void makeALocalChange(int chestNo)
	{
		if (!isStash && localChangeMade)
		{
			NetworkMapSharer.share.localChar.myPickUp.CmdChangeOneInChest(currentlyOpenedChest.xPos, currentlyOpenedChest.yPos, chestNo, chestSlots[chestNo].itemNo, chestSlots[chestNo].stack);
		}
	}

	public void refreshOpenWindow(int xPos, int yPos, HouseDetails inside)
	{
		if (!chestWindowOpen || !ContainerManager.manage.checkIfChestIsInsideAndInThisHouse(inside, currentlyOpenedChest) || currentlyOpenedChest.xPos != xPos || currentlyOpenedChest.yPos != yPos)
		{
			return;
		}
		localChangeMade = false;
		for (int i = 0; i < 24; i++)
		{
			if (chestSlots[i].itemNo != currentlyOpenedChest.itemIds[i] || chestSlots[i].stack != currentlyOpenedChest.itemStacks[i])
			{
				chestSlots[i].chestSlotNo = -1;
				chestSlots[i].updateSlotContentsAndRefresh(currentlyOpenedChest.itemIds[i], currentlyOpenedChest.itemStacks[i]);
				chestSlots[i].chestSlotNo = i;
			}
		}
		localChangeMade = true;
	}

	public void openStashInWindow(int stashId)
	{
		isStash = true;
		currentlyOpenedChest = ContainerManager.manage.privateStashes[stashId];
		titleText.text = "My Travel Bag";
		if (currentlyOpenedChest != null)
		{
			for (int i = 0; i < 24; i++)
			{
				chestSlots[i].chestSlotNo = -1;
				chestSlots[i].updateSlotContentsAndRefresh(currentlyOpenedChest.itemIds[i], currentlyOpenedChest.itemStacks[i]);
				chestSlots[i].chestSlotNo = i;
				arrayForCheckingItems[i] = currentlyOpenedChest.itemIds[i];
				arrayForCheckingStacks[i] = currentlyOpenedChest.itemStacks[i];
			}
			chestWindow.gameObject.SetActive(true);
			chestWindowOpen = true;
			lockBugsAndFishFromChest();
			Inventory.inv.invOpen = true;
			Inventory.inv.openAndCloseInv();
		}
		Inventory.inv.checkIfWindowIsNeeded();
		CurrencyWindows.currency.openJournal();
	}

	public void openChestInWindow(int xPos, int yPos)
	{
		isStash = false;
		currentlyOpenedChest = ContainerManager.manage.getChestForWindow(xPos, yPos, NetworkMapSharer.share.localChar.myInteract.insideHouseDetails);
		if (NetworkMapSharer.share.localChar.myInteract.insideHouseDetails != null)
		{
			if (NetworkMapSharer.share.localChar.myInteract.insideHouseDetails.houseMapOnTile[xPos, yPos] > -1)
			{
				titleText.text = WorldManager.manageWorld.allObjectSettings[NetworkMapSharer.share.localChar.myInteract.insideHouseDetails.houseMapOnTile[xPos, yPos]].dropsItemOnDeath.getInvItemName();
			}
			else
			{
				titleText.text = "Container";
			}
		}
		else if (WorldManager.manageWorld.onTileMap[xPos, yPos] > -1)
		{
			titleText.text = WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].dropsItemOnDeath.getInvItemName();
		}
		else
		{
			titleText.text = "Container";
		}
		if (currentlyOpenedChest != null)
		{
			for (int i = 0; i < 24; i++)
			{
				chestSlots[i].chestSlotNo = -1;
				chestSlots[i].updateSlotContentsAndRefresh(currentlyOpenedChest.itemIds[i], currentlyOpenedChest.itemStacks[i]);
				chestSlots[i].chestSlotNo = i;
				arrayForCheckingItems[i] = currentlyOpenedChest.itemIds[i];
				arrayForCheckingStacks[i] = currentlyOpenedChest.itemStacks[i];
			}
			chestWindow.gameObject.SetActive(true);
			chestWindowOpen = true;
			lockBugsAndFishFromChest();
			Inventory.inv.invOpen = true;
			Inventory.inv.openAndCloseInv();
		}
		Inventory.inv.checkIfWindowIsNeeded();
		CurrencyWindows.currency.openJournal();
	}

	public void closeChestInWindow()
	{
		isStash = false;
		if (currentlyOpenedChest != null)
		{
			for (int i = 0; i < 24; i++)
			{
				currentlyOpenedChest.itemIds[i] = chestSlots[i].itemNo;
				currentlyOpenedChest.itemStacks[i] = chestSlots[i].stack;
			}
			NetworkMapSharer.share.localChar.CmdCloseChest(currentlyOpenedChest.xPos, currentlyOpenedChest.yPos);
			currentlyOpenedChest = null;
		}
		Inventory.inv.invOpen = false;
		Inventory.inv.openAndCloseInv();
		chestWindow.gameObject.SetActive(false);
		chestWindowOpen = false;
		unlockAllSlots();
		Inventory.inv.checkIfWindowIsNeeded();
		CurrencyWindows.currency.closeJournal();
	}

	public void lockBugsAndFishFromChest()
	{
		for (int i = 0; i < Inventory.inv.invSlots.Length; i++)
		{
			if (Inventory.inv.invSlots[i].itemNo >= 0 && ((bool)Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].bug || (bool)Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].fish || Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].isDeed))
			{
				Inventory.inv.invSlots[i].disableForGive();
			}
		}
	}

	public void unlockAllSlots()
	{
		for (int i = 0; i < Inventory.inv.invSlots.Length; i++)
		{
			Inventory.inv.invSlots[i].clearDisable();
		}
	}
}
