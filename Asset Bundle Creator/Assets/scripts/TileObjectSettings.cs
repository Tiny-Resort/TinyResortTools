using UnityEngine;

public class TileObjectSettings : MonoBehaviour
{
	public int tileObjectId;

	[Header("Death stuff --------------")]
	public float fullHealth = 100f;

	public int changeToTileObjectOnDeath = -1;

	public ASound deathSound;

	[Header("Drops On Death --------------")]
	public bool dropsStatusNumberOnDeath;

	public InventoryItem dropsItemOnDeath;

	public InventoryItemLootTable dropFromLootTable;

	public bool onlyDropWhenGrown;

	[Header("Death Object (Not INV item) --------------")]
	public GameObject dropsObjectOnDeath;

	public GameObject spawnCarryableOnDeath;

	public float carryableChance;

	[Header("Death Particles --------------")]
	public int deathParticle = -1;

	public int particlesPerPositon = 5;

	[Header("Damage Stuff --------------")]
	public ASound damageSound;

	public int damageParticle = -1;

	public int damageParticlesPerPosition;

	[Header("Furniture Settings --------------")]
	public bool canBePlacedOnTopOfFurniture;

	[Header("Settings --------------")]
	public bool getRotationFromMap;

	public bool walkable = true;

	public bool isFence;

	public bool canBePickedUp;

	public bool pickUpRequiresEmptyPocket;

	public bool hasRandomRotation = true;

	public bool hasRandomScale;

	public bool isMultiTileObject;

	public int xSize;

	public int ySize;

	[Header("Other Settings --------------")]
	public bool isFlowerBed;

	[Header("Damageable type --------------")]
	public bool isGrass;

	public bool isWood;

	public bool isHardWood;

	public bool isMetal;

	public bool isStone;

	public bool isHardStone;

	public bool isSmallPlant;

	public InventoryItem[] statusObjectsPickUpFirst;

	public LoadBuildingInsides tileObjectLoadInside;

	[Header("Town Beauty --------------")]
	public TownManager.TownBeautyType beautyType;

	public float beautyToAdd = 0.1f;

	[Header("Map Icon --------------")]
	public Sprite mapIcon;

	public Color mapIconColor = Color.white;

	public DailyTaskGenerator.genericTaskType TaskType;

	public void addBeauty()
	{
		TownManager.manage.addTownBeauty(beautyToAdd, beautyType);
	}

	public void removeBeauty()
	{
		TownManager.manage.addTownBeauty(0f - beautyToAdd, beautyType);
	}

	public bool canBePlacedOn()
	{
		if (WorldManager.manageWorld.allObjects[tileObjectId].placedPositions.Length != 0)
		{
			return true;
		}
		return false;
	}

