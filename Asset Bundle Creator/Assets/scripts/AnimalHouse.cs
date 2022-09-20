using System;
using UnityEngine;

[Serializable]
public class AnimalHouse
{
	public int xPos;

	public int yPos;

	public bool belongsToAnimal;

	public AnimalHouse()
	{
	}

	public AnimalHouse(int xPosIn, int yPosIn)
	{
		xPos = xPosIn;
		yPos = yPosIn;
		belongsToAnimal = false;
	}

	public bool isAtPos(int CXPos, int cYPos)
	{
		if (xPos == CXPos)
		{
			return yPos == cYPos;
		}
		return false;
	}

	public int returnTileId()
	{
		return WorldManager.manageWorld.onTileMap[xPos, yPos];
	}

	public Vector3 returnWorldPos()
	{
		return new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2);
	}
}
