using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class Vehicle : NetworkBehaviour
{
	public enum colourVariations
	{
		Bases = 0,
		Black = 1,
		Blue = 2,
		Green = 3,
		Orange = 4,
		Pink = 5,
		Purple = 6,
		Red = 7,
		White = 8,
		Yellow = 9,
		Chrome = 10,
		Gold = 11
	}

	public int saveId;

	public Transform driversPos;

	public BoxCollider driverBoxCollider;

	public Transform myHitBox;

	public Transform hitBoxFollow;

	[SyncVar(hook = "onDriverChange")]
	private uint driver;

	private Transform driverTrans;

	public Rigidbody myRig;

	private bool beingDestroyed;

	[Header("Driver animation stuff")]
	public string driverSittingAnimationBoolName = "Sitting";

	public Transform rightHandle;

	public Transform leftHandle;

	public Transform leftFoot;

	public Transform rightFoot;

	public Transform lookAtPos;

	public VehicleMakeParticles vehicleAnimator;

	public ASound startupSound;

	public Sprite mapIconSprite;

	public Damageable damageWhenUnderWater;

	public int requiresLicenceLevel = 1;

	public bool animateCharAsWell;

	[Header("Vehicle Colours")]
	public bool canBePainted = true;

	[SyncVar(hook = "onColourChange")]
	private int colourVaration;

	public MeshRenderer[] meshToChangeColours;

	public Color defaultTint;

	public MeshRenderer[] meshRenderersToTintColours;

	private Material tintMat;

	[Header("Stuff to disable when its an animal you are riding")]
	public AnimalAI myAi;

	public AnimateAnimalAI myAnimalAnimations;

	private mapIcon myMapIcon;

	private Vector3 folVelocity;

	private Vector3 rotVelocity;

	public bool mountingAnimationComplete;

	public uint Networkdriver
	{
		get
		{
			return driver;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref driver))
			{
				uint old = driver;
				SetSyncVar(value, ref driver, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onDriverChange(old, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public int NetworkcolourVaration
	{
		get
		{
			return colourVaration;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref colourVaration))
			{
				int oldColour = colourVaration;
				SetSyncVar(value, ref colourVaration, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, true);
					onColourChange(oldColour, value);
					setSyncVarHookGuard(2uL, false);
				}
			}
		}
	}

	public void setVariation(int newVariation)
	{
		NetworkcolourVaration = newVariation;
	}

	public int getVariation()
	{
		return colourVaration;
	}

	public override void OnStartServer()
	{
		Networkdriver = 0u;
		int num = (int)base.transform.position.x / 2;
		int num2 = (int)base.transform.position.z / 2;
		if (WorldManager.manageWorld.isPositionOnMap(num, num2))
		{
			if (WorldManager.manageWorld.waterMap[num, num2] && base.transform.position.y < 0.6f)
			{
				myRig.velocity.Set(0f, 0f, 0f);
				base.transform.position = new Vector3(base.transform.position.x, 0.6f, base.transform.position.z);
			}
			else if (base.transform.position.y <= (float)(WorldManager.manageWorld.heightMap[num, num2] - 1))
			{
				myRig.velocity.Set(0f, 0f, 0f);
				base.transform.position = new Vector3(base.transform.position.x, WorldManager.manageWorld.heightMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2], base.transform.position.z);
			}
		}
		else if (base.transform.position.y < 0.6f)
		{
			myRig.velocity.Set(0f, 0f, 0f);
			base.transform.position = new Vector3(base.transform.position.x, 0.6f, base.transform.position.z);
		}
		myHitBox.gameObject.SetActive(true);
	}

	public void destroyServerSelf()
	{
		if (saveId >= 0)
		{
			SaveLoad.saveOrLoad.vehiclesToSave.Remove(this);
			NetworkServer.Destroy(base.gameObject);
		}
	}

	public override void OnStopClient()
	{
		if (base.isServer)
		{
			for (int i = 0; i < myHitBox.childCount; i++)
			{
				PickUpAndCarry component = myHitBox.GetChild(i).GetComponent<PickUpAndCarry>();
				if ((bool)component)
				{
					component.NetworkbeingCarriedBy = 0u;
					component.fallFromDestroyedVehicle();
				}
			}
			myHitBox.gameObject.SetActive(false);
		}
		myHitBox.DetachChildren();
		if ((bool)driverTrans)
		{
			driverTrans.GetComponent<EquipItemToChar>().setVehicleHands(this);
			if (driverSittingAnimationBoolName != "")
			{
				driverTrans.GetComponent<Animator>().SetBool(driverSittingAnimationBoolName, true);
			}
		}
		if (driver != 0)
		{
			NetworkIdentity.spawned[driver].GetComponent<CharPickUp>().RpcStopDrivingFromServer();
			onDriverChange(driver, 0u);
		}
	}

	public override void OnStartClient()
	{
		onDriverChange(driver, driver);
		onColourChange(colourVaration, colourVaration);
		myHitBox.parent = null;
	}

	private void Start()
	{
		myRig = GetComponent<Rigidbody>();
		vehicleAnimator = GetComponent<VehicleMakeParticles>();
		base.gameObject.AddComponent<InteractableObject>().isVehicle = this;
	}

	public override void OnStartAuthority()
	{
		if (Vector3.Distance(CameraController.control.transform.position, base.transform.position) <= (float)(NewChunkLoader.loader.getChunkDistance() * WorldManager.manageWorld.getChunkSize()))
		{
			GetComponent<Rigidbody>().isKinematic = false;
			if (driver == 0)
			{
				StartCoroutine(noDriverTimer());
			}
		}
		else
		{
			GetComponent<Rigidbody>().isKinematic = true;
		}
	}

	public override void OnStopAuthority()
	{
		GetComponent<NetworkTransform>().resetObjectsInterpolation();
		GetComponent<Rigidbody>().isKinematic = false;
	}

	private void OnEnable()
	{
		if (saveId >= 0)
		{
			if (!SaveLoad.saveOrLoad.vehiclesToSave.Contains(this))
			{
				SaveLoad.saveOrLoad.vehiclesToSave.Add(this);
			}
			myMapIcon = RenderMap.map.createMapIconForVehicle(base.transform, saveId);
		}
		myHitBox.gameObject.SetActive(true);
	}

	public void OnDisable()
	{
		if (beingDestroyed)
		{
			SaveLoad.saveOrLoad.vehiclesToSave.Remove(this);
		}
		if ((bool)myMapIcon)
		{
			Object.Destroy(myMapIcon.gameObject);
		}
		myHitBox.gameObject.SetActive(false);
	}

	public bool currentlyHasDriver()
	{
		if (driver == 0)
		{
			return false;
		}
		return true;
	}

	private void LateUpdate()
	{
		if (!base.hasAuthority)
		{
			myHitBox.position = Vector3.SmoothDamp(myHitBox.position, hitBoxFollow.position, ref folVelocity, 0.05f);
			myHitBox.rotation = Quaternion.Lerp(myHitBox.rotation, hitBoxFollow.rotation, Time.deltaTime * 20f);
		}
	}

	private void FixedUpdate()
	{
		if ((bool)driverTrans && !base.hasAuthority && mountingAnimationComplete)
		{
			driverTrans.position = driversPos.position;
			driverTrans.rotation = driversPos.rotation;
		}
		if (base.hasAuthority)
		{
			myHitBox.position = hitBoxFollow.position;
			myHitBox.rotation = hitBoxFollow.rotation;
		}
	}

	private void onDriverChange(uint old, uint newId)
	{
		Networkdriver = newId;
		if (newId == 0)
		{
			if ((bool)driverTrans)
			{
				CharMovement component = driverTrans.GetComponent<CharMovement>();
				if ((bool)component)
				{
					component.col.enabled = true;
					component.myEquip.stopVehicleHands();
					if (driverSittingAnimationBoolName != "")
					{
						component.myAnim.SetBool(driverSittingAnimationBoolName, false);
					}
				}
				if ((bool)vehicleAnimator)
				{
					vehicleAnimator.setCharacterAnimator(null);
				}
			}
			myRig.useGravity = true;
			mountingAnimationComplete = false;
			driverTrans = null;
			driverBoxCollider.gameObject.SetActive(true);
		}
		else
		{
			driverTrans = NetworkIdentity.spawned[newId].GetComponent<Transform>();
			if ((bool)driverTrans)
			{
				StartCoroutine(moveDriverToSeat(driverTrans, driverTrans.GetComponent<Animator>()));
				CharMovement component2 = driverTrans.GetComponent<CharMovement>();
				if ((bool)component2)
				{
					component2.col.enabled = false;
				}
			}
			if ((bool)startupSound)
			{
				SoundManager.manage.playASoundAtPoint(startupSound, base.transform.position);
			}
			driverBoxCollider.gameObject.SetActive(false);
		}
		changeRigidbodyOnDriverChange();
		checkForAnimalAnimationOnDriverChange();
	}

	public void changeRigidbodyOnDriverChange()
	{
		if (base.hasAuthority)
		{
			if (driver == 0)
			{
				StartCoroutine(noDriverTimer());
			}
			else
			{
				myRig.isKinematic = false;
			}
		}
		else
		{
			myRig.isKinematic = true;
		}
	}

	public void startDriving(uint driverId)
	{
		Networkdriver = driverId;
	}

	public void stopDriving()
	{
		Networkdriver = 0u;
		if ((bool)myAi && base.isServer)
		{
			GetComponent<NetworkIdentity>().RemoveClientAuthority();
			GetComponent<NetworkIdentity>().AssignClientAuthority(NetworkMapSharer.share.localChar.connectionToClient);
		}
	}

	public void checkForAnimalAnimationOnDriverChange()
	{
		if (!myAi)
		{
			return;
		}
		if (driver == 0)
		{
			myRig.constraints = RigidbodyConstraints.FreezeAll;
			myAnimalAnimations.enabled = true;
			vehicleAnimator.enabled = false;
		}
		else
		{
			myRig.constraints = RigidbodyConstraints.FreezeRotation;
			myAnimalAnimations.enabled = false;
			vehicleAnimator.enabled = true;
		}
		if ((bool)myAi && base.hasAuthority && !base.isServer)
		{
			myAi.enabled = false;
		}
		else
		{
			myAi.enabled = true;
		}
		if ((bool)myAi && base.isServer)
		{
			if (driver == 0)
			{
				myAi.enabled = true;
				myAi.forceSetUp();
				myAi.myAgent.transform.position = base.transform.position;
				myAi.myAgent.transform.rotation = base.transform.rotation;
				myAi.myAgent.gameObject.SetActive(true);
			}
			else
			{
				myAi.enabled = false;
				myAi.myAgent.gameObject.SetActive(false);
			}
		}
	}

	public bool hasDriver()
	{
		return driver != 0;
	}

	private IEnumerator moveDriverToSeat(Transform driverTransform, Animator driverAnim)
	{
		mountingAnimationComplete = false;
		uint driverId = driver;
		float timer = 0f;
		Vector3 originalPos = driverTransform.position;
		if (driverSittingAnimationBoolName != "")
		{
			driverAnim.SetTrigger("StartDriving");
		}
		while (driverId == driver && timer < 0.25f)
		{
			timer += Time.deltaTime;
			driverTransform.position = Vector3.Lerp(originalPos, driversPos.position, timer / 0.25f);
			driverTrans.rotation = Quaternion.Lerp(driverTrans.rotation, driversPos.rotation, timer / 0.25f);
			yield return null;
			if (driverSittingAnimationBoolName != "")
			{
				driverAnim.SetBool(driverSittingAnimationBoolName, true);
			}
		}
		driverTrans.GetComponent<EquipItemToChar>().setVehicleHands(this);
		if ((bool)vehicleAnimator)
		{
			vehicleAnimator.setCharacterAnimator(driverTrans.GetComponent<Animator>());
		}
		mountingAnimationComplete = true;
	}

	private IEnumerator noDriverTimer()
	{
		if (base.hasAuthority)
		{
			float noDriverTimer = 0f;
			while (driver == 0 && noDriverTimer < 2f)
			{
				noDriverTimer += Time.deltaTime;
				yield return null;
			}
			if (driver == 0)
			{
				myRig.isKinematic = true;
			}
		}
	}

	public void onColourChange(int oldColour, int newColour)
	{
		NetworkcolourVaration = Mathf.Clamp(newColour, 0, EquipWindow.equip.vehicleColours.Length - 1);
		if (meshToChangeColours.Length != 0)
		{
			for (int i = 0; i < meshToChangeColours.Length; i++)
			{
				meshToChangeColours[i].sharedMaterial = EquipWindow.equip.vehicleColours[colourVaration];
			}
		}
		if (meshRenderersToTintColours.Length == 0)
		{
			return;
		}
		if (!tintMat)
		{
			tintMat = Object.Instantiate(meshRenderersToTintColours[0].sharedMaterial);
			for (int j = 0; j < meshRenderersToTintColours.Length; j++)
			{
				meshRenderersToTintColours[j].sharedMaterial = tintMat;
			}
		}
		if (colourVaration == 0)
		{
			tintMat.color = defaultTint;
			tintMat.SetFloat("_Glossiness", 0f);
			tintMat.SetFloat("_Metallic", 0f);
		}
		else
		{
			tintMat.color = EquipWindow.equip.vehicleColoursUI[colourVaration];
			tintMat.SetFloat("_Glossiness", EquipWindow.equip.vehicleColours[colourVaration].GetFloat("_Glossiness"));
			tintMat.SetFloat("_Metallic", EquipWindow.equip.vehicleColours[colourVaration].GetFloat("_Metallic"));
		}
	}

	private void MirrorProcessed()
	{
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteUInt(driver);
			writer.WriteInt(colourVaration);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteUInt(driver);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(colourVaration);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			uint num = driver;
			Networkdriver = reader.ReadUInt();
			if (!SyncVarEqual(num, ref driver))
			{
				onDriverChange(num, driver);
			}
			int num2 = colourVaration;
			NetworkcolourVaration = reader.ReadInt();
			if (!SyncVarEqual(num2, ref colourVaration))
			{
				onColourChange(num2, colourVaration);
			}
			return;
		}
		long num3 = (long)reader.ReadULong();
		if ((num3 & 1L) != 0L)
		{
			uint num4 = driver;
			Networkdriver = reader.ReadUInt();
			if (!SyncVarEqual(num4, ref driver))
			{
				onDriverChange(num4, driver);
			}
		}
		if ((num3 & 2L) != 0L)
		{
			int num5 = colourVaration;
			NetworkcolourVaration = reader.ReadInt();
			if (!SyncVarEqual(num5, ref colourVaration))
			{
				onColourChange(num5, colourVaration);
			}
		}
	}
}
