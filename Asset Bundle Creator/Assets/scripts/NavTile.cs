using UnityEngine;

public class NavTile : MonoBehaviour
{
	public Transform insideTile;

	public NavMeshSourceTag myTag;

	public NavChunk myChunk;

	private bool navMeshOn = true;

	private bool isWater;

	private int showingX;

	private int showingY;

	private int showingOnTile = -1;

	private float showingHeight;

	private void setTileEvent()
	{
		setTile(showingX, showingY);
	}

	public void setTile(int xPos, int yPos, bool forceRefresh = false)
	{
		bool flag = false;
		bool flag2 = navMeshOn;
		if (forceRefresh || showingX != xPos || showingY != yPos)
		{
			flag = true;
			showingY = yPos;
			showingX = xPos;
			int num = showingOnTile;
			showingOnTile = WorldManager.manageWorld.onTileMap[xPos, yPos];
			if (WorldManager.manageWorld.waterMap[xPos, yPos] && WorldManager.manageWorld.heightMap[xPos, yPos] < 0 && showingHeight != -0.2f)
			{
				insideTile.localPosition = new Vector3(0f, -0.2f, 0f);
				showingHeight = 0.5f;
			}
			else if (showingHeight != (float)WorldManager.manageWorld.heightMap[xPos, yPos] || showingOnTile == 15)
			{
				if (showingOnTile == 15)
				{
					showingHeight = WorldManager.manageWorld.onTileStatusMap[xPos, yPos];
					insideTile.localPosition = new Vector3(0f, WorldManager.manageWorld.onTileStatusMap[xPos, yPos], 0f);
				}
				else
				{
					showingHeight = WorldManager.manageWorld.heightMap[xPos, yPos];
					insideTile.localPosition = new Vector3(0f, WorldManager.manageWorld.heightMap[xPos, yPos], 0f);
				}
			}
			if (num != showingOnTile)
			{
				navMeshChanges(xPos, yPos, true);
			}
			else
			{
				navMeshChanges(xPos, yPos, false);
			}
		}
		else
		{
			int num2 = showingOnTile;
			showingOnTile = WorldManager.manageWorld.onTileMap[xPos, yPos];
			if (WorldManager.manageWorld.waterMap[xPos, yPos] && WorldManager.manageWorld.heightMap[xPos, yPos] < 0 && showingHeight != -0.2f)
			{
				insideTile.localPosition = new Vector3(0f, -0.2f, 0f);
				showingHeight = 0.5f;
				flag = true;
			}
			else if (showingHeight != (float)WorldManager.manageWorld.heightMap[xPos, yPos] || (showingOnTile == 15 && showingHeight != (float)WorldManager.manageWorld.onTileStatusMap[xPos, yPos]))
			{
				if (showingOnTile == 15)
				{
					showingHeight = WorldManager.manageWorld.onTileStatusMap[xPos, yPos];
					insideTile.localPosition = new Vector3(0f, WorldManager.manageWorld.onTileStatusMap[xPos, yPos], 0f);
				}
				else
				{
					showingHeight = WorldManager.manageWorld.heightMap[xPos, yPos];
					insideTile.localPosition = new Vector3(0f, WorldManager.manageWorld.heightMap[xPos, yPos], 0f);
				}
				flag = true;
			}
			if (showingOnTile != num2)
			{
				navMeshChanges(xPos, yPos, true);
			}
			else
			{
				navMeshChanges(xPos, yPos, false);
			}
		}
		if (flag || (navMeshOn && navMeshOn != flag2))
		{
			myTag.refreshPositonAndBuild();
		}
		else
		{
			myTag.refreshBuildOnly();
		}
	}

	public void collect()
	{
		myTag.refreshBuildOnly();
	}

	public void navMeshChanges(int xPos, int yPos, bool newOnTile)
	{
		if (WorldManager.manageWorld.heightMap[xPos, yPos] >= 0 || WorldManager.manageWorld.onTileMap[xPos, yPos] == 15)
		{
			isWater = false;
		}
		else if (WorldManager.manageWorld.waterMap[xPos, yPos])
		{
			isWater = true;
		}
		else
		{
			isWater = false;
		}
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] >= 0 && (bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileOnOff && WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileOnOff.isGate)
		{
			if (!WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileOnOff.getIfOpen(xPos, yPos))
			{
				navMeshOn = true;
				myTag.enableGate();
				return;
			}
			navMeshOn = true;
			myTag.enableIt(isWater);
			if (!isWater && navMeshOn && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos, yPos]].isPath)
			{
				myTag.setPath();
			}
		}
		else if (newOnTile)
		{
			if (WorldManager.manageWorld.onTileMap[xPos, yPos] < -1 || (WorldManager.manageWorld.onTileMap[xPos, yPos] >= 0 && !WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].walkable))
			{
				navMeshOn = false;
				myTag.disableIt();
			}
			else
			{
				navMeshOn = true;
				myTag.enableIt(isWater);
			}
			if (!isWater && navMeshOn && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos, yPos]].isPath)
			{
				myTag.setPath();
			}
		}
		else
		{
			if (navMeshOn)
			{
				navMeshOn = true;
				myTag.enableIt(isWater);
			}
			else
			{
				navMeshOn = false;
				myTag.disableIt();
			}
			if (!isWater && navMeshOn && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos, yPos]].isPath)
			{
				myTag.setPath();
			}
		}
	}
}
