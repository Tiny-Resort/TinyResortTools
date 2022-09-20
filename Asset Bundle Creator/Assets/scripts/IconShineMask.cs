using System.Collections;
using UnityEngine;

public class IconShineMask : MonoBehaviour
{
	public RectTransform milestoneShine;

	public float boxSize = 100f;

	public float speed = 1.5f;

	public float waitTime = 2f;

	private void OnEnable()
	{
		StartCoroutine(iconDance());
	}

	private IEnumerator iconDance()
	{
		while (true)
		{
			float shineTimer = 0f;
			while (shineTimer / speed < 1f)
			{
				yield return null;
				shineTimer += Time.deltaTime;
				milestoneShine.anchoredPosition = Vector2.Lerp(new Vector2(0f, 0f), new Vector2(boxSize, 0f - boxSize), shineTimer / speed);
			}
			yield return new WaitForSeconds(waitTime);
		}
	}
}
