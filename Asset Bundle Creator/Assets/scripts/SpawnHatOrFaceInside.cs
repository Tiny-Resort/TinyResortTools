using UnityEngine;

public class SpawnHatOrFaceInside : MonoBehaviour
{
	public Transform spawnHere;

	public void setUpForObject(int objectId)
	{
		Transform obj = Object.Instantiate(Inventory.inv.allItems[objectId].equipable.hatPrefab, spawnHere).transform;
		obj.localRotation = Quaternion.Euler(0f, 0f, 0f);
		obj.localPosition = Vector3.zero;
	}
}
