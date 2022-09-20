using System;
using UnityEngine;

[Serializable]
public class ConversationSequenceAlt
{
	[TextArea(8, 9)]
	public string[] aConverstationSequnce;

	public ConversationManager.specialAction specialAction;

	public TileObject talkingAboutTileObject;

	public InventoryItem talkingAboutItem;

	public NPCDetails talkingAboutNPC;

	public AnimalAI talkingAboutAnimal;
}
