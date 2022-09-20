using UnityEngine;

public class FollowObjectLimit : MonoBehaviour
{
	public Transform followTransform;

	public float limit = 0.3f;

	private Vector3 desiredPos;

	private void Update()
	{
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, new Vector3(Mathf.Clamp(followTransform.localPosition.x, 0f - limit, limit), Mathf.Clamp(followTransform.localPosition.y, 0f - limit, limit), -0.5f), Time.deltaTime * 10f);
	}
}
