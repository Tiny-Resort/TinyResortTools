using System.Collections.Generic;
using UnityEngine;

public class HouseManager : MonoBehaviour
{
	public static HouseManager manage;

	public List<DisplayPlayerHouseTiles> housesOnDisplay = new List<DisplayPlayerHouseTiles>();

	public List<HouseDetails> allHouses = new List<HouseDetails>();

	public List<HouseExterior> allExteriors = new List<HouseExterior>();

	private void Awake()
	{
		manage = this;
	}

	public void onHouseLoad()
	{
	}

	public void clearHouseExteriors()
	{
		allExteriors.Clear();
	}

	public HouseExterior getHouseExterior(int xPos, int yPos)
	{
		for (int i = 0; i < allExteriors.Count; i++)
		{
			if (allExteriors[i].xPos == xPos && allExteriors[i].yPos == yPos)
			{
				return allExteriors[i];
			}
		}
		HouseExterior houseExterior = new HouseExterior(xPos, yPos);
		allExteriors.Add(houseExterior);
		if (!NetworkMapSharer.share.isServer && (bool)NetworkMapSharer.share.localChar)
		{
			NetworkMapSharer.share.localChar.CmdRequestHouseExterior(xPos, yPos);
		}
		return houseExterior;
	}

	public HouseExterior getPlayerHouseExterior()
	{
		for (int i = 0; i < allExteriors.Count; i++)
		{
			if (allExteriors[i].playerHouse)
			{
				return allExteriors[i];
			}
		}
		return null;
	}

	public HouseDetails getHouseInfo(int xPos, int yPos)
	{
		for (int i = 0; i < allHouses.Count; i++)
		{
			if (allHouses[i].xPos == xPos && allHouses[i].yPos == yPos)
			{
				return allHouses[i];
			}
		}
		HouseDetails houseDetails = new HouseDetails(xPos, yPos);
		allHouses.Add(houseDetails);
		return houseDetails;
	}

	public HouseDetails getHouseInfoIfExists(int xPos, int yPos)
	{
		for (int i = 0; i < allHouses.Count; i++)
		{
			if (allHouses[i].xPos == xPos && allHouses[i].yPos == yPos)
			{
				return allHouses[i];
			}
		}
		return null;
	}

	public HouseExterior getHouseInfoForClientExterior(int xPos, int yPos)
	{
		for (int i = 0; i < allExteriors.Count; i++)
		{
			if (allExteriors[i].xPos == xPos && allExteriors[i].yPos == yPos)
			{
				return allExteriors[i];
			}
		}
		HouseExterior houseExterior = new HouseExterior(xPos, yPos);
		allExteriors.Add(houseExterior);
		return houseExterior;
	}

	public HouseDetails getHouseInfoForClientFill(int xPos, int yPos)
	{
		for (int i = 0; i < allHouses.Count; i++)
		{
			if (allHouses[i].xPos == xPos && allHouses[i].yPos == yPos)
			{
				return allHouses[i];
			}
		}
		HouseDetails houseDetails = new HouseDetails(xPos, yPos);
		allHouses.Add(houseDetails);
		return houseDetails;
	}

	public DisplayPlayerHouseTiles findHousesOnDisplay(int xPos, int yPos)
	{
		for (int i = 0; i < housesOnDisplay.Count; i++)
		{
			if (housesOnDisplay[i].GetComponent<TileObject>().tileObjectId == WorldManager.manageWorld.onTileMap[xPos, yPos] && housesOnDisplay[i].housePosX == xPos && housesOnDisplay[i].housePosY == yPos)
			{
				return housesOnDisplay[i];
			}
		}
		if (NetworkMapSharer.share.isServer)
		{
			TileObject tileObjectForServerDrop = WorldManager.manageWorld.getTileObjectForServerDrop(WorldManager.manageWorld.onTileMap[xPos, yPos], new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2));
			tileObjectForServerDrop.setXAndY(xPos, yPos);
			WorldManager.manageWorld.returnTileObject(tileObjectForServerDrop);
			return tileObjectForServerDrop.displayPlayerHouseTiles;
		}
		return null;
	}

	public int[] getPlayersHousePos()
	{
		for (int i = 0; i < allHouses.Count; i++)
		{
			if (allHouses[i].isThePlayersHouse)
			{
				return new int[2]
				{
					allHouses[i].xPos,
					allHouses[i].yPos
				};
			}
		}
		return new int[2] { -1, -1 };
	}

	public void updateAllHouseFurniturePos()
	{
		for (int i = 0; i < housesOnDisplay.Count; i++)
		{
			housesOnDisplay[i].refreshHouseTiles();
		}
	}

	public void moveHousePos(int xPos, int yPos, int newXPos, int newYPos, int oldRotation, int newRotation)
	{
		HouseDetails houseInfo = getHouseInfo(xPos, yPos);
		if (NetworkMapSharer.share.isServer)
		{
			for (int i = 0; i < 25; i++)
			{
				for (int j = 0; j < 25; j++)
				{
					if (houseInfo.houseMapOnTile[j, i] > -1 && (bool)WorldManager.manageWorld.allObjects[houseInfo.houseMapOnTile[j, i]].tileObjectChest)
					{
						ContainerManager.manage.moveHousePosForChest(j, i, houseInfo, newXPos, newYPos);
					}
				}
			}
		}
		ItemOnTopManager.manage.moveItemsOnTopHouse(xPos, yPos, newXPos, newYPos);
		houseInfo.xPos = newXPos;
		houseInfo.yPos = newYPos;
	}

	public void moveChestInHousePos(HouseDetails house, int xPos, int yPos, int newXPos, int newYPos)
	{
		ContainerManager.manage.moveChestInsideHousePositon(house, xPos, yPos, newXPos, newYPos);
	}
}
