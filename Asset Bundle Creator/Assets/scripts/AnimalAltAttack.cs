using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class AnimalAltAttack : NetworkBehaviour
{
	private AnimalAI myAi;

	private AnimalAI_Attack attacks;

	public ItemHitBox attackBox;

	public bool hasAltWindUp;

	public float jumpTowardsDistance = 1f;

	public float attackStartAtFrame = 30f;

	public int totalAnimationFrames = 60;

	public int framesHitBoxOn = 3;

	[Header("AOE Attack Details")]
	public bool AOEAttack = true;

	public float AOEWindUpTime = 1f;

	public GameObject aOESpawn;

	public Transform AOEFireFrom;

	[Header("Charge Attack Details")]
	public bool chargAttack;

	public float chargeAtSpeedMultiplier = 3f;

	public ASound altChargeSound;

	[Header("Spin Attack Details")]
	public bool spinAttack;

	[Header("Call For Help Details")]
	public bool callForHelp;

	public ASound callforHelpSound;

	public LayerMask animalLayer;

	public Transform headPos;

	[Header("Fire Projectile Attack Details")]
	public bool fireProjectile;

	public bool fire3Projectiles;

	public ASound altAttackSound;

	[Header("Alt AnimationName")]
	public string altAnimName = "AltAttack";

	public float chargeTime = 1.5f;

	private bool callforhelpReady = true;

	private void Start()
	{
		myAi = GetComponent<AnimalAI>();
		attacks = GetComponent<AnimalAI_Attack>();
	}

	public override void OnStartServer()
	{
		callforhelpReady = true;
	}

	public override void OnStartClient()
	{
		if (!base.isServer)
		{
			myAi = GetComponent<AnimalAI>();
			attacks = GetComponent<AnimalAI_Attack>();
		}
	}

	public IEnumerator Attack(Transform currentlyAttacking)
	{
		if (!currentlyAttacking)
		{
			RpcCancelAttack();
		}
		if (AOEAttack)
		{
			yield return StartCoroutine(startAOE());
		}
		if (chargAttack)
		{
			yield return StartCoroutine(startChargeAttack());
		}
		if (spinAttack)
		{
			yield return StartCoroutine(startSpinAttack());
		}
		if (callForHelp)
		{
			yield return StartCoroutine(startCallForHelp());
		}
		if (fireProjectile)
		{
			yield return StartCoroutine(startFireProjectile());
		}
	}

	private IEnumerator startAOE()
	{
		float attackTimer = 0f;
		int randomSeed = Mathf.RoundToInt(base.transform.position.x + base.transform.position.y * base.transform.position.z * (float)RealWorldTimeLight.time.currentHour / (float)RealWorldTimeLight.time.currentMinute);
		RpcAltAttack();
		RpcAltAttackAOE(randomSeed);
		float attackAnimationTime = attackStartAtFrame / 60f;
		float totalDamageAnimation = (float)totalAnimationFrames / 60f;
		while (attackTimer < attackAnimationTime && myAi.myAgent.isActiveAndEnabled)
		{
			attackTimer += Time.deltaTime;
			yield return null;
		}
		if (!myAi.myAgent.isActiveAndEnabled)
		{
			yield break;
		}
		if (myAi.myAgent.isActiveAndEnabled)
		{
			if ((bool)aOESpawn)
			{
				spawnAOEObject(randomSeed);
			}
			else
			{
				StartCoroutine(hitBoxOn());
			}
		}
		yield return new WaitForSeconds(totalDamageAnimation - attackAnimationTime);
	}

	public void spawnAOEObject(int randomSeed)
	{
		GameObject gameObject = ((!AOEFireFrom) ? Object.Instantiate(aOESpawn, base.transform.position, base.transform.rotation) : Object.Instantiate(aOESpawn, AOEFireFrom.position, base.transform.rotation));
		if ((bool)gameObject.GetComponent<GroundAttack>())
		{
			gameObject.GetComponent<GroundAttack>().attachedToAnimal = myAi;
		}
		else if ((bool)gameObject.GetComponent<FallingProjectileAOE>())
		{
			gameObject.GetComponent<FallingProjectileAOE>().setUpAndFire(myAi, randomSeed);
		}
		else if ((bool)gameObject.GetComponent<EruptionAttack>())
		{
			gameObject.GetComponent<EruptionAttack>().fireFromAnimal(myAi);
		}
	}

	private IEnumerator startChargeAttack()
	{
		Quaternion startRot = myAi.myAgent.transform.rotation;
		Vector3 travelTowards = myAi.checkIfPointIsWalkable(myAi.myAgent.transform.position + myAi.myAgent.transform.forward * myAi.getBaseSpeed() * chargeAtSpeedMultiplier * Time.deltaTime);
		attackBox.startAttack();
		float chargeTimer = 0f;
		RpcAltAttack();
		while (chargeTimer < chargeTime)
		{
			myAi.myAgent.transform.rotation = startRot;
			myAi.myAgent.transform.position = travelTowards;
			if (myAi.myAgent.isActiveAndEnabled)
			{
				myAi.myAgent.ResetPath();
			}
			travelTowards = myAi.checkIfPointIsWalkable(myAi.myAgent.transform.position + myAi.myAgent.transform.forward * myAi.getBaseSpeed() * chargeAtSpeedMultiplier * Time.deltaTime);
			if (travelTowards == myAi.myAgent.transform.position)
			{
				for (float aBitMoreTimer = 0.25f; aBitMoreTimer > 0f; aBitMoreTimer -= Time.deltaTime)
				{
					yield return null;
					myAi.myAgent.transform.rotation = startRot;
					myAi.myAgent.transform.position = myAi.myAgent.transform.position + myAi.myAgent.transform.forward * myAi.getBaseSpeed() * chargeAtSpeedMultiplier * Time.deltaTime;
				}
				break;
			}
			chargeTimer += Time.deltaTime;
			yield return null;
		}
		RpcEndAlt();
		attackBox.endAttack();
	}

	private IEnumerator startSpinAttack()
	{
		Quaternion rotation = myAi.myAgent.transform.rotation;
		Vector3 travelTowards = myAi.checkIfPointIsWalkable(myAi.myAgent.transform.position + myAi.myAgent.transform.forward * myAi.getBaseSpeed() * 3f * Time.deltaTime);
		attackBox.startAttack();
		float chargeTimer = 0f;
		RpcAltAttack();
		while (chargeTimer < 1.5f)
		{
			myAi.myAgent.transform.position = travelTowards;
			myAi.myAgent.ResetPath();
			myAi.myAgent.transform.Rotate(Vector3.up, 5f);
			travelTowards = myAi.checkIfPointIsWalkable(myAi.myAgent.transform.position + myAi.myAgent.transform.forward * myAi.getBaseSpeed() * 3f * Time.deltaTime);
			if (travelTowards == myAi.myAgent.transform.position)
			{
				break;
			}
			chargeTimer += Time.deltaTime;
			yield return null;
		}
		RpcEndAlt();
		attackBox.endAttack();
	}

	private IEnumerator startCallForHelp()
	{
		float attackTimer = 0f;
		RpcAltAttack();
		float attackAnimationTime = attackStartAtFrame / 60f;
		float totalDamageAnimation = (float)totalAnimationFrames / 60f;
		while (attackTimer < attackAnimationTime && myAi.myAgent.isActiveAndEnabled)
		{
			attackTimer += Time.deltaTime;
			yield return null;
		}
		StartCoroutine(emitCallSoundAndParticle());
		if (Physics.CheckSphere(base.transform.root.position, attacks.chaseDistance, animalLayer))
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, attacks.chaseDistance, animalLayer);
			for (int i = 0; i < array.Length; i++)
			{
				AnimalAI componentInParent = array[i].GetComponentInParent<AnimalAI>();
				if ((bool)componentInParent && componentInParent.animalId == myAi.animalId)
				{
					componentInParent.setCurrentlyAttacking(myAi.currentlyAttacking());
					componentInParent.wakeUpAnimal();
					componentInParent.GetComponent<AnimalAltAttack>().resetCallForHelpTimer();
				}
			}
		}
		resetCallForHelpTimer();
		if (myAi.myAgent.isActiveAndEnabled)
		{
			yield return new WaitForSeconds(totalDamageAnimation - attackAnimationTime);
		}
	}

	private IEnumerator startFireProjectile()
	{
		float attackTimer = 0f;
		RpcAltAttack();
		float attackAnimationTime = attackStartAtFrame / 60f;
		float totalDamageAnimation = (float)totalAnimationFrames / 60f;
		while (attackTimer < attackAnimationTime && myAi.myAgent.isActiveAndEnabled)
		{
			attackTimer += Time.deltaTime;
			yield return null;
		}
		if (fire3Projectiles)
		{
			NetworkMapSharer.share.fireProjectile(1, base.transform, base.transform.forward);
			NetworkMapSharer.share.fireProjectile(1, base.transform, base.transform.forward + base.transform.right / 2f);
			NetworkMapSharer.share.fireProjectile(1, base.transform, base.transform.forward - base.transform.right / 2f);
		}
		else
		{
			NetworkMapSharer.share.fireProjectile(1, base.transform, base.transform.forward);
		}
		if (myAi.myAgent.isActiveAndEnabled)
		{
			yield return new WaitForSeconds(totalDamageAnimation - attackAnimationTime);
		}
	}

	private IEnumerator emitCallSoundAndParticle()
	{
		SoundManager.manage.playASoundAtPoint(callforHelpSound, base.transform.position);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.callForHelpPart, headPos.position, 1);
		float timer = callforHelpSound.myClips[0].length - 0.25f;
		float particleTimer = 0f;
		while (timer > 0f)
		{
			if (particleTimer >= 0.25f)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.callForHelpPart, headPos.position, 1);
				particleTimer = 0f;
			}
			particleTimer += Time.deltaTime;
			timer -= Time.deltaTime;
			yield return null;
		}
	}

	public bool checkIfNeedToCallForHelp()
	{
		if (callforhelpReady && Physics.CheckSphere(base.transform.root.position, attacks.chaseDistance, animalLayer))
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, attacks.chaseDistance, animalLayer);
			for (int i = 0; i < array.Length; i++)
			{
				AnimalAI componentInParent = array[i].GetComponentInParent<AnimalAI>();
				if ((bool)componentInParent && componentInParent.animalId == myAi.animalId && componentInParent.currentlyAttacking() != myAi.currentlyAttacking())
				{
					return true;
				}
			}
		}
		return false;
	}

	private IEnumerator hitBoxOn()
	{
		attackBox.startAttack();
		float timer = (float)framesHitBoxOn / 60f;
		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			yield return null;
		}
		attackBox.endAttack();
	}

	[ClientRpc]
	public void RpcAltAttack()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(AnimalAltAttack), "RpcAltAttack", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcAltAttackAOE(int randomSeed)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(randomSeed);
		SendRPCInternal(typeof(AnimalAltAttack), "RpcAltAttackAOE", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcCancelAttack()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(AnimalAltAttack), "RpcCancelAttack", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator fireProjectileClient()
	{
		float attackTimer = 0f;
		float attackAnimationTime = attackStartAtFrame / 60f;
		while (attackTimer < attackAnimationTime)
		{
			attackTimer += Time.deltaTime;
			yield return null;
		}
		if (fire3Projectiles)
		{
			NetworkMapSharer.share.fireProjectile(1, base.transform, base.transform.forward);
			NetworkMapSharer.share.fireProjectile(1, base.transform, base.transform.forward + base.transform.right / 2f);
			NetworkMapSharer.share.fireProjectile(1, base.transform, base.transform.forward - base.transform.right / 2f);
		}
		else
		{
			NetworkMapSharer.share.fireProjectile(1, base.transform, base.transform.forward);
		}
	}

	private IEnumerator callForHelpClient()
	{
		float attackTimer = 0f;
		float attackAnimationTime = attackStartAtFrame / 60f;
		while (attackTimer < attackAnimationTime)
		{
			attackTimer += Time.deltaTime;
			yield return null;
		}
		StartCoroutine(emitCallSoundAndParticle());
	}

	private IEnumerator spawnAOEClient(int randomSeed)
	{
		float attackTimer = 0f;
		float attackAnimationTime = attackStartAtFrame / 60f;
		while (attackTimer < attackAnimationTime)
		{
			attackTimer += Time.deltaTime;
			yield return null;
		}
		if (!myAi.isDead() && (bool)aOESpawn)
		{
			spawnAOEObject(randomSeed);
		}
	}

	[ClientRpc]
	public void RpcEndAlt()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(AnimalAltAttack), "RpcEndAlt", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private void OnEnable()
	{
		if ((bool)attackBox)
		{
			attackBox.endAttack();
		}
	}

	public void playAltAttackSound()
	{
		if ((bool)altAttackSound)
		{
			SoundManager.manage.playASoundAtPoint(altAttackSound, base.transform.position);
		}
	}

	public void playAltChargeSound()
	{
		if ((bool)altChargeSound)
		{
			SoundManager.manage.playASoundAtPoint(altChargeSound, base.transform.position);
		}
	}

	public void resetCallForHelpTimer()
	{
		StopCoroutine("callForHelpTimer");
		StartCoroutine("callForHelpTimer");
	}

	private IEnumerator callForHelpTimer()
	{
		callforhelpReady = false;
		yield return new WaitForSeconds(Random.Range(5f, 15f));
		callforhelpReady = true;
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcAltAttack()
	{
		attacks.setCurrentlyPlayingAttack(true);
		GetComponent<Animator>().ResetTrigger(altAnimName);
		GetComponent<Animator>().SetTrigger(altAnimName);
		if (!base.isServer && callForHelp)
		{
			StartCoroutine(callForHelpClient());
		}
		if (!base.isServer && fireProjectile)
		{
			StartCoroutine(fireProjectileClient());
		}
	}

	protected static void InvokeUserCode_RpcAltAttack(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAltAttack called on server.");
		}
		else
		{
			((AnimalAltAttack)obj).UserCode_RpcAltAttack();
		}
	}

	protected void UserCode_RpcAltAttackAOE(int randomSeed)
	{
		if (!base.isServer && AOEAttack && (bool)aOESpawn)
		{
			StartCoroutine(spawnAOEClient(randomSeed));
		}
	}

	protected static void InvokeUserCode_RpcAltAttackAOE(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAltAttackAOE called on server.");
		}
		else
		{
			((AnimalAltAttack)obj).UserCode_RpcAltAttackAOE(reader.ReadInt());
		}
	}

	protected void UserCode_RpcCancelAttack()
	{
		GetComponent<Animator>().SetTrigger("StopAnimation");
	}

	protected static void InvokeUserCode_RpcCancelAttack(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCancelAttack called on server.");
		}
		else
		{
			((AnimalAltAttack)obj).UserCode_RpcCancelAttack();
		}
	}

	protected void UserCode_RpcEndAlt()
	{
		GetComponent<Animator>().ResetTrigger("EndAttack");
		GetComponent<Animator>().SetTrigger("EndAttack");
	}

	protected static void InvokeUserCode_RpcEndAlt(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcEndAlt called on server.");
		}
		else
		{
			((AnimalAltAttack)obj).UserCode_RpcEndAlt();
		}
	}

	static AnimalAltAttack()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalAltAttack), "RpcAltAttack", InvokeUserCode_RpcAltAttack);
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalAltAttack), "RpcAltAttackAOE", InvokeUserCode_RpcAltAttackAOE);
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalAltAttack), "RpcCancelAttack", InvokeUserCode_RpcCancelAttack);
		RemoteCallHelper.RegisterRpcDelegate(typeof(AnimalAltAttack), "RpcEndAlt", InvokeUserCode_RpcEndAlt);
	}
}
