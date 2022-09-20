using System.Collections;
using UnityEngine;

public class CurrencyWindows : MonoBehaviour
{
	public static CurrencyWindows currency;

	public Transform cornerPos;

	public Transform moneyInvPos;

	[Header("Actual Boxes")]
	public Transform walletBox;

	public Transform permitPointBox;

	public GameObject buffBox;

	private RectTransform walletRect;

	private RectTransform buffRect;

	[Header("Task Boxes")]
	public Transform journalTaskPos;

	public Transform sideTaskBarSmall;

	public Transform sideTaskBarLarge;

	private RectTransform cornerPosRect;

	private bool cornerPosOn;

	private bool journalOpen;

	private Coroutine runningPointsCoroutine;

	private Coroutine runningMoneyRoutine;

	private void Awake()
	{
		currency = this;
	}

	private void Start()
	{
		walletRect = walletBox.GetComponent<RectTransform>();
		cornerPosRect = cornerPos.GetComponent<RectTransform>();
		buffRect = buffBox.GetComponent<RectTransform>();
	}

	public void openInv()
	{
		if (GiveNPC.give.giveWindowOpen)
		{
			walletBox.gameObject.SetActive(true);
			buffBox.SetActive(false);
		}
		else
		{
			walletBox.SetParent(moneyInvPos);
			walletRect.anchoredPosition = Vector2.zero;
			walletBox.gameObject.SetActive(true);
			buffBox.SetActive(true);
		}
		moneyInvPos.gameObject.SetActive(false);
		Invoke("moveMoneyBoxDelay", 0.1f);
	}

	private void moveMoneyBoxDelay()
	{
		moneyInvPos.transform.position = new Vector2(Inventory.inv.invSlots[11].transform.position.x, 120f);
		moneyInvPos.transform.localPosition = new Vector3(moneyInvPos.transform.localPosition.x + 84f, 120f);
		moneyInvPos.gameObject.SetActive(true);
		buffBox.transform.position = new Vector2(Inventory.inv.invSlots[11].transform.position.x, 120f);
		buffBox.transform.localPosition = new Vector3(buffBox.transform.localPosition.x + 84f + 94f, 120f);
	}

	public void closeInv()
	{
		walletBox.SetParent(cornerPos);
		walletBox.SetSiblingIndex(0);
	}

	public void windowOn(bool enabled)
	{
		cornerPos.gameObject.SetActive(enabled);
		cornerPosOn = enabled;
	}

	public void openJournal()
	{
		if (MenuButtonsTop.menu.subMenuOpen || QuestTracker.track.trackerOpen || PhotoManager.manage.photoTabOpen || LicenceManager.manage.windowOpen)
		{
			walletBox.gameObject.SetActive(false);
		}
		for (int i = 0; i < DailyTaskGenerator.generate.taskIcons.Length; i++)
		{
			DailyTaskGenerator.generate.taskIcons[i].transform.SetParent(journalTaskPos);
			DailyTaskGenerator.generate.taskIcons[i].makeBig();
		}
		journalOpen = true;
		RenderMap.map.changeMapWindow();
		sideTaskBarSmall.gameObject.SetActive(false);
		permitPointBox.gameObject.SetActive(true);
	}

	public void closeJournal()
	{
		walletBox.gameObject.SetActive(true);
		for (int i = 0; i < DailyTaskGenerator.generate.taskIcons.Length; i++)
		{
			DailyTaskGenerator.generate.taskIcons[i].transform.SetParent(sideTaskBarSmall);
			DailyTaskGenerator.generate.taskIcons[i].makeSmall();
		}
		journalOpen = false;
		RenderMap.map.changeMapWindow();
		if (TownManager.manage.journalUnlocked)
		{
			sideTaskBarSmall.gameObject.SetActive(true);
		}
	}

	public void checkIfPointsNeeded()
	{
		if ((bool)cornerPosRect && runningPointsCoroutine == null)
		{
			runningPointsCoroutine = StartCoroutine(checkForPointChange());
		}
	}

	public void checkIfMoneyBoxNeeded()
	{
		if ((bool)cornerPosRect && runningMoneyRoutine == null)
		{
			runningMoneyRoutine = StartCoroutine(checkForWalletChange());
		}
	}

	private IEnumerator checkForPointChange()
	{
		float afterWaitTimer = 0f;
		walletBox.gameObject.SetActive(false);
		if (!PermitPointsManager.manage.isPointTotalShown())
		{
			while (afterWaitTimer <= 2f)
			{
				permitPointBox.gameObject.SetActive(true);
				if (RenderMap.map.mapCircle.gameObject.activeInHierarchy)
				{
					cornerPosRect.anchoredPosition = new Vector2(-165f, 0f);
				}
				else
				{
					cornerPosRect.anchoredPosition = new Vector2(0f, 0f);
				}
				cornerPos.gameObject.SetActive(true);
				afterWaitTimer = ((!PermitPointsManager.manage.isPointTotalShown()) ? 0f : (afterWaitTimer + Time.deltaTime));
				yield return null;
			}
		}
		cornerPosRect.anchoredPosition = new Vector2(0f, 0f);
		cornerPos.gameObject.SetActive(cornerPosOn);
		runningPointsCoroutine = null;
		if (!journalOpen)
		{
			walletBox.gameObject.SetActive(true);
		}
	}

	private IEnumerator checkForWalletChange()
	{
		float afterWaitTimer = 0f;
		permitPointBox.gameObject.SetActive(false);
		if (!Inventory.inv.isWalletTotalShown())
		{
			while (afterWaitTimer <= 2f)
			{
				if (RenderMap.map.mapCircle.gameObject.activeInHierarchy)
				{
					cornerPosRect.anchoredPosition = new Vector2(-165f, 0f);
				}
				else
				{
					cornerPosRect.anchoredPosition = new Vector2(0f, 0f);
				}
				afterWaitTimer = ((!Inventory.inv.isWalletTotalShown()) ? 0f : (afterWaitTimer + Time.deltaTime));
				cornerPos.gameObject.SetActive(true);
				if (!Inventory.inv.invOpen)
				{
					walletBox.gameObject.SetActive(true);
					walletBox.SetParent(cornerPos);
					walletBox.SetSiblingIndex(0);
				}
				yield return null;
			}
		}
		cornerPosRect.anchoredPosition = new Vector2(0f, 0f);
		cornerPos.gameObject.SetActive(cornerPosOn);
		permitPointBox.gameObject.SetActive(true);
		runningMoneyRoutine = null;
	}
}
