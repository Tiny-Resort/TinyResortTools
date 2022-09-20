using System.Collections.Generic;
using UnityEngine;

public class RequestItemGenerator : MonoBehaviour
{
	public static RequestItemGenerator request;

	private List<InventoryItem> allMeat = new List<InventoryItem>();

	private List<InventoryItem> animalProduct = new List<InventoryItem>();

	private List<InventoryItem> allFruit = new List<InventoryItem>();

	public InventoryItem[] woodLogs;

	public InventoryItem[] woodPlanks;

	public InventoryItem[] oreBars;

	private void Awake()
	{
		request = this;
	}

	private void Start()
	{
		fillRandomTables();
	}

	private void fillRandomTables()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].consumeable)
			{
				if (Inventory.inv.allItems[i].consumeable.isAnimalProduct)
				{
					animalProduct.Add(Inventory.inv.allItems[i]);
				}
				if (Inventory.inv.allItems[i].consumeable.isMeat)
				{
					allMeat.Add(Inventory.inv.allItems[i]);
				}
				if (Inventory.inv.allItems[i].consumeable.isFruit)
				{
					allFruit.Add(Inventory.inv.allItems[i]);
				}
			}
		}
	}

	public int getRandomMeatInt()
	{
		return Inventory.inv.getInvItemId(allMeat[Random.Range(0, allMeat.Count)]);
	}

	public int getRandomAnimalProduct()
	{
		return Inventory.inv.getInvItemId(animalProduct[Random.Range(0, animalProduct.Count)]);
	}

	public int getRandomFruit()
	{
		return Inventory.inv.getInvItemId(allFruit[Random.Range(0, allFruit.Count)]);
	}

	public int getRandomWood()
	{
		return Inventory.inv.getInvItemId(woodLogs[Random.Range(0, woodLogs.Length)]);
	}

	public int getRandomPlank()
	{
		return Inventory.inv.getInvItemId(woodPlanks[Random.Range(0, woodPlanks.Length)]);
	}

	public int getRandomOreBar()
	{
		return Inventory.inv.getInvItemId(oreBars[Random.Range(0, oreBars.Length)]);
	}
}
