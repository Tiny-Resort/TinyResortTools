using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBubble : MonoBehaviour
{
	public TextMeshProUGUI sentBy;

	public TextMeshProUGUI contents;

	public RectTransform myRect;

	public Image backFade;

	public Color backFadeFrom;

	public Color backFadeTo;

	private float lifeTimer;

	private IEnumerator killSelf()
	{
		for (lifeTimer = 25f; lifeTimer > 0f; lifeTimer -= Time.deltaTime * ChatBox.chat.chatSpeed)
		{
			yield return null;
		}
		yield return StartCoroutine(fadeBackgroundAndText(false));
		ChatBox.chat.chatLog.Remove(this);
		Object.Destroy(base.gameObject);
	}

	public void fillBubble(string name, string message)
	{
		contents.text = "<color=orange><" + name + "> </color>" + message;
		StartCoroutine(killSelf());
		StartCoroutine(fadeBackgroundAndText(true));
	}

	public float getHeight()
	{
		return myRect.sizeDelta.y;
	}

	private IEnumerator fadeBackgroundAndText(bool fadeIn)
	{
		float timer = 0f;
		while (timer < 1f)
		{
			if (fadeIn)
			{
				contents.fontMaterial.SetColor("_FaceColor", Color.Lerp(Color.clear, Color.white, timer));
				backFade.color = Color.Lerp(backFadeFrom, backFadeTo, timer);
			}
			else
			{
				contents.fontMaterial.SetColor("_FaceColor", Color.Lerp(Color.white, Color.clear, timer));
				backFade.color = Color.Lerp(backFadeTo, backFadeFrom, timer);
			}
			timer += Time.deltaTime * 2f;
			yield return null;
		}
	}
}
