using System;
using UnityEngine;

[Serializable]
public class PostOnBoard
{
	public int postTextId = 1;

	public int daysToComplete = 1;

	public int postTypeId;

	public int templateType;

	public int postedByNpcId;

	public bool hasBeenRead;

	public bool accepted;

	public bool readyForNPC;

	public bool completed;

	public int rewardId = -1;

	public int rewardAmount = 1;

	public bool isHuntingTask;

	public HuntingChallenge myHuntingChallenge;

	public bool isPhotoTask;

	public PhotoChallenge myPhotoChallenge;

	public bool isTrade;

	public bool isCaptureTask;

	public int animalToCapture;

	public int captureVariation;

	public int requiredItem = -1;

	public int requireItemAmount = 1;

	public bool isInvestigation;

	public int[] location = new int[3];

	public PostOnBoard()
	{
	}

	public PostOnBoard(int newPostTextId, int newPostedByNPC, BulletinBoard.BoardPostType postType)
	{
		postTypeId = (int)postType;
		populatePost(newPostTextId, newPostedByNPC, (int)postType);
	}

	public PostOnBoard(int newPostTextId, int newPostedByNPC, int daysRemaining, BulletinBoard.BoardPostType postType)
	{
		postTypeId = (int)postType;
		populatePost(newPostTextId, newPostedByNPC, (int)postType);
		daysToComplete = daysRemaining;
	}

	public void populateOnLoad()
	{
		getTemplateAndAddToList();
	}

	private void populatePost(int newPostTextId, int newPostedByNPC, int postType)
	{
		postTextId = newPostTextId;
		postedByNpcId = newPostedByNPC;
		createRandomPostFromSeed(newPostTextId);
		checkIfExpired();
	}

	public void completeTask(CharMovement completedBy)
	{
		readyForNPC = true;
		daysToComplete = 0;
		completed = true;
		BulletinBoard.board.checkAllMissionsForPhotosOnChange();
		if ((bool)completedBy && completedBy != NetworkMapSharer.share.localChar)
		{
			NotificationManager.manage.makeTopNotification("Request Completed by " + completedBy.myEquip.playerName + "!", "", GiftedItemWindow.gifted.licnecePopUpSound);
		}
		else if (isInvestigation)
		{
			NotificationManager.manage.makeTopNotification("Investigation Request Complete!", "", GiftedItemWindow.gifted.licnecePopUpSound);
		}
		else
		{
			NotificationManager.manage.makeTopNotification("Request Complete!", "", GiftedItemWindow.gifted.licnecePopUpSound);
		}
	}

	public BullitenBoardPost getTemplateAndAddToList()
	{
		BullitenBoardPost bullitenBoardPost = ScriptableObject.CreateInstance("BullitenBoardPost") as BullitenBoardPost;
		if (postTypeId == 1)
		{
			BulletinBoard.board.announcementPosts[0].copyPostContents(bullitenBoardPost);
		}
		else if (templateType == 0)
		{
			BulletinBoard.board.huntingTemplate.copyPostContents(bullitenBoardPost);
		}
		else if (templateType == 1)
		{
			BulletinBoard.board.captureTemplate.copyPostContents(bullitenBoardPost);
		}
		else if (templateType == 2)
		{
			BulletinBoard.board.tradeTemplate.copyPostContents(bullitenBoardPost);
		}
		else if (templateType == 3)
		{
			BulletinBoard.board.photoTemplate.copyPostContents(bullitenBoardPost);
		}
		else if (templateType == 4)
		{
			BulletinBoard.board.cookingTemplate.copyPostContents(bullitenBoardPost);
		}
		else if (templateType == 5)
		{
			BulletinBoard.board.smeltingTemplate.copyPostContents(bullitenBoardPost);
		}
		else if (templateType == 6)
		{
			BulletinBoard.board.compostTemplate.copyPostContents(bullitenBoardPost);
		}
		else if (templateType == 7)
		{
			BulletinBoard.board.sateliteTemplate.copyPostContents(bullitenBoardPost);
		}
		else if (templateType == 8)
		{
			BulletinBoard.board.craftingTemplate.copyPostContents(bullitenBoardPost);
		}
		else if (templateType == 9)
		{
			BulletinBoard.board.shippingRequestTemplate.copyPostContents(bullitenBoardPost);
		}
		BulletinBoard.board.randomPosts.Add(bullitenBoardPost);
		return bullitenBoardPost;
	}

