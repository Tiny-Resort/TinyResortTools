using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MuseumManager : MonoBehaviour
{
	public static MuseumManager manage;

	public List<InventoryItem> allFish;

	public bool[] fishDonated;

	public List<InventoryItem> allBugs;

	public bool[] bugsDonated;

	public List<InventoryItem> allUnderWaterCreatures;

	public bool[] underWaterCreaturesDonated;

	public bool clientNeedsToRequest;

	public Texture2D[] paintingsOnDisplay = new Texture2D[4];

	public List<List<byte>> sentBytes = new List<List<byte>>();

	public UnityEvent onExhibitUpdate = new UnityEvent();

	private void Awake()
	{
		manage = this;
	}

	public void loadMuseum()
	{
		SaveLoad.saveOrLoad.loadPhotos();
		SaveLoad.saveOrLoad.loadMuseum();
		for (int i = 0; i < paintingsOnDisplay.Length; i++)
		{
			if (PhotoManager.manage.displayedPhotos[i] != null && PhotoManager.manage.displayedPhotos[i].photoName != "Dummy")
			{
				paintingsOnDisplay[i] = PhotoManager.manage.loadPhoto(PhotoManager.manage.displayedPhotos[i].photoName);
			}
		}
	}

	private void Start()
	{
		for (int i = 0; i < 4; i++)
		{
			sentBytes.Add(new List<byte>());
		}
		InventoryItem[] allItems = Inventory.inv.allItems;
		foreach (InventoryItem inventoryItem in allItems)
		{
			if ((bool)inventoryItem.fish)
			{
				allFish.Add(inventoryItem);
			}
			if ((bool)inventoryItem.bug)
			{
				allBugs.Add(inventoryItem);
			}
			if ((bool)inventoryItem.underwaterCreature)
			{
				allUnderWaterCreatures.Add(inventoryItem);
			}
		}
		fishDonated = new bool[allFish.Count];
		bugsDonated = new bool[allBugs.Count];
		underWaterCreaturesDonated = new bool[allUnderWaterCreatures.Count];
	}

	public bool checkIfDonationNeeded(InventoryItem item)
	{
		if ((bool)item.fish && !fishDonated[allFish.IndexOf(item)])
		{
			return true;
		}
		if ((bool)item.bug && !bugsDonated[allBugs.IndexOf(item)])
		{
			return true;
		}
		if ((bool)item.underwaterCreature && !underWaterCreaturesDonated[allUnderWaterCreatures.IndexOf(item)])
		{
			return true;
		}
		return false;
	}

	public void donateItem(InventoryItem item)
	{
		if ((bool)item.fish)
		{
			fishDonated[allFish.IndexOf(item)] = true;
		}
		if ((bool)item.bug)
		{
			bugsDonated[allBugs.IndexOf(item)] = true;
		}
		if ((bool)item.underwaterCreature)
		{
			underWaterCreaturesDonated[allUnderWaterCreatures.IndexOf(item)] = true;
		}
		if ((bool)MuseumDisplay.display)
		{
			MuseumDisplay.display.updateExhibits();
		}
	}

	public void donatePhoto(int frameNo, int photoSaveId)
	{
		if (PhotoManager.manage.displayedPhotos[frameNo] != null && PhotoManager.manage.displayedPhotos[frameNo].photoName != "Dummy")
		{
			PhotoManager.manage.savedPhotos.Add(PhotoManager.manage.displayedPhotos[frameNo]);
		}
		PhotoManager.manage.displayedPhotos[frameNo] = PhotoManager.manage.savedPhotos[photoSaveId];
		PhotoManager.manage.donatePhoto(photoSaveId);
		paintingsOnDisplay[frameNo] = PhotoManager.manage.loadPhoto(PhotoManager.manage.displayedPhotos[frameNo].photoName);
		if ((bool)MuseumDisplay.display)
		{
			MuseumDisplay.display.updatePhotoExhibits();
		}
		StartCoroutine(NetworkMapSharer.share.sendNewPaintingToAll(frameNo));
	}

	public bool checkIfItemCanBeDonated(InventoryItem item)
	{
		if ((bool)item.fish)
		{
			return true;
		}
		if ((bool)item.bug)
		{
			return true;
		}
		if ((bool)item.underwaterCreature)
		{
			return true;
		}
		return false;
	}

	public void clearForClient()
	{
		InventoryItem[] allItems = Inventory.inv.allItems;
		foreach (InventoryItem inventoryItem in allItems)
		{
			if ((bool)inventoryItem.fish)
			{
				allFish.Add(inventoryItem);
			}
			if ((bool)inventoryItem.bug)
			{
				allBugs.Add(inventoryItem);
			}
		}
		paintingsOnDisplay = new Texture2D[4];
		PhotoManager.manage.displayedPhotos = new PhotoDetails[paintingsOnDisplay.Length];
		fishDonated = new bool[allFish.Count];
		bugsDonated = new bool[allBugs.Count];
		clientNeedsToRequest = true;
	}
}
