using System;
using UnityEngine;

[Serializable]
internal class LicenceAndPermitPointSave
{
	public int permitPoints;

	public Licence[] licenceSave = new Licence[0];

	public Milestone[] milestoneSave = new Milestone[0];

	public void saveLicencesAndPoints()
	{
		permitPoints = PermitPointsManager.manage.getCurrentPoints();
		licenceSave = LicenceManager.manage.allLicences;
		milestoneSave = MilestoneManager.manage.milestones.ToArray();
	}

	public void loadLicencesAndPoints()
	{
		PermitPointsManager.manage.loadFromSave(permitPoints);
		for (int i = 0; i < licenceSave.Length; i++)
		{
			if (licenceSave[i] != null)
			{
				Debug.Log("Loading licnces");
				LicenceManager.manage.allLicences[i] = licenceSave[i];
				if (LicenceManager.manage.allLicences[i].type == LicenceManager.LicenceTypes.None)
				{
					LicenceManager.manage.allLicences[i] = new Licence((LicenceManager.LicenceTypes)i);
				}
			}
		}
		LicenceManager.manage.setLicenceLevelsAndPrice();
		LicenceManager.manage.checkAllLicenceRewardsOnLoad();
		Inventory.inv.setSlotsUnlocked();
		if (milestoneSave != null)
		{
			for (int j = 0; j < milestoneSave.Length; j++)
			{
				MilestoneManager.manage.milestones[j] = milestoneSave[j];
			}
			MilestoneManager.manage.updateAfterSave();
		}
		LicenceManager.manage.unlockRecipesAlreadyLearntFromAllLicences();
	}
}
