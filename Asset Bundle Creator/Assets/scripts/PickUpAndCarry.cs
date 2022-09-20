using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using UnityEngine;

public class PickUpAndCarry : NetworkBehaviour
{
	[Header("Save Load Prefab ID----")]
	public int prefabId;

	[Header("Other Stuff----")]
	[SyncVar(hook = "onCarriedChanged")]
	public uint beingCarriedBy;

	[SyncVar(hook = "onDelivered")]
	public bool delivered;

	[SyncVar]
	public bool canBePickedUp = true;

	private uint lastCarriedBy;

	public Collider pickUpCollider;

	public float dropToPos = 2f;

	private Transform carriedByTransform;

	[SyncVar]
	private Vector3 vehicleLocalPos = Vector3.zero;

	private Rigidbody myRig;

	public ASound pickUpSound;

	public ASound putDownSound;

	private bool pickUpAnimPlayed;

	public LayerMask vehicleLayer;

	public AnimalAI isAnimal;

	public GameObject[] objectsEnabledOnDelevered;

	public Animator toTriggerFloating;

	private float floatSpeed;

	private Coroutine droppingRoutine;

	public bool isFragile;

	public Transform[] otherVehicleCheckPos;

	public bool moveToGroundLevelOnSpawn = true;

	public bool isAboveGround = true;

	public bool investigationItem;

	public ASound balloonFillSound;

	public uint NetworkbeingCarriedBy
	{
		get
		{
			return beingCarriedBy;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref beingCarriedBy))
			{
				uint old = beingCarriedBy;
				SetSyncVar(value, ref beingCarriedBy, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onCarriedChanged(old, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public bool Networkdelivered
	{
		get
		{
			return delivered;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref delivered))
			{
				bool old = delivered;
				SetSyncVar(value, ref delivered, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, true);
					onDelivered(old, value);
					setSyncVarHookGuard(2uL, false);
				}
			}
		}
	}

