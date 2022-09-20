using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BulletinBoardTaskButton : MonoBehaviour
{
	public TextMeshProUGUI taskTitleText;

	public TextMeshProUGUI nameText;

	public GameObject completedButton;

	public GameObject expiredButton;

	public GameObject acceptedButton;

	public GameObject baseButton;

	public GameObject newButton;

	public Image border;

	private int myPostId;

	public void attachToPost(int postId)
	{
		myPostId = postId;
		if (postId < 0)
		{
			base.gameObject.SetActive(false);
			return;
		}
		base.gameObject.SetActive(true);
		taskTitleText.text = BulletinBoard.board.attachedPosts[myPostId].getTitleText(postId);
		nameText.text = BulletinBoard.board.attachedPosts[myPostId].getPostedByName();
		if (BulletinBoard.board.attachedPosts[myPostId].postedByNpcId > 0)
		{
			border.color = NPCManager.manage.NPCDetails[BulletinBoard.board.attachedPosts[myPostId].postedByNpcId].npcColor;
		}
		else
		{
			border.color = Color.grey;
		}
		completedButton.SetActive(false);
		expiredButton.SetActive(false);
		acceptedButton.SetActive(false);
		newButton.SetActive(false);
		baseButton.SetActive(false);
		if (BulletinBoard.board.attachedPosts[myPostId].checkIfExpired())
		{
			expiredButton.SetActive(true);
		}
		else if (!BulletinBoard.board.attachedPosts[myPostId].hasBeenRead)
		{
			newButton.SetActive(true);
		}
		else if (BulletinBoard.board.attachedPosts[myPostId].completed)
		{
			completedButton.SetActive(true);
		}
		else if (BulletinBoard.board.attachedPosts[myPostId].checkIfAccepted())
		{
			acceptedButton.SetActive(true);
		}
		else
		{
			baseButton.SetActive(true);
		}
	}

	public void pressButton()
	{
		BulletinBoard.board.setSelectedSlotAndShow(myPostId);
	}
}
