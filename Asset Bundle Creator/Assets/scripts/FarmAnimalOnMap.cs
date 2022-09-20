using System;

[Serializable]
public class FarmAnimalOnMap
{
	private int xPos;

	private int yPos;

	public int animalId = -1;

	public string animalName = "Bob";

	public int animalRelationShip;

	public FarmAnimalOnMap(int xPosIn, int yPosIn, int newAnimalId, string newAnimalName)
	{
		xPos = xPosIn;
		yPos = yPosIn;
		animalId = newAnimalId;
		animalName = newAnimalName;
	}

	public bool isAtPos(int CXPos, int cYPos)
	{
		if (xPos == CXPos)
		{
			return yPos == cYPos;
		}
		return false;
	}
}
