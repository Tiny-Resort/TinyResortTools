using System.Collections.Generic;
using UnityEngine;

public class ExhibitSign : MonoBehaviour
{
	public Transform listExhibitInside;

	public Conversation mySign;

	private static string[] emptyConvo;

	private void Start()
	{
		emptyConvo = new string[2] { "This exhibit is currently empty.", "We look forward to future donations!" };
		MuseumManager.manage.onExhibitUpdate.AddListener(updateMySign);
		updateMySign();
	}

	private void updateMySign()
	{
		List<string> list = new List<string>();
		list.Add("In this exhibit:");
		int num = 0;
		string text = "";
		bool flag = false;
		for (int i = 0; i < listExhibitInside.childCount; i++)
		{
			BugExhibit component = listExhibitInside.GetChild(i).GetComponent<BugExhibit>();
			if ((bool)component)
			{
				text = ((num != 0) ? (text + "\n" + UIAnimationManager.manage.getItemColorTag(Inventory.inv.allItems[component.showingBugId].getInvItemName())) : UIAnimationManager.manage.getItemColorTag(Inventory.inv.allItems[component.showingBugId].getInvItemName()));
				flag = true;
				num++;
			}
			else
			{
				FishTankFish component2 = listExhibitInside.GetChild(i).GetComponent<FishTankFish>();
				if ((bool)component2)
				{
					text = ((num != 0) ? (text + "\n" + UIAnimationManager.manage.getItemColorTag(Inventory.inv.allItems[component2.showingFishId].getInvItemName())) : UIAnimationManager.manage.getItemColorTag(Inventory.inv.allItems[component2.showingFishId].getInvItemName()));
					flag = true;
					num++;
				}
			}
			if (num == 4)
			{
				list.Add(text);
				text = "";
				num = 0;
			}
		}
		if (text != "")
		{
			list.Add(text);
		}
		if (!flag)
		{
			mySign.startLineAlt.aConverstationSequnce = emptyConvo;
		}
		else
		{
			mySign.startLineAlt.aConverstationSequnce = list.ToArray();
		}
	}
}
