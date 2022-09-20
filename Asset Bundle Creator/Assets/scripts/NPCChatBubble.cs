using System.Collections;
using UnityEngine;

public class NPCChatBubble : MonoBehaviour
{
	public GameObject speechBubblePrefab;

	public GameObject myCurrentBubble;

	private Coroutine myRoutine;

	public void tryAndTalk(string text, float waitTime = 4f, bool overrideOldBubble = false)
	{
		if ((bool)myCurrentBubble && !overrideOldBubble)
		{
			return;
		}
		if (overrideOldBubble)
		{
			if (myRoutine != null)
			{
				StopCoroutine(myRoutine);
			}
			Object.Destroy(myCurrentBubble);
			myCurrentBubble = null;
		}
		myRoutine = StartCoroutine(saySomething(text, waitTime));
	}

	private IEnumerator saySomething(string text, float waitTime)
	{
		myCurrentBubble = Object.Instantiate(speechBubblePrefab, NotificationManager.manage.speechBubbleWindow);
		myCurrentBubble.GetComponent<SpeechBubble>().setUpBubble(text, base.transform);
		yield return new WaitForSeconds(waitTime);
		Object.Destroy(myCurrentBubble);
		myCurrentBubble = null;
		myRoutine = null;
	}

	private void OnDisable()
	{
		if ((bool)myCurrentBubble)
		{
			Object.Destroy(myCurrentBubble);
			myCurrentBubble = null;
		}
	}
}
