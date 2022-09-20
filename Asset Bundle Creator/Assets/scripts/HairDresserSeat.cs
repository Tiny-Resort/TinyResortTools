using UnityEngine;

public class HairDresserSeat : MonoBehaviour
{
	public static HairDresserSeat seat;

	public FurnitureStatus theSeatsStatus;

	public Conversation hairDresserConversation;

	public Animator chairAnimator;

	private void OnEnable()
	{
		seat = this;
		updateTheSeat(NetworkMapSharer.share.hairDresserSeatOccupied);
	}

	public void updateTheSeat(bool isSeatFull)
	{
		if (isSeatFull)
		{
			theSeatsStatus.disableSeat(theSeatsStatus.seatPosition1);
		}
		else
		{
			theSeatsStatus.enableSeat(theSeatsStatus.seatPosition1);
		}
	}

	public void spinChair()
	{
		chairAnimator.SetTrigger("Spin");
	}
}
