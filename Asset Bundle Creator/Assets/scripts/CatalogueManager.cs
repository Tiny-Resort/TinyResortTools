using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CatalogueManager : MonoBehaviour
{
	public enum EntryType
	{
		Clothing = 0,
		Furniture = 1
	}

	public static CatalogueManager manage;

	public bool[] collectedItem;

	public GameObject catalogueWindow;

	public bool catalogueOpen;

	public GameObject catalogueButton;

	public Camera previewCamera;

	public Transform previewPos;

	public Transform buttonWindow;

	private List<CatalogueButton> allButtons = new List<CatalogueButton>();

	public bool openWindow;

	public GameObject cameraPreview;

	public int furnitureOnOrder;

	public int clothingOnOrder;

	public GameObject infoScreen;

	public GameObject orderButton;

	public GameObject orderCompleteWindow;

	public TextMeshProUGUI itemTitleText;

	public TextMeshProUGUI itemPrice;

	public GameObject orderClothesButtons;

	public GameObject orderFurnitureButtons;

	private int showingItemId = -1;

	[Header("Clothing Select Tabs")]
	private GameObject clothingTabs;

	private GameObject previewObject;

	private bool resetPreviewRot;

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
		collectedItem = new bool[Inventory.inv.allItems.Length];
	}

	private void Update()
	{
		if (openWindow)
		{
			openWindow = false;
			openCatalogue(EntryType.Furniture);
		}
	}

	public void openCatalogue(EntryType filterType)
	{
		orderCompleteWindow.SetActive(false);
		infoScreen.SetActive(false);
		catalogueOpen = true;
		clearButtons();
		catalogueWindow.SetActive(true);
		if (filterType == EntryType.Clothing)
		{
			orderClothesButtons.SetActive(true);
			orderFurnitureButtons.SetActive(false);
			for (int i = 0; i < collectedItem.Length; i++)
			{
				if (collectedItem[i] && (bool)Inventory.inv.allItems[i].equipable && Inventory.inv.allItems[i].equipable.cloths)
				{
					createButtonAndFill(i);
				}
			}
		}
		else
		{
			orderClothesButtons.SetActive(false);
			orderFurnitureButtons.SetActive(true);
			for (int j = 0; j < collectedItem.Length; j++)
			{
				if (collectedItem[j] && Inventory.inv.allItems[j].isFurniture)
				{
					createButtonAndFill(j);
				}
			}
		}
		StartCoroutine(rotatePreviewPos());
		Inventory.inv.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closed = false;
	}

	public void closeCatalogue()
	{
		orderCompleteWindow.SetActive(false);
		infoScreen.SetActive(false);
		catalogueOpen = false;
		catalogueWindow.gameObject.SetActive(false);
		cameraPreview.gameObject.SetActive(false);
		Inventory.inv.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	public void showItemInfo(int itemToShow)
	{
		resetPreviewRot = true;
		previewPos.rotation = Quaternion.identity;
		showingItemId = itemToShow;
		cameraPreview.gameObject.SetActive(true);
		Object.Destroy(previewObject);
		itemTitleText.text = Inventory.inv.allItems[itemToShow].getInvItemName();
		itemPrice.text = "<sprite=11> " + ((float)Inventory.inv.allItems[itemToShow].value * 2.5f).ToString("n0");
		if (Inventory.inv.allItems[itemToShow].isFurniture)
		{
			previewCamera.transform.localPosition = new Vector3(0f, 2.58f, -5.2f);
			previewObject = Object.Instantiate(Inventory.inv.allItems[itemToShow].placeable, previewPos).gameObject;
			previewObject.transform.localPosition = Vector3.zero;
			previewObject.transform.rotation = Quaternion.identity;
			if (WorldManager.manageWorld.allObjectSettings[Inventory.inv.allItems[itemToShow].placeable.tileObjectId].isMultiTileObject)
			{
				previewObject.transform.localPosition -= new Vector3(WorldManager.manageWorld.allObjectSettings[Inventory.inv.allItems[itemToShow].placeable.tileObjectId].xSize / 2, 0f, WorldManager.manageWorld.allObjectSettings[Inventory.inv.allItems[itemToShow].placeable.tileObjectId].ySize / 2);
			}
		}
		else if ((bool)Inventory.inv.allItems[itemToShow].equipable && Inventory.inv.allItems[itemToShow].equipable.cloths)
		{
			if (Inventory.inv.allItems[itemToShow].equipable.hat)
			{
				previewObject = Object.Instantiate(EquipWindow.equip.hatPlaceable, previewPos).gameObject;
			}
			else if (Inventory.inv.allItems[itemToShow].equipable.face)
			{
				previewObject = Object.Instantiate(EquipWindow.equip.hatPlaceable, previewPos).gameObject;
			}
			else if (Inventory.inv.allItems[itemToShow].equipable.shirt)
			{
				previewObject = Object.Instantiate(EquipWindow.equip.shirtPlaceable, previewPos).gameObject;
			}
			else if (Inventory.inv.allItems[itemToShow].equipable.pants)
			{
				previewObject = Object.Instantiate(EquipWindow.equip.pantsPlaceable, previewPos).gameObject;
			}
			else if (Inventory.inv.allItems[itemToShow].equipable.shoes)
			{
				previewObject = Object.Instantiate(EquipWindow.equip.shoePlaceable, previewPos).gameObject;
			}
			previewCamera.transform.localPosition = new Vector3(0f, 1.4f, -2.63f);
			previewObject.GetComponentInChildren<ClothingDisplay>().updateStatus(itemToShow);
			previewObject.transform.localPosition = Vector3.zero;
			previewObject.transform.rotation = Quaternion.identity;
		}
		infoScreen.SetActive(false);
		infoScreen.SetActive(true);
		if (Inventory.inv.wallet < (int)((float)Inventory.inv.allItems[showingItemId].value * 2.5f))
		{
			orderButton.SetActive(false);
		}
		else
		{
			orderButton.SetActive(true);
		}
	}

	public void sortCatalogueByFurnitureType(int type)
	{
		switch (type)
		{
		case 0:
		{
			for (int j = 0; j < allButtons.Count; j++)
			{
				allButtons[j].gameObject.SetActive(true);
			}
			break;
		}
		case 1:
		{
			for (int l = 0; l < allButtons.Count; l++)
			{
				allButtons[l].gameObject.SetActive((bool)allButtons[l].getShowingInvItem().placeable.tileObjectFurniture && !allButtons[l].getShowingInvItem().placeable.tileObjectFurniture.isSeat);
			}
			break;
		}
		case 2:
		{
			for (int m = 0; m < allButtons.Count; m++)
			{
				allButtons[m].gameObject.SetActive((bool)allButtons[m].getShowingInvItem().placeable.tileObjectFurniture && allButtons[m].getShowingInvItem().placeable.tileObjectFurniture.isSeat);
			}
			break;
		}
		case 3:
		{
			for (int k = 0; k < allButtons.Count; k++)
			{
				allButtons[k].gameObject.SetActive(allButtons[k].getShowingInvItem().placeable.tileObjectChest);
			}
			break;
		}
		case 4:
		{
			for (int i = 0; i < allButtons.Count; i++)
			{
				allButtons[i].gameObject.SetActive((bool)allButtons[i].getShowingInvItem().placeable && WorldManager.manageWorld.allObjectSettings[allButtons[i].getShowingInvItem().placeable.tileObjectId].canBePlacedOnTopOfFurniture);
			}
			break;
		}
		}
	}

	public void sortCatalogueByClothType(int type)
	{
		switch (type)
		{
		case 0:
		{
			for (int n = 0; n < allButtons.Count; n++)
			{
				allButtons[n].gameObject.SetActive(true);
			}
			break;
		}
		case 1:
		{
			for (int j = 0; j < allButtons.Count; j++)
			{
				allButtons[j].gameObject.SetActive(allButtons[j].getShowingInvItem().equipable.hat);
			}
			break;
		}
		case 2:
		{
			for (int l = 0; l < allButtons.Count; l++)
			{
				allButtons[l].gameObject.SetActive(allButtons[l].getShowingInvItem().equipable.face);
			}
			break;
		}
		case 3:
		{
			for (int num = 0; num < allButtons.Count; num++)
			{
				allButtons[num].gameObject.SetActive(allButtons[num].getShowingInvItem().equipable.shirt && !allButtons[num].getShowingInvItem().equipable.dress);
			}
			break;
		}
		case 4:
		{
			for (int m = 0; m < allButtons.Count; m++)
			{
				allButtons[m].gameObject.SetActive(allButtons[m].getShowingInvItem().equipable.dress);
			}
			break;
		}
		case 5:
		{
			for (int k = 0; k < allButtons.Count; k++)
			{
				allButtons[k].gameObject.SetActive(allButtons[k].getShowingInvItem().equipable.pants);
			}
			break;
		}
		case 6:
		{
			for (int i = 0; i < allButtons.Count; i++)
			{
				allButtons[i].gameObject.SetActive(allButtons[i].getShowingInvItem().equipable.shoes);
			}
			break;
		}
		}
	}

	public void pickUpItem(int itemId)
	{
		collectedItem[itemId] = true;
	}

	private void clearButtons()
	{
		for (int i = 0; i < allButtons.Count; i++)
		{
			Object.Destroy(allButtons[i].gameObject);
		}
		allButtons.Clear();
	}

	private void createButtonAndFill(int itemId)
	{
		CatalogueButton component = Object.Instantiate(catalogueButton, buttonWindow).GetComponent<CatalogueButton>();
		component.setUpButton(itemId);
		allButtons.Add(component);
	}

	private IEnumerator rotatePreviewPos()
	{
		while (catalogueOpen)
		{
			if (resetPreviewRot)
			{
				previewPos.rotation = Quaternion.identity;
				resetPreviewRot = false;
			}
			previewPos.Rotate(Vector3.up, Time.deltaTime * 5f);
			yield return null;
		}
	}

	public void orderItem()
	{
		Inventory.inv.changeWallet(-(int)((float)Inventory.inv.allItems[showingItemId].value * 2.5f));
		int nPCFrom = 4;
		if (Inventory.inv.allItems[showingItemId].isFurniture)
		{
			nPCFrom = 3;
		}
		MailManager.manage.tomorrowsLetters.Add(new Letter(nPCFrom, Letter.LetterType.CatalogueOrder, showingItemId, 1));
		StartCoroutine(openOrderCompleteWindow());
		if (Inventory.inv.wallet < (int)((float)Inventory.inv.allItems[showingItemId].value * 2.5f))
		{
			orderButton.SetActive(false);
		}
		else
		{
			orderButton.SetActive(true);
		}
	}

	public IEnumerator openOrderCompleteWindow()
	{
		orderCompleteWindow.SetActive(true);
		yield return new WaitForSeconds(0.5f);
		bool holdWindow = true;
		while (holdWindow)
		{
			yield return null;
			if (InputMaster.input.Interact() || InputMaster.input.Use() || InputMaster.input.UICancel())
			{
				holdWindow = false;
			}
		}
		orderCompleteWindow.SetActive(false);
	}
}
