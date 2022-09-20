using UnityEngine;

public class LoadBuildingInsides : MonoBehaviour
{
	public GameObject shopFloor;

	public Transform spawnAtPosition;

	public string buildingName = "";

	public bool isMarketPlace;

	public bool isMoveable;

	public void checkForInterior(int xPos, int YPos)
	{
		if (!NetworkMapSharer.share || !NetworkMapSharer.share.localChar)
		{
			return;
		}
		if (!ShopManager.manage.shopsStarSync)
		{
			ShopManager.manage.shopsStarSync = true;
			NetworkMapSharer.share.localChar.CmdRequestShopStatus();
		}
		if (!TownManager.manage.checkIfBuildingInteriorHasBeenRequested(xPos, YPos))
		{
			RenderMap.map.updateMapOnPlaced();
			NetworkMapSharer.share.localChar.CmdRequestInterior(xPos, YPos);
			if (!NetworkMapSharer.share.isServer)
			{
				TownManager.manage.addBuildingAlreadyRequested(xPos, YPos);
			}
		}
	}

	public void serverSpawnsInteriorAndKeeper(Transform spawnFloorAt, int xPos, int yPos)
	{
		GameObject obj = Object.Instantiate(shopFloor, spawnFloorAt.transform.position, spawnFloorAt.rotation);
		obj.GetComponent<NPCBuildingDoors>().setConnectedToBuildingId(WorldManager.manageWorld.onTileMap[xPos, yPos]);
		NavMeshSourceTag[] componentsInChildren = obj.GetComponentsInChildren<NavMeshSourceTag>();
		foreach (NavMeshSourceTag navMeshSourceTag in componentsInChildren)
		{
			navMeshSourceTag.forceStartForBuildingPlacement(0);
			NetworkNavMesh.nav.otherMeshes.Add(navMeshSourceTag);
		}
	}

	public void overrideOldFloor(int xPos, int yPos)
	{
	}
}
