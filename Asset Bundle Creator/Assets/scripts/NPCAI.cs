using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityChan;
using UnityEngine;
using UnityEngine.AI;

public class NPCAI : NetworkBehaviour
{
	[SyncVar(hook = "onTalkingChange")]
	public uint talkingTo;

	private Transform talkingToTransform;

	[SyncVar(hook = "onFollowChange")]
	public uint followingNetId;

	public NPCIdentity myId;

	public NPCMapAgent myManager;

	public Animator myAnim;

	public AnimateCharFace faceAnim;

	public bool grounded;

	public LayerMask jumpLayers;

	public LayerMask jumpOverLayers;

	public NavMeshAgent myAgent;

	public Transform following;

	public NPCIk npcIk;

	public bool isSign;

	private bool inJump;

	private Quaternion lastRotation;

	private NPCJob myJob;

	private Vector3 lastPos;

	private Vector3 lastPosAnim;

	public AudioSource myAudio;

	private int baseSpeed = 3;

	private bool inWater;

	public NPCDoesTasks doesTask;

	private bool canWalkOnEntrance;

	private bool isCloseEnoughToFollowing;

	public SpringManager hairSpring;

	public NPCChatBubble chatBubble;

	private Vector3 lastNavPos;

	private RaycastHit hit;

	private float hairVel;

	private float travelSpeed;

	private Coroutine gateRoutine;

	public static WaitForSeconds npcWait;

	private static NavMeshPathStatus cantComplete;

	private bool isSitting;

	private int seatPos;

	private int seatX = -1;

	private int seatY = -1;

	public uint NetworktalkingTo
	{
		get
		{
			return talkingTo;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref talkingTo))
			{
				uint old = talkingTo;
				SetSyncVar(value, ref talkingTo, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onTalkingChange(old, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public uint NetworkfollowingNetId
	{
		get
		{
			return followingNetId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref followingNetId))
			{
				uint oldFollow = followingNetId;
				SetSyncVar(value, ref followingNetId, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, true);
					onFollowChange(oldFollow, value);
					setSyncVarHookGuard(2uL, false);
				}
			}
		}
	}

	public override void OnStartServer()
	{
		doesTask = GetComponent<NPCDoesTasks>();
		NetworkfollowingNetId = 0u;
		NetworktalkingTo = 0u;
		if ((bool)myAgent)
		{
			myAgent.updateRotation = true;
			myAgent.transform.parent = null;
			StartCoroutine(startNPC());
		}
		if (!isSign)
		{
			NetworkMapSharer.share.returnAgents.AddListener(despawnOnEvent);
		}
		if (myManager != null && myManager.getFollowingId() != 0)
		{
			NetworkfollowingNetId = myManager.getFollowingId();
			onFollowChange(followingNetId, followingNetId);
		}
	}

	private void OnEnable()
	{
		grounded = true;
	}

	private void Awake()
	{
		myJob = GetComponent<NPCJob>();
		lastPos = base.transform.position;
	}

	public void OnDisable()
	{
		if (base.isServer)
		{
			if (isSitting)
			{
				MonoBehaviour.print("Standing up from pos: " + seatX + "," + seatY);
				isSitting = false;
				NetworkMapSharer.share.RpcGetUp(Mathf.Clamp(WorldManager.manageWorld.onTileStatusMap[seatX, seatY] - seatPos, 0, 3), seatX, seatY, -1, -1);
			}
			StopAllCoroutines();
			if ((bool)myAgent)
			{
				myAgent.enabled = false;
			}
			NetworkMapSharer.share.returnAgents.RemoveListener(despawnOnEvent);
		}
	}

	public bool isAtWork()
	{
		return myJob.atWork;
	}

	public override void OnStartClient()
	{
		if (!isSign)
		{
			doesTask = GetComponent<NPCDoesTasks>();
			lastRotation = base.transform.rotation;
			if (!base.isServer)
			{
				Object.Destroy(myAgent.gameObject);
			}
		}
	}

