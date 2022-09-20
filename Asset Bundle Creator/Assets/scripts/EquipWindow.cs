using System.Collections;
using UnityEngine;

public class EquipWindow : MonoBehaviour
{
	public static EquipWindow equip;

	public Transform slotWindow;

	public InventorySlot hatSlot;

	public InventorySlot faceSlot;

	public InventorySlot shirtSlot;

	public InventorySlot pantsSlot;

	public InventorySlot shoeSlot;

	public InventorySlot idolSlot;

	public TileObject fishTank;

	public TileObject smallFishTank;

	public TileObject largeFishTank;

	public TileObject bugTank;

	public TileObject shirtPlaceable;

	public TileObject hatPlaceable;

	public TileObject pantsPlaceable;

	public TileObject shoePlaceable;

	public GameObject holdingHatOrFaceObject;

	public Mesh defaultShirtMesh;

	public Mesh tShirtMesh;

	public Mesh defualtShoeMesh;

	public Mesh defaultPants;

	public Material underClothes;

	private int headSlotId = -50;

	private int faceSlotId = -1;

	private int shirtSlotId = -1;

	private int pantsSlotId = -1;

	private int shoeSlotId = -1;

	private int idolSlotId = -1;

	public Sprite hatSprite;

	public Sprite shirtSprite;

	public Sprite dressSprite;

	public Sprite jacketSprite;

	public Sprite pantSprite;

	public Sprite shortsSprite;

	public Sprite shoeSprite;

	public Material[] vehicleColours;

	public Color[] vehicleColoursUI;

	public InventoryItem bucketItem;

	private bool slotWindowOpen;

	public InventoryItem minersHelmet;

	public InventoryItem emptyMinersHelmet;

	public InventoryItem spinifexResin;

	public bool keepChanging = true;

	private void Awake()
	{
		equip = this;
	}

	private void Start()
	{
		hatSlot.refreshSlot();
		faceSlot.refreshSlot();
		shirtSlot.refreshSlot();
		pantsSlot.refreshSlot();
		shoeSlot.refreshSlot();
		idolSlot.refreshSlot();
		shirtSlotId = shirtSlot.itemNo;
	}

	public void openEquipWindow()
	{
		if (!keepChanging && !slotWindowOpen && Inventory.inv.invOpen && !GiveNPC.give.giveWindowOpen && !ChestWindow.chests.chestWindowOpen)
		{
			slotWindowOpen = true;
			StartCoroutine(equipWindowOpen());
		}
	}

	public IEnumerator wearingMinersHelmet()
	{
		float minersHelmetTimer = 0f;
		while (hatSlot.itemNo == minersHelmet.getItemId())
		{
			yield return null;
			if (hatSlot.itemNo != minersHelmet.getItemId())
			{
				continue;
			}
			if (minersHelmetTimer < 1f)
			{
				minersHelmetTimer += Time.deltaTime;
				continue;
			}
			minersHelmetTimer = 0f;
			hatSlot.updateSlotContentsAndRefresh(hatSlot.itemNo, Mathf.Clamp(hatSlot.stack - 1, 1, minersHelmet.fuelMax));
			if (hatSlot.stack <= 1)
			{
				if (Inventory.inv.getAmountOfItemInAllSlots(spinifexResin.getItemId()) == 0)
				{
					hatSlot.updateSlotContentsAndRefresh(emptyMinersHelmet.getItemId(), 1);
					Inventory.inv.localChar.CmdChangeHeadId(hatSlot.itemNo);
				}
				else
				{
					Inventory.inv.removeAmountOfItem(spinifexResin.getItemId(), 1);
					hatSlot.stack = hatSlot.itemInSlot.fuelMax;
				}
			}
		}
	}

