using System.Collections;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConversationManager : MonoBehaviour
{
	public enum specialAction
	{
		None = 0,
		GivePhotoNpc = 1,
		GivePhotoMuseum = 2,
		OpenDeedMenu = 3,
		OpenCraftsManMenu = 4,
		OpenDonationMenu = 5,
		OpenGiveMenu = 6,
		OpenGiveMenuToSell = 7,
		OpenHairCutColorMenu = 8,
		OpenHaircutMenu = 9,
		OpenAnimalMenu = 10,
		DonateToMuseum = 11,
		GetRandomRequest = 12,
		CompleteNpcRequest = 13,
		AcceptRequest = 14,
		RewardForRequest = 15,
		OpenGiveForBulletinBoard = 16,
		RewardForBulletinBoard = 17,
		AcceptQuest = 18,
		ReturnItemsInGiveMenu = 19,
		RequestHouseUpgrade = 20,
		ChargeForHouseUpgrade = 21,
		ChatToNPCAtWork = 22,
		GetRandomConvo = 23,
		StopFollowing = 24,
		StartFollowing = 25,
		SellGivenItems = 26,
		BuyItem = 27,
		OpenGiveMenuMuseum = 28,
		CompleteRequestAndGiveReward = 29,
		DeclineNpcRequestAndRemoveRequest = 30,
		AskToHangOut = 31,
		OpenBankMenu = 32,
		OpenHouseEditor = 33,
		OpenTownManager = 34,
		OpenTownConvo = 35,
		AskAboutDeeds = 36,
		OpenDonateWindow = 37,
		ConfirmDeedInConvo = 38,
		OpenConstructionBox = 39,
		AskAboutHouseGeneral = 40,
		PlaceDeed = 41,
		AcceptAndCompleteQuest = 42,
		OpenLicenceWindow = 43,
		StartTeleport = 44,
		OpenGiveForSwap = 45,
		SellItemByWeight = 46,
		DropSwapItem = 47,
		OpenTechDonateWindow = 48,
		DonateTechItems = 49,
		AgreeToCraftsman = 50,
		GiveCompletedTech = 51,
		OpenAnimalMenuForPetDog = 52,
		OpenBuildingManager = 53,
		AgreeToMoveBuilding = 54,
		DeclineRequest = 55,
		GiveTalkedAboutItem = 56,
		AskToMoveHouse = 57,
		OpenCatalogue = 58,
		OpenTrapperCraftMenu = 59,
		RepairItems = 60,
		ConfirmSleep = 61,
		StartLookingForTechConvo = 62
	}

	public enum showEmotion
	{
		None = 0,
		Laugh = 1,
		Cry = 2,
		ShakeFist = 3
	}

	public static ConversationManager manage;

	[Header("Conversations -------")]
	public Conversation[] RandomFollowStops;

	public Conversation atWorkChatConversation;

	[Header("Other -------")]
	public GameObject optionsWindow;

	public HeartContainer[] relationHearts;

	public Transform conversationWindow;

	public Transform OptionWindow;

	public TextMeshProUGUI npcNameText;

	public Image nameBox;

	public Image backBox;

	public Image arrowBounceColour;

	public Text converstationText;

	public TextMeshProUGUI conversationTextPro;

	public GameObject OptionButton;

	public GameObject nextArrowBounce;

	public VerticalLayoutGroup optionLayout;

	public string playerName;

	public bool ready;

	public bool inConversation;

	public NPCAI lastTalkTo;

	public Conversation customConvo;

	private int selectedOption;

	public bool buttonClicked;

	public bool inOptionScreen;

	[Header("Sounds -------")]
	public ASound nextTextSound;

	public ASound skipTextSound;

	public ASound showOptionsSound;

	private bool startDelay;

	private OptionWindow[] optionButtons;

	public ConstructionBoxInput donatingToBuilding;

	private bool conversationLinedUp;

	[Header("Talked about items -------")]
	public InventoryItem blobFish;

	public InventoryItem journal;

	private bool speedUp;

	private float speedWait;

	private WaitForSeconds waitForNext = new WaitForSeconds(0.15f);

	private WaitForSeconds waitForNextSpeedUp = new WaitForSeconds(0.006f);

	private WaitForSeconds wordWait = new WaitForSeconds(0.01f);

	private float startTalkDelay;

	private bool requestJob;

	private bool acceptRequest;

	private bool grabRandomConvo;

	private bool grabAtWorkConvo;

	private bool openAnimalMenu;

	private bool openGiveMenu;

	private bool openGiveMenuMuseum;

	private bool openConstructionBox;

	private bool openForSwap;

	private bool openForTechDonate;

	private bool openGiveBulletinBoard;

	private bool openCraftMenu;

	private bool openTrapperCraftMenu;

	private bool sellGiveItems;

	private bool donateTechItems;

	private bool returnGiveItems;

	private bool agreeToCraftsman;

	private bool tryAndBuyItem;

	private bool setNewFollow;

	private bool stopFollow;

	private bool acceptQuest;

	private bool acceptAndCompleteQuest;

	private bool completeRequest;

	private bool openCatalogue;

	private bool giveReward;

	private bool openDeedCraft;

	private bool showOptionAmountWindow;

	private bool openHairCutMenu;

	private bool openHairCutMenuColour;

	private bool returnExpandHouse;

	private bool askAboutHouseGeneral;

	private bool openTownConvo;

	private bool openGivePhoto;

	private bool openGivePhotoMuseum;

	private bool openGiveMenuSell;

	private bool askToHangOut;

	private bool donatedSomethingAskAgain;

	private bool openBankMenu;

	private bool openHouseEditor;

	private bool openTownManager;

	private bool openBuildingManager;

	private bool startMovingHouseConvo;

	private bool openTownDebtPayment;

	private bool askAboutDeeds;

	private bool confirmDeedInConvo;

	private bool openLicenceMenu;

	private bool giveCompletedTech;

	private bool openAnimalMenuForPetDog;

	private bool agreeToMoveBuilding;

	private bool repairItems;

	private bool lookingForTechConvo;

	private int talkingAboutPhotoId = -1;

	private int petVariationNo;

	private string teledir = "null";

	private int wantsToShowEmotion;

	private int currentlyShowingEmotion;

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
	}

	public void talkToNPC(NPCAI myNPC, Conversation newCustomConvo = null, bool hasStartDelay = false, bool forceUseCustom = false)
	{
		int nPCNo = myNPC.GetComponent<NPCIdentity>().NPCNo;
		if (!inConversation && myNPC.canBeTalkTo())
		{
			Inventory.inv.quickSlotBar.gameObject.SetActive(false);
			startDelay = hasStartDelay;
			NetworkMapSharer.share.localChar.CmdChangeTalkTo(myNPC.netId, true);
			lastTalkTo = myNPC;
			NPCAI nPCAI = lastTalkTo;
			MonoBehaviour.print("Last talked to set to " + (((object)nPCAI != null) ? nPCAI.ToString() : null));
			inConversation = true;
			if ((bool)newCustomConvo && forceUseCustom)
			{
				customConvo = newCustomConvo;
			}
			else if (nPCNo != -1 && NPCManager.manage.shouldAskToMoveIn(nPCNo))
			{
				NotificationManager.manage.makeTopNotification("A new deed is available!", "Talk to Fletch to apply for deeds.", SoundManager.manage.notificationSound);
				customConvo = NPCManager.manage.NPCDetails[nPCNo].moveInRequestConversation;
				DeedManager.manage.unlockDeed(NPCManager.manage.NPCDetails[nPCNo].deedOnMoveRequest);
			}
			else if (nPCNo == -1 || !NetworkMapSharer.share.isServer || NPCManager.manage.npcStatus[nPCNo].hasMet)
			{
				customConvo = QuestManager.manage.checkIfThereIsAQuestToGive(nPCNo);
				if (customConvo == null && (bool)newCustomConvo)
				{
					customConvo = newCustomConvo;
				}
			}
			else
			{
				customConvo = NPCManager.manage.NPCDetails[nPCNo].introductionConversation;
				NPCManager.manage.npcStatus[nPCNo].hasMet = true;
			}
			if (nPCNo == -1)
			{
				npcNameText.text = "";
				updateRelationHeart(nPCNo, true);
				nameBox.color = Color.white;
				backBox.color = Color.grey;
				arrowBounceColour.color = Color.grey;
				nameBox.enabled = false;
			}
			else
			{
				nameBox.enabled = true;
				npcNameText.text = NPCManager.manage.NPCDetails[myNPC.GetComponent<NPCIdentity>().NPCNo].NPCName;
				nameBox.color = NPCManager.manage.NPCDetails[myNPC.GetComponent<NPCIdentity>().NPCNo].npcColor;
				backBox.color = NPCManager.manage.NPCDetails[myNPC.GetComponent<NPCIdentity>().NPCNo].npcColor;
				arrowBounceColour.color = NPCManager.manage.NPCDetails[myNPC.GetComponent<NPCIdentity>().NPCNo].npcColor;
				updateRelationHeart(nPCNo);
			}
			StartCoroutine(readThroughText());
		}
		else if (myNPC.followingNetId == NetworkMapSharer.share.localChar.netId)
		{
			startDelay = hasStartDelay;
			NetworkMapSharer.share.localChar.CmdChangeTalkTo(myNPC.netId, true);
			lastTalkTo = myNPC;
			customConvo = QuestManager.manage.checkIfThereIsAQuestToGive(nPCNo);
			if (customConvo == null)
			{
				customConvo = RandomFollowStops[Random.Range(0, RandomFollowStops.Length)];
			}
			StartCoroutine(readThroughText());
		}
	}

	public bool clickReadyButton()
	{
		if ((InputMaster.input.UISelect() && !inOptionScreen) || (InputMaster.input.UICancel() && !inOptionScreen))
		{
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (!inConversation || TownManager.manage.firstConnect)
		{
			return;
		}
		if (nameBox.gameObject.activeInHierarchy)
		{
			bool flag = false;
			flag = InputMaster.input.UISelectHeld();
			if (!flag)
			{
				flag = InputMaster.input.UICancelHeld();
			}
			if (flag)
			{
				if (speedWait <= 0.5f)
				{
					speedWait += Time.deltaTime;
					speedUp = false;
				}
				else
				{
					speedUp = true;
				}
			}
			else
			{
				speedUp = false;
				speedWait = 0f;
			}
		}
		else
		{
			speedUp = false;
			speedWait = 0f;
		}
	}

	private IEnumerator selectionByKeys()
	{
		int currentlySelected = 0;
		if (!Inventory.inv.usingMouse)
		{
			while (optionButtons == null)
			{
				yield return null;
			}
			yield return null;
			Inventory.inv.cursor.transform.position = optionButtons[0].transform.position + new Vector3(optionButtons[0].GetComponent<RectTransform>().sizeDelta.x / 2f - 1f, 0f, 0f);
		}
		while (inOptionScreen)
		{
			float vertical = 0f - InputMaster.input.UINavigation().y;
			if (InputMaster.input.UICancel())
			{
				currentlySelected = optionButtons.Length - 1;
				selectWithButton(currentlySelected);
				Inventory.inv.cursor.transform.position = optionButtons[currentlySelected].transform.position + new Vector3(optionButtons[currentlySelected].GetComponent<RectTransform>().sizeDelta.x / 2f - 1f, 0f, 0f);
				yield return null;
				yield return null;
				yield return null;
				yield return null;
				buttonClick();
			}
			if (vertical >= 0.45f)
			{
				if (GiveNPC.give.optionWindowOpen && currentlySelected <= -1)
				{
					currentlySelected = Mathf.Clamp(currentlySelected + 1, -2, 0);
					SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
					switch (currentlySelected)
					{
					case -1:
						Inventory.inv.cursor.transform.position = GiveNPC.give.optionAmountWindow.downButton.position;
						break;
					case -2:
						Inventory.inv.cursor.transform.position = GiveNPC.give.optionAmountWindow.upButton.position;
						break;
					case 0:
						selectWithButton(currentlySelected);
						Inventory.inv.cursor.transform.position = optionButtons[currentlySelected].transform.position + new Vector3(optionButtons[currentlySelected].GetComponent<RectTransform>().sizeDelta.x / 2f - 1f, 0f, 0f);
						break;
					}
				}
				else
				{
					SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
					currentlySelected = Mathf.Clamp(currentlySelected + 1, 0, optionButtons.Length - 1);
					selectWithButton(currentlySelected);
					Inventory.inv.cursor.transform.position = optionButtons[currentlySelected].transform.position + new Vector3(optionButtons[currentlySelected].GetComponent<RectTransform>().sizeDelta.x / 2f - 1f, 0f, 0f);
				}
				yield return new WaitForSeconds(0.15f);
			}
			else if (vertical <= -0.45f)
			{
				if (GiveNPC.give.optionWindowOpen && currentlySelected <= 0)
				{
					currentlySelected = Mathf.Clamp(currentlySelected - 1, -2, 0);
					SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
					switch (currentlySelected)
					{
					case -1:
						Inventory.inv.cursor.transform.position = GiveNPC.give.optionAmountWindow.downButton.position;
						break;
					case -2:
						Inventory.inv.cursor.transform.position = GiveNPC.give.optionAmountWindow.upButton.position;
						break;
					case 0:
						selectWithButton(currentlySelected);
						Inventory.inv.cursor.transform.position = optionButtons[currentlySelected].transform.position + new Vector3(optionButtons[currentlySelected].GetComponent<RectTransform>().sizeDelta.x / 2f - 1f, 0f, 0f);
						break;
					}
				}
				else
				{
					SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
					currentlySelected = Mathf.Clamp(currentlySelected - 1, 0, optionButtons.Length - 1);
					selectWithButton(currentlySelected);
					Inventory.inv.cursor.transform.position = optionButtons[currentlySelected].transform.position + new Vector3(optionButtons[currentlySelected].GetComponent<RectTransform>().sizeDelta.x / 2f - 1f, 0f, 0f);
				}
				yield return new WaitForSeconds(0.15f);
			}
			yield return null;
		}
	}

	public void selectWithButton(int selectionInt)
	{
		selectedOption = selectionInt;
	}

	public void buttonClick()
	{
		buttonClicked = true;
	}

	private void tryAndTalk()
	{
		if ((bool)lastTalkTo && !lastTalkTo.isSign && !lastTalkTo.myAudio.isPlaying)
		{
			lastTalkTo.myAudio.pitch = NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].NPCVoice.getPitch();
			lastTalkTo.myAudio.volume = NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].NPCVoice.volume * SoundManager.manage.getSoundVolume();
			lastTalkTo.myAudio.PlayOneShot(NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].NPCVoice.getSound());
		}
	}

	public void setStartTalkDelay(float delay)
	{
		startTalkDelay = delay;
	}

	public void checkIfYouWereTalkingToNPCAndStopTalkingAfterMenuCloses()
	{
		if ((bool)lastTalkTo && lastTalkTo.talkingTo == NetworkMapSharer.share.localChar.netId)
		{
			NetworkMapSharer.share.localChar.CmdChangeTalkTo(lastTalkTo.netId, false);
		}
	}

	public bool checkIfShouldExitConversation()
	{
		if (!openGiveMenu && !openGiveMenuSell && !openGiveMenuMuseum && !openHairCutMenu && !openHairCutMenuColour && !openCraftMenu && !openDeedCraft && !openAnimalMenu && !openGiveBulletinBoard && !openGivePhoto && !openForTechDonate && !openLicenceMenu && !openGiveBulletinBoard && !openGivePhoto)
		{
			return true;
		}
		return false;
	}

	private IEnumerator readThroughText()
	{
		Inventory.inv.quickSlotBar.gameObject.SetActive(false);
		inConversation = true;
		MenuButtonsTop.menu.closed = false;
		Inventory.inv.checkIfWindowIsNeeded();
		optionsWindow.SetActive(false);
		conversationTextPro.text = "";
		if (startDelay)
		{
			startDelay = false;
			yield return new WaitForSeconds(0.3f);
		}
		if (startTalkDelay != 0f)
		{
			yield return new WaitForSeconds(startTalkDelay);
			startTalkDelay = 0f;
		}
		conversationWindow.gameObject.SetActive(true);
		while (!nameBox.gameObject.activeInHierarchy)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.25f);
		Conversation converstationToSay;
		if (!customConvo)
		{
			converstationToSay = (lastTalkTo.isSign ? ConversationGenerator.generate.getRandomComment() : ((!(ConversationGenerator.generate.checkCurrentStatusOnTalk() != null)) ? ConversationGenerator.generate.getGreeting(lastTalkTo.myId.NPCNo) : ConversationGenerator.generate.checkCurrentStatusOnTalk()));
		}
		else
		{
			converstationToSay = customConvo;
			customConvo = null;
		}
		yield return StartCoroutine(readConversationSegment(converstationToSay));
		yield return waitForNext;
		if (converstationToSay.optionNames.Length == 0 && checkIfShouldExitConversation())
		{
			checkIfYouWereTalkingToNPCAndStopTalkingAfterMenuCloses();
		}
		doSpecialFunctions(converstationToSay);
		if (converstationToSay.optionNames.Length != 0)
		{
			nextArrowBounce.SetActive(false);
			if (showOptionAmountWindow)
			{
				GiveNPC.give.openOptionAmountWindow();
				showOptionAmountWindow = false;
			}
			else
			{
				GiveNPC.give.closeOptionAmountWindow();
			}
			optionButtons = new OptionWindow[converstationToSay.optionNames.Length];
			for (int i = 0; i < converstationToSay.optionNames.Length; i++)
			{
				optionButtons[i] = Object.Instantiate(OptionButton, OptionWindow).GetComponent<OptionWindow>();
				optionButtons[i].setOptionText(buttonTextChange(converstationToSay, i));
				optionButtons[i].GetComponent<InvButton>().isConverstationOption = true;
				optionButtons[i].GetComponent<InvButton>().craftRecipeNumber = i;
			}
			for (int j = 0; j < converstationToSay.optionNames.Length; j++)
			{
				optionButtons[j].showOptionBox();
			}
			optionsWindow.SetActive(true);
			SoundManager.manage.play2DSound(showOptionsSound);
			optionLayout.enabled = false;
			optionLayout.enabled = true;
			yield return null;
			ready = false;
			bool optionNotSelected = true;
			selectedOption = 0;
			inOptionScreen = true;
			StartCoroutine(selectionByKeys());
			while (optionNotSelected)
			{
				for (int k = 0; k < converstationToSay.optionNames.Length; k++)
				{
					if (selectedOption == k)
					{
						optionButtons[k].selectedOrNot(true);
					}
					else
					{
						optionButtons[k].selectedOrNot(false);
					}
				}
				if (buttonClicked)
				{
					buttonClicked = false;
					optionNotSelected = false;
					GiveNPC.give.closeOptionAmountWindow();
					OptionWindow[] array = optionButtons;
					for (int l = 0; l < array.Length; l++)
					{
						Object.Destroy(array[l].gameObject);
					}
				}
				yield return null;
			}
			optionsWindow.SetActive(false);
			yield return waitForNext;
			inOptionScreen = false;
			yield return StartCoroutine(readConversationSegment(converstationToSay, selectedOption));
			nextArrowBounce.SetActive(false);
			if (checkIfShouldExitConversation())
			{
				checkIfYouWereTalkingToNPCAndStopTalkingAfterMenuCloses();
			}
			doSpecialFunctions(converstationToSay);
		}
		if (!lastTalkTo.isSign)
		{
			lastTalkTo.faceAnim.stopEmotions();
		}
		inConversation = false;
		if (tryAndBuyItem)
		{
			GiveNPC.give.tryToBuy();
			tryAndBuyItem = false;
		}
		if (donateTechItems)
		{
			donateTechItems = false;
			if (CharLevelManager.manage.isCraftsmanRecipeUnlockedThisLevel())
			{
				conversationLinedUp = true;
				talkToNPC(lastTalkTo, CraftsmanManager.manage.hasLearnedANewRecipeIcon);
			}
		}
		if (returnExpandHouse)
		{
			conversationLinedUp = true;
			talkToNPC(lastTalkTo, NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].keeperConversation.GetComponent<HouseConversationGroup>().getConversation());
			returnExpandHouse = false;
		}
		if (askAboutHouseGeneral)
		{
			conversationLinedUp = true;
			talkToNPC(lastTalkTo, NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].keeperConversation.GetComponent<HouseConversationGroup>().getStartingConversation());
			askAboutHouseGeneral = false;
		}
		if (lookingForTechConvo)
		{
			conversationLinedUp = true;
			talkToNPC(lastTalkTo, CraftsmanManager.manage.lookingForTechConvo);
			lookingForTechConvo = false;
		}
		if (openTownConvo)
		{
			conversationLinedUp = true;
			talkToNPC(lastTalkTo, NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].keeperConversation.GetComponent<TownConversation>().openingConversation);
			openTownConvo = false;
		}
		if (askAboutDeeds)
		{
			conversationLinedUp = true;
			talkToNPC(lastTalkTo, NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].keeperConversation.GetComponent<TownConversation>().askAboutDeeds());
			askAboutDeeds = false;
		}
		if (confirmDeedInConvo)
		{
			confirmDeedInConvo = false;
			DeedManager.manage.confirmDeedInConvo();
		}
		if (giveCompletedTech)
		{
			giveCompletedTech = false;
			CraftsmanManager.manage.tryAndGiveCompletedItem();
		}
		if (grabAtWorkConvo)
		{
			conversationLinedUp = true;
			talkToNPC(lastTalkTo, atWorkChatConversation);
			grabAtWorkConvo = false;
		}
		if (grabRandomConvo)
		{
			conversationLinedUp = true;
			if (!NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].hasBeenTalkedToToday)
			{
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.TalkToPeople);
			}
			int num = Random.Range(0, NPCManager.manage.getNoOfNPCsMovedIn());
			if (!NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].hasBeenTalkedToToday && NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].hasRandomConvoAvaliable())
			{
				talkToNPC(lastTalkTo, NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].getRandomChat(lastTalkTo.myId.NPCNo));
				NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].hasBeenTalkedToToday = true;
				NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].addToRelationshipLevel(1);
			}
			else if (NetworkMapSharer.share.isServer && NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].checkIfHasMovedIn() && !NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].hasGossipedToday && NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].gossip.Length != 0 && num != lastTalkTo.myId.NPCNo && NPCManager.manage.npcStatus[num].checkIfHasMovedIn())
			{
				talkToNPC(lastTalkTo, NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].gossip[num]);
				NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].hasGossipedToday = true;
				if (NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].relationshipLevel <= 25)
				{
					NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].addToRelationshipLevel(1);
				}
			}
			else if (NetworkMapSharer.share.isServer && NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].checkIfHasMovedIn() && NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].townMentions.Length != 0 && Random.Range(0, 5) == 3)
			{
				int noOfNPCsMovedIn = NPCManager.manage.getNoOfNPCsMovedIn();
				int num2 = 0;
				if (noOfNPCsMovedIn <= 2)
				{
					num2 = 0;
				}
				else if (noOfNPCsMovedIn <= 4)
				{
					num2 = 1;
				}
				else if (noOfNPCsMovedIn <= 6)
				{
					num2 = 2;
				}
				else if (noOfNPCsMovedIn <= 8)
				{
					num2 = 3;
				}
				talkToNPC(lastTalkTo, NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].townMentions[num2]);
			}
			else
			{
				talkToNPC(lastTalkTo, ConversationGenerator.generate.getRandomComment());
			}
			grabRandomConvo = false;
		}
		if (askToHangOut)
		{
			conversationLinedUp = true;
			talkToNPC(lastTalkTo, ConversationGenerator.generate.askToHangOutConversation(lastTalkTo.myId.NPCNo));
			askToHangOut = false;
		}
		if (donatedSomethingAskAgain)
		{
			conversationLinedUp = true;
			donatedSomethingAskAgain = false;
			talkToNPC(lastTalkTo, lastTalkTo.getVendorConversation().GetComponent<MuseumConvoGroup>().askIfHasAnotherDonation);
		}
		if (agreeToCraftsman)
		{
			conversationLinedUp = true;
			agreeToCraftsman = false;
			CraftsmanManager.manage.agreeToCrafting();
		}
		if (agreeToMoveBuilding)
		{
			BuildingManager.manage.confirmWantToMoveBuilding();
			agreeToMoveBuilding = false;
		}
		if (openBuildingManager)
		{
			BuildingManager.manage.openWindow();
			openBuildingManager = false;
		}
		if (startMovingHouseConvo)
		{
			conversationLinedUp = true;
			BuildingManager.manage.getWantToMovePlayerHouseConvo();
			startMovingHouseConvo = false;
		}
		if (requestJob)
		{
			conversationLinedUp = true;
			PostOnBoard postOnBoard = BulletinBoard.board.checkMissionsCompletedForNPC(lastTalkTo.myId.NPCNo);
			if (postOnBoard != null)
			{
				talkToNPC(lastTalkTo, postOnBoard.getPostPostsById().convoOnComplete);
			}
			if (!NPCManager.manage.NPCRequests[lastTalkTo.myId.NPCNo].generatedToday)
			{
				NPCManager.manage.NPCRequests[lastTalkTo.myId.NPCNo].getNewRequest(lastTalkTo.myId.NPCNo);
			}
			if (NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].completedRequest)
			{
				talkToNPC(lastTalkTo, ConversationGenerator.generate.returnConversationFromGroup(ConversationGenerator.generate.NoRequestConversations));
			}
			else if (!NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].acceptedRequest)
			{
				talkToNPC(lastTalkTo, ConversationGenerator.generate.getRequestConversation(lastTalkTo.myId.NPCNo));
			}
			else
			{
				talkToNPC(lastTalkTo, ConversationGenerator.generate.getRequestItemAcceptedConversation());
			}
			requestJob = false;
		}
		if (!conversationLinedUp && !inConversation)
		{
			conversationWindow.gameObject.SetActive(false);
		}
		conversationLinedUp = false;
		Inventory.inv.equipNewSelectedSlot();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	private void performSpecialActionIfFound(Conversation checkConvo, int responseNo = -1)
	{
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.None)
		{
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.SellGivenItems)
		{
			sellGiveItems = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.ReturnItemsInGiveMenu)
		{
			returnGiveItems = true;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenHaircutMenu)
		{
			openHairCutMenu = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenGiveMenu)
		{
			openGiveMenu = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenHairCutColorMenu)
		{
			openHairCutMenuColour = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.ChatToNPCAtWork)
		{
			grabAtWorkConvo = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenCraftsManMenu)
		{
			openCraftMenu = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenTrapperCraftMenu)
		{
			openTrapperCraftMenu = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenGiveMenuToSell)
		{
			openGiveMenuSell = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.StopFollowing)
		{
			stopFollow = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.StartFollowing)
		{
			setNewFollow = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenAnimalMenu)
		{
			openAnimalMenu = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.RepairItems)
		{
			CraftingManager.manage.repairItemsInPockets();
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.ConfirmSleep)
		{
			NetworkMapSharer.share.localChar.myPickUp.confirmSleep();
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenAnimalMenuForPetDog)
		{
			openAnimalMenuForPetDog = true;
			petVariationNo = checkConvo.responesAlt[responseNo].talkingAboutAnimal.animalId;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.AcceptRequest)
		{
			acceptRequest = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.AcceptAndCompleteQuest)
		{
			acceptAndCompleteQuest = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.RequestHouseUpgrade)
		{
			returnExpandHouse = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.AskAboutHouseGeneral)
		{
			askAboutHouseGeneral = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.PlaceDeed)
		{
			NetworkMapSharer.share.localChar.myInteract.doDamage();
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenTownConvo)
		{
			openTownConvo = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.GetRandomRequest)
		{
			requestJob = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.GetRandomConvo)
		{
			grabRandomConvo = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.GetRandomConvo)
		{
			grabRandomConvo = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.RewardForRequest)
		{
			giveReward = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.AcceptQuest)
		{
			acceptQuest = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.ChargeForHouseUpgrade)
		{
			TownManager.manage.payForUpgradeAndSetBuildForTomorrow();
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.DonateToMuseum)
		{
			donatedSomethingAskAgain = true;
			GiveNPC.give.donateItemToMuseum();
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenGiveMenuMuseum)
		{
			openGiveMenuMuseum = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenConstructionBox)
		{
			openConstructionBox = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.GiveCompletedTech)
		{
			giveCompletedTech = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenGiveForSwap)
		{
			openForSwap = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.StartLookingForTechConvo)
		{
			lookingForTechConvo = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenTechDonateWindow)
		{
			openForTechDonate = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.GivePhotoNpc)
		{
			openGivePhoto = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.GivePhotoMuseum)
		{
			openGivePhotoMuseum = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenDeedMenu)
		{
			openDeedCraft = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenGiveForBulletinBoard)
		{
			openGiveBulletinBoard = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.CompleteNpcRequest)
		{
			completeRequest = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.BuyItem)
		{
			tryAndBuyItem = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.CompleteRequestAndGiveReward)
		{
			completeRequest = true;
			giveReward = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.AskToHangOut)
		{
			askToHangOut = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenBankMenu)
		{
			openBankMenu = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenHouseEditor)
		{
			openHouseEditor = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenTownManager)
		{
			openTownManager = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenBuildingManager)
		{
			openBuildingManager = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.AskToMoveHouse)
		{
			startMovingHouseConvo = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenLicenceWindow)
		{
			openLicenceMenu = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenCatalogue)
		{
			openCatalogue = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.OpenDonateWindow)
		{
			openTownDebtPayment = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.AskAboutDeeds)
		{
			askAboutDeeds = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.ConfirmDeedInConvo)
		{
			confirmDeedInConvo = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.DonateTechItems)
		{
			donateTechItems = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.AgreeToCraftsman)
		{
			agreeToCraftsman = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.AgreeToMoveBuilding)
		{
			agreeToMoveBuilding = true;
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.SellItemByWeight)
		{
			GiveNPC.give.sellSellingByWeight();
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.DeclineRequest)
		{
			NPCManager.manage.NPCRequests[lastTalkTo.GetComponent<NPCIdentity>().NPCNo].failRequest(lastTalkTo.GetComponent<NPCIdentity>().NPCNo);
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.DropSwapItem)
		{
			NetworkMapSharer.share.localChar.CmdDropItem(Inventory.inv.getInvItemId(BugAndFishCelebration.bugAndFishCel.invFullConvo.startLineAlt.talkingAboutItem), 1, NetworkMapSharer.share.localChar.transform.position, NetworkMapSharer.share.localChar.myInteract.tileHighlighter.transform.position);
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.GiveTalkedAboutItem)
		{
			if (!Inventory.inv.addItemToInventory(Inventory.inv.getInvItemId(checkConvo.startLineAlt.talkingAboutItem), 1))
			{
				NotificationManager.manage.turnOnPocketsFullNotification();
			}
			return;
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.RewardForBulletinBoard)
		{
			int nPCNo = lastTalkTo.GetComponent<NPCIdentity>().NPCNo;
			PostOnBoard givingPost = GiveNPC.give.givingPost;
			if (givingPost != null)
			{
				if (givingPost.rewardId == Inventory.inv.moneyItem.getItemId())
				{
					GiftedItemWindow.gifted.addToListToBeGiven(givingPost.rewardId, givingPost.rewardAmount);
					GiftedItemWindow.gifted.openWindowAndGiveItems();
					NetworkMapSharer.share.localChar.CmdCompleteBulletinBoardPost(BulletinBoard.board.attachedPosts.IndexOf(givingPost));
					DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CompleteABulletinBoardRequest);
					PermitPointsManager.manage.addPoints(100);
					GiveNPC.give.completeRequest();
					NPCManager.manage.npcStatus[nPCNo].addToRelationshipLevel(Random.Range(4, 7));
					QuestTracker.track.updateTasksEvent.Invoke();
					if (givingPost.isPhotoTask)
					{
						PhotoManager.manage.letNPCKeepPhoto();
					}
				}
				else
				{
					if (givingPost.isTrade)
					{
						InventorySlot inventorySlot = null;
						for (int i = 0; i < Inventory.inv.invSlots.Length; i++)
						{
							if (Inventory.inv.invSlots[i].isSelectedForGive())
							{
								inventorySlot = Inventory.inv.invSlots[i];
								break;
							}
						}
						if ((bool)inventorySlot)
						{
							inventorySlot.updateSlotContentsAndRefresh(inventorySlot.itemNo, inventorySlot.stack - 1);
						}
						GiveNPC.give.clearAllSelectedSlots();
					}
					GiftedItemWindow.gifted.addToListToBeGiven(givingPost.rewardId, givingPost.rewardAmount);
					GiftedItemWindow.gifted.openWindowAndGiveItems();
					NetworkMapSharer.share.localChar.CmdCompleteBulletinBoardPost(BulletinBoard.board.attachedPosts.IndexOf(givingPost));
					DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CompleteABulletinBoardRequest);
					PermitPointsManager.manage.addPoints(100);
					NPCManager.manage.npcStatus[nPCNo].addToRelationshipLevel(Random.Range(4, 7));
					GiveNPC.give.completeRequest();
					if (givingPost.isPhotoTask)
					{
						PhotoManager.manage.letNPCKeepPhoto();
					}
				}
				GiveNPC.give.givingPost = null;
			}
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.DeclineNpcRequestAndRemoveRequest)
		{
			NPCManager.manage.NPCRequests[lastTalkTo.myId.NPCNo].completeRequest(lastTalkTo.myId.NPCNo);
		}
		if (checkConvo.checkSpecialAction(responseNo) == specialAction.StartTeleport)
		{
			if (teledir != "null")
			{
				NetworkMapSharer.share.localChar.CmdTeleport(teledir);
			}
			teledir = "null";
		}
	}

	public string buttonTextChange(Conversation convo, int buttonNo)
	{
		string text = convo.optionNames[buttonNo];
		if (!text.Contains("<"))
		{
			return convo.getOption(buttonNo);
		}
		switch (text)
		{
		case "<chat>":
			switch (Random.Range(0, 4))
			{
			case 0:
				return (LocalizedString)"ToolTips/AskToChat_0";
			case 1:
				return (LocalizedString)"ToolTips/AskToChat_1";
			case 2:
				return (LocalizedString)"ToolTips/AskToChat_2";
			default:
				return (LocalizedString)"ToolTips/AskToChat_3";
			}
		case "<cancel>":
			switch (Random.Range(0, 4))
			{
			case 0:
				return (LocalizedString)"ToolTips/CancelConvo_0";
			case 1:
				return (LocalizedString)"ToolTips/CancelConvo_1";
			case 2:
				return (LocalizedString)"ToolTips/CancelConvo_2";
			default:
				return (LocalizedString)"ToolTips/CancelConvo_3";
			}
		case "<hangOut>":
			switch (Random.Range(0, 2))
			{
			case 0:
				return (LocalizedString)"ToolTips/AskToHangOut_0";
			case 1:
				return (LocalizedString)"ToolTips/AskToHangOut_1";
			default:
				return (LocalizedString)"ToolTips/AskToHangOut_2";
			}
		case "<jobCondition>":
		{
			PostOnBoard postOnBoard = BulletinBoard.board.checkMissionsCompletedForNPC(lastTalkTo.GetComponent<NPCIdentity>().NPCNo);
			if (postOnBoard != null)
			{
				GiveNPC.give.givingPost = postOnBoard;
			}
			if (postOnBoard != null)
			{
				GiveNPC.give.givingPost = postOnBoard;
			}
			if (postOnBoard != null)
			{
				if (Random.Range(0, 2) == 0)
				{
					return (LocalizedString)"ToolTips/CompleteBoardRequest_0";
				}
				return (LocalizedString)"ToolTips/CompleteBoardRequest_1";
			}
			if (NPCManager.manage.npcStatus[lastTalkTo.GetComponent<NPCIdentity>().NPCNo].acceptedRequest)
			{
				if (Random.Range(0, 2) == 0)
				{
					return (LocalizedString)"ToolTips/GiveItem_0";
				}
				return (LocalizedString)"ToolTips/GiveItem_1";
			}
			if (Random.Range(0, 2) == 0)
			{
				return (LocalizedString)"ToolTips/AskForJob_0";
			}
			return (LocalizedString)"ToolTips/AskForJob_1";
		}
		case "<donateButton>":
			return "Donate";
		default:
			return convo.getOption(buttonNo);
		}
	}

	public void setTalkingAboutPhotoId(int newId)
	{
		talkingAboutPhotoId = newId;
	}

	public void doSpecialFunctions(Conversation convosationIn)
	{
		if (giveReward)
		{
			findReward();
			giveReward = false;
		}
		if (completeRequest)
		{
			int nPCNo = lastTalkTo.myId.NPCNo;
			NPCManager.manage.NPCRequests[nPCNo].checkForOtherActionsOnComplete();
			NPCManager.manage.NPCRequests[nPCNo].completeRequest(nPCNo);
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DoAFavourSomeone);
			PermitPointsManager.manage.addPoints(50);
			if (NPCManager.manage.npcStatus[nPCNo].relationshipLevel < 45)
			{
				NPCManager.manage.npcStatus[nPCNo].addToRelationshipLevel(Random.Range(3, 5));
			}
			else
			{
				NPCManager.manage.npcStatus[nPCNo].addToRelationshipLevel(Random.Range(1, 3));
			}
			GiveNPC.give.completeRequest();
			completeRequest = false;
		}
		if (acceptAndCompleteQuest)
		{
			QuestManager.manage.acceptQuestLastTalkedAbout();
			QuestManager.manage.completeQuestLastTalkedAbout();
			acceptAndCompleteQuest = false;
		}
		if (acceptQuest)
		{
			QuestManager.manage.acceptQuestLastTalkedAbout();
			acceptQuest = false;
		}
		if (acceptRequest)
		{
			NPCManager.manage.NPCRequests[lastTalkTo.myId.NPCNo].acceptRequest(lastTalkTo.myId.NPCNo);
			acceptRequest = false;
		}
		if (setNewFollow)
		{
			NetworkMapSharer.share.localChar.CmdNPCStartFollow(lastTalkTo.netId, NetworkMapSharer.share.localChar.netId);
			NetworkMapSharer.share.localChar.followedBy = lastTalkTo.myId.NPCNo;
			setNewFollow = false;
			if (!NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].hasHungOutToday)
			{
				NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].hasHungOutToday = true;
				NPCManager.manage.npcStatus[lastTalkTo.myId.NPCNo].addToRelationshipLevel(2);
			}
		}
		if (stopFollow)
		{
			NetworkMapSharer.share.localChar.CmdNPCStartFollow(lastTalkTo.netId, 0u);
			NetworkMapSharer.share.localChar.followedBy = -1;
			stopFollow = false;
		}
		if (openAnimalMenuForPetDog)
		{
			FarmAnimalMenu.menu.openAnimalMenu(19, petVariationNo);
			openAnimalMenuForPetDog = false;
		}
		if (openAnimalMenu)
		{
			FarmAnimalMenu.menu.openAnimalMenu(GiveNPC.give.dropToBuy.sellsAnimal.animalId);
			openAnimalMenu = false;
		}
		if (openBankMenu)
		{
			BankMenu.menu.open();
			openBankMenu = false;
		}
		if (openHouseEditor)
		{
			HouseEditor.edit.openWindow();
			openHouseEditor = false;
		}
		if (openLicenceMenu)
		{
			LicenceManager.manage.openLicenceWindow();
			openLicenceMenu = false;
		}
		if (openTownManager)
		{
			TownManager.manage.openTownManager(TownManager.windowType.Awards);
			openTownManager = false;
		}
		if (openTownDebtPayment)
		{
			BankMenu.menu.openAsDonations();
			openTownDebtPayment = false;
		}
		if (openGiveMenu)
		{
			GiveNPC.give.OpenGiveWindow();
			openGiveMenu = false;
		}
		if (openGiveMenuSell)
		{
			if (lastTalkTo.myId.NPCNo == 5)
			{
				GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.SellToTrapper);
			}
			else if (lastTalkTo.myId.NPCNo == 11)
			{
				GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.SellToJimmy);
			}
			else
			{
				GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.Sell);
			}
			openGiveMenuSell = false;
		}
		if (openGiveMenuMuseum)
		{
			GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.Museum);
			openGiveMenuMuseum = false;
		}
		if (openForSwap)
		{
			GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.Swapping);
			openForSwap = false;
		}
		if (openForTechDonate)
		{
			GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.Tech);
			openForTechDonate = false;
		}
		if (openConstructionBox)
		{
			donatingToBuilding.openForGivingMenus();
			openConstructionBox = false;
		}
		if (openGiveBulletinBoard)
		{
			GiveNPC.give.OpenGiveWindow(GiveNPC.currentlyGivingTo.BulletinBoard);
			openGiveBulletinBoard = false;
		}
		if (openHairCutMenu || openHairCutMenuColour)
		{
			HairDresserMenu.menu.openHairCutMenu(openHairCutMenuColour);
			openHairCutMenu = false;
			openHairCutMenuColour = false;
		}
		if (openCraftMenu)
		{
			CraftingManager.manage.openCloseCraftMenu(true, CraftingManager.CraftingMenuType.CraftingShop);
			openCraftMenu = false;
		}
		if (openTrapperCraftMenu)
		{
			CraftingManager.manage.openCloseCraftMenu(true, CraftingManager.CraftingMenuType.TrapperShop);
			openTrapperCraftMenu = false;
		}
		if (openGivePhoto)
		{
			PhotoManager.manage.openPhotoTab(true);
			openGivePhoto = false;
		}
		if (openGivePhotoMuseum)
		{
			PhotoManager.manage.openPhotoTab(true, talkingAboutPhotoId);
			talkingAboutPhotoId = -1;
			openGivePhotoMuseum = false;
		}
		if (openDeedCraft)
		{
			DeedManager.manage.openDeedWindow();
			openDeedCraft = false;
		}
		if (openCatalogue)
		{
			if (lastTalkTo.myId.NPCNo == 3)
			{
				CatalogueManager.manage.openCatalogue(CatalogueManager.EntryType.Furniture);
			}
			else
			{
				CatalogueManager.manage.openCatalogue(CatalogueManager.EntryType.Clothing);
			}
			openCatalogue = false;
		}
		if (donateTechItems)
		{
			CraftsmanManager.manage.giveCraftsmanXp();
			GiveNPC.give.donateTechItems();
		}
		if (sellGiveItems)
		{
			GiveNPC.give.sellItemsAndEmptyGiveSlots();
			sellGiveItems = false;
		}
		if (returnGiveItems)
		{
			GiveNPC.give.returnItemsAndEmptyGiveSlots();
			returnGiveItems = false;
		}
	}

	private string checkLineForReplacement(string inString, Conversation convo, int response)
	{
		string text = inString;
		wantsToShowEmotion = 0;
		if (text == null)
		{
			MonoBehaviour.print("String was null");
			return text;
		}
		if (!text.Contains("<"))
		{
			return text;
		}
		text = text.Replace("<PlayerName>", UIAnimationManager.manage.getCharacterNameTag(Inventory.inv.playerName));
		if (text.Contains("<NPCName>"))
		{
			text = ((response == -1) ? text.Replace("<NPCName>", UIAnimationManager.manage.getCharacterNameTag(convo.startLineAlt.talkingAboutNPC.NPCName)) : text.Replace("<NPCName>", UIAnimationManager.manage.getCharacterNameTag(convo.responesAlt[response].talkingAboutNPC.NPCName)));
		}
		if (text.Contains("<getOpeningHours>"))
		{
			text = ((response == -1) ? text.Replace("<getOpeningHours>", convo.startLineAlt.talkingAboutNPC.mySchedual.getOpeningHours()) : text.Replace("<getOpeningHours>", convo.responesAlt[response].talkingAboutNPC.mySchedual.getOpeningHours()));
		}
		if (text.Contains("<getClosedDays>"))
		{
			text = ((response == -1) ? text.Replace("<getClosedDays>", convo.startLineAlt.talkingAboutNPC.mySchedual.getDaysClosed()) : text.Replace("<getClosedDays>", convo.responesAlt[response].talkingAboutNPC.mySchedual.getDaysClosed()));
		}
		if (text.Contains("<getTime>"))
		{
			text = text.Replace("<getTime>", UIAnimationManager.manage.getCharacterNameTag(RealWorldTimeLight.time.currentHour + ":" + RealWorldTimeLight.time.currentMinute.ToString("00")));
		}
		if (text.Contains("<followedByName>"))
		{
			text = text.Replace("<followedByName>", NPCManager.manage.NPCDetails[NetworkMapSharer.share.localChar.followedBy].NPCName);
		}
		if (text.Contains("<IslandName>"))
		{
			text = text.Replace("<IslandName>", UIAnimationManager.manage.getCharacterNameTag(NetworkMapSharer.share.islandName));
		}
		if (text.Contains("<SouthCity>"))
		{
			text = text.Replace("<SouthCity>", UIAnimationManager.manage.getCharacterNameTag("South City"));
		}
		if (text.Contains("<favouriteFood>"))
		{
			text = text.Replace("<favouriteFood>", UIAnimationManager.manage.getItemColorTag(NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].favouriteFood.getInvItemName()));
		}
		if (text.Contains("<hatedFood>"))
		{
			text = text.Replace("<hatedFood>", UIAnimationManager.manage.getItemColorTag(NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].hatedFood.getInvItemName()));
		}
		if (text.Contains("<myName>"))
		{
			text = text.Replace("<myName>", UIAnimationManager.manage.getCharacterNameTag(NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].NPCName));
		}
		if (text.Contains("<FarmingLicence>"))
		{
			text = text.Replace("<FarmingLicence>", UIAnimationManager.manage.getCharacterNameTag(LicenceManager.manage.getLicenceName(LicenceManager.LicenceTypes.Farming)));
		}
		if (text.Contains("<MiningLicence>"))
		{
			text = text.Replace("<MiningLicence>", UIAnimationManager.manage.getCharacterNameTag(LicenceManager.manage.getLicenceName(LicenceManager.LicenceTypes.Mining)));
		}
		if (text.Contains("<AnimalHandlingLicence>"))
		{
			text = text.Replace("<AnimalHandlingLicence>", UIAnimationManager.manage.getCharacterNameTag(LicenceManager.manage.getLicenceName(LicenceManager.LicenceTypes.AnimalHandling)));
		}
		if (text.Contains("<licenceType>"))
		{
			if ((bool)GiveNPC.give.dropToBuy.sellsAnimal)
			{
				text = text.Replace("<licenceType>", UIAnimationManager.manage.getCharacterNameTag(LicenceManager.manage.getLicenceName(LicenceManager.LicenceTypes.AnimalHandling) + " " + GiveNPC.give.dropToBuy.sellsAnimal.GetComponent<FarmAnimal>().getLicenceLevelText()));
			}
			else
			{
				int requiredToBuy = (int)Inventory.inv.allItems[GiveNPC.give.dropToBuy.myItemId].requiredToBuy;
				string licenceLevelText = Inventory.inv.allItems[GiveNPC.give.dropToBuy.myItemId].getLicenceLevelText();
				text = text.Replace("<licenceType>", UIAnimationManager.manage.getCharacterNameTag(licenceLevelText + LicenceManager.manage.getLicenceName((LicenceManager.LicenceTypes)requiredToBuy)));
			}
		}
		if (text.Contains("<itemName>"))
		{
			if (response != -1)
			{
				if ((bool)convo.responesAlt[response].talkingAboutItem)
				{
					text = text.Replace("<itemName>", UIAnimationManager.manage.getItemColorTag(convo.responesAlt[response].talkingAboutItem.getInvItemName()));
				}
			}
			else if ((bool)convo.startLineAlt.talkingAboutItem)
			{
				text = text.Replace("<itemName>", UIAnimationManager.manage.getItemColorTag(convo.startLineAlt.talkingAboutItem.getInvItemName()));
			}
		}
		if (text.Contains("<sellByWeightName>"))
		{
			text = text.Replace("<sellByWeightName>", UIAnimationManager.manage.getItemColorTag(GiveNPC.give.getSellByWeightName()));
		}
		if (text.Contains("<getItemWeight>"))
		{
			text = text.Replace("<getItemWeight>", GiveNPC.give.getItemWeight());
		}
		if (text.Contains("<getItemByWeightPrice>"))
		{
			text = text.Replace("<getItemByWeightPrice>", UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + GiveNPC.give.getSellByWeightMoneyValue().ToString("n0")));
		}
		if (text.Contains("<animalType>"))
		{
			text = ((response == -1) ? text.Replace("<animalType>", UIAnimationManager.manage.getCharacterNameTag(convo.startLineAlt.talkingAboutAnimal.animalName)) : text.Replace("<animalType>", UIAnimationManager.manage.getCharacterNameTag(convo.responesAlt[response].talkingAboutAnimal.animalName)));
		}
		if (text.Contains("<animalName>"))
		{
			text = ((response == -1) ? text.Replace("<animalName>", UIAnimationManager.manage.getCharacterNameTag(convo.startLineAlt.talkingAboutAnimal.animalName)) : text.Replace("<animalName>", UIAnimationManager.manage.getCharacterNameTag(convo.responesAlt[response].talkingAboutAnimal.animalName)));
		}
		if (text.Contains("<donationName>"))
		{
			text = text.Replace("<donationName>", UIAnimationManager.manage.getItemColorTag(GiveNPC.give.getDonationName()));
		}
		if (text.Contains("<givenItem>"))
		{
			InventorySlot inventorySlot = null;
			for (int i = 0; i < Inventory.inv.invSlots.Length; i++)
			{
				if (Inventory.inv.invSlots[i].isSelectedForGive())
				{
					inventorySlot = Inventory.inv.invSlots[i];
					break;
				}
			}
			text = text.Replace("<givenItem>", UIAnimationManager.manage.getItemColorTag(NPCManager.manage.NPCRequests[lastTalkTo.GetComponent<NPCIdentity>().NPCNo].getDesiredItemNameByNumber(inventorySlot.itemNo, inventorySlot.stack)));
		}
		if (text.Contains("<requestItem>"))
		{
			PostOnBoard postOnBoard = BulletinBoard.board.checkMissionsCompletedForNPC(lastTalkTo.GetComponent<NPCIdentity>().NPCNo);
			if (postOnBoard != null)
			{
				GiveNPC.give.givingPost = postOnBoard;
			}
			text = ((postOnBoard == null) ? text.Replace("<requestItem>", UIAnimationManager.manage.getItemColorTag(NPCManager.manage.NPCRequests[lastTalkTo.GetComponent<NPCIdentity>().NPCNo].getDesiredItemName())) : text.Replace("<requestItem>", UIAnimationManager.manage.getItemColorTag(postOnBoard.requireItemAmount + " " + Inventory.inv.allItems[postOnBoard.requiredItem].getInvItemName())));
		}
		if (text.Contains("<buyRequestAmount>"))
		{
			text = text.Replace("<buyRequestAmount>", UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + NPCManager.manage.NPCRequests[lastTalkTo.GetComponent<NPCIdentity>().NPCNo].getDesiredPriceToPay().ToString("n0")));
		}
		if (text.Contains("<requestItemLocation>"))
		{
			PostOnBoard postOnBoard2 = BulletinBoard.board.checkMissionsCompletedForNPC(lastTalkTo.GetComponent<NPCIdentity>().NPCNo);
			if (postOnBoard2 != null)
			{
				GiveNPC.give.givingPost = postOnBoard2;
			}
			if (postOnBoard2 == null)
			{
				text = text.Replace("<requestItemLocation>", NPCManager.manage.NPCRequests[lastTalkTo.GetComponent<NPCIdentity>().NPCNo].itemFoundInLocation);
			}
		}
		if (text.Contains("<marketPlaceNPCName>"))
		{
			text = text.Replace("<marketPlaceNPCName>", UIAnimationManager.manage.getCharacterNameTag(MarketPlaceManager.manage.getCurrentVisitorsName()));
		}
		if (text.Contains("<currentItemInHand>"))
		{
			text = text.Replace("<currentItemInHand>", UIAnimationManager.manage.getItemColorTag(NetworkMapSharer.share.localChar.myEquip.currentlyHolding.getInvItemName()));
		}
		if (text.Contains("<farmAnimalType>"))
		{
			text = text.Replace("<farmAnimalType>", UIAnimationManager.manage.getItemColorTag(ConversationGenerator.generate.returnFarmAnimalType()));
		}
		if (text.Contains("<farmAnimalName>"))
		{
			text = text.Replace("<farmAnimalName>", UIAnimationManager.manage.getItemColorTag(ConversationGenerator.generate.returnFarmAnimalName()));
		}
		if (text.Contains("<randomClothing>"))
		{
			text = text.Replace("<randomClothing>", UIAnimationManager.manage.getItemColorTag(ConversationGenerator.generate.getRandomClothingName()));
		}
		if (text.Contains("<$>"))
		{
			text = text.Replace("<$>", UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + GiveNPC.give.moneyOffer.ToString("n0")));
		}
		if (text.Contains("<Dinks>"))
		{
			text = text.Replace("<Dinks>", UIAnimationManager.manage.moneyAmountColorTag(Inventory.inv.moneyItem.getInvItemName() + "s"));
		}
		if (text.Contains("<Dink>"))
		{
			text = text.Replace("<Dink>", UIAnimationManager.manage.moneyAmountColorTag(Inventory.inv.moneyItem.getInvItemName()));
		}
		if (text.Contains("<Journal>"))
		{
			text = text.Replace("<Journal>", UIAnimationManager.manage.getItemColorTag("Journal"));
		}
		if (text.Contains("<Shift>"))
		{
			text = text.Replace("<Shift>", UIAnimationManager.manage.getCharacterNameTag("Shift"));
		}
		if (text.Contains("<Shifts>"))
		{
			text = text.Replace("<Shifts>", UIAnimationManager.manage.getCharacterNameTag("Shifts"));
		}
		if (text.Contains("<Licence>"))
		{
			text = text.Replace("<Licence>", UIAnimationManager.manage.getItemColorTag("Licence"));
		}
		if (text.Contains("<Licences>"))
		{
			text = text.Replace("<Licences>", UIAnimationManager.manage.getItemColorTag("Licences"));
		}
		if (text.Contains("<CommerceLicence>"))
		{
			text = text.Replace("<CommerceLicence>", UIAnimationManager.manage.getItemColorTag(LicenceManager.manage.getLicenceName(LicenceManager.LicenceTypes.Commerce)));
		}
		if (text.Contains("<FarmingLicence>"))
		{
			text = text.Replace("<CommerceLicence>", UIAnimationManager.manage.getItemColorTag(LicenceManager.manage.getLicenceName(LicenceManager.LicenceTypes.Farming)));
		}
		if (text.Contains("<LoggingLicence>"))
		{
			text = text.Replace("<LoggingLicence>", UIAnimationManager.manage.getItemColorTag(LicenceManager.manage.getLicenceName(LicenceManager.LicenceTypes.Logging)));
		}
		if (text.Contains("<AirShip>"))
		{
			text = text.Replace("<AirShip>", UIAnimationManager.manage.getCharacterNameTag("Airship"));
		}
		if (text.Contains("<Nomad>"))
		{
			text = text.Replace("<Nomad>", UIAnimationManager.manage.getCharacterNameTag("Nomad"));
		}
		if (text.Contains("<Nomads>"))
		{
			text = text.Replace("<Nomads>", UIAnimationManager.manage.getCharacterNameTag("Nomads"));
		}
		if (text.Contains("<BlobFish>"))
		{
			text = text.Replace("<BlobFish>", UIAnimationManager.manage.getItemColorTag(blobFish.getInvItemName()));
		}
		if (text.Contains("<Blobfish>"))
		{
			text = text.Replace("<Blobfish>", UIAnimationManager.manage.getItemColorTag(blobFish.getInvItemName()));
		}
		if (text.Contains("<PermitPoints>"))
		{
			text = text.Replace("<PermitPoints>", UIAnimationManager.manage.pointsAmountColorTag("Permit Points"));
		}
		if (text.Contains("<itemLicence>"))
		{
			InventoryItem inventoryItem = null;
			if (response != -1)
			{
				if ((bool)convo.responesAlt[response].talkingAboutItem)
				{
					inventoryItem = convo.responesAlt[response].talkingAboutItem;
				}
			}
			else if ((bool)convo.startLineAlt.talkingAboutItem)
			{
				inventoryItem = convo.startLineAlt.talkingAboutItem;
			}
			text = ((!(inventoryItem != null)) ? text.Replace("<itemLicence>", UIAnimationManager.manage.getCharacterNameTag("Licence")) : text.Replace("<itemLicence>", UIAnimationManager.manage.getCharacterNameTag(LicenceManager.manage.getLicenceName(inventoryItem.requiredToBuy) + inventoryItem.getLicenceLevelText())));
		}
		if (text.Contains("<requestSellPrice>"))
		{
			text = text.Replace("<requestSellPrice>", UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + NPCManager.manage.NPCRequests[lastTalkTo.GetComponent<NPCIdentity>().NPCNo].getSellPrice().ToString("n0")));
		}
		if (text.Contains("<pointsAmount>"))
		{
			text = text.Replace("<pointsAmount>", UIAnimationManager.manage.pointsAmountColorTag("<sprite=15>" + Mathf.Clamp(GiveNPC.give.moneyOffer / 250, 1f, 1E+11f).ToString("n0")));
		}
		if (text.Contains("<donationReward>"))
		{
			text = text.Replace("<donationReward>", UIAnimationManager.manage.pointsAmountColorTag("<sprite=15>" + 100));
		}
		if (text.Contains("<buyItemName>"))
		{
			text = ((!GiveNPC.give.dropToBuy.sellsAnimal) ? text.Replace("<buyItemName>", UIAnimationManager.manage.getItemColorTag(Inventory.inv.allItems[GiveNPC.give.dropToBuy.myItemId].getInvItemName())) : text.Replace("<buyItemName>", UIAnimationManager.manage.getItemColorTag(GiveNPC.give.dropToBuy.sellsAnimal.animalName)));
		}
		if (text.Contains("<getHousePrice>"))
		{
			text = text.Replace("<getHousePrice>", UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + TownManager.manage.getNextHouseCost().ToString("n0")));
		}
		if (text.Contains("<buyItemPrice>"))
		{
			if (GiveNPC.give.dropToBuy.usesPermitPoints)
			{
				text = text.Replace("<buyItemPrice>", UIAnimationManager.manage.pointsAmountColorTag("<sprite=15>" + (Inventory.inv.allItems[GiveNPC.give.dropToBuy.myItemId].value * 2 / 500).ToString("n0")));
			}
			else if ((bool)GiveNPC.give.dropToBuy.sellsAnimal)
			{
				text = text.Replace("<buyItemPrice>", UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + GiveNPC.give.dropToBuy.sellsAnimal.GetComponent<FarmAnimal>().baseValue.ToString("n0")));
			}
			else if (GiveNPC.give.dropToBuy.recipesOnly)
			{
				text = text.Replace("<buyItemPrice>", UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + GiveNPC.give.dropToBuy.getRecipePrice().ToString("n0")));
			}
			else if (GiveNPC.give.dropToBuy.gives10)
			{
				text = text.Replace("<buyItemPrice>", UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + (Inventory.inv.allItems[GiveNPC.give.dropToBuy.myItemId].value * 2).ToString("n0")));
				showOptionAmountWindow = true;
			}
			else
			{
				text = text.Replace("<buyItemPrice>", UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + (Inventory.inv.allItems[GiveNPC.give.dropToBuy.myItemId].value * 2).ToString("n0")));
			}
		}
		if (text.Contains("<craftItemPrice>"))
		{
			text = text.Replace("<craftItemPrice>", UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + CraftsmanManager.manage.getCraftingPrice().ToString("n0")));
		}
		if (text.Contains("<BulletinBoardReward>"))
		{
			PostOnBoard postOnBoard3 = BulletinBoard.board.checkMissionsCompletedForNPC(lastTalkTo.GetComponent<NPCIdentity>().NPCNo);
			if (postOnBoard3 == null)
			{
				postOnBoard3 = GiveNPC.give.givingPost;
			}
			text = ((postOnBoard3 == null) ? text.Replace("<BulletinBoardReward>", "reward") : ((postOnBoard3.rewardId == Inventory.inv.moneyItem.getItemId()) ? text.Replace("<BulletinBoardReward>", UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + postOnBoard3.rewardAmount.ToString("n0"))) : ((postOnBoard3.rewardAmount <= 1) ? text.Replace("<BulletinBoardReward>", UIAnimationManager.manage.getItemColorTag(Inventory.inv.allItems[postOnBoard3.rewardId].getInvItemName())) : text.Replace("<BulletinBoardReward>", UIAnimationManager.manage.getItemColorTag(postOnBoard3.rewardAmount + " " + Inventory.inv.allItems[postOnBoard3.rewardId].getInvItemName())))));
		}
		if (text.Contains("<requestedHuntTarget>"))
		{
			PostOnBoard postOnBoard4 = BulletinBoard.board.checkMissionsCompletedForNPC(lastTalkTo.GetComponent<NPCIdentity>().NPCNo);
			text = ((postOnBoard4 == null) ? text.Replace("<requestedHuntTarget>", "Animal") : text.Replace("<requestedHuntTarget>", postOnBoard4.getPostPostsById().getBoardHuntRequestAnimal(postOnBoard4.getPostIdOnBoard())));
		}
		if (text.Contains("<getPhotoDetails>"))
		{
			switch (Random.Range(0, 5))
			{
			case 0:
				text = text.Replace("<getPhotoDetails>", "I just love the colours!");
				break;
			case 1:
				text = text.Replace("<getPhotoDetails>", "I love this one.");
				break;
			case 2:
				text = text.Replace("<getPhotoDetails>", "The composition is wonderful");
				break;
			case 3:
				text = text.Replace("<getPhotoDetails>", "It speaks to me, you know?");
				break;
			default:
				text = text.Replace("<getPhotoDetails>", "It makes me feel something...");
				break;
			}
		}
		if (text.Contains("<animalJustBoughtName>"))
		{
			text = text.Replace("<animalJustBoughtName>", UIAnimationManager.manage.getCharacterNameTag(FarmAnimalMenu.menu.getLastAnimalName()));
		}
		if (text.Contains("<buyItemDescription>"))
		{
			if (Inventory.inv.getExtraDetails(GiveNPC.give.dropToBuy.myItemId) != "")
			{
				text = text.Replace("<buyItemDescription>", Inventory.inv.getExtraDetails(GiveNPC.give.dropToBuy.myItemId));
			}
			else if (GiveNPC.give.dropToBuy.furnitureOnly)
			{
				text = ((Random.Range(0, 2) != 0) ? text.Replace("<buyItemDescription>", "Finished that one off today!") : text.Replace("<buyItemDescription>", "Made by hand by yours truly!"));
			}
			else if ((bool)Inventory.inv.allItems[GiveNPC.give.dropToBuy.myItemId].equipable && Inventory.inv.allItems[GiveNPC.give.dropToBuy.myItemId].equipable.cloths)
			{
				switch (Random.Range(0, 9))
				{
				case 0:
					text = text.Replace("<buyItemDescription>", "It feels just right for you, " + UIAnimationManager.manage.getCharacterNameTag(Inventory.inv.playerName) + ".");
					break;
				case 1:
					text = text.Replace("<buyItemDescription>", "The colour is very powerful, y'know?");
					break;
				case 2:
					text = text.Replace("<buyItemDescription>", "It will open your chakras, y'know?");
					break;
				case 4:
					text = text.Replace("<buyItemDescription>", "Do you feel the engery coming from it?");
					break;
				case 5:
					text = text.Replace("<buyItemDescription>", "I feel good things coming to whoever buys it.");
					break;
				case 6:
					text = text.Replace("<buyItemDescription>", "The design just came to me, y'know?");
					break;
				case 7:
					text = text.Replace("<buyItemDescription>", "Y'know, that would look great on you, " + UIAnimationManager.manage.getCharacterNameTag(Inventory.inv.playerName) + ".");
					break;
				default:
					text = text.Replace("<buyItemDescription>", "I put a lot of myself into this one.");
					break;
				}
			}
			else
			{
				text = text.Replace("<buyItemDescription>", Inventory.inv.allItems[GiveNPC.give.dropToBuy.myItemId].getItemDescription(GiveNPC.give.dropToBuy.myItemId));
			}
		}
		if (text.Contains("<deedDebtAmount>"))
		{
			text = text.Replace("<deedDebtAmount>", UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + DeedManager.manage.getDeedCost().ToString("n0")));
		}
		if (text.Contains("<deedName>"))
		{
			text = text.Replace("<deedName>", UIAnimationManager.manage.getItemColorTag(DeedManager.manage.getDeedName()));
		}
		if (text.Contains("<questRequiredItems>"))
		{
			text = text.Replace("<questRequiredItems>", UIAnimationManager.manage.getItemColorTag(QuestManager.manage.listRequiredItemsInQuestLastTalkedAbout()));
		}
		if (text.Contains("<nextDayOff>"))
		{
			text = text.Replace("<nextDayOff>", NPCManager.manage.NPCDetails[lastTalkTo.myId.NPCNo].mySchedual.getNextDayOffName());
		}
		if (text.Contains("<getWeatherPrediction>"))
		{
			text = text.Replace("<getWeatherPrediction>", WeatherManager.manage.tomorrowsWeather(NetworkMapSharer.share.tomorrowsMineSeed));
		}
		if (text.Contains("<getCurrentWeather>"))
		{
			text = text.Replace("<getCurrentWeather>", WeatherManager.manage.currentWeather());
		}
		if (text.Contains("<TownDonateText>"))
		{
			text = text.Replace("<TownDonateText>", UIAnimationManager.manage.moneyAmountColorTag("<sprite=11>" + NetworkMapSharer.share.townDebt.ToString("n0")));
		}
		if (text.Contains("<relocateBuildingName>"))
		{
			text = text.Replace("<relocateBuildingName>", UIAnimationManager.manage.getItemColorTag(BuildingManager.manage.getTalkingAboutBuildingName()));
		}
		if (text.Contains("<haha>"))
		{
			wantsToShowEmotion = 1;
			text = text.Replace("<haha>", "");
		}
		if (text.Contains("<angry>"))
		{
			wantsToShowEmotion = 2;
			text = text.Replace("<angry>", "");
		}
		if (text.Contains("<boohoo>"))
		{
			wantsToShowEmotion = 3;
			text = text.Replace("<boohoo>", "");
		}
		if (text.Contains("<wave>"))
		{
			wantsToShowEmotion = 4;
			text = text.Replace("<wave>", "");
		}
		if (text.Contains("<thinking>"))
		{
			wantsToShowEmotion = 5;
			text = text.Replace("<thinking>", "");
		}
		if (text.Contains("<shocked>"))
		{
			wantsToShowEmotion = 6;
			text = text.Replace("<shocked>", "");
		}
		if (text.Contains("<shock>"))
		{
			wantsToShowEmotion = 6;
			text = text.Replace("<shock>", "");
		}
		if (text.Contains("<pumped>"))
		{
			wantsToShowEmotion = 7;
			text = text.Replace("<pumped>", "");
		}
		if (text.Contains("<glee>"))
		{
			wantsToShowEmotion = 8;
			text = text.Replace("<glee>", "");
		}
		if (text.Contains("<shy>"))
		{
			wantsToShowEmotion = 13;
			text = text.Replace("<shy>", "");
		}
		if (text.Contains("?") && wantsToShowEmotion == 0)
		{
			wantsToShowEmotion = 9;
		}
		if (text.Contains("<yes>"))
		{
			wantsToShowEmotion = 10;
			text = text.Replace("<yes>", "");
		}
		if (text.Contains("<no>"))
		{
			wantsToShowEmotion = 11;
			text = text.Replace("<no>", "");
		}
		if (text.Contains("<proud>"))
		{
			wantsToShowEmotion = 14;
			text = text.Replace("<proud>", "");
		}
		if (text.Contains("<worried>"))
		{
			wantsToShowEmotion = 15;
			text = text.Replace("<worried>", "");
		}
		if (text.Contains("<scared>"))
		{
			wantsToShowEmotion = 15;
			text = text.Replace("<scared>", "");
		}
		if (text.Contains("<sigh>"))
		{
			wantsToShowEmotion = 16;
			text = text.Replace("<sigh>", "");
		}
		if (text.Contains("<dance>"))
		{
			wantsToShowEmotion = 17;
			text = text.Replace("<dance>", "");
		}
		if (text.Contains("<tele-"))
		{
			if (text.Contains("<tele-north>"))
			{
				if (NetworkMapSharer.share.northOn)
				{
					teledir = "north";
					text = text.Replace("<tele-north>", "Beginning...");
				}
				else
				{
					text = text.Replace("<tele-north>", "...Nothing happened...");
				}
			}
			if (text.Contains("<tele-east>"))
			{
				if (NetworkMapSharer.share.eastOn)
				{
					teledir = "east";
					text = text.Replace("<tele-east>", "Beginning...");
				}
				else
				{
					text = text.Replace("<tele-east>", "...Nothing happened...");
				}
			}
			if (text.Contains("<tele-south>"))
			{
				if (NetworkMapSharer.share.southOn)
				{
					teledir = "south";
					text = text.Replace("<tele-south>", "Beginning...");
				}
				else
				{
					text = text.Replace("<tele-south>", "...Nothing happened...");
				}
			}
			if (text.Contains("<tele-west>"))
			{
				if (NetworkMapSharer.share.westOn)
				{
					teledir = "west";
					text = text.Replace("<tele-west>", "Beginning...");
				}
				else
				{
					text = text.Replace("<tele-west>", "...Nothing happened...");
				}
			}
		}
		return text.Replace("\r", "");
	}

	public bool checkPunctuation(string punc)
	{
		switch (punc)
		{
		case "?":
		case "!":
		case ".":
			return true;
		default:
			return false;
		}
	}

	public void updateRelationHeart(int npcNo, bool isSign = false)
	{
		HeartContainer[] array = relationHearts;
		foreach (HeartContainer heartContainer in array)
		{
			heartContainer.gameObject.SetActive(!isSign);
			if (!isSign)
			{
				heartContainer.updateHealth(NPCManager.manage.npcStatus[npcNo].relationshipLevel);
			}
		}
	}

	private IEnumerator spellOutWord(int wordNo)
	{
		for (int i = 0; i < conversationTextPro.textInfo.wordInfo[wordNo].characterCount; i++)
		{
			conversationTextPro.maxVisibleCharacters = 1;
			if (speedUp)
			{
				int num = i % 8;
			}
			if (i % 4 != 0)
			{
				yield return null;
			}
		}
	}

	private void findReward()
	{
		int num = Random.Range(0, 75);
		if (num < 10)
		{
			GiftedItemWindow.gifted.addToListToBeGiven(Inventory.inv.getInvItemId(Inventory.inv.moneyItem), GiveNPC.give.getRewardAmount());
			GiftedItemWindow.gifted.openWindowAndGiveItems();
		}
		else if (num <= 45)
		{
			int randomClothing = RandomObjectGenerator.generate.getRandomClothing(true);
			GiftedItemWindow.gifted.addToListToBeGiven(randomClothing, 1);
			GiftedItemWindow.gifted.openWindowAndGiveItems();
		}
		else if (num <= 60)
		{
			bool flag = false;
			int num2 = 0;
			int num3 = 0;
			while (!flag)
			{
				num2 = Random.Range(0, Inventory.inv.allItems.Length);
				if (!Inventory.inv.allItems[num2].craftable || Inventory.inv.allItems[num2].craftable.workPlaceConditions == CraftingManager.CraftingMenuType.CookingTable || Inventory.inv.allItems[num2].craftable.workPlaceConditions == CraftingManager.CraftingMenuType.CraftingShop || Inventory.inv.allItems[num2].craftable.isDeed || Inventory.inv.allItems[num2].craftable.learnThroughQuest || Inventory.inv.allItems[num2].craftable.learnThroughLevels || Inventory.inv.allItems[num2].craftable.learnThroughLicence || CharLevelManager.manage.checkIfIsInStartingRecipes(num2))
				{
					continue;
				}
				if (!CharLevelManager.manage.checkIfUnlocked(num2) && Inventory.inv.allItems[num2].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.Blocked)
				{
					flag = true;
					continue;
				}
				num3++;
				if (num3 >= 25)
				{
					break;
				}
			}
			if (flag)
			{
				GiftedItemWindow.gifted.addRecipeToUnlock(num2);
				GiftedItemWindow.gifted.openWindowAndGiveItems();
			}
			else
			{
				GiftedItemWindow.gifted.addToListToBeGiven(Inventory.inv.getInvItemId(Inventory.inv.moneyItem), GiveNPC.give.getRewardAmount());
				GiftedItemWindow.gifted.openWindowAndGiveItems();
			}
		}
		else
		{
			int itemId = RandomObjectGenerator.generate.getRandomFurniture(true).getItemId();
			GiftedItemWindow.gifted.addToListToBeGiven(itemId, 1);
			GiftedItemWindow.gifted.openWindowAndGiveItems();
		}
	}

	private IEnumerator readConversationSegment(Conversation conversationIn, int responseNo = -1)
	{
		speedUp = false;
		string[] sayingLines = ((responseNo != -1) ? conversationIn.getResponse(responseNo) : conversationIn.getStartLine());
		currentlyShowingEmotion = 0;
		for (int i = 0; i < sayingLines.Length; i++)
		{
			nextArrowBounce.SetActive(false);
			converstationText.text = "";
			conversationTextPro.text = checkLineForReplacement(sayingLines[i], conversationIn, responseNo);
			conversationTextPro.maxVisibleCharacters = 0;
			if (!lastTalkTo.isSign && (wantsToShowEmotion == 0 || wantsToShowEmotion != currentlyShowingEmotion))
			{
				lastTalkTo.faceAnim.stopEmotions();
			}
			if (i != 0)
			{
				SoundManager.manage.play2DSound(nextTextSound);
				if (!speedUp)
				{
					yield return waitForNext;
				}
			}
			for (int c = 0; c < conversationTextPro.textInfo.characterCount + 1; c++)
			{
				int num = c - 1;
				conversationTextPro.maxVisibleCharacters = c + 1;
				lastTalkTo.playingTalkingAnimation(true);
				if (num == -1 || conversationTextPro.textInfo.characterInfo[num].character != ' ')
				{
					if ((bool)lastTalkTo.faceAnim)
					{
						lastTalkTo.faceAnim.setTriggerTalk();
					}
					if (!lastTalkTo.isSign && OptionsMenu.options.voiceOn)
					{
						tryAndTalk();
					}
					else
					{
						SoundManager.manage.play2DSound(SoundManager.manage.signTalk);
					}
				}
				if (OptionsMenu.options.textSpeed == 1)
				{
					if (c % 2 == 0)
					{
						yield return null;
					}
				}
				else if (OptionsMenu.options.textSpeed == 2)
				{
					if (c % 5 == 0)
					{
						yield return null;
					}
				}
				else if ((!speedUp || c % 2 != 0) && (!speedUp || c % 3 != 0) && (!speedUp || c % 4 != 0))
				{
					yield return null;
				}
				if (speedUp)
				{
					continue;
				}
				if (c + 1 < conversationTextPro.textInfo.characterCount && conversationTextPro.textInfo.characterInfo[c + 1].character == ' ' && conversationTextPro.textInfo.characterInfo[c].character == ',')
				{
					if (speedUp)
					{
						yield return waitForNextSpeedUp;
					}
					else
					{
						yield return waitForNext;
					}
				}
				if (c < conversationTextPro.textInfo.characterCount && conversationTextPro.textInfo.characterInfo[c].character == ' ')
				{
					yield return null;
				}
			}
			if (!lastTalkTo.isSign)
			{
				if (wantsToShowEmotion != 0)
				{
					lastTalkTo.faceAnim.setEmotionNo(wantsToShowEmotion);
				}
				currentlyShowingEmotion = wantsToShowEmotion;
			}
			lastTalkTo.playingTalkingAnimation(false);
			if (responseNo != -1)
			{
				if (speedUp)
				{
					yield return waitForNextSpeedUp;
				}
				else
				{
					yield return waitForNext;
				}
				while (!ready && !clickReadyButton())
				{
					yield return null;
				}
				speedWait = 0f;
				yield return null;
				yield return null;
			}
			else if (responseNo == -1 && (conversationIn.optionNames.Length == 0 || i < sayingLines.Length - 1))
			{
				nextArrowBounce.SetActive(true);
				while (!ready && !clickReadyButton())
				{
					yield return null;
				}
				speedWait = 0f;
				yield return null;
				yield return null;
			}
		}
		performSpecialActionIfFound(conversationIn, responseNo);
	}

	private IEnumerator makeCharactersScaleInOld(int charId)
	{
		conversationTextPro.ForceMeshUpdate();
		TMP_CharacterInfo myCharInfo = conversationTextPro.textInfo.characterInfo[charId];
		for (float time = 0f; time < 1f; time += Time.deltaTime * 2f)
		{
			myCharInfo.scale = Mathf.Lerp(0f, 3f, time);
			conversationTextPro.textInfo.characterInfo[charId] = myCharInfo;
			conversationTextPro.UpdateVertexData();
			yield return null;
		}
	}

	private IEnumerator makeCharactersScaleIn(int charId)
	{
		yield return null;
	}
}
