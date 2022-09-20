using UnityEngine;

public class ItemChange : MonoBehaviour
{
	public ItemChangeType[] changesAndTheirChanger;

	public int getChangerResultId(int itemChangerId)
	{
		ItemChangeType[] array = changesAndTheirChanger;
		foreach (ItemChangeType itemChangeType in array)
		{
			if (itemChangeType.depositInto.tileObjectId == itemChangerId)
			{
				if ((bool)itemChangeType.changesWhenCompleteTable)
				{
					return Inventory.inv.getInvItemId(itemChangeType.changesWhenCompleteTable.getRandomDropFromTable());
				}
				return Inventory.inv.getInvItemId(itemChangeType.changesWhenComplete);
			}
		}
		return -1;
	}

	public int getChangeTime(int itemChangerId)
	{
		ItemChangeType[] array = changesAndTheirChanger;
		foreach (ItemChangeType itemChangeType in array)
		{
			if (itemChangeType.depositInto.tileObjectId == itemChangerId)
			{
				return itemChangeType.secondsToComplete;
			}
		}
		return -1;
	}

	public int getChangeDays(int itemChangerId)
	{
		ItemChangeType[] array = changesAndTheirChanger;
		foreach (ItemChangeType itemChangeType in array)
		{
			if (itemChangeType.depositInto.tileObjectId == itemChangerId)
			{
				return itemChangeType.daysToComplete;
			}
		}
		return -1;
	}

	public int getCycles(int itemChangerId)
	{
		ItemChangeType[] array = changesAndTheirChanger;
		foreach (ItemChangeType itemChangeType in array)
		{
			if (itemChangeType.depositInto.tileObjectId == itemChangerId)
			{
				return itemChangeType.cycles;
			}
		}
		return -1;
	}

	public int getAmountNeeded(int itemChangerId)
	{
		ItemChangeType[] array = changesAndTheirChanger;
		foreach (ItemChangeType itemChangeType in array)
		{
			if (itemChangeType.depositInto.tileObjectId == itemChangerId)
			{
				return itemChangeType.amountNeededed;
			}
		}
		return 0;
	}

	public bool checkIfCanBeDeposited(int itemChangerId)
	{
		ItemChangeType[] array = changesAndTheirChanger;
		foreach (ItemChangeType itemChangeType in array)
		{
			if (itemChangeType.depositInto.tileObjectId == itemChangerId && Inventory.inv.getAmountOfItemInAllSlots(Inventory.inv.getInvItemId(GetComponent<InventoryItem>())) >= itemChangeType.amountNeededed)
			{
				return true;
			}
		}
		return false;
	}

	public bool checkIfCanBeDepositedServer(int itemChangerId)
	{
		ItemChangeType[] array = changesAndTheirChanger;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].depositInto.tileObjectId == itemChangerId)
			{
				return true;
			}
		}
		return false;
	}

	public void checkTask(int itemChangerId)
	{
		ItemChangeType[] array = changesAndTheirChanger;
		foreach (ItemChangeType itemChangeType in array)
		{
			if (itemChangeType.depositInto.tileObjectId != itemChangerId)
			{
				continue;
			}
			DailyTaskGenerator.generate.doATask(itemChangeType.taskType);
			if (itemChangeType.givesXp)
			{
				CharLevelManager.manage.addXp(itemChangeType.xPType, 1);
				if ((bool)itemChangeType.changesWhenComplete)
				{
					CharLevelManager.manage.addToDayTally(Inventory.inv.getInvItemId(itemChangeType.changesWhenComplete), 1, (int)itemChangeType.xPType);
				}
			}
		}
	}
}
