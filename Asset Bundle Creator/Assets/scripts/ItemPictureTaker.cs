using System.Collections;
using System.IO;
using UnityEngine;

public class ItemPictureTaker : MonoBehaviour
{
	public bool takePhoto;

	private RenderTexture photoTexture;

	[Header("Object ------")]
	public bool spawnObject;

	public InventoryItem itemToSpawn;

	public InventoryItem[] listOfItemsToSpawn;

	private GameObject currentlySpawned;

	[Header("Default ------")]
	public Camera photoCamera;

	public Transform spawnPos;

	[Header("Positions ------")]
	public Transform fishPos;

	public Transform axesAndPickaxes;

	public Transform placeablePos;

	public Transform pathPos;

	public Transform eatablePos;

	public Transform bugPos;

	public Transform underWaterCreature;

	public void Update()
	{
		if (takePhoto)
		{
			if (listOfItemsToSpawn.Length != 0)
			{
				StartCoroutine(takeMultiPictures());
			}
			else
			{
				takePhotoAndSave(itemToSpawn.itemName);
			}
			takePhoto = false;
		}
		if (spawnObject)
		{
			spawnObject = false;
			if ((bool)currentlySpawned)
			{
				Object.Destroy(currentlySpawned);
			}
			if ((bool)itemToSpawn)
			{
				spawnNewObjectInPos(itemToSpawn);
			}
		}
	}

	public void takePhotoAndSave(string photoName)
	{
		int num = 512;
		photoTexture = new RenderTexture(num, num, 32);
		photoTexture.filterMode = FilterMode.Trilinear;
		photoTexture.antiAliasing = 3;
		photoCamera.targetTexture = photoTexture;
		Texture2D texture2D = new Texture2D(num, num, TextureFormat.ARGB32, false);
		photoCamera.Render();
		RenderTexture.active = photoTexture;
		texture2D.ReadPixels(new Rect(0f, 0f, num, num), 0, 0);
		byte[] bytes = texture2D.EncodeToPNG();
		File.WriteAllBytes(ScreenShotName(num, num, photoName), bytes);
	}

	public static string ScreenShotName(int width, int height, string name)
	{
		SaveLoad.saveOrLoad.createPhotoDir();
		return string.Format("{0}/Photos/{1}.png", SaveLoad.saveOrLoad.saveSlot(), name);
	}

	public void spawnNewObjectInPos(InventoryItem itemToSpawnThisTime)
	{
		int invItemId = Inventory.inv.getInvItemId(itemToSpawnThisTime);
		if ((bool)itemToSpawnThisTime.fish)
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, fishPos);
		}
		else if ((bool)itemToSpawnThisTime.bug)
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, bugPos);
		}
		else if ((bool)itemToSpawnThisTime.underwaterCreature)
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, underWaterCreature);
		}
		else if ((bool)itemToSpawnThisTime.consumeable)
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, eatablePos);
		}
		else if (itemToSpawnThisTime.damageStone || itemToSpawnThisTime.damageWood || itemToSpawnThisTime.isATool)
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, axesAndPickaxes);
		}
		else if ((bool)itemToSpawnThisTime.placeable)
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, placeablePos);
		}
		else if (itemToSpawnThisTime.placeableTileType != -1 && !itemToSpawnThisTime.isATool)
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, pathPos);
		}
		else
		{
			currentlySpawned = Object.Instantiate(itemToSpawnThisTime.itemPrefab, spawnPos);
		}
		Animator[] componentsInChildren = currentlySpawned.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		currentlySpawned.transform.localPosition = Vector3.zero;
		if (currentlySpawned.transform.parent == placeablePos)
		{
			currentlySpawned.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
		}
		else
		{
			currentlySpawned.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
		SetItemTexture component = currentlySpawned.GetComponent<SetItemTexture>();
		if ((bool)component)
		{
			component.setTexture(Inventory.inv.allItems[invItemId]);
			if ((bool)component.changeSize)
			{
				component.changeSizeOfTrans(Inventory.inv.allItems[invItemId].transform.localScale);
			}
		}
	}

	private IEnumerator takeMultiPictures()
	{
		for (int i = 0; i < listOfItemsToSpawn.Length; i++)
		{
			if ((bool)currentlySpawned)
			{
				Object.Destroy(currentlySpawned);
			}
			yield return null;
			spawnNewObjectInPos(listOfItemsToSpawn[i]);
			yield return null;
			takePhotoAndSave(listOfItemsToSpawn[i].itemName);
			yield return null;
		}
	}
}
