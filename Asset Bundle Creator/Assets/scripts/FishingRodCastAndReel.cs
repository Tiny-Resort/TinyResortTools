using System.Collections;
using UnityEngine;

public class FishingRodCastAndReel : MonoBehaviour
{
	[Header("Rod Settings")]
	public float reelSpeed = 0.333f;

	public int maxLineHealth = 3;

	[Header("Other Settings")]
	public NetworkFishingRod networkVersion;

	public GameObject fishDummy;

	private GameObject myFishDummy;

	private Animator myFishDummyAnim;

	public Transform RodEnd;

	public LineRenderer line;

	public Transform bobber;

	public Transform attachToBobber;

	public Transform bobberDummy;

	public Animator bobberAnim;

	public LayerMask castMask;

	public MeshRenderer dummyRen;

	public bool reeling;

	private bool holdingDummy;

	private float castDistance = 5f;

	public bool fishPulling;

	public AudioSource reelSounds;

	private bool castComplete;

	private Transform leftHandPos;

	private CharMovement myChar;

	private bool bobberInWater;

	[Header("Cosmetic line---")]
	public LineRenderer lineOnRod;

	public Transform[] lineOnRodPoints;

	public AudioSource chargeSounds;

	public void Start()
	{
		networkVersion = GetComponentInParent<NetworkFishingRod>();
		if ((bool)networkVersion)
		{
			networkVersion.startControls();
			networkVersion.connectedFishingRod = this;
			if ((bool)networkVersion && (bool)networkVersion.transform.GetComponent<CharMovement>())
			{
				myChar = networkVersion.transform.GetComponent<CharMovement>();
			}
			leftHandPos = base.transform.Find("FishPos");
			bobberDummy.parent = null;
			bobberDummy.localScale = new Vector3(0.5234465f, 0.5234465f, 0.5234465f);
		}
		else
		{
			Object.Destroy(this);
		}
	}

	public void setCosmeticRodPoints()
	{
		for (int i = 0; i < lineOnRodPoints.Length; i++)
		{
			lineOnRod.SetPosition(i, lineOnRodPoints[i].position);
		}
	}

	public void cancelButton()
	{
		if (castComplete)
		{
			castComplete = false;
			networkVersion.lineIsCasted = false;
			networkVersion.CmdCancleCast();
			Inventory.inv.useItemWithFuel();
		}
	}

	public void breakFishOff()
	{
		if (castComplete)
		{
			castComplete = false;
			networkVersion.lineIsCasted = false;
			networkVersion.CmdCancleCast();
			Inventory.inv.useItemWithFuel();
			InputMaster.input.doRumble(0.8f);
		}
	}

	private void Update()
	{
		setCosmeticRodPoints();
		line.SetPosition(0, RodEnd.position);
		if (dummyRen.enabled)
		{
			line.SetPosition(1, bobberDummy.position);
		}
		else
		{
			line.SetPosition(1, attachToBobber.position);
		}
		if ((bool)networkVersion && (bool)myChar && castComplete)
		{
			if (myChar.myEquip.usingItem || myChar.localUsing || networkVersion.pulling)
			{
				if (networkVersion.reelPosition == Vector3.zero)
				{
					stopReelSounds();
				}
				else
				{
					playReelSound(1f);
				}
			}
			else
			{
				stopReelSounds();
			}
		}
		if ((bool)networkVersion && networkVersion.isLocalPlayer)
		{
			reelSounds.transform.position = CameraController.control.transform.position;
		}
		if (!castComplete)
		{
			return;
		}
		bobber.position = Vector3.Lerp(bobber.position, networkVersion.reelPosition, Time.deltaTime * 2f);
		if ((bool)networkVersion)
		{
			fishPulling = networkVersion.pulling;
			myChar.myAnim.SetBool("FishPulling", fishPulling);
		}
		if (networkVersion.isLocalPlayer && Vector3.Distance(networkVersion.reelPosition, base.transform.position) > 40f)
		{
			castComplete = false;
			networkVersion.lineIsCasted = false;
			networkVersion.CmdCancleCast();
			Inventory.inv.useItemWithFuel();
		}
		if (fishPulling || (networkVersion.fishOnLine != -1 && !fishPulling && !myChar.myEquip.usingItem))
		{
			if (fishPulling && Random.Range(0, 5) == 2)
			{
				SoundManager.manage.playASoundAtPoint(SoundManager.manage.waterSplash, bobber.position, 0.35f);
				ParticleManager.manage.waterSplash(bobber.position, 1);
			}
			if ((bool)myFishDummy)
			{
				dummyStruggle();
				Vector3 normalized = (networkVersion.reelPosition - networkVersion.transform.position).normalized;
				myFishDummy.transform.localRotation = Quaternion.Lerp(myFishDummy.transform.localRotation, Quaternion.LookRotation(normalized), Time.deltaTime * 2f);
			}
		}
		else if ((bool)myFishDummy)
		{
			stopDummyStruggle();
			if (!holdingDummy)
			{
				Vector3 normalized2 = (networkVersion.transform.position - bobber.position).normalized;
				myFishDummy.transform.localRotation = Quaternion.Lerp(myFishDummy.transform.localRotation, Quaternion.LookRotation(normalized2), Time.deltaTime * 2f);
			}
		}
		RaycastHit hitInfo;
		if (!bobber.gameObject.activeSelf || !Physics.Raycast(bobber.transform.position + Vector3.up * 3f, Vector3.down, out hitInfo, 25f, castMask))
		{
			return;
		}
		if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Water"))
		{
			if (!bobberInWater)
			{
				bobberAnim.SetTrigger("Landed");
				bobberInWater = true;
			}
		}
		else if (bobberInWater)
		{
			bobberAnim.SetTrigger("LandedDirt");
			bobberInWater = false;
		}
	}

