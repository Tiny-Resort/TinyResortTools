using System;

[Serializable]
public class ItemChangeType
{
	public TileObject depositInto;

	public int amountNeededed = 1;

	public int secondsToComplete = 5;

	public int daysToComplete;

	public int cycles = 1;

	public InventoryItem changesWhenComplete;

	public InventoryItemLootTable changesWhenCompleteTable;

	public DailyTaskGenerator.genericTaskType taskType;

	public bool givesXp;

	public CharLevelManager.SkillTypes xPType;
}
