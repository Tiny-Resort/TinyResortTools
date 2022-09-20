using System.Collections;
using UnityEngine;

public class MeleeAttacks : MonoBehaviour
{
	private CharMovement myCharMovement;

	private NPCHoldsItems myNPC;

	private CharInteract myInteract;

	public Transform toolPos;

	public Transform[] otherToolPos;

	public ASound swordSwing;

	public ItemHitBox myHitBox;

	public Animator myAnim;

	[Header("Swing Particles---------")]
	public ParticleSystem swingParts;

	public int showSwingPartsForFrames;

	public bool playSwingSoundWithPart;

	private float multiplier = 1f;

	[Header("Tool options---------------")]
	public bool isWeapon = true;

	public bool useEnergyOnSwing;

	public bool consumeFuelOnSwing;

	public bool hitBoxMovesToHoldPos;

	[Header("Attack Info ---------------")]
	public int attackFramesLength = 3;

	public float forwardSpeed;

	public float forwardTime;

	[Header("Stand Still ---------------")]
	public int framesBeforeAttackLocked;

	public int framesAfterAttackLocked;

	public bool lockedDuringAttack;

	private bool attackPlaying;

	public bool faceTargetsWhenAttacking;

	[Header("Other---------------")]
	public bool slowWalkWhileUsing;

	public bool cantClimbWhileUsing;

	private bool lockingPos;

	private bool hasCaughtBug;

	private void Start()
	{
		myInteract = GetComponentInParent<CharInteract>();
		myCharMovement = GetComponentInParent<CharMovement>();
		if (slowWalkWhileUsing && (bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			StartCoroutine(slowWalkOnUse());
		}
		if (cantClimbWhileUsing && (bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			StartCoroutine(slowWalkNoClimeOnUse());
		}
		if (hitBoxMovesToHoldPos && (bool)myCharMovement)
		{
			myHitBox.transform.parent = myCharMovement.myEquip.rightHandToolHitPos;
			myHitBox.transform.localPosition = Vector3.zero;
			myHitBox.transform.localRotation = Quaternion.identity;
		}
	}

	public void attack()
	{
		if (((bool)myCharMovement && myCharMovement.isLocalPlayer) || ((bool)myNPC && myNPC.isServer))
		{
			if (!attackPlaying)
			{
				attackPlaying = true;
				StartCoroutine(genericAttack());
			}
		}
		else
		{
			StartCoroutine(nonLocalSwingSound());
		}
	}

	private IEnumerator nonLocalSwingSound()
	{
		if (framesBeforeAttackLocked != 0 || lockedDuringAttack)
		{
			if (framesBeforeAttackLocked != 0 && lockedDuringAttack)
			{
				yield return new WaitForSeconds((float)framesBeforeAttackLocked / 60f);
			}
			else if (framesBeforeAttackLocked != 0)
			{
				yield return new WaitForSeconds((float)framesBeforeAttackLocked / 60f);
			}
		}
		playSwordSwingSound();
	}

	private IEnumerator genericAttack()
	{
		if (faceTargetsWhenAttacking && (bool)myCharMovement)
		{
			myCharMovement.faceClosestTarget();
		}
		if ((bool)myCharMovement && (framesBeforeAttackLocked != 0 || lockedDuringAttack))
		{
			if (framesBeforeAttackLocked != 0 && lockedDuringAttack)
			{
				StartCoroutine(myCharMovement.charLockedStill((float)(framesBeforeAttackLocked + attackFramesLength + framesAfterAttackLocked) / 60f));
				yield return new WaitForSeconds((float)framesBeforeAttackLocked / 60f);
			}
			else if (framesBeforeAttackLocked != 0)
			{
				yield return StartCoroutine(myCharMovement.charLockedStill((float)framesBeforeAttackLocked / 60f));
			}
			else if (lockedDuringAttack)
			{
				StartCoroutine(myCharMovement.charLockedStill((float)(attackFramesLength + framesAfterAttackLocked) / 60f));
			}
		}
		if (!playSwingSoundWithPart)
		{
			playSwordSwingSound();
		}
		myHitBox.startAttack();
		if ((bool)myNPC)
		{
			StartCoroutine(endAttackDelay());
			yield break;
		}
		if (consumeFuelOnSwing)
		{
			Inventory.inv.useItemWithFuel();
		}
		if (useEnergyOnSwing)
		{
			StatusManager.manage.changeStamina(0f - Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemInSlot.getStaminaCost());
		}
		StartCoroutine(endAttackDelay());
		if (forwardSpeed > 0f)
		{
			StartCoroutine(myCharMovement.charAttacksForward(forwardSpeed, forwardTime));
		}
	}

	private IEnumerator endAttackDelay()
	{
		yield return new WaitForSeconds((float)(attackFramesLength + framesBeforeAttackLocked) / 60f);
		myHitBox.endAttack();
		if ((bool)myCharMovement && framesAfterAttackLocked != 0 && !lockedDuringAttack)
		{
			yield return StartCoroutine(myCharMovement.charLockedStill((float)framesAfterAttackLocked / 60f));
		}
		attackPlaying = false;
	}

	private IEnumerator slowWalkOnUse()
	{
		while (true)
		{
			if (myCharMovement.localUsing)
			{
				myCharMovement.isSneaking(true);
			}
			else
			{
				myCharMovement.isSneaking(false);
			}
			yield return null;
		}
	}

	private IEnumerator slowWalkNoClimeOnUse()
	{
		while (true)
		{
			if (myCharMovement.localUsing)
			{
				myCharMovement.isSneaking(true);
				myCharMovement.canClimb = false;
			}
			else
			{
				myCharMovement.isSneaking(false);
				myCharMovement.canClimb = true;
			}
			yield return null;
		}
	}

	public void lockPosForFrames(int frames)
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer && !lockingPos)
		{
			StartCoroutine(lockPosOnly(frames));
		}
	}

