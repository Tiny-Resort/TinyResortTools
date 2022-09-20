using System;
using UnityEngine;

[Serializable]
public class Licence
{
	public LicenceManager.LicenceTypes type;

	public int maxLevel = 3;

	public int currentLevel;

	public int levelCostMuliplier = 2;

	public int levelCost = 250;

	public bool isUnlocked;

	public bool unlockedWithLevel;

	public CharLevelManager.SkillTypes unlockedBySkill;

	public int unlockedEveryLevel = 3;

	public bool hasBeenSeenBefore;

	public int sortingNumber;

	public Licence()
	{
	}

	public Licence(LicenceManager.LicenceTypes newType)
	{
		type = newType;
	}

	public int getCurrentLevel()
	{
		return currentLevel;
	}

	public bool isLevelAvaliable()
	{
		if (currentLevel >= maxLevel)
		{
			return false;
		}
		return true;
	}

	public int getNextLevelPrice()
	{
		return (currentLevel + 1) * levelCost * Mathf.Clamp(currentLevel * levelCostMuliplier, 1, 100);
	}

	public bool canAffordNextLevel()
	{
		if (unlockedWithLevel && CharLevelManager.manage.currentLevels[(int)unlockedBySkill] < currentLevel * unlockedEveryLevel)
		{
			return false;
		}
		return PermitPointsManager.manage.checkIfCanAfford(getNextLevelPrice());
	}

	public void buyNextLevel()
	{
		PermitPointsManager.manage.spendPoints(getNextLevelPrice());
		currentLevel++;
		LicenceManager.manage.checkForUnlocksOnLevelUp(this);
	}

	public bool hasALevelOneOrHigher()
	{
		return currentLevel >= 1;
	}

	public void setLevelCost(int newLevelCost, int newMultiplyer = 2)
	{
		levelCostMuliplier = newMultiplyer;
		levelCost = newLevelCost;
	}

	public void connectToSkillLevel(CharLevelManager.SkillTypes connectedType, int levelsUpEvery)
	{
		unlockedWithLevel = true;
		unlockedBySkill = connectedType;
		unlockedEveryLevel = levelsUpEvery;
		isUnlocked = true;
	}

	public bool isConnectedWithSkillLevel()
	{
		return unlockedWithLevel;
	}

	public string getConnectedSkillName()
	{
		return unlockedBySkill.ToString();
	}

	public int getConnectedSkillId()
	{
		return (int)unlockedBySkill;
	}

	public int getMaxLevel()
	{
		return maxLevel;
	}

	public int getCurrentMaxLevel()
	{
		if (unlockedWithLevel)
		{
			for (int i = 0; i < maxLevel; i++)
			{
				if (i * unlockedEveryLevel > CharLevelManager.manage.currentLevels[(int)unlockedBySkill])
				{
					return i;
				}
			}
		}
		return maxLevel;
	}
}
