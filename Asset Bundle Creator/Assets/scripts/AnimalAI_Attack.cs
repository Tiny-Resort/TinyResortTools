using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class AnimalAI_Attack : NetworkBehaviour
{
	private AnimalAI myAi;

	public bool setPlayerInDangerWhenTargeted;

	public bool attackOnlyOnAttack;

	public bool multipleAttacksOnAttack;

	public bool lookForPrey;

	public bool huntsWhenHungry;

	public bool protectsSleepingSpot;

	public bool attackAndThenRun;

	public bool canJumpWhileChasing;

	public LayerMask myPrey;

	public Transform currentlyAttacking;

	public Damageable currentlyAttacking_Damageable;

	public CharMovement currentlyAttackingChar;

	private Animator myAnim;

	public bool readOutBehaviour;

	private AnimalAltAttack[] hasAlt;

	public bool altAttackOnly;

	[Header("Attack Information")]
	public ItemHitBox myAttackBox;

	public int maxAttacks = 2;

	public float damageOnAttack = 5f;

	public float chaseDistance = 15f;

	public float jumpTowardsDistance = 2f;

	public float windUpMax = 2f;

	public float coolDownTime = 1.5f;

	[Header("Attack Frames Info")]
	public int framesBeforeJump = 12;

	public int framesBeforeAttackStarts = 30;

	public int totalFramesOfAttackAnimation = 60;

	private static int basicAttackAnimation;

	private static int windUpAnimation;

	private static int windUpAltAnimation;

	private static int stopAnimation;

	private bool doingAltAttack;

	private int usingAltNo;

	private float windUpTimer;

	private bool currentlyPlayingAttack;

	private bool firstAttackDelay;

	private bool inJump;

	private void Start()
	{
		basicAttackAnimation = Animator.StringToHash("BasicAttack");
		windUpAnimation = Animator.StringToHash("WindUp");
		windUpAltAnimation = Animator.StringToHash("WindUpAlt");
		stopAnimation = Animator.StringToHash("StopAnimation");
		myAi = GetComponent<AnimalAI>();
		myAnim = GetComponent<Animator>();
		hasAlt = GetComponents<AnimalAltAttack>();
		if ((bool)GetComponent<AnimalAI_Pet>())
		{
			return;
		}
		myAttackBox.canDamageFriendly = true;
		if (hasAlt.Length == 0)
		{
			return;
		}
		for (int i = 0; i < hasAlt.Length; i++)
		{
			if ((bool)hasAlt[i].attackBox)
			{
				hasAlt[i].attackBox.canDamageFriendly = true;
			}
		}
	}

	private void OnEnable()
	{
		currentlyAttacking = null;
		currentlyAttacking_Damageable = null;
		currentlyPlayingAttack = false;
		firstAttackDelay = false;
		currentlyAttackingChar = null;
		inJump = false;
		if ((bool)myAttackBox)
		{
			myAttackBox.endAttack();
		}
	}

	public override void OnStopClient()
	{
		removeAttackingChar();
	}

	public IEnumerator lookForClosetPreyAndChase()
	{
		Transform transform = returnClosestPrey();
		if (transform != null)
		{
			setNewCurrentlyAttacking(transform);
		}
		while ((bool)currentlyAttacking)
		{
			yield return myAi.checkTimer;
			if (!myAi.myAgent.isActiveAndEnabled)
			{
				continue;
			}
			if (currentlyAttacking == null)
			{
				setNewCurrentlyAttacking(returnClosestPrey());
			}
			while ((bool)currentlyAttacking && myAi.myAgent.isActiveAndEnabled)
			{
				float num = Vector3.Distance(new Vector3(currentlyAttacking.position.x, 0f, currentlyAttacking.position.z), new Vector3(myAi.myAgent.transform.position.x, 0f, myAi.myAgent.transform.position.z));
				if (currentlyAttacking == base.transform)
				{
					setNewCurrentlyAttacking(null);
				}
				else if (!currentlyAttacking_Damageable || currentlyAttacking_Damageable.health <= 0)
				{
					setNewCurrentlyAttacking(null);
				}
				else if (canJumpWhileChasing && chasingObjectHigherOrLower() && !myAi.checkIfCanGetToTarget(currentlyAttacking))
				{
					yield return StartCoroutine(chaseAndThenJumpUpOrDown());
				}
				else if (!checkIfCurrentTargetIsStillReachable())
				{
					setNewCurrentlyAttacking(null);
				}
				else if (myAi.isInjuredAndNeedsToRunAway())
				{
					setNewCurrentlyAttacking(null);
				}
				else if (protectsSleepingSpot && Vector3.Distance(currentlyAttacking.position, myAi.getSleepPos()) > chaseDistance * 5.5f)
				{
					setNewCurrentlyAttacking(null);
					myAi.myAgent.SetDestination(myAi.getSleepPos());
				}
				else if (num <= Mathf.Clamp(jumpTowardsDistance, 2f, 45f) && myAi.checkIfTargetIsAStraightLineAhead(currentlyAttacking))
				{
					yield return StartCoroutine(faceEnemy());
					if (attackOnlyOnAttack && !multipleAttacksOnAttack && Random.Range(0f, 15f) < 3f)
					{
						setNewCurrentlyAttacking(null);
					}
					if (attackAndThenRun && (bool)currentlyAttacking)
					{
						myAi.setRunningFrom(currentlyAttacking);
						setNewCurrentlyAttacking(null);
					}
				}
				else if (hasAlt.Length != 0 && Random.Range(0, 120) == 5 && num <= Mathf.Clamp(hasAlt[Random.Range(0, hasAlt.Length)].jumpTowardsDistance, 2f, 45f))
				{
					yield return StartCoroutine(faceEnemy());
					if (attackOnlyOnAttack && !multipleAttacksOnAttack && Random.Range(0f, 15f) < 3f)
					{
						setNewCurrentlyAttacking(null);
					}
					if (attackAndThenRun && (bool)currentlyAttacking)
					{
						myAi.setRunningFrom(currentlyAttacking);
						setNewCurrentlyAttacking(null);
					}
				}
				else
				{
					myAi.myAgent.speed = myAi.getBaseSpeed() * 2f;
					if (myAi.checkIfHasArrivedAtDestination() || Vector3.Distance(currentlyAttacking.transform.position, myAi.myAgent.destination) >= chaseDistance / 2f)
					{
						if (num >= chaseDistance * 1.5f)
						{
							setNewCurrentlyAttacking(null);
						}
						else if (myAi.checkIfShouldContinue())
						{
							myAi.myAgent.SetDestination(currentlyAttacking.position + new Vector3(Random.Range(-3, 3), 0f, Random.Range(-3, 3)));
						}
					}
				}
				yield return null;
			}
		}
	}

	public bool checkIfCurrentTargetIsStillReachable()
	{
		if (canJumpWhileChasing)
		{
			return true;
		}
		if (!myAi.waterOnly || (myAi.waterOnly && currentlyAttacking.position.y <= -0.1f))
		{
			if (myAi.checkIfCanGetToTarget(currentlyAttacking))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public bool chasingObjectHigherOrLower()
	{
		if ((bool)currentlyAttacking)
		{
			int num = WorldManager.manageWorld.heightMap[Mathf.RoundToInt(currentlyAttacking.position.x / 2f), Mathf.RoundToInt(currentlyAttacking.position.z / 2f)] - WorldManager.manageWorld.heightMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)];
			if (num == 2)
			{
				MonoBehaviour.print("Jump Up");
				return true;
			}
			if (num <= -2)
			{
				MonoBehaviour.print("Jump Down");
				return true;
			}
		}
		return false;
	}

	public void checkIfDoingAlt()
	{
		doingAltAttack = false;
		if (hasAlt.Length != 0)
		{
			if (hasAlt[0].callForHelp)
			{
				doingAltAttack = hasAlt[0].checkIfNeedToCallForHelp();
				usingAltNo = 0;
			}
			if (!doingAltAttack)
			{
				float num = Vector3.Distance(new Vector3(currentlyAttacking.position.x, 0f, currentlyAttacking.position.z), new Vector3(myAi.myAgent.transform.position.x, 0f, myAi.myAgent.transform.position.z));
				for (int i = 0; i < hasAlt.Length; i++)
				{
					if (Random.Range(0, 3) == 2 && num <= Mathf.Clamp(hasAlt[i].jumpTowardsDistance, 2f, 45f) && !hasAlt[i].callForHelp)
					{
						doingAltAttack = true;
						usingAltNo = i;
						if (hasAlt[usingAltNo].callForHelp)
						{
							doingAltAttack = false;
						}
						break;
					}
				}
				if (!doingAltAttack && num > jumpTowardsDistance + 1.5f)
				{
					int num2 = Random.Range(0, hasAlt.Length);
					if (!hasAlt[num2].callForHelp && num <= Mathf.Clamp(hasAlt[num2].jumpTowardsDistance, 2f, 45f))
					{
						doingAltAttack = true;
						usingAltNo = num2;
					}
				}
			}
			if (altAttackOnly)
			{
				doingAltAttack = true;
				usingAltNo = Random.Range(0, hasAlt.Length);
			}
		}
		else
		{
			doingAltAttack = false;
		}
	}

	public bool isWindingUp()
	{
		return windUpTimer > 0f;
	}

	private IEnumerator faceEnemy()
	{
		int attackNumbers = Random.Range(1, maxAttacks + 1);
		myAi.myAgent.speed = myAi.getBaseSpeed();
		myAi.myAgent.isStopped = true;
		myAi.myAgent.updateRotation = false;
		myAi.myAgent.ResetPath();
		if ((bool)currentlyAttacking && canCurrentlyBeTargeted(currentlyAttacking))
		{
			while (myAi.checkIfShouldContinue() && (bool)currentlyAttacking && myAi.myAgent.isActiveAndEnabled && attackNumbers > 0 && Vector3.Distance(currentlyAttacking.position, base.transform.position) < chaseDistance && myAi.checkIfPointIsWalkable(currentlyAttacking.transform.position) != base.transform.position)
			{
				checkIfDoingAlt();
				Vector3 lookAtPos = (currentlyAttacking.position - base.transform.position).normalized;
				Quaternion.LookRotation(new Vector3(lookAtPos.x, 0f, lookAtPos.z));
				windUpTimer = windUpMax;
				if (doingAltAttack && hasAlt[usingAltNo].callForHelp)
				{
					windUpTimer = 0f;
				}
				else if (doingAltAttack && hasAlt[usingAltNo].AOEAttack)
				{
					windUpTimer = hasAlt[usingAltNo].AOEWindUpTime;
				}
				else if (firstAttackDelay)
				{
					firstAttackDelay = false;
					windUpTimer *= 1.25f;
				}
				else
				{
					windUpTimer = Mathf.Clamp(windUpMax - windUpMax / (float)attackNumbers / 2f, 0.15f, 15f);
				}
				attackNumbers--;
				if (doingAltAttack && hasAlt[usingAltNo].chargAttack)
				{
					RpcPlayWindUpAlt();
					windUpTimer = 1.6f;
				}
				else if (doingAltAttack && hasAlt[usingAltNo].AOEAttack)
				{
					RpcPlayWindUpAlt();
				}
				else
				{
					RpcPlayWindUp();
				}
				while (myAi.myAgent.isActiveAndEnabled && windUpTimer > 0f)
				{
					if ((bool)currentlyAttacking)
					{
						lookAtPos = (currentlyAttacking.position - base.transform.position).normalized;
					}
					Quaternion quaternion = Quaternion.LookRotation(new Vector3(lookAtPos.x, 0f, lookAtPos.z));
					float num = myAi.myAgent.angularSpeed * Time.deltaTime;
					if (myAi.myAgent.angularSpeed <= 100f)
					{
						num *= 2f;
					}
					myAi.myAgent.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, num);
					if ((doingAltAttack && hasAlt[usingAltNo].chargAttack) || (doingAltAttack && hasAlt[usingAltNo].AOEAttack))
					{
						windUpTimer -= Time.deltaTime;
					}
					else if (myAi.isInKnockBack())
					{
						windUpTimer -= Time.deltaTime / 4f;
					}
					else if (Quaternion.Angle(base.transform.rotation, quaternion) <= 5f)
					{
						windUpTimer -= Time.deltaTime * 1.5f;
					}
					else
					{
						windUpTimer -= Time.deltaTime;
					}
					yield return null;
				}
				if (myAi.checkIfShouldContinue() && (bool)currentlyAttacking && myAi.myAgent.isActiveAndEnabled)
				{
					if (doingAltAttack)
					{
						yield return StartCoroutine(hasAlt[usingAltNo].Attack(currentlyAttacking));
					}
					else
					{
						yield return StartCoroutine(Attack());
					}
				}
				else if (!myAi.isDead())
				{
					RpcCancelAttack();
				}
				windUpTimer = 0f;
				if (!myAi.checkIfTargetIsAStraightLineAhead(currentlyAttacking) && canCurrentlyBeTargeted(currentlyAttacking))
				{
					break;
				}
				if (myAi.isInjuredAndNeedsToRunAway())
				{
					setNewCurrentlyAttacking(null);
				}
				yield return null;
				if ((bool)currentlyAttacking && Vector3.Distance(new Vector3(currentlyAttacking.position.x, 0f, currentlyAttacking.position.z), new Vector3(base.transform.position.x, 0f, base.transform.position.z)) > jumpTowardsDistance * 3f)
				{
					break;
				}
			}
		}
		if (myAi.myAgent.isActiveAndEnabled)
		{
			myAi.myAgent.isStopped = false;
			myAi.myAgent.Warp(myAi.myAgent.transform.position);
			myAi.myAgent.updateRotation = true;
		}
		if (attackNumbers == 0)
		{
			float cooldownTimer = coolDownTime;
			firstAttackDelay = true;
			RpcPlayTiredParts(cooldownTimer);
			while (myAi.myAgent.isActiveAndEnabled && cooldownTimer > 0f && (bool)currentlyAttacking)
			{
				cooldownTimer -= Time.deltaTime;
				yield return null;
			}
		}
	}

	private IEnumerator Attack()
	{
		RpcBasicAttack();
		if ((bool)currentlyAttacking)
		{
			float attackAnimationTime = (float)framesBeforeAttackStarts / 60f;
			StartCoroutine(jumpTowards((float)framesBeforeJump / 60f));
			yield return new WaitForSeconds(attackAnimationTime);
			if (myAi.myAgent.isActiveAndEnabled)
			{
				StartCoroutine(attackBoxOn());
			}
			if (myAi.myAgent.isActiveAndEnabled && (bool)currentlyAttacking)
			{
				yield return new WaitForSeconds((float)totalFramesOfAttackAnimation / 60f - attackAnimationTime);
			}
		}
	}

	private IEnumerator jumpTowards(float waitBefore)
	{
		float jumpDistance = 0f;
		float jumpTowardsDistanceMax = jumpTowardsDistance;
		if ((bool)currentlyAttacking)
		{
			jumpTowardsDistanceMax = ((!currentlyAttacking_Damageable || !currentlyAttacking_Damageable.myChar) ? (Mathf.Clamp(Vector3.Distance(myAi.myAgent.transform.position, currentlyAttacking.position) + 0.5f, 0f, jumpTowardsDistance) - myAi.myAgent.stoppingDistance) : Mathf.Clamp(Vector3.Distance(myAi.myAgent.transform.position, currentlyAttacking.position) + 0.5f, 0f, jumpTowardsDistance));
		}
		Quaternion startRot = myAi.myAgent.transform.rotation;
		Vector3 startingPos = myAi.myAgent.transform.position;
		Vector3 travelTowards = myAi.checkIfPointIsWalkable(myAi.myAgent.transform.position + myAi.myAgent.transform.forward * myAi.getBaseSpeed() * 15f * Time.deltaTime);
		while (jumpDistance < jumpTowardsDistanceMax)
		{
			if (myAi.isInKnockBack())
			{
				jumpTowardsDistanceMax -= 0.08f;
			}
			myAi.myAgent.updateRotation = false;
			myAi.myAgent.transform.position = travelTowards;
			myAi.myAgent.transform.rotation = startRot;
			myAi.myAgent.ResetPath();
			travelTowards = myAi.checkIfPointIsWalkable(myAi.myAgent.transform.position + myAi.myAgent.transform.forward * myAi.getBaseSpeed() * 15f * Time.deltaTime);
			jumpDistance = Vector3.Distance(startingPos, myAi.myAgent.transform.position);
			if (!(travelTowards == myAi.myAgent.transform.position))
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	public IEnumerator attackBoxOn()
	{
		myAttackBox.startAttack();
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		myAttackBox.endAttack();
	}

	public Transform returnClosestPrey(float mult = 1f)
	{
		if (Physics.CheckSphere(base.transform.position + base.transform.forward * (chaseDistance * mult / 1.5f), chaseDistance * mult, myPrey))
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position + base.transform.forward * (chaseDistance * mult / 1.5f), chaseDistance * mult, myPrey);
			if (array.Length != 0)
			{
				int num = -1;
				float num2 = chaseDistance * mult * 2f;
				for (int i = 0; i < array.Length; i++)
				{
					float num3 = Vector3.Distance(base.transform.position, array[i].transform.position);
					if (num3 < num2 && myAi.checkIfCanGetToTarget(array[i].transform) && canCurrentlyBeTargeted(array[i].transform.root) && (!myAi.waterOnly || (myAi.waterOnly && array[i].transform.position.y <= -0.1f)))
					{
						AnimalAI componentInParent = array[i].GetComponentInParent<AnimalAI>();
						if (!componentInParent || ((bool)componentInParent && componentInParent.animalId != myAi.animalId))
						{
							num = i;
							num2 = num3;
						}
					}
				}
				if (num != -1)
				{
					return array[num].transform.root;
				}
			}
		}
		return null;
	}

	public bool isAttackPlaying()
	{
		return false;
	}

	public void setCurrentlyPlayingAttack(bool newPlayingAttack)
	{
		currentlyPlayingAttack = newPlayingAttack;
	}

	[ClientRpc]
	private void RpcBasicAttack()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(AnimalAI_Attack), "RpcBasicAttack", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcPlayWindUp()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(AnimalAI_Attack), "RpcPlayWindUp", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcPlayWindUpAlt()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(AnimalAI_Attack), "RpcPlayWindUpAlt", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcPlayTiredParts(float timer)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteFloat(timer);
		SendRPCInternal(typeof(AnimalAI_Attack), "RpcPlayTiredParts", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcCancelAttack()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(AnimalAI_Attack), "RpcCancelAttack", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator playTiredParts(float timer)
	{
		float sweatTimer = 0f;
		playSweatParticles();
		while (myAi.checkIfShouldContinue() && timer > 0f && !myAi.isDead())
		{
			timer -= Time.deltaTime;
			sweatTimer += Time.deltaTime;
			if (sweatTimer > 0.5f && timer > 0.5f)
			{
				playSweatParticles();
				sweatTimer = 0f;
			}
			yield return null;
		}
	}

	private void endCurrentlyPlayingAttack()
	{
		setCurrentlyPlayingAttack(false);
	}

	public void setNewCurrentlyAttacking(Transform newAttacking)
	{
		bool num = newAttacking == currentlyAttacking;
		if (currentlyAttacking == null && newAttacking != null)
		{
			if (Vector3.Distance(newAttacking.position, base.transform.position) <= 4f)
			{
				firstAttackDelay = true;
			}
			else
			{
				firstAttackDelay = false;
			}
		}
		else
		{
			firstAttackDelay = false;
		}
		if (newAttacking != null)
		{
			currentlyAttacking = newAttacking.root;
		}
		else
		{
			currentlyAttacking = null;
		}
		if (currentlyAttacking == base.transform)
		{
			currentlyAttacking = null;
		}
		if (currentlyAttacking != null)
		{
			currentlyAttacking_Damageable = currentlyAttacking.GetComponentInParent<Damageable>();
			if (!currentlyAttacking_Damageable)
			{
				currentlyAttacking = null;
			}
			else
			{
				if (currentlyAttacking_Damageable.health <= 0)
				{
					currentlyAttacking = null;
				}
				if (((bool)myAi.isAPet() && currentlyAttacking == myAi.isAPet().followingTransform) || ((bool)myAi.isAPet() && (bool)currentlyAttacking.GetComponent<AnimalAI>().isAPet()))
				{
					currentlyAttacking = null;
					currentlyAttacking_Damageable = null;
				}
				if ((bool)currentlyAttacking)
				{
					CharMovement component = currentlyAttacking.GetComponent<CharMovement>();
					if (((bool)component && component.isCurrentlyTalking) || ((bool)component && component.myPickUp.sittingPos != Vector3.zero) || ((bool)component && !NetworkMapSharer.share.nextDayIsReady))
					{
						currentlyAttacking = null;
						currentlyAttacking_Damageable = null;
					}
				}
			}
		}
		else
		{
			currentlyAttacking_Damageable = null;
			myAi.myAgent.speed = myAi.getBaseSpeed();
		}
		if (num && currentlyAttacking != null)
		{
			myAi.cancleCurrentDestination();
			myAi.myAgent.SetDestination(currentlyAttacking.position);
		}
		if ((bool)currentlyAttacking)
		{
			setNewAttackingChar();
			return;
		}
		removeAttackingChar();
		currentlyAttacking_Damageable = null;
	}

	public void removeAttackingChar()
	{
		if (setPlayerInDangerWhenTargeted && (bool)currentlyAttackingChar)
		{
			CharMovement charMovement = currentlyAttackingChar;
			charMovement.NetworkbeingTargetedBy = charMovement.beingTargetedBy - 1;
			currentlyAttackingChar = null;
		}
	}

	public void setNewAttackingChar()
	{
		if (!setPlayerInDangerWhenTargeted)
		{
			return;
		}
		if ((bool)currentlyAttacking_Damageable && (bool)currentlyAttacking_Damageable.myChar)
		{
			if ((bool)currentlyAttackingChar && currentlyAttacking_Damageable.myChar != currentlyAttackingChar)
			{
				removeAttackingChar();
				currentlyAttackingChar = currentlyAttacking_Damageable.myChar;
				CharMovement charMovement = currentlyAttackingChar;
				charMovement.NetworkbeingTargetedBy = charMovement.beingTargetedBy + 1;
			}
			else if (currentlyAttackingChar == null)
			{
				currentlyAttackingChar = currentlyAttacking_Damageable.myChar;
				CharMovement charMovement2 = currentlyAttackingChar;
				charMovement2.NetworkbeingTargetedBy = charMovement2.beingTargetedBy + 1;
			}
		}
		else if ((bool)currentlyAttackingChar)
		{
			removeAttackingChar();
		}
	}

	private IEnumerator chaseAndThenJumpUpOrDown()
	{
		myAi.myAgent.isStopped = true;
		myAi.myAgent.updateRotation = false;
		myAi.myAgent.ResetPath();
		Vector3 normalized = (currentlyAttacking.position - base.transform.position).normalized;
		Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
		float faceTargetTimer = 0f;
		while (!myAi.isDead() && faceTargetTimer < 0.25f && (bool)currentlyAttacking)
		{
			normalized = (currentlyAttacking.position - base.transform.position).normalized;
			Quaternion to = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z));
			float maxDegreesDelta = myAi.myAgent.angularSpeed * Time.deltaTime;
			myAi.myAgent.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, maxDegreesDelta);
			yield return null;
			faceTargetTimer += Time.deltaTime;
		}
		Vector3 posToCheckHeight = base.transform.position + base.transform.forward * Random.Range(1, 5);
		posToCheckHeight.y = WorldManager.manageWorld.heightMap[Mathf.RoundToInt(posToCheckHeight.x / 2f), Mathf.RoundToInt(posToCheckHeight.z / 2f)];
		int num = Mathf.RoundToInt(posToCheckHeight.y) - Mathf.RoundToInt(base.transform.position.y);
		if ((float)num == 2f || (float)num <= -2f)
		{
			yield return StartCoroutine(jumpFeel(posToCheckHeight));
			myAi.myAgent.Warp(posToCheckHeight);
			yield return myAi.checkTimer;
		}
		else
		{
			myAi.myAgent.SetDestination(currentlyAttacking.transform.position);
			yield return myAi.checkTimer;
			yield return myAi.checkTimer;
		}
		myAi.myAgent.speed = myAi.getBaseSpeed();
		myAi.myAgent.isStopped = false;
		myAi.myAgent.updateRotation = true;
	}

	public bool canCurrentlyBeTargeted(Transform target)
	{
		if (!myAi.checkIfShouldContinue())
		{
			return false;
		}
		if (target == base.transform)
		{
			return false;
		}
		if (target != null)
		{
			Damageable componentInParent = target.GetComponentInParent<Damageable>();
			if (!componentInParent)
			{
				return false;
			}
			if (componentInParent.health <= 0)
			{
				return false;
			}
			if (((bool)myAi.isAPet() && target == myAi.isAPet().followingTransform) || ((bool)myAi.isAPet() && (bool)target.GetComponent<AnimalAI>().isAPet()))
			{
				return false;
			}
			CharMovement component = target.GetComponent<CharMovement>();
			if ((bool)component && component.isCurrentlyTalking)
			{
				return false;
			}
		}
		return true;
	}

	public bool hasTarget()
	{
		return currentlyAttacking;
	}

	public void setAnimator(Animator anim)
	{
		myAnim = anim;
	}

	public void playSupriseParticle()
	{
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.supriseParticle, base.transform.position + Vector3.up * 1.5f, 1);
	}

	public void playSweatParticles()
	{
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.sweatParticles, base.transform.position + Vector3.up * 1.5f, Random.Range(4, 5));
	}

	public bool isInJump()
	{
		return inJump;
	}

	private IEnumerator jumpFeel(Vector3 jumpToPosition)
	{
		inJump = true;
		float jumpHeight2 = jumpToPosition.y;
		if (jumpToPosition.y < myAi.myAgent.transform.position.y)
		{
			jumpHeight2 = base.transform.position.y + 1f;
		}
		jumpHeight2 += 1.5f;
		float halfHeight = Mathf.Lerp(myAi.myAgent.transform.position.y, jumpHeight2, 0.6f);
		float jumpSpeed = 2f;
		myAnim.SetBool("Grounded", false);
		while (base.transform.position.y < jumpHeight2)
		{
			yield return null;
			base.transform.position = base.transform.position + Vector3.up * Time.fixedDeltaTime * jumpSpeed;
			if (base.transform.position.y >= halfHeight)
			{
				base.transform.position = Vector3.Lerp(base.transform.position, new Vector3(jumpToPosition.x, base.transform.position.y, jumpToPosition.z), Time.deltaTime * 2f);
			}
			jumpSpeed += Time.deltaTime * 3f;
			MonoBehaviour.print("Moving object up");
		}
		while (Vector3.Distance(base.transform.position, jumpToPosition) > 1f)
		{
			yield return null;
			base.transform.position = Vector3.Lerp(base.transform.position, jumpToPosition, Time.deltaTime * 4f);
		}
		myAnim.SetBool("Grounded", true);
		inJump = false;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcBasicAttack()
	{
		setCurrentlyPlayingAttack(true);
		if ((bool)myAnim)
		{
			myAnim.SetTrigger(basicAttackAnimation);
		}
	}

	protected static void InvokeUserCode_RpcBasicAttack(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcBasicAttack called on server.");
		}
		else
		{
			((AnimalAI_Attack)obj).UserCode_RpcBasicAttack();
		}
	}

	protected void UserCode_RpcPlayWindUp()
	{
		playSupriseParticle();
		if ((bool)myAnim)
		{
			myAnim.SetTrigger(windUpAnimation);
		}
	}

	protected static void InvokeUserCode_RpcPlayWindUp(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayWindUp called on server.");
		}
		else
		{
			((AnimalAI_Attack)obj).UserCode_RpcPlayWindUp();
		}
	}

	protected void UserCode_RpcPlayWindUpAlt()
	{
		playSupriseParticle();
		if ((bool)myAnim)
		{
			myAnim.SetTrigger(windUpAltAnimation);
		}
	}

	protected static void InvokeUserCode_RpcPlayWindUpAlt(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayWindUpAlt called on server.");
		}
		else
		{
			((AnimalAI_Attack)obj).UserCode_RpcPlayWindUpAlt();
		}
	}

	protected void UserCode_RpcPlayTiredParts(float timer)
	{
		StartCoroutine(playTiredParts(timer));
	}

	protected static void InvokeUserCode_RpcPlayTiredParts(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayTiredParts called on server.");
		}
		else
		{
			((AnimalAI_Attack)obj).UserCode_RpcPlayTiredParts(reader.ReadFloat());
		}
	}

	protected void UserCode_RpcCancelAttack()
	{
		myAnim.SetTrigger(stopAnimation);
	}

	protected static void InvokeUserCode_RpcCancelAttack(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCancelAttack called on server.");
		}
		else
		{
			((AnimalAI_Attack)obj).UserCode_RpcCancelAttack();
		}
	}

	static AnimalAI_Attack()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalAI_Attack), "RpcBasicAttack", InvokeUserCode_RpcBasicAttack);
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalAI_Attack), "RpcPlayWindUp", InvokeUserCode_RpcPlayWindUp);
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalAI_Attack), "RpcPlayWindUpAlt", InvokeUserCode_RpcPlayWindUpAlt);
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalAI_Attack), "RpcPlayTiredParts", InvokeUserCode_RpcPlayTiredParts);
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalAI_Attack), "RpcCancelAttack", InvokeUserCode_RpcCancelAttack);
	}
}
