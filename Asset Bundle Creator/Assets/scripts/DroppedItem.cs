using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class DroppedItem : NetworkBehaviour
{
	public HouseDetails inside;

	[SyncVar(hook = "dropItem")]
	public int myItemId = -1;

	[SyncVar(hook = "onStackChange")]
	public int stackAmount;

	[SyncVar(hook = "onDesiredPosChange")]
	public Vector3 desiredPos = new Vector3(-1f, -1f, -1f);

	public Transform bounceAnimation;

	public Transform spawnHere;

	private GameObject itemPrefab;

	public GameObject bagDrop;

	public SpriteRenderer bagDropSprite;

	public Animator myAnim;

	public ASound plop;

	private bool fellinWater;

	public Transform[] otherDropPos;

	public Transform dropForBagPos;

	public GameObject dropForBagObject;

	private GameObject[] otherDropPrefabs = new GameObject[5];

	private GameObject spriteDrop;

	public bool saveDrop;

	[SyncVar]
	public int endOfDayTallyType = -1;

	public bool underground;

	private Coroutine droppingToPosRoutine;

	public Vector2 onTile = Vector2.zero;

	private Vector3 vel = Vector3.zero;

	public int NetworkmyItemId
	{
		get
		{
			return myItemId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref myItemId))
			{
				int oldItemId = myItemId;
				SetSyncVar(value, ref myItemId, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					dropItem(oldItemId, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public int NetworkstackAmount
	{
		get
		{
			return stackAmount;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref stackAmount))
			{
				int oldStack = stackAmount;
				SetSyncVar(value, ref stackAmount, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, true);
					onStackChange(oldStack, value);
					setSyncVarHookGuard(2uL, false);
				}
			}
		}
	}

	public Vector3 NetworkdesiredPos
	{
		get
		{
			return desiredPos;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref desiredPos))
			{
				Vector3 oldPos = desiredPos;
				SetSyncVar(value, ref desiredPos, 4uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4uL))
				{
					setSyncVarHookGuard(4uL, true);
					onDesiredPosChange(oldPos, value);
					setSyncVarHookGuard(4uL, false);
				}
			}
		}
	}

	public int NetworkendOfDayTallyType
	{
		get
		{
			return endOfDayTallyType;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref endOfDayTallyType))
			{
				int num = endOfDayTallyType;
				SetSyncVar(value, ref endOfDayTallyType, 8uL);
			}
		}
	}

	public void Start()
	{
		base.transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
	}

	public override void OnStartClient()
	{
		if (!base.isServer)
		{
			dropItem(myItemId, myItemId);
			onStackChange(stackAmount, stackAmount);
			onDesiredPosChange(desiredPos, desiredPos);
		}
		ParticleManager.manage.emitAttackParticle(base.transform.position, 0);
	}

	public override void OnStopClient()
	{
		if (!base.isServer || (base.isServer && underground == RealWorldTimeLight.time.underGround))
		{
			bounceAnimation.transform.parent = null;
			bounceAnimation.GetComponent<Animator>().enabled = true;
			ParticleManager.manage.emitPickupParticle(base.transform.position);
		}
	}

	public override void OnStartServer()
	{
		underground = RealWorldTimeLight.time.underGround;
	}

	public void onStackChange(int oldStack, int newStack)
	{
		NetworkstackAmount = newStack;
		if (myItemId != -1)
		{
			dropItem(myItemId, myItemId);
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.dropItem, base.transform.position);
			if (newStack > 5)
			{
				animateDropForBag();
			}
		}
	}

	private void onDesiredPosChange(Vector3 oldPos, Vector3 newDesiredPos)
	{
		NetworkdesiredPos = newDesiredPos;
		if (base.gameObject.activeSelf)
		{
			if (droppingToPosRoutine != null)
			{
				StopCoroutine(droppingToPosRoutine);
				droppingToPosRoutine = null;
			}
			droppingToPosRoutine = StartCoroutine(dropToPos());
		}
		if (inside == null)
		{
			onTile.x = Mathf.RoundToInt(newDesiredPos.x / 2f);
			onTile.y = Mathf.RoundToInt(newDesiredPos.z / 2f);
		}
		else
		{
			onTile.x = Mathf.RoundToInt(newDesiredPos.x / 2f);
			onTile.y = Mathf.RoundToInt(newDesiredPos.z / 2f);
		}
	}

	public void setDesiredPos(float height, float xPos, float yPos)
	{
		NetworkdesiredPos = new Vector3(xPos, height, yPos);
		int num = 0;
		int num2 = Mathf.RoundToInt(desiredPos.x / 2f) * 2;
		int num3 = Mathf.RoundToInt(desiredPos.z / 2f) * 2;
		while (WorldManager.manageWorld.checkIfDropIsTooCloseToEachOther(desiredPos) && num < 500)
		{
			desiredPos.x = Mathf.Clamp(desiredPos.x + Random.Range(-0.25f, 0.25f), num2 - 1, num2 + 1);
			desiredPos.z = Mathf.Clamp(desiredPos.z + Random.Range(-0.25f, 0.25f), num3 - 1, num3 + 1);
			num++;
		}
	}

	public void dropItem(int oldItemId, int newItemId)
	{
		NetworkmyItemId = newItemId;
		if (myItemId != -1)
		{
			if (Inventory.inv.allItems[newItemId].isATool || Inventory.inv.allItems[newItemId].hasFuel || (bool)Inventory.inv.allItems[newItemId].spawnPlaceable || Inventory.inv.allItems[newItemId].isFurniture)
			{
				saveDrop = true;
			}
			else
			{
				saveDrop = false;
			}
		}
		int num = Mathf.Clamp(stackAmount, 1, 5);
		if (myItemId <= -1)
		{
			return;
		}
		if (Inventory.inv.allItems[myItemId].hasFuel || Inventory.inv.allItems[myItemId].hasColourVariation)
		{
			num = 1;
		}
		if (stackAmount > 5 && !Inventory.inv.allItems[myItemId].hasFuel && !Inventory.inv.allItems[myItemId].hasColourVariation)
		{
			bagDrop.gameObject.SetActive(true);
			bagDropSprite.sprite = Inventory.inv.allItems[myItemId].getSprite();
			for (int i = 0; i < num; i++)
			{
				if (otherDropPrefabs[i] != null)
				{
					Object.Destroy(otherDropPrefabs[i]);
				}
			}
			return;
		}
		for (int j = 0; j < otherDropPrefabs.Length; j++)
		{
			if ((!Inventory.inv.allItems[myItemId].hasFuel && !Inventory.inv.allItems[myItemId].hasColourVariation && j <= stackAmount - 1) || (Inventory.inv.allItems[myItemId].hasFuel && j == 0) || (Inventory.inv.allItems[myItemId].hasColourVariation && j == 0))
			{
				if (otherDropPrefabs[j] == null)
				{
					if (((bool)Inventory.inv.allItems[myItemId].equipable && Inventory.inv.allItems[myItemId].equipable.cloths && Inventory.inv.allItems[myItemId].equipable.hat) || ((bool)Inventory.inv.allItems[myItemId].equipable && Inventory.inv.allItems[myItemId].equipable.cloths && Inventory.inv.allItems[myItemId].equipable.face))
					{
						otherDropPrefabs[j] = Object.Instantiate(EquipWindow.equip.holdingHatOrFaceObject, otherDropPos[j]);
						otherDropPrefabs[j].GetComponent<SpawnHatOrFaceInside>().setUpForObject(myItemId);
					}
					else if ((bool)Inventory.inv.allItems[myItemId].altDropPrefab)
					{
						otherDropPrefabs[j] = Object.Instantiate(Inventory.inv.allItems[myItemId].altDropPrefab, otherDropPos[j]);
					}
					else
					{
						otherDropPrefabs[j] = Object.Instantiate(Inventory.inv.allItems[myItemId].itemPrefab, otherDropPos[j]);
					}
					otherDropPos[j].parent.localPosition += new Vector3(Random.Range(-0.3f, 0.3f), 0f, Random.Range(-0.3f, 0.3f));
					otherDropPrefabs[j].transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
					otherDropPrefabs[j].transform.localPosition = Vector3.zero;
					SetItemTexture componentInChildren = otherDropPrefabs[j].GetComponentInChildren<SetItemTexture>();
					if ((bool)componentInChildren)
					{
						componentInChildren.setTexture(Inventory.inv.allItems[myItemId]);
						if ((bool)componentInChildren.changeSize)
						{
							componentInChildren.changeSizeOfTrans(Inventory.inv.allItems[myItemId].transform.localScale);
						}
					}
					if ((bool)otherDropPrefabs[j].GetComponent<Animator>())
					{
						Object.Destroy(otherDropPrefabs[j].GetComponent<Animator>());
					}
					otherDropPos[j].parent.GetComponent<Animator>().SetFloat("BounceSpeed", Random.Range(0.6f, 1.2f));
					otherDropPos[j].parent.GetComponent<Animator>().SetTrigger("SecondDropBounce");
				}
				else
				{
					otherDropPos[j].parent.GetComponent<Animator>().SetFloat("BounceSpeed", Random.Range(0.6f, 1.2f));
					otherDropPos[j].parent.GetComponent<Animator>().SetTrigger("Bounce");
				}
			}
			else if (otherDropPrefabs[j] != null)
			{
				Object.Destroy(otherDropPrefabs[j]);
			}
		}
	}

	public void pickUp()
	{
		WorldManager.manageWorld.itemsOnGround.Remove(this);
		Invoke("DestroyDelay", 0.12f);
	}

	public void DestroyDelay()
	{
		if (base.isServer)
		{
			NetworkServer.Destroy(base.gameObject);
		}
	}

	public void bury()
	{
		WorldManager.manageWorld.itemsOnGround.Remove(this);
		NetworkServer.Destroy(base.gameObject);
	}

	[ClientRpc]
	public void RpcMoveTowardsPickedUpBy(uint pickedUpBy)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(pickedUpBy);
		SendRPCInternal(typeof(DroppedItem), "RpcMoveTowardsPickedUpBy", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator moveTowardsPickedUp(Transform pickedUpBy)
	{
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			yield return null;
			base.transform.position = Vector3.Lerp(base.transform.position, pickedUpBy.position + Vector3.up, Time.deltaTime * 8f);
			base.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one / 10f, timer * 10f);
		}
		bounceAnimation.GetComponent<Animator>().enabled = true;
	}

	public void pickUpLocal()
	{
		GetComponent<Collider>().enabled = false;
		if (endOfDayTallyType != -1)
		{
			CharLevelManager.manage.addToDayTally(myItemId, stackAmount, endOfDayTallyType);
		}
	}

	private IEnumerator dropToPos()
	{
		myAnim.SetFloat("BounceSpeed", Random.Range(0.75f, 2.5f));
		float fallSpeedDif = Random.Range(15, 17);
		bool waterOnTile = true;
		if (WorldManager.manageWorld.isPositionOnMap((int)base.transform.position.x / 2, (int)base.transform.position.z / 2))
		{
			waterOnTile = WorldManager.manageWorld.waterMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2];
		}
		bool splashedThrough = false;
		if (base.transform.position.y < -0.2f)
		{
			splashedThrough = true;
			fellinWater = true;
		}
		Vector3 desiredXZ = new Vector3(desiredPos.x, base.transform.position.y, desiredPos.z);
		while (Vector3.Distance(base.transform.position, desiredXZ) > 0.15f)
		{
			base.transform.position = Vector3.SmoothDamp(base.transform.position, desiredXZ, ref vel, 0.1f);
			yield return null;
		}
		while (Vector3.Distance(base.transform.position, desiredPos) > 0.15f)
		{
			float y = Mathf.Clamp(base.transform.position.y - fallSpeedDif * Time.deltaTime, desiredPos.y, 125f);
			Vector3 position = new Vector3(desiredPos.x, y, desiredPos.z);
			base.transform.position = position;
			fallSpeedDif += Time.deltaTime * 5f;
			if (!splashedThrough && waterOnTile && base.transform.position.y <= 0.25f)
			{
				splashedThrough = true;
				ParticleManager.manage.waterSplash(base.transform.position);
			}
			if (!fellinWater && waterOnTile && base.transform.position.y < -0.2f)
			{
				ParticleManager.manage.waterSplash(base.transform.position);
				SoundManager.manage.playASoundAtPoint(plop, base.transform.position);
				fellinWater = true;
			}
			yield return null;
		}
		base.transform.position = desiredPos;
		GetComponent<Animator>().SetTrigger("Bounce");
		yield return new WaitForSeconds(Random.Range(0f, 0.15f));
		SoundManager.manage.playASoundAtPoint(SoundManager.manage.dropItem, base.transform.position);
		droppingToPosRoutine = null;
	}

	public void animateDropForBag()
	{
		if (!dropForBagObject)
		{
			dropForBagObject = Object.Instantiate(Inventory.inv.allItems[myItemId].itemPrefab, dropForBagPos);
			dropForBagObject.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
			dropForBagObject.transform.localPosition = Vector3.zero;
			ParticleManager.manage.emitPickupParticle(base.transform.position);
			SetItemTexture component = dropForBagObject.GetComponent<SetItemTexture>();
			if ((bool)component)
			{
				component.setTexture(Inventory.inv.allItems[myItemId]);
				if ((bool)component.changeSize)
				{
					component.changeSizeOfTrans(Inventory.inv.allItems[myItemId].transform.localScale);
				}
			}
			if ((bool)dropForBagObject.GetComponent<Animator>())
			{
				Object.Destroy(dropForBagObject.GetComponent<Animator>());
			}
		}
		bagDrop.GetComponent<Animator>().SetTrigger("ItemFallsInBag");
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcMoveTowardsPickedUpBy(uint pickedUpBy)
	{
		GetComponent<Collider>().enabled = false;
		StartCoroutine(moveTowardsPickedUp(NetworkIdentity.spawned[pickedUpBy].transform));
	}

	protected static void InvokeUserCode_RpcMoveTowardsPickedUpBy(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMoveTowardsPickedUpBy called on server.");
		}
		else
		{
			((DroppedItem)obj).UserCode_RpcMoveTowardsPickedUpBy(reader.ReadUInt());
		}
	}

	static DroppedItem()
	{
		RemoteCallHelper.RegisterRpcDelegate(typeof(DroppedItem), "RpcMoveTowardsPickedUpBy", InvokeUserCode_RpcMoveTowardsPickedUpBy);
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(myItemId);
			writer.WriteInt(stackAmount);
			writer.WriteVector3(desiredPos);
			writer.WriteInt(endOfDayTallyType);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(myItemId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteInt(stackAmount);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteVector3(desiredPos);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteInt(endOfDayTallyType);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = myItemId;
			NetworkmyItemId = reader.ReadInt();
			if (!SyncVarEqual(num, ref myItemId))
			{
				dropItem(num, myItemId);
			}
			int num2 = stackAmount;
			NetworkstackAmount = reader.ReadInt();
			if (!SyncVarEqual(num2, ref stackAmount))
			{
				onStackChange(num2, stackAmount);
			}
			Vector3 vector = desiredPos;
			NetworkdesiredPos = reader.ReadVector3();
			if (!SyncVarEqual(vector, ref desiredPos))
			{
				onDesiredPosChange(vector, desiredPos);
			}
			int num3 = endOfDayTallyType;
			NetworkendOfDayTallyType = reader.ReadInt();
			return;
		}
		long num4 = (long)reader.ReadULong();
		if ((num4 & 1L) != 0L)
		{
			int num5 = myItemId;
			NetworkmyItemId = reader.ReadInt();
			if (!SyncVarEqual(num5, ref myItemId))
			{
				dropItem(num5, myItemId);
			}
		}
		if ((num4 & 2L) != 0L)
		{
			int num6 = stackAmount;
			NetworkstackAmount = reader.ReadInt();
			if (!SyncVarEqual(num6, ref stackAmount))
			{
				onStackChange(num6, stackAmount);
			}
		}
		if ((num4 & 4L) != 0L)
		{
			Vector3 vector2 = desiredPos;
			NetworkdesiredPos = reader.ReadVector3();
			if (!SyncVarEqual(vector2, ref desiredPos))
			{
				onDesiredPosChange(vector2, desiredPos);
			}
		}
		if ((num4 & 8L) != 0L)
		{
			int num7 = endOfDayTallyType;
			NetworkendOfDayTallyType = reader.ReadInt();
		}
	}
}
