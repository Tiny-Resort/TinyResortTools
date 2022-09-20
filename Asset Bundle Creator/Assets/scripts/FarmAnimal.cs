using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class FarmAnimal : NetworkBehaviour
{
	[SyncVar]
	private string animalName;

	[SyncVar]
	private int animalRelationship;

	[SyncVar]
	private int animalAge;

	[SyncVar]
	public bool hasBeenPatted;

	private FarmAnimalDetails myDetails;

	public Sprite defualtIcon;

	public int baseValue = 5000;

	public bool canPat = true;

	private AnimalAI myAi;

	public RandomDropFromAnimator dropper;

	public ASound animalPatSound;

	public int growUpAge = 5;

	public FarmAnimal growsInto;

	public Transform patParticlePos;

	public int patParticleAmount = 15;

	public FarmAnimalHarvest canBeHarvested;

	public int animalHandlingLevelToBuy = 1;

	public TileObject animalHouse;

	public AnimalAI_Sleep animalSleeps;

	private static WaitForSeconds petWait;

	public string NetworkanimalName
	{
		get
		{
			return animalName;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref animalName))
			{
				string text = animalName;
				SetSyncVar(value, ref animalName, 1uL);
			}
		}
	}

	public int NetworkanimalRelationship
	{
		get
		{
			return animalRelationship;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref animalRelationship))
			{
				int num = animalRelationship;
				SetSyncVar(value, ref animalRelationship, 2uL);
			}
		}
	}

	public int NetworkanimalAge
	{
		get
		{
			return animalAge;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref animalAge))
			{
				int num = animalAge;
				SetSyncVar(value, ref animalAge, 4uL);
			}
		}
	}

	public bool NetworkhasBeenPatted
	{
		get
		{
			return hasBeenPatted;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref hasBeenPatted))
			{
				bool flag = hasBeenPatted;
				SetSyncVar(value, ref hasBeenPatted, 8uL);
			}
		}
	}

	private void Awake()
	{
		myAi = GetComponent<AnimalAI>();
		animalSleeps = GetComponent<AnimalAI_Sleep>();
	}

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isFarmAnimal = this;
	}

	public override void OnStartClient()
	{
	}

	public override void OnStartServer()
	{
	}

	public void OnDisable()
	{
		if (base.isServer)
		{
			myDetails.setPosition(base.transform.position);
		}
	}

	public string getAnimalName()
	{
		return animalName;
	}

	public int getRelationship()
	{
		return animalRelationship;
	}

	public int getAnimalAge()
	{
		return animalAge;
	}

	public string getLicenceLevelText()
	{
		if (animalHandlingLevelToBuy <= 0)
		{
			return "";
		}
		string text = "";
		for (int i = 0; i < animalHandlingLevelToBuy; i++)
		{
			text += "I";
		}
		return text;
	}

	public FarmAnimalDetails getDetails()
	{
		return myDetails;
	}

	public void setUpAnimalServer(FarmAnimalDetails details, bool animalGrew)
	{
		myDetails = details;
		if (!animalGrew)
		{
			details.agentListId = FarmAnimalManager.manage.addActiveAgentAndReturnIndexId(this);
		}
		else
		{
			FarmAnimalManager.manage.activeAnimalAgents[details.agentListId] = this;
		}
		animalFindsSleepSpot(details);
		updateAnimalDetails(details);
	}

	public bool animalFindsSleepSpot(FarmAnimalDetails details)
	{
		FarmAnimalHouseFloor farmAnimalHouseFloor = FarmAnimalManager.manage.findHouseForAnimal(details, base.transform.position);
		animalSleeps.setAnimalSleepSpot(farmAnimalHouseFloor);
		if (farmAnimalHouseFloor == null && details.hasHouse())
		{
			details.clearFromHouse();
			return false;
		}
		return true;
	}

	public void updateAnimalDetails(FarmAnimalDetails updateDetails)
	{
		NetworkanimalName = updateDetails.animalName;
		NetworkanimalAge = updateDetails.animalAge;
		NetworkanimalRelationship = updateDetails.animalRelationShip;
		NetworkhasBeenPatted = updateDetails.hasBeenPatted;
		if ((bool)canBeHarvested && myDetails.ateYesterDay && myDetails.hasHouse())
		{
			canBeHarvested.onAnimalHarvest(myDetails.ateYesterDay, myDetails.ateYesterDay);
		}
	}

	public void onFarmAnimalDeath()
	{
		NotificationManager.manage.createChatNotification(animalName + " has passed away");
		if (base.isServer)
		{
			myDetails.onAnimalDeath();
		}
	}

	public void sendPetHomeNotification()
	{
		NotificationManager.manage.createChatNotification(animalName + " fainted and returned home");
	}

	public void checkEffectOnPet(uint netId)
	{
		RpcPetAnimal();
		if ((bool)myAi.isAPet())
		{
			myAi.GetComponent<AnimalAI_Pet>().askToFollowOrStayThroughPat(netId);
		}
	}

	[ClientRpc]
	public void RpcPetAnimal()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(FarmAnimal), "RpcPetAnimal", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	public void harvestAnimalAnimation()
	{
		if ((bool)myAi)
		{
			myAi.GetComponent<Animator>().SetTrigger("Pat");
			StartCoroutine(petTimer());
		}
	}

	public void checkIfNeedsToGoHomeForSleep()
	{
		bool isNightTime = RealWorldTimeLight.time.isNightTime;
	}

	private IEnumerator petTimer()
	{
		canPat = false;
		if (base.isServer && myAi.myAgent.isActiveAndEnabled && myAi.myAgent.isOnNavMesh)
		{
			myAi.myAgent.isStopped = true;
		}
		if ((bool)animalPatSound)
		{
			SoundManager.manage.playASoundAtPoint(animalPatSound, base.transform.position);
		}
		if (!patParticlePos)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[32], base.transform.position, patParticleAmount);
		}
		else
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[32], patParticlePos.position, patParticleAmount);
		}
		yield return petWait;
		if (base.isServer && myAi.myAgent.isActiveAndEnabled && myAi.myAgent.isOnNavMesh)
		{
			myAi.myAgent.isStopped = false;
		}
		canPat = true;
	}

	public void setEaten(bool hasEaten)
	{
		myDetails.hasEaten = hasEaten;
	}

	public bool checkIfCanBuy()
	{
		if (LicenceManager.manage.allLicences[9].getCurrentLevel() >= animalHandlingLevelToBuy)
		{
			return true;
		}
		return false;
	}

	static FarmAnimal()
	{
		petWait = new WaitForSeconds(2.5f);
		RemoteCallHelper.RegisterRpcDelegate(typeof(FarmAnimal), "RpcPetAnimal", InvokeUserCode_RpcPetAnimal);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPetAnimal()
	{
		if (base.isServer)
		{
			myDetails.hasBeenPatted = true;
			NetworkhasBeenPatted = true;
		}
		if (!animalSleeps || ((bool)animalSleeps && !animalSleeps.checkIfSleeping()))
		{
			myAi.GetComponent<Animator>().SetTrigger("Pat");
		}
		StartCoroutine(petTimer());
	}

	protected static void InvokeUserCode_RpcPetAnimal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPetAnimal called on server.");
		}
		else
		{
			((FarmAnimal)obj).UserCode_RpcPetAnimal();
		}
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteString(animalName);
			writer.WriteInt(animalRelationship);
			writer.WriteInt(animalAge);
			writer.WriteBool(hasBeenPatted);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteString(animalName);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(animalRelationship);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteInt(animalAge);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteBool(hasBeenPatted);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			string text = animalName;
			NetworkanimalName = reader.ReadString();
			int num = animalRelationship;
			NetworkanimalRelationship = reader.ReadInt();
			int num2 = animalAge;
			NetworkanimalAge = reader.ReadInt();
			bool flag = hasBeenPatted;
			NetworkhasBeenPatted = reader.ReadBool();
			return;
		}
		long num3 = (long)reader.ReadULong();
		if ((num3 & 1L) != 0L)
		{
			string text2 = animalName;
			NetworkanimalName = reader.ReadString();
		}
		if ((num3 & 2L) != 0L)
		{
			int num4 = animalRelationship;
			NetworkanimalRelationship = reader.ReadInt();
		}
		if ((num3 & 4L) != 0L)
		{
			int num5 = animalAge;
			NetworkanimalAge = reader.ReadInt();
		}
		if ((num3 & 8L) != 0L)
		{
			bool flag2 = hasBeenPatted;
			NetworkhasBeenPatted = reader.ReadBool();
		}
	}
}
