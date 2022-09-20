using System.IO;
using UnityEngine;

public class ItemExIn : MonoBehaviour
{
	public bool createSheet;

	private void Update()
	{
		if (createSheet)
		{
			createSheet = false;
			ExportFile();
		}
	}

	private void ExportFile()
	{
		StreamWriter streamWriter = new StreamWriter(Application.persistentDataPath + "ItemData.csv");
		streamWriter.WriteLine("ItemName,Sell Price,Buy Price,Price to Craft");
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			int num = 0;
			if ((bool)Inventory.inv.allItems[i].craftable)
			{
				num = Inventory.inv.allItems[i].value * 2;
				for (int j = 0; j < Inventory.inv.allItems[i].craftable.itemsInRecipe.Length; j++)
				{
					num += Inventory.inv.allItems[i].craftable.itemsInRecipe[j].value * Inventory.inv.allItems[i].craftable.stackOfItemsInRecipe[j];
				}
				num /= Inventory.inv.allItems[i].craftable.recipeGiveThisAmount;
			}
			streamWriter.WriteLine(Inventory.inv.allItems[i].itemName + "," + Inventory.inv.allItems[i].value + "," + Inventory.inv.allItems[i].value * 2 + "," + num);
		}
	}
}
