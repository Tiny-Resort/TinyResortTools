using UnityEngine;

public class SortRecipesButton : MonoBehaviour
{
	public Recipe.CraftingCatagory catagory;

	public bool sortCraftUnlocks;

	public void press()
	{
		if (!sortCraftUnlocks)
		{
			CraftingManager.manage.changeListSort(catagory);
		}
		else
		{
			CharLevelManager.manage.showUnlocksForType(catagory, true);
		}
	}
}
