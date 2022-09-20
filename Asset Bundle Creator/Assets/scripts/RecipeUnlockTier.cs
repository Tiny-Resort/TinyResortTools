using System.Collections.Generic;
using UnityEngine;

public class RecipeUnlockTier : MonoBehaviour
{
	public GameObject recipeUnlockButton;

	public List<FillRecipeSlot> showingRecipes = new List<FillRecipeSlot>();

	public Transform buttonParent;

	public GameObject lockedIcon;

	public int tierNo;

	private Recipe.CraftingCatagory type;

	public void populateTier(Recipe.CraftingCatagory showingType, int showingTier)
	{
		tierNo = showingTier;
		type = showingType;
		for (int i = 0; i < CharLevelManager.manage.recipes.Count; i++)
		{
			if (CharLevelManager.manage.recipes[i].getCatagory() == showingType && CharLevelManager.manage.recipes[i].unlockTier() == showingTier && !CharLevelManager.manage.recipes[i].unlockedThroughOtherWay())
			{
				FillRecipeSlot component = Object.Instantiate(recipeUnlockButton, buttonParent).GetComponent<FillRecipeSlot>();
				component.fillRecipeSlotForCraftUnlock(CharLevelManager.manage.recipes[i].recipeId, CharLevelManager.manage.recipes[i].isUnlocked());
				component.GetComponent<InvButton>().craftRecipeNumber = CharLevelManager.manage.recipes[i].recipeId;
				showingRecipes.Add(component);
			}
		}
	}

	public bool checkIfTeirIsComplete()
	{
		for (int i = 0; i < CharLevelManager.manage.recipes.Count; i++)
		{
			if (CharLevelManager.manage.recipes[i].getCatagory() == type && CharLevelManager.manage.recipes[i].unlockTier() == tierNo && !CharLevelManager.manage.recipes[i].unlockedThroughOtherWay() && !CharLevelManager.manage.recipes[i].isUnlocked())
			{
				return false;
			}
		}
		return true;
	}

	public void lockTeir()
	{
		foreach (FillRecipeSlot showingRecipe in showingRecipes)
		{
			showingRecipe.GetComponent<UnlockRecipeButton>().tierLocked = true;
		}
		lockedIcon.SetActive(true);
	}

	public void unLockTier()
	{
		foreach (FillRecipeSlot showingRecipe in showingRecipes)
		{
			showingRecipe.GetComponent<UnlockRecipeButton>().tierLocked = false;
		}
		lockedIcon.SetActive(false);
	}

	public void updateTier()
	{
		foreach (FillRecipeSlot showingRecipe in showingRecipes)
		{
			showingRecipe.fillRecipeSlotForCraftUnlock(showingRecipe.GetComponent<InvButton>().craftRecipeNumber, CharLevelManager.manage.checkIfUnlocked(showingRecipe.GetComponent<InvButton>().craftRecipeNumber));
		}
	}
}
