using System.Collections;
using UnityEngine;

public class TrapActivate : MonoBehaviour
{
	public AnimalAI[] canCatch;

	private void OnTriggerEnter(Collider other)
	{
		AnimalAI componentInParent = other.GetComponentInParent<AnimalAI>();
		if (!componentInParent)
		{
			return;
		}
		AnimalAI[] array = canCatch;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].animalId == componentInParent.animalId)
			{
				NetworkMapSharer.share.ActivateTrap(componentInParent.netId, (int)base.transform.position.x / 2, (int)base.transform.position.z / 2);
				break;
			}
		}
	}

	private IEnumerator delayCaptureForClient(AnimalAI isAnimal)
	{
		yield return new WaitForSeconds(0.05f);
		bool flag = (bool)isAnimal;
	}
}
