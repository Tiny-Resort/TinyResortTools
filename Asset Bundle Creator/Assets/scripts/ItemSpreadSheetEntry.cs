using UnityEngine;
using UnityEngine.UI;

public class ItemSpreadSheetEntry : MonoBehaviour
{
	public Image iconImage;

	public Text nameText;

	public Text sellPriceText;

	public Text buyPriceText;

	public Text craftableText;

	public Text craftableCostText;

	public InventoryItem showingItem;

	public void fillDetails(int itemId)
	{
		showingItem = Inventory.inv.allItems[itemId];
		iconImage.sprite = showingItem.getSprite();
		nameText.text = showingItem.itemName;
		sellPriceText.text = "D " + showingItem.value;
		buyPriceText.text = "D " + showingItem.value * 2;
		if ((bool)showingItem.craftable)
		{
			craftableText.text = "<color=green>True</color>";
			int num = showingItem.value * 2;
			for (int i = 0; i < showingItem.craftable.itemsInRecipe.Length; i++)
			{
				num += showingItem.craftable.itemsInRecipe[i].value * showingItem.craftable.stackOfItemsInRecipe[i];
			}
			craftableCostText.text = "D " + num / showingItem.craftable.recipeGiveThisAmount;
		}
		else
		{
			craftableText.text = "<color=red>False</color>";
			craftableCostText.text = "N/A";
		}
	}
}
