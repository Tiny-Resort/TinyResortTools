using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtonsTop : MonoBehaviour
{
	public static MenuButtonsTop menu;

	public GameObject subMenuWindow;

	public GameObject subMenuButtonsWindow;

	public GameObject hud;

	public GameObject charWindow;

	public GameObject invCamera;

	public GameObject closeButton;

	public GameObject optionsPage;

	public GameObject confirmQuitWindow;

	public GameObject optionButtonNoJournal;

	private Vector2 originalSize;

	private Vector2 originalPos;

	public bool closed = true;

	private bool windowOpenCheck;

	public bool windowNoTop = true;

	public string UpAxis = "";

	public string B_Button = "B_1";

	public string Y_Button = "Y_1";

	public string A_Button = "A_1";

	public string X_Button = "X_1";

	public string RB_Button = "RB_1";

	public string LB_Button = "LB_1";

	public string StartButton = "Start_1";

	public string Select = "Back_1";

	public string Cancel = "Cancel";

	public string MoveX = "L_XAxis_1";

	public string MoveY = "L_YAxis_1";

	public string Chat = "Chat";

	public string Drop = "Drop";

	public string QuickSlot1 = "QS_1";

	public string QuickSlot2 = "QS_2";

	public string QuickSlot3 = "QS_3";

	public string QuickSlot4 = "QS_4";

	public string DpadUPDown = "DPadUpDown";

	public string DpadLeftRight = "DpadLeftRight";

	public string RightTrigger = "RightTrigger";

	public string LeftTrigger = "LeftTrigger";

	public bool subMenuOpen;

	public bool subMenuJustOpened;

	public Transform questTrackerWindow;

	public Transform pages;

	public GameObject[] disabledOnOpenWhenNoJournal;

	public Sprite controllerMapButton;

	public Sprite keyboardMapButton;

	public Sprite controllerJournal;

	public Sprite keyboardJournal;

	private bool quitToDesktop;

	private void Awake()
	{
		menu = this;
	}

	public void openSubMenu()
	{
		hud.gameObject.SetActive(false);
		subMenuWindow.gameObject.SetActive(true);
		closed = false;
		subMenuOpen = true;
		Inventory.inv.checkIfWindowIsNeeded();
		subMenuJustOpened = true;
		StartCoroutine(subMenuDelay());
		MilestoneManager.manage.updateMilestoneList(true);
		subMenuButtonsWindow.gameObject.SetActive(true);
		CurrencyWindows.currency.openJournal();
		GameObject[] array;
		if (!TownManager.manage.journalUnlocked)
		{
			questTrackerWindow.SetParent(subMenuWindow.transform);
			questTrackerWindow.GetComponent<Image>().enabled = true;
			switchToQuests();
			array = disabledOnOpenWhenNoJournal;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(false);
			}
			optionButtonNoJournal.SetActive(true);
			return;
		}
		NetworkMapSharer.share.localChar.myEquip.setNewLookingAtJournal(true);
		questTrackerWindow.GetComponent<Image>().enabled = false;
		questTrackerWindow.SetParent(pages.transform);
		questTrackerWindow.SetSiblingIndex(2);
		questTrackerWindow.gameObject.SetActive(false);
		optionButtonNoJournal.SetActive(false);
		array = disabledOnOpenWhenNoJournal;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(true);
		}
	}

	public void closeSubMenu()
	{
		NetworkMapSharer.share.localChar.myEquip.setNewLookingAtJournal(false);
		if (MilestoneManager.manage.milestoneClaimWindowOpen)
		{
			MilestoneManager.manage.closeMilestoneClaimWindow();
			return;
		}
		if (PediaManager.manage.entryFullScreenShown)
		{
			PediaManager.manage.closeEntryDetails();
			return;
		}
		if (PhotoManager.manage.photoTabOpen && PhotoManager.manage.blownUpWindow.activeInHierarchy)
		{
			PhotoManager.manage.closeBlownUpWindow();
			return;
		}
		hud.gameObject.SetActive(true);
		subMenuWindow.gameObject.SetActive(false);
		closeWindow();
		closeButtonDelay();
		subMenuOpen = false;
		Inventory.inv.checkIfWindowIsNeeded();
		subMenuJustOpened = true;
		StartCoroutine(subMenuDelay());
		CurrencyWindows.currency.closeJournal();
	}

	public IEnumerator subMenuDelay()
	{
		yield return null;
		subMenuJustOpened = false;
	}

	public void Update()
	{
		if (!NetworkMapSharer.share || !NetworkMapSharer.share.localChar || Inventory.inv.menuOpen || StatusManager.manage.dead)
		{
			return;
		}
		if (GiveNPC.give.giveWindowOpen || ChestWindow.chests.chestWindowOpen || FarmAnimalMenu.menu.farmAnimalMenuOpen || HairDresserMenu.menu.hairMenuOpen || (PhotoManager.manage.photoTabOpen && PhotoManager.manage.isGivingToNPC()) || (CraftingManager.manage.craftMenuOpen && CraftingManager.manage.specialCraftMenu) || CharLevelManager.manage.unlockWindowOpen)
		{
			if (!windowNoTop)
			{
				charWindow.SetActive(false);
				if ((PhotoManager.manage.photoTabOpen && PhotoManager.manage.isGivingToNPC()) || (CraftingManager.manage.craftMenuOpen && CraftingManager.manage.specialCraftMenu) || CharLevelManager.manage.unlockWindowOpen)
				{
					closeButton.SetActive(true);
				}
				else
				{
					closeButton.SetActive(false);
				}
				closed = false;
				windowNoTop = true;
			}
			if (!ChestWindow.chests.chestWindowOpen)
			{
				return;
			}
		}
		else if (windowNoTop)
		{
			closeButton.SetActive(true);
			windowNoTop = false;
		}
		if (InputMaster.input.OpenInventory())
		{
			if (ChestWindow.chests.chestWindowOpen)
			{
				Inventory.inv.invOpen = false;
				Inventory.inv.openAndCloseInv();
				ChestWindow.chests.closeChestInWindow();
				closeButtonDelay();
			}
			else if (!Inventory.inv.invOpen && Inventory.inv.canMoveChar())
			{
				switchToInv();
			}
			else if (Inventory.inv.invOpen)
			{
				closeButtonDelay();
				closeCamera();
				Inventory.inv.invOpen = false;
				Inventory.inv.openAndCloseInv();
			}
		}
		if (!ChatBox.chat.chatOpen && !CheatScript.cheat.cheatMenuOpen)
		{
			if (InputMaster.input.OpenMap() && !RenderMap.map.mapOpen && !subMenuOpen && !subMenuJustOpened && Inventory.inv.canMoveChar())
			{
				switchToMap();
			}
			else if (InputMaster.input.OpenMap() && RenderMap.map.mapOpen && !RenderMap.map.iconSelectorOpen && !RenderMap.map.selectTeleWindowOpen)
			{
				Inventory.inv.pressActiveBackButton();
			}
			else if (InputMaster.input.Journal())
			{
				if (subMenuOpen && !Inventory.inv.usingMouse && !RenderMap.map.mapOpen && !QuestTracker.track.trackerOpen && !PhotoManager.manage.photoTabOpen)
				{
					closeSubMenu();
				}
				else if (!subMenuOpen && !subMenuJustOpened && Inventory.inv.canMoveChar() && closed)
				{
					openSubMenu();
				}
			}
		}
		RenderMap.map.runMapFollow();
	}

	public void switchToInv()
	{
		Inventory.inv.invOpen = true;
		Inventory.inv.openAndCloseInv();
		RenderMap.map.closeTheMap();
		QuestTracker.track.closeQuestWindow();
		PhotoManager.manage.closePhotoTab();
		moveCameraToInvPosition();
		Inventory.inv.checkIfWindowIsNeeded();
		closed = false;
	}

	public void switchToCraft()
	{
		Inventory.inv.invOpen = false;
		Inventory.inv.openAndCloseInv();
		RenderMap.map.closeTheMap();
		QuestTracker.track.closeQuestWindow();
		PhotoManager.manage.closePhotoTab();
		closeCamera();
		closed = false;
	}

	public void switchToQuests()
	{
		swapWindow();
		QuestTracker.track.openQuestWindow();
		closeCamera();
		Inventory.inv.checkIfWindowIsNeeded();
		closed = false;
		subMenuButtonsWindow.gameObject.SetActive(false);
	}

	public void switchToAnimals()
	{
		swapWindow();
		FarmAnimalMenu.menu.openJournalTab();
		closeCamera();
		Inventory.inv.checkIfWindowIsNeeded();
		closed = false;
		subMenuButtonsWindow.gameObject.SetActive(false);
	}

	public void switchToMyDetails()
	{
		swapWindow();
		PlayerDetailManager.manage.openTab();
		closeCamera();
		Inventory.inv.checkIfWindowIsNeeded();
		closed = false;
		subMenuButtonsWindow.gameObject.SetActive(false);
	}

	public void switchToPedia()
	{
		swapWindow();
		PediaManager.manage.openPedia();
		closeCamera();
		Inventory.inv.checkIfWindowIsNeeded();
		closed = false;
		subMenuButtonsWindow.gameObject.SetActive(false);
	}

	public void switchToPhotos()
	{
		swapWindow();
		PhotoManager.manage.openPhotoTab();
		closeCamera();
		Inventory.inv.checkIfWindowIsNeeded();
		closed = false;
		subMenuButtonsWindow.gameObject.SetActive(false);
	}

	public void switchToMap()
	{
		RenderMap.map.refreshMap = true;
		swapWindow();
		RenderMap.map.openTheMap();
		RenderMap.map.changeMapWindow();
		PhotoManager.manage.closePhotoTab();
		Inventory.inv.checkIfWindowIsNeeded();
		closeCamera();
		closed = false;
		subMenuButtonsWindow.gameObject.SetActive(false);
	}

	public void switchToOptions()
	{
		swapWindow();
		optionsPage.SetActive(true);
		confirmQuitWindow.SetActive(false);
		PhotoManager.manage.closePhotoTab();
		Inventory.inv.checkIfWindowIsNeeded();
		closeCamera();
		closed = false;
		subMenuButtonsWindow.gameObject.SetActive(false);
	}

	public void quitToDesktopButton()
	{
		confirmQuitWindow.SetActive(true);
		quitToDesktop = true;
	}

	public void quitToMenuButton()
	{
		confirmQuitWindow.SetActive(true);
		quitToDesktop = false;
	}

	public void ConfirmQuitButton()
	{
		if (quitToDesktop)
		{
			SaveLoad.saveOrLoad.quitGame();
		}
		else
		{
			SaveLoad.saveOrLoad.returnToMenu();
		}
	}

	public void moveCameraToInvPosition()
	{
	}

	public void closeCamera()
	{
	}

	public void swapWindow()
	{
		PlayerDetailManager.manage.closeTab();
		FarmAnimalMenu.menu.closeJournalTab();
		RenderMap.map.closeTheMap();
		closeCamera();
		Inventory.inv.invOpen = false;
		Inventory.inv.openAndCloseInv();
		QuestTracker.track.closeQuestWindow();
		PhotoManager.manage.closePhotoTab();
		CharLevelManager.manage.closeUnlockScreen();
		RenderMap.map.closeTheMap();
		PediaManager.manage.closePedia();
		Inventory.inv.checkIfWindowIsNeeded();
		subMenuButtonsWindow.gameObject.SetActive(true);
		MilestoneManager.manage.closeMilestoneClaimWindow();
		optionsPage.SetActive(false);
		confirmQuitWindow.SetActive(false);
	}

	public void closeWindow()
	{
		PlayerDetailManager.manage.closeTab();
		FarmAnimalMenu.menu.closeJournalTab();
		RenderMap.map.closeTheMap();
		closeCamera();
		Inventory.inv.invOpen = false;
		Inventory.inv.openAndCloseInv();
		QuestTracker.track.closeQuestWindow();
		PhotoManager.manage.closePhotoTab();
		CharLevelManager.manage.closeUnlockScreen();
		RenderMap.map.closeTheMap();
		RenderMap.map.changeMapWindow();
		Inventory.inv.checkIfWindowIsNeeded();
		PediaManager.manage.closePedia();
		closeButtonDelay();
		subMenuButtonsWindow.gameObject.SetActive(true);
		MilestoneManager.manage.closeMilestoneClaimWindow();
		optionsPage.gameObject.SetActive(false);
	}

	public void closeCraftWindow()
	{
		RenderMap.map.closeTheMap();
		closeCamera();
		Inventory.inv.invOpen = false;
		Inventory.inv.openAndCloseInv();
		CraftingManager.manage.openCloseCraftMenu(false);
		QuestTracker.track.closeQuestWindow();
		PhotoManager.manage.closePhotoTab();
		CharLevelManager.manage.closeUnlockScreen();
		RenderMap.map.closeTheMap();
		RenderMap.map.changeMapWindow();
		Inventory.inv.checkIfWindowIsNeeded();
		closeButtonDelay();
		subMenuButtonsWindow.gameObject.SetActive(true);
		MilestoneManager.manage.closeMilestoneClaimWindow();
	}

	public void closeButtonDelay(float delayTime = 0.15f)
	{
		Inventory.inv.checkIfWindowIsNeeded();
		Invoke("closeDelay", delayTime);
	}

	private void closeDelay()
	{
		if (!Inventory.inv.isMenuOpen())
		{
			closed = true;
		}
	}
}
