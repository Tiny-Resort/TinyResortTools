using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class NPCDoesTasks : NetworkBehaviour
{
	public enum typeOfTask
	{
		None = 0,
		FindingCrop = 1,
		FindingSeat = 2,
		FindingSomeoneToTalkTo = 3,
		FindingAnimalToPet = 4,
		HavingASnack = 5,
		FollowingAndWatering = 6,
		FollowingAndAttacking = 7,
		FollowingAndDiggingTreasure = 8,
		FollowingAndHarvesting = 9,
		ClappingForPlayer = 10
	}

	public NPCHoldsItems npcHolds;

	public NPCAI myAi;

	public bool hasTask;

	public Vector3 taskPosition;

	public InventoryItem myWaterCan;

	public InventoryItem myWeapon;

	public InventoryItem myAxe;

	public InventoryItem myPickaxe;

	public InventoryItem myShovel;

	public LayerMask myEnemies;

	public LayerMask talkToLayer;

	private CharMovement following;

	public typeOfTask currentTask;

	[SyncVar(hook = "OnGetScared")]
	public bool isScared;

	private float timeSinceLastWonder;

	public InventoryItem metalDetector;

	public InventoryItem[] playerWeapons;

	public InventoryItem[] playerAxes;

	public InventoryItem[] playerPickaxes;

	public InventoryItem[] playerWateringCans;

	private Coroutine myTaskRoutine;

	private WaitForSeconds waitTime = new WaitForSeconds(0.5f);

	private NPCAI wantsToTalkTo;

	private FarmAnimal wantsToPet;

	private WaitForSeconds taskGap = new WaitForSeconds(0.5f);

	private Vector3 sittingPosition;

	private static WaitForSeconds waitWhileInDanger;

	private bool inWater;

	private AnimalAI wantsToAttack;

	private float scaredDistance = 26f;

	private int watchPlayerHoldingId = -1;

	private MetalDetectorUse watchingDetector;

	public bool NetworkisScared
	{
		get
		{
			return isScared;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref isScared))
			{
				bool oldScared = isScared;
				SetSyncVar(value, ref isScared, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					OnGetScared(oldScared, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	private void Start()
	{
		RealWorldTimeLight.time.taskChecker.AddListener(randomiseTasks);
		npcHolds = GetComponent<NPCHoldsItems>();
		randomiseTasks();
	}

	public override void OnStartServer()
	{
		hasTask = false;
		taskPosition = Vector3.zero;
		myAi = GetComponent<NPCAI>();
		myTaskRoutine = StartCoroutine(lookForTasks());
	}

	public override void OnStartClient()
	{
		myAi = GetComponent<NPCAI>();
	}

	public void npcStartNewDay()
	{
		hasTask = false;
		taskPosition = Vector3.zero;
		wantsToTalkTo = null;
		wantsToPet = null;
		currentTask = typeOfTask.None;
		if (myTaskRoutine != null)
		{
			StopCoroutine(myTaskRoutine);
		}
		myTaskRoutine = StartCoroutine(lookForTasks());
	}

	private void OnEnable()
	{
		if ((bool)myAi)
		{
			myAi.myAgent.updateRotation = true;
		}
	}

	public void randomiseTasks()
	{
		if (hasTask)
		{
			return;
		}
		if ((bool)following)
		{
			hasTask = false;
		}
		else
		{
			int num = Random.Range(0, 20);
			if (num <= 2)
			{
				currentTask = typeOfTask.FindingCrop;
			}
			else if (num <= 3)
			{
				currentTask = typeOfTask.FindingSeat;
			}
			else if (num <= 6)
			{
				currentTask = typeOfTask.FindingSomeoneToTalkTo;
			}
			else if (num <= 8)
			{
				if (RealWorldTimeLight.time.currentHour == 0 || RealWorldTimeLight.time.currentHour > 18)
				{
					currentTask = typeOfTask.None;
				}
				else
				{
					currentTask = typeOfTask.FindingAnimalToPet;
				}
			}
			else
			{
				currentTask = typeOfTask.None;
			}
		}
		waitTime = new WaitForSeconds(Random.Range(0.95f, 1.1f));
	}

	private IEnumerator lookForTasks()
	{
		yield return null;
		yield return null;
		if (GetComponent<NPCIdentity>().NPCNo == 5)
		{
			yield break;
		}
		while (true)
		{
			yield return waitTime;
			if (myAi.myManager != null && myAi.myManager.checkIfNPCIsFree())
			{
				if (myAi.talkingTo != 0)
				{
					continue;
				}
				if ((bool)myAi.following)
				{
					yield return StartCoroutine(followingRoutine());
					continue;
				}
				yield return StartCoroutine(checkIfShouldBeScared());
				yield return StartCoroutine(checkIfShouldGreet());
				findATask();
				if (hasTask)
				{
					yield return StartCoroutine(walkToTask());
					continue;
				}
				timeSinceLastWonder += 1f;
				wonderAround();
			}
			else
			{
				if (myAi.myAgent.isActiveAndEnabled && myAi.myAgent.isOnNavMesh)
				{
					myAi.myAgent.isStopped = false;
					myAi.myAgent.updateRotation = true;
				}
				npcHolds.changeItemHolding(-1);
				hasTask = false;
			}
		}
	}

	public void wonderAround()
	{
		if (!hasTask && myAi.talkingTo == 0 && myAi.myManager.checkIfNPCIsFree() && (Random.Range(0, 75) == 5 || timeSinceLastWonder >= 15f || (RealWorldTimeLight.time.currentHour == 7 && RealWorldTimeLight.time.currentMinute < 5) || myAi.myManager.checkIfNPCHasJustExited()) && myAi.myAgent.isActiveAndEnabled && myAi.myAgent.isOnNavMesh && myAi.myAgent.remainingDistance <= myAi.myAgent.stoppingDistance)
		{
			myAi.myAgent.SetDestination(myAi.getSureRandomPos());
			timeSinceLastWonder = 0f;
		}
	}

	public void findATask()
	{
		if (hasTask)
		{
			return;
		}
		if (currentTask == typeOfTask.FindingCrop)
		{
			Vector3 vector = WorldManager.manageWorld.findClosestTileObjectAround(base.transform.position, TownManager.manage.allCropsTypes, 20, true);
			if (vector != Vector3.zero)
			{
				taskPosition = vector;
				hasTask = true;
			}
		}
		else if (currentTask == typeOfTask.FindingSeat)
		{
			Vector3 vector2 = WorldManager.manageWorld.findClosestTileObjectAround(base.transform.position, TownManager.manage.allTownSeats, 20, false, true);
			if (vector2 != Vector3.zero && !WorldManager.manageWorld.isSeatTaken(vector2))
			{
				taskPosition = vector2;
				hasTask = true;
				npcHolds.changeItemHolding(-1);
			}
			else
			{
				npcHolds.changeItemHolding(-1);
				hasTask = false;
				randomiseTasks();
			}
		}
		else if (currentTask == typeOfTask.FindingSomeoneToTalkTo)
		{
			if (Physics.CheckSphere(base.transform.position + base.transform.forward * 25f, 23f, talkToLayer))
			{
				Collider[] array = Physics.OverlapSphere(base.transform.position + base.transform.forward * 25f, 25f, talkToLayer);
				if (array.Length != 0)
				{
					int num = Random.Range(0, array.Length);
					if (array[num].transform.root != base.transform)
					{
						wantsToTalkTo = array[num].GetComponentInParent<NPCAI>();
						if ((bool)wantsToTalkTo && wantsToTalkTo.talkingTo == 0 && myAi.myManager.checkIfNPCIsFree())
						{
							taskPosition = wantsToTalkTo.transform.position;
							hasTask = true;
						}
						else
						{
							wantsToTalkTo = null;
						}
					}
				}
				else
				{
					hasTask = false;
				}
			}
			else
			{
				wantsToTalkTo = null;
				hasTask = false;
				randomiseTasks();
			}
		}
		else if (currentTask == typeOfTask.FindingAnimalToPet)
		{
			if (FarmAnimalManager.manage.activeAnimalAgents.Count > 0)
			{
				int index = Random.Range(0, FarmAnimalManager.manage.activeAnimalAgents.Count);
				if (FarmAnimalManager.manage.activeAnimalAgents[index] != null && RealWorldTimeLight.time.currentHour < 17 && Vector3.Distance(base.transform.position, FarmAnimalManager.manage.activeAnimalAgents[index].transform.position) < 70f)
				{
					wantsToPet = FarmAnimalManager.manage.activeAnimalAgents[index];
					taskPosition = wantsToPet.transform.position;
					hasTask = true;
				}
				else
				{
					wantsToPet = null;
					hasTask = false;
					randomiseTasks();
				}
			}
			else
			{
				wantsToPet = null;
			}
		}
		else if (currentTask == typeOfTask.HavingASnack)
		{
			if (Random.Range(0, 50) > 48)
			{
				taskPosition = base.transform.position;
				hasTask = true;
			}
		}
		else
		{
			npcHolds.changeItemHolding(-1);
			hasTask = false;
		}
	}

	private int getRandomSeatPosition(Vector3 seatPos)
	{
		if (WorldManager.manageWorld.onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] <= 0)
		{
			return Random.Range(1, 2);
		}
		if (WorldManager.manageWorld.onTileStatusMap[(int)seatPos.x / 2, (int)seatPos.z / 2] == 1)
		{
			return 2;
		}
		return 1;
	}

	public void onToolDoesDamage()
	{
		if (npcHolds.currentlyHolding == myWaterCan)
		{
			int resultingPlaceableTileType = npcHolds.currentlyHolding.getResultingPlaceableTileType(WorldManager.manageWorld.tileTypeMap[(int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y]);
			if (resultingPlaceableTileType != 0)
			{
				NetworkMapSharer.share.RpcUpdateTileType(resultingPlaceableTileType, (int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y);
			}
		}
		else if ((bool)npcHolds.currentlyHolding && npcHolds.currentlyHolding.myAnimType == InventoryItem.typeOfAnimation.ShovelAnimation)
		{
			if (npcHolds.usingItem)
			{
				if (npcHolds.currentlyHolding == myShovel)
				{
					NetworkMapSharer.share.changeTileHeight(-1, (int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y, following.myInteract.connectionToClient);
					StartCoroutine(swapShovel(WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[(int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y]].uniqueShovel.getItemId()));
				}
				else
				{
					NetworkMapSharer.share.changeTileHeight(1, (int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y, following.myInteract.connectionToClient);
					StartCoroutine(swapShovel(myShovel.getItemId()));
				}
			}
		}
		else
		{
			RpcNpcDoesDamageToTileObject();
		}
	}

	private IEnumerator waterPlant()
	{
		myAi.myAgent.isStopped = true;
		yield return null;
		npcHolds.NetworkcurrentlyTargetingPos = new Vector2((int)taskPosition.x / 2, (int)taskPosition.z / 2);
		if (npcHolds.currentlyHolding != myWaterCan)
		{
			npcHolds.changeItemHolding(Inventory.inv.getInvItemId(myWaterCan));
		}
		yield return StartCoroutine(faceCurrentTask());
		if (hasTask)
		{
			yield return taskGap;
			npcHolds.NetworkusingItem = true;
			yield return taskGap;
			npcHolds.NetworkusingItem = false;
			yield return taskGap;
			yield return taskGap;
		}
		hasTask = false;
		myAi.myAgent.isStopped = false;
	}

	private IEnumerator swapShovel(int newShovelId)
	{
		npcHolds.NetworkusingItem = false;
		yield return null;
		npcHolds.changeItemHolding(newShovelId);
	}

	private IEnumerator digUp()
	{
		myAi.myAgent.isStopped = true;
		myAi.myAgent.updateRotation = false;
		yield return null;
		npcHolds.NetworkcurrentlyTargetingPos = new Vector2((int)taskPosition.x / 2, (int)taskPosition.z / 2);
		if (npcHolds.currentlyHolding != myShovel)
		{
			npcHolds.changeItemHolding(Inventory.inv.getInvItemId(myShovel));
		}
		yield return StartCoroutine(faceCurrentTask());
		if (hasTask)
		{
			yield return taskGap;
			npcHolds.NetworkusingItem = true;
			while (npcHolds.currentlyHolding == myShovel)
			{
				yield return null;
			}
		}
		yield return taskGap;
		while (Vector3.Distance(following.transform.position, base.transform.position) <= 12f && WorldManager.manageWorld.getAllDropsOnTile((int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y).Count > 0)
		{
			yield return null;
		}
		yield return taskGap;
		npcHolds.NetworkusingItem = true;
		while (npcHolds.currentlyHolding != myShovel)
		{
			yield return null;
		}
		yield return taskGap;
		myAi.myAgent.updateRotation = true;
		myAi.myAgent.isStopped = false;
	}

	private IEnumerator sitDownInSeat()
	{
		if (!WorldManager.manageWorld.isSeatTaken(taskPosition))
		{
			TileObject tileObjectForServerDrop = WorldManager.manageWorld.getTileObjectForServerDrop(WorldManager.manageWorld.onTileMap[(int)taskPosition.x / 2, (int)taskPosition.z / 2], taskPosition);
			WorldManager.manageWorld.returnTileObject(tileObjectForServerDrop);
			int seatSpace = 1;
			if ((bool)tileObjectForServerDrop.tileObjectFurniture.seatPosition2)
			{
				seatSpace = getRandomSeatPosition(taskPosition);
			}
			Vector3 taskPosition2 = taskPosition;
			Vector3 sittingPosition;
			Quaternion sittingRotation;
			if (seatSpace == 1)
			{
				sittingPosition = tileObjectForServerDrop.tileObjectFurniture.seatPosition1.transform.position;
				sittingRotation = tileObjectForServerDrop.tileObjectFurniture.seatPosition1.transform.rotation;
			}
			else
			{
				sittingPosition = tileObjectForServerDrop.tileObjectFurniture.seatPosition2.transform.position;
				sittingRotation = tileObjectForServerDrop.tileObjectFurniture.seatPosition2.transform.rotation;
			}
			if (!WorldManager.manageWorld.isSeatTaken(taskPosition, seatSpace))
			{
				Vector3 walkToSitDownPos = myAi.getClosestPosOnNavMesh(sittingPosition - tileObjectForServerDrop.transform.forward * 2f);
				myAi.myAgent.SetDestination(walkToSitDownPos);
				bool madeItToSeat = true;
				while (Vector3.Distance(base.transform.position, walkToSitDownPos) > 2f && !WorldManager.manageWorld.isSeatTaken(taskPosition, seatSpace))
				{
					if (!myAi.canStillReachTaskLocation(walkToSitDownPos) || !myAi.myManager.checkIfNPCIsFree())
					{
						madeItToSeat = false;
						break;
					}
					yield return null;
				}
				if (madeItToSeat && !WorldManager.manageWorld.isSeatTaken(taskPosition, seatSpace) && myAi.myManager.checkIfNPCIsFree())
				{
					myAi.myAgent.isStopped = true;
					GetComponent<stopNPCsPushing>().stopSelf();
					myAi.RpcSitDown(seatSpace, (int)taskPosition.x / 2, (int)taskPosition.z / 2);
					float sittingTime = Random.Range(45f, 60f);
					while (sittingTime > 0f && myAi.myManager.checkIfNPCIsFree())
					{
						base.transform.position = Vector3.Lerp(base.transform.position, sittingPosition, Time.deltaTime * 8f);
						base.transform.rotation = Quaternion.Lerp(base.transform.rotation, sittingRotation, Time.deltaTime * 8f);
						if (returnClosestEnemy() != null)
						{
							sittingTime = 0f;
						}
						if (myAi.talkingTo == 0)
						{
							sittingTime -= Time.deltaTime;
						}
						yield return null;
					}
					myAi.myAgent.Warp(walkToSitDownPos);
					myAi.RpcGetUp(seatSpace, (int)taskPosition.x / 2, (int)taskPosition.z / 2);
					myAi.myAgent.isStopped = false;
					GetComponent<stopNPCsPushing>().startSelf();
					yield return new WaitForSeconds(0.5f);
				}
			}
		}
		currentTask = typeOfTask.None;
		hasTask = false;
	}

	private IEnumerator walkToTask()
	{
		while (hasTask)
		{
			if (myAi.talkingTo != 0)
			{
				yield return null;
			}
			if (!myAi.myManager.checkIfNPCIsFree())
			{
				npcHolds.changeItemHolding(-1);
				currentTask = typeOfTask.None;
				taskPosition = Vector3.zero;
				hasTask = false;
			}
			yield return StartCoroutine(checkIfShouldBeScared());
			yield return StartCoroutine(checkIfShouldGreet());
			if (Vector3.Distance(base.transform.position, taskPosition) < 2.5f || (currentTask == typeOfTask.FindingSeat && Vector3.Distance(base.transform.position, taskPosition) <= 6f) || (currentTask == typeOfTask.FindingAnimalToPet && Vector3.Distance(base.transform.position, taskPosition) <= 15f))
			{
				if (currentTask == typeOfTask.FindingCrop)
				{
					yield return StartCoroutine(waterPlant());
				}
				else if (currentTask == typeOfTask.FindingSeat)
				{
					yield return StartCoroutine(sitDownInSeat());
				}
				else if (currentTask == typeOfTask.FindingSomeoneToTalkTo)
				{
					yield return StartCoroutine(talkToOtherNpc());
				}
				else if (currentTask == typeOfTask.FindingAnimalToPet)
				{
					if (Vector3.Distance(base.transform.position, taskPosition) <= 2.5f)
					{
						yield return StartCoroutine(petAnimal());
					}
					else if (!myAi.canStillReachTaskLocation(taskPosition))
					{
						hasTask = false;
						randomiseTasks();
					}
				}
				else if (currentTask == typeOfTask.HavingASnack)
				{
					yield return StartCoroutine(haveASnack());
				}
			}
			else if (myAi.talkingTo == 0)
			{
				if (myAi.myAgent.isOnNavMesh)
				{
					if (currentTask != typeOfTask.FindingSeat && !myAi.canStillReachTaskLocation(taskPosition))
					{
						hasTask = false;
						randomiseTasks();
					}
					else if (myAi.myAgent.remainingDistance <= myAi.myAgent.stoppingDistance)
					{
						myAi.myAgent.SetDestination(taskPosition);
					}
				}
				else
				{
					hasTask = false;
				}
			}
			if (currentTask == typeOfTask.FindingSomeoneToTalkTo && (bool)wantsToTalkTo)
			{
				if (wantsToTalkTo.talkingTo != 0 || wantsToTalkTo.followingNetId != 0)
				{
					hasTask = false;
				}
				else
				{
					taskPosition = wantsToTalkTo.transform.position;
				}
			}
			if (currentTask == typeOfTask.FindingAnimalToPet && (bool)wantsToPet)
			{
				taskPosition = wantsToPet.transform.position;
			}
			if (currentTask == typeOfTask.FindingSeat && WorldManager.manageWorld.isSeatTaken(taskPosition))
			{
				hasTask = false;
			}
			if (currentTask == typeOfTask.FindingCrop && WorldManager.manageWorld.hasSquareBeenWatered(taskPosition))
			{
				hasTask = false;
			}
			yield return null;
		}
	}

	private IEnumerator checkIfShouldGreet()
	{
		int randomGreeting = Random.Range(0, NetworkNavMesh.nav.charsConnected.Count);
		if (!NPCManager.manage.npcStatus[myAi.myId.NPCNo].checkIfHasBeenGreeted(randomGreeting) && Vector3.Dot(NetworkNavMesh.nav.charsConnected[randomGreeting].transform.position - base.transform.position, base.transform.forward) >= 0.5f && Vector3.Distance(base.transform.position, NetworkNavMesh.nav.charsConnected[randomGreeting].position) < 8f)
		{
			NPCManager.manage.npcStatus[myAi.myId.NPCNo].greetCharacter(randomGreeting);
			yield return StartCoroutine(faceGreeting(NetworkNavMesh.nav.charsConnected[randomGreeting]));
			yield return StartCoroutine(greetCharacter(NetworkNavMesh.nav.charsConnected[randomGreeting]));
		}
	}

	private IEnumerator checkIfShouldBeScared()
	{
		Transform transform = returnClosestEnemy();
		if ((bool)transform)
		{
			NetworkisScared = true;
			myAi.myAgent.SetDestination(getSureRunPos(transform));
			float runTimer = 0f;
			while ((bool)transform)
			{
				if (myAi.checkIfHasArrivedAtDestination() || runTimer == 60f)
				{
					runTimer = 0f;
					myAi.myAgent.SetDestination(getSureRunPos(transform));
				}
				runTimer += 1f;
				yield return waitWhileInDanger;
				transform = returnClosestEnemy();
			}
		}
		if (isScared)
		{
			NetworkisScared = false;
		}
	}

	private IEnumerator faceCurrentTask()
	{
		myAi.myAgent.updateRotation = false;
		Vector3 normalized = (taskPosition - base.transform.position).normalized;
		Quaternion desiredRotation = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
		float turnTimer = 0f;
		while (Mathf.Abs(Quaternion.Angle(myAi.myAgent.transform.rotation, desiredRotation)) > 2f && turnTimer < 8f && myAi.myAgent.isActiveAndEnabled)
		{
			turnTimer += Time.deltaTime;
			normalized = (taskPosition - base.transform.position).normalized;
			desiredRotation = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
			myAi.myAgent.transform.rotation = Quaternion.RotateTowards(myAi.myAgent.transform.rotation, desiredRotation, 4f);
			yield return null;
		}
		if (turnTimer >= 8f)
		{
			hasTask = false;
		}
		myAi.myAgent.updateRotation = true;
	}

	private IEnumerator faceGreeting(Transform greeting)
	{
		myAi.myAgent.updateRotation = false;
		myAi.myAgent.isStopped = true;
		Vector3 normalized = (greeting.position - base.transform.position).normalized;
		Quaternion desiredRotation = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
		float faceTimer = 3f;
		while (Mathf.Abs(Quaternion.Angle(myAi.myAgent.transform.rotation, desiredRotation)) > 2f && faceTimer > 0f && myAi.myAgent.isActiveAndEnabled)
		{
			normalized = (greeting.position - base.transform.position).normalized;
			desiredRotation = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
			myAi.myAgent.transform.rotation = Quaternion.RotateTowards(myAi.myAgent.transform.rotation, desiredRotation, 4f);
			faceTimer -= Time.deltaTime;
			yield return null;
		}
		myAi.myAgent.updateRotation = true;
	}

	private IEnumerator greetCharacter(Transform greeting)
	{
		float greetTimer = 0f;
		myAi.myAgent.updateRotation = false;
		Vector3 normalized = (greeting.position - base.transform.position).normalized;
		Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
		RpcWave(greeting.GetComponent<NetworkIdentity>().netId);
		while (greetTimer <= 1f && myAi.myAgent.isActiveAndEnabled)
		{
			greetTimer += Time.deltaTime;
			normalized = (greeting.position - base.transform.position).normalized;
			Quaternion to = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
			myAi.myAgent.transform.rotation = Quaternion.RotateTowards(myAi.myAgent.transform.rotation, to, 4f);
			yield return null;
		}
		myAi.myAgent.isStopped = false;
		myAi.myAgent.updateRotation = true;
	}

	private IEnumerator clapForCharacter(Transform greeting)
	{
		float greetTimer = 0f;
		myAi.myAgent.updateRotation = false;
		Vector3 normalized = (greeting.position - base.transform.position).normalized;
		Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
		RpcClap();
		while (greetTimer <= 2f && myAi.myAgent.isActiveAndEnabled)
		{
			greetTimer += Time.deltaTime;
			normalized = (greeting.position - base.transform.position).normalized;
			Quaternion to = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
			myAi.myAgent.transform.rotation = Quaternion.RotateTowards(myAi.myAgent.transform.rotation, to, 4f);
			yield return null;
		}
		myAi.myAgent.isStopped = false;
		myAi.myAgent.updateRotation = true;
	}

	private IEnumerator talkToOtherNpc()
	{
		if (wantsToTalkTo.talkingTo == 0 && wantsToTalkTo.followingNetId == 0)
		{
			wantsToTalkTo.NetworktalkingTo = base.netId;
			myAi.NetworktalkingTo = wantsToTalkTo.netId;
			yield return new WaitForSeconds(Random.Range(2f, 5f));
			if ((bool)wantsToTalkTo)
			{
				wantsToTalkTo.NetworktalkingTo = 0u;
			}
			myAi.NetworktalkingTo = 0u;
		}
		hasTask = false;
		yield return waitTime;
		if (Random.Range(0, 3) == 2)
		{
			currentTask = typeOfTask.None;
		}
	}

	private IEnumerator petAnimal()
	{
		myAi.myAgent.isStopped = true;
		yield return null;
		npcHolds.changeItemHolding(-1);
		yield return StartCoroutine(faceCurrentTask());
		if (hasTask)
		{
			RpcPatAnimation();
			wantsToPet.RpcPetAnimal();
			yield return taskGap;
			yield return taskGap;
			yield return taskGap;
			yield return taskGap;
			hasTask = false;
		}
		if (Random.Range(0, 3) == 2)
		{
			currentTask = typeOfTask.None;
		}
		wantsToPet = null;
		myAi.myAgent.isStopped = false;
	}

	private IEnumerator haveASnack()
	{
		if (myAi.myAgent.isActiveAndEnabled && myAi.myAgent.isOnNavMesh)
		{
			myAi.myAgent.isStopped = true;
			yield return taskGap;
			int num = 0;
			while (!Inventory.inv.allItems[num].consumeable)
			{
				num = Random.Range(0, Inventory.inv.allItems.Length);
			}
			npcHolds.changeItemHolding(num);
			yield return taskGap;
			npcHolds.NetworkusingItem = true;
			yield return taskGap;
			yield return taskGap;
			yield return taskGap;
			yield return taskGap;
			npcHolds.NetworkusingItem = false;
			npcHolds.changeItemHolding(-1);
			hasTask = false;
			myAi.myAgent.isStopped = false;
			currentTask = typeOfTask.None;
		}
	}

	private IEnumerator attackTileForPlayer()
	{
		myAi.myAgent.isStopped = true;
		npcHolds.NetworkcurrentlyTargetingPos = new Vector2((int)taskPosition.x / 2, (int)taskPosition.z / 2);
		yield return StartCoroutine(faceCurrentTask());
		float attackTimer = 0.9f;
		if (hasTask)
		{
			npcHolds.NetworkusingItem = true;
			while (attackTimer > 0f && WorldManager.manageWorld.onTileMap[(int)taskPosition.x / 2, (int)taskPosition.z / 2] > -1 && WorldManager.manageWorld.onTileMap[(int)taskPosition.x / 2, (int)taskPosition.z / 2] != 30)
			{
				yield return StartCoroutine(faceCurrentTask());
				attackTimer = ((!following.myEquip.usingItem) ? (attackTimer - Time.deltaTime) : 0.9f);
				yield return null;
			}
			npcHolds.NetworkusingItem = false;
		}
		hasTask = false;
		myAi.myAgent.isStopped = false;
	}

	public void setFollowing(uint newFollowing)
	{
		if (base.isServer)
		{
			hasTask = false;
		}
		if (newFollowing == 0)
		{
			following = null;
		}
		else if (NetworkIdentity.spawned.ContainsKey(newFollowing))
		{
			following = NetworkIdentity.spawned[newFollowing].GetComponent<CharMovement>();
		}
		hasTask = false;
	}

	public void isInWater(bool isInWater)
	{
		inWater = isInWater;
	}

	[ClientRpc]
	private void RpcPatAnimation()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NPCDoesTasks), "RpcPatAnimation", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcWave(uint greetingNetId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(greetingNetId);
		SendRPCInternal(typeof(NPCDoesTasks), "RpcWave", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcClap()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NPCDoesTasks), "RpcClap", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator clapTimer()
	{
		GetComponent<Animator>().SetInteger("Emotion", 12);
		yield return new WaitForSeconds(2f);
		GetComponent<Animator>().SetInteger("Emotion", 0);
	}

	[ClientRpc]
	private void RpcNpcDoesDamageToTileObject()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(NPCDoesTasks), "RpcNpcDoesDamageToTileObject", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private Transform checkForEnemies()
	{
		if (Physics.CheckSphere(base.transform.position, 6f, myEnemies))
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, 6f, myEnemies);
			if (array.Length != 0)
			{
				int num = 0;
				float num2 = 9f;
				bool flag = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].transform.position.y > 0f && array[i].GetComponentInParent<AnimalAI>().isAttackingOrBeingAttackedBy(following.transform))
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

	private IEnumerator attackEnemy()
	{
		if ((bool)wantsToAttack)
		{
			taskPosition = wantsToAttack.transform.position;
			yield return StartCoroutine(faceCurrentTask());
			myAi.myAgent.isStopped = true;
			npcHolds.NetworkcurrentlyTargetingPos = new Vector2((int)taskPosition.x / 2, (int)taskPosition.z / 2);
			npcHolds.changeItemHolding(myWeapon.getItemId());
			npcHolds.NetworkusingItem = true;
			while ((bool)wantsToAttack && wantsToAttack.gameObject.activeInHierarchy && wantsToAttack.getHealth() > 0 && Vector3.Distance(wantsToAttack.transform.position, base.transform.position) < 2.5f)
			{
				taskPosition = wantsToAttack.transform.position;
				yield return StartCoroutine(faceCurrentTask());
				yield return null;
			}
			npcHolds.NetworkusingItem = false;
			hasTask = false;
			myAi.myAgent.isStopped = false;
		}
	}

	public bool attacksCharacters(LayerMask checkMask)
	{
		return (int)checkMask == ((int)checkMask | 0x100);
	}

	public Transform returnClosestEnemy(float multi = 1f)
	{
		if (Physics.CheckSphere(base.transform.position + base.transform.forward * scaredDistance / 4f, scaredDistance * multi, myEnemies))
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position + base.transform.forward * scaredDistance / 4f, scaredDistance * multi, myEnemies);
			if (array.Length != 0)
			{
				int num = 0;
				float num2 = 2000f;
				bool flag = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (!(array[i].transform.position.y > 0f))
					{
						continue;
					}
					AnimalAI_Attack componentInParent = array[i].GetComponentInParent<AnimalAI_Attack>();
					if ((bool)componentInParent && attacksCharacters(componentInParent.myPrey) && !componentInParent.GetComponent<AnimalAI>().waterOnly)
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
					return array[num].transform.root;
				}
			}
		}
		return null;
	}

	public Vector3 getSureRunPos(Transform runningFrom)
	{
		Vector3 vector = base.transform.position;
		for (int i = 0; i < 500; i++)
		{
			Vector3 vector2 = new Vector3(runningFrom.position.x, base.transform.position.y, runningFrom.position.z);
			vector = base.transform.position + (base.transform.position - vector2).normalized * scaredDistance;
			vector += new Vector3(Random.Range(0f - scaredDistance, scaredDistance), 0f, Random.Range(0f - scaredDistance, scaredDistance));
			if (WorldManager.manageWorld.isPositionOnMap(vector))
			{
				vector.y = WorldManager.manageWorld.heightMap[Mathf.RoundToInt(vector.x / 2f), Mathf.RoundToInt(vector.z / 2f)];
			}
			vector = myAi.checkDestination(vector);
			if (vector != base.transform.position)
			{
				break;
			}
		}
		if (vector == base.transform.position)
		{
			Vector3 vector3 = new Vector3(runningFrom.position.x, base.transform.position.y, runningFrom.position.z) + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
			vector = base.transform.position + (base.transform.position - vector3) * scaredDistance + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-8f, 8f));
		}
		return vector;
	}

	private void OnGetScared(bool oldScared, bool newScared)
	{
		NetworkisScared = newScared;
		myAi.myAnim.SetBool("Scared", newScared);
		if (newScared)
		{
			switch (Random.Range(0, 6))
			{
			case 0:
				myAi.chatBubble.tryAndTalk("Ahhh!", 2f, true);
				break;
			case 1:
				myAi.chatBubble.tryAndTalk("OH NO!", 2f, true);
				break;
			case 2:
				myAi.chatBubble.tryAndTalk("Struth!", 2f, true);
				break;
			case 3:
				myAi.chatBubble.tryAndTalk("HELP!", 2f, true);
				break;
			case 4:
				myAi.chatBubble.tryAndTalk("RUN AWAY!", 2f, true);
				break;
			case 5:
				myAi.chatBubble.tryAndTalk("Predator!", 2f, true);
				break;
			}
		}
		else
		{
			switch (Random.Range(0, 6))
			{
			case 0:
				myAi.chatBubble.tryAndTalk("Phew.", 2f, true);
				break;
			case 1:
				myAi.chatBubble.tryAndTalk("Safe.", 2f, true);
				break;
			case 2:
				myAi.chatBubble.tryAndTalk("Is it gone?", 2f, true);
				break;
			case 3:
				myAi.chatBubble.tryAndTalk("Am I safe?", 2f, true);
				break;
			case 4:
				myAi.chatBubble.tryAndTalk("Woah.", 2f, true);
				break;
			case 5:
				myAi.chatBubble.tryAndTalk("That was close!", 2f, true);
				break;
			}
		}
	}

	private IEnumerator followingRoutine()
	{
		while ((bool)following)
		{
			yield return null;
			if (inWater)
			{
				npcHolds.changeItemHolding(-1);
			}
			if (hasTask)
			{
				if (Vector3.Distance(following.transform.position, base.transform.position) > 12f)
				{
					clearTask();
					continue;
				}
				if (Vector3.Distance(base.transform.position, taskPosition) <= 2.5f || (currentTask == typeOfTask.FollowingAndDiggingTreasure && Vector3.Distance(base.transform.position, taskPosition) <= 3f))
				{
					if (currentTask == typeOfTask.FollowingAndDiggingTreasure)
					{
						yield return StartCoroutine(digUp());
						clearTask();
					}
					else if (currentTask == typeOfTask.FollowingAndWatering)
					{
						yield return StartCoroutine(waterPlant());
						clearTask();
					}
					else if (currentTask == typeOfTask.FollowingAndAttacking)
					{
						yield return StartCoroutine(attackEnemy());
					}
					else if (currentTask == typeOfTask.FollowingAndHarvesting)
					{
						yield return StartCoroutine(attackTileForPlayer());
					}
					continue;
				}
				if (!myAi.canStillReachTaskLocation(taskPosition))
				{
					clearTask();
				}
				else if (currentTask == typeOfTask.FollowingAndDiggingTreasure)
				{
					if (following.myEquip.currentlyHoldingNo != metalDetector.getItemId())
					{
						watchingDetector = null;
						clearTask();
					}
				}
				else if (currentTask == typeOfTask.FollowingAndAttacking)
				{
					if ((bool)wantsToAttack)
					{
						if (wantsToAttack.gameObject.activeInHierarchy && wantsToAttack.getHealth() > 0)
						{
							npcHolds.changeItemHolding(myWeapon.getItemId());
							taskPosition = wantsToAttack.transform.position;
							hasTask = true;
						}
						else
						{
							wantsToAttack = null;
							clearTask();
						}
					}
					else
					{
						currentTask = typeOfTask.None;
						hasTask = false;
					}
				}
				else if (WorldManager.manageWorld.onTileMap[(int)taskPosition.x / 2, (int)taskPosition.z / 2] == -1 || WorldManager.manageWorld.onTileMap[(int)taskPosition.x / 2, (int)taskPosition.z / 2] == 30)
				{
					clearTask();
				}
				if (myAi.talkingTo != 0)
				{
					continue;
				}
				if (myAi.myAgent.isOnNavMesh)
				{
					if (myAi.myAgent.remainingDistance <= myAi.myAgent.stoppingDistance)
					{
						myAi.myAgent.SetDestination(taskPosition);
					}
				}
				else
				{
					hasTask = false;
				}
				continue;
			}
			if (watchPlayerHoldingId != following.myEquip.currentlyHoldingNo && (bool)following.myEquip.currentlyHolding && ((bool)following.myEquip.currentlyHolding.fish || (bool)following.myEquip.currentlyHolding.bug))
			{
				npcHolds.changeItemHolding(-1);
				yield return StartCoroutine(clapForCharacter(following.transform));
			}
			checkWhatPlayerIsHoldingAndChangeItem();
			if (npcHolds.currentlyHolding == myShovel)
			{
				if (following.myEquip.currentlyHoldingNo != metalDetector.getItemId())
				{
					clearTask();
				}
				else if ((bool)watchingDetector && watchingDetector.foundSomething)
				{
					currentTask = typeOfTask.FollowingAndDiggingTreasure;
					taskPosition = watchingDetector.getLastCheckPositionForNPCFollow();
					hasTask = true;
				}
				continue;
			}
			if (npcHolds.currentlyHolding == myWaterCan)
			{
				currentTask = typeOfTask.FollowingAndWatering;
				Vector3 vector = WorldManager.manageWorld.findClosestTileObjectAround(base.transform.position, TownManager.manage.allCropsTypes, 9, true);
				if (vector != Vector3.zero)
				{
					taskPosition = vector;
					hasTask = true;
				}
				continue;
			}
			Transform transform = checkForEnemies();
			if ((bool)transform)
			{
				wantsToAttack = transform.GetComponent<AnimalAI>();
				if ((bool)wantsToAttack)
				{
					taskPosition = wantsToAttack.transform.position;
					hasTask = true;
					npcHolds.changeItemHolding(myWeapon.getItemId());
					currentTask = typeOfTask.FollowingAndAttacking;
				}
			}
			else if (WorldManager.manageWorld.onTileMap[(int)following.myInteract.currentlyAttackingPos.x, (int)following.myInteract.currentlyAttackingPos.y] != -1 && following.myEquip.usingItem)
			{
				currentTask = typeOfTask.FollowingAndHarvesting;
				taskPosition = new Vector3((int)following.myInteract.currentlyAttackingPos.x * 2, WorldManager.manageWorld.heightMap[(int)following.myInteract.currentlyAttackingPos.x, (int)following.myInteract.currentlyAttackingPos.y], (int)following.myInteract.currentlyAttackingPos.y * 2);
				hasTask = true;
			}
		}
	}

	public void clearTask()
	{
		currentTask = typeOfTask.None;
		hasTask = false;
	}

	public void checkWhatPlayerIsHoldingAndChangeItem()
	{
		if (following.myEquip.currentlyHoldingNo == watchPlayerHoldingId)
		{
			return;
		}
		npcHolds.changeItemHolding(-1);
		watchingDetector = null;
		if (following.myEquip.currentlyHoldingNo > -1)
		{
			if ((bool)Inventory.inv.allItems[following.myEquip.currentlyHoldingNo].consumeable || (bool)Inventory.inv.allItems[following.myEquip.currentlyHoldingNo].placeable)
			{
				watchPlayerHoldingId = following.myEquip.currentlyHoldingNo;
				return;
			}
			if (following.myEquip.currentlyHoldingNo == metalDetector.getItemId())
			{
				npcHolds.changeItemHolding(myShovel.getItemId());
				watchingDetector = following.myEquip.holdingPrefab.GetComponent<MetalDetectorUse>();
			}
			else if ((bool)wantsToAttack)
			{
				npcHolds.changeItemHolding(myWeapon.getItemId());
			}
			else
			{
				for (int i = 0; i < playerAxes.Length; i++)
				{
					if (following.myEquip.currentlyHoldingNo == playerAxes[i].getItemId())
					{
						npcHolds.changeItemHolding(myAxe.getItemId());
						watchPlayerHoldingId = following.myEquip.currentlyHoldingNo;
						return;
					}
				}
				for (int j = 0; j < playerWeapons.Length; j++)
				{
					if (following.myEquip.currentlyHoldingNo == playerWeapons[j].getItemId())
					{
						npcHolds.changeItemHolding(myWeapon.getItemId());
						watchPlayerHoldingId = following.myEquip.currentlyHoldingNo;
						return;
					}
				}
				for (int k = 0; k < playerPickaxes.Length; k++)
				{
					if (following.myEquip.currentlyHoldingNo == playerPickaxes[k].getItemId())
					{
						npcHolds.changeItemHolding(myPickaxe.getItemId());
						watchPlayerHoldingId = following.myEquip.currentlyHoldingNo;
						return;
					}
				}
				for (int l = 0; l < playerWateringCans.Length; l++)
				{
					if (following.myEquip.currentlyHoldingNo == playerWateringCans[l].getItemId())
					{
						npcHolds.changeItemHolding(myWaterCan.getItemId());
						watchPlayerHoldingId = following.myEquip.currentlyHoldingNo;
						return;
					}
				}
			}
		}
		watchPlayerHoldingId = following.myEquip.currentlyHoldingNo;
	}

	static NPCDoesTasks()
	{
		waitWhileInDanger = new WaitForSeconds(0.05f);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCDoesTasks), "RpcPatAnimation", InvokeUserCode_RpcPatAnimation);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCDoesTasks), "RpcWave", InvokeUserCode_RpcWave);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCDoesTasks), "RpcClap", InvokeUserCode_RpcClap);
		RemoteCallHelper.RegisterRpcDelegate(typeof(NPCDoesTasks), "RpcNpcDoesDamageToTileObject", InvokeUserCode_RpcNpcDoesDamageToTileObject);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcPatAnimation()
	{
		GetComponent<Animator>().SetTrigger("Pet");
	}

	protected static void InvokeUserCode_RpcPatAnimation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPatAnimation called on server.");
		}
		else
		{
			((NPCDoesTasks)obj).UserCode_RpcPatAnimation();
		}
	}

	protected void UserCode_RpcWave(uint greetingNetId)
	{
		EquipItemToChar componentInParent = NetworkIdentity.spawned[greetingNetId].GetComponentInParent<EquipItemToChar>();
		if ((bool)componentInParent)
		{
			switch (Random.Range(0, 6))
			{
			case 0:
				myAi.chatBubble.tryAndTalk("G'day!", 2f);
				break;
			case 1:
				myAi.chatBubble.tryAndTalk("Nice to see you, " + componentInParent.playerName, 2f);
				break;
			case 2:
				myAi.chatBubble.tryAndTalk("Hi, " + componentInParent.playerName, 2f);
				break;
			case 3:
				myAi.chatBubble.tryAndTalk("Hello, " + componentInParent.playerName + "!", 2f);
				break;
			case 4:
				myAi.chatBubble.tryAndTalk("G'day, " + componentInParent.playerName, 2f);
				break;
			}
		}
		GetComponent<Animator>().SetInteger("Emotion", 4);
	}

	protected static void InvokeUserCode_RpcWave(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcWave called on server.");
		}
		else
		{
			((NPCDoesTasks)obj).UserCode_RpcWave(reader.ReadUInt());
		}
	}

	protected void UserCode_RpcClap()
	{
		StartCoroutine(clapTimer());
	}

	protected static void InvokeUserCode_RpcClap(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcClap called on server.");
		}
		else
		{
			((NPCDoesTasks)obj).UserCode_RpcClap();
		}
	}

	protected void UserCode_RpcNpcDoesDamageToTileObject()
	{
		TileObject tileObject = WorldManager.manageWorld.findTileObjectInUse((int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y);
		if ((bool)tileObject && following.myInteract.checkIfCanDamage(npcHolds.currentlyTargetingPos))
		{
			tileObject.damage();
			tileObject.currentHealth -= npcHolds.currentlyHolding.damagePerAttack;
			Vector3 position = tileObject.transform.position;
			ParticleManager.manage.emitAttackParticle(position);
			if (tileObject.currentHealth <= 0f)
			{
				following.myInteract.followingNPCKillsItem((int)npcHolds.currentlyTargetingPos.x, (int)npcHolds.currentlyTargetingPos.y, tileObject);
			}
		}
	}

	protected static void InvokeUserCode_RpcNpcDoesDamageToTileObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNpcDoesDamageToTileObject called on server.");
		}
		else
		{
			((NPCDoesTasks)obj).UserCode_RpcNpcDoesDamageToTileObject();
		}
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(isScared);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(isScared);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = isScared;
			NetworkisScared = reader.ReadBool();
			if (!SyncVarEqual(flag, ref isScared))
			{
				OnGetScared(flag, isScared);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			bool flag2 = isScared;
			NetworkisScared = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref isScared))
			{
				OnGetScared(flag2, isScared);
			}
		}
	}
}
