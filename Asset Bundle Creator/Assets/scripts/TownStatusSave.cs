using System;

[Serializable]
internal class TownStatusSave
{
	public float[] beautyLevels = new float[6];

	public int moneySpentInTownTotal;

	public int townDebt;

	public int paidDebt;

	public void saveTownStatus()
	{
		beautyLevels = TownManager.manage.beautyLevels;
		moneySpentInTownTotal = TownManager.manage.moneySpentInTownTotal;
		townDebt = NetworkMapSharer.share.townDebt;
	}

	public void loadTownStatus()
	{
		TownManager.manage.beautyLevels = beautyLevels;
		TownManager.manage.moneySpentInTownTotal = moneySpentInTownTotal;
		NetworkMapSharer.share.NetworktownDebt = townDebt;
	}
}
