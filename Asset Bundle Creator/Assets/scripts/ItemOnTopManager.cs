using System.Collections.Generic;
using UnityEngine;

public class ItemOnTopManager : MonoBehaviour
{
	public static ItemOnTopManager manage;

	public List<ItemOnTop> onTopItems = new List<ItemOnTop>();

	private void Awake()
	{
		manage = this;
	}

	public void placeItemOnTop(int itemId, int onTopPos, int status, int rotation, int xPos, int yPos, HouseDetails house)
	{
		onTopItems.Add(new ItemOnTop(itemId, onTopPos, status, rotation, xPos, yPos, house));
	}

	public bool hasItemsOnTop(int xPos, int yPos, HouseDetails myHouse = null)
	{
		for (int i = 0; i < onTopItems.Count; i++)
		{
			if (onTopItems[i].isSittingOnObject(xPos, yPos, myHouse))
			{
				return true;
			}
		}
		return false;
	}

	public ItemOnTop getItemOnTopInPosition(int onTopPos, int xPos, int yPos, HouseDetails myHouse)
	{
		for (int i = 0; i < onTopItems.Count; i++)
		{
			if (onTopItems[i].sittingOnTopPosition(xPos, yPos, myHouse) == onTopPos)
			{
				return onTopItems[i];
			}
		}
		return null;
	}

	public ItemOnTop[] getAllItemsOnTop(int xPos, int yPos, HouseDetails myHouse)
	{
		List<ItemOnTop> list = new List<ItemOnTop>();
		for (int i = 0; i < onTopItems.Count; i++)
		{
			if (onTopItems[i].isSittingOnObject(xPos, yPos, myHouse))
			{
				list.Add(onTopItems[i]);
			}
		}
		return list.ToArray();
	}

	public ItemOnTop[] getAllItemsOnTopInHouse(HouseDetails myHouse)
	{
		List<ItemOnTop> list = new List<ItemOnTop>();
		for (int i = 0; i < onTopItems.Count; i++)
		{
			if (onTopItems[i].isInHouse(myHouse))
			{
				list.Add(onTopItems[i]);
			}
		}
		return list.ToArray();
	}

	public void removeItemOnTop(ItemOnTop removeMe)
	{
		if (removeMe != null)
		{
			onTopItems.Remove(removeMe);
		}
	}

	public void addOnTopObject(ItemOnTop onTop)
	{
		onTopItems.Add(onTop);
	}

	public void moveItemsOnTopHouse(int oldHouseX, int oldHouseY, int newHouseX, int newHouseY)
	{
		for (int i = 0; i < onTopItems.Count; i++)
		{
			onTopItems[i].relocateHousePos(oldHouseX, oldHouseY, newHouseX, newHouseY);
		}
	}
}