	public void createRandomPostFromSeed(int seed)
	{
		UnityEngine.Random.InitState(seed);
		int num = UnityEngine.Random.Range(0, 27);
		daysToComplete = UnityEngine.Random.Range(2, 4);
		if (postTypeId == 1)
		{
			getTemplateAndAddToList();
			daysToComplete = 2;
		}
		else if (num <= 4)
		{
			templateType = 0;
			getTemplateAndAddToList().randomiseHuntingConditions(this);
			isHuntingTask = true;
		}
		else if (num <= 7)
		{
			templateType = 1;
			getTemplateAndAddToList().randomiseCaptureConditions(this);
			daysToComplete = UnityEngine.Random.Range(1, 2);
			postedByNpcId = -1;
		}
		else if (num <= 10)
		{
			templateType = 2;
			getTemplateAndAddToList().randomiseTradeConditions(this);
		}
		else if (num <= 13)
		{
			templateType = 3;
			getTemplateAndAddToList().randomisePhotoConditions(this);
			isPhotoTask = true;
		}
		else if (num <= 16)
		{
			templateType = 4;
			getTemplateAndAddToList().randomiseCookingConditions(this);
		}
		else if (num <= 19)
		{
			templateType = 5;
			getTemplateAndAddToList().randomiseSmeltingCoditions(this);
		}
		else if (num == 21)
		{
			templateType = 6;
			getTemplateAndAddToList().randomiseSmeltingCoditions(this);
		}
		else if (num <= 23)
		{
			templateType = 8;
			getTemplateAndAddToList().randomiseCraftingConditions(this);
		}
		else if (num <= 25)
		{
			templateType = 9;
			getTemplateAndAddToList().randomiseShippingConditions(this);
		}
		else
		{
			templateType = 7;
			getTemplateAndAddToList().randomiseSateliteConditions(this);
		}
	}

	public int getPostIdOnBoard()
	{
		return BulletinBoard.board.attachedPosts.IndexOf(this);
	}

	public BullitenBoardPost getPostPostsById()
	{
		if (postTypeId == 0)
		{
			return BulletinBoard.board.randomPosts[BulletinBoard.board.attachedPosts.IndexOf(this)];
		}
		if (postTypeId == 1)
		{
			return BulletinBoard.board.announcementPosts[postTextId];
		}
		if (postTypeId == 2)
		{
			return BulletinBoard.board.reminderPosts[postTextId];
		}
		Debug.Log(postTypeId);
		return null;
	}

	public string getTitleText(int postId)
	{
		return getPostPostsById().title.Replace("<boardRewardItem>", getPostPostsById().getBoardRewardItem(postId)).Replace("<boardHuntRequestAnimal>", getPostPostsById().getBoardHuntRequestAnimal(postId)).Replace("<boardRequestItem>", getPostPostsById().getBoardRequestItem(postId));
	}

	public string getPostedByName()
	{
		if (postTypeId == 1)
		{
			return "Town Announcement";
		}
		if (postedByNpcId == -1)
		{
			return "Animal Research Centre";
		}
		return NPCManager.manage.NPCDetails[postedByNpcId].NPCName;
	}

	public string getContentText(int postId)
	{
		return getPostPostsById().contentText.Replace("<boardRewardItem>", getPostPostsById().getBoardRewardItem(postId)).Replace("<boardHuntRequestAnimal>", getPostPostsById().getBoardHuntRequestAnimal(postId)).Replace("<getAnimalsInPhotoList>", getPostPostsById().getRequirementsNeededInPhoto(postId))
			.Replace("<boardRequestItem>", getPostPostsById().getBoardRequestItem(postId));
	}

	public bool checkIfExpired()
	{
		return daysToComplete < 0;
	}

	public int getDaysUntilExpired()
	{
		return daysToComplete;
	}

	public bool checkIfAnimalCapturedQuest(int animalId, int variation)
	{
		if (isCaptureTask)
		{
			Debug.Log("checking post with " + animalToCapture + " variation " + captureVariation);
			if ((animalToCapture == animalId && captureVariation == variation) || (animalToCapture == animalId && captureVariation == -1))
			{
				return true;
			}
		}
		return false;
	}

	public float returnDistanceFromKilledPos(Vector3 locationKilled)
	{
		if (isHuntingTask)
		{
			return Vector3.Distance(locationKilled, getRequiredLocation());
		}
		return 5000f;
	}

	public bool checkIfHuntingQuestAndHasSameAnimalId(int animalId)
	{
		if (isHuntingTask && (bool)AnimalManager.manage.allAnimals[animalId].saveAsAlpha && myHuntingChallenge.getAnimalId() == animalId)
		{
			return true;
		}
		return false;
	}

