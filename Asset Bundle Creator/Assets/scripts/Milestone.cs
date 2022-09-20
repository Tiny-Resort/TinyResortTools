using System;
using UnityEngine;

[Serializable]
public class Milestone
{
	public DailyTaskGenerator.genericTaskType myTaskType;

	public int[] pointsPerLevel = new int[4] { 10, 50, 250, 1000 };

	public int rewardPerLevel = 100;

	public int points;

	public int currentLevel;

	private string milestonePreffix = "";

	private string milestoneSuffix = "";

	public Milestone()
	{
	}

	public Milestone(DailyTaskGenerator.genericTaskType taskType)
	{
		myTaskType = taskType;
	}

	public void changeAmountPerLevel(int[] newLevelSteps)
	{
		pointsPerLevel = newLevelSteps;
	}

	public void changeRewardAmount(int newReward)
	{
		rewardPerLevel = newReward;
	}

	public void addPoints(int pointsToAdd)
	{
		points += pointsToAdd;
	}

	public int getPointsInt()
	{
		return points;
	}

	public string getPoints()
	{
		string text = "";
		if (milestonePreffix != "")
		{
			text = milestonePreffix + " ";
		}
		text = ((!(milestoneSuffix == "m") || points <= 1000) ? (text + points.ToString("n0")) : (text + ((float)points / 1000f).ToString("n0")));
		if (milestoneSuffix != "")
		{
			text = ((!(milestoneSuffix == "m") || points <= 1000) ? (text + " " + milestoneSuffix) : (text + " km"));
		}
		return text;
	}

	public int getRequiredPointsForLevelUp()
	{
		if (currentLevel < pointsPerLevel.Length)
		{
			return pointsPerLevel[currentLevel];
		}
		return 0;
	}

	public string getNextRequiredPointString()
	{
		string text = "";
		if (milestonePreffix != "")
		{
			text = milestonePreffix + " ";
		}
		text = ((!(milestoneSuffix == "m") || points <= 1000) ? (text + getRequiredPointsForLevelUp().ToString("n0")) : (text + ((float)getRequiredPointsForLevelUp() / 1000f).ToString("n0")));
		if (milestoneSuffix != "")
		{
			text = ((!(milestoneSuffix == "m") || points <= 1000) ? (text + " " + milestoneSuffix) : (text + " km"));
		}
		return text;
	}

	public bool checkIfLevelUpAvaliable()
	{
		if (pointsPerLevel == null)
		{
			return false;
		}
		if (currentLevel < pointsPerLevel.Length)
		{
			if (points >= pointsPerLevel[currentLevel])
			{
				return currentLevel <= pointsPerLevel.Length;
			}
			return false;
		}
		return false;
	}

	public float getCurrentLevelFill()
	{
		if (currentLevel >= pointsPerLevel.Length)
		{
			return 1f;
		}
		return Mathf.Clamp01((float)points / ((float)pointsPerLevel[currentLevel] * 1f));
	}

	public int getRewardAmount()
	{
		return rewardPerLevel;
	}

	public void levelUp()
	{
		currentLevel++;
	}

	public bool isVisibleOnList()
	{
		if (currentLevel > 0 || checkIfLevelUpAvaliable())
		{
			return true;
		}
		return false;
	}

	public void setPreffix(string newPrefix)
	{
		milestonePreffix = newPrefix;
	}

	public void setSuffix(string newsuffix)
	{
		milestoneSuffix = newsuffix;
	}
}
