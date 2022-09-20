using UnityEngine;

public class BugExhibit : MonoBehaviour
{
	public int showingBugId;

	private GameObject myBugModel;

	public AnimateShopAnimal myAnimate;

	public void placeBugAndShowDisplay(int bugId, Vector2 walkSize)
	{
		showingBugId = bugId;
		if ((bool)Inventory.inv.allItems[bugId].bug)
		{
			myBugModel = Object.Instantiate(Inventory.inv.allItems[bugId].bug.insectType, base.transform);
		}
		else
		{
			myBugModel = Object.Instantiate(Inventory.inv.allItems[bugId].underwaterCreature.creatureModel, base.transform);
		}
		myBugModel.transform.localPosition = Vector3.zero;
		myBugModel.GetComponent<BugAppearance>().setUpBug(Inventory.inv.allItems[bugId]);
		myAnimate.walkAnim = myBugModel.GetComponentInChildren<Animator>();
		myAnimate.walkDistance = walkSize;
		if ((bool)Inventory.inv.allItems[bugId].bug)
		{
			myAnimate.resetStartingPosAndRandomisePos(Inventory.inv.allItems[bugId].bug.bugBaseSpeed);
		}
		else
		{
			myAnimate.resetStartingPosAndRandomisePos(Inventory.inv.allItems[bugId].underwaterCreature.baseSpeed);
		}
	}
}
