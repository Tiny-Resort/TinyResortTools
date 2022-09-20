using System.Collections.Generic;
using UnityEngine;

public class NPCRequest
{
	public enum requestType
	{
		None = 0,
		BugRequest = 1,
		FishRequest = 2,
		FoodRequest = 3,
		WoodRequest = 4,
		PlankRequest = 5,
		OreBarRequest = 6,
		CookingTableRequest = 7,
		FurnitureRequest = 8,
		ClothingRequest = 9,
		InventoryItemRequest = 10,
		SellItemRequest = 11
	}

	public int specificDesiredItem = -1;

	public int desiredAmount;

	public requestType myRequestType;

	public bool generatedToday;

	public string itemFoundInLocation = "";

	public int myNPCId;

	private int priceToPayFor;

	private int sellPrice;

	public void getNewRequest(int newNPCId, bool loadingForSave = false)
	{
		generatedToday = true;
		myNPCId = newNPCId;
		clearRequest(newNPCId, loadingForSave);
		desiredAmount = 1;
		getRandomQuest();
	}

	public void clearRequest(int npcId, bool loadingFromSave = false)
	{
		specificDesiredItem = -1;
		desiredAmount = 1;
		if (!loadingFromSave)
		{
			NPCManager.manage.npcStatus[npcId].completedRequest = false;
			NPCManager.manage.npcStatus[npcId].acceptedRequest = false;
		}
		myRequestType = requestType.None;
	}

	public void completeRequest(int npcId)
	{
		NPCManager.manage.npcStatus[npcId].completedRequest = true;
		clearRequest(npcId);
	}

	public void failRequest(int npcId)
	{
		NPCManager.manage.npcStatus[npcId].completedRequest = true;
		clearRequest(npcId);
	}

	public void checkForOtherActionsOnComplete()
	{
		if (myRequestType == requestType.InventoryItemRequest)
		{
			Inventory.inv.removeAmountOfItem(specificDesiredItem, desiredAmount);
			GiftedItemWindow.gifted.addToListToBeGiven(Inventory.inv.getInvItemId(Inventory.inv.moneyItem), getDesiredPriceToPay());
			GiftedItemWindow.gifted.openWindowAndGiveItems();
		}
		else if (myRequestType == requestType.SellItemRequest)
		{
			Inventory.inv.changeWallet(-getSellPrice());
			sellPrice = 0;
			GiftedItemWindow.gifted.addToListToBeGiven(specificDesiredItem, desiredAmount);
			GiftedItemWindow.gifted.openWindowAndGiveItems();
		}
	}

	public void acceptRequest(int npcId)
	{
		NotificationManager.manage.makeTopNotification("Request added to Journal", "This request must be completed by the end of the day.", SoundManager.manage.taskAcceptedSound);
		NPCManager.manage.npcStatus[npcId].acceptedRequest = true;
	}

	public string getDesiredItemName()
	{
		if (specificDesiredItem != -1)
		{
			return getDesiredItemNameByNumber(specificDesiredItem, desiredAmount);
		}
		if (myRequestType == requestType.BugRequest)
		{
			return "any bug";
		}
		if (myRequestType == requestType.FishRequest)
		{
			return "any fish";
		}
		if (myRequestType == requestType.FoodRequest)
		{
			return "something to eat";
		}
		if (myRequestType == requestType.CookingTableRequest)
		{
			return "something you've made me at a cooking table";
		}
		if (myRequestType == requestType.FurnitureRequest)
		{
			return "furniture";
		}
		if (myRequestType == requestType.ClothingRequest)
		{
			return "clothing";
		}
		return "";
	}

	public string getDesiredItemNameByNumber(int invId, int amount)
	{
		if (amount == 1)
		{
			return Inventory.inv.allItems[invId].getInvItemName();
		}
		return amount + " " + Inventory.inv.allItems[invId].getInvItemName();
	}

	public bool checkIfItemMatchesRequest(int itemGivenId)
	{
		if (specificDesiredItem != -1)
		{
			if (itemGivenId == specificDesiredItem)
			{
				return true;
			}
			return false;
		}
		if (myRequestType == requestType.BugRequest && (bool)Inventory.inv.allItems[itemGivenId].bug)
		{
			return true;
		}
		if (myRequestType == requestType.FishRequest && (bool)Inventory.inv.allItems[itemGivenId].fish)
		{
			return true;
		}
		if (myRequestType == requestType.FoodRequest && (bool)Inventory.inv.allItems[itemGivenId].consumeable)
		{
			return true;
		}
		if (myRequestType == requestType.CookingTableRequest && (bool)Inventory.inv.allItems[itemGivenId].consumeable && (bool)Inventory.inv.allItems[itemGivenId].craftable)
		{
			return true;
		}
		if (myRequestType == requestType.FurnitureRequest && Inventory.inv.allItems[itemGivenId].isFurniture)
		{
			return true;
		}
		if (myRequestType == requestType.ClothingRequest && (bool)Inventory.inv.allItems[itemGivenId].equipable && Inventory.inv.allItems[itemGivenId].equipable.cloths)
		{
			return true;
		}
		return false;
	}

