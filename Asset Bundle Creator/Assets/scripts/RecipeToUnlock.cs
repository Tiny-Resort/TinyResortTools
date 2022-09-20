using System;

[Serializable]
public class RecipeToUnlock
{
	public bool unlocked;

	public int recipeId;

	public RecipeToUnlock()
	{
	}

	public RecipeToUnlock(int newRecipeId)
	{
		recipeId = newRecipeId;
	}

	public bool unlockedThroughOtherWay()
	{
		if (Inventory.inv.allItems[recipeId].craftable.learnThroughQuest || Inventory.inv.allItems[recipeId].craftable.learnThroughLevels || Inventory.inv.allItems[recipeId].craftable.learnThroughLicence)
		{
			return true;
		}
		return false;
	}

	public void unlockRecipe()
	{
		unlocked = true;
	}

	public bool isUnlocked()
	{
		return unlocked;
	}

	public Recipe.CraftingCatagory getCatagory()
	{
		return Inventory.inv.allItems[recipeId].craftable.catagory;
	}

	public int unlockTier()
	{
		return Inventory.inv.allItems[recipeId].craftable.tierLevel;
	}
}
