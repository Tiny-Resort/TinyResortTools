using UnityEngine;

public class MoveInteriorToConsitantY : MonoBehaviour
{
	public float yPosToMoveTo;

	public void OnEnable()
	{
		base.transform.position = new Vector3(base.transform.position.x, yPosToMoveTo, base.transform.position.z);
	}
}
