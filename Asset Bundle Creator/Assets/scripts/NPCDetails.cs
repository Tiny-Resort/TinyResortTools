using UnityEngine;

public class NPCDetails : MonoBehaviour
{
	[Header("Details--------")]
	public string NPCName;

	public GameObject NpcHair;

	public InventoryItem NPCShirt;

	public InventoryItem NPCPants;

	public InventoryItem NPCShoes;

	public Material NpcSkin;

	public Material NpcEyes;

	public Material NpcEyesColor;

	public Material NPCMouth;

	public int nose;

	public Mesh insideMouthMesh;

	public Mesh npcMesh;

	[Header("Conversations--------")]
	public Conversation introductionConversation;

	public Conversation[] randomChats;

	public Conversation moveInRequestConversation;

	public InventoryItem deedOnMoveRequest;

	[Header("Schedual Details--------")]
	public NPCSchedual mySchedual;

	public NPCSchedual.Locations workLocation;

	public Conversation keeperConversation;

	public ASound NPCVoice;

	public ASound NPCLaugh;

	[Header("Move in requirements--------")]
	public int spendBeforeMoveIn = 5000;

	public int relationshipBeforeMove = 15;

	public Color npcColor = Color.blue;

	public Sprite npcSprite;

	[Header("Likes and Dislikes ------")]
	public InventoryItem favouriteFood;

	public InventoryItem hatedFood;

	public bool hatesAnimalProducts;

	public bool hatesMeat;

	public bool hatesFruits;

	public bool hatesVegitables;

	[Header("Animations --------")]
	public AnimatorOverrideController animationOverrride;

	[Header("Villager Details --------")]
	public bool isAVillager;

	public string[] randomNames;

	[Header("Gossip and town mentions --------")]
	public Conversation[] gossip;

	public Conversation[] townMentions;

	[Header("Conversations --------")]
	public Conversation[] friendship0;

	public Conversation[] friendship25;

	public Conversation[] friendship50;

	public Conversation[] friendship75;

	[Header("Greetings --------")]
	public Conversation[] lowFriendshipGreetings;

	public Conversation[] mediumFriendshipGreetings;

	public Conversation[] highFriendshipFriendshipGreetings;

	public Conversation[] highestFriendshipFriendshipGreetings;

	[Header("Time Greetings --------")]
	public Conversation[] morningGreetings;

	public Conversation[] noonGreetings;

	public Conversation[] arvoGreetings;

	public Conversation[] nightGreetings;

	[Header("Weather Greetings --------")]
	public Conversation[] coldWeatherGreetings;

	public Conversation[] hotWeatherGreetings;

	public Conversation[] rainingWeatherGreetings;

	public Conversation[] stormingGreetings;

	public Conversation[] windyGreetings;

	public NPCRequestConversations requestText;

	public void getName(int nameId, bool feminine)
	{
		NPCName = randomNames[nameId];
		if (feminine)
		{
			NPCVoice = SoundManager.manage.highVoice;
		}
		else
		{
			NPCVoice = SoundManager.manage.medVoice;
		}
	}

	public bool hasRandomConvoAvaliable()
	{
		if (friendship0.Length != 0 || randomChats.Length != 0)
		{
			return true;
		}
		return false;
	}

	public Conversation getRandomGreeting(int NPCId)
	{
		if (lowFriendshipGreetings.Length == 0 && mediumFriendshipGreetings.Length == 0 && highFriendshipFriendshipGreetings.Length == 0 && highestFriendshipFriendshipGreetings.Length == 0)
		{
			return null;
		}
		if (NPCManager.manage.npcStatus[NPCId].relationshipLevel < 25)
		{
			return lowFriendshipGreetings[Random.Range(0, lowFriendshipGreetings.Length)];
		}
		if (NPCManager.manage.npcStatus[NPCId].relationshipLevel < 50)
		{
			if (Random.Range(0, 2) == 0)
			{
				return lowFriendshipGreetings[Random.Range(0, lowFriendshipGreetings.Length)];
			}
			return mediumFriendshipGreetings[Random.Range(0, mediumFriendshipGreetings.Length)];
		}
		if (NPCManager.manage.npcStatus[NPCId].relationshipLevel < 75)
		{
			switch (Random.Range(0, 3))
			{
			case 0:
				return lowFriendshipGreetings[Random.Range(0, lowFriendshipGreetings.Length)];
			case 1:
				return mediumFriendshipGreetings[Random.Range(0, mediumFriendshipGreetings.Length)];
			default:
				return highFriendshipFriendshipGreetings[Random.Range(0, highFriendshipFriendshipGreetings.Length)];
			}
		}
		switch (Random.Range(0, 4))
		{
		case 0:
			return lowFriendshipGreetings[Random.Range(0, lowFriendshipGreetings.Length)];
		case 1:
			return mediumFriendshipGreetings[Random.Range(0, mediumFriendshipGreetings.Length)];
		case 2:
			return highFriendshipFriendshipGreetings[Random.Range(0, highFriendshipFriendshipGreetings.Length)];
		default:
			return highestFriendshipFriendshipGreetings[Random.Range(0, highestFriendshipFriendshipGreetings.Length)];
		}
	}

