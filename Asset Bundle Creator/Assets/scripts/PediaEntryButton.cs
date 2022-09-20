using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PediaEntryButton : MonoBehaviour
{
	public Image itemImage;

	private PediaEntry showingEntry;

	public GameObject nameTag;

	public TextMeshProUGUI nameText;

	public WindowAnimator myAnim;

	public GameObject museumIcon;

	private bool isVillager;

	private bool isRecipe;

	public void setUpButton(PediaEntry myEntry, int i)
	{
		isVillager = myEntry.entryType == 5;
		isRecipe = myEntry.entryType == 6;
		if (isRecipe)
		{
			museumIcon.SetActive(false);
			itemImage.sprite = Inventory.inv.allItems[myEntry.itemId].getSprite();
			nameText.text = Inventory.inv.allItems[myEntry.itemId].getInvItemName();
			nameTag.SetActive(true);
		}
		else if (isVillager)
		{
			museumIcon.SetActive(false);
			if (NPCManager.manage.npcStatus[myEntry.itemId].hasMet)
			{
				itemImage.sprite = NPCManager.manage.NPCDetails[myEntry.itemId].getNPCSprite(myEntry.itemId);
				nameText.text = NPCManager.manage.NPCDetails[myEntry.itemId].NPCName;
				nameTag.SetActive(true);
			}
			else
			{
				itemImage.sprite = PediaManager.manage.notFoundSprite;
				nameTag.SetActive(false);
			}
		}
		else
		{
			if (myEntry.amountCaught > 0)
			{
				itemImage.sprite = Inventory.inv.allItems[myEntry.itemId].getSprite();
				nameText.text = Inventory.inv.allItems[myEntry.itemId].getInvItemName();
				nameTag.SetActive(true);
			}
			else
			{
				itemImage.sprite = PediaManager.manage.notFoundSprite;
				nameTag.SetActive(false);
			}
			if ((bool)Inventory.inv.allItems[myEntry.itemId].bug || (bool)Inventory.inv.allItems[myEntry.itemId].fish || (bool)Inventory.inv.allItems[myEntry.itemId].underwaterCreature)
			{
				museumIcon.gameObject.SetActive(!MuseumManager.manage.checkIfDonationNeeded(Inventory.inv.allItems[myEntry.itemId]));
			}
			else
			{
				museumIcon.gameObject.SetActive(false);
			}
		}
		showingEntry = myEntry;
		myAnim.openDelay = 0.01f;
		base.gameObject.SetActive(false);
		base.gameObject.SetActive(true);
	}

	public int getEntryNumber()
	{
		return showingEntry.itemId;
	}

	public SeasonAndTime getMySeasonAndTimeForSort()
	{
		if ((bool)Inventory.inv.allItems[showingEntry.itemId].bug)
		{
			return Inventory.inv.allItems[showingEntry.itemId].bug.mySeason;
		}
		if ((bool)Inventory.inv.allItems[showingEntry.itemId].fish)
		{
			return Inventory.inv.allItems[showingEntry.itemId].fish.mySeason;
		}
		if ((bool)Inventory.inv.allItems[showingEntry.itemId].underwaterCreature)
		{
			return Inventory.inv.allItems[showingEntry.itemId].underwaterCreature.mySeason;
		}
		if ((bool)Inventory.inv.allItems[showingEntry.itemId].relic)
		{
			return Inventory.inv.allItems[showingEntry.itemId].relic.myseason;
		}
		return null;
	}

	public void pressButton()
	{
		if (isVillager && NPCManager.manage.npcStatus[showingEntry.itemId].hasMet)
		{
			PediaManager.manage.showEntryDetails(showingEntry);
		}
		else if (isRecipe)
		{
			PediaManager.manage.showEntryDetails(showingEntry);
		}
		else if (showingEntry.amountCaught > 0)
		{
			PediaManager.manage.showEntryDetails(showingEntry);
		}
	}
}
