using System.Collections.Generic;
using UnityEngine;

public class AnimalChunk
{
	public int chunkX;

	public int chunkY;

	public List<AnimalsSaved> thisChunk = new List<AnimalsSaved>();

	public AnimalChunk(int xChunk, int yChunk)
	{
		chunkX = xChunk;
		chunkY = yChunk;
	}

	public void spawnAnimalsInChunk()
	{
		for (int i = 0; i < thisChunk.Count; i++)
		{
			thisChunk[i].spawnAnimal();
		}
		thisChunk.Clear();
	}

	public void addAnimalToChunk(int id, int xPos, int yPos)
	{
		if (id != -1)
		{
			thisChunk.Add(new AnimalsSaved(id, xPos, yPos));
		}
	}

	public void addAnimalToChunkWithSleepPos(int id, int xPos, int yPos, Vector3 sleepPos, int daysRemaining)
	{
		if (id != -1)
		{
			AnimalsSaved animalsSaved = new AnimalsSaved(id, xPos, yPos, daysRemaining);
			animalsSaved.sleepPos = sleepPos;
			thisChunk.Add(animalsSaved);
		}
	}

	public int checkAmountAlreadyInChunk(int animalIdToCheck)
	{
		int num = 0;
		for (int i = 0; i < thisChunk.Count; i++)
		{
			if (thisChunk[i].animalId >= animalIdToCheck && thisChunk[i].animalId <= animalIdToCheck + 9)
			{
				num++;
			}
		}
		return num;
	}

	public bool clearedChunkNeedsNewAnimals()
	{
		if (thisChunk.Count > 0)
		{
			List<AnimalsSaved> list = new List<AnimalsSaved>();
			for (int i = 0; i < thisChunk.Count; i++)
			{
				if (isFencedOff(thisChunk[i].animalId, thisChunk[i].xPos, thisChunk[i].yPos) || isAlpha(thisChunk[i].animalId))
				{
					list.Add(thisChunk[i]);
				}
			}
			thisChunk.Clear();
			thisChunk = list;
			return true;
		}
		return false;
	}

	public bool isFencedOff(int animalId, int xPos, int yPos)
	{
		int num = (int)((float)animalId / 10f * 10f) / 10;
		if (AnimalManager.manage.allAnimals[num].saveFencedOffAnimalsEvent)
		{
			if (WorldManager.manageWorld.fencedOffMap[xPos, yPos] >= 1)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public bool isAlpha(int animalId)
	{
		int num = (int)((float)animalId / 10f * 10f) / 10;
		return AnimalManager.manage.allAnimals[num].saveAsAlpha;
	}

	public void fillFencedOffAnimalArray()
	{
		for (int i = 0; i < thisChunk.Count; i++)
		{
			if (isFencedOff(thisChunk[i].animalId, thisChunk[i].xPos, thisChunk[i].yPos))
			{
				AnimalManager.manage.fencedOffAnimals.Add(new FencedOffAnimal(thisChunk[i].animalId, thisChunk[i].xPos, thisChunk[i].yPos));
			}
			if (isAlpha(thisChunk[i].animalId))
			{
				AnimalManager.manage.alphaAnimals.Add(new FencedOffAnimal(thisChunk[i].animalId, thisChunk[i].xPos, thisChunk[i].yPos, thisChunk[i].daysRemaining));
			}
		}
	}
}
