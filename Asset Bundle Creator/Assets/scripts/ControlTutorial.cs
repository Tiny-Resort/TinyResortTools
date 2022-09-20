using System.Collections;
using UnityEngine;

public class ControlTutorial : MonoBehaviour
{
	public static ControlTutorial tutorial;

	public GameObject controlTutorial;

	public GameObject cameraTutorial;

	public GameObject freeCamTutorial;

	public GameObject bagTutorial;

	public GameObject journalTutorial;

	public bool tutorialStart;

	private void Awake()
	{
		tutorial = this;
	}

	public void startTutorial()
	{
		StartCoroutine(tutorialRoutine());
	}

	private void Update()
	{
		if (tutorialStart)
		{
			startTutorial();
			tutorialStart = false;
		}
	}

	private IEnumerator tutorialRoutine()
	{
		bool hasBagBeenOpened = false;
		while (ConversationManager.manage.inConversation)
		{
			yield return true;
		}
		yield return null;
		while (GiftedItemWindow.gifted.windowOpen)
		{
			yield return null;
		}
		controlTutorial.gameObject.SetActive(true);
		bool exitControls = false;
		yield return new WaitForSeconds(1f);
		while (!exitControls)
		{
			float x = InputMaster.input.getLeftStick().x;
			float y = InputMaster.input.getLeftStick().y;
			if (x != 0f || y != 0f)
			{
				exitControls = true;
			}
			yield return null;
			if (!hasBagBeenOpened && Inventory.inv.invOpen)
			{
				hasBagBeenOpened = true;
			}
		}
		controlTutorial.gameObject.SetActive(false);
		while (WeatherManager.manage.isInside())
		{
			yield return null;
		}
		while (!ConversationManager.manage.inConversation)
		{
			yield return true;
		}
		while (ConversationManager.manage.inConversation || GiftedItemWindow.gifted.windowOpen)
		{
			yield return true;
		}
		cameraTutorial.gameObject.SetActive(true);
		bool exitCameraTut = false;
		float moveCameraTimer = 0f;
		while (!exitCameraTut)
		{
			float x2 = InputMaster.input.getRightStick().x;
			float y2 = InputMaster.input.getRightStick().y;
			if (InputMaster.input.TriggerLookHeld() || x2 != 0f || y2 != 0f)
			{
				moveCameraTimer += Time.deltaTime;
				if (moveCameraTimer > 2f)
				{
					exitCameraTut = true;
				}
			}
			yield return null;
			if (needsToHide())
			{
				yield return StartCoroutine(hideTutorialBoxWhileInMenu(cameraTutorial));
			}
			if (!hasBagBeenOpened && Inventory.inv.invOpen)
			{
				hasBagBeenOpened = true;
			}
		}
		cameraTutorial.gameObject.SetActive(false);
		yield return new WaitForSeconds(1f);
		if (!hasBagBeenOpened)
		{
			bagTutorial.gameObject.SetActive(true);
			bool exitBagTutorial = false;
			while (!exitBagTutorial)
			{
				if (InputMaster.input.OpenInventory())
				{
					exitBagTutorial = true;
				}
				yield return null;
				if (needsToHide())
				{
					yield return StartCoroutine(hideTutorialBoxWhileInMenu(bagTutorial));
				}
			}
			bagTutorial.gameObject.SetActive(false);
		}
		while (!TownManager.manage.journalUnlocked)
		{
			yield return null;
		}
		yield return new WaitForSeconds(3f);
		journalTutorial.gameObject.SetActive(true);
		bool exitJournalTutorial = false;
		while (!exitJournalTutorial)
		{
			if (InputMaster.input.Journal())
			{
				exitJournalTutorial = true;
			}
			yield return null;
			if (needsToHide(false))
			{
				yield return StartCoroutine(hideTutorialBoxWhileInMenu(journalTutorial));
			}
		}
		journalTutorial.SetActive(false);
	}

	public bool needsToHide(bool includeSubMenu = true)
	{
		if (ConversationManager.manage.inConversation)
		{
			return true;
		}
		if (includeSubMenu && MenuButtonsTop.menu.subMenuOpen)
		{
			return true;
		}
		if (GiftedItemWindow.gifted.windowOpen)
		{
			return true;
		}
		return false;
	}

	private IEnumerator hideTutorialBoxWhileInMenu(GameObject boxToHide)
	{
		if (ConversationManager.manage.inConversation)
		{
			boxToHide.SetActive(false);
			while (ConversationManager.manage.inConversation)
			{
				yield return true;
			}
			boxToHide.SetActive(true);
		}
		if (MenuButtonsTop.menu.subMenuOpen)
		{
			boxToHide.SetActive(false);
			while (MenuButtonsTop.menu.subMenuOpen)
			{
				yield return true;
			}
			boxToHide.SetActive(true);
		}
		if (GiftedItemWindow.gifted.windowOpen)
		{
			boxToHide.SetActive(false);
			while (GiftedItemWindow.gifted.windowOpen)
			{
				yield return null;
			}
			boxToHide.SetActive(true);
		}
	}
}
