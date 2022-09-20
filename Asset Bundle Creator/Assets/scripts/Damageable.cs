using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class Damageable : NetworkBehaviour
{
	public enum onlyDamageWithToolType
	{
		All = 0,
		Wood = 1,
		HardWood = 2,
		Stone = 3,
		HardStone = 4
	}

	[SyncVar(hook = "OnTakeDamage")]
	public int health = 100;

	[SyncVar(hook = "onFireChange")]
	public bool onFire;

	[SyncVar(hook = "onPoisonChange")]
	public bool poisoned;

	[SyncVar(hook = "onStunned")]
	private bool stunned;

	public int maxHealth = 100;

	private AnimalAI myAnimalAi;

	public CharMovement myChar;

	public Transform[] dropPositions;

	public InventoryItemLootTable lootDrops;

	public GameObject spawnWorldObjectOnDeath;

	public Animator myAnim;

	public bool instantDie;

	public bool isFriendly;

	private Vehicle isVehicle;

	public Transform headPos;

	public ASound customDamageSound;

	public ASound customDeathSound;

	[Header("Can only be damaged by tool")]
	public onlyDamageWithToolType damageType;

	public float defence = 1f;

	private bool canBeDamaged = true;

	private static WaitForSeconds delayWait;

	private static WaitForSeconds animalWait;

	private bool canBeStunned = true;

	private Coroutine regenRoutine;

	private static WaitForSeconds regenWait;

	public int Networkhealth
	{
		get
		{
			return health;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref health))
			{
				int oldHealth = health;
				SetSyncVar(value, ref health, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					OnTakeDamage(oldHealth, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public bool NetworkonFire
	{
		get
		{
			return onFire;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref onFire))
			{
				bool old = onFire;
				SetSyncVar(value, ref onFire, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, true);
					onFireChange(old, value);
					setSyncVarHookGuard(2uL, false);
				}
			}
		}
	}

	public bool Networkpoisoned
	{
		get
		{
			return poisoned;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref poisoned))
			{
				bool old = poisoned;
				SetSyncVar(value, ref poisoned, 4uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4uL))
				{
					setSyncVarHookGuard(4uL, true);
					onPoisonChange(old, value);
					setSyncVarHookGuard(4uL, false);
				}
			}
		}
	}

	public bool Networkstunned
	{
		get
		{
			return stunned;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref stunned))
			{
				bool old = stunned;
				SetSyncVar(value, ref stunned, 8uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(8uL))
				{
					setSyncVarHookGuard(8uL, true);
					onStunned(old, value);
					setSyncVarHookGuard(8uL, false);
				}
			}
		}
	}

	private void OnEnable()
	{
		myAnimalAi = GetComponent<AnimalAI>();
		myChar = GetComponent<CharMovement>();
		if ((bool)myChar || (bool)GetComponent<AnimalAI_Pet>() || (bool)GetComponent<NPCAI>() || (bool)GetComponent<FarmAnimal>())
		{
			isFriendly = true;
		}
	}

	public void doDamageFromStatus(int damageToDeal)
	{
		changeHealth(-damageToDeal);
	}

	public AnimalAI isAnAnimal()
	{
		return myAnimalAi;
	}

	public void attackAndDoDamage(int damageToDeal, Transform attackedBy, float knockBackAmount = 2.5f)
	{
		if (canBeDamaged)
		{
			StartCoroutine("canBeDamagedDelay");
			if (knockBackAmount > 0f && (bool)myChar)
			{
				Vector3 knockBackDir = -(attackedBy.position - base.transform.position).normalized;
				knockBackDir.y = 0f;
				myChar.RpcTakeKnockback(knockBackDir, knockBackAmount * 3.5f);
			}
			if ((bool)myAnimalAi && (bool)attackedBy)
			{
				myAnimalAi.takeHitAndKnockBack(attackedBy, knockBackAmount);
			}
			changeHealth(-Mathf.RoundToInt((float)damageToDeal / defence));
		}
	}

	private IEnumerator canBeDamagedDelay()
	{
		canBeDamaged = false;
		if ((bool)myAnimalAi)
		{
			yield return animalWait;
		}
		else
		{
			yield return delayWait;
		}
		canBeDamaged = true;
	}

	public void changeHealth(int dif)
	{
		Networkhealth = Mathf.Clamp(health + dif, 0, maxHealth);
	}

	private void OnTakeDamage(int oldHealth, int newHealth)
	{
		int num = newHealth - oldHealth;
		newHealth = (((bool)isVehicle && (!isVehicle || isVehicle.hasDriver())) ? health : Mathf.Clamp(newHealth, 0, maxHealth));
		if (oldHealth > newHealth && (bool)myChar && this == StatusManager.manage.connectedDamge)
		{
			StatusManager.manage.takeDamageUIChanges(Mathf.Abs(num));
		}
		if (newHealth < oldHealth)
		{
			if ((bool)myAnim && !myAnimalAi)
			{
				if (checkIfShouldShowDamage(num))
				{
					myAnim.SetTrigger("TakeHit");
				}
			}
			else if ((bool)myAnimalAi)
			{
				if (newHealth <= 0)
				{
					onAnimalDeath();
				}
				else if (checkIfShouldShowDamage(num))
				{
					myAnimalAi.takeAHitLocal();
				}
			}
			else if ((bool)isVehicle && newHealth <= 0)
			{
				for (int i = 0; i < Inventory.inv.allItems.Length; i++)
				{
					if ((bool)Inventory.inv.allItems[i].spawnPlaceable && (bool)Inventory.inv.allItems[i].spawnPlaceable.GetComponent<Vehicle>() && Inventory.inv.allItems[i].spawnPlaceable.GetComponent<Vehicle>().saveId == isVehicle.saveId)
					{
						NetworkMapSharer.share.spawnAServerDrop(i, isVehicle.getVariation() + 1, base.transform.position, null, true);
						break;
					}
				}
			}
			if (StatusManager.manage.connectedDamge == this && checkIfShouldShowDamage(num))
			{
				CameraController.control.myShake.addToTraumaMax(0.35f, 0.5f);
			}
			if (checkIfShouldShowDamage(num))
			{
				if (newHealth <= 0)
				{
					ParticleManager.manage.emitAttackParticle(base.transform.position + Vector3.up / 2f, 100);
					ParticleManager.manage.emitRedAttackParticle(base.transform.position + Vector3.up / 2f, 100);
				}
				else
				{
					ParticleManager.manage.emitAttackParticle(base.transform.position + Vector3.up / 2f);
					ParticleManager.manage.emitRedAttackParticle(base.transform.position + Vector3.up / 2f);
				}
			}
			if (checkIfShouldShowDamage(num))
			{
				if ((bool)customDeathSound && newHealth <= 0)
				{
					SoundManager.manage.playASoundAtPoint(customDeathSound, base.transform.position);
				}
				else if ((bool)customDamageSound && newHealth > 0)
				{
					SoundManager.manage.playASoundAtPoint(customDamageSound, base.transform.position);
				}
				else if ((bool)myAnimalAi || (bool)myChar)
				{
					if (newHealth <= 0)
					{
						SoundManager.manage.playASoundAtPoint(SoundManager.manage.finalImpactSound, base.transform.position);
					}
					else
					{
						SoundManager.manage.playASoundAtPoint(SoundManager.manage.impactDamageSound, base.transform.position);
					}
				}
				else if (newHealth <= 0)
				{
					SoundManager.manage.playASoundAtPoint(SoundManager.manage.nonOrganicFinalHitSound, base.transform.position);
				}
				else
				{
					SoundManager.manage.playASoundAtPoint(SoundManager.manage.nonOrganicHitSound, base.transform.position);
				}
			}
			else if (health < maxHealth)
			{
				SoundManager.manage.playASoundAtPoint(SoundManager.manage.statusDamageSound, base.transform.position);
			}
			if (newHealth <= 0 && (bool)spawnWorldObjectOnDeath)
			{
				if (base.isServer)
				{
					PickUpAndCarry component = GetComponent<PickUpAndCarry>();
					if ((bool)component && component.beingCarriedBy != 0)
					{
						NetworkIdentity.spawned[component.beingCarriedBy].GetComponent<CharPickUp>().RpcDropCarriedItem();
						component.NetworkbeingCarriedBy = 0u;
					}
					NetworkServer.Destroy(base.gameObject);
				}
			}
			else if (base.isServer && newHealth <= 0)
			{
				PickUpAndCarry component2 = GetComponent<PickUpAndCarry>();
				TrappedAnimal component3 = GetComponent<TrappedAnimal>();
				if ((bool)component2 && component2.beingCarriedBy != 0)
				{
					NetworkIdentity.spawned[component2.beingCarriedBy].GetComponent<CharPickUp>().RpcDropCarriedItem();
					component2.NetworkbeingCarriedBy = 0u;
				}
				if ((bool)component3 && !component3.hasBeenOpenedLocal)
				{
					component3.RpcOpen();
					if (base.isServer)
					{
						Vector3 position = base.transform.position;
						NetworkNavMesh.nav.SpawnAnAnimalOnTile(component3.trappedAnimalId * 10 + component3.trappedAnimalVariation, (int)(position.x / 2f), (int)(position.z / 2f), null, component3.getAnimalInsideHealth(), GetComponent<PickUpAndCarry>().getLastCarriedBy());
						NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(component3.trapItemDropAfterOpen), 1, base.transform.position);
						NetworkServer.Destroy(base.gameObject);
					}
					component3.hasBeenOpenedLocal = true;
				}
				if ((bool)component2 && !component3)
				{
					Transform[] array = dropPositions;
					foreach (Transform transform in array)
					{
						InventoryItem randomDropFromTable = lootDrops.getRandomDropFromTable();
						if ((bool)randomDropFromTable)
						{
							int xPType = -1;
							if ((bool)isAnAnimal())
							{
								xPType = 5;
							}
							NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(randomDropFromTable), 1, transform.position, null, true, xPType);
						}
					}
					NetworkServer.Destroy(base.gameObject);
				}
			}
		}
		Networkhealth = newHealth;
		if (base.isServer && (bool)isVehicle && health <= 0)
		{
			SaveLoad.saveOrLoad.vehiclesToSave.Remove(isVehicle);
			NetworkServer.Destroy(base.gameObject);
		}
	}

	public bool checkIfShouldShowDamage(int healthDif)
	{
		if (health == maxHealth)
		{
			return false;
		}
		if (healthDif < -1 || (healthDif == -1 && !onFire && !poisoned))
		{
			return true;
		}
		return false;
	}

	public override void OnStopClient()
	{
		if ((base.isServer && health <= 0) || !base.isServer)
		{
			Transform[] array = dropPositions;
			foreach (Transform transform in array)
			{
				ParticleManager.manage.emitDeathParticle(transform.position);
			}
		}
		if ((bool)isVehicle)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position, 10);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position - base.transform.forward * 0.5f, 10);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position - base.transform.forward * 0.5f, 10);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position + base.transform.right * 0.5f, 10);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position - base.transform.right * 0.5f, 10);
		}
		PickUpAndCarry component = GetComponent<PickUpAndCarry>();
		if ((bool)component && health <= 0)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position, 10);
			if (base.isServer && component.investigationItem)
			{
				BulletinBoard.board.checkForInvestigationPostAndComplete(base.transform.position);
			}
		}
		if (base.isServer && (bool)spawnWorldObjectOnDeath && health <= 0)
		{
			NetworkMapSharer.share.RpcSpawnCarryWorldObject(GetComponent<PickUpAndCarry>().prefabId, base.transform.position, base.transform.rotation);
		}
		if (TileObjectHealthBar.tile.currentlyHitting == this)
		{
			TileObjectHealthBar.tile.currentlyHitting = null;
		}
	}

	public void onAnimalDeath()
	{
		myAnimalAi.onDeath();
		if (instantDie)
		{
			StartCoroutine(disapearAfterDeathAnimation(0f));
		}
		else
		{
			StartCoroutine(disapearAfterDeathAnimation(2.5f));
		}
		if ((bool)myAnimalAi && (bool)myAnimalAi.saveAsAlpha)
		{
			NetworkMapSharer.share.RpcCheckHuntingTaskCompletion(myAnimalAi.animalId, base.transform.position);
		}
	}

	public IEnumerator disapearAfterDeathAnimation(float waitTimeBefore)
	{
		yield return new WaitForSeconds(waitTimeBefore);
		SoundManager.manage.playASoundAtPoint(SoundManager.manage.animalDiesSound, base.transform.position);
		if (!base.isServer)
		{
			yield break;
		}
		yield return new WaitForSeconds(0.15f);
		Transform[] array = dropPositions;
		foreach (Transform transform in array)
		{
			InventoryItem randomDropFromTable = lootDrops.getRandomDropFromTable();
			if ((bool)randomDropFromTable)
			{
				int xPType = -1;
				if ((bool)isAnAnimal())
				{
					xPType = 5;
				}
				NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(randomDropFromTable), 1, transform.position, null, true, xPType);
			}
		}
		NetworkNavMesh.nav.UnSpawnAnAnimal(myAnimalAi, false);
	}

	public override void OnStartServer()
	{
		canBeStunned = true;
		Networkhealth = maxHealth;
		canBeDamaged = true;
		NetworkonFire = false;
		Networkpoisoned = false;
		Networkstunned = false;
		isVehicle = GetComponent<Vehicle>();
	}

	[Command]
	public void CmdChangeHealth(int newHealth)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newHealth);
		SendCommandInternal(typeof(Damageable), "CmdChangeHealth", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeHealthTo(int newHealth)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newHealth);
		SendCommandInternal(typeof(Damageable), "CmdChangeHealthTo", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeMaxHealth(int newMaxHealth)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newMaxHealth);
		SendCommandInternal(typeof(Damageable), "CmdChangeMaxHealth", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdStopStatusEffects()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(Damageable), "CmdStopStatusEffects", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void poison()
	{
		Networkpoisoned = true;
	}

	public void setOnFire()
	{
		NetworkonFire = true;
	}

	public void onFireChange(bool old, bool newOnFire)
	{
		NetworkonFire = newOnFire;
		if (onFire)
		{
			StopCoroutine("onFireEffect");
			StartCoroutine("onFireEffect");
		}
	}

	private IEnumerator onFireEffect()
	{
		float fireTimer = 0f;
		float damageTimer = 0f;
		doDamageFromStatus(1);
		while (onFire)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.fireStatusParticle, base.transform.position, Random.Range(1, 2));
			ParticleManager.manage.fireStatusGlowParticles.Emit(3);
			if (base.isServer)
			{
				if (WorldManager.manageWorld.waterMap[Mathf.RoundToInt(base.transform.position.x) / 2, Mathf.RoundToInt(base.transform.position.z) / 2] && (double)base.transform.position.y <= 0.15)
				{
					NetworkonFire = false;
				}
				else if (damageTimer > 0.5f)
				{
					damageTimer = 0f;
					doDamageFromStatus(1);
				}
				else
				{
					damageTimer += Time.deltaTime;
				}
				if (WeatherManager.manage.raining && !RealWorldTimeLight.time.underGround)
				{
					if (fireTimer < 1.25f)
					{
						fireTimer += Time.deltaTime;
					}
					else
					{
						NetworkonFire = false;
					}
				}
				else if (fireTimer < 10f)
				{
					fireTimer += Time.deltaTime;
				}
				else
				{
					NetworkonFire = false;
				}
			}
			yield return null;
		}
	}

	public void onPoisonChange(bool old, bool newPoision)
	{
		Networkpoisoned = newPoision;
		if (poisoned)
		{
			StopCoroutine("poisionEffect");
			StartCoroutine("poisionEffect");
		}
	}

	public void onStunned(bool old, bool newStunned)
	{
		Networkstunned = newStunned;
		GetComponent<Animator>().SetBool("Stunned", newStunned);
		if (newStunned)
		{
			ParticleManager.manage.spawnStunnedParticle(this);
		}
		if (newStunned && base.isServer)
		{
			StartCoroutine(stunnedRoutine());
		}
	}

	public IEnumerator stunTimer()
	{
		canBeStunned = false;
		while (stunned)
		{
			yield return null;
		}
		yield return new WaitForSeconds(Random.Range(35f, 60f));
		canBeStunned = true;
	}

	public void stun()
	{
		if ((bool)myAnimalAi && health > 0 && !stunned)
		{
			Networkstunned = true;
		}
	}

	public void stunWithLight()
	{
		if ((bool)myAnimalAi && health > 0 && !stunned && canBeStunned)
		{
			RpcPlayStunnedByLight();
			StartCoroutine(stunTimer());
			Networkstunned = true;
		}
	}

	public void unStun()
	{
		Networkstunned = false;
	}

	public bool isStunned()
	{
		return stunned;
	}

	private IEnumerator stunnedRoutine()
	{
		float timer = 0f;
		if ((bool)myAnimalAi)
		{
			myAnimalAi.myAgent.isStopped = true;
		}
		while (stunned && health > 0 && timer < 5f)
		{
			timer += Time.deltaTime;
			yield return null;
		}
		if ((bool)myAnimalAi)
		{
			myAnimalAi.myAgent.isStopped = false;
		}
		Networkstunned = false;
	}

	[ClientRpc]
	public void RpcPlayStunnedByLight()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(Damageable), "RpcPlayStunnedByLight", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator poisionEffect()
	{
		float poisionTimer = 0f;
		float damageTimer = 0f;
		if (base.isLocalPlayer)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.GetPoisoned);
		}
		while (poisoned)
		{
			if (Random.Range(0, 2) == 1)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.poisonStatusParticle, base.transform.position, Random.Range(1, 2));
			}
			if (base.isServer)
			{
				if (poisionTimer < 8f)
				{
					poisionTimer += Time.deltaTime;
				}
				else
				{
					Networkpoisoned = false;
				}
			}
			if (damageTimer > 1f)
			{
				damageTimer = 0f;
				if (base.isLocalPlayer && StatusManager.manage.staminaAboveNo(5f))
				{
					StatusManager.manage.changeStamina(-0.35f);
				}
			}
			else
			{
				damageTimer += Time.deltaTime;
			}
			yield return null;
		}
	}

	public void startRegenAndSetTimer(float newTimer, int level)
	{
		if (regenRoutine != null)
		{
			StopCoroutine(regenRoutine);
		}
		regenRoutine = StartCoroutine(startHealthRegen(newTimer, level));
	}

	private IEnumerator startHealthRegen(float seconds, int level)
	{
		while (seconds > 0f)
		{
			yield return regenWait;
			if (health <= 0)
			{
				regenRoutine = null;
				yield break;
			}
			Networkhealth = Mathf.Clamp(health + level, 1, maxHealth);
			seconds -= 2f;
		}
		regenRoutine = null;
	}

	static Damageable()
	{
		delayWait = new WaitForSeconds(0.45f);
		animalWait = new WaitForSeconds(0.25f);
		regenWait = new WaitForSeconds(2f);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Damageable), "CmdChangeHealth", InvokeUserCode_CmdChangeHealth, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Damageable), "CmdChangeHealthTo", InvokeUserCode_CmdChangeHealthTo, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Damageable), "CmdChangeMaxHealth", InvokeUserCode_CmdChangeMaxHealth, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(Damageable), "CmdStopStatusEffects", InvokeUserCode_CmdStopStatusEffects, true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(Damageable), "RpcPlayStunnedByLight", InvokeUserCode_RpcPlayStunnedByLight);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdChangeHealth(int newHealth)
	{
		changeHealth(newHealth);
	}

	protected static void InvokeUserCode_CmdChangeHealth(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeHealth called on client.");
		}
		else
		{
			((Damageable)obj).UserCode_CmdChangeHealth(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeHealthTo(int newHealth)
	{
		MonoBehaviour.print("command called on server");
		Networkhealth = Mathf.Clamp(newHealth, 0, maxHealth);
	}

	protected static void InvokeUserCode_CmdChangeHealthTo(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeHealthTo called on client.");
		}
		else
		{
			((Damageable)obj).UserCode_CmdChangeHealthTo(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeMaxHealth(int newMaxHealth)
	{
		maxHealth = newMaxHealth;
	}

	protected static void InvokeUserCode_CmdChangeMaxHealth(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeMaxHealth called on client.");
		}
		else
		{
			((Damageable)obj).UserCode_CmdChangeMaxHealth(reader.ReadInt());
		}
	}

	protected void UserCode_CmdStopStatusEffects()
	{
		NetworkonFire = false;
		Networkpoisoned = false;
	}

	protected static void InvokeUserCode_CmdStopStatusEffects(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStopStatusEffects called on client.");
		}
		else
		{
			((Damageable)obj).UserCode_CmdStopStatusEffects();
		}
	}

	protected void UserCode_RpcPlayStunnedByLight()
	{
		SoundManager.manage.playASoundAtPoint(SoundManager.manage.stunnedByLightSound, base.transform.position);
	}

	protected static void InvokeUserCode_RpcPlayStunnedByLight(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayStunnedByLight called on server.");
		}
		else
		{
			((Damageable)obj).UserCode_RpcPlayStunnedByLight();
		}
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(health);
			writer.WriteBool(onFire);
			writer.WriteBool(poisoned);
			writer.WriteBool(stunned);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(health);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(onFire);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(poisoned);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteBool(stunned);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = health;
			Networkhealth = reader.ReadInt();
			if (!SyncVarEqual(num, ref health))
			{
				OnTakeDamage(num, health);
			}
			bool flag = onFire;
			NetworkonFire = reader.ReadBool();
			if (!SyncVarEqual(flag, ref onFire))
			{
				onFireChange(flag, onFire);
			}
			bool flag2 = poisoned;
			Networkpoisoned = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref poisoned))
			{
				onPoisonChange(flag2, poisoned);
			}
			bool flag3 = stunned;
			Networkstunned = reader.ReadBool();
			if (!SyncVarEqual(flag3, ref stunned))
			{
				onStunned(flag3, stunned);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			int num3 = health;
			Networkhealth = reader.ReadInt();
			if (!SyncVarEqual(num3, ref health))
			{
				OnTakeDamage(num3, health);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			bool flag4 = onFire;
			NetworkonFire = reader.ReadBool();
			if (!SyncVarEqual(flag4, ref onFire))
			{
				onFireChange(flag4, onFire);
			}
		}
		if ((num2 & 4L) != 0L)
		{
			bool flag5 = poisoned;
			Networkpoisoned = reader.ReadBool();
			if (!SyncVarEqual(flag5, ref poisoned))
			{
				onPoisonChange(flag5, poisoned);
			}
		}
		if ((num2 & 8L) != 0L)
		{
			bool flag6 = stunned;
			Networkstunned = reader.ReadBool();
			if (!SyncVarEqual(flag6, ref stunned))
			{
				onStunned(flag6, stunned);
			}
		}
	}
}
