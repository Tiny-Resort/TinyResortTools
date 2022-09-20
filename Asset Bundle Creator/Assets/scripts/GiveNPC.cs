using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiveNPC : MonoBehaviour
{
	public enum currentlyGivingTo
	{
		Give = 0,
		Sell = 1,
		SellToTrapper = 2,
		Museum = 3,
		Tech = 4,
		Swapping = 5,
		BulletinBoard = 6,
		Build = 7,
		SellToJimmy = 8
	}

	public static GiveNPC give;

	public Transform GiveNPCWindow;

	public Transform GiveNPCSlotWindow;

	public GameObject constructionBoxNeededItems;

	public OptionAmount optionAmountWindow;

	[Header("Regular shop convos---------")]
	public Conversation wantToBuySomething;

	[Header("Regular shop fails---------")]
	public Conversation notEnoughMoneyToBuySomething;

	public Conversation hasEnoughMoneyToBuySomething;

	public Conversation notEnoughSpaceInInvToBuySomething;

	[Header("Selling items convos---------")]
	public Conversation nothingToGive;

	public Conversation somethingToGive;

	public Conversation totalEqualsZero;

	[Header("Selling items to Trapper convos---------")]
	public Conversation nothingToGiveTrapper;

	public Conversation somethingToGiveTrapper;

	public Conversation totalEqualsZeroTrapper;

	[Header("Selling items to Jimmy convos---------")]
	public Conversation nothingToGiveJimmy;

	public Conversation somethingToGiveJimmy;

	public Conversation totalEqualsZeroJimmy;

	[Header("Want to buy Recipes ---------")]
	public Conversation wantToBuyRecipe;

	public Conversation alreadyHaveRecipe;

	[Header("Animal buying convos---------")]
	public Conversation wantToBuyAnimal;

	public Conversation wantToBuyAnimalNotEnoughMoney;

	public Conversation wantToBuyAnimalNoHouses;

	public Conversation cancleAnimalBuying;

	public Conversation onBuyAnimalConvo;

	public Conversation buyAnimalNotLocal;

	public Conversation animalAlreadyInShop;

	[Header("Donate Tech Items")]
	public Conversation noTechToGive;

	public Conversation donateTechItemsConvo;

	[Header("Sell Items by weight----- ")]
	public Conversation giveItemToSellByWeight;

	[Header("Need Licence Convo---------")]
	public Conversation dontHaveLicence;

	public ShopBuyDrop dropToBuy;

	public Conversation noItemToGive;

	public Conversation[] GivenItemIncorrect;

	public Conversation[] GivenCorrectItem;

	public FillRecipeSlot[] requiredItemSlots;

	public bool giveWindowOpen;

	public int moneyOffer;

	public Image giveButton;

	public TextMeshProUGUI giveText;

	public Color giveColour;

	public Color cancleColour;

	private SellByWeight sellingByWeight;

	public PostOnBoard givingPost;

	private int buildingX;

	private int buildingY;

	public currentlyGivingTo giveMenuTypeOpen;

	public bool optionWindowOpen;

	private int storedReward = 1000;

	private void Awake()
	{
		give = this;
	}

	private void Start()
	{
		GiveNPCWindow.gameObject.SetActive(false);
	}

	private IEnumerator UpdateMenu()
	{
		while (giveWindowOpen)
		{
			yield return null;
			if (giveMenuTypeOpen == currentlyGivingTo.Museum || giveMenuTypeOpen == currentlyGivingTo.Sell || giveMenuTypeOpen == currentlyGivingTo.SellToTrapper || giveMenuTypeOpen == currentlyGivingTo.SellToJimmy || giveMenuTypeOpen == currentlyGivingTo.Build || giveMenuTypeOpen == currentlyGivingTo.Tech)
			{
				if (getDollarValueOfGiveSlots())
				{
					giveButton.color = giveColour;
					if (giveMenuTypeOpen == currentlyGivingTo.Build)
					{
						giveText.text = "Place";
					}
					else if (giveMenuTypeOpen == currentlyGivingTo.Museum || giveMenuTypeOpen == currentlyGivingTo.Tech)
					{
						giveText.text = "Donate";
					}
					else
					{
						giveText.text = "Sell";
					}
				}
				else
				{
					giveButton.color = cancleColour;
					giveText.text = "Cancel";
				}
				continue;
			}
			if (giveMenuTypeOpen == currentlyGivingTo.Swapping)
			{
				if (getDollarValueOfGiveSlots())
				{
					giveText.text = "Swap";
					giveButton.color = giveColour;
				}
				else
				{
					giveButton.color = cancleColour;
					giveText.text = "Cancel";
				}
				continue;
			}
			int num = 0;
			for (int i = 0; i < Inventory.inv.invSlots.Length; i++)
			{
				if (Inventory.inv.invSlots[i].isSelectedForGive())
				{
					num++;
				}
			}
			if (num != 0)
			{
				giveButton.color = giveColour;
				giveText.text = "Give";
			}
			else
			{
				giveButton.color = cancleColour;
				giveText.text = "Cancel";
			}
		}
	}

	public void openBuildingGiveMenu(int xPos, int yPos)
	{
		buildingX = xPos;
		buildingY = yPos;
		if (!NetworkMapSharer.share.isServer)
		{
			NetworkMapSharer.share.localChar.CmdGetDeedIngredients(buildingX, buildingY);
		}
		else
		{
			OpenGiveWindow(currentlyGivingTo.Build);
		}
	}

	public void updateDeedGive(int deedX, int deedY)
	{
		if (!giveWindowOpen || giveMenuTypeOpen != currentlyGivingTo.Build || deedX != buildingX || deedY != buildingY)
		{
			return;
		}
		int[] requiredItemsForDeed = DeedManager.manage.getRequiredItemsForDeed(buildingX, buildingY);
		int[] itemsAlreadyGivenForDeed = DeedManager.manage.getItemsAlreadyGivenForDeed(buildingX, buildingY);
		int[] requiredAmountForDeed = DeedManager.manage.getRequiredAmountForDeed(buildingX, buildingY);
		for (int i = 0; i < requiredItemSlots.Length; i++)
		{
			if (i < requiredItemsForDeed.Length)
			{
				requiredItemSlots[i].gameObject.SetActive(true);
				requiredItemSlots[i].fillRecipeSlotWithAmounts(requiredItemsForDeed[i], itemsAlreadyGivenForDeed[i], requiredAmountForDeed[i]);
				if (itemsAlreadyGivenForDeed[i] >= requiredAmountForDeed[i])
				{
					requiredItemSlots[i].setSlotFull();
				}
				else
				{
					requiredItemSlots[i].setSlotEmpty();
				}
			}
			else
			{
				requiredItemSlots[i].gameObject.SetActive(false);
			}
		}
	}

	public void OpenGiveWindow(currentlyGivingTo giveMenuType = currentlyGivingTo.Give)
	{
		givingPost = BulletinBoard.board.checkMissionsCompletedForNPC(ConversationManager.manage.lastTalkTo.GetComponent<NPCIdentity>().NPCNo);
		giveMenuTypeOpen = giveMenuType;
		blackOutNonGiveableObjects();
		constructionBoxNeededItems.gameObject.SetActive(false);
		if (giveMenuType != currentlyGivingTo.Tech && giveMenuType != currentlyGivingTo.Swapping && giveMenuType != currentlyGivingTo.Sell && giveMenuType != currentlyGivingTo.SellToTrapper && giveMenuTypeOpen != currentlyGivingTo.SellToJimmy && giveMenuType == currentlyGivingTo.Build)
		{
			int[] requiredItemsForDeed = DeedManager.manage.getRequiredItemsForDeed(buildingX, buildingY);
			int[] itemsAlreadyGivenForDeed = DeedManager.manage.getItemsAlreadyGivenForDeed(buildingX, buildingY);
			int[] requiredAmountForDeed = DeedManager.manage.getRequiredAmountForDeed(buildingX, buildingY);
			constructionBoxNeededItems.gameObject.SetActive(true);
			for (int i = 0; i < requiredItemSlots.Length; i++)
			{
				if (i < requiredItemsForDeed.Length)
				{
					requiredItemSlots[i].gameObject.SetActive(true);
					requiredItemSlots[i].fillRecipeSlotWithAmounts(requiredItemsForDeed[i], itemsAlreadyGivenForDeed[i], requiredAmountForDeed[i]);
					if (itemsAlreadyGivenForDeed[i] >= requiredAmountForDeed[i])
					{
						requiredItemSlots[i].setSlotFull();
					}
					else
					{
						requiredItemSlots[i].setSlotEmpty();
					}
				}
				else
				{
					requiredItemSlots[i].gameObject.SetActive(false);
				}
			}
		}
		Inventory.inv.weaponSlot.gameObject.SetActive(false);
		giveWindowOpen = true;
		StartCoroutine(UpdateMenu());
		Inventory.inv.OpenInvForGive();
		GiveNPCWindow.gameObject.SetActive(true);
	}

	public void openOptionAmountWindow()
	{
		optionWindowOpen = true;
		optionAmountWindow.gameObject.SetActive(true);
		optionAmountWindow.fillItemDetails(dropToBuy.myItemId);
	}

	public void closeOptionAmountWindow()
	{
		optionWindowOpen = false;
		optionAmountWindow.gameObject.SetActive(false);
	}

	public void CloseGiveWindow()
	{
		GiveNPCWindow.gameObject.SetActive(false);
	}

	public void CloseAndCancel()
	{
		returnItemsAndEmptyGiveSlots();
		CloseAndMakeOffer();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	public void CloseAndMakeOffer()
	{
		GiveNPCWindow.gameObject.SetActive(false);
		Inventory.inv.invOpen = false;
		Inventory.inv.openAndCloseInv();
		bool flag = false;
		if (giveMenuTypeOpen == currentlyGivingTo.Tech)
		{
			getDollarValueOfGiveSlots();
			if (moneyOffer == 0)
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, noTechToGive);
			}
			else
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, donateTechItemsConvo);
			}
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.Swapping)
		{
			for (int i = 0; i < Inventory.inv.invSlots.Length; i++)
			{
				if (Inventory.inv.invSlots[i].isSelectedForGive())
				{
					NetworkMapSharer.share.localChar.CmdDropItem(Inventory.inv.invSlots[i].itemNo, Inventory.inv.invSlots[i].stack, NetworkMapSharer.share.localChar.transform.position, NetworkMapSharer.share.localChar.myInteract.tileHighlighter.transform.position);
					Inventory.inv.invSlots[i].updateSlotContentsAndRefresh(Inventory.inv.getInvItemId(BugAndFishCelebration.bugAndFishCel.invFullConvo.startLineAlt.talkingAboutItem), 1);
					Inventory.inv.equipNewSelectedSlot();
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				NetworkMapSharer.share.localChar.CmdDropItem(Inventory.inv.getInvItemId(BugAndFishCelebration.bugAndFishCel.invFullConvo.startLineAlt.talkingAboutItem), 1, NetworkMapSharer.share.localChar.transform.position, NetworkMapSharer.share.localChar.myInteract.tileHighlighter.transform.position);
			}
			clearAllSelectedSlots();
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.Build)
		{
			bool flag2 = false;
			for (int j = 0; j < Inventory.inv.invSlots.Length; j++)
			{
				if (Inventory.inv.invSlots[j].isSelectedForGive() && Inventory.inv.invSlots[j].itemNo != -1)
				{
					int stack = Inventory.inv.invSlots[j].stack;
					if (Inventory.inv.invSlots[j].getGiveAmount() == 0)
					{
						Inventory.inv.invSlots[j].stack = DeedManager.manage.returnStackAndDonateItemToDeed(Inventory.inv.invSlots[j].itemNo, Inventory.inv.invSlots[j].stack, buildingX, buildingY);
					}
					else
					{
						Inventory.inv.invSlots[j].stack = Inventory.inv.invSlots[j].stack - Inventory.inv.invSlots[j].getGiveAmount() + DeedManager.manage.returnStackAndDonateItemToDeed(Inventory.inv.invSlots[j].itemNo, Inventory.inv.invSlots[j].getGiveAmount(), buildingX, buildingY);
					}
					MonoBehaviour.print("Gave " + Inventory.inv.invSlots[j].stack);
					if (stack > Inventory.inv.invSlots[j].stack)
					{
						flag2 = true;
					}
					if (Inventory.inv.invSlots[j].stack > 0)
					{
						Inventory.inv.invSlots[j].updateSlotContentsAndRefresh(Inventory.inv.invSlots[j].itemNo, Inventory.inv.invSlots[j].stack);
					}
					else
					{
						Inventory.inv.invSlots[j].updateSlotContentsAndRefresh(-1, 0);
					}
				}
			}
			if (flag2)
			{
				SoundManager.manage.play2DSound(SoundManager.manage.dropInBoxSound);
				MonoBehaviour.print("Call update ingredients CMD");
				NetworkMapSharer.share.localChar.CmdDonateDeedIngredients(buildingX, buildingY, DeedManager.manage.getItemsAlreadyGivenForDeed(buildingX, buildingY));
			}
			if (DeedManager.manage.checkIfDeedMaterialsComplete(buildingX, buildingY))
			{
				ConversationManager.manage.talkToNPC(NPCManager.manage.sign, ConversationManager.manage.donatingToBuilding.conversationWhenLastItemsDonated);
			}
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.Museum || giveMenuTypeOpen == currentlyGivingTo.Sell || giveMenuTypeOpen == currentlyGivingTo.SellToJimmy || giveMenuTypeOpen == currentlyGivingTo.SellToTrapper)
		{
			if (getDollarValueOfGiveSlots())
			{
				if (giveMenuTypeOpen == currentlyGivingTo.Museum)
				{
					InventorySlot inventorySlot = null;
					for (int k = 0; k < Inventory.inv.invSlots.Length; k++)
					{
						if (Inventory.inv.invSlots[k].isSelectedForGive())
						{
							inventorySlot = Inventory.inv.invSlots[k];
							break;
						}
					}
					if ((bool)inventorySlot && MuseumManager.manage.checkIfItemCanBeDonated(inventorySlot.itemInSlot))
					{
						ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ConversationManager.manage.lastTalkTo.getVendorConversation().GetComponent<MuseumConvoGroup>().getDonationConversation(MuseumManager.manage.checkIfDonationNeeded(inventorySlot.itemInSlot)));
					}
					else
					{
						ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ConversationManager.manage.lastTalkTo.getVendorConversation().GetComponent<MuseumConvoGroup>().itemCantBeDonated);
					}
				}
				else if (moneyOffer == 0)
				{
					ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, totalEqualsZero);
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToTrapper)
				{
					ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, somethingToGiveTrapper);
				}
				else if (giveMenuTypeOpen == currentlyGivingTo.SellToJimmy)
				{
					ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, somethingToGiveJimmy);
				}
				else
				{
					ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, somethingToGive);
				}
			}
			else if (giveMenuTypeOpen == currentlyGivingTo.Museum)
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ConversationManager.manage.lastTalkTo.getVendorConversation().GetComponent<MuseumConvoGroup>().noItemsGiven);
			}
			else if (giveMenuTypeOpen == currentlyGivingTo.SellToTrapper)
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, nothingToGiveTrapper);
			}
			else if (giveMenuTypeOpen == currentlyGivingTo.SellToJimmy)
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, nothingToGiveJimmy);
			}
			else
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, nothingToGive);
			}
		}
		else
		{
			InventorySlot inventorySlot2 = null;
			for (int l = 0; l < Inventory.inv.invSlots.Length; l++)
			{
				if (Inventory.inv.invSlots[l].isSelectedForGive())
				{
					inventorySlot2 = Inventory.inv.invSlots[l];
					break;
				}
			}
			if (!inventorySlot2 || inventorySlot2.itemNo == -1)
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, noItemToGive);
			}
			else
			{
				int nPCNo = ConversationManager.manage.lastTalkTo.GetComponent<NPCIdentity>().NPCNo;
				if (givingPost != null)
				{
					int num = inventorySlot2.getGiveAmount();
					if (num == 0)
					{
						num = inventorySlot2.stack;
					}
					if (givingPost.isTrade)
					{
						if (givingPost.checkIfTradeItemIsDifferent(inventorySlot2.itemNo))
						{
							if (givingPost.checkIfTradeItemIsOk(inventorySlot2.itemNo))
							{
								ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, givingPost.getPostPostsById().onGivenItem);
								inventorySlot2.updateSlotContentsAndRefresh(inventorySlot2.itemNo, --inventorySlot2.stack);
							}
							else
							{
								ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, givingPost.getPostPostsById().onGivenWrongTypeForTrade);
							}
						}
						else
						{
							ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, givingPost.getPostPostsById().onGivenSameItemForTrade);
						}
					}
					else if (givingPost.requiredItem == inventorySlot2.itemNo && givingPost.requireItemAmount <= num)
					{
						ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, givingPost.getPostPostsById().onGivenItem);
						inventorySlot2.updateSlotContentsAndRefresh(inventorySlot2.itemNo, inventorySlot2.stack - givingPost.requireItemAmount);
					}
					else
					{
						ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, GivenItemIncorrect[Random.Range(0, GivenItemIncorrect.Length)]);
					}
				}
				else
				{
					int num2 = inventorySlot2.getGiveAmount();
					if (num2 == 0)
					{
						num2 = inventorySlot2.stack;
					}
					if (NPCManager.manage.NPCRequests[nPCNo].checkIfItemMatchesRequest(inventorySlot2.itemNo) && NPCManager.manage.NPCRequests[nPCNo].desiredAmount <= num2)
					{
						if (NPCManager.manage.NPCRequests[nPCNo].checkIfNPCAccepts(inventorySlot2.itemNo))
						{
							if (Inventory.inv.getInvItemId(NPCManager.manage.NPCDetails[nPCNo].favouriteFood) == inventorySlot2.itemNo)
							{
								ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ConversationGenerator.generate.GivenFavouriteFood[Random.Range(0, ConversationGenerator.generate.GivenFavouriteFood.Length)]);
							}
							else
							{
								ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, GivenCorrectItem[Random.Range(0, GivenCorrectItem.Length)]);
							}
							inventorySlot2.stack -= NPCManager.manage.NPCRequests[nPCNo].desiredAmount;
							inventorySlot2.updateSlotContentsAndRefresh(inventorySlot2.itemNo, inventorySlot2.stack);
							storeRewardValue();
						}
						else
						{
							if (Inventory.inv.getInvItemId(NPCManager.manage.NPCDetails[nPCNo].hatedFood) == inventorySlot2.itemNo)
							{
								ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ConversationGenerator.generate.GivenHatedFood[Random.Range(0, ConversationGenerator.generate.GivenHatedFood.Length)]);
								NPCManager.manage.npcStatus[nPCNo].addToRelationshipLevel(-1);
							}
							else
							{
								ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ConversationGenerator.generate.GivenDislikeFood[Random.Range(0, ConversationGenerator.generate.GivenDislikeFood.Length)]);
							}
							NPCManager.manage.NPCRequests[nPCNo].failRequest(nPCNo);
						}
					}
					else
					{
						ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, GivenItemIncorrect[Random.Range(0, GivenItemIncorrect.Length)]);
						givingPost = null;
					}
				}
			}
		}
		if (giveMenuTypeOpen == currentlyGivingTo.BulletinBoard || giveMenuTypeOpen == currentlyGivingTo.Give)
		{
			MonoBehaviour.print(givingPost);
			if (givingPost == null || (givingPost.getPostPostsById() != null && givingPost.isTrade))
			{
				clearAllSelectedSlots();
			}
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.Build)
		{
			clearAllSelectedSlots();
		}
		clearAllDisabled();
		giveWindowOpen = false;
	}

	private void storeRewardValue()
	{
		InventorySlot inventorySlot = null;
		for (int i = 0; i < Inventory.inv.invSlots.Length; i++)
		{
			if (Inventory.inv.invSlots[i].isSelectedForGive())
			{
				inventorySlot = Inventory.inv.invSlots[i];
				break;
			}
		}
		if ((bool)inventorySlot && inventorySlot.itemNo > -1)
		{
			storedReward = Inventory.inv.allItems[inventorySlot.itemNo].value * 2 + Random.Range(Inventory.inv.allItems[inventorySlot.itemNo].value / 2, Inventory.inv.allItems[inventorySlot.itemNo].value * 2);
		}
		else
		{
			storedReward = 1000;
		}
	}

	public int getRewardAmount()
	{
		return storedReward;
	}

	public void completeRequest()
	{
		clearAllSelectedSlots();
	}

	public bool getDollarValueOfGiveSlots()
	{
		moneyOffer = 0;
		bool result = false;
		InventorySlot[] invSlots = Inventory.inv.invSlots;
		foreach (InventorySlot inventorySlot in invSlots)
		{
			if (inventorySlot.itemNo != -1 && inventorySlot.isSelectedForGive())
			{
				result = true;
				if (inventorySlot.itemInSlot.hasFuel || inventorySlot.itemInSlot.hasColourVariation)
				{
					moneyOffer += inventorySlot.itemInSlot.value;
				}
				else if (inventorySlot.getGiveAmount() == 0)
				{
					moneyOffer += inventorySlot.itemInSlot.value * inventorySlot.stack;
				}
				else
				{
					moneyOffer += inventorySlot.itemInSlot.value * inventorySlot.getGiveAmount();
				}
			}
		}
		moneyOffer += Mathf.RoundToInt((float)moneyOffer / 10f * (float)LicenceManager.manage.allLicences[8].getCurrentLevel());
		if (giveMenuTypeOpen == currentlyGivingTo.SellToTrapper || giveMenuTypeOpen == currentlyGivingTo.SellToJimmy)
		{
			moneyOffer = Mathf.RoundToInt((float)moneyOffer * 1.5f);
		}
		else if (giveMenuTypeOpen == currentlyGivingTo.Tech)
		{
			moneyOffer = Mathf.RoundToInt((float)moneyOffer * 6f);
		}
		return result;
	}

	public void donateTechItems()
	{
		Inventory.inv.changeWallet(moneyOffer, false);
		InventorySlot[] invSlots = Inventory.inv.invSlots;
		foreach (InventorySlot inventorySlot in invSlots)
		{
			if ((bool)inventorySlot.itemInSlot && inventorySlot.isSelectedForGive())
			{
				if (inventorySlot.getGiveAmount() == 0)
				{
					inventorySlot.updateSlotContentsAndRefresh(-1, 0);
				}
				else
				{
					inventorySlot.updateSlotContentsAndRefresh(inventorySlot.itemNo, inventorySlot.stack - inventorySlot.getGiveAmount());
				}
			}
			inventorySlot.deselectThisSlotForGive();
		}
	}

	public void checkSellSlotForTask(InventorySlot slotToCheck, int stackAmount)
	{
		if (Inventory.inv.allItems[slotToCheck.itemNo].taskWhenSold != 0)
		{
			DailyTaskGenerator.generate.doATask(Inventory.inv.allItems[slotToCheck.itemNo].taskWhenSold, stackAmount);
			return;
		}
		if ((bool)Inventory.inv.allItems[slotToCheck.itemNo].consumeable)
		{
			if (Inventory.inv.allItems[slotToCheck.itemNo].consumeable.isFruit)
			{
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SellFruit, stackAmount);
			}
			else if (Inventory.inv.allItems[slotToCheck.itemNo].consumeable.isVegitable)
			{
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SellCrops, stackAmount);
			}
		}
		if ((bool)Inventory.inv.allItems[slotToCheck.itemNo].fish)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SellFish, stackAmount);
		}
		if ((bool)Inventory.inv.allItems[slotToCheck.itemNo].bug)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SellBugs, stackAmount);
		}
	}

	public void sellItemsAndEmptyGiveSlots()
	{
		InventorySlot[] invSlots = Inventory.inv.invSlots;
		foreach (InventorySlot inventorySlot in invSlots)
		{
			if ((bool)inventorySlot.itemInSlot && inventorySlot.isSelectedForGive())
			{
				if (inventorySlot.getGiveAmount() == 0)
				{
					if (!Inventory.inv.allItems[inventorySlot.itemNo].checkIfStackable())
					{
						CharLevelManager.manage.addToDayTally(inventorySlot.itemNo, 1, CharLevelManager.manage.skillBoxes.Length);
						checkSellSlotForTask(inventorySlot, 1);
					}
					else
					{
						CharLevelManager.manage.addToDayTally(inventorySlot.itemNo, inventorySlot.stack, CharLevelManager.manage.skillBoxes.Length);
						checkSellSlotForTask(inventorySlot, inventorySlot.stack);
					}
					inventorySlot.updateSlotContentsAndRefresh(-1, 0);
				}
				else
				{
					if (!Inventory.inv.allItems[inventorySlot.itemNo].checkIfStackable())
					{
						CharLevelManager.manage.addToDayTally(inventorySlot.itemNo, 1, CharLevelManager.manage.skillBoxes.Length);
						checkSellSlotForTask(inventorySlot, 1);
					}
					else
					{
						CharLevelManager.manage.addToDayTally(inventorySlot.itemNo, inventorySlot.getGiveAmount(), CharLevelManager.manage.skillBoxes.Length);
						checkSellSlotForTask(inventorySlot, inventorySlot.getGiveAmount());
					}
					inventorySlot.updateSlotContentsAndRefresh(inventorySlot.itemNo, inventorySlot.stack - inventorySlot.getGiveAmount());
				}
			}
			inventorySlot.deselectThisSlotForGive();
		}
		if (NetworkMapSharer.share.isServer)
		{
			NPCManager.manage.npcStatus[ConversationManager.manage.lastTalkTo.myId.NPCNo].moneySpentAtStore += moneyOffer;
		}
		Inventory.inv.changeWallet(moneyOffer);
		CharLevelManager.manage.todaysMoneyTotal += moneyOffer;
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SellItems, moneyOffer);
		moneyOffer = 0;
	}

	public void returnItemsAndEmptyGiveSlots()
	{
		clearAllSelectedSlots();
		moneyOffer = 0;
	}

	public void askAboutBuyingSomething(ShopBuyDrop myDrop, NPCAI npcTalkTo)
	{
		dropToBuy = myDrop;
		Inventory.inv.quickSlotBar.gameObject.SetActive(false);
		if (!Inventory.inv.allItems[dropToBuy.myItemId].checkIfCanBuy())
		{
			ConversationManager.manage.talkToNPC(npcTalkTo, dontHaveLicence);
		}
		else
		{
			ConversationManager.manage.talkToNPC(npcTalkTo, wantToBuySomething);
		}
	}

	public void askAboutBuyingAnimal(ShopBuyDrop myDrop, NPCAI npcTalkTo)
	{
		dropToBuy = myDrop;
		Inventory.inv.quickSlotBar.gameObject.SetActive(false);
		if (!NetworkMapSharer.share.isServer)
		{
			ConversationManager.manage.talkToNPC(npcTalkTo, buyAnimalNotLocal);
		}
		else
		{
			if (!myDrop.sellsAnimal.GetComponent<FarmAnimal>().checkIfCanBuy())
			{
				ConversationManager.manage.talkToNPC(npcTalkTo, dontHaveLicence);
				return;
			}
			if (FarmAnimalMenu.menu.checkIfAnimalBoxIsInShop())
			{
				ConversationManager.manage.talkToNPC(npcTalkTo, animalAlreadyInShop);
			}
		}
		if (Inventory.inv.wallet < myDrop.sellsAnimal.GetComponent<FarmAnimal>().baseValue)
		{
			ConversationManager.manage.talkToNPC(npcTalkTo, wantToBuyAnimalNotEnoughMoney);
		}
		else
		{
			ConversationManager.manage.talkToNPC(npcTalkTo, wantToBuyAnimal);
		}
	}

	public void askAboutBuyingRecipe(ShopBuyDrop myDrop, NPCAI npcTalkTo)
	{
		dropToBuy = myDrop;
		Inventory.inv.quickSlotBar.gameObject.SetActive(false);
		if (!CharLevelManager.manage.checkIfUnlocked(myDrop.myItemId))
		{
			ConversationManager.manage.talkToNPC(npcTalkTo, wantToBuyRecipe);
		}
		else
		{
			ConversationManager.manage.talkToNPC(npcTalkTo, alreadyHaveRecipe);
		}
	}

	public void tryToBuy()
	{
		if (!Inventory.inv.allItems[dropToBuy.myItemId].checkIfCanBuy())
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, dontHaveLicence);
		}
		else if (dropToBuy.usesPermitPoints)
		{
			int num = Inventory.inv.allItems[give.dropToBuy.myItemId].value * 2 / 500;
			if (PermitPointsManager.manage.getCurrentPoints() < num)
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, notEnoughMoneyToBuySomething);
			}
			else if (Inventory.inv.addItemToInventory(dropToBuy.myItemId, 1))
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, hasEnoughMoneyToBuySomething);
				PermitPointsManager.manage.spendPoints(num);
				if (NetworkMapSharer.share.isServer)
				{
					NPCManager.manage.npcStatus[ConversationManager.manage.lastTalkTo.myId.NPCNo].moneySpentAtStore += Inventory.inv.allItems[dropToBuy.myItemId].value * 2 * optionAmountWindow.getSelectedAmount();
				}
				dropToBuy.buyTheItem();
				dropToBuy = null;
			}
			else
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, notEnoughSpaceInInvToBuySomething);
			}
		}
		else if (dropToBuy.recipesOnly && Inventory.inv.wallet < dropToBuy.getRecipePrice())
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, notEnoughMoneyToBuySomething);
		}
		else if (!dropToBuy.recipesOnly && dropToBuy.gives10 && Inventory.inv.wallet < Inventory.inv.allItems[dropToBuy.myItemId].value * 2 * optionAmountWindow.getSelectedAmount())
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, notEnoughMoneyToBuySomething);
		}
		else if (!dropToBuy.recipesOnly && !dropToBuy.gives10 && Inventory.inv.wallet < Inventory.inv.allItems[dropToBuy.myItemId].value * 2)
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, notEnoughMoneyToBuySomething);
		}
		else if (dropToBuy.recipesOnly)
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, hasEnoughMoneyToBuySomething);
			Inventory.inv.changeWallet(-dropToBuy.getRecipePrice());
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyItems, dropToBuy.getRecipePrice());
			GiftedItemWindow.gifted.addRecipeToUnlock(dropToBuy.myItemId);
			GiftedItemWindow.gifted.openWindowAndGiveItems();
			dropToBuy.checkIfTaskCompelted();
			dropToBuy.buyTheItem();
			dropToBuy = null;
		}
		else if (dropToBuy.gives10 && Inventory.inv.addItemToInventory(dropToBuy.myItemId, give.optionAmountWindow.getSelectedAmount()))
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, hasEnoughMoneyToBuySomething);
			Inventory.inv.changeWallet(-(Inventory.inv.allItems[dropToBuy.myItemId].value * 2) * optionAmountWindow.getSelectedAmount());
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyItems, Inventory.inv.allItems[dropToBuy.myItemId].value * 2 * optionAmountWindow.getSelectedAmount());
			if (NetworkMapSharer.share.isServer)
			{
				NPCManager.manage.npcStatus[ConversationManager.manage.lastTalkTo.myId.NPCNo].moneySpentAtStore += Inventory.inv.allItems[dropToBuy.myItemId].value * 2 * optionAmountWindow.getSelectedAmount();
			}
			dropToBuy.checkIfTaskCompelted(optionAmountWindow.getSelectedAmount());
			dropToBuy.buyTheItem();
			dropToBuy = null;
		}
		else if ((!Inventory.inv.allItems[dropToBuy.myItemId].hasFuel && Inventory.inv.addItemToInventory(dropToBuy.myItemId, 1)) || (Inventory.inv.allItems[dropToBuy.myItemId].hasFuel && Inventory.inv.addItemToInventory(dropToBuy.myItemId, Inventory.inv.allItems[dropToBuy.myItemId].fuelMax)))
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, hasEnoughMoneyToBuySomething);
			Inventory.inv.changeWallet(-Inventory.inv.allItems[dropToBuy.myItemId].value * 2);
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyItems, Inventory.inv.allItems[dropToBuy.myItemId].value * 2);
			if (NetworkMapSharer.share.isServer)
			{
				NPCManager.manage.npcStatus[ConversationManager.manage.lastTalkTo.myId.NPCNo].moneySpentAtStore += Inventory.inv.allItems[dropToBuy.myItemId].value * 2;
			}
			dropToBuy.checkIfTaskCompelted();
			dropToBuy.buyTheItem();
			dropToBuy = null;
		}
		else
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, notEnoughSpaceInInvToBuySomething);
		}
	}

	public string getDonationName()
	{
		InventorySlot inventorySlot = null;
		for (int i = 0; i < Inventory.inv.invSlots.Length; i++)
		{
			if (Inventory.inv.invSlots[i].isSelectedForGive())
			{
				inventorySlot = Inventory.inv.invSlots[i];
				break;
			}
		}
		if ((bool)inventorySlot && (bool)inventorySlot.itemInSlot)
		{
			return inventorySlot.itemInSlot.getInvItemName();
		}
		return "!";
	}

	public void donateItemToMuseum()
	{
		InventorySlot inventorySlot = null;
		for (int i = 0; i < Inventory.inv.invSlots.Length; i++)
		{
			if (Inventory.inv.invSlots[i].isSelectedForGive())
			{
				inventorySlot = Inventory.inv.invSlots[i];
				break;
			}
		}
		clearAllSelectedSlots();
		NetworkMapSharer.share.localChar.CmdDonateItemToMuseum(inventorySlot.itemNo);
		inventorySlot.updateSlotContentsAndRefresh(-1, 0);
		PermitPointsManager.manage.addPoints(100);
	}

	public void clearAllSelectedSlots()
	{
		InventorySlot[] invSlots = Inventory.inv.invSlots;
		for (int i = 0; i < invSlots.Length; i++)
		{
			invSlots[i].deselectThisSlotForGive();
		}
	}

	public void clearAllDisabled()
	{
		Inventory.inv.weaponSlot.gameObject.SetActive(true);
		InventorySlot[] invSlots = Inventory.inv.invSlots;
		for (int i = 0; i < invSlots.Length; i++)
		{
			invSlots[i].clearDisable();
		}
	}

	public void blackOutNonGiveableObjects()
	{
		InventorySlot[] invSlots;
		if (giveMenuTypeOpen == currentlyGivingTo.Build)
		{
			int[] requiredItemsForDeed = DeedManager.manage.getRequiredItemsForDeed(buildingX, buildingY);
			int[] itemsAlreadyGivenForDeed = DeedManager.manage.getItemsAlreadyGivenForDeed(buildingX, buildingY);
			int[] requiredAmountForDeed = DeedManager.manage.getRequiredAmountForDeed(buildingX, buildingY);
			invSlots = Inventory.inv.invSlots;
			foreach (InventorySlot inventorySlot in invSlots)
			{
				if (inventorySlot.itemNo == -1 || inventorySlot.itemInSlot.isDeed)
				{
					inventorySlot.disableForGive();
					continue;
				}
				bool flag = false;
				for (int j = 0; j < requiredItemsForDeed.Length; j++)
				{
					if (inventorySlot.itemNo == requiredItemsForDeed[j])
					{
						if (itemsAlreadyGivenForDeed[j] >= requiredAmountForDeed[j])
						{
							inventorySlot.disableForGive();
						}
						flag = true;
					}
				}
				if (!flag)
				{
					inventorySlot.disableForGive();
				}
			}
			return;
		}
		if (giveMenuTypeOpen == currentlyGivingTo.Tech)
		{
			invSlots = Inventory.inv.invSlots;
			foreach (InventorySlot inventorySlot2 in invSlots)
			{
				if (inventorySlot2.itemNo == -1 || inventorySlot2.itemNo != CraftsmanManager.manage.shinyDiscItem.getItemId())
				{
					inventorySlot2.disableForGive();
				}
			}
			return;
		}
		if (giveMenuTypeOpen == currentlyGivingTo.Museum)
		{
			invSlots = Inventory.inv.invSlots;
			foreach (InventorySlot inventorySlot3 in invSlots)
			{
				if (inventorySlot3.itemNo == -1 || inventorySlot3.itemInSlot.isDeed)
				{
					inventorySlot3.disableForGive();
				}
				else if (!inventorySlot3.itemInSlot.fish && !inventorySlot3.itemInSlot.bug && !inventorySlot3.itemInSlot.underwaterCreature)
				{
					inventorySlot3.disableForGive();
				}
				else if (!MuseumManager.manage.checkIfDonationNeeded(inventorySlot3.itemInSlot))
				{
					inventorySlot3.disableForGive();
				}
			}
			return;
		}
		if (giveMenuTypeOpen == currentlyGivingTo.Sell || giveMenuTypeOpen == currentlyGivingTo.Swapping || giveMenuTypeOpen == currentlyGivingTo.SellToTrapper || giveMenuTypeOpen == currentlyGivingTo.SellToJimmy)
		{
			Vector3 position = NetworkMapSharer.share.localChar.myInteract.tileHighlighter.transform.position;
			bool flag2 = WorldManager.manageWorld.checkIfFishCanBeDropped(position);
			invSlots = Inventory.inv.invSlots;
			foreach (InventorySlot inventorySlot4 in invSlots)
			{
				if (inventorySlot4.itemNo == -1 || inventorySlot4.itemInSlot.isDeed || inventorySlot4.itemInSlot.getItemId() == Inventory.inv.moneyItem.getItemId())
				{
					inventorySlot4.disableForGive();
				}
				if (giveMenuTypeOpen == currentlyGivingTo.Swapping && inventorySlot4.itemNo != -1 && (bool)inventorySlot4.itemInSlot.fish && !flag2)
				{
					inventorySlot4.disableForGive();
				}
				if (giveMenuTypeOpen == currentlyGivingTo.SellToJimmy)
				{
					MonoBehaviour.print("Checking slot for jimmy");
					if (inventorySlot4.itemNo != -1 && inventorySlot4.stack >= 50 && !Inventory.inv.allItems[inventorySlot4.itemNo].hasFuel && !Inventory.inv.allItems[inventorySlot4.itemNo].hasColourVariation)
					{
						MonoBehaviour.print("Should be selectedable");
					}
					else
					{
						inventorySlot4.disableForGive();
					}
				}
				else
				{
					if (giveMenuTypeOpen != currentlyGivingTo.SellToTrapper)
					{
						continue;
					}
					if (inventorySlot4.itemNo != -1 && (bool)inventorySlot4.itemInSlot.consumeable)
					{
						if (!inventorySlot4.itemInSlot.consumeable.isMeat)
						{
							inventorySlot4.disableForGive();
						}
					}
					else if (inventorySlot4.itemNo != -1 && (bool)inventorySlot4.itemInSlot.itemChange && (bool)inventorySlot4.itemInSlot.itemChange.changesAndTheirChanger[0].changesWhenComplete && (bool)inventorySlot4.itemInSlot.itemChange.changesAndTheirChanger[0].changesWhenComplete.consumeable)
					{
						if (!inventorySlot4.itemInSlot.itemChange.changesAndTheirChanger[0].changesWhenComplete.consumeable.isMeat)
						{
							inventorySlot4.disableForGive();
						}
					}
					else if (inventorySlot4.itemNo != -1 && !inventorySlot4.itemInSlot.fish && !inventorySlot4.itemInSlot.bug && !inventorySlot4.itemInSlot.underwaterCreature)
					{
						inventorySlot4.disableForGive();
					}
				}
			}
			return;
		}
		int nPCNo = ConversationManager.manage.lastTalkTo.GetComponent<NPCIdentity>().NPCNo;
		invSlots = Inventory.inv.invSlots;
		foreach (InventorySlot inventorySlot5 in invSlots)
		{
			if (inventorySlot5.itemNo == -1 || inventorySlot5.itemInSlot.isDeed)
			{
				inventorySlot5.disableForGive();
			}
			else if (givingPost != null)
			{
				if (givingPost.isTrade)
				{
					if (!givingPost.checkIfTradeItemIsOk(inventorySlot5.itemNo))
					{
						inventorySlot5.disableForGive();
					}
				}
				else if (givingPost.requiredItem != inventorySlot5.itemNo)
				{
					inventorySlot5.disableForGive();
				}
			}
			else if (!NPCManager.manage.NPCRequests[nPCNo].checkIfItemMatchesRequest(inventorySlot5.itemNo))
			{
				inventorySlot5.disableForGive();
			}
		}
	}

	public void setSellingByWeight(SellByWeight selling)
	{
		sellingByWeight = selling;
	}

	public string getSellByWeightName()
	{
		if ((bool)sellingByWeight)
		{
			return sellingByWeight.itemName;
		}
		return "Thing";
	}

	public string getItemWeight()
	{
		if ((bool)sellingByWeight)
		{
			return sellingByWeight.getMyWeight().ToString("0.00") + "Kg";
		}
		return "1";
	}

	public int getSellByWeightMoneyValue()
	{
		return sellingByWeight.getPrice();
	}

	public void sellSellingByWeight()
	{
		CharLevelManager.manage.todaysMoneyTotal += getSellByWeightMoneyValue();
		Inventory.inv.changeWallet(getSellByWeightMoneyValue());
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.SellItems, getSellByWeightMoneyValue());
		if (sellingByWeight.taskWhenSold != 0)
		{
			DailyTaskGenerator.generate.doATask(sellingByWeight.taskWhenSold);
		}
		NetworkMapSharer.share.localChar.CmdSellByWeight(sellingByWeight.netId);
		sellingByWeight = null;
	}
}
