using UnityEngine;

public class WildAnimalHomeAndSleep : MonoBehaviour
{
	private AnimalAI myAi;

	private void Start()
	{
		myAi = GetComponent<AnimalAI>();
	}
}
