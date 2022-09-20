using UnityEngine;

public class HouseConversationGroup : MonoBehaviour
{
	public Conversation startingConversation;

	public Conversation startingConversationTent;

	[Header("Tent Convos")]
	public Conversation UpgradeVersionTent;

	public Conversation UpgradeVersionNotEnoughMoneyTent;

	public Conversation houseAlreadyBeingUpgradedTent;

	[Header("House Convos")]
	public Conversation noHouseVersion;

	public Conversation UpgradeVersion;

	public Conversation UpgradeVersionNotEnoughMoney;

	public Conversation houseAtMax;

	public Conversation houseAlreadyBeingUpgraded;

	public Conversation houseIsBeingMoved;

	[Header("House Convos")]
	public Conversation notALocal;

	public Conversation getStartingConversation()
	{
		if (!NetworkMapSharer.share.isServer)
		{
			return notALocal;
		}
		if (TownManager.manage.getCurrentHouseStage() == 0)
		{
			if (TownManager.manage.checkIfHouseIsBeingUpgraded())
			{
				return houseAlreadyBeingUpgradedTent;
			}
			return startingConversationTent;
		}
		return startingConversation;
	}

	public Conversation getConversation()
	{
		if (!NetworkMapSharer.share.isServer)
		{
			return notALocal;
		}
		if (TownManager.manage.checkIfHouseIsBeingMoved())
		{
			return houseIsBeingMoved;
		}
		if (TownManager.manage.checkIfHouseIsBeingUpgraded())
		{
			return houseAlreadyBeingUpgraded;
		}
		if (TownManager.manage.getCurrentHouseStage() == -1)
		{
			return noHouseVersion;
		}
		if (TownManager.manage.getCurrentHouseStage() >= 0 && TownManager.manage.getCurrentHouseStage() != TownManager.manage.playerHouseStages.Length - 1)
		{
			if (TownManager.manage.getCurrentHouseStage() == 0)
			{
				if (Inventory.inv.wallet >= TownManager.manage.getNextHouseCost())
				{
					return UpgradeVersionTent;
				}
				return UpgradeVersionNotEnoughMoneyTent;
			}
			if (Inventory.inv.wallet >= TownManager.manage.getNextHouseCost())
			{
				return UpgradeVersion;
			}
			return UpgradeVersionNotEnoughMoney;
		}
		return houseAtMax;
	}
}
