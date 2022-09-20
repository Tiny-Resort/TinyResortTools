using System.Collections;
using UnityEngine;

public class followProjectile : MonoBehaviour
{
	public Transform toFollow;

	public ParticleSystem myPartSystem;

	private void Start()
	{
		base.transform.parent = null;
		StartCoroutine(followMyProjectile());
	}

	private IEnumerator followMyProjectile()
	{
		while ((bool)toFollow)
		{
			base.transform.position = toFollow.position;
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		if ((bool)myPartSystem)
		{
			myPartSystem.Stop();
			yield return new WaitForSeconds(1f);
		}
		Object.Destroy(base.gameObject);
	}
}
