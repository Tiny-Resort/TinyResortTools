using UnityEngine;

public class RandomManager : MonoBehaviour
{
	public static RandomManager manage;

	private bool locked;

	private void Awake()
	{
		manage = this;
	}
}
