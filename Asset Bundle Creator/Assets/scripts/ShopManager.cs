using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
	public enum stallTypes
	{
		JohnsGoods = 0,
		Clothing = 1,
		Furniture = 2,
		Weapon = 3,
		Animal = 4,
		Plants = 5,
		Crafting = 6,
		Museum = 7,
		Jimmy = 8
	}

	public static ShopManager manage;

	public bool shopsStarSync;

	public NetStall[] JohnsGoodsStalls;

	public NetStall[] ClothingStalls;

	public NetStall[] FurnitureStalls;

	public NetStall[] WeaponStalls;

	public NetStall[] AnimalStalls;

	public NetStall[] PlantStalls;

	public NetStall[] CrafterStalls;

	public NetStall[] MuseumStalls;

	public NetStall[] JimmyStalls;

	public List<InventoryItem> allSeeds = new List<InventoryItem>();

	private List<NetStall[]> quickGetList = new List<NetStall[]>();

	private void Awake()
	{
		manage = this;
		refreshNewDay();
		quickGetList.Add(JohnsGoodsStalls);
		quickGetList.Add(ClothingStalls);
		quickGetList.Add(FurnitureStalls);
		quickGetList.Add(WeaponStalls);
		quickGetList.Add(AnimalStalls);
		quickGetList.Add(PlantStalls);
		quickGetList.Add(CrafterStalls);
		quickGetList.Add(MuseumStalls);
		quickGetList.Add(JimmyStalls);
	}

	private void Start()
	{
		WorldManager.manageWorld.changeDayEvent.AddListener(refreshNewDay);
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].placeable && (bool)Inventory.inv.allItems[i].placeable.tileObjectGrowthStages && Inventory.inv.allItems[i].placeable.tileObjectGrowthStages.needsTilledSoil)
			{
				allSeeds.Add(Inventory.inv.allItems[i]);
			}
		}
	}

	public void sellStall(int type, int id)
	{
		quickGetList[type][id].hasBeenSold = true;
		quickGetList[type][id].sellIfConnected();
	}

	public NetStall connectStall(ShopBuyDrop toConnect)
	{
		quickGetList[(int)toConnect.myStallType][toConnect.shopStallNo].connectedStall = toConnect;
		return quickGetList[(int)toConnect.myStallType][toConnect.shopStallNo];
	}

	public void refreshNewDay()
	{
		for (int i = 0; i < quickGetList.Count; i++)
		{
			for (int j = 0; j < quickGetList[i].Length; j++)
			{
				quickGetList[i][j].hasBeenSold = false;
			}
		}
	}

	public void fillStallsFromRequest(bool[] requestedDetails)
	{
		int num = 0;
		for (int i = 0; i < quickGetList.Count; i++)
		{
			for (int j = 0; j < quickGetList[i].Length; j++)
			{
				quickGetList[i][j].hasBeenSold = requestedDetails[num];
				quickGetList[i][j].sellIfConnected();
				num++;
			}
		}
	}

	public bool[] getBoolArrayForSync()
	{
		List<bool> list = new List<bool>();
		for (int i = 0; i < quickGetList.Count; i++)
		{
			for (int j = 0; j < quickGetList[i].Length; j++)
			{
				list.Add(quickGetList[i][j].hasBeenSold);
			}
		}
		return list.ToArray();
	}
}
