using System;
using UnityEngine;

[Serializable]
public class DropToSave
{
	public int itemId;

	public int stackId;

	public int[] desiredPos = new int[3];

	public int houseX;

	public int houseY;

	public DropToSave()
	{
	}

	public DropToSave(int itemIdSave, int stackIdSave, Vector3 posToSave, int houseXSave, int houseYSave)
	{
		itemId = itemIdSave;
		stackId = stackIdSave;
		houseX = houseXSave;
		houseY = houseYSave;
		desiredPos = new int[3];
		desiredPos[0] = (int)posToSave.x;
		desiredPos[1] = (int)posToSave.y;
		desiredPos[2] = (int)posToSave.z;
	}

	public void spawnDrop()
	{
		if (houseX != -1 && houseY != -1)
		{
			HouseManager.manage.getHouseInfoIfExists(houseX, houseY);
			NetworkMapSharer.share.spawnAServerDropToSave(itemId, stackId, new Vector3(desiredPos[0], desiredPos[1], desiredPos[2]), HouseManager.manage.getHouseInfoIfExists(houseX, houseY));
		}
		else
		{
			NetworkMapSharer.share.spawnAServerDropToSave(itemId, stackId, new Vector3(desiredPos[0], desiredPos[1], desiredPos[2]));
		}
	}
}
