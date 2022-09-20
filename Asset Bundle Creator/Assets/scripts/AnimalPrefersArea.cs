using System.Collections;
using UnityEngine;

public class AnimalPrefersArea : MonoBehaviour
{
	public bool prefersWater;

	public bool prefersSpawnPoint;

	public bool prefersSleepPos;

	public float strayDistance = 20f;

	private Vector3 spawnedAtPos;

	private Vector3 closestWater;

	private AnimalAI myAi;

	private AnimalAI_Sleep mySleep;

	private void Start()
	{
		myAi = GetComponent<AnimalAI>();
		mySleep = GetComponent<AnimalAI_Sleep>();
		spawnedAtPos = base.transform.position;
		closestWater = Vector3.zero;
	}

	public Vector3 returnClosestPreferedArea()
	{
		if (prefersWater && Vector3.Distance(base.transform.position, closestWater) > strayDistance)
		{
			Vector3 vector = WorldManager.manageWorld.findClosestWaterTile(base.transform.position, (int)strayDistance * 2, true);
			if (vector != Vector3.zero)
			{
				closestWater = vector;
			}
			return closestWater;
		}
		if (prefersSleepPos && myAi.currentlyAttacking() == null && Vector3.Distance(base.transform.position, mySleep.getSleepPos()) >= strayDistance)
		{
			return mySleep.getSleepPos();
		}
		if (prefersSpawnPoint && Vector3.Distance(base.transform.position, spawnedAtPos) > strayDistance)
		{
			return spawnedAtPos;
		}
		return base.transform.position;
	}

	public IEnumerator checkForPreferedArea()
	{
		if ((!mySleep || !mySleep.tryingToSleep()) && returnClosestPreferedArea() != base.transform.position)
		{
			if (myAi.myAgent.isActiveAndEnabled)
			{
				myAi.myAgent.SetDestination(returnClosestPreferedArea());
			}
			yield return myAi.checkTimer;
			yield return myAi.checkTimer;
			yield return myAi.checkTimer;
		}
	}
}
