using UnityEngine;

public class DeactivateOnAnimator : MonoBehaviour
{
	public void disableOnComplete()
	{
		base.gameObject.SetActive(false);
	}
}
