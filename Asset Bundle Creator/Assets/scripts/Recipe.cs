using UnityEngine;

public class Recipe : MonoBehaviour
{
	public enum CraftingCatagory
	{
		None = 0,
		All = 1,
		Tools = 2,
		Decorations = 3,
		Light = 4,
		Gardening = 5,
		House = 6,
		Charm = 7,
		Misc = 8
	}

	public enum SubCatagory
	{
		None = 0,
		Tools = 1,
		Weapon = 2,
		Path = 3,
		Fence = 4,
		Gate = 5,
		Bridge = 6,
		Decoration = 7
	}

	public InventoryItem[] itemsInRecipe;

	public int[] stackOfItemsInRecipe;

	public int recipeGiveThisAmount = 1;

	public bool buildOnce;

	public bool isDeed;

	public CraftingManager.CraftingMenuType workPlaceConditions;

	public CraftingCatagory catagory;

	public SubCatagory subCatagory;

	public int tierLevel;

	public bool learnThroughQuest;

	public Recipe[] altRecipes;

	[Header("Level Unlock -------")]
	public bool learnThroughLevels;

	public CharLevelManager.SkillTypes skill;

	public int levelSkillLearnt;

	[Header("Licence Unlock -------")]
	public bool learnThroughLicence;

	public LicenceManager.LicenceTypes licenceType;

	public int licenceLevelLearnt;

	[Header("Crafter Unlock Level-------")]
	public int crafterLevelLearnt;

	[Header("Crafter Unlock Level-------")]
	public DailyTaskGenerator.genericTaskType completeTaskOnCraft;

	public bool meetsRequirement(int skillNo, int levelNo)
	{
		if (learnThroughLevels && skillNo == (int)skill && levelNo == levelSkillLearnt)
		{
			return true;
		}
		return false;
	}

	public bool checkIfMeetsLicenceRequirement(LicenceManager.LicenceTypes checkLicenceType, int levelNo)
	{
		if (learnThroughLicence && checkLicenceType == licenceType && levelNo >= licenceLevelLearnt)
		{
			return true;
		}
		return false;
	}
}
