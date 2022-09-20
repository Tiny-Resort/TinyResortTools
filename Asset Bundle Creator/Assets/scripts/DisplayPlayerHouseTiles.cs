using UnityEngine;
using UnityEngine.Events;

public class DisplayPlayerHouseTiles : MonoBehaviour
{
	public bool isPlayerHouse;

	public Transform[] gridStartingPositions;

	public int rotationNo;

	private Transform startingPosition;

	public UnityEvent updateHouseContents;

	public TileObject[,] tileObjectsInHouse = new TileObject[25, 25];

	public TileObject[,] tileObjectsOnTop = new TileObject[25, 25];

	public int xSize = 7;

	public int ySize = 5;

	public int xLength;

	public int yLength;

	private bool firstSetUp = true;

	public int housePosX;

	public int housePosY;

	public MeshRenderer insideMesh;

	private TileObject myTileObject;

	private HouseDetails myHouseDetails;

	private HouseExterior exteriorDetails;

	public PlayerHouseExterior myHouseExterior;

	public int houseLevel;

	public void Awake()
	{
		myTileObject = GetComponent<TileObject>();
		HouseManager.manage.housesOnDisplay.Add(this);
	}

	private void OnDestroy()
	{
		clearHouse();
		HouseManager.manage.housesOnDisplay.Remove(this);
	}

	private void OnDisable()
	{
		clearHouse();
	}

	public void Start()
	{
	}

	private bool checkIfNeedsUpgrade()
	{
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				if (j < xLength && i < yLength && myHouseDetails.houseMapOnTile[j, i] == -2)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void upgradeHouseSize()
	{
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				if (j >= xLength || i >= yLength)
				{
					myHouseDetails.houseMapOnTile[j, i] = -2;
				}
				else if (myHouseDetails.houseMapOnTile[j, i] == -2)
				{
					myHouseDetails.houseMapOnTile[j, i] = -1;
				}
			}
		}
	}

	public void refreshWalls()
	{
		if (myHouseDetails != null)
		{
			Material[] materials = new Material[2]
			{
				Inventory.inv.allItems[myHouseDetails.floor].equipable.material,
				Inventory.inv.allItems[myHouseDetails.wall].equipable.material
			};
			if ((bool)insideMesh)
			{
				insideMesh.materials = materials;
			}
		}
	}

	public void updateHouseExterior()
	{
		if (exteriorDetails != null && (bool)myHouseExterior)
		{
			myHouseExterior.setExterior(exteriorDetails);
		}
	}

	public void setInteriorPosAndRotation(int xPos, int yPos)
	{
		if ((bool)NetworkMapSharer.share)
		{
			myHouseDetails = HouseManager.manage.getHouseInfo(xPos, yPos);
			exteriorDetails = HouseManager.manage.getHouseExterior(xPos, yPos);
			exteriorDetails.playerHouse = true;
			exteriorDetails.houseLevel = houseLevel;
			if ((bool)myHouseExterior)
			{
				myHouseExterior.setExterior(exteriorDetails);
			}
			rotationNo = WorldManager.manageWorld.rotationMap[xPos, yPos];
			startingPosition = gridStartingPositions[rotationNo - 1];
			xLength = xSize;
			yLength = ySize;
			housePosX = xPos;
			housePosY = yPos;
			if (rotationNo == 2 || rotationNo == 4)
			{
				xLength = ySize;
				yLength = xSize;
			}
		}
	}

	public void firstRefresh()
	{
		refreshHouseTiles(true);
	}

	public int getCurrentHouseId()
	{
		return WorldManager.manageWorld.onTileMap[housePosX, housePosY];
	}

	public Transform getStartingPosTransform()
	{
		return startingPosition;
	}

	public void clearForUpgrade()
	{
		clearHouse();
		myHouseDetails = null;
	}

	public void clearHouse()
	{
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				if (tileObjectsInHouse[j, i] != null)
				{
					WorldManager.manageWorld.returnTileObject(tileObjectsInHouse[j, i]);
					tileObjectsInHouse[j, i].gameObject.SetActive(false);
					tileObjectsInHouse[j, i] = null;
				}
				if (tileObjectsOnTop[j, i] != null)
				{
					WorldManager.manageWorld.returnTileObject(tileObjectsOnTop[j, i]);
					tileObjectsOnTop[j, i].gameObject.SetActive(false);
					tileObjectsOnTop[j, i] = null;
				}
			}
		}
	}

	public void refreshHouseTiles(bool firstTime = false)
	{
		if (myHouseDetails == null)
		{
			return;
		}
		xLength = xSize;
		yLength = ySize;
		if (rotationNo == 2 || rotationNo == 4)
		{
			xLength = ySize;
			yLength = xSize;
		}
		refreshWalls();
		for (int i = 0; i < yLength; i++)
		{
			for (int j = 0; j < xLength; j++)
			{
				if (myHouseDetails.houseMapOnTile[j, i] > -1)
				{
					Vector3 moveTo = startingPosition.position + new Vector3(j * 2, 0f, i * 2);
					if (tileObjectsInHouse[j, i] == null)
					{
						tileObjectsInHouse[j, i] = WorldManager.manageWorld.getTileObjectForHouse(myHouseDetails.houseMapOnTile[j, i], moveTo, j, i, myHouseDetails);
						if (!firstTime)
						{
							tileObjectsInHouse[j, i].placeDown();
						}
					}
					else if (tileObjectsInHouse[j, i].tileObjectId != myHouseDetails.houseMapOnTile[j, i])
					{
						WorldManager.manageWorld.returnTileObject(tileObjectsInHouse[j, i]);
						tileObjectsInHouse[j, i] = WorldManager.manageWorld.getTileObjectForHouse(myHouseDetails.houseMapOnTile[j, i], moveTo, j, i, myHouseDetails);
						if (!firstTime)
						{
							tileObjectsInHouse[j, i].placeDown();
						}
					}
					if ((bool)tileObjectsInHouse[j, i])
					{
						tileObjectsInHouse[j, i].setXAndYForHouse(j, i);
						tileObjectsInHouse[j, i].checkOnTopInside(j, i, myHouseDetails);
						if ((bool)tileObjectsInHouse[j, i].showObjectOnStatusChange)
						{
							tileObjectsInHouse[j, i].showObjectOnStatusChange.showGameObject(j, i, myHouseDetails);
						}
						if ((bool)tileObjectsInHouse[j, i].tileObjectFurniture)
						{
							tileObjectsInHouse[j, i].tileObjectFurniture.updateOnTileStatus(j, i, myHouseDetails);
						}
						if ((bool)tileObjectsInHouse[j, i].tileObjectItemChanger)
						{
							tileObjectsInHouse[j, i].tileObjectItemChanger.mapUpdatePos(j, i, myHouseDetails);
						}
					}
				}
				else if (tileObjectsInHouse[j, i] != null)
				{
					tileObjectsInHouse[j, i].onDeath();
					WorldManager.manageWorld.returnTileObject(tileObjectsInHouse[j, i]);
					tileObjectsInHouse[j, i] = null;
				}
			}
		}
		for (int k = 0; k < yLength; k++)
		{
			for (int l = 0; l < xLength; l++)
			{
				if (tileObjectsOnTop[l, k] != null)
				{
					WorldManager.manageWorld.returnTileObject(tileObjectsOnTop[l, k]);
					tileObjectsOnTop[l, k] = null;
				}
			}
		}
	}
}
