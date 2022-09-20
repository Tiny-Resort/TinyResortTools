using System.Collections;
using UnityEngine;

public class MapStorer : MonoBehaviour
{
	public static MapStorer store;

	public int[,] overWorldHeight;

	public int[,] overWorldOnTile;

	public int[,] overWorldTileType;

	public int[,] overWorldOnTileStatus;

	public int[,] overWorldTileTypeStatus;

	public int[,] overWorldRotationMap;

	public bool[,] overWorldWaterMap;

	public bool[,] overWorldChangedMap;

	public bool[,] overWorldWaterChangeMap;

	public bool[,] overWorldHeightChangeMap;

	public bool[,] overWorldOnTileChangedMap;

	public bool[,] overWorldTileTypeChangedMap;

	public int[,] underWorldHeight;

	public int[,] underWorldOnTile;

	public int[,] underWorldTileType;

	public int[,] underWorldOnTileStatus;

	public int[,] underWorldTileTypeStatus;

	public int[,] underWorldRotationMap;

	public bool[,] underWorldWaterMap;

	public bool[,] underWorldChangedMap;

	public bool[,] underWorldHeightChangedMap;

	public bool[,] underWorldWaterChangedMap;

	public bool[,] underworldOnTileChangedMap;

	public bool[,] underworldTileTypeChangedMap;

	public bool overWorldStored;

	public bool waitingForMapToStore;

	public bool waitForMapToLoad;

	private WaitForEndOfFrame wait = new WaitForEndOfFrame();

	private void Awake()
	{
		store = this;
	}

	private void Start()
	{
		underWorldHeight = new int[WorldManager.manageWorld.getMapSize(), WorldManager.manageWorld.getMapSize()];
		underWorldOnTile = new int[WorldManager.manageWorld.getMapSize(), WorldManager.manageWorld.getMapSize()];
		underWorldTileType = new int[WorldManager.manageWorld.getMapSize(), WorldManager.manageWorld.getMapSize()];
		underWorldOnTileStatus = new int[WorldManager.manageWorld.getMapSize(), WorldManager.manageWorld.getMapSize()];
		underWorldTileTypeStatus = new int[WorldManager.manageWorld.getMapSize(), WorldManager.manageWorld.getMapSize()];
		underWorldRotationMap = new int[WorldManager.manageWorld.getMapSize(), WorldManager.manageWorld.getMapSize()];
		underWorldWaterMap = new bool[WorldManager.manageWorld.getMapSize(), WorldManager.manageWorld.getMapSize()];
		underWorldChangedMap = new bool[WorldManager.manageWorld.getMapSize() / WorldManager.manageWorld.getChunkSize(), WorldManager.manageWorld.getMapSize() / WorldManager.manageWorld.getChunkSize()];
		underWorldHeightChangedMap = new bool[WorldManager.manageWorld.getMapSize() / WorldManager.manageWorld.getChunkSize(), WorldManager.manageWorld.getMapSize() / WorldManager.manageWorld.getChunkSize()];
		underWorldWaterChangedMap = new bool[WorldManager.manageWorld.getMapSize() / WorldManager.manageWorld.getChunkSize(), WorldManager.manageWorld.getMapSize() / WorldManager.manageWorld.getChunkSize()];
		underworldOnTileChangedMap = new bool[WorldManager.manageWorld.getMapSize() / WorldManager.manageWorld.getChunkSize(), WorldManager.manageWorld.getMapSize() / WorldManager.manageWorld.getChunkSize()];
		underworldTileTypeChangedMap = new bool[WorldManager.manageWorld.getMapSize() / WorldManager.manageWorld.getChunkSize(), WorldManager.manageWorld.getMapSize() / WorldManager.manageWorld.getChunkSize()];
	}

	public void storeMap(string map = "overWorld")
	{
		StartCoroutine(storeMapWithDelay(map));
	}

