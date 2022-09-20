using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPost", menuName = "BoardPost")]
public class BullitenBoardPost : ScriptableObject
{
	public string title;

	[TextArea(8, 9)]
	public string contentText;

	public bool textOnly;

	[Header("Complete Covos --------------")]
	public Conversation convoOnComplete;

	public Conversation onGivenItem;

	public Conversation onGivenWrongPhoto;

	public Conversation onGivenSameItemForTrade;

	public Conversation onGivenWrongTypeForTrade;

	public TileObject transformativeTileObject;

	public InventoryItem[] itemsRequiredPool;

	public string getBoardRewardItem(int attachedPostId)
	{
		if (BulletinBoard.board.attachedPosts[attachedPostId].rewardId > -1)
		{
			if (BulletinBoard.board.attachedPosts[attachedPostId].rewardAmount > 1)
			{
				return BulletinBoard.board.attachedPosts[attachedPostId].rewardAmount + " " + Inventory.inv.allItems[BulletinBoard.board.attachedPosts[attachedPostId].rewardId].getInvItemName();
			}
			return Inventory.inv.allItems[BulletinBoard.board.attachedPosts[attachedPostId].rewardId].getInvItemName();
		}
		return "";
	}

	public string getBoardHuntRequestAnimal(int attachedPostId)
	{
		if (BulletinBoard.board.attachedPosts[attachedPostId].isHuntingTask)
		{
			return BulletinBoard.board.attachedPosts[attachedPostId].myHuntingChallenge.getAnimalName();
		}
		if (BulletinBoard.board.attachedPosts[attachedPostId].isCaptureTask)
		{
			if (BulletinBoard.board.attachedPosts[attachedPostId].captureVariation != -1)
			{
				return AnimalManager.manage.allAnimals[BulletinBoard.board.attachedPosts[attachedPostId].animalToCapture].hasVariation.variationAdjective[BulletinBoard.board.attachedPosts[attachedPostId].captureVariation] + AnimalManager.manage.allAnimals[BulletinBoard.board.attachedPosts[attachedPostId].animalToCapture].animalName;
			}
			return AnimalManager.manage.allAnimals[BulletinBoard.board.attachedPosts[attachedPostId].animalToCapture].animalName;
		}
		return "";
	}

	public void copyPostContents(BullitenBoardPost copyTo)
	{
		copyTo.title = title;
		copyTo.contentText = contentText;
		copyTo.convoOnComplete = convoOnComplete;
		copyTo.onGivenItem = onGivenItem;
		copyTo.onGivenWrongPhoto = onGivenWrongPhoto;
		copyTo.transformativeTileObject = transformativeTileObject;
		copyTo.onGivenSameItemForTrade = onGivenSameItemForTrade;
		copyTo.onGivenWrongTypeForTrade = onGivenWrongTypeForTrade;
		copyTo.itemsRequiredPool = itemsRequiredPool;
	}

