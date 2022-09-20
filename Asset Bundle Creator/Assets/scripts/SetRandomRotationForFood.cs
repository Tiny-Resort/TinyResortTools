using System;
using UnityEngine;

public class SetRandomRotationForFood : MonoBehaviour
{
	public Transform setToRandomRotation;

	public bool isFood = true;

	public bool isFlower;

	public void setRandomRotation()
	{
		setToRandomRotation.localRotation = Quaternion.Euler(0f, 0f, 90 * UnityEngine.Random.Range(0, 4));
	}

	public void setRandomRotationFlower()
	{
		UnityEngine.Random.InitState((int)(setToRandomRotation.position.x * setToRandomRotation.position.z + setToRandomRotation.position.x - setToRandomRotation.position.z));
		setToRandomRotation.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 1800f), 0f);
		UnityEngine.Random.InitState(Environment.TickCount);
	}

	public void OnEnable()
	{
		if (isFood)
		{
			setRandomRotation();
		}
		if (isFlower)
		{
			setRandomRotationFlower();
		}
	}
}
