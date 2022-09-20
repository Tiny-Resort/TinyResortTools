using System.Collections;
using UnityEngine;

public class TurnOffLightInside : MonoBehaviour
{
	private Light myLight;

	private WaitForSeconds wait;

	private void Start()
	{
		myLight = GetComponent<Light>();
		StartCoroutine(lightRoutine());
	}

	private IEnumerator lightRoutine()
	{
		while (true)
		{
			yield return null;
			if (base.transform.position.y <= -5f)
			{
				myLight.enabled = false;
				while (base.transform.position.y <= -5f)
				{
					yield return null;
				}
			}
			if (base.transform.position.y > -5f)
			{
				myLight.enabled = true;
				while (base.transform.position.y > -5f)
				{
					yield return null;
				}
			}
		}
	}
}