	public bool NetworkcanBePickedUp
	{
		get
		{
			return canBePickedUp;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref canBePickedUp))
			{
				bool flag = canBePickedUp;
				SetSyncVar(value, ref canBePickedUp, 4uL);
			}
		}
	}

	public Vector3 NetworkvehicleLocalPos
	{
		get
		{
			return vehicleLocalPos;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref vehicleLocalPos))
			{
				Vector3 vector = vehicleLocalPos;
				SetSyncVar(value, ref vehicleLocalPos, 8uL);
			}
		}
	}

	private void Start()
	{
		myRig = GetComponent<Rigidbody>();
		base.gameObject.AddComponent<InteractableObject>().isPickUpAndCarry = this;
		if (prefabId >= 0)
		{
			WorldManager.manageWorld.allCarriables.Add(this);
		}
	}

	public override void OnStartServer()
	{
		isAboveGround = !RealWorldTimeLight.time.underGround;
		moveToGroundLevelOnSpawn = false;
	}

	private void OnDestroy()
	{
		if (prefabId >= 0)
		{
			WorldManager.manageWorld.allCarriables.Remove(this);
		}
	}

	private void LateUpdate()
	{
		if (!base.isServer && base.transform.parent != null)
		{
			base.transform.localPosition = vehicleLocalPos;
		}
		if ((bool)carriedByTransform && pickUpAnimPlayed)
		{
			base.transform.position = carriedByTransform.position + Vector3.up - carriedByTransform.forward / 2f;
			base.transform.rotation = Quaternion.Euler(0f, carriedByTransform.eulerAngles.y, 0f);
		}
	}

	private void FixedUpdate()
	{
		if (!base.isServer)
		{
			return;
		}
		if (delivered)
		{
			myRig.isKinematic = true;
			myRig.MovePosition(base.transform.position + Vector3.up * floatSpeed * Time.fixedDeltaTime);
			floatSpeed = Mathf.Lerp(floatSpeed, 2.5f, Time.deltaTime);
			if (base.transform.position.y > 100f)
			{
				NetworkServer.Destroy(base.gameObject);
			}
		}
		else
		{
			if (beingCarriedBy != 0 || myRig.isKinematic)
			{
				return;
			}
			if (base.transform.position.y < dropToPos)
			{
				base.transform.position = new Vector3(base.transform.position.x, dropToPos, base.transform.position.z);
				myRig.isKinematic = true;
			}
			RaycastHit hitInfo;
			if (Physics.Raycast(base.transform.position + Vector3.up / 2f, Vector3.down, out hitInfo, 1.3f, vehicleLayer))
			{
				base.transform.position = new Vector3(base.transform.position.x, hitInfo.point.y, base.transform.position.z);
				NetworkbeingCarriedBy = hitInfo.transform.GetComponent<VehicleHitBox>().connectedTo.netId;
				NetworkvehicleLocalPos = base.transform.localPosition;
				myRig.isKinematic = true;
			}
			if (beingCarriedBy != 0)
			{
				return;
			}
			for (int i = 0; i < otherVehicleCheckPos.Length; i++)
			{
				if (Physics.Raycast(otherVehicleCheckPos[i].position + Vector3.up / 2f, Vector3.down, out hitInfo, 1.3f, vehicleLayer))
				{
					base.transform.position = new Vector3(base.transform.position.x, hitInfo.point.y, base.transform.position.z);
					NetworkbeingCarriedBy = hitInfo.transform.GetComponent<VehicleHitBox>().connectedTo.netId;
					NetworkvehicleLocalPos = base.transform.localPosition;
					myRig.isKinematic = true;
					break;
				}
			}
		}
	}

	public void moveToNewDropPos(float newDropToPos)
	{
		dropToPos = newDropToPos;
		base.transform.position = new Vector3(base.transform.position.x, newDropToPos, base.transform.position.z);
	}

	public void fallFromDestroyedVehicle()
	{
		if (base.isServer)
		{
			int num = Mathf.RoundToInt(base.transform.position.x / 2f);
			int num2 = Mathf.RoundToInt(base.transform.position.z / 2f);
			if (WorldManager.manageWorld.isPositionOnMap(num, num2))
			{
				dropToPos = WorldManager.manageWorld.heightMap[num, num2];
			}
			else
			{
				dropToPos = -2f;
			}
			if (droppingRoutine == null)
			{
				droppingRoutine = StartCoroutine(throwDown());
			}
		}
	}

	private void onCarriedChanged(uint old, uint newCarriedBy)
	{
		lastCarriedBy = old;
		NetworkbeingCarriedBy = newCarriedBy;
		base.transform.parent = null;
		if (newCarriedBy == 0 && (bool)carriedByTransform)
		{
			if (base.isServer)
			{
				if (droppingRoutine == null)
				{
					droppingRoutine = StartCoroutine(throwDown());
				}
			}
			else
			{
				pickUpAnimPlayed = false;
			}
		}
		if (newCarriedBy != 0)
		{
			SoundManager.manage.playASoundAtPoint(pickUpSound, base.transform.position);
			EquipItemToChar component = NetworkIdentity.spawned[newCarriedBy].gameObject.GetComponent<EquipItemToChar>();
			if ((bool)component)
			{
				base.transform.parent = null;
				carriedByTransform = component.holdPos.transform;
				if (base.isServer)
				{
					StartCoroutine(pickUpDelay());
				}
				else
				{
					StartCoroutine(pickUpDelayClient());
				}
				isAboveGround = true;
			}
			else
			{
				base.transform.parent = NetworkIdentity.spawned[newCarriedBy].GetComponent<Vehicle>().myHitBox;
			}
		}
		else
		{
			SoundManager.manage.playASoundAtPoint(putDownSound, base.transform.position);
			carriedByTransform = null;
		}
		if (beingCarriedBy == 0)
		{
			if (base.transform.position.y <= -12f)
			{
				isAboveGround = true;
			}
			else
			{
				isAboveGround = !RealWorldTimeLight.time.underGround;
			}
		}
		serverOnChangeCarried(newCarriedBy);
	}

	private void serverOnChangeCarried(uint newStatus)
	{
		if (newStatus != 0 && base.transform.parent == null)
		{
			pickUpCollider.enabled = false;
		}
		else
		{
			pickUpCollider.enabled = true;
		}
		if (base.isServer)
		{
			if (newStatus == 0 || (bool)base.transform.parent)
			{
				myRig.isKinematic = false;
			}
			else
			{
				myRig.isKinematic = true;
			}
		}
	}

	public void dropAndPlaceAtPos(float dropPos)
	{
		dropToPos = dropPos;
		NetworkbeingCarriedBy = 0u;
	}

	public void dropAndPlaceAtDropPos(Vector3 dropSpotPos)
	{
		droppingRoutine = StartCoroutine(moveToDropPos(dropSpotPos));
		NetworkbeingCarriedBy = 0u;
		dropToPos = dropSpotPos.y;
	}

	private IEnumerator moveToDropPos(Vector3 dropPos)
	{
		yield return null;
		float throwTimer = 0f;
		bool damageWhenDropped = false;
		if (isFragile && dropPos.y <= base.transform.position.y && Mathf.Abs(base.transform.position.y - dropPos.y) >= 4f)
		{
			damageWhenDropped = true;
		}
		while (beingCarriedBy == 0 && !delivered)
		{
			throwTimer = Mathf.Clamp(throwTimer + Time.deltaTime, 0f, 0.25f);
			base.transform.position = Vector3.Lerp(base.transform.position, dropPos, throwTimer * 4f);
			Vector3 eulerAngles = base.transform.eulerAngles;
			eulerAngles.y = Mathf.Round(eulerAngles.y / 90f) * 90f;
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(eulerAngles), Time.deltaTime * 2f);
			if (base.isServer && damageWhenDropped && Vector3.Distance(base.transform.position, dropPos) < 0.25f && Mathf.Abs(base.transform.position.y - dropToPos) <= 0.1f)
			{
				GetComponent<Damageable>().attackAndDoDamage(10, base.transform);
			}
			yield return null;
		}
		droppingRoutine = null;
	}

	private IEnumerator rotateToClosest90()
	{
		Quaternion desiredRot = Quaternion.Euler(0f, Mathf.Round(base.transform.rotation.y / 90f) * 90f, 0f);
		while (Quaternion.Angle(base.transform.rotation, desiredRot) > 5f)
		{
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, desiredRot, Time.deltaTime * 2f);
			yield return null;
		}
	}

	private IEnumerator throwDown()
	{
		pickUpAnimPlayed = false;
		float throwTimer = 0f;
		Vector3 throwTo = base.transform.position;
		if ((bool)carriedByTransform)
		{
			throwTo += carriedByTransform.forward * 2.5f;
		}
		float startingYPos = base.transform.position.y;
		while (beingCarriedBy == 0 && throwTimer < 0.25f)
		{
			throwTimer += Time.deltaTime;
			base.transform.position = Vector3.Lerp(base.transform.position, new Vector3(throwTo.x, base.transform.position.y, throwTo.z), throwTimer * 4f);
			if (base.isServer && isFragile && Mathf.Abs(startingYPos - base.transform.position.y) >= 4f && Mathf.Abs(base.transform.position.y - dropToPos) <= 0.1f)
			{
				GetComponent<Damageable>().attackAndDoDamage(10, base.transform);
			}
			yield return null;
		}
		while (isFragile && beingCarriedBy == 0)
		{
			if (base.isServer && isFragile && Mathf.Abs(startingYPos - base.transform.position.y) >= 4f && Mathf.Abs(base.transform.position.y - dropToPos) <= 0.1f)
			{
				GetComponent<Damageable>().attackAndDoDamage(10, base.transform);
			}
			yield return null;
		}
		if (base.isServer && (bool)isAnimal)
		{
			isAnimal.myAgent.Warp(base.transform.position);
			isAnimal.myAgent.transform.position = base.transform.position;
		}
		droppingRoutine = null;
	}

	public void onDelivered(bool old, bool newDelivered)
	{
		Networkdelivered = newDelivered;
		if (delivered)
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			GameObject[] array = objectsEnabledOnDelevered;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(true);
			}
			if ((bool)toTriggerFloating && !GetComponent<AnimalCarryBox>())
			{
				toTriggerFloating.SetTrigger("Floating");
			}
			SoundManager.manage.playASoundAtPoint(balloonFillSound, base.transform.position);
		}
	}

	private IEnumerator pickUpDelay()
	{
		float throwTimer = 0f;
		while (beingCarriedBy == 0 && throwTimer < 0.25f)
		{
			throwTimer += Time.deltaTime * 2f;
			Vector3 b = carriedByTransform.position + Vector3.up - carriedByTransform.forward / 2f;
			base.transform.position = Vector3.Lerp(base.transform.position, b, throwTimer * 4f);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(0f, carriedByTransform.root.eulerAngles.y, 0f), throwTimer * 4f);
			yield return null;
		}
		pickUpAnimPlayed = true;
	}

	private IEnumerator pickUpDelayClient()
	{
		uint holding = beingCarriedBy;
		float throwTimer = 0f;
		while (holding == beingCarriedBy && throwTimer < 0.25f)
		{
			throwTimer += Time.deltaTime * 2f;
			yield return null;
		}
		if (holding == beingCarriedBy)
		{
			pickUpAnimPlayed = true;
		}
	}

	public uint getLastCarriedBy()
	{
		return lastCarriedBy;
	}

	private void MirrorProcessed()
	{
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteUInt(beingCarriedBy);
			writer.WriteBool(delivered);
			writer.WriteBool(canBePickedUp);
			writer.WriteVector3(vehicleLocalPos);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteUInt(beingCarriedBy);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(delivered);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(canBePickedUp);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteVector3(vehicleLocalPos);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			uint num = beingCarriedBy;
			NetworkbeingCarriedBy = reader.ReadUInt();
			if (!SyncVarEqual(num, ref beingCarriedBy))
			{
				onCarriedChanged(num, beingCarriedBy);
			}
			bool flag = delivered;
			Networkdelivered = reader.ReadBool();
			if (!SyncVarEqual(flag, ref delivered))
			{
				onDelivered(flag, delivered);
			}
			bool flag2 = canBePickedUp;
			NetworkcanBePickedUp = reader.ReadBool();
			Vector3 vector = vehicleLocalPos;
			NetworkvehicleLocalPos = reader.ReadVector3();
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			uint num3 = beingCarriedBy;
			NetworkbeingCarriedBy = reader.ReadUInt();
			if (!SyncVarEqual(num3, ref beingCarriedBy))
			{
				onCarriedChanged(num3, beingCarriedBy);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			bool flag3 = delivered;
			Networkdelivered = reader.ReadBool();
			if (!SyncVarEqual(flag3, ref delivered))
			{
				onDelivered(flag3, delivered);
			}
		}
		if ((num2 & 4L) != 0L)
		{
			bool flag4 = canBePickedUp;
			NetworkcanBePickedUp = reader.ReadBool();
		}
		if ((num2 & 8L) != 0L)
		{
			Vector3 vector2 = vehicleLocalPos;
			NetworkvehicleLocalPos = reader.ReadVector3();
		}
	}
}
