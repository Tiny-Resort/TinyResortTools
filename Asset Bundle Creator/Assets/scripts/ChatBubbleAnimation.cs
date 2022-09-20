using System.Collections;
using UnityEngine;

public class ChatBubbleAnimation : MonoBehaviour
{
	private void OnEnable()
	{
		StartCoroutine(animateBubble());
	}

	private IEnumerator animateBubble()
	{
		float scaleMax = 1.1f;
		float scaleMin = 0.85f;
		float scale = Random.Range(scaleMin, scaleMax);
		bool shrinking = scale > 1f;
		float wobbleSpeed = Random.Range(0.001f, 0.005f);
		while (true)
		{
			yield return true;
			base.transform.localScale = new Vector3(scale, scale, scale);
			if (shrinking)
			{
				scale -= wobbleSpeed;
				if (scale <= scaleMin)
				{
					wobbleSpeed = Random.Range(0.001f, 0.005f);
					shrinking = false;
				}
			}
			else
			{
				scale += wobbleSpeed;
				if (scale >= scaleMax)
				{
					wobbleSpeed = Random.Range(0.001f, 0.005f);
					shrinking = true;
				}
			}
		}
	}
}
