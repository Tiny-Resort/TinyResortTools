using UnityEngine;

public class VehicleHitBox : MonoBehaviour
{
	public Vehicle connectedTo;

	public GameObject destroyWhenConnected;

	private void OnDestroy()
	{
		if ((bool)destroyWhenConnected)
		{
			Object.Destroy(destroyWhenConnected);
		}
	}
}
