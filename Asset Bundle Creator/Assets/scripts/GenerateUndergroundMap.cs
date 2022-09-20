using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateUndergroundMap : MonoBehaviour
{
	public static GenerateUndergroundMap generate;

	private BiomMap biomGrass = new BiomMap();

	private BiomMap heightBooshLand = new BiomMap();

	private BiomMap minesMap = new BiomMap();

	private BiomMap minesWalls = new BiomMap();

	private BiomMap undergroundPonds = new BiomMap();

	private BiomMap undergroundForest = new BiomMap();

	private BiomMap undergroundForestHeight = new BiomMap();

	private BiomMap undergroundIronVien = new BiomMap();

	public BiomSpawnTable normalUnderGroundBiome;

	public BiomSpawnTable undergroundPondBiome;

	public BiomSpawnTable undergroundIronVienBiom;

	public BiomSpawnTable undergroundForestBiom;

	public bool mineGeneratedToday;

	private List<int[]> multiTiledObjectsPlaceAfterMap = new List<int[]>();

	private MapRand mapRand = new MapRand(0);

	public DungeonScript basicDungeon;

	public DungeonScript basicDungeon2DoorUp;

	public DungeonScript basicDungeon2DoorSide;

	public DungeonScript windingWoodPath;

	public DungeonScript woodPathWithBarrels;

	public DungeonScript barrelRound;

	private void Awake()
	{
		generate = this;
	}

	private void Start()
	{
	}

	public void setUpMineSeedFirstTime()
	{
		if (NetworkMapSharer.share.mineSeed == 0 && NetworkMapSharer.share.tomorrowsMineSeed == 0)
		{
			UnityEngine.Random.InitState(Environment.TickCount);
			NetworkMapSharer.share.NetworkmineSeed = UnityEngine.Random.Range(-32000, 32000) + UnityEngine.Random.Range(-32000, 32000);
			NetworkMapSharer.share.tomorrowsMineSeed = UnityEngine.Random.Range(-32000, 32000) + UnityEngine.Random.Range(-32000, 32000);
		}
	}

	public void generateMineSeedForNewDay()
	{
		NetworkMapSharer.share.NetworkmineSeed = NetworkMapSharer.share.tomorrowsMineSeed;
		NetworkMapSharer.share.tomorrowsMineSeed = UnityEngine.Random.Range(-32000, 32000) + UnityEngine.Random.Range(-32000, 32000);
	}

	public IEnumerator generateNewMinesForDay()
	{
		mapRand = new MapRand(NetworkMapSharer.share.mineSeed);
		yield return StartCoroutine(generateMines());
	}

	public IEnumerator generateMineForClient(int mineSeedIn)
	{
		mapRand = new MapRand(mineSeedIn);
		yield return StartCoroutine(generateMines(true));
		MapStorer.store.getStoredMineMapForConnect();
	}

	private IEnumerator generateMines(bool clientOnConnect = false)
	{
		int pause = 0;
		multiTiledObjectsPlaceAfterMap = new List<int[]>();
		biomGrass = new BiomMap(mapRand);
		biomGrass.randomisePosition();
		heightBooshLand = new BiomMap(mapRand);
		heightBooshLand.randomisePosition();
		minesMap = new BiomMap(mapRand);
		minesMap.randomisePosition();
		minesWalls = new BiomMap(mapRand);
		minesWalls.randomisePosition();
		undergroundPonds = new BiomMap(mapRand);
		undergroundPonds.randomisePosition();
		undergroundForest = new BiomMap(mapRand);
		undergroundForest.randomisePosition();
		undergroundForestHeight = new BiomMap(mapRand);
		undergroundForestHeight.randomisePosition();
		undergroundIronVien = new BiomMap(mapRand);
		undergroundIronVien.randomisePosition();
		minesMap.biomWidth = 15f;
		minesWalls.biomWidth = 25f;
		biomGrass.biomWidth = 4f;
		heightBooshLand.biomWidth = 20f;
		undergroundPonds.biomWidth = 25f;
		undergroundIronVien.biomWidth = 10f;
		int xEntrance = -1;
		int yEntrance = -1;
		int mapSize = WorldManager.manageWorld.getMapSize();
		MapStorer.store.underWorldChangedMap = new bool[200, 200];
		MapStorer.store.underWorldWaterChangedMap = new bool[200, 200];
		MapStorer.store.underWorldHeightChangedMap = new bool[200, 200];
		for (int y = 0; y < mapSize; y++)
		{
			for (int i = 0; i < mapSize; i++)
			{
				if (WorldManager.manageWorld.onTileMap[i, y] == 54)
				{
					xEntrance = i;
					yEntrance = y;
				}
				MapStorer.store.underWorldOnTile[i, y] = -1;
				MapStorer.store.underWorldTileType[i, y] = 0;
				MapStorer.store.underWorldOnTileStatus[i, y] = -1;
				MapStorer.store.underWorldTileTypeStatus[i, y] = 9;
				MapStorer.store.underWorldWaterMap[i, y] = false;
				MapStorer.store.underWorldHeight[i, y] = 1;
				MapStorer.store.underWorldTileType[i, y] = 10;
				if (biomGrass.getNoise(i, y) < 0.4f)
				{
					MapStorer.store.underWorldTileType[i, y] = 9;
				}
				if (i <= 5 || y <= 5 || i >= mapSize - 5 || y >= mapSize - 5)
				{
					MapStorer.store.underWorldOnTile[i, y] = 344;
				}
				else if (undergroundPonds.getNoise(i, y) > 0.7f)
				{
					MapStorer.store.underWorldWaterMap[i, y] = true;
					if (undergroundPonds.getNoise(i, y) > 0.75f)
					{
						MapStorer.store.underWorldHeight[i, y] = -1;
					}
					else if (undergroundPonds.getNoise(i, y) > 0.85f)
					{
						MapStorer.store.underWorldHeight[i, y] = -2;
					}
					else if (undergroundPonds.getNoise(i, y) > 0.91f)
					{
						placeUnderWorldObject(i, y, undergroundIronVienBiom);
						MapStorer.store.underWorldWaterMap[i, y] = false;
					}
					else
					{
						MapStorer.store.underWorldHeight[i, y] = 0;
					}
					if (MapStorer.store.underWorldHeight[i, y] < 0)
					{
						placeUnderWorldObject(i, y, undergroundPondBiome);
					}
				}
				else if (undergroundIronVien.getNoise(i, y) > 0.75f)
				{
					if (undergroundIronVien.getNoise(i, y) > 0.8f)
					{
						placeUnderWorldObject(i, y, undergroundIronVienBiom);
					}
					else if (mapRand.Range(0, 5) >= 4)
					{
						MapStorer.store.underWorldOnTile[i, y] = 29;
						MapStorer.store.underWorldTileType[i, y] = 9;
					}
				}
				else if (minesMap.getNoise(i, y) < 0.48f)
				{
					if (minesMap.getNoise(i, y) < 0.4f)
					{
						MapStorer.store.underWorldOnTile[i, y] = 29;
					}
					else if (mapRand.Range(0, 5) == 2)
					{
						MapStorer.store.underWorldOnTile[i, y] = 29;
					}
				}
				else if (minesWalls.getNoise(i, y) > 0.55f && (double)minesWalls.getNoise(i, y) < 0.6)
				{
					MapStorer.store.underWorldHeight[i, y] = 1;
					if (mapRand.Range(0, 3) <= 1)
					{
						MapStorer.store.underWorldOnTile[i, y] = 29;
					}
					else
					{
						MapStorer.store.underWorldOnTile[i, y] = -1;
					}
				}
				else
				{
					placeUnderWorldObject(i, y, normalUnderGroundBiome);
				}
			}
			if (pause < 100)
			{
				pause++;
				continue;
			}
			pause = 0;
			yield return null;
		}
		for (int j = 0; j < 450; j++)
		{
			int xPos = mapRand.Range(100, WorldManager.manageWorld.getMapSize() - 100);
			int yPos = mapRand.Range(100, WorldManager.manageWorld.getMapSize() - 100);
			generateRoundDungeon(xPos, yPos);
		}
		if (xEntrance != -1 && yEntrance != -1)
		{
			generateMineEntrance(xEntrance, yEntrance);
		}
		for (int k = 0; k < multiTiledObjectsPlaceAfterMap.Count; k++)
		{
			if (WorldManager.manageWorld.allObjects[multiTiledObjectsPlaceAfterMap[k][2]].checkIfMultiTileObjectCanBePlacedUnderGround(multiTiledObjectsPlaceAfterMap[k][0], multiTiledObjectsPlaceAfterMap[k][1], multiTiledObjectsPlaceAfterMap[k][3]))
			{
				WorldManager.manageWorld.allObjects[multiTiledObjectsPlaceAfterMap[k][2]].placeMultiTiledObjectUnderGround(multiTiledObjectsPlaceAfterMap[k][0], multiTiledObjectsPlaceAfterMap[k][1], multiTiledObjectsPlaceAfterMap[k][3]);
			}
		}
		if (clientOnConnect)
		{
			WorldManager.manageWorld.resetAllChunkChangedMaps();
		}
	}

	public void generateMineEntrance(int xPos, int yPos)
	{
		for (int i = -10; i < 10; i++)
		{
			for (int j = -10; j < 10; j++)
			{
				float num = 10f;
				float num2 = (num - (float)(j + 10)) * (num - (float)(j + 10));
				float num3 = (num - (float)(i + 10)) * (num - (float)(i + 10));
				if (Mathf.Sqrt(num2 + num3) / num < 0.8f)
				{
					MapStorer.store.underWorldOnTile[xPos + j, yPos + i] = -1;
					MapStorer.store.underWorldHeight[xPos + j, yPos + i] = 1;
				}
				if (NetworkMapSharer.share.isServer)
				{
					int num4 = Mathf.RoundToInt((xPos + j) / WorldManager.manageWorld.getChunkSize()) * WorldManager.manageWorld.getChunkSize();
					int num5 = Mathf.RoundToInt((yPos + i) / WorldManager.manageWorld.getChunkSize()) * WorldManager.manageWorld.getChunkSize();
					MapStorer.store.underWorldChangedMap[num4 / WorldManager.manageWorld.getChunkSize(), num5 / WorldManager.manageWorld.getChunkSize()] = true;
					MapStorer.store.underworldOnTileChangedMap[num4 / WorldManager.manageWorld.getChunkSize(), num5 / WorldManager.manageWorld.getChunkSize()] = true;
					MapStorer.store.underWorldHeightChangedMap[num4 / WorldManager.manageWorld.getChunkSize(), num5 / WorldManager.manageWorld.getChunkSize()] = true;
					MapStorer.store.underworldTileTypeChangedMap[num4 / WorldManager.manageWorld.getChunkSize(), num5 / WorldManager.manageWorld.getChunkSize()] = true;
				}
				if ((i < 0 || i > 3) && (j < 0 || j > 3))
				{
					if (mapRand.Range(1, 100) < 2)
					{
						MapStorer.store.underWorldOnTile[xPos + j, yPos + i] = 2;
					}
					if (mapRand.Range(1, 50) < 2)
					{
						MapStorer.store.underWorldOnTile[xPos + j, yPos + i] = 56;
						MapStorer.store.underWorldOnTileStatus[xPos + j, yPos + i] = 0;
					}
				}
				MapStorer.store.underWorldTileType[xPos + j, yPos + i] = 10;
				if (biomGrass.getNoise(xPos + j, yPos + i) < 0.4f)
				{
					MapStorer.store.underWorldTileType[xPos + j, yPos + i] = 9;
				}
			}
		}
		WorldManager.manageWorld.allObjects[55].placeMultiTiledObjectUnderGround(xPos, yPos, WorldManager.manageWorld.rotationMap[xPos, yPos]);
	}

	public void generateRandomDungeon(int xPos, int yPos)
	{
		for (int i = 0; i < 9; i++)
		{
			for (int j = 0; j < 9; j++)
			{
				if (MapStorer.store.underWorldTileType[xPos + j, yPos + i] == 11)
				{
					return;
				}
			}
		}
		for (int k = 0; k < 9; k++)
		{
			for (int l = 0; l < 9; l++)
			{
				if (NetworkMapSharer.share.isServer)
				{
					int num = Mathf.RoundToInt((xPos + l) / WorldManager.manageWorld.getChunkSize()) * WorldManager.manageWorld.getChunkSize();
					int num2 = Mathf.RoundToInt((yPos + k) / WorldManager.manageWorld.getChunkSize()) * WorldManager.manageWorld.getChunkSize();
					MapStorer.store.underWorldChangedMap[num / WorldManager.manageWorld.getChunkSize(), num2 / WorldManager.manageWorld.getChunkSize()] = true;
					MapStorer.store.underWorldHeightChangedMap[num / WorldManager.manageWorld.getChunkSize(), num2 / WorldManager.manageWorld.getChunkSize()] = true;
				}
				MapStorer.store.underWorldHeight[xPos + l, yPos + k] = 1;
				if (l == 0 || k == 0 || l == 8 || k == 8)
				{
					if (k == 4 || l == 4)
					{
						MapStorer.store.underWorldOnTile[xPos + l, yPos + k] = 186;
						MapStorer.store.underWorldOnTileStatus[xPos + l, yPos + k] = 0;
						MapStorer.store.underWorldRotationMap[xPos + l, yPos + k] = 1;
					}
					else
					{
						MapStorer.store.underWorldOnTile[xPos + l, yPos + k] = 185;
					}
				}
				else if ((l == 1 && k == 1) || (l == 7 && k == 1) || (l == 1 && k == 7) || (l == 1 && k == 7))
				{
					MapStorer.store.underWorldOnTile[xPos + l, yPos + k] = 28;
				}
				else
				{
					MapStorer.store.underWorldOnTile[xPos + l, yPos + k] = -1;
				}
				MapStorer.store.underWorldTileType[xPos + l, yPos + k] = 11;
				MapStorer.store.underWorldTileTypeStatus[xPos + l, yPos + k] = 10;
			}
		}
	}

	public void generateRoundDungeon(int xPos, int yPos)
	{
		int[,] array = new int[0, 0];
		int num = 11;
		int num2 = mapRand.Range(0, 20);
		if (num2 == 1)
		{
			array = basicDungeon.convertTo2dArray();
		}
		else if (num2 <= 8)
		{
			num = 10;
			array = new int[1, 1] { { -1 } };
		}
		else if (num2 <= 10)
		{
			num = 10;
			array = new int[3, 5]
			{
				{ -1, -1, -1, -1, -1 },
				{ -1, 57, -1, -1, -1 },
				{ -1, -1, -1, 57, -1 }
			};
		}
		else if (num2 <= 11)
		{
			num = 10;
			array = new int[3, 5]
			{
				{ -1, -1, -1, 119, -1 },
				{ -1, -1, 119, -1, -1 },
				{ -1, -1, -1, -1, -1 }
			};
		}
		else
		{
			switch (num2)
			{
			case 12:
				array = basicDungeon2DoorSide.convertTo2dArray();
				break;
			case 13:
				array = basicDungeon2DoorUp.convertTo2dArray();
				break;
			case 14:
				num = 16;
				array = woodPathWithBarrels.convertTo2dArray();
				break;
			case 15:
				num = 16;
				array = windingWoodPath.convertTo2dArray();
				break;
			case 16:
				num = 16;
				array = barrelRound.convertTo2dArray();
				array = getRandomDungeonRot(array);
				break;
			case 18:
				num = 10;
				array = new int[1, 1] { { 317 } };
				break;
			default:
				num = 10;
				array = new int[2, 1]
				{
					{ -1 },
					{ -1 }
				};
				break;
			}
		}
		for (int i = 0; i < array.GetLength(1); i++)
		{
			for (int j = 0; j < array.GetLength(0); j++)
			{
				if (MapStorer.store.underWorldTileType[xPos + j, yPos + i] == 11 || MapStorer.store.underWorldTileType[xPos + j, yPos + i] == 16)
				{
					return;
				}
			}
		}
		for (int k = 0; k < array.GetLength(1); k++)
		{
			for (int l = 0; l < array.GetLength(0); l++)
			{
				if (NetworkMapSharer.share.isServer)
				{
					int num3 = Mathf.RoundToInt((xPos + l) / WorldManager.manageWorld.getChunkSize()) * WorldManager.manageWorld.getChunkSize();
					int num4 = Mathf.RoundToInt((yPos + k) / WorldManager.manageWorld.getChunkSize()) * WorldManager.manageWorld.getChunkSize();
					MapStorer.store.underWorldChangedMap[num3 / WorldManager.manageWorld.getChunkSize(), num4 / WorldManager.manageWorld.getChunkSize()] = true;
					MapStorer.store.underWorldHeightChangedMap[num3 / WorldManager.manageWorld.getChunkSize(), num4 / WorldManager.manageWorld.getChunkSize()] = true;
				}
				int num5 = array[l, k];
				if (num5 != -3)
				{
					MapStorer.store.underWorldHeight[xPos + l, yPos + k] = 1;
					MapStorer.store.underWorldOnTile[xPos + l, yPos + k] = num5;
					if (NetworkMapSharer.share.isServer && num5 == 200)
					{
						MapStorer.store.underWorldOnTileStatus[xPos + l, yPos + k] = 0;
						ContainerManager.manage.generateUndergroundChest(xPos + l, yPos + k);
						MapStorer.store.underWorldRotationMap[xPos + l, yPos + k] = UnityEngine.Random.Range(1, 5);
					}
					MapStorer.store.underWorldTileType[xPos + l, yPos + k] = num;
					MapStorer.store.underWorldTileTypeStatus[xPos + l, yPos + k] = 10;
					if (num5 == 186)
					{
						MapStorer.store.underWorldOnTileStatus[xPos + l, yPos + k] = 0;
						MapStorer.store.underWorldRotationMap[xPos + l, yPos + k] = 1;
					}
					if (num5 == 364)
					{
						MapStorer.store.underWorldRotationMap[xPos + l, yPos + k] = UnityEngine.Random.Range(1, 5);
					}
				}
			}
		}
	}

	public void placeUnderWorldObject(int xPos, int yPos, BiomSpawnTable spawnFrom)
	{
		int biomObject = spawnFrom.getBiomObject(mapRand);
		if (biomObject == -1 || MapStorer.store.underWorldOnTile[xPos, yPos] != -1)
		{
			return;
		}
		if ((bool)WorldManager.manageWorld.allObjects[biomObject].tileObjectGrowthStages)
		{
			MapStorer.store.underWorldOnTileStatus[xPos, yPos] = WorldManager.manageWorld.allObjects[biomObject].tileObjectGrowthStages.objectStages.Length - 1;
		}
		if (WorldManager.manageWorld.allObjects[biomObject].isMultiTileObject())
		{
			if (xPos > 10 && xPos < WorldManager.manageWorld.getMapSize() - 10 && yPos > 10 && yPos < WorldManager.manageWorld.getMapSize() - 10)
			{
				int num = mapRand.Range(1, 4);
				multiTiledObjectsPlaceAfterMap.Add(new int[4] { xPos, yPos, biomObject, num });
			}
			else
			{
				MapStorer.store.underWorldOnTile[xPos, yPos] = -1;
			}
		}
		else
		{
			MapStorer.store.underWorldOnTile[xPos, yPos] = biomObject;
		}
	}

	private int[,] getRandomDungeonRot(int[,] dungeonArray)
	{
		if (mapRand.Range(1, 4) < 2)
		{
			int[,] array = new int[dungeonArray.GetLength(1), dungeonArray.GetLength(0)];
			for (int i = 0; i < array.GetLength(1); i++)
			{
				for (int j = 0; j < array.GetLength(0); j++)
				{
					array[j, i] = dungeonArray[i, j];
				}
			}
			return array;
		}
		return dungeonArray;
	}
}
