using System;
using System.Collections;
using UnityEngine;

public class EruptionAttack : MonoBehaviour
{
	public GameObject erruptionPrefab;

	private AnimalAI shotBy;

	public ASound eruptionSound;

	public ASound slamSound;

	public void fireFromAnimal(AnimalAI shotByAnimal)
	{
		shotBy = shotByAnimal;
		StartCoroutine(circleDelay());
		StartCoroutine(playSoundAndShakeScreen());
	}

	private IEnumerator circleDelay()
	{
		int circleAmount = 8;
		for (int y = 0; (float)y < 4f; y++)
		{
			for (int i = 0; i < circleAmount; i++)
			{
				Vector3 newPos = RandomCircle((float)y * 3f + 3f, i, circleAmount);
				EruptionAttackSpike component = UnityEngine.Object.Instantiate(erruptionPrefab, base.transform).GetComponent<EruptionAttackSpike>();
				component.setShotByAnimal(shotBy);
				StartCoroutine(component.popUpAtPos(newPos));
			}
			yield return new WaitForSeconds(0.5f);
		}
		yield return new WaitForSeconds(3f);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private Vector3 RandomCircle(float radius, int id, int circleAmount)
	{
		float num = (float)id * (360f / (float)circleAmount);
		return new Vector3(radius * Mathf.Sin(num * ((float)Math.PI / 180f)), 0f, radius * Mathf.Cos(num * ((float)Math.PI / 180f)));
	}

	private IEnumerator playSoundAndShakeScreen()
	{
		float timer = 1.5f;
		SoundManager.manage.playASoundAtPoint(slamSound, base.transform.position);
		SoundManager.manage.playASoundAtPoint(eruptionSound, base.transform.position);
		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			CameraController.control.shakeScreenMax(timer / 10f, 0.15f);
			yield return null;
		}
	}
}
