using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheatMenuButton : MonoBehaviour
{
	public Image icon;

	public TextMeshProUGUI text;

	public int myItemNo;

	public void setUpButton(int itemNo)
	{
		myItemNo = itemNo;
		text.text = Inventory.inv.allItems[itemNo].getInvItemName();
		icon.sprite = Inventory.inv.allItems[itemNo].getSprite();
	}

	public void pressButton()
	{
		if (Inventory.inv.allItems[myItemNo].hasFuel)
		{
			if (!Inventory.inv.addItemToInventory(myItemNo, Inventory.inv.allItems[myItemNo].fuelMax))
			{
				SoundManager.manage.play2DSound(SoundManager.manage.pocketsFull);
			}
		}
		else if (Inventory.inv.allItems[myItemNo].isDeed || !Inventory.inv.allItems[myItemNo].checkIfStackable())
		{
			if (!Inventory.inv.addItemToInventory(myItemNo, 1))
			{
				SoundManager.manage.play2DSound(SoundManager.manage.pocketsFull);
			}
		}
		else if (!Inventory.inv.addItemToInventory(myItemNo, CheatScript.cheat.amountToGive))
		{
			SoundManager.manage.play2DSound(SoundManager.manage.pocketsFull);
		}
	}
}
