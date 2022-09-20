using System.Collections.Generic;
using UnityEngine;

public class AnimalDropOffSpot : MonoBehaviour
{
	public enum depositType
	{
		AnimalTrap = 0,
		SellByWeight = 1
	}

	public depositType dropOffType;

	public NPCSchedual.Locations foundInLocation;

	private List<SellByWeight> objectsInThePickUpPoint = new List<SellByWeight>();

	public bool lineUpOnly;

	private void OnTriggerEnter(Collider other)
	{
		if (lineUpOnly)
		{
			return;
		}
		if (dropOffType == depositType.AnimalTrap)
		{
			TrappedAnimal componentInParent = other.GetComponentInParent<TrappedAnimal>();
			if ((bool)componentInParent && !componentInParent.GetComponent<PickUpAndCarry>().delivered)
			{
				componentInParent.GetComponent<PickUpAndCarry>().Networkdelivered = true;
				NetworkMapSharer.share.placeAnimalInCollectionPoint(componentInParent.netId);
			}
		}
		if (dropOffType == depositType.SellByWeight)
		{
			SellByWeight componentInParent2 = other.GetComponentInParent<SellByWeight>();
			if ((bool)componentInParent2)
			{
				NetworkMapSharer.share.RpcSellByWeight(componentInParent2.GetComponentInParent<PickUpAndCarry>().getLastCarriedBy(), componentInParent2.netId, (int)foundInLocation);
			}
		}
	}
}
