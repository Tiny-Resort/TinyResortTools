using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PhotoChallenge
{
	public float[] photoLocation = new float[3];

	public int[] photoTime = new int[6];

	public int subjectType;

	public int[] subjectIds = new int[1];

	public int requiredAmountOfSubject = 1;

	public int reward = 1000;

	public PhotoChallenge()
	{
	}

	public PhotoChallenge(PhotoChallengeManager.photoSubject setSubjectType)
	{
		subjectType = (int)setSubjectType;
		randomiseRequirements();
	}

	public int getReward()
	{
		return reward;
	}

	public bool checkIfPhotoMeetsRequirements(PhotoDetails photoToCheck)
	{
		if (subjectType == 1)
		{
			if (photoToCheck.checkIfListOfAnimalsIsInPhoto(animalsRequiredInPhoto()))
			{
				return true;
			}
		}
		else if (subjectType == 3)
		{
			if (photoToCheck.checkIfPhotoWasTakenNearLocation(new Vector3(photoLocation[0], photoLocation[1], photoLocation[2]), 20f))
			{
				return true;
			}
		}
		else if (subjectType == 2)
		{
			if (photoToCheck.checkIfCarryableIsInPhoto(subjectIds))
			{
				return true;
			}
		}
		else if (subjectType == 0)
		{
			if (photoToCheck.checkIfNPCIsInPhoto(subjectIds))
			{
				return true;
			}
		}
		else if (subjectType == 4 && photoToCheck.checkIfPhotoIsTakenInBiome(subjectIds))
		{
			return true;
		}
		return false;
	}

	public PhotoChallengeManager.photoSubject getSubjectType()
	{
		return (PhotoChallengeManager.photoSubject)subjectType;
	}

	public AnimalAI[] animalsRequiredInPhoto()
	{
		List<AnimalAI> list = new List<AnimalAI>();
		for (int i = 0; i < subjectIds.Length; i++)
		{
			list.Add(AnimalManager.manage.allAnimals[subjectIds[i]]);
		}
		return list.ToArray();
	}

	public int[] returnSubjectId()
	{
		return subjectIds;
	}

	public string returnRequiredLocationBiomeName()
	{
		if (subjectType == 4)
		{
			return GenerateMap.generate.getBiomeNameById(subjectIds[0]);
		}
		return GenerateMap.generate.getBiomeNameById(GenerateMap.generate.checkBiomType(Mathf.RoundToInt(photoLocation[0] / 2f), Mathf.RoundToInt(photoLocation[2] / 2f)));
	}

	public Vector3 getRequiredLocation()
	{
		return new Vector3(photoLocation[0], photoLocation[1], photoLocation[2]);
	}

	public void randomiseRequirements()
	{
		if (subjectType == 1)
		{
			subjectIds = new int[UnityEngine.Random.Range(1, 3)];
			reward = 1000;
			for (int i = 0; i < subjectIds.Length; i++)
			{
				int num = UnityEngine.Random.Range(0, AnimalManager.manage.allAnimals.Length);
				while (!AnimalManager.manage.allAnimals[num].photoRequestable)
				{
					num = UnityEngine.Random.Range(0, AnimalManager.manage.allAnimals.Length);
				}
				subjectIds[i] = num;
				reward += AnimalManager.manage.allAnimals[num].dangerValue * 2;
			}
		}
		else if (subjectType == 3)
		{
			int num2 = UnityEngine.Random.Range(500, 1500);
			int num3 = UnityEngine.Random.Range(500, 1500);
			photoLocation = new float[3]
			{
				num2,
				WorldManager.manageWorld.heightMap[num2 / 2, num3 / 2],
				num3
			};
			reward = UnityEngine.Random.Range(9, 14) * 1000;
		}
		else if (subjectType == 4)
		{
			subjectIds = new int[1] { UnityEngine.Random.Range(0, 12) };
			reward = UnityEngine.Random.Range(6, 14) * 1000;
		}
		else if (subjectType == 2)
		{
			int num4 = UnityEngine.Random.Range(0, SaveLoad.saveOrLoad.carryablePrefabs.Length);
			while (!SaveLoad.saveOrLoad.carryablePrefabs[num4].GetComponent<SellByWeight>() || (num4 == 3 && WorldManager.manageWorld.month != 4))
			{
				num4 = UnityEngine.Random.Range(0, SaveLoad.saveOrLoad.carryablePrefabs.Length);
			}
			subjectIds = new int[1] { num4 };
			reward = Mathf.RoundToInt((float)SaveLoad.saveOrLoad.carryablePrefabs[num4].GetComponent<SellByWeight>().rewardPerKG / 1.25f);
		}
		else if (subjectType == 0)
		{
			int num5 = UnityEngine.Random.Range(0, NPCManager.manage.NPCDetails.Length);
			while (!NPCManager.manage.npcStatus[num5].checkIfHasMovedIn())
			{
				num5 = UnityEngine.Random.Range(0, NPCManager.manage.NPCDetails.Length);
			}
			subjectIds = new int[1] { num5 };
			reward = 4000;
		}
	}
}
