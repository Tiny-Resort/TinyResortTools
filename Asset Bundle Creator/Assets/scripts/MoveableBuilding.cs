using System;

[Serializable]
internal class MoveableBuilding
{
	private int xPos;

	private int yPos;

	private bool beingMoved;

	public MoveableBuilding(int xPosition, int yPosition)
	{
		xPos = xPosition;
		yPos = yPosition;
	}

	public bool isBeingUpgraded()
	{
		return WorldManager.manageWorld.onTileStatusMap[xPos, yPos] == 1;
	}

	public int getBuildingId()
	{
		return WorldManager.manageWorld.onTileMap[xPos, yPos];
	}

	public void moveBuildingToNewPos(int newPosX, int newPosY)
	{
		int buildingId = getBuildingId();
		int give = WorldManager.manageWorld.onTileStatusMap[xPos, yPos];
		NetworkMapSharer.share.RpcClearHouseForMove(xPos, yPos);
		NetworkMapSharer.share.RpcRemoveMultiTiledObject(buildingId, xPos, yPos, WorldManager.manageWorld.rotationMap[xPos, yPos]);
		NetworkMapSharer.share.RpcUpdateOnTileObject(buildingId, newPosX, newPosY);
		if ((bool)WorldManager.manageWorld.allObjects[buildingId].displayPlayerHouseTiles)
		{
			NetworkMapSharer.share.RpcMoveHouseExterior(xPos, yPos, newPosX, newPosY);
			NetworkMapSharer.share.RpcMoveHouseInterior(xPos, yPos, newPosX, newPosY, WorldManager.manageWorld.rotationMap[xPos, yPos], WorldManager.manageWorld.rotationMap[newPosX, newPosY]);
			NetworkMapSharer.share.RpcGiveOnTileStatus(give, newPosX, newPosY);
		}
		BuildingManager.manage.currentlyMoving = -1;
		NetworkMapSharer.share.NetworkmovingBuilding = -1;
		xPos = newPosX;
		yPos = newPosY;
	}
}
