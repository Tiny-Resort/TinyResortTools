using UnityEngine;

public class HouseDetails
{
	public int xPos = -1;

	public int yPos = -1;

	public bool isThePlayersHouse;

	public int[,] houseMapOnTile = new int[25, 25];

	public int[,] houseMapRotation = new int[25, 25];

	public int[,] houseMapOnTileStatus = new int[25, 25];

	public int wall;

	public int floor;

	public HouseDetails(int newXPos, int newYPos)
	{
		xPos = newXPos;
		yPos = newYPos;
		checkIfIsPlayerHouse();
		createNewHouseMap();
	}

	public HouseDetails()
	{
	}

	public void checkIfIsPlayerHouse()
	{
		int num = -1;
		num = WorldManager.manageWorld.onTileMap[xPos, yPos];
		for (int i = 0; i < TownManager.manage.playerHouseStages.Length; i++)
		{
			if (TownManager.manage.playerHouseStages[i].placeable.tileObjectId == num)
			{
				isThePlayersHouse = true;
			}
		}
	}

	public void clearFurnitureStatus()
	{
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				if (houseMapOnTile[j, i] > -1 && (bool)WorldManager.manageWorld.allObjects[houseMapOnTile[j, i]].tileObjectFurniture)
				{
					houseMapOnTileStatus[j, i] = 0;
				}
			}
		}
	}

	public void resetHouseMap()
	{
		createNewHouseMap();
	}

	private void createNewHouseMap()
	{
		wall = 372;
		floor = 371;
		int num = WorldManager.manageWorld.rotationMap[xPos, yPos];
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] <= -1 || !WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].displayPlayerHouseTiles)
		{
			return;
		}
		int xSize = WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].displayPlayerHouseTiles.xSize;
		int ySize = WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].displayPlayerHouseTiles.ySize;
		int num2 = WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].displayPlayerHouseTiles.xSize;
		int num3 = WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].displayPlayerHouseTiles.ySize;
		if (num == 2 || num == 4)
		{
			num2 = xSize;
			num3 = ySize;
		}
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				if (j >= num2 || i >= num3)
				{
					houseMapOnTile[j, i] = -2;
				}
				else
				{
					houseMapOnTile[j, i] = -1;
				}
				houseMapOnTileStatus[j, i] = -1;
			}
		}
	}

	public void upgradeHouseSize()
	{
		int num = WorldManager.manageWorld.onTileMap[xPos, yPos];
		int num2 = WorldManager.manageWorld.allObjects[num].displayPlayerHouseTiles.xSize;
		int num3 = WorldManager.manageWorld.allObjects[num].displayPlayerHouseTiles.ySize;
		int num4 = WorldManager.manageWorld.rotationMap[xPos, yPos];
		if (num4 == 2 || num4 == 4)
		{
			num2 = WorldManager.manageWorld.allObjects[num].displayPlayerHouseTiles.ySize;
			num3 = WorldManager.manageWorld.allObjects[num].displayPlayerHouseTiles.xSize;
		}
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				if (j >= num2 || i >= num3)
				{
					houseMapOnTile[j, i] = -2;
				}
				else if (houseMapOnTile[j, i] == -2)
				{
					houseMapOnTile[j, i] = -1;
				}
			}
		}
	}

	public void rotateHouse(int oldRotation, int desiredRotation)
	{
		int num = WorldManager.manageWorld.onTileMap[xPos, yPos];
		int xLength = WorldManager.manageWorld.allObjects[num].displayPlayerHouseTiles.xSize;
		int yLength = WorldManager.manageWorld.allObjects[num].displayPlayerHouseTiles.ySize;
		if (oldRotation == 2 || oldRotation == 4)
		{
			xLength = WorldManager.manageWorld.allObjects[num].displayPlayerHouseTiles.ySize;
			yLength = WorldManager.manageWorld.allObjects[num].displayPlayerHouseTiles.xSize;
		}
		int num2 = oldRotation;
		while (num2 != desiredRotation)
		{
			Debug.Log("Rotated house once");
			rotateHouseMap90(xLength, yLength);
			num2++;
			if (num2 > 4)
			{
				num2 = 1;
			}
		}
	}

	public void rotateHouseMap90(int xLength, int yLength)
	{
		int[,] array = new int[xLength, yLength];
		int[,] array2 = new int[xLength, yLength];
		int[,] array3 = new int[xLength, yLength];
		int[,] array4 = new int[xLength, yLength];
		int[,] array5 = new int[xLength, yLength];
		int[,] array6 = new int[xLength, yLength];
		int[,] array7 = new int[xLength, yLength];
		int[,] array8 = new int[xLength, yLength];
		for (int i = 0; i < yLength; i++)
		{
			for (int j = 0; j < xLength; j++)
			{
				array[j, i] = Mathf.Clamp(houseMapOnTile[j, i], -1, WorldManager.manageWorld.allObjects.Length);
				array3[j, i] = houseMapRotation[j, i];
				array4[j, i] = array4[j, i];
			}
		}
		int[,] array9 = new int[xLength, yLength];
		for (int num = xLength - 1; num >= 0; num--)
		{
			for (int k = 0; k < yLength; k++)
			{
				if (array[num, k] > 0 && (bool)WorldManager.manageWorld.allObjects[array[num, k]].tileObjectChest)
				{
					HouseManager.manage.moveChestInHousePos(this, num, k, num, k);
				}
				array5[k, xLength - 1 - num] = array[num, k];
				array6[k, xLength - 1 - num] = array2[num, k];
				array7[k, xLength - 1 - num] = array3[num, k];
				if (array7[k, xLength - 1 - num] > 0)
				{
					array7[k, xLength - 1 - num]++;
					if (array7[k, xLength - 1 - num] > 4)
					{
						array7[k, xLength - 1 - num] = 1;
					}
				}
				array8[k, xLength - 1 - num] = array4[num, k];
			}
		}
		for (int l = 0; l < yLength; l++)
		{
			for (int m = 0; m < xLength; m++)
			{
				houseMapOnTile[m, l] = array5[m, l];
				houseMapRotation[m, l] = array7[m, l];
				array4[m, l] = array8[m, l];
			}
		}
		for (int n = 0; n < yLength; n++)
		{
			for (int num2 = 0; num2 < xLength; num2++)
			{
				if (houseMapOnTile[num2, n] > -1 && WorldManager.manageWorld.allObjectSettings[houseMapOnTile[num2, n]].isMultiTileObject)
				{
					if (houseMapRotation[num2, n] == 1 || houseMapRotation[num2, n] == 3 || houseMapRotation[num2, n] == 4)
					{
						WorldManager.manageWorld.allObjects[houseMapOnTile[num2, n]].placeMultiTiledObjectInside(num2, n, houseMapRotation[num2, n], this);
					}
					else if (houseMapRotation[num2, n] == 2)
					{
						int num3 = WorldManager.manageWorld.allObjects[houseMapOnTile[num2, n]].getXSize() / 2;
						houseMapOnTile[num2, n - num3] = houseMapOnTile[num2, n];
						houseMapOnTile[num2, n] = -1;
						houseMapRotation[num2, n - num3] = houseMapRotation[num2, n];
						houseMapRotation[num2, n] = 0;
						WorldManager.manageWorld.allObjects[houseMapOnTile[num2, n - num3]].placeMultiTiledObjectInside(num2, n - num3, houseMapRotation[num2, n - num3], this);
					}
				}
			}
		}
	}
}
