using UnityEngine;

public class BulletinBoardShowNewMessage : MonoBehaviour
{
	public static BulletinBoardShowNewMessage showMessage;

	public GameObject newNotification;

	private void Awake()
	{
		showMessage = this;
	}

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isBulletinBoard = this;
		WorldManager.manageWorld.changeDayEvent.AddListener(showIfNewMessage);
		BulletinBoard.board.closeBoardEvent.AddListener(showIfNewMessage);
	}

	private void OnEnable()
	{
		if ((bool)NetworkMapSharer.share && NetworkMapSharer.share.isServer && BulletinBoard.board.attachedPosts.Count == 0)
		{
			PostOnBoard item = new PostOnBoard(0, -1, BulletinBoard.BoardPostType.Announcement);
			BulletinBoard.board.attachedPosts.Add(item);
		}
		showIfNewMessage();
	}

	public void showIfNewMessage()
	{
		newNotification.SetActive(BulletinBoard.board.checkIfAnyUnread());
	}
}
