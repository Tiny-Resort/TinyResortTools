using UnityEngine;

public class CheckLootTableAndGetTotal : MonoBehaviour
{
	private InventoryItemLootTable tracking;

	private void Start()
	{
		tracking = GetComponent<InventoryItemLootTable>();
	}

	private void Update()
	{
		Debug.Log(tracking.getTotal());
	}
}
