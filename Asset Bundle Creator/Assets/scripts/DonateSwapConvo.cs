using UnityEngine;

public class DonateSwapConvo : MonoBehaviour
{
	public Conversation acceptingDonationConvo;

	public Conversation notAcceptingDonations;

	public Conversation returnConvo()
	{
		if (NetworkMapSharer.share.townDebt <= 0)
		{
			return notAcceptingDonations;
		}
		return acceptingDonationConvo;
	}
}
