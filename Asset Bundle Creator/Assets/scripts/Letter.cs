using System;
using UnityEngine;

[Serializable]
public class Letter
{
	public enum LetterType
	{
		randomLetter = 0,
		thankYouLetter = 1,
		moveInLetter = 2,
		fullInvLetter = 3,
		AnimalResearchLetter = 4,
		AnimalTrapReturn = 5,
		DevLetter = 6,
		CatalogueOrder = 7,
		CraftsmanClosedLetter = 8,
		FishingTips = 9,
		BugTips = 10,
		LicenceUnlock = 11
	}

	public int letterTemplateNo;

	public int sentById;

	public int seasonSent;

	public int itemAttached = -1;

	public int stackOfItemAttached = 1;

	public int itemOriginallAttached = -1;

	public LetterType myType;

	public bool hasBeenRead;

	public Letter()
	{
	}

	public Letter(int NPCFrom, LetterType letterType)
	{
		sentById = NPCFrom;
		myType = letterType;
		getRewardFromTemplate();
		itemOriginallAttached = itemAttached;
		seasonSent = WorldManager.manageWorld.month;
	}

	public Letter(int NPCFrom, LetterType letterType, int rewardId, int rewardStack)
	{
		sentById = NPCFrom;
		itemAttached = rewardId;
		stackOfItemAttached = rewardStack;
		myType = letterType;
		itemOriginallAttached = rewardId;
		seasonSent = WorldManager.manageWorld.month;
	}

	public Letter(int NPCFrom, LetterType letterType, int itemSubject)
	{
		sentById = NPCFrom;
		itemAttached = -1;
		stackOfItemAttached = -1;
		myType = letterType;
		itemOriginallAttached = itemSubject;
		seasonSent = WorldManager.manageWorld.month;
	}

	private void getRewardFromTemplate()
	{
		if (getMyTemplate() == null)
		{
			Debug.Log("Template was null");
		}
		if (getMyTemplate() != null && getMyTemplate().gift != null)
		{
			itemAttached = Inventory.inv.getInvItemId(getMyTemplate().gift);
			stackOfItemAttached = getMyTemplate().stackOfGift;
		}
		else if (getMyTemplate() != null && getMyTemplate().giftFromTable != null)
		{
			Debug.Log(getMyTemplate().giftFromTable.name);
			itemAttached = Inventory.inv.getInvItemId(getMyTemplate().giftFromTable.getRandomDropFromTable());
			stackOfItemAttached = getMyTemplate().stackOfGift;
		}
		else if (getMyTemplate() != null && getMyTemplate().useRandomFromType)
		{
			itemAttached = getMyTemplate().getRandomGiftFromType();
			stackOfItemAttached = getMyTemplate().stackOfGift;
		}
	}

	public LetterTemplate getMyTemplate()
	{
		if (myType == LetterType.moveInLetter)
		{
			return null;
		}
		if (myType == LetterType.randomLetter)
		{
			if (letterTemplateNo == 0)
			{
				letterTemplateNo = UnityEngine.Random.Range(0, MailManager.manage.randomLetters.Length);
				UnityEngine.Random.InitState(UnityEngine.Random.Range(205050, -209848));
			}
			return MailManager.manage.randomLetters[letterTemplateNo];
		}
		if (myType == LetterType.thankYouLetter)
		{
			if (letterTemplateNo == 0)
			{
				letterTemplateNo = UnityEngine.Random.Range(0, MailManager.manage.thankYouLetters.Length);
				UnityEngine.Random.InitState(UnityEngine.Random.Range(205050, -209848));
			}
			return MailManager.manage.thankYouLetters[letterTemplateNo];
		}
		if (myType == LetterType.fullInvLetter)
		{
			return MailManager.manage.didNotFitInInvLetter[letterTemplateNo];
		}
		if (myType == LetterType.FishingTips)
		{
			return MailManager.manage.fishingTips[letterTemplateNo];
		}
		if (myType == LetterType.LicenceUnlock)
		{
			return MailManager.manage.licenceLevelUp[letterTemplateNo];
		}
		if (myType == LetterType.BugTips)
		{
			return MailManager.manage.bugTips[letterTemplateNo];
		}
		if (myType == LetterType.AnimalResearchLetter)
		{
			return MailManager.manage.animalResearchLetter;
		}
		if (myType == LetterType.AnimalTrapReturn)
		{
			return MailManager.manage.returnTrapLetter;
		}
		if (myType == LetterType.CraftsmanClosedLetter)
		{
			return MailManager.manage.craftmanDayOff;
		}
		if (myType == LetterType.CatalogueOrder)
		{
			return MailManager.manage.catalogueItemLetter;
		}
		if (myType == LetterType.DevLetter)
		{
			return MailManager.manage.devLetter;
		}
		return null;
	}
}
