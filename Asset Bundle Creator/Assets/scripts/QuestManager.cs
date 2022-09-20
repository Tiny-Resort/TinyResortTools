using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
	public static QuestManager manage;

	public GameObject questNotificationPrefab;

	public Quest[] allQuests;

	public bool[] isQuestAccepted;

	public bool[] isQuestCompleted;

	public List<QuestNotification> currentQuestsNotifications;

	public Transform QuestScrollWindow;

	public Text QuestSelectedName;

	public Text QuestSelectedDescription;

	public Transform QuestSelectedRequiredItemsWindow;

	public Text QuestSelectedReward;

	public Quest lastQuestTalkingAbout;

	private void Awake()
	{
		manage = this;
		isQuestCompleted = new bool[allQuests.Length];
		isQuestAccepted = new bool[allQuests.Length];
	}

	public void addAQuest(Quest newQuest)
	{
	}

	public void completeAQuest(Quest completedQuest)
	{
	}

	public Conversation checkIfThereIsAQuestToGive(int NPCNo)
	{
		if (!NetworkMapSharer.share.localChar.isServer)
		{
			return null;
		}
		if (NPCNo < 0)
		{
			return null;
		}
		TownManager.manage.calculateTownScore();
		for (int i = 0; i < isQuestCompleted.Length; i++)
		{
			if (NPCNo == allQuests[i].offeredByNpc && allQuests[i].attractResidentsQuest)
			{
				if (WorldManager.manageWorld.month > 1 || WorldManager.manageWorld.year > 1 || WorldManager.manageWorld.day + WorldManager.manageWorld.week * 7 >= 16)
				{
					if (!isQuestAccepted[i] && NPCManager.manage.getNoOfNPCsMovedIn() < 5)
					{
						isQuestAccepted[i] = true;
						return allQuests[i].questAcceptedConversation;
					}
					if (!isQuestCompleted[i] && NPCManager.manage.getNoOfNPCsMovedIn() >= 5)
					{
						if (BuildingManager.manage.currentlyMoving == -1 || BuildingManager.manage.currentlyMoving != allQuests[i].requiredBuilding[0].tileObjectId)
						{
							completeQuest(i);
							isQuestAccepted[i] = true;
							return allQuests[i].completedConversation;
						}
						return null;
					}
				}
				return null;
			}
			if (NPCNo == allQuests[i].offeredByNpc && !isQuestCompleted[i] && allQuests[i].teleporterQuest)
			{
				if (!NPCManager.manage.npcStatus[allQuests[i].offeredByNpc].checkIfHasMovedIn())
				{
					continue;
				}
				int num = 0;
				if (NetworkMapSharer.share.northOn)
				{
					num++;
				}
				if (NetworkMapSharer.share.eastOn)
				{
					num++;
				}
				if (NetworkMapSharer.share.southOn)
				{
					num++;
				}
				if (NetworkMapSharer.share.westOn)
				{
					num++;
				}
				if (num >= 2)
				{
					isQuestAccepted[i] = true;
					completeQuest(i);
					if ((bool)allQuests[i].questConversation)
					{
						return allQuests[i].questConversation;
					}
				}
			}
			else if (NPCNo == allQuests[i].offeredByNpc && !isQuestCompleted[i] && allQuests[i].townBeautyLevelRequired > 0f)
			{
				if (TownManager.manage.townBeautyLevel >= allQuests[i].townBeautyLevelRequired)
				{
					isQuestAccepted[i] = true;
					completeQuest(i);
					if ((bool)allQuests[i].questConversation)
					{
						return allQuests[i].questConversation;
					}
				}
			}
			else if (NPCNo == allQuests[i].offeredByNpc && !isQuestCompleted[i] && allQuests[i].townEcconnomyLevelRequired > 0f)
			{
				if (TownManager.manage.townEconomyLevel >= allQuests[i].townEcconnomyLevelRequired)
				{
					isQuestAccepted[i] = true;
					completeQuest(i);
					if ((bool)allQuests[i].questConversation)
					{
						return allQuests[i].questConversation;
					}
				}
			}
			else
			{
				if (NPCNo != allQuests[i].offeredByNpc || isQuestCompleted[i] || NPCManager.manage.npcStatus[NPCNo].relationshipLevel < allQuests[i].relationshipLevelNeeded || NPCNo != allQuests[i].offeredByNpc)
				{
					continue;
				}
				lastQuestTalkingAbout = allQuests[i];
				if (!isQuestAccepted[i])
				{
					if ((bool)allQuests[i].questConversation)
					{
						return allQuests[i].questConversation;
					}
					continue;
				}
				if (!allQuests[i].checkIfComplete())
				{
					if ((bool)allQuests[i].questAcceptedConversation)
					{
						return allQuests[i].questAcceptedConversation;
					}
					continue;
				}
				completeQuest(i);
				if ((bool)allQuests[i].completedConversation)
				{
					return allQuests[i].completedConversation;
				}
			}
		}
		return null;
	}

	public void completeQuest(int questNo)
	{
		isQuestCompleted[questNo] = true;
		DailyTaskGenerator.generate.doATask(allQuests[questNo].doTaskOnComplete);
		if (allQuests[questNo].townHallQuest || allQuests[questNo].attractResidentsQuest)
		{
			allQuests[questNo].upgradeBaseTent();
		}
		else if ((bool)allQuests[questNo].deedUnlockedOnAccept)
		{
			NotificationManager.manage.makeTopNotification("A new deed is available!", "Talk to Fletch to apply for deeds.", SoundManager.manage.notificationSound);
			DeedManager.manage.unlockDeed(allQuests[questNo].deedUnlockedOnAccept);
		}
		if (allQuests[questNo].acceptQuestOnComplete != -1)
		{
			isQuestAccepted[allQuests[questNo].acceptQuestOnComplete] = true;
		}
		for (int i = 0; i < allQuests[questNo].rewardOnComplete.Length; i++)
		{
			GiftedItemWindow.gifted.addToListToBeGiven(Inventory.inv.getInvItemId(allQuests[questNo].rewardOnComplete[i]), allQuests[questNo].rewardStacksGiven[i]);
		}
		InventoryItem[] unlockRecipeOnComplete = allQuests[questNo].unlockRecipeOnComplete;
		foreach (InventoryItem item in unlockRecipeOnComplete)
		{
			GiftedItemWindow.gifted.addRecipeToUnlock(Inventory.inv.getInvItemId(item));
		}
		GiftedItemWindow.gifted.openWindowAndGiveItems();
	}

	public void acceptQuestLastTalkedAbout()
	{
		for (int i = 0; i < isQuestAccepted.Length; i++)
		{
			if (!(allQuests[i] == lastQuestTalkingAbout) || isQuestAccepted[i])
			{
				continue;
			}
			isQuestAccepted[i] = true;
			if (!QuestTracker.track.hasPinnedMission())
			{
				QuestTracker.track.pinTheTask(QuestTracker.typeOfTask.Quest, i);
			}
			for (int j = 0; j < lastQuestTalkingAbout.giveItemsOnAccept.Length; j++)
			{
				if (j < lastQuestTalkingAbout.amountGivenOnAccept.Length)
				{
					GiftedItemWindow.gifted.addToListToBeGiven(Inventory.inv.getInvItemId(lastQuestTalkingAbout.giveItemsOnAccept[j]), lastQuestTalkingAbout.amountGivenOnAccept[j]);
				}
				else
				{
					GiftedItemWindow.gifted.addToListToBeGiven(Inventory.inv.getInvItemId(lastQuestTalkingAbout.giveItemsOnAccept[j]), 1);
				}
			}
			GiftedItemWindow.gifted.openWindowAndGiveItems();
			if ((bool)lastQuestTalkingAbout.deedUnlockedOnAccept)
			{
				DeedManager.manage.unlockDeed(lastQuestTalkingAbout.deedUnlockedOnAccept);
			}
		}
	}

	public void completeQuestLastTalkedAbout()
	{
		for (int i = 0; i < isQuestAccepted.Length; i++)
		{
			if (allQuests[i] == lastQuestTalkingAbout && !isQuestCompleted[i])
			{
				completeQuest(i);
			}
		}
	}

	public string listRequiredItemsInQuestLastTalkedAbout()
	{
		string text = "";
		for (int i = 0; i < lastQuestTalkingAbout.requiredItems.Length; i++)
		{
			text = ((i == lastQuestTalkingAbout.requiredItems.Length - 1) ? (text + lastQuestTalkingAbout.requiredStacks[i] + " " + lastQuestTalkingAbout.requiredItems[i].getInvItemName() + " ") : (text + lastQuestTalkingAbout.requiredStacks[i] + " " + lastQuestTalkingAbout.requiredItems[i].getInvItemName() + ", "));
		}
		return text;
	}
}
