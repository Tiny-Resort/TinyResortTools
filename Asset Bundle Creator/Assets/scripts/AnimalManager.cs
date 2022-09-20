using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimalManager : MonoBehaviour
{
	public static AnimalManager manage;

	public AnimalAI[] allAnimals;

	private List<AnimalAI> animalPool = new List<AnimalAI>();

	public List<Nest> allNests = new List<Nest>();

	public GameObject releasedBug;

	public GameObject releaseFish;

	public UnityEvent lookAtBugBook = new UnityEvent();

	public bool bugBookOpen;

	public UnityEvent lookAtFishBook = new UnityEvent();

	public bool fishBookOpen;

	public List<FencedOffAnimal> fencedOffAnimals = new List<FencedOffAnimal>();

	public List<FencedOffAnimal> alphaAnimals = new List<FencedOffAnimal>();

	public bool crocDay;

	public TileObject crocoBerleyBox;

	public TileObject devilBerleyBox;

	public TileObject sharkBerleyBox;

	public bool trapperCanSpawn;

	public LayerMask swimLayerCheck;

	[Header("Fishing Tables --------------")]
	public InventoryLootTableTimeWeatherMaster northernOceanFish;

	public InventoryLootTableTimeWeatherMaster southernOceanFish;

	public InventoryLootTableTimeWeatherMaster riverFish;

	public InventoryLootTableTimeWeatherMaster mangroveFish;

	public InventoryLootTableTimeWeatherMaster billabongFish;

	[Header("Bug Tables --------------")]
	public InventoryLootTableTimeWeatherMaster topicalBugs;

	public InventoryLootTableTimeWeatherMaster desertBugs;

	public InventoryLootTableTimeWeatherMaster bushlandBugs;

	public InventoryLootTableTimeWeatherMaster pineLandBugs;

	public InventoryLootTableTimeWeatherMaster plainsBugs;

	[Header("Ocean Creatures Tables --------------")]
	public InventoryLootTableTimeWeatherMaster underWaterOceanCreatures;

	public InventoryLootTableTimeWeatherMaster underWaterRiverCreatures;

	[Header("Animals Spawn by Biome --------------")]
	public AnimalBiomeTable tropicalAnimals;

	public AnimalBiomeTable billabongAnimals;

	public AnimalBiomeTable bushlandAnimals;

	public AnimalBiomeTable desertAnimals;

	public AnimalBiomeTable pineForestAnimals;

	public AnimalBiomeTable plainsAnimals;

	[Header("Underground Animals Spawn by Biome ------")]
	public AnimalBiomeTable undergroundAnimals;

	public List<AnimalChunk> loadedChunks = new List<AnimalChunk>();

	public List<AnimalChunk> loadedUndergroundChunks = new List<AnimalChunk>();

	public UnityEvent saveFencedOffAnimalsEvent = new UnityEvent();

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
		populateFishingTables();
	}

	public AnimalAI spawnFreeAnimal(int animalId, Vector3 spawnPos)
	{
		if (animalId < 0 || animalId >= allAnimals.Length)
		{
			return null;
		}
		AnimalAI animalAI = null;
		Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
		foreach (AnimalAI item in animalPool)
		{
			if (item.animalId == animalId)
			{
				animalAI = item;
				item.transform.position = spawnPos;
				item.transform.rotation = rotation;
				item.gameObject.SetActive(true);
				animalPool.Remove(item);
				if (animalId == 1)
				{
					animalAI.GetComponent<FishType>().generateFishForEnviroment();
				}
				return animalAI;
			}
		}
		animalAI = Object.Instantiate(allAnimals[animalId].gameObject, spawnPos, rotation).GetComponent<AnimalAI>();
		animalAI.setUp();
		if (animalId == 1)
		{
			animalAI.GetComponent<FishType>().generateFishForEnviroment();
		}
		return animalAI;
	}

	public void returnAnimalAndSaveToMap(AnimalAI returnMe)
	{
		int xPos = Mathf.RoundToInt(returnMe.transform.position.x / 2f);
		int yPos = Mathf.RoundToInt(returnMe.transform.position.z / 2f);
		safeSaveAnimalOnMap(returnMe, xPos, yPos);
		animalPool.Add(returnMe);
	}

	public void returnAnimalAndDoNotSaveToMap(AnimalAI returnMe)
	{
		animalPool.Add(returnMe);
	}

	private void safeSaveAnimalOnMap(AnimalAI animal, int xPos, int yPos)
	{
		if ((bool)animal.saveAsAlpha)
		{
			saveAnimalToChunk(animal.animalId * 10 + animal.getVariationNo(), xPos, yPos, animal.getSleepPos(), animal.saveAsAlpha.daysRemaining);
		}
		else
		{
			saveAnimalToChunk(animal.animalId * 10 + animal.getVariationNo(), xPos, yPos, animal.getSleepPos());
		}
	}

	public bool checkIfTileIsWalkable(int xPos, int yPos)
	{
		if (!WorldManager.manageWorld.isPositionOnMap(xPos, yPos))
		{
			return false;
		}
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] == -1)
		{
			return true;
		}
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] >= 0 && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].walkable)
		{
			return true;
		}
		return false;
	}

	public string fillAnimalLocation(InventoryItem item)
	{
		if ((bool)item.fish)
		{
			if (northernOceanFish.isBugOrFishInTable(item))
			{
				return northernOceanFish.locationName;
			}
			if (southernOceanFish.isBugOrFishInTable(item))
			{
				return southernOceanFish.locationName;
			}
			if (riverFish.isBugOrFishInTable(item))
			{
				return riverFish.locationName;
			}
			if (mangroveFish.isBugOrFishInTable(item))
			{
				return mangroveFish.locationName;
			}
		}
		if ((bool)item.bug)
		{
			if (bushlandBugs.isBugOrFishInTable(item))
			{
				return bushlandBugs.locationName;
			}
			if (desertBugs.isBugOrFishInTable(item))
			{
				return desertBugs.locationName;
			}
			if (pineLandBugs.isBugOrFishInTable(item))
			{
				return pineLandBugs.locationName;
			}
			if (plainsBugs.isBugOrFishInTable(item))
			{
				return plainsBugs.locationName;
			}
			if (topicalBugs.isBugOrFishInTable(item))
			{
				return plainsBugs.locationName;
			}
		}
		return "unknown location";
	}

	public string fillAnimalTimeOfDay(InventoryItem item)
	{
		if ((bool)item.fish)
		{
			if (northernOceanFish.isBugOrFishInTable(item))
			{
				return northernOceanFish.getTimeOfDayFound(item);
			}
			if (southernOceanFish.isBugOrFishInTable(item))
			{
				return southernOceanFish.getTimeOfDayFound(item);
			}
			if (riverFish.isBugOrFishInTable(item))
			{
				return riverFish.getTimeOfDayFound(item);
			}
			if (mangroveFish.isBugOrFishInTable(item))
			{
				return mangroveFish.getTimeOfDayFound(item);
			}
		}
		if ((bool)item.bug)
		{
			if (bushlandBugs.isBugOrFishInTable(item))
			{
				return bushlandBugs.getTimeOfDayFound(item);
			}
			if (desertBugs.isBugOrFishInTable(item))
			{
				return desertBugs.getTimeOfDayFound(item);
			}
			if (pineLandBugs.isBugOrFishInTable(item))
			{
				return pineLandBugs.getTimeOfDayFound(item);
			}
			if (plainsBugs.isBugOrFishInTable(item))
			{
				return plainsBugs.getTimeOfDayFound(item);
			}
			if (topicalBugs.isBugOrFishInTable(item))
			{
				return plainsBugs.getTimeOfDayFound(item);
			}
		}
		return "all day";
	}

	public void fillBugPediaEntry(InventoryItem item)
	{
		string text = "";
		if (topicalBugs.isBugOrFishInTable(item) && desertBugs.isBugOrFishInTable(item) && bushlandBugs.isBugOrFishInTable(item) && pineLandBugs.isBugOrFishInTable(item) && plainsBugs.isBugOrFishInTable(item))
		{
			text += capitaliseFirstLetter("Everywhere");
			plainsBugs.getTimeOfDayFound(item);
			plainsBugs.getSeason(item);
		}
		else
		{
			if (topicalBugs.isBugOrFishInTable(item))
			{
				text += capitaliseFirstLetter(topicalBugs.locationName);
				topicalBugs.getTimeOfDayFound(item);
				topicalBugs.getSeason(item);
			}
			if (desertBugs.isBugOrFishInTable(item))
			{
				if (text != "")
				{
					text += " & ";
				}
				text += capitaliseFirstLetter(desertBugs.locationName);
				desertBugs.getTimeOfDayFound(item);
				desertBugs.getSeason(item);
			}
			if (bushlandBugs.isBugOrFishInTable(item))
			{
				if (text != "")
				{
					text += " & ";
				}
				text += capitaliseFirstLetter(bushlandBugs.locationName);
				bushlandBugs.getTimeOfDayFound(item);
				bushlandBugs.getSeason(item);
			}
			if (pineLandBugs.isBugOrFishInTable(item))
			{
				if (text != "")
				{
					text += " & ";
				}
				text += capitaliseFirstLetter(pineLandBugs.locationName);
				pineLandBugs.getTimeOfDayFound(item);
				pineLandBugs.getSeason(item);
			}
			if (plainsBugs.isBugOrFishInTable(item))
			{
				if (text != "")
				{
					text += " & ";
				}
				text += capitaliseFirstLetter(plainsBugs.locationName);
				plainsBugs.getTimeOfDayFound(item);
				plainsBugs.getSeason(item);
			}
		}
		PediaManager.manage.locationText.text = text;
	}

	private string capitaliseFirstLetter(string toChange)
	{
		toChange = char.ToUpper(toChange[0]) + toChange.Substring(1);
		return toChange;
	}

	private void populateChunkUnderground(AnimalChunk toFill, int chunkX, int chunkY)
	{
		for (int i = chunkY; i < chunkY + WorldManager.manageWorld.getChunkSize(); i++)
		{
			for (int j = chunkX; j < chunkX + WorldManager.manageWorld.getChunkSize(); j++)
			{
				if (WorldManager.manageWorld.onTileMap[j, i] != -1)
				{
					continue;
				}
				if (Random.Range(0, 1500) == 2)
				{
					toFill.addAnimalToChunk(200, j, i);
				}
				if (WorldManager.manageWorld.heightMap[j, i] >= 1 && Random.Range(0, 1600) == 2)
				{
					toFill.addAnimalToChunk(140, j, i);
				}
				if (Random.Range(0, 800) == 2 && WorldManager.manageWorld.heightMap[j, i] >= 1)
				{
					toFill.addAnimalToChunk(270, j, i);
					toFill.addAnimalToChunk(270, j, i);
					if (Random.Range(0, 2) == 1)
					{
						toFill.addAnimalToChunk(270, j, i);
					}
					if (Random.Range(0, 5) == 3)
					{
						toFill.addAnimalToChunk(270, j, i);
					}
					if (Random.Range(0, 9) == 7)
					{
						toFill.addAnimalToChunk(270, j, i);
					}
					if (Random.Range(0, 13) == 7)
					{
						toFill.addAnimalToChunk(270, j, i);
					}
					if (Random.Range(0, 20) == 7)
					{
						toFill.addAnimalToChunk(270, j, i);
					}
					if (Random.Range(0, 28) == 7)
					{
						toFill.addAnimalToChunk(270, j, i);
					}
				}
			}
		}
	}

	public bool canSpawnOnTile(int xPos, int yPos)
	{
		if (WorldManager.manageWorld.fencedOffMap[xPos, yPos] > 0)
		{
			return false;
		}
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] == -1)
		{
			return true;
		}
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] > -1 && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].walkable)
		{
			return true;
		}
		return false;
	}

	private void populateChunk(AnimalChunk toFill, int chunkX, int chunkY)
	{
		if (1 == 0)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		List<int[]> list = new List<int[]>();
		for (int i = chunkY; i < chunkY + WorldManager.manageWorld.getChunkSize(); i++)
		{
			for (int j = chunkX; j < chunkX + WorldManager.manageWorld.getChunkSize(); j++)
			{
				switch (GenerateMap.generate.checkBiomType(j, i))
				{
				case 15:
					if (WorldManager.manageWorld.onTileMap[j, i] == GenerateMap.generate.denWallObjects.objectsInBiom[0].tileObjectId)
					{
						flag = true;
					}
					break;
				case 14:
					if (WorldManager.manageWorld.onTileMap[j, i] == GenerateMap.generate.denWallObjects.objectsInBiom[0].tileObjectId)
					{
						flag2 = true;
					}
					break;
				}
				if (WorldManager.manageWorld.onTileMap[j, i] == crocoBerleyBox.tileObjectId)
				{
					list.Add(new int[2] { j, i });
				}
				else if (WorldManager.manageWorld.onTileMap[j, i] == devilBerleyBox.tileObjectId)
				{
					list.Add(new int[2] { j, i });
				}
				else if (WorldManager.manageWorld.onTileMap[j, i] == sharkBerleyBox.tileObjectId)
				{
					list.Add(new int[2] { j, i });
				}
			}
		}
		for (int k = chunkY; k < chunkY + WorldManager.manageWorld.getChunkSize(); k++)
		{
			for (int l = chunkX; l < chunkX + WorldManager.manageWorld.getChunkSize(); l++)
			{
				if (!canSpawnOnTile(l, k))
				{
					continue;
				}
				if (WorldManager.manageWorld.waterMap[l, k] && WorldManager.manageWorld.heightMap[l, k] < 0)
				{
					if (WorldManager.manageWorld.tileTypeMap[l, k] == 2)
					{
						if (Random.Range(0, 100) < 2)
						{
							toFill.addAnimalToChunk(10, l, k);
						}
						if (Random.Range(0, 250) < 2)
						{
							toFill.addAnimalToChunk(230, l, k);
						}
						if (GenerateMap.generate.checkBiomType(l, k) == 2)
						{
							if (crocDay)
							{
								if (Random.Range(0, 50) == 25)
								{
									toFill.addAnimalToChunk(30, l, k);
								}
							}
							else if (Random.Range(0, 300) == 100)
							{
								toFill.addAnimalToChunk(30, l, k);
							}
						}
						else if (crocDay)
						{
							if (Random.Range(0, 100) == 50)
							{
								toFill.addAnimalToChunk(30, l, k);
							}
						}
						else if (Random.Range(0, 1500) == 100)
						{
							toFill.addAnimalToChunk(30, l, k);
						}
						continue;
					}
					if (Random.Range(0, 100) == 1)
					{
						toFill.addAnimalToChunk(10, l, k);
					}
					if (Random.Range(0, 5000) == 1)
					{
						toFill.addAnimalToChunk(50, l, k);
					}
					if (Random.Range(0, 600) < 2)
					{
						toFill.addAnimalToChunk(230, l, k);
					}
					if (Random.Range(0, 260) == 1)
					{
						toFill.addAnimalToChunk(180, l, k);
						if (Random.Range(0, 2) == 1)
						{
							toFill.addAnimalToChunk(180, l, k);
						}
						if (Random.Range(0, 4) == 3)
						{
							toFill.addAnimalToChunk(180, l, k);
						}
					}
					continue;
				}
				if (trapperCanSpawn && RealWorldTimeLight.time.currentHour >= 12 && Random.Range(0, 19500) == 1 && NetworkNavMesh.nav.farAwayFromAllNPCs())
				{
					trapperCanSpawn = false;
					NPCManager.manage.npcsOnMap.Add(new NPCMapAgent(5, l, k));
					NetworkMapSharer.share.RpcPlayTrapperSound(new Vector3(l * 2, 0f, k * 2));
				}
				int num = GenerateMap.generate.checkBiomType(l, k);
				switch (num)
				{
				case 1:
					toFill.addAnimalToChunk(tropicalAnimals.getBiomeAnimal(), l, k);
					break;
				case 2:
				case 3:
					toFill.addAnimalToChunk(billabongAnimals.getBiomeAnimal(), l, k);
					break;
				case 4:
					toFill.addAnimalToChunk(bushlandAnimals.getBiomeAnimal(), l, k);
					if (Random.Range(0, 2000) == 2)
					{
						toFill.addAnimalToChunk(40, l, k);
						if (Random.Range(0, 2) == 1)
						{
							toFill.addAnimalToChunk(40, l, k);
						}
						if (Random.Range(0, 5) == 3)
						{
							toFill.addAnimalToChunk(40, l, k);
						}
						if (Random.Range(0, 9) == 7)
						{
							toFill.addAnimalToChunk(40, l, k);
						}
					}
					break;
				case 5:
					toFill.addAnimalToChunk(desertAnimals.getBiomeAnimal(), l, k);
					break;
				case 6:
					toFill.addAnimalToChunk(pineForestAnimals.getBiomeAnimal(), l, k);
					break;
				case 7:
					toFill.addAnimalToChunk(plainsAnimals.getBiomeAnimal(), l, k);
					break;
				case 8:
					toFill.addAnimalToChunk(pineForestAnimals.getBiomeAnimal(), l, k);
					break;
				case 13:
					if (WorldManager.manageWorld.onTileMap[l, k] == GenerateMap.generate.cassowaryNestObjects.getBiomObject())
					{
						addNestToList(l, k);
						toFill.addAnimalToChunk(160, l, k);
					}
					break;
				default:
					if (flag2 && num == 14)
					{
						if (toFill.checkAmountAlreadyInChunk(140) < 1 && Random.Range(0, 10) == 2)
						{
							toFill.addAnimalToChunk(140, l, k);
						}
					}
					else if (flag && num == 15 && toFill.checkAmountAlreadyInChunk(60) <= 1 && Random.Range(0, 15) == 2)
					{
						toFill.addAnimalToChunk(60 + allAnimals[6].getRandomVariationNo(), l, k);
					}
					break;
				case 0:
					break;
				}
				if (WorldManager.manageWorld.heightMap[l, k] > 0)
				{
					if (Random.Range(0, 500) == 1)
					{
						toFill.addAnimalToChunk(20, l, k);
					}
					else if (WeatherManager.manage.raining && Random.Range(0, 1500) == 1)
					{
						toFill.addAnimalToChunk(150, l, k);
					}
				}
			}
		}
		for (int m = 0; m < list.Count; m++)
		{
			int id = 30;
			int num2 = 0;
			if (WorldManager.manageWorld.onTileMap[list[m][0], list[m][1]] == crocoBerleyBox.tileObjectId)
			{
				num2 = Random.Range(2, 4);
			}
			else if (WorldManager.manageWorld.onTileMap[list[m][0], list[m][1]] == devilBerleyBox.tileObjectId)
			{
				num2 = Random.Range(2, 4);
				id = 140;
			}
			else if (WorldManager.manageWorld.onTileMap[list[m][0], list[m][1]] == sharkBerleyBox.tileObjectId)
			{
				num2 = Random.Range(2, 4);
				id = 50;
			}
			NetworkMapSharer.share.RpcClearOnTileObjectNoDrop(list[m][0], list[m][1]);
			while (num2 > 0)
			{
				for (int n = -5; n <= 5; n++)
				{
					for (int num3 = -5; num3 <= 5; num3++)
					{
						if (num2 <= 0)
						{
							break;
						}
						if (Random.Range(0, 5) == 3 && WorldManager.manageWorld.isPositionOnMap(list[m][0] + num3, list[m][1] + n))
						{
							toFill.addAnimalToChunk(id, list[m][0] + num3, list[m][1] + n);
							num2--;
						}
					}
					if (num2 <= 0)
					{
						break;
					}
				}
			}
		}
	}

	public void checkChunkForAnimals(int chunkX, int chunkY)
	{
		if (RealWorldTimeLight.time.underGround)
		{
			for (int i = 0; i < loadedUndergroundChunks.Count; i++)
			{
				if (loadedUndergroundChunks[i].chunkX == chunkX && loadedUndergroundChunks[i].chunkY == chunkY)
				{
					loadedUndergroundChunks[i].spawnAnimalsInChunk();
					return;
				}
			}
		}
		else
		{
			for (int j = 0; j < loadedChunks.Count; j++)
			{
				if (loadedChunks[j].chunkX == chunkX && loadedChunks[j].chunkY == chunkY)
				{
					loadedChunks[j].spawnAnimalsInChunk();
					return;
				}
			}
		}
		createNewChunkAndPopulateAndSpawn(chunkX, chunkY);
	}

	public void nextDayAnimalChunks()
	{
		getAllFencedOffAnimals();
		StartCoroutine(refreshLoadedAnimalChunksOnNewDay());
		addEggsToNests();
		List<FencedOffAnimal> list = new List<FencedOffAnimal>();
		for (int i = 0; i < alphaAnimals.Count; i++)
		{
			alphaAnimals[i].daysRemaining--;
			if (alphaAnimals[i].daysRemaining < 0)
			{
				list.Add(alphaAnimals[i]);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			alphaAnimals.Remove(list[j]);
		}
	}

	private IEnumerator refreshLoadedAnimalChunksOnNewDay()
	{
		for (int i = 0; i < loadedChunks.Count; i++)
		{
			if (loadedChunks[i].clearedChunkNeedsNewAnimals())
			{
				populateChunk(loadedChunks[i], loadedChunks[i].chunkX, loadedChunks[i].chunkY);
			}
			yield return null;
		}
	}

	private void createNewChunkAndPopulateAndSpawn(int chunkX, int chunkY)
	{
		AnimalChunk animalChunk = new AnimalChunk(chunkX, chunkY);
		if (RealWorldTimeLight.time.underGround)
		{
			populateChunkUnderground(animalChunk, chunkX, chunkY);
			loadedUndergroundChunks.Add(animalChunk);
		}
		else
		{
			populateChunk(animalChunk, chunkX, chunkY);
			loadedChunks.Add(animalChunk);
		}
		animalChunk.spawnAnimalsInChunk();
	}

	public void saveAnimalToChunk(int id, int xPos, int yPos, Vector3 sleepPos, int daysRemaining = 1)
	{
		int num = Mathf.RoundToInt(xPos / WorldManager.manageWorld.getChunkSize()) * WorldManager.manageWorld.getChunkSize();
		int num2 = Mathf.RoundToInt(yPos / WorldManager.manageWorld.getChunkSize()) * WorldManager.manageWorld.getChunkSize();
		if (RealWorldTimeLight.time.underGround)
		{
			for (int i = 0; i < loadedUndergroundChunks.Count; i++)
			{
				if (loadedUndergroundChunks[i].chunkX == num && loadedUndergroundChunks[i].chunkY == num2)
				{
					loadedUndergroundChunks[i].addAnimalToChunkWithSleepPos(id, xPos, yPos, sleepPos, daysRemaining);
					return;
				}
			}
			AnimalChunk animalChunk = new AnimalChunk(num, num2);
			populateChunk(animalChunk, num, num2);
			animalChunk.addAnimalToChunkWithSleepPos(id, xPos, yPos, sleepPos, daysRemaining);
			loadedUndergroundChunks.Add(animalChunk);
			return;
		}
		for (int j = 0; j < loadedChunks.Count; j++)
		{
			if (loadedChunks[j].chunkX == num && loadedChunks[j].chunkY == num2)
			{
				loadedChunks[j].addAnimalToChunkWithSleepPos(id, xPos, yPos, sleepPos, daysRemaining);
				return;
			}
		}
		AnimalChunk animalChunk2 = new AnimalChunk(num, num2);
		populateChunk(animalChunk2, num, num2);
		animalChunk2.addAnimalToChunkWithSleepPos(id, xPos, yPos, sleepPos, daysRemaining);
		loadedChunks.Add(animalChunk2);
	}

	public void addNestToList(int xPos, int yPos)
	{
		for (int i = 0; i < allNests.Count; i++)
		{
			if (allNests[i].check(xPos, yPos))
			{
				return;
			}
		}
		Nest nest = new Nest(xPos, yPos);
		allNests.Add(nest);
		if (WorldManager.manageWorld.month == 4 && !nest.isEggNearby() && nest.canHaveEgg() && Random.Range(0, 5) == 3)
		{
			nest.giveEgg();
			NetworkMapSharer.share.spawnACarryable(NetworkMapSharer.share.cassowaryEgg, new Vector3(nest.xPos * 2, WorldManager.manageWorld.heightMap[nest.xPos, nest.yPos], nest.yPos * 2));
		}
	}

	public void loadEggsIntoNests()
	{
	}

	public void addEggsToNests()
	{
		if (WorldManager.manageWorld.month != 4)
		{
			return;
		}
		int biomObject = GenerateMap.generate.cassowaryNestObjects.getBiomObject();
		List<Nest> list = new List<Nest>();
		for (int i = 0; i < allNests.Count; i++)
		{
			if (WorldManager.manageWorld.onTileMap[allNests[i].xPos, allNests[i].yPos] != biomObject)
			{
				list.Add(allNests[i]);
			}
			else if (!allNests[i].isEggNearby())
			{
				if (allNests[i].canHaveEgg() && Random.Range(0, 5) == 3)
				{
					allNests[i].giveEgg();
					NetworkMapSharer.share.spawnACarryable(NetworkMapSharer.share.cassowaryEgg, new Vector3(allNests[i].xPos * 2, WorldManager.manageWorld.heightMap[allNests[i].xPos, allNests[i].yPos], allNests[i].yPos * 2));
				}
				else
				{
					allNests[i].addDaySinceEgg();
				}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			allNests.Remove(list[j]);
		}
	}

	public void loadEggs()
	{
		int month = WorldManager.manageWorld.month;
		int num = 3;
	}

	public void getAllFencedOffAnimals()
	{
		fencedOffAnimals.Clear();
		alphaAnimals.Clear();
		saveFencedOffAnimalsEvent.Invoke();
		for (int i = 0; i < loadedChunks.Count; i++)
		{
			loadedChunks[i].fillFencedOffAnimalArray();
		}
	}

	public void placeFencedOffAnimalIntoChunk(FencedOffAnimal animal)
	{
		saveAnimalToChunk(animal.animalId, animal.xPos, animal.yPos, Vector3.zero);
	}

	public void placeHuntingTargetOnMap(int newAnimalId, int xPos, int yPos, int daysRemaining)
	{
		if (NetworkNavMesh.nav.doesPositionHaveNavChunk(xPos, yPos))
		{
			new AnimalsSaved(newAnimalId, xPos, yPos, daysRemaining).spawnAnimal();
		}
		else
		{
			saveAnimalToChunk(newAnimalId, xPos, yPos, Vector3.zero, daysRemaining);
		}
	}

	public void populateFishingTables()
	{
		List<InventoryItem> list = new List<InventoryItem>();
		List<InventoryItem> list2 = new List<InventoryItem>();
		List<InventoryItem> list3 = new List<InventoryItem>();
		List<InventoryItem> list4 = new List<InventoryItem>();
		List<InventoryItem> list5 = new List<InventoryItem>();
		List<InventoryItem> list6 = new List<InventoryItem>();
		List<InventoryItem> list7 = new List<InventoryItem>();
		List<InventoryItem> list8 = new List<InventoryItem>();
		List<InventoryItem> list9 = new List<InventoryItem>();
		List<InventoryItem> list10 = new List<InventoryItem>();
		List<InventoryItem> list11 = new List<InventoryItem>();
		List<InventoryItem> list12 = new List<InventoryItem>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].fish)
			{
				SeasonAndTime.waterLocation[] myWaterLocation = Inventory.inv.allItems[i].fish.mySeason.myWaterLocation;
				for (int j = 0; j < myWaterLocation.Length; j++)
				{
					switch (myWaterLocation[j])
					{
					case SeasonAndTime.waterLocation.Billabongs:
						list5.Add(Inventory.inv.allItems[i]);
						break;
					case SeasonAndTime.waterLocation.NorthOcean:
						list.Add(Inventory.inv.allItems[i]);
						break;
					case SeasonAndTime.waterLocation.SouthOcean:
						list2.Add(Inventory.inv.allItems[i]);
						break;
					case SeasonAndTime.waterLocation.Mangroves:
						list3.Add(Inventory.inv.allItems[i]);
						break;
					case SeasonAndTime.waterLocation.Rivers:
						list4.Add(Inventory.inv.allItems[i]);
						break;
					}
				}
			}
			else if ((bool)Inventory.inv.allItems[i].bug)
			{
				SeasonAndTime.landLocation[] myLandLocation = Inventory.inv.allItems[i].bug.mySeason.myLandLocation;
				for (int j = 0; j < myLandLocation.Length; j++)
				{
					switch (myLandLocation[j])
					{
					case SeasonAndTime.landLocation.All:
						list6.Add(Inventory.inv.allItems[i]);
						list7.Add(Inventory.inv.allItems[i]);
						list10.Add(Inventory.inv.allItems[i]);
						list8.Add(Inventory.inv.allItems[i]);
						list9.Add(Inventory.inv.allItems[i]);
						break;
					case SeasonAndTime.landLocation.Bushland:
						list6.Add(Inventory.inv.allItems[i]);
						break;
					case SeasonAndTime.landLocation.Pines:
						list10.Add(Inventory.inv.allItems[i]);
						break;
					case SeasonAndTime.landLocation.Plains:
						list8.Add(Inventory.inv.allItems[i]);
						break;
					case SeasonAndTime.landLocation.Tropics:
						list7.Add(Inventory.inv.allItems[i]);
						break;
					case SeasonAndTime.landLocation.Desert:
						list9.Add(Inventory.inv.allItems[i]);
						break;
					}
				}
			}
			else
			{
				if (!Inventory.inv.allItems[i].underwaterCreature)
				{
					continue;
				}
				SeasonAndTime.waterLocation[] myWaterLocation = Inventory.inv.allItems[i].underwaterCreature.mySeason.myWaterLocation;
				foreach (SeasonAndTime.waterLocation waterLocation in myWaterLocation)
				{
					if (waterLocation == SeasonAndTime.waterLocation.NorthOcean || waterLocation == SeasonAndTime.waterLocation.SouthOcean)
					{
						list11.Add(Inventory.inv.allItems[i]);
					}
					else
					{
						list12.Add(Inventory.inv.allItems[i]);
					}
				}
			}
		}
		topicalBugs.populateTable(list7);
		bushlandBugs.populateTable(list6);
		pineLandBugs.populateTable(list10);
		desertBugs.populateTable(list9);
		plainsBugs.populateTable(list8);
		northernOceanFish.populateTable(list);
		southernOceanFish.populateTable(list2);
		riverFish.populateTable(list4);
		mangroveFish.populateTable(list3);
		billabongFish.populateTable(list5);
		underWaterOceanCreatures.populateTable(list11);
		underWaterRiverCreatures.populateTable(list12);
	}
}