	public bool wantsCertainFishOrBug()
	{
		if ((myRequestType == requestType.BugRequest || myRequestType == requestType.FishRequest) && specificDesiredItem != -1)
		{
			return true;
		}
		return false;
	}

	public bool wantsSomethingToEat()
	{
		return myRequestType == requestType.FoodRequest;
	}

	public bool wantToSellSomething()
	{
		return myRequestType == requestType.SellItemRequest;
	}

	public bool wantsSomeLogging()
	{
		if (myRequestType == requestType.WoodRequest || myRequestType == requestType.PlankRequest)
		{
			return true;
		}
		return false;
	}

	public bool wantsSomeMining()
	{
		return myRequestType == requestType.OreBarRequest;
	}

	public bool wantsSomeFurniture()
	{
		return myRequestType == requestType.FurnitureRequest;
	}

	public bool wantsSomeClothing()
	{
		return myRequestType == requestType.ClothingRequest;
	}

	public bool wantsAnItemInYourInv()
	{
		if (myRequestType == requestType.InventoryItemRequest)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < Inventory.inv.invSlots.Length; i++)
			{
				if (Inventory.inv.invSlots[i].itemNo != -1 && ((bool)Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].fish || (bool)Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].bug || Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].isFurniture || ((bool)Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].equipable && Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].equipable.cloths) || ((bool)Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].equipable && Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].equipable.wallpaper) || ((bool)Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].equipable && Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].equipable.flooring) || (bool)Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].consumeable || (bool)Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].itemChange) && !Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].isATool && !Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].isATool && !Inventory.inv.allItems[Inventory.inv.invSlots[i].itemNo].hasColourVariation)
				{
					list.Add(i);
				}
			}
			if (list.Count > 0)
			{
				int num = list[Random.Range(0, list.Count)];
				int num2 = 0;
				while (!checkIfNPCAccepts(num) && num2 < 25)
				{
					num = list[Random.Range(0, list.Count)];
					num2++;
				}
				if (num2 < 25)
				{
					specificDesiredItem = Inventory.inv.invSlots[num].itemNo;
					desiredAmount = Mathf.Clamp(Inventory.inv.invSlots[num].stack, 1, 5);
					priceToPayFor = (int)((float)Inventory.inv.allItems[specificDesiredItem].value * Random.Range(1.1f, 3.5f)) * desiredAmount;
					return true;
				}
			}
			while (myRequestType == requestType.InventoryItemRequest)
			{
				completeRequest(myNPCId);
				getNewRequest(myNPCId);
			}
		}
		return false;
	}

	public int getDesiredPriceToPay()
	{
		return priceToPayFor;
	}

	public void setRandomFishNoAndLocation()
	{
		switch (Random.Range(0, 5))
		{
		case 0:
			specificDesiredItem = getFishFromBiomWithLowChanceOfCommon(AnimalManager.manage.northernOceanFish);
			itemFoundInLocation = AnimalManager.manage.northernOceanFish.locationName;
			break;
		case 1:
			specificDesiredItem = getFishFromBiomWithLowChanceOfCommon(AnimalManager.manage.southernOceanFish);
			itemFoundInLocation = AnimalManager.manage.southernOceanFish.locationName;
			break;
		case 2:
			specificDesiredItem = getFishFromBiomWithLowChanceOfCommon(AnimalManager.manage.riverFish);
			itemFoundInLocation = AnimalManager.manage.riverFish.locationName;
			break;
		case 3:
			specificDesiredItem = getFishFromBiomWithLowChanceOfCommon(AnimalManager.manage.billabongFish);
			itemFoundInLocation = AnimalManager.manage.billabongFish.locationName;
			break;
		default:
			specificDesiredItem = getFishFromBiomWithLowChanceOfCommon(AnimalManager.manage.mangroveFish);
			itemFoundInLocation = AnimalManager.manage.mangroveFish.locationName;
			break;
		}
	}

	public int getFishFromBiomWithLowChanceOfCommon(InventoryLootTableTimeWeatherMaster biome)
	{
		int i = 0;
		InventoryItem inventoryItem = biome.getInventoryItem();
		for (; i < 15; i++)
		{
			if (inventoryItem.fish.mySeason.myRarity != 0)
			{
				break;
			}
		}
		return inventoryItem.getItemId();
	}

	public int getBugFromBiomWithLowChanceOfCommon(InventoryLootTableTimeWeatherMaster biome)
	{
		int i = 0;
		InventoryItem inventoryItem = biome.getInventoryItem();
		for (; i < 15; i++)
		{
			if (inventoryItem.bug.mySeason.myRarity != 0)
			{
				break;
			}
		}
		return inventoryItem.getItemId();
	}

	public void setRandomBugNoAndLocation()
	{
		switch (Random.Range(0, 5))
		{
		case 0:
			specificDesiredItem = Inventory.inv.getInvItemId(AnimalManager.manage.bushlandBugs.getInventoryItem());
			itemFoundInLocation = AnimalManager.manage.bushlandBugs.locationName;
			break;
		case 1:
			specificDesiredItem = Inventory.inv.getInvItemId(AnimalManager.manage.desertBugs.getInventoryItem());
			itemFoundInLocation = AnimalManager.manage.desertBugs.locationName;
			break;
		case 2:
			specificDesiredItem = Inventory.inv.getInvItemId(AnimalManager.manage.topicalBugs.getInventoryItem());
			itemFoundInLocation = AnimalManager.manage.topicalBugs.locationName;
			break;
		case 3:
			specificDesiredItem = Inventory.inv.getInvItemId(AnimalManager.manage.plainsBugs.getInventoryItem());
			itemFoundInLocation = AnimalManager.manage.plainsBugs.locationName;
			break;
		default:
			specificDesiredItem = Inventory.inv.getInvItemId(AnimalManager.manage.pineLandBugs.getInventoryItem());
			itemFoundInLocation = AnimalManager.manage.pineLandBugs.locationName;
			break;
		}
	}

	public void getRandomItemToSell()
	{
		bool flag = false;
		int num = 0;
		while (!flag)
		{
			num = Random.Range(0, Inventory.inv.allItems.Length);
			if ((bool)Inventory.inv.allItems[num].equipable)
			{
				if (Inventory.inv.allItems[num].equipable.hat || Inventory.inv.allItems[num].equipable.shirt || Inventory.inv.allItems[num].equipable.pants || Inventory.inv.allItems[num].equipable.shoes)
				{
					flag = true;
				}
				else if (Inventory.inv.allItems[num].isFurniture)
				{
					flag = true;
				}
				else if ((bool)Inventory.inv.allItems[num].consumeable)
				{
					flag = true;
				}
			}
		}
		specificDesiredItem = num;
		sellPrice = (int)((float)Inventory.inv.allItems[num].value * Random.Range(0.5f, 3.5f));
		desiredAmount = 1;
	}

	public int getSellPrice()
	{
		return sellPrice;
	}

	public int checkAmountOfItemsInInv()
	{
		return Inventory.inv.getAmountOfItemInAllSlots(specificDesiredItem);
	}

	public string getMissionText(int npcId)
	{
		if (specificDesiredItem != -1)
		{
			string text = " [" + checkAmountOfItemsInInv() + "/" + desiredAmount + "]";
			if (checkAmountOfItemsInInv() >= desiredAmount)
			{
				return "<sprite=13> Collect " + getDesiredItemName() + text + "\n<sprite=12> Bring " + NPCManager.manage.NPCDetails[npcId].NPCName + " " + getDesiredItemName();
			}
			return "<sprite=12> Collect " + getDesiredItemName() + text + "\n<sprite=12> Bring " + NPCManager.manage.NPCDetails[npcId].NPCName + " " + getDesiredItemName();
		}
		return "<sprite=12> Bring " + NPCManager.manage.NPCDetails[npcId].NPCName + " " + getDesiredItemName();
	}

	public bool checkIfNPCAccepts(int itemId)
	{
		InventoryItem inventoryItem = Inventory.inv.allItems[itemId];
		if (inventoryItem == NPCManager.manage.NPCDetails[myNPCId].hatedFood)
		{
			return false;
		}
		if (inventoryItem == NPCManager.manage.NPCDetails[myNPCId].favouriteFood)
		{
			return true;
		}
		if ((bool)inventoryItem.consumeable)
		{
			if (inventoryItem.consumeable.isMeat && NPCManager.manage.NPCDetails[myNPCId].hatesMeat)
			{
				return false;
			}
			if (inventoryItem.consumeable.isAnimalProduct && NPCManager.manage.NPCDetails[myNPCId].hatesAnimalProducts)
			{
				return false;
			}
			if (inventoryItem.consumeable.isFruit && NPCManager.manage.NPCDetails[myNPCId].hatesFruits)
			{
				return false;
			}
			if (inventoryItem.consumeable.isVegitable && NPCManager.manage.NPCDetails[myNPCId].hatesVegitables)
			{
				return false;
			}
		}
		return true;
	}

	public void getRandomQuest()
	{
		bool flag = false;
		while (!flag)
		{
			int num = Random.Range(0, 16);
			if (num <= 2)
			{
				myRequestType = requestType.BugRequest;
				flag = true;
				setRandomBugNoAndLocation();
				continue;
			}
			if (num <= 4 && LicenceManager.manage.allLicences[3].getCurrentLevel() >= 1)
			{
				myRequestType = requestType.FishRequest;
				flag = true;
				setRandomFishNoAndLocation();
				continue;
			}
			if (num <= 6)
			{
				myRequestType = requestType.FoodRequest;
				if (Random.Range(0, 11) < 5 && !NPCManager.manage.NPCDetails[myNPCId].hatesMeat && LicenceManager.manage.allLicences[4].getCurrentLevel() >= 1)
				{
					specificDesiredItem = RequestItemGenerator.request.getRandomMeatInt();
					while ((bool)NPCManager.manage.NPCDetails[myNPCId].hatedFood && specificDesiredItem == Inventory.inv.getInvItemId(NPCManager.manage.NPCDetails[myNPCId].hatedFood))
					{
						specificDesiredItem = RequestItemGenerator.request.getRandomMeatInt();
					}
				}
				else if (Random.Range(0, 11) < 5 && !NPCManager.manage.NPCDetails[myNPCId].hatesAnimalProducts && LicenceManager.manage.allLicences[9].getCurrentLevel() >= 1)
				{
					specificDesiredItem = RequestItemGenerator.request.getRandomAnimalProduct();
					while ((bool)NPCManager.manage.NPCDetails[myNPCId].hatedFood && specificDesiredItem == Inventory.inv.getInvItemId(NPCManager.manage.NPCDetails[myNPCId].hatedFood))
					{
						specificDesiredItem = RequestItemGenerator.request.getRandomAnimalProduct();
					}
				}
				else if (Random.Range(0, 11) < 5 && !NPCManager.manage.NPCDetails[myNPCId].hatesFruits)
				{
					specificDesiredItem = RequestItemGenerator.request.getRandomFruit();
					while ((bool)NPCManager.manage.NPCDetails[myNPCId].hatedFood && specificDesiredItem == Inventory.inv.getInvItemId(NPCManager.manage.NPCDetails[myNPCId].hatedFood))
					{
						specificDesiredItem = RequestItemGenerator.request.getRandomFruit();
					}
				}
				flag = true;
				continue;
			}
			switch (num)
			{
			case 7:
				myRequestType = requestType.InventoryItemRequest;
				flag = true;
				continue;
			case 8:
				if (Random.Range(0, 11) <= 5)
				{
					myRequestType = requestType.FurnitureRequest;
					flag = true;
				}
				continue;
			case 9:
				if (Random.Range(0, 11) <= 5)
				{
					myRequestType = requestType.ClothingRequest;
					flag = true;
				}
				continue;
			case 10:
				if (Random.Range(0, 4) == 2)
				{
					myRequestType = requestType.InventoryItemRequest;
					flag = true;
					continue;
				}
				break;
			}
			if (num == 11 && (float)NPCManager.manage.npcStatus[myNPCId].relationshipLevel < 25f && LicenceManager.manage.allLicences[2].hasALevelOneOrHigher())
			{
				myRequestType = requestType.WoodRequest;
				desiredAmount = Random.Range(2, 7);
				specificDesiredItem = RequestItemGenerator.request.getRandomWood();
				flag = true;
			}
			else if (num == 12 && (float)NPCManager.manage.npcStatus[myNPCId].relationshipLevel < 25f && LicenceManager.manage.allLicences[2].hasALevelOneOrHigher())
			{
				myRequestType = requestType.PlankRequest;
				desiredAmount = Random.Range(2, 7);
				specificDesiredItem = RequestItemGenerator.request.getRandomPlank();
				flag = true;
			}
			else if (num == 13 && (float)NPCManager.manage.npcStatus[myNPCId].relationshipLevel < 35f && LicenceManager.manage.allLicences[1].hasALevelOneOrHigher())
			{
				myRequestType = requestType.OreBarRequest;
				desiredAmount = Random.Range(1, 3);
				specificDesiredItem = RequestItemGenerator.request.getRandomOreBar();
				flag = true;
			}
			else if (num <= 15)
			{
				myRequestType = requestType.SellItemRequest;
				getRandomItemToSell();
				flag = true;
			}
		}
	}
}