	private IEnumerator lockPosOnly(int frames)
	{
		lockingPos = true;
		yield return StartCoroutine(myCharMovement.charLockedStill((float)frames / 60f));
		lockingPos = false;
	}

	public void turnOnLookLockForFrames(int framesToLock)
	{
		if ((bool)myCharMovement && myCharMovement.myEquip.usingItem)
		{
			StartCoroutine(lookLockFrames(framesToLock));
		}
	}

	public void turnOnLookLockForFramesWithoutUsing(int framesToLock)
	{
		if ((bool)myCharMovement)
		{
			StartCoroutine(lookLockFrames(framesToLock));
		}
	}

	private IEnumerator lookLockFrames(int frames)
	{
		myCharMovement.myEquip.setLookLock(true);
		yield return new WaitForSeconds((float)frames / 60f);
		myCharMovement.myEquip.setLookLock(false);
	}

	public void lockPlayerInPlace()
	{
		StartCoroutine(myCharMovement.charAttacksForward());
	}

	private void OnDisable()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.unlockAll();
			endCharSneak();
			myCharMovement.canClimb = true;
		}
		if ((bool)myCharMovement)
		{
			myCharMovement.myEquip.setLookLock(false);
		}
		if ((hitBoxMovesToHoldPos && (bool)myCharMovement) || (hitBoxMovesToHoldPos && (bool)myNPC))
		{
			Object.Destroy(myHitBox.gameObject);
		}
	}

	public void charAttackForward(float newMultiplier = 1f)
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			multiplier = newMultiplier;
			myHitBox.startAttack();
			StartCoroutine(myCharMovement.charAttacksForward());
		}
		if ((bool)swordSwing)
		{
			SoundManager.manage.playASoundAtPoint(swordSwing, base.transform.position);
		}
	}

	public void faceTarget()
	{
		if ((bool)myCharMovement)
		{
			myCharMovement.faceClosestTarget();
		}
	}

	public void lockPositionAllowRotation()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.attackLockOn(true);
			myCharMovement.moveLockRotateSlowOn(true);
		}
	}

	public void lockPositionAndRotation()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.attackLockOn(true);
		}
	}

	public void fireProjectile(int projectileNo = 0)
	{
	}

	public void rotationOnlyAttack_Start()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.lockRotation(true);
			myCharMovement.isSneaking(true);
		}
	}

	public void rotationOnlyAttack_End()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.lockRotation(false);
			myCharMovement.isSneaking(false);
		}
	}

	public void startAttackNoLock()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			if (useEnergyOnSwing)
			{
				StatusManager.manage.changeStamina(0f - Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemInSlot.getStaminaCost());
			}
			if (consumeFuelOnSwing)
			{
				Inventory.inv.useItemWithFuel();
			}
			if ((bool)myHitBox)
			{
				myHitBox.startAttack();
			}
		}
		if ((bool)myNPC && myNPC.isServer && (bool)myHitBox)
		{
			myHitBox.startAttack();
		}
		if ((bool)swordSwing)
		{
			SoundManager.manage.playASoundAtPoint(swordSwing, base.transform.position);
		}
	}

	public void startAttackSlowWalk()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			if (useEnergyOnSwing)
			{
				StatusManager.manage.changeStamina(0f - Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemInSlot.getStaminaCost());
			}
			if (consumeFuelOnSwing)
			{
				Inventory.inv.useItemWithFuel();
			}
			if ((bool)myHitBox)
			{
				myHitBox.startAttack();
			}
			myCharMovement.isSneaking(true);
		}
		if ((bool)myNPC && myNPC.isServer && (bool)myHitBox)
		{
			myHitBox.startAttack();
		}
		if ((bool)swordSwing)
		{
			SoundManager.manage.playASoundAtPoint(swordSwing, base.transform.position);
		}
	}

	public void endAttackSlowWalk()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			if ((bool)myHitBox)
			{
				myHitBox.endAttack();
			}
			myCharMovement.isSneaking(false);
		}
		if ((bool)myNPC && myNPC.isServer && (bool)myHitBox)
		{
			myHitBox.endAttack();
		}
	}

	public void endAttackNoLock()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer && (bool)myHitBox)
		{
			myHitBox.endAttack();
		}
		if ((bool)myNPC && myNPC.isServer && (bool)myHitBox)
		{
			myHitBox.endAttack();
		}
	}

	public void slowWalkNoClimb()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.canClimb = false;
			myCharMovement.isSneaking(true);
		}
	}

	public void stopSlowWalkNoClimb()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.canClimb = true;
			myCharMovement.isSneaking(false);
		}
	}

	public void startCharSneak()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.isSneaking(true);
		}
	}

	public void endCharSneak()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myCharMovement.isSneaking(false);
		}
	}

	public void charStartAttackStill()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			if (useEnergyOnSwing)
			{
				StatusManager.manage.changeStamina(0f - Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemInSlot.getStaminaCost());
			}
			if (consumeFuelOnSwing)
			{
				Inventory.inv.useItemWithFuel();
			}
			myCharMovement.attackLockOn(true);
			myCharMovement.moveLockRotateSlowOn(false);
			if ((bool)myHitBox)
			{
				myHitBox.startAttack();
			}
		}
		if ((bool)myNPC && myNPC.isServer && (bool)myHitBox)
		{
			myHitBox.startAttack();
		}
		if ((bool)swordSwing)
		{
			SoundManager.manage.playASoundAtPoint(swordSwing, base.transform.position);
		}
	}

	public void endAttackStill()
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			if ((bool)myHitBox)
			{
				myHitBox.endAttack();
			}
			myCharMovement.attackLockOn(false);
			myCharMovement.moveLockRotateSlowOn(false);
		}
		if ((bool)myNPC && myNPC.isServer && (bool)myHitBox)
		{
			myHitBox.endAttack();
		}
	}

	private IEnumerator pauseAttack()
	{
		if ((bool)myCharMovement && myCharMovement.myEquip.currentlyHoldingNo > -1)
		{
			myAnim.SetFloat("speed", 0f);
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			myAnim.SetFloat("speed", 1f);
		}
		else
		{
			myAnim.SetFloat("speed", 0f);
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			myAnim.SetFloat("speed", 1f);
		}
	}

	public void npcRelease()
	{
		if ((bool)myNPC)
		{
			myNPC.NetworkusingItem = false;
		}
	}

	public void toolDoesDamageToolPos()
	{
		if ((bool)toolPos && (bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myInteract.doDamageToolPos(toolPos.position);
		}
	}

	public void toolDoesDamageToolPosNo(int noToUse)
	{
		if (otherToolPos.Length > noToUse && otherToolPos[noToUse] != null && (bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myInteract.doDamageToolPos(otherToolPos[noToUse].position);
		}
	}

	public void toolDoesDamageToolPosDif()
	{
		if ((bool)toolPos && (bool)myCharMovement && myCharMovement.isLocalPlayer)
		{
			myInteract.doDamageWithDif(myCharMovement.transform.forward * 2f);
		}
	}

	public void finishAttack()
	{
		if ((bool)myCharMovement)
		{
			bool isLocalPlayer = myCharMovement.isLocalPlayer;
		}
	}

	public void attackAndDealDamage(Damageable damageable)
	{
		if ((bool)myCharMovement && myCharMovement.isLocalPlayer && damageable.health > 0)
		{
			if (myHitBox.attackDamageAmount > 0)
			{
				CameraController.control.shakeScreenMax(0.35f, 0.2f);
			}
			TileObjectHealthBar.tile.setCurrentlyHitting(damageable);
			InputMaster.input.doRumble(0.25f);
			myCharMovement.CmdDealDamage(damageable.netId, 1f + (float)StatusManager.manage.getBuffLevel(StatusManager.BuffType.huntingBuff) / 2f);
			Inventory.inv.useItemWithFuel();
		}
		if ((bool)myNPC)
		{
			damageable.attackAndDoDamage(Mathf.RoundToInt(myNPC.currentlyHolding.weaponDamage), base.transform.root, myNPC.currentlyHolding.weaponKnockback);
		}
	}

	public void catchBug(BugTypes catchThisBug)
	{
		if (myCharMovement.isLocalPlayer && !hasCaughtBug)
		{
			int invItemId = Inventory.inv.getInvItemId(catchThisBug.bugInvItem());
			SoundManager.manage.play2DSound(SoundManager.manage.pickUpItem);
			ParticleManager.manage.emitAttackParticle(myHitBox.transform.position, 5);
			hasCaughtBug = true;
			myCharMovement.CmdCatchBug(catchThisBug.netId);
			Inventory.inv.useItemWithFuel();
			myCharMovement.myEquip.catchAndShowBug(invItemId);
			if (!NetworkMapSharer.share.isServer)
			{
				catchThisBug.gameObject.SetActive(false);
			}
		}
	}

	public void playSwingParticles()
	{
		swingParts.Emit(8);
	}

	public void playSwingPartsForFrames()
	{
		if (playSwingSoundWithPart)
		{
			playSwordSwingSound();
		}
		StartCoroutine(swingPartsForFrames(showSwingPartsForFrames));
	}

	private IEnumerator swingPartsForFrames(int frames)
	{
		while (frames > 0)
		{
			swingParts.Emit(10);
			yield return null;
			frames--;
		}
	}

	public void lookLockOn()
	{
		if ((bool)myCharMovement)
		{
			myCharMovement.myEquip.setLookLock(true);
		}
	}

	public void lookLockOff()
	{
		if ((bool)myCharMovement)
		{
			myCharMovement.myEquip.setLookLock(false);
		}
	}

	public void playGroundParticles()
	{
		ParticleManager.manage.emitGroundAttackParticles(toolPos.position);
	}

	public void playSwordSwingSound()
	{
		if ((bool)swordSwing)
		{
			SoundManager.manage.playASoundAtPoint(swordSwing, base.transform.position);
		}
	}

	public void attachNPC(NPCHoldsItems NPC)
	{
		myNPC = NPC;
		if (hitBoxMovesToHoldPos)
		{
			myHitBox.transform.parent = myNPC.rightHandMoveHitboxTo;
			myHitBox.transform.localPosition = Vector3.zero;
			myHitBox.transform.localRotation = Quaternion.identity;
		}
	}
}
