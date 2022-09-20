using UnityEngine;

public class ToolWorksAtNight : MonoBehaviour
{
	public MeleeAttacks myAttacks;

	public void useAtNight()
	{
		if (RealWorldTimeLight.time.currentHour >= 18 || RealWorldTimeLight.time.currentHour <= 6 || RealWorldTimeLight.time.underGround)
		{
			myAttacks.attack();
		}
		else if (myAttacks.consumeFuelOnSwing)
		{
			Inventory.inv.useItemWithFuel();
		}
	}
}