	private IEnumerator equipWindowOpen()
	{
		yield return null;
		slotWindow.transform.position = new Vector2(Inventory.inv.invSlots[11].transform.position.x, 0f);
		slotWindow.transform.localPosition = new Vector3(slotWindow.transform.localPosition.x - 80f, 0f);
		while (keepChanging || (Inventory.inv.invOpen && !GiveNPC.give.giveWindowOpen && !ChestWindow.chests.chestWindowOpen))
		{
			slotWindow.gameObject.SetActive(true);
			if (hatSlot.itemNo != headSlotId)
			{
				if (hatSlot.itemNo == bucketItem.getItemId() && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.PutBucketOnHead) < 1)
				{
					DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.PutBucketOnHead);
				}
				if (headSlotId != minersHelmet.getItemId() && headSlotId != emptyMinersHelmet.getItemId() && hatSlot.itemNo == minersHelmet.getItemId())
				{
					StartCoroutine(wearingMinersHelmet());
				}
				Inventory.inv.localChar.CmdChangeHeadId(hatSlot.itemNo);
				headSlotId = hatSlot.itemNo;
			}
			if (faceSlot.itemNo != faceSlotId)
			{
				faceSlotId = faceSlot.itemNo;
				Inventory.inv.localChar.CmdChangeFaceId(faceSlot.itemNo);
			}
			if (shirtSlot.itemNo != shirtSlotId)
			{
				shirtSlotId = shirtSlot.itemNo;
				Inventory.inv.localChar.CmdChangeShirtId(shirtSlot.itemNo);
			}
			if (pantsSlot.itemNo != pantsSlotId)
			{
				pantsSlotId = pantsSlot.itemNo;
				Inventory.inv.localChar.CmdChangePantsId(pantsSlot.itemNo);
			}
			if (shoeSlot.itemNo != shoeSlotId)
			{
				shoeSlotId = shoeSlot.itemNo;
				Inventory.inv.localChar.CmdChangeShoesId(shoeSlot.itemNo);
			}
			if (idolSlot.itemNo != idolSlotId)
			{
				if (idolSlot.itemNo != -1)
				{
					Inventory.inv.localChar.GetComponent<CharMovement>().giveIdolStats(idolSlot.itemNo);
				}
				else
				{
					Inventory.inv.localChar.GetComponent<CharMovement>().removeIdolStatus(idolSlotId);
				}
				idolSlotId = idolSlot.itemNo;
			}
			yield return null;
		}
		slotWindowOpen = false;
		slotWindow.gameObject.SetActive(false);
	}

	public int[] getEquipSlotsArray()
	{
		hatSlot.refreshSlot();
		faceSlot.refreshSlot();
		shirtSlot.refreshSlot();
		pantsSlot.refreshSlot();
		shoeSlot.refreshSlot();
		idolSlot.refreshSlot();
		int itemNo = hatSlot.itemNo;
		int num = 0;
		return new int[4] { itemNo, shirtSlot.itemNo, pantsSlot.itemNo, shoeSlot.itemNo };
	}

	public void randomiseCharacter()
	{
		StartCoroutine(equipWindowOpen());
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = -1;
		int num5 = -1;
		if (Random.Range(0, 4) == 2)
		{
			bool flag = false;
			while (!flag)
			{
				num = Random.Range(0, Inventory.inv.allItems.Length);
				if ((bool)Inventory.inv.allItems[num].equipable && Inventory.inv.allItems[num].equipable.hat)
				{
					flag = true;
				}
			}
		}
		bool flag2 = false;
		while (!flag2)
		{
			num2 = Random.Range(0, Inventory.inv.allItems.Length);
			if ((bool)Inventory.inv.allItems[num2].equipable && Inventory.inv.allItems[num2].equipable.shirt)
			{
				flag2 = true;
			}
		}
		bool flag3 = false;
		int num6 = 10;
		while (!flag3)
		{
			num3 = Random.Range(0, Inventory.inv.allItems.Length);
			if ((bool)Inventory.inv.allItems[num3].equipable && Inventory.inv.allItems[num3].equipable.face)
			{
				flag3 = true;
			}
			if (num6 <= 0)
			{
				num3 = -1;
			}
			num6--;
		}
		bool flag4 = false;
		while (!flag4)
		{
			num4 = Random.Range(0, Inventory.inv.allItems.Length);
			if ((bool)Inventory.inv.allItems[num4].equipable && Inventory.inv.allItems[num4].equipable.pants)
			{
				flag4 = true;
			}
		}
		if (Random.Range(0, 3) == 2)
		{
			bool flag5 = false;
			while (!flag5)
			{
				num5 = Random.Range(0, Inventory.inv.allItems.Length);
				if ((bool)Inventory.inv.allItems[num5].equipable && Inventory.inv.allItems[num5].equipable.shoes)
				{
					flag5 = true;
				}
			}
		}
		hatSlot.updateSlotContentsAndRefresh(num, 1);
		faceSlot.updateSlotContentsAndRefresh(num3, 1);
		shirtSlot.updateSlotContentsAndRefresh(num2, 1);
		pantsSlot.updateSlotContentsAndRefresh(num4, 1);
		shoeSlot.updateSlotContentsAndRefresh(num5, 1);
		Inventory.inv.localChar.CmdChangeHairColour(Random.Range(0, CharacterCreatorScript.create.allHairColours.Length));
		Inventory.inv.localChar.CmdChangeHairId(Random.Range(0, 20));
		Inventory.inv.localChar.CmdChangeEyes(Random.Range(0, CharacterCreatorScript.create.allEyeTypes.Length), Random.Range(0, CharacterCreatorScript.create.eyeColours.Length));
		Inventory.inv.localChar.CmdChangeNose(Random.Range(0, CharacterCreatorScript.create.noseMeshes.Length));
	}

	public IEnumerator randomClothes()
	{
		keepChanging = true;
		StartCoroutine(equipWindowOpen());
		while (keepChanging)
		{
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			int num4 = -1;
			int num5 = -1;
			if (Random.Range(0, 4) == 2)
			{
				bool flag = false;
				while (!flag)
				{
					num = Random.Range(0, Inventory.inv.allItems.Length);
					if ((bool)Inventory.inv.allItems[num].equipable && Inventory.inv.allItems[num].equipable.hat)
					{
						flag = true;
					}
				}
			}
			bool flag2 = false;
			while (!flag2)
			{
				num2 = Random.Range(0, Inventory.inv.allItems.Length);
				if ((bool)Inventory.inv.allItems[num2].equipable && Inventory.inv.allItems[num2].equipable.shirt)
				{
					flag2 = true;
				}
			}
			bool flag3 = false;
			int num6 = 10;
			while (!flag3)
			{
				num3 = Random.Range(0, Inventory.inv.allItems.Length);
				if ((bool)Inventory.inv.allItems[num3].equipable && Inventory.inv.allItems[num3].equipable.face)
				{
					flag3 = true;
				}
				if (num6 <= 0)
				{
					num3 = -1;
				}
				num6--;
			}
			bool flag4 = false;
			while (!flag4)
			{
				num4 = Random.Range(0, Inventory.inv.allItems.Length);
				if ((bool)Inventory.inv.allItems[num4].equipable && Inventory.inv.allItems[num4].equipable.pants)
				{
					flag4 = true;
				}
			}
			if (Random.Range(0, 3) == 2)
			{
				bool flag5 = false;
				while (!flag5)
				{
					num5 = Random.Range(0, Inventory.inv.allItems.Length);
					if ((bool)Inventory.inv.allItems[num5].equipable && Inventory.inv.allItems[num5].equipable.shoes)
					{
						flag5 = true;
					}
				}
			}
			hatSlot.updateSlotContentsAndRefresh(num, 1);
			faceSlot.updateSlotContentsAndRefresh(num3, 1);
			shirtSlot.updateSlotContentsAndRefresh(num2, 1);
			pantsSlot.updateSlotContentsAndRefresh(num4, 1);
			shoeSlot.updateSlotContentsAndRefresh(num5, 1);
			yield return new WaitForSeconds(1.5f);
		}
	}
}
