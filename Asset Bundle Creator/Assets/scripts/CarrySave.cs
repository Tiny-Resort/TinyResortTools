using System;
using System.Collections.Generic;

[Serializable]
public class CarrySave
{
	public CarryableObject[] allCarryables;

	public void saveAllCarryable()
	{
		List<PickUpAndCarry> list = new List<PickUpAndCarry>();
		for (int i = 0; i < WorldManager.manageWorld.allCarriables.Count; i++)
		{
			if (WorldManager.manageWorld.allCarriables[i] != null && (bool)WorldManager.manageWorld.allCarriables[i].gameObject && (WorldManager.manageWorld.allCarriables[i].isAboveGround || WorldManager.manageWorld.allCarriables[i].transform.position.y <= -12f) && !WorldManager.manageWorld.allCarriables[i].delivered)
			{
				list.Add(WorldManager.manageWorld.allCarriables[i]);
			}
		}
		allCarryables = new CarryableObject[list.Count];
		for (int j = 0; j < list.Count; j++)
		{
			allCarryables[j] = new CarryableObject(list[j]);
		}
	}

	public void loadAllCarryable()
	{
		if (allCarryables != null)
		{
			for (int i = 0; i < allCarryables.Length; i++)
			{
				allCarryables[i].SpawnTheObject();
			}
		}
	}
}
