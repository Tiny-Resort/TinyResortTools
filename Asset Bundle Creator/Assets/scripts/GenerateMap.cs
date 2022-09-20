using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
	public enum biomNames
	{
		Beach = 0,
		Tropical = 1,
		BillabongWater = 2,
		BillabongBank = 3,
		Bushland = 4,
		Desert = 5,
		PineForest = 6,
		Plains = 7,
		RockyIsland = 8,
		WarmOcean = 9,
		ColdOcean = 10,
		OceanShallows = 11,
		Quary = 12,
		CassowaryNest = 13,
		BunyipDen = 14,
		DingoDen = 15,
		ShellSpace = 16
	}

	public static GenerateMap generate;

	public GameObject jamesLogo;

	private BiomMap biomHeight = new BiomMap();

	private BiomMap biomHeightDif = new BiomMap();

	private BiomMap biomCreek = new BiomMap();

	private BiomMap biomGrass = new BiomMap();

	private BiomMap biomRiver = new BiomMap();

	private BiomMap biomRoad = new BiomMap();

	private BiomMap heighBiomDesert = new BiomMap();

	private BiomMap heightBooshLand = new BiomMap();

	private BiomMap tropicalClimate = new BiomMap();

	private BiomMap heightTropicalClimate = new BiomMap();

	private BiomMap coldClimate = new BiomMap();

	private BiomMap bushLandClimate = new BiomMap();

	private BiomMap billabongMap = new BiomMap();

	private BiomMap desertCliffs = new BiomMap();

	private BiomMap quarys = new BiomMap();

	private BiomMap cassowaryNests = new BiomMap();

	private BiomMap dingoDens = new BiomMap();

	private BiomMap bunyipDens = new BiomMap();

	private BiomMap plantBioms = new BiomMap();

	[Header("Biome item tables--------------")]
	public BiomSpawnTable beachObjects;

	public BiomSpawnTable bushLandObjects;

	public BiomSpawnTable bushLandObjectsDirt;

	public BiomSpawnTable billabongObjects;

	public BiomSpawnTable desertObjects;

	public BiomSpawnTable tropicalObjects;

	public BiomSpawnTable tropicalIslandObjects;

	public BiomSpawnTable oceanObjects;

	public BiomSpawnTable southernOceanObejects;

	public BiomSpawnTable coldObjects;

	public BiomSpawnTable plainsObjects;

	public BiomSpawnTable plainsBottleTreeObjects;

	public BiomSpawnTable desertRockBiomObjects;

	public BiomSpawnTable oreNodeObjects;

	[Header("Animal Den Tables")]
	public BiomSpawnTable cassowaryNestObjects;

	public BiomSpawnTable aroundCassowaryNestObjects;

	public BiomSpawnTable boneObjects;

	public BiomSpawnTable denWallObjects;

	[Header("Biome Grow Back tables --------------")]
	public BiomSpawnTable bushLandGrowBack;

	public BiomSpawnTable coldLandGrowBack;

	public BiomSpawnTable tropicalGrowBack;

	public BiomSpawnTable mangroveGrowback;

	public BiomSpawnTable quaryGrowBack0;

	public BiomSpawnTable quaryGrowBack1;

	public BiomSpawnTable quaryGrowBack2;

	public BiomSpawnTable beachGrowBack;

	[Header("Other Stuff --------------")]
	public InventoryItemLootTable burriedItemTable;

	public TileObject[] buildingsToSpawn;

	private List<int[]> tilesToSpawnBuildingsOn = new List<int[]>();

	private List<int[]> multiTiledObjectsPlaceAfterMap = new List<int[]>();

	private List<int[]> possibleDenPositions = new List<int[]>();

	public int seed;

	public float riverHeight;

	public bool spawnPointFound = true;

	public bool isMapGenerating;

	public Vector3 originalSpawnPoint;

	private MapRand mapRand;

	private bool logoComplete;

	private string riverBiomeName = "River";

	private string[] getBiomeNameForMap = new string[18]
	{
		"Beach", "Tropics", "Billabong", "Billabong", "Bushlands", "Desert", "Pine Forest", "Plains", "Pine Forest", "Ocean",
		"Ocean", "Ocean", "Rough Soil", "Tropics", "Pine Forest", "Bushlands", "Beach", "Unknown"
	};

	private void Awake()
	{
		generate = this;
	}

	private void Start()
	{
		originalSpawnPoint = new Vector3(WorldManager.manageWorld.getMapSize() / 2, 2f, WorldManager.manageWorld.getMapSize() / 2);
		StartCoroutine(startDelay());
	}

	private IEnumerator logoTimer()
	{
		jamesLogo.SetActive(true);
		yield return new WaitForSeconds(3.5f);
		jamesLogo.SetActive(false);
	}

	public IEnumerator startDelay()
	{
		SaveLoad.saveOrLoad.loadingScreen.appear("loading");
		SaveLoad.saveOrLoad.loadingScreen.blackness.setBlack();
		StartCoroutine(logoTimer());
		yield return null;
		yield return null;
		yield return null;
		UnityEngine.Random.InitState(Environment.TickCount);
		seed = UnityEngine.Random.Range(-20000, 20000) + UnityEngine.Random.Range(-20000, 20000);
		yield return StartCoroutine(generateNewMap(seed));
		yield return null;
		yield return null;
		yield return null;
		CameraController.control.newChunkLoader.enabled = true;
		yield return null;
		SaveLoad.saveOrLoad.loadingScreen.completed();
		yield return new WaitForSeconds(1f);
		while (jamesLogo.activeInHierarchy)
		{
			yield return null;
		}
		CameraController.control.wondering.finishLoading();
		SaveLoad.saveOrLoad.loadingScreen.disappear();
	}

	public void onFileLoaded()
	{
		searchMapForSpawnPosAndPlace();
		startChunks();
		if (TownManager.manage.lastSavedPos != Vector3.zero)
		{
			WorldManager.manageWorld.spawnPos.position = TownManager.manage.lastSavedPos;
		}
		else if (TownManager.manage.lastSleptPos != Vector3.zero)
		{
			WorldManager.manageWorld.spawnPos.position = TownManager.manage.lastSleptPos;
		}
		UnityEngine.Random.InitState(Environment.TickCount);
	}

	public void onClientDisconnect()
	{
		cameraWonderOnMenu.wonder.finishLoading();
	}

	public void startChunks()
	{
		CameraController.control.newChunkLoader.enabled = true;
		CameraController.control.wondering.finishLoading();
	}

	public void searchMapForSpawnPosAndPlace()
	{
		int num = WorldManager.manageWorld.getMapSize() / WorldManager.manageWorld.getChunkSize();
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				if (!WorldManager.manageWorld.chunkChangedMap[j, i])
				{
					continue;
				}
				for (int k = 0; k < 10; k++)
				{
					for (int l = 0; l < 10; l++)
					{
						if (WorldManager.manageWorld.onTileMap[j * 10 + l, i * 10 + k] < 0)
						{
							continue;
						}
						if (WorldManager.manageWorld.onTileMap[j * 10 + l, i * 10 + k] == 44)
						{
							WorldManager.manageWorld.spawnPos.position = new Vector3((j * 10 + l) * 2, WorldManager.manageWorld.heightMap[j, i] + 10, (i * 10 + k) * 2);
							if (WorldManager.manageWorld.rotationMap[j * 10 + l, i * 10 + k] == 1)
							{
								WorldManager.manageWorld.spawnPos.position += new Vector3(0f, 0f, -2f);
							}
							else if (WorldManager.manageWorld.rotationMap[j * 10 + l, i * 10 + k] == 2)
							{
								WorldManager.manageWorld.spawnPos.position += new Vector3(-2f, 0f, 0f);
							}
							else if (WorldManager.manageWorld.rotationMap[j * 10 + l, i * 10 + k] == 3)
							{
								WorldManager.manageWorld.spawnPos.position += new Vector3(0f, 0f, 2f);
							}
							else
							{
								WorldManager.manageWorld.spawnPos.position += new Vector3(2f, 0f, 0f);
							}
							spawnPointFound = true;
							originalSpawnPoint = WorldManager.manageWorld.spawnPos.position;
							continue;
						}
						for (int m = 0; m < buildingsToSpawn.Length; m++)
						{
							if (WorldManager.manageWorld.onTileMap[j * 10 + l, i * 10 + k] == buildingsToSpawn[m].tileObjectId)
							{
								tilesToSpawnBuildingsOn.Add(new int[2]
								{
									j * 10 + l,
									i * 10 + k
								});
							}
						}
					}
				}
			}
		}
	}

	private void placeNPCOnMap()
	{
	}

	private void placeAnimalsOnMap()
	{
	}

	public IEnumerator generateNewMap(int inSeed)
	{
		NewChunkLoader.loader.inside = true;
		mapRand = new MapRand(inSeed);
		multiTiledObjectsPlaceAfterMap = new List<int[]>();
		possibleDenPositions = new List<int[]>();
		resetAllBioms();
		yield return null;
		int mapSize = WorldManager.manageWorld.getMapSize();
		int pauseCheck = 0;
		for (int y = 0; y < mapSize; y++)
		{
			for (int i = 0; i < mapSize; i++)
			{
				WorldManager.manageWorld.tileTypeMap[i, y] = 0;
				WorldManager.manageWorld.onTileMap[i, y] = -1;
				WorldManager.manageWorld.tileTypeStatusMap[i, y] = -1;
				WorldManager.manageWorld.onTileStatusMap[i, y] = -1;
				WorldManager.manageWorld.waterMap[i, y] = false;
				float distanceToCentre = getDistanceToCentre(i, y, mapSize / 2, mapSize / 2);
				float num = biomHeight.getNoise(i, y) * 2f + biomHeightDif.getNoise(i, y) * 2f - distanceToCentre * 6.5f;
				float num2 = getDistanceToCentre(i, y, 500f, 825f) * 2.25f;
				float num3 = getDistanceToCentre(i, y, 825f, 375f) * 1.5f;
				float num4 = getDistanceToCentre(i, y, 200f, 200f) * 1.5f;
				float num5 = getDistanceToCentre(i, y, 800f, 800f) * 1.5f;
				WorldManager.manageWorld.heightMap[i, y] = 1;
				if (num > 0f)
				{
					if (num < 0.23f)
					{
						if (distanceToCentre > 0.2f)
						{
							WorldManager.manageWorld.tileTypeMap[i, y] = 3;
							if (num < 0.08f)
							{
								placeWorldObject(i, y, beachObjects);
							}
						}
						if (distanceToCentre > 0.21f && desertCliffs.getNoise(i, y) < 0.35f && mapRand.Range(0, 3) >= 2)
						{
							placeWorldObject(i, y, desertRockBiomObjects);
						}
						placeABurriedItem(i, y, 1.75f);
					}
					else if (tropicalClimate.getNoise(i, y) - num2 > 0f || num2 < 0.35f)
					{
						if (desertCliffs.getNoise(i, y) < 0.1f && mapRand.Range(0, 3) >= 2)
						{
							placeWorldObject(i, y, desertRockBiomObjects);
						}
						WorldManager.manageWorld.heightMap[i, y] += (int)(heightTropicalClimate.getNoise(i, y) * 9f);
						if (cassowaryNests.getNoise(i, y) > 0.77f)
						{
							if (biomGrass.getNoise(i, y) < 0.77f)
							{
								WorldManager.manageWorld.tileTypeMap[i, y] = 4;
							}
							if (cassowaryNests.getNoise(i, y) > 0.8f)
							{
								possibleDenPositions.Add(new int[2] { i, y });
							}
							else if (biomGrass.getNoise(i, y) < 0.77f)
							{
								placeWorldObject(i, y, aroundCassowaryNestObjects);
							}
						}
						else if (quarys.getNoise(i, y) > 0.8f)
						{
							WorldManager.manageWorld.tileTypeMap[i, y] = 18;
							placeWorldObject(i, y, oreNodeObjects);
						}
						else if (biomGrass.getNoise(i, y) < 0.77f)
						{
							WorldManager.manageWorld.tileTypeMap[i, y] = 4;
							placeWorldObject(i, y, tropicalObjects);
						}
						placeABurriedItem(i, y, 0.75f);
					}
					else if (bushLandClimate.getNoise(i, y) - num3 > 0f || num3 < 0.35f)
					{
						if (desertCliffs.getNoise(i, y) < 0.25f && mapRand.Range(0, 3) >= 2)
						{
							placeWorldObject(i, y, desertRockBiomObjects);
						}
						WorldManager.manageWorld.heightMap[i, y] += (int)(heightBooshLand.getNoise(i, y) * 3f);
						if (getBillabongHeight(i, y) != -5)
						{
							if (getBillabongHeight(i, y) < 0)
							{
								getRiverGround(distanceToCentre, num, i, y, getBillabongHeight(i, y), num2, num3);
							}
							WorldManager.manageWorld.heightMap[i, y] = getBillabongHeight(i, y);
							if (WorldManager.manageWorld.heightMap[i, y] > 0 && biomGrass.getNoise(i, y) < 0.75f)
							{
								WorldManager.manageWorld.tileTypeMap[i, y] = 1;
								placeWorldObject(i, y, billabongObjects);
							}
						}
						else if (quarys.getNoise(i, y) > 0.8f)
						{
							WorldManager.manageWorld.tileTypeMap[i, y] = 18;
							placeWorldObject(i, y, oreNodeObjects);
						}
						else if (dingoDens.getNoise(i, y) > 0.78f)
						{
							if (dingoDens.getNoise(i, y) > 0.8f)
							{
								WorldManager.manageWorld.heightMap[i, y] = Mathf.Clamp(WorldManager.manageWorld.heightMap[i, y] - 1, 1, 4);
								placeWorldObject(i, y, boneObjects);
							}
							else
							{
								placeWorldObject(i, y, denWallObjects);
							}
						}
						else if (biomGrass.getNoise(i, y) < 0.55f)
						{
							WorldManager.manageWorld.tileTypeMap[i, y] = 1;
							placeWorldObject(i, y, bushLandObjects);
						}
						else
						{
							placeWorldObject(i, y, bushLandObjectsDirt);
						}
						placeABurriedItem(i, y, 0.95f);
					}
					else if (num - distanceToCentre > 0.9f)
					{
						WorldManager.manageWorld.heightMap[i, y] += (int)(heighBiomDesert.getNoise(i, y) * 2f);
						WorldManager.manageWorld.tileTypeMap[i, y] = 5;
						if (quarys.getNoise(i, y) > 0.8f)
						{
							WorldManager.manageWorld.tileTypeMap[i, y] = 18;
							placeWorldObject(i, y, oreNodeObjects);
						}
						else if (heightBooshLand.getNoise(i, y) < 0.35f)
						{
							WorldManager.manageWorld.tileTypeMap[i, y] = 0;
						}
						else
						{
							WorldManager.manageWorld.tileTypeMap[i, y] = 5;
						}
						if (desertCliffs.getNoise(i, y) > 0.65f)
						{
							placeWorldObject(i, y, desertRockBiomObjects);
						}
						else if (plantBioms.getNoise(i, y) > 0.9f || plantBioms.getNoise(i, y) < 0.1f)
						{
							WorldManager.manageWorld.onTileMap[i, y] = 13;
						}
						placeWorldObject(i, y, desertObjects);
						placeABurriedItem(i, y, 3.25f);
					}
					else if (coldClimate.getNoise(i, y) - num4 > 0f || num4 < 0.45f)
					{
						if (desertCliffs.getNoise(i, y) > 0.8f && mapRand.Range(0, 3) >= 2)
						{
							placeWorldObject(i, y, desertRockBiomObjects);
						}
						WorldManager.manageWorld.heightMap[i, y] += (int)(heightTropicalClimate.getNoise(i, y) * 5f + heightBooshLand.getNoise(i, y) * 5f - num4 * 3f);
						if ((double)bunyipDens.getNoise(i, y) > 0.79)
						{
							WorldManager.manageWorld.tileTypeMap[i, y] = 18;
							if (bunyipDens.getNoise(i, y) > 0.82f)
							{
								WorldManager.manageWorld.heightMap[i, y]--;
								placeWorldObject(i, y, boneObjects);
							}
							else
							{
								placeWorldObject(i, y, denWallObjects);
							}
						}
						else if (heightBooshLand.getNoise(i, y) > 0.25f && biomGrass.getNoise(i, y) < 0.7f)
						{
							WorldManager.manageWorld.tileTypeMap[i, y] = 15;
							placeWorldObject(i, y, coldObjects);
						}
						placeABurriedItem(i, y, 1.15f);
					}
					else
					{
						if (desertCliffs.getNoise(i, y) > 0.5f && mapRand.Range(0, 3) >= 2)
						{
							placeWorldObject(i, y, desertRockBiomObjects);
						}
						if (getBillabongHeight(i, y) != -5)
						{
							if (getBillabongHeight(i, y) < 0)
							{
								getRiverGround(distanceToCentre, num, i, y, getBillabongHeight(i, y), num2, num3);
							}
							WorldManager.manageWorld.heightMap[i, y] = getBillabongHeight(i, y);
						}
						else if (biomGrass.getNoise(i, y) > 0.5f && i < mapSize / 2)
						{
							WorldManager.manageWorld.tileTypeMap[i, y] = 1;
							placeWorldObject(i, y, plainsObjects);
						}
						else if (biomGrass.getNoise(i, y) < 0.65f && i >= mapSize / 2)
						{
							WorldManager.manageWorld.tileTypeMap[i, y] = 1;
							placeWorldObject(i, y, plainsBottleTreeObjects);
						}
						placeABurriedItem(i, y, 1.45f);
					}
					int posRiverHeight = getPosRiverHeight(i, y, distanceToCentre);
					if (posRiverHeight != -5)
					{
						if (posRiverHeight != -4 && WorldManager.manageWorld.heightMap[i, y] > posRiverHeight)
						{
							WorldManager.manageWorld.heightMap[i, y] = posRiverHeight;
						}
						if (posRiverHeight == -4 && WorldManager.manageWorld.heightMap[i, y] > 1)
						{
							WorldManager.manageWorld.heightMap[i, y] = 1;
						}
						if (posRiverHeight != -4)
						{
							if (posRiverHeight <= 0)
							{
								WorldManager.manageWorld.waterMap[i, y] = true;
							}
							getRiverGround(distanceToCentre, num, i, y, posRiverHeight, num2, num3);
						}
						if (!(bushLandClimate.getNoise(i, y) - num3 > 0f) && !(num3 < 0.35f) && (tropicalClimate.getNoise(i, y) - num2 > 0f || num2 < 0.35f))
						{
							getRiverGround(distanceToCentre, num, i, y, posRiverHeight, num2, num3);
						}
						placeABurriedItem(i, y, 12.5f);
					}
					if ((double)biomRoad.getNoise(i, y) > 0.47 && (double)biomRoad.getNoise(i, y) < 0.53)
					{
						WorldManager.manageWorld.heightMap[i, y] = Mathf.Clamp(WorldManager.manageWorld.heightMap[i, y], -5, 1);
					}
					continue;
				}
				if (desertCliffs.getNoise(i, y) > 0.65f && mapRand.Range(0, 3) >= 2)
				{
					placeWorldObject(i, y, desertRockBiomObjects);
				}
				int posRiverHeight2 = getPosRiverHeight(i, y, distanceToCentre);
				if (num > -0.065f)
				{
					WorldManager.manageWorld.heightMap[i, y] = 0;
				}
				else if (num > -0.15f)
				{
					WorldManager.manageWorld.heightMap[i, y] = -1;
				}
				else
				{
					WorldManager.manageWorld.heightMap[i, y] = -1 + (int)(heightBooshLand.getNoise(i, y) * -4f);
				}
				if (posRiverHeight2 != -5 && posRiverHeight2 < 0 && posRiverHeight2 != -4 && WorldManager.manageWorld.heightMap[i, y] > posRiverHeight2)
				{
					WorldManager.manageWorld.heightMap[i, y] = posRiverHeight2;
				}
				if (coldClimate.getNoise(i, y) - num4 * 3.5f > 0f)
				{
					WorldManager.manageWorld.tileTypeMap[i, y] = 3;
					if (heightTropicalClimate.getNoise(i, y) > 0.65f)
					{
						WorldManager.manageWorld.heightMap[i, y] = 4;
						if (heightBooshLand.getNoise(i, y) > 0.25f)
						{
							if (biomGrass.getNoise(i, y) < 0.7f)
							{
								WorldManager.manageWorld.tileTypeMap[i, y] = 15;
								placeWorldObject(i, y, coldObjects);
							}
							else
							{
								WorldManager.manageWorld.tileTypeMap[i, y] = 0;
							}
						}
					}
				}
				else if (heightTropicalClimate.getNoise(i, y) - num5 * 3.8f > 0f)
				{
					if (heightTropicalClimate.getNoise(i, y) > 0.65f)
					{
						if (heightTropicalClimate.getNoise(i, y) > 0.85f)
						{
							WorldManager.manageWorld.heightMap[i, y] = 2;
							WorldManager.manageWorld.tileTypeMap[i, y] = 4;
							placeWorldObject(i, y, tropicalIslandObjects);
						}
						else
						{
							if (heightTropicalClimate.getNoise(i, y) > 0.75f)
							{
								WorldManager.manageWorld.heightMap[i, y] = 1;
							}
							else if (heightTropicalClimate.getNoise(i, y) > 0.7f)
							{
								WorldManager.manageWorld.heightMap[i, y] = 0;
							}
							else
							{
								WorldManager.manageWorld.heightMap[i, y] = -1;
							}
							WorldManager.manageWorld.tileTypeMap[i, y] = 3;
						}
					}
				}
				else if (distanceToCentre < 0.2f)
				{
					getRiverGround(distanceToCentre, num, i, y, posRiverHeight2, num2, num3);
				}
				else
				{
					WorldManager.manageWorld.tileTypeMap[i, y] = 3;
				}
				if (WorldManager.manageWorld.heightMap[i, y] < 0)
				{
					if (tropicalClimate.getNoise(i, y) - num2 > 0f || num2 < 0.35f)
					{
						if (biomGrass.getNoise(i, y) < 0.45f)
						{
							placeWorldObject(i, y, oceanObjects);
						}
					}
					else if (biomGrass.getNoise(i, y) > 0.55f)
					{
						placeWorldObject(i, y, southernOceanObejects);
					}
				}
				if (WorldManager.manageWorld.heightMap[i, y] <= 0)
				{
					WorldManager.manageWorld.waterMap[i, y] = true;
				}
			}
			if (pauseCheck > 50)
			{
				SaveLoad.saveOrLoad.loadingScreen.showPercentage((float)y / 1000f / 2f);
				pauseCheck = 0;
				yield return null;
			}
			else
			{
				pauseCheck++;
			}
		}
		placeStartingDock();
		placeTeleporter(288, 0, 1);
		placeTeleporter(289, 1, 0);
		placeTeleporter(290, 0, -1);
		placeTeleporter(291, -1, 0);
		for (int j = 0; j < multiTiledObjectsPlaceAfterMap.Count; j++)
		{
			if (WorldManager.manageWorld.allObjects[multiTiledObjectsPlaceAfterMap[j][2]].checkIfMultiTileObjectCanBePlacedMapGenerate(multiTiledObjectsPlaceAfterMap[j][0], multiTiledObjectsPlaceAfterMap[j][1], multiTiledObjectsPlaceAfterMap[j][3]))
			{
				WorldManager.manageWorld.allObjects[multiTiledObjectsPlaceAfterMap[j][2]].placeMultiTiledObject(multiTiledObjectsPlaceAfterMap[j][0], multiTiledObjectsPlaceAfterMap[j][1], multiTiledObjectsPlaceAfterMap[j][3]);
			}
		}
		placeAnimalDens();
		WorldManager.manageWorld.chunksToRefresh.Clear();
		NewChunkLoader.loader.inside = false;
		WorldManager.manageWorld.resetAllChunkChangedMaps();
		UnityEngine.Random.InitState(Environment.TickCount);
	}

	public void placeWorldObject(int xPos, int yPos, BiomSpawnTable spawnFrom)
	{
		int biomObject = spawnFrom.getBiomObject(mapRand);
		if (biomObject != -1 && WorldManager.manageWorld.onTileMap[xPos, yPos] == -1)
		{
			if ((bool)WorldManager.manageWorld.allObjects[biomObject].tileObjectGrowthStages)
			{
				WorldManager.manageWorld.onTileStatusMap[xPos, yPos] = WorldManager.manageWorld.allObjects[biomObject].tileObjectGrowthStages.objectStages.Length - 1;
			}
			if (WorldManager.manageWorld.allObjects[biomObject].isMultiTileObject())
			{
				multiTiledObjectsPlaceAfterMap.Add(new int[4]
				{
					xPos,
					yPos,
					biomObject,
					mapRand.Range(1, 4)
				});
			}
			else
			{
				WorldManager.manageWorld.onTileMap[xPos, yPos] = biomObject;
			}
		}
	}

	public void placeAnimalDens()
	{
		for (int num = possibleDenPositions.Count - 1; num >= 0; num--)
		{
			int num2 = possibleDenPositions[num][0];
			int num3 = possibleDenPositions[num][1];
			int num4 = checkBiomType(num2, num3);
			int num5 = -1;
			if (num4 == 13)
			{
				num5 = cassowaryNestObjects.getBiomObject();
			}
			bool flag = false;
			for (int i = -5; i < 6; i++)
			{
				for (int j = -5; j < 6; j++)
				{
					if (checkBiomType(Mathf.Clamp(num2 + j, 0, WorldManager.manageWorld.getMapSize() - 1), Mathf.Clamp(num3 + i, 0, WorldManager.manageWorld.getMapSize() - 1)) == num4 && WorldManager.manageWorld.onTileMap[Mathf.Clamp(num2 + j, 0, WorldManager.manageWorld.getMapSize() - 1), Mathf.Clamp(num3 + i, 0, WorldManager.manageWorld.getMapSize() - 1)] == num5)
					{
						flag = true;
						break;
					}
					if (flag)
					{
						break;
					}
				}
			}
			while (!flag)
			{
				for (int k = -2; k < 2; k++)
				{
					for (int l = -2; l < 2; l++)
					{
						if (checkBiomType(Mathf.Clamp(num2 + l, 0, WorldManager.manageWorld.getMapSize() - 1), Mathf.Clamp(num3 + k, 0, WorldManager.manageWorld.getMapSize() - 1)) == num4 && mapRand.Range(0, 25) == 2)
						{
							WorldManager.manageWorld.onTileMap[Mathf.Clamp(num2 + l, 0, WorldManager.manageWorld.getMapSize() - 1), Mathf.Clamp(num3 + k, 0, WorldManager.manageWorld.getMapSize() - 1)] = num5;
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
		}
	}

	public int getPosRiverHeight(int x, int y, float distanceFromCentre)
	{
		float noise = biomRiver.getNoise(x, y);
		float num = distanceFromCentre / 35.5f + 0.014f;
		if ((double)noise > 0.49 - (double)num && (double)noise < 0.56 + (double)num)
		{
			if ((double)noise > 0.515 - (double)num && (double)noise < 0.535 + (double)num)
			{
				return -2;
			}
			if ((double)noise > 0.51 - (double)num && (double)noise < 0.54 + (double)num)
			{
				return -1;
			}
			if ((double)noise > 0.505 - (double)num && (double)noise < 0.545 + (double)num)
			{
				return 0;
			}
			if ((double)noise > 0.5 - (double)num && (double)noise < 0.55 + (double)num)
			{
				return 1;
			}
			return -4;
		}
		return -5;
	}

	public void placeABurriedItem(int x, int y, float percentChance, InventoryItemLootTable useTable = null)
	{
		if (mapRand.Range(0f, 100f) < percentChance && WorldManager.manageWorld.onTileMap[x, y] == -1)
		{
			WorldManager.manageWorld.onTileMap[x, y] = 30;
		}
	}

	public int getBillabongHeight(int x, int y)
	{
		float noise = billabongMap.getNoise(x, y);
		int num = -5;
		if (noise > 0.65f)
		{
			if (!((double)noise < 0.7))
			{
				num = (((double)noise < 0.73) ? Mathf.Clamp(WorldManager.manageWorld.heightMap[x, y] - 1, 1, 4) : (((double)noise < 0.76) ? Mathf.Clamp(WorldManager.manageWorld.heightMap[x, y] - 2, 1, 4) : ((noise < 0.79f) ? Mathf.Clamp(WorldManager.manageWorld.heightMap[x, y] - 4, 0, 4) : ((!(noise < 0.81f)) ? Mathf.Clamp(WorldManager.manageWorld.heightMap[x, y] - 8, -2, 4) : Mathf.Clamp(WorldManager.manageWorld.heightMap[x, y] - 6, -1, 4)))));
			}
			else
			{
				num = Mathf.Clamp(WorldManager.manageWorld.heightMap[x, y] - 1, 1, 4);
				if (mapRand.Range(0, 8) < 1 && WorldManager.manageWorld.tileTypeMap[x, y] != 1)
				{
				}
			}
			if (num <= 0)
			{
				WorldManager.manageWorld.waterMap[x, y] = true;
			}
			if (num < 1 && num < 0 && biomGrass.getNoise(x + WorldManager.manageWorld.getMapSize(), y + WorldManager.manageWorld.getMapSize()) > 0.65f && mapRand.Range(0, 5) < 3)
			{
				WorldManager.manageWorld.onTileMap[x, y] = 121;
			}
		}
		return num;
	}

	public void getRiverGround(float distanceToCenter, float mapHeight, int x, int y, float height, float tropicalCentre, float bushlandCentre)
	{
		if (mapHeight < 0.35f && distanceToCenter > 0.2f)
		{
			WorldManager.manageWorld.onTileMap[x, y] = -1;
			WorldManager.manageWorld.tileTypeMap[x, y] = 3;
			return;
		}
		if (tropicalCentre < 0.65f)
		{
			WorldManager.manageWorld.tileTypeMap[x, y] = 14;
			if (height >= -1f || height == -4f)
			{
				if (mapRand.Range(0, 18) < 2)
				{
					WorldManager.manageWorld.onTileMap[x, y] = 130;
				}
				else if (mapRand.Range(0, 10) < 2)
				{
					WorldManager.manageWorld.onTileMap[x, y] = 131;
				}
				else if (mapRand.Range(0, 8) < 3 && biomGrass.getNoise(x, y) < 0.35f)
				{
					WorldManager.manageWorld.onTileMap[x, y] = 133;
				}
				else
				{
					WorldManager.manageWorld.onTileMap[x, y] = -1;
				}
			}
			else if (height != -4f)
			{
				if (height == -2f && biomGrass.getNoise(x, y) < 0.35f)
				{
					WorldManager.manageWorld.onTileMap[x, y] = 133;
				}
				else
				{
					WorldManager.manageWorld.onTileMap[x, y] = -1;
				}
			}
			return;
		}
		if (bushLandClimate.getNoise(x, y) - bushlandCentre > 0f)
		{
			WorldManager.manageWorld.tileTypeMap[x, y] = 2;
			if (height == -2f && biomGrass.getNoise(x, y) < 0.35f)
			{
				if (mapRand.Range(0, 4) < 3)
				{
					WorldManager.manageWorld.onTileMap[x, y] = 133;
				}
				else
				{
					WorldManager.manageWorld.onTileMap[x, y] = -1;
				}
			}
			else if (mapRand.Range(0, 25) < 1)
			{
				WorldManager.manageWorld.onTileMap[x, y] = 2;
			}
			else
			{
				WorldManager.manageWorld.onTileMap[x, y] = -1;
			}
			return;
		}
		if (mapHeight - distanceToCenter > 0.9f)
		{
			WorldManager.manageWorld.tileTypeMap[x, y] = 2;
			if (height == -2f && biomGrass.getNoise(x, y) < 0.35f)
			{
				if (mapRand.Range(0, 4) < 3)
				{
					WorldManager.manageWorld.onTileMap[x, y] = 133;
				}
				else
				{
					WorldManager.manageWorld.onTileMap[x, y] = -1;
				}
			}
			else if (mapRand.Range(0, 25) < 1)
			{
				WorldManager.manageWorld.onTileMap[x, y] = 2;
			}
			else
			{
				WorldManager.manageWorld.onTileMap[x, y] = -1;
			}
			return;
		}
		WorldManager.manageWorld.tileTypeMap[x, y] = 2;
		if (height == -2f && biomGrass.getNoise(x, y) < 0.35f)
		{
			if (mapRand.Range(0, 4) < 3)
			{
				WorldManager.manageWorld.onTileMap[x, y] = 133;
			}
			else
			{
				WorldManager.manageWorld.onTileMap[x, y] = -1;
			}
		}
		else if (mapRand.Range(0, 25) < 1)
		{
			WorldManager.manageWorld.onTileMap[x, y] = 2;
		}
		else
		{
			WorldManager.manageWorld.onTileMap[x, y] = -1;
		}
	}

	public void placeStartingDock()
	{
		int num = WorldManager.manageWorld.getMapSize() / 2;
		int i = WorldManager.manageWorld.getMapSize() / 2;
		bool flag = false;
		for (; WorldManager.manageWorld.tileTypeMap[i, num] != 3; i++)
		{
			if (i == WorldManager.manageWorld.getMapSize() - 1)
			{
				num++;
				i = 500;
			}
		}
		while (!flag)
		{
			if (i == WorldManager.manageWorld.getMapSize() - 1)
			{
				num++;
				i = 500;
			}
			if (WorldManager.manageWorld.heightMap[i, num] == 1 && WorldManager.manageWorld.tileTypeMap[i, num] == 3 && WorldManager.manageWorld.heightMap[i + 1, num] <= 0)
			{
				flag = true;
			}
			else
			{
				i++;
			}
		}
		WorldManager.manageWorld.allObjects[215].placeMultiTiledObject(i, num, 2);
		WorldManager.manageWorld.onTileStatusMap[i, num] = 0;
		TownManager.manage.startingDockPosition = new int[2] { i, num };
		WorldManager.manageWorld.spawnPos.position = new Vector3(i * 2, 4f, num * 2 + 4);
		if (TownManager.manage.firstConnect)
		{
			tilesToSpawnBuildingsOn.Add(new int[2] { i, num });
			placeAllBuildings();
			WorldManager.manageWorld.spawnPos.position = TownManager.manage.allShopFloors[13].transform.Find("FirstConnectSpawnPos").position;
		}
		originalSpawnPoint = new Vector3(i * 2, 4f, num * 2 + 4);
	}

	public void placeTeleporter(int tileId, int xDir, int yDir)
	{
		int num = WorldManager.manageWorld.getMapSize() / 2 + yDir * 250;
		int num2 = WorldManager.manageWorld.getMapSize() / 2 + xDir * 250;
		bool flag = false;
		int num3 = 1;
		while (!flag)
		{
			if (num2 <= 0 || num <= 0 || num >= WorldManager.manageWorld.getMapSize() - 1 || num2 >= WorldManager.manageWorld.getMapSize() - 1)
			{
				num = WorldManager.manageWorld.getMapSize() / 2 + yDir * 250;
				num2 = WorldManager.manageWorld.getMapSize() / 2 + xDir * 250;
				if (xDir == 0)
				{
					num2 += num3;
				}
				if (yDir == 0)
				{
					num += num3;
				}
				num3++;
				continue;
			}
			if (WorldManager.manageWorld.heightMap[num2, num] >= 1 && WorldManager.manageWorld.onTileMap[num2, num] >= -1 && WorldManager.manageWorld.onTileMap[num2, num] != 215)
			{
				flag = true;
				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < 2; j++)
					{
						if (WorldManager.manageWorld.onTileMap[num2 + i, num + j] > -1 && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[num2 + i, num + j]].isWood && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[num2 + i, num + j]].isHardWood && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[num2 + i, num + j]].isSmallPlant && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[num2 + i, num + j]].isStone && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[num2 + i, num + j]].isHardStone)
						{
							flag = false;
							break;
						}
					}
				}
			}
			if (!flag)
			{
				num2 += xDir;
				num += yDir;
			}
		}
		WorldManager.manageWorld.allObjects[tileId].placeMultiTiledObject(num2, num, 0);
		WorldManager.manageWorld.allObjectSettings[tileId].flattenPosUnderMultitiledObject(num2, num, WorldManager.manageWorld.heightMap[num2, num], 0);
		WorldManager.manageWorld.onTileStatusMap[num2, num] = 1;
		switch (xDir)
		{
		case -1:
			TownManager.manage.westTowerPos = new int[2] { num2, num };
			break;
		default:
			TownManager.manage.eastTowerPos = new int[2] { num2, num };
			break;
		case 0:
			if (yDir == -1)
			{
				TownManager.manage.southTowerPos = new int[2] { num2, num };
			}
			else
			{
				TownManager.manage.northTowerPos = new int[2] { num2, num };
			}
			break;
		}
	}

	public float getDistanceToCentre(int x, int y, float xHalfPoint, float yHalfPoint)
	{
		return Mathf.Sqrt((xHalfPoint - (float)x) * (xHalfPoint - (float)x) + (yHalfPoint - (float)y) * (yHalfPoint - (float)y)) / (float)WorldManager.manageWorld.getMapSize();
	}

	public bool riverNoiseOrOceanNoise(Vector3 position)
	{
		float num = position.x / 2f;
		float num2 = position.z / 2f;
		float distanceToCentre = getDistanceToCentre((int)num, (int)num2, 1000f, 1000f);
		float num3 = biomHeight.getNoise(num, num2) * 2f + biomHeightDif.getNoise(num, num2) * 2f - distanceToCentre * 6.5f;
		if (num3 > 0f)
		{
			if (num3 < 0.23f && distanceToCentre > 0.2f)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public float getRiverNoise(Vector3 position)
	{
		float noise = biomRiver.getNoise(position.x / 2f, position.z / 2f);
		if (noise > 0.47f && noise < 0.58f)
		{
			if ((double)noise > 0.515 && (double)noise < 0.535)
			{
				return 0.6f;
			}
			if ((double)noise > 0.51 && (double)noise < 0.54)
			{
				return 0.6f;
			}
			if ((double)noise > 0.505 && (double)noise < 0.545)
			{
				return 0.5f;
			}
			if ((double)noise > 0.5 && (double)noise < 0.55)
			{
				return 0.4f;
			}
			return 0.3f;
		}
		return 0f;
	}

	public float getRiverHeight(int x, int y)
	{
		return biomRiver.getNoise(x, y);
	}

	public void placeAllBuildings()
	{
		foreach (int[] item in tilesToSpawnBuildingsOn)
		{
			int num = WorldManager.manageWorld.onTileMap[item[0], item[1]];
			NetworkMapSharer.share.requestInterior(item[0], item[1]);
		}
		if (tilesToSpawnBuildingsOn.Count > 1)
		{
			ConnectToBoatEntrance.connect.setUpBoatFloor();
		}
	}

	public int getTileTemperature(int x, int y)
	{
		int seasonAverageTemp = RealWorldTimeLight.time.seasonAverageTemp;
		float num = getDistanceToCentre(x, y, 500f, 825f) * -18f + 18f;
		float num2 = getDistanceToCentre(x, y, 200f, 200f) * 18f + -18f;
		return (int)((float)seasonAverageTemp + num + num2);
	}

	public int getPlaceTemperature(Vector3 position)
	{
		int x = (int)position.x / 2;
		int y = (int)position.z / 2;
		return getTileTemperature(x, y);
	}

	public string getBiomeNameById(int id)
	{
		biomNames biomNames = (biomNames)id;
		return SplitCamelCase(biomNames.ToString()).ToLower();
	}

	public string getBiomeNameUnderMapCursor(int xPos, int yPos)
	{
		if (!WorldManager.manageWorld.isPositionOnMap(xPos, yPos))
		{
			return "Unknown";
		}
		int value = checkBiomType(xPos, yPos);
		if (WorldManager.manageWorld.waterMap[xPos, yPos] && WorldManager.manageWorld.heightMap[xPos, yPos] < 0 && WorldManager.manageWorld.waterMap[xPos, yPos] && WorldManager.manageWorld.tileTypeMap[xPos, yPos] != 3)
		{
			if (WorldManager.manageWorld.tileTypeMap[xPos, yPos] == 14)
			{
				return "Mangroves";
			}
			if (getBiomeNameForMap[Mathf.Clamp(value, 0, 17)] != "Billabong")
			{
				return riverBiomeName;
			}
		}
		return getBiomeNameForMap[Mathf.Clamp(value, 0, 17)];
	}

	public static string SplitCamelCase(string input)
	{
		return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
	}

	public int checkBiomType(int x, int y)
	{
		int mapSize = WorldManager.manageWorld.getMapSize();
		float distanceToCentre = getDistanceToCentre(x, y, mapSize / 2, mapSize / 2);
		float num = biomHeight.getNoise(x, y) * 2f + biomHeightDif.getNoise(x, y) * 2f - distanceToCentre * 6.5f;
		float num2 = getDistanceToCentre(x, y, 500f, 825f) * 2.25f;
		float num3 = getDistanceToCentre(x, y, 825f, 375f) * 1.5f;
		float num4 = getDistanceToCentre(x, y, 200f, 200f) * 1.5f;
		getDistanceToCentre(x, y, 1800f, 1800f);
		if (num > 0f)
		{
			if (num < 0.23f)
			{
				if (distanceToCentre > 0.2f && num < 0.08f)
				{
					return 16;
				}
				return 0;
			}
			if (tropicalClimate.getNoise(x, y) - num2 > 0f || num2 < 0.35f)
			{
				if (cassowaryNests.getNoise(x, y) > 0.8f)
				{
					return 13;
				}
				if (quarys.getNoise(x, y) > 0.8f)
				{
					return 12;
				}
				return 1;
			}
			if (bushLandClimate.getNoise(x, y) - num3 > 0f || num3 < 0.35f)
			{
				if (billabongMap.getNoise(x, y) > 0.65f)
				{
					if (WorldManager.manageWorld.heightMap[x, y] < 0 && WorldManager.manageWorld.waterMap[x, y])
					{
						return 2;
					}
					return 3;
				}
				if (quarys.getNoise(x, y) > 0.8f)
				{
					return 12;
				}
				biomGrass.getNoise(x, y);
				float num5 = 0.55f;
				if (dingoDens.getNoise(x, y) > 0.78f)
				{
					return 15;
				}
				return 4;
			}
			if (num - distanceToCentre > 0.9f)
			{
				if (quarys.getNoise(x, y) > 0.8f)
				{
					return 12;
				}
				return 5;
			}
			if (coldClimate.getNoise(x, y) - num4 > 0f || num4 < 0.45f)
			{
				if (bunyipDens.getNoise(x, y) > 0.79f)
				{
					return 14;
				}
				return 6;
			}
			if (billabongMap.getNoise(x, y) > 0.65f)
			{
				if (WorldManager.manageWorld.heightMap[x, y] < 0 && WorldManager.manageWorld.waterMap[x, y])
				{
					return 2;
				}
				return 3;
			}
			biomGrass.getNoise(x, y);
			float num6 = 0.5f;
			return 7;
		}
		if (coldClimate.getNoise(x, y) - num4 * 3.5f > 0f)
		{
			return 8;
		}
		if (WorldManager.manageWorld.heightMap[x, y] < 0)
		{
			if (tropicalClimate.getNoise(x, y) - num2 > 0f || num2 < 0.35f)
			{
				if (biomGrass.getNoise(x, y) < 0.45f)
				{
					return 9;
				}
			}
			else if (biomGrass.getNoise(x, y) > 0.55f)
			{
				return 10;
			}
		}
		return 11;
	}

	public void resetAllBioms()
	{
		biomHeight = new BiomMap(mapRand);
		biomHeight.randomisePosition();
		biomHeightDif = new BiomMap(mapRand);
		biomHeightDif.randomisePosition();
		biomCreek = new BiomMap(mapRand);
		biomCreek.randomisePosition();
		biomGrass = new BiomMap(mapRand);
		biomGrass.randomisePosition();
		biomRiver = new BiomMap(mapRand);
		biomRiver.randomisePosition();
		biomRoad = new BiomMap(mapRand);
		biomRoad.randomisePosition();
		heighBiomDesert = new BiomMap(mapRand);
		heighBiomDesert.randomisePosition();
		heightBooshLand = new BiomMap(mapRand);
		heightBooshLand.randomisePosition();
		heightTropicalClimate = new BiomMap(mapRand);
		heightTropicalClimate.randomisePosition();
		tropicalClimate = new BiomMap(mapRand);
		tropicalClimate.randomisePosition();
		bushLandClimate = new BiomMap(mapRand);
		bushLandClimate.randomisePosition();
		coldClimate = new BiomMap(mapRand);
		coldClimate.randomisePosition();
		billabongMap = new BiomMap(mapRand);
		billabongMap.randomisePosition();
		quarys = new BiomMap(mapRand);
		quarys.randomisePosition();
		cassowaryNests = new BiomMap(mapRand);
		cassowaryNests.randomisePosition();
		dingoDens = new BiomMap(mapRand);
		dingoDens.randomisePosition();
		bunyipDens = new BiomMap(mapRand);
		bunyipDens.randomisePosition();
		plantBioms = new BiomMap(mapRand);
		plantBioms.randomisePosition();
		plantBioms.biomWidth = 6.3f;
		desertCliffs = new BiomMap(mapRand);
		desertCliffs.randomisePosition();
		billabongMap.biomWidth = 44.2f;
		quarys.biomWidth = 20.3f;
		cassowaryNests.biomWidth = 30f;
		dingoDens.biomWidth = 19.5f;
		bunyipDens.biomWidth = 19.5f;
		desertCliffs.biomWidth = 15f;
		biomHeight.biomWidth = 115.35f;
		biomHeightDif.biomWidth = 50.1f;
		tropicalClimate.biomWidth = 35.4f;
		bushLandClimate.biomWidth = 44.6f;
		coldClimate.biomWidth = 38f;
		biomCreek.biomWidth = 123.3f;
		biomGrass.biomWidth = 4f;
		biomRiver.biomWidth = 213.86665f;
		biomRiver.biomScale = 2.35f;
		biomRoad.biomWidth = 213.86665f;
		biomRoad.biomScale = 2.35f;
		heighBiomDesert.biomWidth = 15f;
		heightBooshLand.biomWidth = 20f;
		heightTropicalClimate.biomWidth = 25f;
	}
}