	public void castLine()
	{
		RaycastHit hitInfo;
		if (myChar.isLocalPlayer && Physics.Raycast(networkVersion.transform.position + networkVersion.transform.forward * castDistance + Vector3.up * 3f, Vector3.down, out hitInfo, 25f, castMask))
		{
			networkVersion.NetworkreelPosition = hitInfo.point;
			networkVersion.CmdCastLine(hitInfo.point);
			Inventory.inv.useItemWithFuel();
			StatusManager.manage.changeStamina(0f - Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemInSlot.getStaminaCost());
		}
	}

	public void reelIn()
	{
		reeling = true;
		if (myChar.isLocalPlayer && castComplete && !fishPulling)
		{
			getNewReelPos();
			Vector3 position = bobber.position;
			position.y = 0.6f;
			Vector3 position2 = networkVersion.transform.position;
			position2.y = 0.6f;
			if (Vector3.Distance(position, position2) < 4f)
			{
				networkVersion.lineIsCasted = false;
				castComplete = false;
				networkVersion.CmdCompleteReel();
			}
		}
	}

	private void getNewReelPos()
	{
		Vector3 normalized = (networkVersion.transform.position - bobber.position).normalized;
		float num = 1.5f * (1f + (float)StatusManager.manage.getBuffLevel(StatusManager.BuffType.fishingBuff) / 2f);
		RaycastHit hitInfo;
		if (Physics.Raycast(bobber.position + normalized * num + Vector3.up * 3f, Vector3.down, out hitInfo, 25f, castMask))
		{
			networkVersion.CmdChangeReelPos(hitInfo.point);
		}
	}

	private void OnDisable()
	{
		if ((bool)networkVersion && networkVersion.isLocalPlayer)
		{
			networkVersion.stopControls();
			if (castComplete)
			{
				Inventory.inv.useItemWithFuel();
			}
			completeReel(true);
			networkVersion.CmdCancleCast();
			CameraController.control.stopFishing();
			networkVersion.lineIsCasted = false;
		}
		if ((bool)networkVersion && networkVersion.isLocalPlayer)
		{
			MenuButtonsTop.menu.closeButtonDelay();
		}
		if ((bool)myFishDummy)
		{
			Object.Destroy(myFishDummy);
		}
		bobber.parent = RodEnd;
		bobberDummy.parent = RodEnd;
	}

	public void spawnFishDummy(int newFishId, Quaternion rotation)
	{
		switch (newFishId)
		{
		case -2:
			if (!myFishDummy)
			{
				myFishDummy = Object.Instantiate(networkVersion.sharkDummy, bobber.position, rotation);
			}
			myFishDummyAnim = myFishDummy.GetComponent<Animator>();
			myFishDummy.transform.parent = bobber.transform;
			return;
		case -1:
			return;
		}
		if (!myFishDummy)
		{
			myFishDummy = Object.Instantiate(fishDummy, bobber.position, rotation);
		}
		myFishDummy.GetComponent<FishType>().setFishType(newFishId, newFishId);
		myFishDummyAnim = myFishDummy.GetComponent<Animator>();
		myFishDummy.transform.parent = bobber.transform;
	}

	public void removeDummyFish()
	{
		if ((bool)myFishDummy)
		{
			Object.Destroy(myFishDummy);
		}
		myFishDummy = null;
		myFishDummyAnim = null;
	}

