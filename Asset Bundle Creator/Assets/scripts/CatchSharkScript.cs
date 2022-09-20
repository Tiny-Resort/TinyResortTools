using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class CatchSharkScript : NetworkBehaviour
{
	public AnimalAI myAi;

	public AnimalAI_Attack myAttack;

	public LayerMask lureLayer;

	private WaitForSeconds myWaitTime = new WaitForSeconds(0.75f);

	private Transform lureInterestedIn;

	private NetworkFishingRod fishingRodInterestedIn;

	[SyncVar(hook = "onLandChange")]
	private bool isOnLand;

	private bool fishInterestedAndBiting;

	private int chaseDistanceWater = 15;

	public ASound sharkJumpSound;

	private Coroutine returningCoroutine;

	public bool NetworkisOnLand
	{
		get
		{
			return isOnLand;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref isOnLand))
			{
				bool oldLand = isOnLand;
				SetSyncVar(value, ref isOnLand, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onLandChange(oldLand, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public override void OnStartServer()
	{
		myAi = GetComponent<AnimalAI>();
		NetworkisOnLand = false;
		returningCoroutine = null;
		checkIfIsOnLand();
		clearInterestInLure();
		StartCoroutine(checkForLure());
	}

	private IEnumerator checkForLure()
	{
		while (true)
		{
			yield return myWaitTime;
			Vector3 position = base.transform.position + base.transform.forward;
			position.y = 0.6f;
			if (!myAttack.currentlyAttacking && !myAi.isInjuredAndNeedsToRunAway() && Physics.CheckSphere(position, 3.5f, lureLayer))
			{
				Collider[] array = Physics.OverlapSphere(position, 3.5f, lureLayer);
				if (array.Length != 0 && (bool)array[0].GetComponentInParent<BobberScript>() && array[0].GetComponentInParent<BobberScript>().catchSharks)
				{
					lureInterestedIn = array[0].transform;
					fishingRodInterestedIn = lureInterestedIn.GetComponentInParent<BobberScript>().connectedToRod.GetComponentInParent<NetworkFishingRod>();
					float waitTimer = Random.Range(1.5f, 2.5f);
					while (lureInterestedIn.gameObject.activeInHierarchy && waitTimer > 0f && isOnLand)
					{
						yield return null;
						waitTimer -= Time.deltaTime;
					}
					while (lureInterestedIn.gameObject.activeInHierarchy && !isOnLand)
					{
						myAi.myAgent.SetDestination(lureInterestedIn.position);
						GetComponent<AnimateAnimalAI>().makeFishInterested(true);
						yield return null;
						if (myAi.myAgent.stoppingDistance <= Vector3.Distance(myAi.myAgent.transform.position, lureInterestedIn.transform.position))
						{
							RpcBiteHook();
							yield return new WaitForSeconds(1f);
							fishingRodInterestedIn.HookShark(base.netId);
						}
					}
				}
			}
			lureInterestedIn = null;
			fishingRodInterestedIn = null;
		}
	}

	public void clearInterestInLure()
	{
		lureInterestedIn = null;
		fishingRodInterestedIn = null;
		GetComponent<AnimateAnimalAI>().makeFishInterested(false);
	}

	[ClientRpc]
	public void RpcBiteHook()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CatchSharkScript), "RpcBiteHook", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator windUpAndAttack()
	{
		GetComponent<Animator>().SetTrigger("WindUp");
		yield return new WaitForSeconds(0.1f);
		GetComponent<Animator>().SetTrigger("BasicAttack");
	}

	public void checkIfIsOnLand()
	{
		if (WorldManager.manageWorld.heightMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)] > -1 || !WorldManager.manageWorld.waterMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)])
		{
			NetworkisOnLand = true;
			myAi.myAgent.areaMask = AnimalManager.manage.allAnimals[3].myAgent.areaMask;
			myAi.waterOnly = false;
			if (returningCoroutine == null)
			{
				returningCoroutine = StartCoroutine(returnToWaterWait());
			}
			myAttack.chaseDistance = (float)chaseDistanceWater / 5f;
		}
		else
		{
			NetworkisOnLand = false;
			myAi.myAgent.areaMask = AnimalManager.manage.allAnimals[5].myAgent.areaMask;
			myAi.waterOnly = true;
			myAttack.chaseDistance = chaseDistanceWater;
		}
	}

	public void onLandChange(bool oldLand, bool newLand)
	{
		NetworkisOnLand = newLand;
		GetComponent<Animator>().SetBool("InWater", !isOnLand);
		if (!newLand)
		{
			GetComponent<FootStepEffects>().enterDeepWater();
		}
	}

	private IEnumerator returnToWaterWait()
	{
		while (isOnLand)
		{
			yield return null;
			checkIfIsOnLand();
			if (!myAttack.currentlyAttacking && myAi.checkIfHasArrivedAtDestination())
			{
				Vector3 destination = WorldManager.manageWorld.findClosestWaterTile(base.transform.position, 10, true, -1);
				myAi.myAgent.SetDestination(destination);
			}
		}
		returningCoroutine = null;
	}

	public void playSharkJumpSound()
	{
		SoundManager.manage.playASoundAtPoint(sharkJumpSound, base.transform.position);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcBiteHook()
	{
		StartCoroutine(windUpAndAttack());
	}

	protected static void InvokeUserCode_RpcBiteHook(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcBiteHook called on server.");
		}
		else
		{
			((CatchSharkScript)obj).UserCode_RpcBiteHook();
		}
	}

	static CatchSharkScript()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(CatchSharkScript), "RpcBiteHook", InvokeUserCode_RpcBiteHook);
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(isOnLand);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(isOnLand);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = isOnLand;
			NetworkisOnLand = reader.ReadBool();
			if (!SyncVarEqual(flag, ref isOnLand))
			{
				onLandChange(flag, isOnLand);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool flag2 = isOnLand;
			NetworkisOnLand = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref isOnLand))
			{
				onLandChange(flag2, isOnLand);
			}
		}
	}
}
