using UnityEngine;

public class LevelTerrain : MonoBehaviour
{
	public CharInteract myCharInteract;

	public Animator myAnim;

	private void Start()
	{
		myCharInteract = GetComponentInParent<CharInteract>();
	}

	private void doLevelTerrain()
	{
		if (!myCharInteract)
		{
			return;
		}
		int num;
		int num2;
		if (myCharInteract.isLocalPlayer)
		{
			num = (int)myCharInteract.selectedTile.x;
			num2 = (int)myCharInteract.selectedTile.y;
		}
		else
		{
			num = (int)myCharInteract.currentlyAttackingPos.x;
			num2 = (int)myCharInteract.currentlyAttackingPos.y;
		}
		if (myCharInteract.checkIfCanDamage(new Vector2(num, num2)))
		{
			if (WorldManager.manageWorld.heightMap[num, num2] == Mathf.RoundToInt(base.transform.root.position.y + 1f) && !WorldManager.manageWorld.checkTileClientLock(num, num2))
			{
				myCharInteract.doDamage();
			}
			else if (WorldManager.manageWorld.heightMap[num, num2] > Mathf.RoundToInt(base.transform.root.position.y + 1f))
			{
				myAnim.SetTrigger("Clang");
			}
		}
		else if ((bool)myAnim)
		{
			myAnim.SetTrigger("Clang");
		}
	}

	private void OnDisable()
	{
		if ((bool)myCharInteract && myCharInteract.isLocalPlayer)
		{
			myCharInteract.GetComponent<CharMovement>().canClimb = true;
		}
	}
}
