using UnityEngine;

public class ConversationGenerator : MonoBehaviour
{
	public static ConversationGenerator generate;

	[Header("Grettings -------")]
	public Conversation[] morningGreetings;

	public Conversation[] middayGreetings;

	public Conversation[] afternoonGreetings;

	public Conversation[] nightTimeGreetings;

	public Conversation[] rainingGreetings;

	public Conversation[] stormingConversation;

	public Conversation[] windyConversation;

	public Conversation[] foggyGreetings;

	public Conversation[] hotGreetings;

	public Conversation[] coldGreetings;

	public Conversation[] randomGreetings;

	[Header("Request Conversations")]
	public Conversation[] RequestSpecificFishOrBugRequest;

	public Conversation[] RequestSomethingToEatConvo;

	public Conversation[] RequestSomeLoggingItems;

	public Conversation[] RequestMiningItems;

	public Conversation[] RequestSellingItem;

	public Conversation[] RequestSellingItemNotEnoughMoney;

	public Conversation[] RequestNewClothes;

	public Conversation[] RequestNewFurniture;

	public Conversation[] RequestItemFromInv;

	public Conversation[] RequestItemAcceptConversations;

	public Conversation[] NoRequestConversations;

	[Header("Dislike and Hate Conversations")]
	public Conversation[] GivenDislikeFood;

	public Conversation[] GivenHatedFood;

	public Conversation[] GivenFavouriteFood;

	[Header("Request Conversations")]
	public Conversation[] askToHangOutNotGoodEnoughFriends;

	public Conversation[] askToHangOutNoDayOff;

	public Conversation[] askToHangOutAccept;

	public Conversation[] alreadHangingOutWithSomeone;

	[Header("Character Complement/ Observations")]
	public Conversation[] commentOnUndies;

	public Conversation[] commentOnClothing;

	[Header("Character Complement/ Observations")]
	public Conversation[] commentOnStamina;

	public Conversation[] commentOnHealth;

	public Conversation[] commentOnFire;

	public Conversation[] commentOnPoisoned;

	public Conversation[] commentOnAnimal;

	public Conversation[] commentOnActivities;

	public Conversation[] commentOnCrops;

	public Conversation[] commentOnVisitorInTent;

	[Header("Character Complement/ Observations")]
	public Conversation[] randomConvos;

	private void Awake()
	{
		generate = this;
	}

	private void Start()
	{
		translateTextBlock(RequestSpecificFishOrBugRequest);
		translateTextBlock(RequestSomethingToEatConvo);
		translateTextBlock(RequestSomeLoggingItems);
		translateTextBlock(RequestMiningItems);
		translateTextBlock(RequestSellingItem);
		translateTextBlock(RequestSellingItemNotEnoughMoney);
		translateTextBlock(RequestNewClothes);
		translateTextBlock(RequestNewFurniture);
		translateTextBlock(RequestItemFromInv);
		translateTextBlock(RequestItemAcceptConversations);
		translateTextBlock(NoRequestConversations);
		translateTextBlock(GivenDislikeFood);
		translateTextBlock(GivenHatedFood);
		translateTextBlock(GivenFavouriteFood);
		translateTextBlock(askToHangOutNotGoodEnoughFriends);
		translateTextBlock(askToHangOutNoDayOff);
		translateTextBlock(askToHangOutAccept);
		translateTextBlock(alreadHangingOutWithSomeone);
		translateTextBlock(commentOnUndies);
		translateTextBlock(commentOnClothing);
		translateTextBlock(commentOnStamina);
		translateTextBlock(commentOnHealth);
		translateTextBlock(commentOnFire);
		translateTextBlock(commentOnPoisoned);
		translateTextBlock(commentOnAnimal);
		translateTextBlock(commentOnActivities);
		translateTextBlock(commentOnCrops);
		translateTextBlock(commentOnVisitorInTent);
		translateTextBlock(randomConvos);
	}

	public void translateTextBlock(Conversation[] toTranslate)
	{
		for (int i = 0; i < toTranslate.Length; i++)
		{
			if (toTranslate[i].saidBy != 0)
			{
				toTranslate[i].fillConvoTranslations();
			}
		}
	}

