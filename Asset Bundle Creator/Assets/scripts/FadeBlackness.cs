using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeBlackness : MonoBehaviour
{
	public Image blackness;

	public float fadeTime = 1f;

	public Color alpha;

	public TextMeshProUGUI dayText;

	public TextMeshProUGUI seasonText;

	public TextMeshProUGUI yearText;

	public void fadeOut()
	{
		StopCoroutine("FadeInBlack");
		StartCoroutine("FadeOutBlack");
	}

	public void fadeIn()
	{
		StopCoroutine("FadeOutBlack");
		StartCoroutine("FadeInBlack");
	}

	public IEnumerator fadeInDateText()
	{
		int num = WorldManager.manageWorld.day + (WorldManager.manageWorld.week - 1) * 7;
		yearText.text = "Year " + WorldManager.manageWorld.year;
		dayText.text = RealWorldTimeLight.time.getDayName(WorldManager.manageWorld.day - 1) + " " + num;
		seasonText.text = RealWorldTimeLight.time.getSeasonName(WorldManager.manageWorld.month - 1);
		yearText.color = Color.clear;
		seasonText.color = Color.clear;
		dayText.color = Color.clear;
		dayText.gameObject.SetActive(true);
		seasonText.gameObject.SetActive(true);
		yearText.gameObject.SetActive(true);
		float fade3 = 0f;
		while (fade3 < 1f)
		{
			yearText.color = Color.Lerp(Color.clear, Color.white, fade3);
			fade3 += Time.deltaTime * 1.5f;
			yield return null;
		}
		yield return null;
		fade3 = 0f;
		while (fade3 < 1f)
		{
			dayText.color = Color.Lerp(Color.clear, Color.white, fade3);
			fade3 += Time.deltaTime * 1.5f;
			yield return null;
		}
		fade3 = 0f;
		while (fade3 < 1f)
		{
			seasonText.color = Color.Lerp(Color.clear, Color.white, fade3);
			fade3 += Time.deltaTime * 1.5f;
			yield return null;
		}
	}

	public IEnumerator fadeOutDateText()
	{
		float fade = 0f;
		while (fade < 1f)
		{
			yearText.color = Color.Lerp(Color.white, Color.clear, fade);
			seasonText.color = Color.Lerp(Color.white, Color.clear, fade);
			dayText.color = Color.Lerp(Color.white, Color.clear, fade);
			fade += Time.deltaTime * 1.5f;
			yield return null;
		}
		dayText.gameObject.SetActive(false);
		seasonText.gameObject.SetActive(false);
		yearText.gameObject.SetActive(false);
	}

	private IEnumerator FadeOutBlack()
	{
		blackness.enabled = true;
		float timer = 0f;
		float percent = 0f;
		while (percent < 1f)
		{
			timer += Time.deltaTime;
			percent = timer / fadeTime;
			blackness.color = Color.Lerp(Color.black, alpha, percent);
			yield return null;
		}
		blackness.enabled = false;
	}

	private IEnumerator FadeInBlack()
	{
		blackness.enabled = true;
		float timer = 0f;
		float percent = 0f;
		while (percent < 1f)
		{
			timer += Time.deltaTime;
			percent = timer / fadeTime;
			blackness.color = Color.Lerp(alpha, Color.black, percent);
			yield return null;
		}
	}

	public void setBlack()
	{
		StopCoroutine("FadeOutBlack");
		StopCoroutine("FadeInBlack");
		blackness.enabled = true;
		blackness.color = Color.black;
	}
}
