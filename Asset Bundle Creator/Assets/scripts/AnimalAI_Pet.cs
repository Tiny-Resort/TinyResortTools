using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.AI;

public class AnimalAI_Pet : NetworkBehaviour
{
	[SyncVar(hook = "onFollowChange")]
	public uint followingId;

	public Transform followingTransform;

	private AnimalAI_Attack attacks;

	private AnimalAI myAi;

	public LayerMask myEnemies;

	public LayerMask characterLayer;

	public TileObject[] findTileObject;

	[SyncVar(hook = "onTrackingChange")]
	public bool tracking;

	private CharNetworkAnimator connectedChar;

	private WaitForSeconds followWait = new WaitForSeconds(0.1f);

	private Vector3 currentlyLookingForPos = Vector3.zero;

	private bool signalling;

	public uint NetworkfollowingId
	{
		get
		{
			return followingId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref followingId))
			{
				uint oldFollow = followingId;
				SetSyncVar(value, ref followingId, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onFollowChange(oldFollow, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public bool Networktracking
	{
		get
		{
			return tracking;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref tracking))
			{
				bool old = tracking;
				SetSyncVar(value, ref tracking, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, true);
					onTrackingChange(old, value);
					setSyncVarHookGuard(2uL, false);
				}
			}
		}
	}

	public override void OnStartServer()
	{
		attacks = GetComponent<AnimalAI_Attack>();
		myAi = GetComponent<AnimalAI>();
		StartCoroutine(setUp());
		myAi.myAgent.avoidancePriority = Random.Range(96, 99);
	}

	public override void OnStartClient()
	{
		onTrackingChange(tracking, tracking);
	}

	public void onTrackingChange(bool old, bool newTracking)
	{
		Networktracking = newTracking;
		if (findTileObject.Length != 0)
		{
			GetComponent<Animator>().SetBool("Tracking", newTracking);
		}
	}

	private IEnumerator setUp()
	{
		while (!myAi.myAgent.isActiveAndEnabled && !myAi.myAgent.isOnNavMesh)
		{
			yield return null;
		}
		StartCoroutine(follow());
	}

	public void onFollowChange(uint oldFollow, uint newFollow)
	{
		NetworkfollowingId = newFollow;
		if (!base.isServer)
		{
			return;
		}
		if (followingId == 0)
		{
			followingTransform = null;
			connectedChar = null;
			return;
		}
		if (NetworkIdentity.spawned.ContainsKey(newFollow))
		{
			followingTransform = NetworkIdentity.spawned[newFollow].transform;
		}
		else
		{
			followingTransform = null;
		}
		connectedChar = followingTransform.GetComponent<CharNetworkAnimator>();
		myAi.myAgent.SetDestination(followingTransform.position - followingTransform.forward * 2f + new Vector3(Random.Range(-4, 4), 0f, Random.Range(-4, 4)));
	}

	public void setNewFollowTo(uint newFollow)
	{
		if (base.isServer)
		{
			if (followingId == 0)
			{
				NetworkfollowingId = newFollow;
			}
			else if (followingId == newFollow)
			{
				NetworkfollowingId = 0u;
			}
		}
	}

