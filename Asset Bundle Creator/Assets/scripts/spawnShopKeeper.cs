using UnityEngine;

public class spawnShopKeeper : MonoBehaviour
{
	public bool spawnCrafter;

	public bool spawnClothKeeper;

	public bool spawnWeaponKeeper;

	public bool spawnFurnitureKeeper;

	public bool spawnPostOfficeKeeper;

	public void serverSpawnKeeper()
	{
		if (NetworkMapSharer.share.isServer)
		{
			if (spawnPostOfficeKeeper)
			{
				base.transform.parent = null;
			}
			else if (spawnFurnitureKeeper)
			{
				base.transform.parent = null;
			}
			else if (spawnWeaponKeeper)
			{
				base.transform.parent = null;
			}
			else if (spawnCrafter)
			{
				base.transform.parent = null;
			}
			else if (spawnClothKeeper)
			{
				base.transform.parent = null;
			}
			else
			{
				base.transform.parent = null;
			}
		}
	}
}
