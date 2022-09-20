using System.Collections.Generic;
using UnityEngine;

public class LoadGameWindow : MonoBehaviour
{
	public GameObject slotButtonPrefab;

	public Transform slotsParent;

	private List<SaveSlotButton> buttonsShown = new List<SaveSlotButton>();

	public bool loadOnlyPlayerOnClick;

	private void OnEnable()
	{
		if (loadOnlyPlayerOnClick)
		{
			NetworkPlayersManager.manage.singlePlayerOptions.SetActive(false);
		}
		else
		{
			NetworkPlayersManager.manage.singlePlayerOptions.SetActive(true);
		}
		refreshList();
	}

	private void OnDisable()
	{
		destroyList();
	}

	private void destroyList()
	{
		for (int i = 0; i < buttonsShown.Count; i++)
		{
			Object.Destroy(buttonsShown[i].gameObject);
		}
		buttonsShown.Clear();
	}

	private void refreshList()
	{
		destroyList();
		for (int i = 0; i < 100; i++)
		{
			PlayerInv saveDetailsForFileButton = SaveLoad.saveOrLoad.getSaveDetailsForFileButton(i);
			DateSave saveDateDetailsForButton = SaveLoad.saveOrLoad.getSaveDateDetailsForButton(i);
			if (saveDetailsForFileButton != null)
			{
				buttonsShown.Add(Object.Instantiate(slotButtonPrefab, slotsParent).GetComponent<SaveSlotButton>());
				buttonsShown[buttonsShown.Count - 1].setSlotNo(i, saveDetailsForFileButton, saveDateDetailsForButton, loadOnlyPlayerOnClick);
			}
		}
		sortByLastSaved();
	}

	public void sortByLastSaved()
	{
		if (buttonsShown.Count > 0)
		{
			buttonsShown.Sort(sortButtons);
			for (int i = 0; i < buttonsShown.Count; i++)
			{
				buttonsShown[i].transform.SetSiblingIndex(i);
			}
			Inventory.inv.setCurrentlySelectedAndMoveCursor(buttonsShown[0].GetComponent<RectTransform>());
		}
	}

	public int sortButtons(SaveSlotButton a, SaveSlotButton b)
	{
		if (a.savedTime < b.savedTime)
		{
			return 1;
		}
		if (a.savedTime > b.savedTime)
		{
			return -1;
		}
		return 0;
	}
}
