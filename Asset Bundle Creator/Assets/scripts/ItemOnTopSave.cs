using System;

[Serializable]
public class ItemOnTopSave
{
	public ItemOnTop[] savedObjects;

	public void saveObjectsOnTop()
	{
		savedObjects = ItemOnTopManager.manage.onTopItems.ToArray();
	}

	public void loadObjectsOnTop()
	{
		if (savedObjects != null)
		{
			for (int i = 0; i < savedObjects.Length; i++)
			{
				ItemOnTopManager.manage.onTopItems.Add(savedObjects[i]);
			}
		}
	}
}
