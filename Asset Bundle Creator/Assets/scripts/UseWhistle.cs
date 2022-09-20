using UnityEngine;

public class UseWhistle : MonoBehaviour
{
	public ASound whistleSound;

	public int[] callAnimalId;

	public float whistleRadius = 10f;

	public LayerMask animalLayer;

	public ParticleSystem whistleParts;

	public bool isPetWhistle;

	public bool petsAllAnimalsThatHear;

	public void useWhistle()
	{
		if ((bool)whistleSound)
		{
			SoundManager.manage.playASoundAtPoint(whistleSound, base.transform.position);
		}
		if (!NetworkMapSharer.share.isServer)
		{
			return;
		}
		Invoke("shootWhisleParticles", 0.1f);
		if (!Physics.CheckSphere(base.transform.root.position + base.transform.root.forward * whistleRadius / 4f, whistleRadius, animalLayer))
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, whistleRadius, animalLayer);
		for (int i = 0; i < array.Length; i++)
		{
			AnimalAI componentInParent = array[i].GetComponentInParent<AnimalAI>();
			if (!componentInParent)
			{
				continue;
			}
			for (int j = 0; j < callAnimalId.Length; j++)
			{
				if (componentInParent.animalId != callAnimalId[j])
				{
					continue;
				}
				if (!componentInParent.isAPet())
				{
					StopCoroutine(componentInParent.callAnimalToPos(base.transform.root.position));
					StartCoroutine(componentInParent.callAnimalToPos(base.transform.root.position));
				}
				if (isPetWhistle && (bool)componentInParent.isAPet())
				{
					componentInParent.GetComponent<AnimalAI_Pet>().setNewFollowTo(base.transform.root.GetComponent<CharMovement>().netId);
				}
				if (petsAllAnimalsThatHear && Random.Range(0, 10) == 2)
				{
					FarmAnimal component = componentInParent.GetComponent<FarmAnimal>();
					if ((bool)component)
					{
						component.RpcPetAnimal();
					}
				}
			}
		}
	}

	private void shootWhisleParticles()
	{
		if ((bool)whistleParts)
		{
			whistleParts.Emit(15);
		}
	}
}
