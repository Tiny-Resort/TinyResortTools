using System.Collections;
using UnityEngine;

public class FaceItemMoveWithMouth : MonoBehaviour
{
	public bool scaleOnly;

	private Coroutine running;

	public void openMouth()
	{
		if (running != null)
		{
			StopCoroutine(running);
		}
		running = StartCoroutine(moveMouthItem());
	}

	private IEnumerator moveMouthItem()
	{
		if (scaleOnly)
		{
			base.transform.localPosition = Vector3.zero;
			base.transform.localScale = Vector3.one;
			yield return null;
			yield return null;
			yield return null;
			base.transform.localScale = new Vector3(1f, 1f, 0.98f);
			yield return null;
			yield return null;
			yield return null;
			base.transform.localScale = new Vector3(1f, 1f, 0.96f);
			yield return null;
			yield return null;
			yield return null;
			base.transform.localScale = new Vector3(1f, 1f, 0.98f);
			yield return null;
			yield return null;
			yield return null;
		}
		else
		{
			base.transform.localPosition = Vector3.zero;
			base.transform.localScale = Vector3.one;
			yield return null;
			yield return null;
			yield return null;
			base.transform.localPosition = new Vector3(-0.03f, 0f, 0f);
			base.transform.localScale = new Vector3(1.03f, 1f, 0.9f);
			yield return null;
			yield return null;
			yield return null;
			base.transform.localPosition = new Vector3(-0.06f, 0f, 0f);
			base.transform.localScale = new Vector3(1.06f, 1f, 0.8f);
			yield return null;
			yield return null;
			yield return null;
			base.transform.localPosition = new Vector3(-0.03f, 0f, 0f);
			base.transform.localScale = new Vector3(1.03f, 1f, 0.9f);
			yield return null;
			yield return null;
			yield return null;
		}
		base.transform.localPosition = Vector3.zero;
		base.transform.localScale = Vector3.one;
		running = null;
	}
}
