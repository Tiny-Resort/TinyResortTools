using System.Collections.Generic;
using UnityEngine;

public class HouseEditor : MonoBehaviour
{
	public static HouseEditor edit;

	public GameObject housePartButtonPrefab;

	public bool windowOpen;

	public GameObject window;

	public GameObject houseWithCamera;

	public GameObject mainButtonsWindow;

	public GameObject subButtonWindow;

	public GameObject paintSubMenu;

	public GameObject colorSubMenu;

	public GameObject paintOptionButtons;

	public Transform subButtonParent;

	public Transform textureButtonParent;

	public PlayerHouseExterior dummyHouse;

	public HouseExterior dummyExterior = new HouseExterior(-2, -2);

	public PlayerHouseExterior.houseParts currentlyPainting;

	private List<HousePartButton> buttonsCurrentlyShown = new List<HousePartButton>();

	private List<HousePartButton> colorButtonsShown = new List<HousePartButton>();

	private void Awake()
	{
		edit = this;
	}

	private void Start()
	{
		textureButtonParent.gameObject.SetActive(false);
	}

	public void openWindow()
	{
		if (HouseManager.manage.getPlayerHouseExterior() != null)
		{
			CurrencyWindows.currency.openJournal();
			dummyExterior.copyFromAnotherHouseExterior(HouseManager.manage.getPlayerHouseExterior());
			dummyHouse.setExterior(dummyExterior);
			windowOpen = true;
			window.SetActive(true);
			houseWithCamera.SetActive(true);
			subButtonWindow.SetActive(false);
			openSubButtonWindow_House();
			paintHouse_Color1();
		}
	}

	public void closeWindow()
	{
		CurrencyWindows.currency.closeJournal();
		HouseExterior playerHouseExterior = HouseManager.manage.getPlayerHouseExterior();
		dummyExterior.copyToAnotherHouseExterior(playerHouseExterior);
		HouseManager.manage.findHousesOnDisplay(playerHouseExterior.xPos, playerHouseExterior.yPos).updateHouseExterior();
		windowOpen = false;
		window.SetActive(false);
		MenuButtonsTop.menu.closeButtonDelay();
	}

	public void closeSubWindow()
	{
		subButtonWindow.SetActive(false);
		paintSubMenu.SetActive(false);
		colorSubMenu.SetActive(false);
		paintOptionButtons.SetActive(false);
	}

	public void updateDummy()
	{
		dummyHouse.setExterior(dummyExterior);
	}

	public void paintHouse_Color1()
	{
		currentlyPainting = PlayerHouseExterior.houseParts.houseBase;
		fillButtonsTextureArray(dummyHouse.wallMaterials);
		paintOptionButtons.SetActive(true);
	}

	public void paintHouse_Color2()
	{
		currentlyPainting = PlayerHouseExterior.houseParts.houseDetailsColor;
		fillButtonsTextureArray(dummyHouse.houseMaterials);
		paintOptionButtons.SetActive(true);
	}

	public void paintHouse_Roof()
	{
		currentlyPainting = PlayerHouseExterior.houseParts.roof;
		fillButtonsTextureArray(dummyHouse.roofMaterials);
		paintOptionButtons.SetActive(false);
	}

	public void openPaintSubMenu()
	{
	}

	public void openColorSubMenu()
	{
		colorSubMenu.SetActive(true);
	}

	public void openSubButtonWindow_House()
	{
		fillButtonsForArray(dummyHouse.bases[0].baseParts, PlayerHouseExterior.houseParts.houseBase);
		colorSubMenu.SetActive(true);
	}

	public void openSubButtonWindow_Roof()
	{
		fillButtonsForArray(dummyHouse.roofs[0].baseParts, PlayerHouseExterior.houseParts.roof);
		colorSubMenu.SetActive(true);
	}

	public void openSubButtonWindow_Door()
	{
		fillButtonsForArray(dummyHouse.doors[0].baseParts, PlayerHouseExterior.houseParts.door);
		colorSubMenu.SetActive(false);
	}

	public void openSubButtonWindow_Windows()
	{
		fillButtonsForArray(dummyHouse.windows[0].baseParts, PlayerHouseExterior.houseParts.window);
		colorSubMenu.SetActive(false);
	}

	public void fillButtonsForArray(GameObject[] array, PlayerHouseExterior.houseParts partsToShow)
	{
		for (int i = 0; i < buttonsCurrentlyShown.Count; i++)
		{
			Object.Destroy(buttonsCurrentlyShown[i].gameObject);
		}
		buttonsCurrentlyShown.Clear();
		for (int j = 0; j < array.Length; j++)
		{
			buttonsCurrentlyShown.Add(Object.Instantiate(housePartButtonPrefab, subButtonParent.transform).GetComponent<HousePartButton>());
			buttonsCurrentlyShown[j].setUpButton(dummyExterior, j, partsToShow);
		}
		subButtonWindow.SetActive(true);
	}

	public void tintColorButtons(Color tintColor)
	{
		foreach (HousePartButton item in colorButtonsShown)
		{
			item.textureImage.color = tintColor;
		}
	}

	public void fillButtonsTextureArray(Material[] array)
	{
		for (int i = 0; i < colorButtonsShown.Count; i++)
		{
			Object.Destroy(colorButtonsShown[i].gameObject);
		}
		colorButtonsShown.Clear();
		for (int j = 0; j < array.Length; j++)
		{
			colorButtonsShown.Add(Object.Instantiate(housePartButtonPrefab, textureButtonParent.transform).GetComponent<HousePartButton>());
			colorButtonsShown[j].setUpTextureButton(dummyExterior, j, currentlyPainting, array[j]);
		}
	}
}
