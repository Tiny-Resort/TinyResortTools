using UnityEngine;

public class LimitBobberDummy : MonoBehaviour
{
	public Transform rodsEnd;

	private void Update()
	{
		if (Vector3.Distance(base.transform.position, rodsEnd.position) > 3f)
		{
			Vector3 normalized = (base.transform.position - rodsEnd.position).normalized;
			base.transform.position = rodsEnd.position + normalized * 3f;
		}
	}
}
