using System;
using UnityEngine;

[Serializable]
public class HuntingChallenge
{
	public int[] spawnPos;

	public int challengeAnimalId;

	public int spawnChunkX;

	public int spawnChunkY;

	public HuntingChallenge()
	{
	}

	public HuntingChallenge(int spawnInBiomeId, int animalId)
	{
		challengeAnimalId = animalId;
		generatePosition(spawnInBiomeId);
	}

	public void generatePosition(int desiredBiomeId)
	{
		int num = UnityEngine.Random.Range(200, 800);
		int num2 = UnityEngine.Random.Range(200, 900);
		int num3 = 10000;
		while (GenerateMap.generate.checkBiomType(num, num2) != desiredBiomeId || num3 > 0)
		{
			num = UnityEngine.Random.Range(200, 800);
			num2 = UnityEngine.Random.Range(200, 800);
			num3--;
		}
		if (num3 == 0)
		{
			Debug.Log("Tried 10000 times and couldn't find the biome");
		}
		int num4 = challengeAnimalId;
		int num5 = num;
		int i = num2;
		for (bool canSpawnInWater = AnimalManager.manage.allAnimals[num4].saveAsAlpha.canSpawnInWater; (WorldManager.manageWorld.isPositionOnMap(num5, i) && !canSpawnInWater && WorldManager.manageWorld.waterMap[num5, i]) || !AnimalManager.manage.checkIfTileIsWalkable(num5, i); i += UnityEngine.Random.Range(-1, 2))
		{
			num5 += UnityEngine.Random.Range(-1, 2);
		}
		spawnPos = new int[3]
		{
			num5 * 2,
			WorldManager.manageWorld.heightMap[num, num2],
			i * 2
		};
		spawnChunkX = Mathf.RoundToInt(num / WorldManager.manageWorld.getChunkSize()) * WorldManager.manageWorld.getChunkSize();
		spawnChunkY = Mathf.RoundToInt(num2 / WorldManager.manageWorld.getChunkSize()) * WorldManager.manageWorld.getChunkSize();
	}

	public Vector3 getLocation()
	{
		return new Vector3(spawnPos[0], spawnPos[1], spawnPos[2]);
	}

	public bool isHuntingTargetInThisChunk(int chunkX, int chunkY)
	{
		if (chunkX == spawnChunkX && chunkY == spawnChunkY)
		{
			return true;
		}
		return false;
	}

	public string getAnimalName()
	{
		return AnimalManager.manage.allAnimals[challengeAnimalId].animalName;
	}

	public int getAnimalId()
	{
		return challengeAnimalId;
	}

	public void addToChunkOnAccept(int daysRemaining)
	{
		Vector3 location = getLocation();
		AnimalManager.manage.placeHuntingTargetOnMap(challengeAnimalId * 10, Mathf.RoundToInt(location.x / 2f), Mathf.RoundToInt(location.z / 2f), daysRemaining);
	}

	public void removeSelf()
	{
	}
}
