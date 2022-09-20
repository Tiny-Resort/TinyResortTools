using System;

[Serializable]
public class FencedOffAnimal
{
	public int animalId;

	public int xPos;

	public int yPos;

	public int daysRemaining;

	public FencedOffAnimal()
	{
	}

	public FencedOffAnimal(int id, int savedX, int savedY)
	{
		animalId = id;
		xPos = savedX;
		yPos = savedY;
	}

	public FencedOffAnimal(int id, int savedX, int savedY, int remainingDays)
	{
		animalId = id;
		xPos = savedX;
		yPos = savedY;
		daysRemaining = remainingDays;
	}
}
