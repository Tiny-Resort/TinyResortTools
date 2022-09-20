using UnityEngine;

public class WaterFollowsCamera : MonoBehaviour
{
	public Transform cameraToFollow;

	public Vector3 dif;

	public bool followAtStandardHeight;

	private void FixedUpdate()
	{
		if (followAtStandardHeight)
		{
			base.transform.position = new Vector3(cameraToFollow.position.x, 0.6f, cameraToFollow.position.z) + dif;
		}
		else
		{
			base.transform.position = cameraToFollow.position;
		}
	}
}
