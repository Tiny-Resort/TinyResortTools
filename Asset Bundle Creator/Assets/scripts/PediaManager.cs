using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PediaManager : MonoBehaviour
{
	public enum PediaEntryType
	{
		Fish = 0,
		Bug = 1,
		Animal = 2,
		FarmAnimal = 3,
		Relic = 4,
		Villager = 5,
		Recipe = 6,
		Critter = 7
	}

	public static PediaManager manage;

	public GameObject pediaEntryPrefab;

	public bool pediaOpen;

	public GameObject pediaWindow;

	public GameObject entryFullScreenWindow;

	public Transform pediaGridSpace;

	public List<PediaEntry> allEntries = new List<PediaEntry>();

	public List<PediaEntry> villagerEntries = new List<PediaEntry>();

	public List<PediaEntry> animalEntries = new List<PediaEntry>();

	public List<PediaEntry> recipeEntries = new List<PediaEntry>();

	public bool entryFullScreenShown;

	public Image[] timeIcons;

	public Image[] seasonIcons;

	public Color iconOnColor;

	public Color iconOffColor;

	[Header("Fullscreen Entry Details--------")]
	public TextMeshProUGUI entryTitle;

	public TextMeshProUGUI entryDesc;

	public Image entryIcon;

	public TextMeshProUGUI caughtAmountText;

	public TextMeshProUGUI locationText;

	[Header("Fullscreen Entry Details--------")]
	public TextMeshProUGUI fishAmountText;

	public TextMeshProUGUI bugAmountText;

	public TextMeshProUGUI relicAmountText;

	public TextMeshProUGUI critterAmountText;

	public GameObject bugAndFishDetailBox;

	public GameObject heartBox;

	public HeartContainer[] hearts;

	public Sprite notFoundSprite;

	private RectTransform entryIconSize;

	private List<PediaEntryButton> showingButtons = new List<PediaEntryButton>();

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
		createPediaEntries();
		entryIconSize = entryIcon.GetComponent<RectTransform>();
	}

	public void showEntryType(PediaEntryType type)
	{
		for (int i = 0; i < showingButtons.Count; i++)
		{
			Object.Destroy(showingButtons[i].gameObject);
		}
		showingButtons.Clear();
		int num = 0;
		switch (type)
		{
		case PediaEntryType.Fish:
		case PediaEntryType.Bug:
		case PediaEntryType.Relic:
		case PediaEntryType.Critter:
		{
			for (int l = 0; l < allEntries.Count; l++)
			{
				if (allEntries[l].entryType == (int)type)
				{
					PediaEntryButton component2 = Object.Instantiate(pediaEntryPrefab, pediaGridSpace).GetComponent<PediaEntryButton>();
					component2.setUpButton(allEntries[l], num);
					showingButtons.Add(component2);
					num++;
				}
			}
			break;
		}
		case PediaEntryType.Villager:
		{
			for (int m = 0; m < villagerEntries.Count; m++)
			{
				PediaEntryButton component3 = Object.Instantiate(pediaEntryPrefab, pediaGridSpace).GetComponent<PediaEntryButton>();
				component3.setUpButton(villagerEntries[m], num);
				showingButtons.Add(component3);
				num++;
			}
			break;
		}
		case PediaEntryType.Recipe:
		{
			for (int j = 0; j < recipeEntries.Count; j++)
			{
				if (CharLevelManager.manage.checkIfUnlocked(recipeEntries[j].itemId))
				{
					PediaEntryButton component = Object.Instantiate(pediaEntryPrefab, pediaGridSpace).GetComponent<PediaEntryButton>();
					component.setUpButton(recipeEntries[j], num);
					showingButtons.Add(component);
					num++;
				}
			}
			for (int k = 0; k < showingButtons.Count; k++)
			{
				if (Inventory.inv.allItems[showingButtons[k].getEntryNumber()].craftable.catagory != 0 || Inventory.inv.allItems[showingButtons[k].getEntryNumber()].craftable.catagory != Recipe.CraftingCatagory.All)
				{
					showingButtons[k].transform.SetSiblingIndex((int)(Inventory.inv.allItems[showingButtons[k].getEntryNumber()].craftable.catagory - 2 + Inventory.inv.allItems[showingButtons[k].getEntryNumber()].craftable.tierLevel));
				}
			}
			break;
		}
		}
		sortPediaButtons(type);
	}

	public void sortPediaButtons(PediaEntryType type)
	{
		if (type == PediaEntryType.Fish || type == PediaEntryType.Bug || type == PediaEntryType.Relic || type == PediaEntryType.Critter)
		{
			showingButtons.Sort(sortButtons);
			for (int i = 0; i < showingButtons.Count; i++)
			{
				showingButtons[i].transform.SetSiblingIndex(i);
			}
		}
	}

	public int sortButtons(PediaEntryButton a, PediaEntryButton b)
	{
		SeasonAndTime mySeasonAndTimeForSort = a.getMySeasonAndTimeForSort();
		SeasonAndTime mySeasonAndTimeForSort2 = b.getMySeasonAndTimeForSort();
		if (mySeasonAndTimeForSort.myRarity < mySeasonAndTimeForSort2.myRarity)
		{
			return -1;
		}
		if (mySeasonAndTimeForSort.myRarity > mySeasonAndTimeForSort2.myRarity)
		{
			return 1;
		}
		if (Inventory.inv.allItems[a.getEntryNumber()].value < Inventory.inv.allItems[b.getEntryNumber()].value)
		{
			return -1;
		}
		if (Inventory.inv.allItems[a.getEntryNumber()].value > Inventory.inv.allItems[b.getEntryNumber()].value)
		{
			return 1;
		}
		return 0;
	}

	public void showEntryType(int typeId)
	{
		showEntryType((PediaEntryType)typeId);
	}

	public void openPedia()
	{
		pediaOpen = true;
		showEntryType(PediaEntryType.Fish);
		pediaWindow.SetActive(true);
		countPediaEntries();
	}

	public void closePedia()
	{
		pediaOpen = false;
		entryFullScreenWindow.SetActive(false);
		pediaWindow.SetActive(false);
	}

	public void countPediaEntries()
	{
		fishAmountText.text = getPediaEntryAmount(PediaEntryType.Fish, true) + " / " + getPediaEntryAmount(PediaEntryType.Fish, false);
		bugAmountText.text = getPediaEntryAmount(PediaEntryType.Bug, true) + " / " + getPediaEntryAmount(PediaEntryType.Bug, false);
		relicAmountText.text = getPediaEntryAmount(PediaEntryType.Relic, true) + " / " + getPediaEntryAmount(PediaEntryType.Relic, false);
		critterAmountText.text = getPediaEntryAmount(PediaEntryType.Critter, true) + " / " + getPediaEntryAmount(PediaEntryType.Critter, false);
	}

	public int getPediaEntryAmount(PediaEntryType typeToCount, bool countingUnlocked)
	{
		int num = 0;
		for (int i = 0; i < allEntries.Count; i++)
		{
			if (allEntries[i].entryType != (int)typeToCount)
			{
				continue;
			}
			if (countingUnlocked)
			{
				if (allEntries[i].amountCaught >= 1)
				{
					num++;
				}
			}
			else
			{
				num++;
			}
		}
		return num;
	}

	public void showEntryDetails(PediaEntry entryToShow)
	{
		entryFullScreenWindow.SetActive(true);
		entryFullScreenShown = true;
		if (entryToShow.entryType == 1 || entryToShow.entryType == 0 || entryToShow.entryType == 4 || entryToShow.entryType == 7)
		{
			heartBox.SetActive(false);
			bugAndFishDetailBox.SetActive(true);
			entryTitle.text = Inventory.inv.allItems[entryToShow.itemId].getInvItemName();
			entryDesc.text = "";
			caughtAmountText.text = entryToShow.amountCaught.ToString("n0");
			entryIcon.sprite = Inventory.inv.allItems[entryToShow.itemId].getSprite();
			if ((bool)Inventory.inv.allItems[entryToShow.itemId].fish)
			{
				locationText.text = Inventory.inv.allItems[entryToShow.itemId].fish.mySeason.getLocationName();
				Inventory.inv.allItems[entryToShow.itemId].fish.mySeason.getTime();
				Inventory.inv.allItems[entryToShow.itemId].fish.mySeason.getSeasonName();
				entryIcon.sprite = Inventory.inv.allItems[entryToShow.itemId].fish.pediaImage;
				entryIconSize.sizeDelta = new Vector2(320f, 160f);
				entryIconSize.localPosition = Vector2.zero;
			}
			else if ((bool)Inventory.inv.allItems[entryToShow.itemId].bug)
			{
				locationText.text = Inventory.inv.allItems[entryToShow.itemId].bug.mySeason.getLocationName();
				Inventory.inv.allItems[entryToShow.itemId].bug.mySeason.getTime();
				Inventory.inv.allItems[entryToShow.itemId].bug.mySeason.getSeasonName();
				entryIcon.sprite = Inventory.inv.allItems[entryToShow.itemId].bug.bugPediaPhoto;
				entryIconSize.sizeDelta = new Vector2(160f, 160f);
				entryIconSize.localPosition = Vector2.zero;
			}
			else if ((bool)Inventory.inv.allItems[entryToShow.itemId].underwaterCreature)
			{
				locationText.text = Inventory.inv.allItems[entryToShow.itemId].underwaterCreature.mySeason.getLocationName();
				Inventory.inv.allItems[entryToShow.itemId].underwaterCreature.mySeason.getTime();
				Inventory.inv.allItems[entryToShow.itemId].underwaterCreature.mySeason.getSeasonName();
				entryIconSize.sizeDelta = new Vector2(128f, 128f);
				entryIconSize.localPosition = Vector2.zero;
			}
			else
			{
				entryIconSize.sizeDelta = new Vector2(128f, 128f);
				entryIconSize.localPosition = Vector2.zero;
			}
		}
		else if (entryToShow.entryType != 6)
		{
			entryTitle.text = NPCManager.manage.NPCDetails[entryToShow.itemId].NPCName;
			entryDesc.text = "";
			heartBox.SetActive(true);
			bugAndFishDetailBox.SetActive(false);
			entryIconSize.sizeDelta = new Vector2(128f, 128f);
			entryIconSize.localPosition = Vector2.zero;
			entryIcon.sprite = NPCManager.manage.NPCDetails[entryToShow.itemId].npcSprite;
			for (int i = 0; i < hearts.Length; i++)
			{
				hearts[i].updateHealth(NPCManager.manage.npcStatus[entryToShow.itemId].relationshipLevel);
			}
		}
	}

	public void closeEntryDetails()
	{
		entryFullScreenWindow.SetActive(false);
		entryFullScreenShown = false;
	}

	public void createPediaEntries()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].fish)
			{
				allEntries.Add(new PediaEntry(i, PediaEntryType.Fish));
			}
			else if ((bool)Inventory.inv.allItems[i].bug)
			{
				allEntries.Add(new PediaEntry(i, PediaEntryType.Bug));
			}
			else if ((bool)Inventory.inv.allItems[i].relic)
			{
				allEntries.Add(new PediaEntry(i, PediaEntryType.Relic));
			}
			else if ((bool)Inventory.inv.allItems[i].underwaterCreature)
			{
				allEntries.Add(new PediaEntry(i, PediaEntryType.Critter));
			}
		}
		for (int j = 0; j < NPCManager.manage.NPCDetails.Length; j++)
		{
			villagerEntries.Add(new PediaEntry(j, PediaEntryType.Villager));
		}
		for (int k = 0; k < Inventory.inv.allItems.Length; k++)
		{
			if ((bool)Inventory.inv.allItems[k].craftable)
			{
				recipeEntries.Add(new PediaEntry(k, PediaEntryType.Recipe));
			}
		}
	}

	public void addCaughtToList(int itemId)
	{
		for (int i = 0; i < allEntries.Count; i++)
		{
			if (allEntries[i].itemId != itemId)
			{
				continue;
			}
			if (allEntries[i].amountCaught == 0)
			{
				if ((bool)Inventory.inv.allItems[allEntries[i].itemId].bug)
				{
					DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.AddNewBugToPedia);
				}
				else if ((bool)Inventory.inv.allItems[allEntries[i].itemId].fish)
				{
					DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.AddNewFishToPedia);
				}
				else
				{
					bool flag = (bool)Inventory.inv.allItems[allEntries[i].itemId].underwaterCreature;
				}
			}
			allEntries[i].amountCaught++;
		}
	}

	public bool isInPedia(int itemId)
	{
		for (int i = 0; i < allEntries.Count; i++)
		{
			if (allEntries[i].itemId == itemId && allEntries[i].amountCaught > 0)
			{
				return true;
			}
		}
		return false;
	}

	public int getUndiscoveredFish()
	{
		int num = -1;
		num = searchTableForUndiscoveredAnimal(AnimalManager.manage.northernOceanFish);
		if (num != -1)
		{
			return num;
		}
		num = searchTableForUndiscoveredAnimal(AnimalManager.manage.southernOceanFish);
		if (num != -1)
		{
			return num;
		}
		num = searchTableForUndiscoveredAnimal(AnimalManager.manage.riverFish);
		if (num != -1)
		{
			return num;
		}
		num = searchTableForUndiscoveredAnimal(AnimalManager.manage.billabongFish);
		if (num != -1)
		{
			return num;
		}
		num = searchTableForUndiscoveredAnimal(AnimalManager.manage.mangroveFish);
		int num2 = -1;
		return num;
	}

	public int getUndiscoveredBug()
	{
		int num = -1;
		num = searchTableForUndiscoveredAnimal(AnimalManager.manage.bushlandBugs);
		if (num != -1)
		{
			return num;
		}
		num = searchTableForUndiscoveredAnimal(AnimalManager.manage.desertBugs);
		if (num != -1)
		{
			return num;
		}
		num = searchTableForUndiscoveredAnimal(AnimalManager.manage.pineLandBugs);
		if (num != -1)
		{
			return num;
		}
		num = searchTableForUndiscoveredAnimal(AnimalManager.manage.plainsBugs);
		if (num != -1)
		{
			return num;
		}
		num = searchTableForUndiscoveredAnimal(AnimalManager.manage.topicalBugs);
		int num2 = -1;
		return num;
	}

	private int searchTableForUndiscoveredAnimal(InventoryLootTableTimeWeatherMaster table)
	{
		return table.getSomethingUndiscovered();
	}
}
