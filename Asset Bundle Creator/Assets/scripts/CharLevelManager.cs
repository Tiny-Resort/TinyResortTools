using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharLevelManager : MonoBehaviour
{
	public enum SkillTypes
	{
		Farming = 0,
		Foraging = 1,
		Mining = 2,
		Fishing = 3,
		BugCatching = 4,
		Hunting = 5
	}

	public static CharLevelManager manage;

	public GameObject unlockTierPrefab;

	public GameObject itemTallyPrefab;

	public GameObject unlockWindow;

	public GameObject levelUpwindow;

	public GameObject nextArrow;

	public GameObject confirmWindow;

	public FillRecipeSlot confirmRecipeSlot;

	public Transform tierParent;

	public Text bluePrintAmountText;

	public InventoryItem bluePrintItem;

	public List<RecipeToUnlock> recipes = new List<RecipeToUnlock>();

	public bool unlockWindowOpen;

	public bool levelUpWindowOpen;

	public List<RecipeUnlockTier> unlockTiersShowing = new List<RecipeUnlockTier>();

	private Recipe.CraftingCatagory showingCatagory = Recipe.CraftingCatagory.Tools;

	public InventoryItem[] recipesUnlockedFromBegining;

	public int[] todaysXp;

	public int[] currentXp;

	public int[] currentLevels;

	public SkillBox[] skillBoxes;

	public Transform[] pickupBoxes;

	public ASound soundToMakeWhenPickupAppears;

	public Transform moneyEarntBox;

	public TextMeshProUGUI moneyEarntText;

	public int[] beforeLevel;

	private List<GameObject> pickupTallyObjects = new List<GameObject>();

	private WaitForSeconds childWait = new WaitForSeconds(0.1f);

	private int interestedIn = -1;

	private List<EndOFDayItem> itemTally = new List<EndOFDayItem>();

	public int todaysMoneyTotal;

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].craftable)
			{
				recipes.Add(new RecipeToUnlock(i));
			}
		}
		todaysXp = new int[Enum.GetNames(typeof(SkillTypes)).Length];
		currentXp = new int[Enum.GetNames(typeof(SkillTypes)).Length];
		currentLevels = new int[Enum.GetNames(typeof(SkillTypes)).Length];
		recipesAlwaysUnlocked();
	}

	public bool checkIfIsInStartingRecipes(int itemId)
	{
		InventoryItem[] array = recipesUnlockedFromBegining;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == Inventory.inv.allItems[itemId])
			{
				return true;
			}
		}
		return false;
	}

	public void recipesAlwaysUnlocked()
	{
		InventoryItem[] array = recipesUnlockedFromBegining;
		foreach (InventoryItem invItem in array)
		{
			unlockRecipe(invItem);
		}
		for (int j = 0; j < recipes.Count; j++)
		{
			if ((Inventory.inv.allItems[recipes[j].recipeId].craftable.workPlaceConditions == CraftingManager.CraftingMenuType.CraftingShop && Inventory.inv.allItems[recipes[j].recipeId].craftable.crafterLevelLearnt == 0) || (Inventory.inv.allItems[recipes[j].recipeId].craftable.workPlaceConditions == CraftingManager.CraftingMenuType.TrapperShop && Inventory.inv.allItems[recipes[j].recipeId].craftable.crafterLevelLearnt == 0))
			{
				unlockRecipe(Inventory.inv.allItems[recipes[j].recipeId]);
			}
		}
	}

	public void checkIfLevelUpAndNeedToSendLetter()
	{
		for (int i = 1; i < LicenceManager.manage.allLicences.Length; i++)
		{
			if (LicenceManager.manage.allLicences[i].getCurrentMaxLevel() > beforeLevel[i])
			{
				MailManager.manage.sendLicenceUnlockMail(i);
			}
		}
	}

	public void getLicenceBeforeLevel()
	{
		beforeLevel = new int[LicenceManager.manage.allLicences.Length];
		for (int i = 1; i < LicenceManager.manage.allLicences.Length; i++)
		{
			beforeLevel[i] = LicenceManager.manage.allLicences[i].getCurrentMaxLevel();
		}
	}

	public IEnumerator openLevelUpWindow()
	{
		levelUpWindowOpen = true;
		nextArrow.gameObject.SetActive(false);
		levelUpwindow.gameObject.SetActive(true);
		moneyEarntBox.gameObject.SetActive(false);
		getLicenceBeforeLevel();
		for (int j = 0; j < skillBoxes.Length; j++)
		{
			skillBoxes[j].gameObject.SetActive(false);
		}
		yield return new WaitForSeconds(2f);
		for (int k = 0; k < pickupBoxes.Length; k++)
		{
			pickupBoxes[k].gameObject.SetActive(false);
			for (int l = 0; l < itemTally.Count; l++)
			{
				if (itemTally[l].pickUpType == k)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(itemTallyPrefab, pickupBoxes[k]);
					gameObject.GetComponent<EndOfDayTally>().setUp(itemTally[l].id, itemTally[l].currentTotal);
					pickupTallyObjects.Add(gameObject);
					gameObject.SetActive(false);
				}
			}
		}
		for (int i = skillBoxes.Length - 1; i >= 0; i--)
		{
			if (todaysXp[i] > 0)
			{
				if (pickupBoxes[i].childCount != 0)
				{
					pickupBoxes[i].gameObject.SetActive(true);
					float num = Mathf.Ceil((float)pickupBoxes[i].childCount / 7f) * 35f + 20f;
					skillBoxes[i].GetComponent<RectTransform>().sizeDelta = new Vector2(700f, 70f + num);
				}
				else
				{
					skillBoxes[i].GetComponent<RectTransform>().sizeDelta = new Vector2(700f, 70f);
				}
				skillBoxes[i].setToCurrent(i, currentXp[i]);
				skillBoxes[i].gameObject.SetActive(true);
				yield return StartCoroutine(boxChildrenAppear(pickupBoxes[i]));
				float timer = 0.9f;
				while (timer > 0f)
				{
					timer = ((!InputMaster.input.UISelectHeld() && !InputMaster.input.UICancelHeld()) ? (timer - Time.deltaTime) : (timer - Time.deltaTime * 2f));
					yield return null;
				}
				if (todaysXp[i] > 0)
				{
					yield return StartCoroutine(fillSkillBoxBar(i));
				}
				yield return new WaitForSeconds(0.25f);
			}
		}
		moneyEarntText.text = todaysMoneyTotal.ToString("n0");
		moneyEarntBox.gameObject.SetActive(true);
		Transform transform = moneyEarntBox.Find("SoldPickups");
		for (int m = 0; m < itemTally.Count; m++)
		{
			if (itemTally[m].pickUpType == pickupBoxes.Length)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(itemTallyPrefab, transform);
				gameObject2.GetComponent<EndOfDayTally>().setUp(itemTally[m].id, itemTally[m].currentTotal);
				pickupTallyObjects.Add(gameObject2);
				gameObject2.SetActive(false);
			}
		}
		if (transform.childCount != 0)
		{
			transform.gameObject.SetActive(true);
			float num2 = Mathf.Ceil((float)transform.childCount / 7f) * 35f + 20f;
			moneyEarntBox.GetComponent<RectTransform>().sizeDelta = new Vector2(700f, 100f + num2);
			yield return StartCoroutine(boxChildrenAppear(transform));
		}
		else
		{
			moneyEarntBox.GetComponent<RectTransform>().sizeDelta = new Vector2(700f, 100f);
		}
		todaysMoneyTotal = 0;
		while (!allBoxesComplete())
		{
			yield return null;
		}
		GiftedItemWindow.gifted.openWindowAndGiveItems();
		while (GiftedItemWindow.gifted.windowOpen)
		{
			yield return null;
		}
		bool ready = false;
		nextArrow.gameObject.SetActive(true);
		while (!ready)
		{
			if (InputMaster.input.UISelect())
			{
				ready = true;
				SoundManager.manage.play2DSound(ConversationManager.manage.nextTextSound);
			}
			yield return null;
		}
		for (int n = 0; n < pickupTallyObjects.Count; n++)
		{
			UnityEngine.Object.Destroy(pickupTallyObjects[n]);
		}
		checkIfLevelUpAndNeedToSendLetter();
		pickupTallyObjects.Clear();
		itemTally.Clear();
		nextArrow.gameObject.SetActive(false);
		levelUpwindow.gameObject.SetActive(false);
		levelUpWindowOpen = false;
		SaveLoad.saveOrLoad.loadingScreen.appear("saving", true);
	}

	private IEnumerator boxChildrenAppear(Transform parent)
	{
		bool skipFrame = false;
		for (int i = 0; i < parent.childCount; i++)
		{
			if (InputMaster.input.UISelectHeld())
			{
				if (!skipFrame)
				{
					yield return childWait;
				}
				skipFrame = !skipFrame;
			}
			else
			{
				skipFrame = false;
				yield return childWait;
			}
			if (!skipFrame)
			{
				SoundManager.manage.play2DSound(soundToMakeWhenPickupAppears);
			}
			parent.GetChild(i).gameObject.SetActive(true);
		}
	}

	public bool allBoxesComplete()
	{
		for (int i = 0; i < skillBoxes.Length; i++)
		{
			if (!skillBoxes[i].completed)
			{
				return false;
			}
		}
		return true;
	}

	public IEnumerator fillSkillBoxBar(int i)
	{
		skillBoxes[i].completed = false;
		yield return StartCoroutine(skillBoxes[i].fillProgressBar(i, currentXp[i], clampXPForLevel(i, currentXp[i] + todaysXp[i])));
		currentXp[i] += todaysXp[i];
		todaysXp[i] = 0;
		while (checkIfLeveledUp(i))
		{
			todaysXp[i] = currentXp[i] - getLevelRequiredXP(i);
			currentXp[i] = 0;
			currentLevels[i]++;
			yield return StartCoroutine(skillBoxes[i].levelUp(currentLevels[i]));
			giveRecipesUnlockedOnLevelUp(i, currentLevels[i]);
			yield return StartCoroutine(skillBoxes[i].fillProgressBar(i, currentXp[i], clampXPForLevel(i, currentXp[i] + todaysXp[i])));
			currentXp[i] += todaysXp[i];
			todaysXp[i] = 0;
		}
		yield return StartCoroutine(skillBoxes[i].fillProgressBar(i, currentXp[i], clampXPForLevel(i, currentXp[i] + todaysXp[i])));
		skillBoxes[i].completed = true;
	}

	public int getLevelRequiredXP(int skillId)
	{
		return currentLevels[skillId] * 5 + 25;
	}

	public int clampXPForLevel(int skillId, int xP)
	{
		return Mathf.Clamp(xP, 0, getLevelRequiredXP(skillId));
	}

	public bool checkIfLeveledUp(int skillId)
	{
		if (currentXp[skillId] >= getLevelRequiredXP(skillId))
		{
			return true;
		}
		return false;
	}

	public void giveRecipesUnlockedOnLevelUp(int skillType, int level)
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].craftable && Inventory.inv.allItems[i].craftable.meetsRequirement(skillType, level))
			{
				GiftedItemWindow.gifted.addRecipeToUnlock(Inventory.inv.getInvItemId(Inventory.inv.allItems[i]));
			}
		}
	}

	public bool isCraftsmanRecipeUnlockedThisLevel()
	{
		bool result = false;
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].craftable && Inventory.inv.allItems[i].craftable.workPlaceConditions == CraftingManager.CraftingMenuType.CraftingShop && (bool)Inventory.inv.allItems[i].craftable && Inventory.inv.allItems[i].craftable.crafterLevelLearnt <= CraftsmanManager.manage.currentLevel && !checkIfUnlocked(i))
			{
				unlockRecipe(Inventory.inv.allItems[i]);
				result = true;
			}
		}
		return result;
	}

	public void openUnlockScreen()
	{
		unlockWindowOpen = true;
		unlockWindow.SetActive(true);
		showUnlocksForType(showingCatagory);
		confirmWindow.SetActive(false);
		Inventory.inv.checkIfWindowIsNeeded();
	}

	public void closeUnlockScreen()
	{
		unlockWindowOpen = false;
		unlockWindow.SetActive(false);
		confirmWindow.SetActive(false);
	}

	public void showUnlocksForType(Recipe.CraftingCatagory skillToShow, bool resetToTop = false)
	{
		if (resetToTop)
		{
			Inventory.inv.activeScrollBar.resetToTop();
		}
		showingCatagory = skillToShow;
		bluePrintAmountText.text = Inventory.inv.getAmountOfItemInAllSlots(Inventory.inv.getInvItemId(bluePrintItem)).ToString() ?? "";
		foreach (RecipeUnlockTier item in unlockTiersShowing)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		unlockTiersShowing.Clear();
		bool flag = true;
		for (int i = 0; i < 4; i++)
		{
			unlockTiersShowing.Add(UnityEngine.Object.Instantiate(unlockTierPrefab, tierParent).GetComponent<RecipeUnlockTier>());
			unlockTiersShowing[unlockTiersShowing.Count - 1].populateTier(skillToShow, i);
			if (!flag)
			{
				unlockTiersShowing[unlockTiersShowing.Count - 1].lockTeir();
			}
			else
			{
				flag = unlockTiersShowing[unlockTiersShowing.Count - 1].checkIfTeirIsComplete();
			}
		}
	}

	public void refreshCurrentTier()
	{
		bluePrintAmountText.text = Inventory.inv.getAmountOfItemInAllSlots(Inventory.inv.getInvItemId(bluePrintItem)).ToString() ?? "";
		bool flag = true;
		for (int i = 0; i < 4; i++)
		{
			unlockTiersShowing[i].updateTier();
			if (!flag)
			{
				unlockTiersShowing[i].lockTeir();
				continue;
			}
			unlockTiersShowing[i].unLockTier();
			flag = unlockTiersShowing[i].checkIfTeirIsComplete();
		}
	}

	public void clickOnRecipe(int recipeId)
	{
		interestedIn = recipeId;
		confirmRecipeSlot.fillRecipeSlotForCraftUnlock(recipeId, true);
		confirmWindow.gameObject.SetActive(true);
	}

	public void confirmButton()
	{
		for (int i = 0; i < recipes.Count; i++)
		{
			if (recipes[i].recipeId == interestedIn)
			{
				GiftedItemWindow.gifted.addRecipeToUnlock(recipes[i].recipeId);
				GiftedItemWindow.gifted.openWindowAndGiveItems(0f);
				break;
			}
		}
		Inventory.inv.removeAmountOfItem(Inventory.inv.getInvItemId(bluePrintItem), 1);
		Inventory.inv.equipNewSelectedSlot();
		refreshCurrentTier();
		confirmWindow.SetActive(false);
		interestedIn = -1;
	}

	public void unlockRecipe(InventoryItem invItem)
	{
		for (int i = 0; i < recipes.Count; i++)
		{
			if (recipes[i].recipeId == Inventory.inv.getInvItemId(invItem))
			{
				recipes[i].unlockRecipe();
				break;
			}
		}
	}

	public bool checkIfUnlocked(int checkId)
	{
		for (int i = 0; i < recipes.Count; i++)
		{
			if (recipes[i].recipeId == checkId)
			{
				return recipes[i].isUnlocked();
			}
		}
		return true;
	}

	public void addXp(SkillTypes skillToAddTo, int xpAmount)
	{
		todaysXp[(int)skillToAddTo] += xpAmount * (1 + StatusManager.manage.getBuffLevel(StatusManager.BuffType.xPBuff));
	}

	public int getLevelNo(SkillTypes skillToCheckLevel)
	{
		return currentLevels[(int)skillToCheckLevel];
	}

	public bool checkIfHasBluePrint()
	{
		if (Inventory.inv.getAmountOfItemInAllSlots(Inventory.inv.getInvItemId(bluePrintItem)) > 0)
		{
			return true;
		}
		return false;
	}

	public float getStaminaCost(int skillId)
	{
		if (skillId < 0)
		{
			return Mathf.Clamp(0.149f, 0.01f, 0.15f);
		}
		switch (skillId)
		{
		case 3:
			return Mathf.Clamp(0.85f - (float)currentLevels[skillId] * 0.01f, 0.1f, 0.85f);
		case 4:
			return Mathf.Clamp(0.85f - (float)currentLevels[skillId] * 0.01f, 0.1f, 0.85f);
		case 5:
			return Mathf.Clamp(0.6f - (float)currentLevels[skillId] * 0.01f, 0.1f, 0.6f);
		default:
			return Mathf.Clamp(0.95f - (float)currentLevels[skillId] * 0.01f, 0.1f, 0.95f);
		}
	}

	public void addToDayTally(int itemId, int amount, int skillType)
	{
		if (Inventory.inv.allItems[itemId].checkIfStackable())
		{
			for (int i = 0; i < itemTally.Count; i++)
			{
				if (itemTally[i].id == itemId)
				{
					itemTally[i].currentTotal += amount;
					return;
				}
			}
		}
		itemTally.Add(new EndOFDayItem(itemId, amount, skillType));
	}
}
