using UnityEngine;

public class AnimalsSaved
{
	public int animalId;

	public int xPos;

	public int yPos;

	public Vector3 sleepPos;

	public int daysRemaining;

	public AnimalsSaved(int id, int xPosSpawn, int yPosSpawn, int remainingDays = 1)
	{
		animalId = id;
		xPos = xPosSpawn;
		yPos = yPosSpawn;
		daysRemaining = remainingDays;
	}

	public void spawnAnimal()
	{
		NetworkNavMesh.nav.SpawnAnAnimalOnTile(animalId, xPos, yPos, this);
	}
}
