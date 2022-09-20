using UnityEngine;

public class ReadableSign : MonoBehaviour
{
	public Conversation signSays;

	private DonateSwapConvo donate;

	private ConstructionBoxInput construct;

	private void Start()
	{
		donate = GetComponent<DonateSwapConvo>();
		construct = GetComponent<ConstructionBoxInput>();
		base.gameObject.AddComponent<InteractableObject>().isSign = this;
	}

	public void readSign()
	{
		if ((bool)construct)
		{
			ConversationManager.manage.donatingToBuilding = construct;
			ConversationManager.manage.talkToNPC(NPCManager.manage.sign, construct.getConversation());
		}
		if ((bool)donate)
		{
			ConversationManager.manage.talkToNPC(NPCManager.manage.sign, donate.returnConvo());
		}
		else
		{
			ConversationManager.manage.talkToNPC(NPCManager.manage.sign, signSays);
		}
	}
}
