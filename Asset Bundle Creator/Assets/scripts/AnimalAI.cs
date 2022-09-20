using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class AnimalAI : NetworkBehaviour
{
	public int animalId;

	public string animalName = "Animal";

	public int dangerValue = 1000;

	public bool nocturnal;

	public NavMeshAgent myAgent;

	private float baseSpeed;

	private FarmAnimal isFarmAnimal;

	private AnimalAILookForFood looksForFood;

	public float minWaitTime = 1f;

	public float maxWaitTime = 2f;

	public float animalRunAwayAtHealth = 100f;

	public float runCheckDistance = 10f;

	private AnimalAI_Attack attacks;

	public LayerMask myEnemies;

	private Damageable myDamageable;

	public bool isSkiddish = true;

	private Transform currentlyRunningFrom;

	public bool waterOnly;

	private AnimateAnimalAI myAnimation;

	private AnimalAI_Sleep doesSleep;

	private AnimalPrefersArea prefersArea;

	private AnimalAI_Pet isPet;

	public AnimalVariation hasVariation;

	public bool flyingAnimal;

	public float waterSpeedMultiplier = 1f;

	[Header("Can be fenced off")]
	public bool saveFencedOffAnimalsEvent;

	public bool photoRequestable = true;

	public SaveAlphaAnimal saveAsAlpha;

	public WaitForSeconds checkTimer = new WaitForSeconds(0.1f);

	public static NavMeshPathStatus cantComplete = NavMeshPathStatus.PathPartial;

	public static NavMeshPathStatus completedPath = NavMeshPathStatus.PathComplete;

	private Transform lastAttackedBy;

	private bool rangeExtended;

	private bool inKnockback;

	private void Start()
	{
	}

	public void setUp()
	{
		attacks = GetComponent<AnimalAI_Attack>();
		isFarmAnimal = GetComponent<FarmAnimal>();
		isPet = GetComponent<AnimalAI_Pet>();
		looksForFood = GetComponent<AnimalAILookForFood>();
		myAnimation = GetComponent<AnimateAnimalAI>();
		myDamageable = GetComponent<Damageable>();
		doesSleep = GetComponent<AnimalAI_Sleep>();
		prefersArea = GetComponent<AnimalPrefersArea>();
		myAgent.autoTraverseOffMeshLink = false;
	}

	public override void OnStartClient()
	{
		if (!base.isServer)
		{
			baseSpeed = myAgent.speed;
		}
		setUp();
	}

	public override void OnStartServer()
	{
		lastAttackedBy = null;
		currentlyRunningFrom = null;
		baseSpeed = myAgent.speed;
		attacks = GetComponent<AnimalAI_Attack>();
		myAgent.avoidancePriority = Random.Range(55, 70);
		if (saveFencedOffAnimalsEvent || (bool)saveAsAlpha)
		{
			AnimalManager.manage.saveFencedOffAnimalsEvent.AddListener(saveAsFencedOff);
		}
		NetworkMapSharer.share.returnAgents.AddListener(returnOnLevelChange);
		WorldManager.manageWorld.changeDayEvent.AddListener(startNewDay);
		StartCoroutine(setUpDelay());
	}

	private void OnDisable()
	{
		if (base.isServer)
		{
			NetworkMapSharer.share.returnAgents.RemoveListener(returnOnLevelChange);
			WorldManager.manageWorld.changeDayEvent.RemoveListener(startNewDay);
			if (saveFencedOffAnimalsEvent || (bool)saveAsAlpha)
			{
				AnimalManager.manage.saveFencedOffAnimalsEvent.RemoveListener(saveAsFencedOff);
			}
			NetworkNavMesh.nav.checkNavMeshEvent.RemoveListener(checkOnNavmeshRebuild);
		}
	}

	private void saveAsFencedOff()
	{
		if ((bool)saveAsAlpha)
		{
			int id = animalId * 10 + getVariationNo();
			if (saveAsAlpha.daysRemaining != 0)
			{
				AnimalManager.manage.alphaAnimals.Add(new FencedOffAnimal(id, Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f), saveAsAlpha.daysRemaining));
			}
			else
			{
				AnimalManager.manage.returnAnimalAndDoNotSaveToMap(this);
			}
		}
		else if (WorldManager.manageWorld.isPositionOnMap(Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)) && WorldManager.manageWorld.fencedOffMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)] >= 1)
		{
			int id2 = animalId * 10 + getVariationNo();
			AnimalManager.manage.fencedOffAnimals.Add(new FencedOffAnimal(id2, Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)));
		}
	}

	public override void OnStopClient()
	{
		myAgent.speed = baseSpeed;
		clearRangeExtention();
	}

	private void returnOnLevelChange()
	{
		if (!isFarmAnimal)
		{
			NetworkNavMesh.nav.UnSpawnAnAnimal(this);
		}
	}

	private void startNewDay()
	{
		if ((bool)looksForFood)
		{
			looksForFood.isHungry = true;
		}
		if ((bool)doesSleep)
		{
			doesSleep.sendAnimalToSleep();
		}
	}

	public void forceSetUp()
	{
		StartCoroutine(setUpDelay());
	}

	private IEnumerator setUpDelay()
	{
		myAgent.transform.parent = null;
		myAgent.transform.position = base.transform.position;
		myAgent.enabled = false;
		yield return new WaitForSeconds(0.1f);
		while (!NetworkNavMesh.nav.doesPositionHaveNavChunk(Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)))
		{
			yield return null;
		}
		float checkDistance = 2f;
		Vector3 groundPos = myAgent.transform.position;
		groundPos.y = WorldManager.manageWorld.heightMap[(int)myAgent.transform.position.x / 2, (int)myAgent.transform.position.z / 2];
		if (groundPos.y < 0f && WorldManager.manageWorld.waterMap[(int)myAgent.transform.position.x / 2, (int)myAgent.transform.position.z / 2])
		{
			groundPos.y = -0.2f;
		}
		NavMeshHit hit;
		while (!NavMesh.SamplePosition(myAgent.transform.position, out hit, checkDistance, myAgent.areaMask) && !NavMesh.SamplePosition(groundPos, out hit, checkDistance, myAgent.areaMask))
		{
			yield return null;
		}
		myAgent.transform.position = hit.position;
		myAgent.enabled = true;
		while (!myAgent.isOnNavMesh)
		{
			yield return null;
		}
		myAgent.isStopped = false;
		myAgent.updateRotation = true;
		StartCoroutine(behave());
		StartCoroutine(checkDistanceAndEnableAgent());
		NetworkNavMesh.nav.checkNavMeshEvent.AddListener(checkOnNavmeshRebuild);
	}

	private IEnumerator checkDistanceAndEnableAgent()
	{
		yield return checkTimer;
		while (true)
		{
			yield return checkTimer;
			checkDistanceToPlayerAndReturn();
		}
	}

	public bool checkIfShouldContinue()
	{
		if (((bool)myDamageable && myDamageable.isStunned()) || !myAgent.enabled)
		{
			return false;
		}
		return true;
	}

	private IEnumerator behave()
	{
		float waitTime = Random.Range(minWaitTime, maxWaitTime);
		float stillTimer = waitTime;
		while (true)
		{
			yield return null;
			if (!myAgent.enabled)
			{
				continue;
			}
			if ((bool)myDamageable && myDamageable.isStunned())
			{
				while (myDamageable.isStunned())
				{
					yield return null;
				}
			}
			if ((bool)doesSleep)
			{
				yield return StartCoroutine(doesSleep.checkIfNeedsSleep());
			}
			if (((bool)doesSleep && doesSleep.checkIfSleeping()) || !myAgent.isActiveAndEnabled || !base.gameObject.activeInHierarchy)
			{
				continue;
			}
			if ((checkIfShouldContinue() && isSkiddish) || (checkIfShouldContinue() && isInjuredAndNeedsToRunAway()))
			{
				if ((bool)attacks && attacks.lookForPrey && !isSkiddish)
				{
					LayerMask myEnemiesTemp = myEnemies;
					myEnemies = attacks.myPrey;
					yield return StartCoroutine(checkForEnemiesAndRunIfFound());
					myEnemies = myEnemiesTemp;
				}
				else
				{
					yield return StartCoroutine(checkForEnemiesAndRunIfFound());
				}
			}
			if (checkIfShouldContinue())
			{
				AnimalAI_Attack animalAI_Attack = attacks;
				if ((object)animalAI_Attack != null && animalAI_Attack.lookForPrey && !isInjuredAndNeedsToRunAway())
				{
					if (attacks.huntsWhenHungry)
					{
						if (looksForFood.isHungry)
						{
							yield return StartCoroutine(attacks.lookForClosetPreyAndChase());
						}
					}
					else
					{
						yield return StartCoroutine(attacks.lookForClosetPreyAndChase());
					}
				}
			}
			if (checkIfShouldContinue() && !isInjuredAndNeedsToRunAway())
			{
				AnimalAI_Attack animalAI_Attack2 = attacks;
				if ((object)animalAI_Attack2 != null && animalAI_Attack2.attackOnlyOnAttack && attacks.hasTarget())
				{
					yield return StartCoroutine(attacks.lookForClosetPreyAndChase());
				}
			}
			if (checkIfShouldContinue())
			{
				AnimalAILookForFood animalAILookForFood = looksForFood;
				if ((object)animalAILookForFood != null && animalAILookForFood.isHungry)
				{
					yield return StartCoroutine(looksForFood.searchForFoodNearby());
				}
			}
			if (checkIfShouldContinue() && (bool)prefersArea)
			{
				yield return StartCoroutine(prefersArea.checkForPreferedArea());
			}
			if (checkIfShouldContinue() && checkIfHasArrivedAtDestination())
			{
				if (stillTimer < waitTime)
				{
					stillTimer += Time.deltaTime;
				}
				else if (myAgent.isActiveAndEnabled)
				{
					Vector3 sureRandomPos = getSureRandomPos();
					if (sureRandomPos != base.transform.position)
					{
						myAgent.SetDestination(sureRandomPos);
						stillTimer = 0f;
						waitTime = Random.Range(minWaitTime, maxWaitTime);
					}
				}
			}
			else if (flyingAnimal)
			{
				if (myAgent.isActiveAndEnabled && myAgent.remainingDistance > 4f)
				{
					myAgent.speed = getBaseSpeed() * 6f;
				}
				else
				{
					myAgent.speed = getBaseSpeed();
				}
			}
		}
	}

	private IEnumerator checkForEnemiesAndRunIfFound()
	{
		float closestEnemyCheck = 0f;
		Transform transform = returnClosestEnemy();
		if (transform != null)
		{
			if (transform != currentlyRunningFrom)
			{
				currentlyRunningFrom = transform;
				cancleCurrentDestination();
			}
			if (!flyingAnimal)
			{
				myAgent.speed = getBaseSpeed() * 2f;
			}
			else
			{
				myAgent.speed = getBaseSpeed() * 6f;
			}
			myAgent.SetDestination(getSureRunPos(currentlyRunningFrom));
		}
		while ((bool)currentlyRunningFrom)
		{
			yield return null;
			if (myAgent.isActiveAndEnabled)
			{
				if (closestEnemyCheck > 5f)
				{
					Transform transform2 = returnClosestEnemy();
					if (transform2 != null || transform2 == currentlyRunningFrom)
					{
						currentlyRunningFrom = transform2;
						cancleCurrentDestination();
						if (!flyingAnimal)
						{
							myAgent.speed = getBaseSpeed() * 2f;
						}
						else
						{
							myAgent.speed = getBaseSpeed() * 6f;
						}
						myAgent.SetDestination(getSureRunPos(currentlyRunningFrom));
					}
					closestEnemyCheck = 0f;
				}
				else
				{
					closestEnemyCheck += Time.deltaTime;
				}
				if ((bool)currentlyRunningFrom && checkIfHasArrivedAtDestination())
				{
					if (Vector3.Distance(currentlyRunningFrom.position, base.transform.position) > runCheckDistance * 1.3f)
					{
						currentlyRunningFrom = null;
					}
					else
					{
						myAgent.SetDestination(getSureRunPos(currentlyRunningFrom));
					}
				}
			}
			if (!checkIfShouldContinue())
			{
				break;
			}
		}
		if (!isPet && !flyingAnimal)
		{
			myAgent.speed = getBaseSpeed();
		}
	}

	public bool checkIfHasArrivedAtDestination()
	{
		if (myAgent.isOnNavMesh && myAgent.remainingDistance - 1f <= myAgent.stoppingDistance && myAgent.path.status != cantComplete)
		{
			return true;
		}
		if (myAgent.pathStatus != completedPath)
		{
			return true;
		}
		return false;
	}

	public bool checkIfCanGetToTarget(Transform target)
	{
		NavMeshHit hit;
		if (NavMesh.SamplePosition(target.position, out hit, 3f, myAgent.areaMask))
		{
			if (myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
			{
				NavMeshPath navMeshPath = new NavMeshPath();
				myAgent.CalculatePath(hit.position, navMeshPath);
				if (navMeshPath.status != 0)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public bool checkIfTargetIsAStraightLineAhead(Transform target)
	{
		if ((bool)target && myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
		{
			NavMeshPath navMeshPath = new NavMeshPath();
			Vector3 normalized = (target.position - myAgent.transform.position).normalized;
			myAgent.CalculatePath(target.position - normalized * 1.5f, navMeshPath);
			if (navMeshPath.corners.Length == 0)
			{
				return false;
			}
			if (navMeshPath.corners.Length <= 3)
			{
				return true;
			}
		}
		return false;
	}

	public void cancleCurrentDestination()
	{
		if (myAgent.enabled)
		{
			myAgent.ResetPath();
		}
	}

	public Transform returnClosestEnemy(float multi = 1f)
	{
		if (Physics.CheckSphere(base.transform.position + base.transform.forward * runCheckDistance / 4f, runCheckDistance * multi, myEnemies))
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position + base.transform.forward * runCheckDistance / 4f, runCheckDistance * multi, myEnemies);
			if (array.Length != 0)
			{
				int num = 0;
				float num2 = 2000f;
				bool flag = false;
				for (int i = 0; i < array.Length; i++)
				{
					if (!waterOnly || (waterOnly && array[i].transform.position.y < 0f))
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

	public Vector3 checkDestination(Vector3 destinationToCheck)
	{
		NavMeshHit hit;
		if (NavMesh.SamplePosition(destinationToCheck, out hit, 10f, myAgent.areaMask))
		{
			if (myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
			{
				NavMeshPath navMeshPath = new NavMeshPath();
				myAgent.CalculatePath(hit.position, navMeshPath);
				if (navMeshPath.status == cantComplete)
				{
					return base.transform.position;
				}
			}
			return hit.position;
		}
		return base.transform.position;
	}

	public bool checkIfWarpIsPossible(Vector3 warpDestination)
	{
		NavMeshHit hit;
		if (NavMesh.SamplePosition(warpDestination, out hit, 0.4f, myAgent.areaMask))
		{
			if (myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
			{
				NavMeshPath navMeshPath = new NavMeshPath();
				myAgent.CalculatePath(hit.position, navMeshPath);
				if (navMeshPath.status != 0)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public Vector3 checkIfPointIsWalkable(Vector3 warpDestination)
	{
		if (waterOnly)
		{
			warpDestination.y = WorldManager.manageWorld.heightMap[(int)warpDestination.x / 2, (int)warpDestination.z / 2];
		}
		else
		{
			warpDestination.y = WorldManager.manageWorld.heightMap[Mathf.RoundToInt(warpDestination.x / 2f), Mathf.RoundToInt(warpDestination.z / 2f)];
		}
		if (myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
		{
			NavMeshPath navMeshPath = new NavMeshPath();
			myAgent.CalculatePath(warpDestination, navMeshPath);
			if (navMeshPath.status != NavMeshPathStatus.PathInvalid)
			{
				return warpDestination;
			}
		}
		return myAgent.transform.position;
	}

	public Vector3 getSureRunPos(Transform runningFrom)
	{
		Vector3 vector = base.transform.position;
		for (int i = 0; i < 500; i++)
		{
			Vector3 vector2 = new Vector3(runningFrom.position.x, base.transform.position.y, runningFrom.position.z);
			vector = base.transform.position + (base.transform.position - vector2).normalized * runCheckDistance / 2f;
			vector += new Vector3(Random.Range((0f - runCheckDistance) / 2f, runCheckDistance / 2f), 0f, Random.Range((0f - runCheckDistance) / 2f, runCheckDistance / 2f));
			if (WorldManager.manageWorld.isPositionOnMap(vector))
			{
				vector.y = WorldManager.manageWorld.heightMap[Mathf.RoundToInt(vector.x / 2f), Mathf.RoundToInt(vector.z / 2f)];
			}
			vector = checkDestination(vector);
			if (vector != base.transform.position)
			{
				break;
			}
		}
		if (vector == base.transform.position)
		{
			Vector3 vector3 = new Vector3(runningFrom.position.x, base.transform.position.y, runningFrom.position.z) + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
			vector = base.transform.position + (base.transform.position - vector3) * runCheckDistance + new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-8f, 8f));
		}
		return vector;
	}

	public Vector3 getSureRandomPos()
	{
		int num = 0;
		Vector3 vector = base.transform.position;
		int num2 = 12;
		if (flyingAnimal)
		{
			num2 = 45;
		}
		while (vector != base.transform.position)
		{
			vector = checkDestination(base.transform.position + new Vector3(Random.Range(-num2, num2), 0f, Random.Range(-num2, num2)));
			num++;
			if (num >= 50)
			{
				vector = base.transform.position;
				break;
			}
		}
		if (vector == base.transform.position)
		{
			return base.transform.position + new Vector3(Random.Range(-num2, num2), 0f, Random.Range(-num2, num2));
		}
		return vector;
	}

	public float getBaseSpeed()
	{
		if (WorldManager.manageWorld.waterMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)] && WorldManager.manageWorld.heightMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)] < 0)
		{
			return baseSpeed * waterSpeedMultiplier;
		}
		return baseSpeed;
	}

	public bool isAttackingOrBeingAttackedBy(Transform check)
	{
		if (base.gameObject.activeSelf && myDamageable.health > 0)
		{
			if (lastAttackedBy == check)
			{
				return true;
			}
			if ((bool)attacks && attacks.currentlyAttacking == check)
			{
				return true;
			}
		}
		return false;
	}

	public void damageTaken(Transform attackedBy)
	{
		lastAttackedBy = attackedBy;
		wakeUpAnimal();
		AnimalAI component = attackedBy.GetComponent<AnimalAI>();
		if ((bool)component && component.animalId == animalId)
		{
			return;
		}
		AnimalAI_Attack animalAI_Attack = attacks;
		if ((object)animalAI_Attack != null && animalAI_Attack.attackOnlyOnAttack)
		{
			if (!attacks.hasTarget() || Random.Range(0, 10) == 5)
			{
				attacks.setNewCurrentlyAttacking(attackedBy.root);
			}
			currentlyRunningFrom = null;
		}
		else
		{
			AnimalAI_Attack animalAI_Attack2 = attacks;
			if ((object)animalAI_Attack2 != null && animalAI_Attack2.lookForPrey)
			{
				if (!attacks.hasTarget() || (Random.Range(0, 5) == 3 && Vector3.Distance(attacks.currentlyAttacking.position, base.transform.position) < Vector3.Distance(attackedBy.root.position, base.transform.position)))
				{
					attacks.setNewCurrentlyAttacking(attackedBy.root);
				}
			}
			else if (isSkiddish)
			{
				currentlyRunningFrom = attackedBy;
				myAgent.SetDestination(getSureRunPos(attackedBy.root));
			}
		}
		if (!rangeExtended && Vector3.Distance(base.transform.position, attackedBy.position) >= runCheckDistance - 2f && !rangeExtended)
		{
			rangeExtended = true;
			StartCoroutine(extendRangeTemp());
		}
	}

	private IEnumerator extendRangeTemp()
	{
		rangeExtended = true;
		runCheckDistance *= 3f;
		if ((bool)attacks)
		{
			attacks.chaseDistance *= 3f;
		}
		yield return new WaitForSeconds(5f);
		runCheckDistance /= 3f;
		if ((bool)attacks)
		{
			attacks.chaseDistance /= 3f;
		}
		rangeExtended = false;
	}

	public void clearRangeExtention()
	{
		if (rangeExtended)
		{
			runCheckDistance /= 3f;
			if ((bool)attacks)
			{
				attacks.chaseDistance /= 3f;
			}
		}
	}

	public bool isSleeping()
	{
		if ((bool)doesSleep && doesSleep.checkIfSleeping())
		{
			return true;
		}
		return false;
	}

	public void wakeUpAnimal()
	{
		AnimalAI_Sleep animalAI_Sleep = doesSleep;
		if ((object)animalAI_Sleep != null && animalAI_Sleep.checkIfSleeping())
		{
			doesSleep.wakeUpNow();
		}
	}

	public void takeHitAndKnockBack(Transform attackedBy, float knockBackAmount)
	{
		if (myAgent.isActiveAndEnabled)
		{
			if (!inKnockback)
			{
				StartCoroutine(knockBackTimer());
				StartCoroutine(knockBackMove(attackedBy, knockBackAmount));
			}
			damageTaken(attackedBy);
		}
	}

	private IEnumerator knockBackTimer()
	{
		inKnockback = true;
		for (float knockBackTimer = 0f; knockBackTimer < 0.75f; knockBackTimer += Time.deltaTime)
		{
			yield return null;
		}
		inKnockback = false;
	}

	private IEnumerator knockBackMove(Transform hitby, float distance)
	{
		float knockBackDistance = 0f;
		Vector3 startingPos = myAgent.transform.position;
		Vector3 pushDir = -(hitby.position - base.transform.position).normalized;
		pushDir.y = 0f;
		Vector3 travelTowards = checkIfPointIsWalkable(myAgent.transform.position + pushDir * 25f * Time.deltaTime);
		if ((bool)attacks && attacks.isWindingUp())
		{
			distance /= 3.5f;
		}
		while (knockBackDistance < distance)
		{
			myAgent.transform.position = travelTowards;
			if (myAgent.isActiveAndEnabled && myAgent.isOnNavMesh)
			{
				myAgent.ResetPath();
			}
			travelTowards = checkIfPointIsWalkable(myAgent.transform.position + pushDir * 25f * Time.deltaTime);
			knockBackDistance = Vector3.Distance(startingPos, myAgent.transform.position);
			if (!(travelTowards == myAgent.transform.position))
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	public bool isInKnockBack()
	{
		return inKnockback;
	}

	public void onDeath()
	{
		if (base.isServer)
		{
			myAgent.enabled = false;
		}
		if ((bool)attacks)
		{
			attacks.removeAttackingChar();
		}
		if ((bool)isFarmAnimal)
		{
			isFarmAnimal.onFarmAnimalDeath();
		}
		if ((bool)myAnimation)
		{
			myAnimation.playDeathAnimation();
		}
	}

	public bool isDead()
	{
		if (!myDamageable)
		{
			return false;
		}
		return myDamageable.health <= 0;
	}

	public int getHealth()
	{
		return myDamageable.health;
	}

	public int getMaxHealth()
	{
		return myDamageable.maxHealth;
	}

	public bool isInjuredAndNeedsToRunAway()
	{
		Damageable damageable = myDamageable;
		return (float?)(((object)damageable != null) ? new int?(damageable.health) : null) <= animalRunAwayAtHealth;
	}

	public void checkDistanceToPlayerAndReturn()
	{
		if (!isFarmAnimal && !NetworkNavMesh.nav.doesPositionHaveNavChunk((int)(myAgent.transform.position.x / 2f), (int)(myAgent.transform.position.z / 2f)) && !checkIfThereIsStillNavmeshThere())
		{
			NetworkNavMesh.nav.UnSpawnAnAnimal(this);
		}
		else if ((bool)isFarmAnimal)
		{
			checkOnNavmeshRebuild();
		}
	}

	public bool checkIfThereIsStillNavmeshThere()
	{
		NavMeshHit hit;
		if (NavMesh.SamplePosition(myAgent.transform.position, out hit, 1f, myAgent.areaMask))
		{
			return true;
		}
		return false;
	}

	public void checkOnNavmeshRebuild()
	{
		if (NetworkNavMesh.nav.isCloseEnoughToNavChunk((int)base.transform.position.x / 2, (int)base.transform.position.z / 2))
		{
			NavMeshHit hit;
			if (!myAgent.isActiveAndEnabled && NavMesh.SamplePosition(myAgent.transform.position, out hit, 1.5f, myAgent.areaMask) && (!myDamageable || myDamageable.health > 0))
			{
				myAgent.enabled = true;
			}
		}
		else if (myAgent.isActiveAndEnabled)
		{
			myAgent.enabled = false;
		}
	}

	public void setRunningFrom(Transform newRunningFrom)
	{
		currentlyRunningFrom = newRunningFrom;
	}

	public void setBaseSpeed(float newBaseSpeed)
	{
		myAgent.speed = newBaseSpeed;
		baseSpeed = newBaseSpeed;
	}

	public void setAttackType(bool attacksWhenClose)
	{
		if ((bool)attacks)
		{
			attacks.attackAndThenRun = attacksWhenClose;
		}
	}

	public void takeAHitLocal()
	{
		if (!isAttackPlaying() && (bool)myAnimation && myDamageable.health > 0)
		{
			myAnimation.takeAHitLocal();
		}
	}

	public bool isAttackPlaying()
	{
		if ((bool)attacks)
		{
			return attacks.isAttackPlaying();
		}
		return false;
	}

	public IEnumerator callAnimalToPos(Vector3 callToPos)
	{
		AnimalAI_Sleep animalAI_Sleep = doesSleep;
		if ((object)animalAI_Sleep != null && !animalAI_Sleep.checkIfSleeping())
		{
			myAgent.speed = getBaseSpeed();
			yield return new WaitForSeconds(Random.Range(0f, 0.45f));
			myAgent.speed = getBaseSpeed() * 2f;
			myAgent.SetDestination(callToPos + new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f)));
			yield return new WaitForSeconds(Random.Range(1.5f, 2.5f));
			myAgent.speed = getBaseSpeed();
		}
	}

	public int getVariationNo()
	{
		if ((bool)hasVariation)
		{
			return hasVariation.getVaritationNo();
		}
		return 0;
	}

	public void setVariation(int newVariation)
	{
		if ((bool)hasVariation)
		{
			hasVariation.setVariation(newVariation);
		}
	}

	public int getRandomVariationNo()
	{
		if ((bool)hasVariation)
		{
			if (hasVariation.randomVariationLimit != -1)
			{
				return Random.Range(0, hasVariation.randomVariationLimit);
			}
			return Random.Range(0, hasVariation.variations.Length);
		}
		return 0;
	}

	public void setCurrentlyAttacking(Transform newCurrentlyAttacking)
	{
		if ((bool)attacks)
		{
			attacks.setNewCurrentlyAttacking(newCurrentlyAttacking);
			currentlyRunningFrom = null;
		}
	}

	public Transform currentlyAttacking()
	{
		if (!attacks)
		{
			return null;
		}
		return attacks.currentlyAttacking;
	}

	public bool isInJump()
	{
		if (!attacks)
		{
			return false;
		}
		return attacks.isInJump();
	}

	public AnimalAI_Pet isAPet()
	{
		return isPet;
	}

	public void setSleepPos(Vector3 sleepPos)
	{
		if ((bool)doesSleep && !isFarmAnimal)
		{
			doesSleep.setDesiredSleepPos(sleepPos);
		}
	}

	public Vector3 getSleepPos()
	{
		if ((bool)doesSleep)
		{
			return doesSleep.getSleepPos();
		}
		return Vector3.zero;
	}

	private void MirrorProcessed()
	{
	}
}
