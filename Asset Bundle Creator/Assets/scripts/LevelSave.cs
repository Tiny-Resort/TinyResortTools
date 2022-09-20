using System;

[Serializable]
public class LevelSave
{
	public int[] todaysXp;

	public int[] currentXp;

	public int[] currentLevels;

	public void saveLevels(int[] saveTodaysXp, int[] saveCurrentXp, int[] saveCurrentLevels)
	{
		todaysXp = saveTodaysXp;
		currentXp = saveCurrentXp;
		currentLevels = saveCurrentLevels;
	}

	public void loadLevels()
	{
		if (todaysXp != null)
		{
			CharLevelManager.manage.todaysXp = todaysXp;
		}
		CharLevelManager.manage.currentXp = currentXp;
		CharLevelManager.manage.currentLevels = currentLevels;
	}
}
