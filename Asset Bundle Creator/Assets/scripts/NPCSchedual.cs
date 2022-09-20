using System.Collections.Generic;
using UnityEngine;

public class NPCSchedual : MonoBehaviour
{
	public enum Locations
	{
		Wonder = 0,
		Exit = 1,
		Market_place = 2,
		Johns_Goods = 3,
		Craft_Workshop = 4,
		Post_Office = 5,
		Cloth_Shop = 6,
		Weapon_Shop = 7,
		Museum = 8,
		Animal_Shop = 9,
		Furniture_Shop = 10,
		Plant_Shop = 11,
		Town_Hall = 12,
		Harbour_House = 13,
		Bank = 14,
		Hair_Dresser = 15,
		NPCHouse1 = 16,
		NPCHouse2 = 17,
		NPCHouse3 = 18,
		NPCHouse4 = 19,
		JimmysBoat = 20,
		Telepad = 21,
		Mine = 22
	}

	public string[] visitorTimeTable;

	public Locations[] dailySchedual;

	public bool[] dayOff = new bool[7];

	public Locations[] dayOff1;

	public Locations[] dayOff2;

	public Locations[] dayOff3;

	private int dayOffNo = -1;

	public void randomiseDayOffSchedual()
	{
		dayOffNo = Random.Range(0, 3);
	}

	public Locations getDesiredLocation(int npcNo, int hour)
	{
		if (NPCManager.manage.npcStatus[npcNo].checkIfHasMovedIn() || npcNo == 11 || npcNo == 5)
		{
			if (!dayOff[WorldManager.manageWorld.day - 1])
			{
				return dailySchedual[hour];
			}
			if (dayOffNo == -1)
			{
				randomiseDayOffSchedual();
			}
			if (dayOffNo == 0)
			{
				return dayOff1[hour];
			}
			if (dayOffNo == 1)
			{
				return dayOff2[hour];
			}
			return dayOff3[hour];
		}
		return NPCManager.manage.visitingSchedual[hour];
	}

	public string getNextDayOffName()
	{
		for (int i = WorldManager.manageWorld.day - 1; i < dayOff.Length; i++)
		{
			if (dayOff[i])
			{
				return RealWorldTimeLight.time.getDayName(i);
			}
		}
		for (int j = 0; j < dayOff.Length; j++)
		{
			if (dayOff[j])
			{
				return RealWorldTimeLight.time.getDayName(j);
			}
		}
		return "No Day off";
	}

	public bool checkIfOpen()
	{
		if (RealWorldTimeLight.time.currentHour == 0 || RealWorldTimeLight.time.currentHour == 24)
		{
			return false;
		}
		if (dayOff[WorldManager.manageWorld.day - 1])
		{
			return false;
		}
		if (dailySchedual[RealWorldTimeLight.time.currentHour] != 0 && dailySchedual[RealWorldTimeLight.time.currentHour] != Locations.Exit)
		{
			return true;
		}
		return false;
	}

	public bool checkIfLate()
	{
		if (RealWorldTimeLight.time.currentHour == 0 || RealWorldTimeLight.time.currentHour == 24)
		{
			return false;
		}
		if (dayOff[WorldManager.manageWorld.day - 1])
		{
			return false;
		}
		if (dailySchedual[RealWorldTimeLight.time.currentHour] != 0 && dailySchedual[RealWorldTimeLight.time.currentHour] != Locations.Exit && (dailySchedual[RealWorldTimeLight.time.currentHour - 1] != 0 || RealWorldTimeLight.time.currentMinute >= 15))
		{
			return true;
		}
		return false;
	}

	public string getOpeningHours()
	{
		string text = "Open: ";
		bool flag = false;
		for (int i = 8; i < dailySchedual.Length; i++)
		{
			if (dailySchedual[i] != 0 && !flag)
			{
				flag = true;
				text = ((i >= 12) ? ((i <= 12) ? (text + (i - 12) + "PM") : (text + i + "PM")) : (text + i + "AM - "));
			}
			else if (dailySchedual[i] == Locations.Exit)
			{
				text = ((i >= 12) ? ((i != 12) ? (text + (i - 12) + "PM") : (text + i + "PM")) : (text + i + "AM"));
				break;
			}
		}
		return text;
	}

	public string getDaysClosed()
	{
		string text = "Closed: ";
		List<string> list = new List<string>();
		for (int i = 0; i < dayOff.Length; i++)
		{
			if (dayOff[i])
			{
				list.Add(RealWorldTimeLight.time.getDayName(i));
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			text = ((j != 0) ? ((j != list.Count - 1) ? (text + ", " + list[j]) : (text + " and " + list[j])) : (text + list[j]));
		}
		return text;
	}
}
