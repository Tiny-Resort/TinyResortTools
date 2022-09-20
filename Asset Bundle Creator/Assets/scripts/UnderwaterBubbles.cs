using System.Collections;
using UnityEngine;

public class UnderwaterBubbles : MonoBehaviour
{
	public Transform[] bubbles;

	private void OnEnable()
	{
		if (LicenceManager.manage.allLicences[3].getCurrentLevel() >= 2)
		{
			for (int i = 0; i < bubbles.Length; i++)
			{
				StartCoroutine(bubbleBehave(bubbles[i]));
			}
		}
		else
		{
			for (int j = 0; j < bubbles.Length; j++)
			{
				bubbles[j].gameObject.SetActive(false);
			}
		}
	}

	private IEnumerator bubbleBehave(Transform bubble)
	{
		yield return new WaitForSeconds(Random.Range(0.5f, 2f));
		Random.Range(5f, 8f);
		bubble.gameObject.SetActive(false);
		while (true)
		{
			bubble.transform.localPosition = Vector3.zero;
			Vector3 startingPos = bubble.transform.position;
			float speed = Random.Range(1f, 2.5f);
			bubble.gameObject.SetActive(true);
			for (float completetion2 = 0f; completetion2 < 1f; completetion2 += Time.deltaTime / speed)
			{
				yield return null;
				bubble.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, completetion2);
				bubble.position = Vector3.Lerp(startingPos, new Vector3(startingPos.x, 0.6f, startingPos.z), completetion2);
			}
			for (float completetion2 = 0f; completetion2 < 1f; completetion2 += Time.deltaTime * 5f)
			{
				yield return null;
				bubble.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, completetion2);
			}
			bubble.gameObject.SetActive(false);
			bubble.localScale = Vector3.zero;
			yield return new WaitForSeconds(Random.Range(3f, 8f));
		}
	}
}
