using I2.Loc;
using UnityEngine;

public class InventoryItem : MonoBehaviour
{
	public enum staminaType
	{
		None = 0,
		Farming = 1,
		Foraging = 2,
		Mining = 3,
		Fishing = 4,
		BugCatching = 5,
		Hunting = 6
	}

	public enum typeOfAnimation
	{
		ShovelAnimation = 0,
		Pickaxe = 1,
		Axe = 2,
		BugNet = 3,
		FishingRod = 4,
		Bat = 5,
		Scyth = 6,
		Spear = 7,
		MetalDetector = 8,
		Hammer = 9,
		WateringCan = 10,
		UpgradedWateringCan = 11,
		UpgradedHoe = 12,
		Knife = 13,
		Glider = 14
	}

	public string itemName;

	public string itemDescription;

	public int value;

	public Sprite itemSprite;

	public GameObject itemPrefab;

	public GameObject altDropPrefab;

	[Header("Special Options-------")]
	public InventoryItem changeToWhenUsed;

	public bool changeToAndStillUseFuel;

	public bool hideHighlighter;

	[Header("Animation Settings")]
	public bool hasUseAnimationStance = true;

	public bool useRightHandAnim;

	public typeOfAnimation myAnimType;

	[Header("Placeable --------------")]
	public TileObject placeable;

	public bool burriedPlaceable;

	public bool ignoreOnTileObject;

	public int[] canBePlacedOntoTileType;

	public int placeableTileType = -1;

	[Header("Placeable On To OTher Tile Object --------------")]
	public TileObject[] canBePlacedOnToTileObject;

	public int statusToChangeToWhenPlacedOnTop;

	[Header("Item Type --------------")]
	public bool isStackable = true;

	public int maxStack = -1;

	public bool isATool;

	public bool isPowerTool;

	public bool isFurniture;

	public bool canBePlacedInHouse;

	public bool canBeUsedInShops;

	public bool isRequestable;

	public bool isUniqueItem;

	[Header("Fuel and Stamina Options-------")]
	public staminaType staminaTypeUse;

	public bool hasFuel;

	public int fuelMax;

	public int fuelOnUse = 5;

	public Color customFuelColour;

	[Header("Weapon Info --------------")]
	public float weaponDamage = 1f;

	public float weaponKnockback = 2.5f;

	public bool canBlock;

	[Header("Damage Tile Object Info --------------")]
	public float damagePerAttack = 1f;

	public bool damageWood;

	public bool damageHardWood;

	public bool damageMetal;

	public bool damageStone;

	public bool damageHardStone;

	public bool damageSmallPlants;

	public int changeToHeightTiles;

	public bool onlyChangeHeightPaths = true;

	public bool anyHeight;

	[Header("Damage Tile Types --------------")]
	public bool grassGrowable;

	public bool canDamagePath;

	public bool canDamageDirt;

	public bool canDamageStone;

	public bool canDamageTilledDirt;

	public bool canDamageWetTilledDirt;

	public bool canDamageFertilizedSoil;

	public bool canDamageWetFertilizedSoil;

	public int[] resultingTileType;

	public bool ignoreTwoArmAnim;

	public bool isDeed;

	[Header("Other Settings---------")]
	public bool hasColourVariation;

	public bool isRepairable;

	public bool canUseUnderWater;

	[Header("Spawn a world object or vehicle ----")]
	public GameObject spawnPlaceable;

	[Header("Milestone & Licence ----")]
	public LicenceManager.LicenceTypes requiredToBuy;

	public int requiredLicenceLevel = 1;

	public DailyTaskGenerator.genericTaskType assosiatedTask;

	public DailyTaskGenerator.genericTaskType taskWhenSold;

	[Header("Other Scripts --------------")]
	public Equipable equipable;

	public Recipe craftable;

	public Consumeable consumeable;

	public ItemChange itemChange;

	public BugIdentity bug;

	public FishIdentity fish;

	public UnderWaterCreature underwaterCreature;

	public Relic relic;

	private int itemId = -1;

	public void setItemId(int setTo)
	{
		itemId = setTo;
	}

	public int getItemId()
	{
		if (itemId == -1)
		{
			int num = Inventory.inv.itemIdBackUp(this);
			if (num != -1)
			{
				return num;
			}
			Debug.LogError("Item Id of -1 was given");
		}
		return itemId;
	}

	public string getLicenceLevelText()
	{
		if (requiredLicenceLevel <= 1)
		{
			return "";
		}
		string text = "LVL ";
		for (int i = 0; i < requiredLicenceLevel; i++)
		{
			text += "I";
		}
		return text;
	}

	public bool checkIfCanBuy()
	{
		if (requiredToBuy == LicenceManager.LicenceTypes.None)
		{
			return true;
		}
		if (LicenceManager.manage.allLicences[(int)requiredToBuy].getCurrentLevel() >= requiredLicenceLevel)
		{
			return true;
		}
		return false;
	}

	public Sprite getSprite()
	{
		if ((bool)equipable && equipable.cloths)
		{
			if (equipable.hat && !equipable.useOwnSprite)
			{
				return EquipWindow.equip.hatSprite;
			}
			if (equipable.dress)
			{
				return EquipWindow.equip.dressSprite;
			}
			if (equipable.shirt)
			{
				return EquipWindow.equip.shirtSprite;
			}
			if (equipable.pants)
			{
				return EquipWindow.equip.pantSprite;
			}
			if (equipable.shoes)
			{
				return EquipWindow.equip.shoeSprite;
			}
		}
		return itemSprite;
	}

	public bool checkIfStackable()
	{
		if (isStackable && !isATool && !hasColourVariation && !hasFuel)
		{
			return true;
		}
		return false;
	}

	public bool canDamageTileTypes()
	{
		if (canDamageStone || canDamagePath || canDamageDirt || canDamageTilledDirt)
		{
			return true;
		}
		return false;
	}

	public int getResultingPlaceableTileType(int placingOnToType)
	{
		if (resultingTileType.Length != 0)
		{
			for (int i = 0; i < resultingTileType.Length; i++)
			{
				if (canBePlacedOntoTileType[i] == placingOnToType)
				{
					return resultingTileType[i];
				}
			}
			return 0;
		}
		return placeableTileType;
	}

	public string getInvItemName()
	{
		LocalizedString localizedString = "InventoryItemNames/InvItem_" + Inventory.inv.getInvItemId(this);
		if ((string)localizedString == null)
		{
			return itemName;
		}
		return localizedString;
	}

	public string getItemDescription(int itemId)
	{
		LocalizedString localizedString = "InventoryItemDescriptions/InvDesc_" + itemId;
		if ((string)localizedString == null)
		{
			return itemDescription;
		}
		return localizedString;
	}

	public float getStaminaCost()
	{
		return CharLevelManager.manage.getStaminaCost((int)(staminaTypeUse - 1));
	}

	public void checkForTask()
	{
		if (assosiatedTask != 0)
		{
			DailyTaskGenerator.generate.doATask(assosiatedTask);
		}
	}
}
