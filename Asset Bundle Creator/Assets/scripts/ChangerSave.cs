using System;

[Serializable]
public class ChangerSave
{
	public CurrentChanger[] allChangers = new CurrentChanger[0];

	public void saveChangers()
	{
		allChangers = WorldManager.manageWorld.allChangers.ToArray();
	}

	public void loadChangers()
	{
		for (int i = 0; i < allChangers.Length; i++)
		{
			WorldManager.manageWorld.loadCountDownForTile(allChangers[i]);
		}
	}
}
