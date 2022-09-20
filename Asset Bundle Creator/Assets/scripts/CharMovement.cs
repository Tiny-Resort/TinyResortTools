using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class CharMovement : NetworkBehaviour
{
	private Rigidbody myRig;

	private float currentSpeed;

	public CharInteract myInteract;

	public EquipItemToChar myEquip;

	public CharPickUp myPickUp;

	public CharTalkUse myTalkUse;

	public Animator myAnim;

	private float runningMultipier;

	private float animSpeed;

	public LayerMask jumpLayers;

	public LayerMask autoWalkLayer;

	public LayerMask swimLayers;

	public LayerMask vehicleLayers;

	private bool inJump;

	private Transform cameraContainer;

	public CapsuleCollider col;

	public bool grounded;

	public bool swimming;

	[SyncVar(hook = "onChangeUnderWater")]
	public bool underWater;

	private float pickUpTimer;

	private float pickUpTileObjectTimer;

	private Vector3 lasHighLighterPos = Vector3.zero;

	private bool attackLock;

	private bool moveLockRotateSlow;

	private bool rotationLock;

	private bool sneaking;

	public bool localUsing;

	public GameObject underWaterHit;

	public Vehicle driving;

	public Vehicle passenger;

	public Transform parentTrans;

	[SyncVar(hook = "onChangeStamina")]
	public int stamina = 50;

	[SyncVar]
	public uint standingOn;

	public LayerMask myEnemies;

	public int followedBy = -1;

	private RuntimeAnimatorController defaultController;

	private NetworkFishingRod myRod;

	public GameObject reviveBox;

	public bool localBlocking;

	private NetworkTransform networkTransform;

	public Transform wallCheck1;

	public Transform wallCheck2;

	public bool usingHangGlider;

	public bool usingBoogieBoard;

	[SyncVar]
	public int beingTargetedBy;

	private bool lastSwimming;

	public bool canClimb = true;

	private float jumpDif;

	private float swimDif = 1f;

	private float runDif;

	private WaitForSeconds passengerWait = new WaitForSeconds(0.05f);

	private static WaitForFixedUpdate jumpWait;

	public float jumpUpHeight = 3f;

	public float fallSpeed = -1f;

	private bool facingTarget;

	public bool isCurrentlyTalking;

	private bool animatedTired;

	private bool beingKnockedBack;

	private WaitForSeconds swimWait = new WaitForSeconds(0.25f);

	private bool landedInWater;

	public bool NetworkunderWater
	{
		get
		{
			return underWater;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref underWater))
			{
				bool old = underWater;
				SetSyncVar(value, ref underWater, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onChangeUnderWater(old, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public int Networkstamina
	{
		get
		{
			return stamina;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref stamina))
			{
				int oldStam = stamina;
				SetSyncVar(value, ref stamina, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, true);
					onChangeStamina(oldStam, value);
					setSyncVarHookGuard(2uL, false);
				}
			}
		}
	}

	public uint NetworkstandingOn
	{
		get
		{
			return standingOn;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref standingOn))
			{
				uint num = standingOn;
				SetSyncVar(value, ref standingOn, 4uL);
			}
		}
	}

	public int NetworkbeingTargetedBy
	{
		get
		{
			return beingTargetedBy;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref beingTargetedBy))
			{
				int num = beingTargetedBy;
				SetSyncVar(value, ref beingTargetedBy, 8uL);
			}
		}
	}

	private void Start()
	{
		col = GetComponent<CapsuleCollider>();
		myRig = GetComponent<Rigidbody>();
		myAnim = GetComponent<Animator>();
		myRod = GetComponent<NetworkFishingRod>();
		defaultController = myAnim.runtimeAnimatorController;
		networkTransform = GetComponent<NetworkTransform>();
		if (!base.isLocalPlayer)
		{
			myRig.isKinematic = true;
		}
	}

	public override void OnStartServer()
	{
		NetworkstandingOn = 0u;
		NetworkMapSharer.share.RpcSyncDate(WorldManager.manageWorld.day, WorldManager.manageWorld.week, WorldManager.manageWorld.month);
		NetworkPlayersManager.manage.addPlayer(this);
	}

	public override void OnStopServer()
	{
		NetworkPlayersManager.manage.removePlayer(this);
	}

	public override void OnStartLocalPlayer()
	{
		myRig = GetComponent<Rigidbody>();
		NetworkMapSharer.share.localChar = this;
		cameraContainer = CameraController.control.transform;
		StatusManager.manage.connectPlayer(GetComponent<Damageable>());
		RenderMap.map.connectMainChar(base.transform);
		lockClientOnLoad();
		RenderMap.map.unTrackOtherPlayers(base.transform);
		myEquip.CmdEquipNewItem(Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemNo);
		HouseDetails houseDetails = null;
		if (TownManager.manage.savedInside[0] != -1 && TownManager.manage.savedInside[1] != -1)
		{
			houseDetails = HouseManager.manage.getHouseInfoIfExists(TownManager.manage.savedInside[0], TownManager.manage.savedInside[1]);
		}
		if (base.isServer && houseDetails != null)
		{
			myInteract.changeInsideOut(true, TownManager.manage.sleepInsideHouse);
			WeatherManager.manage.goInside(false);
			RealWorldTimeLight.time.goInside();
			MusicManager.manage.changeInside(true, false);
			Inventory.inv.equipNewSelectedSlot();
		}
		else if (base.transform.position.y <= -12f)
		{
			WeatherManager.manage.goInside(false);
			RealWorldTimeLight.time.goInside();
			myEquip.setInsideOrOutside(true, false);
			MusicManager.manage.changeInside(true, false);
		}
		NetworkMapSharer.share.personalSpawnPoint = base.transform.position;
		base.transform.eulerAngles = new Vector3(0f, TownManager.manage.savedRot, 0f);
		StartCoroutine(swimmingAndDivingStamina());
	}

	public override void OnStartClient()
	{
		col = GetComponent<CapsuleCollider>();
		myRig = GetComponent<Rigidbody>();
		myAnim = GetComponent<Animator>();
		NetworkNavMesh.nav.addAPlayer(base.transform);
		updateStandingOn(standingOn);
	}

	public override void OnStopClient()
	{
		if (base.isServer)
		{
			NetworkNavMesh.nav.removeSleepingChar(base.transform);
		}
		NetworkNavMesh.nav.removeAPlayer(base.transform);
	}

	private void Update()
	{
		if (!myEquip.isInVehicle())
		{
			if ((bool)myEquip.currentlyHolding && myEquip.currentlyHolding.isATool)
			{
				if (base.isLocalPlayer)
				{
					myAnim.SetBool(CharNetworkAnimator.usingAnimName, localUsing);
				}
				else
				{
					myAnim.SetBool(CharNetworkAnimator.usingAnimName, myEquip.usingItem);
				}
			}
			else
			{
				myAnim.SetBool(CharNetworkAnimator.usingAnimName, false);
			}
		}
		if (!base.isLocalPlayer)
		{
			return;
		}
		lastSwimming = swimming;
		swimming = Physics.CheckSphere(base.transform.position, 0.1f, swimLayers);
		if (!lastSwimming && swimming)
		{
			StartCoroutine(landInWaterTimer());
		}
		grounded = Physics.CheckSphere(base.transform.position + Vector3.up * 0.3f, 0.6f, jumpLayers);
		myEquip.setSwimming(swimming);
		myAnim.SetBool(CharNetworkAnimator.groundedAnimName, grounded);
		if (base.transform.position.y < -1000f)
		{
			myRig.velocity = Vector3.zero;
			base.transform.position = new Vector3(base.transform.position.x, 10f, base.transform.position.z);
			NewChunkLoader.loader.inside = false;
			myInteract.changeInsideOut(false);
			WeatherManager.manage.goOutside();
			RealWorldTimeLight.time.goOutside();
			myEquip.setInsideOrOutside(false, false);
		}
		if (StatusManager.manage.dead)
		{
			if (localUsing)
			{
				myEquip.CmdUsingItem(false);
				localUsing = false;
			}
			if (underWater)
			{
				myRig.useGravity = true;
				NetworkunderWater = false;
				col.enabled = true;
				underWaterHit.SetActive(false);
				SoundManager.manage.switchUnderWater(false);
				myAnim.SetBool(CharNetworkAnimator.underwaterAnimName, false);
				CmdChangeUnderWater(false);
			}
			myEquip.animateOnUse(false, false);
			if ((double)base.transform.position.y < 0.5 && WorldManager.manageWorld.waterMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)])
			{
				myRig.isKinematic = true;
				base.transform.position = Vector3.Lerp(base.transform.position, new Vector3(base.transform.position.x, 0.5f, base.transform.position.z), Time.deltaTime / 2f);
			}
			return;
		}
		if (myPickUp.sitting)
		{
			if (!myInteract.placingDeed && InputMaster.input.Other())
			{
				myPickUp.pressY();
			}
			if ((bool)myEquip.currentlyHolding && (bool)myEquip.currentlyHolding.consumeable)
			{
				checkUseItemButton();
			}
			return;
		}
		if (PhotoManager.manage.cameraViewOpen && (InputMaster.input.UICancel() || InputMaster.input.Journal() || InputMaster.input.OpenInventory()))
		{
			myEquip.holdingPrefabAnimator.SetTrigger("CloseCamera");
			CmdCloseCamera();
		}
		if (!Inventory.inv.canMoveChar())
		{
			if (localUsing && !myEquip.isWhistling())
			{
				localUsing = false;
				myEquip.CmdUsingItem(false);
			}
			myEquip.animateOnUse(false, false);
			return;
		}
		if (underWater && InputMaster.input.JumpHeld())
		{
			myRig.MovePosition(myRig.position + Vector3.up * 3f * Time.fixedDeltaTime);
			if (base.transform.position.y >= -0.38f)
			{
				myRig.useGravity = true;
				NetworkunderWater = false;
				col.enabled = true;
				underWaterHit.SetActive(false);
				SoundManager.manage.switchUnderWater(false);
				myAnim.SetBool(CharNetworkAnimator.underwaterAnimName, false);
				CmdChangeUnderWater(false);
			}
		}
		if (!myInteract.placingDeed && !PhotoManager.manage.cameraViewOpen && !myRod.lineIsCasted && InputMaster.input.Jump() && !underWater)
		{
			if (grounded && !inJump)
			{
				inJump = true;
				StartCoroutine(jumpFeel());
			}
			else if ((swimming && usingBoogieBoard && !inJump) || (swimming && !inJump && Physics.Raycast(base.transform.position + base.transform.forward * 1.5f + Vector3.up * 3f, Vector3.down, 3f, jumpLayers)))
			{
				inJump = true;
				StartCoroutine(jumpFeel());
			}
		}
		if (InputMaster.input.Interact() && !myInteract.placingDeed && !myRod.lineIsCasted)
		{
			if ((bool)myEquip.currentlyHolding && myEquip.currentlyHolding.itemName == "Camera" && PhotoManager.manage.cameraViewOpen && PhotoManager.manage.canMoveCam)
			{
				myEquip.holdingPrefabAnimator.SetTrigger("CloseCamera");
				CmdCloseCamera();
			}
			else if (!myPickUp.isCarryingSomething())
			{
				if (!myPickUp.pickUp())
				{
					myInteract.doATileInteractions();
				}
			}
			else
			{
				myPickUp.pickUp();
			}
		}
		if (InputMaster.input.Other() && !myPickUp.drivingVehicle)
		{
			if (myInteract.tileHighlighter.position != lasHighLighterPos)
			{
				pickUpTileObjectTimer = 0f;
				lasHighLighterPos = myInteract.tileHighlighter.position;
			}
			if ((myInteract.myEquip.currentlyHolding != null && !myInteract.insidePlayerHouse && myInteract.myEquip.currentlyHoldingSinglePlaceableItem() && myInteract.myEquip.currentlyHolding.placeable.getsRotationFromMap()) || myInteract.placingDeed || ((bool)myInteract.myEquip.currentlyHolding && (bool)myInteract.myEquip.currentlyHolding.spawnPlaceable))
			{
				myInteract.rotatePreview();
			}
			else
			{
				myInteract.pickUpTileObject();
			}
		}
		if (localUsing || !myEquip.currentlyHolding || !myEquip.currentlyHolding.canBlock)
		{
			if (localBlocking)
			{
				CmdChangeBlocking(false);
				localBlocking = false;
			}
			if (InputMaster.input.Other() && swimming && !underWater)
			{
				NetworkunderWater = true;
				col.enabled = false;
				underWaterHit.SetActive(true);
				SoundManager.manage.switchUnderWater(true);
				myAnim.SetBool(CharNetworkAnimator.underwaterAnimName, true);
				CmdChangeUnderWater(true);
				myRig.useGravity = false;
				pickUpTimer = 0f;
			}
			if (InputMaster.input.InteractHeld())
			{
				if (pickUpTimer < 1f)
				{
					pickUpTimer += Time.deltaTime;
				}
				if (pickUpTimer > 0.25f)
				{
					myPickUp.holdingPickUp = true;
				}
			}
			else
			{
				myPickUp.holdingPickUp = false;
				pickUpTimer = 0f;
			}
		}
		if (InputMaster.input.Use() && (!myEquip.currentlyHolding || !(myEquip.currentlyHolding.itemName == "Camera") || !PhotoManager.manage.cameraViewOpen || !PhotoManager.manage.canMoveCam) && !myEquip.getDriving())
		{
			myPickUp.pressX();
		}
		if (InputMaster.input.Other())
		{
			myPickUp.pressY();
		}
		checkUseItemButton();
	}

	public void checkUseItemButton()
	{
		if (InputMaster.input.Use() && myEquip.needsHandPlaceable())
		{
			localUsing = false;
			myEquip.CmdUsingItem(false);
			StartCoroutine(replaceHandPlaceableDelay());
		}
		else if (InputMaster.input.UseHeld() || myEquip.isWhistling())
		{
			if (myInteract.selectedTileNeedsServerRefresh())
			{
				myInteract.CmdCurrentlyAttackingPos((int)myInteract.selectedTile.x, (int)myInteract.selectedTile.y);
			}
			if (!localUsing)
			{
				localUsing = true;
				myEquip.CmdUsingItem(true);
			}
			myEquip.animateOnUse(true, localBlocking);
		}
		else
		{
			if (localUsing)
			{
				localUsing = false;
				myEquip.CmdUsingItem(false);
			}
			myEquip.animateOnUse(false, localBlocking);
		}
	}

	private void LateUpdate()
	{
		if (!base.isLocalPlayer && myPickUp.sittingPos != Vector3.zero)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, myPickUp.sittingPos, Time.deltaTime * 8f);
		}
	}

	private void FixedUpdate()
	{
		if (!base.isLocalPlayer || StatusManager.manage.dead)
		{
			return;
		}
		if (myPickUp.drivingVehicle && myPickUp.currentlyDriving.mountingAnimationComplete)
		{
			base.transform.position = driving.driversPos.position;
			base.transform.rotation = driving.driversPos.rotation;
		}
		if (myPickUp.sitting)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, myPickUp.sittingPosition.position, Time.deltaTime * 8f);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, myPickUp.sittingPosition.rotation, Time.deltaTime * 8f);
		}
		if (myInteract.placingDeed || myPickUp.drivingVehicle || !Inventory.inv.canMoveChar())
		{
			if (!myPickUp.drivingVehicle || !myPickUp.currentlyDriving.animateCharAsWell)
			{
				myAnim.SetFloat(CharNetworkAnimator.walkingAnimName, Mathf.Lerp(myAnim.GetFloat(CharNetworkAnimator.walkingAnimName), 0f, Time.deltaTime * 2f));
			}
			return;
		}
		if (!driving)
		{
			RaycastHit hitInfo;
			if ((bool)base.transform.parent && standingOn != 0 && !inJump)
			{
				if (!Physics.Raycast(base.transform.position + Vector3.up / 4f, Vector3.down, out hitInfo, 1f, vehicleLayers))
				{
					CmdChangeStandingOn(0u);
					networkTransform.enabled = false;
					NetworkstandingOn = 0u;
					passenger = null;
				}
			}
			else if (standingOn == 0 && !inJump)
			{
				if (Physics.Raycast(base.transform.position + Vector3.up / 4f, Vector3.down, out hitInfo, 0.5f, vehicleLayers))
				{
					passenger = hitInfo.transform.gameObject.GetComponent<VehicleHitBox>().connectedTo;
					CmdChangeStandingOn(passenger.netId);
					networkTransform.enabled = false;
					NetworkstandingOn = passenger.netId;
				}
			}
			else if ((bool)base.transform.parent && standingOn != 0 && inJump)
			{
				CmdChangeStandingOn(0u);
				networkTransform.enabled = false;
				NetworkstandingOn = 0u;
				passenger = null;
			}
			else if ((standingOn != 0 && !base.transform.parent) || (standingOn != 0 && passenger != null))
			{
				CmdChangeStandingOn(0u);
				networkTransform.enabled = false;
				NetworkstandingOn = 0u;
				passenger = null;
			}
		}
		if (!attackLock && !myPickUp.sitting)
		{
			charMoves(InputMaster.input.getLeftStick().x, InputMaster.input.getLeftStick().y);
			return;
		}
		myAnim.SetFloat(CharNetworkAnimator.walkingAnimName, Mathf.Lerp(myAnim.GetFloat(CharNetworkAnimator.walkingAnimName), 0f, Time.deltaTime * 5f));
		if (moveLockRotateSlow)
		{
			rotateCharToDir(InputMaster.input.getLeftStick().x, InputMaster.input.getLeftStick().y, 1f);
		}
	}

	private void charMoves(float xSpeed, float zSpeed)
	{
		bool flag = false;
		if (xSpeed != 0f || zSpeed != 0f)
		{
			if ((StatusManager.manage.tired && !usingHangGlider) || sneaking)
			{
				currentSpeed = Mathf.Lerp(currentSpeed, 5f, Time.deltaTime * 2f);
				flag = true;
			}
			else if (!swimming)
			{
				currentSpeed = Mathf.Lerp(currentSpeed, 9f + runDif, Time.deltaTime * 2f);
			}
			else
			{
				currentSpeed = Mathf.Lerp(currentSpeed, 9f, Time.deltaTime * 2f);
			}
			flag = true;
		}
		else
		{
			currentSpeed = Mathf.Lerp(currentSpeed, 5f, Time.deltaTime * 2f);
		}
		if (!rotationLock)
		{
			if (currentSpeed < 3f)
			{
				rotateCharToDir(xSpeed, zSpeed, 4f);
			}
			else
			{
				rotateCharToDir(xSpeed, zSpeed);
			}
		}
		Vector3 vector = cameraContainer.TransformDirection(Vector3.forward) * zSpeed;
		Vector3 vector2 = cameraContainer.TransformDirection(Vector3.right) * xSpeed;
		Vector3 vector3 = Vector3.ClampMagnitude(vector + vector2, 1f);
		Vector3 vector4 = vector3 * currentSpeed;
		if (underWater)
		{
			if (!InputMaster.input.JumpHeld() && !grounded)
			{
				myRig.MovePosition(myRig.position + Vector3.down * Time.deltaTime);
			}
			myRig.MovePosition(myRig.position + vector4 / 4.5f * swimDif * Time.fixedDeltaTime);
		}
		else if (swimming)
		{
			if (!landedInWater)
			{
				myRig.MovePosition(myRig.position + vector4 / 3f * swimDif * Time.fixedDeltaTime);
			}
			else
			{
				myRig.MovePosition(myRig.position + vector4 / 6f * swimDif * Time.fixedDeltaTime);
			}
		}
		else if (canClimb && !inJump && Physics.Raycast(base.transform.position, vector3, col.radius + 0.35f, autoWalkLayer) && Physics.Raycast(base.transform.position + Vector3.up * 1.35f + vector3, Vector3.down, 0.55f, autoWalkLayer))
		{
			myRig.MovePosition(myRig.position + (vector4 + Vector3.up * 25f) * Time.fixedDeltaTime);
		}
		else if ((inJump || !Physics.Raycast(base.transform.position + Vector3.up, base.transform.forward, col.radius + 0.15f, jumpLayers)) && (inJump || !Physics.Raycast(base.transform.position + Vector3.up, wallCheck1.forward, col.radius + 0.15f, jumpLayers) || !Physics.Raycast(base.transform.position + Vector3.up, wallCheck2.forward, col.radius + 0.15f, jumpLayers)) && (!inJump || !Physics.Raycast(base.transform.position, vector3, col.radius + 0.15f, jumpLayers)) && (!inJump || !Physics.Raycast(base.transform.position, vector3 + base.transform.right / 3f, col.radius + 0.15f, jumpLayers)) && (!inJump || !Physics.Raycast(base.transform.position, vector3 - base.transform.right / 3f, col.radius + 0.15f, jumpLayers)))
		{
			if ((bool)passenger && (bool)parentTrans)
			{
				base.transform.localPosition = base.transform.localPosition + parentTrans.InverseTransformDirection(vector4) * Time.fixedDeltaTime;
			}
			else
			{
				myRig.MovePosition(myRig.position + vector4 * Time.fixedDeltaTime);
			}
		}
		animSpeed = Mathf.Lerp(animSpeed, Mathf.Clamp01(Mathf.Abs(zSpeed) + Mathf.Abs(xSpeed)), Time.deltaTime * 10f);
		if (StatusManager.manage.tired || sneaking)
		{
			animSpeed /= 1.2f;
		}
		myAnim.SetBool(CharNetworkAnimator.swimmingAnimName, swimming);
		if ((bool)myAnim)
		{
			if (swimming)
			{
				runningMultipier = Mathf.Lerp(runningMultipier, 1f, Time.deltaTime);
				myAnim.SetFloat(CharNetworkAnimator.walkingAnimName, animSpeed * runningMultipier);
			}
			else if (!grounded || attackLock)
			{
				runningMultipier = Mathf.Lerp(runningMultipier, 0f, Time.deltaTime * 2f);
				myAnim.SetFloat(CharNetworkAnimator.walkingAnimName, animSpeed * runningMultipier);
			}
			else if (flag)
			{
				runningMultipier = Mathf.Lerp(runningMultipier, 2f, Time.deltaTime * 5f);
				myAnim.SetFloat(CharNetworkAnimator.walkingAnimName, animSpeed * runningMultipier);
			}
			else
			{
				runningMultipier = Mathf.Lerp(runningMultipier, 1f, Time.deltaTime);
				myAnim.SetFloat(CharNetworkAnimator.walkingAnimName, animSpeed * runningMultipier);
			}
		}
	}

	public void startAttackSpeed(float newSpeed)
	{
		currentSpeed = newSpeed;
		runningMultipier = 1f;
	}

	public void charMovesForward()
	{
		currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * 2f);
		Vector3 vector = base.transform.forward * currentSpeed;
		if (swimming)
		{
			myRig.MovePosition(myRig.position + vector / 2.5f * swimDif * Time.fixedDeltaTime);
		}
		else if (!inJump || (inJump && !Physics.Raycast(base.transform.position, base.transform.forward, col.radius + 0.1f, jumpLayers)))
		{
			myRig.MovePosition(myRig.position + vector * Time.fixedDeltaTime);
		}
		animSpeed = 2f;
		myAnim.SetBool(CharNetworkAnimator.swimmingAnimName, swimming);
		if ((bool)myAnim)
		{
			runningMultipier = Mathf.Lerp(runningMultipier, 0f, Time.deltaTime * 3f);
			myAnim.SetFloat(CharNetworkAnimator.walkingAnimName, animSpeed * runningMultipier);
		}
	}

	public void isSneaking(bool isSneaking)
	{
		sneaking = isSneaking;
		if (isSneaking)
		{
			base.transform.tag = "Sneaking";
		}
		else
		{
			base.transform.tag = "Untagged";
		}
	}

	private void rotateCharToDir(float x, float y, float rotSpeed = 7f)
	{
		if (x != 0f || y != 0f)
		{
			Vector3 normalized = new Vector3(x, 0f, y).normalized;
			normalized = cameraContainer.transform.TransformDirection(normalized);
			if (parentTrans != null)
			{
				normalized = parentTrans.InverseTransformDirection(normalized);
			}
			if (normalized != Vector3.zero)
			{
				Quaternion b = Quaternion.LookRotation(normalized);
				base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, b, Time.deltaTime * rotSpeed);
			}
		}
	}

	public void setSpeedDif(int dif)
	{
		runDif = dif;
	}

	public void addOrRemoveJumpDif(int dif)
	{
		jumpDif += dif;
	}

	public void addOrRemoveSwimDif(float dif)
	{
		swimDif += dif;
	}

	public void giveIdolStats(int idolId)
	{
		jumpDif += Inventory.inv.allItems[idolId].equipable.jumpDif;
		swimDif += Mathf.Clamp(Inventory.inv.allItems[idolId].equipable.swimSpeedDif, 1f, 100f);
		runDif += Inventory.inv.allItems[idolId].equipable.runSpeedDif;
	}

	public void removeIdolStatus(int idolId)
	{
		jumpDif -= Inventory.inv.allItems[idolId].equipable.jumpDif;
		swimDif -= Mathf.Clamp(Inventory.inv.allItems[idolId].equipable.swimSpeedDif, 1f, 100f);
		runDif -= Inventory.inv.allItems[idolId].equipable.runSpeedDif;
	}

	private IEnumerator jumpFeel()
	{
		float desiredHeight = 0f;
		float multi = 25f;
		while (desiredHeight < jumpUpHeight)
		{
			yield return jumpWait;
			myRig.MovePosition(myRig.position + Vector3.up * desiredHeight * Time.fixedDeltaTime);
			desiredHeight = Mathf.Lerp(desiredHeight, jumpUpHeight + 1f, Time.fixedDeltaTime * multi);
			multi = Mathf.Lerp(multi, 10f, Time.deltaTime * 25f);
		}
		while (desiredHeight > 0f && !Physics.CheckSphere(base.transform.position + Vector3.up * 0.3f, 0.6f, jumpLayers) && !Physics.CheckSphere(base.transform.position, 0.1f, swimLayers))
		{
			yield return jumpWait;
			myRig.MovePosition(myRig.position + Vector3.up * desiredHeight * Time.fixedDeltaTime);
			desiredHeight = Mathf.Lerp(desiredHeight, -1f, Time.deltaTime * 2f);
		}
		while (!Physics.CheckSphere(base.transform.position + Vector3.up * 0.3f, 0.6f, jumpLayers) && !Physics.CheckSphere(base.transform.position, 0.1f, swimLayers))
		{
			yield return null;
		}
		inJump = false;
	}

	public void lockClientOnLoad()
	{
		myRig.isKinematic = true;
		CameraController.control.transform.position = base.transform.position;
		CameraController.control.setFollowTransform(base.transform);
		attackLock = true;
	}

	public void unlockClientOnLoad()
	{
		attackLock = false;
		myRig.isKinematic = false;
	}

	public void lockCharOnFreeCam()
	{
		myRig.isKinematic = true;
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		attackLock = true;
	}

	public void unlocklockCharOnFreeCam()
	{
		attackLock = false;
		myRig.isKinematic = false;
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
	}

	public void getInVehicle(Vehicle drivingVehicle)
	{
		myRig.isKinematic = true;
		driving = drivingVehicle;
		if ((bool)passenger)
		{
			base.transform.SetParent(null);
			parentTrans = null;
			passenger = null;
			CmdChangeStandingOn(0u);
		}
	}

	public void getOutVehicle()
	{
		myRig.isKinematic = false;
		driving = null;
	}

	public void onChangeUnderWater(bool old, bool newUnderWater)
	{
		NetworkunderWater = newUnderWater;
		if (!base.isLocalPlayer)
		{
			NetworkunderWater = newUnderWater;
			col.enabled = !newUnderWater;
			underWaterHit.SetActive(newUnderWater);
			myAnim.SetBool(CharNetworkAnimator.underwaterAnimName, newUnderWater);
		}
	}

	public void unlockAll()
	{
		attackLock = false;
		moveLockRotateSlow = false;
		rotationLock = false;
	}

	public void lockRotation(bool isLocked)
	{
		if (rotationLock != isLocked)
		{
			currentSpeed = 4f;
		}
		rotationLock = isLocked;
	}

	public void faceClosestTarget()
	{
		if (base.isLocalPlayer)
		{
			StartCoroutine(findClosestTargetAndFace());
		}
	}

	private IEnumerator findClosestInteractable()
	{
		yield return null;
	}

	private IEnumerator findClosestTargetAndFace()
	{
		facingTarget = true;
		float y = InputMaster.input.getLeftStick().y;
		float x = InputMaster.input.getLeftStick().x;
		Vector3 vector = base.transform.forward;
		if (y != 0f && x != 0f)
		{
			Vector3 vector2 = cameraContainer.TransformDirection(Vector3.forward) * y;
			Vector3 vector3 = cameraContainer.TransformDirection(Vector3.left) * x;
			vector = Vector3.ClampMagnitude(vector2 + vector3, 1f);
		}
		if (Physics.CheckSphere(base.transform.position + Vector3.up + vector * 2f, 2.5f, myEnemies))
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position + Vector3.up + vector * 2.5f, 3f, myEnemies);
			for (int i = 0; i < array.Length; i++)
			{
				if (!(array[i].transform != base.transform) || !array[i])
				{
					continue;
				}
				AnimalAI component = array[i].GetComponent<AnimalAI>();
				if ((bool)component && !component.isDead() && !component.isAPet())
				{
					float lookTimer = 0f;
					Quaternion desiredLook = Quaternion.LookRotation((new Vector3(array[i].transform.position.x, base.transform.position.y, array[i].transform.position.z) - base.transform.position).normalized);
					while (lookTimer < 1f)
					{
						lookTimer += Time.deltaTime;
						base.transform.rotation = Quaternion.Lerp(base.transform.rotation, desiredLook, Time.deltaTime * 7.5f);
						yield return null;
					}
					break;
				}
			}
		}
		facingTarget = false;
	}

	public void attackLockOn(bool isOn)
	{
		if (attackLock != isOn)
		{
			currentSpeed = 4f;
		}
		attackLock = isOn;
	}

	public void moveLockRotateSlowOn(bool isOn)
	{
		if (moveLockRotateSlow != isOn)
		{
			currentSpeed = 4f;
		}
		moveLockRotateSlow = isOn;
	}

	[Command]
	public void CmdChangeTalkTo(uint npcToTalkTo, bool isTalking)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(npcToTalkTo);
		writer.WriteBool(isTalking);
		SendCommandInternal(typeof(CharMovement), "CmdChangeTalkTo", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestInterior(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdRequestInterior", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestHouseInterior(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdRequestHouseInterior", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestHouseExterior(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdRequestHouseExterior", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdDonateItemToMuseum(int itemId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		SendCommandInternal(typeof(CharMovement), "CmdDonateItemToMuseum", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestShopStatus()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdRequestShopStatus", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestMuseumInterior()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdRequestMuseumInterior", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeUnderWater(bool newUnderWater)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(newUnderWater);
		SendCommandInternal(typeof(CharMovement), "CmdChangeUnderWater", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdNPCStartFollow(uint tellNPCtoFollow, uint transformToFollow)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(tellNPCtoFollow);
		writer.WriteUInt(transformToFollow);
		SendCommandInternal(typeof(CharMovement), "CmdNPCStartFollow", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestMapChunk(int chunkPosX, int chunkPosY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(chunkPosX);
		writer.WriteInt(chunkPosY);
		SendCommandInternal(typeof(CharMovement), "CmdRequestMapChunk", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestItemOnTopForChunk(int chunkX, int chunkY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(chunkX);
		writer.WriteInt(chunkY);
		SendCommandInternal(typeof(CharMovement), "CmdRequestItemOnTopForChunk", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSendChatMessage(string newMessage)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(newMessage);
		SendCommandInternal(typeof(CharMovement), "CmdSendChatMessage", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSendEmote(int newEmote)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newEmote);
		SendCommandInternal(typeof(CharMovement), "CmdSendEmote", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdDealDamage(uint netId, float multiplier)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(netId);
		writer.WriteFloat(multiplier);
		SendCommandInternal(typeof(CharMovement), "CmdDealDamage", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdTakeDamage(int damageAmount)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(damageAmount);
		SendCommandInternal(typeof(CharMovement), "CmdTakeDamage", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCloseChest(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdCloseChest", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestOnTileStatus(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdRequestOnTileStatus", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestTileRotation(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdRequestTileRotation", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdReviveSelf()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdReviveSelf", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCatchBug(uint bugToCatch)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(bugToCatch);
		SendCommandInternal(typeof(CharMovement), "CmdCatchBug", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcUpdateStandOn(uint standingOnForRPC)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(standingOnForRPC);
		SendRPCInternal(typeof(CharMovement), "RpcUpdateStandOn", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private void updateStandingOn(uint standingOnForRPC)
	{
		if (standingOnForRPC != 0)
		{
			parentTrans = NetworkIdentity.spawned[standingOnForRPC].GetComponent<Vehicle>().myHitBox;
			base.transform.SetParent(parentTrans);
		}
		else
		{
			base.transform.SetParent(null);
			parentTrans = null;
		}
		if ((bool)networkTransform)
		{
			networkTransform.resetObjectsInterpolation();
			if (base.isLocalPlayer)
			{
				networkTransform.enabled = true;
			}
		}
	}

	public void onChangeStamina(int oldStam, int newStam)
	{
		Networkstamina = newStam;
		if (newStam < 10)
		{
			if (!animatedTired)
			{
				animatedTired = true;
				myAnim.SetBool("Tired", true);
			}
		}
		else if (animatedTired)
		{
			animatedTired = false;
			myAnim.SetBool("Tired", false);
		}
	}

	[Command]
	public void CmdSetNewStamina(int newStam)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newStam);
		SendCommandInternal(typeof(CharMovement), "CmdSetNewStamina", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdDropItem(int itemId, int stackAmount, Vector3 dropPos, Vector3 desirePos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(stackAmount);
		writer.WriteVector3(dropPos);
		writer.WriteVector3(desirePos);
		SendCommandInternal(typeof(CharMovement), "CmdDropItem", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcReleaseBug(int bugId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(bugId);
		SendRPCInternal(typeof(CharMovement), "RpcReleaseBug", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcReleaseFish(int fishId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(fishId);
		SendRPCInternal(typeof(CharMovement), "RpcReleaseFish", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeStandingOn(uint newStandOn)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(newStandOn);
		SendCommandInternal(typeof(CharMovement), "CmdChangeStandingOn", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdAgreeToCraftsmanCrafting()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdAgreeToCraftsmanCrafting", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPlaceAnimalInCollectionPoint(uint animalTrapPlaced)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(animalTrapPlaced);
		SendCommandInternal(typeof(CharMovement), "CmdPlaceAnimalInCollectionPoint", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSpawnAnimalBox(int animalId, int variation, string animalName)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(animalId);
		writer.WriteInt(variation);
		writer.WriteString(animalName);
		SendCommandInternal(typeof(CharMovement), "CmdSpawnAnimalBox", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator moveBoxToPos(PickUpAndCarry carry)
	{
		yield return null;
		carry.dropToPos = FarmAnimalMenu.menu.spawnFarmAnimalPos.position.y;
		carry.transform.position = FarmAnimalMenu.menu.spawnFarmAnimalPos.position;
	}

	[Command]
	public void CmdSellByWeight(uint itemPlaced)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(itemPlaced);
		SendCommandInternal(typeof(CharMovement), "CmdSellByWeight", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdActivateTrap(uint animalToTrapId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(animalToTrapId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharMovement), "CmdActivateTrap", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSetOnFire(uint damageableId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(damageableId);
		SendCommandInternal(typeof(CharMovement), "CmdSetOnFire", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdBuyItemFromStall(int stallType, int shopStallNo)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(stallType);
		writer.WriteInt(shopStallNo);
		SendCommandInternal(typeof(CharMovement), "CmdBuyItemFromStall", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCloseCamera()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdCloseCamera", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcCloseCamera()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CharMovement), "RpcCloseCamera", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcTakeKnockback(Vector3 knockBackDir, float knockBackAmount)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(knockBackDir);
		writer.WriteFloat(knockBackAmount);
		SendRPCInternal(typeof(CharMovement), "RpcTakeKnockback", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPlaceMarkerOnMap(Vector2 markPos, int iconId, int iconType)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector2(markPos);
		writer.WriteInt(iconId);
		writer.WriteInt(iconType);
		SendCommandInternal(typeof(CharMovement), "CmdPlaceMarkerOnMap", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlaceMarkerOnMap(uint placedBy, Vector2 markPos, int iconNo, int iconType)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(placedBy);
		writer.WriteVector2(markPos);
		writer.WriteInt(iconNo);
		writer.WriteInt(iconType);
		SendRPCInternal(typeof(CharMovement), "RpcPlaceMarkerOnMap", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRemoveMyMarker()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdRemoveMyMarker", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcRemoveMarker(uint removeMarkerOwner)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(removeMarkerOwner);
		SendRPCInternal(typeof(CharMovement), "RpcRemoveMarker", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdRequestNPCInv(int npcId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(npcId);
		SendCommandInternal(typeof(CharMovement), "CmdRequestNPCInv", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPayTownDebt(int payment)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(payment);
		SendCommandInternal(typeof(CharMovement), "CmdPayTownDebt", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdGetDeedIngredients(int deedX, int deedY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(deedX);
		writer.WriteInt(deedY);
		SendCommandInternal(typeof(CharMovement), "CmdGetDeedIngredients", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdDonateDeedIngredients(int deedX, int deedY, int[] alreadyGiven)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(deedX);
		writer.WriteInt(deedY);
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, alreadyGiven);
		SendCommandInternal(typeof(CharMovement), "CmdDonateDeedIngredients", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCharFaints()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharMovement), "CmdCharFaints", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcSetCharFaints(bool isFainted)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(isFainted);
		SendRPCInternal(typeof(CharMovement), "RpcSetCharFaints", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeBlocking(bool isBlocking)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(isBlocking);
		SendCommandInternal(typeof(CharMovement), "CmdChangeBlocking", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdAcceptBulletinBoardPost(int id)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(id);
		SendCommandInternal(typeof(CharMovement), "CmdAcceptBulletinBoardPost", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcAcceptBulletinBoardPost(int id)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(id);
		SendRPCInternal(typeof(CharMovement), "RpcAcceptBulletinBoardPost", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCompleteBulletinBoardPost(int id)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(id);
		SendCommandInternal(typeof(CharMovement), "CmdCompleteBulletinBoardPost", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcCompleteBulletinBoardPost(int id)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(id);
		SendRPCInternal(typeof(CharMovement), "RpcCompleteBulletinBoardPost", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSetDefenceBuff(float newDefence)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteFloat(newDefence);
		SendCommandInternal(typeof(CharMovement), "CmdSetDefenceBuff", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSetHealthRegen(float timer, int level)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteFloat(timer);
		writer.WriteInt(level);
		SendCommandInternal(typeof(CharMovement), "CmdSetHealthRegen", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdTeleport(string teledir)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(teledir);
		SendCommandInternal(typeof(CharMovement), "CmdTeleport", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcTeleportChar(int[] pos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, pos);
		SendRPCInternal(typeof(CharMovement), "RpcTeleportChar", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	public IEnumerator teleportCharToPos(int[] pos)
	{
		int[] startPos = new int[2]
		{
			(int)base.transform.position.x / 2,
			(int)base.transform.position.z / 2
		};
		ParticleManager.manage.startTeleportParticles(startPos, pos);
		if (base.isLocalPlayer)
		{
			StartCoroutine(charLockedStill(2.5f));
			SoundManager.manage.play2DSound(SoundManager.manage.teleportCharge);
		}
		else
		{
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.teleportCharge, base.transform.position);
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.teleportCharge, new Vector3((float)pos[0] * 2f + 1f, (float)WorldManager.manageWorld.heightMap[pos[0], pos[1]] + 0.61f, (float)pos[1] * 2f + 1.5f));
		}
		yield return new WaitForSeconds(1.5f);
		if (base.isLocalPlayer)
		{
			SoundManager.manage.play2DSound(SoundManager.manage.teleportSound);
		}
		else
		{
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.teleportSound, base.transform.position);
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.teleportSound, new Vector3((float)pos[0] * 2f + 1f, (float)WorldManager.manageWorld.heightMap[pos[0], pos[1]] + 0.61f, (float)pos[1] * 2f + 1.5f));
		}
		yield return new WaitForSeconds(0.25f);
		if (base.isLocalPlayer)
		{
			base.transform.position = new Vector3((float)pos[0] * 2f + 1f, (float)WorldManager.manageWorld.heightMap[pos[0], pos[1]] + 0.61f, (float)pos[1] * 2f + 1.5f);
			CameraController.control.transform.position = NetworkMapSharer.share.localChar.transform.position;
			NewChunkLoader.loader.forceInstantUpdateAtPos();
		}
	}

	public IEnumerator charAttacksForward(float forwardSpeed = 5f, float forwardTime = 0.35f)
	{
		attackLockOn(true);
		float attackTimer = 0f;
		while (attackTimer < forwardTime)
		{
			yield return null;
			attackTimer += Time.deltaTime;
			forwardSpeed -= Time.deltaTime;
			myRig.MovePosition(myRig.position + base.transform.forward * forwardSpeed * Time.fixedDeltaTime);
		}
		attackLockOn(false);
	}

	public bool isInDanger()
	{
		return beingTargetedBy > 0;
	}

	public IEnumerator charLockedStill(float time)
	{
		attackLockOn(true);
		yield return new WaitForSeconds(time);
		attackLockOn(false);
	}

	private IEnumerator knockBack(Vector3 dir, float knockBackAmount)
	{
		beingKnockedBack = true;
		attackLockOn(true);
		float knockTimer = 0f;
		while (knockTimer < 0.35f)
		{
			yield return null;
			knockTimer += Time.deltaTime;
			if (!Physics.Raycast(base.transform.position + Vector3.up * 0.2f, dir, col.radius + 0.2f, jumpLayers))
			{
				myRig.MovePosition(myRig.position + dir * knockBackAmount * Time.fixedDeltaTime);
			}
		}
		attackLockOn(false);
		beingKnockedBack = false;
	}

	private IEnumerator swimmingAndDivingStamina()
	{
		while (true)
		{
			yield return null;
			while (swimming)
			{
				if (!usingBoogieBoard)
				{
					if (underWater)
					{
						StatusManager.manage.changeStamina(-0.1f);
					}
					else
					{
						StatusManager.manage.changeStamina(-0.05f);
					}
				}
				else if (underWater)
				{
					StatusManager.manage.changeStamina(-0.025f);
				}
				else
				{
					StatusManager.manage.changeStamina(-0.0125f);
				}
				yield return swimWait;
			}
		}
	}

	public IEnumerator landInWaterTimer()
	{
		landedInWater = true;
		float timer = 0.65f;
		while (timer >= 0f)
		{
			timer -= Time.deltaTime;
			yield return null;
			if (!swimming)
			{
				break;
			}
		}
		landedInWater = false;
	}

	private IEnumerator replaceHandPlaceableDelay()
	{
		yield return new WaitForSeconds(0.2f);
		myEquip.placeHandPlaceable();
		myInteract.refreshSelection = true;
	}

	public void forceNoStandingOn()
	{
		if (standingOn != 0)
		{
			CmdChangeStandingOn(0u);
			networkTransform.enabled = false;
			NetworkstandingOn = 0u;
			passenger = null;
			base.transform.SetParent(null);
			parentTrans = null;
		}
	}

	[TargetRpc]
	public void TargetKick(NetworkConnection conn)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendTargetRPCInternal(conn, typeof(CharMovement), "TargetKick", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	static CharMovement()
	{
		jumpWait = new WaitForFixedUpdate();
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdChangeTalkTo", InvokeUserCode_CmdChangeTalkTo, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestInterior", InvokeUserCode_CmdRequestInterior, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestHouseInterior", InvokeUserCode_CmdRequestHouseInterior, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestHouseExterior", InvokeUserCode_CmdRequestHouseExterior, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdDonateItemToMuseum", InvokeUserCode_CmdDonateItemToMuseum, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestShopStatus", InvokeUserCode_CmdRequestShopStatus, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestMuseumInterior", InvokeUserCode_CmdRequestMuseumInterior, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdChangeUnderWater", InvokeUserCode_CmdChangeUnderWater, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdNPCStartFollow", InvokeUserCode_CmdNPCStartFollow, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestMapChunk", InvokeUserCode_CmdRequestMapChunk, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestItemOnTopForChunk", InvokeUserCode_CmdRequestItemOnTopForChunk, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSendChatMessage", InvokeUserCode_CmdSendChatMessage, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSendEmote", InvokeUserCode_CmdSendEmote, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdDealDamage", InvokeUserCode_CmdDealDamage, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdTakeDamage", InvokeUserCode_CmdTakeDamage, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdCloseChest", InvokeUserCode_CmdCloseChest, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestOnTileStatus", InvokeUserCode_CmdRequestOnTileStatus, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestTileRotation", InvokeUserCode_CmdRequestTileRotation, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdReviveSelf", InvokeUserCode_CmdReviveSelf, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdCatchBug", InvokeUserCode_CmdCatchBug, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSetNewStamina", InvokeUserCode_CmdSetNewStamina, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdDropItem", InvokeUserCode_CmdDropItem, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdChangeStandingOn", InvokeUserCode_CmdChangeStandingOn, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdAgreeToCraftsmanCrafting", InvokeUserCode_CmdAgreeToCraftsmanCrafting, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdPlaceAnimalInCollectionPoint", InvokeUserCode_CmdPlaceAnimalInCollectionPoint, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSpawnAnimalBox", InvokeUserCode_CmdSpawnAnimalBox, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSellByWeight", InvokeUserCode_CmdSellByWeight, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdActivateTrap", InvokeUserCode_CmdActivateTrap, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSetOnFire", InvokeUserCode_CmdSetOnFire, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdBuyItemFromStall", InvokeUserCode_CmdBuyItemFromStall, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdCloseCamera", InvokeUserCode_CmdCloseCamera, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdPlaceMarkerOnMap", InvokeUserCode_CmdPlaceMarkerOnMap, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRemoveMyMarker", InvokeUserCode_CmdRemoveMyMarker, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdRequestNPCInv", InvokeUserCode_CmdRequestNPCInv, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdPayTownDebt", InvokeUserCode_CmdPayTownDebt, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdGetDeedIngredients", InvokeUserCode_CmdGetDeedIngredients, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdDonateDeedIngredients", InvokeUserCode_CmdDonateDeedIngredients, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdCharFaints", InvokeUserCode_CmdCharFaints, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdChangeBlocking", InvokeUserCode_CmdChangeBlocking, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdAcceptBulletinBoardPost", InvokeUserCode_CmdAcceptBulletinBoardPost, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdCompleteBulletinBoardPost", InvokeUserCode_CmdCompleteBulletinBoardPost, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSetDefenceBuff", InvokeUserCode_CmdSetDefenceBuff, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdSetHealthRegen", InvokeUserCode_CmdSetHealthRegen, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharMovement), "CmdTeleport", InvokeUserCode_CmdTeleport, true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcUpdateStandOn", InvokeUserCode_RpcUpdateStandOn);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcReleaseBug", InvokeUserCode_RpcReleaseBug);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcReleaseFish", InvokeUserCode_RpcReleaseFish);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcCloseCamera", InvokeUserCode_RpcCloseCamera);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcTakeKnockback", InvokeUserCode_RpcTakeKnockback);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcPlaceMarkerOnMap", InvokeUserCode_RpcPlaceMarkerOnMap);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcRemoveMarker", InvokeUserCode_RpcRemoveMarker);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcSetCharFaints", InvokeUserCode_RpcSetCharFaints);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcAcceptBulletinBoardPost", InvokeUserCode_RpcAcceptBulletinBoardPost);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcCompleteBulletinBoardPost", InvokeUserCode_RpcCompleteBulletinBoardPost);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "RpcTeleportChar", InvokeUserCode_RpcTeleportChar);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharMovement), "TargetKick", InvokeUserCode_TargetKick);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdChangeTalkTo(uint npcToTalkTo, bool isTalking)
	{
		if (isTalking)
		{
			NetworkIdentity.spawned[npcToTalkTo].GetComponent<NPCAI>().NetworktalkingTo = base.netId;
			isCurrentlyTalking = true;
		}
		else
		{
			NetworkIdentity.spawned[npcToTalkTo].GetComponent<NPCAI>().NetworktalkingTo = 0u;
			isCurrentlyTalking = false;
		}
	}

	protected static void InvokeUserCode_CmdChangeTalkTo(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeTalkTo called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdChangeTalkTo(reader.ReadUInt(), reader.ReadBool());
		}
	}

	protected void UserCode_CmdRequestInterior(int xPos, int yPos)
	{
		NetworkMapSharer.share.requestInterior(xPos, yPos);
	}

	protected static void InvokeUserCode_CmdRequestInterior(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestInterior called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestInterior(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRequestHouseInterior(int xPos, int yPos)
	{
		HouseDetails houseInfo = HouseManager.manage.getHouseInfo(xPos, yPos);
		NetworkMapSharer.share.TargetRequestHouse(base.connectionToClient, xPos, yPos, WorldManager.manageWorld.getHouseDetailsArray(houseInfo.houseMapOnTile), WorldManager.manageWorld.getHouseDetailsArray(houseInfo.houseMapOnTileStatus), WorldManager.manageWorld.getHouseDetailsArray(houseInfo.houseMapRotation), houseInfo.wall, houseInfo.floor, ItemOnTopManager.manage.getAllItemsOnTopInHouse(houseInfo));
	}

	protected static void InvokeUserCode_CmdRequestHouseInterior(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestHouseInterior called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestHouseInterior(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRequestHouseExterior(int xPos, int yPos)
	{
		HouseExterior houseExterior = HouseManager.manage.getHouseExterior(xPos, yPos);
		NetworkMapSharer.share.TargetRequestExterior(base.connectionToClient, xPos, yPos, houseExterior.houseBase, houseExterior.roof, houseExterior.windows, houseExterior.door, houseExterior.wallMat, houseExterior.wallColor, houseExterior.houseMat, houseExterior.houseColor, houseExterior.roofMat, houseExterior.roofColor);
	}

	protected static void InvokeUserCode_CmdRequestHouseExterior(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestHouseExterior called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestHouseExterior(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdDonateItemToMuseum(int itemId)
	{
		NetworkMapSharer.share.RpcAddToMuseum(itemId, myEquip.playerName);
	}

	protected static void InvokeUserCode_CmdDonateItemToMuseum(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDonateItemToMuseum called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdDonateItemToMuseum(reader.ReadInt());
		}
	}

	protected void UserCode_CmdRequestShopStatus()
	{
		NetworkMapSharer.share.TargetRequestShopStall(base.connectionToClient, ShopManager.manage.getBoolArrayForSync());
	}

	protected static void InvokeUserCode_CmdRequestShopStatus(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestShopStatus called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestShopStatus();
		}
	}

	protected void UserCode_CmdRequestMuseumInterior()
	{
		NetworkMapSharer.share.TargetRequestMuseum(base.connectionToClient, MuseumManager.manage.fishDonated, MuseumManager.manage.bugsDonated);
		StartCoroutine(NetworkMapSharer.share.sendPaintingsToClient(base.connectionToClient));
	}

	protected static void InvokeUserCode_CmdRequestMuseumInterior(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestMuseumInterior called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestMuseumInterior();
		}
	}

	protected void UserCode_CmdChangeUnderWater(bool newUnderWater)
	{
		NetworkunderWater = newUnderWater;
	}

	protected static void InvokeUserCode_CmdChangeUnderWater(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeUnderWater called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdChangeUnderWater(reader.ReadBool());
		}
	}

	protected void UserCode_CmdNPCStartFollow(uint tellNPCtoFollow, uint transformToFollow)
	{
		NetworkIdentity.spawned[tellNPCtoFollow].GetComponent<NPCAI>().NetworkfollowingNetId = transformToFollow;
		if (transformToFollow != 0)
		{
			followedBy = NetworkIdentity.spawned[tellNPCtoFollow].GetComponent<NPCAI>().myId.NPCNo;
		}
		else
		{
			followedBy = -1;
		}
	}

	protected static void InvokeUserCode_CmdNPCStartFollow(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdNPCStartFollow called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdNPCStartFollow(reader.ReadUInt(), reader.ReadUInt());
		}
	}

	protected void UserCode_CmdRequestMapChunk(int chunkPosX, int chunkPosY)
	{
		NetworkMapSharer.share.callRequest(base.connectionToClient, chunkPosX, chunkPosY);
	}

	protected static void InvokeUserCode_CmdRequestMapChunk(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestMapChunk called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestMapChunk(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRequestItemOnTopForChunk(int chunkX, int chunkY)
	{
		ItemOnTop[] itemsOnTopInChunk = WorldManager.manageWorld.getItemsOnTopInChunk(chunkX, chunkY);
		if (WorldManager.manageWorld.chunkHasItemsOnTop(chunkX, chunkY))
		{
			NetworkMapSharer.share.TargetGiveChunkOnTopDetails(base.connectionToClient, itemsOnTopInChunk);
		}
	}

	protected static void InvokeUserCode_CmdRequestItemOnTopForChunk(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestItemOnTopForChunk called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestItemOnTopForChunk(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSendChatMessage(string newMessage)
	{
		NetworkMapSharer.share.RpcMakeChatBubble(newMessage, base.netId);
	}

	protected static void InvokeUserCode_CmdSendChatMessage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendChatMessage called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSendChatMessage(reader.ReadString());
		}
	}

	protected void UserCode_CmdSendEmote(int newEmote)
	{
		NetworkMapSharer.share.RpcCharEmotes(newEmote, base.netId);
	}

	protected static void InvokeUserCode_CmdSendEmote(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendEmote called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSendEmote(reader.ReadInt());
		}
	}

	protected void UserCode_CmdDealDamage(uint netId, float multiplier)
	{
		Damageable component = NetworkIdentity.spawned[netId].GetComponent<Damageable>();
		if (Inventory.inv.allItems[myEquip.currentlyHoldingNo].weaponDamage * multiplier > 0f)
		{
			component.attackAndDoDamage(Mathf.RoundToInt(Inventory.inv.allItems[myEquip.currentlyHoldingNo].weaponDamage * multiplier), base.transform, Inventory.inv.allItems[myEquip.currentlyHoldingNo].weaponKnockback);
		}
		if ((bool)myEquip.currentlyHolding)
		{
			MeleeAttacks component2 = myEquip.currentlyHolding.itemPrefab.GetComponent<MeleeAttacks>();
			if ((bool)component2 && component2.myHitBox.checkForStun())
			{
				if (component2.myHitBox.stunWithLight)
				{
					component.stunWithLight();
				}
				else
				{
					component.stun();
				}
			}
		}
		if (component.health <= 0)
		{
			if ((bool)component.isAnAnimal() && component.health <= 0)
			{
				NetworkMapSharer.share.TargetGiveHuntingXp(base.connectionToClient, component.isAnAnimal().animalId, component.isAnAnimal().getVariationNo());
			}
		}
		else if (Inventory.inv.allItems[myEquip.currentlyHoldingNo].itemPrefab.GetComponent<MeleeAttacks>().myHitBox.fireDamage)
		{
			component.setOnFire();
		}
	}

	protected static void InvokeUserCode_CmdDealDamage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDealDamage called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdDealDamage(reader.ReadUInt(), reader.ReadFloat());
		}
	}

	protected void UserCode_CmdTakeDamage(int damageAmount)
	{
		GetComponent<Damageable>().attackAndDoDamage(damageAmount, base.transform);
	}

	protected static void InvokeUserCode_CmdTakeDamage(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTakeDamage called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdTakeDamage(reader.ReadInt());
		}
	}

	protected void UserCode_CmdCloseChest(int xPos, int yPos)
	{
		ContainerManager.manage.playerCloseChest(xPos, yPos, myInteract.insideHouseDetails);
	}

	protected static void InvokeUserCode_CmdCloseChest(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCloseChest called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdCloseChest(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRequestOnTileStatus(int xPos, int yPos)
	{
		NetworkMapSharer.share.RpcGiveOnTileStatus(WorldManager.manageWorld.onTileStatusMap[xPos, yPos], xPos, yPos);
	}

	protected static void InvokeUserCode_CmdRequestOnTileStatus(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestOnTileStatus called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestOnTileStatus(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRequestTileRotation(int xPos, int yPos)
	{
		NetworkMapSharer.share.TargetGetRotationForTile(base.connectionToClient, xPos, yPos, WorldManager.manageWorld.rotationMap[xPos, yPos]);
	}

	protected static void InvokeUserCode_CmdRequestTileRotation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestTileRotation called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestTileRotation(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdReviveSelf()
	{
		GetComponent<Damageable>().Networkhealth = 5;
		Networkstamina = 5;
		RpcSetCharFaints(false);
	}

	protected static void InvokeUserCode_CmdReviveSelf(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdReviveSelf called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdReviveSelf();
		}
	}

	protected void UserCode_CmdCatchBug(uint bugToCatch)
	{
		NetworkNavMesh.nav.UnSpawnAnAnimal(NetworkIdentity.spawned[bugToCatch].GetComponent<AnimalAI>(), false);
	}

	protected static void InvokeUserCode_CmdCatchBug(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCatchBug called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdCatchBug(reader.ReadUInt());
		}
	}

	protected void UserCode_RpcUpdateStandOn(uint standingOnForRPC)
	{
		updateStandingOn(standingOnForRPC);
	}

	protected static void InvokeUserCode_RpcUpdateStandOn(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateStandOn called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcUpdateStandOn(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdSetNewStamina(int newStam)
	{
		Networkstamina = newStam;
	}

	protected static void InvokeUserCode_CmdSetNewStamina(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetNewStamina called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSetNewStamina(reader.ReadInt());
		}
	}

	protected void UserCode_CmdDropItem(int itemId, int stackAmount, Vector3 dropPos, Vector3 desirePos)
	{
		if ((bool)Inventory.inv.allItems[itemId].bug)
		{
			RpcReleaseBug(itemId);
		}
		else if ((bool)Inventory.inv.allItems[itemId].fish)
		{
			RpcReleaseFish(itemId);
		}
		else
		{
			NetworkMapSharer.share.spawnAServerDrop(itemId, stackAmount, dropPos, desirePos, myInteract.insideHouseDetails);
		}
	}

	protected static void InvokeUserCode_CmdDropItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDropItem called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdDropItem(reader.ReadInt(), reader.ReadInt(), reader.ReadVector3(), reader.ReadVector3());
		}
	}

	protected void UserCode_RpcReleaseBug(int bugId)
	{
		Object.Instantiate(AnimalManager.manage.releasedBug, base.transform.position + base.transform.forward, base.transform.rotation).GetComponent<ReleaseBug>().setUpForBug(bugId);
	}

	protected static void InvokeUserCode_RpcReleaseBug(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReleaseBug called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcReleaseBug(reader.ReadInt());
		}
	}

	protected void UserCode_RpcReleaseFish(int fishId)
	{
		Object.Instantiate(AnimalManager.manage.releaseFish, base.transform.position + base.transform.forward, base.transform.rotation).GetComponent<ReleaseBug>().setUpForFish(fishId);
	}

	protected static void InvokeUserCode_RpcReleaseFish(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReleaseFish called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcReleaseFish(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeStandingOn(uint newStandOn)
	{
		NetworkstandingOn = newStandOn;
		RpcUpdateStandOn(standingOn);
	}

	protected static void InvokeUserCode_CmdChangeStandingOn(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeStandingOn called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdChangeStandingOn(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdAgreeToCraftsmanCrafting()
	{
		NetworkMapSharer.share.NetworkcraftsmanWorking = true;
	}

	protected static void InvokeUserCode_CmdAgreeToCraftsmanCrafting(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAgreeToCraftsmanCrafting called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdAgreeToCraftsmanCrafting();
		}
	}

	protected void UserCode_CmdPlaceAnimalInCollectionPoint(uint animalTrapPlaced)
	{
		PickUpAndCarry component = NetworkIdentity.spawned[animalTrapPlaced].GetComponent<PickUpAndCarry>();
		if ((bool)component)
		{
			TrappedAnimal component2 = NetworkIdentity.spawned[animalTrapPlaced].GetComponent<TrappedAnimal>();
			int rewardForCapturingAnimalIncludingBulletinBoards = BulletinBoard.board.getRewardForCapturingAnimalIncludingBulletinBoards(component2.trappedAnimalId, component2.trappedAnimalVariation);
			NetworkMapSharer.share.RpcDeliverAnimal(component.getLastCarriedBy(), component2.trappedAnimalId, component2.trappedAnimalVariation, rewardForCapturingAnimalIncludingBulletinBoards, Inventory.inv.getInvItemId(component2.trapItemDropAfterOpen));
			component.Networkdelivered = true;
		}
	}

	protected static void InvokeUserCode_CmdPlaceAnimalInCollectionPoint(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceAnimalInCollectionPoint called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdPlaceAnimalInCollectionPoint(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdSpawnAnimalBox(int animalId, int variation, string animalName)
	{
		GameObject gameObject = Object.Instantiate(FarmAnimalMenu.menu.animalBoxPrefab, FarmAnimalMenu.menu.spawnFarmAnimalPos.position, FarmAnimalMenu.menu.spawnFarmAnimalPos.rotation);
		gameObject.GetComponent<AnimalCarryBox>().setUp(animalId, variation, animalName);
		NetworkServer.Spawn(gameObject);
		StartCoroutine(moveBoxToPos(gameObject.GetComponent<PickUpAndCarry>()));
	}

	protected static void InvokeUserCode_CmdSpawnAnimalBox(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnAnimalBox called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSpawnAnimalBox(reader.ReadInt(), reader.ReadInt(), reader.ReadString());
		}
	}

	protected void UserCode_CmdSellByWeight(uint itemPlaced)
	{
		NetworkServer.Destroy(NetworkIdentity.spawned[itemPlaced].gameObject);
	}

	protected static void InvokeUserCode_CmdSellByWeight(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSellByWeight called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSellByWeight(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdActivateTrap(uint animalToTrapId, int xPos, int yPos)
	{
		AnimalAI component = NetworkIdentity.spawned[animalToTrapId].GetComponent<AnimalAI>();
		if ((bool)component && WorldManager.manageWorld.onTileMap[xPos, yPos] != -1)
		{
			GameObject original = NetworkMapSharer.share.trapObject;
			if (WorldManager.manageWorld.onTileMap[xPos, yPos] == 306)
			{
				original = NetworkMapSharer.share.stickTrapObject;
			}
			NetworkNavMesh.nav.UnSpawnAnAnimal(component, false);
			TrappedAnimal component2 = Object.Instantiate(original, new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2), Quaternion.identity).GetComponent<TrappedAnimal>();
			component2.NetworktrappedAnimalId = component.animalId;
			component2.NetworktrappedAnimalVariation = component.getVariationNo();
			NetworkServer.Spawn(component2.gameObject);
			NetworkMapSharer.share.RpcActivateTrap(xPos, yPos);
			WorldManager.manageWorld.onTileMap[xPos, yPos] = -1;
		}
	}

	protected static void InvokeUserCode_CmdActivateTrap(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdActivateTrap called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdActivateTrap(reader.ReadUInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSetOnFire(uint damageableId)
	{
		NetworkIdentity.spawned[damageableId].GetComponent<Damageable>().setOnFire();
	}

	protected static void InvokeUserCode_CmdSetOnFire(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetOnFire called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSetOnFire(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdBuyItemFromStall(int stallType, int shopStallNo)
	{
		NetworkMapSharer.share.RpcStallSold(stallType, shopStallNo);
	}

	protected static void InvokeUserCode_CmdBuyItemFromStall(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdBuyItemFromStall called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdBuyItemFromStall(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdCloseCamera()
	{
		RpcCloseCamera();
	}

	protected static void InvokeUserCode_CmdCloseCamera(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCloseCamera called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdCloseCamera();
		}
	}

	protected void UserCode_RpcCloseCamera()
	{
		if (!base.isLocalPlayer && (bool)myEquip.currentlyHolding && myEquip.currentlyHolding.itemName == "Camera")
		{
			myEquip.holdingPrefabAnimator.SetTrigger("CloseCamera");
		}
	}

	protected static void InvokeUserCode_RpcCloseCamera(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCloseCamera called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcCloseCamera();
		}
	}

	protected void UserCode_RpcTakeKnockback(Vector3 knockBackDir, float knockBackAmount)
	{
		if (base.isLocalPlayer && !beingKnockedBack)
		{
			StartCoroutine(knockBack(knockBackDir, knockBackAmount));
		}
	}

	protected static void InvokeUserCode_RpcTakeKnockback(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTakeKnockback called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcTakeKnockback(reader.ReadVector3(), reader.ReadFloat());
		}
	}

	protected void UserCode_CmdPlaceMarkerOnMap(Vector2 markPos, int iconId, int iconType)
	{
		RpcPlaceMarkerOnMap(base.netId, markPos, iconId, iconType);
	}

	protected static void InvokeUserCode_CmdPlaceMarkerOnMap(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceMarkerOnMap called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdPlaceMarkerOnMap(reader.ReadVector2(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcPlaceMarkerOnMap(uint placedBy, Vector2 markPos, int iconNo, int iconType)
	{
		RenderMap.map.createCustomMarker(base.netId, markPos, iconNo, (mapIcon.iconType)iconType);
	}

	protected static void InvokeUserCode_RpcPlaceMarkerOnMap(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaceMarkerOnMap called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcPlaceMarkerOnMap(reader.ReadUInt(), reader.ReadVector2(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdRemoveMyMarker()
	{
		RpcRemoveMarker(base.netId);
	}

	protected static void InvokeUserCode_CmdRemoveMyMarker(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRemoveMyMarker called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRemoveMyMarker();
		}
	}

	protected void UserCode_RpcRemoveMarker(uint removeMarkerOwner)
	{
		RenderMap.map.removeMarkerByPlayer(removeMarkerOwner);
	}

	protected static void InvokeUserCode_RpcRemoveMarker(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveMarker called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcRemoveMarker(reader.ReadUInt());
		}
	}

	protected void UserCode_CmdRequestNPCInv(int npcId)
	{
		NPCInventory nPCInventory = NPCManager.manage.npcInvs[npcId];
		NPCAI nPCAI = NPCManager.manage.returnLiveAgentWithNPCId(npcId);
		uint num = 0u;
		if ((bool)nPCAI)
		{
			num = nPCAI.netId;
		}
		NetworkMapSharer.share.RpcFillVillagerDetails(num, npcId, nPCInventory.isFem, nPCInventory.nameId, nPCInventory.skinId, nPCInventory.hairId, nPCInventory.hairColorId, nPCInventory.eyesId, nPCInventory.eyeColorId, nPCInventory.shirtId, nPCInventory.pantsId, nPCInventory.shoesId);
	}

	protected static void InvokeUserCode_CmdRequestNPCInv(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestNPCInv called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdRequestNPCInv(reader.ReadInt());
		}
	}

	protected void UserCode_CmdPayTownDebt(int payment)
	{
		TownManager.manage.payTownDebt(payment);
		NetworkMapSharer.share.RpcPayTownDebt(payment, base.netId);
	}

	protected static void InvokeUserCode_CmdPayTownDebt(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPayTownDebt called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdPayTownDebt(reader.ReadInt());
		}
	}

	protected void UserCode_CmdGetDeedIngredients(int deedX, int deedY)
	{
		NetworkMapSharer.share.TargetOpenBuildWindowForClient(base.connectionToClient, deedX, deedY, DeedManager.manage.getItemsAlreadyGivenForDeed(deedX, deedY));
	}

	protected static void InvokeUserCode_CmdGetDeedIngredients(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGetDeedIngredients called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdGetDeedIngredients(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdDonateDeedIngredients(int deedX, int deedY, int[] alreadyGiven)
	{
		NetworkMapSharer.share.RpcRefreshDeedIngredients(deedX, deedY, alreadyGiven);
	}

	protected static void InvokeUserCode_CmdDonateDeedIngredients(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDonateDeedIngredients called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdDonateDeedIngredients(reader.ReadInt(), reader.ReadInt(), GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_CmdCharFaints()
	{
		RpcSetCharFaints(true);
	}

	protected static void InvokeUserCode_CmdCharFaints(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCharFaints called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdCharFaints();
		}
	}

	protected void UserCode_RpcSetCharFaints(bool isFainted)
	{
		myAnim.SetBool("Fainted", isFainted);
		reviveBox.SetActive(isFainted);
	}

	protected static void InvokeUserCode_RpcSetCharFaints(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSetCharFaints called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcSetCharFaints(reader.ReadBool());
		}
	}

	protected void UserCode_CmdChangeBlocking(bool isBlocking)
	{
		myEquip.Networkblocking = isBlocking;
	}

	protected static void InvokeUserCode_CmdChangeBlocking(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeBlocking called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdChangeBlocking(reader.ReadBool());
		}
	}

	protected void UserCode_CmdAcceptBulletinBoardPost(int id)
	{
		RpcAcceptBulletinBoardPost(id);
	}

	protected static void InvokeUserCode_CmdAcceptBulletinBoardPost(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAcceptBulletinBoardPost called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdAcceptBulletinBoardPost(reader.ReadInt());
		}
	}

	protected void UserCode_RpcAcceptBulletinBoardPost(int id)
	{
		BulletinBoard.board.attachedPosts[id].acceptTask(this);
		BulletinBoard.board.showSelectedPost();
		BulletinBoard.board.updateTaskButtons();
	}

	protected static void InvokeUserCode_RpcAcceptBulletinBoardPost(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAcceptBulletinBoardPost called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcAcceptBulletinBoardPost(reader.ReadInt());
		}
	}

	protected void UserCode_CmdCompleteBulletinBoardPost(int id)
	{
		RpcCompleteBulletinBoardPost(id);
	}

	protected static void InvokeUserCode_CmdCompleteBulletinBoardPost(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCompleteBulletinBoardPost called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdCompleteBulletinBoardPost(reader.ReadInt());
		}
	}

	protected void UserCode_RpcCompleteBulletinBoardPost(int id)
	{
		BulletinBoard.board.attachedPosts[id].completeTask(this);
		BulletinBoard.board.showSelectedPost();
		BulletinBoard.board.updateTaskButtons();
	}

	protected static void InvokeUserCode_RpcCompleteBulletinBoardPost(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCompleteBulletinBoardPost called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcCompleteBulletinBoardPost(reader.ReadInt());
		}
	}

	protected void UserCode_CmdSetDefenceBuff(float newDefence)
	{
		GetComponent<Damageable>().defence = newDefence;
	}

	protected static void InvokeUserCode_CmdSetDefenceBuff(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetDefenceBuff called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSetDefenceBuff(reader.ReadFloat());
		}
	}

	protected void UserCode_CmdSetHealthRegen(float timer, int level)
	{
		GetComponent<Damageable>().startRegenAndSetTimer(timer, level);
	}

	protected static void InvokeUserCode_CmdSetHealthRegen(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetHealthRegen called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdSetHealthRegen(reader.ReadFloat(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdTeleport(string teledir)
	{
		int[] pos = new int[2];
		switch (teledir)
		{
		case "private":
			pos = new int[2]
			{
				(int)NetworkMapSharer.share.privateTowerPos.x,
				(int)NetworkMapSharer.share.privateTowerPos.y
			};
			break;
		case "north":
			pos = TownManager.manage.northTowerPos;
			break;
		case "east":
			pos = TownManager.manage.eastTowerPos;
			break;
		case "south":
			pos = TownManager.manage.southTowerPos;
			break;
		case "west":
			pos = TownManager.manage.westTowerPos;
			break;
		}
		RpcTeleportChar(pos);
	}

	protected static void InvokeUserCode_CmdTeleport(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTeleport called on client.");
		}
		else
		{
			((CharMovement)obj).UserCode_CmdTeleport(reader.ReadString());
		}
	}

	protected void UserCode_RpcTeleportChar(int[] pos)
	{
		StartCoroutine(teleportCharToPos(pos));
	}

	protected static void InvokeUserCode_RpcTeleportChar(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTeleportChar called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_RpcTeleportChar(GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_TargetKick(NetworkConnection conn)
	{
		CustomNetworkManager.manage.lobby.LeaveGameLobby();
		SaveLoad.saveOrLoad.returnToMenu();
	}

	protected static void InvokeUserCode_TargetKick(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetKick called on server.");
		}
		else
		{
			((CharMovement)obj).UserCode_TargetKick(NetworkClient.readyConnection);
		}
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteBool(underWater);
			writer.WriteInt(stamina);
			writer.WriteUInt(standingOn);
			writer.WriteInt(beingTargetedBy);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteBool(underWater);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(stamina);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteUInt(standingOn);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteInt(beingTargetedBy);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			bool flag = underWater;
			NetworkunderWater = reader.ReadBool();
			if (!SyncVarEqual(flag, ref underWater))
			{
				onChangeUnderWater(flag, underWater);
			}
			int num = stamina;
			Networkstamina = reader.ReadInt();
			if (!SyncVarEqual(num, ref stamina))
			{
				onChangeStamina(num, stamina);
			}
			uint num2 = standingOn;
			NetworkstandingOn = reader.ReadUInt();
			int num3 = beingTargetedBy;
			NetworkbeingTargetedBy = reader.ReadInt();
			return;
		}
		long num4 = (long)reader.ReadULong();
		if ((num4 & 1L) != 0L)
		{
			bool flag2 = underWater;
			NetworkunderWater = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref underWater))
			{
				onChangeUnderWater(flag2, underWater);
			}
		}
		if ((num4 & 2L) != 0L)
		{
			int num5 = stamina;
			Networkstamina = reader.ReadInt();
			if (!SyncVarEqual(num5, ref stamina))
			{
				onChangeStamina(num5, stamina);
			}
		}
		if ((num4 & 4L) != 0L)
		{
			uint num6 = standingOn;
			NetworkstandingOn = reader.ReadUInt();
		}
		if ((num4 & 8L) != 0L)
		{
			int num7 = beingTargetedBy;
			NetworkbeingTargetedBy = reader.ReadInt();
		}
	}
}
