using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
	public InventoryItem itemInSlot;

	public Sprite invSlotFull;

	public Sprite invSlotEmpty;

	public int itemNo = -1;

	public int stack;

	public Image slotBackgroundImage;

	public Image itemIcon;

	public TextMeshProUGUI stackText;

	public Image backingGlow;

	public Color selectedInQuickSlot;

	public Color cursorSelected;

	public Color originalColour;

	public Color defaulBarColor;

	public Sprite emptySprite;

	public InvSlotAnimator myAnim;

	public Equipable equipSlot;

	public GameObject fuelBar;

	public Image fuelBarFill;

	public GameObject stackBack;

	private bool isSelected;

	public int chestSlotNo = -1;

	public bool canNotBeSnappedTo;

	public GameObject selectedForGiveIcon;

	public GameObject giveAmountPopUp;

	public TextMeshProUGUI giveAmountText;

	private bool isBeingSnown = true;

	private bool selectedForGive;

	private bool disabledForGive;

	private int giveAmount;

	public bool slotUnlocked;

	private void Awake()
	{
	}

	private void Start()
	{
		if ((bool)selectedForGiveIcon)
		{
			selectedForGiveIcon.SetActive(false);
		}
	}

	public void updateSlotContentsAndRefresh(int newItemNo, int amount)
	{
		itemNo = newItemNo;
		stack = amount;
		refreshSlot();
		if (chestSlotNo != -1)
		{
			ChestWindow.chests.makeALocalChange(chestSlotNo);
		}
		setUpSlotColours();
	}

	public void refreshSlot(bool playAnimation = true)
	{
		if ((bool)fuelBar)
		{
			if (itemNo != -1 && Inventory.inv.allItems[itemNo].hasFuel)
			{
				if (stack > Inventory.inv.allItems[itemNo].fuelMax)
				{
					stack = Inventory.inv.allItems[itemNo].fuelMax;
				}
				if (itemIcon.gameObject.activeSelf)
				{
					fuelBar.gameObject.SetActive(true);
				}
				if (Inventory.inv.allItems[itemNo].customFuelColour != Color.clear)
				{
					fuelBarFill.color = Inventory.inv.allItems[itemNo].customFuelColour;
				}
				else
				{
					fuelBarFill.color = defaulBarColor;
				}
				fuelBarFill.fillAmount = (float)stack / (float)Inventory.inv.allItems[itemNo].fuelMax;
			}
			else if (itemNo != -1 && Inventory.inv.allItems[itemNo].hasColourVariation)
			{
				fuelBarFill.color = EquipWindow.equip.vehicleColoursUI[Mathf.Clamp(stack - 1, 0, EquipWindow.equip.vehicleColoursUI.Length - 1)];
				fuelBarFill.fillAmount = 1f;
				if (itemIcon.gameObject.activeSelf)
				{
					fuelBar.gameObject.SetActive(true);
				}
			}
			else
			{
				fuelBar.gameObject.SetActive(false);
			}
		}
		if (playAnimation)
		{
			myAnim.UpdateSlotContents();
		}
		if (itemNo > -1 && stack <= 0 && (bool)itemInSlot && !Inventory.inv.allItems[itemNo].hasFuel)
		{
			itemNo = -1;
			stack = 0;
		}
		if (itemNo > -1)
		{
			itemInSlot = Inventory.inv.allItems[itemNo];
			itemIcon.sprite = itemInSlot.getSprite();
			itemIcon.enabled = true;
			if (!itemInSlot.hasFuel && !itemInSlot.hasColourVariation && stack > 1)
			{
				if (stack > 999)
				{
					stackText.enableAutoSizing = true;
				}
				else
				{
					stackText.enableAutoSizing = false;
				}
				stackText.text = stack.ToString();
				if ((bool)stackBack)
				{
					stackBack.SetActive(true);
				}
				stackText.enabled = true;
			}
			else
			{
				stackText.enabled = false;
				if ((bool)stackBack)
				{
					stackBack.SetActive(false);
				}
			}
			slotBackgroundImage.sprite = UIAnimationManager.manage.getSlotSprite(itemNo);
		}
		else
		{
			itemInSlot = null;
			if ((bool)emptySprite)
			{
				itemIcon.sprite = emptySprite;
			}
			else
			{
				itemIcon.enabled = false;
			}
			stackText.enabled = false;
			if ((bool)stackBack)
			{
				stackBack.SetActive(false);
			}
			GetComponent<Image>().sprite = invSlotEmpty;
		}
		BulletinBoard.board.checkAllMissionsForItems();
		if (disabledForGive)
		{
			disableForGive();
		}
	}

	public void hideSlot(bool isSlotShown)
	{
		base.gameObject.SetActive(isSlotShown);
	}

	private void OnDisable()
	{
		if (!canNotBeSnappedTo)
		{
			Inventory.inv.buttonsToSnapTo.Remove(GetComponent<RectTransform>());
		}
		if (isSelected)
		{
			base.transform.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	private void OnEnable()
	{
		if (!canNotBeSnappedTo && !Inventory.inv.buttonsToSnapTo.Contains(GetComponent<RectTransform>()))
		{
			Inventory.inv.buttonsToSnapTo.Add(GetComponent<RectTransform>());
		}
	}

	public void selectInQuickSlot()
	{
		if (!disabledForGive)
		{
			slotBackgroundImage.color = selectedInQuickSlot;
			myAnim.SelectSlot();
		}
		isSelected = true;
	}

	public void deselectSlot()
	{
		if (!disabledForGive)
		{
			slotBackgroundImage.color = originalColour;
			myAnim.DeSelectSlot();
			setUpSlotColours();
		}
		isSelected = false;
	}

	public void setUpSlotColours()
	{
		if ((bool)backingGlow)
		{
			backingGlow.enabled = false;
		}
	}

	public bool isSelectedForGive()
	{
		return selectedForGive;
	}

	public void selectThisSlotForGive()
	{
		if (GiveNPC.give.giveWindowOpen && GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.Sell && GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.Build && GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.Tech && GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.SellToTrapper && GiveNPC.give.giveMenuTypeOpen != GiveNPC.currentlyGivingTo.SellToJimmy)
		{
			GiveNPC.give.clearAllSelectedSlots();
		}
		itemIcon.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
		base.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
		selectedForGive = true;
		selectedForGiveIcon.transform.localScale = new Vector3(1f, 1f, 1f);
		selectedForGiveIcon.SetActive(true);
		selectedForGiveOrNotAnim();
	}

	public void deselectThisSlotForGive()
	{
		removeAllGiveAmount();
		itemIcon.transform.localScale = new Vector3(1f, 1f, 1f);
		base.transform.localScale = new Vector3(1f, 1f, 1f);
		selectedForGive = false;
		selectedForGiveIcon.transform.localScale = new Vector3(1f, 1f, 1f);
		selectedForGiveIcon.SetActive(false);
		selectedForGiveOrNotAnim();
	}

	public bool isDisabledForGive()
	{
		return disabledForGive;
	}

	public void disableForGive()
	{
		removeAllGiveAmount();
		disabledForGive = true;
		slotBackgroundImage.color = Color.Lerp(Color.white, Color.black, 0.35f);
		itemIcon.color = Color.Lerp(Color.white, Color.black, 0.35f);
		stackBack.SetActive(false);
		fuelBar.SetActive(false);
	}

	public void slotDisableOnly()
	{
		disabledForGive = true;
	}

	public void slotEnableOnly()
	{
		disabledForGive = false;
	}

	public void clearDisable()
	{
		disabledForGive = false;
		slotBackgroundImage.color = Color.white;
		itemIcon.color = Color.white;
		updateSlotContentsAndRefresh(itemNo, stack);
	}

	public int getGiveAmount()
	{
		return giveAmount;
	}

	public void addGiveAmount(int amount = 1)
	{
		if (!selectedForGive)
		{
			SoundManager.manage.play2DSound(SoundManager.manage.selectSlotForGive);
			selectThisSlotForGive();
		}
		if (!Inventory.inv.allItems[itemNo].hasFuel && stack != 1)
		{
			if (giveAmount + amount > stack)
			{
				SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
			}
			else
			{
				SoundManager.manage.play2DSound(SoundManager.manage.selectSlotForGive);
			}
			if (GiveNPC.give.giveMenuTypeOpen == GiveNPC.currentlyGivingTo.SellToJimmy)
			{
				giveAmount = Mathf.Clamp(giveAmount + amount, 50, stack);
			}
			else
			{
				giveAmount = Mathf.Clamp(giveAmount + amount, 0, stack);
			}
			giveAmountPopUp.SetActive(true);
			giveAmountText.text = giveAmount.ToString();
		}
	}

	public void removeAllGiveAmount()
	{
		if ((bool)giveAmountPopUp)
		{
			giveAmountPopUp.gameObject.SetActive(false);
			giveAmount = 0;
		}
	}

	private void selectedForGiveOrNotAnim()
	{
	}

	private IEnumerator selectSlotForGiveAnimation()
	{
		yield return null;
	}
}
