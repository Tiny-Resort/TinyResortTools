using System.Collections;
using UnityEngine;

public class VehicleVisability : MonoBehaviour
{
	public MeshRenderer[] myRenderers;

	private bool visiable;

	private static WaitForSeconds distanceCheckTimer = new WaitForSeconds(0.5f);

	private Vehicle myVehicle;

	private IEnumerator Start()
	{
		myVehicle = GetComponent<Vehicle>();
		while (true)
		{
			if (isCloseEnoughToSee())
			{
				renderersAreEnabled(true);
				while (isCloseEnoughToSee())
				{
					yield return distanceCheckTimer;
				}
				renderersAreEnabled(false);
			}
			else
			{
				renderersAreEnabled(false);
				while (!isCloseEnoughToSee())
				{
					yield return distanceCheckTimer;
				}
				renderersAreEnabled(true);
			}
			yield return distanceCheckTimer;
		}
	}

	public bool isCloseEnoughToSee()
	{
		if (myVehicle.hasDriver() || NewChunkLoader.loader.inside)
		{
			return true;
		}
		return Vector3.Distance(new Vector3(CameraController.control.transform.position.x, 0f, CameraController.control.transform.position.z), new Vector3(base.transform.position.x, 0f, base.transform.position.z)) <= (float)NetworkNavMesh.nav.animalDistance * 1.5f;
	}

	public void renderersAreEnabled(bool renderersOn)
	{
		for (int i = 0; i < myRenderers.Length; i++)
		{
			myRenderers[i].enabled = renderersOn;
		}
	}
}
