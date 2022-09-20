using System;

[Serializable]
internal class HouseListSave
{
	public HouseSave[] houseList;

	public HouseExterior[] houseExteriorList;

	public void save()
	{
		houseExteriorList = HouseManager.manage.allExteriors.ToArray();
		houseList = new HouseSave[HouseManager.manage.allHouses.Count];
		for (int i = 0; i < houseList.Length; i++)
		{
			houseList[i] = new HouseSave(HouseManager.manage.allHouses[i]);
		}
	}

	public void load()
	{
		for (int i = 0; i < houseList.Length; i++)
		{
			HouseManager.manage.allHouses.Add(new HouseDetails());
			houseList[i].loadDetails(HouseManager.manage.allHouses[i]);
		}
		for (int j = 0; j < houseExteriorList.Length; j++)
		{
			HouseManager.manage.allExteriors.Add(houseExteriorList[j]);
		}
	}
}
