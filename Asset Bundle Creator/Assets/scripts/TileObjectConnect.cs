using UnityEngine;
using UnityEngine.Events;

public class TileObjectConnect : MonoBehaviour
{
	public GameObject leftConnect;

	public GameObject rightConnect;

	public GameObject upConnect;

	public GameObject downConnect;

	private GameObject[] connections = new GameObject[4];

	public Transform heighTrans;

	public static UnityEvent heightPlaceChange = new UnityEvent();

	private int myTileType;

	private int myTileHeight;

	private int myTileX;

	private int myTileY;

	public bool matchHeight;

	public bool connectToNeighbours = true;

	public bool inverted;

	public bool specialWhenNoNeighbour;

	public GameObject showInsteadOfAllConnect;

	public TileObject myTileObject;

	public TileObject[] canConnectTo;

	public TileObjectConnect secondConnect;

	public bool secondConnectUseNeighbourHeight;

	public bool isFence;

	private void Awake()
	{
		connections[0] = upConnect;
		connections[1] = leftConnect;
		connections[2] = rightConnect;
		connections[3] = downConnect;
	}

	private void OnEnable()
	{
		if (matchHeight)
		{
			heightPlaceChange.AddListener(getHighestNeighbour);
			heightPlaceChange.Invoke();
		}
	}

	private void OnDisable()
	{
		if (matchHeight)
		{
			heightPlaceChange.RemoveListener(getHighestNeighbour);
		}
	}

	public void connectToTiles(int xTile, int yTile, int rotPreview = -1)
	{
		if (connectToNeighbours && (bool)myTileObject && myTileObject.getsRotationFromMap())
		{
			if (rotPreview == -1)
			{
				swapForRotation(WorldManager.manageWorld.rotationMap[xTile, yTile]);
			}
			else
			{
				swapForRotation(rotPreview);
			}
		}
		myTileType = WorldManager.manageWorld.onTileMap[xTile, yTile];
		myTileHeight = WorldManager.manageWorld.heightMap[xTile, yTile];
		myTileX = xTile;
		myTileY = yTile;
		if (connectToNeighbours)
		{
			if ((bool)showInsteadOfAllConnect && neighbourSame(xTile - 1, yTile) && neighbourSame(xTile + 1, yTile) && neighbourSame(xTile, yTile + 1) && neighbourSame(xTile, yTile - 1))
			{
				showInsteadOfAllConnect.SetActive(true);
				pieceOn(leftConnect, false);
				pieceOn(rightConnect, false);
				pieceOn(upConnect, false);
				pieceOn(downConnect, false);
				if ((bool)secondConnect)
				{
					secondConnect.pieceOn(secondConnect.leftConnect, false);
					secondConnect.pieceOn(secondConnect.rightConnect, false);
					secondConnect.pieceOn(secondConnect.upConnect, false);
					secondConnect.pieceOn(secondConnect.downConnect, false);
				}
				return;
			}
			if ((bool)showInsteadOfAllConnect)
			{
				showInsteadOfAllConnect.SetActive(false);
			}
			if (!inverted)
			{
				pieceOn(leftConnect, neighbourSame(xTile - 1, yTile));
				pieceOn(rightConnect, neighbourSame(xTile + 1, yTile));
				pieceOn(upConnect, neighbourSame(xTile, yTile + 1));
				pieceOn(downConnect, neighbourSame(xTile, yTile - 1));
			}
			else
			{
				pieceOn(leftConnect, !neighbourSame(xTile - 1, yTile));
				pieceOn(rightConnect, !neighbourSame(xTile + 1, yTile));
				pieceOn(upConnect, !neighbourSame(xTile, yTile + 1));
				pieceOn(downConnect, !neighbourSame(xTile, yTile - 1));
			}
		}
		if (specialWhenNoNeighbour)
		{
			if (rightConnect.activeInHierarchy || leftConnect.activeInHierarchy)
			{
				pieceOn(leftConnect, true);
				pieceOn(rightConnect, true);
			}
			if (upConnect.activeInHierarchy || downConnect.activeInHierarchy)
			{
				pieceOn(upConnect, true);
				pieceOn(downConnect, true);
			}
			if (!neighbourSame(xTile - 1, yTile) && !neighbourSame(xTile + 1, yTile) && !neighbourSame(xTile, yTile + 1) && !neighbourSame(xTile, yTile - 1))
			{
				pieceOn(leftConnect, true);
				pieceOn(rightConnect, true);
				MonoBehaviour.print("NO NEighbour special called");
			}
		}
		if (matchHeight && base.gameObject.activeSelf)
		{
			getHighestNeighbour();
		}
		if ((bool)secondConnect)
		{
			secondConnect.connectToTiles(xTile, yTile);
		}
	}