	public void stopDummyStruggle()
	{
		if ((bool)myFishDummyAnim)
		{
			myFishDummyAnim.SetBool("Struggle", false);
			myFishDummyAnim.SetBool("Walking", true);
		}
	}

	public void dummyStruggle()
	{
		if ((bool)myFishDummyAnim)
		{
			myFishDummyAnim.SetBool("Struggle", true);
		}
	}

	public void putAwayFish()
	{
		holdingDummy = false;
		myFishDummyAnim = null;
		Object.Destroy(myFishDummy);
		if (networkVersion.isLocalPlayer)
		{
			myChar.myEquip.catchAndShowFish(myFishDummy.GetComponent<FishType>().getFishInvId());
		}
	}

	private IEnumerator fishFlysIntoHands()
	{
		holdingDummy = true;
		bool reachedTarget2 = false;
		myFishDummy.transform.parent = null;
		bool isFish = myFishDummy.GetComponent<FishType>();
		if (isFish && (bool)networkVersion && networkVersion.isLocalPlayer)
		{
			MenuButtonsTop.menu.closed = false;
		}
		while (!reachedTarget2)
		{
			Vector3 position = bobber.position;
			myFishDummy.transform.rotation = Quaternion.Lerp(myFishDummy.transform.rotation, myChar.myEquip.holdPos.rotation * Quaternion.Euler(0f, 90f, 0f), Time.deltaTime * 10f);
			myFishDummy.transform.position = Vector3.Lerp(myFishDummy.transform.position, myChar.myEquip.holdPos.position + Vector3.up * 3f + myChar.transform.forward / 2f, Time.deltaTime * 10f);
			if (isFish)
			{
				base.transform.localScale = Vector3.Lerp(base.transform.localScale, Vector3.zero, Time.deltaTime * 3f);
			}
			if (Vector3.Distance(myChar.myEquip.holdPos.position + Vector3.up * 3f + myChar.transform.forward / 2f, myFishDummy.transform.position) <= 0.25f)
			{
				reachedTarget2 = true;
			}
			yield return null;
		}
		reachedTarget2 = false;
		while (!reachedTarget2)
		{
			myFishDummy.transform.rotation = Quaternion.Lerp(myFishDummy.transform.rotation, myChar.myEquip.holdPos.rotation * Quaternion.Euler(0f, 90f, 0f), Time.deltaTime * 10f);
			myFishDummy.transform.position = Vector3.Lerp(myFishDummy.transform.position, myChar.myEquip.holdPos.position, Time.deltaTime * 25f);
			if (isFish)
			{
				base.transform.localScale = Vector3.Lerp(base.transform.localScale, Vector3.zero, Time.deltaTime * 3f);
			}
			if (Vector3.Distance(myChar.myEquip.holdPos.position, myFishDummy.transform.position) <= 0.05f)
			{
				reachedTarget2 = true;
			}
			yield return null;
		}
		if (isFish)
		{
			putAwayFish();
			yield break;
		}
		networkVersion.spawnFakeShark(myFishDummy.transform.rotation.y);
		holdingDummy = false;
		myFishDummyAnim = null;
		Object.Destroy(myFishDummy);
	}