	public bool checkIfMultiTileObjectCanBePlaced(int startingXPos, int startingYPos, int rotation)
	{
		if (startingXPos < 5 || startingXPos > WorldManager.manageWorld.getMapSize() - 5 || startingYPos < 5 || startingYPos > WorldManager.manageWorld.getMapSize() - 5)
		{
			return false;
		}
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		int num3 = WorldManager.manageWorld.heightMap[startingXPos, startingYPos];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (WorldManager.manageWorld.heightMap[startingXPos + i, startingYPos + j] != num3)
				{
					return false;
				}
				if (WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] < -1)
				{
					return false;
				}
				if (WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] != -1 && WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] > -1 && WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] != 30 && !WorldManager.manageWorld.getTileObjectSettings(startingXPos + i, startingYPos + j).isGrass)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool checkIfMultiTileObjectCanBePlacedMapGenerationOnly(int startingXPos, int startingYPos, int rotation)
	{
		if (startingXPos < 5 || startingXPos > WorldManager.manageWorld.getMapSize() - 5 || startingYPos < 5 || startingYPos > WorldManager.manageWorld.getMapSize() - 5)
		{
			return false;
		}
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		int num3 = WorldManager.manageWorld.heightMap[startingXPos, startingYPos];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (WorldManager.manageWorld.heightMap[startingXPos + i, startingYPos + j] != num3)
				{
					return false;
				}
				if (WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] != -1 && WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] != 30)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool checkIfDeedCanBePlaced(int startingXPos, int startingYPos, int rotation)
	{
		if (startingXPos < 5 || startingXPos > WorldManager.manageWorld.getMapSize() - 5 || startingYPos < 5 || startingYPos > WorldManager.manageWorld.getMapSize() - 5)
		{
			return false;
		}
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
		{
			Vector3 position = NetworkNavMesh.nav.charsConnected[i].transform.position;
			if (position.x > (float)(startingXPos * 2) && position.x < (float)((startingXPos + num) * 2) && position.z > (float)(startingYPos * 2) && position.z < (float)((startingYPos + num2) * 2))
			{
				return false;
			}
		}
		int num3 = WorldManager.manageWorld.heightMap[startingXPos, startingYPos];
		for (int j = 0; j < num; j++)
		{
			for (int k = 0; k < num2; k++)
			{
				if (WorldManager.manageWorld.heightMap[startingXPos + j, startingYPos + k] != num3)
				{
					return false;
				}
				if (WorldManager.manageWorld.heightMap[startingXPos + j, startingYPos + k] < 0 && WorldManager.manageWorld.waterMap[startingXPos + j, startingYPos + k])
				{
					return false;
				}
				if (WorldManager.manageWorld.onTileMap[startingXPos + j, startingYPos + k] > -1)
				{
					if (!WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + j, startingYPos + k]].isWood && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + j, startingYPos + k]].isHardWood && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + j, startingYPos + k]].isSmallPlant && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + j, startingYPos + k]].isStone && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + j, startingYPos + k]].isHardStone)
					{
						return false;
					}
				}
				else if (WorldManager.manageWorld.onTileMap[startingXPos + j, startingYPos + k] < -1)
				{
					return false;
				}
			}
		}
		return true;
	}

	public string getWhyCantPlaceDeedText(int startingXPos, int startingYPos, int rotation)
	{
		if (startingXPos < 5 || startingXPos > WorldManager.manageWorld.getMapSize() - 5 || startingYPos < 5 || startingYPos > WorldManager.manageWorld.getMapSize() - 5)
		{
			return "Can't place here";
		}
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
		{
			Vector3 position = NetworkNavMesh.nav.charsConnected[i].transform.position;
			if (position.x > (float)(startingXPos * 2) && position.x < (float)((startingXPos + num) * 2) && position.z > (float)(startingYPos * 2) && position.z < (float)((startingYPos + num2) * 2))
			{
				return "Someone is in the way";
			}
		}
		int num3 = WorldManager.manageWorld.heightMap[startingXPos, startingYPos];
		for (int j = 0; j < num; j++)
		{
			for (int k = 0; k < num2; k++)
			{
				if (WorldManager.manageWorld.heightMap[startingXPos + j, startingYPos + k] != num3)
				{
					return "Not on level ground";
				}
				if (WorldManager.manageWorld.heightMap[startingXPos + j, startingYPos + k] < 0 && WorldManager.manageWorld.waterMap[startingXPos + j, startingYPos + k])
				{
					return "Can't be placed in water";
				}
				if (WorldManager.manageWorld.onTileMap[startingXPos + j, startingYPos + k] > -1)
				{
					if (!WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + j, startingYPos + k]].isWood && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + j, startingYPos + k]].isHardWood && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + j, startingYPos + k]].isSmallPlant && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + j, startingYPos + k]].isStone && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + j, startingYPos + k]].isHardStone)
					{
						return "Something in the way";
					}
				}
				else if (WorldManager.manageWorld.onTileMap[startingXPos + j, startingYPos + k] < -1)
				{
					return "Something in the way";
				}
			}
		}
		return "";
	}

	public int checkBridgLenth(int startX, int startY, int xCheck = 0, int yCheck = 0)
	{
		int i;
		for (i = 1; i <= 15; i++)
		{
			int num = startX + xCheck * i;
			int num2 = startY + yCheck * i;
			if (num < 5 || num > WorldManager.manageWorld.getMapSize() - 5 || num2 < 5 || num2 > WorldManager.manageWorld.getMapSize() - 5)
			{
				i = 20;
				break;
			}
			if (WorldManager.manageWorld.heightMap[startX, startY] == WorldManager.manageWorld.heightMap[num, num2] && i >= 2)
			{
				i++;
				break;
			}
		}
		return i;
	}

	public bool checkIfBridgeCanBePlaced(int startingXPos, int startingYPos, int rotation)
	{
		if (startingXPos < 5 || startingXPos > WorldManager.manageWorld.getMapSize() - 5 || startingYPos < 5 || startingYPos > WorldManager.manageWorld.getMapSize() - 5)
		{
			return false;
		}
		if (WorldManager.manageWorld.waterMap[startingXPos, startingYPos] && WorldManager.manageWorld.heightMap[startingXPos, startingYPos] <= -1)
		{
			return false;
		}
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		int num3 = 2;
		switch (rotation)
		{
		case 1:
			num3 = checkBridgLenth(startingXPos, startingYPos, 0, -1);
			if (num3 <= 15)
			{
				num2 = num3;
				startingYPos -= num3 - 1;
				break;
			}
			return false;
		case 2:
			num3 = checkBridgLenth(startingXPos, startingYPos, -1);
			if (num3 <= 15)
			{
				num = num3;
				startingXPos -= num3 - 1;
				break;
			}
			return false;
		case 3:
			num3 = checkBridgLenth(startingXPos, startingYPos, 0, 1);
			if (num3 <= 15)
			{
				num2 = num3;
				break;
			}
			return false;
		case 4:
			num3 = checkBridgLenth(startingXPos, startingYPos, 1);
			if (num3 <= 15)
			{
				num = num3;
				break;
			}
			return false;
		}
		int num4 = WorldManager.manageWorld.heightMap[startingXPos, startingYPos];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (WorldManager.manageWorld.heightMap[startingXPos + i, startingYPos + j] > num4)
				{
					return false;
				}
				if (WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] > -1)
				{
					if (!WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j]].isWood && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j]].isHardWood && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j]].isSmallPlant && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j]].isStone && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j]].isHardStone)
					{
						return false;
					}
				}
				else if (WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] < -1)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool checkIfMultiTileObjectCanBePlacedUnderGround(int startingXPos, int startingYPos, int rotation)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		if (startingXPos < 5 || startingXPos > WorldManager.manageWorld.getMapSize() - 5 || startingYPos < 5 || startingYPos > WorldManager.manageWorld.getMapSize() - 5)
		{
			return false;
		}
		int num3 = MapStorer.store.underWorldHeight[startingXPos, startingYPos];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (MapStorer.store.underWorldHeight[startingXPos + i, startingYPos + j] != num3)
				{
					return false;
				}
				if (MapStorer.store.underWorldOnTile[startingXPos + i, startingYPos + j] != -1 && MapStorer.store.underWorldOnTile[startingXPos + i, startingYPos + j] != 30)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool checkIfMultiTileObjectCanBePlacedInside(int startingXPos, int startingYPos, int rotation, HouseDetails houseDetails)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (houseDetails.houseMapOnTile[startingXPos + i, startingYPos + j] != -1)
				{
					return false;
				}
			}
		}
		return true;
	}

	public int[] placeBridgeTiledObject(int startingXPos, int startingYPos, int rotation = 4, int length = 2)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		switch (rotation)
		{
		case 1:
			num2 = length;
			startingYPos -= length - 1;
			rotation = 3;
			break;
		case 2:
			num = length;
			startingXPos -= length - 1;
			rotation = 4;
			break;
		case 3:
			num2 = length;
			break;
		case 4:
			num = length;
			break;
		}
		WorldManager.manageWorld.rotationMap[startingXPos, startingYPos] = rotation;
		WorldManager.manageWorld.onTileStatusMap[startingXPos, startingYPos] = length;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (i == 0 && j == 0)
				{
					WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] = tileObjectId;
				}
				else if (i == 0)
				{
					WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] = -3;
				}
				else if (j == 0)
				{
					WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] = -4;
				}
				else
				{
					WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] = -2;
				}
				WorldManager.manageWorld.addToChunksToRefreshList(startingXPos + i, startingYPos + j);
				Vector3 position = new Vector3((float)(startingXPos + i) * 2f, WorldManager.manageWorld.heightMap[startingXPos + i, startingYPos + j], (float)(startingYPos + j) * 2f);
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], position, 2);
				WorldManager.manageWorld.onTileChunkHasChanged(startingXPos + i, startingYPos + j);
				if (NetworkMapSharer.share.isServer)
				{
					WorldManager.manageWorld.findSpaceForDropAfterTileObjectChange(startingXPos + i, startingYPos + j);
				}
			}
		}
		return new int[2] { startingXPos, startingYPos };
	}

	public void placeMultiTiledObject(int startingXPos, int startingYPos, int rotation = 4)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		WorldManager.manageWorld.rotationMap[startingXPos, startingYPos] = rotation;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] != -1 && WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] != 30)
				{
					TileObject tileObject = WorldManager.manageWorld.findTileObjectInUse(startingXPos + i, startingYPos + j);
					if ((bool)tileObject)
					{
						tileObject.onDeath();
					}
				}
				if (i == 0 && j == 0)
				{
					WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] = tileObjectId;
				}
				else if (i == 0)
				{
					WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] = -3;
				}
				else if (j == 0)
				{
					WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] = -4;
				}
				else
				{
					WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] = -2;
				}
				WorldManager.manageWorld.addToChunksToRefreshList(startingXPos + i, startingYPos + j);
				Vector3 position = new Vector3((float)(startingXPos + i) * 2f, WorldManager.manageWorld.heightMap[startingXPos + i, startingYPos + j], (float)(startingYPos + j) * 2f);
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], position, 2);
				WorldManager.manageWorld.onTileChunkHasChanged(startingXPos + i, startingYPos + j);
				if (CustomNetworkManager.manage.isNetworkActive)
				{
					WorldManager.manageWorld.findSpaceForDropAfterTileObjectChange(startingXPos + i, startingYPos + j);
				}
			}
		}
	}

	public void flattenPosUnderMultitiledObject(int startingXPos, int startingYPos, int height, int rotation = 4)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				WorldManager.manageWorld.heightMap[startingXPos + i, startingYPos + j] = height;
			}
		}
	}

	public void placeMultiTiledObjectUnderGround(int startingXPos, int startingYPos, int rotation = 4)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		MapStorer.store.underWorldRotationMap[startingXPos, startingYPos] = rotation;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (i == 0 && j == 0)
				{
					MapStorer.store.underWorldOnTile[startingXPos + i, startingYPos + j] = tileObjectId;
				}
				else if (i == 0)
				{
					MapStorer.store.underWorldOnTile[startingXPos + i, startingYPos + j] = -3;
				}
				else if (j == 0)
				{
					MapStorer.store.underWorldOnTile[startingXPos + i, startingYPos + j] = -4;
				}
				else
				{
					MapStorer.store.underWorldOnTile[startingXPos + i, startingYPos + j] = -2;
				}
			}
		}
	}

	public void placeMultiTiledObjectInside(int startingXPos, int startingYPos, int rotation, HouseDetails placeInside)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		placeInside.houseMapRotation[startingXPos, startingYPos] = rotation;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (i == 0 && j == 0)
				{
					placeInside.houseMapOnTile[startingXPos + i, startingYPos + j] = tileObjectId;
				}
				else if (i == 0)
				{
					placeInside.houseMapOnTile[startingXPos + i, startingYPos + j] = -3;
				}
				else if (j == 0)
				{
					placeInside.houseMapOnTile[startingXPos + i, startingYPos + j] = -4;
				}
				else
				{
					placeInside.houseMapOnTile[startingXPos + i, startingYPos + j] = -2;
				}
			}
		}
	}

	public void removeMultiTiledObject(int startingXPos, int startingYPos, int rotation)
	{
		int num = xSize;
		int num2 = ySize;
		if (WorldManager.manageWorld.onTileMap[startingXPos, startingYPos] > -1 && (bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[startingXPos, startingYPos]].tileObjectBridge)
		{
			MonoBehaviour.print("Length = " + WorldManager.manageWorld.onTileStatusMap[startingXPos, startingYPos]);
			num2 = WorldManager.manageWorld.onTileStatusMap[startingXPos, startingYPos];
			if (rotation == 2 || rotation == 4)
			{
				num = num2;
				num2 = xSize;
			}
		}
		else if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				WorldManager.manageWorld.onTileMap[startingXPos + i, startingYPos + j] = -1;
				Vector3 position = new Vector3((float)(startingXPos + i) * 2f, WorldManager.manageWorld.heightMap[startingXPos + i, startingYPos + j], (float)(startingYPos + j) * 2f);
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], position, 2);
				WorldManager.manageWorld.onTileChunkHasChanged(startingXPos + i, startingYPos + j);
			}
		}
	}

	public void removeMultiTiledObjectInside(int startingXPos, int startingYPos, int rotation, HouseDetails removeFrom)
	{
		int num = xSize;
		int num2 = ySize;
		if (rotation == 2 || rotation == 4)
		{
			num = ySize;
			num2 = xSize;
		}
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				removeFrom.houseMapOnTile[startingXPos + i, startingYPos + j] = -1;
			}
		}
	}

	public void onDeathServer(int xPos, int yPos, HouseDetails inside, TileObjectGrowthStages tileObjectGrowthStages, Transform _transform, Transform[] dropObjectFromPositions)
	{
		int num = 0;
		int num2 = WorldManager.manageWorld.onTileStatusMap[xPos, yPos];
		if (inside != null)
		{
			num2 = inside.houseMapOnTileStatus[xPos, yPos];
			MonoBehaviour.print("Using house status: " + num2);
		}
		if (dropsStatusNumberOnDeath && !pickUpRequiresEmptyPocket)
		{
			NetworkMapSharer.share.spawnAServerDrop(num2, 1, _transform.position, inside);
		}
		else if (((bool)WorldManager.manageWorld.allObjectSettings[tileObjectId].dropsItemOnDeath || (bool)WorldManager.manageWorld.allObjectSettings[tileObjectId].dropFromLootTable) && (!onlyDropWhenGrown || (onlyDropWhenGrown && WorldManager.manageWorld.onTileStatusMap[xPos, yPos] >= WorldManager.manageWorld.allObjects[tileObjectId].tileObjectGrowthStages.objectStages.Length + WorldManager.manageWorld.allObjects[tileObjectId].tileObjectGrowthStages.takeOrAddFromStateOnHarvest - 1)))
		{
			if (dropObjectFromPositions.Length == 0)
			{
				if (!tileObjectGrowthStages || ((bool)tileObjectGrowthStages && tileObjectGrowthStages.dropsForStages.Length == 0) || num < tileObjectGrowthStages.dropsForStages[num2])
				{
					num++;
					if ((bool)WorldManager.manageWorld.allObjectSettings[tileObjectId].dropFromLootTable)
					{
						InventoryItem randomDropFromTable = WorldManager.manageWorld.allObjectSettings[tileObjectId].dropFromLootTable.getRandomDropFromTable();
						if (randomDropFromTable != null)
						{
							if (randomDropFromTable.hasFuel)
							{
								NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(randomDropFromTable), Random.Range(10, (int)((float)randomDropFromTable.fuelMax / 1.5f)), _transform.position, inside, true, WorldManager.manageWorld.allObjects[tileObjectId].getXpTallyType());
							}
							else
							{
								NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(randomDropFromTable), 1, _transform.position, inside, true, WorldManager.manageWorld.allObjects[tileObjectId].getXpTallyType());
							}
						}
					}
					else if (WorldManager.manageWorld.allObjectSettings[tileObjectId].canBePickedUp)
					{
						NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(WorldManager.manageWorld.allObjectSettings[tileObjectId].dropsItemOnDeath), 1, _transform.position + Vector3.up, inside, false, WorldManager.manageWorld.allObjects[tileObjectId].getXpTallyType());
					}
					else
					{
						NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(WorldManager.manageWorld.allObjectSettings[tileObjectId].dropsItemOnDeath), 1, _transform.position + Vector3.up, inside, true, WorldManager.manageWorld.allObjects[tileObjectId].getXpTallyType());
					}
				}
			}
			else
			{
				foreach (Transform transform in dropObjectFromPositions)
				{
					if ((bool)tileObjectGrowthStages && (!tileObjectGrowthStages || tileObjectGrowthStages.dropsForStages.Length != 0) && num >= tileObjectGrowthStages.dropsForStages[num2])
					{
						continue;
					}
					num++;
					if ((bool)WorldManager.manageWorld.allObjectSettings[tileObjectId].dropFromLootTable)
					{
						InventoryItem randomDropFromTable2 = WorldManager.manageWorld.allObjectSettings[tileObjectId].dropFromLootTable.getRandomDropFromTable();
						if (randomDropFromTable2 != null)
						{
							if (randomDropFromTable2.hasFuel)
							{
								NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(randomDropFromTable2), Random.Range(10, (int)((float)randomDropFromTable2.fuelMax / 1.5f)), transform.position, inside, true, WorldManager.manageWorld.allObjects[tileObjectId].getXpTallyType());
							}
							else
							{
								NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(randomDropFromTable2), 1, transform.position, inside, true, WorldManager.manageWorld.allObjects[tileObjectId].getXpTallyType());
							}
						}
					}
					else if (WorldManager.manageWorld.allObjectSettings[tileObjectId].canBePickedUp)
					{
						NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(WorldManager.manageWorld.allObjectSettings[tileObjectId].dropsItemOnDeath), 1, transform.position, inside, false, WorldManager.manageWorld.allObjects[tileObjectId].getXpTallyType());
					}
					else
					{
						NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(WorldManager.manageWorld.allObjectSettings[tileObjectId].dropsItemOnDeath), 1, transform.position, inside, true, WorldManager.manageWorld.allObjects[tileObjectId].getXpTallyType());
					}
				}
			}
		}
		if ((bool)tileObjectGrowthStages && tileObjectGrowthStages.canBeHarvested(WorldManager.manageWorld.onTileStatusMap[xPos, yPos]))
		{
			tileObjectGrowthStages.harvest(xPos, yPos);
		}
		if ((bool)WorldManager.manageWorld.allObjectSettings[tileObjectId].dropsObjectOnDeath)
		{
			NetworkMapSharer.share.RpcSpawnATileObjectDrop(tileObjectId, xPos, yPos, num2);
		}
		if ((bool)WorldManager.manageWorld.allObjectSettings[tileObjectId].spawnCarryableOnDeath && WorldManager.manageWorld.allObjectSettings[tileObjectId].carryableChance >= Random.Range(0f, 100f))
		{
			Vector3 pos = new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2);
			NetworkMapSharer.share.spawnACarryable(WorldManager.manageWorld.allObjectSettings[tileObjectId].spawnCarryableOnDeath, pos);
		}
		if (statusObjectsPickUpFirst.Length != 0 && num2 > 0)
		{
			if ((bool)statusObjectsPickUpFirst[num2].placeable)
			{
				WorldManager.manageWorld.allObjectSettings[statusObjectsPickUpFirst[num2].placeable.tileObjectId].removeBeauty();
			}
			NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(statusObjectsPickUpFirst[num2]), 1, _transform.position + Vector3.up, inside, true);
		}
	}
}
