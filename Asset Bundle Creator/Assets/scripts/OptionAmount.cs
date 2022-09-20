using System.Collections;
using TMPro;
using UnityEngine;

public class OptionAmount : MonoBehaviour
{
	public TextMeshProUGUI selectedAmountText;

	public TextMeshProUGUI moneyAmount;

	public Transform upButton;

	public Transform downButton;

	private int selectedAmount = 1;

	private int itemIdBeingShown;

	public void selectedAmountUp()
	{
		if (selectedAmount == 99)
		{
			selectedAmount = 1;
		}
		else
		{
			selectedAmount = Mathf.Clamp(selectedAmount + 1, 1, 99);
			StartCoroutine(holdUpOrDown(1));
		}
		selectedAmountText.text = selectedAmount.ToString() ?? "";
		moneyAmount.text = "<sprite=11>" + (selectedAmount * (Inventory.inv.allItems[itemIdBeingShown].value * 2)).ToString("n0");
		SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
	}

	public void selectedAmountDown()
	{
		if (selectedAmount == 1)
		{
			selectedAmount = 99;
		}
		else
		{
			selectedAmount = Mathf.Clamp(selectedAmount - 1, 1, 99);
			StartCoroutine(holdUpOrDown(-1));
		}
		selectedAmountText.text = selectedAmount.ToString() ?? "";
		moneyAmount.text = "<sprite=11>" + (selectedAmount * (Inventory.inv.allItems[itemIdBeingShown].value * 2)).ToString("n0");
		SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
	}

	public int getSelectedAmount()
	{
		return selectedAmount;
	}

	public void fillItemDetails(int itemId)
	{
		itemIdBeingShown = itemId;
		selectedAmount = 1;
		selectedAmountText.text = selectedAmount.ToString() ?? "";
		moneyAmount.text = "<sprite=11>" + (selectedAmount * (Inventory.inv.allItems[itemIdBeingShown].value * 2)).ToString("n0");
	}

	private IEnumerator holdUpOrDown(int dif)
	{
		float increaseCheck = 0f;
		float holdTimer = 0f;
		while (InputMaster.input.UISelectHeld())
		{
			if (increaseCheck < 0.15f - holdTimer)
			{
				increaseCheck += Time.deltaTime;
			}
			else
			{
				increaseCheck = 0f;
				if (selectedAmount + dif == Mathf.Clamp(selectedAmount + dif, 1, 99))
				{
					SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
				}
				selectedAmount = Mathf.Clamp(selectedAmount + dif, 1, 99);
				selectedAmountText.text = selectedAmount.ToString() ?? "";
				moneyAmount.text = "<sprite=11>" + (selectedAmount * (Inventory.inv.allItems[itemIdBeingShown].value * 2)).ToString("n0");
			}
			holdTimer = Mathf.Clamp(holdTimer + Time.deltaTime / 8f, 0f, 0.14f);
			yield return null;
		}
	}
}
