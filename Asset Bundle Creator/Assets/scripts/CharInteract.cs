using System.Collections;
using System.Runtime.InteropServices;
using I2.Loc;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class CharInteract : NetworkBehaviour
{
	public enum TilePromptType
	{
		None = 0,
		Harvest = 1,
		ItemName = 2,
		Open = 3,
		Close = 4
	}

	public EquipItemToChar myEquip;

	private InventoryItem lastEquip;

	public Vector2 selectedTile = new Vector2(0f, 0f);

	public Transform tileHighlighter;

	public GameObject tileHighlighterRotArrow;

	public TileObject objectAttacking;

	private bool canAttackSelectedTile;

	public bool refreshSelection;

	[SyncVar(hook = "OnChangeAttackingPos")]
	public Vector2 currentlyAttackingPos = new Vector2(0f, 0f);

	private int currentlyAttackingId = -1;

	private Vector2 selectedTileBeforeToolUse;

	private int placeableRotation = 1;

	public Material previewYes;

	public Material previewNo;

	public bool insidePlayerHouse;

	public HouseDetails insideHouseDetails;

	public DisplayPlayerHouseTiles insideHouseDisplay;

	public Transform playerHouseTransform;

	private TileObject previewObject;

	private Vector3 selectPositionDif = Vector3.zero;

	public bool placingDeed;

	public float attackingHealth;

	public TileHighlighter highLighterChange;

	private CharMovement myMove;

	public InventoryItem emptyShovel;

	private Vector3 desiredHighlighterPos = Vector3.zero;

	private Vector3 desiredPreviewPos;

	private int previewShowingRot;

	private GameObject vehiclePreview;

	private VehiclePreview canPlaceVehiclePreview;

	private bool hasBeenInsideBefore;

	private MeshRenderer[] previewRens = new MeshRenderer[0];

	private Coroutine placingRoutine;

	public Vector2 NetworkcurrentlyAttackingPos
	{
		get
		{
			return currentlyAttackingPos;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref currentlyAttackingPos))
			{
				Vector2 oldPos = currentlyAttackingPos;
				SetSyncVar(value, ref currentlyAttackingPos, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					OnChangeAttackingPos(oldPos, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public void Start()
	{
		myMove = GetComponent<CharMovement>();
		if (base.isLocalPlayer)
		{
			tileHighlighter.parent = null;
			tileHighlighter.transform.rotation = Quaternion.identity;
			highLighterChange = tileHighlighter.GetComponent<TileHighlighter>();
			highLighterChange.setAtHighliter();
			StartCoroutine(hidePreviewInMenuAndConvo());
		}
		else
		{
			Object.Destroy(tileHighlighter.gameObject);
		}
	}

	public void Update()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		Vector3 vector = base.transform.position + base.transform.forward * 2f;
		if (placingDeed)
		{
			vector = base.transform.position;
			vector += selectPositionDif;
			if (!ConversationManager.manage.inConversation && (InputMaster.input.UICancel() || (Inventory.inv.usingMouse && InputMaster.input.Interact()) || !myEquip.currentlyHolding || !myEquip.currentlyHolding.placeable || (!insidePlayerHouse && myEquip.currentlyHoldingSinglePlaceableItem() && myEquip.currentlyHolding.placeable.getsRotationFromMap())))
			{
				changePlacingDeedBool(false);
			}
		}
		if (insidePlayerHouse && (bool)playerHouseTransform)
		{
			vector -= playerHouseTransform.position;
		}
		vector += highLighterPosDif();
		if (insidePlayerHouse && (bool)insideHouseDisplay)
		{
			if (myEquip.currentlyHoldingMultiTiledPlaceableItem() && placingDeed)
			{
				int num = myEquip.currentlyHolding.placeable.getXSize();
				int num2 = myEquip.currentlyHolding.placeable.getYSize();
				if (placeableRotation == 2 || placeableRotation == 4)
				{
					num = myEquip.currentlyHolding.placeable.getYSize();
					num2 = myEquip.currentlyHolding.placeable.getXSize();
				}
				vector = new Vector3(Mathf.Clamp(vector.x, 0f, insideHouseDisplay.xLength * 2 - 2 - num), vector.y, Mathf.Clamp(vector.z, 0f, insideHouseDisplay.yLength * 2 - 2 - num2));
			}
			else
			{
				vector = new Vector3(Mathf.Clamp(vector.x, 0f, insideHouseDisplay.xLength * 2 - 2), vector.y, Mathf.Clamp(vector.z, 0f, insideHouseDisplay.yLength * 2 - 2));
			}
		}
		int num3 = (int)(Mathf.Round(vector.x + 0.5f) / 2f);
		int num4 = (int)(Mathf.Round(vector.z + 0.5f) / 2f);
		if (!insidePlayerHouse)
		{
			num3 = Mathf.Clamp(num3, 0, WorldManager.manageWorld.getMapSize() - 1);
			num4 = Mathf.Clamp(num4, 0, WorldManager.manageWorld.getMapSize() - 1);
		}
		if (refreshSelection || (int)selectedTile.x != num3 || (int)selectedTile.y != num4 || lastEquip != myEquip.currentlyHolding)
		{
			refreshTileSelection(num3, num4);
		}
		smoothMoveHighlighterAndPreview();
	}

	public void rotatePreview()
	{
		if (!myEquip.getDriving() && !ConversationManager.manage.inConversation && ((myEquip.currentlyHoldingSinglePlaceableItem() && myEquip.currentlyHolding.placeable.getsRotationFromMap()) || (myEquip.currentlyHoldingMultiTiledPlaceableItem() && placingDeed) || ((bool)myEquip.currentlyHolding && (bool)myEquip.currentlyHolding.spawnPlaceable)))
		{
			placeableRotation++;
			if (placeableRotation > 4)
			{
				placeableRotation = 1;
			}
			createPreviewOnHighlighter((int)selectedTile.x, (int)selectedTile.y);
			refreshPreview((int)selectedTile.x, (int)selectedTile.y);
			SoundManager.manage.play2DSound(SoundManager.manage.rotationSound);
		}
	}

	private IEnumerator hidePreviewInMenuAndConvo()
	{
		while (true)
		{
			if ((bool)previewObject && !Inventory.inv.canMoveChar())
			{
				refreshPreview((int)selectedTile.x, (int)selectedTile.y);
				while (!Inventory.inv.canMoveChar())
				{
					yield return null;
				}
				refreshPreview((int)selectedTile.x, (int)selectedTile.y);
			}
			yield return null;
		}
	}

	public bool selectedTileNeedsServerRefresh()
	{
		if (selectedTile != currentlyAttackingPos)
		{
			return true;
		}
		return false;
	}

	public bool tileNeedsPopup()
	{
		int num = WorldManager.manageWorld.heightMap[(int)selectedTile.x, (int)selectedTile.y];
		if (base.transform.position.y < (float)num + 1.5f && base.transform.position.y > (float)num - 1.5f)
		{
			if ((bool)objectAttacking && (bool)objectAttacking.tileObjectGrowthStages && objectAttacking.tileObjectGrowthStages.canBeHarvested(WorldManager.manageWorld.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y]))
			{
				return true;
			}
			if ((bool)objectAttacking && (bool)objectAttacking.showObjectOnStatusChange && objectAttacking.showObjectOnStatusChange.canBePickedUpByHand && WorldManager.manageWorld.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y] > 0)
			{
				return true;
			}
			if ((bool)objectAttacking && (bool)objectAttacking.tileOnOff && objectAttacking.tileOnOff.isGate)
			{
				bool isOpen = objectAttacking.tileOnOff.isOpen;
				return true;
			}
		}
		return false;
	}

	public bool canTileBePickedUp()
	{
		if ((bool)objectAttacking && objectAttacking.canBePickedUp())
		{
			if ((bool)WorldManager.manageWorld.allObjects[objectAttacking.tileObjectId].tileObjectItemChanger && WorldManager.manageWorld.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y] >= 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public TilePromptType tileCloseNeedsPrompt(out int xPos, out int yPos)
	{
		TilePromptType tilePopUp = getTilePopUp((int)selectedTile.x, (int)selectedTile.y);
		if (tilePopUp != 0)
		{
			xPos = (int)selectedTile.x;
			yPos = (int)selectedTile.y;
			return tilePopUp;
		}
		tilePopUp = getTilePopUp((int)selectedTile.x - 1, (int)selectedTile.y);
		if (tilePopUp != 0)
		{
			xPos = (int)selectedTile.x - 1;
			yPos = (int)selectedTile.y;
			return tilePopUp;
		}
		tilePopUp = getTilePopUp((int)selectedTile.x + 1, (int)selectedTile.y);
		if (tilePopUp != 0)
		{
			xPos = (int)selectedTile.x + 1;
			yPos = (int)selectedTile.y;
			return tilePopUp;
		}
		tilePopUp = getTilePopUp((int)selectedTile.x, (int)selectedTile.y - 1);
		if (tilePopUp != 0)
		{
			xPos = (int)selectedTile.x;
			yPos = (int)selectedTile.y - 1;
			return tilePopUp;
		}
		tilePopUp = getTilePopUp((int)selectedTile.x, (int)selectedTile.y + 1);
		if (tilePopUp != 0)
		{
			xPos = (int)selectedTile.x;
			yPos = (int)selectedTile.y + 1;
			return tilePopUp;
		}
		xPos = (int)selectedTile.x;
		yPos = (int)selectedTile.x;
		return TilePromptType.None;
	}

	public string getPromptStringTile(TilePromptType type, int xPos, int yPos)
	{
		switch (type)
		{
		case TilePromptType.Harvest:
			return (LocalizedString)"ToolTips/Tip_Harvest";
		case TilePromptType.ItemName:
			return WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.harvestDrop.getInvItemName();
		case TilePromptType.Open:
			return (LocalizedString)"ToolTips/Tip_Open";
		case TilePromptType.Close:
			return (LocalizedString)"ToolTips/Tip_Close";
		default:
			return "";
		}
	}

	private TilePromptType getTilePopUp(int interactX, int interactY)
	{
		if (!WorldManager.manageWorld.isPositionOnMap(interactX, interactY))
		{
			return TilePromptType.None;
		}
		int num = WorldManager.manageWorld.heightMap[interactX, interactY];
		TileObject tileObject = null;
		if (WorldManager.manageWorld.onTileMap[interactX, interactY] > -1)
		{
			tileObject = WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[interactX, interactY]];
			if (Vector3.Distance(new Vector3(interactX * 2, base.transform.position.y, interactY * 2), base.transform.position) >= 5f)
			{
				return TilePromptType.None;
			}
			if (Vector3.Dot(base.transform.forward, (new Vector3(interactX * 2, base.transform.position.y, interactY * 2) - base.transform.position).normalized) < 0.7f)
			{
				return TilePromptType.None;
			}
			if (base.transform.position.y < (float)num + 1.5f && base.transform.position.y > (float)num - 1.5f)
			{
				if ((bool)tileObject && (bool)tileObject.tileObjectGrowthStages && tileObject.tileObjectGrowthStages.canBeHarvested(WorldManager.manageWorld.onTileStatusMap[interactX, interactY]))
				{
					if (tileObject.tileObjectGrowthStages.normalPickUp)
					{
						return TilePromptType.ItemName;
					}
					return TilePromptType.Harvest;
				}
				if ((bool)tileObject && (bool)tileObject.showObjectOnStatusChange && tileObject.showObjectOnStatusChange.canBePickedUpByHand && interactX == (int)selectedTile.x && interactY == (int)selectedTile.y && WorldManager.manageWorld.onTileStatusMap[interactX, interactY] > 0)
				{
					return TilePromptType.Harvest;
				}
				if ((bool)tileObject && (bool)tileObject.tileOnOff && interactX == (int)selectedTile.x && interactY == (int)selectedTile.y && tileObject.tileOnOff.isGate)
				{
					if (tileObject.tileOnOff.isOpen)
					{
						return TilePromptType.Close;
					}
					return TilePromptType.Open;
				}
			}
			return TilePromptType.None;
		}
		return TilePromptType.None;
	}

	public void doDamageToolPos(Vector3 newToolPos)
	{
		selectedTileBeforeToolUse = selectedTile;
		int num = Mathf.RoundToInt(newToolPos.x / 2f);
		int num2 = Mathf.RoundToInt(newToolPos.z / 2f);
		selectedTile = new Vector2(num, num2);
		refreshTileSelection(num, num2);
		doDamage(false);
		selectedTile = selectedTileBeforeToolUse;
		refreshTileSelection((int)selectedTile.x, (int)selectedTile.y);
		refreshSelection = true;
	}

	public void doDamageWithDif(Vector3 tilePosDif)
	{
	}

	public void wiggleBeforePickUp()
	{
		if (placingDeed)
		{
			return;
		}
		if (insidePlayerHouse && (bool)objectAttacking && objectAttacking.canBePlaceOn() && objectAttacking.checkIfHasAnyItemsOnTop(playerHouseTransform.position, insideHouseDetails, (int)selectedTile.x, (int)selectedTile.y))
		{
			if (objectAttacking.checkIfHasAnyItemsOnTop(playerHouseTransform.position, insideHouseDetails, (int)selectedTile.x, (int)selectedTile.y))
			{
				Vector3 vector = objectAttacking.findClosestPlacedPosition(desiredHighlighterPos).position - playerHouseTransform.position;
				int num = Mathf.RoundToInt(vector.x / 2f);
				int num2 = Mathf.RoundToInt(vector.z / 2f);
				if ((bool)insideHouseDisplay && (bool)insideHouseDisplay.tileObjectsOnTop[num, num2])
				{
					insideHouseDisplay.tileObjectsOnTop[num, num2].damage(false);
				}
			}
		}
		else if ((bool)objectAttacking && objectAttacking.canBePickedUp())
		{
			objectAttacking.damage(false);
		}
	}

	public void doATileInteractions()
	{
		if (!tileInteract((int)selectedTile.x, (int)selectedTile.y) && !tileInteract((int)selectedTile.x - 1, (int)selectedTile.y) && !tileInteract((int)selectedTile.x + 1, (int)selectedTile.y) && !tileInteract((int)selectedTile.x, (int)selectedTile.y - 1))
		{
			tileInteract((int)selectedTile.x, (int)selectedTile.y + 1);
		}
	}

	public bool tileInteract(int interactX, int interactY)
	{
		if (insideHouseDetails != null)
		{
			return false;
		}
		int num = WorldManager.manageWorld.heightMap[interactX, interactY];
		TileObject tileObject = null;
		if (WorldManager.manageWorld.onTileMap[interactX, interactY] > -1)
		{
			tileObject = WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[interactX, interactY]];
			if (Vector3.Distance(new Vector3(interactX * 2, base.transform.position.y, interactY * 2), base.transform.position) >= 5f)
			{
				return false;
			}
			if (Vector3.Dot(base.transform.forward, (new Vector3(interactX * 2, base.transform.position.y, interactY * 2) - base.transform.position).normalized) < 0.7f)
			{
				return false;
			}
			if (base.transform.position.y < (float)num + 1.5f && base.transform.position.y > (float)num - 1.5f)
			{
				if (!placingDeed && (bool)tileObject && (bool)tileObject.tileOnOff && checkClientLocked() && tileObject.tileOnOff.isGate)
				{
					WorldManager.manageWorld.lockTileClient(interactX, interactY);
					CmdOpenClose(interactX, interactY);
					return true;
				}
				if (!placingDeed && (bool)tileObject && (bool)tileObject.showObjectOnStatusChange && tileObject.showObjectOnStatusChange.canBePickedUpByHand && WorldManager.manageWorld.onTileStatusMap[interactX, interactY] > 0)
				{
					CmdPickUpObjectOnTop(0, interactX, interactY);
					return true;
				}
				if (!placingDeed && (bool)tileObject && (bool)tileObject.tileObjectGrowthStages && checkClientLocked() && tileObject.tileObjectGrowthStages.canBeHarvested(WorldManager.manageWorld.onTileStatusMap[interactX, interactY]))
				{
					WorldManager.manageWorld.lockTileClient(interactX, interactY);
					if (tileObject.tileObjectGrowthStages.needsTilledSoil || ((bool)tileObject.tileObjectGrowthStages && (bool)tileObject.tileObjectConnect))
					{
						if (tileObject.tileObjectGrowthStages.diesOnHarvest)
						{
							CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Farming, Mathf.Clamp(tileObject.tileObjectGrowthStages.objectStages.Length / 3, 1, 12));
							DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.HarvestCrops);
						}
						else
						{
							DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.HarvestCrops);
							CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Farming, Mathf.Clamp(tileObject.tileObjectGrowthStages.objectStages.Length / 8, 1, 12));
						}
					}
					else if (tileObject.tileObjectGrowthStages.mustBeInWater)
					{
						CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Fishing, 3);
					}
					else if ((bool)tileObject.tileObjectGrowthStages.harvestDrop && (!tileObject.tileObjectGrowthStages.harvestDrop.placeable || tileObject.tileObjectGrowthStages.harvestDrop.placeable.tileObjectId != tileObject.tileObjectId))
					{
						CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Foraging, 1);
					}
					if (!tileObject.tileObjectGrowthStages.normalPickUp && !tileObject.tileObjectGrowthStages.autoPickUpOnHarvest)
					{
						InputMaster.input.harvestRumble();
						CmdHarvestOnTile(interactX, interactY, false);
						DailyTaskGenerator.generate.doATaskTileObject(DailyTaskGenerator.genericTaskType.Harvest, tileObject.tileObjectId, tileObject.tileObjectGrowthStages.harvestSpots.Length);
						return true;
					}
					int num2 = -1;
					if (tileObject.tileObjectGrowthStages.isCrabPot)
					{
						num2 = tileObject.tileObjectGrowthStages.getCrabTrapDrop(interactX, interactY);
						if (Inventory.inv.addItemToInventory(num2, 1))
						{
							if ((bool)Inventory.inv.allItems[num2].underwaterCreature)
							{
								CharLevelManager.manage.addToDayTally(num2, 1, 3);
							}
							SoundManager.manage.play2DSound(SoundManager.manage.pickUpItem);
							InputMaster.input.harvestRumble();
							CmdHarvestOnTile(interactX, interactY, true);
							return true;
						}
						NotificationManager.manage.turnOnPocketsFullNotification();
						InputMaster.input.harvestRumble();
						CmdHarvestCrabPot(interactX, interactY, num2);
						return true;
					}
					num2 = ((!tileObject.tileObjectGrowthStages.dropsFromLootTable) ? Inventory.inv.getInvItemId(tileObject.tileObjectGrowthStages.harvestDrop) : Inventory.inv.getInvItemId(tileObject.tileObjectGrowthStages.dropsFromLootTable.getRandomDropFromTable()));
					if (num2 != -1)
					{
						if (Inventory.inv.addItemToInventory(num2, 1))
						{
							CharLevelManager.manage.addToDayTally(num2, 1, 1);
							SoundManager.manage.play2DSound(SoundManager.manage.pickUpItem);
							InputMaster.input.harvestRumble();
							CmdHarvestOnTile(interactX, interactY, true);
							DailyTaskGenerator.generate.doATaskTileObject(DailyTaskGenerator.genericTaskType.Harvest, tileObject.tileObjectId);
							return true;
						}
						NotificationManager.manage.turnOnPocketsFullNotification();
						InputMaster.input.harvestRumble();
						CmdHarvestOnTile(interactX, interactY, false);
						DailyTaskGenerator.generate.doATaskTileObject(DailyTaskGenerator.genericTaskType.Harvest, tileObject.tileObjectId);
						return true;
					}
				}
			}
			return false;
		}
		return false;
	}

	public void pickUpTileObject()
	{
		if ((placingDeed || !objectAttacking || !objectAttacking.canBePickedUp()) && (placingDeed || !objectAttacking || !objectAttacking.tileObjectBridge || !WorldManager.manageWorld.waterMap[(int)selectedTile.x, (int)selectedTile.y]))
		{
			return;
		}
		if (insidePlayerHouse)
		{
			Vector2 vector = WorldManager.manageWorld.findMultiTileObjectPos((int)selectedTile.x, (int)selectedTile.y, insideHouseDetails);
			int num = insideHouseDetails.houseMapOnTile[(int)vector.x, (int)vector.y];
			if (objectAttacking == null && ItemOnTopManager.manage.hasItemsOnTop(objectAttacking.xPos, objectAttacking.yPos, insideHouseDetails))
			{
				MonoBehaviour.print("this has an item on top of nothing");
			}
			if (objectAttacking.canBePlaceOn() && ItemOnTopManager.manage.hasItemsOnTop(objectAttacking.xPos, objectAttacking.yPos, insideHouseDetails))
			{
				int num2 = objectAttacking.returnClosestPlacedPositionId(tileHighlighter.position);
				if (ItemOnTopManager.manage.getItemOnTopInPosition(num2, objectAttacking.xPos, objectAttacking.yPos, insideHouseDetails) != null)
				{
					MonoBehaviour.print("There is an item above the tile selector so I'm going to pick it up");
					ItemOnTop itemOnTopInPosition = ItemOnTopManager.manage.getItemOnTopInPosition(num2, objectAttacking.xPos, objectAttacking.yPos, insideHouseDetails);
					if (!WorldManager.manageWorld.allObjectSettings[itemOnTopInPosition.getTileObjectId()].pickUpRequiresEmptyPocket)
					{
						CmdPickUpObjectOnTopOfInside(num2, objectAttacking.xPos, objectAttacking.yPos);
					}
					else if (Inventory.inv.addItemToInventory(itemOnTopInPosition.getStatus(), 1))
					{
						CmdPickUpObjectOnTopOfInside(num2, objectAttacking.xPos, objectAttacking.yPos);
					}
					else
					{
						NotificationManager.manage.turnOnPocketsFullNotification();
					}
				}
				else
				{
					NotificationManager.manage.pocketsFull.showMustBeEmpty();
				}
			}
			else if (num > -1 && (bool)WorldManager.manageWorld.allObjects[num].tileObjectChest && !WorldManager.manageWorld.allObjects[num].tileObjectChest.checkIfEmpty((int)vector.x, (int)vector.y, insideHouseDetails))
			{
				NotificationManager.manage.pocketsFull.showMustBeEmpty();
			}
			else if (num > -1 && (bool)WorldManager.manageWorld.allObjects[num].tileObjectItemChanger && insideHouseDetails.houseMapOnTileStatus[(int)vector.x, (int)vector.y] > 0)
			{
				if (base.isServer)
				{
					WorldManager.manageWorld.checkIfTileHasChanger((int)vector.x, (int)vector.y, insideHouseDetails);
				}
				NotificationManager.manage.pocketsFull.showMustBeEmpty();
			}
			else if (num > -1 && (bool)WorldManager.manageWorld.allObjects[num].tileObjectFurniture && insideHouseDetails.houseMapOnTileStatus[(int)selectedTile.x, (int)selectedTile.y] >= 1)
			{
				NotificationManager.manage.pocketsFull.showMustBeEmpty();
			}
			else if (num > -1)
			{
				if (!WorldManager.manageWorld.allObjectSettings[num].pickUpRequiresEmptyPocket || (WorldManager.manageWorld.allObjectSettings[num].pickUpRequiresEmptyPocket && Inventory.inv.addItemToInventory(insideHouseDetails.houseMapOnTileStatus[(int)vector.x, (int)vector.y], 1)))
				{
					CmdPickUpOnTileInside((int)selectedTile.x, (int)selectedTile.y, playerHouseTransform.position.y);
				}
				else
				{
					NotificationManager.manage.pocketsFull.showPocketsFull(false);
				}
			}
		}
		else if (objectAttacking.canBePlaceOn() && ItemOnTopManager.manage.hasItemsOnTop(objectAttacking.xPos, objectAttacking.yPos))
		{
			int num3 = objectAttacking.returnClosestPlacedPositionId(tileHighlighter.position);
			if (ItemOnTopManager.manage.getItemOnTopInPosition(num3, objectAttacking.xPos, objectAttacking.yPos, null) != null)
			{
				MonoBehaviour.print("There is an item above the tile selector so I'm going to pick it up");
				ItemOnTop itemOnTopInPosition2 = ItemOnTopManager.manage.getItemOnTopInPosition(num3, objectAttacking.xPos, objectAttacking.yPos, null);
				if (!WorldManager.manageWorld.allObjectSettings[itemOnTopInPosition2.getTileObjectId()].pickUpRequiresEmptyPocket)
				{
					WorldManager.manageWorld.lockTileClient(objectAttacking.xPos, objectAttacking.yPos);
					CmdPickUpObjectOnTopOf(num3, objectAttacking.xPos, objectAttacking.yPos);
				}
				else if (Inventory.inv.addItemToInventory(itemOnTopInPosition2.getStatus(), 1))
				{
					WorldManager.manageWorld.lockTileClient(objectAttacking.xPos, objectAttacking.yPos);
					CmdPickUpObjectOnTopOf(num3, objectAttacking.xPos, objectAttacking.yPos);
				}
				else
				{
					NotificationManager.manage.turnOnPocketsFullNotification();
				}
			}
			else
			{
				NotificationManager.manage.pocketsFull.showMustBeEmpty();
			}
		}
		else if ((bool)objectAttacking && (bool)WorldManager.manageWorld.allObjects[objectAttacking.tileObjectId].tileObjectChest && !WorldManager.manageWorld.allObjects[objectAttacking.tileObjectId].tileObjectChest.checkIfEmpty((int)selectedTile.x, (int)selectedTile.y, insideHouseDetails))
		{
			NotificationManager.manage.pocketsFull.showMustBeEmpty();
		}
		else if ((bool)WorldManager.manageWorld.allObjects[objectAttacking.tileObjectId].tileObjectFurniture && WorldManager.manageWorld.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y] >= 1)
		{
			NotificationManager.manage.pocketsFull.showMustBeEmpty();
		}
		else if ((bool)WorldManager.manageWorld.allObjects[objectAttacking.tileObjectId].tileObjectItemChanger && WorldManager.manageWorld.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y] > 0)
		{
			if (base.isServer)
			{
				WorldManager.manageWorld.checkIfTileHasChanger((int)selectedTile.x, (int)selectedTile.y);
			}
			NotificationManager.manage.pocketsFull.showMustBeEmpty();
		}
		else if ((bool)objectAttacking && WorldManager.manageWorld.allObjectSettings[objectAttacking.tileObjectId].pickUpRequiresEmptyPocket)
		{
			if (WorldManager.manageWorld.allObjectSettings[objectAttacking.tileObjectId].dropsStatusNumberOnDeath)
			{
				if (Inventory.inv.addItemToInventory(WorldManager.manageWorld.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y], 1))
				{
					CmdPickUpOnTile((int)selectedTile.x, (int)selectedTile.y);
				}
				else
				{
					NotificationManager.manage.turnOnPocketsFullNotification();
				}
			}
		}
		else
		{
			CmdPickUpOnTile((int)selectedTile.x, (int)selectedTile.y);
		}
	}

	public void spawnPlaceableObject()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (!canPlaceVehiclePreview)
		{
			if (tileHighlighter.position.y > base.transform.position.y + 2f)
			{
				SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
				return;
			}
			CmdSpawnPlaceable(new Vector3(tileHighlighter.transform.position.x, base.transform.position.y + 1.4f, tileHighlighter.transform.position.z), Inventory.inv.getInvItemId(myEquip.currentlyHolding));
			Inventory.inv.consumeItemInHand();
		}
		else if ((bool)canPlaceVehiclePreview && canPlaceVehiclePreview.canBePlaced)
		{
			CmdSpawnVehicle(Inventory.inv.getInvItemId(myEquip.currentlyHolding), placeableRotation, Inventory.inv.invSlots[Inventory.inv.selectedSlot].stack - 1);
			Inventory.inv.consumeItemInHand();
		}
		else
		{
			SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
		}
	}

	public void doDamage(bool useStamina = true)
	{
		if (base.isLocalPlayer && useStamina)
		{
			refreshTileSelection((int)selectedTile.x, (int)selectedTile.y);
		}
		bool flag = true;
		if (!base.isLocalPlayer)
		{
			if (WorldManager.manageWorld.onTileMap[(int)currentlyAttackingPos.x, (int)currentlyAttackingPos.y] != currentlyAttackingId)
			{
				OnChangeAttackingPos(currentlyAttackingPos, currentlyAttackingPos);
			}
			if ((bool)objectAttacking && checkIfCanDamage(currentlyAttackingPos))
			{
				if (myMove.stamina <= 10 && useStamina)
				{
					StatusManager.manage.sweatParticlesNotLocal(base.transform.position);
				}
				objectAttacking.damage();
				objectAttacking.currentHealth -= myEquip.currentlyHolding.damagePerAttack;
				Vector3 position = objectAttacking.transform.position;
				ParticleManager.manage.emitAttackParticle(position);
			}
			return;
		}
		if (insidePlayerHouse)
		{
			if (((bool)myEquip.currentlyHolding && (bool)myEquip.currentlyHolding.equipable && myEquip.currentlyHolding.equipable.wallpaper) || ((bool)myEquip.currentlyHolding && (bool)myEquip.currentlyHolding.equipable && myEquip.currentlyHolding.equipable.flooring))
			{
				if (myEquip.currentlyHolding.equipable.wallpaper)
				{
					Inventory.inv.addItemToInventory(Inventory.inv.wallSlot.itemNo, 1);
					CmdUpdateHouseWall(Inventory.inv.getInvItemId(myEquip.currentlyHolding), insideHouseDetails.xPos, insideHouseDetails.yPos);
				}
				else
				{
					Inventory.inv.addItemToInventory(Inventory.inv.floorSlot.itemNo, 1);
					CmdUpdateHouseFloor(Inventory.inv.getInvItemId(myEquip.currentlyHolding), insideHouseDetails.xPos, insideHouseDetails.yPos);
				}
				Inventory.inv.consumeItemInHand();
			}
			else if ((bool)myEquip.currentlyHolding && (bool)myEquip.currentlyHolding.placeable && myEquip.currentlyHolding.placeable.canBePlacedOntoFurniture() && (bool)objectAttacking && objectAttacking.canBePlaceOn())
			{
				if (!placingDeed)
				{
					changePlacingDeedBool(true);
					return;
				}
				int num = objectAttacking.returnClosestPlacedPositionId(tileHighlighter.position);
				if (ItemOnTopManager.manage.getItemOnTopInPosition(num, objectAttacking.xPos, objectAttacking.yPos, insideHouseDetails) == null)
				{
					int status = 0;
					if (myEquip.usesHandPlaceable())
					{
						status = myEquip.currentlyHolding.getItemId();
					}
					CmdPlaceItemOnTopOfInside(myEquip.currentlyHolding.placeable.tileObjectId, num, status, previewShowingRot, objectAttacking.xPos, objectAttacking.yPos);
					Inventory.inv.consumeItemInHand();
				}
			}
			else if (myEquip.currentlyHoldingMultiTiledPlaceableItem())
			{
				if (!placingDeed)
				{
					changePlacingDeedBool(true);
				}
				else if (myEquip.currentlyHolding.placeable.checkIfMultiTileObjectCanBePlacedInside(insideHouseDetails, (int)selectedTile.x, (int)selectedTile.y, placeableRotation))
				{
					CmdPlayPlaceableAnimation();
					int tileObjectId = myEquip.currentlyHolding.placeable.tileObjectId;
					CmdChangeOnTileInside(tileObjectId, (int)selectedTile.x, (int)selectedTile.y, placeableRotation);
					if (tileObjectId > -1 && WorldManager.manageWorld.allObjectSettings[tileObjectId].dropsStatusNumberOnDeath)
					{
						CmdGiveStatusInside(Inventory.inv.getInvItemId(myEquip.currentlyHolding), (int)selectedTile.x, (int)selectedTile.y, insideHouseDetails.xPos, insideHouseDetails.yPos);
					}
					if (!myEquip.currentlyHolding.isATool)
					{
						Inventory.inv.consumeItemInHand();
					}
					refreshSelection = true;
				}
			}
			else
			{
				if (!myEquip.currentlyHoldingSinglePlaceableItem())
				{
					return;
				}
				if (!placingDeed)
				{
					changePlacingDeedBool(true);
				}
				else if (checkIfCanDamageInside(selectedTile))
				{
					CmdPlayPlaceableAnimation();
					int tileObjectId2 = myEquip.currentlyHolding.placeable.tileObjectId;
					CmdChangeOnTileInside(tileObjectId2, (int)selectedTile.x, (int)selectedTile.y, placeableRotation);
					if (tileObjectId2 > -1 && WorldManager.manageWorld.allObjectSettings[tileObjectId2].dropsStatusNumberOnDeath)
					{
						CmdGiveStatusInside(Inventory.inv.getInvItemId(myEquip.currentlyHolding), (int)selectedTile.x, (int)selectedTile.y, insideHouseDetails.xPos, insideHouseDetails.yPos);
					}
					if (!myEquip.currentlyHolding.isATool)
					{
						Inventory.inv.consumeItemInHand();
					}
					refreshSelection = true;
				}
			}
			return;
		}
		if ((bool)myEquip.currentlyHolding && (bool)myEquip.currentlyHolding.placeable && myEquip.currentlyHolding.placeable.canBePlacedOntoFurniture() && (bool)objectAttacking && objectAttacking.canBePlaceOn())
		{
			int num2 = objectAttacking.returnClosestPlacedPositionId(tileHighlighter.position);
			if (ItemOnTopManager.manage.getItemOnTopInPosition(num2, objectAttacking.xPos, objectAttacking.yPos, null) == null)
			{
				WorldManager.manageWorld.lockTileClient(objectAttacking.xPos, objectAttacking.yPos);
				int status2 = 0;
				if (WorldManager.manageWorld.allObjectSettings[myEquip.currentlyHolding.placeable.tileObjectId].dropsStatusNumberOnDeath)
				{
					status2 = myEquip.currentlyHolding.getItemId();
				}
				CmdPlaceItemOnTopOf(myEquip.currentlyHolding.placeable.tileObjectId, num2, status2, previewShowingRot, objectAttacking.xPos, objectAttacking.yPos);
				Inventory.inv.consumeItemInHand();
			}
		}
		else if (canAttackSelectedTile && myEquip.currentlyHolding.placeableTileType != -1 && checkClientLocked())
		{
			if (!myEquip.currentlyHolding.hasFuel || Inventory.inv.hasFuelAndCanBeUsed())
			{
				if ((WorldManager.manageWorld.tileTypes[myEquip.currentlyHolding.getResultingPlaceableTileType(WorldManager.manageWorld.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y])].isWetFertilizedDirt || WorldManager.manageWorld.tileTypes[myEquip.currentlyHolding.getResultingPlaceableTileType(WorldManager.manageWorld.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y])].isWetTilledDirt) && WorldManager.manageWorld.onTileMap[(int)selectedTile.x, (int)selectedTile.y] > -1 && (bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[(int)selectedTile.x, (int)selectedTile.y]].tileObjectGrowthStages && WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[(int)selectedTile.x, (int)selectedTile.y]].tileObjectGrowthStages.needsTilledSoil)
				{
					DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.WaterCrops);
					if (Random.Range(0, 5) == 2)
					{
						CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Farming, 1);
					}
				}
				if (WorldManager.manageWorld.onTileMap[(int)selectedTile.x, (int)selectedTile.y] > -1 && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[(int)selectedTile.x, (int)selectedTile.y]].isGrass)
				{
					changeTile();
				}
				myEquip.currentlyHolding.checkForTask();
				changeTileType(myEquip.currentlyHolding.getResultingPlaceableTileType(WorldManager.manageWorld.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]));
				CmdPlayPlaceableAnimation();
				WorldManager.manageWorld.lockTileClient((int)selectedTile.x, (int)selectedTile.y);
			}
			if (!myEquip.currentlyHolding.isATool)
			{
				Inventory.inv.consumeItemInHand();
			}
		}
		else if (myEquip.currentlyHoldingSinglePlaceableItem() && !myEquip.currentlyHolding.burriedPlaceable && checkClientLocked() && canAttackSelectedTile)
		{
			WorldManager.manageWorld.lockTileClient((int)selectedTile.x, (int)selectedTile.y);
			if ((bool)myEquip.currentlyHolding.placeable.tileObjectGrowthStages && myEquip.currentlyHolding.placeable.tileObjectGrowthStages.needsTilledSoil)
			{
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.PlantSeeds);
			}
			CmdPlayPlaceableAnimation();
			changeTile(myEquip.currentlyHolding.placeable.tileObjectId, placeableRotation);
			Inventory.inv.consumeItemInHand();
			refreshSelection = true;
		}
		else if (checkClientLocked() && myEquip.currentlyHoldingMultiTiledPlaceableItem())
		{
			if (!placingDeed)
			{
				changePlacingDeedBool(true);
			}
			else if (myEquip.currentlyHoldingDeed() && !ConversationManager.manage.inConversation && myEquip.currentlyHolding.placeable.checkIfDeedCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placeableRotation))
			{
				if (base.isServer)
				{
					ConversationManager.manage.talkToNPC(NPCManager.manage.sign, myEquip.confirmDeedConvo);
				}
				else
				{
					ConversationManager.manage.talkToNPC(NPCManager.manage.sign, myEquip.confirmDeedNotServer);
				}
			}
			else if ((myEquip.currentlyHoldingDeed() && myEquip.currentlyHolding.placeable.checkIfDeedCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placeableRotation)) || (!myEquip.currentlyHoldingDeed() && !myEquip.currentlyHolding.placeable.tileObjectBridge && myEquip.currentlyHolding.placeable.checkIfMultiTileObjectCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placeableRotation)) || (!myEquip.currentlyHoldingDeed() && (bool)myEquip.currentlyHolding.placeable.tileObjectBridge && myEquip.currentlyHolding.placeable.checkIfBridgeCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placeableRotation)))
			{
				WorldManager.manageWorld.lockTileClient((int)selectedTile.x, (int)selectedTile.y);
				CmdPlayPlaceableAnimation();
				if ((bool)myEquip.currentlyHolding.placeable.tileObjectBridge)
				{
					int length = 2;
					if (placeableRotation == 1)
					{
						length = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, 0, -1);
					}
					else if (placeableRotation == 2)
					{
						length = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, -1);
					}
					else if (placeableRotation == 3)
					{
						length = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, 0, 1);
					}
					else if (placeableRotation == 4)
					{
						length = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, 1);
					}
					CmdPlaceBridgeTileObject(myEquip.currentlyHolding.placeable.tileObjectId, (int)selectedTile.x, (int)selectedTile.y, placeableRotation, length);
				}
				else
				{
					int tileObjectId3 = myEquip.currentlyHolding.placeable.tileObjectId;
					CmdPlaceMultiTiledObject(tileObjectId3, (int)selectedTile.x, (int)selectedTile.y, placeableRotation);
					if (tileObjectId3 > -1 && WorldManager.manageWorld.allObjectSettings[tileObjectId3].dropsStatusNumberOnDeath)
					{
						CmdGiveStatus(Inventory.inv.getInvItemId(myEquip.currentlyHolding), (int)selectedTile.x, (int)selectedTile.y);
					}
				}
				Inventory.inv.consumeItemInHand();
			}
		}
		else if (checkIfPlaceableOnSelectedTileObject() && checkClientLocked())
		{
			CmdPlaceItemInToTileObject(myEquip.currentlyHolding.statusToChangeToWhenPlacedOnTop, (int)selectedTile.x, (int)selectedTile.y);
			WorldManager.manageWorld.lockTileClient((int)selectedTile.x, (int)selectedTile.y);
			Inventory.inv.consumeItemInHand();
		}
		else if ((bool)objectAttacking && canAttackSelectedTile)
		{
			objectAttacking.currentHealth -= myEquip.currentlyHolding.damagePerAttack + (float)objectAttacking.getEffectedBuffLevel() * (myEquip.currentlyHolding.damagePerAttack / 2f);
			InputMaster.input.doRumble(0.2f);
			CameraController.control.shakeScreenMax(0.05f, 0.05f);
			if (!WorldManager.manageWorld.checkTileClientLock((int)selectedTile.x, (int)selectedTile.y) && (bool)objectAttacking && (bool)objectAttacking.tileObjectGrowthStages && objectAttacking.tileObjectGrowthStages.canBeHarvested(WorldManager.manageWorld.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y], true))
			{
				WorldManager.manageWorld.lockTileClient((int)selectedTile.x, (int)selectedTile.y);
				if (objectAttacking.tileObjectGrowthStages.needsTilledSoil)
				{
					CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Farming, 1);
				}
				else if ((bool)objectAttacking)
				{
					DailyTaskGenerator.generate.doATaskTileObject(DailyTaskGenerator.genericTaskType.Harvest, objectAttacking.tileObjectId);
				}
				CmdHarvestOnTileDeath((int)selectedTile.x, (int)selectedTile.y);
			}
			Vector3 position2 = objectAttacking.transform.position;
			ParticleManager.manage.emitAttackParticle(position2);
			objectAttacking.damage();
			if (objectAttacking.currentHealth <= 0f)
			{
				float rumbleMax = Mathf.Clamp(WorldManager.manageWorld.allObjectSettings[objectAttacking.tileObjectId].fullHealth / 15f, 0.1f, 0.5f);
				InputMaster.input.doRumble(rumbleMax, 1.5f);
				if (((bool)objectAttacking.tileObjectGrowthStages && objectAttacking.tileObjectGrowthStages.needsTilledSoil) || ((bool)objectAttacking.tileObjectGrowthStages && (bool)objectAttacking.tileObjectGrowthStages && (bool)objectAttacking.tileObjectConnect && (bool)objectAttacking.tileObjectConnect.canConnectTo[0].tileObjectGrowthStages && objectAttacking.tileObjectConnect.canConnectTo[0].tileObjectGrowthStages.needsTilledSoil))
				{
					if (objectAttacking.tileObjectGrowthStages.diesOnHarvest)
					{
						CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Farming, Mathf.Clamp(objectAttacking.tileObjectGrowthStages.objectStages.Length / 3, 1, 12));
						DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.HarvestCrops);
					}
					else
					{
						DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.HarvestCrops);
						CharLevelManager.manage.addXp(CharLevelManager.SkillTypes.Farming, Mathf.Clamp(objectAttacking.tileObjectGrowthStages.objectStages.Length / 8, 1, 12));
					}
				}
				changeTile(objectAttacking.getTileObjectChangeToOnDeath());
				refreshSelection = true;
			}
			if (myEquip.currentlyHolding.changeToHeightTiles != 0 && !myEquip.currentlyHolding.onlyChangeHeightPaths)
			{
				if ((bool)WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].dropOnChange)
				{
					changeTileType(0);
				}
				else if ((myEquip.currentlyHolding.changeToHeightTiles < 0 && (bool)WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].dropOnChange) || (myEquip.currentlyHolding.changeToHeightTiles < 0 && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].saveUnderTile) || (myEquip.currentlyHolding.changeToHeightTiles < 0 && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].changeTileKeepUnderTile))
				{
					changeTileType(0);
				}
				else
				{
					changeTileHeight(myEquip.currentlyHolding.changeToHeightTiles);
					if ((bool)myEquip.currentlyHolding.changeToWhenUsed && myEquip.currentlyHolding.changeToAndStillUseFuel)
					{
						Inventory.inv.changeItemInHand();
					}
				}
			}
		}
		else if (canAttackSelectedTile && myEquip.currentlyHolding.changeToHeightTiles != 0)
		{
			if ((bool)WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].dropOnChange)
			{
				changeTileType(0);
			}
			else if ((myEquip.currentlyHolding.changeToHeightTiles < 0 && (bool)WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].dropOnChange) || (myEquip.currentlyHolding.changeToHeightTiles < 0 && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].saveUnderTile) || (myEquip.currentlyHolding.changeToHeightTiles < 0 && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].changeTileKeepUnderTile))
			{
				changeTileType(0);
			}
			else
			{
				changeTileHeight(myEquip.currentlyHolding.changeToHeightTiles);
				if ((bool)myEquip.currentlyHolding.changeToWhenUsed && myEquip.currentlyHolding.changeToAndStillUseFuel)
				{
					Inventory.inv.changeItemInHand();
				}
			}
		}
		else if ((bool)objectAttacking && (bool)objectAttacking.tileOnOff && checkClientLocked())
		{
			if (WorldManager.manageWorld.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y] == 0 && (bool)myEquip.currentlyHolding && myEquip.currentlyHolding == objectAttacking.tileOnOff.requiredToOpen)
			{
				if (objectAttacking.tileOnOff.taskWhenUnlocked != 0)
				{
					DailyTaskGenerator.generate.doATask(objectAttacking.tileOnOff.taskWhenUnlocked);
				}
				Inventory.inv.consumeItemInHand();
				CmdPlayPlaceableAnimation();
				WorldManager.manageWorld.lockTileClient((int)selectedTile.x, (int)selectedTile.y);
				CmdFillFood((int)selectedTile.x, (int)selectedTile.y);
			}
		}
		else
		{
			flag = false;
		}
		if (flag && (bool)myEquip.currentlyHolding && myEquip.currentlyHolding.isATool && useStamina)
		{
			if ((bool)objectAttacking && WorldManager.manageWorld.allObjectSettings[objectAttacking.tileObjectId].isGrass)
			{
				if (!myEquip.currentlyHolding.isPowerTool)
				{
					StatusManager.manage.changeStamina(0f - CharLevelManager.manage.getStaminaCost(-1));
				}
				else
				{
					StatusManager.manage.changeStamina((0f - CharLevelManager.manage.getStaminaCost(-1)) / 3f);
				}
			}
			else if (!myEquip.currentlyHolding.isPowerTool)
			{
				StatusManager.manage.changeStamina(0f - myEquip.currentlyHolding.getStaminaCost());
			}
			else
			{
				StatusManager.manage.changeStamina((0f - myEquip.currentlyHolding.getStaminaCost()) / 3f);
			}
		}
		if ((bool)myEquip.currentlyHolding && myEquip.currentlyHolding.hasFuel && checkIfCanDamage(selectedTile))
		{
			Inventory.inv.useItemWithFuel();
		}
	}

	public void insertItemInTo(ItemDepositAndChanger changer)
	{
		if (changer.canDepositThisItem(myEquip.currentlyHolding, insideHouseDetails))
		{
			myEquip.currentlyHolding.itemChange.checkTask(changer.GetComponent<TileObject>().tileObjectId);
			if (insidePlayerHouse)
			{
				CmdDepositItemInside(myEquip.currentlyHoldingNo, changer.currentXPos, changer.currentYPos, insideHouseDetails.xPos, insideHouseDetails.yPos);
			}
			else
			{
				CmdDepositItem(myEquip.currentlyHoldingNo, changer.currentXPos, changer.currentYPos);
			}
			WorldManager.manageWorld.lockTileClient(changer.currentXPos, changer.currentYPos);
			Inventory.inv.placeItemIntoSomething(myEquip.currentlyHolding, changer);
		}
	}

	public bool checkIfPlaceableOnSelectedTileObject()
	{
		if ((bool)myEquip.currentlyHolding && myEquip.currentlyHolding.canBePlacedOnToTileObject.Length != 0 && WorldManager.manageWorld.onTileStatusMap[(int)selectedTile.x, (int)selectedTile.y] <= 0)
		{
			for (int i = 0; i < myEquip.currentlyHolding.canBePlacedOnToTileObject.Length; i++)
			{
				if (myEquip.currentlyHolding.canBePlacedOnToTileObject[i].tileObjectId == WorldManager.manageWorld.onTileMap[(int)selectedTile.x, (int)selectedTile.y])
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool checkIfCanDamage(Vector2 selectedTile)
	{
		int num = (int)selectedTile.x;
		int num2 = (int)selectedTile.y;
		if ((bool)myEquip.currentlyHolding && !myEquip.currentlyHolding.anyHeight && (!(myEquip.transform.position.y <= (float)WorldManager.manageWorld.heightMap[num, num2] + 1.5f) || !(myEquip.transform.position.y >= (float)WorldManager.manageWorld.heightMap[num, num2] - 1.5f)))
		{
			return false;
		}
		if (!myEquip.currentlyHolding)
		{
			return false;
		}
		if (((bool)myEquip.currentlyHolding.placeable && WorldManager.manageWorld.onTileMap[num, num2] == -1) || ((bool)myEquip.currentlyHolding.placeable && WorldManager.manageWorld.onTileMap[num, num2] == 30) || ((bool)myEquip.currentlyHolding.placeable && WorldManager.manageWorld.onTileMap[num, num2] > -1 && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[num, num2]].isGrass))
		{
			if (myEquip.currentlyHolding.canBePlacedOntoTileType.Length == 0)
			{
				return true;
			}
			for (int i = 0; i < myEquip.currentlyHolding.canBePlacedOntoTileType.Length; i++)
			{
				if (myEquip.currentlyHolding.canBePlacedOntoTileType[i] == WorldManager.manageWorld.tileTypeMap[num, num2])
				{
					return true;
				}
			}
			return false;
		}
		if (!myEquip.currentlyHolding.ignoreOnTileObject && WorldManager.manageWorld.onTileMap[num, num2] > -1 && WorldManager.manageWorld.onTileMap[num, num2] != 30)
		{
			TileObjectSettings tileObjectSettings = WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[num, num2]];
			if (myEquip.currentlyHolding.placeableTileType > -1 && WorldManager.manageWorld.tileTypes[myEquip.currentlyHolding.placeableTileType].isPath && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[num, num2]].isGrass)
			{
				return true;
			}
			if (myEquip.currentlyHolding.placeableTileType > -1 && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[num, num2]].isGrass && (WorldManager.manageWorld.tileTypes[myEquip.currentlyHolding.placeableTileType].isTilledDirt || WorldManager.manageWorld.tileTypes[myEquip.currentlyHolding.placeableTileType].isFertilizedDirt || WorldManager.manageWorld.tileTypes[myEquip.currentlyHolding.placeableTileType].isWetFertilizedDirt || WorldManager.manageWorld.tileTypes[myEquip.currentlyHolding.placeableTileType].isWetTilledDirt))
			{
				return true;
			}
			if ((tileObjectSettings.isStone && myEquip.currentlyHolding.damageStone) || (tileObjectSettings.isHardStone && myEquip.currentlyHolding.damageHardStone) || (tileObjectSettings.isWood && myEquip.currentlyHolding.damageWood) || (tileObjectSettings.isHardWood && myEquip.currentlyHolding.damageHardWood) || (tileObjectSettings.isMetal && myEquip.currentlyHolding.damageMetal) || (tileObjectSettings.isSmallPlant && myEquip.currentlyHolding.damageSmallPlants))
			{
				return true;
			}
			return false;
		}
		if (myEquip.currentlyHolding.ignoreOnTileObject || WorldManager.manageWorld.onTileMap[num, num2] == -1 || WorldManager.manageWorld.onTileMap[num, num2] == 30 || (WorldManager.manageWorld.onTileMap[num, num2] > -1 && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[num, num2]].isGrass))
		{
			if (myEquip.currentlyHolding.canDamagePath && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].isPath)
			{
				return true;
			}
			if (myEquip.currentlyHolding.grassGrowable && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].isGrassGrowable)
			{
				return true;
			}
			if (myEquip.currentlyHolding.canDamageDirt && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].isDirt)
			{
				if (myEquip.currentlyHolding.changeToHeightTiles != 0 && (myEquip.currentlyHolding.changeToHeightTiles != -1 || WorldManager.manageWorld.heightMap[num, num2] <= -5))
				{
					if (myEquip.currentlyHolding.changeToHeightTiles != 1)
					{
						return true;
					}
					int num3 = WorldManager.manageWorld.heightMap[num, num2];
					int num4 = 12;
				}
				return true;
			}
			if (myEquip.currentlyHolding.canDamageStone && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].isStone)
			{
				return true;
			}
			if (myEquip.currentlyHolding.canDamageTilledDirt && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].isTilledDirt)
			{
				return true;
			}
			if (myEquip.currentlyHolding.canDamageWetTilledDirt && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].isWetTilledDirt)
			{
				return true;
			}
			if (myEquip.currentlyHolding.canDamageFertilizedSoil && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].isFertilizedDirt)
			{
				return true;
			}
			if (myEquip.currentlyHolding.canDamageWetFertilizedSoil && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num, num2]].isWetFertilizedDirt)
			{
				return true;
			}
		}
		return false;
	}

	public bool checkIfCanDamageInside(Vector2 selectedTile)
	{
		if (!myEquip.currentlyHolding)
		{
			return false;
		}
		if ((bool)myEquip.currentlyHolding.placeable && insideHouseDetails.houseMapOnTile[(int)selectedTile.x, (int)selectedTile.y] == -1)
		{
			return true;
		}
		return false;
	}

	public void smoothMoveHighlighterAndPreview()
	{
		if (Vector3.Distance(desiredHighlighterPos, tileHighlighter.position) > 6f)
		{
			tileHighlighter.position = desiredHighlighterPos;
		}
		else
		{
			tileHighlighter.position = Vector3.Lerp(tileHighlighter.position, desiredHighlighterPos, Time.deltaTime * 15f);
		}
		if ((bool)previewObject)
		{
			if (Vector3.Distance(desiredPreviewPos, previewObject.transform.position) > 6f)
			{
				previewObject.transform.position = desiredPreviewPos;
			}
			else
			{
				previewObject.transform.position = Vector3.Lerp(previewObject.transform.position, desiredPreviewPos, Time.deltaTime * 15f);
			}
		}
		if ((bool)vehiclePreview)
		{
			if (Vector3.Distance(desiredPreviewPos, vehiclePreview.transform.position) > 6f)
			{
				vehiclePreview.transform.position = desiredPreviewPos;
			}
			else
			{
				vehiclePreview.transform.position = Vector3.Lerp(vehiclePreview.transform.position, desiredPreviewPos, Time.deltaTime * 15f);
			}
		}
	}

	public void createPreviewOnHighlighter(int xPos, int yPos)
	{
		if (!myEquip.currentlyHolding)
		{
			highLighterChange.showNormal();
			if (insidePlayerHouse)
			{
				desiredHighlighterPos = playerHouseTransform.position + new Vector3(xPos * 2, -0.5f, yPos * 2);
			}
			else
			{
				desiredHighlighterPos = new Vector3(xPos * 2, (float)WorldManager.manageWorld.heightMap[xPos, yPos] - 0.5f, yPos * 2);
			}
		}
		else if ((bool)myEquip.currentlyHolding.placeable && myEquip.currentlyHolding.placeable.isMultiTileObject() && placingDeed)
		{
			if (!placingDeed)
			{
				highLighterChange.showNormal();
				if (insidePlayerHouse)
				{
					desiredHighlighterPos = playerHouseTransform.position + new Vector3(xPos * 2, -0.5f, yPos * 2);
				}
				else
				{
					desiredHighlighterPos = new Vector3(xPos * 2, (float)WorldManager.manageWorld.heightMap[xPos, yPos] - 0.5f, yPos * 2);
				}
				return;
			}
			int num = myEquip.currentlyHolding.placeable.getXSize();
			int num2 = myEquip.currentlyHolding.placeable.getYSize();
			if (placeableRotation == 2 || placeableRotation == 4)
			{
				num = myEquip.currentlyHolding.placeable.getYSize();
				num2 = myEquip.currentlyHolding.placeable.getXSize();
			}
			if ((bool)myEquip.currentlyHolding.placeable.tileObjectBridge)
			{
				int num3 = 2;
				if (placeableRotation == 1)
				{
					num3 = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, 0, -1);
					if (num3 <= 15)
					{
						num2 = num3;
						yPos -= num3 - 1;
					}
				}
				else if (placeableRotation == 2)
				{
					num3 = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, -1);
					if (num3 <= 15)
					{
						num = num3;
						xPos -= num3 - 1;
					}
				}
				else if (placeableRotation == 3)
				{
					num3 = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, 0, 1);
					if (num3 <= 15)
					{
						num2 = num3;
					}
				}
				else if (placeableRotation == 4)
				{
					num3 = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, 1);
					if (num3 <= 15)
					{
						num = num3;
					}
				}
			}
			highLighterChange.showMultiTiled(num, num2);
			if (insidePlayerHouse)
			{
				desiredHighlighterPos = playerHouseTransform.position + new Vector3(xPos * 2 + (num - 1), -0.5f, yPos * 2 + (num2 - 1));
			}
			else
			{
				desiredHighlighterPos = new Vector3(xPos * 2 + (num - 1), (float)WorldManager.manageWorld.heightMap[xPos, yPos] - 0.5f, yPos * 2 + (num2 - 1));
			}
		}
		else
		{
			highLighterChange.showNormal();
			if (insidePlayerHouse)
			{
				desiredHighlighterPos = playerHouseTransform.position + new Vector3(xPos * 2, -0.5f, yPos * 2);
			}
			else
			{
				desiredHighlighterPos = new Vector3(xPos * 2, (float)WorldManager.manageWorld.heightMap[xPos, yPos] - 0.5f, yPos * 2);
			}
		}
	}

	public void followingNPCKillsItem(int xPos, int yPos, TileObject npcAttackingObject)
	{
		if ((bool)npcAttackingObject)
		{
			npcAttackingObject.onDeath();
			npcAttackingObject.addXp();
			DailyTaskGenerator.generate.doATask(WorldManager.manageWorld.allObjectSettings[npcAttackingObject.tileObjectId].TaskType);
		}
		CmdChangeOnTile(npcAttackingObject.getTileObjectChangeToOnDeath(), xPos, yPos);
	}

	public void changeTile(int newOnTile = -1, int rotation = -1)
	{
		if ((bool)objectAttacking)
		{
			objectAttacking.onDeath();
			objectAttacking.addXp();
			DailyTaskGenerator.generate.doATask(WorldManager.manageWorld.allObjectSettings[objectAttacking.tileObjectId].TaskType);
		}
		if (rotation != -1)
		{
			CmdSetRotation((int)selectedTile.x, (int)selectedTile.y, rotation);
		}
		CmdChangeOnTile(newOnTile, (int)selectedTile.x, (int)selectedTile.y);
		if (newOnTile > -1 && WorldManager.manageWorld.allObjectSettings[newOnTile].dropsStatusNumberOnDeath)
		{
			CmdGiveStatus(Inventory.inv.getInvItemId(myEquip.currentlyHolding), (int)selectedTile.x, (int)selectedTile.y);
		}
		refreshSelection = true;
	}

	public void changeTileHeight(int heightDif)
	{
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SoilMover);
		InputMaster.input.doRumble(0.15f, 1.5f);
		if (heightDif > 0 && (bool)myEquip.currentlyHolding && myEquip.currentlyHolding.useRightHandAnim && myEquip.currentlyHolding.myAnimType == InventoryItem.typeOfAnimation.ShovelAnimation)
		{
			CmdChangeTileHeight(heightDif, myEquip.currentlyHolding.resultingTileType[0], (int)selectedTile.x, (int)selectedTile.y);
		}
		else
		{
			CmdChangeTileHeight(heightDif, WorldManager.manageWorld.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y], (int)selectedTile.x, (int)selectedTile.y);
		}
		refreshSelection = true;
	}

	public void changeTileType(int newTileType)
	{
		CmdChangeTileType(newTileType, (int)selectedTile.x, (int)selectedTile.y);
		refreshSelection = true;
	}

	public Vector3 highLighterPosDif()
	{
		if ((!myEquip.currentlyHoldingDeed() || placingDeed) && myEquip.currentlyHoldingMultiTiledPlaceableItem() && placingDeed)
		{
			int num = myEquip.currentlyHolding.placeable.getXSize();
			int num2 = myEquip.currentlyHolding.placeable.getYSize();
			if (placeableRotation == 2 || placeableRotation == 4)
			{
				num = myEquip.currentlyHolding.placeable.getYSize();
				num2 = myEquip.currentlyHolding.placeable.getXSize();
			}
			float num3 = 0f;
			float num4 = 0f;
			if ((base.transform.position + base.transform.forward).x < base.transform.position.x - 0.8f)
			{
				num3 = -num * 2;
			}
			else if ((base.transform.position + base.transform.forward).x > base.transform.position.x + 0.8f)
			{
				num3 = 2f;
			}
			if ((base.transform.position + base.transform.forward).z < base.transform.position.z - 0.8f)
			{
				num4 = -num2 * 2 + 2;
			}
			else if ((base.transform.position + base.transform.forward).z > base.transform.position.z + 0.8f)
			{
				num4 = 1f;
			}
			if (num3 != 0f || num4 != 0f)
			{
				if (num3 == 0f)
				{
					num3 = -num;
				}
				else if (num4 == 0f)
				{
					num4 = -num2 + 1;
				}
			}
			return new Vector3(num3, 0f, num4);
		}
		return Vector3.zero;
	}

	public void moveSelectionToMainTileForMultiTiledObject(int xPos, int yPos)
	{
		selectedTile = WorldManager.manageWorld.findMultiTileObjectPos(xPos, yPos, insideHouseDetails);
	}

	private bool checkIfOnMap(int intToCheck)
	{
		if (insidePlayerHouse)
		{
			if (intToCheck >= 0 && intToCheck < 25)
			{
				return true;
			}
			return false;
		}
		if (intToCheck >= 0 && intToCheck < WorldManager.manageWorld.getMapSize())
		{
			return true;
		}
		return false;
	}

	public void refreshTileSelection(int xPos, int yPos)
	{
		lastEquip = myEquip.currentlyHolding;
		refreshSelection = false;
		if ((!insidePlayerHouse && WorldManager.manageWorld.onTileMap[xPos, yPos] <= -2) || (insidePlayerHouse && checkIfOnMap((int)selectedTile.x) && checkIfOnMap((int)selectedTile.y) && insideHouseDetails.houseMapOnTile[xPos, yPos] <= -2))
		{
			moveSelectionToMainTileForMultiTiledObject(xPos, yPos);
		}
		else
		{
			selectedTile = new Vector2(xPos, yPos);
		}
		if (insidePlayerHouse && (bool)insideHouseDisplay)
		{
			if (checkIfOnMap((int)selectedTile.x) && checkIfOnMap((int)selectedTile.y))
			{
				if (insideHouseDetails.houseMapOnTile[(int)selectedTile.x, (int)selectedTile.y] > -1)
				{
					objectAttacking = insideHouseDisplay.tileObjectsInHouse[(int)selectedTile.x, (int)selectedTile.y];
				}
				else
				{
					objectAttacking = null;
				}
			}
		}
		else
		{
			objectAttacking = WorldManager.manageWorld.findTileObjectInUse((int)selectedTile.x, (int)selectedTile.y);
			TileObjectHealthBar.tile.setCurrentlyAttacking(objectAttacking);
		}
		if (myEquip.currentlyHolding == emptyShovel)
		{
			myEquip.currentlyHolding.changeToWhenUsed = WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[(int)selectedTile.x, (int)selectedTile.y]].uniqueShovel;
		}
		canAttackSelectedTile = checkIfCanDamage(selectedTile);
		createPreviewOnHighlighter(xPos, yPos);
		refreshPreview(xPos, yPos);
	}

	private void refreshPreview(int xPos, int yPos)
	{
		if ((bool)previewObject && (!myEquip.currentlyHolding || !myEquip.currentlyHolding.placeable || (myEquip.currentlyHoldingDeed() && !placingDeed) || (myEquip.currentlyHoldingMultiTiledPlaceableItem() && !placingDeed) || (myEquip.currentlyHoldingSinglePlaceableItem() && !myEquip.currentlyHolding.placeable.getsRotationFromMap() && !insidePlayerHouse) || (myEquip.currentlyHoldingSinglePlaceableItem() && insidePlayerHouse && !placingDeed) || previewObject.tileObjectId != myEquip.currentlyHolding.placeable.tileObjectId || !Inventory.inv.canMoveChar()))
		{
			Object.Destroy(previewObject.gameObject);
			previewObject = null;
			tileHighlighterRotArrow.SetActive(false);
		}
		if (!Inventory.inv.canMoveChar())
		{
			return;
		}
		if ((bool)myEquip.currentlyHolding && (bool)myEquip.currentlyHolding.spawnPlaceable && (bool)myEquip.currentlyHolding.spawnPlaceable.GetComponent<Vehicle>())
		{
			desiredPreviewPos = base.transform.position + base.transform.forward * 2f;
			int num = (int)(Mathf.Round(desiredPreviewPos.x + 0.5f) / 2f);
			int num2 = (int)(Mathf.Round(desiredPreviewPos.z + 0.5f) / 2f);
			if (WorldManager.manageWorld.heightMap[num, num2] > 0)
			{
				desiredPreviewPos = new Vector3(num * 2, WorldManager.manageWorld.heightMap[num, num2], num2 * 2);
			}
			else
			{
				desiredPreviewPos = new Vector3(num * 2, 1f, num2 * 2);
			}
			if (vehiclePreview == null)
			{
				vehiclePreview = Object.Instantiate(NetworkMapSharer.share.vehicleBoxPreview, desiredPreviewPos, Quaternion.identity);
				canPlaceVehiclePreview = vehiclePreview.GetComponent<VehiclePreview>();
			}
			if (placeableRotation == 1)
			{
				vehiclePreview.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
			}
			else if (placeableRotation == 2)
			{
				vehiclePreview.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
			}
			else if (placeableRotation == 3)
			{
				vehiclePreview.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
			else if (placeableRotation == 4)
			{
				vehiclePreview.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
			}
			return;
		}
		if ((bool)vehiclePreview)
		{
			Object.Destroy(vehiclePreview);
		}
		if ((bool)myEquip.currentlyHolding && (bool)myEquip.currentlyHolding.placeable && !myEquip.currentlyHolding.burriedPlaceable && !previewObject)
		{
			if ((!myEquip.currentlyHolding.placeable.isMultiTileObject() || !placingDeed) && (!myEquip.currentlyHoldingSinglePlaceableItem() || !myEquip.currentlyHolding.placeable.getsRotationFromMap() || insidePlayerHouse) && (!myEquip.currentlyHoldingSinglePlaceableItem() || !insidePlayerHouse || !placingDeed))
			{
				return;
			}
			getStartingPlaceableRotation();
			previewObject = Object.Instantiate(WorldManager.manageWorld.allObjects[myEquip.currentlyHolding.placeable.tileObjectId]).GetComponent<TileObject>();
			previewObject._transform = previewObject.transform;
			Light[] componentsInChildren = previewObject.gameObject.GetComponentsInChildren<Light>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.SetActive(false);
			}
			Collider[] componentsInChildren2 = previewObject.gameObject.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				Object.Destroy(componentsInChildren2[i]);
			}
			previewRens = previewObject.gameObject.GetComponentsInChildren<MeshRenderer>();
		}
		if (!previewObject)
		{
			return;
		}
		if (!insidePlayerHouse)
		{
			desiredPreviewPos = new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2);
		}
		else
		{
			desiredPreviewPos = playerHouseTransform.position + new Vector3(xPos * 2, 0f, yPos * 2);
		}
		if (myEquip.currentlyHolding.placeable.getsRotationFromMap() || myEquip.currentlyHoldingMultiTiledPlaceableItem())
		{
			if ((bool)myEquip.currentlyHolding.placeable.tileObjectBridge)
			{
				int value = 4;
				if (placeableRotation == 1)
				{
					value = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, 0, -1);
				}
				else if (placeableRotation == 2)
				{
					value = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, -1);
				}
				else if (placeableRotation == 3)
				{
					value = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, 0, 1);
				}
				else if (placeableRotation == 4)
				{
					value = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, 1);
				}
				value = Mathf.Clamp(value, 4, 15);
				desiredPreviewPos += previewObject.setRotatiomNumberForPreviewObject(placeableRotation, value);
				if (previewShowingRot != placeableRotation)
				{
					if ((bool)previewObject)
					{
						previewObject.transform.position = desiredPreviewPos;
						tileHighlighter.transform.position += previewObject.setRotatiomNumberForPreviewObject(placeableRotation, value);
					}
					previewShowingRot = placeableRotation;
				}
			}
			else
			{
				desiredPreviewPos += previewObject.setRotatiomNumberForPreviewObject(placeableRotation);
				if (previewShowingRot != placeableRotation)
				{
					if ((bool)previewObject)
					{
						previewObject.transform.position = desiredPreviewPos;
						tileHighlighter.transform.position += previewObject.setRotatiomNumberForPreviewObject(placeableRotation);
					}
					previewShowingRot = placeableRotation;
				}
			}
			tileHighlighterRotArrow.SetActive(true);
			if (placeableRotation == 1)
			{
				tileHighlighterRotArrow.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
			}
			else if (placeableRotation == 2)
			{
				tileHighlighterRotArrow.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
			}
			else if (placeableRotation == 3)
			{
				tileHighlighterRotArrow.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
			else if (placeableRotation == 4)
			{
				tileHighlighterRotArrow.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
			}
		}
		if (myEquip.currentlyHoldingSinglePlaceableItem())
		{
			if (insidePlayerHouse)
			{
				if (insideHouseDetails.houseMapOnTile[(int)selectedTile.x, (int)selectedTile.y] == -1)
				{
					changePreviewColor(previewYes);
				}
				else if (myEquip.currentlyHolding.placeable.canBePlacedOntoFurniture() && insideHouseDetails.houseMapOnTile[(int)selectedTile.x, (int)selectedTile.y] > -1 && WorldManager.manageWorld.allObjectSettings[insideHouseDetails.houseMapOnTile[(int)selectedTile.x, (int)selectedTile.y]].canBePlacedOn())
				{
					int num3 = objectAttacking.returnClosestPlacedPositionId(tileHighlighter.position);
					if ((bool)previewObject)
					{
						desiredPreviewPos = objectAttacking.placedPositions[num3].position;
					}
					if (ItemOnTopManager.manage.getItemOnTopInPosition(num3, (int)selectedTile.x, (int)selectedTile.y, insideHouseDetails) == null)
					{
						changePreviewColor(previewYes);
					}
					else
					{
						changePreviewColor(previewNo);
					}
				}
				else
				{
					changePreviewColor(previewNo);
				}
			}
			else if (checkIfCanDamage(selectedTile) || myEquip.currentlyHolding.placeable.canBePlacedOntoFurniture())
			{
				if (myEquip.currentlyHolding.placeable.canBePlacedOntoFurniture() && WorldManager.manageWorld.onTileMap[(int)selectedTile.x, (int)selectedTile.y] > -1 && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[(int)selectedTile.x, (int)selectedTile.y]].canBePlacedOn())
				{
					int num4 = objectAttacking.returnClosestPlacedPositionId(tileHighlighter.position);
					if ((bool)previewObject)
					{
						desiredPreviewPos = objectAttacking.placedPositions[num4].position;
					}
					if (ItemOnTopManager.manage.getItemOnTopInPosition(num4, (int)selectedTile.x, (int)selectedTile.y, null) == null)
					{
						changePreviewColor(previewYes);
					}
					else
					{
						changePreviewColor(previewNo);
					}
				}
				else if (!myEquip.currentlyHolding.placeable.canBePlacedOntoFurniture())
				{
					changePreviewColor(previewYes);
				}
				else if (checkIfCanDamage(selectedTile))
				{
					changePreviewColor(previewYes);
				}
				else
				{
					changePreviewColor(previewNo);
				}
			}
			else if ((bool)objectAttacking && myEquip.currentlyHolding.canBePlacedOnToTileObject.Length != 0)
			{
				bool flag = false;
				for (int j = 0; j < myEquip.currentlyHolding.canBePlacedOnToTileObject.Length; j++)
				{
					if (myEquip.currentlyHolding.canBePlacedOnToTileObject[j].tileObjectId == objectAttacking.tileObjectId)
					{
						desiredPreviewPos = objectAttacking.loadInsidePos.position;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					if (WorldManager.manageWorld.onTileStatusMap[xPos, yPos] <= 0)
					{
						changePreviewColor(previewYes);
					}
					else
					{
						changePreviewColor(previewNo);
					}
				}
				else
				{
					changePreviewColor(previewNo);
				}
			}
			else
			{
				changePreviewColor(previewNo);
			}
		}
		else if (!insidePlayerHouse)
		{
			if ((!myEquip.currentlyHoldingDeed() && !myEquip.currentlyHolding.placeable.tileObjectBridge && myEquip.currentlyHolding.placeable.checkIfMultiTileObjectCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placeableRotation)) || (!myEquip.currentlyHoldingDeed() && (bool)myEquip.currentlyHolding.placeable.tileObjectBridge && myEquip.currentlyHolding.placeable.checkIfBridgeCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placeableRotation)) || (myEquip.currentlyHoldingDeed() && myEquip.currentlyHolding.placeable.checkIfDeedCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placeableRotation)))
			{
				changePreviewColor(previewYes);
			}
			else
			{
				changePreviewColor(previewNo);
			}
			if (myEquip.currentlyHoldingDeed() && !myEquip.currentlyHolding.placeable.checkIfDeedCanBePlaced((int)selectedTile.x, (int)selectedTile.y, placeableRotation))
			{
				NotificationManager.manage.pocketsFull.showCanPlaceText(myEquip.currentlyHolding.placeable.getWhyCantPlaceDeedText((int)selectedTile.x, (int)selectedTile.y, placeableRotation));
			}
			else if (myEquip.currentlyHoldingDeed())
			{
				NotificationManager.manage.pocketsFull.hidePopUp();
			}
		}
		else if (myEquip.currentlyHolding.placeable.checkIfMultiTileObjectCanBePlacedInside(insideHouseDetails, (int)selectedTile.x, (int)selectedTile.y, placeableRotation))
		{
			changePreviewColor(previewYes);
		}
		else
		{
			changePreviewColor(previewNo);
		}
	}

	private void checkIfIsFarmAnimalHouse(int xPos, int yPos, int newId, int rotation = -1)
	{
		if (newId >= 0 && (bool)WorldManager.manageWorld.allObjects[newId].tileObjectAnimalHouse)
		{
			StartCoroutine(farmAnimalPlacedDelay(xPos, yPos, newId, rotation));
		}
		if (newId == -1)
		{
			FarmAnimalManager.manage.removeAnimalHouse(xPos, yPos);
		}
	}

	private IEnumerator farmAnimalPlacedDelay(int xPos, int yPos, int newId, int rotation = -1)
	{
		yield return null;
		FarmAnimalManager.manage.createNewAnimalHouseWithHouseId(xPos, yPos, newId, rotation);
	}

	public void forceRequestHouse()
	{
		hasBeenInsideBefore = false;
		changeInsideOut(insidePlayerHouse, insideHouseDetails);
	}

	public bool isInside()
	{
		return insidePlayerHouse;
	}

	public void changeInsideOut(bool isEntry, HouseDetails details = null)
	{
		insidePlayerHouse = isEntry;
		insideHouseDetails = details;
		if (details != null)
		{
			Inventory.inv.wallSlot.updateSlotContentsAndRefresh(insideHouseDetails.wall, 1);
			Inventory.inv.floorSlot.updateSlotContentsAndRefresh(insideHouseDetails.floor, 1);
		}
		if (insideHouseDetails != null)
		{
			insideHouseDisplay = HouseManager.manage.findHousesOnDisplay(insideHouseDetails.xPos, insideHouseDetails.yPos);
			playerHouseTransform = insideHouseDisplay.getStartingPosTransform();
		}
		myEquip.setInsideOrOutside(isEntry, isEntry);
		if (!base.isServer)
		{
			if (insidePlayerHouse)
			{
				CmdisInsidePlayerHouse(insideHouseDetails.xPos, insideHouseDetails.yPos);
			}
			else
			{
				CmdGoOutside();
			}
		}
		if (isEntry && !hasBeenInsideBefore)
		{
			hasBeenInsideBefore = true;
			bool isServer2 = base.isServer;
		}
		if (insidePlayerHouse)
		{
			if ((bool)insideHouseDisplay)
			{
				playerHouseTransform = insideHouseDisplay.getStartingPosTransform();
				insideHouseDisplay.refreshHouseTiles();
			}
		}
		else
		{
			playerHouseTransform = null;
		}
	}

	[Command]
	public void CmdPickUpObjectOnTop(int newStatus, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newStatus);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPickUpObjectOnTop", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPickUpObjectOnTopOf(int posId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(posId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPickUpObjectOnTopOf", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPickUpObjectOnTopOfInside(int posId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(posId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPickUpObjectOnTopOfInside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPlaceItemOnTopOf(int objectId, int posId, int status, int rotation, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(objectId);
		writer.WriteInt(posId);
		writer.WriteInt(status);
		writer.WriteInt(rotation);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPlaceItemOnTopOf", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPlaceItemOnTopOfInside(int objectId, int posId, int status, int rotation, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(objectId);
		writer.WriteInt(posId);
		writer.WriteInt(status);
		writer.WriteInt(rotation);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPlaceItemOnTopOfInside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlaceItemOnTopOfInside(int objectId, int posId, int status, int rotation, int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(objectId);
		writer.WriteInt(posId);
		writer.WriteInt(status);
		writer.WriteInt(rotation);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(CharInteract), "RpcPlaceItemOnTopOfInside", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPickUpItemOnTopOfInside(int posId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(posId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPickUpItemOnTopOfInside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcUnlockClient(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(CharInteract), "RpcUnlockClient", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlaceItemOnTop(int objectId, int posId, int status, int rotation, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(objectId);
		writer.WriteInt(posId);
		writer.WriteInt(status);
		writer.WriteInt(rotation);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(CharInteract), "RpcPlaceItemOnTop", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcRemoveItemOnTop(int posId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(posId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendRPCInternal(typeof(CharInteract), "RpcRemoveItemOnTop", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcRemoveItemOnTopInside(int posId, int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(posId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendRPCInternal(typeof(CharInteract), "RpcRemoveItemOnTopInside", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdisInsidePlayerHouse(int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendCommandInternal(typeof(CharInteract), "CmdisInsidePlayerHouse", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdGoOutside()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharInteract), "CmdGoOutside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdPlaceItemInToTileObject(int newStatus, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newStatus);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPlaceItemInToTileObject", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSpawnPlaceable(Vector3 spawnPos, int id)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteVector3(spawnPos);
		writer.WriteInt(id);
		SendCommandInternal(typeof(CharInteract), "CmdSpawnPlaceable", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdSpawnVehicle(int id, int rot, int variation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(id);
		writer.WriteInt(rot);
		writer.WriteInt(variation);
		SendCommandInternal(typeof(CharInteract), "CmdSpawnVehicle", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdFixTeleport(string dir)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteString(dir);
		SendCommandInternal(typeof(CharInteract), "CmdFixTeleport", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeOnTile(int newTileType, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newTileType);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdChangeOnTile", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdChangeOnTileInside(int newTileType, int xPos, int yPos, int rotation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newTileType);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		SendCommandInternal(typeof(CharInteract), "CmdChangeOnTileInside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPickUpOnTile(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdPickUpOnTile", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPickUpOnTileInside(int xPos, int yPos, float playerHouseTransformY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteFloat(playerHouseTransformY);
		SendCommandInternal(typeof(CharInteract), "CmdPickUpOnTileInside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdHarvestCrabPot(int xPos, int yPos, int drop)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(drop);
		SendCommandInternal(typeof(CharInteract), "CmdHarvestCrabPot", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdHarvestOnTile(int xPos, int yPos, bool pickedUpAuto)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteBool(pickedUpAuto);
		SendCommandInternal(typeof(CharInteract), "CmdHarvestOnTile", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdHarvestOnTileDeath(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdHarvestOnTileDeath", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdFillFood(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdFillFood", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdOpenClose(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdOpenClose", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdChangeTileHeight(int newTileHeight, int newTileType, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newTileHeight);
		writer.WriteInt(newTileType);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdChangeTileHeight", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdChangeTileType(int newTileType, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newTileType);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdChangeTileType", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPlaceMultiTiledObject(int multiTiledObjectId, int xPos, int yPos, int rotation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(multiTiledObjectId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		SendCommandInternal(typeof(CharInteract), "CmdPlaceMultiTiledObject", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdPlaceBridgeTileObject(int multiTiledObjectId, int xPos, int yPos, int rotation, int length)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(multiTiledObjectId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		writer.WriteInt(length);
		SendCommandInternal(typeof(CharInteract), "CmdPlaceBridgeTileObject", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdUpdateHouseWall(int itemId, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendCommandInternal(typeof(CharInteract), "CmdUpdateHouseWall", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdUpdateHouseFloor(int itemId, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(itemId);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendCommandInternal(typeof(CharInteract), "CmdUpdateHouseFloor", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdSetRotation(int xPos, int yPos, int rotation)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(rotation);
		SendCommandInternal(typeof(CharInteract), "CmdSetRotation", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdDepositItem(int depositItemId, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(depositItemId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdDepositItem", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	private void CmdDepositItemInside(int depositItemId, int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(depositItemId);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendCommandInternal(typeof(CharInteract), "CmdDepositItemInside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdCurrentlyAttackingPos(int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdCurrentlyAttackingPos", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void changePreviewColor(Material newColor)
	{
		if ((bool)previewObject.GetComponent<TileObjectGrowthStages>())
		{
			previewObject.GetComponent<TileObjectGrowthStages>().showOnlyFirstStageForPreview();
			previewRens = previewObject.gameObject.GetComponentsInChildren<MeshRenderer>();
		}
		if ((bool)previewObject.GetComponent<TileObjectConnect>())
		{
			previewObject.GetComponent<TileObjectConnect>().connectToTiles((int)selectedTile.x, (int)selectedTile.y, placeableRotation);
			previewRens = previewObject.gameObject.GetComponentsInChildren<MeshRenderer>();
		}
		if ((bool)previewObject.GetComponent<TileObjectBridge>())
		{
			int value = 4;
			if (placeableRotation == 1)
			{
				value = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, 0, -1);
			}
			else if (placeableRotation == 2)
			{
				value = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, -1);
			}
			else if (placeableRotation == 3)
			{
				value = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, 0, 1);
			}
			else if (placeableRotation == 4)
			{
				value = WorldManager.manageWorld.allObjectSettings[0].checkBridgLenth((int)selectedTile.x, (int)selectedTile.y, 1);
			}
			value = Mathf.Clamp(value, 5, 15);
			previewObject.GetComponent<TileObjectBridge>().setUpBridge(value);
			previewRens = previewObject.gameObject.GetComponentsInChildren<MeshRenderer>();
		}
		MeshRenderer[] array = previewRens;
		foreach (MeshRenderer meshRenderer in array)
		{
			Material[] materials = meshRenderer.materials;
			for (int j = 0; j < materials.Length; j++)
			{
				materials[j] = newColor;
			}
			meshRenderer.materials = materials;
		}
	}

	private void OnChangeAttackingPos(Vector2 oldPos, Vector2 newPos)
	{
		NetworkcurrentlyAttackingPos = newPos;
		if (!base.isLocalPlayer)
		{
			if (insidePlayerHouse)
			{
				objectAttacking = insideHouseDisplay.tileObjectsInHouse[(int)currentlyAttackingPos.x, (int)currentlyAttackingPos.y];
			}
			else
			{
				objectAttacking = WorldManager.manageWorld.findTileObjectInUse((int)currentlyAttackingPos.x, (int)currentlyAttackingPos.y);
			}
			currentlyAttackingId = WorldManager.manageWorld.onTileMap[(int)currentlyAttackingPos.x, (int)currentlyAttackingPos.y];
		}
	}

	private void changePlaceableForDeed()
	{
		if (placingDeed)
		{
			bool inConversation = ConversationManager.manage.inConversation;
		}
	}

	private void changePlacingDeedBool(bool newIsPlacingDeed)
	{
		if (!newIsPlacingDeed)
		{
			selectPositionDif = Vector3.zero;
			CameraController.control.setFollowTransform(base.transform);
		}
		else
		{
			selectPositionDif = base.transform.forward * 2f;
			getStartingPlaceableRotation();
			CameraController.control.setFollowTransform(tileHighlighter);
		}
		placingDeed = newIsPlacingDeed;
		if (placingDeed && placingRoutine == null)
		{
			placingRoutine = StartCoroutine(movePlacingObjectPreview());
		}
		refreshPreview((int)selectedTile.x, (int)selectedTile.y);
	}

	private void playerTurnsTowardsPreview()
	{
	}

	private IEnumerator movePlacingObjectPreview()
	{
		float moveTimer = 0.2f;
		bool resetToFreeCam = CameraController.control.isFreeCamOn() && myEquip.currentlyHolding.isDeed;
		if (!resetToFreeCam)
		{
			resetToFreeCam = CameraController.control.isFreeCamOn() && (bool)myEquip.currentlyHolding.placeable && (bool)myEquip.currentlyHolding.placeable.tileObjectBridge;
		}
		if (resetToFreeCam)
		{
			CameraController.control.setCamDistanceForDeedPlacement();
			CameraController.control.swapFreeCam();
		}
		while (placingDeed)
		{
			playerTurnsTowardsPreview();
			yield return null;
			if (ConversationManager.manage.inConversation)
			{
				continue;
			}
			float num = InputMaster.input.getLeftStick().x;
			float num2 = InputMaster.input.getLeftStick().y;
			if (num == 0f && num2 == 0f)
			{
				moveTimer = 0.25f;
			}
			if (moveTimer > 0.2f)
			{
				if (1f - Mathf.Abs(num) <= 0.25f)
				{
					if (num > 0f)
					{
						num = 1f;
					}
					if (num < 0f)
					{
						num = -1f;
					}
				}
				if (1f - Mathf.Abs(num2) <= 0.25f)
				{
					if (num2 > 0f)
					{
						num2 = 1f;
					}
					if (num2 < 0f)
					{
						num2 = -1f;
					}
				}
				if (num != 0f || num2 != 0f)
				{
					Vector3 vector = CameraController.control.transform.forward * num2;
					Vector3 vector2 = CameraController.control.transform.right * num;
					selectPositionDif += (vector + vector2).normalized * 2f;
					if ((bool)myEquip.currentlyHolding && myEquip.currentlyHolding.isDeed)
					{
						selectPositionDif = new Vector3(Mathf.Clamp(selectPositionDif.x, -20f, 20f), 0f, Mathf.Clamp(selectPositionDif.z, -20f, 20f));
					}
					else
					{
						selectPositionDif = new Vector3(Mathf.Clamp(selectPositionDif.x, -5f, 5f), 0f, Mathf.Clamp(selectPositionDif.z, -4f, 4f));
					}
					moveTimer = 0f;
				}
			}
			else
			{
				moveTimer += Time.deltaTime;
			}
		}
		NotificationManager.manage.pocketsFull.hidePopUp();
		placingRoutine = null;
		if (resetToFreeCam && !CameraController.control.isFreeCamOn())
		{
			CameraController.control.swapFreeCam();
		}
	}

	private bool checkClientLocked()
	{
		if (!WorldManager.manageWorld.checkTileClientLock((int)selectedTile.x, (int)selectedTile.y))
		{
			return true;
		}
		return false;
	}

	public void getStartingPlaceableRotation()
	{
		int num = (int)(Mathf.Round(base.transform.eulerAngles.y / 90f) * 90f);
		if (num == 0 || base.transform.eulerAngles.y > 350f)
		{
			placeableRotation = 1;
		}
		else
		{
			switch (num)
			{
			case 90:
				placeableRotation = 2;
				break;
			case 180:
				placeableRotation = 3;
				break;
			case 270:
				placeableRotation = 4;
				break;
			}
		}
		if ((bool)myEquip.currentlyHolding && (bool)myEquip.currentlyHolding.placeable && (bool)myEquip.currentlyHolding.placeable.tileObjectBridge)
		{
			placeableRotation += 2;
			if (placeableRotation > 4)
			{
				placeableRotation -= 4;
			}
		}
	}

	[Command]
	public void CmdPlayPlaceableAnimation()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendCommandInternal(typeof(CharInteract), "CmdPlayPlaceableAnimation", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[ClientRpc]
	public void RpcPlayPlaceableAnimation()
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendRPCInternal(typeof(CharInteract), "RpcPlayPlaceableAnimation", writer, 0, true);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdGiveStatus(int newStatus, int xPos, int yPos)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newStatus);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		SendCommandInternal(typeof(CharInteract), "CmdGiveStatus", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[Command]
	public void CmdGiveStatusInside(int newStatus, int xPos, int yPos, int houseX, int houseY)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteInt(newStatus);
		writer.WriteInt(xPos);
		writer.WriteInt(yPos);
		writer.WriteInt(houseX);
		writer.WriteInt(houseY);
		SendCommandInternal(typeof(CharInteract), "CmdGiveStatusInside", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	[TargetRpc]
	public void TargetSendMustBeEmptyPrompt(NetworkConnection con)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		SendTargetRPCInternal(con, typeof(CharInteract), "TargetSendMustBeEmptyPrompt", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdPickUpObjectOnTop(int newStatus, int xPos, int yPos)
	{
		if ((bool)WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]].placeable)
		{
			WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]].placeable.tileObjectId].removeBeauty();
		}
		int invItemId = Inventory.inv.getInvItemId(WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]]);
		NetworkMapSharer.share.spawnAServerDrop(invItemId, 1, new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2));
		NetworkMapSharer.share.RpcGiveOnTileStatus(0, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdPickUpObjectOnTop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpObjectOnTop called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPickUpObjectOnTop(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPickUpObjectOnTopOf(int posId, int xPos, int yPos)
	{
		if (ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, null) == null)
		{
			RpcUnlockClient(xPos, yPos);
			return;
		}
		ItemOnTop itemOnTopInPosition = ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, null);
		TileObject tileObjectForServerDrop = WorldManager.manageWorld.getTileObjectForServerDrop(WorldManager.manageWorld.onTileMap[xPos, yPos], new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2));
		if (!WorldManager.manageWorld.allObjectSettings[itemOnTopInPosition.getTileObjectId()].pickUpRequiresEmptyPocket)
		{
			if (WorldManager.manageWorld.allObjectSettings[itemOnTopInPosition.getTileObjectId()].dropsStatusNumberOnDeath)
			{
				NetworkMapSharer.share.spawnAServerDrop(itemOnTopInPosition.getStatus(), 1, tileObjectForServerDrop.placedPositions[posId].position);
			}
			else
			{
				NetworkMapSharer.share.spawnAServerDrop(WorldManager.manageWorld.allObjectSettings[itemOnTopInPosition.getTileObjectId()].dropsItemOnDeath.getItemId(), 1, tileObjectForServerDrop.placedPositions[posId].position);
			}
		}
		WorldManager.manageWorld.returnTileObject(tileObjectForServerDrop);
		RpcRemoveItemOnTop(posId, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdPickUpObjectOnTopOf(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpObjectOnTopOf called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPickUpObjectOnTopOf(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPickUpObjectOnTopOfInside(int posId, int xPos, int yPos)
	{
		if (ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, insideHouseDetails) == null)
		{
			return;
		}
		ItemOnTop itemOnTopInPosition = ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, insideHouseDetails);
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(insideHouseDetails.xPos, insideHouseDetails.yPos);
		TileObject tileObjectForHouse = WorldManager.manageWorld.getTileObjectForHouse(insideHouseDetails.houseMapOnTile[xPos, yPos], displayPlayerHouseTiles.getStartingPosTransform().position + new Vector3(xPos * 2, 0f, yPos * 2), xPos, yPos, insideHouseDetails);
		if (!WorldManager.manageWorld.allObjectSettings[itemOnTopInPosition.getTileObjectId()].pickUpRequiresEmptyPocket)
		{
			if (WorldManager.manageWorld.allObjectSettings[itemOnTopInPosition.getTileObjectId()].dropsStatusNumberOnDeath)
			{
				NetworkMapSharer.share.spawnAServerDrop(itemOnTopInPosition.getStatus(), 1, tileObjectForHouse.placedPositions[posId].position, insideHouseDetails);
			}
			else
			{
				NetworkMapSharer.share.spawnAServerDrop(WorldManager.manageWorld.allObjectSettings[itemOnTopInPosition.getTileObjectId()].dropsItemOnDeath.getItemId(), 1, tileObjectForHouse.placedPositions[posId].position, insideHouseDetails);
			}
		}
		RpcRemoveItemOnTopInside(posId, xPos, yPos, insideHouseDetails.xPos, insideHouseDetails.yPos);
		WorldManager.manageWorld.returnTileObject(tileObjectForHouse);
	}

	protected static void InvokeUserCode_CmdPickUpObjectOnTopOfInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpObjectOnTopOfInside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPickUpObjectOnTopOfInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceItemOnTopOf(int objectId, int posId, int status, int rotation, int xPos, int yPos)
	{
		if (ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, null) != null)
		{
			RpcUnlockClient(xPos, yPos);
		}
		else
		{
			RpcPlaceItemOnTop(objectId, posId, status, rotation, xPos, yPos);
		}
	}

	protected static void InvokeUserCode_CmdPlaceItemOnTopOf(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceItemOnTopOf called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPlaceItemOnTopOf(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceItemOnTopOfInside(int objectId, int posId, int status, int rotation, int xPos, int yPos)
	{
		if (ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, insideHouseDetails) == null)
		{
			RpcPlaceItemOnTopOfInside(objectId, posId, status, rotation, xPos, yPos, insideHouseDetails.xPos, insideHouseDetails.yPos);
		}
	}

	protected static void InvokeUserCode_CmdPlaceItemOnTopOfInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceItemOnTopOfInside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPlaceItemOnTopOfInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcPlaceItemOnTopOfInside(int objectId, int posId, int status, int rotation, int xPos, int yPos, int houseX, int houseY)
	{
		ItemOnTopManager.manage.placeItemOnTop(objectId, posId, status, rotation, xPos, yPos, HouseManager.manage.getHouseInfo(houseX, houseY));
		SoundManager.manage.playASoundAtPoint(SoundManager.manage.placeItem, new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2));
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if ((bool)displayPlayerHouseTiles && (bool)displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos])
		{
			displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos].checkOnTopInside(xPos, yPos, HouseManager.manage.getHouseInfo(houseX, houseY));
		}
	}

	protected static void InvokeUserCode_RpcPlaceItemOnTopOfInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaceItemOnTopOfInside called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_RpcPlaceItemOnTopOfInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPickUpItemOnTopOfInside(int posId, int xPos, int yPos)
	{
	}

	protected static void InvokeUserCode_CmdPickUpItemOnTopOfInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpItemOnTopOfInside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPickUpItemOnTopOfInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcUnlockClient(int xPos, int yPos)
	{
		WorldManager.manageWorld.unlockClientTile(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcUnlockClient(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUnlockClient called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_RpcUnlockClient(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcPlaceItemOnTop(int objectId, int posId, int status, int rotation, int xPos, int yPos)
	{
		ItemOnTopManager.manage.placeItemOnTop(objectId, posId, status, rotation, xPos, yPos, null);
		SoundManager.manage.playASoundAtPoint(SoundManager.manage.placeItem, new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2));
		WorldManager.manageWorld.unlockClientTile(xPos, yPos);
		WorldManager.manageWorld.refreshAllChunksInUse(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcPlaceItemOnTop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaceItemOnTop called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_RpcPlaceItemOnTop(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcRemoveItemOnTop(int posId, int xPos, int yPos)
	{
		ItemOnTopManager.manage.removeItemOnTop(ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, null));
		WorldManager.manageWorld.unlockClientTile(xPos, yPos);
		WorldManager.manageWorld.refreshAllChunksInUse(xPos, yPos);
	}

	protected static void InvokeUserCode_RpcRemoveItemOnTop(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveItemOnTop called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_RpcRemoveItemOnTop(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_RpcRemoveItemOnTopInside(int posId, int xPos, int yPos, int houseX, int houseY)
	{
		ItemOnTopManager.manage.removeItemOnTop(ItemOnTopManager.manage.getItemOnTopInPosition(posId, xPos, yPos, HouseManager.manage.getHouseInfo(houseX, houseY)));
		DisplayPlayerHouseTiles displayPlayerHouseTiles = HouseManager.manage.findHousesOnDisplay(houseX, houseY);
		if ((bool)displayPlayerHouseTiles && (bool)displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos])
		{
			displayPlayerHouseTiles.tileObjectsInHouse[xPos, yPos].checkOnTopInside(xPos, yPos, HouseManager.manage.getHouseInfo(houseX, houseY));
		}
	}

	protected static void InvokeUserCode_RpcRemoveItemOnTopInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveItemOnTopInside called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_RpcRemoveItemOnTopInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdisInsidePlayerHouse(int houseX, int houseY)
	{
		insidePlayerHouse = true;
		insideHouseDetails = HouseManager.manage.getHouseInfo(houseX, houseY);
		NetworkMapSharer.share.TargetRequestHouse(base.connectionToClient, houseX, houseY, WorldManager.manageWorld.getHouseDetailsArray(insideHouseDetails.houseMapOnTile), WorldManager.manageWorld.getHouseDetailsArray(insideHouseDetails.houseMapOnTileStatus), WorldManager.manageWorld.getHouseDetailsArray(insideHouseDetails.houseMapRotation), insideHouseDetails.wall, insideHouseDetails.floor, ItemOnTopManager.manage.getAllItemsOnTopInHouse(insideHouseDetails));
		if (insideHouseDetails != null)
		{
			insideHouseDisplay = HouseManager.manage.findHousesOnDisplay(insideHouseDetails.xPos, insideHouseDetails.yPos);
			playerHouseTransform = insideHouseDisplay.getStartingPosTransform();
		}
	}

	protected static void InvokeUserCode_CmdisInsidePlayerHouse(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdisInsidePlayerHouse called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdisInsidePlayerHouse(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdGoOutside()
	{
		insidePlayerHouse = false;
		insideHouseDetails = null;
		insideHouseDisplay = null;
		playerHouseTransform = null;
	}

	protected static void InvokeUserCode_CmdGoOutside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGoOutside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdGoOutside();
		}
	}

	protected void UserCode_CmdPlaceItemInToTileObject(int newStatus, int xPos, int yPos)
	{
		NetworkMapSharer.share.RpcPlaceItemOnToTileObject(newStatus, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdPlaceItemInToTileObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceItemInToTileObject called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPlaceItemInToTileObject(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSpawnPlaceable(Vector3 spawnPos, int id)
	{
		NetworkServer.Spawn(Object.Instantiate(Inventory.inv.allItems[id].spawnPlaceable, spawnPos, Quaternion.identity));
	}

	protected static void InvokeUserCode_CmdSpawnPlaceable(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnPlaceable called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdSpawnPlaceable(reader.ReadVector3(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSpawnVehicle(int id, int rot, int variation)
	{
		Vector3 position = base.transform.position + base.transform.forward * 2f;
		int num = (int)(Mathf.Round(position.x + 0.5f) / 2f);
		int num2 = (int)(Mathf.Round(position.z + 0.5f) / 2f);
		Vector3 position2 = base.transform.position;
		if (WorldManager.manageWorld.isPositionOnMap(num, num2))
		{
			position = ((WorldManager.manageWorld.heightMap[num, num2] <= 0) ? new Vector3(num * 2, 1f, num2 * 2) : new Vector3(num * 2, WorldManager.manageWorld.heightMap[num, num2], num2 * 2));
		}
		Quaternion rotation;
		switch (rot)
		{
		case 1:
			rotation = Quaternion.Euler(0f, 180f, 0f);
			break;
		case 2:
			rotation = Quaternion.Euler(0f, 270f, 0f);
			break;
		case 3:
			rotation = Quaternion.Euler(0f, 0f, 0f);
			break;
		default:
			rotation = Quaternion.Euler(0f, 90f, 0f);
			break;
		}
		GameObject obj = Object.Instantiate(NetworkMapSharer.share.vehicleBox, position, rotation);
		obj.GetComponent<SpawnVehicleOnOpen>().fillDetails(Inventory.inv.allItems[id].spawnPlaceable, variation, base.connectionToClient);
		NetworkServer.Spawn(obj);
	}

	protected static void InvokeUserCode_CmdSpawnVehicle(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnVehicle called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdSpawnVehicle(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdFixTeleport(string dir)
	{
		switch (dir)
		{
		case "north":
			WorldManager.manageWorld.onTileChunkHasChanged(TownManager.manage.northTowerPos[0], TownManager.manage.northTowerPos[1]);
			NetworkMapSharer.share.RpcUpdateOnTileObject(292, TownManager.manage.northTowerPos[0], TownManager.manage.northTowerPos[1]);
			NetworkMapSharer.share.NetworknorthOn = true;
			break;
		case "east":
			WorldManager.manageWorld.onTileChunkHasChanged(TownManager.manage.eastTowerPos[0], TownManager.manage.eastTowerPos[1]);
			NetworkMapSharer.share.RpcUpdateOnTileObject(292, TownManager.manage.eastTowerPos[0], TownManager.manage.eastTowerPos[1]);
			NetworkMapSharer.share.NetworkeastOn = true;
			break;
		case "south":
			WorldManager.manageWorld.onTileChunkHasChanged(TownManager.manage.southTowerPos[0], TownManager.manage.southTowerPos[1]);
			NetworkMapSharer.share.RpcUpdateOnTileObject(292, TownManager.manage.southTowerPos[0], TownManager.manage.southTowerPos[1]);
			NetworkMapSharer.share.NetworksouthOn = true;
			break;
		case "west":
			WorldManager.manageWorld.onTileChunkHasChanged(TownManager.manage.westTowerPos[0], TownManager.manage.westTowerPos[1]);
			NetworkMapSharer.share.RpcUpdateOnTileObject(292, TownManager.manage.westTowerPos[0], TownManager.manage.westTowerPos[1]);
			NetworkMapSharer.share.NetworkwestOn = true;
			break;
		}
	}

	protected static void InvokeUserCode_CmdFixTeleport(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFixTeleport called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdFixTeleport(reader.ReadString());
		}
	}

	protected void UserCode_CmdChangeOnTile(int newTileType, int xPos, int yPos)
	{
		if (checkIfCanDamage(new Vector2(xPos, yPos)))
		{
			if (myEquip.currentlyHoldingDeed())
			{
				DeedManager.manage.placeDeed(myEquip.currentlyHolding);
			}
			WorldManager.manageWorld.onTileChunkHasChanged(xPos, yPos);
			if (WorldManager.manageWorld.onTileMap[xPos, yPos] >= 0 && WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].isMultiTileObject())
			{
				NetworkMapSharer.share.RpcRemoveMultiTiledObject(WorldManager.manageWorld.onTileMap[xPos, yPos], xPos, yPos, WorldManager.manageWorld.rotationMap[xPos, yPos]);
			}
			else if (newTileType == -1 && BuriedManager.manage.checkIfBuriedItem(xPos, yPos) != null)
			{
				NetworkMapSharer.share.RpcUpdateOnTileObject(30, xPos, yPos);
			}
			else if (newTileType == -1 && BuriedManager.manage.checkIfShouldTurnIntoBuriedItem(xPos, yPos))
			{
				NetworkMapSharer.share.RpcUpdateOnTileObject(30, xPos, yPos);
			}
			else
			{
				NetworkMapSharer.share.RpcUpdateOnTileObject(newTileType, xPos, yPos);
			}
			checkIfIsFarmAnimalHouse(xPos, yPos, newTileType, placeableRotation);
		}
	}

	protected static void InvokeUserCode_CmdChangeOnTile(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeOnTile called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdChangeOnTile(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeOnTileInside(int newTileType, int xPos, int yPos, int rotation)
	{
		NetworkMapSharer.share.RpcChangeHouseOnTile(newTileType, xPos, yPos, rotation, insideHouseDetails.xPos, insideHouseDetails.yPos);
	}

	protected static void InvokeUserCode_CmdChangeOnTileInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeOnTileInside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdChangeOnTileInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPickUpOnTile(int xPos, int yPos)
	{
		WorldManager.manageWorld.onTileChunkHasChanged(xPos, yPos);
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] < 0)
		{
			return;
		}
		if ((bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectChest && !WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectChest.checkIfEmpty(xPos, yPos, insideHouseDetails))
		{
			TargetSendMustBeEmptyPrompt(base.connectionToClient);
			return;
		}
		if ((bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectItemChanger && WorldManager.manageWorld.onTileStatusMap[xPos, yPos] >= 0)
		{
			TargetSendMustBeEmptyPrompt(base.connectionToClient);
			return;
		}
		TileObject tileObjectForServerDrop = WorldManager.manageWorld.getTileObjectForServerDrop(WorldManager.manageWorld.onTileMap[xPos, yPos], new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2));
		if (tileObjectForServerDrop.canBePickedUp() || ((bool)tileObjectForServerDrop.tileObjectBridge && WorldManager.manageWorld.waterMap[xPos, yPos]))
		{
			if (WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].pickUpRequiresEmptyPocket && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].dropsStatusNumberOnDeath && WorldManager.manageWorld.onTileStatusMap[xPos, yPos] != -1)
			{
				DroppedItem component = Object.Instantiate(WorldManager.manageWorld.droppedItemPrefab, new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2), Quaternion.identity).GetComponent<DroppedItem>();
				component.GetComponent<DroppedItem>().setDesiredPos(component.transform.position.y, component.transform.position.x, component.transform.position.z);
				component.NetworkstackAmount = 1;
				component.NetworkmyItemId = WorldManager.manageWorld.onTileStatusMap[xPos, yPos];
				NetworkServer.Spawn(component.gameObject);
				component.pickUp();
				component.RpcMoveTowardsPickedUpBy(base.netId);
			}
			if (WorldManager.manageWorld.onTileStatusMap[xPos, yPos] > 0 && WorldManager.manageWorld.allObjectSettings[tileObjectForServerDrop.tileObjectId].statusObjectsPickUpFirst.Length != 0)
			{
				if ((bool)WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]].placeable)
				{
					WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]].placeable.tileObjectId].removeBeauty();
				}
				int invItemId = Inventory.inv.getInvItemId(WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].statusObjectsPickUpFirst[WorldManager.manageWorld.onTileStatusMap[xPos, yPos]]);
				NetworkMapSharer.share.spawnAServerDrop(invItemId, 1, new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2));
				NetworkMapSharer.share.RpcGiveOnTileStatus(0, xPos, yPos);
			}
			else if (tileObjectForServerDrop.isMultiTileObject())
			{
				NetworkMapSharer.share.RpcRemoveMultiTiledObject(WorldManager.manageWorld.onTileMap[xPos, yPos], xPos, yPos, WorldManager.manageWorld.rotationMap[xPos, yPos]);
			}
			else if (BuriedManager.manage.checkIfBuriedItem(xPos, yPos) != null)
			{
				NetworkMapSharer.share.RpcUpdateOnTileObject(30, xPos, yPos);
			}
			else
			{
				NetworkMapSharer.share.RpcUpdateOnTileObject(-1, xPos, yPos);
			}
		}
		WorldManager.manageWorld.returnTileObject(tileObjectForServerDrop);
		checkIfIsFarmAnimalHouse(xPos, yPos, -1);
	}

	protected static void InvokeUserCode_CmdPickUpOnTile(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpOnTile called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPickUpOnTile(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPickUpOnTileInside(int xPos, int yPos, float playerHouseTransformY)
	{
		if (insideHouseDetails.houseMapOnTile[xPos, yPos] < 0 || ((bool)WorldManager.manageWorld.allObjects[insideHouseDetails.houseMapOnTile[xPos, yPos]].tileObjectChest && !WorldManager.manageWorld.allObjects[insideHouseDetails.houseMapOnTile[xPos, yPos]].tileObjectChest.checkIfEmpty(xPos, yPos, insideHouseDetails)))
		{
			return;
		}
		TileObject tileObjectForServerDrop = WorldManager.manageWorld.getTileObjectForServerDrop(insideHouseDetails.houseMapOnTile[xPos, yPos], new Vector3(xPos * 2, playerHouseTransformY, yPos * 2));
		if (tileObjectForServerDrop.canBePickedUp())
		{
			if ((bool)tileObjectForServerDrop && WorldManager.manageWorld.allObjectSettings[tileObjectForServerDrop.tileObjectId].pickUpRequiresEmptyPocket)
			{
				if (WorldManager.manageWorld.allObjectSettings[tileObjectForServerDrop.tileObjectId].dropsStatusNumberOnDeath && insideHouseDetails.houseMapOnTileStatus[xPos, yPos] != -1)
				{
					DroppedItem component = Object.Instantiate(WorldManager.manageWorld.droppedItemPrefab, base.transform.position + base.transform.forward * 1.5f, Quaternion.identity).GetComponent<DroppedItem>();
					component.GetComponent<DroppedItem>().setDesiredPos(component.transform.position.y, component.transform.position.x, component.transform.position.z);
					component.NetworkstackAmount = 1;
					component.NetworkmyItemId = insideHouseDetails.houseMapOnTileStatus[xPos, yPos];
					NetworkServer.Spawn(component.gameObject);
					component.pickUp();
					component.RpcMoveTowardsPickedUpBy(base.netId);
					NetworkMapSharer.share.RpcChangeHouseOnTile(-1, xPos, yPos, WorldManager.manageWorld.rotationMap[xPos, yPos], insideHouseDetails.xPos, insideHouseDetails.yPos);
				}
			}
			else
			{
				NetworkMapSharer.share.RpcChangeHouseOnTile(-1, xPos, yPos, WorldManager.manageWorld.rotationMap[xPos, yPos], insideHouseDetails.xPos, insideHouseDetails.yPos);
			}
		}
		WorldManager.manageWorld.returnTileObject(tileObjectForServerDrop);
	}

	protected static void InvokeUserCode_CmdPickUpOnTileInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickUpOnTileInside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPickUpOnTileInside(reader.ReadInt(), reader.ReadInt(), reader.ReadFloat());
		}
	}

	protected void UserCode_CmdHarvestCrabPot(int xPos, int yPos, int drop)
	{
		NetworkMapSharer.share.spawnAServerDrop(drop, 1, new Vector3((float)xPos * 2f, 0.6f, yPos * 2), null, false, 3);
		NetworkMapSharer.share.RpcHarvestObject(0, xPos, yPos, false);
	}

	protected static void InvokeUserCode_CmdHarvestCrabPot(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHarvestCrabPot called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdHarvestCrabPot(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdHarvestOnTile(int xPos, int yPos, bool pickedUpAuto)
	{
		if (!WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.canBeHarvested(WorldManager.manageWorld.onTileStatusMap[xPos, yPos]))
		{
			return;
		}
		int num = WorldManager.manageWorld.onTileStatusMap[xPos, yPos] + WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.takeOrAddFromStateOnHarvest;
		if (num < 0)
		{
			num = 0;
		}
		if (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.diesOnHarvest)
		{
			if (pickedUpAuto)
			{
				if (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.milestoneOnHarvest != 0)
				{
					NetworkMapSharer.share.TargetGiveHarvestMilestone(base.connectionToClient, WorldManager.manageWorld.onTileMap[xPos, yPos]);
				}
				NetworkMapSharer.share.RpcHarvestObject(-1, xPos, yPos, false);
				int num2 = -1;
				if (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.isCrabPot)
				{
					num2 = WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.getCrabTrapDrop(xPos, yPos);
				}
				else if ((bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.dropsFromLootTable)
				{
					num2 = Inventory.inv.getInvItemId(WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.dropsFromLootTable.getRandomDropFromTable());
				}
				else if ((bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.harvestDrop)
				{
					num2 = Inventory.inv.getInvItemId(WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.harvestDrop);
				}
				if (num2 != -1)
				{
					DroppedItem component = Object.Instantiate(WorldManager.manageWorld.droppedItemPrefab, new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2), Quaternion.identity).GetComponent<DroppedItem>();
					component.GetComponent<DroppedItem>().setDesiredPos(component.transform.position.y, component.transform.position.x, component.transform.position.z);
					component.NetworkstackAmount = 1;
					component.NetworkmyItemId = num2;
					NetworkServer.Spawn(component.gameObject);
					component.pickUp();
					component.RpcMoveTowardsPickedUpBy(base.netId);
				}
			}
			else
			{
				if (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.milestoneOnHarvest != 0)
				{
					NetworkMapSharer.share.TargetGiveHarvestMilestone(base.connectionToClient, WorldManager.manageWorld.onTileMap[xPos, yPos]);
				}
				NetworkMapSharer.share.RpcHarvestObject(-1, xPos, yPos, true);
			}
			WorldManager.manageWorld.onTileChunkHasChanged(xPos, yPos);
		}
		else
		{
			if (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.milestoneOnHarvest != 0)
			{
				NetworkMapSharer.share.TargetGiveHarvestMilestone(base.connectionToClient, WorldManager.manageWorld.onTileMap[xPos, yPos]);
			}
			NetworkMapSharer.share.RpcHarvestObject(num, xPos, yPos, !pickedUpAuto);
			WorldManager.manageWorld.onTileStatusMap[xPos, yPos] = num;
		}
	}

	protected static void InvokeUserCode_CmdHarvestOnTile(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHarvestOnTile called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdHarvestOnTile(reader.ReadInt(), reader.ReadInt(), reader.ReadBool());
		}
	}

	protected void UserCode_CmdHarvestOnTileDeath(int xPos, int yPos)
	{
		if (!WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.canBeHarvested(WorldManager.manageWorld.onTileStatusMap[xPos, yPos], true))
		{
			return;
		}
		int num = WorldManager.manageWorld.onTileStatusMap[xPos, yPos] + WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.takeOrAddFromStateOnHarvest;
		if (num < 0)
		{
			num = 0;
		}
		if (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.diesOnHarvest)
		{
			if (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.milestoneOnHarvest != 0)
			{
				NetworkMapSharer.share.TargetGiveHarvestMilestone(base.connectionToClient, WorldManager.manageWorld.onTileMap[xPos, yPos]);
			}
			NetworkMapSharer.share.RpcHarvestObject(-1, xPos, yPos, true);
			WorldManager.manageWorld.onTileChunkHasChanged(xPos, yPos);
		}
		else
		{
			if (WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages.milestoneOnHarvest != 0)
			{
				NetworkMapSharer.share.TargetGiveHarvestMilestone(base.connectionToClient, WorldManager.manageWorld.onTileMap[xPos, yPos]);
			}
			NetworkMapSharer.share.RpcHarvestObject(num, xPos, yPos, true);
			WorldManager.manageWorld.onTileStatusMap[xPos, yPos] = num;
		}
	}

	protected static void InvokeUserCode_CmdHarvestOnTileDeath(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHarvestOnTileDeath called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdHarvestOnTileDeath(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdFillFood(int xPos, int yPos)
	{
		NetworkMapSharer.share.RpcOpenCloseTile(xPos, yPos, 1);
	}

	protected static void InvokeUserCode_CmdFillFood(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFillFood called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdFillFood(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdOpenClose(int xPos, int yPos)
	{
		if (WorldManager.manageWorld.onTileStatusMap[xPos, yPos] == 0)
		{
			NetworkMapSharer.share.RpcOpenCloseTile(xPos, yPos, 1);
		}
		else
		{
			NetworkMapSharer.share.RpcOpenCloseTile(xPos, yPos, 0);
		}
	}

	protected static void InvokeUserCode_CmdOpenClose(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOpenClose called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdOpenClose(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeTileHeight(int newTileHeight, int newTileType, int xPos, int yPos)
	{
		if (checkIfCanDamage(new Vector2(xPos, yPos)))
		{
			if (newTileHeight > 0)
			{
				NetworkMapSharer.share.RpcUpdateTileType(newTileType, xPos, yPos);
			}
			NetworkMapSharer.share.changeTileHeight(newTileHeight, xPos, yPos, base.connectionToClient);
		}
	}

	protected static void InvokeUserCode_CmdChangeTileHeight(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeTileHeight called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdChangeTileHeight(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdChangeTileType(int newTileType, int xPos, int yPos)
	{
		if (!checkIfCanDamage(new Vector2(xPos, yPos)))
		{
			return;
		}
		WorldManager.manageWorld.tileTypeChunkHasChanged(xPos, yPos);
		if (((bool)WorldManager.manageWorld.tileTypes[newTileType].dropOnChange || WorldManager.manageWorld.tileTypes[newTileType].saveUnderTile || WorldManager.manageWorld.tileTypes[newTileType].changeToUnderTileAndChangeHeight) && !WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos, yPos]].dropOnChange && !WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos, yPos]].changeToUnderTileAndChangeHeight && (WorldManager.manageWorld.tileTypeMap[xPos, yPos] < 0 || WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos, yPos]].canBeSavedUnder))
		{
			WorldManager.manageWorld.tileTypeStatusMap[xPos, yPos] = WorldManager.manageWorld.tileTypeMap[xPos, yPos];
		}
		if (WeatherManager.manage.raining)
		{
			switch (newTileType)
			{
			case 7:
				newTileType = 8;
				break;
			case 12:
				newTileType = 13;
				break;
			}
		}
		if ((bool)WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos, yPos]].dropOnChange)
		{
			NetworkMapSharer.share.spawnAServerDrop(Inventory.inv.getInvItemId(WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos, yPos]].dropOnChange), 1, new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2));
			NetworkMapSharer.share.RpcUpdateTileType(WorldManager.manageWorld.tileTypeStatusMap[xPos, yPos], xPos, yPos);
		}
		else if (!WorldManager.manageWorld.tileTypes[newTileType].changeTileKeepUnderTile && WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos, yPos]].changeTileKeepUnderTile)
		{
			NetworkMapSharer.share.RpcUpdateTileType(WorldManager.manageWorld.tileTypeStatusMap[xPos, yPos], xPos, yPos);
		}
		else if (WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[xPos, yPos]].saveUnderTile && !WorldManager.manageWorld.tileTypes[newTileType].changeTileKeepUnderTile)
		{
			NetworkMapSharer.share.RpcUpdateTileType(WorldManager.manageWorld.tileTypeStatusMap[xPos, yPos], xPos, yPos);
		}
		else
		{
			NetworkMapSharer.share.RpcUpdateTileType(newTileType, xPos, yPos);
		}
	}

	protected static void InvokeUserCode_CmdChangeTileType(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeTileType called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdChangeTileType(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceMultiTiledObject(int multiTiledObjectId, int xPos, int yPos, int rotation)
	{
		if (myEquip.currentlyHoldingDeed())
		{
			DeedManager.manage.placeDeed(myEquip.currentlyHolding);
		}
		WorldManager.manageWorld.onTileChunkHasChanged(xPos, yPos);
		NetworkMapSharer.share.RpcPlaceMultiTiledObject(multiTiledObjectId, xPos, yPos, rotation);
		if ((bool)WorldManager.manageWorld.allObjects[multiTiledObjectId].tileObjectFurniture || (bool)WorldManager.manageWorld.allObjects[multiTiledObjectId].showObjectOnStatusChange)
		{
			WorldManager.manageWorld.onTileStatusMap[xPos, yPos] = 0;
		}
		checkIfIsFarmAnimalHouse(xPos, yPos, multiTiledObjectId, rotation);
	}

	protected static void InvokeUserCode_CmdPlaceMultiTiledObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceMultiTiledObject called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPlaceMultiTiledObject(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlaceBridgeTileObject(int multiTiledObjectId, int xPos, int yPos, int rotation, int length)
	{
		WorldManager.manageWorld.onTileChunkHasChanged(xPos, yPos);
		NetworkMapSharer.share.RpcPlaceBridgeTiledObject(multiTiledObjectId, xPos, yPos, rotation, length);
		int[] array = WorldManager.manageWorld.allObjects[multiTiledObjectId].placeBridgeTiledObject(xPos, yPos, rotation, length);
		checkIfIsFarmAnimalHouse(array[0], array[1], multiTiledObjectId, rotation);
	}

	protected static void InvokeUserCode_CmdPlaceBridgeTileObject(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceBridgeTileObject called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPlaceBridgeTileObject(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdUpdateHouseWall(int itemId, int houseX, int houseY)
	{
		Inventory.inv.wallSlot.itemNo = itemId;
		Inventory.inv.wallSlot.stack = 1;
		NetworkMapSharer.share.RpcUpdateHouseWall(itemId, houseX, houseY);
	}

	protected static void InvokeUserCode_CmdUpdateHouseWall(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUpdateHouseWall called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdUpdateHouseWall(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdUpdateHouseFloor(int itemId, int houseX, int houseY)
	{
		Inventory.inv.floorSlot.itemNo = itemId;
		Inventory.inv.floorSlot.stack = 1;
		NetworkMapSharer.share.RpcUpdateHouseFloor(itemId, houseX, houseY);
	}

	protected static void InvokeUserCode_CmdUpdateHouseFloor(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUpdateHouseFloor called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdUpdateHouseFloor(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdSetRotation(int xPos, int yPos, int rotation)
	{
		NetworkMapSharer.share.RpcSetRotation(xPos, yPos, rotation);
	}

	protected static void InvokeUserCode_CmdSetRotation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetRotation called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdSetRotation(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdDepositItem(int depositItemId, int xPos, int yPos)
	{
		if (depositItemId >= 0 && (bool)Inventory.inv.allItems[depositItemId].itemChange && Inventory.inv.allItems[depositItemId].itemChange.checkIfCanBeDepositedServer(WorldManager.manageWorld.onTileMap[xPos, yPos]))
		{
			NetworkMapSharer.share.RpcDepositItemIntoChanger(depositItemId, xPos, yPos);
			NetworkMapSharer.share.startTileTimerOnServer(depositItemId, xPos, yPos);
		}
	}

	protected static void InvokeUserCode_CmdDepositItem(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDepositItem called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdDepositItem(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdDepositItemInside(int depositItemId, int xPos, int yPos, int houseX, int houseY)
	{
		if (depositItemId >= 0 && (bool)Inventory.inv.allItems[depositItemId].itemChange && Inventory.inv.allItems[depositItemId].itemChange.checkIfCanBeDepositedServer(insideHouseDetails.houseMapOnTile[xPos, yPos]))
		{
			NetworkMapSharer.share.RpcDepositItemIntoChangerInside(depositItemId, xPos, yPos, houseX, houseY);
			NetworkMapSharer.share.startTileTimerOnServer(depositItemId, xPos, yPos, insideHouseDetails);
		}
	}

	protected static void InvokeUserCode_CmdDepositItemInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDepositItemInside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdDepositItemInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdCurrentlyAttackingPos(int xPos, int yPos)
	{
		NetworkcurrentlyAttackingPos = new Vector2(xPos, yPos);
	}

	protected static void InvokeUserCode_CmdCurrentlyAttackingPos(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCurrentlyAttackingPos called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdCurrentlyAttackingPos(reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdPlayPlaceableAnimation()
	{
		RpcPlayPlaceableAnimation();
	}

	protected static void InvokeUserCode_CmdPlayPlaceableAnimation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlayPlaceableAnimation called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdPlayPlaceableAnimation();
		}
	}

	protected void UserCode_RpcPlayPlaceableAnimation()
	{
		myEquip.playPlaceableAnimation();
	}

	protected static void InvokeUserCode_RpcPlayPlaceableAnimation(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayPlaceableAnimation called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_RpcPlayPlaceableAnimation();
		}
	}

	protected void UserCode_CmdGiveStatus(int newStatus, int xPos, int yPos)
	{
		NetworkMapSharer.share.RpcGiveOnTileStatus(newStatus, xPos, yPos);
	}

	protected static void InvokeUserCode_CmdGiveStatus(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGiveStatus called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdGiveStatus(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_CmdGiveStatusInside(int newStatus, int xPos, int yPos, int houseX, int houseY)
	{
		NetworkMapSharer.share.RpcGiveOnTileStatusInside(newStatus, xPos, yPos, houseX, houseY);
	}

	protected static void InvokeUserCode_CmdGiveStatusInside(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGiveStatusInside called on client.");
		}
		else
		{
			((CharInteract)obj).UserCode_CmdGiveStatusInside(reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
		}
	}

	protected void UserCode_TargetSendMustBeEmptyPrompt(NetworkConnection con)
	{
		NotificationManager.manage.pocketsFull.showMustBeEmpty();
	}

	protected static void InvokeUserCode_TargetSendMustBeEmptyPrompt(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSendMustBeEmptyPrompt called on server.");
		}
		else
		{
			((CharInteract)obj).UserCode_TargetSendMustBeEmptyPrompt(NetworkClient.readyConnection);
		}
	}

	static CharInteract()
	{
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPickUpObjectOnTop", InvokeUserCode_CmdPickUpObjectOnTop, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPickUpObjectOnTopOf", InvokeUserCode_CmdPickUpObjectOnTopOf, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPickUpObjectOnTopOfInside", InvokeUserCode_CmdPickUpObjectOnTopOfInside, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPlaceItemOnTopOf", InvokeUserCode_CmdPlaceItemOnTopOf, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPlaceItemOnTopOfInside", InvokeUserCode_CmdPlaceItemOnTopOfInside, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPickUpItemOnTopOfInside", InvokeUserCode_CmdPickUpItemOnTopOfInside, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdisInsidePlayerHouse", InvokeUserCode_CmdisInsidePlayerHouse, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdGoOutside", InvokeUserCode_CmdGoOutside, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPlaceItemInToTileObject", InvokeUserCode_CmdPlaceItemInToTileObject, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdSpawnPlaceable", InvokeUserCode_CmdSpawnPlaceable, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdSpawnVehicle", InvokeUserCode_CmdSpawnVehicle, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdFixTeleport", InvokeUserCode_CmdFixTeleport, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdChangeOnTile", InvokeUserCode_CmdChangeOnTile, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdChangeOnTileInside", InvokeUserCode_CmdChangeOnTileInside, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPickUpOnTile", InvokeUserCode_CmdPickUpOnTile, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPickUpOnTileInside", InvokeUserCode_CmdPickUpOnTileInside, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdHarvestCrabPot", InvokeUserCode_CmdHarvestCrabPot, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdHarvestOnTile", InvokeUserCode_CmdHarvestOnTile, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdHarvestOnTileDeath", InvokeUserCode_CmdHarvestOnTileDeath, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdFillFood", InvokeUserCode_CmdFillFood, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdOpenClose", InvokeUserCode_CmdOpenClose, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdChangeTileHeight", InvokeUserCode_CmdChangeTileHeight, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdChangeTileType", InvokeUserCode_CmdChangeTileType, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPlaceMultiTiledObject", InvokeUserCode_CmdPlaceMultiTiledObject, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPlaceBridgeTileObject", InvokeUserCode_CmdPlaceBridgeTileObject, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdUpdateHouseWall", InvokeUserCode_CmdUpdateHouseWall, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdUpdateHouseFloor", InvokeUserCode_CmdUpdateHouseFloor, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdSetRotation", InvokeUserCode_CmdSetRotation, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdDepositItem", InvokeUserCode_CmdDepositItem, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdDepositItemInside", InvokeUserCode_CmdDepositItemInside, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdCurrentlyAttackingPos", InvokeUserCode_CmdCurrentlyAttackingPos, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdPlayPlaceableAnimation", InvokeUserCode_CmdPlayPlaceableAnimation, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdGiveStatus", InvokeUserCode_CmdGiveStatus, true);
		RemoteCallHelper.RegisterCommandDelegate(typeof(CharInteract), "CmdGiveStatusInside", InvokeUserCode_CmdGiveStatusInside, true);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "RpcPlaceItemOnTopOfInside", InvokeUserCode_RpcPlaceItemOnTopOfInside);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "RpcUnlockClient", InvokeUserCode_RpcUnlockClient);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "RpcPlaceItemOnTop", InvokeUserCode_RpcPlaceItemOnTop);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "RpcRemoveItemOnTop", InvokeUserCode_RpcRemoveItemOnTop);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "RpcRemoveItemOnTopInside", InvokeUserCode_RpcRemoveItemOnTopInside);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "RpcPlayPlaceableAnimation", InvokeUserCode_RpcPlayPlaceableAnimation);
		RemoteCallHelper.RegisterRpcDelegate(typeof(CharInteract), "TargetSendMustBeEmptyPrompt", InvokeUserCode_TargetSendMustBeEmptyPrompt);
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteVector2(currentlyAttackingPos);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteVector2(currentlyAttackingPos);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			Vector2 vector = currentlyAttackingPos;
			NetworkcurrentlyAttackingPos = reader.ReadVector2();
			if (!SyncVarEqual(vector, ref currentlyAttackingPos))
			{
				OnChangeAttackingPos(vector, currentlyAttackingPos);
			}
			return;
		}
		long num = (long)reader.ReadULong();
		if ((num & 1L) != 0L)
		{
			Vector2 vector2 = currentlyAttackingPos;
			NetworkcurrentlyAttackingPos = reader.ReadVector2();
			if (!SyncVarEqual(vector2, ref currentlyAttackingPos))
			{
				OnChangeAttackingPos(vector2, currentlyAttackingPos);
			}
		}
	}
}
