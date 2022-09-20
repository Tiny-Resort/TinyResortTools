using System;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LicenceManager : MonoBehaviour
{
	public enum LicenceTypes
	{
		None = 0,
		Mining = 1,
		Logging = 2,
		Fishing = 3,
		Hunting = 4,
		LandScaping = 5,
		MetalDetecting = 6,
		Vehicle = 7,
		Commerce = 8,
		AnimalHandling = 9,
		Bridge = 10,
		Farming = 11,
		Cargo = 12,
		DeepMining = 13,
		ToolBelt = 14,
		AnimalTrapping = 15,
		Excavation = 16,
		Irrigation = 17,
		AgriculturalVehicle = 18
	}

	public enum LicenceSortingGroup
	{
		None = 0,
		Mining = 1,
		Logging = 2,
		Fishing = 3,
		Digging = 4,
		MetalDetecting = 5,
		Hunting = 6,
		Farming = 7,
		AnimalFarming = 8,
		Inventory = 9,
		Building = 10,
		Vechile = 11,
		Bonus = 12
	}

	public static LicenceManager manage;

	public GameObject licenceButtonPrefab;

	public Licence[] allLicences;

	public Sprite[] licenceIcons;

	public Color[] licenceColours;

	public bool windowOpen;

	public GameObject licenceWindow;

	public GameObject confirmWindow;

	public GameObject licenceListWindow;

	public Transform licenceButtonParent;

	public Image licenceIcon;

	public TextMeshProUGUI confirmWindowTitle;

	public TextMeshProUGUI confirmWindowLevel;

	public TextMeshProUGUI confirmWindowDescription;

	public TextMeshProUGUI confirmWindowCost;

	public InvButton confirmWindowConfirmButton;

	private List<LicenceButton> licenceButtons = new List<LicenceButton>();

	private List<LicenceButton> journalButtons = new List<LicenceButton>();

	[Header("Journal Details")]
	public Transform journalWindow;

	public GameObject journalButtonPrefab;

	private Licence licenceToConfirm;

	private void Awake()
	{
		manage = this;
		allLicences = new Licence[Enum.GetNames(typeof(LicenceTypes)).Length];
	}

	private void Start()
	{
		for (int i = 1; i < allLicences.Length; i++)
		{
			allLicences[i] = new Licence((LicenceTypes)i);
			LicenceButton component = UnityEngine.Object.Instantiate(licenceButtonPrefab, licenceButtonParent).GetComponent<LicenceButton>();
			component.fillButton(i);
			licenceButtons.Add(component);
			LicenceButton component2 = UnityEngine.Object.Instantiate(journalButtonPrefab, journalWindow).GetComponent<LicenceButton>();
			component2.fillDetailsForJournal(i);
			journalButtons.Add(component2);
		}
		setLicenceLevelsAndPrice();
		for (int j = 0; j < licenceButtons.Count; j++)
		{
			licenceButtons[j].updateButton();
			journalButtons[j].updateJournalButton();
		}
		sortLicenceList(licenceButtons);
		sortLicenceList(journalButtons);
	}

	public void setLicenceLevelsAndPrice()
	{
		allLicences[1].connectToSkillLevel(CharLevelManager.SkillTypes.Mining, 10);
		allLicences[11].connectToSkillLevel(CharLevelManager.SkillTypes.Farming, 10);
		allLicences[3].connectToSkillLevel(CharLevelManager.SkillTypes.Fishing, 5);
		allLicences[2].connectToSkillLevel(CharLevelManager.SkillTypes.Foraging, 10);
		allLicences[4].connectToSkillLevel(CharLevelManager.SkillTypes.Hunting, 5);
		allLicences[16].isUnlocked = true;
		allLicences[16].setLevelCost(500);
		allLicences[16].maxLevel = 1;
		allLicences[17].setLevelCost(1000);
		allLicences[17].maxLevel = 2;
		allLicences[18].setLevelCost(2000, 0);
		allLicences[18].maxLevel = 3;
		allLicences[5].setLevelCost(250, 1);
		allLicences[5].maxLevel = 2;
		allLicences[6].maxLevel = 2;
		allLicences[6].setLevelCost(500);
		allLicences[8].setLevelCost(750);
		allLicences[13].setLevelCost(3500);
		allLicences[13].maxLevel = 1;
		allLicences[15].maxLevel = 2;
		allLicences[15].setLevelCost(500, 1);
		allLicences[12].setLevelCost(500);
		allLicences[8].setLevelCost(750);
		allLicences[7].setLevelCost(1200, 1);
		allLicences[7].isUnlocked = true;
		allLicences[1].sortingNumber = 1;
		allLicences[2].sortingNumber = 2;
		allLicences[3].sortingNumber = 3;
		allLicences[4].sortingNumber = 6;
		allLicences[5].sortingNumber = 10;
		allLicences[6].sortingNumber = 5;
		allLicences[7].sortingNumber = 11;
		allLicences[8].sortingNumber = 12;
		allLicences[9].sortingNumber = 8;
		allLicences[10].sortingNumber = 10;
		allLicences[11].sortingNumber = 7;
		allLicences[12].sortingNumber = 9;
		allLicences[13].sortingNumber = 1;
		allLicences[14].sortingNumber = 9;
		allLicences[15].sortingNumber = 6;
		allLicences[16].sortingNumber = 4;
		allLicences[17].sortingNumber = 7;
		allLicences[18].sortingNumber = 11;
	}

	public void checkAllLicenceRewardsOnLoad()
	{
		for (int i = 0; i < allLicences.Length; i++)
		{
			checkForUnlocksOnLevelUp(allLicences[i], true);
		}
	}

	public void sortLicenceList(List<LicenceButton> listToSort)
	{
		listToSort.Sort(sortButtons);
		for (int i = 0; i < listToSort.Count; i++)
		{
			listToSort[i].transform.SetSiblingIndex(i);
		}
	}

	public int sortButtons(LicenceButton a, LicenceButton b)
	{
		if (allLicences[a.myLicenceId].sortingNumber < allLicences[b.myLicenceId].sortingNumber)
		{
			return -1;
		}
		if (allLicences[a.myLicenceId].sortingNumber > allLicences[b.myLicenceId].sortingNumber)
		{
			return 1;
		}
		if (a.myLicenceId < b.myLicenceId)
		{
			return -1;
		}
		if (a.myLicenceId > b.myLicenceId)
		{
			return 1;
		}
		return 0;
	}

	public void openLicenceWindow()
	{
		for (int i = 0; i < licenceButtons.Count; i++)
		{
			licenceButtons[i].updateButton();
		}
		closeConfirmWindow();
		windowOpen = true;
		licenceWindow.SetActive(true);
		confirmWindow.gameObject.SetActive(false);
		MenuButtonsTop.menu.closed = false;
		Inventory.inv.checkIfWindowIsNeeded();
		CurrencyWindows.currency.openJournal();
	}

	public void refreshCharacterJournalTab()
	{
		for (int i = 0; i < licenceButtons.Count; i++)
		{
			journalButtons[i].updateJournalButton();
		}
	}

	public void closeLicenceWindow()
	{
		windowOpen = false;
		licenceWindow.SetActive(false);
		confirmWindow.gameObject.SetActive(false);
		Inventory.inv.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
		CurrencyWindows.currency.closeJournal();
		refreshCharacterJournalTab();
		ConversationManager.manage.checkIfYouWereTalkingToNPCAndStopTalkingAfterMenuCloses();
	}

	public void openConfirmWindow(LicenceTypes type)
	{
		confirmWindow.gameObject.SetActive(false);
		licenceIcon.sprite = licenceIcons[(int)type];
		licenceToConfirm = allLicences[(int)type];
		confirmWindowTitle.text = getLicenceName(type);
		confirmWindowLevel.text = "Level " + (allLicences[(int)type].getCurrentLevel() + 1);
		confirmWindowDescription.text = getLicenceLevelDescription(type, allLicences[(int)type].getCurrentLevel() + 1);
		confirmWindowCost.text = "<sprite=15> " + allLicences[(int)type].getNextLevelPrice().ToString("n0");
		if (allLicences[(int)type].getCurrentLevel() == allLicences[(int)type].getCurrentMaxLevel())
		{
			confirmWindowConfirmButton.gameObject.SetActive(false);
			confirmWindowConfirmButton.enabled = false;
			confirmWindowCost.text = "";
			if (allLicences[(int)type].getCurrentLevel() == allLicences[(int)type].getMaxLevel())
			{
				confirmWindowLevel.text = "Level " + allLicences[(int)type].getCurrentLevel();
				confirmWindowDescription.text = "You hold all " + getLicenceName(type) + " levels";
			}
			else
			{
				confirmWindowLevel.text = "Level " + allLicences[(int)type].getCurrentLevel();
				if (allLicences[(int)type].isConnectedWithSkillLevel())
				{
					confirmWindowDescription.text = "Level up your " + allLicences[(int)type].getConnectedSkillName() + " skill to unlock further levels";
				}
				else
				{
					confirmWindowDescription.text = "You hold all current " + getLicenceName(type) + " levels";
				}
			}
		}
		else if (allLicences[(int)type].canAffordNextLevel())
		{
			confirmWindowConfirmButton.gameObject.SetActive(true);
			confirmWindowConfirmButton.enabled = true;
		}
		else
		{
			confirmWindowConfirmButton.gameObject.SetActive(false);
			confirmWindowConfirmButton.enabled = false;
		}
		licenceListWindow.gameObject.SetActive(false);
		confirmWindow.gameObject.SetActive(true);
	}

	public void closeConfirmWindow()
	{
		licenceListWindow.SetActive(true);
		confirmWindow.SetActive(false);
	}

	public void confirmAndBuy()
	{
		licenceToConfirm.buyNextLevel();
		for (int i = 0; i < licenceButtons.Count; i++)
		{
			licenceButtons[i].updateButton();
		}
		GiftedItemWindow.gifted.addLicenceToBeGiven((int)licenceToConfirm.type);
		closeLicenceWindow();
		GiftedItemWindow.gifted.openWindowAndGiveItems();
	}

	public string getLicenceName(LicenceTypes type)
	{
		int num = (int)type;
		return (LocalizedString)("LicenceNames/Licence_" + num);
	}

	public void checkForUnlocksOnLevelUp(Licence check, bool loadCheck = false)
	{
		if (check == null)
		{
			return;
		}
		giveRecipeOnLicenceLevelUp(check);
		if (check.type == LicenceTypes.Logging && check.getCurrentLevel() >= 1 && !allLicences[5].isUnlocked)
		{
			allLicences[5].isUnlocked = true;
			NotificationManager.manage.makeTopNotification("A new Licence is available!", getLicenceName(LicenceTypes.LandScaping));
		}
		if (check.type == LicenceTypes.Hunting && check.getCurrentLevel() == 1 && !allLicences[15].isUnlocked)
		{
			allLicences[15].isUnlocked = true;
			NotificationManager.manage.makeTopNotification("A new Licence is available!", getLicenceName(LicenceTypes.AnimalTrapping));
		}
		if (check.type == LicenceTypes.Mining && check.getCurrentLevel() >= 2 && !allLicences[13].isUnlocked)
		{
			allLicences[13].isUnlocked = true;
			NotificationManager.manage.makeTopNotification("A new Licence is available!", getLicenceName(LicenceTypes.DeepMining));
		}
		if (check.type == LicenceTypes.Excavation && check.getCurrentLevel() >= 1 && !allLicences[6].isUnlocked)
		{
			allLicences[6].isUnlocked = true;
			NotificationManager.manage.makeTopNotification("A new Licence is available!", getLicenceName(LicenceTypes.MetalDetecting));
		}
		if (check.type == LicenceTypes.DeepMining)
		{
			if (!loadCheck && check.getCurrentLevel() >= 1 && !DeedManager.manage.isDeedUnlockedAndUnbought(DeedManager.manage.mineDeed))
			{
				DeedManager.manage.unlockDeed(DeedManager.manage.mineDeed);
				NotificationManager.manage.makeTopNotification("A new deed is available!", "Talk to Fletch to apply for deeds.", SoundManager.manage.notificationSound);
			}
			if (check.getCurrentLevel() >= 1)
			{
				MonoBehaviour.print("Unlocking mining licence");
				DeedManager.manage.unlockDeed(DeedManager.manage.mineDeed);
			}
		}
		if (check.type == LicenceTypes.Farming && check.getCurrentLevel() >= 3 && !allLicences[17].isUnlocked)
		{
			allLicences[17].isUnlocked = true;
			NotificationManager.manage.makeTopNotification("A new Licence is available!", getLicenceName(LicenceTypes.Irrigation));
		}
		if (check.type == LicenceTypes.Farming && check.getCurrentLevel() >= 1 && !allLicences[9].isUnlocked)
		{
			NotificationManager.manage.makeTopNotification("A new Licence is available!", getLicenceName(LicenceTypes.AnimalHandling));
			allLicences[9].isUnlocked = true;
		}
		if (check.type == LicenceTypes.Logging && check.getCurrentLevel() >= 1 && !allLicences[10].isUnlocked)
		{
			NotificationManager.manage.makeTopNotification("A new Licence is available!", getLicenceName(LicenceTypes.Bridge));
			allLicences[10].isUnlocked = true;
		}
		if (allLicences[1].getCurrentLevel() >= 1 && allLicences[2].getCurrentLevel() >= 1 && allLicences[3].getCurrentLevel() >= 1 && allLicences[4].getCurrentLevel() >= 1 && !allLicences[14].isUnlocked)
		{
			allLicences[14].isUnlocked = true;
			NotificationManager.manage.makeTopNotification("A new Licence is available!", getLicenceName(LicenceTypes.ToolBelt));
		}
		if (allLicences[1].getCurrentLevel() >= 2 && allLicences[2].getCurrentLevel() >= 2 && allLicences[3].getCurrentLevel() >= 2 && allLicences[4].getCurrentLevel() >= 2 && !allLicences[8].isUnlocked)
		{
			allLicences[8].isUnlocked = true;
			NotificationManager.manage.makeTopNotification("A new Licence is available!", getLicenceName(LicenceTypes.Commerce));
		}
		if (allLicences[11].getCurrentLevel() >= 3 && allLicences[7].getCurrentLevel() >= 2 && allLicences[17].getCurrentLevel() >= 2 && !allLicences[18].isUnlocked)
		{
			allLicences[18].isUnlocked = true;
			NotificationManager.manage.makeTopNotification("A new Licence is available!", getLicenceName(LicenceTypes.AgriculturalVehicle));
		}
		if (check.type == LicenceTypes.ToolBelt)
		{
			if (check.getCurrentLevel() >= 1 && !allLicences[12].isUnlocked)
			{
				allLicences[12].isUnlocked = true;
				NotificationManager.manage.makeTopNotification("A new Licence is available!", getLicenceName(LicenceTypes.Cargo));
			}
			Inventory.inv.setSlotsUnlocked(true);
		}
		if (check.type == LicenceTypes.Cargo)
		{
			Inventory.inv.setSlotsUnlocked(true);
		}
	}

	public string getLicenceLevelDescription(LicenceTypes type, int level)
	{
		switch (type)
		{
		case LicenceTypes.Logging:
			return (LocalizedString)("LicenceDesc/Licence_Logging_" + level + "_Desc");
		case LicenceTypes.Mining:
			return (LocalizedString)("LicenceDesc/Licence_Mining_" + level + "_Desc");
		case LicenceTypes.LandScaping:
			return (LocalizedString)("LicenceDesc/Licence_Landscaping_" + level + "_Desc");
		case LicenceTypes.Fishing:
			return (LocalizedString)("LicenceDesc/Licence_Fishing_" + level + "_Desc");
		case LicenceTypes.Hunting:
			return (LocalizedString)("LicenceDesc/Licence_Hunting_" + level + "_Desc");
		case LicenceTypes.MetalDetecting:
			return (LocalizedString)("LicenceDesc/Licence_MetalDetecting_" + level + "_Desc");
		case LicenceTypes.Vehicle:
			return (LocalizedString)("LicenceDesc/Licence_Vehicle_" + level + "_Desc");
		case LicenceTypes.Farming:
			return (LocalizedString)("LicenceDesc/Licence_Farming_" + level + "_Desc");
		case LicenceTypes.Commerce:
			return (LocalizedString)("LicenceDesc/Licence_Commerce_" + level + "_Desc");
		case LicenceTypes.Cargo:
			return (LocalizedString)"LicenceDesc/Licence_Cargo_1_Desc";
		case LicenceTypes.ToolBelt:
			return (LocalizedString)"LicenceDesc/Licence_ToolBelt_Desc";
		case LicenceTypes.AnimalHandling:
			return (LocalizedString)("LicenceDesc/Licence_AnimalHandling_" + level + "_Desc");
		case LicenceTypes.Bridge:
			if (level == 3)
			{
				return "Coming soon. The holder will get instant access to Building Level 3 once it has arrived";
			}
			return (LocalizedString)("LicenceDesc/Licence_Building_" + level + "_Desc");
		case LicenceTypes.AnimalTrapping:
			return (LocalizedString)("LicenceDesc/Licence_AnimalTrapping_" + level + "_Desc");
		case LicenceTypes.Irrigation:
			return (LocalizedString)("LicenceDesc/Licence_Irrigation_" + level + "_Desc");
		case LicenceTypes.Excavation:
			return (LocalizedString)"LicenceDesc/Licence_Excavation_Desc";
		case LicenceTypes.AgriculturalVehicle:
			return (LocalizedString)("LicenceDesc/Licence_AgriculturalVehicle_" + level + "_Desc");
		case LicenceTypes.DeepMining:
			return (LocalizedString)"LicenceDesc/Licence_DeepMining_Desc";
		default:
			return "";
		}
	}

	public void giveRecipeOnLicenceLevelUp(Licence check)
	{
		if (check == null)
		{
			return;
		}
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].craftable && Inventory.inv.allItems[i].craftable.checkIfMeetsLicenceRequirement(check.type, check.getCurrentLevel()) && Inventory.inv.allItems[i].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.TrapperShop && !CharLevelManager.manage.checkIfUnlocked(i))
			{
				GiftedItemWindow.gifted.addRecipeToUnlock(Inventory.inv.getInvItemId(Inventory.inv.allItems[i]));
			}
		}
		GiftedItemWindow.gifted.openWindowAndGiveItems();
	}

	public void unlockRecipesAlreadyLearntFromAllLicences()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (!Inventory.inv.allItems[i].craftable || Inventory.inv.allItems[i].isDeed || Inventory.inv.allItems[i].craftable.workPlaceConditions == CraftingManager.CraftingMenuType.CraftingShop)
			{
				continue;
			}
			if (Inventory.inv.allItems[i].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.TrapperShop && Inventory.inv.allItems[i].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.Blocked)
			{
				int num = 0;
				for (int j = 0; j < Inventory.inv.allItems[i].craftable.itemsInRecipe.Length; j++)
				{
					num += Inventory.inv.allItems[i].craftable.itemsInRecipe[j].value * Inventory.inv.allItems[i].craftable.stackOfItemsInRecipe[j];
				}
				Inventory.inv.allItems[i].value = Mathf.RoundToInt((float)(num / Inventory.inv.allItems[i].craftable.recipeGiveThisAmount) * 1.25f);
			}
			if (CharLevelManager.manage.checkIfUnlocked(i))
			{
				continue;
			}
			for (int k = 1; k < allLicences.Length; k++)
			{
				if (Inventory.inv.allItems[i].craftable.checkIfMeetsLicenceRequirement(allLicences[k].type, allLicences[k].getCurrentLevel()))
				{
					CharLevelManager.manage.unlockRecipe(Inventory.inv.allItems[i]);
				}
			}
		}
		if (CatalogueManager.manage.collectedItem[DeedManager.manage.deedsUnlockedOnStart[0].getItemId()])
		{
			CharLevelManager.manage.unlockRecipe(WorldManager.manageWorld.allObjectSettings[DeedManager.manage.deedsUnlockedOnStart[0].placeable.tileObjectId].dropsItemOnDeath);
		}
	}
}
