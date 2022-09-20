using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjectGenerator : MonoBehaviour
{
	public static RandomObjectGenerator generate;

	private void Awake()
	{
		generate = this;
	}

	public InventoryItem getRandomShirtForGender(bool feminine)
	{
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].equipable && Inventory.inv.allItems[i].equipable.shirt)
			{
				list.Add(Inventory.inv.allItems[i]);
			}
		}
		if (!feminine)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			while (list[index].equipable.dress)
			{
				index = UnityEngine.Random.Range(0, list.Count);
			}
			return list[index];
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public int getRandomHair(bool feminine)
	{
		int[] array = new int[10] { 0, 1, 2, 3, 4, 8, 9, 12, 13, 15 };
		int num = UnityEngine.Random.Range(0, CharacterCreatorScript.create.allHairStyles.Length);
		bool flag = false;
		if (feminine)
		{
			while (!flag)
			{
				bool flag2 = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (num == array[i])
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					flag = true;
				}
				else
				{
					num = UnityEngine.Random.Range(0, CharacterCreatorScript.create.allHairStyles.Length);
				}
			}
		}
		else
		{
			while (!flag)
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (num == array[j])
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					num = UnityEngine.Random.Range(0, CharacterCreatorScript.create.allHairStyles.Length);
				}
			}
		}
		return num;
	}

	public InventoryItem getRandomPants(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (!Inventory.inv.allItems[i].isUniqueItem && (bool)Inventory.inv.allItems[i].equipable && Inventory.inv.allItems[i].equipable.cloths && Inventory.inv.allItems[i].equipable.pants)
			{
				list.Add(Inventory.inv.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomFaceItem(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (!Inventory.inv.allItems[i].isUniqueItem && (bool)Inventory.inv.allItems[i].equipable && Inventory.inv.allItems[i].equipable.cloths && Inventory.inv.allItems[i].equipable.face)
			{
				list.Add(Inventory.inv.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomHat(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (!Inventory.inv.allItems[i].isUniqueItem && (bool)Inventory.inv.allItems[i].equipable && Inventory.inv.allItems[i].equipable.cloths && Inventory.inv.allItems[i].equipable.hat)
			{
				list.Add(Inventory.inv.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomShoes(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (!Inventory.inv.allItems[i].isUniqueItem && (bool)Inventory.inv.allItems[i].equipable && Inventory.inv.allItems[i].equipable.cloths && Inventory.inv.allItems[i].equipable.shoes)
			{
				list.Add(Inventory.inv.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomShirtOrDressForShop(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (!Inventory.inv.allItems[i].isUniqueItem && (bool)Inventory.inv.allItems[i].equipable && Inventory.inv.allItems[i].equipable.cloths && (Inventory.inv.allItems[i].equipable.shirt || Inventory.inv.allItems[i].equipable.dress))
			{
				list.Add(Inventory.inv.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomFurniture(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (!Inventory.inv.allItems[i].isUniqueItem && Inventory.inv.allItems[i].isFurniture)
			{
				list.Add(Inventory.inv.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomFurnitureForShop(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (!Inventory.inv.allItems[i].isUniqueItem && Inventory.inv.allItems[i].isFurniture && (bool)Inventory.inv.allItems[i].placeable && !WorldManager.manageWorld.allObjectSettings[Inventory.inv.allItems[i].placeable.tileObjectId].canBePlacedOnTopOfFurniture)
			{
				list.Add(Inventory.inv.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public InventoryItem getRandomOnTopFurniture(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (!Inventory.inv.allItems[i].isUniqueItem && Inventory.inv.allItems[i].isFurniture && (bool)Inventory.inv.allItems[i].placeable && WorldManager.manageWorld.allObjectSettings[Inventory.inv.allItems[i].placeable.tileObjectId].canBePlacedOnTopOfFurniture)
			{
				list.Add(Inventory.inv.allItems[i]);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public int getRandomClothing(bool resetSeed = false)
	{
		resetTheRandomSeed(resetSeed);
		switch (UnityEngine.Random.Range(0, 5))
		{
		case 0:
			return getRandomFaceItem().getItemId();
		case 1:
			return getRandomHat().getItemId();
		case 2:
			return getRandomShirtOrDressForShop().getItemId();
		case 3:
			return getRandomPants().getItemId();
		case 4:
			return getRandomShoes().getItemId();
		default:
			return getRandomShirtOrDressForShop().getItemId();
		}
	}

	public void resetTheRandomSeed(bool reset)
	{
		if (reset)
		{
			UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
		}
	}
}
