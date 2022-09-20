using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class AnimalCarryBox : NetworkBehaviour
{
	[SyncVar(hook = "onAnimalIdSet")]
	public int animalId;

	[SyncVar]
	public int variation;

	[SyncVar]
	public string animalName;

	public AnimalMakeSounds makeSoundOfAnimalInside;

	public ASound boxOpensSound;

	public Transform[] boxSides;

	private PickUpAndCarry myCarry;

	private Animator myAnim;

	public GameObject interactBox;

	public SpriteRenderer[] pictures;

	public int NetworkanimalId
	{
		get
		{
			return animalId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref animalId))
			{
				int oldId = animalId;
				SetSyncVar(value, ref animalId, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onAnimalIdSet(oldId, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public int Networkvariation
	{
		get
		{
			return variation;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref variation))
			{
				int num = variation;
				SetSyncVar(value, ref variation, 2uL);
			}
		}
	}

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
				SetSyncVar(value, ref animalName, 4uL);
			}
		}
	}

	private void Start()
	{
		myCarry = GetComponent<PickUpAndCarry>();
		myAnim = GetComponent<Animator>();
	}

	public override void OnStartClient()
	{
		onAnimalIdSet(animalId, animalId);
	}

	public void releaseAnimal()
	{
		FarmAnimalManager.manage.spawnNewFarmAnimalWithDetails(animalId, variation, animalName, base.transform.position);
	}

	public void onAnimalIdSet(int oldId, int newId)
	{
		AnimalMakeSounds component = AnimalManager.manage.allAnimals[newId].GetComponent<AnimalMakeSounds>();
		if ((bool)component)
		{
			makeSoundOfAnimalInside.animalSoundPool = component.animalSoundPool;
			makeSoundOfAnimalInside.soundCheckTime = component.soundCheckTime;
			StartCoroutine(makeSoundOfAnimalInside.makeSoundForBox());
		}
		NetworkanimalId = newId;
		pictures[0].sprite = AnimalManager.manage.allAnimals[animalId].GetComponent<FarmAnimal>().defualtIcon;
		pictures[1].sprite = AnimalManager.manage.allAnimals[animalId].GetComponent<FarmAnimal>().defualtIcon;
	}

	public void playOpenBoxEffect()
	{
		playSmokeParticles();
		playPlacedDownSound();
	}

	private void playSmokeParticles()
	{
		for (int i = 0; i < boxSides.Length; i++)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], boxSides[i].position);
		}
	}

	private void playPlacedDownSound()
	{
		SoundManager.manage.playASoundAtPoint(boxOpensSound, base.transform.position);
	}

	public void OpenOnServerWhenOnFloor()
	{
		StartCoroutine(waitUntilOnGround());
	}

	private IEnumerator waitUntilOnGround()
	{
		while (Mathf.Abs(base.transform.position.y - myCarry.dropToPos) <= 0.15f)
		{
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		RpcOpen();
		yield return new WaitForSeconds(0.3f);
		releaseAnimal();
		yield return new WaitForSeconds(3f);
		NetworkServer.Destroy(base.gameObject);
	}

	[ClientRpc]
	public void RpcOpen()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(AnimalCarryBox), "RpcOpen", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	public void setUp(int setId, int setVariation, string setName)
	{
		NetworkanimalId = setId;
		Networkvariation = setVariation;
		NetworkanimalName = setName;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcOpen()
	{
		myAnim.SetBool("Open", true);
		interactBox.SetActive(false);
	}

	protected static void InvokeUserCode_RpcOpen(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOpen called on server.");
		}
		else
		{
			((AnimalCarryBox)obj).UserCode_RpcOpen();
		}
	}

	static AnimalCarryBox()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalCarryBox), "RpcOpen", InvokeUserCode_RpcOpen);
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(animalId);
			writer.WriteInt(variation);
			writer.WriteString(animalName);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(animalId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(variation);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteString(animalName);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = animalId;
			NetworkanimalId = reader.ReadInt();
			if (!SyncVarEqual(num, ref animalId))
			{
				onAnimalIdSet(num, animalId);
			}
			int num2 = variation;
			Networkvariation = reader.ReadInt();
			string text = animalName;
			NetworkanimalName = reader.ReadString();
			return;
		}
		long num3 = (long)reader.ReadULong();
		if ((num3 & 1L) != 0L)
		{
			int num4 = animalId;
			NetworkanimalId = reader.ReadInt();
			if (!SyncVarEqual(num4, ref animalId))
			{
				onAnimalIdSet(num4, animalId);
			}
		}
		if ((num3 & 2L) != 0L)
		{
			int num5 = variation;
			Networkvariation = reader.ReadInt();
		}
		if ((num3 & 4L) != 0L)
		{
			string text2 = animalName;
			NetworkanimalName = reader.ReadString();
		}
	}
}
