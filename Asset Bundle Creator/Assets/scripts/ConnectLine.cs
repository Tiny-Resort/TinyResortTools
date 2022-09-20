using UnityEngine;

public class ConnectLine : MonoBehaviour
{
	public Transform conectedTo;

	private LineRenderer line;

	private void Start()
	{
		line = GetComponent<LineRenderer>();
	}

	private void Update()
	{
		line.SetPosition(0, base.transform.position);
		line.SetPosition(1, conectedTo.position);
	}
}
