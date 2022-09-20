using System.Collections;
using System.Runtime.InteropServices;
using Mirror;
using Mirror.RemoteCalls;
using UnityChan;
using UnityEngine;

public class EquipItemToChar : NetworkBehaviour
{
	public InventoryItem currentlyHolding;

	private Animator myAnim;

	public Animator itemHolderAnim;

	public GameObject holdingPrefab;

	public Animator holdingPrefabAnimator;

	[SyncVar(hook = "equipNewItemNetwork")]
	public int currentlyHoldingNo = -1;

	private Transform leftHandPos;

	private Transform rightHandPos;

	public float lookingWeight;

	public Transform lookable;

	public Transform holdPos;

	public Transform rightHandToolHitPos;

	public Transform rightHandHoldPos;

	public SkinnedMeshRenderer skinRen;

	public SkinnedMeshRenderer shirtRen;

	public SkinnedMeshRenderer pantsRen;

	public SkinnedMeshRenderer shoeRen;

	public MeshRenderer noseRen;

	public Transform onHeadPosition;

	public EyesScript eyes;

	public InventoryItem dogWhistleItem;

	private GameObject itemOnHead;

	private GameObject hairOnHead;

	private GameObject itemOnFace;

	public NameTag myNameTag;

	[SyncVar(hook = "onChangeName")]
	public string playerName = "";

	[SyncVar]
	public bool usingItem;

	[SyncVar]
	public bool blocking;

	[SyncVar(hook = "onHairColourChange")]
	public int hairColor;

	[SyncVar(hook = "onFaceChange")]
	public int faceId = -1;

	[SyncVar(hook = "onHeadChange")]
	public int headId = -1;

	[SyncVar(hook = "onHairChange")]
	public int hairId = -1;

	[SyncVar(hook = "onChangeShirt")]
	public int shirtId = -1;

	[SyncVar(hook = "onChangePants")]
	public int pantsId = -1;

	[SyncVar(hook = "onChangeShoes")]
	public int shoeId = -1;

	[SyncVar(hook = "onChangeEyes")]
	public int eyeId;

	[SyncVar(hook = "onChangeEyeColor")]
	public int eyeColor;

	[SyncVar(hook = "onChangeSkin")]
	public int skinId = 1;

	[SyncVar(hook = "onNoseChange")]
	public int noseId = 1;

	[SyncVar(hook = "onMouthChange")]
	public int mouthId = 1;

	[SyncVar(hook = "changeOpenBag")]
	public bool bagOpenEmoteOn;

	private bool swimming;

	private bool doingEmote;

	private bool carrying;

	private bool driving;

	private bool petting;

	private bool whistling;

	private bool lookingAtMap;

	private bool lookingAtJournal;

	private bool crafting;

	private bool cooking;

	private bool layingDown;

	public GameObject carryingingOverHeadObject;

	public GameObject holdingMapPrefab;

	public GameObject craftingHammer;

	public GameObject cookingPan;

	public Conversation confirmDeedConvo;

	public Conversation confirmDeedNotServer;

	public bool nameHasBeenUpdated;

	public TileHighlighter highlighter;

	private int holdingToolAnimation;

	private int usingAnimation;

	private int usingStanceAnimation;

	public bool lookLock;

	private float leftHandWeight = 1f;

	private Vehicle inVehicle;

	private Transform leftFoot;

	private Transform rightFoot;

	private static Vector3 dif;

	private ToolDoesDamage useTool;

	private MeleeAttacks toolWeapon;

	private bool insidePlayerHouse;

	private bool inside;

	private Coroutine doingEmotion;

	private bool fishInHandPlaying;

	private bool bugInHandPlaying;

	private Vector3 aimLookablePos;

	public int NetworkcurrentlyHoldingNo
	{
		get
		{
			return currentlyHoldingNo;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref currentlyHoldingNo))
			{
				int oldItem = currentlyHoldingNo;
				SetSyncVar(value, ref currentlyHoldingNo, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					equipNewItemNetwork(oldItem, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public string NetworkplayerName
	{
		get
		{
			return playerName;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref playerName))
			{
				string oldName = playerName;
				SetSyncVar(value, ref playerName, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, true);
					onChangeName(oldName, value);
					setSyncVarHookGuard(2uL, false);
				}
			}
		}
	}

