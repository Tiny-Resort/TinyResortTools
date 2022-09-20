using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
	public bool invOpen;

	public bool menuOpen = true;

	public static Inventory inv;

	public int wallet = 250;

	public InventorySlot walletSlot;

	public InventoryItem moneyItem;

	public InventoryItem minePass;

	private int shownWalletAmount = 250;

	public InventoryItem[] allItems;

	public Transform inventoryWindow;

	public Transform InvDescription;

	public Image descriptionTopBar;

	public Transform invDescriptionFollower;

	public Transform snappingCursor;

	public TextMeshProUGUI InvDescriptionTitle;

	public TextMeshProUGUI InvDescriptionText;

	public Transform quickSlotBar;

	public Transform quickSlotDesc;

	public TileObjectHealthBar tileObjectHealthBar;

	public TextMeshProUGUI quickSlotText;

	public RectTransform cursor;

	public GridLayoutGroup quickSlotsGroup;

	public GameObject inventorySlotPrefab;

	public InventorySlot weaponSlot;

	public InventorySlot dragSlot;

	public InventorySlot[] invSlots;

	public GameObject windowBackgroud;

	public TextMeshProUGUI WalletText;

	public UnityEvent changeControlsEvent = new UnityEvent();

	public string playerName;

	public string islandName;

	public int playerHair;

	public int playerEyes;

	public int playerEyeColor;

	public int playerHairColour;

	public int selectedSlot;

	public int skinTone = 1;

	public int nose;

	public int mouth;

	public EquipItemToChar localChar;

	private int numberOfSlots = 44;

	private int slotPerRow = 11;

	private float cursorSpeed = 10f;

	private bool cursorHovering;

	private bool hoveringOnButton;

	private bool hoveringOnSlot;

	private bool hoveringOnRecipe;

	private Vector3[] lastMousePositions = new Vector3[5];

	private int mousePosIndex;

	public bool usingMouse;

	public AudioSource invAudio;

	private RectTransform canvas;

	public GraphicRaycaster[] casters;

	public InventorySlot craftingRollOverSlot;

	public InventorySlot wallSlot;

	public InventorySlot floorSlot;

	public UIScrollBar activeScrollBar;

	private InvButton activeCloseButton;

	private InvButton lastActiveCloseButton;

	private InvButton activeConfirmButton;

	private InvButton lastActiveConfirmButton;

	private Vector3 desiredPos;

	public List<RectTransform> buttonsToSnapTo;

	public RectTransform currentlySelected;

	public GameObject buttonBackAnimate;

	public GridLayoutGroup invGrid;

	public GridLayoutGroup quickSlotGrid;

	public InventoryItemDescription specialItemDescription;

	public GameObject backButton;

	private bool cursorIsOn = true;

	private InvButton lastRollOver;

	private FillRecipeSlot lastRecipeSlotRollOverDesc;

	private FillRecipeSlot slotRollOver;

	private InventorySlot rollOverSlot;

	private bool coinDropLastTime = true;

	private InventorySlot lastSlotClicked;

	private Coroutine walletChanging;

	private InventorySlot lastRolledOverSlotForDesc;

	private bool quickBarIsLocked;

	public bool snapCursorOn = true;

	private RectTransform lastInvSlotSelected;

	public bool lockCursorSnap;

	[Header("Controller Popup")]
	public GameObject changeControllerPopUp;

	public GameObject controllerPopUp;

	public GameObject keyboardPopUp;

	private void Awake()
	{
		inv = this;
		canvas = GetComponent<RectTransform>();
		buttonsToSnapTo = new List<RectTransform>();
	}

	private void Start()
	{
		Cursor.visible = false;
		checkIfWindowIsNeeded();
		setUpSlots();
		openAndCloseInv();
		dragSlot.updateSlotContentsAndRefresh(dragSlot.itemNo, dragSlot.stack);
		shownWalletAmount = wallet;
		StartCoroutine(showAndHideCursors());
		RenderMap.map.turnOnMapButtonIcon.sprite = MenuButtonsTop.menu.keyboardMapButton;
		MilestoneManager.manage.buttonIcon.sprite = MenuButtonsTop.menu.keyboardJournal;
		balanceSeedPrices();
	}

	public void balanceSeedPrices()
	{
		for (int i = 0; i < allItems.Length; i++)
		{
			if (!inv.allItems[i].placeable || !inv.allItems[i].placeable.tileObjectGrowthStages || !inv.allItems[i].placeable.tileObjectGrowthStages.needsTilledSoil)
			{
				continue;
			}
			if ((bool)inv.allItems[i].placeable.tileObjectGrowthStages.steamsOutInto)
			{
				float num = 2f + (float)((allItems[i].placeable.tileObjectGrowthStages.objectStages.Length + inv.allItems[i].placeable.tileObjectGrowthStages.steamsOutInto.tileObjectGrowthStages.objectStages.Length) / 2);
				inv.allItems[i].placeable.tileObjectGrowthStages.steamsOutInto.tileObjectGrowthStages.harvestDrop.value = Mathf.RoundToInt((float)inv.allItems[i].value * num);
			}
			else if (inv.allItems[i].placeable.tileObjectGrowthStages.diesOnHarvest)
			{
				float num2 = Mathf.RoundToInt(1.85f + (float)allItems[i].placeable.tileObjectGrowthStages.objectStages.Length / 2.3f);
				inv.allItems[i].placeable.tileObjectGrowthStages.harvestDrop.value = Mathf.RoundToInt((float)inv.allItems[i].value * num2 / (float)inv.allItems[i].placeable.tileObjectGrowthStages.harvestSpots.Length);
				if (inv.allItems[i].placeable.tileObjectGrowthStages.growsAllYear())
				{
					inv.allItems[i].placeable.tileObjectGrowthStages.harvestDrop.value = Mathf.RoundToInt((float)inv.allItems[i].placeable.tileObjectGrowthStages.harvestDrop.value / 2.5f);
				}
			}
			else
			{
				inv.allItems[i].placeable.tileObjectGrowthStages.harvestDrop.value = Mathf.RoundToInt((float)inv.allItems[i].value * 1.3f / (float)inv.allItems[i].placeable.tileObjectGrowthStages.harvestSpots.Length);
				if (inv.allItems[i].placeable.tileObjectGrowthStages.growsAllYear())
				{
					inv.allItems[i].placeable.tileObjectGrowthStages.harvestDrop.value = Mathf.RoundToInt((float)inv.allItems[i].placeable.tileObjectGrowthStages.harvestDrop.value / 2.5f);
				}
			}
		}
	}

	public void fillItemTranslations()
	{
		for (int i = 0; i < allItems.Length; i++)
		{
			if ((string)(LocalizedString)("InventoryItemNames/InvItem_" + i) == null)
			{
				LocalizationManager.Sources[0].AddTerm("InventoryItemDescriptions/InvDesc_" + i).Languages[0] = allItems[i].itemDescription;
				LocalizationManager.Sources[0].AddTerm("InventoryItemNames/InvItem_" + i).Languages[0] = allItems[i].itemName;
			}
		}
		LocalizationManager.Sources[0].UpdateDictionary();
	}

	public void setAsActiveCloseButton(InvButton newActive)
	{
		if (activeCloseButton != newActive)
		{
			lastActiveCloseButton = activeCloseButton;
			activeCloseButton = newActive;
		}
		checkIfCloseButtonNeeded();
	}

	public void setAsLastActiveCloseButton(InvButton newActive)
	{
		lastActiveCloseButton = newActive;
		checkIfCloseButtonNeeded();
	}

	public void setAsActiveConfirmButton(InvButton newActive)
	{
		if (activeConfirmButton != newActive)
		{
			lastActiveConfirmButton = activeConfirmButton;
			activeConfirmButton = newActive;
		}
	}

	public void removeAsActiveCloseButton(InvButton deActive)
	{
		if (activeCloseButton == deActive)
		{
			if ((bool)lastActiveCloseButton && lastActiveCloseButton.isActiveAndEnabled)
			{
				activeCloseButton = lastActiveCloseButton;
				lastActiveCloseButton = null;
			}
			else
			{
				activeCloseButton = null;
			}
		}
		checkIfCloseButtonNeeded();
	}

	public void removeAsActiveConfirmButton(InvButton deActive)
	{
		if (activeConfirmButton == deActive)
		{
			if ((bool)lastActiveConfirmButton && lastActiveConfirmButton.isActiveAndEnabled)
			{
				activeConfirmButton = lastActiveConfirmButton;
				lastActiveConfirmButton = null;
			}
			else
			{
				activeConfirmButton = null;
			}
		}
	}

	public void pressActiveBackButton()
	{
		if ((bool)activeCloseButton)
		{
			activeCloseButton.PressButton();
		}
	}

	public void checkIfCloseButtonNeeded()
	{
		if ((bool)activeCloseButton && !invOpen)
		{
			backButton.SetActive(true);
		}
		else
		{
			backButton.SetActive(false);
		}
	}

	private void Update()
	{
		InputMaster.input.UINavigation();
		InputMaster.input.UINavigation();
		if (!Application.isEditor && !OptionsMenu.options.optionWindowOpen)
		{
			if (cursorIsOn)
			{
				Cursor.lockState = CursorLockMode.Confined;
			}
			else
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
		}
		if (usingMouse)
		{
			if (OptionsMenu.options.autoDetectOn && (InputMaster.input.ChangeToController() || InputMaster.input.UINavigation() != Vector2.zero))
			{
				CameraController.control.updateCameraSwitchPrompt(false);
				usingMouse = false;
				RenderMap.map.turnOnMapButtonIcon.sprite = MenuButtonsTop.menu.controllerMapButton;
				MilestoneManager.manage.buttonIcon.sprite = MenuButtonsTop.menu.controllerJournal;
				StartCoroutine(swapControllerPopUp());
				changeControlsEvent.Invoke();
			}
		}
		else if (!usingMouse && InputMaster.input.ChangeToKeyboard())
		{
			CameraController.control.updateCameraSwitchPrompt(true);
			usingMouse = true;
			InputMaster.input.stopRumble();
			cursor.position = InputMaster.input.getMousePos();
			RenderMap.map.turnOnMapButtonIcon.sprite = MenuButtonsTop.menu.keyboardMapButton;
			MilestoneManager.manage.buttonIcon.sprite = MenuButtonsTop.menu.keyboardJournal;
			StartCoroutine(swapControllerPopUp());
			changeControlsEvent.Invoke();
		}
		if (InputMaster.input.UICancel() && (bool)activeCloseButton && activeCloseButton.isActiveAndEnabled && !MenuButtonsTop.menu.subMenuJustOpened)
		{
			activeCloseButton.PressButtonDelay();
		}
		if (InputMaster.input.UISelectActiveConfirmButton() && (bool)activeConfirmButton && activeConfirmButton.isActiveAndEnabled && !MenuButtonsTop.menu.subMenuJustOpened)
		{
			activeConfirmButton.PressButtonDelay();
		}
		if (invOpen)
		{
			if (shownWalletAmount != wallet || wallet != walletSlot.stack)
			{
				wallet = walletSlot.stack;
				if (walletChanging == null)
				{
					walletChanging = StartCoroutine(dealWithWallet());
				}
			}
			Vector3 position = NetworkMapSharer.share.localChar.myInteract.tileHighlighter.transform.position;
			position.y = NetworkMapSharer.share.localChar.transform.position.y;
			Vector3 position2 = NetworkMapSharer.share.localChar.transform.position;
			if (!usingMouse && dragSlot.itemNo != -1 && InputMaster.input.drop() && WorldManager.manageWorld.checkIfDropCanFitOnGround(dragSlot.itemNo, dragSlot.stack, position, NetworkMapSharer.share.localChar.myInteract.insideHouseDetails))
			{
				NetworkMapSharer.share.localChar.CmdDropItem(dragSlot.itemNo, dragSlot.stack, position2, position);
				dragSlot.updateSlotContentsAndRefresh(-1, 0);
				equipNewSelectedSlot();
			}
			else if (!usingMouse && InputMaster.input.drop() && (bool)currentlySelected.GetComponent<InventorySlot>())
			{
				InventorySlot component = currentlySelected.GetComponent<InventorySlot>();
				if (component.itemNo != -1 && WorldManager.manageWorld.checkIfDropCanFitOnGround(component.itemNo, component.stack, position, NetworkMapSharer.share.localChar.myInteract.insideHouseDetails))
				{
					NetworkMapSharer.share.localChar.CmdDropItem(component.itemNo, component.stack, position2, position);
					component.updateSlotContentsAndRefresh(-1, 0);
					equipNewSelectedSlot();
				}
			}
			else if (weaponSlot.itemNo != -1 && WorldManager.manageWorld.checkIfDropCanFitOnGround(weaponSlot.itemNo, weaponSlot.stack, position, NetworkMapSharer.share.localChar.myInteract.insideHouseDetails))
			{
				NetworkMapSharer.share.localChar.CmdDropItem(weaponSlot.itemNo, weaponSlot.stack, position2, position);
				weaponSlot.updateSlotContentsAndRefresh(-1, 0);
				equipNewSelectedSlot();
			}
			else if (weaponSlot.itemNo != -1)
			{
				dragSlot.updateSlotContentsAndRefresh(weaponSlot.itemNo, dragSlot.stack + weaponSlot.stack);
				SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
				weaponSlot.updateSlotContentsAndRefresh(-1, 0);
				equipNewSelectedSlot();
			}
		}
		if (!ChatBox.chat.chatOpen && !StatusManager.manage.dead)
		{
			quickSwitch();
			if ((bool)localChar && InputMaster.input.drop() && canMoveChar() && invSlots[selectedSlot].itemNo != -1)
			{
				Vector3 position3 = NetworkMapSharer.share.localChar.myInteract.tileHighlighter.transform.position;
				position3.y = NetworkMapSharer.share.localChar.transform.position.y;
				Vector3 position4 = NetworkMapSharer.share.localChar.transform.position;
				if (WorldManager.manageWorld.checkIfDropCanFitOnGround(invSlots[selectedSlot].itemNo, invSlots[selectedSlot].stack, position3, NetworkMapSharer.share.localChar.myInteract.insideHouseDetails))
				{
					if (allItems[invSlots[selectedSlot].itemNo].hasFuel || allItems[invSlots[selectedSlot].itemNo].hasColourVariation)
					{
						NetworkMapSharer.share.localChar.CmdDropItem(invSlots[selectedSlot].itemNo, invSlots[selectedSlot].stack, position4, position3);
						invSlots[selectedSlot].updateSlotContentsAndRefresh(-1, 0);
					}
					else if (allItems[invSlots[selectedSlot].itemNo].isATool)
					{
						NetworkMapSharer.share.localChar.CmdDropItem(invSlots[selectedSlot].itemNo, 1, position4, position3);
						invSlots[selectedSlot].updateSlotContentsAndRefresh(-1, 0);
					}
					else
					{
						NetworkMapSharer.share.localChar.CmdDropItem(invSlots[selectedSlot].itemNo, 1, position4, position3);
						invSlots[selectedSlot].updateSlotContentsAndRefresh(invSlots[selectedSlot].itemNo, invSlots[selectedSlot].stack - 1);
					}
					localChar.equipNewItem(invSlots[selectedSlot].itemNo);
				}
				else
				{
					SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
				}
			}
		}
		if ((bool)activeScrollBar)
		{
			float scrollWheel = InputMaster.input.getScrollWheel();
			if (scrollWheel == 0f && !snapCursorOn)
			{
				activeScrollBar.scrollUpOrDown(InputMaster.input.getRightStick().y * 20f);
			}
			else
			{
				activeScrollBar.scrollUpOrDown((0f - scrollWheel) / 5f);
			}
		}
	}

	public void moveCursor()
	{
		if (hoveringOnButton || hoveringOnSlot || hoveringOnRecipe)
		{
			cursorHovering = true;
		}
		else
		{
			cursorHovering = false;
		}
		float num = InputMaster.input.UINavigation().x * cursorSpeed;
		float num2 = InputMaster.input.UINavigation().y * cursorSpeed;
		if (RenderMap.map.mapOpen)
		{
			bool selectTeleWindowOpen = RenderMap.map.selectTeleWindowOpen;
		}
		if (!usingMouse)
		{
			if (num != 0f || num2 != 0f)
			{
				if (cursorHovering)
				{
					cursorSpeed = Mathf.Lerp(cursorSpeed, 10f, Time.deltaTime * 10f);
				}
				else
				{
					cursorSpeed = Mathf.Lerp(cursorSpeed, 15f, Time.deltaTime * 15f);
				}
			}
			else
			{
				cursorSpeed = Mathf.Lerp(cursorSpeed, 8f, Time.deltaTime * 10f);
			}
			if (!ConversationManager.manage.inConversation || !ConversationManager.manage.inOptionScreen)
			{
				if (snapCursorOn)
				{
					if ((bool)currentlySelected && !currentlySelected.gameObject.activeInHierarchy)
					{
						currentlySelected = null;
					}
				}
				else
				{
					desiredPos = Vector3.Lerp(cursor.localPosition, cursor.localPosition + new Vector3(num, num2, 0f) * cursorSpeed, Time.deltaTime * 3f);
					cursor.localPosition = desiredPos;
					cursor.anchoredPosition = new Vector2(Mathf.Clamp(cursor.anchoredPosition.x, 0f + cursor.sizeDelta.x / 2f, canvas.sizeDelta.x - cursor.sizeDelta.x / 2f), Mathf.Clamp(cursor.anchoredPosition.y, 0f + cursor.sizeDelta.y / 2f, canvas.sizeDelta.y - cursor.sizeDelta.y / 2f));
				}
			}
		}
		else if (usingMouse)
		{
			cursor.position = InputMaster.input.getMousePos();
		}
		if (!CraftingManager.manage.craftMenuOpen && !MailManager.manage.mailWindowOpen && !BulletinBoard.board.windowOpen && !QuestTracker.track.trackerOpen && !CharLevelManager.manage.unlockWindowOpen)
		{
			if (dragSlot.itemNo == -1)
			{
				rollOverSlot = cursorRollOver();
				if ((bool)rollOverSlot && rollOverSlot.itemNo != -1)
				{
					fillHoverDescription(rollOverSlot);
					InvDescription.gameObject.SetActive(true);
				}
				else
				{
					lastRolledOverSlotForDesc = null;
					if (!slotRollOver)
					{
						InvDescription.gameObject.SetActive(false);
					}
				}
			}
			else
			{
				rollOverSlot = cursorRollOver();
				if (!slotRollOver)
				{
					InvDescription.gameObject.SetActive(false);
				}
			}
		}
		else
		{
			rollOverSlot = cursorRollOver();
			rollOverSlot = null;
		}
		InvButton invButton = cursorRollOverForButtons();
		if (invButton != lastRollOver)
		{
			if (lastRollOver != null)
			{
				lastRollOver.RollOut();
			}
			lastRollOver = invButton;
		}
		if (isMenuOpen())
		{
			slotRollOver = recipeItemRollOverForButtons();
			if ((bool)slotRollOver)
			{
				if (slotRollOver != lastRecipeSlotRollOverDesc)
				{
					craftingRollOverSlot.itemInSlot = slotRollOver.itemInSlot;
					craftingRollOverSlot.stack = 1;
					lastRolledOverSlotForDesc = null;
					fillHoverDescription(craftingRollOverSlot);
					InvDescription.gameObject.SetActive(true);
				}
				lastRecipeSlotRollOverDesc = slotRollOver;
			}
			else if (!rollOverSlot)
			{
				InvDescription.gameObject.SetActive(false);
				lastRecipeSlotRollOverDesc = null;
			}
		}
		else
		{
			slotRollOver = null;
		}
		if ((bool)invButton)
		{
			invButton.RollOver();
		}
		if (InputMaster.input.Other() || InputMaster.input.OtherKeyboard())
		{
			if (GiveNPC.give.giveWindowOpen)
			{
				return;
			}
			if (ChestWindow.chests.chestWindowOpen)
			{
				InventorySlot inventorySlot = cursorPress();
				if (!inventorySlot || inventorySlot.isDisabledForGive() || inventorySlot.itemNo == -1)
				{
					return;
				}
				bool flag = false;
				for (int i = 0; i < ChestWindow.chests.chestSlots.Length; i++)
				{
					if (inventorySlot == ChestWindow.chests.chestSlots[i] && addItemToInventory(inventorySlot.itemNo, inventorySlot.stack, false))
					{
						inventorySlot.updateSlotContentsAndRefresh(-1, 0);
						SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
						flag = true;
					}
				}
				if (!flag && inventorySlot.chestSlotNo == -1)
				{
					for (int j = 0; j < ChestWindow.chests.chestSlots.Length; j++)
					{
						if (ChestWindow.chests.chestSlots[j].itemNo == inventorySlot.itemNo && allItems[inventorySlot.itemNo].isStackable && !allItems[inventorySlot.itemNo].isATool && !allItems[inventorySlot.itemNo].hasFuel && !allItems[inventorySlot.itemNo].hasColourVariation)
						{
							ChestWindow.chests.chestSlots[j].updateSlotContentsAndRefresh(ChestWindow.chests.chestSlots[j].itemNo, ChestWindow.chests.chestSlots[j].stack + inventorySlot.stack);
							inventorySlot.updateSlotContentsAndRefresh(-1, 0);
							flag = true;
							break;
						}
					}
				}
				if (!flag && inventorySlot.chestSlotNo == -1)
				{
					for (int k = 0; k < ChestWindow.chests.chestSlots.Length; k++)
					{
						if (ChestWindow.chests.chestSlots[k].itemNo == -1)
						{
							swapSlots(ChestWindow.chests.chestSlots[k], inventorySlot);
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					equipNewSelectedSlot();
				}
			}
			else
			{
				if (!invOpen)
				{
					return;
				}
				InventorySlot inventorySlot2 = cursorPress();
				if (dragSlot.itemNo != -1)
				{
					bool flag2 = false;
					for (int l = 0; l < slotPerRow; l++)
					{
						if (invSlots[l].slotUnlocked && invSlots[l].itemNo == -1)
						{
							currentlySelected = invSlots[l].GetComponent<RectTransform>();
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						return;
					}
					int num3 = 0;
					float num4 = 2000f;
					for (int m = 0; m < slotPerRow; m++)
					{
						if (invSlots[m].slotUnlocked)
						{
							float num5 = Vector2.Distance(currentlySelected.transform.position, invSlots[m].transform.position);
							if (num5 < num4)
							{
								num3 = m;
								num4 = num5;
							}
							break;
						}
					}
					currentlySelected = invSlots[num3].GetComponent<RectTransform>();
				}
				else
				{
					if (!inventorySlot2 || inventorySlot2.itemNo == -1)
					{
						return;
					}
					if ((bool)inventorySlot2.itemInSlot.equipable && inventorySlot2.itemInSlot.equipable.cloths)
					{
						if (((bool)inventorySlot2.itemInSlot && inventorySlot2 == EquipWindow.equip.faceSlot) || ((bool)inventorySlot2.itemInSlot && inventorySlot2 == EquipWindow.equip.hatSlot) || ((bool)inventorySlot2.itemInSlot && inventorySlot2 == EquipWindow.equip.shirtSlot) || ((bool)inventorySlot2.itemInSlot && inventorySlot2 == EquipWindow.equip.pantsSlot) || ((bool)inventorySlot2.itemInSlot && inventorySlot2 == EquipWindow.equip.shoeSlot))
						{
							for (int n = 0; n < invSlots.Length; n++)
							{
								if (invSlots[n].slotUnlocked && invSlots[n].itemNo == -1)
								{
									swapSlots(invSlots[n], inventorySlot2);
									equipNewSelectedSlot();
									break;
								}
							}
						}
						else if (inventorySlot2.itemInSlot.equipable.hat)
						{
							swapSlots(EquipWindow.equip.hatSlot, inventorySlot2);
							equipNewSelectedSlot();
						}
						else if (inventorySlot2.itemInSlot.equipable.face)
						{
							swapSlots(EquipWindow.equip.faceSlot, inventorySlot2);
							equipNewSelectedSlot();
						}
						else if (inventorySlot2.itemInSlot.equipable.shirt)
						{
							swapSlots(EquipWindow.equip.shirtSlot, inventorySlot2);
							equipNewSelectedSlot();
						}
						else if (inventorySlot2.itemInSlot.equipable.pants)
						{
							swapSlots(EquipWindow.equip.pantsSlot, inventorySlot2);
							equipNewSelectedSlot();
						}
						else if (inventorySlot2.itemInSlot.equipable.shoes)
						{
							swapSlots(EquipWindow.equip.shoeSlot, inventorySlot2);
							equipNewSelectedSlot();
						}
						return;
					}
					bool flag3 = false;
					bool flag4 = false;
					for (int num6 = 0; num6 < invSlots.Length; num6++)
					{
						if (invSlots[num6] == inventorySlot2)
						{
							if (num6 < slotPerRow)
							{
								flag4 = true;
								break;
							}
							flag3 = true;
						}
					}
					if (flag3)
					{
						for (int num7 = 0; num7 < slotPerRow; num7++)
						{
							if (invSlots[num7].slotUnlocked && invSlots[num7].itemNo == -1)
							{
								swapSlots(invSlots[num7], inventorySlot2);
								equipNewSelectedSlot();
								break;
							}
						}
					}
					if (!flag4)
					{
						return;
					}
					for (int num8 = slotPerRow; num8 < invSlots.Length; num8++)
					{
						if (invSlots[num8].slotUnlocked && invSlots[num8].itemNo == -1)
						{
							swapSlots(invSlots[num8], inventorySlot2);
							equipNewSelectedSlot();
							break;
						}
					}
				}
			}
		}
		else if (InputMaster.input.UISelect())
		{
			InventorySlot inventorySlot3 = cursorPress();
			if ((bool)inventorySlot3)
			{
				if (inventorySlot3.isDisabledForGive())
				{
					SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
				}
				else if (GiveNPC.give.giveWindowOpen && dragSlot.itemNo == -1)
				{
					if (inventorySlot3.itemNo != -1 && !inventorySlot3.isDisabledForGive())
					{
						if (inventorySlot3.isSelectedForGive())
						{
							inventorySlot3.deselectThisSlotForGive();
							SoundManager.manage.play2DSound(SoundManager.manage.deselectSlotForGive);
						}
						else
						{
							inventorySlot3.selectThisSlotForGive();
							SoundManager.manage.play2DSound(SoundManager.manage.selectSlotForGive);
						}
					}
				}
				else if (dragSlot.itemNo == inventorySlot3.itemNo && inventorySlot3.itemNo != -1 && allItems[inventorySlot3.itemNo].checkIfStackable())
				{
					inventorySlot3.updateSlotContentsAndRefresh(inventorySlot3.itemNo, inventorySlot3.stack + dragSlot.stack);
					dragSlot.updateSlotContentsAndRefresh(-1, 0);
					SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
				}
				else
				{
					swapSlots(dragSlot, inventorySlot3);
				}
				equipNewSelectedSlot();
			}
			else
			{
				InvButton invButton2 = cursorPressForButtons();
				if ((bool)invButton2)
				{
					invButton2.PressButton();
				}
			}
		}
		else
		{
			if (!InputMaster.input.UIAlt())
			{
				return;
			}
			InventorySlot inventorySlot4 = cursorPress();
			if (!inventorySlot4)
			{
				return;
			}
			if (GiveNPC.give.giveWindowOpen)
			{
				if (GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.Swapping && inventorySlot4 != weaponSlot && !inventorySlot4.isDisabledForGive())
				{
					inventorySlot4.addGiveAmount();
					StartCoroutine(continueGiveOnHoldDown(inventorySlot4));
				}
			}
			else if ((bool)inventorySlot4 && !inventorySlot4.isDisabledForGive())
			{
				splitSlot(inventorySlot4);
				equipNewSelectedSlot();
			}
			else
			{
				SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
			}
		}
	}

	private IEnumerator dealWithWallet()
	{
		float fillTime = 0f;
		SoundManager.manage.play2DSound(SoundManager.manage.coinsChange);
		while (fillTime <= 1f)
		{
			fillTime += Time.deltaTime / Mathf.Clamp((float)Mathf.Abs(wallet - Mathf.RoundToInt(shownWalletAmount)) / 100f, 0f, 500f);
			shownWalletAmount = Mathf.RoundToInt(Mathf.Lerp(shownWalletAmount, wallet, fillTime));
			WalletText.text = Mathf.RoundToInt(shownWalletAmount).ToString("n0");
			if (!invAudio.isPlaying)
			{
				invAudio.pitch = Random.Range(SoundManager.manage.coinsChange.pitchLow, SoundManager.manage.coinsChange.pitchHigh);
				invAudio.PlayOneShot(SoundManager.manage.coinsChange.myClips[Random.Range(0, SoundManager.manage.coinsChange.myClips.Length)], SoundManager.manage.coinsChange.volume * SoundManager.manage.getUiVolume());
			}
			if (coinDropLastTime)
			{
				SoundManager.manage.play2DSound(SoundManager.manage.coinsChange);
				coinDropLastTime = !coinDropLastTime;
			}
			yield return null;
		}
		shownWalletAmount = wallet;
		WalletText.text = wallet.ToString("n0");
		walletChanging = null;
	}

	public bool isWalletTotalShown()
	{
		return Mathf.RoundToInt(shownWalletAmount) == wallet;
	}

	public void equipNewSelectedSlot()
	{
		if (!localChar)
		{
			return;
		}
		if (selectedSlot < 0)
		{
			selectedSlot = slotPerRow - 1 - (3 - LicenceManager.manage.allLicences[14].getCurrentLevel());
		}
		else if (selectedSlot >= slotPerRow - (3 - LicenceManager.manage.allLicences[14].getCurrentLevel()))
		{
			selectedSlot = 0;
		}
		localChar.equipNewItem(invSlots[selectedSlot].itemNo);
		for (int i = 0; i < slotPerRow; i++)
		{
			if (i != selectedSlot)
			{
				invSlots[i].deselectSlot();
				continue;
			}
			invSlots[i].selectInQuickSlot();
			if (invSlots[selectedSlot].itemNo != -1)
			{
				quickSlotDesc.transform.position = invSlots[i].transform.position + new Vector3(0f, 50f * canvas.localScale.y);
				quickSlotDesc.gameObject.SetActive(false);
				quickSlotText.text = allItems[invSlots[selectedSlot].itemNo].getInvItemName();
				quickSlotDesc.gameObject.SetActive(true);
			}
			else
			{
				quickSlotText.text = "";
				quickSlotDesc.gameObject.SetActive(false);
			}
			checkQuickSlotDesc();
		}
	}

	public void quickSwitch()
	{
		if (!localChar || quickBarIsLocked || ConversationManager.manage.inConversation || !canMoveChar())
		{
			return;
		}
		if (InputMaster.input.invSlotNumberPressed())
		{
			int invSlotNumber = InputMaster.input.getInvSlotNumber();
			if (selectedSlot != invSlotNumber && invSlotNumber <= slotPerRow - 1 - (3 - LicenceManager.manage.allLicences[14].getCurrentLevel()))
			{
				selectedSlot = InputMaster.input.getInvSlotNumber();
				equipNewSelectedSlot();
				SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
			}
		}
		if (InputMaster.input.RB() || InputMaster.input.RBKeyBoard())
		{
			selectedSlot++;
			equipNewSelectedSlot();
			SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
		}
		if (InputMaster.input.LB() || InputMaster.input.LBKeyBoard())
		{
			selectedSlot--;
			equipNewSelectedSlot();
			SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
		}
		if (InputMaster.input.getScrollWheel() / 20f > 0f)
		{
			selectedSlot--;
			equipNewSelectedSlot();
			SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
		}
		if (InputMaster.input.getScrollWheel() < 0f)
		{
			selectedSlot++;
			equipNewSelectedSlot();
			SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
		}
	}

	public bool checkIfItemCanFit(int itemNo, int stackAmount)
	{
		bool flag = false;
		int itemNo2 = invSlots[selectedSlot].itemNo;
		if (allItems[itemNo].checkIfStackable())
		{
			for (int i = 0; i < numberOfSlots; i++)
			{
				if (invSlots[i].slotUnlocked && invSlots[i].itemNo == itemNo)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			for (int j = 0; j < numberOfSlots; j++)
			{
				if (invSlots[j].slotUnlocked && invSlots[j].itemNo == -1)
				{
					flag = true;
					break;
				}
			}
		}
		return flag;
	}

	public bool addItemToInventory(int itemNo, int stackAmount, bool showNotification = true)
	{
		bool flag = false;
		int itemNo2 = invSlots[selectedSlot].itemNo;
		if (itemNo == inv.getInvItemId(moneyItem))
		{
			changeWallet(stackAmount);
			return true;
		}
		if (allItems[itemNo].checkIfStackable())
		{
			for (int i = 0; i < numberOfSlots; i++)
			{
				if (invSlots[i].slotUnlocked && invSlots[i].itemNo == itemNo)
				{
					invSlots[i].updateSlotContentsAndRefresh(itemNo, invSlots[i].stack + stackAmount);
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			for (int j = 0; j < numberOfSlots; j++)
			{
				if (invSlots[j].slotUnlocked && invSlots[j].itemNo == -1)
				{
					invSlots[j].updateSlotContentsAndRefresh(itemNo, stackAmount);
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			if (showNotification)
			{
				NotificationManager.manage.createPickUpNotification(itemNo, stackAmount);
			}
			if (itemNo2 != invSlots[selectedSlot].itemNo)
			{
				equipNewSelectedSlot();
			}
		}
		if (flag)
		{
			QuestTracker.track.updateTasksEvent.Invoke();
			CatalogueManager.manage.pickUpItem(itemNo);
		}
		return flag;
	}

	public int getInvItemId(InventoryItem item)
	{
		if (item == null)
		{
			return -1;
		}
		return item.getItemId();
	}

	public int itemIdBackUp(InventoryItem item)
	{
		for (int i = 0; i < allItems.Length; i++)
		{
			if (allItems[i] == item)
			{
				return i;
			}
		}
		return -1;
	}

	public int getAmountOfItemInAllSlots(int itemId)
	{
		int num = 0;
		for (int i = 0; i < numberOfSlots; i++)
		{
			if (invSlots[i].itemNo == itemId && invSlots[i].itemNo != -1)
			{
				num = ((!allItems[itemId].hasFuel && !allItems[itemId].hasColourVariation) ? (num + invSlots[i].stack) : (num + 1));
			}
		}
		return num;
	}

	public void removeAmountOfItem(int itemId, int amountToRemove)
	{
		int num = 0;
		for (int i = 0; i < numberOfSlots; i++)
		{
			if (num == amountToRemove)
			{
				break;
			}
			if (invSlots[i].itemNo == itemId)
			{
				if (allItems[invSlots[i].itemNo].hasFuel || allItems[invSlots[i].itemNo].hasColourVariation)
				{
					num++;
					invSlots[i].stack = 0;
					invSlots[i].updateSlotContentsAndRefresh(-1, 0);
				}
				else if (invSlots[i].stack < amountToRemove - num)
				{
					num += invSlots[i].stack;
					invSlots[i].stack = 0;
					invSlots[i].updateSlotContentsAndRefresh(invSlots[i].itemNo, invSlots[i].stack);
				}
				else if (invSlots[i].stack >= amountToRemove - num)
				{
					invSlots[i].stack -= amountToRemove - num;
					num += amountToRemove - num;
					invSlots[i].updateSlotContentsAndRefresh(invSlots[i].itemNo, invSlots[i].stack);
				}
			}
		}
	}

	public void splitSlot(InventorySlot slotToSplit)
	{
		if ((bool)slotToSplit.equipSlot)
		{
			SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
		}
		else if (slotToSplit.itemNo != -1 && dragSlot.itemNo == -1)
		{
			if (slotToSplit.chestSlotNo == -1)
			{
				lastSlotClicked = slotToSplit;
			}
			if (slotToSplit.stack != 1 && !allItems[slotToSplit.itemNo].hasFuel && !allItems[slotToSplit.itemNo].hasColourVariation)
			{
				int num = slotToSplit.stack / 2;
				slotToSplit.updateSlotContentsAndRefresh(slotToSplit.itemNo, slotToSplit.stack - num);
				dragSlot.updateSlotContentsAndRefresh(slotToSplit.itemNo, num);
			}
			else
			{
				swapSlots(slotToSplit, dragSlot);
			}
			SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
		}
		else if (slotToSplit.itemNo != -1 && dragSlot.itemNo == slotToSplit.itemNo && allItems[slotToSplit.itemNo].isStackable && !allItems[slotToSplit.itemNo].isATool && slotToSplit.itemNo != -1 && !allItems[slotToSplit.itemNo].hasColourVariation && !allItems[slotToSplit.itemNo].hasFuel)
		{
			slotToSplit.updateSlotContentsAndRefresh(dragSlot.itemNo, slotToSplit.stack + 1);
			dragSlot.updateSlotContentsAndRefresh(dragSlot.itemNo, dragSlot.stack - 1);
			SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
		}
		else if (slotToSplit.itemNo == -1 && dragSlot.itemNo != -1)
		{
			if (!allItems[dragSlot.itemNo].hasFuel && !allItems[dragSlot.itemNo].hasColourVariation)
			{
				slotToSplit.updateSlotContentsAndRefresh(dragSlot.itemNo, 1);
				dragSlot.updateSlotContentsAndRefresh(dragSlot.itemNo, dragSlot.stack - 1);
			}
			else
			{
				swapSlots(slotToSplit, dragSlot);
			}
			SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
		}
	}

	public void swapSlots(InventorySlot firstSlot, InventorySlot secondSlot)
	{
		if ((bool)secondSlot.equipSlot && !secondSlot.equipSlot.canEquipInThisSlot(firstSlot.itemNo))
		{
			SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
			return;
		}
		if ((bool)firstSlot.equipSlot && !firstSlot.equipSlot.canEquipInThisSlot(secondSlot.itemNo))
		{
			SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
			return;
		}
		if (firstSlot == dragSlot)
		{
			if (!secondSlot.equipSlot && secondSlot.chestSlotNo == 1)
			{
				lastSlotClicked = secondSlot;
			}
			else
			{
				lastSlotClicked = null;
			}
		}
		if (secondSlot == dragSlot)
		{
			if (!firstSlot.equipSlot && firstSlot.chestSlotNo == 1)
			{
				lastSlotClicked = firstSlot;
			}
			else
			{
				lastSlotClicked = null;
			}
		}
		int[] array = new int[2] { firstSlot.itemNo, firstSlot.stack };
		int[] array2 = new int[2] { secondSlot.itemNo, secondSlot.stack };
		if (array[0] != -1 || array2[0] != -1)
		{
			SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
		}
		firstSlot.updateSlotContentsAndRefresh(array2[0], array2[1]);
		secondSlot.updateSlotContentsAndRefresh(array[0], array[1]);
	}

	public void setUpSlots()
	{
		invSlots = new InventorySlot[numberOfSlots];
		for (int i = 0; i < numberOfSlots; i++)
		{
			if (i < slotPerRow)
			{
				invSlots[i] = Object.Instantiate(inventorySlotPrefab, quickSlotBar).GetComponent<InventorySlot>();
			}
			else
			{
				invSlots[i] = Object.Instantiate(inventorySlotPrefab, inventoryWindow).GetComponent<InventorySlot>();
			}
			invSlots[i].refreshSlot();
		}
		setSlotsUnlocked();
	}

	public void setSlotsUnlocked(bool updateBelt = false)
	{
		for (int i = 0; i < numberOfSlots; i++)
		{
			if (i < slotPerRow)
			{
				if (i <= slotPerRow - (4 - LicenceManager.manage.allLicences[14].getCurrentLevel()))
				{
					invSlots[i].hideSlot(true);
					invSlots[i].slotUnlocked = true;
				}
				else
				{
					invSlots[i].slotUnlocked = false;
				}
			}
			else if (i <= invSlots.Length - (10 - LicenceManager.manage.allLicences[12].getCurrentLevel() * 3))
			{
				invSlots[i].slotUnlocked = true;
			}
			else
			{
				invSlots[i].slotUnlocked = false;
			}
		}
		if (updateBelt)
		{
			for (int j = 0; j < numberOfSlots; j++)
			{
				if (j < slotPerRow)
				{
					if (j <= slotPerRow - (4 - LicenceManager.manage.allLicences[14].getCurrentLevel()))
					{
						invSlots[j].hideSlot(true);
						invSlots[j].refreshSlot(false);
					}
					else
					{
						invSlots[j].hideSlot(false);
					}
				}
				else if (j <= invSlots.Length - (10 - LicenceManager.manage.allLicences[12].getCurrentLevel() * 3))
				{
					invSlots[j].hideSlot(invOpen);
					invSlots[j].refreshSlot(false);
				}
				else
				{
					invSlots[j].hideSlot(false);
				}
			}
		}
		invGrid.constraintCount = 8 + LicenceManager.manage.allLicences[12].getCurrentLevel();
		quickSlotGrid.constraintCount = 8 + LicenceManager.manage.allLicences[14].getCurrentLevel();
	}

	public void OpenInvForGive()
	{
		invOpen = true;
		openAndCloseInv();
	}

	public void openAndCloseInv()
	{
		for (int i = 0; i < numberOfSlots; i++)
		{
			if (i < slotPerRow)
			{
				if (i <= slotPerRow - (4 - LicenceManager.manage.allLicences[14].getCurrentLevel()))
				{
					invSlots[i].hideSlot(true);
					invSlots[i].refreshSlot(false);
				}
				else
				{
					invSlots[i].hideSlot(false);
				}
			}
			else if (i <= invSlots.Length - (10 - LicenceManager.manage.allLicences[12].getCurrentLevel() * 3))
			{
				invSlots[i].hideSlot(invOpen);
				invSlots[i].refreshSlot(false);
			}
			else
			{
				invSlots[i].hideSlot(false);
			}
		}
		invGrid.constraintCount = 8 + LicenceManager.manage.allLicences[12].getCurrentLevel();
		quickSlotGrid.constraintCount = 8 + LicenceManager.manage.allLicences[14].getCurrentLevel();
		walletSlot.hideSlot(false);
		weaponSlot.hideSlot(invOpen);
		weaponSlot.refreshSlot();
		cursor.gameObject.SetActive(invOpen);
		if (invOpen)
		{
			quickSlotBar.localScale = new Vector3(1f, 1f, 1f);
		}
		else
		{
			quickSlotBar.localScale = new Vector3(0.75f, 0.75f, 0.75f);
		}
		checkQuickSlotDesc();
		quickSlotBar.gameObject.SetActive(false);
		quickSlotBar.gameObject.SetActive(true);
		checkIfWindowIsNeeded();
		if (invOpen)
		{
			CurrencyWindows.currency.openInv();
			EquipWindow.equip.openEquipWindow();
		}
		else
		{
			CurrencyWindows.currency.closeInv();
		}
		if ((bool)localChar)
		{
			if (invOpen)
			{
				localChar.CmdOpenBag();
			}
			else
			{
				localChar.CmdCloseBag();
			}
		}
		if (invOpen && snapCursorOn && lastInvSlotSelected != null)
		{
			setCurrentlySelectedAndMoveCursor(lastInvSlotSelected);
		}
	}

	public void changeItemInHand()
	{
		invSlots[selectedSlot].itemNo = getInvItemId(invSlots[selectedSlot].itemInSlot.changeToWhenUsed);
		invSlots[selectedSlot].refreshSlot();
		localChar.equipNewItem(invSlots[selectedSlot].itemNo);
	}

	public void changeToFullItem()
	{
		invSlots[selectedSlot].updateSlotContentsAndRefresh(getInvItemId(invSlots[selectedSlot].itemInSlot.changeToWhenUsed), invSlots[selectedSlot].itemInSlot.changeToWhenUsed.fuelMax);
		localChar.equipNewItem(invSlots[selectedSlot].itemNo);
	}

	public void fillFuelInItem()
	{
		invSlots[selectedSlot].updateSlotContentsAndRefresh(invSlots[selectedSlot].itemNo, invSlots[selectedSlot].itemInSlot.fuelMax);
		localChar.equipNewItem(invSlots[selectedSlot].itemNo);
	}

	public void consumeItemInHand()
	{
		if (invSlots[selectedSlot].itemNo >= 0 && inv.allItems[invSlots[selectedSlot].itemNo].hasColourVariation)
		{
			invSlots[selectedSlot].updateSlotContentsAndRefresh(-1, 0);
		}
		else
		{
			invSlots[selectedSlot].stack--;
		}
		invSlots[selectedSlot].refreshSlot();
		if (invSlots[selectedSlot].stack <= 0)
		{
			localChar.equipNewItem(invSlots[selectedSlot].itemNo);
		}
		equipNewSelectedSlot();
	}

	public void placeItemIntoSomething(InventoryItem itemToConsume, ItemDepositAndChanger changerToPlaceInto)
	{
		removeAmountOfItem(getInvItemId(itemToConsume), changerToPlaceInto.returnAmountNeeded(itemToConsume));
		invSlots[selectedSlot].refreshSlot();
		if (invSlots[selectedSlot].stack == 0)
		{
			localChar.equipNewItem(invSlots[selectedSlot].itemNo);
		}
	}

	public void useItemWithFuel()
	{
		if (invSlots[selectedSlot].itemNo < 0 || !allItems[invSlots[selectedSlot].itemNo].hasFuel)
		{
			return;
		}
		invSlots[selectedSlot].updateSlotContentsAndRefresh(invSlots[selectedSlot].itemNo, Mathf.Clamp(invSlots[selectedSlot].stack - allItems[invSlots[selectedSlot].itemNo].fuelOnUse, 0, allItems[invSlots[selectedSlot].itemNo].fuelMax));
		if (invSlots[selectedSlot].stack == 0)
		{
			if (!allItems[invSlots[selectedSlot].itemNo].changeToWhenUsed || allItems[invSlots[selectedSlot].itemNo].changeToAndStillUseFuel)
			{
				localChar.CmdBrokenItem();
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BreakATool);
				invSlots[selectedSlot].updateSlotContentsAndRefresh(-1, 0);
				inv.checkQuickSlotDesc();
			}
			else
			{
				invSlots[selectedSlot].updateSlotContentsAndRefresh(getInvItemId(allItems[invSlots[selectedSlot].itemNo].changeToWhenUsed), 1);
			}
			localChar.equipNewItem(invSlots[selectedSlot].itemNo);
		}
	}

	public bool hasFuelAndCanBeUsed()
	{
		if (allItems[invSlots[selectedSlot].itemNo].hasFuel)
		{
			return true;
		}
		return false;
	}

	public void changeWallet(int dif, bool addToTownEconomy = true)
	{
		if (walletSlot.itemNo == -1)
		{
			walletSlot.updateSlotContentsAndRefresh(getInvItemId(moneyItem), dif);
		}
		else
		{
			walletSlot.updateSlotContentsAndRefresh(getInvItemId(moneyItem), walletSlot.stack + dif);
		}
		if (addToTownEconomy && dif < 0)
		{
			TownManager.manage.moneySpentInTownTotal += Mathf.Abs(dif);
		}
		wallet = walletSlot.stack;
		if (wallet < 0)
		{
			wallet = 0;
		}
		if (walletChanging == null)
		{
			walletChanging = StartCoroutine(dealWithWallet());
		}
		CurrencyWindows.currency.checkIfMoneyBoxNeeded();
	}

	public void changeWalletToLoad(int loadAmount)
	{
		walletSlot.updateSlotContentsAndRefresh(getInvItemId(moneyItem), loadAmount);
		shownWalletAmount = loadAmount;
		WalletText.text = loadAmount.ToString("n0");
		wallet = loadAmount;
	}

	public InventorySlot cursorPress()
	{
		PointerEventData pointerEventData = new PointerEventData(null);
		pointerEventData.position = cursor.position;
		List<RaycastResult> list = new List<RaycastResult>();
		for (int num = casters.Length - 1; num >= 0; num--)
		{
			if (casters[num].enabled)
			{
				casters[num].Raycast(pointerEventData, list);
				if (list.Count > 0)
				{
					return list[0].gameObject.GetComponent<InventorySlot>();
				}
			}
		}
		return null;
	}

	public InvButton cursorPressForButtons()
	{
		PointerEventData pointerEventData = new PointerEventData(null);
		pointerEventData.position = cursor.position;
		List<RaycastResult> list = new List<RaycastResult>();
		for (int num = casters.Length - 1; num >= 0; num--)
		{
			if (casters[num].enabled)
			{
				casters[num].Raycast(pointerEventData, list);
				if (list.Count > 0)
				{
					return list[0].gameObject.GetComponent<InvButton>();
				}
			}
		}
		return null;
	}

	public InvButton cursorRollOverForButtons()
	{
		PointerEventData pointerEventData = new PointerEventData(null);
		pointerEventData.position = cursor.position;
		List<RaycastResult> list = new List<RaycastResult>();
		for (int num = casters.Length - 1; num >= 0; num--)
		{
			if (casters[num].enabled)
			{
				casters[num].Raycast(pointerEventData, list);
				if (list.Count > 0)
				{
					InvButton component = list[0].gameObject.GetComponent<InvButton>();
					hoveringOnButton = component;
					return component;
				}
			}
		}
		hoveringOnButton = false;
		return null;
	}

	public FillRecipeSlot recipeItemRollOverForButtons()
	{
		PointerEventData pointerEventData = new PointerEventData(null);
		pointerEventData.position = cursor.position;
		List<RaycastResult> list = new List<RaycastResult>();
		for (int num = casters.Length - 1; num >= 0; num--)
		{
			if (casters[num].enabled)
			{
				casters[num].Raycast(pointerEventData, list);
				if (list.Count > 0)
				{
					FillRecipeSlot component = list[0].gameObject.GetComponent<FillRecipeSlot>();
					hoveringOnRecipe = component;
					return component;
				}
			}
		}
		hoveringOnRecipe = false;
		return null;
	}

	public InventorySlot cursorRollOver()
	{
		PointerEventData pointerEventData = new PointerEventData(null);
		pointerEventData.position = cursor.position;
		List<RaycastResult> list = new List<RaycastResult>();
		for (int num = casters.Length - 1; num >= 0; num--)
		{
			if (casters[num].enabled)
			{
				casters[num].Raycast(pointerEventData, list);
				if (list.Count > 0)
				{
					InventorySlot component = list[0].gameObject.GetComponent<InventorySlot>();
					if (component != weaponSlot)
					{
						hoveringOnSlot = component;
					}
					else
					{
						hoveringOnSlot = false;
					}
					return component;
				}
			}
		}
		hoveringOnSlot = false;
		return null;
	}

	public bool canBeSelected(RectTransform trans, bool ignoreScrollBox = false)
	{
		if (!trans || !currentlySelected)
		{
			return false;
		}
		if (!ignoreScrollBox && (bool)activeScrollBar && currentlySelected.IsChildOf(activeScrollBar.windowThatScrolls) && trans.IsChildOf(activeScrollBar.windowThatScrolls))
		{
			return true;
		}
		PointerEventData pointerEventData = new PointerEventData(null);
		pointerEventData.position = trans.position + (Vector3)trans.rect.center;
		List<RaycastResult> list = new List<RaycastResult>();
		for (int num = casters.Length - 1; num >= 0; num--)
		{
			if (casters[num].enabled)
			{
				casters[num].Raycast(pointerEventData, list);
				if (list.Count > 0 && list[0].gameObject == trans.gameObject)
				{
					return true;
				}
				Vector3[] array = new Vector3[4];
				Vector3[] array2 = new Vector3[4]
				{
					new Vector3(1f, 1f, 0f),
					new Vector3(1f, -1f, 0f),
					new Vector3(-1f, -1f, 0f),
					new Vector3(-1f, 1f, 0f)
				};
				trans.GetWorldCorners(array);
				for (int i = 0; i < array.Length; i++)
				{
					pointerEventData.position = array[i] + array2[i];
					casters[num].Raycast(pointerEventData, list);
					if (list.Count > 0 && list[0].gameObject == trans.gameObject)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void fillHoverDescription(InventorySlot rollOverSlot)
	{
		if (rollOverSlot != lastRolledOverSlotForDesc && (bool)rollOverSlot.itemInSlot)
		{
			descriptionTopBar.color = UIAnimationManager.manage.getSlotColour(rollOverSlot.itemNo);
			string text = rollOverSlot.itemInSlot.getItemDescription(getInvItemId(rollOverSlot.itemInSlot));
			if (rollOverSlot.itemInSlot.isFurniture)
			{
				text = "";
			}
			InvDescriptionTitle.text = rollOverSlot.itemInSlot.getInvItemName();
			InvDescriptionText.text = text;
			specialItemDescription.fillItemDescription(rollOverSlot.itemInSlot);
		}
		lastRolledOverSlotForDesc = rollOverSlot;
	}

	public string getExtraDetails(int itemId)
	{
		string text = "";
		if ((bool)allItems[itemId].placeable && (bool)allItems[itemId].placeable.tileObjectGrowthStages && !allItems[itemId].consumeable)
		{
			string text2 = "";
			if (allItems[itemId].placeable.tileObjectGrowthStages.growsInSummer && allItems[itemId].placeable.tileObjectGrowthStages.growsInWinter && allItems[itemId].placeable.tileObjectGrowthStages.growsInSpring && allItems[itemId].placeable.tileObjectGrowthStages.growsInAutum)
			{
				text2 = "all year";
			}
			else
			{
				text2 += "during ";
				if (allItems[itemId].placeable.tileObjectGrowthStages.growsInSummer)
				{
					text2 += "Summer";
				}
				if (allItems[itemId].placeable.tileObjectGrowthStages.growsInAutum)
				{
					if (text2 != "during ")
					{
						text2 += " & ";
					}
					text2 += "Autum";
				}
				if (allItems[itemId].placeable.tileObjectGrowthStages.growsInWinter)
				{
					if (text2 != "during ")
					{
						text2 += " & ";
					}
					text2 += "Winter";
				}
				if (allItems[itemId].placeable.tileObjectGrowthStages.growsInSpring)
				{
					if (text2 != "during ")
					{
						text2 += " & ";
					}
					text2 += "Spring";
				}
			}
			if (allItems[itemId].placeable.tileObjectGrowthStages.needsTilledSoil)
			{
				text = text + "These grow " + text2 + ". ";
			}
			if (allItems[itemId].placeable.tileObjectGrowthStages.objectStages.Length != 0)
			{
				text = ((!allItems[itemId].placeable.tileObjectGrowthStages.steamsOutInto) ? (text + "They grow over " + allItems[itemId].placeable.tileObjectGrowthStages.objectStages.Length + " days. They produce " + allItems[itemId].placeable.tileObjectGrowthStages.harvestSpots.Length + " " + allItems[itemId].placeable.tileObjectGrowthStages.harvestDrop.itemName) : (text + "These need a bit of room around them because they will spread to nearby spaces producing <b>" + allItems[itemId].placeable.tileObjectGrowthStages.steamsOutInto.tileObjectGrowthStages.harvestSpots.Length + " " + allItems[itemId].placeable.tileObjectGrowthStages.steamsOutInto.tileObjectGrowthStages.harvestDrop.itemName + "s</b>. There is a chance the plant can have up to 4 offshoots! "));
			}
			text = ((allItems[itemId].placeable.tileObjectGrowthStages.harvestSpots.Length <= 1) ? (text + ".") : (text + "s."));
			if (!allItems[itemId].placeable.tileObjectGrowthStages.diesOnHarvest && !allItems[itemId].placeable.tileObjectGrowthStages.steamsOutInto)
			{
				text = text + " They continue to produce " + allItems[itemId].placeable.tileObjectGrowthStages.harvestSpots.Length + " " + allItems[itemId].placeable.tileObjectGrowthStages.harvestDrop.itemName + "s every " + Mathf.Abs(allItems[itemId].placeable.tileObjectGrowthStages.takeOrAddFromStateOnHarvest) + " days. ";
			}
			if (!WorldManager.manageWorld.allObjectSettings[allItems[itemId].placeable.tileObjectId].walkable)
			{
				text += "Oh, and these will need a plant stake. ";
			}
		}
		return text;
	}

	public void checkIfWindowIsNeeded()
	{
		if (!invOpen && dragSlot.itemNo != -1)
		{
			if (((bool)lastSlotClicked && lastSlotClicked.itemNo == -1) || ((bool)lastSlotClicked && lastSlotClicked.itemNo == dragSlot.itemNo))
			{
				if (lastSlotClicked.itemNo == dragSlot.itemNo)
				{
					lastSlotClicked.itemNo = dragSlot.itemNo;
					lastSlotClicked.stack += dragSlot.stack;
					dragSlot.updateSlotContentsAndRefresh(-1, 0);
					lastSlotClicked.updateSlotContentsAndRefresh(lastSlotClicked.itemNo, lastSlotClicked.stack);
				}
				else
				{
					swapSlots(dragSlot, lastSlotClicked);
				}
			}
			else if (addItemToInventory(dragSlot.itemNo, dragSlot.stack, false))
			{
				dragSlot.updateSlotContentsAndRefresh(-1, 0);
				equipNewSelectedSlot();
			}
			else
			{
				Vector3 position = NetworkMapSharer.share.localChar.myInteract.tileHighlighter.transform.position;
				if (WorldManager.manageWorld.checkIfDropCanFitOnGround(dragSlot.itemNo, dragSlot.stack, position, NetworkMapSharer.share.localChar.myInteract.insideHouseDetails))
				{
					NetworkMapSharer.share.localChar.CmdDropItem(dragSlot.itemNo, dragSlot.stack, NetworkMapSharer.share.localChar.transform.position, NetworkMapSharer.share.localChar.myInteract.tileHighlighter.transform.position);
					dragSlot.updateSlotContentsAndRefresh(-1, 0);
					equipNewSelectedSlot();
				}
			}
		}
		if (isMenuOpen())
		{
			if (invOpen)
			{
				windowBackgroud.SetActive(true);
				if (ChestWindow.chests.chestWindowOpen)
				{
					StatusManager.manage.staminaAndHealthBarOn(false);
				}
			}
			else
			{
				StatusManager.manage.staminaAndHealthBarOn(false);
				windowBackgroud.SetActive(false);
			}
			turnCursorOnOrOff(true);
			cursor.gameObject.SetActive(true);
		}
		else
		{
			StatusManager.manage.staminaAndHealthBarOn(true);
			turnCursorOnOrOff(false);
			windowBackgroud.SetActive(false);
			cursor.gameObject.SetActive(false);
		}
		checkQuickSlotDesc();
	}

	public void checkQuickSlotDesc()
	{
		if (isMenuOpen() || BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen)
		{
			tileObjectHealthBar.gameObject.SetActive(false);
			tileObjectHealthBar.canBeShown(false);
			if (!invOpen)
			{
				quickSlotBar.gameObject.SetActive(false);
			}
			quickSlotDesc.gameObject.SetActive(false);
		}
		else
		{
			quickSlotBar.gameObject.SetActive(true);
			if (quickSlotText.text != "" && invSlots[selectedSlot].itemNo != -1)
			{
				quickSlotDesc.gameObject.SetActive(true);
				quickSlotDesc.transform.position = invSlots[selectedSlot].transform.position + new Vector3(0f, 50f * canvas.localScale.y);
			}
			else
			{
				quickSlotDesc.gameObject.SetActive(false);
			}
			tileObjectHealthBar.canBeShown(true);
		}
	}

	private void turnCursorOnOrOff(bool cursorOn)
	{
		if (Application.isEditor)
		{
			cursorIsOn = cursorOn;
		}
		StopCoroutine("cursorMoves");
		if (cursorOn)
		{
			StartCoroutine("cursorMoves");
		}
	}

	public void quickBarLocked(bool isLocked)
	{
		quickBarIsLocked = isLocked;
	}

	private IEnumerator cursorMoves()
	{
		while (true)
		{
			if (invOpen && !GiveNPC.give.giveWindowOpen)
			{
				if ((walletSlot.stack != 0 && dragSlot.itemNo == -1) || dragSlot.itemNo == getInvItemId(moneyItem))
				{
					walletSlot.hideSlot(true);
				}
				else
				{
					walletSlot.hideSlot(false);
				}
			}
			moveCursor();
			yield return null;
		}
	}

	private IEnumerator showAndHideCursors()
	{
		Image cursorImage = cursor.Find("CursorHand").GetComponent<Image>();
		Vector3 descriptionLocalPos = InvDescription.transform.localPosition;
		while (true)
		{
			if (RenderMap.map.mapOpen && !RenderMap.map.selectTeleWindowOpen)
			{
				cursorImage.enabled = false;
				while (RenderMap.map.mapOpen && !RenderMap.map.selectTeleWindowOpen)
				{
					yield return null;
				}
				cursorImage.enabled = true;
			}
			if (snapCursorOn && !usingMouse)
			{
				cursorImage.enabled = false;
				invDescriptionFollower.transform.parent = snappingCursor;
				InvDescription.parent = snappingCursor;
				InvDescription.localPosition = descriptionLocalPos;
				while (snapCursorOn && !usingMouse)
				{
					yield return null;
				}
				cursorImage.enabled = true;
				invDescriptionFollower.transform.parent = cursor;
				InvDescription.parent = cursor;
				InvDescription.localPosition = descriptionLocalPos;
			}
			yield return null;
		}
	}

	private IEnumerator startEquip()
	{
		yield return null;
		equipNewSelectedSlot();
	}

	public bool isMenuOpen()
	{
		if (invOpen || ChatBox.chat.chatOpen || menuOpen || PediaManager.manage.pediaOpen || CraftingManager.manage.craftMenuOpen || ChestWindow.chests.chestWindowOpen || ConversationManager.manage.inConversation || RenderMap.map.mapOpen || StatusManager.manage.dead || FarmAnimalMenu.menu.farmAnimalMenuOpen || HairDresserMenu.menu.hairMenuOpen || BulletinBoard.board.windowOpen || MailManager.manage.mailWindowOpen || QuestTracker.track.trackerOpen || PhotoManager.manage.photoTabOpen || CharLevelManager.manage.unlockWindowOpen || BankMenu.menu.bankOpen || HouseEditor.edit.windowOpen || TownManager.manage.townManagerOpen || MenuButtonsTop.menu.subMenuOpen || LicenceManager.manage.windowOpen || CatalogueManager.manage.catalogueOpen || PlayerDetailManager.manage.windowOpen)
		{
			return true;
		}
		return false;
	}

	public bool canMoveChar()
	{
		if (ChatBox.chat.chatOpen || NetworkMapSharer.share.sleeping || BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen || CameraController.control.cameraShowingSomething || !MenuButtonsTop.menu.closed || isMenuOpen() || GiftedItemWindow.gifted.windowOpen || CatalogueManager.manage.catalogueOpen)
		{
			return false;
		}
		return true;
	}

	public void turnSnapCursorOnOff(bool isUsingSnapCursor)
	{
		snapCursorOn = isUsingSnapCursor;
		if (isUsingSnapCursor)
		{
			StartCoroutine(snapCursorCoroutine());
		}
		if (!usingMouse && isUsingSnapCursor)
		{
			snappingCursor.gameObject.SetActive(isUsingSnapCursor);
		}
		if (!isUsingSnapCursor)
		{
			snappingCursor.gameObject.SetActive(false);
		}
	}

	public bool checkIfToolNearlyBroken()
	{
		for (int i = 0; i < invSlots.Length; i++)
		{
			if (invSlots[i].itemNo != -1 && allItems[invSlots[i].itemNo].isATool && invSlots[i].stack <= 30)
			{
				return true;
			}
		}
		return false;
	}

	public void checkAllClickableButtons()
	{
	}

	public void setCurrentlySelectedAndMoveCursor(RectTransform selected)
	{
		currentlySelected = selected;
		cursor.position = currentlySelected.position + (Vector3)currentlySelected.rect.center;
		snappingCursor.position = cursor.position + new Vector3(currentlySelected.sizeDelta.x / 3f, (0f - currentlySelected.sizeDelta.y) / 4f, 0f);
		if (snapCursorOn && (bool)currentlySelected && (bool)activeScrollBar && currentlySelected.IsChildOf(activeScrollBar.windowThatScrolls))
		{
			StartCoroutine(moveScrollWindowDelay());
		}
	}

	private IEnumerator moveScrollWindowDelay()
	{
		yield return null;
		if ((bool)activeScrollBar)
		{
			activeScrollBar.moveDirectlyToSelectedButton();
		}
	}

	private IEnumerator moveCursorForSnap()
	{
		while (snapCursorOn)
		{
			if (usingMouse)
			{
				snappingCursor.gameObject.SetActive(false);
				while (usingMouse)
				{
					yield return null;
				}
				snappingCursor.gameObject.SetActive(true);
			}
			if (RenderMap.map.mapOpen && !RenderMap.map.selectTeleWindowOpen)
			{
				snappingCursor.gameObject.SetActive(false);
				while (RenderMap.map.mapOpen && !RenderMap.map.selectTeleWindowOpen)
				{
					yield return null;
				}
				snappingCursor.gameObject.SetActive(true);
			}
			if (!currentlySelected || !isMenuOpen() || ConversationManager.manage.inConversation)
			{
				snappingCursor.gameObject.SetActive(false);
				while ((!currentlySelected || !isMenuOpen() || ConversationManager.manage.inConversation) && !usingMouse)
				{
					yield return null;
				}
				snappingCursor.gameObject.SetActive(true);
			}
			if ((bool)currentlySelected)
			{
				cursor.position = currentlySelected.position + (Vector3)currentlySelected.rect.center;
				if (lockCursorSnap)
				{
					snappingCursor.position = cursor.position + new Vector3(currentlySelected.sizeDelta.x / 3f, (0f - currentlySelected.sizeDelta.y) / 4f, 0f);
				}
				else
				{
					snappingCursor.position = Vector3.Lerp(snappingCursor.position, cursor.position + new Vector3(currentlySelected.sizeDelta.x / 3f, (0f - currentlySelected.sizeDelta.y) / 4f, 0f), Time.deltaTime * 25f);
				}
			}
			yield return null;
		}
	}

	private IEnumerator snapCursorCoroutine()
	{
		StartCoroutine(moveCursorForSnap());
		UIScrollBar cursorInsideScrollBar = null;
		while (snapCursorOn)
		{
			bool usingController = !usingMouse;
			while (usingMouse || !isMenuOpen() || ConversationManager.manage.inConversation || lockCursorSnap)
			{
				yield return null;
			}
			if (!usingController && !usingMouse && (bool)currentlySelected)
			{
				yield return new WaitForSeconds(0.15f);
			}
			float cursorX = InputMaster.input.UINavigation().x;
			float cursorY = InputMaster.input.UINavigation().y;
			if ((currentlySelected == null || !currentlySelected.gameObject.activeInHierarchy) && buttonsToSnapTo.Count > 0)
			{
				float y = buttonsToSnapTo[0].position.y;
				int index = 0;
				for (int i = 0; i < buttonsToSnapTo.Count; i++)
				{
					if (canBeSelected(buttonsToSnapTo[i]) && buttonsToSnapTo[i].position.y >= y)
					{
						y = buttonsToSnapTo[i].position.y;
						if (buttonsToSnapTo[i].position.x < buttonsToSnapTo[index].position.x)
						{
							index = i;
						}
					}
				}
				currentlySelected = buttonsToSnapTo[index];
			}
			if ((cursorX != 0f || cursorY != 0f) && (bool)currentlySelected)
			{
				List<RectTransform> list = new List<RectTransform>();
				List<RectTransform> list2 = new List<RectTransform>();
				for (int j = 0; j < buttonsToSnapTo.Count; j++)
				{
					float num = Vector3.Dot(new Vector3(cursorX, cursorY, 0f), (buttonsToSnapTo[j].position + (Vector3)buttonsToSnapTo[j].rect.center - (currentlySelected.position + (Vector3)currentlySelected.rect.center)).normalized);
					if (num >= 0.75f)
					{
						if (canBeSelected(buttonsToSnapTo[j]))
						{
							list.Add(buttonsToSnapTo[j]);
						}
					}
					else if (num > 0.48f && canBeSelected(buttonsToSnapTo[j]))
					{
						list2.Add(buttonsToSnapTo[j]);
					}
				}
				RectTransform rectTransform = null;
				for (int k = 0; k < list.Count; k++)
				{
					if (!rectTransform)
					{
						rectTransform = list[k];
						continue;
					}
					RectTransform rectTransform2 = list[k];
					Vector3 vector = rectTransform2.transform.position + (Vector3)rectTransform2.rect.center - (currentlySelected.transform.position + (Vector3)currentlySelected.rect.center);
					if ((rectTransform.transform.position + (Vector3)rectTransform.rect.center - (currentlySelected.transform.position + (Vector3)currentlySelected.rect.center)).sqrMagnitude > vector.sqrMagnitude)
					{
						rectTransform = rectTransform2;
					}
				}
				if (rectTransform == null)
				{
					for (int l = 0; l < list2.Count; l++)
					{
						if (!rectTransform)
						{
							rectTransform = list2[l];
							continue;
						}
						RectTransform rectTransform3 = list2[l];
						Vector3 vector2 = rectTransform3.transform.position + (Vector3)rectTransform3.rect.center - (currentlySelected.transform.position + (Vector3)currentlySelected.rect.center);
						if ((rectTransform.transform.position + (Vector3)rectTransform.rect.center - (currentlySelected.transform.position + (Vector3)currentlySelected.rect.center)).sqrMagnitude > vector2.sqrMagnitude)
						{
							rectTransform = rectTransform3;
						}
					}
				}
				if (rectTransform != null)
				{
					currentlySelected = rectTransform;
					SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
					if ((bool)activeScrollBar && currentlySelected.IsChildOf(activeScrollBar.windowThatScrolls))
					{
						if (cursorInsideScrollBar != activeScrollBar)
						{
							cursorInsideScrollBar = activeScrollBar;
							for (int m = 0; m < activeScrollBar.windowThatScrolls.childCount; m++)
							{
								if (canBeSelected(activeScrollBar.windowThatScrolls.GetChild(m).GetComponent<RectTransform>(), true))
								{
									currentlySelected = activeScrollBar.windowThatScrolls.GetChild(m).GetComponent<RectTransform>();
									break;
								}
							}
						}
						activeScrollBar.moveDirectlyToSelectedButton();
					}
					else
					{
						cursorInsideScrollBar = null;
					}
				}
				if (invOpen)
				{
					lastInvSlotSelected = currentlySelected;
				}
				float timer = 0.15f;
				while (timer > 0f)
				{
					timer -= Time.deltaTime;
					if (cursorX == 0f && cursorY == 0f)
					{
						timer = 0f;
					}
					yield return null;
				}
			}
			yield return null;
		}
		snappingCursor.gameObject.SetActive(false);
	}

	public void damageAllTools()
	{
		for (int i = 0; i < invSlots.Length; i++)
		{
			if (invSlots[i].itemNo >= 0 && allItems[invSlots[i].itemNo].hasFuel && (!allItems[invSlots[i].itemNo].changeToWhenUsed || ((bool)allItems[invSlots[i].itemNo].changeToWhenUsed && allItems[invSlots[i].itemNo].changeToAndStillUseFuel)))
			{
				invSlots[i].stack -= allItems[invSlots[i].itemNo].fuelMax / 3;
				invSlots[i].stack = Mathf.Clamp(invSlots[i].stack, 2, 10000000);
				invSlots[i].refreshSlot();
			}
		}
	}

	private IEnumerator continueGiveOnHoldDown(InventorySlot slotSelected)
	{
		float increaseCheck = 0f;
		float holdTimer = 0f;
		if (slotSelected.getGiveAmount() == slotSelected.stack)
		{
			yield break;
		}
		while (InputMaster.input.UISelect())
		{
			if (increaseCheck < 0.15f - holdTimer)
			{
				increaseCheck += Time.deltaTime;
			}
			else
			{
				increaseCheck = 0f;
				slotSelected.addGiveAmount();
			}
			holdTimer = Mathf.Clamp(holdTimer + Time.deltaTime / 8f, 0f, 0.14f);
			yield return null;
			if (slotSelected.getGiveAmount() == slotSelected.stack)
			{
				break;
			}
		}
	}

	private IEnumerator swapControllerPopUp()
	{
		changeControllerPopUp.SetActive(false);
		controllerPopUp.SetActive(!usingMouse);
		keyboardPopUp.SetActive(usingMouse);
		yield return null;
		changeControllerPopUp.SetActive(true);
		float timer = 1f;
		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			yield return null;
			if (!changeControllerPopUp.activeSelf)
			{
				yield break;
			}
		}
		changeControllerPopUp.SetActive(false);
	}
}
