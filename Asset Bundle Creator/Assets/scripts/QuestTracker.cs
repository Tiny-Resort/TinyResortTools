using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuestTracker : MonoBehaviour
{
	public enum typeOfTask
	{
		None = 0,
		Quest = 1,
		BulletinBoard = 2,
		Request = 3,
		DeedItems = 4,
		Recipe = 5
	}

	public static QuestTracker track;

	public GameObject requiredItemPrefab;

	public GameObject QuestWindow;

	public GameObject noTaskAvaliableWindow;

	public GameObject questButtonPrefab;

	public GameObject requiredItemsBox;

	public Transform questListWindow;

	public TextMeshProUGUI questTitle;

	public TextMeshProUGUI questDesc;

	public TextMeshProUGUI questGiverName;

	public TextMeshProUGUI questDateGiven;

	public TextMeshProUGUI questMission;

	public FillRecipeSlot rewardInfo;

	public bool trackerOpen;

	private List<InvButton> questButtons = new List<InvButton>();

	private int currentlyDisplayingQuestNo = -1;

	private Color defualtColor;

	private Color defualtHoverColor;

	public Image givenByCharImage;

	public GameObject questDescriptionWindow;

	public TextMeshProUGUI pinMissionText;

	public bool pinnedMissionTextOn;

	public UnityEvent updateTasksEvent = new UnityEvent();

	public TextMeshProUGUI pinTaskButtonText;

	public TextMeshProUGUI pinRecipeButtonText;

	public Sprite bulletinBoardMissionIcon;

	public QuestButton trackingRecipeButton;

	private List<GameObject> currentRequiredItemIcons = new List<GameObject>();

	private typeOfTask pinnedType;

	private int pinnedId = -1;

	private typeOfTask lookingAtTask;

	private int lookingAtId = -1;

	private int pinnedRecipeId = -1;

	private PostOnBoard pinnedBuletingBoardPost;

	private void Awake()
	{
		track = this;
	}

	private void Start()
	{
		updateTasksEvent.AddListener(updatePinnedTask);
		updatePinnedTask();
	}

	public void openQuestWindow()
	{
		QuestWindow.SetActive(true);
		defualtColor = questButtonPrefab.GetComponent<InvButton>().defaultColor;
		defualtHoverColor = questButtonPrefab.GetComponent<InvButton>().hoverColor;
		trackerOpen = true;
		fillQuestList();
	}

	public void closeQuestWindow()
	{
		QuestWindow.SetActive(false);
		trackerOpen = false;
	}

	private void fillQuestList()
	{
		for (int i = 0; i < questButtons.Count; i++)
		{
			Object.Destroy(questButtons[i].gameObject);
		}
		questButtons.Clear();
		for (int j = 0; j < QuestManager.manage.allQuests.Length; j++)
		{
			if (QuestManager.manage.isQuestAccepted[j] && !QuestManager.manage.isQuestCompleted[j])
			{
				InvButton component = Object.Instantiate(questButtonPrefab, questListWindow).GetComponent<InvButton>();
				component.craftRecipeNumber = j;
				questButtons.Add(component);
				component.GetComponent<QuestButton>().setUpMainQuest(j);
			}
		}
		for (int k = 0; k < BulletinBoard.board.attachedPosts.Count; k++)
		{
			if (BulletinBoard.board.attachedPosts[k].checkIfAccepted() && !BulletinBoard.board.attachedPosts[k].completed && !BulletinBoard.board.attachedPosts[k].checkIfExpired())
			{
				InvButton component2 = Object.Instantiate(questButtonPrefab, questListWindow).GetComponent<InvButton>();
				component2.craftRecipeNumber = k;
				questButtons.Add(component2);
				component2.GetComponent<QuestButton>().setUp(true, k);
			}
		}
		for (int l = 0; l < NPCManager.manage.NPCRequests.Length; l++)
		{
			if (NPCManager.manage.npcStatus[l].acceptedRequest && !NPCManager.manage.npcStatus[l].completedRequest)
			{
				InvButton component3 = Object.Instantiate(questButtonPrefab, questListWindow).GetComponent<InvButton>();
				component3.craftRecipeNumber = l;
				questButtons.Add(component3);
				component3.GetComponent<QuestButton>().setUp(false, l);
			}
		}
		if (questButtons.Count > 0)
		{
			if (currentlyDisplayingQuestNo == -1)
			{
				questButtons[0].PressButtonDelay();
			}
			else
			{
				displayQuest(currentlyDisplayingQuestNo);
			}
			noTaskAvaliableWindow.gameObject.SetActive(false);
		}
		else
		{
			currentlyDisplayingQuestNo = -1;
			trackingRecipeButton.onPress();
			if (pinnedRecipeId <= -1)
			{
				noTaskAvaliableWindow.gameObject.SetActive(true);
			}
			else
			{
				noTaskAvaliableWindow.gameObject.SetActive(false);
			}
		}
		refreshButtonSelection();
	}

	private void refreshButtonSelection()
	{
	}

	public void displayTrackingRecipe()
	{
		currentlyDisplayingQuestNo = -1;
		refreshButtonSelection();
		if (pinnedRecipeId > -1)
		{
			questTitle.text = Inventory.inv.allItems[pinnedRecipeId].getInvItemName() + " Recipe";
			questDesc.text = "These items are required to craft " + Inventory.inv.allItems[pinnedRecipeId].getInvItemName() + "\n Unpin this to stop tracking recipe.";
			questGiverName.text = "";
			questDateGiven.text = "";
			givenByCharImage.sprite = Inventory.inv.allItems[pinnedRecipeId].getSprite();
			fillRequiredItemsBox(Inventory.inv.allItems[pinnedRecipeId]);
			questMission.text = "";
			rewardInfo.gameObject.SetActive(false);
			updateLookingAtTask(typeOfTask.Recipe, 0);
		}
	}

	public void displayMainQuest(int questNo)
	{
		currentlyDisplayingQuestNo = -1;
		refreshButtonSelection();
		questTitle.text = QuestManager.manage.allQuests[questNo].QuestName;
		questDesc.text = QuestManager.manage.allQuests[questNo].QuestDescription.Replace("<IslandName>", Inventory.inv.islandName);
		questGiverName.text = NPCManager.manage.NPCDetails[QuestManager.manage.allQuests[questNo].offeredByNpc].NPCName;
		questDateGiven.text = "";
		fillRequiredItemsBox(QuestManager.manage.allQuests[questNo]);
		questMission.text = QuestManager.manage.allQuests[questNo].getMissionObjText();
		rewardInfo.gameObject.SetActive(false);
		if (QuestManager.manage.allQuests[questNo].offeredByNpc <= 11)
		{
			givenByCharImage.sprite = NPCManager.manage.NPCDetails[QuestManager.manage.allQuests[questNo].offeredByNpc].npcSprite;
		}
		else
		{
			givenByCharImage.sprite = NPCManager.manage.NPCDetails[QuestManager.manage.allQuests[questNo].offeredByNpc].getNPCSprite(QuestManager.manage.allQuests[questNo].offeredByNpc);
		}
		updateLookingAtTask(typeOfTask.Quest, questNo);
		replaceMissionTextSprites();
	}

	public void displayRequest(int requestNo)
	{
		currentlyDisplayingQuestNo = -1;
		refreshButtonSelection();
		questTitle.text = "Request for " + NPCManager.manage.NPCDetails[requestNo].NPCName;
		questDesc.text = NPCManager.manage.NPCDetails[requestNo].NPCName + " has asked you to get " + NPCManager.manage.NPCRequests[requestNo].getDesiredItemName();
		questGiverName.text = NPCManager.manage.NPCDetails[requestNo].NPCName;
		questDateGiven.text = "By the end of the day";
		questMission.text = NPCManager.manage.NPCRequests[requestNo].getMissionText(requestNo);
		rewardInfo.gameObject.SetActive(false);
		fillRequiredItemsBox(NPCManager.manage.NPCRequests[requestNo]);
		if (requestNo <= 11)
		{
			givenByCharImage.sprite = NPCManager.manage.NPCDetails[requestNo].npcSprite;
		}
		else
		{
			givenByCharImage.sprite = NPCManager.manage.NPCDetails[requestNo].getNPCSprite(requestNo);
		}
		updateLookingAtTask(typeOfTask.Request, requestNo);
		replaceMissionTextSprites();
	}

	public void displayQuest(int boardId)
	{
		currentlyDisplayingQuestNo = boardId;
		refreshButtonSelection();
		questTitle.text = BulletinBoard.board.attachedPosts[boardId].getTitleText(boardId);
		questDesc.text = BulletinBoard.board.attachedPosts[boardId].getContentText(boardId);
		questGiverName.text = BulletinBoard.board.attachedPosts[boardId].getPostedByName();
		questDateGiven.text = BulletinBoard.board.attachedPosts[boardId].getDaysUntilExpired() + " days remaining";
		questMission.text = BulletinBoard.board.getMissionText(boardId);
		fillRequiredItemsBox(BulletinBoard.board.attachedPosts[boardId]);
		if (BulletinBoard.board.attachedPosts[boardId].rewardId > -1)
		{
			rewardInfo.fillRecipeSlotForQuestReward(BulletinBoard.board.attachedPosts[boardId].rewardId, BulletinBoard.board.attachedPosts[boardId].rewardAmount);
			rewardInfo.gameObject.SetActive(false);
		}
		else
		{
			rewardInfo.gameObject.SetActive(false);
		}
		questGiverName.text = BulletinBoard.board.attachedPosts[boardId].getPostedByName();
		if (BulletinBoard.board.attachedPosts[boardId].postedByNpcId < 0)
		{
			givenByCharImage.sprite = bulletinBoardMissionIcon;
		}
		else if (BulletinBoard.board.attachedPosts[boardId].postedByNpcId > 0 && BulletinBoard.board.attachedPosts[boardId].postedByNpcId <= 11)
		{
			givenByCharImage.sprite = NPCManager.manage.NPCDetails[BulletinBoard.board.attachedPosts[boardId].postedByNpcId].npcSprite;
		}
		else
		{
			givenByCharImage.sprite = NPCManager.manage.NPCDetails[BulletinBoard.board.attachedPosts[boardId].postedByNpcId].getNPCSprite(BulletinBoard.board.attachedPosts[boardId].postedByNpcId);
		}
		updateLookingAtTask(typeOfTask.BulletinBoard, boardId);
		replaceMissionTextSprites();
	}

	public void fillRequiredItemsBox(Quest questToFillFrom)
	{
		foreach (GameObject currentRequiredItemIcon in currentRequiredItemIcons)
		{
			Object.Destroy(currentRequiredItemIcon);
		}
		currentRequiredItemIcons.Clear();
		requiredItemsBox.SetActive(questToFillFrom.requiredItems.Length != 0);
		for (int i = 0; i < questToFillFrom.requiredItems.Length; i++)
		{
			int invItemId = Inventory.inv.getInvItemId(questToFillFrom.requiredItems[i]);
			currentRequiredItemIcons.Add(Object.Instantiate(requiredItemPrefab, requiredItemsBox.transform));
			currentRequiredItemIcons[currentRequiredItemIcons.Count - 1].GetComponent<FillRecipeSlot>().fillRecipeSlotWithAmounts(invItemId, Inventory.inv.getAmountOfItemInAllSlots(invItemId), questToFillFrom.requiredStacks[i]);
		}
	}

	public void fillRequiredItemsBox(InventoryItem craftable)
	{
		foreach (GameObject currentRequiredItemIcon in currentRequiredItemIcons)
		{
			Object.Destroy(currentRequiredItemIcon);
		}
		currentRequiredItemIcons.Clear();
		requiredItemsBox.SetActive(true);
		for (int i = 0; i < craftable.craftable.itemsInRecipe.Length; i++)
		{
			int invItemId = Inventory.inv.getInvItemId(craftable.craftable.itemsInRecipe[i]);
			currentRequiredItemIcons.Add(Object.Instantiate(requiredItemPrefab, requiredItemsBox.transform));
			currentRequiredItemIcons[currentRequiredItemIcons.Count - 1].GetComponent<FillRecipeSlot>().fillRecipeSlotWithAmounts(invItemId, Inventory.inv.getAmountOfItemInAllSlots(invItemId), craftable.craftable.stackOfItemsInRecipe[i]);
		}
	}

	public void fillRequiredItemsBox(NPCRequest request)
	{
		foreach (GameObject currentRequiredItemIcon in currentRequiredItemIcons)
		{
			Object.Destroy(currentRequiredItemIcon);
		}
		currentRequiredItemIcons.Clear();
		requiredItemsBox.SetActive(true);
		if (request.specificDesiredItem == -1)
		{
			requiredItemsBox.SetActive(false);
			return;
		}
		int specificDesiredItem = request.specificDesiredItem;
		currentRequiredItemIcons.Add(Object.Instantiate(requiredItemPrefab, requiredItemsBox.transform));
		currentRequiredItemIcons[0].GetComponent<FillRecipeSlot>().fillRecipeSlotWithAmounts(specificDesiredItem, Inventory.inv.getAmountOfItemInAllSlots(specificDesiredItem), request.desiredAmount);
	}

	public void fillRequiredItemsBox(PostOnBoard boardPost)
	{
		foreach (GameObject currentRequiredItemIcon in currentRequiredItemIcons)
		{
			Object.Destroy(currentRequiredItemIcon);
		}
		currentRequiredItemIcons.Clear();
		if (boardPost.requiredItem <= -1)
		{
			requiredItemsBox.SetActive(false);
			return;
		}
		requiredItemsBox.SetActive(true);
		currentRequiredItemIcons.Add(Object.Instantiate(requiredItemPrefab, requiredItemsBox.transform));
		currentRequiredItemIcons[0].GetComponent<FillRecipeSlot>().fillRecipeSlotWithAmounts(boardPost.requiredItem, Inventory.inv.getAmountOfItemInAllSlots(boardPost.requiredItem), boardPost.requireItemAmount);
	}

	public bool hasPinnedMission()
	{
		if (pinnedId != -1)
		{
			return true;
		}
		return false;
	}

	public void pressPinTaskButton()
	{
		pinnedRecipeId = -1;
		trackingRecipeButton.gameObject.SetActive(false);
		if (pinnedType == typeOfTask.Recipe)
		{
			openQuestWindow();
		}
		if (pinnedType == lookingAtTask && pinnedId == lookingAtId)
		{
			unpinTask();
		}
		else if (lookingAtId == -1 || lookingAtTask == typeOfTask.None)
		{
			pinTheTask(typeOfTask.None, -1);
		}
		else if (lookingAtTask == typeOfTask.BulletinBoard)
		{
			if (lookingAtId < BulletinBoard.board.attachedPosts.Count)
			{
				MonoBehaviour.print("Pinned a bulletin board post");
				pinnedBuletingBoardPost = BulletinBoard.board.attachedPosts[lookingAtId];
				pinTheTask(lookingAtTask, lookingAtId);
			}
			else
			{
				pinnedBuletingBoardPost = null;
			}
		}
		else
		{
			pinnedBuletingBoardPost = null;
			pinTheTask(lookingAtTask, lookingAtId);
		}
	}

	public void pressPinRecipeButton()
	{
		if (pinnedRecipeId == CraftingManager.manage.craftableItemId)
		{
			pinnedRecipeId = -1;
			pinTheTask(typeOfTask.None, -1);
		}
		else
		{
			pinnedRecipeId = CraftingManager.manage.craftableItemId;
			if (pinnedRecipeId != -1)
			{
				pinTheTask(typeOfTask.Recipe, lookingAtId);
			}
		}
		if (pinnedRecipeId != -1)
		{
			trackingRecipeButton.gameObject.SetActive(true);
			trackingRecipeButton.npcIcon.sprite = Inventory.inv.allItems[pinnedRecipeId].getSprite();
			trackingRecipeButton.buttonText.text = Inventory.inv.allItems[pinnedRecipeId].getInvItemName() + " Recipe";
		}
		else
		{
			trackingRecipeButton.gameObject.SetActive(false);
		}
		updatePinnedRecipeButton();
	}

	public void unpinTask()
	{
		pinTheTask(typeOfTask.None, -1);
	}

	public void updateLookingAtTask(typeOfTask type, int id)
	{
		if (type == typeOfTask.Recipe)
		{
			pinTaskButtonText.text = "<sprite=17> Pinned";
			return;
		}
		lookingAtId = id;
		lookingAtTask = type;
		if (pinnedId == id && pinnedType == type)
		{
			pinTaskButtonText.text = "<sprite=17> Pinned";
		}
		else
		{
			pinTaskButtonText.text = "<sprite=16> Pinned";
		}
	}

	public void pinTheTask(typeOfTask type, int id)
	{
		pinnedType = type;
		pinnedId = id;
		if (type != typeOfTask.BulletinBoard || (type == typeOfTask.BulletinBoard && id >= BulletinBoard.board.attachedPosts.Count))
		{
			pinnedBuletingBoardPost = null;
		}
		updatePinnedTask();
		updateLookingAtTask(lookingAtTask, lookingAtId);
	}

	public void updatePinnedTask()
	{
		if (pinnedType == typeOfTask.None)
		{
			pinMissionText.gameObject.SetActive(false);
		}
		else if (!pinnedMissionTextOn)
		{
			pinMissionText.gameObject.SetActive(false);
		}
		else
		{
			pinMissionText.gameObject.SetActive(true);
		}
		if (pinnedType == typeOfTask.Recipe)
		{
			fillMissionTextForRecipe(pinnedRecipeId);
		}
		else if (pinnedType == typeOfTask.Quest)
		{
			if (QuestManager.manage.isQuestCompleted[pinnedId])
			{
				if (QuestManager.manage.allQuests[pinnedId].acceptQuestOnComplete != -1)
				{
					pinnedId = QuestManager.manage.allQuests[pinnedId].acceptQuestOnComplete;
					updatePinnedTask();
				}
				else
				{
					unpinTask();
				}
			}
			else
			{
				pinMissionText.text = QuestManager.manage.allQuests[pinnedId].QuestName + "\n<size=11>" + QuestManager.manage.allQuests[pinnedId].getMissionObjText();
			}
		}
		else if (pinnedType == typeOfTask.BulletinBoard)
		{
			if (pinnedBuletingBoardPost != null)
			{
				if (BulletinBoard.board.attachedPosts.Contains(pinnedBuletingBoardPost))
				{
					pinnedId = BulletinBoard.board.attachedPosts.IndexOf(pinnedBuletingBoardPost);
					if (BulletinBoard.board.attachedPosts[pinnedId].checkIfExpired() || BulletinBoard.board.attachedPosts[pinnedId].completed)
					{
						unpinTask();
					}
					else
					{
						pinMissionText.text = BulletinBoard.board.attachedPosts[pinnedId].getTitleText(pinnedId) + "\n<size=11>" + BulletinBoard.board.getMissionText(pinnedId);
					}
				}
				else
				{
					MonoBehaviour.print("Buleltiung board  Not containted in list");
					unpinTask();
				}
			}
			else
			{
				MonoBehaviour.print("No pinned bulleting board at all");
				unpinTask();
			}
		}
		else if (pinnedType == typeOfTask.Request)
		{
			if (NPCManager.manage.npcStatus[pinnedId].completedRequest)
			{
				unpinTask();
			}
			else
			{
				pinMissionText.text = "Request for " + NPCManager.manage.NPCDetails[pinnedId].NPCName + "\n<size=11>" + NPCManager.manage.NPCRequests[pinnedId].getMissionText(pinnedId);
			}
		}
		else if (pinnedType != typeOfTask.DeedItems)
		{
			pinMissionText.text = "";
			pinMissionText.gameObject.SetActive(false);
		}
	}

	public void replaceMissionTextSprites()
	{
		questMission.text = questMission.text.Replace("<sprite=12>", "<sprite=16>");
		questMission.text = questMission.text.Replace("<sprite=13>", "<sprite=17>");
	}

	public void updatePinnedRecipeButton()
	{
		if (pinnedRecipeId == CraftingManager.manage.craftableItemId)
		{
			pinRecipeButtonText.text = "<sprite=13> Track Recipe Ingredients";
		}
		else
		{
			pinRecipeButtonText.text = "<sprite=12> Track Recipe Ingredients";
		}
	}

	public void fillMissionTextForRecipe(int recipeId)
	{
		Recipe craftable = Inventory.inv.allItems[recipeId].craftable;
		pinMissionText.text = "Crafting " + Inventory.inv.allItems[recipeId].getInvItemName() + "\n<size=11>";
		for (int i = 0; i < craftable.itemsInRecipe.Length; i++)
		{
			int amountOfItemInAllSlots = Inventory.inv.getAmountOfItemInAllSlots(craftable.itemsInRecipe[i].getItemId());
			string text = " [" + amountOfItemInAllSlots + "/" + craftable.stackOfItemsInRecipe[i] + "]";
			if (amountOfItemInAllSlots >= craftable.stackOfItemsInRecipe[i])
			{
				TextMeshProUGUI textMeshProUGUI = pinMissionText;
				textMeshProUGUI.text = textMeshProUGUI.text + "<sprite=13> " + craftable.itemsInRecipe[i].getInvItemName() + text + "\n";
			}
			else
			{
				TextMeshProUGUI textMeshProUGUI = pinMissionText;
				textMeshProUGUI.text = textMeshProUGUI.text + "<sprite=12> " + craftable.itemsInRecipe[i].getInvItemName() + text + "\n";
			}
		}
	}
}
