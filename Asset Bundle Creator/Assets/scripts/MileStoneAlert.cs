using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MileStoneAlert : MonoBehaviour
{
	public Image image;

	public Sprite[] animations;

	private void OnEnable()
	{
		StartCoroutine(animate());
	}

	private IEnumerator changeScale()
	{
		bool small = false;
		while (true)
		{
			yield return null;
			if (small)
			{
			}
		}
	}

	private IEnumerator animate()
	{
		while (true)
		{
			image.sprite = animations[0];
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			image.sprite = animations[1];
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			image.sprite = animations[2];
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			image.sprite = animations[3];
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			image.sprite = animations[2];
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			image.sprite = animations[1];
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			image.sprite = animations[0];
		}
	}
}
