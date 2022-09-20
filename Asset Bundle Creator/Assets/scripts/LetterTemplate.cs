using UnityEngine;

[CreateAssetMenu(fileName = "New_Letter", menuName = "LetterTemplate")]
public class LetterTemplate : ScriptableObject
{
	public enum rewardType
	{
		Furniture = 0,
		Clothing = 1,
		WallPaperOrFlooring = 2
	}

	public InventoryItem gift;

	public InventoryItemLootTable giftFromTable;

	public bool useRandomFromType;

	public rewardType randomFromType;

	public int stackOfGift = 1;

	[TextArea(10, 10)]
	public string letterText;

	public string signOff = "From,";

	public int getRandomGiftFromType()
	{
		bool flag = false;
		int num = Random.Range(0, Inventory.inv.allItems.Length);
		if (randomFromType == rewardType.Furniture)
		{
			while (!flag)
			{
				num = Random.Range(0, Inventory.inv.allItems.Length);
				if (Inventory.inv.allItems[num].isFurniture)
				{
					flag = true;
				}
			}
		}
		else if (randomFromType == rewardType.Clothing)
		{
			while (!flag)
			{
				num = Random.Range(0, Inventory.inv.allItems.Length);
				if ((bool)Inventory.inv.allItems[num].equipable && Inventory.inv.allItems[num].equipable.cloths)
				{
					flag = true;
				}
			}
		}
		else if (randomFromType == rewardType.WallPaperOrFlooring)
		{
			while (!flag)
			{
				num = Random.Range(0, Inventory.inv.allItems.Length);
				if ((bool)Inventory.inv.allItems[num].equipable && (Inventory.inv.allItems[num].equipable.flooring || Inventory.inv.allItems[num].equipable.wallpaper))
				{
					flag = true;
				}
			}
		}
		return num;
	}
}
