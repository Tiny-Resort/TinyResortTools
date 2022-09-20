using I2.Loc;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
	public ShopBuyDrop isShopBuyDrop;

	public ReadableSign isSign;

	public MailBoxShowsMail isMailBox;

	public BulletinBoardShowNewMessage isBulletinBoard;

	public ChestPlaceable isChest;

	public FurnitureStatus isFurniture;

	public WorkTable isWorkTable;

	public MineControls isMineControls;

	public Vehicle isVehicle;

	public TileObjectAnimalHouse isAnimalHouse;

	public PickUpAndCarry isPickUpAndCarry;

	public ItemDepositAndChanger isItemChanger;

	public FarmAnimal isFarmAnimal;

	public MuseumPainting isPainting;

	public ItemSign isItemSign;

	public TileObjectGrowthStages isGrowable;

	public bool showingToolTip(Transform rayPos, CharPickUp myPickUp)
	{
		if ((bool)isShopBuyDrop)
		{
			if (isShopBuyDrop.canTalkToKeeper() && !isShopBuyDrop.isKeeperWorking())
			{
				if (isShopBuyDrop.canTalkToKeeper() && (bool)isShopBuyDrop.closedConversation)
				{
					NotificationManager.manage.showButtonPrompt(string.Concat((LocalizedString)"ToolTips/Tip_Buy", " ", Inventory.inv.allItems[isShopBuyDrop.myItemId].getInvItemName()), "B", rayPos.position);
				}
				else
				{
					NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_ShopClosed", "no", rayPos.position);
				}
			}
			else if (isShopBuyDrop.canTalkToKeeper())
			{
				if ((bool)isShopBuyDrop.sellsAnimal && isShopBuyDrop.dummyAnimal.activeSelf)
				{
					NotificationManager.manage.showButtonPrompt(string.Concat((LocalizedString)"ToolTips/Tip_Buy", " ", isShopBuyDrop.sellsAnimal.animalName), "B", rayPos.position);
				}
				else if (isShopBuyDrop.myItemId != -1)
				{
					NotificationManager.manage.showButtonPrompt(string.Concat((LocalizedString)"ToolTips/Tip_Buy", " ", Inventory.inv.allItems[isShopBuyDrop.myItemId].getInvItemName()), "B", rayPos.position);
				}
			}
			else
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_ClerkBusy", "no", rayPos.position);
			}
			return true;
		}
		if ((bool)isSign)
		{
			NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Read", "B", rayPos.position);
			return true;
		}
		if ((bool)isMailBox)
		{
			if (isMailBox.hasMail)
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Read", "B", rayPos.position);
			}
			NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Read", "B", rayPos.position);
			return true;
		}
		if ((bool)isBulletinBoard)
		{
			NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Read", "B", rayPos.position);
			return true;
		}
		if ((bool)isChest)
		{
			NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Open", "B", rayPos.position);
			return true;
		}
		if ((bool)isFurniture)
		{
			if (isFurniture.isSeat)
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Sit", "B", rayPos.position);
			}
			else
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_LayDown", "B", rayPos.position);
			}
			return true;
		}
		if ((bool)isWorkTable)
		{
			NotificationManager.manage.showButtonPrompt(string.Concat((LocalizedString)"ToolTips/Tip_Use", " ", isWorkTable.workTableName), "B", rayPos.position);
			return true;
		}
		if ((bool)isMineControls)
		{
			if (NetworkMapSharer.share.canUseMineControls)
			{
				if (isMineControls.forEntrance)
				{
					NotificationManager.manage.showButtonPrompt("Go Down", "B", rayPos.position);
				}
				else
				{
					NotificationManager.manage.showButtonPrompt("Go Up", "B", rayPos.position);
				}
			}
			return true;
		}
		if ((bool)isVehicle)
		{
			NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Drive", "B", rayPos.position);
			return true;
		}
		bool flag = (bool)isAnimalHouse;
		if ((bool)isPickUpAndCarry && isPickUpAndCarry.canBePickedUp)
		{
			NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_PickUp", "B", rayPos.position);
			return true;
		}
		if ((bool)isItemChanger)
		{
			if ((bool)myPickUp.myInteract.myEquip.currentlyHolding && isItemChanger.canDepositThisItem(myPickUp.myInteract.myEquip.currentlyHolding, myPickUp.myInteract.insideHouseDetails))
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Insert", "X", rayPos.position);
			}
			else if ((bool)myPickUp.myInteract.myEquip.currentlyHolding && isItemChanger.returnAmountNeeded(myPickUp.myInteract.myEquip.currentlyHolding) > 0 && !isItemChanger.getIfProcessing())
			{
				NotificationManager.manage.showButtonPrompt(string.Concat((LocalizedString)"ToolTips/Tip_Requires", " ", isItemChanger.returnAmountNeeded(myPickUp.myInteract.myEquip.currentlyHolding).ToString()), "no", rayPos.position);
			}
			else
			{
				NotificationManager.manage.hideButtonPrompt();
			}
			return true;
		}
		if ((bool)isFarmAnimal)
		{
			if ((bool)isFarmAnimal.canBeHarvested && isFarmAnimal.canBeHarvested.checkIfCanHarvest(myPickUp.myEquip.currentlyHolding))
			{
				NotificationManager.manage.showButtonPrompt(isFarmAnimal.canBeHarvested.getToolTip(myPickUp.myEquip.currentlyHolding), "X", rayPos.position);
				return true;
			}
			NotificationManager.manage.showButtonPrompt(string.Concat((LocalizedString)"ToolTips/Tip_Pet", " ", isFarmAnimal.getAnimalName()), "B", rayPos.position);
			return true;
		}
		if ((bool)isPainting)
		{
			if (isPainting.checkIfMuseumKeeperCanBeTalkedTo() && !isPainting.checkIfMuseumKeeperIsAtWork())
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_ShopClosed", "no", rayPos.position);
			}
			else if (isPainting.checkIfMuseumKeeperCanBeTalkedTo())
			{
				NotificationManager.manage.showButtonPrompt("Check", "B", rayPos.position);
			}
			else
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_ClerkBusy", "no", rayPos.position);
			}
			return true;
		}
		if ((bool)isItemSign)
		{
			if (isItemSign.isSilo)
			{
				if (myPickUp.myInteract.myEquip.currentlyHolding == isItemSign.itemCanPlaceIn)
				{
					TileObject componentInParent = isItemSign.GetComponentInParent<TileObject>();
					if (WorldManager.manageWorld.onTileStatusMap[componentInParent.xPos, componentInParent.yPos] < 200)
					{
						NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Insert", "X", rayPos.position);
					}
					else
					{
						NotificationManager.manage.showButtonPrompt("Full", "no", rayPos.position);
					}
				}
			}
			else if ((bool)myPickUp.myInteract.myEquip.currentlyHolding)
			{
				NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Insert", "X", rayPos.position);
			}
		}
		if ((bool)isGrowable && (bool)myPickUp.myInteract.myEquip.currentlyHolding)
		{
			for (int i = 0; i < isGrowable.itemsToPlace.Length; i++)
			{
				if (myPickUp.myInteract.myEquip.currentlyHolding == isGrowable.itemsToPlace[i])
				{
					TileObject componentInParent2 = isGrowable.GetComponentInParent<TileObject>();
					if (WorldManager.manageWorld.onTileStatusMap[componentInParent2.xPos, componentInParent2.yPos] >= isGrowable.maxStageToReachByPlacing)
					{
						NotificationManager.manage.hideButtonPrompt();
						return false;
					}
					NotificationManager.manage.showButtonPrompt((LocalizedString)"ToolTips/Tip_Insert", "X", rayPos.position);
				}
			}
		}
		return false;
	}
}
