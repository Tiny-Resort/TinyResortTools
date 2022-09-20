using UnityEngine;

public class SeasonAndTime : MonoBehaviour
{
	public enum landLocation
	{
		All = 0,
		Bushland = 1,
		Tropics = 2,
		Pines = 3,
		Plains = 4,
		Desert = 5
	}

	public enum waterLocation
	{
		NorthOcean = 0,
		SouthOcean = 1,
		Rivers = 2,
		Mangroves = 3,
		Billabongs = 4
	}

	public enum seasonFound
	{
		All = 0,
		Summer = 1,
		Autum = 2,
		Winter = 3,
		Spring = 4
	}

	public enum timeOfDay
	{
		All = 0,
		Morning = 1,
		Day = 2,
		Night = 3
	}

	public enum rarity
	{
		Common = 0,
		Uncommon = 1,
		Rare = 2,
		VeryRare = 3,
		SuperRare = 4
	}

	public landLocation[] myLandLocation;

	public waterLocation[] myWaterLocation;

	public seasonFound[] mySeasons;

	public timeOfDay[] myTimeOfDay;

	public rarity myRarity;

	public string getLocationName()
	{
		string text = "";
		if (myWaterLocation.Length != 0)
		{
			waterLocation[] array = myWaterLocation;
			for (int i = 0; i < array.Length; i++)
			{
				switch (array[i])
				{
				case waterLocation.Billabongs:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.billabongFish.locationName);
					break;
				case waterLocation.NorthOcean:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.northernOceanFish.locationName);
					break;
				case waterLocation.SouthOcean:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.southernOceanFish.locationName);
					break;
				case waterLocation.Rivers:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.riverFish.locationName);
					break;
				case waterLocation.Mangroves:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.mangroveFish.locationName);
					break;
				}
			}
		}
		if (myLandLocation.Length != 0)
		{
			landLocation[] array2 = myLandLocation;
			for (int i = 0; i < array2.Length; i++)
			{
				switch (array2[i])
				{
				case landLocation.All:
					return "Everywhere";
				case landLocation.Bushland:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.bushlandBugs.locationName);
					break;
				case landLocation.Desert:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.desertBugs.locationName);
					break;
				case landLocation.Pines:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.pineLandBugs.locationName);
					break;
				case landLocation.Plains:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.plainsBugs.locationName);
					break;
				case landLocation.Tropics:
					if (text != "")
					{
						text += " & ";
					}
					text += capitaliseFirstLetter(AnimalManager.manage.topicalBugs.locationName);
					break;
				}
			}
		}
		return text;
	}

	public void getSeasonName()
	{
		PediaManager.manage.seasonIcons[0].color = PediaManager.manage.iconOffColor;
		PediaManager.manage.seasonIcons[1].color = PediaManager.manage.iconOffColor;
		PediaManager.manage.seasonIcons[2].color = PediaManager.manage.iconOffColor;
		PediaManager.manage.seasonIcons[3].color = PediaManager.manage.iconOffColor;
		seasonFound[] array = mySeasons;
		foreach (seasonFound seasonFound in array)
		{
			if (seasonFound == seasonFound.All)
			{
				PediaManager.manage.seasonIcons[0].color = PediaManager.manage.iconOnColor;
				PediaManager.manage.seasonIcons[1].color = PediaManager.manage.iconOnColor;
				PediaManager.manage.seasonIcons[2].color = PediaManager.manage.iconOnColor;
				PediaManager.manage.seasonIcons[3].color = PediaManager.manage.iconOnColor;
			}
			else
			{
				PediaManager.manage.seasonIcons[(int)(seasonFound - 1)].color = PediaManager.manage.iconOnColor;
			}
		}
	}

	public void getTime()
	{
		PediaManager.manage.timeIcons[0].color = PediaManager.manage.iconOffColor;
		PediaManager.manage.timeIcons[1].color = PediaManager.manage.iconOffColor;
		PediaManager.manage.timeIcons[2].color = PediaManager.manage.iconOffColor;
		timeOfDay[] array = myTimeOfDay;
		foreach (timeOfDay timeOfDay in array)
		{
			switch (timeOfDay)
			{
			case timeOfDay.All:
				PediaManager.manage.timeIcons[0].color = PediaManager.manage.iconOnColor;
				PediaManager.manage.timeIcons[1].color = PediaManager.manage.iconOnColor;
				PediaManager.manage.timeIcons[2].color = PediaManager.manage.iconOnColor;
				continue;
			case timeOfDay.Morning:
				PediaManager.manage.timeIcons[0].color = PediaManager.manage.iconOnColor;
				break;
			}
			if (timeOfDay == timeOfDay.Day)
			{
				PediaManager.manage.timeIcons[1].color = PediaManager.manage.iconOnColor;
			}
			if (timeOfDay == timeOfDay.Night)
			{
				PediaManager.manage.timeIcons[2].color = PediaManager.manage.iconOnColor;
			}
		}
	}

	private string capitaliseFirstLetter(string toChange)
	{
		toChange = char.ToUpper(toChange[0]) + toChange.Substring(1);
		return toChange;
	}
}
