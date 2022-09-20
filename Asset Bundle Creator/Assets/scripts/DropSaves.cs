using System;
using System.Collections.Generic;

[Serializable]
public class DropSaves
{
	public DropToSave[] savedDrops;

	public void saveDrops()
	{
		List<DroppedItem> dropsToSave = WorldManager.manageWorld.getDropsToSave();
		savedDrops = new DropToSave[dropsToSave.Count];
		for (int i = 0; i < dropsToSave.Count; i++)
		{
			if (dropsToSave[i].inside == null)
			{
				savedDrops[i] = new DropToSave(dropsToSave[i].myItemId, dropsToSave[i].stackAmount, dropsToSave[i].desiredPos, -1, -1);
			}
			else
			{
				savedDrops[i] = new DropToSave(dropsToSave[i].myItemId, dropsToSave[i].stackAmount, dropsToSave[i].desiredPos, dropsToSave[i].inside.xPos, dropsToSave[i].inside.yPos);
			}
		}
	}

	public void loadDrops()
	{
		for (int i = 0; i < savedDrops.Length; i++)
		{
			savedDrops[i].spawnDrop();
		}
	}
}
