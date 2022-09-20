using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldManager : MonoBehaviour
{
	public static WorldManager manageWorld;

	public int versionNumber;

	public int masterVersionNumber = 4;

	public NetworkMapSharer netMapSharer;

	public NetworkNavMesh netNavMesh;

	public RealWorldTimeLight netTime;

	private static int mapSize = 1000;

	public int chunkSize = 10;

	public int tileSize = 2;

	public float testSize;

	public int[,] heightMap;

	public int[,] tileTypeMap;

	public int[,] onTileMap;

	public int[,] onTileStatusMap;

	public int[,] tileTypeStatusMap;

	public int[,] rotationMap;

	public bool[,] waterMap;

	public int[,] fencedOffMap;

	public bool[,] clientRequestedMap;

	public bool[,] chunkChangedMap;

	public bool[,] changedMapOnTile;

	public bool[,] changedMapHeight;

	public bool[,] changedMapWater;

	public bool[,] changedMapTileType;

	public bool[,] chunkHasChangedToday;

	public bool[,] chunkWithFenceInIt;

	public Transform spawnPos;

	public TileObject[] allObjects;

	public TileObjectSettings[] allObjectSettings;

	public List<Chunk> chunksInUse;

	public List<TileObject> freeObjects;

	public List<DroppedItem> itemsOnGround;

	public List<PickUpAndCarry> allCarriables;

	private int freeObjectsCount;

	public GameObject ChunkPrefab;

	public GameObject ChunkLoaderPrfab;

	public GameObject droppedItemPrefab;

	public bool firstChunkLayed;

	public Material stoneSide;

	public TileTypes fallBackTileType;

	public TileTypes[] tileTypes;

	public UnityEvent changeDayEvent = new UnityEvent();

	public int day = 1;

	public int week = 1;

	public int month = 1;

	public int year = 1;

	public List<int[]> chunksToRefresh = new List<int[]>();

	public GameObject firstConnectAirShip;

	public ReadableSign confirmSleepSign;

	public Conversation confirmSleepConvo;

	public Conversation sleepUnderground;

	public Conversation sleepHouseMoving;

	private List<int[]> clientLock = new List<int[]>();

	public bool chunkRefreshCompleted;

	private int completedCropChecker;

	public List<CurrentChanger> allChangers = new List<CurrentChanger>();

	private WaitForSeconds waterSec = new WaitForSeconds(0.25f);

	private WaitForSeconds sec = new WaitForSeconds(1f);

	public LayerMask pickUpLayer;

	private void Awake()
	{
		manageWorld = this;
		heightMap = new int[mapSize, mapSize];
		tileTypeMap = new int[mapSize, mapSize];
		onTileMap = new int[mapSize, mapSize];
		onTileStatusMap = new int[mapSize, mapSize];
		tileTypeStatusMap = new int[mapSize, mapSize];
		rotationMap = new int[mapSize, mapSize];
		waterMap = new bool[mapSize, mapSize];
		fencedOffMap = new int[mapSize, mapSize];
		clientRequestedMap = new bool[mapSize / chunkSize, mapSize / chunkSize];
		chunkChangedMap = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapOnTile = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapHeight = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapWater = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapTileType = new bool[mapSize / chunkSize, mapSize / chunkSize];
		chunkHasChangedToday = new bool[mapSize / chunkSize, mapSize / chunkSize];
		chunkWithFenceInIt = new bool[mapSize / chunkSize, mapSize / chunkSize];
		NetworkMapSharer.share = netMapSharer;
		NetworkNavMesh.nav = netNavMesh;
		RealWorldTimeLight.time = netTime;
		for (int i = 0; i < allObjects.Length; i++)
		{
			if (allObjects[i] != null)
			{
				if (allObjects[i].tileObjectId != i)
				{
					MonoBehaviour.print(allObjects[i].name + " Object in list ID does not math its position in objects. Current position = " + i);
				}
				if (allObjectSettings[i].tileObjectId != i)
				{
					MonoBehaviour.print(allObjects[i].name + " Object in list ID does not math its position in settings. Current position = " + i);
				}
			}
		}
	}

	public Conversation getSleepText()
	{
		if (RealWorldTimeLight.time.underGround)
		{
			return sleepUnderground;
		}
		if (TownManager.manage.checkIfInMovingBuildingForSleep())
		{
			return sleepHouseMoving;
		}
		return confirmSleepConvo;
	}

	private void Start()
	{
		heightMap = new int[mapSize, mapSize];
		tileTypeMap = new int[mapSize, mapSize];
		onTileMap = new int[mapSize, mapSize];
		onTileStatusMap = new int[mapSize, mapSize];
		tileTypeStatusMap = new int[mapSize, mapSize];
		rotationMap = new int[mapSize, mapSize];
		waterMap = new bool[mapSize, mapSize];
		fencedOffMap = new int[mapSize, mapSize];
		clientRequestedMap = new bool[mapSize / chunkSize, mapSize / chunkSize];
		chunkChangedMap = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapOnTile = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapHeight = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapWater = new bool[mapSize / chunkSize, mapSize / chunkSize];
		changedMapTileType = new bool[mapSize / chunkSize, mapSize / chunkSize];
		chunkHasChangedToday = new bool[mapSize / chunkSize, mapSize / chunkSize];
		TileObject[] array = allObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].checkForAllExtensions();
		}
	}

	public bool checkTileClientLock(int xPos, int yPos)
	{
		for (int i = 0; i < clientLock.Count; i++)
		{
			if (clientLock[i][0] == xPos && clientLock[i][1] == yPos)
			{
				return true;
			}
		}
		return false;
	}

	public void lockTileClient(int xPos, int yPos)
	{
		clientLock.Add(new int[2] { xPos, yPos });
	}

	public void unlockClientTile(int xPos, int yPos)
	{
		for (int i = 0; i < clientLock.Count; i++)
		{
			if (clientLock[i][0] == xPos && clientLock[i][1] == yPos)
			{
				clientLock.RemoveAt(i);
				break;
			}
		}
	}

	public DateSave getDateSave()
	{
		return new DateSave
		{
			day = day,
			week = week,
			month = month,
			year = year
		};
	}

	public void loadDateFromSave(DateSave loadFrom)
	{
		day = loadFrom.day;
		week = loadFrom.week;
		month = loadFrom.month;
		year = loadFrom.year;
	}

	private bool checkIfDropCanDrop(int xPos, int yPos, HouseDetails inside = null)
	{
		return true;
	}

	public bool tryAndStackItem(int itemId, int stack, int xPos, int yPos, HouseDetails inside)
	{
		if (Inventory.inv.allItems[itemId].checkIfStackable() && inside == null)
		{
			List<DroppedItem> allDropsOnTile = getAllDropsOnTile(xPos, yPos);
			for (int i = 0; i < allDropsOnTile.Count; i++)
			{
				if (allDropsOnTile[i].myItemId == itemId)
				{
					DroppedItem droppedItem = allDropsOnTile[i];
					droppedItem.NetworkstackAmount = droppedItem.stackAmount + stack;
					return true;
				}
			}
		}
		return false;
	}

	public bool checkIfFishCanBeDropped(Vector3 positionToDrop)
	{
		if (waterMap[(int)(positionToDrop.x / 2f), (int)(positionToDrop.z / 2f)])
		{
			return true;
		}
		return false;
	}

	public bool checkIfDropCanFitOnGround(int itemId, int stackAmount, Vector3 positionToDrop, HouseDetails inside)
	{
		if (!WeatherManager.manage.isInside() || (WeatherManager.manage.isInside() && NetworkMapSharer.share.localChar.myInteract.insidePlayerHouse))
		{
			if (Inventory.inv.allItems[itemId].isDeed)
			{
				return false;
			}
			if ((bool)Inventory.inv.allItems[itemId].fish && !checkIfFishCanBeDropped(positionToDrop))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public List<DroppedItem> getAllDropsOnTile(int xPos, int yPos)
	{
		Vector2 other = new Vector2(xPos, yPos);
		List<DroppedItem> list = new List<DroppedItem>();
		for (int i = 0; i < itemsOnGround.Count; i++)
		{
			if (itemsOnGround[i].underground == RealWorldTimeLight.time.underGround && itemsOnGround[i].inside == null && itemsOnGround[i].onTile.Equals(other))
			{
				list.Add(itemsOnGround[i]);
			}
		}
		return list;
	}

	public List<DroppedItem> getDropsToSave()
	{
		List<DroppedItem> list = new List<DroppedItem>();
		for (int i = 0; i < itemsOnGround.Count; i++)
		{
			if (itemsOnGround != null && !itemsOnGround[i].underground && itemsOnGround[i].saveDrop)
			{
				list.Add(itemsOnGround[i]);
			}
		}
		return list;
	}

	public bool checkIfDropIsTooCloseToEachOther(Vector3 positionToCheck)
	{
		for (int i = 0; i < itemsOnGround.Count; i++)
		{
			if (Vector3.Distance(itemsOnGround[i].transform.position, positionToCheck) < 0.2f)
			{
				return true;
			}
		}
		return false;
	}

	public void updateDropsOnTileHeight(int xPos, int yPos)
	{
		List<DroppedItem> allDropsOnTile = getAllDropsOnTile(xPos, yPos);
		for (int i = 0; i < allDropsOnTile.Count; i++)
		{
			allDropsOnTile[i].NetworkdesiredPos = new Vector3(allDropsOnTile[i].desiredPos.x, heightMap[xPos, yPos], allDropsOnTile[i].desiredPos.z);
		}
	}

	public bool isChecking()
	{
		return true;
	}

	public Vector3 moveDropPosToSafeOutside(Vector3 pos, bool useNavMesh = true)
	{
		if (!isPositionOnMap((int)pos.x / 2, (int)pos.z / 2))
		{
			pos.y = -2f;
			return pos;
		}
		bool flag = false;
		int checkHeight = heightMap[(int)pos.x / 2, (int)pos.z / 2];
		if (spaceCanBeDroppedOn(pos, checkHeight))
		{
			flag = true;
		}
		else
		{
			if (useNavMesh && NetworkNavMesh.nav.checkIfPlaceOnNavMeshForDrop(pos) != Vector3.zero)
			{
				return NetworkNavMesh.nav.checkIfPlaceOnNavMeshForDrop(pos);
			}
			if (spaceCanBeDroppedOn(pos + new Vector3(-2f, 0f, 0f), checkHeight))
			{
				pos.x -= 2f;
				flag = true;
			}
			else if (spaceCanBeDroppedOn(pos + new Vector3(2f, 0f, 0f), checkHeight))
			{
				pos.x += 2f;
				flag = true;
			}
			else if (spaceCanBeDroppedOn(pos + new Vector3(0f, 0f, -2f), checkHeight))
			{
				pos.z -= 2f;
				flag = true;
			}
			else if (spaceCanBeDroppedOn(pos + new Vector3(0f, 0f, 2f), checkHeight))
			{
				pos.z += 2f;
				flag = true;
			}
			else
			{
				Vector3 vector = NetworkNavMesh.nav.checkIfPlaceOnNavMeshForDrop(pos);
				if (vector != Vector3.zero)
				{
					return vector;
				}
			}
		}
		int num = 300;
		int num2 = (int)pos.x;
		int num3 = (int)pos.z;
		while (!flag)
		{
			num--;
			if (num <= 0)
			{
				pos.x += num2;
				pos.z += num3;
				flag = true;
				break;
			}
			if (spaceCanBeDroppedOn(pos, checkHeight))
			{
				flag = true;
				continue;
			}
			pos.x += Random.Range(-2, 2);
			pos.z += Random.Range(-2, 2);
		}
		if (onTileMap[Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2] == 15)
		{
			pos.y = onTileStatusMap[Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2];
		}
		else
		{
			pos.y = heightMap[Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2];
		}
		return pos;
	}

	public Vector3 moveDropPosToSafeInside(Vector3 pos, HouseDetails inside, DisplayPlayerHouseTiles display)
	{
		bool flag = false;
		if (spaceCanBeDroppedInside(pos, inside, display))
		{
			flag = true;
		}
		else if (spaceCanBeDroppedInside(pos + new Vector3(-2f, 0f, 0f), inside, display))
		{
			pos.x -= 2f;
			flag = true;
		}
		else if (spaceCanBeDroppedInside(pos + new Vector3(2f, 0f, 0f), inside, display))
		{
			pos.x += 2f;
			flag = true;
		}
		else if (spaceCanBeDroppedInside(pos + new Vector3(0f, 0f, -2f), inside, display))
		{
			pos.z -= 2f;
			flag = true;
		}
		else if (spaceCanBeDroppedInside(pos + new Vector3(0f, 0f, 2f), inside, display))
		{
			pos.z += 2f;
			flag = true;
		}
		int num = 200;
		while (!flag)
		{
			num--;
			if (spaceCanBeDroppedInside(pos, inside, display) || num <= 0)
			{
				flag = true;
				continue;
			}
			pos.x += Random.Range(-1, 2) * 2;
			pos.x = Mathf.Clamp(pos.x, display.getStartingPosTransform().position.x, display.getStartingPosTransform().position.x + 50f);
			pos.z += Random.Range(-1, 2) * 2;
			pos.z = Mathf.Clamp(pos.z, display.getStartingPosTransform().position.z, display.getStartingPosTransform().position.z + 50f);
		}
		return pos;
	}

	private bool spaceCanBeDroppedOn(Vector3 pos, int checkHeight)
	{
		if (isPositionOnMap(Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2))
		{
			if (heightMap[Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2] > checkHeight)
			{
				return false;
			}
			if (onTileMap[Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2] == -1 || (onTileMap[Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2] > -1 && allObjectSettings[onTileMap[Mathf.RoundToInt(pos.x) / 2, Mathf.RoundToInt(pos.z) / 2]].walkable))
			{
				return true;
			}
		}
		return false;
	}

	private bool spaceCanBeDroppedInside(Vector3 pos, HouseDetails details, DisplayPlayerHouseTiles inside)
	{
		int num = Mathf.RoundToInt(pos.x - inside.getStartingPosTransform().position.x) / 2;
		int num2 = Mathf.RoundToInt(pos.z - inside.getStartingPosTransform().position.z) / 2;
		if (checkIfOnMap(num, true) && checkIfOnMap(num2, true) && (details.houseMapOnTile[num, num2] == -1 || (details.houseMapOnTile[num, num2] > -1 && allObjectSettings[details.houseMapOnTile[num, num2]].walkable)))
		{
			return true;
		}
		return false;
	}

	public GameObject dropAnItem(int itemId, int stackAmount, Vector3 positionToDrop, HouseDetails inside, bool tryNotToStack)
	{
		if (!tryNotToStack && tryAndStackItem(itemId, stackAmount, Mathf.RoundToInt(positionToDrop.x / 2f), Mathf.RoundToInt(positionToDrop.z / 2f), inside))
		{
			return null;
		}
		if (inside == null)
		{
			GameObject obj = Object.Instantiate(droppedItemPrefab, positionToDrop, Quaternion.identity);
			DroppedItem component = obj.GetComponent<DroppedItem>();
			positionToDrop = moveDropPosToSafeOutside(positionToDrop);
			component.setDesiredPos(positionToDrop.y, positionToDrop.x, positionToDrop.z);
			component.NetworkstackAmount = stackAmount;
			component.NetworkmyItemId = itemId;
			itemsOnGround.Add(component);
			return obj;
		}
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(inside.xPos, inside.yPos);
		positionToDrop = moveDropPosToSafeInside(positionToDrop, inside, displayPlayerHouseTiles);
		positionToDrop.y = displayPlayerHouseTiles.getStartingPosTransform().position.y;
		GameObject obj2 = Object.Instantiate(droppedItemPrefab, positionToDrop, Quaternion.identity);
		DroppedItem component2 = obj2.GetComponent<DroppedItem>();
		component2.inside = inside;
		component2.setDesiredPos(displayPlayerHouseTiles.getStartingPosTransform().position.y, positionToDrop.x, positionToDrop.z);
		component2.NetworkstackAmount = stackAmount;
		component2.NetworkmyItemId = itemId;
		return obj2;
	}

	public void getFreeChunkAndSetInPos(int xPos, int yPos)
	{
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (!chunksInUse[i].isActiveAndEnabled)
			{
				chunksInUse[i]._transform.position = Vector3.zero;
				chunksInUse[i].setChunkAndRefresh(xPos, yPos);
				return;
			}
		}
		Chunk component = Object.Instantiate(ChunkPrefab).GetComponent<Chunk>();
		component.transform.position = Vector3.zero;
		component.setChunkAndRefresh(xPos, yPos);
		chunksInUse.Add(component);
	}

	public void giveBackChunk(Chunk giveBackChunk)
	{
		giveBackChunk.returnAllTileObjects();
		giveBackChunk.gameObject.SetActive(false);
	}

	public void refreshAllChunksForSwitch(Vector3 mineEntranceExitPos)
	{
		chunkRefreshCompleted = false;
		StartCoroutine(refreshChunkDelay(mineEntranceExitPos));
	}

	private IEnumerator refreshChunkDelay(Vector3 mineEntranceExitPos)
	{
		Chunk mineEntranceOrExitChunk = null;
		Chunk mineEntranceLeft = null;
		Chunk mineEntranceRight = null;
		Chunk mineEntranceUp = null;
		Chunk mineEntranceDown = null;
		int entranceChunkX = (int)(Mathf.Round(mineEntranceExitPos.x) / 2f) / chunkSize * chunkSize;
		int entranceChunkY = (int)(Mathf.Round(mineEntranceExitPos.z) / 2f) / chunkSize * chunkSize;
		for (int j = 0; j < chunksInUse.Count; j++)
		{
			if (chunksInUse[j].gameObject.activeInHierarchy)
			{
				if (chunksInUse[j].showingChunkX == entranceChunkX && chunksInUse[j].showingChunkY == entranceChunkY)
				{
					chunksInUse[j].setChunkAndRefresh(chunksInUse[j].showingChunkX, chunksInUse[j].showingChunkY, true);
					mineEntranceOrExitChunk = chunksInUse[j];
				}
				else if (chunksInUse[j].showingChunkX == entranceChunkX + 10 && chunksInUse[j].showingChunkY == entranceChunkY)
				{
					chunksInUse[j].setChunkAndRefresh(chunksInUse[j].showingChunkX, chunksInUse[j].showingChunkY, true);
					mineEntranceRight = chunksInUse[j];
					yield return null;
				}
				else if (chunksInUse[j].showingChunkX == entranceChunkX - 10 && chunksInUse[j].showingChunkY == entranceChunkY)
				{
					chunksInUse[j].setChunkAndRefresh(chunksInUse[j].showingChunkX, chunksInUse[j].showingChunkY, true);
					mineEntranceLeft = chunksInUse[j];
					yield return null;
				}
				else if (chunksInUse[j].showingChunkX == entranceChunkX && chunksInUse[j].showingChunkY == entranceChunkY - 10)
				{
					chunksInUse[j].setChunkAndRefresh(chunksInUse[j].showingChunkX, chunksInUse[j].showingChunkY, true);
					mineEntranceDown = chunksInUse[j];
					yield return null;
				}
				else if (chunksInUse[j].showingChunkX == entranceChunkX && chunksInUse[j].showingChunkY == entranceChunkY + 10)
				{
					chunksInUse[j].setChunkAndRefresh(chunksInUse[j].showingChunkX, chunksInUse[j].showingChunkY, true);
					mineEntranceUp = chunksInUse[j];
					yield return null;
				}
			}
		}
		int chunkCounter = 0;
		for (int j = 0; j < chunksInUse.Count; j++)
		{
			if (chunksInUse[j].gameObject.activeInHierarchy && chunksInUse[j] != mineEntranceOrExitChunk && chunksInUse[j] != mineEntranceUp && chunksInUse[j] != mineEntranceDown && chunksInUse[j] != mineEntranceLeft && chunksInUse[j] != mineEntranceRight)
			{
				chunksInUse[j].setChunkAndRefresh(chunksInUse[j].showingChunkX, chunksInUse[j].showingChunkY, true);
				chunkCounter++;
				if (chunkCounter >= 4)
				{
					chunkCounter = 0;
					yield return null;
				}
			}
		}
		chunkRefreshCompleted = true;
	}

	public void refreshAllChunksForConnect()
	{
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (chunksInUse[i].gameObject.activeInHierarchy)
			{
				giveBackChunk(chunksInUse[i]);
			}
		}
		NewChunkLoader.loader.resetChunksViewing();
	}

	public IEnumerator refreshAllChunksNewDay()
	{
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (chunksInUse[i].gameObject.activeInHierarchy && chunkChangedMap[chunksInUse[i].showingChunkX / 10, chunksInUse[i].showingChunkY / 10] && chunkHasChangedToday[chunksInUse[i].showingChunkX / 10, chunksInUse[i].showingChunkY / 10])
			{
				chunksInUse[i].refreshChunk();
				yield return null;
			}
		}
	}

	public bool clientHasRequestedChunk(int changeXPos, int changeYPos)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		return clientRequestedMap[changeXPos / chunkSize, changeYPos / chunkSize];
	}

	public void waterChunkHasChanged(int changeXPos, int changeYPos)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		changedMapWater[changeXPos / chunkSize, changeYPos / chunkSize] = true;
		chunkChangedMap[changeXPos / chunkSize, changeYPos / chunkSize] = true;
	}

	public void setChunkHasChangedToday(int changeXPos, int changeYPos)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		chunkHasChangedToday[changeXPos / chunkSize, changeYPos / chunkSize] = true;
	}

	public void heightChunkHasChanged(int changeXPos, int changeYPos)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		changedMapHeight[changeXPos / chunkSize, changeYPos / chunkSize] = true;
		chunkChangedMap[changeXPos / chunkSize, changeYPos / chunkSize] = true;
	}

	public void onTileChunkHasChanged(int changeXPos, int changeYPos)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		changedMapOnTile[changeXPos / chunkSize, changeYPos / chunkSize] = true;
		chunkChangedMap[changeXPos / chunkSize, changeYPos / chunkSize] = true;
	}

	public void tileTypeChunkHasChanged(int changeXPos, int changeYPos)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		changedMapTileType[changeXPos / chunkSize, changeYPos / chunkSize] = true;
		chunkChangedMap[changeXPos / chunkSize, changeYPos / chunkSize] = true;
	}

	public void placeFenceInChunk(int changeXPos, int changeYPos)
	{
		fencedOffMap[changeXPos, changeYPos] = 1;
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		chunkWithFenceInIt[changeXPos / chunkSize, changeYPos / chunkSize] = true;
	}

	public void refreshAllChunksInUse(int changeXPos, int changeYPos, bool networkRefresh = false)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (chunksInUse[i].gameObject.activeInHierarchy)
			{
				if (chunksInUse[i].showingChunkY == changeYPos && chunksInUse[i].showingChunkX == changeXPos)
				{
					chunksInUse[i].refreshChunk();
				}
				if ((chunksInUse[i].showingChunkX == changeXPos || chunksInUse[i].showingChunkX == changeXPos + chunkSize || chunksInUse[i].showingChunkX == changeXPos - chunkSize) && (chunksInUse[i].showingChunkY == changeYPos || chunksInUse[i].showingChunkY == changeYPos + chunkSize || chunksInUse[i].showingChunkY == changeYPos - chunkSize))
				{
					chunksInUse[i].refreshChunk(false);
				}
			}
		}
	}

	public void refreshTileObjectsOnChunksInUse(int changeXPos, int changeYPos, bool networkRefresh = false)
	{
		changeXPos = Mathf.RoundToInt(changeXPos / chunkSize) * chunkSize;
		changeYPos = Mathf.RoundToInt(changeYPos / chunkSize) * chunkSize;
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (chunksInUse[i].gameObject.activeInHierarchy)
			{
				if (chunksInUse[i].showingChunkY == changeYPos && chunksInUse[i].showingChunkX == changeXPos)
				{
					chunksInUse[i].refreshChunksOnTileObjects();
				}
				if ((chunksInUse[i].showingChunkX == changeXPos || chunksInUse[i].showingChunkX == changeXPos + chunkSize || chunksInUse[i].showingChunkX == changeXPos - chunkSize) && (chunksInUse[i].showingChunkY == changeYPos || chunksInUse[i].showingChunkY == changeYPos + chunkSize || chunksInUse[i].showingChunkY == changeYPos - chunkSize))
				{
					chunksInUse[i].refreshChunksOnTileObjects(true);
				}
			}
		}
	}

	public int[] getChunkDetails(int chunkX, int chunkY, int[,] requestedMap)
	{
		int[] array = new int[chunkSize * chunkSize];
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				array[i * chunkSize + j] = requestedMap[chunkX + j, chunkY + i];
			}
		}
		return array;
	}

	public bool chunkHasItemsOnTop(int chunkX, int chunkY)
	{
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				if (onTileMap[chunkX + j, chunkY + i] > -1 && allObjects[onTileMap[chunkX + j, chunkY + i]].canBePlaceOn() && ItemOnTopManager.manage.hasItemsOnTop(chunkX + j, chunkY + i))
				{
					return true;
				}
			}
		}
		return false;
	}

	public ItemOnTop[] getItemsOnTopInChunk(int chunkX, int chunkY)
	{
		List<ItemOnTop> list = new List<ItemOnTop>();
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				if (onTileMap[chunkX + j, chunkY + i] > -1 && allObjects[onTileMap[chunkX + j, chunkY + i]].canBePlaceOn() && ItemOnTopManager.manage.hasItemsOnTop(chunkX + j, chunkY + i))
				{
					ItemOnTop[] allItemsOnTop = ItemOnTopManager.manage.getAllItemsOnTop(chunkX + j, chunkY + i, null);
					for (int k = 0; k < allItemsOnTop.Length; k++)
					{
						list.Add(allItemsOnTop[k]);
					}
				}
			}
		}
		return list.ToArray();
	}

	public int[] getChunkStatusDetails(int chunkX, int chunkY)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				if (manageWorld.onTileMap[chunkX + j, chunkY + i] > -1)
				{
					if (allObjectSettings[manageWorld.onTileMap[chunkX + j, chunkY + i]].getRotationFromMap)
					{
						list.Add(manageWorld.rotationMap[chunkX + j, chunkY + i]);
					}
					if (allObjects[manageWorld.onTileMap[chunkX + j, chunkY + i]].hasExtensions)
					{
						list.Add(manageWorld.onTileStatusMap[chunkX + j, chunkY + i]);
					}
				}
			}
		}
		int[] array = new int[list.Count];
		for (int k = 0; k < list.Count; k++)
		{
			array[k] = list[k];
		}
		return array;
	}

	public bool[] getWaterChunkDetails(int chunkX, int chunkY, bool[,] requestedMap)
	{
		bool[] array = new bool[chunkSize * chunkSize];
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				array[i * chunkSize + j] = requestedMap[chunkX + j, chunkY + i];
			}
		}
		return array;
	}

	public int[] getHouseDetailsArray(int[,] requestedMap)
	{
		int[] array = new int[625];
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				array[i * 25 + j] = requestedMap[j, i];
			}
		}
		return array;
	}

	public int[,] fillHouseDetailsArray(int[] convertMap)
	{
		int[,] array = new int[25, 25];
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				array[j, i] = convertMap[i * 25 + j];
			}
		}
		return array;
	}

	public void fillOnTileChunkDetails(int chunkX, int chunkY, int[] onTileDetails, int[] otherDetails)
	{
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				manageWorld.onTileMap[chunkX + j, chunkY + i] = onTileDetails[i * chunkSize + j];
			}
		}
		bool flag = false;
		int num = 0;
		for (int k = 0; k < chunkSize; k++)
		{
			for (int l = 0; l < chunkSize; l++)
			{
				if (manageWorld.onTileMap[chunkX + l, chunkY + k] > -1)
				{
					if (!flag && allObjectSettings[manageWorld.onTileMap[chunkX + l, chunkY + k]].canBePlacedOn())
					{
						flag = true;
					}
					if (allObjectSettings[manageWorld.onTileMap[chunkX + l, chunkY + k]].getRotationFromMap)
					{
						manageWorld.rotationMap[chunkX + l, chunkY + k] = otherDetails[num];
						num++;
					}
					if (allObjects[manageWorld.onTileMap[chunkX + l, chunkY + k]].hasExtensions)
					{
						manageWorld.onTileStatusMap[chunkX + l, chunkY + k] = otherDetails[num];
						num++;
					}
				}
			}
		}
		if (flag)
		{
			MonoBehaviour.print("Requesting on top for chunk");
			NetworkMapSharer.share.localChar.CmdRequestItemOnTopForChunk(chunkX, chunkY);
		}
	}

	public void fillTileTypeChunkDetails(int chunkX, int chunkY, int[] tileTypeDetails)
	{
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				manageWorld.tileTypeMap[chunkX + j, chunkY + i] = tileTypeDetails[i * chunkSize + j];
			}
		}
	}

	public void fillWaterChunkDetails(int chunkX, int chunkY, bool[] waterDetails)
	{
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				manageWorld.waterMap[chunkX + j, chunkY + i] = waterDetails[i * chunkSize + j];
			}
		}
	}

	public void fillHeightChunkDetails(int chunkX, int chunkY, int[] heightTileDetails)
	{
		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				manageWorld.heightMap[chunkX + j, chunkY + i] = heightTileDetails[i * chunkSize + j];
			}
		}
	}

	public bool doesPositionNeedsChunk(int changeXPos, int changeYPos)
	{
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (chunksInUse[i].isActiveAndEnabled && chunksInUse[i].showingChunkY == changeYPos && chunksInUse[i].showingChunkX == changeXPos)
			{
				return false;
			}
		}
		return true;
	}

	public void getNoOfWaterTilesClose(int changeXPos, int changeYPos)
	{
		NewChunkLoader.loader.oceanTilesNearChar = 0;
		NewChunkLoader.loader.waterTilesNearChar = 0;
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (chunksInUse[i].gameObject.activeInHierarchy)
			{
				if (chunksInUse[i].showingChunkX == changeXPos && chunksInUse[i].showingChunkY == changeYPos)
				{
					NewChunkLoader.loader.riverTilesInCharChunk = chunksInUse[i].waterTilesOnChunk;
				}
				int num = chunkSize + chunkSize;
				if (chunksInUse[i].showingChunkY < changeYPos + num && chunksInUse[i].showingChunkY > changeYPos - num && chunksInUse[i].showingChunkX < changeXPos + num && chunksInUse[i].showingChunkX > changeXPos - num)
				{
					NewChunkLoader.loader.waterTilesNearChar += chunksInUse[i].waterTilesOnChunk;
				}
				int num2 = 2 * chunkSize + chunkSize;
				if (chunksInUse[i].showingChunkY < changeYPos + num2 && chunksInUse[i].showingChunkY > changeYPos - num2 && chunksInUse[i].showingChunkX < changeXPos + num2 && chunksInUse[i].showingChunkX > changeXPos - num2)
				{
					NewChunkLoader.loader.oceanTilesNearChar += chunksInUse[i].oceanTilesOnChunk;
				}
			}
		}
	}

	public void returnChunksNotCloseEnough(int changeXPos, int changeYPos, int amountOfChunksCloseToChar)
	{
		for (int i = 0; i < chunksInUse.Count; i++)
		{
			if (chunksInUse[i].isActiveAndEnabled)
			{
				int num = amountOfChunksCloseToChar * chunkSize + chunkSize;
				if (chunksInUse[i].showingChunkY >= changeYPos + num || chunksInUse[i].showingChunkY <= changeYPos - num || chunksInUse[i].showingChunkX >= changeXPos + num || chunksInUse[i].showingChunkX <= changeXPos - num)
				{
					giveBackChunk(chunksInUse[i]);
				}
			}
		}
	}

	public TileObject findTileObjectInUse(int xPos, int yPos)
	{
		if ((bool)netMapSharer && netMapSharer.isActiveAndEnabled && !netMapSharer.isServer && !clientHasRequestedChunk(xPos, yPos))
		{
			return null;
		}
		TileObject result = null;
		if (onTileMap[xPos, yPos] == -1 || onTileMap[xPos, yPos] == 30)
		{
			return result;
		}
		int num = Mathf.RoundToInt(xPos / chunkSize) * chunkSize;
		int num2 = Mathf.RoundToInt(yPos / chunkSize) * chunkSize;
		foreach (Chunk item in chunksInUse)
		{
			if (item.gameObject.activeInHierarchy && item.showingChunkX == num && item.showingChunkY == num2)
			{
				int num3 = xPos - num;
				int num4 = yPos - num2;
				result = item.chunksTiles[num3, num4].onThisTile;
			}
		}
		return result;
	}

	public TileObject getTileObject(int desiredObject, int xPos, int yPos)
	{
		if (desiredObject == 500)
		{
			return null;
		}
		TileObject tileObject = null;
		for (int num = freeObjectsCount - 1; num >= 0; num--)
		{
			if (freeObjects[num].tileObjectId == desiredObject && !freeObjects[num].active)
			{
				tileObject = freeObjects[num];
				break;
			}
		}
		if (tileObject == null)
		{
			tileObject = Object.Instantiate(allObjects[desiredObject].gameObject, new Vector3(xPos * 2, manageWorld.heightMap[xPos, yPos], yPos * 2), Quaternion.identity).GetComponent<TileObject>();
			tileObject._transform = tileObject.transform;
			tileObject._gameObject = tileObject.gameObject;
			tileObject.currentHealth = allObjectSettings[desiredObject].fullHealth;
			freeObjects.Add(tileObject);
			freeObjectsCount++;
		}
		tileObject._transform.localPosition = new Vector3(xPos * 2, manageWorld.heightMap[xPos, yPos], yPos * 2);
		tileObject.getRotation(xPos, yPos);
		if (!tileObject.active)
		{
			tileObject.active = true;
			tileObject._gameObject.SetActive(true);
		}
		return tileObject;
	}

	private IEnumerator delayActivateObject(GameObject dObject)
	{
		yield return null;
		if (Random.Range(0, 2) == 1)
		{
			yield return null;
		}
		dObject.SetActive(true);
	}

	public TileObject getTileObjectForHouse(int desiredObject, Vector3 moveTo, int xPos, int yPos, HouseDetails thisHouse)
	{
		if (desiredObject == 500)
		{
			return null;
		}
		TileObject tileObject = null;
		for (int i = 0; i < freeObjectsCount; i++)
		{
			if (freeObjects[i].tileObjectId == desiredObject && !freeObjects[i].active)
			{
				tileObject = freeObjects[i];
				break;
			}
		}
		if (tileObject == null)
		{
			tileObject = Object.Instantiate(allObjects[desiredObject].gameObject, moveTo, Quaternion.identity).GetComponent<TileObject>();
			tileObject._transform = tileObject.transform;
			tileObject._gameObject = tileObject.gameObject;
			freeObjects.Add(tileObject);
			freeObjectsCount++;
		}
		tileObject._transform.localPosition = moveTo;
		tileObject.getRotationInside(xPos, yPos, thisHouse);
		if (!tileObject.active)
		{
			tileObject.active = true;
			tileObject._gameObject.SetActive(true);
		}
		return tileObject;
	}

	public TileObject getTileObjectForOnTop(int desiredObject, Vector3 pos)
	{
		TileObject component = Object.Instantiate(allObjects[desiredObject].gameObject, pos, Quaternion.identity).GetComponent<TileObject>();
		component._transform = component.transform;
		component._gameObject = component.gameObject;
		return component;
	}

	public TileObject getTileObjectForServerDrop(int desiredObject, Vector3 position)
	{
		if (desiredObject == 500)
		{
			return null;
		}
		TileObject tileObject = null;
		for (int i = 0; i < freeObjectsCount; i++)
		{
			if (freeObjects[i].tileObjectId == desiredObject && !freeObjects[i].active)
			{
				tileObject = freeObjects[i];
				break;
			}
		}
		if (tileObject == null)
		{
			tileObject = Object.Instantiate(allObjects[desiredObject].gameObject, position, Quaternion.identity).GetComponent<TileObject>();
			tileObject._transform = tileObject.transform;
			tileObject._gameObject = tileObject.gameObject;
			freeObjects.Add(tileObject);
			freeObjectsCount++;
		}
		tileObject._transform.localPosition = position;
		tileObject.getRotation((int)position.x / 2, (int)position.z / 2);
		return tileObject;
	}

	public TileObject getTileObjectForShopInterior(int desiredObject, Vector3 position)
	{
		if (desiredObject == 500)
		{
			return null;
		}
		TileObject component = Object.Instantiate(allObjects[desiredObject].gameObject, position, Quaternion.identity).GetComponent<TileObject>();
		Object.Destroy(component.GetComponentInChildren<MineEnterExit>());
		component._transform = component.transform;
		component._transform.localPosition = position;
		component.getRotation((int)position.x / 2, (int)position.z / 2);
		return component;
	}

	public void returnTileObject(TileObject returnedObject)
	{
		returnedObject.currentHealth = allObjectSettings[returnedObject.tileObjectId].fullHealth;
		returnedObject.active = false;
		returnedObject.gameObject.SetActive(false);
	}

	public void destroyTileObject(TileObject returnedObject)
	{
		freeObjects.Remove(returnedObject);
		freeObjectsCount--;
		Object.Destroy(returnedObject.gameObject);
	}

	public void nextDay()
	{
		GenerateUndergroundMap.generate.generateMineSeedForNewDay();
		NetworkMapSharer.share.NetworkcraftsmanWorking = false;
		NetworkMapSharer.share.syncLicenceLevels();
		NetworkMapSharer.share.NetworknextDayIsReady = false;
		updateAllChangers();
		NetworkMapSharer.share.RpcAddADay(NetworkMapSharer.share.mineSeed);
	}

	public void updateAllChangers()
	{
		for (int i = 0; i < allChangers.Count; i++)
		{
			if (allChangers[i].counterDays == 0)
			{
				if (RealWorldTimeLight.time.currentHour == 0)
				{
					allChangers[i].counterSeconds -= 420;
				}
				else
				{
					allChangers[i].counterSeconds -= (24 - RealWorldTimeLight.time.currentHour) * 120 + 840;
				}
			}
			else
			{
				allChangers[i].counterDays--;
			}
		}
	}

	public void addToCropChecker()
	{
		completedCropChecker++;
	}

	public int getNoOfCompletedCrops()
	{
		return completedCropChecker;
	}

	public void doNextDayChange()
	{
		StartCoroutine(nextDayChanges(false, Random.Range(-200000, 200000)));
	}

	public IEnumerator nextDayChanges(bool raining, int mineSeed)
	{
		List<int[]> sprinklerPos = new List<int[]>();
		List<int[]> waterTankPos = new List<int[]>();
		chunkHasChangedToday = new bool[mapSize / 10, mapSize / 10];
		int grassType = 1;
		int tropicalGrassType = 4;
		int pineGrassType = 15;
		int chunkCounter = 0;
		completedCropChecker = 0;
		for (int chunkY = 0; chunkY < mapSize / 10; chunkY++)
		{
			for (int chunkX = 0; chunkX < mapSize / 10; chunkX++)
			{
				if (!chunkChangedMap[chunkX, chunkY])
				{
					continue;
				}
				Random.InitState(mineSeed + chunkX * chunkY);
				for (int i = chunkY * 10; i < chunkY * 10 + 10; i++)
				{
					for (int j = chunkX * 10; j < chunkX * 10 + 10; j++)
					{
						if (onTileMap[j, i] >= -1)
						{
							if (onTileMap[j, i] == -1)
							{
								if (tileTypeMap[j, i] == 14 && (!waterMap[j, i] || (waterMap[j, i] && heightMap[j, i] >= -1)))
								{
									GenerateMap.generate.mangroveGrowback.getRandomObjectAndPlaceWithGrowth(j, i);
									if (onTileMap[j, i] != -1)
									{
										chunkHasChangedToday[chunkX, chunkY] = true;
									}
								}
								if (tileTypeMap[j, i] == 3 && GenerateMap.generate.checkBiomType(j, i) == 16)
								{
									GenerateMap.generate.beachGrowBack.getRandomObjectAndPlaceWithGrowth(j, i);
									if (onTileMap[j, i] != -1)
									{
										chunkHasChangedToday[chunkX, chunkY] = true;
									}
								}
								if (GenerateMap.generate.checkBiomType(j, i) == 12 && tileTypeMap[j, i] == 18 && checkAllNeighboursAreEmpty(j, i))
								{
									if (NetworkMapSharer.share.miningLevel == 2)
									{
										onTileMap[j, i] = GenerateMap.generate.quaryGrowBack1.getBiomObject();
									}
									else if (NetworkMapSharer.share.miningLevel == 3)
									{
										onTileMap[j, i] = GenerateMap.generate.quaryGrowBack2.getBiomObject();
									}
									else
									{
										onTileMap[j, i] = GenerateMap.generate.quaryGrowBack0.getBiomObject();
									}
									if (onTileMap[j, i] != -1)
									{
										chunkHasChangedToday[chunkX, chunkY] = true;
									}
								}
								if (tileTypeMap[j, i] == grassType || tileTypeMap[j, i] == tropicalGrassType || tileTypeMap[j, i] == pineGrassType)
								{
									if (tileTypeMap[j, i] == grassType)
									{
										GenerateMap.generate.bushLandGrowBack.getRandomObjectAndPlaceWithGrowth(j, i);
									}
									else if (tileTypeMap[j, i] == tropicalGrassType)
									{
										onTileMap[j, i] = GenerateMap.generate.tropicalGrowBack.getBiomObject();
									}
									else if (tileTypeMap[j, i] == pineGrassType)
									{
										onTileMap[j, i] = GenerateMap.generate.coldLandGrowBack.getBiomObject();
									}
									if (onTileMap[j, i] > -1)
									{
										if ((bool)allObjects[onTileMap[j, i]].tileObjectGrowthStages)
										{
											onTileStatusMap[j, i] = 0;
										}
										chunkHasChangedToday[chunkX, chunkY] = true;
									}
								}
							}
							else if (allObjectSettings[onTileMap[j, i]].isFlowerBed && onTileStatusMap[j, i] <= 0)
							{
								if (j != 0 && i != 0 && j < mapSize && i < mapSize)
								{
									switch (Random.Range(0, 7))
									{
									case 0:
										if (onTileMap[j + 1, i] > -1 && allObjectSettings[onTileMap[j + 1, i]].isFlowerBed && onTileStatusMap[j + 1, i] > 0)
										{
											onTileStatusMap[j, i] = onTileStatusMap[j + 1, i];
											chunkHasChangedToday[chunkX, chunkY] = true;
										}
										break;
									case 1:
										if (onTileMap[j - 1, i] > -1 && allObjectSettings[onTileMap[j - 1, i]].isFlowerBed && onTileStatusMap[j - 1, i] > 0)
										{
											onTileStatusMap[j, i] = onTileStatusMap[j - 1, i];
											chunkHasChangedToday[chunkX, chunkY] = true;
										}
										break;
									case 2:
										if (onTileMap[j, i + 1] > -1 && allObjectSettings[onTileMap[j, i + 1]].isFlowerBed && onTileStatusMap[j, i + 1] > 0)
										{
											onTileStatusMap[j, i] = onTileStatusMap[j, i + 1];
											chunkHasChangedToday[chunkX, chunkY] = true;
										}
										break;
									case 3:
										if (onTileMap[j, i - 1] > -1 && allObjectSettings[onTileMap[j, i - 1]].isFlowerBed && onTileStatusMap[j, i - 1] > 0)
										{
											onTileStatusMap[j, i] = onTileStatusMap[j, i - 1];
											chunkHasChangedToday[chunkX, chunkY] = true;
										}
										break;
									}
								}
							}
							else if ((bool)allObjects[onTileMap[j, i]].sprinklerTile)
							{
								int[] item = new int[2] { j, i };
								if (allObjects[onTileMap[j, i]].sprinklerTile.isTank)
								{
									waterTankPos.Add(item);
								}
								else
								{
									sprinklerPos.Add(item);
								}
							}
							else if (allObjects[onTileMap[j, i]].hasExtensions)
							{
								if (onTileMap[j, i] == 28)
								{
									onTileMap[j, i] = -1;
									chunkHasChangedToday[chunkX, chunkY] = true;
								}
								else if ((bool)allObjects[onTileMap[j, i]].tileObjectGrowthStages)
								{
									allObjects[onTileMap[j, i]].tileObjectGrowthStages.checkIfShouldGrow(j, i);
									if ((bool)allObjects[onTileMap[j, i]].tileObjectGrowthStages && allObjects[onTileMap[j, i]].tileObjectGrowthStages.checkIfShouldDie(j, i) && (bool)allObjects[onTileMap[j, i]].tileObjectGrowthStages.changeToWhenDead)
									{
										onTileMap[j, i] = allObjects[onTileMap[j, i]].tileObjectGrowthStages.changeToWhenDead.tileObjectId;
										onTileStatusMap[j, i] = Mathf.Clamp(onTileStatusMap[j, i], 0, allObjects[onTileMap[j, i]].tileObjectGrowthStages.objectStages.Length);
										if (tileTypeMap[j, i] == 12 || tileTypeMap[j, i] == 13)
										{
											tileTypeMap[j, i] = 7;
										}
									}
									chunkHasChangedToday[chunkX, chunkY] = true;
								}
							}
						}
						if (!raining)
						{
							if (tileTypes[tileTypeMap[j, i]].dryVersion != -1)
							{
								tileTypeMap[j, i] = tileTypes[tileTypeMap[j, i]].dryVersion;
								chunkHasChangedToday[chunkX, chunkY] = true;
							}
							if (tileTypeMap[j, i] == 12 || tileTypeMap[j, i] == 13)
							{
								if ((onTileMap[j, i] <= -1 || !allObjects[onTileMap[j, i]].tileObjectGrowthStages || !allObjects[onTileMap[j, i]].tileObjectGrowthStages.needsTilledSoil) && Random.Range(0, 4) == 2)
								{
									tileTypeMap[j, i] = 7;
									chunkHasChangedToday[chunkX, chunkY] = true;
								}
							}
							else if ((tileTypeMap[j, i] == 7 || tileTypeMap[j, i] == 8) && (onTileMap[j, i] <= -1 || !allObjects[onTileMap[j, i]].tileObjectGrowthStages || !allObjects[onTileMap[j, i]].tileObjectGrowthStages.needsTilledSoil) && Random.Range(0, 3) == 2 && NetworkMapSharer.share.isServer)
							{
								NetworkMapSharer.share.RpcUpdateTileType(tileTypeStatusMap[j, i], j, i);
							}
						}
						else if (tileTypes[tileTypeMap[j, i]].wetVersion != -1)
						{
							tileTypeMap[j, i] = tileTypes[tileTypeMap[j, i]].wetVersion;
							chunkHasChangedToday[chunkX, chunkY] = true;
						}
					}
				}
				if (chunkCounter >= 10)
				{
					chunkCounter = 0;
					yield return null;
				}
				else
				{
					chunkCounter++;
				}
			}
		}
		foreach (int[] item2 in sprinklerPos)
		{
			allObjects[onTileMap[item2[0], item2[1]]].sprinklerTile.waterTiles(item2[0], item2[1], waterTankPos);
		}
		StartCoroutine(refreshAllChunksNewDay());
	}

	public bool checkAllNeighboursAreEmpty(int x, int y)
	{
		if (onTileMap[Mathf.Clamp(x, 0, mapSize - 1), Mathf.Clamp(y + 1, 0, mapSize - 1)] == -1 && onTileMap[Mathf.Clamp(x, 0, mapSize - 1), Mathf.Clamp(y - 1, 0, mapSize - 1)] == -1 && onTileMap[Mathf.Clamp(x + 1, 0, mapSize - 1), Mathf.Clamp(y, 0, mapSize - 1)] == -1 && onTileMap[Mathf.Clamp(x - 1, 0, mapSize - 1), Mathf.Clamp(y, 0, mapSize - 1)] == -1)
		{
			return true;
		}
		return false;
	}

	public void sprinkerContinuesToWater(int xPos, int yPos)
	{
		StartCoroutine(continueWateringSprinkler(xPos, yPos));
	}

	private IEnumerator continueWateringSprinkler(int xPos, int yPos)
	{
		if (!allObjects[onTileMap[xPos, yPos]].sprinklerTile)
		{
			yield break;
		}
		while (onTileMap[xPos, yPos] > -1 && onTileStatusMap[xPos, yPos] != 0 && RealWorldTimeLight.time.currentHour >= 1 && RealWorldTimeLight.time.currentHour < 9)
		{
			yield return new WaitForSeconds(0.25f);
			for (int x = -allObjects[onTileMap[xPos, yPos]].sprinklerTile.horizontalSize; x < allObjects[onTileMap[xPos, yPos]].sprinklerTile.horizontalSize + 1; x++)
			{
				for (int y = -allObjects[onTileMap[xPos, yPos]].sprinklerTile.verticlSize; y < allObjects[onTileMap[xPos, yPos]].sprinklerTile.verticlSize + 1; y++)
				{
					if (tileTypes[tileTypeMap[xPos + x, yPos + y]].wetVersion != -1)
					{
						NetworkMapSharer.share.RpcUpdateTileType(tileTypes[tileTypeMap[xPos + x, yPos + y]].wetVersion, xPos + x, yPos + y);
						yield return new WaitForSeconds(0.1f);
					}
				}
			}
		}
	}

	public int getChunkSize()
	{
		return chunkSize;
	}

	public int getTileSize()
	{
		return tileSize;
	}

	public int getMapSize()
	{
		return mapSize;
	}

	public void startCountDownForTile(int itemId, int xPos, int yPos, HouseDetails inside = null)
	{
		CurrentChanger currentChanger = new CurrentChanger(xPos, yPos);
		ItemChange itemChange = Inventory.inv.allItems[itemId].itemChange;
		if (itemChange == null)
		{
			return;
		}
		if (inside != null)
		{
			currentChanger.houseX = inside.xPos;
			currentChanger.houseY = inside.yPos;
			currentChanger.counterSeconds = itemChange.getChangeTime(inside.houseMapOnTile[xPos, yPos]);
			currentChanger.counterDays = itemChange.getChangeDays(inside.houseMapOnTile[xPos, yPos]);
			currentChanger.cycles = itemChange.getCycles(inside.houseMapOnTile[xPos, yPos]);
			currentChanger.timePerCycles = currentChanger.counterSeconds;
			if (currentChanger.counterDays <= 0)
			{
			}
		}
		else
		{
			currentChanger.houseX = -1;
			currentChanger.houseY = -1;
			currentChanger.counterSeconds = itemChange.getChangeTime(onTileMap[xPos, yPos]);
			currentChanger.counterDays = itemChange.getChangeDays(onTileMap[xPos, yPos]);
			currentChanger.cycles = itemChange.getCycles(onTileMap[xPos, yPos]);
			currentChanger.timePerCycles = currentChanger.counterSeconds;
		}
		allChangers.Add(currentChanger);
		StartCoroutine(countDownPos(currentChanger));
	}

	public void loadCountDownForTile(CurrentChanger thisChanger)
	{
		allChangers.Add(thisChanger);
		StartCoroutine(countDownPos(thisChanger));
	}

	public bool checkIfTileHasChanger(int xPos, int yPos, HouseDetails house = null)
	{
		if (house == null && onTileMap[xPos, yPos] > -1 && (!allObjects[onTileMap[xPos, yPos]].tileObjectItemChanger || ((bool)allObjects[onTileMap[xPos, yPos]].tileObjectItemChanger && onTileStatusMap[xPos, yPos] <= 0)))
		{
			MonoBehaviour.print("Doesn't need a countdown");
			return false;
		}
		if (house != null && house.houseMapOnTile[xPos, yPos] > -1 && (!allObjects[house.houseMapOnTile[xPos, yPos]].tileObjectItemChanger || ((bool)allObjects[house.houseMapOnTile[xPos, yPos]].tileObjectItemChanger && house.houseMapOnTileStatus[xPos, yPos] <= 0)))
		{
			MonoBehaviour.print("Doesn't need a countdown");
			return false;
		}
		for (int i = 0; i < allChangers.Count; i++)
		{
			if (house == null)
			{
				if (allChangers[i].xPos == xPos && allChangers[i].yPos == yPos && allChangers[i].houseX == -1 && allChangers[i].houseY == -1)
				{
					return true;
				}
			}
			else if (allChangers[i].xPos == xPos && allChangers[i].yPos == yPos && allChangers[i].houseX == house.xPos && allChangers[i].houseY == house.yPos)
			{
				return true;
			}
		}
		MonoBehaviour.print("Item changer had no timer, so you can pick it up");
		if (house == null)
		{
			NetworkMapSharer.share.RpcGiveOnTileStatus(-2, xPos, yPos);
		}
		else
		{
			NetworkMapSharer.share.RpcGiveOnTileStatusInside(-2, xPos, yPos, house.xPos, house.yPos);
		}
		return false;
	}

	private bool checkNeighbourIsWater(int xPos, int yPos)
	{
		if (waterMap[xPos + 1, yPos])
		{
			return true;
		}
		if (waterMap[xPos - 1, yPos])
		{
			return true;
		}
		if (waterMap[xPos, yPos + 1])
		{
			return true;
		}
		if (waterMap[xPos, yPos - 1])
		{
			return true;
		}
		return false;
	}

	public void doWaterCheckOnHeightChange(int xPos, int yPos)
	{
		StartCoroutine(checkWaterAndFlow(xPos, yPos));
	}

	private IEnumerator checkWaterAndFlow(int xPos, int yPos)
	{
		yield return waterSec;
		if (heightMap[xPos, yPos] <= 0 && !waterMap[xPos, yPos] && xPos != 0 && yPos != 0 && yPos != mapSize - 1 && xPos != mapSize - 1 && checkNeighbourIsWater(xPos, yPos))
		{
			NetworkMapSharer.share.RpcFillWithWater(xPos, yPos);
			waterMap[xPos, yPos] = true;
			if (heightMap[xPos + 1, yPos] <= 0 && !waterMap[xPos + 1, yPos])
			{
				StartCoroutine(checkWaterAndFlow(xPos + 1, yPos));
			}
			if (heightMap[xPos - 1, yPos] <= 0 && !waterMap[xPos - 1, yPos])
			{
				StartCoroutine(checkWaterAndFlow(xPos - 1, yPos));
			}
			if (heightMap[xPos, yPos + 1] <= 0 && !waterMap[xPos, yPos + 1])
			{
				StartCoroutine(checkWaterAndFlow(xPos, yPos + 1));
			}
			if (heightMap[xPos, yPos - 1] <= 0 && !waterMap[xPos, yPos - 1])
			{
				StartCoroutine(checkWaterAndFlow(xPos, yPos - 1));
			}
		}
	}

	private IEnumerator countDownPos(CurrentChanger change)
	{
		HouseDetails inside = null;
		if (change.houseX != -1 && change.houseY != -1)
		{
			inside = HouseManager.manage.getHouseInfo(change.houseX, change.houseY);
		}
		bool doubleSpeed = false;
		if (change.cycles == 0)
		{
			change.cycles = 1;
		}
		while (change.cycles > 0)
		{
			change.counterSeconds = change.timePerCycles;
			if (inside == null && onTileMap[change.xPos, change.yPos] > -1 && allObjects[onTileMap[change.xPos, change.yPos]].tileObjectItemChanger.useWindMill)
			{
				for (int i = -14; i < 15; i++)
				{
					for (int j = -14; j < 15; j++)
					{
						if (isPositionOnMap(change.xPos + i, change.yPos + j) && onTileMap[change.xPos + i, change.yPos + j] == 16)
						{
							doubleSpeed = true;
							break;
						}
					}
				}
			}
			while (change.counterSeconds > 0 || change.counterDays > 0)
			{
				yield return sec;
				if (change.counterDays <= 0)
				{
					if (doubleSpeed)
					{
						change.counterSeconds -= 2;
					}
					else
					{
						change.counterSeconds--;
					}
				}
			}
			while (!NetworkMapSharer.share.nextDayIsReady)
			{
				yield return sec;
			}
			while (change.startedUnderground != RealWorldTimeLight.time.underGround)
			{
				yield return sec;
			}
			while (!NetworkMapSharer.share.serverActive())
			{
				yield return sec;
			}
			TileObject tileObject = null;
			if (inside != null)
			{
				DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(inside.xPos, inside.yPos);
				tileObject = manageWorld.getTileObjectForHouse(inside.houseMapOnTile[change.xPos, change.yPos], displayPlayerHouseTiles.getStartingPosTransform().position + new Vector3(change.xPos * 2, 0f, change.yPos * 2), change.xPos, change.yPos, inside);
			}
			else if (onTileMap[change.xPos, change.yPos] > 0)
			{
				tileObject = getTileObjectForServerDrop(onTileMap[change.xPos, change.yPos], new Vector3(change.xPos * 2, heightMap[change.xPos, change.yPos], change.yPos * 2));
			}
			if ((bool)tileObject)
			{
				if ((bool)tileObject.tileObjectItemChanger)
				{
					tileObject.tileObjectItemChanger.ejectItemOnCycle(change.xPos, change.yPos, inside);
				}
				returnTileObject(tileObject);
			}
			change.cycles--;
		}
		if (inside != null)
		{
			NetworkMapSharer.share.RpcEjectItemFromChangerInside(change.xPos, change.yPos, inside.xPos, inside.yPos);
		}
		else
		{
			NetworkMapSharer.share.RpcEjectItemFromChanger(change.xPos, change.yPos);
		}
		yield return null;
		allChangers.Remove(change);
	}

	private bool checkIfOnMap(int intToCheck, bool inside)
	{
		if (inside)
		{
			if (intToCheck >= 0 && intToCheck < 25)
			{
				return true;
			}
			return false;
		}
		if (intToCheck >= 0 && intToCheck < manageWorld.getMapSize())
		{
			return true;
		}
		return false;
	}

	public Vector2 findMultiTileObjectPos(int xPos, int yPos, HouseDetails house = null)
	{
		if (house == null)
		{
			if (onTileMap[xPos, yPos] >= -1)
			{
				return new Vector2(xPos, yPos);
			}
			bool flag = false;
			bool flag2 = false;
			int num = 0;
			int num2 = 0;
			bool inside = false;
			int num3 = 1000;
			while (!flag || !flag2)
			{
				num3--;
				if (num3 <= 0)
				{
					Debug.Log("Search size reached - This should never be called.");
					return new Vector2(xPos, yPos);
				}
				if (!flag2)
				{
					if (checkIfOnMap(xPos + num, inside))
					{
						if (onTileMap[xPos + num, yPos] == -3)
						{
							flag2 = true;
						}
						else if (onTileMap[xPos + num, yPos] == -4 && checkIfOnMap(xPos + (num - 1), inside) && onTileMap[xPos + (num - 1), yPos] != -4)
						{
							num--;
							flag2 = true;
						}
						else
						{
							num--;
						}
					}
					else
					{
						num = 0;
						flag2 = true;
					}
				}
				if (!flag2)
				{
					continue;
				}
				if (checkIfOnMap(yPos + num2, inside))
				{
					if (onTileMap[xPos + num, yPos + num2] != -3)
					{
						flag = true;
					}
					else if (onTileMap[xPos + num, yPos + num2] == -3 && checkIfOnMap(yPos + (num2 - 1), inside) && onTileMap[xPos + num, yPos + (num2 - 1)] != -3)
					{
						num2--;
						flag = true;
					}
					else
					{
						num2--;
					}
				}
				else
				{
					num2 = 0;
					flag = true;
				}
			}
			xPos += num;
			yPos += num2;
			return new Vector2(xPos, yPos);
		}
		if (house.houseMapOnTile[xPos, yPos] >= -1)
		{
			return new Vector2(xPos, yPos);
		}
		bool flag3 = false;
		bool flag4 = false;
		int num4 = 0;
		int num5 = 0;
		bool inside2 = true;
		int num6 = 1000;
		while (!flag3 || !flag4)
		{
			num6--;
			if (num6 <= 0)
			{
				Debug.Log("Search size reached - This should never be called.");
				return new Vector2(xPos, yPos);
			}
			if (!flag4)
			{
				if (checkIfOnMap(xPos + num4, inside2))
				{
					if (house.houseMapOnTile[xPos + num4, yPos] == -3)
					{
						flag4 = true;
					}
					else if (house.houseMapOnTile[xPos + num4, yPos] == -4 && checkIfOnMap(xPos + (num4 - 1), inside2) && house.houseMapOnTile[xPos + (num4 - 1), yPos] != -4)
					{
						num4--;
						flag4 = true;
					}
					else
					{
						num4--;
					}
				}
				else
				{
					num4 = 0;
					flag4 = true;
				}
			}
			if (!flag4)
			{
				continue;
			}
			if (checkIfOnMap(yPos + num5, inside2))
			{
				if (house.houseMapOnTile[xPos + num4, yPos + num5] != -3)
				{
					flag3 = true;
				}
				else if (house.houseMapOnTile[xPos + num4, yPos + num5] == -3 && checkIfOnMap(yPos + (num5 - 1), inside2) && house.houseMapOnTile[xPos + num4, yPos + (num5 - 1)] != -3)
				{
					num5--;
					flag3 = true;
				}
				else
				{
					num5--;
				}
			}
			else
			{
				num5 = 0;
				flag3 = true;
			}
		}
		xPos += num4;
		yPos += num5;
		return new Vector2(xPos, yPos);
	}

	public Vector3 findTileObjectAround(Vector3 position, TileObject[] lookingForObjects, int distance = 5, bool checkIfFencedOff = false)
	{
		int num = (int)position.x / 2;
		int num2 = (int)position.z / 2;
		Vector3 result = Vector3.zero;
		int num3 = fencedOffMap[num, num2];
		for (int i = -distance; i < distance; i++)
		{
			for (int j = -distance; j < distance; j++)
			{
				for (int k = 0; k < lookingForObjects.Length; k++)
				{
					if (onTileMap[num + i, num2 + j] == lookingForObjects[k].tileObjectId && (!allObjects[onTileMap[num + i, num2 + j]].tileOnOff || ((bool)allObjects[onTileMap[num + i, num2 + j]].tileOnOff && onTileStatusMap[num + i, num2 + j] == 1)) && (!checkIfFencedOff || (checkIfFencedOff && num3 == fencedOffMap[num + i, num2 + j])))
					{
						if (checkIfFencedOff || Random.Range(0, 3) == 2)
						{
							return new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2);
						}
						result = new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2);
					}
				}
			}
		}
		return result;
	}

	public Vector3 findDroppedObjectAround(Vector3 position, InventoryItem[] lookingForObjects, float distance = 5f, bool checkIfFencedOff = false)
	{
		Vector3 result = Vector3.zero;
		int num = fencedOffMap[(int)position.x / 2, (int)position.z / 2];
		if (Physics.CheckSphere(position, distance, pickUpLayer))
		{
			Collider[] array = Physics.OverlapSphere(position, distance, pickUpLayer);
			if (array.Length != 0)
			{
				float num2 = distance + 2f;
				for (int i = 0; i < array.Length; i++)
				{
					if (!(Vector3.Distance(position, array[i].transform.position) < num2) || (checkIfFencedOff && (!checkIfFencedOff || fencedOffMap[(int)array[i].transform.position.x / 2, (int)array[i].transform.position.z / 2] != num)))
					{
						continue;
					}
					DroppedItem componentInParent = array[i].GetComponentInParent<DroppedItem>();
					if (!componentInParent)
					{
						continue;
					}
					for (int j = 0; j < lookingForObjects.Length; j++)
					{
						if (componentInParent.myItemId == Inventory.inv.getInvItemId(lookingForObjects[j]))
						{
							num2 = Vector3.Distance(position, componentInParent.transform.position);
							result = new Vector3(componentInParent.onTile.x * 2f, heightMap[Mathf.RoundToInt(componentInParent.transform.position.x) / 2, Mathf.RoundToInt(componentInParent.transform.position.z) / 2], componentInParent.onTile.y * 2f);
						}
					}
				}
			}
		}
		return result;
	}

	public bool isSeatTaken(Vector3 seatPos, int desiredPos = -1)
	{
		int num = onTileMap[(int)seatPos.x / 2, (int)seatPos.z / 2];
		if (onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] == 3)
		{
			return true;
		}
		if (num < 0 || !allObjects[num].tileObjectFurniture)
		{
			return true;
		}
		if (!allObjects[num].tileObjectFurniture.seatPosition2 && desiredPos == 2)
		{
			return true;
		}
		switch (desiredPos)
		{
		case -1:
			if ((bool)allObjects[num].tileObjectFurniture.seatPosition2)
			{
				if (onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] == 3)
				{
					return true;
				}
				return false;
			}
			if (onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] == 1)
			{
				return true;
			}
			return false;
		case 1:
			if (onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] == 2 || onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] <= 0)
			{
				return false;
			}
			return true;
		case 2:
			if (onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] == 1 || onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] <= 0)
			{
				return false;
			}
			return true;
		default:
			return false;
		}
	}

	public bool hasSquareBeenWatered(Vector3 cropPos)
	{
		int num = (int)cropPos.x / 2;
		int num2 = (int)cropPos.z / 2;
		if (manageWorld.tileTypeMap[num, num2] == 8 || manageWorld.tileTypeMap[num, num2] == 13)
		{
			return true;
		}
		return false;
	}

	public Vector3 findClosestTileObjectAround(Vector3 position, TileObject[] lookingForObjects, int distance = 5, bool checkIfWatered = false, bool checkIfSeatEmpty = false)
	{
		int num = (int)position.x / 2;
		int num2 = (int)position.z / 2;
		float num3 = (float)distance * 2f;
		float num4 = num3;
		Vector3 result = Vector3.zero;
		int num5 = fencedOffMap[num, num2];
		for (int i = -distance; i < distance; i++)
		{
			for (int j = -distance; j < distance; j++)
			{
				for (int k = 0; k < lookingForObjects.Length; k++)
				{
					if (onTileMap[num + i, num2 + j] == lookingForObjects[k].tileObjectId && ((!checkIfSeatEmpty && !checkIfWatered) || (checkIfWatered && !hasSquareBeenWatered(new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2))) || (checkIfSeatEmpty && !isSeatTaken(new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2)))))
					{
						num4 = Vector3.Distance(new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2), position);
						if (num4 < num3)
						{
							num3 = num4;
							result = new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2);
						}
					}
				}
			}
		}
		return result;
	}

	public Vector3 findClosestWaterTile(Vector3 position, int distance = 5, bool checkIfFencedOff = false, int depth = 0)
	{
		int num = (int)position.x / 2;
		int num2 = (int)position.z / 2;
		Vector3 result = Vector3.zero;
		int num3 = fencedOffMap[num, num2];
		float num4 = (float)distance * 0.1f;
		float num5 = num4;
		for (int i = -distance; i < distance; i++)
		{
			for (int j = -distance; j < distance; j++)
			{
				if (waterMap[num + i, num2 + j] && heightMap[num + i, num2 + j] <= depth && (!checkIfFencedOff || (checkIfFencedOff && num3 == fencedOffMap[num + i, num2 + j])))
				{
					num5 = Vector3.Distance(new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2), position);
					if (num5 < num4)
					{
						num4 = num5;
						result = new Vector3((num + i) * 2, heightMap[num + i, num2 + j], (num2 + j) * 2);
					}
				}
			}
		}
		return result;
	}

	private bool chunkNeedsFenceCheck(bool[,] hadMapCheck, int x, int y)
	{
		if (x == 0 || y == 0)
		{
			return false;
		}
		if (hadMapCheck[x - 1, y] || hadMapCheck[x, y - 1])
		{
			return true;
		}
		return false;
	}

	public void resetAllChunkChangedMaps()
	{
		clientRequestedMap = new bool[mapSize / 10, mapSize / 10];
		chunkChangedMap = new bool[mapSize / 10, mapSize / 10];
		changedMapWater = new bool[mapSize / 10, mapSize / 10];
		changedMapHeight = new bool[mapSize / 10, mapSize / 10];
		changedMapOnTile = new bool[mapSize / 10, mapSize / 10];
		changedMapTileType = new bool[mapSize / 10, mapSize / 10];
	}

	public bool isPositionOnMap(int xPos, int yPos)
	{
		if (xPos < 0 || xPos >= manageWorld.getMapSize() || yPos < 0 || yPos >= manageWorld.getMapSize())
		{
			return false;
		}
		return true;
	}

	public bool isPositionOnMap(Vector3 position)
	{
		int num = (int)position.x / 2;
		int num2 = (int)position.z / 2;
		if (num < 0 || num >= manageWorld.getMapSize() || num2 < 0 || num2 >= manageWorld.getMapSize())
		{
			return false;
		}
		return true;
	}

	private bool isAFence(int xPos, int yPos)
	{
		return fencedOffMap[xPos, yPos] == 1;
	}

	public IEnumerator fenceCheck()
	{
		int size = mapSize / chunkSize;
		bool[,] hadMapCheck = new bool[size, size];
		int yieldAmount = 0;
		for (int y = 1; y < size; y++)
		{
			for (int x = 1; x < size; x++)
			{
				if (chunkWithFenceInIt[x, y] || chunkNeedsFenceCheck(hadMapCheck, x, y))
				{
					if (!changedMapOnTile[x, y] && !chunkNeedsFenceCheck(hadMapCheck, x, y))
					{
						continue;
					}
					hadMapCheck[x, y] = true;
					for (int i = 0; i < 10; i++)
					{
						for (int j = 0; j < 10; j++)
						{
							if (fencedOffMap[x * 10 + j, y * 10 + i] == 1)
							{
								continue;
							}
							if (fencedOffMap[x * 10 + j - 1, y * 10 + i] > 0 && fencedOffMap[x * 10 + j, y * 10 + i - 1] > 0)
							{
								fencedOffMap[x * 10 + j, y * 10 + i] = 2;
								continue;
							}
							fencedOffMap[x * 10 + j, y * 10 + i] = 0;
							int num = i - 1;
							int num2 = j - 1;
							while (fencedOffMap[x * 10 + num2, y * 10 + i] > 1)
							{
								fencedOffMap[x * 10 + num2, y * 10 + i] = 0;
								for (int k = i + 1; fencedOffMap[x * 10 + num2, y * 10 + k] > 1; k++)
								{
									fencedOffMap[x * 10 + num2, y * 10 + k] = 0;
								}
								int num3 = i - 1;
								while (fencedOffMap[x * 10 + num2, y * 10 + num3] > 1)
								{
									fencedOffMap[x * 10 + num2, y * 10 + num3] = 0;
									num3--;
								}
								num2--;
							}
							while (fencedOffMap[x * 10 + j, y * 10 + num] > 1)
							{
								fencedOffMap[x * 10 + j, y * 10 + num] = 0;
								for (int l = j + 1; fencedOffMap[x * 10 + l, y * 10 + num] > 1; l++)
								{
									fencedOffMap[x * 10 + l, y * 10 + num] = 0;
								}
								num2 = j - 1;
								while (fencedOffMap[x * 10 + num2, y * 10 + num] > 1)
								{
									fencedOffMap[x * 10 + num2, y * 10 + num] = 0;
									num2--;
								}
								num--;
							}
						}
					}
					yieldAmount++;
					if (yieldAmount >= 100)
					{
						yieldAmount = 0;
						yield return null;
					}
				}
				else
				{
					yieldAmount++;
					if (yieldAmount >= 500)
					{
						yieldAmount = 0;
						yield return null;
					}
				}
			}
		}
		yield return StartCoroutine(labelFencedOffAreas());
	}

	public IEnumerator labelFencedOffAreas()
	{
		int size = mapSize / chunkSize;
		bool[,] hadMapCheck = new bool[size, size];
		int fenceGroup = 3;
		int yieldAmount = 0;
		for (int y = 0; y < size; y++)
		{
			for (int x = 0; x < size; x++)
			{
				if (chunkWithFenceInIt[x, y])
				{
					if (!changedMapOnTile[x, y] && !chunkNeedsFenceCheck(hadMapCheck, x, y))
					{
						continue;
					}
					for (int i = 0; i < 10; i++)
					{
						for (int j = 0; j < 10; j++)
						{
							if (fencedOffMap[x * 10 + j, y * 10 + i] == 2)
							{
								fenceGroup = ((fencedOffMap[x * 10 + j, y * 10 + i - 1] > 2) ? fencedOffMap[x * 10 + j, y * 10 + i - 1] : ((fencedOffMap[x * 10 + j, y * 10 + i + 1] > 2) ? fencedOffMap[x * 10 + j, y * 10 + i + 1] : ((fencedOffMap[x * 10 + j - 1, y * 10 + i] > 2) ? fencedOffMap[x * 10 + j - 1, y * 10 + i] : ((fencedOffMap[x * 10 + j + 1, y * 10 + i] <= 2) ? (fenceGroup + 1) : fencedOffMap[x * 10 + j + 1, y * 10 + i]))));
								fencedOffMap[x * 10 + j, y * 10 + i] = fenceGroup;
								int num = j - 1;
								while (fencedOffMap[x * 10 + num, y * 10 + i] != fenceGroup && fencedOffMap[x * 10 + num, y * 10 + i] > 1)
								{
									fencedOffMap[x * 10 + num, y * 10 + i] = fenceGroup;
									num--;
								}
							}
						}
					}
					yieldAmount++;
					if (yieldAmount >= 100)
					{
						yieldAmount = 0;
						yield return null;
					}
				}
				else
				{
					yieldAmount++;
					if (yieldAmount >= 500)
					{
						yieldAmount = 0;
						yield return null;
					}
				}
			}
		}
	}

	public void findSpaceForDropAfterTileObjectChange(int xPos, int yPos)
	{
		List<DroppedItem> allDropsOnTile = getAllDropsOnTile(xPos, yPos);
		for (int i = 0; i < allDropsOnTile.Count; i++)
		{
			Vector3 vector = moveDropPosToSafeOutside(allDropsOnTile[i].transform.position, false);
			allDropsOnTile[i].setDesiredPos(vector.y, vector.x, vector.z);
		}
	}

	public bool isOnTileEmpty(int xPos, int yPos)
	{
		return onTileMap[xPos, yPos] == -1;
	}

	public TileObjectSettings getTileObjectSettings(int xPos, int yPos)
	{
		if (onTileMap[xPos, yPos] > -1)
		{
			return allObjectSettings[onTileMap[xPos, yPos]];
		}
		return null;
	}

	public void addToChunksToRefreshList(int xPos, int yPos)
	{
		if (!chunksToRefresh.Contains(new int[2] { xPos, yPos }))
		{
			chunksToRefresh.Add(new int[2] { xPos, yPos });
		}
	}

	public void refreshChunksInChunksToRefreshList()
	{
		for (int i = 0; i < chunksToRefresh.Count; i++)
		{
			refreshTileObjectsOnChunksInUse(chunksToRefresh[i][0], chunksToRefresh[i][1]);
		}
		chunksToRefresh.Clear();
	}

	public bool canReleaseTrapHere(int xPos, int yPos, float height)
	{
		if ((int)height < heightMap[xPos, yPos] || (float)(int)height > (float)heightMap[xPos, yPos] + 2f)
		{
			return false;
		}
		if (onTileMap[xPos, yPos] == -1 || onTileMap[xPos, yPos] == 30)
		{
			return true;
		}
		if (onTileMap[xPos, yPos] >= 0 && allObjectSettings[onTileMap[xPos, yPos]].walkable)
		{
			return true;
		}
		return false;
	}

	public void spawnFirstConnectAirShip()
	{
		Vector3 position = spawnPos.position + new Vector3(-160f, 0f, -10f);
		position.y = 20f;
		Object.Instantiate(firstConnectAirShip, position, Quaternion.identity);
	}

	public void checkAllCarryHeight(int xPos, int yPos)
	{
		int num = heightMap[xPos, yPos];
		for (int i = 0; i < allCarriables.Count; i++)
		{
			if (allCarriables[i].gameObject.activeInHierarchy && Mathf.RoundToInt(allCarriables[i].transform.position.x / 2f) == xPos && Mathf.RoundToInt(allCarriables[i].transform.position.z / 2f) == yPos && (allCarriables[i].dropToPos < (float)num || Mathf.Abs(allCarriables[i].dropToPos - (float)num) <= 1f || (manageWorld.onTileMap[xPos, yPos] == -1 && allCarriables[i].dropToPos > (float)num)))
			{
				allCarriables[i].moveToNewDropPos(num);
			}
		}
	}

	public void moveAllCarriablesToSpawn()
	{
		int prefabId = NetworkMapSharer.share.cassowaryEgg.GetComponent<PickUpAndCarry>().prefabId;
		for (int i = 0; i < allCarriables.Count; i++)
		{
			if ((bool)allCarriables[i].gameObject && allCarriables[i].gameObject.activeInHierarchy && !allCarriables[i].investigationItem && allCarriables[i].prefabId != prefabId)
			{
				allCarriables[i].dropToPos = manageWorld.spawnPos.position.y;
				allCarriables[i].transform.position = manageWorld.spawnPos.position;
			}
		}
	}
}
