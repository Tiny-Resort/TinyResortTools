using UnityEngine;

public class SpawnFarmAnimalPos : MonoBehaviour
{
	public bool isMarketPlace;

	private void OnEnable()
	{
		if (!isMarketPlace)
		{
			FarmAnimalMenu.menu.spawnFarmAnimalPos = base.transform;
		}
		else if (FarmAnimalMenu.menu.spawnFarmAnimalPos == null)
		{
			FarmAnimalMenu.menu.spawnFarmAnimalPos = base.transform;
		}
	}

	private void OnDisable()
	{
		if (FarmAnimalMenu.menu.spawnFarmAnimalPos == base.transform)
		{
			FarmAnimalMenu.menu.spawnFarmAnimalPos = null;
		}
	}
}
