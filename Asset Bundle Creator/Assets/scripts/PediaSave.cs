using System;

[Serializable]
public class PediaSave
{
	public PediaEntry[] entries = new PediaEntry[0];

	public void saveEntries(PediaEntry[] newEntries)
	{
		entries = newEntries;
	}

	public void loadEntries()
	{
		if (entries != null)
		{
			for (int i = 0; i < PediaManager.manage.allEntries.Count && i < entries.Length; i++)
			{
				PediaManager.manage.allEntries[i] = entries[i];
			}
		}
	}
}
