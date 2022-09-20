using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FishingLineHealthBar : MonoBehaviour
{
	public static FishingLineHealthBar bar;

	public RectTransform myTrans;

	public GameObject toHide;

	public Image healthBar;

	public Color fullHealthColour;

	public Color lowHealthColour;

	public AudioSource mySound;

	private float lastShowingHealth = 1f;

	private float hideTimer;

	private Vector2 shakePos = Vector2.zero;

	private void Awake()
	{
		bar = this;
	}

	public void showHealthbar()
	{
		StartCoroutine(barWorks());
	}

	public IEnumerator barWorks()
	{
		while (NetworkMapSharer.share.localChar.myPickUp.netRod.lineIsCasted && NetworkMapSharer.share.localChar.myPickUp.netRod.fishOnLine == -1)
		{
			yield return null;
		}
		if (NetworkMapSharer.share.localChar.myPickUp.netRod.lineIsCasted && NetworkMapSharer.share.localChar.myPickUp.netRod.fishOnLine != -1)
		{
			lastShowingHealth = 1f;
			while (NetworkMapSharer.share.localChar.myPickUp.netRod.lineIsCasted)
			{
				yield return null;
				healthBar.fillAmount = NetworkMapSharer.share.localChar.myPickUp.netRod.currentLineHealth / (float)NetworkMapSharer.share.localChar.myPickUp.netRod.fullLineHealth;
				if (healthBar.fillAmount == lastShowingHealth)
				{
					if (hideTimer <= 0f)
					{
						toHide.SetActive(false);
					}
					else
					{
						hideTimer -= Time.deltaTime;
					}
				}
				else
				{
					toHide.SetActive(true);
					lastShowingHealth = healthBar.fillAmount;
					hideTimer = 2f;
				}
				healthBar.color = Color.Lerp(fullHealthColour, lowHealthColour, 1f - healthBar.fillAmount);
				shakePos = Vector2.Lerp(shakePos, Vector2.zero, Time.deltaTime * 2f);
				myTrans.anchoredPosition = new Vector2(-35f, 0f) + shakePos;
			}
		}
		stopSound();
		toHide.SetActive(false);
	}

	public void shakeBar()
	{
		shakePos = new Vector3(Random.Range(1f, -1f), Random.Range(1f, -1f));
	}

	public void playSound()
	{
		if (!mySound.isPlaying)
		{
			mySound.pitch = 8f;
			mySound.volume = 0.25f * SoundManager.manage.getUiVolume();
			mySound.Play();
		}
	}

	public void stopSound()
	{
		mySound.Pause();
	}
}
