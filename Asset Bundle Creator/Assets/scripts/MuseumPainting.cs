using UnityEngine;

public class MuseumPainting : MonoBehaviour
{
	public int paintingNo;

	public MeshRenderer mesh;

	private void Start()
	{
		base.gameObject.AddComponent<InteractableObject>().isPainting = this;
	}

	public void askAboutPainting()
	{
		ConversationManager.manage.setTalkingAboutPhotoId(paintingNo);
		ConversationManager.manage.lastTalkTo = NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Museum);
		if (NetworkMapSharer.share.isServer)
		{
			if (PhotoManager.manage.savedPhotos.Count == 0)
			{
				if (PhotoManager.manage.displayedPhotos[paintingNo] != null && PhotoManager.manage.displayedPhotos[paintingNo].photoName != "Dummy")
				{
					ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ConversationManager.manage.lastTalkTo.getVendorConversation().GetComponent<MuseumConvoGroup>().askAboutPaintingInFrameNoPhotos);
				}
				else
				{
					ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ConversationManager.manage.lastTalkTo.getVendorConversation().GetComponent<MuseumConvoGroup>().askAboutEmptyFrameNoPhotos);
				}
			}
			else if (PhotoManager.manage.displayedPhotos[paintingNo] != null && PhotoManager.manage.displayedPhotos[paintingNo].photoName != "Dummy")
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ConversationManager.manage.lastTalkTo.getVendorConversation().GetComponent<MuseumConvoGroup>().askAboutPaintingInFrameWithPhotos);
			}
			else
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ConversationManager.manage.lastTalkTo.getVendorConversation().GetComponent<MuseumConvoGroup>().askAboutEmptyFrameWithPhotos);
			}
		}
		else if (PhotoManager.manage.displayedPhotos[paintingNo] != null && PhotoManager.manage.displayedPhotos[paintingNo].photoName != "Dummy")
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ConversationManager.manage.lastTalkTo.getVendorConversation().GetComponent<MuseumConvoGroup>().nonLocalAskAboutPhoto);
		}
		else
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ConversationManager.manage.lastTalkTo.getVendorConversation().GetComponent<MuseumConvoGroup>().nonLocalAskAboutEmpty);
		}
	}

	public bool checkIfMuseumKeeperIsAtWork()
	{
		return NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Museum).isAtWork();
	}

	public bool checkIfMuseumKeeperCanBeTalkedTo()
	{
		return NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Museum).canBeTalkTo();
	}

	public void updatePainting()
	{
		if (MuseumManager.manage.paintingsOnDisplay[paintingNo] != null)
		{
			mesh.materials[1].SetTexture("_MainTex", MuseumManager.manage.paintingsOnDisplay[paintingNo]);
		}
	}
}
