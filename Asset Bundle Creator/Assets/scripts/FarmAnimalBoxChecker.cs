using UnityEngine;

public class FarmAnimalBoxChecker : MonoBehaviour
{
	public LayerMask carryableLayer;

	public Transform moveToIfFound;

	public Vector3 size;

	private void OnEnable()
	{
		WorldManager.manageWorld.changeDayEvent.AddListener(endOfDayAnimalCheckAndMove);
	}

	private void OnDisable()
	{
		WorldManager.manageWorld.changeDayEvent.RemoveListener(endOfDayAnimalCheckAndMove);
	}

	public void endOfDayAnimalCheckAndMove()
	{
		checkMyBox(true);
	}

	public bool checkIfAnimalIsInBuilding()
	{
		return checkMyBox();
	}

	public bool checkMyBox(bool moveBox = false)
	{
		Collider[] array = Physics.OverlapBox(base.transform.position, size, Quaternion.identity, carryableLayer);
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			if ((bool)array[i].GetComponentInParent<AnimalCarryBox>())
			{
				flag = true;
				if (moveBox)
				{
					MonoBehaviour.print("moving a box");
					array[i].transform.root.position = moveToIfFound.transform.position;
					array[i].GetComponentInParent<PickUpAndCarry>().moveToNewDropPos(moveToIfFound.transform.position.y);
				}
				else if (!moveBox && flag)
				{
					break;
				}
			}
		}
		return flag;
	}
}
