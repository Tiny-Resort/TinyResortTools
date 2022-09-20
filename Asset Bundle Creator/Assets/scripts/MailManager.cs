using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MailManager : MonoBehaviour
{
	public static MailManager manage;

	public GameObject mailButtonPrefab;

	public Transform LettersWindow;

	public GameObject letterListWindow;

	public GameObject showLetterWindow;

	public GameObject deleteButtonGO;

	public bool mailWindowOpen;

	public TextMeshProUGUI letterText;

	public FillRecipeSlot attachmentSlot;

	public GameObject mailWindow;

	public GameObject takeRewardButton;

	public List<Letter> mailInBox = new List<Letter>();

	public List<Letter> tomorrowsLetters = new List<Letter>();

	public LetterTemplate animalResearchLetter;

	public LetterTemplate returnTrapLetter;

	public LetterTemplate devLetter;

	public LetterTemplate catalogueItemLetter;

	public LetterTemplate craftmanDayOff;

	public LetterTemplate[] randomLetters;

	public LetterTemplate[] thankYouLetters;

	public LetterTemplate[] didNotFitInInvLetter;

	public LetterTemplate[] fishingTips;

	public LetterTemplate[] bugTips;

	public LetterTemplate[] licenceLevelUp;

	public UnityEvent newMailEvent = new UnityEvent();

	public Image mailBorder;

	private List<MailButton> showingButtons = new List<MailButton>();

	public GameObject noMailScreen;

	public Animator windowBounceAnim;

	public TextMeshProUGUI fromText;

	public GameObject unopenedWindow;

	public ASound openLetterSound;

	public Image letterSprite;

	public WindowAnimator changeLetterWindowMask;

	private int showingLetterId;

	private List<Letter> deleteOnClose = new List<Letter>();

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
	}

	public void sendDailyMail()
	{
		for (int i = 0; i < tomorrowsLetters.Count; i++)
		{
			mailInBox.Add(tomorrowsLetters[i]);
		}
		if (WorldManager.manageWorld.day == 6 && WorldManager.manageWorld.week == 3 && LicenceManager.manage.allLicences[3].getCurrentLevel() >= 1)
		{
			int undiscoveredFish = PediaManager.manage.getUndiscoveredFish();
			if (undiscoveredFish != -1)
			{
				mailInBox.Add(new Letter(-3, Letter.LetterType.FishingTips, undiscoveredFish));
			}
		}
		if (WorldManager.manageWorld.day == 3 && WorldManager.manageWorld.week == 2)
		{
			int undiscoveredBug = PediaManager.manage.getUndiscoveredBug();
			if (undiscoveredBug != -1)
			{
				mailInBox.Add(new Letter(-4, Letter.LetterType.BugTips, undiscoveredBug));
			}
		}
		tomorrowsLetters.Clear();
		for (int j = 0; j < NPCManager.manage.NPCDetails.Length; j++)
		{
			if (NPCManager.manage.npcStatus[j].checkIfHasMovedIn() && Random.Range(0, 109 - NPCManager.manage.npcStatus[j].relationshipLevel) == 1)
			{
				mailInBox.Add(new Letter(j, Letter.LetterType.randomLetter));
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.GetLetters);
			}
		}
		newMailEvent.Invoke();
	}

	public void sendAnInvFullLetter(int itemId, int stack)
	{
		mailInBox.Add(new Letter(ConversationManager.manage.lastTalkTo.myId.NPCNo, Letter.LetterType.fullInvLetter, itemId, stack));
		newMailEvent.Invoke();
	}

	public void sendAnAnimalCapturedLetter(int rewardAmount, int trapType)
	{
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.TrapAnAnimal);
		tomorrowsLetters.Add(new Letter(-1, Letter.LetterType.AnimalResearchLetter, Inventory.inv.getInvItemId(Inventory.inv.moneyItem), rewardAmount));
		tomorrowsLetters.Add(new Letter(-1, Letter.LetterType.AnimalTrapReturn, trapType, 1));
	}

	public void checkForUpdateLetter()
	{
		mailInBox.Add(new Letter(-2, Letter.LetterType.DevLetter));
	}

	public void openMailWindow()
	{
		mailWindowOpen = true;
		mailWindow.gameObject.SetActive(true);
		showLetterWindow.gameObject.SetActive(false);
		letterListWindow.gameObject.SetActive(true);
		fillButtons();
		Inventory.inv.checkIfWindowIsNeeded();
		if ((bool)MailBoxShowsMail.showsMail)
		{
			MailBoxShowsMail.showsMail.refresh();
		}
		if ((bool)Inventory.inv.activeScrollBar)
		{
			Inventory.inv.activeScrollBar.resetToTop();
		}
		MenuButtonsTop.menu.closed = false;
		letterListWindow.SetActive(true);
	}

	public void closeMailWindow()
	{
		mailWindowOpen = false;
		mailWindow.gameObject.SetActive(false);
		showLetterWindow.gameObject.SetActive(false);
		letterListWindow.gameObject.SetActive(true);
		if ((bool)MailBoxShowsMail.showsMail)
		{
			MailBoxShowsMail.showsMail.refresh();
		}
		for (int i = 0; i < deleteOnClose.Count; i++)
		{
			mailInBox.Remove(deleteOnClose[i]);
		}
		deleteOnClose.Clear();
		Inventory.inv.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	private void fillButtons()
	{
		for (int i = 0; i < showingButtons.Count; i++)
		{
			Object.Destroy(showingButtons[i].gameObject);
		}
		showingButtons.Clear();
		for (int j = 0; j < mailInBox.Count; j++)
		{
			MailButton component = Object.Instantiate(mailButtonPrefab, LettersWindow).GetComponent<MailButton>();
			component.letterId = j;
			component.buttonText.text = getSentByName(mailInBox[j].sentById);
			component.iconColor = getLetterColour(mailInBox[j].sentById);
			component.showOpen(mailInBox[j].hasBeenRead, mailInBox[j].itemAttached);
			showingButtons.Add(component);
		}
		if (mailInBox.Count <= 0)
		{
			noMailScreen.SetActive(true);
		}
		else
		{
			noMailScreen.SetActive(false);
		}
	}

	private void selectorDeselectButtons()
	{
		for (int i = 0; i < showingButtons.Count; i++)
		{
			if (showingLetterId == i)
			{
				showingButtons[i].showOpen(mailInBox[i].hasBeenRead, mailInBox[i].itemAttached);
			}
		}
	}

	public void closeShowLetterWindow()
	{
		showLetterWindow.SetActive(false);
		letterListWindow.SetActive(true);
		if (mailInBox.Count <= 0)
		{
			noMailScreen.SetActive(true);
		}
		else
		{
			noMailScreen.SetActive(false);
		}
	}

	public void showLetter(int letterId)
	{
		letterListWindow.SetActive(false);
		changeLetterWindowMask.refreshAnimation();
		letterSprite.color = getLetterColour(mailInBox[letterId].sentById);
		showingLetterId = letterId;
		mailBorder.color = getLetterColour(mailInBox[letterId].sentById);
		if (mailInBox[showingLetterId].hasBeenRead)
		{
			unopenedWindow.SetActive(false);
		}
		else
		{
			fromText.text = "From " + getSentByName(mailInBox[letterId].sentById);
			unopenedWindow.SetActive(true);
		}
		showLetterWindow.gameObject.SetActive(true);
		SoundManager.manage.play2DSound(BulletinBoard.board.pageTurnSound);
		if (mailInBox[letterId].itemOriginallAttached != -1)
		{
			letterText.text = "<size=18><b>To " + Inventory.inv.playerName + "</size></b>,\n\n" + mailInBox[letterId].getMyTemplate().letterText.Replace("<itemAttachedName>", Inventory.inv.allItems[mailInBox[letterId].itemOriginallAttached].getInvItemName()) + "\n\n<size=18><b>From " + getSentByName(mailInBox[letterId].sentById) + "</size></b>.";
			letterText.text = letterText.text.Replace("<season>", RealWorldTimeLight.time.getSeasonName(mailInBox[letterId].seasonSent - 1));
			letterText.text = letterText.text.Replace("<biomeName>", AnimalManager.manage.fillAnimalLocation(Inventory.inv.allItems[mailInBox[letterId].itemOriginallAttached]));
			letterText.text = letterText.text.Replace("<timeOfDay>", AnimalManager.manage.fillAnimalTimeOfDay(Inventory.inv.allItems[mailInBox[letterId].itemOriginallAttached]));
			letterText.text = letterText.text.Replace("<licenceName>", LicenceManager.manage.getLicenceName((LicenceManager.LicenceTypes)mailInBox[letterId].itemOriginallAttached));
		}
		else
		{
			letterText.text = "<size=18><b>To " + Inventory.inv.playerName + "</size></b>,\n\n" + mailInBox[letterId].getMyTemplate().letterText + "\n\n<size=18><b>From " + getSentByName(mailInBox[letterId].sentById) + "</b>.";
		}
		if (mailInBox[letterId].itemAttached != -1)
		{
			takeRewardButton.SetActive(true);
			Inventory.inv.setCurrentlySelectedAndMoveCursor(takeRewardButton.GetComponent<RectTransform>());
			deleteButtonGO.SetActive(false);
			attachmentSlot.gameObject.SetActive(true);
			attachmentSlot.fillRecipeSlotForQuestReward(mailInBox[letterId].itemAttached, mailInBox[letterId].stackOfItemAttached);
		}
		else
		{
			takeRewardButton.SetActive(false);
			deleteButtonGO.SetActive(true);
			Inventory.inv.setCurrentlySelectedAndMoveCursor(deleteButtonGO.GetComponent<RectTransform>());
			attachmentSlot.gameObject.SetActive(false);
		}
		selectorDeselectButtons();
	}

	public void takeAttachment()
	{
		if (Inventory.inv.moneyItem == Inventory.inv.allItems[mailInBox[showingLetterId].itemAttached])
		{
			takeRewardButton.SetActive(false);
			deleteButtonGO.SetActive(true);
			Inventory.inv.setCurrentlySelectedAndMoveCursor(deleteButtonGO.GetComponent<RectTransform>());
			attachmentSlot.gameObject.SetActive(false);
			mailInBox[showingLetterId].itemAttached = -1;
			showingButtons[showingLetterId].showOpen(true, mailInBox[showingLetterId].itemAttached);
			Inventory.inv.changeWallet(mailInBox[showingLetterId].stackOfItemAttached);
		}
		else if (Inventory.inv.addItemToInventory(mailInBox[showingLetterId].itemAttached, mailInBox[showingLetterId].stackOfItemAttached))
		{
			takeRewardButton.SetActive(false);
			deleteButtonGO.SetActive(true);
			Inventory.inv.setCurrentlySelectedAndMoveCursor(deleteButtonGO.GetComponent<RectTransform>());
			attachmentSlot.gameObject.SetActive(false);
			mailInBox[showingLetterId].itemAttached = -1;
			showingButtons[showingLetterId].showOpen(true, mailInBox[showingLetterId].itemAttached);
		}
		else
		{
			NotificationManager.manage.createChatNotification("Pockets Full", true);
		}
	}

	public void openLetter()
	{
		SoundManager.manage.play2DSound(openLetterSound);
		windowBounceAnim.SetTrigger("Open");
		Invoke("openLetterDelay", 0.35f);
	}

	private void openLetterDelay()
	{
		mailInBox[showingLetterId].hasBeenRead = true;
		showLetter(showingLetterId);
	}

	public void deleteButton()
	{
		deleteOnClose.Add(mailInBox[showingLetterId]);
		showingButtons[showingLetterId].gameObject.SetActive(false);
		closeShowLetterWindow();
		for (int num = showingLetterId; num >= 0; num--)
		{
			if (showingButtons[num].gameObject.activeSelf)
			{
				Inventory.inv.setCurrentlySelectedAndMoveCursor(showingButtons[num].GetComponent<RectTransform>());
				return;
			}
		}
		for (int i = 0; i < showingButtons.Count; i++)
		{
			if (showingButtons[i].gameObject.activeSelf)
			{
				Inventory.inv.setCurrentlySelectedAndMoveCursor(showingButtons[i].GetComponent<RectTransform>());
				return;
			}
		}
		noMailScreen.SetActive(true);
	}

	public string getSentByName(int id)
	{
		if (id >= 0)
		{
			return NPCManager.manage.NPCDetails[id].NPCName;
		}
		switch (id)
		{
		case -1:
			return "Animal Research Centre";
		case -2:
			return "Dinkum Dev";
		case -3:
			return "Fishin' Tipster";
		case -4:
			return "Bug Tipster";
		default:
			return "Unknown";
		}
	}

	public Color getLetterColour(int id)
	{
		if (id >= 0)
		{
			return NPCManager.manage.NPCDetails[id].npcColor;
		}
		return Color.white;
	}

	public bool checkIfAnyUndreadLetters()
	{
		for (int i = 0; i < mailInBox.Count; i++)
		{
			if (!mailInBox[i].hasBeenRead)
			{
				return true;
			}
		}
		return false;
	}

	public void sendLicenceUnlockMail(int licenceId)
	{
		mailInBox.Add(new Letter(6, Letter.LetterType.LicenceUnlock, licenceId));
	}
}
