using UnityEngine;

public class TileTypes : MonoBehaviour
{
	public enum tiles
	{
		Dirt = 0,
		Grass = 1,
		RockyDirt = 2,
		Sand = 3,
		TropicalGrass = 4,
		RedSand = 5,
		BrickPath = 6,
		TilledDirt = 7,
		WetTilledDirt = 8,
		MineStoneTile = 9,
		MineDirtTile = 10,
		CementPath = 11,
		TilledDirtFertilizer = 12,
		WetTilledDirtFertilizer = 13,
		Mud = 14,
		PineGrass = 15,
		WoodPath = 16,
		BasicRockPath = 17
	}

	public Material myTileMaterial;

	public bool sideOfTileSame;

	public Color tileColorOnMap;

	public InventoryItem dropOnChange;

	public bool saveUnderTile;

	public bool changeTileKeepUnderTile;

	public bool changeToUnderTileAndChangeHeight;

	public ASound onPickUp;

	public ASound onPutDown;

	public ASound onHeightUp;

	public ASound onHeightDown;

	public int changeToOnHeightChange = -1;

	public int onChangeParticle = -1;

	public int onHeightChangePart = -1;

	public int changeParticleAmount = 25;

	public int specialDustPart = -1;

	public int footStepParticle = -1;

	public ASound footStepSound;

	public bool isPath;

	public bool isDirt;

	public bool isStone;

	public bool isTilledDirt;

	public bool isWetTilledDirt;

	public bool isFertilizedDirt;

	public bool isWetFertilizedDirt;

	public bool isGrassGrowable;

	public bool canBeSavedUnder = true;

	public int wetVersion = -1;

	public int dryVersion = -1;

	public InventoryItem uniqueShovel;

	public int mowedVariation = -1;
}