	public Conversation getGreeting(int npcId)
	{
		if (WeatherManager.manage.storming && Random.Range(0, 3) == 2)
		{
			if (NPCManager.manage.NPCDetails[npcId].stormingGreetings.Length != 0)
			{
				return returnConversationFromGroup(NPCManager.manage.NPCDetails[npcId].stormingGreetings);
			}
			return returnConversationFromGroup(stormingConversation);
		}
		if (WeatherManager.manage.raining && Random.Range(0, 3) == 2)
		{
			if (NPCManager.manage.NPCDetails[npcId].rainingWeatherGreetings.Length != 0)
			{
				return returnConversationFromGroup(NPCManager.manage.NPCDetails[npcId].rainingWeatherGreetings);
			}
			return returnConversationFromGroup(rainingGreetings);
		}
		if (!WeatherManager.manage.storming && WeatherManager.manage.windy && RealWorldTimeLight.time.currentHour < 18 && Random.Range(0, 3) == 2)
		{
			if (NPCManager.manage.NPCDetails[npcId].windyGreetings.Length != 0)
			{
				return returnConversationFromGroup(NPCManager.manage.NPCDetails[npcId].windyGreetings);
			}
			return returnConversationFromGroup(windyConversation);
		}
		if (RealWorldTimeLight.time.currentHour != 0 && RealWorldTimeLight.time.currentHour < 9)
		{
			if (Random.Range(0, 2) == 1)
			{
				if (NPCManager.manage.NPCDetails[npcId].morningGreetings.Length != 0)
				{
					return returnConversationFromGroup(NPCManager.manage.NPCDetails[npcId].morningGreetings);
				}
				return returnConversationFromGroup(morningGreetings);
			}
		}
		else if (RealWorldTimeLight.time.currentHour != 0 && RealWorldTimeLight.time.currentHour < 15)
		{
			if (Random.Range(0, 2) == 1)
			{
				if (NPCManager.manage.NPCDetails[npcId].noonGreetings.Length != 0)
				{
					return returnConversationFromGroup(NPCManager.manage.NPCDetails[npcId].noonGreetings);
				}
				return returnConversationFromGroup(middayGreetings);
			}
		}
		else if (RealWorldTimeLight.time.currentHour != 0 && RealWorldTimeLight.time.currentHour <= 17)
		{
			if (Random.Range(0, 2) == 1)
			{
				if (NPCManager.manage.NPCDetails[npcId].arvoGreetings.Length != 0)
				{
					return returnConversationFromGroup(NPCManager.manage.NPCDetails[npcId].arvoGreetings);
				}
				return returnConversationFromGroup(afternoonGreetings);
			}
		}
		else if ((RealWorldTimeLight.time.currentHour == 0 || RealWorldTimeLight.time.currentHour > 17) && Random.Range(0, 2) == 1)
		{
			if (NPCManager.manage.NPCDetails[npcId].nightGreetings.Length != 0)
			{
				return returnConversationFromGroup(NPCManager.manage.NPCDetails[npcId].nightGreetings);
			}
			return returnConversationFromGroup(nightTimeGreetings);
		}
		if ((float)GenerateMap.generate.getPlaceTemperature(CameraController.control.transform.position) < 10f && Random.Range(0, 2) == 1)
		{
			if (NPCManager.manage.NPCDetails[npcId].coldWeatherGreetings.Length != 0)
			{
				return returnConversationFromGroup(NPCManager.manage.NPCDetails[npcId].coldWeatherGreetings);
			}
			return returnConversationFromGroup(coldGreetings);
		}
		if ((float)GenerateMap.generate.getPlaceTemperature(CameraController.control.transform.position) > 30f && Random.Range(0, 2) == 1)
		{
			if (NPCManager.manage.NPCDetails[npcId].hotWeatherGreetings.Length != 0)
			{
				return returnConversationFromGroup(NPCManager.manage.NPCDetails[npcId].hotWeatherGreetings);
			}
			return returnConversationFromGroup(hotGreetings);
		}
		Conversation randomGreeting = NPCManager.manage.NPCDetails[npcId].getRandomGreeting(npcId);
		if (randomGreeting != null)
		{
			return randomGreeting;
		}
		return randomGreetings[Random.Range(0, randomGreetings.Length)];
	}

	public Conversation getRequestConversation(int NPCNo)
	{
		if (NPCManager.manage.NPCRequests[NPCNo].wantToSellSomething())
		{
			if (Inventory.inv.wallet >= NPCManager.manage.NPCRequests[NPCNo].getSellPrice())
			{
				return returnConversationFromGroup(RequestSellingItem);
			}
			return returnConversationFromGroup(RequestSellingItemNotEnoughMoney);
		}
		if (NPCManager.manage.NPCRequests[NPCNo].wantsAnItemInYourInv())
		{
			return returnConversationFromGroup(RequestItemFromInv);
		}
		if (NPCManager.manage.NPCRequests[NPCNo].wantsCertainFishOrBug())
		{
			return returnConversationFromGroup(RequestSpecificFishOrBugRequest);
		}
		if (NPCManager.manage.NPCRequests[NPCNo].wantsSomethingToEat())
		{
			return returnConversationFromGroup(RequestSomethingToEatConvo);
		}
		if (NPCManager.manage.NPCRequests[NPCNo].wantsSomeFurniture())
		{
			return returnConversationFromGroup(RequestNewFurniture);
		}
		if (NPCManager.manage.NPCRequests[NPCNo].wantsSomeClothing())
		{
			return returnConversationFromGroup(RequestNewClothes);
		}
		if (NPCManager.manage.NPCRequests[NPCNo].wantsSomeLogging())
		{
			return returnConversationFromGroup(RequestSomeLoggingItems);
		}
		if (NPCManager.manage.NPCRequests[NPCNo].wantsSomeMining())
		{
			return returnConversationFromGroup(RequestMiningItems);
		}
		return returnConversationFromGroup(NoRequestConversations);
	}