	public void getHighestNeighbour()
	{
		if (!matchHeight || !myTileObject || WorldManager.manageWorld.onTileMap[myTileX, myTileY] != myTileObject.tileObjectId || myTileX == 0 || myTileY == 0 || !matchHeight)
		{
			return;
		}
		int num = myTileX;
		int num2 = myTileY;
		int num3 = myTileHeight;
		int num4 = WorldManager.manageWorld.onTileStatusMap[num, num2];
		for (int i = 1; checkNeighbourIsConnectable(WorldManager.manageWorld.onTileMap[num - i, num2]); i++)
		{
			if (WorldManager.manageWorld.heightMap[num - i, num2] > num3)
			{
				num3 = WorldManager.manageWorld.heightMap[num - i, num2];
			}
			if (WorldManager.manageWorld.onTileStatusMap[num - i, num2] > num4)
			{
				num4 = WorldManager.manageWorld.onTileStatusMap[num - i, num2];
			}
		}
		for (int j = 1; checkNeighbourIsConnectable(WorldManager.manageWorld.onTileMap[num + j, num2]); j++)
		{
			if (WorldManager.manageWorld.heightMap[num + j, num2] > num3)
			{
				num3 = WorldManager.manageWorld.heightMap[num + j, num2];
			}
			if (WorldManager.manageWorld.onTileStatusMap[num + j, num2] > num4)
			{
				num4 = WorldManager.manageWorld.onTileStatusMap[num + j, num2];
			}
		}
		for (int k = 1; checkNeighbourIsConnectable(WorldManager.manageWorld.onTileMap[num, num2 - k]); k++)
		{
			if (WorldManager.manageWorld.heightMap[num, num2 - k] > num3)
			{
				num3 = WorldManager.manageWorld.heightMap[num, num2 - k];
			}
			if (WorldManager.manageWorld.onTileStatusMap[num, num2 - k] > num4)
			{
				num4 = WorldManager.manageWorld.onTileStatusMap[num, num2 - k];
			}
		}
		for (int l = 1; checkNeighbourIsConnectable(WorldManager.manageWorld.onTileMap[num, num2 + l]); l++)
		{
			if (WorldManager.manageWorld.heightMap[num, num2 + l] > num3)
			{
				num3 = WorldManager.manageWorld.heightMap[num, num2 + l];
			}
			if (WorldManager.manageWorld.onTileStatusMap[num, num2 + l] > num4)
			{
				num4 = WorldManager.manageWorld.onTileStatusMap[num, num2 + l];
			}
		}
		if (num3 > num4)
		{
			heighTrans.position = new Vector3(base.transform.position.x, num3, base.transform.position.z);
			WorldManager.manageWorld.onTileStatusMap[num, num2] = num3;
		}
		else
		{
			heighTrans.position = new Vector3(base.transform.position.x, num4, base.transform.position.z);
			WorldManager.manageWorld.onTileStatusMap[num, num2] = num4;
		}
	}