	private IEnumerator storeMapWithDelay(string map)
	{
		waitingForMapToStore = true;
		if (map == "overWorld")
		{
			overWorldHeight = WorldManager.manageWorld.heightMap;
			overWorldOnTile = WorldManager.manageWorld.onTileMap;
			overWorldTileType = WorldManager.manageWorld.tileTypeMap;
			yield return wait;
			overWorldOnTileStatus = WorldManager.manageWorld.onTileStatusMap;
			overWorldTileTypeStatus = WorldManager.manageWorld.tileTypeStatusMap;
			overWorldRotationMap = WorldManager.manageWorld.rotationMap;
			yield return wait;
			overWorldChangedMap = WorldManager.manageWorld.chunkChangedMap;
			yield return wait;
			overWorldHeightChangeMap = WorldManager.manageWorld.changedMapHeight;
			overWorldWaterChangeMap = WorldManager.manageWorld.changedMapWater;
			overWorldOnTileChangedMap = WorldManager.manageWorld.changedMapOnTile;
			overWorldTileTypeChangedMap = WorldManager.manageWorld.changedMapTileType;
			overWorldWaterMap = WorldManager.manageWorld.waterMap;
			overWorldStored = true;
		}
		else
		{
			underWorldHeight = WorldManager.manageWorld.heightMap;
			underWorldOnTile = WorldManager.manageWorld.onTileMap;
			underWorldTileType = WorldManager.manageWorld.tileTypeMap;
			yield return wait;
			underWorldOnTileStatus = WorldManager.manageWorld.onTileStatusMap;
			underWorldTileTypeStatus = WorldManager.manageWorld.tileTypeStatusMap;
			underWorldRotationMap = WorldManager.manageWorld.rotationMap;
			yield return wait;
			underWorldChangedMap = WorldManager.manageWorld.chunkChangedMap;
			yield return wait;
			underWorldHeightChangedMap = WorldManager.manageWorld.changedMapHeight;
			underWorldWaterChangedMap = WorldManager.manageWorld.changedMapWater;
			underworldOnTileChangedMap = WorldManager.manageWorld.changedMapOnTile;
			underworldTileTypeChangedMap = WorldManager.manageWorld.changedMapTileType;
			underWorldWaterMap = WorldManager.manageWorld.waterMap;
		}
		waitingForMapToStore = false;
	}

	public void loadStoredMap(string map = "overWorld")
	{
		StartCoroutine(loadStoredMapWithDelay(map));
	}

	private IEnumerator loadStoredMapWithDelay(string map)
	{
		waitForMapToLoad = true;
		if (map == "overWorld")
		{
			WorldManager.manageWorld.heightMap = overWorldHeight;
			WorldManager.manageWorld.onTileMap = overWorldOnTile;
			WorldManager.manageWorld.tileTypeMap = overWorldTileType;
			yield return wait;
			WorldManager.manageWorld.onTileStatusMap = overWorldOnTileStatus;
			WorldManager.manageWorld.tileTypeStatusMap = overWorldTileTypeStatus;
			WorldManager.manageWorld.rotationMap = overWorldRotationMap;
			yield return wait;
			WorldManager.manageWorld.chunkChangedMap = overWorldChangedMap;
			yield return wait;
			WorldManager.manageWorld.changedMapHeight = overWorldHeightChangeMap;
			WorldManager.manageWorld.changedMapWater = overWorldWaterChangeMap;
			WorldManager.manageWorld.changedMapOnTile = overWorldOnTileChangedMap;
			WorldManager.manageWorld.changedMapTileType = overWorldTileTypeChangedMap;
			WorldManager.manageWorld.waterMap = overWorldWaterMap;
		}
		else
		{
			WorldManager.manageWorld.heightMap = underWorldHeight;
			WorldManager.manageWorld.onTileMap = underWorldOnTile;
			WorldManager.manageWorld.tileTypeMap = underWorldTileType;
			yield return wait;
			WorldManager.manageWorld.onTileStatusMap = underWorldOnTileStatus;
			WorldManager.manageWorld.tileTypeStatusMap = underWorldTileTypeStatus;
			WorldManager.manageWorld.rotationMap = underWorldRotationMap;
			yield return wait;
			WorldManager.manageWorld.chunkChangedMap = underWorldChangedMap;
			yield return wait;
			WorldManager.manageWorld.changedMapHeight = underWorldHeightChangedMap;
			WorldManager.manageWorld.changedMapWater = underWorldWaterChangedMap;
			WorldManager.manageWorld.changedMapOnTile = underworldOnTileChangedMap;
			WorldManager.manageWorld.changedMapTileType = underworldTileTypeChangedMap;
			WorldManager.manageWorld.waterMap = underWorldWaterMap;
		}
		waitForMapToLoad = false;
	}

	public void getStoredMineMapForConnect()
	{
		WorldManager.manageWorld.heightMap = underWorldHeight;
		WorldManager.manageWorld.onTileMap = underWorldOnTile;
		WorldManager.manageWorld.tileTypeMap = underWorldTileType;
		WorldManager.manageWorld.onTileStatusMap = underWorldOnTileStatus;
		WorldManager.manageWorld.tileTypeStatusMap = underWorldTileTypeStatus;
		WorldManager.manageWorld.rotationMap = underWorldRotationMap;
		WorldManager.manageWorld.chunkChangedMap = underWorldChangedMap;
		WorldManager.manageWorld.changedMapHeight = underWorldHeightChangedMap;
		WorldManager.manageWorld.changedMapWater = underWorldWaterChangedMap;
		WorldManager.manageWorld.changedMapOnTile = underworldOnTileChangedMap;
		WorldManager.manageWorld.changedMapTileType = underworldTileTypeChangedMap;
		WorldManager.manageWorld.waterMap = underWorldWaterMap;
		NetworkMapSharer.share.onChangeMaps.Invoke();
	}
}
