using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatBox : MonoBehaviour
{
	public static ChatBox chat;

	public TMP_InputField chatBox;

	public GameObject enterChatTextWindow;

	public GameObject chatLogWindow;

	public bool chatOpen;

	private List<string> history = new List<string>();

	private int showingHistoryNo = -1;

	public bool chatLogOpen = true;

	public ASound chatSend;

	private uint lastPersonToTalk;

	private bool showingHud = true;

	public GameObject chatBubble;

	public Transform chatBubbleWindow;

	public UIScrollBar myScrollbar;

	public List<ChatBubble> chatLog;

	public float chatSpeed = 1f;

	public GameObject whistleButton;

	private bool commandsOn;

	private bool canOpen = true;

	private bool closeChatOnEmote;

	private void Awake()
	{
		chat = this;
		StartCoroutine(chatBubbleWindowScroll());
		if (PlayerPrefs.HasKey("DevCommandOn"))
		{
			if (PlayerPrefs.GetInt("DevCommandOn") == 1)
			{
				commandsOn = true;
			}
			else
			{
				commandsOn = false;
			}
		}
	}

	public void chatLogWindowToggle()
	{
		chatLogOpen = chatLogWindow.activeSelf;
	}

	private IEnumerator chatBubbleWindowScroll()
	{
		while (true)
		{
			float num = 0f;
			for (int num2 = chatLog.Count - 1; num2 > -1; num2--)
			{
				float b = num;
				num += chatLog[num2].getHeight() + 4f;
				float y = Mathf.Lerp(chatLog[num2].transform.localPosition.y, b, Time.deltaTime * 10f);
				chatLog[num2].transform.localPosition = new Vector3(chatLog[num2].transform.localPosition.x, y, chatLog[num2].transform.localPosition.z);
			}
			if (chatLog.Count <= 10)
			{
				chatSpeed = 1f;
			}
			else if (chatLog.Count <= 20)
			{
				chatSpeed = 2f;
			}
			else if (chatLog.Count < 30)
			{
				chatSpeed = 4f;
			}
			else if (chatLog.Count < 40)
			{
				chatSpeed = 6f;
			}
			else
			{
				chatSpeed = 8f;
			}
			yield return null;
		}
	}

	public void addToChatBox(EquipItemToChar charThatTalked, string message, bool specialMessage = false)
	{
		ChatBubble component = Object.Instantiate(chatBubble, chatBubbleWindow).GetComponent<ChatBubble>();
		component.fillBubble(charThatTalked.playerName, message);
		chatLog.Add(component);
		lastPersonToTalk = charThatTalked.netId;
	}

	private IEnumerator switchCanOpenTimer()
	{
		canOpen = false;
		yield return new WaitForSeconds(0.25f);
		canOpen = true;
	}

	private void Update()
	{
		if (!NetworkMapSharer.share || !NetworkMapSharer.share.localChar || Inventory.inv.menuOpen)
		{
			return;
		}
		if (chatOpen)
		{
			if (NetworkMapSharer.share.localChar.myPickUp.drivingVehicle)
			{
				whistleButton.SetActive(false);
			}
			else
			{
				whistleButton.SetActive(true);
			}
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				showingHistoryNo = Mathf.Clamp(showingHistoryNo - 1, 0, history.Count);
				if (showingHistoryNo == history.Count)
				{
					chatBox.text = "";
				}
				else
				{
					chatBox.text = history[showingHistoryNo];
				}
			}
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				showingHistoryNo = Mathf.Clamp(showingHistoryNo + 1, 0, history.Count);
				if (showingHistoryNo == history.Count)
				{
					chatBox.text = "";
				}
				else
				{
					chatBox.text = history[showingHistoryNo];
				}
			}
		}
		if (!chatOpen && !Inventory.inv.canMoveChar())
		{
			return;
		}
		if (InputMaster.input.OpenChat())
		{
			Inventory.inv.usingMouse = true;
		}
		if (!InputMaster.input.OpenChat() && (!chatOpen || !InputMaster.input.UICancel()) && (!chatOpen || !InputMaster.input.UISelectActiveConfirmButton()) && !closeChatOnEmote)
		{
			return;
		}
		closeChatOnEmote = false;
		StartCoroutine(switchCanOpenTimer());
		enterChatTextWindow.gameObject.SetActive(!chatOpen);
		chatOpen = !chatOpen;
		Inventory.inv.checkIfWindowIsNeeded();
		if (chatOpen)
		{
			MenuButtonsTop.menu.closed = false;
		}
		else
		{
			MenuButtonsTop.menu.closeButtonDelay();
		}
		if (!showingHud)
		{
			showingHud = !Inventory.inv.casters[0].GetComponent<Canvas>().enabled;
			GraphicRaycaster[] casters = Inventory.inv.casters;
			for (int i = 0; i < casters.Length; i++)
			{
				casters[i].GetComponent<Canvas>().enabled = showingHud;
			}
		}
		if (chatOpen)
		{
			chatBox.ActivateInputField();
			return;
		}
		if (chatBox.text != "")
		{
			history.Add(chatBox.text);
		}
		showingHistoryNo = history.Count;
		string[] array = chatBox.text.Split(' ');
		if (array[0] == "devCommandsOn")
		{
			PlayerPrefs.SetInt("DevCommandOn", 1);
			commandsOn = true;
		}
		else if (array[0] == "devCommandsOff")
		{
			PlayerPrefs.SetInt("DevCommandOn", 0);
			commandsOn = false;
		}
		else if (commandsOn && array[0] == "giveMilestone")
		{
			MilestoneManager.manage.doATaskAndCountToMilestone((DailyTaskGenerator.genericTaskType)Random.Range(1, MilestoneManager.manage.milestones.Count), 100);
		}
		else if (commandsOn && array[0] == "giveGift")
		{
			GiftedItemWindow.gifted.addToListToBeGiven(Inventory.inv.moneyItem.getItemId(), 1000);
			GiftedItemWindow.gifted.openWindowAndGiveItems();
		}
		else if (commandsOn && array[0] == "spawnBoat")
		{
			MarketPlaceManager.manage.spawnJimmysBoat();
		}
		else if (commandsOn && array[0] == "dropAllFurniture")
		{
			for (int j = 0; j < Inventory.inv.allItems.Length; j++)
			{
				if (Inventory.inv.allItems[j].isFurniture)
				{
					NetworkMapSharer.share.spawnAServerDrop(j, 1, CameraController.control.transform.position);
				}
			}
		}
		else if (commandsOn && array[0] == "nextDayChange")
		{
			WorldManager.manageWorld.doNextDayChange();
		}
		else if (commandsOn && array[0] == "renameIsland")
		{
			NetworkMapSharer.share.NetworkislandName = array[1];
			Inventory.inv.islandName = array[1];
		}
		else if (commandsOn && array[0] == "spawnCarry")
		{
			NetworkMapSharer.share.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[int.Parse(array[1])], NetworkMapSharer.share.localChar.transform.position);
		}
		else if (commandsOn && array[0] == "resetHouse")
		{
			if (NetworkMapSharer.share.localChar.myInteract.insideHouseDetails != null)
			{
				NetworkMapSharer.share.localChar.myInteract.insideHouseDetails.resetHouseMap();
			}
		}
		else if (commandsOn && array[0] == "resetHouseExteriors")
		{
			HouseManager.manage.clearHouseExteriors();
		}
		else if (commandsOn && array[0] == "refreshInside")
		{
			if (NetworkMapSharer.share.localChar.myInteract.insideHouseDetails != null)
			{
				NetworkMapSharer.share.localChar.myInteract.insideHouseDetails.upgradeHouseSize();
			}
		}
		else if (commandsOn && array[0] == "cropsGrowAllSeasons")
		{
			for (int k = 0; k < WorldManager.manageWorld.allObjects.Length; k++)
			{
				if ((bool)WorldManager.manageWorld.allObjects[k].tileObjectGrowthStages && WorldManager.manageWorld.allObjects[k].tileObjectGrowthStages.needsTilledSoil)
				{
					WorldManager.manageWorld.allObjects[k].tileObjectGrowthStages.growsInWinter = true;
					WorldManager.manageWorld.allObjects[k].tileObjectGrowthStages.growsInSummer = true;
					WorldManager.manageWorld.allObjects[k].tileObjectGrowthStages.growsInAutum = true;
					WorldManager.manageWorld.allObjects[k].tileObjectGrowthStages.growsInSpring = true;
				}
			}
		}
		else if (!commandsOn || !(array[0] == "compassLock"))
		{
			if (commandsOn && array[0] == "cheatsOn")
			{
				PlayerPrefs.SetInt("Cheats", 1);
				CheatScript.cheat.cheatsOn = true;
			}
			else if (commandsOn && array[0] == "npcPhoto")
			{
				CharacterCreatorScript.create.takeNPCPhoto(int.Parse(array[1]));
			}
			else if (commandsOn && array[0] == "setTired")
			{
				StatusManager.manage.changeStamina(-40f);
			}
			else if (commandsOn && array[0] == "givePoints")
			{
				PermitPointsManager.manage.addPoints(int.Parse(array[1]));
			}
			else if (commandsOn && array[0] == "/e")
			{
				NetworkMapSharer.share.localChar.CmdSendEmote(int.Parse(array[1]));
			}
			else if (commandsOn && array[0] == "skipSong")
			{
				MusicManager.manage.outsideMusic.time = MusicManager.manage.outsideMusic.clip.length - 30f;
			}
			else if (commandsOn && array[0] == "setAnimalRel")
			{
				foreach (FarmAnimalDetails farmAnimalDetail in FarmAnimalManager.manage.farmAnimalDetails)
				{
					farmAnimalDetail.animalRelationShip = int.Parse(array[1]);
				}
			}
			else if (commandsOn && array[0] == "fullPedia")
			{
				foreach (PediaEntry allEntry in PediaManager.manage.allEntries)
				{
					allEntry.amountCaught = 1;
				}
			}
			else if (commandsOn && array[0] == "placeItem")
			{
				NetworkMapSharer.share.RpcUpdateOnTileObject(int.Parse(array[1]), (int)NetworkMapSharer.share.localChar.myInteract.selectedTile.x, (int)NetworkMapSharer.share.localChar.myInteract.selectedTile.y);
			}
			else if (commandsOn && array[0] == "placeItemFix")
			{
				NetworkMapSharer.share.RpcUpdateOnTileObject(int.Parse(array[1]), Mathf.RoundToInt(NetworkMapSharer.share.localChar.myInteract.tileHighlighter.position.x / 2f), Mathf.RoundToInt(NetworkMapSharer.share.localChar.myInteract.tileHighlighter.position.z / 2f));
			}
			else if (commandsOn && array[0] == "setDate")
			{
				WorldManager.manageWorld.day = int.Parse(array[1]);
				WorldManager.manageWorld.week = int.Parse(array[2]);
				WorldManager.manageWorld.month = int.Parse(array[3]);
				WorldManager.manageWorld.year = int.Parse(array[4]);
			}
			else if (commandsOn && array[0] == "scanMap")
			{
				StartCoroutine(RenderMap.map.scanTheMap());
			}
			else if (commandsOn && array[0] == "hairStyle")
			{
				int result;
				if (int.TryParse(array[1], out result))
				{
					NetworkMapSharer.share.localChar.myEquip.CmdChangeHairId(result);
				}
			}
			else if (commandsOn && array[0] == "strikeLightning")
			{
				NetworkMapSharer.share.RpcThunderStrike(new Vector2(int.Parse(array[1]), int.Parse(array[2])));
			}
			else if (commandsOn && array[0] == "hairColour")
			{
				int result2;
				if (int.TryParse(array[1], out result2))
				{
					NetworkMapSharer.share.localChar.myEquip.CmdChangeHairColour(result2);
				}
			}
			else if (commandsOn && array[0] == "setStatus")
			{
				NetworkMapSharer.share.RpcGiveOnTileStatus(int.Parse(array[1]), Mathf.RoundToInt(NetworkMapSharer.share.localChar.myInteract.tileHighlighter.position.x / 2f), Mathf.RoundToInt(NetworkMapSharer.share.localChar.myInteract.tileHighlighter.position.z / 2f));
			}
			else if (commandsOn && array[0] == "skinTone")
			{
				int result3;
				if (int.TryParse(array[1], out result3))
				{
					NetworkMapSharer.share.localChar.myEquip.CmdChangeSkin(result3);
				}
			}
			else if (commandsOn && array[0] == "hideGuide")
			{
				NetworkMapSharer.share.localChar.myInteract.tileHighlighter.gameObject.SetActive(!NetworkMapSharer.share.localChar.myInteract.tileHighlighter.gameObject.activeSelf);
				TileHighlighter.highlight.off = !TileHighlighter.highlight.off;
			}
			else if (commandsOn && array[0] == "noClip")
			{
				CameraController.control.swapFlyCam();
			}
			else if (commandsOn && array[0] == "noClipNoFollow")
			{
				CameraController.control.swapFlyCam(false);
			}
			else if (commandsOn && array[0] == "saveFreeCam")
			{
				CameraController.control.saveFreeCam();
			}
			else if (commandsOn && array[0] == "loadFreeCam")
			{
				CameraController.control.loadFreeCam();
			}
			else if (commandsOn && array[0] == "clearFreeCam")
			{
				CameraController.control.clearFreeCam();
			}
			else if (commandsOn && array[0] == "hideHud")
			{
				showingHud = !Inventory.inv.casters[0].GetComponent<Canvas>().enabled;
				GraphicRaycaster[] casters = Inventory.inv.casters;
				for (int i = 0; i < casters.Length; i++)
				{
					casters[i].GetComponent<Canvas>().enabled = showingHud;
				}
			}
			else if (commandsOn && array[0] == "moveInNPC")
			{
				int result4;
				if (int.TryParse(array[1], out result4) && result4 < NPCManager.manage.npcStatus.Count)
				{
					NPCManager.manage.moveInNPC(result4);
					NPCManager.manage.npcStatus[result4].hasAskedToMoveIn = true;
				}
			}
			else if (!commandsOn || !(array[0] == "makeWindy"))
			{
				if (commandsOn && array[0] == "completeNPC")
				{
					int result5;
					if (int.TryParse(array[1], out result5) && result5 < NPCManager.manage.npcStatus.Count)
					{
						NPCManager.manage.npcStatus[result5].hasMet = true;
						NPCManager.manage.npcStatus[result5].relationshipLevel = NPCManager.manage.NPCDetails[result5].relationshipBeforeMove;
						NPCManager.manage.npcStatus[result5].moneySpentAtStore = NPCManager.manage.NPCDetails[result5].spendBeforeMoveIn;
					}
				}
				else if (commandsOn && array[0] == "maxRelation")
				{
					int result6;
					if (int.TryParse(array[1], out result6) && result6 < NPCManager.manage.npcStatus.Count)
					{
						NPCManager.manage.npcStatus[result6].hasMet = true;
						NPCManager.manage.npcStatus[result6].relationshipLevel = 100;
					}
				}
				else if (commandsOn && array[0] == "spawnFarmAnimal")
				{
					FarmAnimalManager.manage.spawnNewFarmAnimalWithDetails(int.Parse(array[1]), int.Parse(array[2]), array[3], NetworkMapSharer.share.localChar.transform.position);
				}
				else if (commandsOn && array[0] == "moveAllCarry")
				{
					WorldManager.manageWorld.moveAllCarriablesToSpawn();
				}
				else if (commandsOn && array[0] == "save")
				{
					StartCoroutine(SaveLoad.saveOrLoad.saveRoutine(NetworkMapSharer.share.isServer, true, false));
				}
				else if (commandsOn && array[0] == "crocDay")
				{
					AnimalManager.manage.crocDay = !AnimalManager.manage.crocDay;
				}
				else if (commandsOn && array[0] == "randomClothing")
				{
					StartCoroutine(EquipWindow.equip.randomClothes());
				}
				else if (commandsOn && array[0] == "randomiseCharacter")
				{
					EquipWindow.equip.randomiseCharacter();
				}
				else if (commandsOn && array[0] == "stopRandom")
				{
					EquipWindow.equip.keepChanging = false;
				}
				else if (commandsOn && array[0] == "nextDay")
				{
					if ((bool)NetworkMapSharer.share && NetworkMapSharer.share.isServer)
					{
						RealWorldTimeLight.time.NetworkcurrentHour = 7;
						WorldManager.manageWorld.nextDay();
					}
				}
				else if (commandsOn && array[0] == "chunkDistance")
				{
					NewChunkLoader.loader.setChunkDistance(int.Parse(array[1]));
				}
				else if (commandsOn && array[0] == "freeCam")
				{
					CameraController.control.swapFreeCam();
				}
				else if (commandsOn && array[0] == "spawnAnimal")
				{
					Vector3 position = NetworkMapSharer.share.localChar.transform.position;
					NetworkNavMesh.nav.SpawnAnAnimalOnTile(int.Parse(array[1]), (int)(position.x / 2f), (int)(position.z / 2f));
				}
				else if (chatBox.text == "debug")
				{
					bool flag = !base.transform.Find("Debug").gameObject.activeSelf;
					base.transform.Find("Debug").gameObject.SetActive(flag);
					if (flag)
					{
						NPCManager.manage.turnOnNpcDebugMarkers();
					}
					else
					{
						NPCManager.manage.removeDebugMarkers();
					}
				}
				else if (commandsOn && array[0] == "giveMoney")
				{
					Inventory.inv.changeWallet(int.Parse(array[1]));
				}
				else if (commandsOn && array[0] == "teleport")
				{
					NetworkMapSharer.share.localChar.transform.position = new Vector3(int.Parse(array[1]), 10f, int.Parse(array[2]));
					CameraController.control.transform.position = NetworkMapSharer.share.localChar.transform.position;
					NewChunkLoader.loader.forceInstantUpdateAtPos();
				}
				else if (commandsOn && array[0] == "changeSpeed")
				{
					RealWorldTimeLight.time.changeSpeed(float.Parse(array[1]));
				}
				else if (commandsOn && array[0] == "setTime")
				{
					RealWorldTimeLight.time.useTime = false;
					RealWorldTimeLight.time.NetworkcurrentHour = int.Parse(array[1]);
				}
				else if (commandsOn && array[0] == "spawnNpc")
				{
					if (int.Parse(array[1]) >= 0 && int.Parse(array[1]) < NPCManager.manage.NPCDetails.Length)
					{
						NetworkNavMesh.nav.SpawnAnNPCAtPosition(int.Parse(array[1]), NetworkMapSharer.share.localChar.transform.position);
					}
				}
				else if (commandsOn && chatBox.text == "completeQuests")
				{
					for (int l = 0; l < QuestManager.manage.allQuests.Length; l++)
					{
						QuestManager.manage.isQuestAccepted[l] = true;
						QuestManager.manage.isQuestCompleted[l] = true;
						for (int m = 0; m < QuestManager.manage.allQuests[l].unlockRecipeOnComplete.Length; m++)
						{
							GiftedItemWindow.gifted.addRecipeToUnlock(Inventory.inv.getInvItemId(QuestManager.manage.allQuests[l].unlockRecipeOnComplete[m]));
						}
					}
					GiftedItemWindow.gifted.openWindowAndGiveItems();
				}
				else if (commandsOn && chatBox.text == "unlockRecipes")
				{
					for (int n = 0; n < Inventory.inv.allItems.Length; n++)
					{
						if ((bool)Inventory.inv.allItems[n].craftable)
						{
							CharLevelManager.manage.unlockRecipe(Inventory.inv.allItems[n]);
						}
					}
				}
				else if (!commandsOn || !(chatBox.text == "changeRain"))
				{
					if (commandsOn && chatBox.text == "setTimeDay")
					{
						RealWorldTimeLight.time.useTime = false;
						RealWorldTimeLight.time.NetworkcurrentHour = 10;
					}
					else if (commandsOn && chatBox.text == "setTimeNight")
					{
						RealWorldTimeLight.time.useTime = false;
						RealWorldTimeLight.time.NetworkcurrentHour = 19;
					}
					else if (commandsOn && chatBox.text == "setTimeReal")
					{
						RealWorldTimeLight.time.useTime = true;
					}
					else if (chatBox.text != "")
					{
						NetworkMapSharer.share.localChar.CmdSendChatMessage(chatBox.text);
					}
				}
			}
		}
		chatBox.text = "";
	}

	public void sendEmote(int emoteNo)
	{
		NetworkMapSharer.share.localChar.CmdSendEmote(emoteNo);
		closeChatOnEmote = true;
	}

	public void whistle()
	{
		if (!NetworkMapSharer.share.localChar.myEquip.isWhistling())
		{
			NetworkMapSharer.share.localChar.myEquip.CharWhistles();
			closeChatOnEmote = true;
		}
	}
}
