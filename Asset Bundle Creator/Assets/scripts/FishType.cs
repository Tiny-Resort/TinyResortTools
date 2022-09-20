using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class FishType : NetworkBehaviour
{
	[SyncVar(hook = "setFishType")]
	private int myFishType = -1;

	public SkinnedMeshRenderer fishRen;

	public Transform fishSizeTrans;

	public FishType fishTypeForDummy;

	private Mesh defualtMesh;

	public NameTag myNameTag;

	public int NetworkmyFishType
	{
		get
		{
			return myFishType;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref myFishType))
			{
				int old = myFishType;
				SetSyncVar(value, ref myFishType, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					setFishType(old, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	private void Awake()
	{
		defualtMesh = fishRen.sharedMesh;
	}

	private void Start()
	{
		AnimalManager.manage.lookAtFishBook.AddListener(openBook);
	}

	public override void OnStartClient()
	{
		if (myFishType != -1)
		{
			setFishType(myFishType, myFishType);
			openBook();
		}
	}

	public override void OnStartServer()
	{
		WorldManager.manageWorld.changeDayEvent.AddListener(resetFishTypeOnNewDay);
	}

	private void OnDisable()
	{
		WorldManager.manageWorld.changeDayEvent.RemoveListener(resetFishTypeOnNewDay);
	}

	public void resetFishTypeOnNewDay()
	{
		if (base.isServer)
		{
			generateFishForEnviroment();
		}
	}

	public int getFishTypeId()
	{
		return myFishType;
	}

	public void generateFishForEnviroment()
	{
		int num = (int)base.transform.position.x / 2;
		int num2 = (int)base.transform.position.z / 2;
		InventoryItem item = ((WorldManager.manageWorld.tileTypeMap[num, num2] == 14) ? AnimalManager.manage.mangroveFish.getInventoryItem() : ((GenerateMap.generate.checkBiomType(num, num2) == 11) ? ((num2 <= 500) ? AnimalManager.manage.southernOceanFish.getInventoryItem() : AnimalManager.manage.northernOceanFish.getInventoryItem()) : ((GenerateMap.generate.checkBiomType(num, num2) == 9) ? AnimalManager.manage.northernOceanFish.getInventoryItem() : ((GenerateMap.generate.checkBiomType(num, num2) == 10 || GenerateMap.generate.checkBiomType(num, num2) == 8) ? AnimalManager.manage.southernOceanFish.getInventoryItem() : ((GenerateMap.generate.checkBiomType(num, num2) != 2) ? AnimalManager.manage.riverFish.getInventoryItem() : AnimalManager.manage.billabongFish.getInventoryItem())))));
		NetworkmyFishType = Inventory.inv.getInvItemId(item);
		if (Inventory.inv.allItems[myFishType].fish.mySeason.myRarity == SeasonAndTime.rarity.SuperRare && Random.Range(0, 5) != 4)
		{
			generateFishForEnviroment();
		}
		if (Inventory.inv.allItems[myFishType].fish.mySeason.myRarity == SeasonAndTime.rarity.VeryRare && Random.Range(0, 3) == 1)
		{
			generateFishForEnviroment();
		}
	}

	public void setFishType(int old, int fishType)
	{
		NetworkmyFishType = fishType;
		if (myFishType == -1)
		{
			return;
		}
		if (!fishTypeForDummy)
		{
			fishSizeTrans.localScale = Inventory.inv.allItems[fishType].fish.fishScale();
			fishRen.material = Inventory.inv.allItems[fishType].equipable.material;
			if ((bool)Inventory.inv.allItems[fishType].equipable.useAltMesh)
			{
				fishRen.sharedMesh = Inventory.inv.allItems[fishType].equipable.useAltMesh;
			}
			else
			{
				fishRen.sharedMesh = defualtMesh;
			}
		}
		else
		{
			fishSizeTrans.localScale = Inventory.inv.allItems[fishType].fish.fishScale();
			fishRen.material = Inventory.inv.allItems[fishType].equipable.material;
			if ((bool)Inventory.inv.allItems[fishType].equipable.useAltMesh)
			{
				fishRen.sharedMesh = Inventory.inv.allItems[fishType].equipable.useAltMesh;
			}
			else
			{
				fishRen.sharedMesh = defualtMesh;
			}
		}
		if ((bool)GetComponent<AnimateAnimalAI>())
		{
			GetComponent<AnimateAnimalAI>().setScaleSwimDif();
		}
	}

	public InventoryItem getFishInvItem()
	{
		return Inventory.inv.allItems[myFishType];
	}

	public int getFishInvId()
	{
		return myFishType;
	}

	public void openBook()
	{
		if (base.gameObject.activeSelf && AnimalManager.manage.fishBookOpen)
		{
			if (myFishType >= 0 && PediaManager.manage.isInPedia(myFishType))
			{
				myNameTag.turnOn("<sprite=11>" + Inventory.inv.allItems[myFishType].value + "\n" + Inventory.inv.allItems[myFishType].getInvItemName());
			}
			else
			{
				myNameTag.turnOn("<sprite=11>????\n?????");
			}
			myNameTag.transform.parent = null;
			myNameTag.transform.localScale = Vector3.one;
			myNameTag.transform.parent = base.transform;
		}
		else
		{
			myNameTag.turnOff();
		}
	}

	private void MirrorProcessed()
	{
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(myFishType);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(myFishType);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = myFishType;
			NetworkmyFishType = reader.ReadInt();
			if (!SyncVarEqual(num, ref myFishType))
			{
				setFishType(num, myFishType);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			int num3 = myFishType;
			NetworkmyFishType = reader.ReadInt();
			if (!SyncVarEqual(num3, ref myFishType))
			{
				setFishType(num3, myFishType);
			}
		}
	}
}