	public bool checkIfTradeItemIsOk(int itemToTrade)
	{
		if (Inventory.inv.allItems[rewardId].isFurniture && Inventory.inv.allItems[itemToTrade].isFurniture)
		{
			return true;
		}
		if ((bool)Inventory.inv.allItems[rewardId].equipable && Inventory.inv.allItems[rewardId].equipable.cloths && (bool)Inventory.inv.allItems[itemToTrade].equipable && Inventory.inv.allItems[itemToTrade].equipable.cloths)
		{
			return true;
		}
		return false;
	}

	public bool checkIfTradeItemIsDifferent(int itemId)
	{
		if (itemId == rewardId)
		{
			return false;
		}
		return true;
	}

	public void checkIfLatestPhotoHasRequests(PhotoDetails checkPhoto)
	{
		BulletinBoard.board.checkAllMissionsForPhotosOnChange();
	}

	public bool checkIfPhotoShownIsCorrect(PhotoDetails checkPhoto)
	{
		if (checkIfAccepted() && isPhotoTask && myPhotoChallenge.checkIfPhotoMeetsRequirements(checkPhoto))
		{
			QuestTracker.track.updateTasksEvent.Invoke();
			return true;
		}
		return false;
	}

	public void acceptTask(CharMovement charToAccept)
	{
		accepted = true;
		checkIfHasEnoughItems();
		if (isTrade)
		{
			readyForNPC = true;
		}
		BulletinBoard.board.checkAllMissionsForPhotosOnChange();
		BulletinBoard.board.checkIfHuntingTargetNeedsToBeSpawned(getPostIdOnBoard());
		RenderMap.map.createTaskIcon(this);
		if (getRequiredLocation() != Vector3.zero)
		{
			if (charToAccept != NetworkMapSharer.share.localChar)
			{
				NotificationManager.manage.makeTopNotification("Request added to Journal by " + charToAccept.myEquip.playerName, "A location was added to your map.", SoundManager.manage.taskAcceptedSound);
			}
			else
			{
				NotificationManager.manage.makeTopNotification("Request added to Journal", "A location was added to your map.", SoundManager.manage.taskAcceptedSound);
			}
		}
		else if (charToAccept != NetworkMapSharer.share.localChar)
		{
			NotificationManager.manage.makeTopNotification("Request added to Journal by " + charToAccept.myEquip.playerName, "This request has a time limit.", SoundManager.manage.taskAcceptedSound);
		}
		else
		{
			NotificationManager.manage.makeTopNotification("Request added to Journal", "This request has a time limit.", SoundManager.manage.taskAcceptedSound);
		}
	}

	public void checkIfHasEnoughItems()
	{
		int num = 0;
		if (requiredItem <= -1)
		{
			return;
		}
		InventorySlot inventorySlot = null;
		for (int i = 0; i < Inventory.inv.invSlots.Length; i++)
		{
			if (Inventory.inv.invSlots[i].isSelectedForGive())
			{
				inventorySlot = Inventory.inv.invSlots[i];
				break;
			}
		}
		if ((bool)GiveNPC.give && (bool)inventorySlot && inventorySlot.itemNo == requiredItem)
		{
			num = inventorySlot.stack;
		}
		num += Inventory.inv.getAmountOfItemInAllSlots(requiredItem);
		if (num >= requireItemAmount)
		{
			readyForNPC = true;
		}
		else
		{
			readyForNPC = false;
		}
		QuestTracker.track.updateTasksEvent.Invoke();
	}

	public int checkAmountOfItemsInInv()
	{
		return Inventory.inv.getAmountOfItemInAllSlots(requiredItem);
	}

	public bool checkIfCanBeAccepted()
	{
		if (postTypeId == 0 && !checkIfExpired())
		{
			return true;
		}
		return false;
	}

	public Vector3 getRequiredLocation()
	{
		if (isPhotoTask)
		{
			return myPhotoChallenge.getRequiredLocation();
		}
		if (isHuntingTask)
		{
			return myHuntingChallenge.getLocation();
		}
		if (isInvestigation)
		{
			return new Vector3(location[0] * 2, location[1], location[2] * 2);
		}
		return Vector3.zero;
	}

	public bool checkIfAccepted()
	{
		return accepted;
	}

	public BulletinBoard.BoardPostType getPostTypeId()
	{
		return (BulletinBoard.BoardPostType)postTypeId;
	}
}
