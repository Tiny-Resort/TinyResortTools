using System.Collections;
using TMPro;
using UnityEngine;

public class TopNotification : MonoBehaviour
{
	public TextMeshProUGUI notificationText;

	public TextMeshProUGUI littleText;

	public RectTransform myTrans;

	public void setText(string text, string subText)
	{
		notificationText.text = text;
		if (subText == "")
		{
			littleText.gameObject.SetActive(false);
			notificationText.maxVisibleCharacters = 0;
		}
		else
		{
			littleText.gameObject.SetActive(true);
			littleText.text = subText;
			littleText.maxVisibleCharacters = 0;
		}
	}

	public void startShowText()
	{
		StartCoroutine(lettersAppear());
	}

	private IEnumerator lettersAppear()
	{
		yield return new WaitForSeconds(0.1f);
		notificationText.maxVisibleCharacters = 0;
		for (int j = 0; j < notificationText.text.Length + 1; j++)
		{
			notificationText.maxVisibleCharacters = j;
			yield return null;
		}
		if (littleText.gameObject.activeInHierarchy)
		{
			littleText.maxVisibleCharacters = 0;
			for (int j = 0; j < littleText.text.Length + 1; j++)
			{
				littleText.maxVisibleCharacters = j;
				yield return null;
			}
		}
	}
}