	private IEnumerator follow()
	{
		while (true)
		{
			if ((bool)followingTransform && !myAi.isDead())
			{
				if (myAi.myAgent.isActiveAndEnabled && myAi.myAgent.isOnNavMesh)
				{
					float distance = Vector3.Distance(followingTransform.position, base.transform.position);
					if (distance >= 8f)
					{
						if (currentlyLookingForPos != Vector3.zero)
						{
							myAi.myAgent.speed = myAi.getBaseSpeed() / 2f;
						}
						else if (!attacks || !attacks.currentlyAttacking)
						{
							myAi.myAgent.speed = myAi.getBaseSpeed() * 2.5f;
						}
					}
					else if (currentlyLookingForPos != Vector3.zero)
					{
						myAi.myAgent.speed = myAi.getBaseSpeed() / 2f;
					}
					else if (!attacks || !attacks.currentlyAttacking)
					{
						myAi.myAgent.speed = myAi.getBaseSpeed();
					}
					if (distance > 8f && !attacks.currentlyAttacking && currentlyLookingForPos == Vector3.zero)
					{
						myAi.myAgent.SetDestination(followingTransform.position - followingTransform.forward * 2f + new Vector3(Random.Range(-4, 4), 0f, Random.Range(-4, 4)));
					}
					float timer = 0f;
					while (myAi.myAgent.isActiveAndEnabled && myAi.myAgent.isOnNavMesh && Mathf.Abs(followingTransform.position.y - myAi.myAgent.transform.position.y) >= 1.85f && connectedChar.grounded && !boolCheckIfCanRunToChar() && timer < 3f)
					{
						timer += Time.deltaTime;
						stopAllTracking();
						Vector3 destination = base.transform.position + (followingTransform.position - base.transform.position).normalized * 1.5f;
						destination.y = WorldManager.manageWorld.heightMap[(int)destination.x / 2, (int)destination.z / 2];
						myAi.myAgent.SetDestination(destination);
						Vector3 checkPos = base.transform.position + (followingTransform.position - base.transform.position).normalized * 6f;
						checkPos.y = WorldManager.manageWorld.heightMap[(int)checkPos.x / 2, (int)checkPos.z / 2];
						float num = Mathf.Abs(base.transform.position.y - checkPos.y);
						if ((base.transform.position.y < checkPos.y && num <= 3.25f && num >= 1.85f) || (base.transform.position.y > checkPos.y && num >= 1.85f))
						{
							myAi.myAgent.updateRotation = false;
							myAi.myAgent.SetDestination(base.transform.position);
							Vector3 toPosition = new Vector3(checkPos.x, base.transform.position.y, checkPos.z) - base.transform.position;
							float step = 200f * Time.deltaTime;
							while (Vector3.Angle(base.transform.forward, toPosition) > 20f)
							{
								toPosition = new Vector3(checkPos.x, base.transform.position.y, checkPos.z) - base.transform.position;
								myAi.myAgent.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(toPosition), step);
								base.transform.rotation = myAi.myAgent.transform.rotation;
								yield return null;
							}
							myAi.myAgent.transform.rotation = Quaternion.LookRotation(toPosition);
							base.transform.rotation = myAi.myAgent.transform.rotation;
							RpcPlayJump();
							yield return new WaitForSeconds(0.45f);
							myAi.myAgent.Warp(checkPos);
							yield return new WaitForSeconds(1f);
							myAi.myAgent.updateRotation = true;
						}
						yield return null;
					}
					if (myAi.myAgent.isActiveAndEnabled && myAi.myAgent.isOnNavMesh)
					{
						if (distance > 12f)
						{
							if ((bool)attacks)
							{
								attacks.setNewCurrentlyAttacking(null);
								myAi.myAgent.speed = myAi.getBaseSpeed() * 2f;
							}
							stopAllTracking();
							if (myAi.checkDestination(followingTransform.position) != base.transform.position)
							{
								myAi.myAgent.SetDestination(followingTransform.position);
							}
							else
							{
								Vector3 normalized = (followingTransform.position - base.transform.position).normalized;
								normalized = myAi.myAgent.transform.position + normalized * 10f;
								normalized = new Vector3(normalized.x, WorldManager.manageWorld.heightMap[(int)normalized.x / 2, (int)normalized.z / 2], normalized.z);
								myAi.myAgent.SetDestination(normalized);
							}
						}
						else
						{
							if ((bool)attacks && !attacks.currentlyAttacking)
							{
								Transform transform = checkForEnemies();
								if ((bool)transform)
								{
									attacks.setNewCurrentlyAttacking(transform);
									myAi.myAgent.SetDestination(transform.position);
									stopAllTracking();
								}
							}
							if (findTileObject.Length != 0 && (bool)attacks && !attacks.currentlyAttacking)
							{
								checkForTileObjects();
							}
						}
					}
				}
				else
				{
					Vector3 normalized2 = (followingTransform.position - base.transform.position).normalized;
					normalized2 = myAi.myAgent.transform.position + normalized2;
					normalized2 = new Vector3(normalized2.x, WorldManager.manageWorld.heightMap[(int)normalized2.x / 2, (int)normalized2.z / 2], normalized2.z);
					myAi.myAgent.transform.position = normalized2;
					if ((bool)attacks)
					{
						attacks.setNewCurrentlyAttacking(null);
					}
					stopAllTracking();
				}
			}
			yield return followWait;
		}
	}

	public void checkForTileObjects()
	{
		if (currentlyLookingForPos == Vector3.zero)
		{
			stopAllTracking();
			if (Random.Range(0, 250) == 1)
			{
				currentlyLookingForPos = WorldManager.manageWorld.findTileObjectAround(base.transform.position, findTileObject);
			}
		}
		if (!(currentlyLookingForPos != Vector3.zero))
		{
			return;
		}
		if (!WorldManager.manageWorld.waterMap[Mathf.RoundToInt(currentlyLookingForPos.x / 2f), Mathf.RoundToInt(currentlyLookingForPos.z / 2f)])
		{
			Networktracking = true;
			if (Vector3.Distance(base.transform.position, currentlyLookingForPos) < 2.5f)
			{
				faceTracking();
				myAi.myAgent.SetDestination(base.transform.position);
				if (!signalling)
				{
					RpcStartSignal();
					signalling = true;
				}
				if (signalling && Random.Range(0, 125) == 2)
				{
					stopAllTracking();
					currentlyLookingForPos = Vector3.zero;
				}
			}
			else
			{
				myAi.myAgent.SetDestination(new Vector3(currentlyLookingForPos.x, WorldManager.manageWorld.heightMap[(int)currentlyLookingForPos.x / 2, (int)currentlyLookingForPos.z / 2], currentlyLookingForPos.z));
			}
			if (WorldManager.manageWorld.onTileMap[(int)currentlyLookingForPos.x / 2, (int)currentlyLookingForPos.z / 2] != findTileObject[0].tileObjectId)
			{
				stopAllTracking();
			}
		}
		else
		{
			stopAllTracking();
		}
	}

	private void stopAllTracking()
	{
		currentlyLookingForPos = Vector3.zero;
		Networktracking = false;
		if (signalling)
		{
			RpcStopSignal();
			signalling = false;
		}
	}

	public void faceTracking()
	{
		Vector3 forward = new Vector3(currentlyLookingForPos.x, base.transform.position.y, currentlyLookingForPos.z) - base.transform.position;
		myAi.myAgent.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(forward), 200f * Time.deltaTime);
		base.transform.rotation = myAi.myAgent.transform.rotation;
	}

	private Transform checkForEnemies()
	{
		if (Physics.CheckSphere(base.transform.position, 15f, myEnemies))
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, 15f, myEnemies);
			if (array.Length != 0)
			{
				int num = 0;
				float num2 = 9f;
				bool flag = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (!(array[i].transform.position.y > 0f))
					{
						continue;
					}
					AnimalAI componentInParent = array[i].GetComponentInParent<AnimalAI>();
					if (componentInParent.isAttackingOrBeingAttackedBy(followingTransform) && array[i].transform != base.transform && !componentInParent.isAPet())
					{
						float num3 = Vector3.Distance(base.transform.position, array[i].transform.position);
						if (num3 < num2)
						{
							num = i;
							num2 = num3;
							flag = true;
						}
					}
				}
				if (flag)
				{
					return array[num].transform;
				}
			}
		}
		return null;
	}

	[ClientRpc]
	private void RpcStopSignal()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(AnimalAI_Pet), "RpcStopSignal", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcStartSignal()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(AnimalAI_Pet), "RpcStartSignal", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	public bool boolCheckIfCanRunToChar()
	{
		Vector3 vector = base.transform.position + (followingTransform.position - base.transform.position).normalized * 8f;
		vector.y = WorldManager.manageWorld.heightMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2];
		NavMeshHit hit;
		if (NavMesh.SamplePosition(followingTransform.position, out hit, 1.5f, myAi.myAgent.areaMask) && myAi.myAgent.isActiveAndEnabled && myAi.myAgent.isOnNavMesh)
		{
			NavMeshPath navMeshPath = new NavMeshPath();
			myAi.myAgent.CalculatePath(hit.position, navMeshPath);
			if (navMeshPath.status == NavMeshPathStatus.PathComplete)
			{
				return true;
			}
		}
		return false;
	}

	[ClientRpc]
	private void RpcPlayJump()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(AnimalAI_Pet), "RpcPlayJump", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator agentJumps(Vector3 jumpToPos)
	{
		bool jumpComplete = false;
		bool midWay = false;
		Vector3 midwayPoint = Vector3.Lerp(base.transform.position, jumpToPos, 0.6f) + Vector3.up * 5f;
		Vector3 vel = Vector3.zero;
		while (!jumpComplete)
		{
			if (!midWay)
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, midwayPoint, ref vel, 0.1f);
				if (Vector3.Distance(base.transform.position, midwayPoint) < 0.1f)
				{
					vel = Vector3.zero;
					midWay = true;
				}
			}
			else
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, jumpToPos, ref vel, 0.15f);
				if (Vector3.Distance(base.transform.position, jumpToPos) < 1f)
				{
					jumpComplete = true;
				}
			}
			yield return null;
		}
	}

	public void onPetDeath()
	{
		SoundManager.manage.playASoundAtPoint(SoundManager.manage.animalDiesSound, base.transform.position);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position, 20);
		GetComponent<FarmAnimal>().sendPetHomeNotification();
		GetComponent<AnimalAI_Sleep>().sendAnimalToSleep();
	}

	public void askToFollowOrStayThroughPat(uint newId)
	{
		if (newId == followingId)
		{
			NetworkfollowingId = 0u;
		}
		else if (followingId == 0 && newId != 0)
		{
			NetworkfollowingId = newId;
		}
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcStopSignal()
	{
		GetComponent<Animator>().SetBool("Signal", false);
	}

	protected static void InvokeUserCode_RpcStopSignal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStopSignal called on server.");
		}
		else
		{
			((AnimalAI_Pet)obj).UserCode_RpcStopSignal();
		}
	}

	protected void UserCode_RpcStartSignal()
	{
		GetComponent<Animator>().SetBool("Signal", true);
	}

	protected static void InvokeUserCode_RpcStartSignal(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStartSignal called on server.");
		}
		else
		{
			((AnimalAI_Pet)obj).UserCode_RpcStartSignal();
		}
	}

	protected void UserCode_RpcPlayJump()
	{
		GetComponent<Animator>().SetTrigger("Jump");
	}

	protected static void InvokeUserCode_RpcPlayJump(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayJump called on server.");
		}
		else
		{
			((AnimalAI_Pet)obj).UserCode_RpcPlayJump();
		}
	}

	static AnimalAI_Pet()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalAI_Pet), "RpcStopSignal", InvokeUserCode_RpcStopSignal);
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalAI_Pet), "RpcStartSignal", InvokeUserCode_RpcStartSignal);
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalAI_Pet), "RpcPlayJump", InvokeUserCode_RpcPlayJump);
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteUInt(followingId);
			writer.WriteBool(tracking);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteUInt(followingId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(tracking);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			uint num = followingId;
			NetworkfollowingId = reader.ReadUInt();
			if (!SyncVarEqual(num, ref followingId))
			{
				onFollowChange(num, followingId);
			}
			bool flag = tracking;
			Networktracking = reader.ReadBool();
			if (!SyncVarEqual(flag, ref tracking))
			{
				onTrackingChange(flag, tracking);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			uint num3 = followingId;
			NetworkfollowingId = reader.ReadUInt();
			if (!SyncVarEqual(num3, ref followingId))
			{
				onFollowChange(num3, followingId);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			bool flag2 = tracking;
			Networktracking = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref tracking))
			{
				onTrackingChange(flag2, tracking);
			}
		}
	}
}
