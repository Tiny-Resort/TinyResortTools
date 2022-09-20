using System.Collections;
using TMPro;
using UnityEngine;

public class PocketsFullNotification : MonoBehaviour
{
	private float onTimer;

	private Coroutine running;

	public Animator myAnim;

	public TextMeshProUGUI promptText;

	public void showNoLicence(LicenceManager.LicenceTypes type)
	{
		promptText.text = "Need Licence";
		turnOn(false);
	}

	public void showMustBeEmpty()
	{
		promptText.text = "Must be empty";
		turnOn(false);
	}

	public void showTooFull()
	{
		promptText.text = "Too full";
		turnOn(false);
	}

	public void showPocketsFull(bool isHolding)
	{
		promptText.text = "Pockets Full";
		turnOn(isHolding);
	}

	public void showCanPlaceText(string showText)
	{
		promptText.text = showText;
		turnOn(true);
	}

	public void hidePopUp()
	{
		onTimer = 0f;
	}

	private void turnOn(bool isHolding)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			isHolding = false;
		}
		if (!isHolding && base.gameObject.activeInHierarchy)
		{
			myAnim.SetTrigger("Bounce");
		}
		base.gameObject.SetActive(true);
		onTimer = 2f;
		if (!isHolding)
		{
			SoundManager.manage.play2DSound(SoundManager.manage.pocketsFull);
		}
		if (running == null)
		{
			running = StartCoroutine(runPocketsFull());
		}
	}

	private IEnumerator runPocketsFull()
	{
		while (onTimer > 0f)
		{
			yield return null;
			onTimer -= Time.deltaTime;
			if (!Inventory.inv.canMoveChar())
			{
				break;
			}
		}
		running = null;
		base.gameObject.SetActive(false);
	}
}
