using UnityEngine;

public class TurnOnObjectOnEnable : MonoBehaviour
{
	public GameObject toBeDisabledOrEnabled;

	private void OnDisable()
	{
		toBeDisabledOrEnabled.SetActive(false);
	}

	private void OnEnable()
	{
		toBeDisabledOrEnabled.SetActive(true);
	}
}
