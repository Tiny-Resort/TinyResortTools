using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PhotoDetails
{
	public string photoName;

	public string photoNickname;

	public PhotographedObject[] inThisPhoto;

	private int dayTaken;

	private int monthTaken;

	private int yearTaken;

	private string islandNameTaken;

	private float[] locationTaken = new float[3];

	private int hourTaken;

	private int minuteTaken;

	private bool takenAtHome = true;

	public PhotoDetails(string newPhotoName, List<PhotographedObject> objectsInPhoto, Vector3 savedFromLocation, int score)
	{
		photoName = newPhotoName;
		inThisPhoto = objectsInPhoto.ToArray();
		locationTaken = new float[3] { savedFromLocation.x, savedFromLocation.y, savedFromLocation.z };
		islandNameTaken = NetworkMapSharer.share.islandName;
		hourTaken = RealWorldTimeLight.time.currentHour;
		minuteTaken = RealWorldTimeLight.time.currentMinute;
		takenAtHome = NetworkMapSharer.share.isServer;
		photoNickname = "Untitled";
		fillDate();
	}

	public PhotoDetails()
	{
		photoName = "Dummy";
	}

	public bool checkIfPhotoWasTakenNearLocation(Vector3 checkPosition, float checkDistance)
	{
		if (locationTaken == null)
		{
			return false;
		}
		Vector3 b = new Vector3(locationTaken[0], locationTaken[1], locationTaken[2]);
		if (Vector3.Distance(checkPosition, b) < checkDistance)
		{
			return true;
		}
		return false;
	}

	public bool checkIfListOfAnimalsIsInPhoto(AnimalAI[] animalsRequiredInPicture)
	{
		List<AnimalAI> list = new List<AnimalAI>();
		for (int i = 0; i < animalsRequiredInPicture.Length; i++)
		{
			list.Add(animalsRequiredInPicture[i]);
		}
		for (int j = 0; j < inThisPhoto.Length; j++)
		{
			if (list.Count <= 0)
			{
				continue;
			}
			for (int k = 0; k < list.Count; k++)
			{
				if (inThisPhoto[j].objectType == 0 && inThisPhoto[j].id == list[k].animalId)
				{
					list.RemoveAt(k);
				}
			}
		}
		if (list.Count == 0)
		{
			return true;
		}
		return false;
	}

	public bool checkIfCarryableIsInPhoto(int[] carryableId)
	{
		for (int i = 0; i < carryableId.Length; i++)
		{
			for (int j = 0; j < inThisPhoto.Length; j++)
			{
				if (inThisPhoto[i].objectType == 3 && inThisPhoto[j].id == carryableId[i])
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool checkIfNPCIsInPhoto(int[] npcId)
	{
		for (int i = 0; i < npcId.Length; i++)
		{
			for (int j = 0; j < inThisPhoto.Length; j++)
			{
				if (inThisPhoto[i].objectType == 1 && inThisPhoto[j].id == npcId[i])
				{
					Debug.Log("NPC was found in photo id " + j);
					return true;
				}
			}
		}
		return false;
	}

	public bool checkIfPhotoIsTakenInBiome(int[] biomeId)
	{
		if (locationTaken == null)
		{
			Debug.Log("No locaiton on photo");
			return false;
		}
		Debug.Log("Photo taken in biome = " + GenerateMap.generate.checkBiomType(Mathf.RoundToInt(locationTaken[0] / 2f), Mathf.RoundToInt(locationTaken[2] / 2f)));
		Debug.Log("Biome being checked against = " + biomeId[0]);
		if (GenerateMap.generate.checkBiomType(Mathf.RoundToInt(locationTaken[0] / 2f), Mathf.RoundToInt(locationTaken[2] / 2f)) == biomeId[0])
		{
			return true;
		}
		return false;
	}

	public int getBiomeTakenIn()
	{
		if (locationTaken == null)
		{
			return 0;
		}
		return GenerateMap.generate.checkBiomType(Mathf.RoundToInt(locationTaken[0] / 2f), Mathf.RoundToInt(locationTaken[2] / 2f));
	}

	public string getListOfPhotoContents()
	{
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		string text = "";
		for (int i = 0; i < inThisPhoto.Length; i++)
		{
			if (inThisPhoto[i].objectType == 0)
			{
				string item = AnimalManager.manage.allAnimals[inThisPhoto[i].id].animalName;
				if (inThisPhoto[i].id == 1 || inThisPhoto[i].id == 2)
				{
					item = Inventory.inv.allItems[inThisPhoto[i].variation].getInvItemName();
				}
				if (!list.Contains(item))
				{
					list.Add(item);
					list2.Add(1);
				}
				else
				{
					list2[list.IndexOf(item)]++;
				}
			}
			if (inThisPhoto[i].objectType == 1)
			{
				text = text + NPCManager.manage.NPCDetails[inThisPhoto[i].id].NPCName + "\n";
			}
			if (inThisPhoto[i].objectType == 2)
			{
				text = text + inThisPhoto[i].otherObjectName + "\n";
			}
			if (inThisPhoto[i].objectType == 3)
			{
				text = text + SaveLoad.saveOrLoad.carryablePrefabs[inThisPhoto[i].id].name + "\n";
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			text = ((list2[j] <= 1) ? (text + list[j] + "\n") : (text + list2[j] + " <b>x</b> " + list[j] + "\n"));
		}
		return text;
	}

	public void fillDate()
	{
		dayTaken = WorldManager.manageWorld.day * WorldManager.manageWorld.week;
		monthTaken = WorldManager.manageWorld.month;
		yearTaken = WorldManager.manageWorld.year;
	}

	public string getDateString()
	{
		return dayTaken + " " + RealWorldTimeLight.time.getSeasonName(monthTaken - 1) + ", Year " + yearTaken;
	}

	public string getIslandName()
	{
		if (islandNameTaken == null)
		{
			islandNameTaken = NetworkMapSharer.share.islandName;
		}
		return islandNameTaken;
	}

	public string getTimeString()
	{
		string text = "<size=10>PM</size>";
		int num = hourTaken;
		if (hourTaken < 12)
		{
			text = "<size=10>AM</size>";
		}
		else if (hourTaken > 12)
		{
			num -= 12;
		}
		if (hourTaken != 0)
		{
			return num.ToString("00") + ":" + minuteTaken.ToString("00") + text;
		}
		return "Late";
	}
}