	public string getRequirementsNeededInPhoto(int postId)
	{
		if (!BulletinBoard.board.attachedPosts[postId].isPhotoTask)
		{
			return "";
		}
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		string text = "of ";
		if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Animal)
		{
			for (int i = 0; i < BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.animalsRequiredInPhoto().Length; i++)
			{
				string item = BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.animalsRequiredInPhoto()[i].animalName;
				if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.animalsRequiredInPhoto()[i].animalId == 1)
				{
					item = AnimalManager.manage.allAnimals[1].GetComponent<FishType>().getFishInvItem().getInvItemName();
				}
				else if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.animalsRequiredInPhoto()[i].animalId == 2)
				{
					item = BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.animalsRequiredInPhoto()[i].GetComponent<BugTypes>().bugInvItem().itemName;
				}
				if (!list.Contains(item))
				{
					list.Add(item);
					list2.Add(1);
				}
				else
				{
					list2[list.IndexOf(item)]++;
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				text = ((list2[j] <= 1) ? (text + "a " + list[j]) : (text + list2[j] + " " + list[j]));
				if (j != list.Count - 1)
				{
					text = ((j != list.Count - 2 || list.Count <= 1) ? (text + ", ") : (text + " and "));
				}
			}
		}
		else
		{
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Npc)
			{
				return "of " + NPCManager.manage.NPCDetails[BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.returnSubjectId()[0]].NPCName;
			}
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Location)
			{
				return "taken near this location.";
			}
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Carryable)
			{
				if ((bool)SaveLoad.saveOrLoad.carryablePrefabs[BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.returnSubjectId()[0]].GetComponent<SellByWeight>())
				{
					return "of a " + SaveLoad.saveOrLoad.carryablePrefabs[BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.returnSubjectId()[0]].GetComponent<SellByWeight>().itemName;
				}
				return "of a " + SaveLoad.saveOrLoad.carryablePrefabs[BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.returnSubjectId()[0]].name;
			}
			if (BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.getSubjectType() == PhotoChallengeManager.photoSubject.Biome)
			{
				return "taken in the  " + BulletinBoard.board.attachedPosts[postId].myPhotoChallenge.returnRequiredLocationBiomeName();
			}
		}
		return text;
	}

	public string getBoardRequestItem(int attachedPostId)
	{
		if (BulletinBoard.board.attachedPosts[attachedPostId].requiredItem > -1)
		{
			if (BulletinBoard.board.attachedPosts[attachedPostId].requireItemAmount <= 1)
			{
				return Inventory.inv.allItems[BulletinBoard.board.attachedPosts[attachedPostId].requiredItem].getInvItemName();
			}
			return BulletinBoard.board.attachedPosts[attachedPostId].requireItemAmount + " " + Inventory.inv.allItems[BulletinBoard.board.attachedPosts[attachedPostId].requiredItem].getInvItemName();
		}
		if (BulletinBoard.board.attachedPosts[attachedPostId].isTrade)
		{
			if (Inventory.inv.allItems[BulletinBoard.board.attachedPosts[attachedPostId].rewardId].isFurniture)
			{
				return "any other furniture";
			}
			if ((bool)Inventory.inv.allItems[BulletinBoard.board.attachedPosts[attachedPostId].rewardId].equipable && Inventory.inv.allItems[BulletinBoard.board.attachedPosts[attachedPostId].rewardId].equipable.cloths)
			{
				return "any other clothing";
			}
		}
		return "";
	}

	public void randomiseHuntingConditions(PostOnBoard postToRandomise)
	{
		postToRandomise.isHuntingTask = true;
		postToRandomise.myHuntingChallenge = HuntingChallengeManager.manage.createNewChallengeAndAttachToPost();
		postToRandomise.rewardId = Inventory.inv.moneyItem.getItemId();
		postToRandomise.rewardAmount = Mathf.RoundToInt((float)AnimalManager.manage.allAnimals[postToRandomise.myHuntingChallenge.getAnimalId()].dangerValue * 1.5f);
	}

	public void randomiseCaptureConditions(PostOnBoard postToRandomise)
	{
		postToRandomise.isCaptureTask = true;
		TrapActivate component = WorldManager.manageWorld.allObjects[141].transform.Find("TouchSpot").GetComponent<TrapActivate>();
		int num = Random.Range(0, component.canCatch.Length - 1);
		postToRandomise.animalToCapture = component.canCatch[num].animalId;
		if ((bool)AnimalManager.manage.allAnimals[postToRandomise.animalToCapture].hasVariation)
		{
			postToRandomise.captureVariation = AnimalManager.manage.allAnimals[postToRandomise.animalToCapture].getRandomVariationNo();
		}
		else
		{
			postToRandomise.captureVariation = -1;
		}
		postToRandomise.rewardId = Inventory.inv.moneyItem.getItemId();
		postToRandomise.rewardAmount = (int)((float)AnimalManager.manage.allAnimals[postToRandomise.animalToCapture].dangerValue * Random.Range(15f, 20f));
	}

	public void randomiseTradeConditions(PostOnBoard postToRandomise)
	{
		new List<InventoryItem>();
		postToRandomise.rewardId = RandomObjectGenerator.generate.getRandomClothing();
		postToRandomise.rewardAmount = 1;
		postToRandomise.isTrade = true;
	}

	public void randomisePhotoConditions(PostOnBoard postToRandomise)
	{
		postToRandomise.isPhotoTask = true;
		postToRandomise.rewardId = Inventory.inv.moneyItem.getItemId();
		postToRandomise.myPhotoChallenge = PhotoChallengeManager.manage.createRandomPhotoChallengeAndAttachToPost();
		postToRandomise.rewardId = Inventory.inv.moneyItem.getItemId();
		postToRandomise.rewardAmount = postToRandomise.myPhotoChallenge.getReward();
	}

	public void randomiseCookingConditions(PostOnBoard postToRandomise)
	{
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].craftable && Inventory.inv.allItems[i].craftable.workPlaceConditions == CraftingManager.CraftingMenuType.CookingTable)
			{
				list.Add(Inventory.inv.allItems[i]);
			}
		}
		int index = Random.Range(0, list.Count - 1);
		int num = Random.Range(0, list[index].craftable.itemsInRecipe.Length);
		postToRandomise.requiredItem = list[index].craftable.itemsInRecipe[num].getItemId();
		postToRandomise.requireItemAmount = 1;
		postToRandomise.rewardId = list[index].getItemId();
		postToRandomise.rewardAmount = 1;
	}

	public void randomiseSmeltingCoditions(PostOnBoard postToRandomise)
	{
		List<InventoryItem> list = new List<InventoryItem>();
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].itemChange && Inventory.inv.allItems[i].itemChange.getChangerResultId(transformativeTileObject.tileObjectId) != -1)
			{
				list.Add(Inventory.inv.allItems[i]);
			}
		}
		InventoryItem inventoryItem = list[Random.Range(0, list.Count)];
		postToRandomise.requiredItem = inventoryItem.getItemId();
		postToRandomise.requireItemAmount = inventoryItem.itemChange.getAmountNeeded(transformativeTileObject.tileObjectId);
		postToRandomise.rewardId = Inventory.inv.allItems[inventoryItem.itemChange.getChangerResultId(transformativeTileObject.tileObjectId)].getItemId();
		postToRandomise.rewardAmount = Random.Range(2, 4);
	}

	public void randomiseCraftingConditions(PostOnBoard postToRandomise)
	{
		postToRandomise.requiredItem = itemsRequiredPool[Random.Range(0, itemsRequiredPool.Length)].getItemId();
		postToRandomise.requireItemAmount = Random.Range(1, 4);
		if ((bool)Inventory.inv.allItems[postToRandomise.requiredItem].craftable)
		{
			postToRandomise.requireItemAmount *= Inventory.inv.allItems[postToRandomise.requiredItem].craftable.recipeGiveThisAmount;
		}
		postToRandomise.rewardId = Inventory.inv.moneyItem.getItemId();
		postToRandomise.rewardAmount = (int)((float)Random.Range(3000, 6000) + (float)(Inventory.inv.allItems[postToRandomise.requiredItem].value * postToRandomise.requireItemAmount) * 1.5f);
	}

	public void randomiseShippingConditions(PostOnBoard postToRandomise)
	{
		postToRandomise.requiredItem = itemsRequiredPool[Random.Range(0, itemsRequiredPool.Length)].getItemId();
		postToRandomise.requireItemAmount = Random.Range(20, 31);
		postToRandomise.rewardId = Inventory.inv.moneyItem.getItemId();
		postToRandomise.rewardAmount = (int)((float)Random.Range(4000, 6000) + (float)(Inventory.inv.allItems[postToRandomise.requiredItem].value * postToRandomise.requireItemAmount) * 2f);
	}

	public void randomiseSateliteConditions(PostOnBoard postToRandomise)
	{
		bool flag = false;
		int num = 2000;
		int num2 = Random.Range(200, 800);
		int num3 = Random.Range(200, 800);
		while (!flag)
		{
			if (tileIsEmptyOrHasGrass(num2, num3) && tileIsEmptyOrHasGrass(num2 + 1, num3) && tileIsEmptyOrHasGrass(num2, num3 + 1) && tileIsEmptyOrHasGrass(num2 + 1, num3 + 1))
			{
				flag = true;
			}
			else
			{
				num2 = Random.Range(200, 800);
				num3 = Random.Range(200, 800);
				num--;
			}
			if (num <= 0)
			{
				Debug.LogError("No location found for satelite");
				break;
			}
		}
		if (flag)
		{
			Debug.Log("Spawned a satelite");
			NetworkMapSharer.share.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[9], new Vector3(num2 * 2 + 1, WorldManager.manageWorld.heightMap[num2, num3], num3 * 2 + 1));
		}
		postToRandomise.isInvestigation = true;
		postToRandomise.location = new int[3]
		{
			num2,
			WorldManager.manageWorld.heightMap[num2, num3],
			num3
		};
	}

	public bool tileIsEmptyOrHasGrass(int xPos, int yPos)
	{
		if (WorldManager.manageWorld.waterMap[xPos, yPos])
		{
			return false;
		}
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] == -1)
		{
			return true;
		}
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] >= 0 && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].isGrass)
		{
			return true;
		}
		return false;
	}
}
