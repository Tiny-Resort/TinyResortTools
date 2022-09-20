using System;

[Serializable]
public class DeedSave
{
	public DeedStatus[] deeds;

	public void saveDeeds(DeedStatus[] allDeeds)
	{
		deeds = allDeeds;
	}

	public void loadDeeds()
	{
		if (deeds != null)
		{
			for (int i = 0; i < deeds.Length; i++)
			{
				DeedManager.manage.deedDetails[i] = deeds[i];
			}
		}
		DeedManager.manage.loadDeedIngredients();
		DeedManager.manage.unlockStartingDeeds();
	}
}
