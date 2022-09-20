using UnityEngine;

public class FarmAnimalHouseFloor : MonoBehaviour
{
	public Transform sleepingSpot;

	public bool smallAnimalsOnly;

	public bool mediumAnimalsOnly;

	public int xPos;

	public int yPos;

	public TileObjectBridge isBridge;

	public void setXY(int newXPos, int newYPos)
	{
		xPos = newXPos;
		yPos = newYPos;
	}

	public void setBridge(int xPos, int yPos)
	{
		if ((bool)isBridge)
		{
			isBridge.setUpBridge(WorldManager.manageWorld.onTileStatusMap[xPos, yPos]);
		}
	}
}
