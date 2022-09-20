using UnityEngine;

public class DisableOnEnabled : MonoBehaviour
{
	private void OnEnable()
	{
		base.gameObject.SetActive(false);
	}
}
