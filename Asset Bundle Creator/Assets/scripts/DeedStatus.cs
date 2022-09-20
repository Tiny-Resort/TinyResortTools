using System;
using UnityEngine;

[Serializable]
public class DeedStatus
{
	public bool unlocked;

	public int[] requiredItems;

	public int[] requiredAmount;

	public int[] givenAmounts;

	public DeedStatus()
	{
	}

	public DeedStatus(int[] rquiredItemId, int[] requiredItemsAmount)
	{
		requiredItems = rquiredItemId;
		requiredAmount = requiredItemsAmount;
		givenAmounts = new int[requiredItems.Length];
	}

	public bool showInBuyList(int deedId)
	{
		if (deedId == 129)
		{
			Debug.Log("Mine deed unlocked:" + unlocked);
			Debug.Log("Mine deed in catalogue:" + CatalogueManager.manage.collectedItem[deedId]);
		}
		if (unlocked && !CatalogueManager.manage.collectedItem[deedId])
		{
			return true;
		}
		return false;
	}

	public void fillRequireditems(int[] rquiredItemId, int[] requiredItemsAmount)
	{
		requiredItems = rquiredItemId;
		requiredAmount = requiredItemsAmount;
		if (givenAmounts.Length >= requiredItems.Length)
		{
			return;
		}
		int[] array = givenAmounts;
		givenAmounts = new int[requiredItems.Length];
		for (int i = 0; i < givenAmounts.Length; i++)
		{
			if (i < array.Length)
			{
				givenAmounts[i] = array[i];
			}
			else
			{
				givenAmounts[i] = 0;
			}
		}
	}
}
