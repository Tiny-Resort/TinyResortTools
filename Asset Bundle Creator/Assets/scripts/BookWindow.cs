using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookWindow : MonoBehaviour
{
	public static BookWindow book;

	public FillRecipeSlot[] slots;

	public GameObject bookWindow;

	public TextMeshProUGUI objectTitle;

	public bool open;

	private int showingTileObjectId = -1;

	public GridLayoutGroup myGrid;

	public GameObject plantBook;

	public GameObject machineBook;

	public TextMeshProUGUI plantBookText;

	public GameObject nothingFound;

	private void Awake()
	{
		book = this;
	}

	public void openBook()
	{
		open = true;
		machineBook.SetActive(true);
		plantBook.SetActive(false);
		StartCoroutine(runWhileBookOpen());
	}

	public void closeBook()
	{
		open = false;
		bookWindow.SetActive(false);
	}

	public void openPlantBook(string textToShow)
	{
		bookWindow.SetActive(true);
		machineBook.SetActive(false);
		plantBook.SetActive(true);
		plantBookText.text = textToShow;
	}

	private void whileBookOpen()
	{
		int num = WorldManager.manageWorld.onTileMap[(int)NetworkMapSharer.share.localChar.myInteract.selectedTile.x, (int)NetworkMapSharer.share.localChar.myInteract.selectedTile.y];
		if (num > 0 && (bool)WorldManager.manageWorld.allObjects[num].tileObjectItemChanger)
		{
			bookWindow.SetActive(true);
			if (num != showingTileObjectId)
			{
				bookWindow.SetActive(false);
				showingTileObjectId = num;
				getAllItemsForChanger(num);
				bookWindow.SetActive(true);
			}
		}
		else
		{
			bookWindow.SetActive(false);
		}
	}

	private IEnumerator runWhileBookOpen()
	{
		while (open)
		{
			yield return null;
			whileBookOpen();
		}
		bookWindow.SetActive(false);
	}

	public void getAllItemsForChanger(int tileObject)
	{
		objectTitle.text = WorldManager.manageWorld.allObjectSettings[tileObject].dropsItemOnDeath.getInvItemName();
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if (CatalogueManager.manage.collectedItem[i] && (bool)Inventory.inv.allItems[i].itemChange && Inventory.inv.allItems[i].itemChange.getAmountNeeded(tileObject) > 0)
			{
				list.Add(Inventory.inv.allItems[i]);
			}
		}
		if (list.Count == 0)
		{
			myGrid.constraintCount = 2;
			nothingFound.SetActive(true);
			for (int j = 0; j < slots.Length; j++)
			{
				slots[j].gameObject.SetActive(false);
			}
			return;
		}
		nothingFound.SetActive(false);
		list.Sort(sortIngredients);
		if (list.Count < 8)
		{
			myGrid.constraintCount = list.Count;
		}
		else
		{
			myGrid.constraintCount = 8;
		}
		for (int k = 0; k < slots.Length; k++)
		{
			slots[k].gameObject.SetActive(false);
			if (k < list.Count)
			{
				slots[k].gameObject.SetActive(true);
				slots[k].fillDeedBuySlot(list[k].getItemId());
				slots[k].itemAmounts.text = "";
			}
			else
			{
				slots[k].gameObject.SetActive(false);
			}
		}
	}

	public int sortIngredients(InventoryItem a, InventoryItem b)
	{
		if (a.value < b.value)
		{
			return -1;
		}
		if (a.value > b.value)
		{
			return 1;
		}
		return 0;
	}
}