	public Conversation getRandomChat(int NPCId)
	{
		if (friendship0.Length != 0)
		{
			if (NPCManager.manage.npcStatus[NPCId].relationshipLevel < 25)
			{
				int randomNumberNotUsedYesterday = getRandomNumberNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), 0, friendship0.Length);
				NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomNumberNotUsedYesterday);
				return friendship0[randomNumberNotUsedYesterday];
			}
			if (NPCManager.manage.npcStatus[NPCId].relationshipLevel < 50)
			{
				if (Random.Range(0, 2) == 0)
				{
					int randomNumberNotUsedYesterday2 = getRandomNumberNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), 0, friendship0.Length);
					NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomNumberNotUsedYesterday2);
					return friendship0[randomNumberNotUsedYesterday2];
				}
				int randomNumberNotUsedYesterday3 = getRandomNumberNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), friendship0.Length, friendship25.Length + friendship0.Length);
				NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomNumberNotUsedYesterday3);
				return friendship25[randomNumberNotUsedYesterday3 - friendship0.Length];
			}
			if (NPCManager.manage.npcStatus[NPCId].relationshipLevel < 75)
			{
				switch (Random.Range(0, 3))
				{
				case 0:
				{
					int randomNumberNotUsedYesterday6 = getRandomNumberNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), 0, friendship0.Length);
					NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomNumberNotUsedYesterday6);
					return friendship0[randomNumberNotUsedYesterday6];
				}
				case 1:
				{
					int randomNumberNotUsedYesterday5 = getRandomNumberNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), friendship0.Length, friendship25.Length + friendship0.Length);
					NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomNumberNotUsedYesterday5);
					return friendship25[randomNumberNotUsedYesterday5 - friendship0.Length];
				}
				default:
				{
					int randomNumberNotUsedYesterday4 = getRandomNumberNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), friendship0.Length + friendship25.Length, friendship50.Length + (friendship0.Length + friendship25.Length));
					NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomNumberNotUsedYesterday4);
					return friendship50[randomNumberNotUsedYesterday4 - (friendship0.Length + friendship25.Length)];
				}
				}
			}
			switch (Random.Range(0, 4))
			{
			case 0:
			{
				int randomNumberNotUsedYesterday10 = getRandomNumberNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), 0, friendship0.Length);
				NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomNumberNotUsedYesterday10);
				return friendship0[randomNumberNotUsedYesterday10];
			}
			case 1:
			{
				int randomNumberNotUsedYesterday9 = getRandomNumberNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), friendship0.Length, friendship25.Length + friendship0.Length);
				NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomNumberNotUsedYesterday9);
				return friendship25[randomNumberNotUsedYesterday9 - friendship0.Length];
			}
			case 2:
			{
				int randomNumberNotUsedYesterday8 = getRandomNumberNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), friendship0.Length + friendship25.Length, friendship50.Length + (friendship0.Length + friendship25.Length));
				NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomNumberNotUsedYesterday8);
				return friendship50[randomNumberNotUsedYesterday8 - (friendship0.Length + friendship25.Length)];
			}
			default:
			{
				int randomNumberNotUsedYesterday7 = getRandomNumberNotUsedYesterday(NPCManager.manage.npcStatus[NPCId].getLastTextSaid(), friendship0.Length + friendship25.Length + friendship50.Length, friendship75.Length + (friendship0.Length + friendship25.Length + friendship50.Length));
				NPCManager.manage.npcStatus[NPCId].addLastTextSaidToList(randomNumberNotUsedYesterday7);
				return friendship75[randomNumberNotUsedYesterday7 - (friendship0.Length + friendship25.Length + friendship50.Length)];
			}
			}
		}
		if (randomChats.Length != 0)
		{
			return randomChats[Random.Range(0, randomChats.Length)];
		}
		return null;
	}

	private int getRandomNumberNotUsedYesterday(int[] lastused, int min, int max)
	{
		int num = Random.Range(min, max);
		for (int num2 = 2500; num2 > 0; num2--)
		{
			num = Random.Range(min, max);
			if (lastused[0] != num && lastused[1] != num && lastused[2] != num)
			{
				break;
			}
		}
		return num;
	}

	public Sprite getNPCSprite(int npcNo)
	{
		if ((bool)NPCManager.manage.NPCDetails[npcNo].npcSprite)
		{
			return NPCManager.manage.NPCDetails[npcNo].npcSprite;
		}
		MonoBehaviour.print("Creating new npc image");
		Sprite sprite = CharacterCreatorScript.create.loadNPCPhoto(npcNo);
		if ((bool)sprite)
		{
			NPCManager.manage.NPCDetails[npcNo].npcSprite = sprite;
			return NPCManager.manage.NPCDetails[npcNo].npcSprite;
		}
		CharacterCreatorScript.create.takeNPCPhoto(npcNo);
		return null;
	}
}