	public bool NetworkusingItem
	{
		get
		{
			return usingItem;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref usingItem))
			{
				bool flag = usingItem;
				SetSyncVar(value, ref usingItem, 4uL);
			}
		}
	}

	public bool Networkblocking
	{
		get
		{
			return blocking;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref blocking))
			{
				bool flag = blocking;
				SetSyncVar(value, ref blocking, 8uL);
			}
		}
	}

	public int NetworkhairColor
	{
		get
		{
			return hairColor;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref hairColor))
			{
				int oldColour = hairColor;
				SetSyncVar(value, ref hairColor, 16uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(16uL))
				{
					setSyncVarHookGuard(16uL, true);
					onHairColourChange(oldColour, value);
					setSyncVarHookGuard(16uL, false);
				}
			}
		}
	}

	public int NetworkfaceId
	{
		get
		{
			return faceId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref faceId))
			{
				int oldId = faceId;
				SetSyncVar(value, ref faceId, 32uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(32uL))
				{
					setSyncVarHookGuard(32uL, true);
					onFaceChange(oldId, value);
					setSyncVarHookGuard(32uL, false);
				}
			}
		}
	}

	public int NetworkheadId
	{
		get
		{
			return headId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref headId))
			{
				int oldId = headId;
				SetSyncVar(value, ref headId, 64uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(64uL))
				{
					setSyncVarHookGuard(64uL, true);
					onHeadChange(oldId, value);
					setSyncVarHookGuard(64uL, false);
				}
			}
		}
	}

	public int NetworkhairId
	{
		get
		{
			return hairId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref hairId))
			{
				int oldId = hairId;
				SetSyncVar(value, ref hairId, 128uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(128uL))
				{
					setSyncVarHookGuard(128uL, true);
					onHairChange(oldId, value);
					setSyncVarHookGuard(128uL, false);
				}
			}
		}
	}

	public int NetworkshirtId
	{
		get
		{
			return shirtId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref shirtId))
			{
				int oldId = shirtId;
				SetSyncVar(value, ref shirtId, 256uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(256uL))
				{
					setSyncVarHookGuard(256uL, true);
					onChangeShirt(oldId, value);
					setSyncVarHookGuard(256uL, false);
				}
			}
		}
	}

	public int NetworkpantsId
	{
		get
		{
			return pantsId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref pantsId))
			{
				int oldId = pantsId;
				SetSyncVar(value, ref pantsId, 512uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(512uL))
				{
					setSyncVarHookGuard(512uL, true);
					onChangePants(oldId, value);
					setSyncVarHookGuard(512uL, false);
				}
			}
		}
	}

	public int NetworkshoeId
	{
		get
		{
			return shoeId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref shoeId))
			{
				int oldId = shoeId;
				SetSyncVar(value, ref shoeId, 1024uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1024uL))
				{
					setSyncVarHookGuard(1024uL, true);
					onChangeShoes(oldId, value);
					setSyncVarHookGuard(1024uL, false);
				}
			}
		}
	}

	public int NetworkeyeId
	{
		get
		{
			return eyeId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref eyeId))
			{
				int oldId = eyeId;
				SetSyncVar(value, ref eyeId, 2048uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2048uL))
				{
					setSyncVarHookGuard(2048uL, true);
					onChangeEyes(oldId, value);
					setSyncVarHookGuard(2048uL, false);
				}
			}
		}
	}

	public int NetworkeyeColor
	{
		get
		{
			return eyeColor;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref eyeColor))
			{
				int oldId = eyeColor;
				SetSyncVar(value, ref eyeColor, 4096uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(4096uL))
				{
					setSyncVarHookGuard(4096uL, true);
					onChangeEyeColor(oldId, value);
					setSyncVarHookGuard(4096uL, false);
				}
			}
		}
	}

	public int NetworkskinId
	{
		get
		{
			return skinId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref skinId))
			{
				int oldSkin = skinId;
				SetSyncVar(value, ref skinId, 8192uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(8192uL))
				{
					setSyncVarHookGuard(8192uL, true);
					onChangeSkin(oldSkin, value);
					setSyncVarHookGuard(8192uL, false);
				}
			}
		}
	}

	public int NetworknoseId
	{
		get
		{
			return noseId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref noseId))
			{
				int oldNose = noseId;
				SetSyncVar(value, ref noseId, 16384uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(16384uL))
				{
					setSyncVarHookGuard(16384uL, true);
					onNoseChange(oldNose, value);
					setSyncVarHookGuard(16384uL, false);
				}
			}
		}
	}

	public int NetworkmouthId
	{
		get
		{
			return mouthId;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref mouthId))
			{
				int oldMouth = mouthId;
				SetSyncVar(value, ref mouthId, 32768uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(32768uL))
				{
					setSyncVarHookGuard(32768uL, true);
					onMouthChange(oldMouth, value);
					setSyncVarHookGuard(32768uL, false);
				}
			}
		}
	}

	public bool NetworkbagOpenEmoteOn
	{
		get
		{
			return bagOpenEmoteOn;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref bagOpenEmoteOn))
			{
				bool oldBagOpen = bagOpenEmoteOn;
				SetSyncVar(value, ref bagOpenEmoteOn, 65536uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(65536uL))
				{
					setSyncVarHookGuard(65536uL, true);
					changeOpenBag(oldBagOpen, value);
					setSyncVarHookGuard(65536uL, false);
				}
			}
		}
	}

	private void Awake()
	{
		myAnim = GetComponent<Animator>();
		holdingToolAnimation = Animator.StringToHash("HoldingTool");
		usingAnimation = Animator.StringToHash("Using");
		usingStanceAnimation = Animator.StringToHash("UsingStance");
	}

	public override void OnStartLocalPlayer()
	{
		Inventory.inv.localChar = this;
		CmdChangeSkin(Inventory.inv.skinTone);
		CmdChangeHairId(Inventory.inv.playerHair);
		CmdSendName(Inventory.inv.playerName);
		CmdSendEquipedClothes(EquipWindow.equip.getEquipSlotsArray());
		CmdChangeHairColour(Inventory.inv.playerHairColour);
		CmdChangeEyes(Inventory.inv.playerEyes, Inventory.inv.playerEyeColor);
		CmdChangeFaceId(EquipWindow.equip.faceSlot.itemNo);
		CmdChangeNose(Inventory.inv.nose);
		CmdChangeMouth(Inventory.inv.mouth);
		Inventory.inv.equipNewSelectedSlot();
	}

	public override void OnStartServer()
	{
		StartCoroutine(nameDelay());
	}

	private IEnumerator nameDelay()
	{
		while (!nameHasBeenUpdated)
		{
			yield return null;
		}
		if (!base.isLocalPlayer)
		{
			RpcCharacterJoinedPopup(playerName, NetworkMapSharer.share.islandName);
		}
	}

	[ClientRpc]
	private void RpcCharacterJoinedPopup(string newName, string sendIslandName)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(newName);
		writer.WriteString(sendIslandName);
		SendRPCInternal(typeof(EquipItemToChar), "RpcCharacterJoinedPopup", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private void setNameTagOnOff()
	{
		if (!base.isLocalPlayer)
		{
			if (OptionsMenu.options.nameTagsOn)
			{
				myNameTag.turnOn(playerName);
			}
			else
			{
				myNameTag.turnOff();
			}
		}
	}

	public override void OnStartClient()
	{
		equipNewItemNetwork(currentlyHoldingNo, currentlyHoldingNo);
		onChangeShirt(shirtId, shirtId);
		onChangePants(pantsId, pantsId);
		onChangeShoes(shoeId, shoeId);
		onHeadChange(headId, headId);
		onChangeEyes(eyeId, eyeId);
		onChangeEyeColor(eyeColor, eyeColor);
		onFaceChange(faceId, faceId);
		onHairChange(hairId, hairId);
		onChangeSkin(skinId, skinId);
		onNoseChange(noseId, noseId);
		onMouthChange(mouthId, mouthId);
		OptionsMenu.options.nameTagSwitch.AddListener(setNameTagOnOff);
	}

	private void Update()
	{
		if (!base.isLocalPlayer)
		{
			animateOnUse(usingItem, blocking);
		}
	}

	public void removeLeftHand()
	{
		leftHandWeight = 0f;
		leftHandPos = null;
	}

	public void attachLeftHand()
	{
		leftHandPos = holdingPrefab.transform.Find("Animation/LeftHandle");
		leftHandWeight = 1f;
	}

	public void onChangeName(string oldName, string newName)
	{
		nameHasBeenUpdated = true;
		NetworkplayerName = newName;
		if (!base.isLocalPlayer)
		{
			RenderMap.map.changeMapIconName(base.transform, newName);
			setNameTagOnOff();
		}
	}

	private void OnDestroy()
	{
		NotificationManager.manage.makeTopNotification(playerName + " has left");
	}

	public void setLookLock(bool isLocked)
	{
		lookLock = isLocked;
	}

	public void animateOnUse(bool beingUsed, bool blocking)
	{
		if ((bool)inVehicle)
		{
			return;
		}
		if ((bool)holdingPrefabAnimator || (currentlyHoldingNo < -1 && currentlyHoldingNo != -2))
		{
			holdingPrefabAnimator.SetBool(usingAnimation, beingUsed);
			if (beingUsed || lookLock)
			{
				if ((bool)currentlyHolding && currentlyHolding.hasUseAnimationStance && !currentlyHolding.consumeable)
				{
					myAnim.SetBool(usingStanceAnimation, true);
				}
				else
				{
					myAnim.SetBool(usingStanceAnimation, false);
				}
				float b = 0f;
				if ((bool)lookable && currentlyHoldingNo < -1)
				{
					b = 0.05f;
				}
				else if ((bool)lookable && (bool)currentlyHolding)
				{
					b = ((!currentlyHolding.placeable) ? 0.05f : 0.05f);
				}
				lookingWeight = Mathf.Lerp(lookingWeight, b, Time.deltaTime * 10f);
			}
			else
			{
				lookingWeight = Mathf.Lerp(lookingWeight, 0f, Time.deltaTime * 8f);
				myAnim.SetBool(usingStanceAnimation, false);
			}
		}
		else if ((bool)holdingPrefab)
		{
			myAnim.SetBool(usingAnimation, beingUsed);
			if (lookLock)
			{
				if (currentlyHolding.hasUseAnimationStance && !currentlyHolding.consumeable)
				{
					myAnim.SetBool(usingStanceAnimation, true);
				}
				else
				{
					myAnim.SetBool(usingStanceAnimation, false);
				}
			}
			else if (currentlyHolding.hasUseAnimationStance && !currentlyHolding.consumeable)
			{
				myAnim.SetBool(usingStanceAnimation, beingUsed);
			}
			else
			{
				myAnim.SetBool(usingStanceAnimation, false);
			}
		}
		else
		{
			lookingWeight = Mathf.Lerp(lookingWeight, 0f, Time.deltaTime * 10f);
			myAnim.SetBool(usingStanceAnimation, false);
		}
	}

	public void equipNewItem(int inventoryItemNo)
	{
		if (base.isLocalPlayer && !carrying)
		{
			myAnim.SetBool("CarryingItem", false);
		}
		Inventory.inv.checkQuickSlotDesc();
		setLeftHandWeight(1f);
		if (base.isLocalPlayer && inside)
		{
			if (insidePlayerHouse)
			{
				if ((inventoryItemNo <= -1 || !Inventory.inv.allItems[inventoryItemNo].isFurniture) && (inventoryItemNo <= -1 || !Inventory.inv.allItems[inventoryItemNo].itemChange) && (inventoryItemNo <= -1 || !Inventory.inv.allItems[inventoryItemNo].consumeable) && (inventoryItemNo <= -1 || !Inventory.inv.allItems[inventoryItemNo].canBePlacedInHouse) && (inventoryItemNo <= -1 || !Inventory.inv.allItems[inventoryItemNo].fish) && (inventoryItemNo <= -1 || !Inventory.inv.allItems[inventoryItemNo].bug) && (inventoryItemNo <= -1 || !Inventory.inv.allItems[inventoryItemNo].equipable || !Inventory.inv.allItems[inventoryItemNo].equipable.shirt) && (inventoryItemNo <= -1 || !Inventory.inv.allItems[inventoryItemNo].equipable || !Inventory.inv.allItems[inventoryItemNo].equipable.pants) && (inventoryItemNo <= -1 || !Inventory.inv.allItems[inventoryItemNo].equipable || !Inventory.inv.allItems[inventoryItemNo].equipable.hat) && (inventoryItemNo <= -1 || !Inventory.inv.allItems[inventoryItemNo].equipable || !Inventory.inv.allItems[inventoryItemNo].equipable.face) && (inventoryItemNo <= -1 || !Inventory.inv.allItems[inventoryItemNo].equipable || !Inventory.inv.allItems[inventoryItemNo].equipable.shoes) && (inventoryItemNo <= -1 || !Inventory.inv.allItems[inventoryItemNo].equipable || !Inventory.inv.allItems[inventoryItemNo].equipable.flooring) && (inventoryItemNo <= -1 || !Inventory.inv.allItems[inventoryItemNo].equipable || !Inventory.inv.allItems[inventoryItemNo].equipable.wallpaper))
				{
					inventoryItemNo = -1;
				}
			}
			else if (inventoryItemNo > -1 && !Inventory.inv.allItems[inventoryItemNo].canBeUsedInShops)
			{
				inventoryItemNo = -1;
			}
		}
		if (((base.isLocalPlayer && swimming) || (base.isLocalPlayer && doingEmote)) && (doingEmote || (swimming && inventoryItemNo > -1 && !Inventory.inv.allItems[inventoryItemNo].canUseUnderWater)))
		{
			inventoryItemNo = -1;
		}
		if (base.isLocalPlayer && carrying)
		{
			inventoryItemNo = -2;
		}
		if (base.isLocalPlayer && layingDown)
		{
			inventoryItemNo = -1;
		}
		if (base.isLocalPlayer && lookingAtMap)
		{
			inventoryItemNo = -3;
		}
		if (base.isLocalPlayer && lookingAtJournal)
		{
			inventoryItemNo = Inventory.inv.getInvItemId(GiftedItemWindow.gifted.journalItem);
		}
		if (base.isLocalPlayer && crafting)
		{
			inventoryItemNo = -4;
		}
		if (base.isLocalPlayer && cooking)
		{
			inventoryItemNo = -5;
		}
		if ((base.isLocalPlayer && driving) || (base.isLocalPlayer && petting))
		{
			inventoryItemNo = -1;
		}
		if (base.isLocalPlayer && whistling)
		{
			inventoryItemNo = Inventory.inv.getInvItemId(dogWhistleItem);
		}
		if (((bool)holdingPrefab && currentlyHoldingNo != inventoryItemNo) || inventoryItemNo < 0)
		{
			Object.Destroy(holdingPrefab);
			holdingPrefabAnimator = null;
			holdingPrefab = null;
			currentlyHolding = null;
		}
		if (inventoryItemNo <= -1)
		{
			myAnim.SetInteger(holdingToolAnimation, -1);
		}
		if (!currentlyHolding && inventoryItemNo != -1)
		{
			switch (inventoryItemNo)
			{
			case -5:
				if (holdingPrefab == null)
				{
					holdingPrefab = Object.Instantiate(cookingPan, holdPos);
					holdingPrefab.transform.localPosition = Vector3.zero;
				}
				break;
			case -4:
				if (holdingPrefab == null)
				{
					holdingPrefab = Object.Instantiate(craftingHammer, holdPos);
					holdingPrefab.transform.localPosition = Vector3.zero;
				}
				break;
			case -3:
				if (holdingPrefab == null)
				{
					holdingPrefab = Object.Instantiate(holdingMapPrefab, holdPos);
					holdingPrefab.transform.localPosition = Vector3.zero;
				}
				break;
			case -2:
				if (holdingPrefab == null)
				{
					myAnim.SetBool("CarryingItem", true);
				}
				break;
			default:
				currentlyHolding = Inventory.inv.allItems[inventoryItemNo];
				clearHandPlaceable();
				if (((bool)Inventory.inv.allItems[inventoryItemNo].equipable && Inventory.inv.allItems[inventoryItemNo].equipable.cloths && Inventory.inv.allItems[inventoryItemNo].equipable.hat) || ((bool)Inventory.inv.allItems[inventoryItemNo].equipable && Inventory.inv.allItems[inventoryItemNo].equipable.cloths && Inventory.inv.allItems[inventoryItemNo].equipable.face))
				{
					holdingPrefab = Object.Instantiate(EquipWindow.equip.holdingHatOrFaceObject, holdPos);
					holdingPrefab.GetComponent<SpawnHatOrFaceInside>().setUpForObject(inventoryItemNo);
				}
				else if (Inventory.inv.allItems[inventoryItemNo].useRightHandAnim)
				{
					holdingPrefab = Object.Instantiate(Inventory.inv.allItems[inventoryItemNo].itemPrefab, rightHandHoldPos);
					if (myAnim.GetInteger(holdingToolAnimation) != (int)Inventory.inv.allItems[inventoryItemNo].myAnimType)
					{
						myAnim.SetTrigger("ChangeItem");
					}
					myAnim.SetInteger(holdingToolAnimation, (int)Inventory.inv.allItems[inventoryItemNo].myAnimType);
					useTool = holdingPrefab.GetComponent<ToolDoesDamage>();
					toolWeapon = holdingPrefab.GetComponent<MeleeAttacks>();
				}
				else
				{
					holdingPrefab = Object.Instantiate(Inventory.inv.allItems[inventoryItemNo].itemPrefab, holdPos);
					myAnim.SetInteger(holdingToolAnimation, -1);
				}
				holdingPrefab.transform.localPosition = Vector3.zero;
				break;
			}
			if ((bool)holdingPrefab)
			{
				SetItemTexture componentInChildren = holdingPrefab.GetComponentInChildren<SetItemTexture>();
				if ((bool)componentInChildren)
				{
					componentInChildren.setTexture(Inventory.inv.allItems[inventoryItemNo]);
					if ((bool)componentInChildren.changeSize)
					{
						componentInChildren.changeSizeOfTrans(Inventory.inv.allItems[inventoryItemNo].transform.localScale);
					}
				}
				holdingPrefabAnimator = holdingPrefab.GetComponent<Animator>();
				leftHandPos = holdingPrefab.transform.Find("Animation/LeftHandle");
				rightHandPos = holdingPrefab.transform.Find("Animation/RightHandle");
				lookable = holdingPrefab.transform.Find("Animation/Lookable");
				if ((bool)currentlyHolding && !currentlyHolding.useRightHandAnim && (bool)holdingPrefabAnimator && currentlyHolding.isATool && (bool)leftHandPos && (bool)rightHandPos && !currentlyHolding.ignoreTwoArmAnim)
				{
					myAnim.SetBool("TwoArms", true);
				}
				else
				{
					myAnim.SetBool("TwoArms", false);
				}
			}
		}
		else
		{
			myAnim.SetBool("TwoArms", false);
		}
		NetworkcurrentlyHoldingNo = inventoryItemNo;
		highlighter.checkIfHidden(currentlyHolding);
		CmdEquipNewItem(inventoryItemNo);
	}

	public void placeHandPlaceable()
	{
		if (!currentlyHolding || ((!currentlyHolding.equipable || !currentlyHolding.equipable.cloths) && !currentlyHolding.fish && !currentlyHolding.bug) || currentlyHolding == EquipWindow.equip.minersHelmet || currentlyHolding == EquipWindow.equip.emptyMinersHelmet)
		{
			return;
		}
		if ((bool)currentlyHolding.fish)
		{
			if (currentlyHolding.transform.localScale.z >= 1.5f)
			{
				currentlyHolding.placeable = EquipWindow.equip.largeFishTank;
			}
			else
			{
				currentlyHolding.placeable = EquipWindow.equip.fishTank;
			}
		}
		else if ((bool)currentlyHolding.bug)
		{
			currentlyHolding.placeable = EquipWindow.equip.bugTank;
		}
		else if ((bool)currentlyHolding.equipable && currentlyHolding.equipable.shirt)
		{
			currentlyHolding.placeable = EquipWindow.equip.shirtPlaceable;
		}
		else if (((bool)currentlyHolding.equipable && currentlyHolding.equipable.hat) || ((bool)currentlyHolding.equipable && currentlyHolding.equipable.face))
		{
			currentlyHolding.placeable = EquipWindow.equip.hatPlaceable;
		}
		else if ((bool)currentlyHolding.equipable && currentlyHolding.equipable.pants)
		{
			currentlyHolding.placeable = EquipWindow.equip.pantsPlaceable;
		}
		else if ((bool)currentlyHolding.equipable && currentlyHolding.equipable.shoes)
		{
			currentlyHolding.placeable = EquipWindow.equip.shoePlaceable;
		}
	}

	public void clearHandPlaceable()
	{
		if ((bool)currentlyHolding && (((bool)currentlyHolding.equipable && currentlyHolding.equipable.cloths) || (bool)currentlyHolding.fish || (bool)currentlyHolding.bug))
		{
			currentlyHolding.placeable = null;
		}
	}

	public bool usesHandPlaceable()
	{
		if ((bool)currentlyHolding && ((bool)currentlyHolding.equipable || (bool)currentlyHolding.fish || (bool)currentlyHolding.bug))
		{
			return true;
		}
		return false;
	}

	public bool needsHandPlaceable()
	{
		if ((bool)currentlyHolding)
		{
			if ((bool)currentlyHolding.placeable)
			{
				return false;
			}
			if ((bool)currentlyHolding.equipable || (bool)currentlyHolding.fish || (bool)currentlyHolding.bug)
			{
				return true;
			}
		}
		return false;
	}

	public void equipNewItemNetwork(int oldItem, int inventoryItemNo)
	{
		if (base.isLocalPlayer)
		{
			return;
		}
		if (oldItem == -2 && inventoryItemNo != -2)
		{
			myAnim.SetBool("CarryingItem", false);
		}
		setLeftHandWeight(1f);
		if ((bool)holdingPrefab && oldItem != inventoryItemNo)
		{
			Object.Destroy(holdingPrefab);
			holdingPrefabAnimator = null;
			holdingPrefab = null;
			currentlyHolding = null;
		}
		if (inventoryItemNo <= -1)
		{
			myAnim.SetInteger(holdingToolAnimation, -1);
		}
		if (!currentlyHolding && inventoryItemNo != -1)
		{
			switch (inventoryItemNo)
			{
			case -5:
				if (holdingPrefab == null)
				{
					holdingPrefab = Object.Instantiate(cookingPan, holdPos);
					holdingPrefab.transform.localPosition = Vector3.zero;
				}
				break;
			case -4:
				if (holdingPrefab == null)
				{
					holdingPrefab = Object.Instantiate(craftingHammer, holdPos);
					holdingPrefab.transform.localPosition = Vector3.zero;
				}
				break;
			case -3:
				if (holdingPrefab == null)
				{
					holdingPrefab = Object.Instantiate(holdingMapPrefab, holdPos);
					holdingPrefab.transform.localPosition = Vector3.zero;
				}
				break;
			case -2:
				if (holdingPrefab == null)
				{
					myAnim.SetBool("CarryingItem", true);
				}
				break;
			default:
				currentlyHolding = Inventory.inv.allItems[inventoryItemNo];
				clearHandPlaceable();
				if (((bool)Inventory.inv.allItems[inventoryItemNo].equipable && Inventory.inv.allItems[inventoryItemNo].equipable.cloths && Inventory.inv.allItems[inventoryItemNo].equipable.hat) || ((bool)Inventory.inv.allItems[inventoryItemNo].equipable && Inventory.inv.allItems[inventoryItemNo].equipable.cloths && Inventory.inv.allItems[inventoryItemNo].equipable.face))
				{
					holdingPrefab = Object.Instantiate(EquipWindow.equip.holdingHatOrFaceObject, holdPos);
					holdingPrefab.GetComponent<SpawnHatOrFaceInside>().setUpForObject(inventoryItemNo);
				}
				else if (Inventory.inv.allItems[inventoryItemNo].useRightHandAnim)
				{
					holdingPrefab = Object.Instantiate(Inventory.inv.allItems[inventoryItemNo].itemPrefab, rightHandHoldPos);
					if (myAnim.GetInteger(holdingToolAnimation) != (int)Inventory.inv.allItems[inventoryItemNo].myAnimType)
					{
						myAnim.SetTrigger("ChangeItem");
					}
					myAnim.SetInteger(holdingToolAnimation, (int)Inventory.inv.allItems[inventoryItemNo].myAnimType);
					useTool = holdingPrefab.GetComponent<ToolDoesDamage>();
					toolWeapon = holdingPrefab.GetComponent<MeleeAttacks>();
				}
				else
				{
					holdingPrefab = Object.Instantiate(Inventory.inv.allItems[inventoryItemNo].itemPrefab, holdPos);
					myAnim.SetInteger(holdingToolAnimation, -1);
				}
				holdingPrefab.transform.localPosition = Vector3.zero;
				break;
			}
			if ((bool)holdingPrefab)
			{
				SetItemTexture componentInChildren = holdingPrefab.GetComponentInChildren<SetItemTexture>();
				if ((bool)componentInChildren)
				{
					componentInChildren.setTexture(Inventory.inv.allItems[inventoryItemNo]);
					if ((bool)componentInChildren.changeSize)
					{
						componentInChildren.changeSizeOfTrans(Inventory.inv.allItems[inventoryItemNo].transform.localScale);
					}
				}
				holdingPrefab.transform.localPosition = Vector3.zero;
				holdingPrefabAnimator = holdingPrefab.GetComponent<Animator>();
				leftHandPos = holdingPrefab.transform.Find("Animation/LeftHandle");
				rightHandPos = holdingPrefab.transform.Find("Animation/RightHandle");
				lookable = holdingPrefab.transform.Find("Animation/Lookable");
				if ((bool)currentlyHolding && !currentlyHolding.useRightHandAnim && (bool)holdingPrefabAnimator && currentlyHolding.isATool && (bool)leftHandPos && (bool)rightHandPos)
				{
					myAnim.SetBool("TwoArms", true);
				}
				else
				{
					myAnim.SetBool("TwoArms", false);
				}
			}
		}
		else
		{
			myAnim.SetBool("TwoArms", false);
		}
		NetworkcurrentlyHoldingNo = inventoryItemNo;
		if (!base.isLocalPlayer)
		{
			placeHandPlaceable();
		}
	}

	public void setLeftHandWeight(float newWeight)
	{
		leftHandWeight = 1f;
	}

	public bool isInVehicle()
	{
		return inVehicle;
	}

	public void setVehicleHands(Vehicle drivingVehicle)
	{
		rightHandPos = drivingVehicle.rightHandle;
		leftHandPos = drivingVehicle.leftHandle;
		leftFoot = drivingVehicle.leftFoot;
		rightFoot = drivingVehicle.rightFoot;
		myAnim.SetBool("TwoArms", true);
		if ((bool)drivingVehicle.leftHandle)
		{
			setLeftHandWeight(1f);
		}
		inVehicle = drivingVehicle;
		lookable = drivingVehicle.lookAtPos;
		lookingWeight = 1f;
	}

	public void stopVehicleHands()
	{
		inVehicle = null;
		lookingWeight = 0f;
		if (currentlyHolding == null)
		{
			rightHandPos = null;
			leftHandPos = null;
			leftFoot = null;
			rightFoot = null;
			lookable = null;
			lookingWeight = 0f;
			myAnim.SetBool("TwoArms", false);
		}
	}

	private void OnAnimatorIK()
	{
		if ((bool)rightHandPos)
		{
			myAnim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
			myAnim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
			myAnim.SetIKPosition(AvatarIKGoal.RightHand, rightHandPos.position + dif);
			myAnim.SetIKRotation(AvatarIKGoal.RightHand, rightHandPos.rotation);
		}
		if ((bool)leftHandPos && leftHandWeight > 0f)
		{
			myAnim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
			myAnim.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
			myAnim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPos.position + dif);
			myAnim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandPos.rotation);
		}
		if ((bool)lookable && (bool)inVehicle)
		{
			myAnim.SetLookAtPosition(lookable.position);
			myAnim.SetLookAtWeight(1f, 1f, 1f);
			if (!inVehicle.hasAuthority && inVehicle.mountingAnimationComplete)
			{
				base.transform.position = inVehicle.driversPos.position;
				base.transform.rotation = inVehicle.driversPos.rotation;
			}
		}
		if ((bool)inVehicle)
		{
			if ((bool)leftFoot)
			{
				myAnim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
				myAnim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
				myAnim.SetIKPosition(AvatarIKGoal.LeftFoot, leftFoot.position);
				myAnim.SetIKRotation(AvatarIKGoal.LeftFoot, leftFoot.rotation);
			}
			if ((bool)rightFoot)
			{
				myAnim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
				myAnim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
				myAnim.SetIKPosition(AvatarIKGoal.RightFoot, rightFoot.position);
				myAnim.SetIKRotation(AvatarIKGoal.RightFoot, rightFoot.rotation);
			}
		}
	}

	public bool isCarrying()
	{
		return carrying;
	}

	public void setCarrying(bool newCarrying)
	{
		if (newCarrying != carrying)
		{
			carrying = newCarrying;
			if (carrying)
			{
				equipNewItem(currentlyHoldingNo);
			}
			else
			{
				Inventory.inv.equipNewSelectedSlot();
			}
		}
	}

	public void setLayDown(bool newLayingDown)
	{
		if (newLayingDown != layingDown)
		{
			layingDown = newLayingDown;
			if (layingDown)
			{
				equipNewItem(currentlyHoldingNo);
			}
			else
			{
				Inventory.inv.equipNewSelectedSlot();
			}
		}
	}

	public bool getSwimming()
	{
		return swimming;
	}

	public void doDamageNow()
	{
		if ((bool)holdingPrefab && (bool)useTool)
		{
			useTool.doDamageNow();
		}
	}

	public void checkRefill()
	{
		if ((bool)holdingPrefab && (bool)useTool)
		{
			useTool.checkRefill();
		}
	}

	public void playToolParticles()
	{
		if ((bool)holdingPrefab)
		{
			holdingPrefab.GetComponent<ActivateAnimationParticles>().emitParticles(20);
		}
	}

	public void playToolSound()
	{
		if ((bool)holdingPrefab)
		{
			holdingPrefab.GetComponent<ActivateAnimationParticles>().playSound();
		}
	}

	public void lookLockFrames(int frame)
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.turnOnLookLockForFramesWithoutUsing(frame);
		}
	}

	public void startAttack()
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.attack();
		}
	}

	public void lockPosForFrames(int frames)
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.lockPosForFrames(frames);
		}
	}

	public void toolDoesDamageToolPosNo(int noToUse)
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.toolDoesDamageToolPosNo(noToUse);
		}
	}

	public void makeSwingSound()
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.playSwordSwingSound();
		}
	}

	public void playSwingPartsForFrames(int frames)
	{
		if ((bool)toolWeapon)
		{
			toolWeapon.playSwingPartsForFrames();
		}
	}

	public void checkForClang()
	{
		if ((bool)useTool && useTool.checkIfNeedClang())
		{
			useTool.playClangSound();
			myAnim.SetTrigger("Clang");
			if (base.isLocalPlayer)
			{
				InputMaster.input.doRumble(0.35f, 1f);
			}
		}
	}

	public void startCrafting()
	{
		StartCoroutine(playCraftingAnimation());
	}

	public void startCooking()
	{
		StartCoroutine(playCookingAnimation());
	}

	public IEnumerator playCraftingAnimation()
	{
		if (!crafting)
		{
			crafting = true;
			equipNewItem(currentlyHoldingNo);
			yield return new WaitForSeconds(1.5f);
			crafting = false;
			Inventory.inv.equipNewSelectedSlot();
		}
	}

	public IEnumerator playCookingAnimation()
	{
		if (!cooking)
		{
			cooking = true;
			equipNewItem(currentlyHoldingNo);
			yield return new WaitForSeconds(1.5f);
			cooking = false;
			Inventory.inv.equipNewSelectedSlot();
		}
	}

	public void setNewLookingAtJournal(bool isLookingAtJournalNow)
	{
		if (isLookingAtJournalNow != lookingAtJournal)
		{
			lookingAtJournal = isLookingAtJournalNow;
			if (lookingAtJournal)
			{
				equipNewItem(currentlyHoldingNo);
			}
			else
			{
				Inventory.inv.equipNewSelectedSlot();
			}
		}
	}

	public void setNewLookingAtMap(bool newLookingAtMap)
	{
		if (newLookingAtMap != lookingAtMap)
		{
			lookingAtMap = newLookingAtMap;
			if (lookingAtMap)
			{
				equipNewItem(currentlyHoldingNo);
			}
			else
			{
				Inventory.inv.equipNewSelectedSlot();
			}
		}
	}

	public void setPetting(bool newPetting)
	{
		if (newPetting != petting)
		{
			petting = newPetting;
			if (petting)
			{
				equipNewItem(currentlyHoldingNo);
			}
			else
			{
				Inventory.inv.equipNewSelectedSlot();
			}
		}
	}

	public bool isWhistling()
	{
		return whistling;
	}

	public void CharWhistles()
	{
		StartCoroutine(playWhistle());
	}

	private IEnumerator playWhistle()
	{
		if (base.isLocalPlayer)
		{
			setWhistling(true);
			GetComponent<CharMovement>();
			for (float whistleTimer = 1f; whistleTimer > 0f; whistleTimer -= Time.deltaTime)
			{
				yield return null;
			}
			setWhistling(false);
		}
	}

	public void setWhistling(bool newWhistle)
	{
		if (newWhistle != whistling)
		{
			whistling = newWhistle;
			if (whistling)
			{
				equipNewItem(currentlyHoldingNo);
			}
			else
			{
				Inventory.inv.equipNewSelectedSlot();
			}
		}
	}

	public void setSwimming(bool newSwimming)
	{
		if (newSwimming != swimming)
		{
			swimming = newSwimming;
			if (swimming)
			{
				equipNewItem(currentlyHoldingNo);
			}
			else
			{
				Inventory.inv.equipNewSelectedSlot();
			}
		}
	}

	public void setDoingEmote(bool newEmote)
	{
		if (newEmote == doingEmote)
		{
			return;
		}
		doingEmote = newEmote;
		if (base.isLocalPlayer)
		{
			if (doingEmote)
			{
				equipNewItem(currentlyHoldingNo);
			}
			else
			{
				Inventory.inv.equipNewSelectedSlot();
			}
		}
	}

	public bool isInside()
	{
		return inside;
	}

	public void setInsideOrOutside(bool insideOrOut, bool playersHouse)
	{
		if (inside != insideOrOut)
		{
			inside = insideOrOut;
			insidePlayerHouse = playersHouse;
			equipNewItem(currentlyHoldingNo);
		}
		if (!insideOrOut)
		{
			Inventory.inv.equipNewSelectedSlot();
		}
	}

	public bool getDriving()
	{
		return driving;
	}

	public void setDriving(bool newDriving)
	{
		if (newDriving != driving)
		{
			driving = newDriving;
			if (driving)
			{
				equipNewItem(currentlyHoldingNo);
			}
			else
			{
				Inventory.inv.equipNewSelectedSlot();
			}
		}
	}

	private IEnumerator swapAnim()
	{
		itemHolderAnim.SetTrigger("PutAway");
		yield return new WaitForSeconds(0.5f);
		itemHolderAnim.SetTrigger("PutAway");
	}

	private void equipMaterialFromInvItem(int inventoryItem, SkinnedMeshRenderer renToPutOn)
	{
		if (inventoryItem == -1)
		{
			if (renToPutOn != shoeRen)
			{
				renToPutOn.gameObject.SetActive(true);
				renToPutOn.material = EquipWindow.equip.underClothes;
			}
			else
			{
				renToPutOn.gameObject.SetActive(false);
			}
		}
		else
		{
			renToPutOn.gameObject.SetActive(true);
			renToPutOn.material = Inventory.inv.allItems[inventoryItem].equipable.material;
		}
	}

	private void equipHeadItem(int itemNoToEquip)
	{
		NetworkheadId = itemNoToEquip;
		if (itemOnHead != null)
		{
			Object.Destroy(itemOnHead);
		}
		if (hairOnHead != null)
		{
			Object.Destroy(hairOnHead);
		}
		if (hairId >= 0 && (itemNoToEquip < 0 || !Inventory.inv.allItems[itemNoToEquip].equipable || !Inventory.inv.allItems[itemNoToEquip].equipable.hideHair))
		{
			if (itemNoToEquip >= 0 && (bool)Inventory.inv.allItems[itemNoToEquip].equipable && Inventory.inv.allItems[itemNoToEquip].equipable.useHelmetHair)
			{
				hairOnHead = Object.Instantiate(CharacterCreatorScript.create.allHairStyles[0], onHeadPosition);
			}
			else
			{
				hairOnHead = Object.Instantiate(CharacterCreatorScript.create.allHairStyles[hairId], onHeadPosition);
			}
			hairOnHead.transform.localPosition = Vector3.zero;
			hairOnHead.transform.localRotation = Quaternion.Euler(Vector3.zero);
			if ((bool)hairOnHead.GetComponent<SpringManager>())
			{
				GetComponent<CharNetworkAnimator>().hairSpring = hairOnHead.GetComponent<SpringManager>();
			}
		}
		if (itemNoToEquip >= 0)
		{
			if ((bool)hairOnHead && (bool)Inventory.inv.allItems[itemNoToEquip].equipable && !Inventory.inv.allItems[itemNoToEquip].equipable.useRegularHair)
			{
				hairOnHead.transform.Find("Hair").gameObject.SetActive(false);
				hairOnHead.transform.Find("Hair_Hat").gameObject.SetActive(true);
				hairOnHead.transform.localPosition = Vector3.zero;
				hairOnHead.transform.localRotation = Quaternion.Euler(Vector3.zero);
			}
			itemOnHead = Object.Instantiate(Inventory.inv.allItems[itemNoToEquip].equipable.hatPrefab, onHeadPosition);
			if ((bool)itemOnHead.GetComponent<SetItemTexture>())
			{
				itemOnHead.GetComponent<SetItemTexture>().setTexture(Inventory.inv.allItems[itemNoToEquip]);
				if ((bool)itemOnHead.GetComponent<SetItemTexture>().changeSize)
				{
					itemOnHead.GetComponent<SetItemTexture>().changeSizeOfTrans(Inventory.inv.allItems[itemNoToEquip].transform.localScale);
				}
			}
			itemOnHead.transform.localPosition = Vector3.zero;
			itemOnHead.transform.localRotation = Quaternion.Euler(Vector3.zero);
		}
		equipHairColour(hairColor);
		StopCoroutine("hairBounce");
		StartCoroutine("hairBounce");
	}

	private void equipHairColour(int colourNo)
	{
		if ((bool)hairOnHead)
		{
			colourNo = Mathf.Clamp(colourNo, 0, CharacterCreatorScript.create.allHairColours.Length - 1);
			if ((bool)hairOnHead.GetComponentInChildren<MeshRenderer>())
			{
				hairOnHead.GetComponentInChildren<MeshRenderer>().material.color = CharacterCreatorScript.create.getHairColour(colourNo);
			}
			if ((bool)hairOnHead.GetComponentInChildren<SkinnedMeshRenderer>())
			{
				hairOnHead.GetComponentInChildren<SkinnedMeshRenderer>().material.color = CharacterCreatorScript.create.getHairColour(colourNo);
			}
		}
	}

	private void onHairColourChange(int oldColour, int newHairColour)
	{
		NetworkhairColor = Mathf.Clamp(newHairColour, 0, CharacterCreatorScript.create.allHairColours.Length - 1);
		equipHairColour(newHairColour);
	}

	private void onChangeSkin(int oldSkin, int newSkin)
	{
		NetworkskinId = Mathf.Clamp(newSkin, 0, CharacterCreatorScript.create.skinTones.Length - 1);
		skinRen.material = CharacterCreatorScript.create.skinTones[skinId];
		eyes.changeSkinColor(CharacterCreatorScript.create.skinTones[skinId].color);
	}

	private void onMouthChange(int oldMouth, int newMouth)
	{
		NetworkmouthId = newMouth;
		eyes.changeMouthMat(CharacterCreatorScript.create.mouthTypes[mouthId], CharacterCreatorScript.create.skinTones[skinId].color);
	}

	private void onNoseChange(int oldNose, int newNose)
	{
		NetworknoseId = newNose;
		eyes.noseMesh.GetComponent<MeshFilter>().sharedMesh = CharacterCreatorScript.create.noseMeshes[noseId];
	}

	private void onHeadChange(int oldId, int newId)
	{
		bool flag = (bool)myAnim;
		NetworkheadId = newId;
		equipHeadItem(newId);
	}

	private void onHairChange(int oldId, int newId)
	{
		bool flag = (bool)myAnim;
		NetworkhairId = Mathf.Clamp(newId, 0, CharacterCreatorScript.create.allHairStyles.Length);
		equipHeadItem(headId);
	}

	private void onFaceChange(int oldId, int newId)
	{
		if ((bool)itemOnFace)
		{
			Object.Destroy(itemOnFace);
		}
		if (newId > -1)
		{
			itemOnFace = Object.Instantiate(Inventory.inv.allItems[newId].equipable.hatPrefab, onHeadPosition);
			itemOnFace.transform.localPosition = Vector3.zero;
			itemOnFace.transform.localRotation = Quaternion.Euler(Vector3.zero);
			if ((bool)itemOnFace.GetComponent<SetItemTexture>())
			{
				itemOnFace.GetComponent<SetItemTexture>().setTexture(Inventory.inv.allItems[newId]);
			}
		}
		NetworkfaceId = newId;
	}

	private void onChangeShirt(int oldId, int newId)
	{
		bool flag = (bool)myAnim;
		NetworkshirtId = newId;
		if (newId != -1 && (bool)Inventory.inv.allItems[shirtId].equipable.shirtMesh)
		{
			shirtRen.sharedMesh = Inventory.inv.allItems[shirtId].equipable.shirtMesh;
		}
		else if (newId == -1)
		{
			shirtRen.sharedMesh = EquipWindow.equip.tShirtMesh;
		}
		else
		{
			shirtRen.sharedMesh = EquipWindow.equip.defaultShirtMesh;
		}
		equipMaterialFromInvItem(newId, shirtRen);
	}

	private void onChangePants(int oldId, int newId)
	{
		bool flag = (bool)myAnim;
		if (newId != -1 && (bool)Inventory.inv.allItems[newId].equipable.useAltMesh)
		{
			pantsRen.sharedMesh = Inventory.inv.allItems[newId].equipable.useAltMesh;
		}
		else
		{
			pantsRen.sharedMesh = EquipWindow.equip.defaultPants;
		}
		NetworkpantsId = newId;
		equipMaterialFromInvItem(newId, pantsRen);
	}

	private void onChangeShoes(int oldId, int newId)
	{
		bool flag = (bool)myAnim;
		if (newId != -1 && (bool)Inventory.inv.allItems[newId].equipable.useAltMesh)
		{
			shoeRen.sharedMesh = Inventory.inv.allItems[newId].equipable.useAltMesh;
		}
		else
		{
			shoeRen.sharedMesh = EquipWindow.equip.defualtShoeMesh;
		}
		NetworkshoeId = newId;
		equipMaterialFromInvItem(newId, shoeRen);
	}

	private void onChangeEyes(int oldId, int newId)
	{
		eyes.changeEyeMat(CharacterCreatorScript.create.allEyeTypes[newId], CharacterCreatorScript.create.skinTones[skinId].color);
		NetworkeyeId = newId;
	}

	private void onChangeEyeColor(int oldId, int newColor)
	{
		eyes.changeEyeColor(CharacterCreatorScript.create.eyeColours[newColor]);
		NetworkeyeColor = newColor;
	}

	public void doEmotion(int emotion)
	{
		if (doingEmotion != null)
		{
			StopCoroutine(doingEmotion);
		}
		doingEmotion = StartCoroutine(doEmote(emotion));
	}

	public bool checkIfDoingEmote()
	{
		return doingEmote;
	}

	private IEnumerator doEmote(int emotion)
	{
		setDoingEmote(true);
		GetComponent<AnimateCharFace>().emotionsLocked = false;
		myAnim.SetInteger("Emotion", emotion);
		yield return new WaitForSeconds(2.5f);
		GetComponent<AnimateCharFace>().emotionsLocked = true;
		myAnim.SetInteger("Emotion", 0);
		GetComponent<AnimateCharFace>().stopFaceEmotion();
		doingEmotion = null;
		setDoingEmote(false);
	}

	public void breakItemAnimation()
	{
		GetComponent<AnimateCharFace>().emotionsLocked = false;
		myAnim.SetInteger("Emotion", 6);
		Invoke("delayStop", 0.75f);
	}

	private void delayStop()
	{
		GetComponent<AnimateCharFace>().emotionsLocked = true;
		myAnim.SetInteger("Emotion", 0);
		GetComponent<AnimateCharFace>().stopFaceEmotion();
	}

	[ClientRpc]
	private void RpcBreakItem()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(EquipItemToChar), "RpcBreakItem", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdBrokenItem()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(EquipItemToChar), "CmdBrokenItem", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeSkin(int newSkin)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newSkin);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeSkin", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeFaceId(int newFaceId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newFaceId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeFaceId", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeHairId(int newHairId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newHairId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeHairId", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeEyes(int newEyeId, int newEyeColor)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newEyeId);
		writer.WriteInt(newEyeColor);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeEyes", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeHeadId(int newHeadId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newHeadId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeHeadId", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeShirtId(int newShirtId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newShirtId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeShirtId", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangePantsId(int newPantsId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newPantsId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangePantsId", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeShoesId(int newShoesId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newShoesId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeShoesId", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdEquipNewItem(int newEquip)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newEquip);
		SendCommandInternal(typeof(EquipItemToChar), "CmdEquipNewItem", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdUsingItem(bool isUsing)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteBool(isUsing);
		SendCommandInternal(typeof(EquipItemToChar), "CmdUsingItem", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeHairColour(int newHair)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newHair);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeHairColour", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeEyeColour(int newEye)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newEye);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeEyeColour", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSendEquipedClothes(int[] clothesArray)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		GeneratedNetworkCode._Write_System_002EInt32_005B_005D(writer, clothesArray);
		SendCommandInternal(typeof(EquipItemToChar), "CmdSendEquipedClothes", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeNose(int newNose)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newNose);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeNose", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeMouth(int newMouth)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newMouth);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeMouth", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSendName(string setPlayerName)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(setPlayerName);
		SendCommandInternal(typeof(EquipItemToChar), "CmdSendName", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdMakeHairDresserSpin()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(EquipItemToChar), "CmdMakeHairDresserSpin", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCallforBugNet()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(EquipItemToChar), "CmdCallforBugNet", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdOpenBag()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(EquipItemToChar), "CmdOpenBag", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCloseBag()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(EquipItemToChar), "CmdCloseBag", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	public void changeOpenBag(bool oldBagOpen, bool newBagOpen)
	{
		NetworkbagOpenEmoteOn = newBagOpen;
		if (newBagOpen)
		{
			if (doingEmotion != null)
			{
				StopCoroutine(doingEmotion);
			}
			doingEmotion = StartCoroutine(bagOpenEmote());
		}
	}

	private IEnumerator bagOpenEmote()
	{
		setDoingEmote(true);
		GetComponent<AnimateCharFace>().emotionsLocked = false;
		myAnim.SetInteger("Emotion", 5);
		while (bagOpenEmoteOn)
		{
			yield return null;
		}
		GetComponent<AnimateCharFace>().emotionsLocked = true;
		myAnim.SetInteger("Emotion", 0);
		GetComponent<AnimateCharFace>().stopFaceEmotion();
		doingEmotion = null;
		setDoingEmote(false);
	}

	[ClientRpc]
	public void RpcPutBugNetInHand()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(EquipItemToChar), "RpcPutBugNetInHand", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	private IEnumerator bugNetWait()
	{
		while (!currentlyHolding || !currentlyHolding.bug)
		{
			yield return null;
		}
		while (!holdingPrefabAnimator)
		{
			yield return null;
		}
		holdingPrefabAnimator.GetComponent<Animator>().SetTrigger("UseBugNet");
		leftHandPos = holdingPrefab.transform.Find("Animation/LeftHandleNet");
		myAnim.SetBool("TwoArms", true);
	}

	public bool currentlyHoldingSinglePlaceableItem()
	{
		if ((bool)currentlyHolding && (bool)currentlyHolding.placeable && !currentlyHolding.placeable.isMultiTileObject())
		{
			return true;
		}
		return false;
	}

	public bool currentlyHoldingMultiTiledPlaceableItem()
	{
		if ((bool)currentlyHolding && (bool)currentlyHolding.placeable && currentlyHolding.placeable.isMultiTileObject())
		{
			return true;
		}
		return false;
	}

	public bool currentlyHoldingDeed()
	{
		if ((bool)currentlyHolding && currentlyHolding.isDeed)
		{
			return true;
		}
		return false;
	}

	public void catchAndShowFish(int fishId)
	{
		if (!fishInHandPlaying)
		{
			BugAndFishCelebration.bugAndFishCel.openWindow(fishId);
			PediaManager.manage.addCaughtToList(fishId);
			fishInHandPlaying = true;
			StartCoroutine(fishLandsInHand(fishId));
		}
	}

	public void catchAndShowBug(int bugId)
	{
		if (!bugInHandPlaying)
		{
			BugAndFishCelebration.bugAndFishCel.openWindow(bugId);
			PediaManager.manage.addCaughtToList(bugId);
			bugInHandPlaying = true;
			StartCoroutine(bugCatchHoldInHand(bugId));
		}
	}

	private IEnumerator fishLandsInHand(int fishId)
	{
		equipNewItem(fishId);
		Inventory.inv.quickBarLocked(true);
		CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Fishing, (int)Mathf.Clamp((float)Inventory.inv.allItems[fishId].value / 200f, 1f, 30f));
		CharLevelManager.manage.addToDayTally(fishId, 1, 3);
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CatchFish);
		while (BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen || (ConversationManager.manage.inConversation && ConversationManager.manage.lastTalkTo.isSign))
		{
			yield return null;
		}
		Inventory.inv.quickBarLocked(false);
		equipNewItem(Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemNo);
		fishInHandPlaying = false;
	}

	private IEnumerator bugCatchHoldInHand(int bugId)
	{
		equipNewItem(bugId);
		CmdCallforBugNet();
		if (base.isLocalPlayer)
		{
			StartCoroutine(bugNetWait());
		}
		Inventory.inv.quickBarLocked(true);
		CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.BugCatching, (int)Mathf.Clamp((float)Inventory.inv.allItems[bugId].value / 200f, 1f, 100f));
		CharLevelManager.manage.addToDayTally(bugId, 1, 4);
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.CatchBugs);
		while (BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen || (ConversationManager.manage.inConversation && ConversationManager.manage.lastTalkTo.isSign))
		{
			yield return null;
		}
		Inventory.inv.quickBarLocked(false);
		equipNewItem(Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemNo);
		bugInHandPlaying = false;
	}

	private IEnumerator hairBounce()
	{
		float journey = 0f;
		float duration = 0.35f;
		while (journey <= duration)
		{
			journey += Time.deltaTime;
			float time = Mathf.Clamp01(journey / duration);
			float t = UIAnimationManager.manage.hairChangeBounce.Evaluate(time);
			float num = Mathf.LerpUnclamped(0.95f, 1f, t);
			if ((bool)hairOnHead)
			{
				hairOnHead.transform.localScale = new Vector3(num, 1f + (1f - num), 1f + (1f - num));
			}
			if ((bool)itemOnHead)
			{
				itemOnHead.transform.localScale = new Vector3(num, 1f + (1f - num), 1f + (1f - num));
			}
			yield return null;
		}
	}

	public void playPlaceableAnimation()
	{
		if ((bool)holdingPrefab && (bool)holdingPrefabAnimator)
		{
			holdingPrefabAnimator.SetTrigger("PlaceItemAnimation");
		}
	}

	[Command]
	public void CmdChangeLookableForAiming(Vector3 newPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(newPos);
		SendCommandInternal(typeof(EquipItemToChar), "CmdChangeLookableForAiming", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcMoveLookableForRanged(Vector3 newPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(newPos);
		SendRPCInternal(typeof(EquipItemToChar), "RpcMoveLookableForRanged", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdFireProjectileAtDir(Vector3 spawnPos, Vector3 direction, float strength, int projectileId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(spawnPos);
		writer.WriteVector3(direction);
		writer.WriteFloat(strength);
		writer.WriteInt(projectileId);
		SendCommandInternal(typeof(EquipItemToChar), "CmdFireProjectileAtDir", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	private void RpcFireAtAngle(Vector3 spawnPos, Vector3 forward, float strength, int projectileId)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(spawnPos);
		writer.WriteVector3(forward);
		writer.WriteFloat(strength);
		writer.WriteInt(projectileId);
		SendRPCInternal(typeof(EquipItemToChar), "RpcFireAtAngle", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	static EquipItemToChar()
	{
		dif = new Vector3(0f, -0.18f, 0f);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdBrokenItem", InvokeUserCode_CmdBrokenItem, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeSkin", InvokeUserCode_CmdChangeSkin, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeFaceId", InvokeUserCode_CmdChangeFaceId, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeHairId", InvokeUserCode_CmdChangeHairId, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeEyes", InvokeUserCode_CmdChangeEyes, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeHeadId", InvokeUserCode_CmdChangeHeadId, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeShirtId", InvokeUserCode_CmdChangeShirtId, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangePantsId", InvokeUserCode_CmdChangePantsId, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeShoesId", InvokeUserCode_CmdChangeShoesId, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdEquipNewItem", InvokeUserCode_CmdEquipNewItem, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdUsingItem", InvokeUserCode_CmdUsingItem, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeHairColour", InvokeUserCode_CmdChangeHairColour, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeEyeColour", InvokeUserCode_CmdChangeEyeColour, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdSendEquipedClothes", InvokeUserCode_CmdSendEquipedClothes, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeNose", InvokeUserCode_CmdChangeNose, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeMouth", InvokeUserCode_CmdChangeMouth, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdSendName", InvokeUserCode_CmdSendName, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdMakeHairDresserSpin", InvokeUserCode_CmdMakeHairDresserSpin, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdCallforBugNet", InvokeUserCode_CmdCallforBugNet, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdOpenBag", InvokeUserCode_CmdOpenBag, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdCloseBag", InvokeUserCode_CmdCloseBag, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdChangeLookableForAiming", InvokeUserCode_CmdChangeLookableForAiming, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(EquipItemToChar), "CmdFireProjectileAtDir", InvokeUserCode_CmdFireProjectileAtDir, true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(EquipItemToChar), "RpcCharacterJoinedPopup", InvokeUserCode_RpcCharacterJoinedPopup);
		RemoteCallHelper.RegisterRpcDelegate(typeof(EquipItemToChar), "RpcBreakItem", InvokeUserCode_RpcBreakItem);
		RemoteCallHelper.RegisterRpcDelegate(typeof(EquipItemToChar), "RpcPutBugNetInHand", InvokeUserCode_RpcPutBugNetInHand);
		RemoteCallHelper.RegisterRpcDelegate(typeof(EquipItemToChar), "RpcMoveLookableForRanged", InvokeUserCode_RpcMoveLookableForRanged);
		RemoteCallHelper.RegisterRpcDelegate(typeof(EquipItemToChar), "RpcFireAtAngle", InvokeUserCode_RpcFireAtAngle);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_RpcCharacterJoinedPopup(string newName, string sendIslandName)
	{
		if (base.isLocalPlayer)
		{
			NotificationManager.manage.makeTopNotification("Welcome to " + sendIslandName);
		}
		else
		{
			NotificationManager.manage.makeTopNotification(newName + " has joined");
		}
	}

	protected static void InvokeUserCode_RpcCharacterJoinedPopup(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCharacterJoinedPopup called on server.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_RpcCharacterJoinedPopup(reader.ReadString(), reader.ReadString());
		}
	}

	protected void UserCode_RpcBreakItem()
	{
		ParticleManager.manage.emitBrokenItemPart(holdPos.position + holdPos.forward);
	}

	protected static void InvokeUserCode_RpcBreakItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcBreakItem called on server.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_RpcBreakItem();
		}
	}

	protected void UserCode_CmdBrokenItem()
	{
		RpcBreakItem();
		NetworkMapSharer.share.RpcBreakToolReact(base.netId);
	}

	protected static void InvokeUserCode_CmdBrokenItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdBrokenItem called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdBrokenItem();
		}
	}

	protected void UserCode_CmdChangeSkin(int newSkin)
	{
		NetworkskinId = newSkin;
	}

	protected static void InvokeUserCode_CmdChangeSkin(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeSkin called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeSkin(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeFaceId(int newFaceId)
	{
		NetworkfaceId = newFaceId;
	}

	protected static void InvokeUserCode_CmdChangeFaceId(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeFaceId called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeFaceId(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeHairId(int newHairId)
	{
		NetworkhairId = newHairId;
	}

	protected static void InvokeUserCode_CmdChangeHairId(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeHairId called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeHairId(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeEyes(int newEyeId, int newEyeColor)
	{
		NetworkeyeId = newEyeId;
		NetworkeyeColor = newEyeColor;
	}

	protected static void InvokeUserCode_CmdChangeEyes(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeEyes called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeEyes(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeHeadId(int newHeadId)
	{
		NetworkheadId = newHeadId;
	}

	protected static void InvokeUserCode_CmdChangeHeadId(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeHeadId called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeHeadId(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeShirtId(int newShirtId)
	{
		NetworkshirtId = newShirtId;
	}

	protected static void InvokeUserCode_CmdChangeShirtId(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeShirtId called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeShirtId(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangePantsId(int newPantsId)
	{
		NetworkpantsId = newPantsId;
	}

	protected static void InvokeUserCode_CmdChangePantsId(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangePantsId called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangePantsId(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeShoesId(int newShoesId)
	{
		NetworkshoeId = newShoesId;
	}

	protected static void InvokeUserCode_CmdChangeShoesId(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeShoesId called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeShoesId(reader.ReadInt());
		}
	}

	protected void UserCode_CmdEquipNewItem(int newEquip)
	{
		NetworkcurrentlyHoldingNo = newEquip;
	}

	protected static void InvokeUserCode_CmdEquipNewItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEquipNewItem called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdEquipNewItem(reader.ReadInt());
		}
	}

	protected void UserCode_CmdUsingItem(bool isUsing)
	{
		NetworkusingItem = isUsing;
	}

	protected static void InvokeUserCode_CmdUsingItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUsingItem called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdUsingItem(reader.ReadBool());
		}
	}

	protected void UserCode_CmdChangeHairColour(int newHair)
	{
		NetworkhairColor = newHair;
	}

	protected static void InvokeUserCode_CmdChangeHairColour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeHairColour called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeHairColour(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeEyeColour(int newEye)
	{
		NetworkeyeId = newEye;
	}

	protected static void InvokeUserCode_CmdChangeEyeColour(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeEyeColour called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeEyeColour(reader.ReadInt());
		}
	}

	protected void UserCode_CmdSendEquipedClothes(int[] clothesArray)
	{
		NetworkheadId = clothesArray[0];
		NetworkshirtId = clothesArray[1];
		NetworkpantsId = clothesArray[2];
		NetworkshoeId = clothesArray[3];
	}

	protected static void InvokeUserCode_CmdSendEquipedClothes(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendEquipedClothes called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdSendEquipedClothes(GeneratedNetworkCode._Read_System_002EInt32_005B_005D(reader));
		}
	}

	protected void UserCode_CmdChangeNose(int newNose)
	{
		NetworknoseId = newNose;
	}

	protected static void InvokeUserCode_CmdChangeNose(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeNose called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeNose(reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeMouth(int newMouth)
	{
		NetworkmouthId = newMouth;
	}

	protected static void InvokeUserCode_CmdChangeMouth(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeMouth called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeMouth(reader.ReadInt());
		}
	}

	protected void UserCode_CmdSendName(string setPlayerName)
	{
		NetworkplayerName = setPlayerName;
	}

	protected static void InvokeUserCode_CmdSendName(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendName called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdSendName(reader.ReadString());
		}
	}

	protected void UserCode_CmdMakeHairDresserSpin()
	{
		NetworkMapSharer.share.RpcSpinChair();
	}

	protected static void InvokeUserCode_CmdMakeHairDresserSpin(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdMakeHairDresserSpin called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdMakeHairDresserSpin();
		}
	}

	protected void UserCode_CmdCallforBugNet()
	{
		RpcPutBugNetInHand();
	}

	protected static void InvokeUserCode_CmdCallforBugNet(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCallforBugNet called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdCallforBugNet();
		}
	}

	protected void UserCode_CmdOpenBag()
	{
		NetworkbagOpenEmoteOn = true;
	}

	protected static void InvokeUserCode_CmdOpenBag(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOpenBag called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdOpenBag();
		}
	}

	protected void UserCode_CmdCloseBag()
	{
		NetworkbagOpenEmoteOn = false;
	}

	protected static void InvokeUserCode_CmdCloseBag(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCloseBag called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdCloseBag();
		}
	}

	protected void UserCode_RpcPutBugNetInHand()
	{
		if (!base.isLocalPlayer)
		{
			StartCoroutine(bugNetWait());
		}
	}

	protected static void InvokeUserCode_RpcPutBugNetInHand(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPutBugNetInHand called on server.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_RpcPutBugNetInHand();
		}
	}

	protected void UserCode_CmdChangeLookableForAiming(Vector3 newPos)
	{
		RpcMoveLookableForRanged(newPos);
	}

	protected static void InvokeUserCode_CmdChangeLookableForAiming(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeLookableForAiming called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdChangeLookableForAiming(reader.ReadVector3());
		}
	}

	protected void UserCode_RpcMoveLookableForRanged(Vector3 newPos)
	{
		aimLookablePos = newPos;
	}

	protected static void InvokeUserCode_RpcMoveLookableForRanged(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMoveLookableForRanged called on server.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_RpcMoveLookableForRanged(reader.ReadVector3());
		}
	}

	protected void UserCode_CmdFireProjectileAtDir(Vector3 spawnPos, Vector3 direction, float strength, int projectileId)
	{
		RpcFireAtAngle(spawnPos, direction, strength, projectileId);
	}

	protected static void InvokeUserCode_CmdFireProjectileAtDir(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFireProjectileAtDir called on client.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_CmdFireProjectileAtDir(reader.ReadVector3(), reader.ReadVector3(), reader.ReadFloat(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcFireAtAngle(Vector3 spawnPos, Vector3 forward, float strength, int projectileId)
	{
		Object.Instantiate(NetworkMapSharer.share.projectile, spawnPos, holdPos.rotation).GetComponent<Projectile>().SetUpProjectile(projectileId, base.transform, forward, strength);
	}

	protected static void InvokeUserCode_RpcFireAtAngle(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFireAtAngle called on server.");
		}
		else
		{
			((EquipItemToChar)obj).UserCode_RpcFireAtAngle(reader.ReadVector3(), reader.ReadVector3(), reader.ReadFloat(), reader.ReadInt());
		}
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(currentlyHoldingNo);
			writer.WriteString(playerName);
			writer.WriteBool(usingItem);
			writer.WriteBool(blocking);
			writer.WriteInt(hairColor);
			writer.WriteInt(faceId);
			writer.WriteInt(headId);
			writer.WriteInt(hairId);
			writer.WriteInt(shirtId);
			writer.WriteInt(pantsId);
			writer.WriteInt(shoeId);
			writer.WriteInt(eyeId);
			writer.WriteInt(eyeColor);
			writer.WriteInt(skinId);
			writer.WriteInt(noseId);
			writer.WriteInt(mouthId);
			writer.WriteBool(bagOpenEmoteOn);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(currentlyHoldingNo);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteString(playerName);
			result = true;
		}
		if ((base.syncVarDirtyBits & 4L) != 0L)
		{
			writer.WriteBool(usingItem);
			result = true;
		}
		if ((base.syncVarDirtyBits & 8L) != 0L)
		{
			writer.WriteBool(blocking);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x10L) != 0L)
		{
			writer.WriteInt(hairColor);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x20L) != 0L)
		{
			writer.WriteInt(faceId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x40L) != 0L)
		{
			writer.WriteInt(headId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x80L) != 0L)
		{
			writer.WriteInt(hairId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x100L) != 0L)
		{
			writer.WriteInt(shirtId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x200L) != 0L)
		{
			writer.WriteInt(pantsId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x400L) != 0L)
		{
			writer.WriteInt(shoeId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x800L) != 0L)
		{
			writer.WriteInt(eyeId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x1000L) != 0L)
		{
			writer.WriteInt(eyeColor);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x2000L) != 0L)
		{
			writer.WriteInt(skinId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x4000L) != 0L)
		{
			writer.WriteInt(noseId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x8000L) != 0L)
		{
			writer.WriteInt(mouthId);
			result = true;
		}
		if ((base.syncVarDirtyBits & 0x10000L) != 0L)
		{
			writer.WriteBool(bagOpenEmoteOn);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = currentlyHoldingNo;
			NetworkcurrentlyHoldingNo = reader.ReadInt();
			if (!SyncVarEqual(num, ref currentlyHoldingNo))
			{
				equipNewItemNetwork(num, currentlyHoldingNo);
			}
			string text = playerName;
			NetworkplayerName = reader.ReadString();
			if (!SyncVarEqual(text, ref playerName))
			{
				onChangeName(text, playerName);
			}
			bool flag = usingItem;
			NetworkusingItem = reader.ReadBool();
			bool flag2 = blocking;
			Networkblocking = reader.ReadBool();
			int num2 = hairColor;
			NetworkhairColor = reader.ReadInt();
			if (!SyncVarEqual(num2, ref hairColor))
			{
				onHairColourChange(num2, hairColor);
			}
			int num3 = faceId;
			NetworkfaceId = reader.ReadInt();
			if (!SyncVarEqual(num3, ref faceId))
			{
				onFaceChange(num3, faceId);
			}
			int num4 = headId;
			NetworkheadId = reader.ReadInt();
			if (!SyncVarEqual(num4, ref headId))
			{
				onHeadChange(num4, headId);
			}
			int num5 = hairId;
			NetworkhairId = reader.ReadInt();
			if (!SyncVarEqual(num5, ref hairId))
			{
				onHairChange(num5, hairId);
			}
			int num6 = shirtId;
			NetworkshirtId = reader.ReadInt();
			if (!SyncVarEqual(num6, ref shirtId))
			{
				onChangeShirt(num6, shirtId);
			}
			int num7 = pantsId;
			NetworkpantsId = reader.ReadInt();
			if (!SyncVarEqual(num7, ref pantsId))
			{
				onChangePants(num7, pantsId);
			}
			int num8 = shoeId;
			NetworkshoeId = reader.ReadInt();
			if (!SyncVarEqual(num8, ref shoeId))
			{
				onChangeShoes(num8, shoeId);
			}
			int num9 = eyeId;
			NetworkeyeId = reader.ReadInt();
			if (!SyncVarEqual(num9, ref eyeId))
			{
				onChangeEyes(num9, eyeId);
			}
			int num10 = eyeColor;
			NetworkeyeColor = reader.ReadInt();
			if (!SyncVarEqual(num10, ref eyeColor))
			{
				onChangeEyeColor(num10, eyeColor);
			}
			int num11 = skinId;
			NetworkskinId = reader.ReadInt();
			if (!SyncVarEqual(num11, ref skinId))
			{
				onChangeSkin(num11, skinId);
			}
			int num12 = noseId;
			NetworknoseId = reader.ReadInt();
			if (!SyncVarEqual(num12, ref noseId))
			{
				onNoseChange(num12, noseId);
			}
			int num13 = mouthId;
			NetworkmouthId = reader.ReadInt();
			if (!SyncVarEqual(num13, ref mouthId))
			{
				onMouthChange(num13, mouthId);
			}
			bool flag3 = bagOpenEmoteOn;
			NetworkbagOpenEmoteOn = reader.ReadBool();
			if (!SyncVarEqual(flag3, ref bagOpenEmoteOn))
			{
				changeOpenBag(flag3, bagOpenEmoteOn);
			}
			return;
		}
		long num14 = (long)reader.ReadULong();
		if ((num14 & 1L) != 0L)
		{
			int num15 = currentlyHoldingNo;
			NetworkcurrentlyHoldingNo = reader.ReadInt();
			if (!SyncVarEqual(num15, ref currentlyHoldingNo))
			{
				equipNewItemNetwork(num15, currentlyHoldingNo);
			}
		}
		if ((num14 & 2L) != 0L)
		{
			string text2 = playerName;
			NetworkplayerName = reader.ReadString();
			if (!SyncVarEqual(text2, ref playerName))
			{
				onChangeName(text2, playerName);
			}
		}
		if ((num14 & 4L) != 0L)
		{
			bool flag4 = usingItem;
			NetworkusingItem = reader.ReadBool();
		}
		if ((num14 & 8L) != 0L)
		{
			bool flag5 = blocking;
			Networkblocking = reader.ReadBool();
		}
		if ((num14 & 0x10L) != 0L)
		{
			int num16 = hairColor;
			NetworkhairColor = reader.ReadInt();
			if (!SyncVarEqual(num16, ref hairColor))
			{
				onHairColourChange(num16, hairColor);
			}
		}
		if ((num14 & 0x20L) != 0L)
		{
			int num17 = faceId;
			NetworkfaceId = reader.ReadInt();
			if (!SyncVarEqual(num17, ref faceId))
			{
				onFaceChange(num17, faceId);
			}
		}
		if ((num14 & 0x40L) != 0L)
		{
			int num18 = headId;
			NetworkheadId = reader.ReadInt();
			if (!SyncVarEqual(num18, ref headId))
			{
				onHeadChange(num18, headId);
			}
		}
		if ((num14 & 0x80L) != 0L)
		{
			int num19 = hairId;
			NetworkhairId = reader.ReadInt();
			if (!SyncVarEqual(num19, ref hairId))
			{
				onHairChange(num19, hairId);
			}
		}
		if ((num14 & 0x100L) != 0L)
		{
			int num20 = shirtId;
			NetworkshirtId = reader.ReadInt();
			if (!SyncVarEqual(num20, ref shirtId))
			{
				onChangeShirt(num20, shirtId);
			}
		}
		if ((num14 & 0x200L) != 0L)
		{
			int num21 = pantsId;
			NetworkpantsId = reader.ReadInt();
			if (!SyncVarEqual(num21, ref pantsId))
			{
				onChangePants(num21, pantsId);
			}
		}
		if ((num14 & 0x400L) != 0L)
		{
			int num22 = shoeId;
			NetworkshoeId = reader.ReadInt();
			if (!SyncVarEqual(num22, ref shoeId))
			{
				onChangeShoes(num22, shoeId);
			}
		}
		if ((num14 & 0x800L) != 0L)
		{
			int num23 = eyeId;
			NetworkeyeId = reader.ReadInt();
			if (!SyncVarEqual(num23, ref eyeId))
			{
				onChangeEyes(num23, eyeId);
			}
		}
		if ((num14 & 0x1000L) != 0L)
		{
			int num24 = eyeColor;
			NetworkeyeColor = reader.ReadInt();
			if (!SyncVarEqual(num24, ref eyeColor))
			{
				onChangeEyeColor(num24, eyeColor);
			}
		}
		if ((num14 & 0x2000L) != 0L)
		{
			int num25 = skinId;
			NetworkskinId = reader.ReadInt();
			if (!SyncVarEqual(num25, ref skinId))
			{
				onChangeSkin(num25, skinId);
			}
		}
		if ((num14 & 0x4000L) != 0L)
		{
			int num26 = noseId;
			NetworknoseId = reader.ReadInt();
			if (!SyncVarEqual(num26, ref noseId))
			{
				onNoseChange(num26, noseId);
			}
		}
		if ((num14 & 0x8000L) != 0L)
		{
			int num27 = mouthId;
			NetworkmouthId = reader.ReadInt();
			if (!SyncVarEqual(num27, ref mouthId))
			{
				onMouthChange(num27, mouthId);
			}
		}
		if ((num14 & 0x10000L) != 0L)
		{
			bool flag6 = bagOpenEmoteOn;
			NetworkbagOpenEmoteOn = reader.ReadBool();
			if (!SyncVarEqual(flag6, ref bagOpenEmoteOn))
			{
				changeOpenBag(flag6, bagOpenEmoteOn);
			}
		}
	}
}