	public void completeReel(bool beingDisabled = false)
	{
		if (!myChar)
		{
			return;
		}
		if (!beingDisabled)
		{
			StartCoroutine(playCompleteReelSound());
		}
		reeling = false;
		if (!beingDisabled)
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(bobber.position + Vector3.up * 3f, Vector3.down, out hitInfo, 25f, castMask) && hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Water"))
			{
				SoundManager.manage.playASoundAtPoint(SoundManager.manage.waterSplash, bobber.position, 0.8f);
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.waterParticle, bobber.position, 30);
			}
			bobberAnim.enabled = true;
		}
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			CameraController.control.setFollowTransform(networkVersion.transform);
		}
		myChar.myAnim.SetTrigger("ReelCompleted");
		if ((bool)myFishDummy)
		{
			StartCoroutine(fishFlysIntoHands());
		}
		bobber.parent = RodEnd;
		bobber.gameObject.SetActive(false);
		if ((bool)myFishDummy)
		{
			bobberDummy.position = bobber.position;
		}
		else
		{
			bobberDummy.position = leftHandPos.position;
		}
		if (networkVersion.fishOnLine != -1 && !beingDisabled && WorldManager.manageWorld.isPositionOnMap(bobberDummy.position) && WorldManager.manageWorld.waterMap[Mathf.RoundToInt(bobberDummy.position.x / 2f), Mathf.RoundToInt(bobberDummy.position.z / 2f)])
		{
			Vector3 position = bobberDummy.position;
			position.y = 0.6f;
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.waterSplash, position);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.waterParticle, position, 30);
		}
		dummyRen.enabled = true;
	}

	public void setDistance(float newDistance)
	{
		castDistance = newDistance;
		playChargeSound(newDistance / 5f * 1.5f);
	}

	public void biteBobber()
	{
		bobberAnim.SetTrigger("Bite");
	}

	public void fakeBiteBobber()
	{
		bool isLocalPlayer = myChar.isLocalPlayer;
		bobberAnim.SetTrigger("FakeBite");
	}

	private IEnumerator castLineToPosition()
	{
		reelSounds.pitch = 10f;
		yield return new WaitForSeconds(0.1f);
		castComplete = false;
		bool midWay = false;
		Vector3 midwayPoint = Vector3.Lerp(base.transform.position, networkVersion.reelPosition, 0.8f) + Vector3.up * 5f;
		bobber.position = bobberDummy.position;
		bobber.rotation = Quaternion.Euler(Vector3.zero);
		bobber.gameObject.SetActive(true);
		dummyRen.enabled = false;
		Vector3 vel = Vector3.zero;
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			CameraController.control.setFollowTransform(bobber, 0.5f);
		}
		while (!castComplete)
		{
			yield return null;
			playCastSound();
			if (!midWay)
			{
				bobber.position = Vector3.SmoothDamp(bobber.position, midwayPoint, ref vel, 0.1f);
				if (Vector3.Distance(bobber.position, midwayPoint) < 0.1f)
				{
					vel = Vector3.zero;
					midWay = true;
				}
				continue;
			}
			bobber.position = Vector3.SmoothDamp(bobber.position, networkVersion.reelPosition, ref vel, 0.15f);
			if (!(Vector3.Distance(bobber.position, networkVersion.reelPosition) < 1f))
			{
				continue;
			}
			bobber.position = networkVersion.reelPosition;
			RaycastHit hitInfo;
			if (Physics.Raycast(networkVersion.transform.position + networkVersion.transform.forward * castDistance + Vector3.up * 3f, Vector3.down, out hitInfo, 25f, castMask))
			{
				if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Water"))
				{
					bobberAnim.SetTrigger("Landed");
					bobberInWater = true;
				}
				else
				{
					bobberAnim.SetTrigger("LandedDirt");
					bobberInWater = false;
				}
			}
			castComplete = true;
			networkVersion.lineIsCasted = true;
		}
		stopCastSound();
	}

	private void playCastSound()
	{
		reelSounds.volume = 0.36f * SoundManager.manage.getSoundVolume();
		if (!reelSounds.isPlaying)
		{
			reelSounds.Play();
		}
		reelSounds.pitch = Mathf.Clamp(reelSounds.pitch - 0.25f, 2f, 10f);
	}

	private void stopCastSound()
	{
		reelSounds.pitch = 2f;
		reelSounds.Stop();
	}

	public void playReelSound(float pitch)
	{
		if (networkVersion.fishOnLine != -1)
		{
			if (networkVersion.pulling)
			{
				pitch *= 2.5f;
				reelSounds.volume = 0.56f * SoundManager.manage.getSoundVolume();
			}
			else
			{
				pitch *= 1.5f;
				reelSounds.volume = 0.46f * SoundManager.manage.getSoundVolume();
			}
		}
		else
		{
			reelSounds.volume = 0.36f * SoundManager.manage.getSoundVolume();
		}
		reelSounds.pitch = pitch;
		if (!reelSounds.isPlaying)
		{
			reelSounds.Play();
		}
	}

	public void stopReelSounds()
	{
		reelSounds.Stop();
	}

	public void playChargeSound(float pitch)
	{
		chargeSounds.pitch = pitch;
		chargeSounds.volume = 0.25f * SoundManager.manage.getSoundVolume();
	}

	public void doCast()
	{
		reeling = false;
		bobber.parent = null;
		StartCoroutine(castLineToPosition());
	}

	private IEnumerator playCompleteReelSound()
	{
		float timer2 = 0.5f;
		reelSounds.volume = 0.8f * SoundManager.manage.getSoundVolume();
		while (timer2 > 0f)
		{
			reelSounds.pitch = 1f + 10f * timer2;
			if (!reelSounds.isPlaying)
			{
				reelSounds.Play();
			}
			timer2 -= Time.deltaTime;
			yield return null;
		}
		timer2 = 0.25f;
		while (timer2 > 0f)
		{
			reelSounds.volume = Mathf.Lerp(0f, 0.8f * SoundManager.manage.getSoundVolume(), timer2 * 4f);
			timer2 -= Time.deltaTime;
			yield return null;
		}
		stopReelSounds();
	}
}
