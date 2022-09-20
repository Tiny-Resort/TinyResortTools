using UnityEngine;

public class NPCBuildingDoors : MonoBehaviour
{
	public Transform outside;

	public Transform inside;

	public Transform workPos;

	public NPCSchedual.Locations myLocation;

	public int connectedToBuilingId;

	[Header("non local spawn pos ---")]
	public Transform nonLocalSpawnPoint;

	public void setConnectedToBuildingId(int newId)
	{
		connectedToBuilingId = newId;
		TownManager.manage.allShopFloors[(int)myLocation] = this;
		if ((bool)nonLocalSpawnPoint)
		{
			NetworkMapSharer.share.setNonLocalSpawnPos(nonLocalSpawnPoint);
		}
	}

	public void removeSelfFromNavMesh()
	{
		if (TownManager.manage.allShopFloors[(int)myLocation] != null)
		{
			NavMeshSourceTag[] componentsInChildren = TownManager.manage.allShopFloors[(int)myLocation].GetComponentsInChildren<NavMeshSourceTag>();
			foreach (NavMeshSourceTag item in componentsInChildren)
			{
				NetworkNavMesh.nav.otherMeshes.Remove(item);
			}
			TownManager.manage.allShopFloors[(int)myLocation] = null;
		}
	}

	public void addMeshesToNavMesh()
	{
		NavMeshSourceTag[] componentsInChildren = GetComponentsInChildren<NavMeshSourceTag>();
		foreach (NavMeshSourceTag navMeshSourceTag in componentsInChildren)
		{
			navMeshSourceTag.forceStartForBuildingPlacement(0);
			NetworkNavMesh.nav.otherMeshes.Add(navMeshSourceTag);
		}
	}

	public void refreshMeshLocations()
	{
		NavMeshSourceTag[] componentsInChildren = GetComponentsInChildren<NavMeshSourceTag>();
		foreach (NavMeshSourceTag navMeshSourceTag in componentsInChildren)
		{
			navMeshSourceTag.refreshPositonAndBuild();
			NetworkNavMesh.nav.otherMeshes.Add(navMeshSourceTag);
		}
	}

	public bool checkIfIWorkHere(int NPCVendorId)
	{
		if (myLocation == (NPCSchedual.Locations)NPCVendorId || myLocation == NPCSchedual.Locations.Market_place)
		{
			return true;
		}
		return false;
	}
}
