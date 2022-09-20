using System;

[Serializable]
public class ItemOnTop
{
	public int sittingOnX;

	public int sittingOnY;

	public int houseX = -1;

	public int houseY = -1;

	public int onTopPosition;

	public int itemId;

	public int itemStatus;

	public int itemRotation;

	public ItemOnTop(int newItemId, int newOnTopPos, int status, int rotation, int xPos, int yPos, HouseDetails house)
	{
		itemId = newItemId;
		itemRotation = rotation;
		onTopPosition = newOnTopPos;
		itemStatus = status;
		sittingOnX = xPos;
		sittingOnY = yPos;
		if (house != null)
		{
			houseX = house.xPos;
			houseY = house.yPos;
		}
	}

	public ItemOnTop()
	{
	}

	public bool isSittingOnObject(int xPos, int yPos, HouseDetails house)
	{
		if (xPos == sittingOnX && yPos == sittingOnY && houseMatched(house))
		{
			return true;
		}
		return false;
	}

	public bool relocateHousePos(int oldHouseX, int oldHouseY, int newHouseX, int newHouseY)
	{
		if (houseX == oldHouseX && houseY == oldHouseY)
		{
			houseX = newHouseX;
			houseY = newHouseY;
		}
		return false;
	}

	public int sittingOnTopPosition(int xPos, int yPos, HouseDetails house)
	{
		if (xPos == sittingOnX && yPos == sittingOnY && houseMatched(house))
		{
			return onTopPosition;
		}
		return -1;
	}

	public bool isInHouse(HouseDetails house)
	{
		if (houseX == house.xPos)
		{
			return houseY == house.yPos;
		}
		return false;
	}

	public int sittingOnTopPosition()
	{
		return onTopPosition;
	}

	public int getTileObjectId()
	{
		return itemId;
	}

	public int getRotation()
	{
		return itemRotation;
	}

	public int getStatus()
	{
		return itemStatus;
	}

	private bool houseMatched(HouseDetails house)
	{
		if (house == null)
		{
			if (houseX == -1 && houseY == -1)
			{
				return true;
			}
			return false;
		}
		if (house.xPos == houseX && house.yPos == houseY)
		{
			return true;
		}
		return false;
	}
}
