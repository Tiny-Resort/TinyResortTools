using System;

[Serializable]
internal class NPCsave
{
	public NPCStatus[] savedStatuses;

	public NPCInventory[] saveInvs;

	public void loadNpcs()
	{
		if (savedStatuses != null)
		{
			for (int i = 0; i < savedStatuses.Length; i++)
			{
				if (i < savedStatuses.Length)
				{
					NPCManager.manage.npcStatus[i] = savedStatuses[i];
					NPCManager.manage.npcStatus[i].hasBeenTalkedToToday = false;
				}
			}
		}
		if (saveInvs == null)
		{
			return;
		}
		for (int j = 0; j < saveInvs.Length; j++)
		{
			if (j < saveInvs.Length)
			{
				NPCManager.manage.npcInvs[j] = saveInvs[j];
			}
		}
	}
}
