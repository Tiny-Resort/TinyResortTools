using System.Collections;
using TMPro;
using UnityEngine;

public class PermitPointsManager : MonoBehaviour
{
	public static PermitPointsManager manage;

	public TextMeshProUGUI permitPointsText;

	private int permitPoints;

	private float showingPoints;

	private Coroutine fillingPoints;

	public AudioSource myAudio;

	private float velocity;

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
		updateText();
	}

	public void addPoints(int add)
	{
		permitPoints += add;
		updateText();
	}

	public void spendPoints(int spend)
	{
		permitPoints -= spend;
		updateText();
	}

	public bool checkIfCanAfford(int amount)
	{
		if (amount <= permitPoints)
		{
			return true;
		}
		return false;
	}

	public void updateText()
	{
		CurrencyWindows.currency.checkIfPointsNeeded();
		if (fillingPoints == null)
		{
			fillingPoints = StartCoroutine(fillPoints());
		}
	}

	public int getCurrentPoints()
	{
		return permitPoints;
	}

	public void loadFromSave(int loadPoints)
	{
		permitPoints = loadPoints;
		showingPoints = permitPoints;
		updateText();
	}

	public bool isPointTotalShown()
	{
		return Mathf.RoundToInt(showingPoints) == permitPoints;
	}

	private IEnumerator fillPoints()
	{
		while (Mathf.Abs(Mathf.RoundToInt(showingPoints) - permitPoints) > 2)
		{
			showingPoints = Mathf.SmoothDamp(showingPoints, permitPoints, ref velocity, 0.45f);
			permitPointsText.text = ((int)showingPoints).ToString("n0");
			if (!myAudio.isPlaying)
			{
				myAudio.pitch = Random.Range(4f, 6f);
				myAudio.volume = 0.4f * SoundManager.manage.getUiVolume();
				myAudio.Play();
			}
			yield return null;
		}
		showingPoints = permitPoints;
		permitPointsText.text = permitPoints.ToString("n0");
		fillingPoints = null;
	}
}
