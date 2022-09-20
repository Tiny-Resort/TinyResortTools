using UnityEngine;

public class BugAppearance : MonoBehaviour
{
	public SkinnedMeshRenderer skinRen;

	public MeshFilter body;

	public MeshFilter wingsOrLegs;

	private ASound movementSound;

	public Transform groundBugCheckTrans;

	public Transform setUpBug(InventoryItem bugInv)
	{
		base.transform.localScale = bugInv.transform.localScale;
		if ((bool)bugInv.underwaterCreature)
		{
			if ((bool)bugInv.underwaterCreature.altBody)
			{
				if ((bool)skinRen)
				{
					skinRen.sharedMesh = bugInv.underwaterCreature.altBody;
				}
				if ((bool)body)
				{
					body.mesh = bugInv.underwaterCreature.altBody;
				}
			}
		}
		else
		{
			if ((bool)bugInv.bug.altBody)
			{
				if ((bool)skinRen)
				{
					skinRen.sharedMesh = bugInv.bug.altBody;
				}
				if ((bool)body)
				{
					body.mesh = bugInv.bug.altBody;
				}
			}
			if ((bool)bugInv.bug.altWingsOrLegs)
			{
				wingsOrLegs.mesh = bugInv.bug.altWingsOrLegs;
			}
			if ((bool)bugInv.bug.control)
			{
				GetComponent<Animator>().runtimeAnimatorController = bugInv.bug.control;
			}
			if ((bool)bugInv.bug.bugMovementSound)
			{
				movementSound = bugInv.bug.bugMovementSound;
			}
		}
		return groundBugCheckTrans;
	}

	public void makeMovementSound()
	{
		if ((bool)movementSound)
		{
			SoundManager.manage.playASoundAtPoint(movementSound, base.transform.position);
		}
	}
}
