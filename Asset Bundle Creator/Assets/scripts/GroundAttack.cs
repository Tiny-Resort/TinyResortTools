using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundAttack : MonoBehaviour
{
	private List<Damageable> ringCentre = new List<Damageable>();

	public LayerMask damageLayer;

	public AnimalAI attachedToAnimal;

	public ParticleSystem myPart;

	private ParticleSystem.ShapeModule myPartShape;

	public float startSize = 0.5f;

	public ASound soundEffect;

	[Header("Stay In Position AOE")]
	public bool isDelayed;

	public float remainingTime = 4f;

	public int delayedDamageAmount = 2;

	public void OnEnable()
	{
		myPartShape = myPart.shape;
		myPartShape.radius = startSize;
		if (isDelayed)
		{
			StartCoroutine(growAndStay());
		}
		else
		{
			StartCoroutine(growInSize());
		}
	}

	private IEnumerator growAndStay()
	{
		yield return null;
		float timer = 5f;
		float currentSize = startSize;
		SoundManager.manage.playASoundAtPoint(soundEffect, base.transform.position);
		while (timer > 0f)
		{
			myPart.Emit(Mathf.RoundToInt(100f * currentSize));
			timer -= Time.deltaTime * 6.5f;
			currentSize += Time.deltaTime * 6.5f;
			yield return null;
			myPartShape.radius = currentSize;
			checkForCollisionsSphere(currentSize);
		}
		float delayTimer = remainingTime;
		while (delayTimer > 0f)
		{
			yield return null;
			delayTimer -= Time.deltaTime;
			checkForCollisionsSphere(currentSize);
			myPart.Emit(Mathf.RoundToInt(100f * currentSize));
		}
		yield return new WaitForSeconds(3f);
		Object.Destroy(base.gameObject);
	}

	private IEnumerator growInSize()
	{
		yield return null;
		CameraController.control.myShake.addToTraumaMax(0.35f, 0.5f);
		float timer = 8.5f;
		float currentSize = startSize;
		SoundManager.manage.playASoundAtPoint(soundEffect, base.transform.position);
		while (timer > 0f)
		{
			myPart.Emit(Mathf.RoundToInt(100f * currentSize));
			timer -= Time.deltaTime * 6.5f;
			currentSize += Time.deltaTime * 6.5f;
			yield return null;
			myPartShape.radius = currentSize;
			checkForCollisionsRing(currentSize);
		}
		yield return new WaitForSeconds(3f);
		Object.Destroy(base.gameObject);
	}

	public void checkForCollisionsSphere(float radius)
	{
		if (!NetworkMapSharer.share.isServer)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius, damageLayer);
		for (int i = 0; i < array.Length; i++)
		{
			Damageable componentInParent = array[i].GetComponentInParent<Damageable>();
			if ((bool)componentInParent && !ringCentre.Contains(componentInParent) && (!componentInParent.isAnAnimal() || ((bool)componentInParent.isAnAnimal() && componentInParent.isAnAnimal().animalId != attachedToAnimal.animalId)))
			{
				componentInParent.attackAndDoDamage(delayedDamageAmount, attachedToAnimal.transform, 0f);
			}
		}
	}

	public void checkForCollisionsRing(float radius)
	{
		if (!NetworkMapSharer.share.isServer)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius - 0.75f, damageLayer);
		ringCentre.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			ringCentre.Add(array[i].GetComponentInParent<Damageable>());
		}
		Collider[] array2 = Physics.OverlapSphere(base.transform.position, radius, damageLayer);
		for (int j = 0; j < array2.Length; j++)
		{
			if (Mathf.Abs(array2[j].transform.root.position.y - base.transform.position.y) < 0.45f)
			{
				Damageable componentInParent = array2[j].GetComponentInParent<Damageable>();
				if ((bool)componentInParent && !ringCentre.Contains(componentInParent) && (!componentInParent.isAnAnimal() || ((bool)componentInParent.isAnAnimal() && componentInParent.isAnAnimal().animalId != attachedToAnimal.animalId)))
				{
					componentInParent.attackAndDoDamage(10, attachedToAnimal.transform, 4f);
				}
			}
		}
	}
}
