using System.Collections.Generic;

public class SeasonTable
{
	public List<InventoryItem> morning = new List<InventoryItem>();

	public List<InventoryItem> day = new List<InventoryItem>();

	public List<InventoryItem> night = new List<InventoryItem>();

	public void addToTable(InventoryItem toAdd, SeasonAndTime.timeOfDay[] myTime)
	{
		for (int i = 0; i < myTime.Length; i++)
		{
			switch (myTime[i])
			{
			case SeasonAndTime.timeOfDay.All:
				if (!morning.Contains(toAdd))
				{
					morning.Add(toAdd);
				}
				if (!day.Contains(toAdd))
				{
					day.Add(toAdd);
				}
				if (!night.Contains(toAdd))
				{
					night.Add(toAdd);
				}
				break;
			case SeasonAndTime.timeOfDay.Morning:
				if (!morning.Contains(toAdd))
				{
					morning.Add(toAdd);
				}
				break;
			case SeasonAndTime.timeOfDay.Day:
				if (!day.Contains(toAdd))
				{
					day.Add(toAdd);
				}
				break;
			case SeasonAndTime.timeOfDay.Night:
				if (!night.Contains(toAdd))
				{
					night.Add(toAdd);
				}
				break;
			}
		}
	}
}
