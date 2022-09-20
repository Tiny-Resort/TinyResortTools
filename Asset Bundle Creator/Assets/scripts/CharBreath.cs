using System.Collections;
using UnityEngine;

public class CharBreath : MonoBehaviour
{
	public bool autoBreath;

	public Transform breathingPos;

	public static WaitForSeconds breathWait = new WaitForSeconds(3.45f);

	private Coroutine breathRoutine;

	private void Start()
	{
		if (autoBreath)
		{
			WorldManager.manageWorld.changeDayEvent.AddListener(startBreathOnColdDay);
		}
	}

	public void startBreathOnColdDay()
	{
		if (breathRoutine != null)
		{
			StopCoroutine(breathRoutine);
			breathRoutine = null;
		}
		if (RealWorldTimeLight.time.seasonAverageTemp <= 20)
		{
			breathRoutine = StartCoroutine(breathAuto());
		}
	}

	private IEnumerator breathAuto()
	{
		while (true)
		{
			yield return breathWait;
			if ((float)GenerateMap.generate.getPlaceTemperature(CameraController.control.transform.position) < 12f)
			{
				ParticleManager.manage.breathParticleAtPos(breathingPos);
			}
		}
	}

	public void takeBreathAnim()
	{
		ParticleManager.manage.breathParticleAtPos(breathingPos);
	}
}