	private bool neighbourSame(int neighbourX, int neighbourY)
	{
		if (neighbourX < 0 || neighbourX > WorldManager.manageWorld.getMapSize() - 1 || neighbourY < 0 || neighbourY > WorldManager.manageWorld.getMapSize() - 1)
		{
			return false;
		}
		if (WorldManager.manageWorld.onTileMap[neighbourX, neighbourY] < -1)
		{
			Vector2 vector = moveSelectionToMainTileForMultiTiledObject(neighbourX, neighbourY);
			neighbourX = (int)vector.x;
			neighbourY = (int)vector.y;
		}
		if (checkNeighbourIsConnectable(WorldManager.manageWorld.onTileMap[neighbourX, neighbourY]))
		{
			if (matchHeight)
			{
				return true;
			}
			if (secondConnectUseNeighbourHeight)
			{
				return true;
			}
			if (!matchHeight && myTileHeight == WorldManager.manageWorld.heightMap[neighbourX, neighbourY])
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private bool neighbourIsHigher(int neighbourX, int neighbourY)
	{
		if (myTileHeight < WorldManager.manageWorld.heightMap[neighbourX, neighbourY])
		{
			return true;
		}
		return false;
	}

	public void pieceOn(GameObject dirPiece, bool isEnabled)
	{
		dirPiece.SetActive(isEnabled);
	}

	private bool checkNeighbourIsConnectable(int neighbourToCheck)
	{
		if (neighbourToCheck == -1)
		{
			return false;
		}
		if (myTileType == neighbourToCheck)
		{
			return true;
		}
		if (isFence && neighbourToCheck > -1 && (bool)WorldManager.manageWorld.allObjects[neighbourToCheck].tileObjectConnect && WorldManager.manageWorld.allObjects[neighbourToCheck].tileObjectConnect.isFence)
		{
			return true;
		}
		if (canConnectTo.Length != 0)
		{
			for (int i = 0; i < canConnectTo.Length; i++)
			{
				if (canConnectTo[i].tileObjectId == neighbourToCheck)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void swapForRotation(int rotation)
	{
		switch (rotation)
		{
		case 1:
			leftConnect = connections[1];
			rightConnect = connections[2];
			upConnect = connections[0];
			downConnect = connections[3];
			break;
		case 2:
			leftConnect = connections[3];
			rightConnect = connections[0];
			upConnect = connections[1];
			downConnect = connections[2];
			break;
		case 3:
			leftConnect = connections[2];
			rightConnect = connections[1];
			upConnect = connections[3];
			downConnect = connections[0];
			break;
		default:
			leftConnect = connections[0];
			rightConnect = connections[3];
			upConnect = connections[2];
			downConnect = connections[1];
			break;
		}
	}

	public Vector2 moveSelectionToMainTileForMultiTiledObject(int xPos, int yPos)
	{
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		int num2 = 0;
		while (!flag || !flag2)
		{
			if (!flag2)
			{
				if (checkIfOnMap(xPos + num))
				{
					if (WorldManager.manageWorld.onTileMap[xPos + num, yPos] == -3)
					{
						flag2 = true;
					}
					else if (WorldManager.manageWorld.onTileMap[xPos + num, yPos] == -4 && checkIfOnMap(xPos + (num - 1)) && WorldManager.manageWorld.onTileMap[xPos + (num - 1), yPos] != -4)
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
			if (checkIfOnMap(yPos + num2))
			{
				if (WorldManager.manageWorld.onTileMap[xPos + num, yPos + num2] != -3)
				{
					flag = true;
				}
				else if (WorldManager.manageWorld.onTileMap[xPos + num, yPos + num2] == -3 && checkIfOnMap(yPos + (num2 - 1)) && WorldManager.manageWorld.onTileMap[xPos + num, yPos + (num2 - 1)] != -3)
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

	private bool checkIfOnMap(int intToCheck)
	{
		if (intToCheck >= 0 && intToCheck < WorldManager.manageWorld.getMapSize())
		{
			return true;
		}
		return false;
	}
}
