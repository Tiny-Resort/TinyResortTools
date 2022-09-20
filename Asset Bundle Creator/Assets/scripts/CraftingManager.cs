using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingManager : MonoBehaviour
{
	public enum CraftingMenuType
	{
		None = 0,
		CraftingTable = 1,
		CookingTable = 2,
		PostOffice = 3,
		CraftingShop = 4,
		TrapperShop = 5,
		Blocked = 6
	}

	public static CraftingManager manage;

	public Transform CraftWindow;

	public Transform RecipeList;

	public WindowAnimator recipeWindowAnim;

	private RectTransform recipeListTrans;

	public RectTransform recipeMask;

	public Transform RecipeWindow;

	public Transform RecipeIngredients;

	public Transform CraftButton;

	public GameObject craftWindowPopup;

	public GameObject craftProgressionBar;

	public GameObject scrollBar;

	public Image craftBarFill;

	public GameObject completedItemWindow;

	public FillRecipeSlot completedItemIcon;

	public List<FillRecipeSlot> recipeButtons;

	public List<GameObject> currentRecipeObjects;

	public GameObject recipeButton;

	public GameObject craftsmanRecipeButton;

	public GameObject recipeSlot;

	public Text craftCostText;

	public TextMeshProUGUI craftButtonText;

	public TextMeshProUGUI craftingText;

	public int craftableItemId = -1;

	public bool craftMenuOpen;

	private Vector2 desiredPos;

	public GameObject upButton;

	public GameObject downButton;

	private InventoryItem[] craftableOnceItems;

	public InventoryItem[] deedsCraftableAtStart;

	public SnapSelectionForWindow snapBack;

	public GameObject pinRecipeButton;

	public int[] craftableRecipeIds;

	public InventoryItem repairKit;

	public GameObject[] topButtons;

	public Image[] craftableBoxColours;

	private CraftingMenuType showingRecipesFromMenu;

	private Recipe.CraftingCatagory sortingBy = Recipe.CraftingCatagory.All;

	public bool specialCraftMenu;

	private bool crafting;

	private CraftingMenuType menuTypeOpen = CraftingMenuType.CraftingTable;

	private int currentVariation = -1;

	public GameObject variationLeftButton;

	public GameObject variationRightButton;

	private void Awake()
	{
		manage = this;
		desiredPos = new Vector2(0f, -5f);
	}

	private void Start()
	{
		recipeListTrans = RecipeList.GetComponent<RectTransform>();
		int num = 0;
		List<int> list = new List<int>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].craftable)
			{
				list.Add(i);
			}
			if ((bool)Inventory.inv.allItems[i].craftable && Inventory.inv.allItems[i].craftable.buildOnce)
			{
				num++;
			}
		}
		craftableRecipeIds = list.ToArray();
		craftableOnceItems = new InventoryItem[num];
		num = 0;
		for (int j = 0; j < Inventory.inv.allItems.Length; j++)
		{
			if ((bool)Inventory.inv.allItems[j].craftable && Inventory.inv.allItems[j].craftable.buildOnce)
			{
				craftableOnceItems[num] = Inventory.inv.allItems[j];
				num++;
			}
		}
		InventoryItem[] array = deedsCraftableAtStart;
		foreach (InventoryItem itemToMakeAvaliable in array)
		{
			makeRecipeAvaliable(itemToMakeAvaliable);
		}
	}

	private bool checkIfCanBeenCrafted(int itemId)
	{
		bool buildOnce = Inventory.inv.allItems[itemId].craftable.buildOnce;
		return true;
	}

	public void setCraftOnlyOnceToFalse(int itemId)
	{
		for (int i = 0; i < craftableOnceItems.Length; i++)
		{
			bool flag = craftableOnceItems[i] == Inventory.inv.allItems[itemId];
		}
	}

	public void makeRecipeAvaliable(InventoryItem itemToMakeAvaliable)
	{
		for (int i = 0; i < craftableOnceItems.Length; i++)
		{
			bool flag = craftableOnceItems[i] == itemToMakeAvaliable;
		}
	}

	public bool isRecipeAvaliable(InventoryItem itemToCheck)
	{
		for (int i = 0; i < craftableOnceItems.Length; i++)
		{
			bool flag = craftableOnceItems[i] == itemToCheck;
		}
		return false;
	}

	public void changeListSort(Recipe.CraftingCatagory sortBy)
	{
		if (sortingBy != sortBy)
		{
			sortingBy = sortBy;
			populateCraftList(showingRecipesFromMenu);
			Inventory.inv.activeScrollBar.resetToTop();
			recipeListTrans.anchoredPosition = desiredPos;
			snapBack.reselectDelay();
		}
	}

	public IEnumerator startCrafting(int currentlyCrafting)
	{
		if (showingRecipesFromMenu == CraftingMenuType.CraftingShop)
		{
			CraftsmanManager.manage.askAboutCraftingItem(Inventory.inv.allItems[currentlyCrafting]);
			openCloseCraftMenu(false);
			yield break;
		}
		if (showingRecipesFromMenu == CraftingMenuType.TrapperShop)
		{
			craftItem(currentlyCrafting);
			openCloseCraftMenu(false);
			CraftsmanManager.manage.trapperCraftingCompleted.startLineAlt.talkingAboutItem = Inventory.inv.allItems[currentlyCrafting];
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, CraftsmanManager.manage.trapperCraftingCompleted);
			yield break;
		}
		crafting = true;
		CraftButton.gameObject.SetActive(false);
		pinRecipeButton.SetActive(false);
		craftProgressionBar.SetActive(true);
		if (menuTypeOpen != CraftingMenuType.CookingTable)
		{
			NetworkMapSharer.share.localChar.myEquip.startCrafting();
		}
		else
		{
			NetworkMapSharer.share.localChar.myEquip.startCooking();
		}
		float timer = 0f;
		while (timer < 1.5f)
		{
			timer += Time.deltaTime;
			craftBarFill.fillAmount = timer / 1.5f;
			yield return null;
		}
		crafting = false;
		CraftButton.gameObject.SetActive(true);
		pinRecipeButton.SetActive(true);
		craftProgressionBar.SetActive(false);
		craftItem(currentlyCrafting);
		if (Inventory.inv.allItems[currentlyCrafting].craftable.completeTaskOnCraft != 0)
		{
			DailyTaskGenerator.generate.doATask(Inventory.inv.allItems[currentlyCrafting].craftable.completeTaskOnCraft);
		}
		if (showingRecipesFromMenu == CraftingMenuType.CookingTable)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CookAtCookingTable);
		}
		else
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CraftAnything);
		}
		updateCanBeCraftedOnAllRecipeButtons();
	}

	public void checkIfNeedTopButtons()
	{
		if (craftWindowPopup.activeSelf)
		{
			topButtons[0].SetActive(false);
			topButtons[1].SetActive(false);
			topButtons[0].GetComponent<ButtonTabs>().selectFirstButtonOnEnable = false;
		}
		else if (menuTypeOpen == CraftingMenuType.CraftingTable)
		{
			topButtons[0].SetActive(true);
			topButtons[1].SetActive(true);
			topButtons[0].GetComponent<ButtonTabs>().selectFirstButtonOnEnable = true;
		}
		else
		{
			topButtons[0].SetActive(false);
			topButtons[1].SetActive(false);
		}
	}

	private void populateCraftList(CraftingMenuType listType = CraftingMenuType.CraftingTable)
	{
		menuTypeOpen = listType;
		checkIfNeedTopButtons();
		switch (listType)
		{
		case CraftingMenuType.CookingTable:
			craftButtonText.text = "COOK";
			craftingText.text = "COOKING";
			break;
		case CraftingMenuType.CraftingShop:
			craftButtonText.text = "COMMISSION";
			craftingText.text = "CRAFTING";
			break;
		default:
			craftButtonText.text = "CRAFT";
			craftingText.text = "CRAFTING";
			break;
		}
		specialCraftMenu = true;
		GameObject original = recipeButton;
		GridLayoutGroup component = RecipeList.GetComponent<GridLayoutGroup>();
		if (listType == CraftingMenuType.CraftingShop || listType == CraftingMenuType.TrapperShop)
		{
			original = craftsmanRecipeButton;
			component.cellSize = new Vector2(412f, 70f);
			component.constraintCount = 1;
		}
		else
		{
			component.cellSize = new Vector2(76.8f, 105.600006f);
			component.constraintCount = 8;
		}
		foreach (FillRecipeSlot recipeButton in recipeButtons)
		{
			Object.Destroy(recipeButton.gameObject);
		}
		recipeButtons.Clear();
		showingRecipesFromMenu = listType;
		for (int i = 0; i < craftableRecipeIds.Length; i++)
		{
			int num = craftableRecipeIds[i];
			if (((Inventory.inv.allItems[num].craftable.isDeed && Inventory.inv.allItems[num].craftable.workPlaceConditions == CraftingMenuType.None && listType == CraftingMenuType.PostOffice) || (CharLevelManager.manage.checkIfUnlocked(num) && Inventory.inv.allItems[num].craftable.workPlaceConditions == CraftingMenuType.None && listType == CraftingMenuType.CraftingTable) || (!Inventory.inv.allItems[num].craftable.isDeed && CharLevelManager.manage.checkIfUnlocked(num) && Inventory.inv.allItems[num].craftable.workPlaceConditions == CraftingMenuType.CraftingShop && listType == CraftingMenuType.CraftingShop) || (!Inventory.inv.allItems[num].craftable.isDeed && CharLevelManager.manage.checkIfUnlocked(num) && Inventory.inv.allItems[num].craftable.workPlaceConditions == CraftingMenuType.TrapperShop && listType == CraftingMenuType.TrapperShop) || (!Inventory.inv.allItems[num].craftable.isDeed && CharLevelManager.manage.checkIfUnlocked(num) && Inventory.inv.allItems[num].craftable.workPlaceConditions == listType)) && checkIfCanBeenCrafted(num) && (sortingBy == Recipe.CraftingCatagory.All || Inventory.inv.allItems[num].craftable.catagory == sortingBy || (Inventory.inv.allItems[num].craftable.catagory == Recipe.CraftingCatagory.None && sortingBy == Recipe.CraftingCatagory.Misc)))
			{
				recipeButtons.Add(Object.Instantiate(original, RecipeList).GetComponent<FillRecipeSlot>());
				recipeButtons[recipeButtons.Count - 1].GetComponent<InvButton>().craftRecipeNumber = num;
				recipeButtons[recipeButtons.Count - 1].fillRecipeSlot(num);
				if (showingRecipesFromMenu == CraftingMenuType.CraftingShop || showingRecipesFromMenu == CraftingMenuType.TrapperShop)
				{
					recipeButtons[recipeButtons.Count - 1].transform.Find("Price").GetComponent<TextMeshProUGUI>().text = "<sprite=11> " + (Inventory.inv.allItems[num].value * 2).ToString("n0");
					recipeButtons[recipeButtons.Count - 1].transform.Find("Titlebox").GetComponent<Image>().color = UIAnimationManager.manage.getSlotColour(num);
				}
			}
		}
		sortRecipeList();
	}

	public void sortRecipeList()
	{
		recipeButtons.Sort(sortButtons);
		for (int i = 0; i < recipeButtons.Count; i++)
		{
			recipeButtons[i].transform.SetSiblingIndex(i);
		}
	}

	public void closeCraftPopup()
	{
		RecipeList.parent.gameObject.SetActive(true);
		craftWindowPopup.SetActive(false);
		checkIfNeedTopButtons();
		scrollBar.SetActive(true);
	}

	public int sortButtons(FillRecipeSlot a, FillRecipeSlot b)
	{
		if (a.itemInSlot.craftable.catagory < b.itemInSlot.craftable.catagory)
		{
			return -1;
		}
		if (a.itemInSlot.craftable.catagory > b.itemInSlot.craftable.catagory)
		{
			return 1;
		}
		if (a.itemInSlot.craftable.subCatagory < b.itemInSlot.craftable.subCatagory)
		{
			return -1;
		}
		if (a.itemInSlot.craftable.subCatagory > b.itemInSlot.craftable.subCatagory)
		{
			return 1;
		}
		if (a.itemInSlot.craftable.tierLevel < b.itemInSlot.craftable.tierLevel)
		{
			return -1;
		}
		if (a.itemInSlot.craftable.tierLevel > b.itemInSlot.craftable.tierLevel)
		{
			return 1;
		}
		if (a.itemInSlot.getItemId() < b.itemInSlot.getItemId())
		{
			return -1;
		}
		if (a.itemInSlot.getItemId() > b.itemInSlot.getItemId())
		{
			return 1;
		}
		return 0;
	}

	private void fillRecipeIngredients(int recipeNo, int variation)
	{
		if (variation == -1)
		{
			for (int i = 0; i < Inventory.inv.allItems[recipeNo].craftable.itemsInRecipe.Length; i++)
			{
				int invItemId = Inventory.inv.getInvItemId(Inventory.inv.allItems[recipeNo].craftable.itemsInRecipe[i]);
				currentRecipeObjects.Add(Object.Instantiate(recipeSlot, RecipeIngredients));
				currentRecipeObjects[currentRecipeObjects.Count - 1].GetComponent<FillRecipeSlot>().fillRecipeSlotWithAmounts(invItemId, Inventory.inv.getAmountOfItemInAllSlots(invItemId), Inventory.inv.allItems[recipeNo].craftable.stackOfItemsInRecipe[i]);
			}
		}
		else
		{
			for (int j = 0; j < Inventory.inv.allItems[recipeNo].craftable.altRecipes[variation].itemsInRecipe.Length; j++)
			{
				int invItemId2 = Inventory.inv.getInvItemId(Inventory.inv.allItems[recipeNo].craftable.altRecipes[variation].itemsInRecipe[j]);
				currentRecipeObjects.Add(Object.Instantiate(recipeSlot, RecipeIngredients));
				currentRecipeObjects[currentRecipeObjects.Count - 1].GetComponent<FillRecipeSlot>().fillRecipeSlotWithAmounts(invItemId2, Inventory.inv.getAmountOfItemInAllSlots(invItemId2), Inventory.inv.allItems[recipeNo].craftable.altRecipes[variation].stackOfItemsInRecipe[j]);
			}
		}
	}

	public void changeVariation(int dif)
	{
		currentVariation += dif;
		if (currentVariation < -1)
		{
			currentVariation = Inventory.inv.allItems[craftableItemId].craftable.altRecipes.Length - 1;
		}
		else if (currentVariation > Inventory.inv.allItems[craftableItemId].craftable.altRecipes.Length - 1)
		{
			currentVariation = -1;
		}
		showRecipeForItem(craftableItemId, currentVariation, false);
	}

	public void updateCanBeCraftedOnAllRecipeButtons()
	{
		for (int i = 0; i < recipeButtons.Count; i++)
		{
			recipeButtons[i].updateIfCanBeCrafted();
		}
	}

	public void showRecipeForItem(int recipeNo, int recipeVariation = -1, bool moveToAvaliableRecipe = true)
	{
		craftWindowPopup.SetActive(true);
		RecipeList.parent.gameObject.SetActive(false);
		scrollBar.SetActive(false);
		checkIfNeedTopButtons();
		currentVariation = recipeVariation;
		int num = craftableItemId;
		if (recipeNo != craftableItemId)
		{
			RecipeWindow.gameObject.SetActive(false);
		}
		craftableItemId = recipeNo;
		if (Inventory.inv.allItems[craftableItemId].craftable.altRecipes.Length != 0)
		{
			variationLeftButton.SetActive(true);
			variationRightButton.SetActive(true);
		}
		else
		{
			variationLeftButton.SetActive(false);
			variationRightButton.SetActive(false);
		}
		foreach (GameObject currentRecipeObject in currentRecipeObjects)
		{
			Object.Destroy(currentRecipeObject);
		}
		currentRecipeObjects.Clear();
		if (moveToAvaliableRecipe && recipeVariation == -1 && !canBeCrafted(recipeNo))
		{
			for (int i = 0; i < Inventory.inv.allItems[recipeNo].craftable.altRecipes.Length; i++)
			{
				currentVariation = i;
				if (canBeCrafted(recipeNo))
				{
					break;
				}
				currentVariation = recipeVariation;
			}
		}
		fillRecipeIngredients(recipeNo, currentVariation);
		RecipeWindow.gameObject.SetActive(true);
		Invoke("delaySizeRefresh", 0.001f);
		if (num != craftableItemId)
		{
			currentRecipeObjects[currentRecipeObjects.Count - 1].GetComponent<WindowAnimator>().enabled = true;
		}
		if (currentVariation == -1)
		{
			completedItemIcon.fillRecipeSlotWithCraftAmount(recipeNo, Inventory.inv.allItems[recipeNo].craftable.recipeGiveThisAmount);
		}
		else
		{
			completedItemIcon.fillRecipeSlotWithCraftAmount(recipeNo, Inventory.inv.allItems[recipeNo].craftable.altRecipes[currentVariation].recipeGiveThisAmount);
		}
		int num2 = Inventory.inv.allItems[craftableItemId].value * 2;
		if (CharLevelManager.manage.checkIfUnlocked(craftableItemId))
		{
			num2 = 0;
		}
		if (num2 == 0)
		{
			craftCostText.gameObject.SetActive(false);
		}
		else
		{
			craftCostText.gameObject.SetActive(true);
			craftCostText.text = "$" + num2;
		}
		if (Inventory.inv.wallet < num2)
		{
			craftCostText.GetComponent<FadeImagesAndText>().isFaded(true);
		}
		else
		{
			craftCostText.GetComponent<FadeImagesAndText>().isFaded(false);
		}
		completedItemWindow.SetActive(true);
		if (!crafting)
		{
			CraftButton.gameObject.SetActive(true);
			pinRecipeButton.SetActive(true);
			pinRecipeButton.transform.SetAsLastSibling();
		}
		if (!canBeCrafted(recipeNo))
		{
			CraftButton.GetComponent<Image>().color = UIAnimationManager.manage.noColor;
		}
		else
		{
			CraftButton.GetComponent<Image>().color = UIAnimationManager.manage.yesColor;
		}
		fillRecipeColourBoxes();
		QuestTracker.track.updatePinnedRecipeButton();
	}

	private void fillRecipeColourBoxes()
	{
		for (int i = 0; i < craftableBoxColours.Length; i++)
		{
			craftableBoxColours[i].color = UIAnimationManager.manage.getSlotColour(craftableItemId);
		}
	}

	private void delaySizeRefresh()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(RecipeIngredients.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(RecipeWindow.GetComponent<RectTransform>());
	}

	public bool canBeCraftedInAVariation(int recipeId)
	{
		int num = currentVariation;
		currentVariation = -1;
		if (canBeCrafted(recipeId))
		{
			currentVariation = num;
			return true;
		}
		for (int i = 0; i < Inventory.inv.allItems[recipeId].craftable.altRecipes.Length; i++)
		{
			currentVariation = i;
			if (canBeCrafted(recipeId))
			{
				currentVariation = num;
				return true;
			}
		}
		currentVariation = num;
		return false;
	}

	public bool canBeCrafted(int itemId)
	{
		bool result = true;
		int num = Inventory.inv.allItems[itemId].value * 2;
		if (CharLevelManager.manage.checkIfUnlocked(craftableItemId) && Inventory.inv.allItems[itemId].craftable.workPlaceConditions != CraftingMenuType.TrapperShop)
		{
			num = 0;
		}
		if (Inventory.inv.wallet < num)
		{
			return false;
		}
		if (currentVariation == -1 || Inventory.inv.allItems[itemId].craftable.altRecipes.Length == 0)
		{
			for (int i = 0; i < Inventory.inv.allItems[itemId].craftable.itemsInRecipe.Length; i++)
			{
				int invItemId = Inventory.inv.getInvItemId(Inventory.inv.allItems[itemId].craftable.itemsInRecipe[i]);
				int num2 = Inventory.inv.allItems[itemId].craftable.stackOfItemsInRecipe[i];
				if (Inventory.inv.getAmountOfItemInAllSlots(invItemId) < num2)
				{
					result = false;
					break;
				}
			}
		}
		else
		{
			for (int j = 0; j < Inventory.inv.allItems[itemId].craftable.altRecipes[currentVariation].itemsInRecipe.Length; j++)
			{
				int invItemId2 = Inventory.inv.getInvItemId(Inventory.inv.allItems[itemId].craftable.altRecipes[currentVariation].itemsInRecipe[j]);
				int num3 = Inventory.inv.allItems[itemId].craftable.altRecipes[currentVariation].stackOfItemsInRecipe[j];
				if (Inventory.inv.getAmountOfItemInAllSlots(invItemId2) < num3)
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	public void pressCraftButton()
	{
		if (!canBeCrafted(craftableItemId))
		{
			SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
		}
		else if (showingRecipesFromMenu != CraftingMenuType.CraftingShop && showingRecipesFromMenu != CraftingMenuType.TrapperShop && !Inventory.inv.checkIfItemCanFit(craftableItemId, Inventory.inv.allItems[craftableItemId].craftable.recipeGiveThisAmount))
		{
			SoundManager.manage.play2DSound(SoundManager.manage.pocketsFull);
			NotificationManager.manage.createChatNotification((LocalizedString)"ToolTips/Tip_PocketsFull", true);
		}
		else
		{
			StartCoroutine(startCrafting(craftableItemId));
		}
	}

	public void takeItemsForRecipe(int currentlyCrafting)
	{
		if (currentVariation == -1)
		{
			for (int i = 0; i < Inventory.inv.allItems[currentlyCrafting].craftable.itemsInRecipe.Length; i++)
			{
				int invItemId = Inventory.inv.getInvItemId(Inventory.inv.allItems[currentlyCrafting].craftable.itemsInRecipe[i]);
				int amountToRemove = Inventory.inv.allItems[currentlyCrafting].craftable.stackOfItemsInRecipe[i];
				Inventory.inv.removeAmountOfItem(invItemId, amountToRemove);
			}
		}
		else
		{
			for (int j = 0; j < Inventory.inv.allItems[currentlyCrafting].craftable.altRecipes[currentVariation].itemsInRecipe.Length; j++)
			{
				int invItemId2 = Inventory.inv.getInvItemId(Inventory.inv.allItems[currentlyCrafting].craftable.altRecipes[currentVariation].itemsInRecipe[j]);
				int amountToRemove2 = Inventory.inv.allItems[currentlyCrafting].craftable.altRecipes[currentVariation].stackOfItemsInRecipe[j];
				Inventory.inv.removeAmountOfItem(invItemId2, amountToRemove2);
			}
		}
	}

	public void craftItem(int currentlyCrafting)
	{
		int num = Inventory.inv.allItems[currentlyCrafting].value * 2;
		if (CharLevelManager.manage.checkIfUnlocked(currentlyCrafting) && showingRecipesFromMenu != CraftingMenuType.TrapperShop)
		{
			num = 0;
		}
		if (showingRecipesFromMenu == CraftingMenuType.CraftingShop)
		{
			if ((bool)NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Craft_Workshop))
			{
				NPCManager.manage.npcStatus[NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Craft_Workshop).myId.NPCNo].moneySpentAtStore += Inventory.inv.allItems[currentlyCrafting].value;
			}
			return;
		}
		CraftingMenuType showingRecipesFromMenu2 = showingRecipesFromMenu;
		int num2 = 5;
		takeItemsForRecipe(currentlyCrafting);
		Inventory.inv.changeWallet(-num);
		showRecipeForItem(craftableItemId, currentVariation);
		if (Inventory.inv.allItems[currentlyCrafting].craftable.buildOnce)
		{
			setCraftOnlyOnceToFalse(currentlyCrafting);
			populateCraftList(CraftingMenuType.PostOffice);
			RecipeWindow.gameObject.SetActive(false);
		}
		else
		{
			foreach (FillRecipeSlot recipeButton in recipeButtons)
			{
				recipeButton.refreshRecipeSlot();
			}
		}
		if (Inventory.inv.allItems[currentlyCrafting].hasFuel)
		{
			Inventory.inv.addItemToInventory(currentlyCrafting, Inventory.inv.allItems[currentlyCrafting].fuelMax);
		}
		else if (currentVariation == -1)
		{
			Inventory.inv.addItemToInventory(currentlyCrafting, Inventory.inv.allItems[currentlyCrafting].craftable.recipeGiveThisAmount);
		}
		else
		{
			Inventory.inv.addItemToInventory(currentlyCrafting, Inventory.inv.allItems[currentlyCrafting].craftable.altRecipes[currentVariation].recipeGiveThisAmount);
		}
		SoundManager.manage.play2DSound(SoundManager.manage.craftingComplete);
	}

	public void openCloseCraftMenu(bool isMenuOpen, CraftingMenuType optionsType = CraftingMenuType.None)
	{
		CraftButton.gameObject.SetActive(false);
		pinRecipeButton.SetActive(false);
		completedItemWindow.SetActive(false);
		craftCostText.text = "";
		craftMenuOpen = isMenuOpen;
		CraftWindow.gameObject.SetActive(isMenuOpen);
		desiredPos = new Vector2(0f, -5f);
		sortingBy = Recipe.CraftingCatagory.All;
		closeCraftPopup();
		if (!isMenuOpen && (showingRecipesFromMenu == CraftingMenuType.CraftingShop || showingRecipesFromMenu == CraftingMenuType.PostOffice))
		{
			ConversationManager.manage.checkIfYouWereTalkingToNPCAndStopTalkingAfterMenuCloses();
		}
		if (!isMenuOpen)
		{
			RecipeWindow.gameObject.SetActive(isMenuOpen);
		}
		else
		{
			populateCraftList(optionsType);
		}
		Inventory.inv.checkIfWindowIsNeeded();
		if (isMenuOpen)
		{
			CurrencyWindows.currency.openJournal();
		}
		else
		{
			CurrencyWindows.currency.closeJournal();
		}
	}

	public void repairItemsInPockets()
	{
		StartCoroutine(delayRepair());
		NetworkMapSharer.share.localChar.myEquip.startCrafting();
		Inventory.inv.removeAmountOfItem(repairKit.getItemId(), 1);
	}

	private IEnumerator delayRepair()
	{
		yield return new WaitForSeconds(1f);
		for (int i = 0; i < Inventory.inv.invSlots.Length; i++)
		{
			if ((bool)Inventory.inv.invSlots[i].itemInSlot && Inventory.inv.invSlots[i].itemInSlot.hasFuel && Inventory.inv.invSlots[i].itemInSlot.isRepairable)
			{
				Inventory.inv.invSlots[i].updateSlotContentsAndRefresh(Inventory.inv.invSlots[i].itemNo, Inventory.inv.invSlots[i].itemInSlot.fuelMax);
			}
		}
		SoundManager.manage.play2DSound(SoundManager.manage.craftingComplete);
	}
}
