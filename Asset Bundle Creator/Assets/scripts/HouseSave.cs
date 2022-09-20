using System;

[Serializable]
public class HouseSave
{
	public int xPos = -1;

	public int yPos = -1;

	public int[,] houseMapOnTile = new int[25, 25];

	public int[,] houseMapOnTop = new int[25, 25];

	public int[,] houseMapRotation = new int[25, 25];

	public int[,] houseMapOnTileStatus = new int[25, 25];

	public int wall;

	public int floor;

	public bool isPlayerHouse;

	public HouseSave()
	{
	}

	public HouseSave(HouseDetails copyFrom)
	{
		saveDetails(copyFrom);
	}

	private void saveDetails(HouseDetails copyFrom)
	{
		xPos = copyFrom.xPos;
		yPos = copyFrom.yPos;
		isPlayerHouse = copyFrom.isThePlayersHouse;
		houseMapOnTile = copyFrom.houseMapOnTile;
		houseMapRotation = copyFrom.houseMapRotation;
		houseMapOnTileStatus = copyFrom.houseMapOnTileStatus;
		wall = copyFrom.wall;
		floor = copyFrom.floor;
	}

	public void loadDetails(HouseDetails copyTo)
	{
		correctStatusOfChangers();
		copyTo.xPos = xPos;
		copyTo.yPos = yPos;
		copyTo.isThePlayersHouse = isPlayerHouse;
		copyTo.houseMapOnTile = houseMapOnTile;
		copyTo.houseMapRotation = houseMapRotation;
		copyTo.houseMapOnTileStatus = houseMapOnTileStatus;
		copyTo.wall = wall;
		copyTo.floor = floor;
	}

	public void correctStatusOfChangers()
	{
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				if (houseMapOnTile[j, i] > 0 && (bool)WorldManager.manageWorld.allObjects[houseMapOnTile[j, i]].tileObjectItemChanger && houseMapOnTileStatus[j, i] == 0)
				{
					houseMapOnTileStatus[j, i] = -2;
				}
			}
		}
	}
}