	public Conversation getRequestItemAcceptedConversation()
	{
		return returnConversationFromGroup(RequestItemAcceptConversations);
	}

	public Conversation askToHangOutConversation(int NPCNo)
	{
		if (NetworkMapSharer.share.localChar.followedBy != -1)
		{
			return returnConversationFromGroup(alreadHangingOutWithSomeone);
		}
		if (NPCManager.manage.npcStatus[NPCNo].relationshipLevel < 45)
		{
			return returnConversationFromGroup(askToHangOutNotGoodEnoughFriends);
		}
		if (NPCManager.manage.NPCDetails[NPCNo].mySchedual.dayOff[WorldManager.manageWorld.day - 1])
		{
			return returnConversationFromGroup(askToHangOutAccept);
		}
		return returnConversationFromGroup(askToHangOutNoDayOff);
	}

	public Conversation checkCurrentStatusOnTalk()
	{
		if (StatusManager.manage.connectedDamge.onFire)
		{
			return returnConversationFromGroup(commentOnFire);
		}
		return null;
	}

	public Conversation getRandomComment()
	{
		if (MarketPlaceManager.manage.someoneVisiting() && NPCManager.manage.npcStatus[ConversationManager.manage.lastTalkTo.myId.NPCNo].checkIfHasMovedIn() && Random.Range(0, 9) == 3)
		{
			return returnConversationFromGroup(commentOnVisitorInTent);
		}
		if (!StatusManager.manage.staminaAboveNo(30f) && Random.Range(0, 5) == 3 && Random.Range(0, 2) == 1)
		{
			return returnConversationFromGroup(commentOnStamina);
		}
		if (StatusManager.manage.connectedDamge.health < 30 && Random.Range(0, 5) == 3 && Random.Range(0, 2) == 1)
		{
			return returnConversationFromGroup(commentOnHealth);
		}
		if (Random.Range(0, 6) == 3)
		{
			if (checkIfWearingNothing())
			{
				return returnConversationFromGroup(commentOnUndies);
			}
			return returnConversationFromGroup(commentOnClothing);
		}
		return returnConversationFromGroup(randomConvos);
	}

	public Conversation returnConversationFromGroup(Conversation[] group)
	{
		return group[Random.Range(0, group.Length)];
	}

	public bool checkIfWearingNothing()
	{
		if (NetworkMapSharer.share.localChar.myEquip.shirtId == -1 && NetworkMapSharer.share.localChar.myEquip.pantsId == -1)
		{
			return true;
		}
		return false;
	}

	public string getRandomClothingName()
	{
		bool flag = false;
		while (!flag)
		{
			if (Random.Range(0, 3) == 2 && NetworkMapSharer.share.localChar.myEquip.shirtId != -1)
			{
				return Inventory.inv.allItems[NetworkMapSharer.share.localChar.myEquip.shirtId].getInvItemName();
			}
			if (Random.Range(0, 3) == 2 && NetworkMapSharer.share.localChar.myEquip.pantsId != -1)
			{
				return Inventory.inv.allItems[NetworkMapSharer.share.localChar.myEquip.pantsId].getInvItemName();
			}
			if (Random.Range(0, 3) == 2 && NetworkMapSharer.share.localChar.myEquip.shoeId != -1)
			{
				return Inventory.inv.allItems[NetworkMapSharer.share.localChar.myEquip.shoeId].getInvItemName();
			}
			if (Random.Range(0, 3) == 2 && NetworkMapSharer.share.localChar.myEquip.faceId != -1)
			{
				return Inventory.inv.allItems[NetworkMapSharer.share.localChar.myEquip.faceId].getInvItemName();
			}
			if (Random.Range(0, 3) == 2 && NetworkMapSharer.share.localChar.myEquip.headId != -1)
			{
				return Inventory.inv.allItems[NetworkMapSharer.share.localChar.myEquip.headId].getInvItemName();
			}
		}
		return "";
	}

	public string returnFarmAnimalType()
	{
		return "Animal Type";
	}

	public string returnFarmAnimalName()
	{
		return "Animal Name";
	}
}
