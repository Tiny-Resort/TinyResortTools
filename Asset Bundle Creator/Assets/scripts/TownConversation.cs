using UnityEngine;

public class TownConversation : MonoBehaviour
{
	public Conversation openingConversation;

	[Header("Options-----")]
	public Conversation noDeedsAvalible;

	public Conversation payOffTownDebtFirst;

	public Conversation openDeedConvo;

	public Conversation noRoomForDeeds;

	[Header("Confirmation-----")]
	public Conversation confirmDeedConvo;

	public Conversation closeDeedWindowConvo;

	[Header("Not a local -----")]
	public Conversation notALocalConvo;

	public Conversation askAboutDeeds()
	{
		if (!NetworkMapSharer.share.isServer)
		{
			return notALocalConvo;
		}
		if (NetworkMapSharer.share.townDebt > 0)
		{
			return payOffTownDebtFirst;
		}
		if (!DeedManager.manage.checkIfDeedsAvaliable())
		{
			return noDeedsAvalible;
		}
		if (!Inventory.inv.checkIfItemCanFit(0, 1))
		{
			return noRoomForDeeds;
		}
		return openDeedConvo;
	}
}
