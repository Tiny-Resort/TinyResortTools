using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CheatScript : MonoBehaviour
{
	public static CheatScript cheat;

	public GameObject cheatWindow;

	public GameObject cheatMenuButton;

	private GameObject[] cheatButtons;

	public Transform cheatScreen;

	public InputField priceField;

	public bool cheatMenuOpen;

	public InputField searchBar;

	public Transform itemSpreadSheetWindow;

	public Transform itemSpeadSheetContent;

	public GameObject itemSpreadSheetEntryPrefab;

	private ItemSpreadSheetEntry[] allItemEntrys;

	public bool cheatsOn;

	public int amountToGive = 1;

	public InventorySlot bin;

	public Sprite selectedIcon;

	public Sprite notSelectedIcon;

	public Image NintyNineSelected;

	public Image OneSelected;

	private bool inOpenOrClose;

	private void Awake()
	{
		cheat = this;
	}

	private void Start()
	{
		amountToGive = 1;
		cheatButtons = new GameObject[Inventory.inv.allItems.Length];
		bin.updateSlotContentsAndRefresh(-1, 0);
		if (PlayerPrefs.HasKey("Cheats"))
		{
			cheatsOn = true;
		}
	}

	private void Update()
	{
		if ((cheatsOn && Input.GetButtonDown("Cheat") && !inOpenOrClose) || (cheatMenuOpen && InputMaster.input.UICancel() && !inOpenOrClose))
		{
			cheatMenuOpen = !cheatMenuOpen;
			if (cheatMenuOpen)
			{
				cheatWindow.SetActive(true);
				Inventory.inv.invOpen = true;
				Inventory.inv.openAndCloseInv();
				searchBar.ActivateInputField();
				StartCoroutine(populateList());
				RenderMap.map.changeMapWindow();
			}
			else
			{
				StartCoroutine(destroyList());
				RenderMap.map.changeMapWindow();
			}
		}
		if (cheatMenuOpen)
		{
			if (bin.itemNo != -1)
			{
				bin.updateSlotContentsAndRefresh(-1, 0);
			}
			if (!Inventory.inv.invOpen)
			{
				Inventory.inv.invOpen = true;
				Inventory.inv.openAndCloseInv();
			}
		}
	}

	private IEnumerator populateList()
	{
		inOpenOrClose = true;
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			cheatButtons[i] = Object.Instantiate(cheatMenuButton, cheatScreen);
			cheatButtons[i].GetComponent<CheatMenuButton>().setUpButton(i);
			if (i % 50 == 0)
			{
				yield return null;
			}
		}
		inOpenOrClose = false;
	}

	public IEnumerator destroyList()
	{
		inOpenOrClose = true;
		for (int i = 0; i < cheatButtons.Length; i++)
		{
			Object.Destroy(cheatButtons[i]);
			if (i % 100 == 0)
			{
				yield return null;
			}
		}
		cheatWindow.SetActive(false);
		inOpenOrClose = false;
	}

	public void giveAmount(int amount)
	{
		amountToGive = amount;
		if (amountToGive == 99)
		{
			NintyNineSelected.sprite = selectedIcon;
			OneSelected.sprite = notSelectedIcon;
		}
		else
		{
			NintyNineSelected.sprite = notSelectedIcon;
			OneSelected.sprite = selectedIcon;
		}
	}

	public void showAll()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			cheatButtons[i].gameObject.SetActive(true);
		}
	}

	public void showAllHideClothes()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].equipable && Inventory.inv.allItems[i].equipable.cloths)
			{
				cheatButtons[i].gameObject.SetActive(false);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(true);
			}
		}
	}

	public void showAllWallpaper()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (((bool)Inventory.inv.allItems[i].equipable && Inventory.inv.allItems[i].equipable.wallpaper) || ((bool)Inventory.inv.allItems[i].equipable && Inventory.inv.allItems[i].equipable.flooring))
			{
				cheatButtons[i].gameObject.SetActive(true);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(false);
			}
		}
	}

	public void showAllFlooring()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].equipable && Inventory.inv.allItems[i].equipable.flooring)
			{
				cheatButtons[i].gameObject.SetActive(true);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(false);
			}
		}
	}

	public void showAllVehicles()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].spawnPlaceable)
			{
				cheatButtons[i].gameObject.SetActive(true);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(false);
			}
		}
	}

	public void showAllTools()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (Inventory.inv.allItems[i].isATool)
			{
				cheatButtons[i].gameObject.SetActive(true);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(false);
			}
		}
	}

	public void showAllPlaceables()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].placeable)
			{
				cheatButtons[i].gameObject.SetActive(true);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(false);
			}
		}
	}

	public void showAllClothes()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].equipable && Inventory.inv.allItems[i].equipable.cloths)
			{
				cheatButtons[i].gameObject.SetActive(true);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(false);
			}
		}
	}

	public void showAllRequestable()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (Inventory.inv.allItems[i].isRequestable)
			{
				cheatButtons[i].gameObject.SetActive(true);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(false);
			}
		}
	}

	public void showAllFishAndBugs()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].fish || (bool)Inventory.inv.allItems[i].bug || (bool)Inventory.inv.allItems[i].underwaterCreature)
			{
				cheatButtons[i].gameObject.SetActive(true);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(false);
			}
		}
	}

	public void showMisc()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (!Inventory.inv.allItems[i].isATool && !Inventory.inv.allItems[i].placeable && !Inventory.inv.allItems[i].equipable && !Inventory.inv.allItems[i].consumeable)
			{
				cheatButtons[i].gameObject.SetActive(true);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(false);
			}
		}
	}

	public void showAllEatable()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].consumeable)
			{
				cheatButtons[i].gameObject.SetActive(true);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(false);
			}
		}
	}

	public void showAllCraftable()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].craftable && !Inventory.inv.allItems[i].isDeed)
			{
				cheatButtons[i].gameObject.SetActive(true);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(false);
			}
		}
	}

	public void searchCheatMenu()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (Inventory.inv.allItems[i].itemName.ToLower().Contains(searchBar.text.ToLower()))
			{
				cheatButtons[i].gameObject.SetActive(true);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(false);
			}
		}
	}

	public void showPlaceableDeeds()
	{
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (Inventory.inv.allItems[i].isDeed && (bool)Inventory.inv.allItems[i].placeable && (bool)Inventory.inv.allItems[i].placeable.tileObjectGrowthStages && Inventory.inv.allItems[i].placeable.tileObjectGrowthStages.NPCMovesInWhenBuilt.Length >= 1)
			{
				cheatButtons[i].gameObject.SetActive(true);
			}
			else
			{
				cheatButtons[i].gameObject.SetActive(false);
			}
		}
	}
}
