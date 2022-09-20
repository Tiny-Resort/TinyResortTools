using UnityEngine;

public class MuseumConvoGroup : MonoBehaviour
{
	[Header("Donation conversations --------------")]
	public Conversation noItemsGiven;

	public Conversation allItemsAreAlreadyDonated;

	public Conversation allItemsAreNew;

	public Conversation itemCantBeDonated;

	public Conversation askIfHasAnotherDonation;

	public Conversation notLocal;

	[Header("Photo Conversations --------------")]
	public Conversation askAboutPaintingInFrameNoPhotos;

	public Conversation askAboutPaintingInFrameWithPhotos;

	public Conversation askAboutEmptyFrameNoPhotos;

	public Conversation askAboutEmptyFrameWithPhotos;

	public Conversation thankForPhoto;

	public Conversation nonLocalAskAboutPhoto;

	public Conversation nonLocalAskAboutEmpty;

	public Conversation getDonationConversation(bool newDonation)
	{
		if (!newDonation)
		{
			return allItemsAreAlreadyDonated;
		}
		return allItemsAreNew;
	}
}
