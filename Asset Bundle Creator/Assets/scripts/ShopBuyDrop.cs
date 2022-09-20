using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopBuyDrop : MonoBehaviour
{
	public ShopManager.stallTypes myStallType;

	public int shopStallNo = -1;

	public int myItemId;

	private int previouslyStocked;

	public InventoryItem onlySells;

	public InventoryItemLootTable canSell;

	public GameObject spriteDrop;

	private GameObject itemPrefab;

	public bool gives10;

	public bool disapearwhenBuy = true;

	public Transform createObjectHere;

	public Transform FourSquareFurniture;

	public Transform TwoSquareFurniture;

	public Transform TwoSquareFurnitureVert;

	public MeshRenderer objectMaterialHere;

	public bool hatsOnly;

	public bool faceAccessoriesOnly;

	public bool shirtOnly;

	public bool pantsOnly;

	public bool shoesOnly;

	public bool furnitureOnly;

	public bool furnitureOnTop;

	public bool flooringOnly;

	public bool wallPaperOnly;

	public bool recipesOnly;

	public bool sellsSeasonsCrops;

	public bool usesPermitPoints;

	public NPCSchedual.Locations insideShop;

	public AnimalAI sellsAnimal;

	public GameObject dummyAnimal;

	public CopyShopItem copyTo;

	public TextMeshPro priceTag;

	public Conversation uniqueConversation;

	public Conversation closedConversation;

	private NetStall myNetStall;

	public void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isShopBuyDrop = this;
		if ((bool)sellsAnimal)
		{
			WorldManager.manageWorld.changeDayEvent.AddListener(refreshAnimal);
			return;
		}
		if (!onlySells)
		{
			WorldManager.manageWorld.changeDayEvent.AddListener(changeItem);
			changeItem();
			return;
		}
		WorldManager.manageWorld.changeDayEvent.AddListener(refreshItem);
		if ((bool)onlySells)
		{
			myItemId = Inventory.inv.getInvItemId(onlySells);
		}
		createItem(myItemId);
		previouslyStocked = myItemId;
	}

	private void OnEnable()
	{
		Invoke("refreshConnectionToShopManager", Random.Range(0.1f, 0.25f));
		if ((bool)createObjectHere)
		{
			createObjectHere.gameObject.SetActive(true);
		}
	}

	private void OnDisable()
	{
		if ((bool)createObjectHere)
		{
			createObjectHere.gameObject.SetActive(false);
		}
	}

	public int getRecipePrice()
	{
		return 5000;
	}

	private void refreshConnectionToShopManager()
	{
		if (shopStallNo == -1)
		{
			return;
		}
		myNetStall = ShopManager.manage.connectStall(this);
		if (myNetStall.hasBeenSold)
		{
			sold(false);
		}
		else
		{
			if ((bool)sellsAnimal)
			{
				return;
			}
			if (!onlySells)
			{
				changeItem();
				return;
			}
			if ((bool)onlySells)
			{
				myItemId = Inventory.inv.getInvItemId(onlySells);
			}
			Object.Destroy(itemPrefab);
			createItem(myItemId);
			previouslyStocked = myItemId;
		}
	}

	public void generateRandomItem()
	{
		bool flag = false;
		if (shirtOnly)
		{
			myItemId = RandomObjectGenerator.generate.getRandomShirtOrDressForShop().getItemId();
			flag = true;
		}
		if (hatsOnly)
		{
			myItemId = RandomObjectGenerator.generate.getRandomHat().getItemId();
			flag = true;
		}
		if (pantsOnly)
		{
			myItemId = RandomObjectGenerator.generate.getRandomPants().getItemId();
			flag = true;
		}
		if (shoesOnly)
		{
			myItemId = RandomObjectGenerator.generate.getRandomShoes().getItemId();
			flag = true;
		}
		if (faceAccessoriesOnly)
		{
			myItemId = RandomObjectGenerator.generate.getRandomFaceItem().getItemId();
			flag = true;
		}
		if (furnitureOnly)
		{
			if (furnitureOnTop)
			{
				myItemId = RandomObjectGenerator.generate.getRandomOnTopFurniture().getItemId();
				flag = true;
			}
			else
			{
				myItemId = RandomObjectGenerator.generate.getRandomFurnitureForShop().getItemId();
				flag = true;
			}
		}
		while (!flag)
		{
			myItemId = Random.Range(0, Inventory.inv.allItems.Length);
			if (recipesOnly)
			{
				if ((bool)Inventory.inv.allItems[myItemId].craftable && Inventory.inv.allItems[myItemId].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.CookingTable && Inventory.inv.allItems[myItemId].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.CraftingShop && Inventory.inv.allItems[myItemId].craftable.workPlaceConditions != CraftingManager.CraftingMenuType.Blocked && !Inventory.inv.allItems[myItemId].craftable.isDeed && !Inventory.inv.allItems[myItemId].craftable.learnThroughQuest && !Inventory.inv.allItems[myItemId].craftable.learnThroughLevels && !Inventory.inv.allItems[myItemId].craftable.learnThroughLicence && !CharLevelManager.manage.checkIfIsInStartingRecipes(myItemId))
				{
					flag = true;
				}
			}
			else if (wallPaperOnly || flooringOnly)
			{
				if ((bool)Inventory.inv.allItems[myItemId].equipable && ((flooringOnly && Inventory.inv.allItems[myItemId].equipable.flooring) || (wallPaperOnly && Inventory.inv.allItems[myItemId].equipable.wallpaper)))
				{
					flag = true;
				}
			}
			else if (furnitureOnly)
			{
				if (Inventory.inv.allItems[myItemId].isFurniture)
				{
					flag = true;
				}
			}
			else if (hatsOnly)
			{
				if ((bool)Inventory.inv.allItems[myItemId].equipable && Inventory.inv.allItems[myItemId].equipable.hat)
				{
					flag = true;
				}
			}
			else if (faceAccessoriesOnly)
			{
				if ((bool)Inventory.inv.allItems[myItemId].equipable && Inventory.inv.allItems[myItemId].equipable.face)
				{
					flag = true;
				}
			}
			else if (shoesOnly || pantsOnly || shirtOnly)
			{
				if (shoesOnly && (bool)Inventory.inv.allItems[myItemId].equipable && Inventory.inv.allItems[myItemId].equipable.shoes)
				{
					flag = true;
				}
				if (pantsOnly && (bool)Inventory.inv.allItems[myItemId].equipable && Inventory.inv.allItems[myItemId].equipable.pants)
				{
					flag = true;
				}
				if (shirtOnly && (bool)Inventory.inv.allItems[myItemId].equipable && Inventory.inv.allItems[myItemId].equipable.shirt)
				{
					flag = true;
				}
			}
			else if (Inventory.inv.allItems[myItemId].isATool)
			{
				flag = true;
			}
		}
	}

	public void createItem(int itemId)
	{
		myItemId = itemId;
		if ((bool)createObjectHere || furnitureOnly)
		{
			if (furnitureOnly && Inventory.inv.allItems[itemId].placeable.isMultiTileObject())
			{
				Transform parent;
				if (Inventory.inv.allItems[itemId].placeable.getXSize() == 2 && Inventory.inv.allItems[itemId].placeable.getYSize() == 2)
				{
					parent = FourSquareFurniture;
					FourSquareFurniture.gameObject.SetActive(true);
					createObjectHere.gameObject.SetActive(false);
					TwoSquareFurniture.gameObject.SetActive(false);
					TwoSquareFurnitureVert.gameObject.SetActive(false);
				}
				else if (Inventory.inv.allItems[itemId].placeable.getXSize() == 2)
				{
					parent = TwoSquareFurniture;
					FourSquareFurniture.gameObject.SetActive(false);
					createObjectHere.gameObject.SetActive(false);
					TwoSquareFurniture.gameObject.SetActive(true);
					TwoSquareFurnitureVert.gameObject.SetActive(false);
				}
				else
				{
					parent = TwoSquareFurnitureVert;
					FourSquareFurniture.gameObject.SetActive(false);
					createObjectHere.gameObject.SetActive(false);
					TwoSquareFurniture.gameObject.SetActive(false);
					TwoSquareFurnitureVert.gameObject.SetActive(true);
				}
				itemPrefab = Object.Instantiate(Inventory.inv.allItems[itemId].placeable.gameObject, parent);
			}
			else if (furnitureOnly)
			{
				FourSquareFurniture.gameObject.SetActive(false);
				createObjectHere.gameObject.SetActive(true);
				TwoSquareFurniture.gameObject.SetActive(false);
				itemPrefab = Object.Instantiate(Inventory.inv.allItems[itemId].placeable.gameObject, createObjectHere);
			}
			else if (hatsOnly || faceAccessoriesOnly)
			{
				itemPrefab = Object.Instantiate(Inventory.inv.allItems[itemId].equipable.hatPrefab, createObjectHere);
			}
			else if ((bool)Inventory.inv.allItems[itemId].altDropPrefab)
			{
				itemPrefab = Object.Instantiate(Inventory.inv.allItems[itemId].altDropPrefab, createObjectHere);
			}
			else
			{
				itemPrefab = Object.Instantiate(Inventory.inv.allItems[itemId].itemPrefab, createObjectHere);
			}
			if (furnitureOnly && (bool)itemPrefab.GetComponent<FurnitureStatus>())
			{
				FurnitureStatus component = itemPrefab.GetComponent<FurnitureStatus>();
				if ((bool)component.seatPosition1)
				{
					Object.Destroy(component.seatPosition1.gameObject);
				}
				if ((bool)component.seatPosition2)
				{
					Object.Destroy(component.seatPosition2.gameObject);
				}
				Object.Destroy(component);
			}
			if ((bool)itemPrefab.GetComponent<Animator>())
			{
				Object.Destroy(itemPrefab.GetComponent<Animator>());
			}
			if ((bool)itemPrefab.GetComponent<SetItemTexture>())
			{
				itemPrefab.GetComponent<SetItemTexture>().setTexture(Inventory.inv.allItems[itemId]);
			}
			Light[] componentsInChildren = itemPrefab.GetComponentsInChildren<Light>();
			if (componentsInChildren.Length != 0)
			{
				Light[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].enabled = false;
				}
			}
			itemPrefab.transform.localPosition = Vector3.zero;
			itemPrefab.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
		else if ((bool)objectMaterialHere)
		{
			if (shirtOnly && (bool)Inventory.inv.allItems[myItemId].equipable.shirtMesh)
			{
				objectMaterialHere.gameObject.GetComponent<MeshFilter>().mesh = Inventory.inv.allItems[myItemId].equipable.shirtMesh;
				if ((bool)copyTo)
				{
					copyTo.changeMesh(Inventory.inv.allItems[myItemId].equipable.shirtMesh);
				}
			}
			else if (shirtOnly)
			{
				objectMaterialHere.gameObject.GetComponent<MeshFilter>().mesh = EquipWindow.equip.defaultShirtMesh;
			}
			objectMaterialHere.material = Inventory.inv.allItems[myItemId].equipable.material;
			objectMaterialHere.enabled = true;
			if ((bool)copyTo)
			{
				copyTo.changeMaterial(Inventory.inv.allItems[myItemId].equipable.material);
			}
		}
		else if ((bool)createObjectHere)
		{
			if (hatsOnly || faceAccessoriesOnly)
			{
				itemPrefab = Object.Instantiate(Inventory.inv.allItems[itemId].equipable.hatPrefab, createObjectHere);
			}
			else if ((bool)Inventory.inv.allItems[itemId].altDropPrefab)
			{
				itemPrefab = Object.Instantiate(Inventory.inv.allItems[itemId].altDropPrefab, Vector3.zero, Quaternion.Euler(0f, 0f, 0f), createObjectHere);
			}
			else
			{
				itemPrefab = Object.Instantiate(Inventory.inv.allItems[itemId].itemPrefab, Vector3.zero, Quaternion.Euler(0f, 0f, 0f), createObjectHere);
				Object.Destroy(itemPrefab.GetComponent<Animator>());
			}
		}
		else
		{
			itemPrefab = Object.Instantiate(spriteDrop, base.transform);
			itemPrefab.GetComponent<SpriteLookAtCam>().changeSprite(itemId);
			if (recipesOnly)
			{
				itemPrefab.GetComponent<SpriteLookAtCam>().setAsBluePrint();
			}
			itemPrefab.transform.localPosition = new Vector3(0f, 0f, 1.7f);
		}
		if ((bool)priceTag)
		{
			priceTag.text = ((float)Inventory.inv.allItems[myItemId].value * 2f).ToString("n0");
		}
	}

	private void changeItem()
	{
		Object.Destroy(itemPrefab);
		Random.InitState(NetworkMapSharer.share.mineSeed + (int)base.transform.position.x + (int)base.transform.position.z + (int)base.transform.position.y + (int)((float)((int)base.transform.position.x / (int)base.transform.position.z) + base.transform.position.y) + WorldManager.manageWorld.day * WorldManager.manageWorld.week + WorldManager.manageWorld.year);
		if ((bool)canSell)
		{
			if (sellsSeasonsCrops)
			{
				List<InventoryItem> list = new List<InventoryItem>();
				for (int i = 0; i < ShopManager.manage.allSeeds.Count; i++)
				{
					if (WorldManager.manageWorld.month == 1 && ShopManager.manage.allSeeds[i].placeable.tileObjectGrowthStages.growsInSummer)
					{
						list.Add(ShopManager.manage.allSeeds[i]);
					}
					else if (WorldManager.manageWorld.month == 2 && ShopManager.manage.allSeeds[i].placeable.tileObjectGrowthStages.growsInAutum)
					{
						list.Add(ShopManager.manage.allSeeds[i]);
					}
					if (WorldManager.manageWorld.month == 3 && ShopManager.manage.allSeeds[i].placeable.tileObjectGrowthStages.growsInWinter)
					{
						list.Add(ShopManager.manage.allSeeds[i]);
					}
					if (WorldManager.manageWorld.month == 4 && ShopManager.manage.allSeeds[i].placeable.tileObjectGrowthStages.growsInSpring)
					{
						list.Add(ShopManager.manage.allSeeds[i]);
					}
				}
				canSell.autoFillFromArray(list.ToArray());
			}
			myItemId = Inventory.inv.getInvItemId(canSell.getRandomDropFromTable());
		}
		else
		{
			generateRandomItem();
		}
		createItem(myItemId);
	}

	private void refreshItem()
	{
		if (myItemId == -1)
		{
			myItemId = previouslyStocked;
			Object.Destroy(itemPrefab);
			createItem(myItemId);
		}
	}

	public void TryAndBuyItem()
	{
		if ((canTalkToKeeper() && myItemId != -1) || (canTalkToKeeper() && (bool)sellsAnimal && dummyAnimal.activeSelf))
		{
			if (canTalkToKeeper() && !isKeeperWorking() && (bool)closedConversation)
			{
				ConversationManager.manage.talkToNPC(NPCManager.manage.sign, closedConversation);
			}
			else if ((bool)uniqueConversation)
			{
				GiveNPC.give.dropToBuy = this;
				ConversationManager.manage.talkToNPC(NPCManager.manage.getVendorNPC(insideShop), uniqueConversation);
			}
			else if ((bool)sellsAnimal)
			{
				GiveNPC.give.askAboutBuyingAnimal(this, NPCManager.manage.getVendorNPC(insideShop));
			}
			else if (recipesOnly)
			{
				GiveNPC.give.askAboutBuyingRecipe(this, NPCManager.manage.getVendorNPC(insideShop));
			}
			else
			{
				GiveNPC.give.askAboutBuyingSomething(this, NPCManager.manage.getVendorNPC(insideShop));
			}
		}
	}

	public bool isKeeperWorking()
	{
		if (!NPCManager.manage.getVendorNPC(insideShop))
		{
			return false;
		}
		return NPCManager.manage.getVendorNPC(insideShop).isAtWork();
	}

	public bool canTalkToKeeper()
	{
		if (!NPCManager.manage.getVendorNPC(insideShop))
		{
			if ((bool)closedConversation)
			{
				return true;
			}
			return false;
		}
		return NPCManager.manage.getVendorNPC(insideShop).canBeTalkTo();
	}

	public void checkIfTaskCompelted(int amount = 1)
	{
		if (furnitureOnly)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyFurniture, amount);
		}
		else if (shirtOnly || hatsOnly || pantsOnly || shoesOnly || faceAccessoriesOnly)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyShirt, amount);
		}
		else if (wallPaperOnly)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyWallpaper, amount);
		}
		else if (flooringOnly)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyFlooring, amount);
		}
		else if (!recipesOnly)
		{
			if (myItemId > -1 && (bool)Inventory.inv.allItems[myItemId].placeable && (bool)Inventory.inv.allItems[myItemId].placeable.tileObjectGrowthStages && Inventory.inv.allItems[myItemId].placeable.tileObjectGrowthStages.needsTilledSoil)
			{
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuySeeds, amount);
			}
			else if (myItemId > -1 && Inventory.inv.allItems[myItemId].isATool)
			{
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyATool);
			}
		}
	}

	public void buyTheItem()
	{
		if (disapearwhenBuy)
		{
			if (shopStallNo > -1)
			{
				NetworkMapSharer.share.localChar.CmdBuyItemFromStall((int)myStallType, shopStallNo);
			}
			sold();
		}
	}

	public void sold(bool partsShown = true)
	{
		if ((bool)sellsAnimal && (bool)dummyAnimal)
		{
			dummyAnimal.SetActive(false);
		}
		if ((bool)objectMaterialHere)
		{
			objectMaterialHere.enabled = false;
			if ((bool)copyTo)
			{
				copyTo.disable();
			}
		}
		else
		{
			Object.Destroy(itemPrefab);
		}
		myItemId = -1;
		if (partsShown && (bool)createObjectHere)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], createObjectHere.position, 10);
		}
		if ((bool)priceTag)
		{
			priceTag.text = "<color=red>Sold</color>";
		}
	}

	public void refreshAnimal()
	{
		dummyAnimal.SetActive(true);
	}
}
