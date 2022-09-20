using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CatalogueButton : MonoBehaviour
{
	private int showingItemId;

	public TextMeshProUGUI itemNameText;

	public TextMeshProUGUI itemPriceText;

	public Image itemIcon;

	public void setUpButton(int itemId)
	{
		showingItemId = itemId;
		itemIcon.sprite = Inventory.inv.allItems[itemId].getSprite();
		itemNameText.text = Inventory.inv.allItems[itemId].getInvItemName();
		itemPriceText.text = "<sprite=11>" + ((int)((float)Inventory.inv.allItems[itemId].value * 2.5f)).ToString("n0");
	}

	public void pressButton()
	{
		CatalogueManager.manage.showItemInfo(showingItemId);
	}

	public InventoryItem getShowingInvItem()
	{
		return Inventory.inv.allItems[showingItemId];
	}
}
