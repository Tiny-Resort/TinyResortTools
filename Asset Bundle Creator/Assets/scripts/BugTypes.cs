using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class BugTypes : NetworkBehaviour
{
	[SyncVar(hook = "setUpBug")]
	private int bugType;

	public GameObject bugLight;

	public GameObject crawling;

	public GameObject wingFlying;

	public GameObject jumping;

	public MeshRenderer crawlingRen;

	public MeshRenderer wingFlyingRen;

	public MeshRenderer jumpingRen;

	public AnimalAI myAi;

	public Animator myAnim;

	public AudioSource bugSoundSource;

	public ItemHitBox myAttackBox;

	private GameObject myBugModel;

	public NameTag myNameTag;

	public Transform groundBugTrans;

	[Header("Underwater Creatures-----")]
	public bool isUnderwaterCreature;

	public GameObject stationaryUnderwaterCreatures;

	public GameObject movingUnderwaterCreatures;

	public int NetworkbugType
	{
		get
		{
			return bugType;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref bugType))
			{
				int old = bugType;
				SetSyncVar(value, ref bugType, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					setUpBug(old, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	private void OnDisable()
	{
		if (base.isServer && !isUnderwaterCreature)
		{
			WorldManager.manageWorld.changeDayEvent.RemoveListener(resetBugTypeNewDay);
		}
	}

	private void Start()
	{
		if (!isUnderwaterCreature)
		{
			AnimalManager.manage.lookAtBugBook.AddListener(openBook);
		}
	}

	public void resetBugTypeNewDay()
	{
		getBugType();
	}

	public override void OnStartServer()
	{
		getBugType();
		WorldManager.manageWorld.changeDayEvent.AddListener(resetBugTypeNewDay);
	}

	public void openBook()
	{
		if (base.gameObject.activeSelf && AnimalManager.manage.bugBookOpen)
		{
			if (bugType >= 0 && PediaManager.manage.isInPedia(bugType))
			{
				myNameTag.turnOn("<sprite=11>" + Inventory.inv.allItems[bugType].value + "\n" + Inventory.inv.allItems[bugType].getInvItemName());
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

	public override void OnStartClient()
	{
		if (bugType != -1)
		{
			setUpBug(bugType, bugType);
			if (!isUnderwaterCreature)
			{
				openBook();
			}
		}
	}

	public int getBugTypeId()
	{
		return bugType;
	}

	private void getBugType()
	{
		int num = (int)base.transform.position.x / 2;
		int num2 = (int)base.transform.position.z / 2;
		int num4 = WorldManager.manageWorld.tileTypeMap[num, num2];
		int num3 = GenerateMap.generate.checkBiomType(num, num2);
		InventoryItem item;
		if (isUnderwaterCreature)
		{
			item = ((WorldManager.manageWorld.tileTypeMap[num, num2] != 3 && num3 != 9 && num3 != 9 && num3 != 10 && num3 != 11) ? AnimalManager.manage.underWaterRiverCreatures.getInventoryItem() : AnimalManager.manage.underWaterOceanCreatures.getInventoryItem());
		}
		else
		{
			switch (num3)
			{
			case 1:
				item = AnimalManager.manage.topicalBugs.getInventoryItem();
				break;
			case 2:
			case 3:
				item = AnimalManager.manage.bushlandBugs.getInventoryItem();
				break;
			case 4:
				item = AnimalManager.manage.bushlandBugs.getInventoryItem();
				break;
			case 5:
				item = AnimalManager.manage.desertBugs.getInventoryItem();
				break;
			case 6:
				item = AnimalManager.manage.pineLandBugs.getInventoryItem();
				break;
			case 7:
				item = AnimalManager.manage.plainsBugs.getInventoryItem();
				break;
			default:
				item = AnimalManager.manage.bushlandBugs.getInventoryItem();
				break;
			}
		}
		NetworkbugType = Inventory.inv.getInvItemId(item);
	}

	private void connectBugAnim()
	{
		if ((bool)myBugModel)
		{
			GetComponent<AnimateAnimalAI>().setAnimator(myBugModel.GetComponentInChildren<Animator>());
		}
	}

	private IEnumerator checkForBugInWater()
	{
		while ((bool)groundBugTrans)
		{
			if (groundBugTrans.position.y <= 0.6f && WorldManager.manageWorld.waterMap[Mathf.RoundToInt(groundBugTrans.position.x / 2f), Mathf.RoundToInt(groundBugTrans.position.z / 2f)])
			{
				RpcSplashInWater(base.transform.position);
				NetworkNavMesh.nav.UnSpawnAnAnimal(GetComponent<AnimalAI>(), false);
				break;
			}
			yield return null;
		}
	}

	[ClientRpc]
	private void RpcSplashInWater(Vector3 position)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(position);
		SendRPCInternal(typeof(BugTypes), "RpcSplashInWater", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	public void setUpBug(int old, int newBugType)
	{
		NetworkbugType = newBugType;
		if (bugType == -1)
		{
			return;
		}
		if ((bool)myBugModel)
		{
			Object.Destroy(myBugModel);
		}
		if (!isUnderwaterCreature)
		{
			myBugModel = Object.Instantiate(Inventory.inv.allItems[bugType].bug.insectType, base.transform);
			myBugModel.transform.localPosition = Vector3.zero;
			Invoke("connectBugAnim", 0.25f);
			groundBugTrans = myBugModel.GetComponent<BugAppearance>().setUpBug(Inventory.inv.allItems[bugType]);
			if ((bool)groundBugTrans && base.isServer)
			{
				StartCoroutine(checkForBugInWater());
			}
			if ((bool)Inventory.inv.allItems[bugType].bug.bugSounds)
			{
				bugSoundSource.clip = Inventory.inv.allItems[bugType].bug.bugSounds.getSound();
				bugSoundSource.pitch = Inventory.inv.allItems[bugType].bug.bugSounds.getPitch();
				bugSoundSource.volume = Inventory.inv.allItems[bugType].bug.bugSounds.volume;
				bugSoundSource.loop = Inventory.inv.allItems[bugType].bug.bugSounds.loop;
				bugSoundSource.maxDistance = Inventory.inv.allItems[bugType].bug.bugSounds.maxDistance;
				bugSoundSource.enabled = true;
				if (Inventory.inv.allItems[bugType].bug.bugSounds.loop)
				{
					bugSoundSource.Play();
				}
			}
			else
			{
				bugSoundSource.enabled = false;
			}
			if (Inventory.inv.allItems[bugType].bug.glows)
			{
				bugLight.gameObject.SetActive(true);
			}
			else
			{
				bugLight.gameObject.SetActive(false);
			}
			myAi.setBaseSpeed(Inventory.inv.allItems[bugType].bug.bugBaseSpeed);
			myAi.setAttackType(Inventory.inv.allItems[bugType].bug.attacksWhenClose);
			if (Inventory.inv.allItems[bugType].bug.attacksWhenClose)
			{
				myAi.isSkiddish = false;
				myAttackBox.gameObject.SetActive(true);
				if (Inventory.inv.allItems[bugType].bug.poisonOnAttack)
				{
					myAttackBox.poisonDamage = true;
				}
				else
				{
					myAttackBox.poisonDamage = false;
				}
			}
			else
			{
				myAi.isSkiddish = true;
				myAttackBox.gameObject.SetActive(false);
			}
		}
		else
		{
			myBugModel = Object.Instantiate(Inventory.inv.allItems[bugType].underwaterCreature.creatureModel, base.transform);
			myBugModel.transform.localPosition = Vector3.zero;
			Invoke("connectBugAnim", 0.25f);
			groundBugTrans = myBugModel.GetComponent<BugAppearance>().setUpBug(Inventory.inv.allItems[bugType]);
			if ((bool)groundBugTrans && base.isServer)
			{
				StartCoroutine(checkForBugInWater());
			}
			bugSoundSource.enabled = false;
			myAi.setBaseSpeed(Inventory.inv.allItems[bugType].underwaterCreature.baseSpeed);
		}
	}

	public void playBugNoiseAnimator()
	{
		if ((bool)Inventory.inv.allItems[bugType].bug.bugSounds && !Inventory.inv.allItems[bugType].bug.bugSounds.loop)
		{
			bugSoundSource.PlayOneShot(Inventory.inv.allItems[bugType].bug.bugSounds.getSound());
		}
	}

	public InventoryItem bugInvItem()
	{
		return Inventory.inv.allItems[bugType];
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSplashInWater(Vector3 position)
	{
		ParticleManager.manage.waterSplash(position);
	}

	protected static void InvokeUserCode_RpcSplashInWater(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSplashInWater called on server.");
		}
		else
		{
			((BugTypes)obj).UserCode_RpcSplashInWater(reader.ReadVector3());
		}
	}

	static BugTypes()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(BugTypes), "RpcSplashInWater", InvokeUserCode_RpcSplashInWater);
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(bugType);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(bugType);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = bugType;
			NetworkbugType = reader.ReadInt();
			if (!SyncVarEqual(num, ref bugType))
			{
				setUpBug(num, bugType);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			int num3 = bugType;
			NetworkbugType = reader.ReadInt();
			if (!SyncVarEqual(num3, ref bugType))
			{
				setUpBug(num3, bugType);
			}
		}
	}
}
