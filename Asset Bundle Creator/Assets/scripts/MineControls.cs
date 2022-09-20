using UnityEngine;

public class MineControls : MonoBehaviour
{
	public bool forEntrance;

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isMineControls = this;
	}

	public void useControls()
	{
		if (forEntrance)
		{
			NetworkMapSharer.share.tryAndMoveUnderGround();
		}
		else
		{
			NetworkMapSharer.share.tryAndMoveAboveGround();
		}
	}
}
