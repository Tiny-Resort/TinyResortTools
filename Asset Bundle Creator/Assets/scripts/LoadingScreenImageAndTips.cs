using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenImageAndTips : MonoBehaviour
{
	public Image picture;

	public Sprite[] randomImages;

	public Color fadeOutColour;

	public GameObject tipBox;

	public TextMeshProUGUI tipWords;

	public string[] tips;

	private void OnEnable()
	{
		picture.color = fadeOutColour;
		tipBox.SetActive(false);
		picture.rectTransform.anchoredPosition = Vector2.zero;
		picture.sprite = randomImages[Random.Range(0, randomImages.Length)];
		StartCoroutine(fadeInImage());
	}

	private IEnumerator fadeInImage()
	{
		yield return new WaitForSeconds(0.8f);
		float fadeTime = 0f;
		while (fadeTime < 0.5f)
		{
			picture.color = Color.Lerp(fadeOutColour, Color.white, fadeTime * 2f);
			fadeTime += Time.deltaTime;
			yield return null;
		}
		tipWords.text = tips[Random.Range(0, tips.Length)];
		tipBox.SetActive(true);
	}

	public void fadeAway()
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(fadeAwayFast());
		}
	}

	private IEnumerator fadeAwayFast()
	{
		tipBox.SetActive(false);
		float fadeTime = 0f;
		while (fadeTime < 0.1f)
		{
			picture.color = Color.Lerp(Color.white, fadeOutColour, fadeTime * 10f);
			fadeTime += Time.deltaTime;
			yield return null;
		}
	}
}
