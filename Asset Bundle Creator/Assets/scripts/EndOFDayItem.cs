public class EndOFDayItem
{
	public int id = -1;

	public int pickUpType = -1;

	public int currentTotal;

	public EndOFDayItem(int itemId, int amount, int skillType)
	{
		id = itemId;
		pickUpType = skillType;
		currentTotal = amount;
	}
}
