using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
	public enum toolTipType
	{
		None = 0,
		multiTiledPlacing = 1,
		singleTiledPlacing = 2,
		PickUp = 3,
		Dive = 4,
		StopDriving = 5,
		GetUp = 6,
		CarryingItem = 7,
		CarryingAnimal = 8,
		Fishing = 9,
		InChest = 10,
		InGiveMenu = 11,
		InChestWhileHoldingItem = 12
	}

	public static NotificationManager manage;

	public GameObject itemNotificationPrefab;

	public GameObject damageNotificationPrefab;

	public GameObject chatBubblePrefab;

	public PickUpNotification buttonPromptNotification;

	public RectTransform buttonPromptTransform;

	public Transform notificationWindow;

	public Transform topNotificationWindow;

	public TopNotification topNotification;

	public List<PickUpNotification> notifications;

	private RectTransform canvasTrans;

	private float notificationTimer;

	private bool floatUp = true;

	private float floatDif;

	public GameObject hintWindow;

	public PickUpNotification xButtonHint;

	public PickUpNotification yButtonHint;

	public PickUpNotification bButtonHint;

	public PickUpNotification aButtonHint;

	public PickUpNotification splitStackHint;

	public PickUpNotification quickMoveStackHint;

	public PocketsFullNotification pocketsFull;

	public Transform speechBubbleWindow;

	private float sideNotificationYdif = -35f;

	private toolTipType showingTip;

	private bool showingUsingMouse = true;

	private Coroutine topNotificationRunning;

	private List<string> toNotify = new List<string>();

	private List<string> subTextNot = new List<string>();

	private List<ASound> soundToPlay = new List<ASound>();

	private void Awake()
	{
		manage = this;
		canvasTrans = GetComponent<RectTransform>();
		buttonPromptNotification.gameObject.SetActive(false);
	}

	private IEnumerator moveNotifications()
	{
		yield return null;
		while (notifications.Count > 0)
		{
			for (int i = 0; i < notifications.Count; i++)
			{
				float value = Mathf.Lerp(notifications[i].transform.localPosition.x, 0f, Time.deltaTime * 25f);
				notifications[i].transform.localPosition = new Vector3(Mathf.Clamp(value, 0f, 100f), (float)(i * -35) + sideNotificationYdif, 0f);
			}
			float num = 3f;
			if (notifications.Count > 8)
			{
				num = 0.05f;
			}
			else if (notifications.Count > 6)
			{
				num = 0.2f;
			}
			else if (notifications.Count > 5)
			{
				num = 0.5f;
			}
			else if (notifications.Count > 4)
			{
				num = 1f;
			}
			else if (notifications.Count > 3)
			{
				num = 1.5f;
			}
			sideNotificationYdif = Mathf.Lerp(sideNotificationYdif, 0f, Time.deltaTime * 10f);
			if (notificationTimer < num)
			{
				notificationTimer += Time.deltaTime;
			}
			else
			{
				Mathf.Lerp(notifications[0].transform.localPosition.x, 150f, Time.deltaTime * 25f);
				while (notifications[0].transform.localPosition.x < 100f)
				{
					yield return null;
					float x = Mathf.Lerp(notifications[0].transform.localPosition.x, 150f, Time.deltaTime * 25f);
					notifications[0].transform.localPosition = new Vector3(x, 0f + sideNotificationYdif, 0f);
				}
				notificationTimer = 0f;
				Object.Destroy(notifications[0].gameObject);
				notifications.Remove(notifications[0]);
				sideNotificationYdif = -35f;
			}
			yield return null;
		}
	}

	public void createDamageNotification(int damgeToShow, Transform connectToTrans)
	{
		Object.Instantiate(damageNotificationPrefab, connectToTrans.position + new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(0.15f, 0.25f), Random.Range(-0.15f, 0.15f)), Quaternion.identity).GetComponent<DamageNumberPop>().setDamageText(damgeToShow);
	}

	public void createChatBubble(string message, Transform connectedTransform)
	{
		Object.Instantiate(chatBubblePrefab, CameraController.control.mainCamera.WorldToScreenPoint(connectedTransform.position), Quaternion.identity, base.transform).GetComponent<SpeechBubble>();
	}

	public void createPickUpNotification(int inventoryId, int stackAmount = 1)
	{
		notificationTimer = 0f;
		bool flag = false;
		if (Inventory.inv.allItems[inventoryId].checkIfStackable())
		{
			for (int i = 0; i < notifications.Count; i++)
			{
				if (notifications[i].showingItemId == inventoryId)
				{
					notifications[i].fillItemDetails(notifications[i].showingItemId, notifications[i].showingStack + stackAmount);
					notifications[i].GetComponent<WindowAnimator>().refreshAnimation();
					PickUpNotification item = notifications[i];
					notifications.RemoveAt(i);
					notifications.Add(item);
					flag = true;
					break;
				}
			}
		}
		if (notifications.Count == 0)
		{
			sideNotificationYdif = -35f;
			StartCoroutine(moveNotifications());
		}
		if (!flag)
		{
			PickUpNotification component = Object.Instantiate(itemNotificationPrefab, notificationWindow).GetComponent<PickUpNotification>();
			component.transform.localPosition = new Vector3(100f, notifications.Count * -35 - 25, 0f);
			component.fillItemDetails(inventoryId, stackAmount);
			notifications.Add(component);
			if (notifications.Count < 2)
			{
				notificationTimer = 0f;
			}
		}
	}

	public void turnOnPocketsFullNotification(bool holdingButton = false)
	{
		pocketsFull.showPocketsFull(holdingButton);
	}

	public void createChatNotification(string chatText, bool specialTip = false)
	{
		if (specialTip)
		{
			List<PickUpNotification> list = new List<PickUpNotification>();
			for (int i = 0; i < notifications.Count; i++)
			{
				if (notifications[i].itemText.text == chatText)
				{
					list.Add(notifications[i]);
				}
			}
			foreach (PickUpNotification item in list)
			{
				notifications.Remove(item);
				sideNotificationYdif = -35f;
				Object.Destroy(item.gameObject);
			}
		}
		if (notifications.Count == 0)
		{
			sideNotificationYdif = -35f;
			StartCoroutine(moveNotifications());
		}
		PickUpNotification component = Object.Instantiate(itemNotificationPrefab, notificationWindow).GetComponent<PickUpNotification>();
		component.transform.localPosition = new Vector3(100f, notifications.Count * -35 - 25, 0f);
		component.fillChatDetails(chatText);
		notifications.Add(component);
		if (notifications.Count < 5)
		{
			notificationTimer = -3f;
		}
	}

	public void showButtonPrompt(string promptText, string buttonName, Vector3 buttonPromptPosition)
	{
		if (Inventory.inv.usingMouse)
		{
			switch (buttonName)
			{
			case "Z":
				buttonPromptNotification.fillButtonPrompt(promptText, buttonPromptNotification.controllerZ);
				break;
			case "B":
				buttonPromptNotification.fillButtonPrompt(promptText, buttonPromptNotification.controllerRightClick);
				break;
			case "X":
				buttonPromptNotification.fillButtonPrompt(promptText, buttonPromptNotification.controllerLeftClick);
				break;
			case "no":
				buttonPromptNotification.fillButtonPrompt(promptText, buttonPromptNotification.cantSprite);
				break;
			default:
				buttonPromptNotification.fillButtonPrompt(promptText, null);
				break;
			}
		}
		else
		{
			switch (buttonName)
			{
			case "Z":
				buttonPromptNotification.fillButtonPrompt(promptText, buttonPromptNotification.controllerRightStick);
				break;
			case "B":
				buttonPromptNotification.fillButtonPrompt(promptText, buttonPromptNotification.controllerB);
				break;
			case "X":
				buttonPromptNotification.fillButtonPrompt(promptText, buttonPromptNotification.controllerX);
				break;
			case "no":
				buttonPromptNotification.fillButtonPrompt(promptText, buttonPromptNotification.cantSprite);
				break;
			default:
				buttonPromptNotification.fillButtonPrompt(promptText, null);
				break;
			}
		}
		buttonPromptNotification.gameObject.SetActive(true);
		Vector3 vector = CameraController.control.mainCamera.WorldToViewportPoint(buttonPromptPosition + Vector3.up * 2f + Vector3.up * floatDif);
		if (floatUp)
		{
			if (floatDif > 0.15f)
			{
				floatUp = false;
			}
			floatDif += 0.005f;
		}
		else
		{
			if (floatDif < -0.15f)
			{
				floatUp = true;
			}
			floatDif -= 0.005f;
		}
		buttonPromptTransform.anchoredPosition = new Vector2(vector.x * canvasTrans.sizeDelta.x - canvasTrans.sizeDelta.x * 0.5f, vector.y * canvasTrans.sizeDelta.y - canvasTrans.sizeDelta.y * 0.5f);
		buttonPromptTransform.anchoredPosition += new Vector2(buttonPromptTransform.sizeDelta.x * 4f + 50f, 0f);
	}

	public void hideButtonPrompt()
	{
		if (buttonPromptNotification.gameObject.activeSelf)
		{
			buttonPromptNotification.gameObject.SetActive(false);
		}
	}

	public void hitWindowOpen(bool isOpen)
	{
		if (hintWindow.gameObject.activeInHierarchy != isOpen)
		{
			hintWindow.gameObject.SetActive(isOpen);
		}
	}

	public void hintWindowOpen(toolTipType toolTip)
	{
		if (toolTip == showingTip && showingUsingMouse == Inventory.inv.usingMouse)
		{
			return;
		}
		showingTip = toolTip;
		showingUsingMouse = Inventory.inv.usingMouse;
		if (showingTip == toolTipType.None)
		{
			hintWindow.gameObject.SetActive(false);
			return;
		}
		hintWindow.gameObject.SetActive(true);
		if (showingTip == toolTipType.InChest || showingTip == toolTipType.InGiveMenu || showingTip == toolTipType.InChestWhileHoldingItem)
		{
			toolTipMenu(showingTip);
		}
		else if (showingTip == toolTipType.PickUp)
		{
			toolTipHintBox("null", "Pick Up", "null", "null");
		}
		else if (showingTip == toolTipType.Dive)
		{
			toolTipHintBox("null", "Dive", "null", "null");
		}
		else if (showingTip == toolTipType.Fishing)
		{
			toolTipHintBox("Reel In", "null", (LocalizedString)"ToolTips/Tip_Cancel", "null");
		}
		else if (showingTip == toolTipType.CarryingAnimal)
		{
			toolTipHintBox("Release", "null", "null", (LocalizedString)"ToolTips/Tip_Drop");
		}
		else if (showingTip == toolTipType.CarryingItem)
		{
			toolTipHintBox("null", "null", "null", (LocalizedString)"ToolTips/Tip_Drop");
		}
		else if (showingTip == toolTipType.multiTiledPlacing)
		{
			toolTipHintBox((LocalizedString)"ToolTips/Tip_PlaceDeed", (LocalizedString)"ToolTips/Tip_Rotate", (LocalizedString)"ToolTips/Tip_Cancel", "null");
		}
		else if (showingTip == toolTipType.singleTiledPlacing)
		{
			toolTipHintBox("null", (LocalizedString)"ToolTips/Tip_Rotate", "null", "null");
		}
		else if (showingTip == toolTipType.StopDriving)
		{
			toolTipHintBox("null", (LocalizedString)"ToolTips/Tip_StopDriving", "null", "null");
		}
		else if (showingTip == toolTipType.GetUp)
		{
			toolTipHintBox("null", (LocalizedString)"ToolTips/Tip_GetUp", "null", "null");
		}
	}

	public void toolTipMenu(toolTipType menuType)
	{
		switch (menuType)
		{
		case toolTipType.InGiveMenu:
			if (Inventory.inv.usingMouse)
			{
				splitStackHint.fillButtonPrompt("Select Amount", splitStackHint.controllerRightClick);
			}
			else
			{
				splitStackHint.fillButtonPrompt("Select Amount", splitStackHint.controllerX);
			}
			splitStackHint.gameObject.SetActive(true);
			quickMoveStackHint.gameObject.SetActive(false);
			break;
		case toolTipType.InChest:
			if (Inventory.inv.usingMouse)
			{
				splitStackHint.fillButtonPrompt("Split Stack", splitStackHint.controllerRightClick);
			}
			else
			{
				splitStackHint.fillButtonPrompt("Split Stack", splitStackHint.controllerX);
			}
			splitStackHint.gameObject.SetActive(true);
			if (Inventory.inv.usingMouse)
			{
				quickMoveStackHint.fillButtonPrompt("Quick Move", quickMoveStackHint.controllerE);
			}
			else
			{
				quickMoveStackHint.fillButtonPrompt("Quick Move", quickMoveStackHint.controllerY);
			}
			quickMoveStackHint.gameObject.SetActive(true);
			break;
		case toolTipType.InChestWhileHoldingItem:
			if (Inventory.inv.usingMouse)
			{
				splitStackHint.fillButtonPrompt("Place One", splitStackHint.controllerRightClick);
			}
			else
			{
				splitStackHint.fillButtonPrompt("Place One", splitStackHint.controllerX);
			}
			splitStackHint.gameObject.SetActive(true);
			quickMoveStackHint.gameObject.SetActive(false);
			break;
		default:
			quickMoveStackHint.gameObject.SetActive(false);
			splitStackHint.gameObject.SetActive(false);
			break;
		}
		xButtonHint.gameObject.SetActive(false);
		yButtonHint.gameObject.SetActive(false);
		bButtonHint.gameObject.SetActive(false);
		aButtonHint.gameObject.SetActive(false);
	}

	public void toolTipHintBox(string xHintText, string yHintText, string bHintText, string aButtonText)
	{
		if (xHintText != "null")
		{
			xButtonHint.gameObject.SetActive(true);
			if (Inventory.inv.usingMouse)
			{
				xButtonHint.fillButtonPrompt(xHintText, xButtonHint.controllerLeftClick);
			}
			else
			{
				xButtonHint.fillButtonPrompt(xHintText, xButtonHint.controllerX);
			}
		}
		else
		{
			xButtonHint.gameObject.SetActive(false);
		}
		if (bHintText != "null")
		{
			bButtonHint.gameObject.SetActive(true);
			if (Inventory.inv.usingMouse)
			{
				bButtonHint.fillButtonPrompt(bHintText, bButtonHint.controllerRightClick);
			}
			else
			{
				bButtonHint.fillButtonPrompt(bHintText, bButtonHint.controllerB);
			}
		}
		else
		{
			bButtonHint.gameObject.SetActive(false);
		}
		if (yHintText != "null")
		{
			yButtonHint.gameObject.SetActive(true);
			if (Inventory.inv.usingMouse)
			{
				yButtonHint.fillButtonPrompt(yHintText, yButtonHint.controllerE);
			}
			else
			{
				yButtonHint.fillButtonPrompt(yHintText, yButtonHint.controllerY);
			}
		}
		else
		{
			yButtonHint.gameObject.SetActive(false);
		}
		if (aButtonText != "null")
		{
			aButtonHint.gameObject.SetActive(true);
			if (Inventory.inv.usingMouse)
			{
				aButtonHint.fillButtonPrompt(aButtonText, aButtonHint.controllerRightClick);
			}
			else
			{
				aButtonHint.fillButtonPrompt(aButtonText, aButtonHint.controllerB);
			}
		}
		else
		{
			aButtonHint.gameObject.SetActive(false);
		}
		quickMoveStackHint.gameObject.SetActive(false);
		splitStackHint.gameObject.SetActive(false);
	}

	public void makeTopNotification(string notificationString, string subText = "", ASound notificationSound = null)
	{
		toNotify.Add(notificationString);
		subTextNot.Add(subText);
		soundToPlay.Add(notificationSound);
		if (topNotificationRunning == null)
		{
			topNotificationRunning = StartCoroutine(runTopNotification());
		}
	}

	private IEnumerator runTopNotification()
	{
		topNotification.gameObject.SetActive(false);
		while (ConversationManager.manage.inConversation)
		{
			yield return null;
		}
		while (GiftedItemWindow.gifted.windowOpen)
		{
			yield return null;
		}
		while (toNotify.Count > 0)
		{
			topNotification.setText(toNotify[0], subTextNot[0]);
			topNotification.gameObject.SetActive(true);
			topNotification.startShowText();
			if (soundToPlay[0] != null)
			{
				SoundManager.manage.play2DSound(soundToPlay[0]);
			}
			yield return new WaitForSeconds(5f);
			float num = 0f;
			float num2 = 0f;
			if (num < 1f || num2 < 100f)
			{
				float num3 = num + Time.deltaTime;
				yield return null;
			}
			yield return StartCoroutine(topNotification.GetComponent<WindowAnimator>().closeWithMask());
			toNotify.RemoveAt(0);
			subTextNot.RemoveAt(0);
			soundToPlay.RemoveAt(0);
		}
		topNotification.gameObject.SetActive(false);
		topNotificationRunning = null;
	}
}
