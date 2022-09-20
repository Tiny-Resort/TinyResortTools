using System.Collections;
using UnityEngine;

public class StopLightStack : MonoBehaviour
{
	public LayerMask hitMask;

	public Light connectedLight;

	public float origIntensity = 1f;

	public float desiredIntensity = 1f;

	private void OnEnable()
	{
		if ((bool)RealWorldTimeLight.time)
		{
			RealWorldTimeLight.time.onLightPlaced.AddListener(checkForCloseLights);
			RealWorldTimeLight.time.onLightPlaced.Invoke();
		}
	}

	private void OnDisable()
	{
		if ((bool)RealWorldTimeLight.time)
		{
			RealWorldTimeLight.time.onLightPlaced.RemoveListener(checkForCloseLights);
		}
	}

	private void OnDestroy()
	{
		if ((bool)RealWorldTimeLight.time)
		{
			RealWorldTimeLight.time.onLightPlaced.RemoveListener(checkForCloseLights);
		}
	}

	public void checkForCloseLights()
	{
		StartCoroutine(checkForLights());
	}

	private IEnumerator checkForLights()
	{
		yield return new WaitForSeconds(Random.Range(0.01f, 0.2f));
		yield return null;
		Collider[] array = Physics.OverlapSphere(base.transform.position, connectedLight.range / 4f, hitMask);
		if (array.Length > 1)
		{
			connectedLight.renderMode = LightRenderMode.ForceVertex;
			desiredIntensity = Mathf.Clamp(origIntensity / ((float)array.Length / 1.5f), 0f, 1f);
			if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.currentHour != 17)
			{
				connectedLight.intensity = desiredIntensity;
			}
		}
		else
		{
			desiredIntensity = origIntensity;
			if (RealWorldTimeLight.time.underGround || RealWorldTimeLight.time.currentHour != 17)
			{
				connectedLight.intensity = origIntensity;
			}
		}
	}
}
