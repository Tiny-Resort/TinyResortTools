using UnityEngine;

public class UnlockRecipeButton : MonoBehaviour
{
	private int recipeNo;

	public bool tierLocked;

	private void Start()
	{
		recipeNo = GetComponent<InvButton>().craftRecipeNumber;
	}

	public void clickToUnlock()
	{
		if (!tierLocked && !CharLevelManager.manage.checkIfUnlocked(recipeNo) && CharLevelManager.manage.checkIfHasBluePrint())
		{
			CharLevelManager.manage.clickOnRecipe(recipeNo);
		}
	}
}
