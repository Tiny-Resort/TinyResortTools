using UnityEngine;

public class CraftsmanManager : MonoBehaviour
{
	public static CraftsmanManager manage;

	public int currentPoints;

	public int currentLevel;

	public InventoryItem shinyDiscItem;

	public Conversation hasLearnedANewRecipeIcon;

	public Conversation canCraftItem;

	public Conversation canCraftItemNoMoney;

	public Conversation doesNotHaveLicence;

	public Conversation agreeToCraftingConvo;

	public Conversation lookingForTechConvo;

	[Header("Vendor Convos ----------")]
	public Conversation normalWorkConvo;

	public Conversation currentlyCraftingConvo;

	public Conversation itemIsCompleted;

	public Conversation giveItemOnComplete;

	public Conversation itemCompletedNoSpace;

	private InventoryItem itemAskingAbout;

	public int itemCurrentlyCrafting = -1;

	[Header("Trapper Convos ----------")]
	public Conversation trapperCraftingCompleted;

	private void Awake()
	{
		manage = this;
	}

	public void giveCraftsmanXp()
	{
		currentPoints += Mathf.RoundToInt(GiveNPC.give.moneyOffer / 6 / shinyDiscItem.value);
		if (NetworkMapSharer.share.isServer)
		{
			NPCManager.manage.npcStatus[2].moneySpentAtStore += GiveNPC.give.moneyOffer;
		}
		while (currentPoints >= getPointsForNextLevel(currentLevel + 1))
		{
			currentPoints -= getPointsForNextLevel(currentLevel + 1);
			currentLevel++;
		}
	}

	public int getPointsForNextLevel(int levelToCheck)
	{
		if (levelToCheck == 1)
		{
			return 1;
		}
		if (levelToCheck <= 4)
		{
			return 2;
		}
		if (levelToCheck <= 7)
		{
			return 3;
		}
		return 4;
	}

	public void askAboutCraftingItem(InventoryItem item)
	{
		itemAskingAbout = item;
		doesNotHaveLicence.startLineAlt.talkingAboutItem = item;
		canCraftItem.startLineAlt.talkingAboutItem = item;
		canCraftItemNoMoney.startLineAlt.talkingAboutItem = item;
		if (item.requiredToBuy != 0 && LicenceManager.manage.allLicences[(int)item.requiredToBuy].getCurrentLevel() < item.requiredLicenceLevel)
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, doesNotHaveLicence);
		}
		else if (Inventory.inv.wallet >= getCraftingPrice())
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, canCraftItem);
		}
		else
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, canCraftItemNoMoney);
		}
	}

	public void agreeToCrafting()
	{
		CraftingManager.manage.takeItemsForRecipe(Inventory.inv.getInvItemId(itemAskingAbout));
		Inventory.inv.changeWallet(-itemAskingAbout.value * 2);
		NPCManager.manage.npcStatus[NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Craft_Workshop).myId.NPCNo].moneySpentAtStore += itemAskingAbout.value * 2;
		ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, agreeToCraftingConvo);
		NetworkMapSharer.share.localChar.CmdAgreeToCraftsmanCrafting();
		itemCurrentlyCrafting = Inventory.inv.getInvItemId(itemAskingAbout);
		MailManager.manage.tomorrowsLetters.Add(new Letter(2, Letter.LetterType.CraftsmanClosedLetter, manage.itemCurrentlyCrafting, manage.getAmountOnCraft()));
		manage.itemCurrentlyCrafting = -1;
		manage.switchCrafterConvo();
	}

	public int getCraftingPrice()
	{
		return itemAskingAbout.value * 2;
	}

	public int getAmountOnCraft()
	{
		int result = 1;
		if ((bool)Inventory.inv.allItems[itemCurrentlyCrafting].craftable)
		{
			result = Inventory.inv.allItems[itemCurrentlyCrafting].craftable.recipeGiveThisAmount;
		}
		if (Inventory.inv.allItems[itemCurrentlyCrafting].hasFuel)
		{
			result = Inventory.inv.allItems[itemCurrentlyCrafting].fuelMax;
		}
		return result;
	}

	public void tryAndGiveCompletedItem()
	{
		int stackAmount = 1;
		if ((bool)Inventory.inv.allItems[itemCurrentlyCrafting].craftable)
		{
			stackAmount = Inventory.inv.allItems[itemCurrentlyCrafting].craftable.recipeGiveThisAmount;
		}
		if (Inventory.inv.allItems[itemCurrentlyCrafting].hasFuel)
		{
			stackAmount = Inventory.inv.allItems[itemCurrentlyCrafting].fuelMax;
		}
		if (Inventory.inv.addItemToInventory(itemCurrentlyCrafting, stackAmount))
		{
			giveItemOnComplete.startLineAlt.talkingAboutItem = Inventory.inv.allItems[itemCurrentlyCrafting];
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, giveItemOnComplete);
			itemCurrentlyCrafting = -1;
			switchCrafterConvo();
		}
		else
		{
			itemCompletedNoSpace.startLineAlt.talkingAboutItem = Inventory.inv.allItems[itemCurrentlyCrafting];
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, itemCompletedNoSpace);
		}
	}

	public void switchCrafterConvo()
	{
		if (NetworkMapSharer.share.craftsmanWorking)
		{
			NPCManager.manage.NPCDetails[2].keeperConversation = currentlyCraftingConvo;
		}
		else if (NetworkMapSharer.share.isServer && itemCurrentlyCrafting != -1)
		{
			NPCManager.manage.NPCDetails[2].keeperConversation = itemIsCompleted;
		}
		else
		{
			NPCManager.manage.NPCDetails[2].keeperConversation = normalWorkConvo;
		}
	}

	public bool craftsmanHasItemReady()
	{
		return itemCurrentlyCrafting != -1;
	}
}
