using System;

[Serializable]
public class PediaEntry
{
	public int itemId;

	public int entryType;

	public float largestSize = 1f;

	public int amountCaught;

	public PediaEntry(int myItemId, PediaManager.PediaEntryType myType)
	{
		itemId = myItemId;
		entryType = (int)myType;
	}

	public PediaEntry()
	{
	}
}