	public void moveSpawnSpot(Vector3 newSpawnPos)
	{
		StopAllCoroutines();
		myAgent.enabled = false;
		myAgent.transform.position = newSpawnPos;
		base.transform.position = newSpawnPos;
		StartCoroutine(startNPC());
	}

	private IEnumerator SolveStuck()
	{
		while (true)
		{
			if (!checkIfHasArrivedAtDestination() && Vector3.Distance(lastNavPos, base.transform.position) < 0.1f)
			{
				Vector3 destination = myAgent.destination;
				myAgent.SetDestination(base.transform.position + base.transform.forward + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-8f, 8f)));
				yield return new WaitForSeconds(3f);
				myAgent.ResetPath();
				myAgent.SetDestination(destination);
			}
			lastNavPos = base.transform.position;
			yield return new WaitForSeconds(1f);
		}
	}

	private void LateUpdate()
	{
		animateNPC();
		if (!isSitting && Physics.Raycast(base.transform.position + Vector3.up, Vector3.down, out hit, 2f, jumpLayers))
		{
			base.transform.position = Vector3.Lerp(base.transform.position, new Vector3(base.transform.position.x, hit.point.y, base.transform.position.z), Time.deltaTime * 2f);
			lastPos = base.transform.position;
		}
	}

	private void animateNPC()
	{
		if (isSign)
		{
			return;
		}
		if (Vector3.Distance(base.transform.position, lastPos) >= 8f)
		{
			lastPos = base.transform.position;
		}
		float num = Vector3.Distance(base.transform.position, lastPosAnim);
		if (Time.deltaTime != 0f)
		{
			num /= Time.deltaTime;
		}
		if (num / 6f * 2f < 0.1f && Quaternion.Angle(base.transform.rotation, lastRotation) > 0.2f)
		{
			num = 2.5f;
		}
		if (Vector3.Distance(base.transform.position, lastPosAnim) >= 4f)
		{
			travelSpeed = 0f;
		}
		else if (num > 0.15f)
		{
			travelSpeed = Mathf.Lerp(travelSpeed, num, Time.deltaTime * 6f);
		}
		else
		{
			travelSpeed = Mathf.Lerp(travelSpeed, num, Time.deltaTime * 3f);
		}
		if (base.transform.position.y < -0.35f && base.transform.position.y > -8f && WorldManager.manageWorld.waterMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2])
		{
			myAnim.SetBool(CharNetworkAnimator.swimmingAnimName, true);
		}
		else
		{
			myAnim.SetBool(CharNetworkAnimator.swimmingAnimName, false);
		}
		lastPosAnim = base.transform.position;
		myAnim.SetBool(CharNetworkAnimator.groundedAnimName, grounded);
		if ((bool)hairSpring)
		{
			if (travelSpeed > 0.5f)
			{
				hairSpring.dynamicRatio = Mathf.SmoothDamp(hairSpring.dynamicRatio, Mathf.Clamp(1f - num, 0.15f, 1f), ref hairVel, 0.05f);
			}
			else
			{
				hairSpring.dynamicRatio = Mathf.SmoothDamp(hairSpring.dynamicRatio, Mathf.Clamp(1f - num, 0.15f, 1f), ref hairVel, 0.15f);
			}
		}
		myAnim.SetFloat("WalkSpeed", travelSpeed / 6f * 2f);
		lastRotation = base.transform.rotation;
	}

	private void FixedUpdate()
	{
		if (base.isServer)
		{
			if (!myAgent || !myAgent.enabled)
			{
				if ((bool)myAnim)
				{
					myAnim.SetBool(CharNetworkAnimator.groundedAnimName, true);
				}
				return;
			}
			if (talkingTo == 0 && (bool)following && !doesTask.hasTask)
			{
				if (Vector3.Distance(following.position, base.transform.position) > 4f)
				{
					myAgent.SetDestination(following.position - following.forward * 2f);
					isCloseEnoughToFollowing = false;
				}
				else if (!isCloseEnoughToFollowing)
				{
					isCloseEnoughToFollowing = true;
					myAgent.SetDestination(base.transform.position + base.transform.forward);
				}
			}
			if (!myAgent.isActiveAndEnabled || !myAgent.isOnNavMesh)
			{
				return;
			}
			Vector3 b = myAgent.transform.position;
			if (!checkIfHasArrivedAtDestination() && WorldManager.manageWorld.onTileMap[Mathf.RoundToInt(base.transform.position.x) / 2, Mathf.RoundToInt(base.transform.position.z) / 2] > 0 && (bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[Mathf.RoundToInt(base.transform.position.x) / 2, Mathf.RoundToInt(base.transform.position.z) / 2]].tileOnOff && WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[Mathf.RoundToInt(base.transform.position.x) / 2, Mathf.RoundToInt(base.transform.position.z) / 2]].tileOnOff.isGate && WorldManager.manageWorld.onTileStatusMap[Mathf.RoundToInt(base.transform.position.x) / 2, Mathf.RoundToInt(base.transform.position.z) / 2] == 0 && gateRoutine == null)
			{
				gateRoutine = StartCoroutine(openGateAndThenClose(Mathf.RoundToInt(base.transform.position.x) / 2, Mathf.RoundToInt(base.transform.position.z) / 2));
			}
			if (inJump)
			{
				Vector3 b2 = new Vector3(myAgent.transform.position.x, base.transform.position.y, myAgent.transform.position.z);
				base.transform.position = Vector3.Lerp(base.transform.position, b2, Time.fixedDeltaTime * 10f);
			}
			else
			{
				if (myAgent.transform.position.y < -0.35f && base.transform.position.y > -8f && WorldManager.manageWorld.waterMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2])
				{
					b = new Vector3(myAgent.transform.position.x, -0.35f, myAgent.transform.position.z);
					if (!inWater)
					{
						myAgent.speed = (float)baseSpeed / 1.5f;
						doesTask.isInWater(true);
						inWater = true;
					}
				}
				else if (Physics.Raycast(myAgent.transform.position + Vector3.up, Vector3.down, out hit, 2f, jumpLayers))
				{
					if (inWater)
					{
						if ((bool)following)
						{
							myAgent.speed = (float)baseSpeed * 2f;
						}
						else
						{
							myAgent.speed = baseSpeed;
						}
						doesTask.isInWater(false);
						inWater = false;
					}
					b = new Vector3(b.x, hit.point.y, b.z);
					if (grounded && myAgent.remainingDistance > myAgent.stoppingDistance)
					{
						if (myAgent.transform.position.y - lastPos.y > 0.95f)
						{
							bool enabled2 = base.enabled;
						}
						lastPos = new Vector3(b.x, hit.point.y, b.z);
					}
				}
				if (((bool)talkingToTransform && checkIfHasArrivedAtDestination()) || ((bool)talkingToTransform && Vector3.Distance(base.transform.position, talkingToTransform.position) <= 2.5f))
				{
					if (myAgent.remainingDistance > 1f)
					{
						myAgent.SetDestination(base.transform.position);
					}
					characterTurnsToLookAtTalkingTo();
				}
				if (!isSitting)
				{
					base.transform.position = Vector3.Lerp(base.transform.position, b, Time.fixedDeltaTime * 8f);
					base.transform.rotation = myAgent.transform.rotation;
				}
				if (!following)
				{
					if (myJob.isRunningLate() || doesTask.isScared)
					{
						myAgent.speed = (float)baseSpeed * 2.8f;
					}
					else
					{
						myAgent.speed = baseSpeed;
					}
				}
			}
		}
		if (!isSign)
		{
			grounded = true;
		}
	}

	public void TalkToNpcWithDelay(float delay, Conversation forcedConvo = null)
	{
		MonoBehaviour.print("Started talking with delay");
		ConversationManager.manage.setStartTalkDelay(delay);
		TalkToNPC(forcedConvo);
	}

	public void TalkToNPC(Conversation forcedConvo = null)
	{
		if ((bool)forcedConvo)
		{
			ConversationManager.manage.talkToNPC(this, forcedConvo, true, true);
		}
		else if ((myJob.atWork && (bool)getVendorConversation() && RealWorldTimeLight.time.currentHour != 0) || myId.NPCNo == 11 || myId.NPCNo == 5)
		{
			ConversationManager.manage.talkToNPC(this, getVendorConversation(), true);
		}
		else
		{
			ConversationManager.manage.talkToNPC(this, null, true);
		}
	}

	public void playingTalkingAnimation(bool isPlaying)
	{
		if (!isSign)
		{
			if (isPlaying && !myAnim.GetBool("Talking"))
			{
				myAnim.SetFloat("TalkingOffset", Random.Range(0f, 1f));
			}
			myAnim.SetBool("Talking", isPlaying);
		}
	}

	private IEnumerator startNPC()
	{
		myAgent.updateRotation = true;
		yield return new WaitForSeconds(0.1f);
		NavMeshHit navMeshHit;
		while (!NavMesh.SamplePosition(base.transform.position, out navMeshHit, 4f, myAgent.areaMask))
		{
			yield return null;
		}
		myAgent.transform.position = navMeshHit.position;
		myAgent.enabled = true;
		myAgent.Warp(navMeshHit.position);
		while (!myAgent.isOnNavMesh)
		{
			yield return null;
		}
		base.transform.position = myAgent.transform.position;
		base.transform.rotation = myAgent.transform.rotation;
		myAgent.SetDestination(base.transform.position + new Vector3(Random.Range(-12f, 12f), 0f, Random.Range(-12f, 12f)));
		myAgent.isStopped = false;
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(NPCWonder());
			while (myManager == null)
			{
				myManager = NPCManager.manage.getNPCMapAgentForNPC(myId.NPCNo);
				yield return null;
			}
			StartCoroutine(NpcTask());
		}
	}

	private IEnumerator jumpFeel()
	{
		yield return new WaitForSeconds(0.05f);
	}

	public bool canBeTalkTo()
	{
		if (isSign)
		{
			return true;
		}
		if ((followingNetId == 0 && talkingTo == 0) || (followingNetId == 0 && talkingTo == NetworkMapSharer.share.localChar.netId))
		{
			return true;
		}
		return false;
	}

	public bool canBeTalkedToFollowing()
	{
		if (followingNetId == NetworkMapSharer.share.localChar.netId)
		{
			return true;
		}
		return false;
	}

	private void characterTurnsToLookAtTalkingTo()
	{
		if ((bool)talkingToTransform && !isSitting)
		{
			Quaternion b = Quaternion.LookRotation(new Vector3(talkingToTransform.position.x, base.transform.position.y, talkingToTransform.position.z) - base.transform.position);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, Time.deltaTime * 4f);
			myAgent.transform.rotation = base.transform.rotation;
		}
	}

	public void onTalkingChange(uint old, uint newTalkingTo)
	{
		if (isSign)
		{
			return;
		}
		NetworktalkingTo = newTalkingTo;
		if (talkingTo != 0)
		{
			if (NetworkIdentity.spawned.ContainsKey(newTalkingTo))
			{
				talkingToTransform = NetworkIdentity.spawned[newTalkingTo].transform;
			}
			if ((bool)talkingToTransform && base.isServer && myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
			{
				myAgent.SetDestination(talkingToTransform.position + talkingToTransform.forward * 2.5f);
			}
		}
		else
		{
			talkingToTransform = null;
		}
		if (talkingTo != 0 && talkingToTransform != null && talkingToTransform != NetworkMapSharer.share.localChar.transform)
		{
			StartCoroutine(talkToNonLocal());
		}
		else if (talkingTo == 0)
		{
			playingTalkingAnimation(false);
		}
	}

	public void onFollowChange(uint oldFollow, uint newFollowId)
	{
		if (isSign)
		{
			return;
		}
		NetworkfollowingNetId = newFollowId;
		if ((bool)doesTask)
		{
			doesTask.setFollowing(newFollowId);
		}
		if (newFollowId != 0)
		{
			if (base.isServer)
			{
				myAgent.speed = (float)baseSpeed * 2.8f;
				if (!following)
				{
					int areaMask = myAgent.areaMask;
					areaMask += 1 << NavMesh.GetAreaFromName("Water");
					myAgent.areaMask = areaMask;
				}
			}
			following = NetworkIdentity.spawned[newFollowId].transform;
			myManager.setFollowing(following);
			return;
		}
		if (base.isServer)
		{
			myAgent.speed = baseSpeed;
			if ((bool)following)
			{
				int areaMask2 = myAgent.areaMask;
				areaMask2 += 0 << NavMesh.GetAreaFromName("Water");
				myAgent.areaMask = areaMask2;
			}
		}
		following = null;
		myManager.setFollowing(null);
	}

	private IEnumerator NPCWonder()
	{
		while (true)
		{
			bool flag = NetworkNavMesh.nav.doesPositionHaveNavChunk((int)base.transform.position.x / 2, (int)base.transform.position.z / 2);
			if (!isVendor() && !flag)
			{
				NetworkNavMesh.nav.UnSpawnNPCOnTile(this);
			}
			if (!NetworkNavMesh.nav.isCloseEnoughToNavChunk((int)base.transform.position.x / 2, (int)base.transform.position.z / 2))
			{
				myAgent.updatePosition = false;
				myAgent.transform.position = myAgent.nextPosition;
			}
			else
			{
				myAgent.updatePosition = true;
			}
			yield return null;
		}
	}

	public bool checkIfHasArrivedAtDestination()
	{
		if (!myAgent.enabled || !myAgent.isOnNavMesh)
		{
			return true;
		}
		if (!myAgent.hasPath)
		{
			return true;
		}
		if (myAgent.remainingDistance - 1f <= myAgent.stoppingDistance && myAgent.path.status != cantComplete)
		{
			return true;
		}
		if (myAgent.pathStatus != 0)
		{
			return true;
		}
		return false;
	}

	public bool isVendor()
	{
		if (myManager == null)
		{
			return true;
		}
		myJob.NetworkatWork = myManager.isAtWork();
		if (myJob.atWork)
		{
			return true;
		}
		return false;
	}

	public void despawnOnEvent()
	{
		if (base.isActiveAndEnabled && !isVendor())
		{
			NetworkNavMesh.nav.UnSpawnNPCOnTile(this);
		}
	}

	public void changeWalkOnEntrance(bool can)
	{
		if (can != canWalkOnEntrance)
		{
			if (can)
			{
				jumpLayers = (int)jumpLayers + (1 << NavMesh.GetAreaFromName("Entrance"));
			}
			else
			{
				jumpLayers = (int)jumpLayers - (1 << NavMesh.GetAreaFromName("Entrance"));
			}
		}
		canWalkOnEntrance = can;
	}

	public Transform getTalkingToTransform()
	{
		return talkingToTransform;
	}

	private IEnumerator talkToNonLocal()
	{
		while (talkingTo != 0)
		{
			playingTalkingAnimation(true);
			yield return new WaitForSeconds(Random.Range(0.25f, 0.45f));
		}
		playingTalkingAnimation(false);
	}

	private IEnumerator openGateAndThenClose(int xPos, int yPos)
	{
		NetworkMapSharer.share.RpcNPCOpenGate(xPos, yPos, base.netId);
		while (Mathf.RoundToInt(base.transform.position.x) / 2 == xPos && Mathf.RoundToInt(base.transform.position.z) / 2 == yPos)
		{
			yield return null;
		}
		gateRoutine = null;
	}

	private void RotateAgentTowards(Transform target)
	{
		Quaternion b = Quaternion.LookRotation((target.position - myAgent.transform.position).normalized);
		myAgent.transform.rotation = Quaternion.Slerp(myAgent.transform.rotation, b, Time.deltaTime * myAgent.angularSpeed);
	}

	private IEnumerator NpcTask()
	{
		npcWait = new WaitForSeconds(Random.Range(0.5f, 0.85f));
		int cyclesBeforeRetry = 0;
		while (true)
		{
			yield return npcWait;
			if (myManager == null)
			{
				MonoBehaviour.print("NO Manager");
			}
			if (followingNetId == 0 && talkingTo == 0 && myAgent.isActiveAndEnabled && myAgent.isOnNavMesh && myManager != null)
			{
				myJob.NetworkatWork = myManager.isAtWork();
				Vector3 newDesiredLocation = myManager.getPositionForLiveAgent();
				if ((isAtWork() && newDesiredLocation != Vector3.zero) || (checkIfHasArrivedAtDestination() && newDesiredLocation != Vector3.zero) || (cyclesBeforeRetry >= 5 && newDesiredLocation != Vector3.zero))
				{
					cyclesBeforeRetry = 0;
					if (isAtWork() || NetworkNavMesh.nav.doesPositionHaveNavChunk((int)newDesiredLocation.x / 2, (int)newDesiredLocation.z / 2))
					{
						if (Vector3.Distance(myAgent.transform.position, newDesiredLocation) <= myAgent.stoppingDistance)
						{
							while (Vector3.Distance(myAgent.transform.position, newDesiredLocation) <= myAgent.stoppingDistance && myManager.hasDesiredRotation())
							{
								yield return null;
								myAgent.transform.rotation = Quaternion.Slerp(myAgent.transform.rotation, myManager.getDesiredRotation(), Time.deltaTime * 2f);
							}
						}
						else
						{
							myAgent.SetDestination(newDesiredLocation);
						}
					}
					else
					{
						Vector3 destination = base.transform.position + (newDesiredLocation - base.transform.position).normalized * 8f;
						myAgent.SetDestination(destination);
						Vector3 positionToMoveTo = myAgent.transform.position + myAgent.transform.forward * 3.5f;
						if (!NetworkNavMesh.nav.doesPositionHaveNavChunk(Mathf.RoundToInt(positionToMoveTo.x / 2f), Mathf.RoundToInt(positionToMoveTo.z / 2f)))
						{
							myManager.moveOffNavMesh(positionToMoveTo);
						}
					}
				}
			}
			cyclesBeforeRetry++;
		}
	}

	[ClientRpc]
	public void RpcSitDown(int sittingInPos, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(sittingInPos);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NPCAI), "RpcSitDown", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcGetUp(int sittingInPos, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(sittingInPos);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(NPCAI), "RpcGetUp", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	public Conversation getVendorConversation()
	{
		return NPCManager.manage.NPCDetails[myId.NPCNo].keeperConversation;
	}

	public Vector3 getSureRandomPos()
	{
		Vector3 destinationToCheck = myAgent.transform.position + new Vector3(Random.Range(-15f, 15f), 0f, Random.Range(-15f, 15f));
		destinationToCheck = checkDestination(destinationToCheck);
		int num = 0;
		while (destinationToCheck == base.transform.position)
		{
			destinationToCheck = myAgent.transform.position + new Vector3(Random.Range(-10, 10), 0f, Random.Range(-10, 10));
			destinationToCheck = checkDestination(destinationToCheck);
			num++;
			if (num >= 30)
			{
				break;
			}
		}
		if (num >= 30)
		{
			destinationToCheck = myAgent.transform.position + new Vector3(Random.Range(-10, 10), 0f, Random.Range(-10, 10));
		}
		return destinationToCheck;
	}

	public Vector3 checkDestination(Vector3 destinationToCheck)
	{
		NavMeshHit navMeshHit;
		if (NavMesh.SamplePosition(destinationToCheck, out navMeshHit, 5f, myAgent.areaMask))
		{
			if (myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
			{
				NavMeshPath navMeshPath = new NavMeshPath();
				myAgent.CalculatePath(navMeshHit.position, navMeshPath);
				if (navMeshPath.status == cantComplete)
				{
					return base.transform.position;
				}
			}
			if (NavMesh.SamplePosition(destinationToCheck, out navMeshHit, 1f, myAgent.areaMask))
			{
				return navMeshHit.position;
			}
		}
		return base.transform.position;
	}

	public bool checkPositionIsOnNavmesh(Vector3 destinationToCheck)
	{
		NavMeshHit navMeshHit;
		if (NavMesh.SamplePosition(destinationToCheck, out navMeshHit, 3f, myAgent.areaMask))
		{
			return true;
		}
		return false;
	}

	public Vector3 getClosestPosOnNavMesh(Vector3 destinationToCheck)
	{
		NavMeshHit navMeshHit;
		if (NavMesh.SamplePosition(destinationToCheck, out navMeshHit, 2.5f, myAgent.areaMask))
		{
			return navMeshHit.position;
		}
		return destinationToCheck;
	}

	public bool canStillReachTaskLocation(Vector3 taskPos)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		myAgent.CalculatePath(taskPos, navMeshPath);
		if (navMeshPath.status != 0)
		{
			return false;
		}
		return true;
	}

	static NPCAI()
	{
		npcWait = new WaitForSeconds(1f);
		cantComplete = NavMeshPathStatus.PathPartial;
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCAI), "RpcSitDown", InvokeUserCode_RpcSitDown);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCAI), "RpcGetUp", InvokeUserCode_RpcGetUp);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcSitDown(int sittingInPos, int xPos, int yPos)
	{
		isSitting = true;
		seatPos = sittingInPos;
		seatX = xPos;
		seatY = yPos;
		WorldManager.manageWorld.onTileStatusMap[xPos, yPos] = Mathf.Clamp(WorldManager.manageWorld.onTileStatusMap[xPos, yPos] + sittingInPos, 0, 3);
		TileObject tileObject = WorldManager.manageWorld.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject)
		{
			tileObject.tileObjectFurniture.updateOnTileStatus(xPos, yPos);
		}
		GetComponent<Animator>().SetTrigger("Sitting");
		GetComponent<Animator>().SetBool("SittingOrLaying", true);
	}

	protected static void InvokeUserCode_RpcSitDown(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSitDown called on server.");
		}
		else
		{
			((NPCAI)obj).UserCode_RpcSitDown(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcGetUp(int sittingInPos, int xPos, int yPos)
	{
		isSitting = false;
		WorldManager.manageWorld.onTileStatusMap[xPos, yPos] = Mathf.Clamp(WorldManager.manageWorld.onTileStatusMap[xPos, yPos] - sittingInPos, 0, 3);
		TileObject tileObject = WorldManager.manageWorld.findTileObjectInUse(xPos, yPos);
		if ((bool)tileObject)
		{
			tileObject.tileObjectFurniture.updateOnTileStatus(xPos, yPos);
		}
		GetComponent<Animator>().SetTrigger("Standing");
		GetComponent<Animator>().SetBool("SittingOrLaying", false);
	}

	protected static void InvokeUserCode_RpcGetUp(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcGetUp called on server.");
		}
		else
		{
			((NPCAI)obj).UserCode_RpcGetUp(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteUInt(talkingTo);
			writer.WriteUInt(followingNetId);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteUInt(talkingTo);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteUInt(followingNetId);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			uint num = talkingTo;
			NetworktalkingTo = reader.ReadUInt();
			if (!SyncVarEqual(num, ref talkingTo))
			{
				onTalkingChange(num, talkingTo);
			}
			uint num2 = followingNetId;
			NetworkfollowingNetId = reader.ReadUInt();
			if (!SyncVarEqual(num2, ref followingNetId))
			{
				onFollowChange(num2, followingNetId);
			}
			return;
		}
		long num3 = (long)reader.ReadULong();
		if ((num3 & 1L) != 0L)
		{
			uint num4 = talkingTo;
			NetworktalkingTo = reader.ReadUInt();
			if (!SyncVarEqual(num4, ref talkingTo))
			{
				onTalkingChange(num4, talkingTo);
			}
		}
		if ((num3 & 2L) != 0L)
		{
			uint num5 = followingNetId;
			NetworkfollowingNetId = reader.ReadUInt();
			if (!SyncVarEqual(num5, ref followingNetId))
			{
				onFollowChange(num5, followingNetId);
			}
		}
	}
}
