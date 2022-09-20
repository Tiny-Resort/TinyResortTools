using UnityEngine;

public class InventoryItemLootTable : MonoBehaviour
{
	public InventoryItem[] itemsInLootTable;

	public float[] rarityPercentage;

	public float totalToShowInEditor;

	public InventoryItem getRandomDropFromTable(MapRand generator = null)
	{
		if (itemsInLootTable.Length != rarityPercentage.Length)
		{
			GameObject obj = base.gameObject;
			Debug.LogError((((object)obj != null) ? obj.ToString() : null) + " Loot table rarity does not match item length - Was created at runtime to work. Check this out James");
			rarityPercentage = new float[itemsInLootTable.Length];
			for (int i = 0; i < rarityPercentage.Length; i++)
			{
				rarityPercentage[i] = 100f / (float)rarityPercentage.Length;
			}
		}
		float num = 0f;
		for (int j = 0; j < rarityPercentage.Length; j++)
		{
			num += rarityPercentage[j];
		}
		float num2 = ((generator == null) ? Random.Range(0f, num) : generator.Range(0f, num));
		float num3 = 0f;
		for (int k = 0; k < rarityPercentage.Length; k++)
		{
			num3 += rarityPercentage[k];
			if (num2 < num3)
			{
				return itemsInLootTable[k];
			}
		}
		return null;
	}

	public InventoryItem getRandomDropWithAddedLuck(int luckToAdd)
	{
		if (itemsInLootTable.Length != rarityPercentage.Length)
		{
			GameObject obj = base.gameObject;
			Debug.LogError((((object)obj != null) ? obj.ToString() : null) + " Loot table rarity does not match item length - Was created at runtime to work. Check this out James");
			rarityPercentage = new float[itemsInLootTable.Length];
			for (int i = 0; i < rarityPercentage.Length; i++)
			{
				rarityPercentage[i] = 100f / (float)rarityPercentage.Length;
			}
		}
		float num = 0f;
		for (int j = 0; j < rarityPercentage.Length; j++)
		{
			num += rarityPercentage[j];
		}
		float num2 = Random.Range(0f, num);
		float num3 = 0f;
		for (int k = 0; k < itemsInLootTable.Length; k++)
		{
			num3 += rarityPercentage[k];
			if (num2 < num3)
			{
				return itemsInLootTable[k];
			}
		}
		return null;
	}

	public float getTotal()
	{
		float num = 0f;
		for (int i = 0; i < rarityPercentage.Length; i++)
		{
			num += rarityPercentage[i];
		}
		return num;
	}

	public bool isInTable(InventoryItem item)
	{
		for (int i = 0; i < itemsInLootTable.Length; i++)
		{
			if (itemsInLootTable[i] == item)
			{
				return true;
			}
		}
		return false;
	}

	public int getSomethingUnDiscovered()
	{
		for (int i = 0; i < itemsInLootTable.Length; i++)
		{
			if (!PediaManager.manage.isInPedia(itemsInLootTable[i].getItemId()))
			{
				return itemsInLootTable[i].getItemId();
			}
		}
		return -1;
	}

	public void autoFillFromArray(InventoryItem[] array)
	{
		itemsInLootTable = array;
		rarityPercentage = new float[itemsInLootTable.Length];
		for (int i = 0; i < itemsInLootTable.Length; i++)
		{
			float num = 1f;
			if ((bool)array[i].fish)
			{
				num += (float)array[i].fish.mySeason.myRarity * (float)array[i].fish.mySeason.myRarity;
			}
			else if ((bool)array[i].bug)
			{
				num += (float)array[i].bug.mySeason.myRarity * (float)array[i].bug.mySeason.myRarity;
			}
			else if ((bool)array[i].underwaterCreature)
			{
				num += (float)array[i].underwaterCreature.mySeason.myRarity * (float)array[i].underwaterCreature.mySeason.myRarity;
			}
			else if ((bool)array[i].relic)
			{
				num += (float)array[i].relic.myseason.myRarity + (float)array[i].relic.myseason.myRarity + (float)array[i].value / 10000f;
				if (array[i].relic.myseason.myRarity == SeasonAndTime.rarity.Common)
				{
					num /= 1.5f;
				}
				else if (array[i].relic.myseason.myRarity == SeasonAndTime.rarity.Uncommon)
				{
					num /= 1.2f;
				}
				else if (array[i].relic.myseason.myRarity == SeasonAndTime.rarity.Rare)
				{
					num /= 1.1f;
				}
			}
			rarityPercentage[i] = 100 / itemsInLootTable.Length;
			rarityPercentage[i] /= num;
		}
	}
}
