using System;

[Serializable]
internal class RecipesUnlockedSave
{
	public int crafterLevel;

	public int crafterWorkingOnItemId = -1;

	public int currentPoints;

	public bool crafterCurrentlyWorking;

	public RecipeToUnlock[] recipesUnlocked;

	public void loadRecipes()
	{
		for (int i = 0; i < CharLevelManager.manage.recipes.Count; i++)
		{
			if (i < recipesUnlocked.Length)
			{
				CharLevelManager.manage.recipes[i] = recipesUnlocked[i];
			}
		}
	}
}
