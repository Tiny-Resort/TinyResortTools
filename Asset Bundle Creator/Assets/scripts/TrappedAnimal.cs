using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;

public class TrappedAnimal : NetworkBehaviour
{
	[SyncVar(hook = "onTrapAnimal")]
	public int trappedAnimalId;

	[SyncVar(hook = "onVariationSet")]
	public int trappedAnimalVariation;

	public InventoryItem trapItemDropAfterOpen;

	public GameObject[] trappedAnimals;

	public Animator anim;

	public NavMeshObstacle obs;

	public bool hasBeenOpenedLocal;

	public GameObject interactBox;

	private PickUpAndCarry myCarry;

	private int animalInsideHealth;

	[SyncVar]
	public bool caught = true;

	public int NetworktrappedAnimalId
	{
		get
		{
			return trappedAnimalId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref trappedAnimalId))
			{
				int old = trappedAnimalId;
				SetSyncVar(value, ref trappedAnimalId, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onTrapAnimal(old, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public int NetworktrappedAnimalVariation
	{
		get
		{
			return trappedAnimalVariation;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref trappedAnimalVariation))
			{
				int old = trappedAnimalVariation;
				SetSyncVar(value, ref trappedAnimalVariation, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, true);
					onVariationSet(old, value);
					setSyncVarHookGuard(2uL, false);
				}
			}
		}
	}

	public bool Networkcaught
	{
		get
		{
			return caught;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref caught))
			{
				bool flag = caught;
				SetSyncVar(value, ref caught, 4uL);
			}
		}
	}

	private void Start()
	{
		onTrapAnimal(trappedAnimalId, trappedAnimalId);
		onVariationSet(trappedAnimalVariation, trappedAnimalVariation);
		myCarry = GetComponent<PickUpAndCarry>();
		anim.SetBool("Caught", caught);
	}

	public int getAnimalInsideHealth()
	{
		return animalInsideHealth;
	}

	public void setAnimalInsideHealthDif(int newHealth)
	{
		animalInsideHealth = newHealth;
	}

	private void onTrapAnimal(int old, int newId)
	{
		NetworktrappedAnimalId = newId;
		for (int i = 0; i < trappedAnimals.Length; i++)
		{
			if (i == newId)
			{
				trappedAnimals[i].SetActive(true);
				trappedAnimals[i].transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
			}
			else
			{
				trappedAnimals[i].SetActive(false);
			}
		}
	}

	private void onVariationSet(int old, int newVar)
	{
		NetworktrappedAnimalVariation = newVar;
		if (trappedAnimalVariation == 0)
		{
			return;
		}
		for (int i = 0; i < trappedAnimals.Length; i++)
		{
			if (i == trappedAnimalId)
			{
				trappedAnimals[i].GetComponent<SetAnimalVariationTrap>().setVariationNo(trappedAnimalId, trappedAnimalVariation);
			}
		}
	}

	[ClientRpc]
	public void RpcOpen()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(TrappedAnimal), "RpcOpen", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	public void OpenOnServerWhenOnFloor()
	{
		StartCoroutine(waitToOpenDelay());
	}

	private IEnumerator waitToOpenDelay()
	{
		obs.enabled = false;
		interactBox.SetActive(false);
		while (Mathf.Abs(base.transform.position.y - myCarry.dropToPos) <= 0.15f)
		{
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		if (!myCarry.delivered)
		{
			RpcOpen();
			if (base.isServer)
			{
				Vector3 position = base.transform.position;
				NetworkNavMesh.nav.SpawnAnAnimalOnTile(trappedAnimalId * 10 + trappedAnimalVariation, (int)(position.x / 2f), (int)(position.z / 2f), null, animalInsideHealth, myCarry.getLastCarriedBy());
				yield return new WaitForSeconds(0.5f);
				NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(trapItemDropAfterOpen), 1, base.transform.position);
				NetworkServer.Destroy(base.gameObject);
			}
		}
	}

	private IEnumerator openTheTrap()
	{
		if (!myCarry.delivered)
		{
			obs.enabled = false;
			interactBox.SetActive(false);
			anim.SetTrigger("Open");
			for (int i = 0; i < trappedAnimals.Length; i++)
			{
				trappedAnimals[i].SetActive(false);
			}
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position, 20);
			ParticleManager.manage.emitAttackParticle(base.transform.position);
			yield return new WaitForSeconds(0.5f);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position, 20);
		}
	}

	public void startFreeSelfRoutine()
	{
		StartCoroutine(animalNotCaught());
	}

	private IEnumerator animalNotCaught()
	{
		anim.SetBool("Caught", caught);
		Networkcaught = false;
		yield return new WaitForSeconds(Random.Range(3f, 6f));
		if (!myCarry.delivered)
		{
			GetComponent<Damageable>().attackAndDoDamage(5, null, 0f);
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcOpen()
	{
		StartCoroutine(openTheTrap());
	}

	protected static void InvokeUserCode_RpcOpen(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOpen called on server.");
		}
		else
		{
			((TrappedAnimal)obj).UserCode_RpcOpen();
		}
	}

	static TrappedAnimal()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(TrappedAnimal), "RpcOpen", InvokeUserCode_RpcOpen);
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(trappedAnimalId);
			writer.WriteInt(trappedAnimalVariation);
			writer.WriteBool(caught);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(trappedAnimalId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(trappedAnimalVariation);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(caught);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = trappedAnimalId;
			NetworktrappedAnimalId = reader.ReadInt();
			if (!SyncVarEqual(num, ref trappedAnimalId))
			{
				onTrapAnimal(num, trappedAnimalId);
			}
			int num2 = trappedAnimalVariation;
			NetworktrappedAnimalVariation = reader.ReadInt();
			if (!SyncVarEqual(num2, ref trappedAnimalVariation))
			{
				onVariationSet(num2, trappedAnimalVariation);
			}
			bool flag = caught;
			Networkcaught = reader.ReadBool();
			return;
		}
		long num3 = (long)reader.ReadULong();
		if ((num3 & 1L) != 0L)
		{
			int num4 = trappedAnimalId;
			NetworktrappedAnimalId = reader.ReadInt();
			if (!SyncVarEqual(num4, ref trappedAnimalId))
			{
				onTrapAnimal(num4, trappedAnimalId);
			}
		}
		if ((num3 & 2L) != 0L)
		{
			int num5 = trappedAnimalVariation;
			NetworktrappedAnimalVariation = reader.ReadInt();
			if (!SyncVarEqual(num5, ref trappedAnimalVariation))
			{
				onVariationSet(num5, trappedAnimalVariation);
			}
		}
		if ((num3 & 4L) != 0L)
		{
			bool flag2 = caught;
			Networkcaught = reader.ReadBool();
		}
	}
}
