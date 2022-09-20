using UnityEngine;

public class ConstructionBoxInput : MonoBehaviour
{
	public Conversation constructionBoxNeedsMats;

	public Conversation constructionBoxFull;

	public Conversation conversationWhenLastItemsDonated;

	public TileObject myTileObject;

	public void Start()
	{
		myTileObject = GetComponentInParent<TileObject>();
	}

	public Conversation getConversation()
	{
		if (DeedManager.manage.checkIfDeedMaterialsComplete(myTileObject.xPos, myTileObject.yPos))
		{
			return constructionBoxFull;
		}
		return constructionBoxNeedsMats;
	}

	public void openForGivingMenus()
	{
		GiveNPC.give.openBuildingGiveMenu(myTileObject.xPos, myTileObject.yPos);
	}
}
