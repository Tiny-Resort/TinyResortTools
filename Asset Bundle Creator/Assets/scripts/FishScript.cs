using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class FishScript : NetworkBehaviour
{
	private AnimalAI myAi;

	public FishType myFishType;

	public LayerMask lureLayer;

	public Transform lureInterestedIn;

	public Transform fishMouth;

	public NetworkFishingRod fishingRodInterestedIn;

	private WaitForSeconds myWaitTime;

	private Animator myAnim;

	private AnimateAnimalAI animalAnim;

	private bool canSearch = true;

	private bool fishInterestedAndBiting;

	private void Start()
	{
		animalAnim = GetComponent<AnimateAnimalAI>();
		myWaitTime = new WaitForSeconds(Random.Range(0.5f, 1f));
		myAnim = GetComponent<Animator>();
	}

	public override void OnStartServer()
	{
		myAi = GetComponent<AnimalAI>();
		clearInterestInLure();
		StartCoroutine(checkForLure());
	}

	private void OnDisable()
	{
		if (base.isServer)
		{
			fishingRodInterestedIn = null;
		}
	}

	private IEnumerator checkForLure()
	{
		while (true)
		{
			yield return myWaitTime;
			Vector3 position = base.transform.position + base.transform.forward;
			position.y = 0.6f;
			if (canSearch && Physics.CheckSphere(position, 3.5f, lureLayer))
			{
				Collider[] array = Physics.OverlapSphere(position, 3.5f, lureLayer);
				if (array.Length != 0)
				{
					lureInterestedIn = array[0].transform;
					fishingRodInterestedIn = lureInterestedIn.GetComponentInParent<BobberScript>().connectedToRod.GetComponentInParent<NetworkFishingRod>();
					StartCoroutine(fishRandomBites());
					while ((bool)lureInterestedIn && fishingRodInterestedIn.fishOnLine == -1)
					{
						if ((fishInterestedAndBiting && lureInterestedIn.parent.tag == "Scare") || (fishInterestedAndBiting && fishingRodInterestedIn.fishOnLine != -1))
						{
							lureInterestedIn = null;
							fishingRodInterestedIn = null;
							StartCoroutine(cantSearchTimer());
							break;
						}
						myAi.setRunningFrom(null);
						myAi.myAgent.SetDestination(lureInterestedIn.position);
						yield return myWaitTime;
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
	}

	public void fishStartScared(Transform scaredOfBobber)
	{
		StartCoroutine(cantSearchTimer());
	}

	private IEnumerator cantSearchTimer()
	{
		canSearch = false;
		yield return new WaitForSeconds(3f);
		canSearch = true;
	}

	private IEnumerator fishRandomBites()
	{
		int fakeBites = 0;
		animalAnim.makeFishInterested(true);
		while ((bool)fishingRodInterestedIn && fishingRodInterestedIn.fishOnLine == -1)
		{
			yield return new WaitForSeconds(Random.Range(1f, 3f));
			if (!lureInterestedIn || !(Vector3.Distance(base.transform.position, lureInterestedIn.position) < 3f))
			{
				continue;
			}
			if (!fishInterestedAndBiting)
			{
				yield return new WaitForSeconds(Random.Range(1f, 2f));
			}
			fishInterestedAndBiting = true;
			fakeBites += Random.Range(0, 10);
			if (fakeBites >= 9)
			{
				RpcFishBite();
				yield return new WaitForSeconds(0.2f);
				fishingRodInterestedIn.biteLine(base.netId);
				yield return new WaitForSeconds(0.7f);
				if ((bool)lureInterestedIn)
				{
					myAi.setRunningFrom(lureInterestedIn);
				}
				clearInterestInLure();
				StartCoroutine(cantSearchTimer());
				break;
			}
			RpcFakeFishBite(fishingRodInterestedIn.netId);
		}
		animalAnim.makeFishInterested(false);
		fishInterestedAndBiting = false;
	}

	private IEnumerator fakeFishBite()
	{
		myAnim.SetTrigger("Bitting");
		yield return new WaitForSeconds(0.2f);
		if ((bool)fishingRodInterestedIn)
		{
			fishingRodInterestedIn.fakeBitLine();
		}
		if ((bool)fishingRodInterestedIn && fishingRodInterestedIn.isLocalPlayer)
		{
			InputMaster.input.doRumble(0.1f, 6f);
		}
	}

	[ClientRpc]
	private void RpcFishBite()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(FishScript), "RpcFishBite", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcFakeFishBite(uint rodId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(rodId);
		SendRPCInternal(typeof(FishScript), "RpcFakeFishBite", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcFishBite()
	{
		myAnim.SetTrigger("Bitting");
	}

	protected static void InvokeUserCode_RpcFishBite(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFishBite called on server.");
		}
		else
		{
			((FishScript)obj).UserCode_RpcFishBite();
		}
	}

	protected void UserCode_RpcFakeFishBite(uint rodId)
	{
		if (!base.isServer)
		{
			fishingRodInterestedIn = NetworkIdentity.spawned[rodId].GetComponent<NetworkFishingRod>();
		}
		StartCoroutine(fakeFishBite());
	}

	protected static void InvokeUserCode_RpcFakeFishBite(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFakeFishBite called on server.");
		}
		else
		{
			((FishScript)obj).UserCode_RpcFakeFishBite(reader.ReadUInt());
		}
	}

	static FishScript()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(FishScript), "RpcFishBite", InvokeUserCode_RpcFishBite);
		RemoteCallHelper.RegisterRpcDelegate(typeof(FishScript), "RpcFakeFishBite", InvokeUserCode_RpcFakeFishBite);
	}
}
