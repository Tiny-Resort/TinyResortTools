using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiftedItemWindow : MonoBehaviour
{
	public static GiftedItemWindow gifted;

	public GameObject window;

	private List<int> itemsToBeGiven = new List<int>();

	private List<int> amountToBeGiven = new List<int>();

	private List<int> recipeLearnt = new List<int>();

	private List<int> licenceToBeGiven = new List<int>();

	public TextMeshProUGUI recipeOrReceivedText;

	public TextMeshProUGUI titleText;

	public TextMeshProUGUI descText;

	public TextMeshProUGUI amountText;

	public Image itemIcon;

	public ASound windowPopUpSound;

	public ASound recipePopUpSound;

	public ASound licnecePopUpSound;

	public ASound giftWindowPopupSound;

	public GameObject nextArrow;

	public GameObject blueprintIcon;

	public bool windowOpen;

	public Color textColor;

	public Color recipeTextColor;

	public InventoryItem mapItem;

	public InventoryItem journalItem;

	public GameObject giftBox;

	public GameObject licenceStamp;

	private void Awake()
	{
		gifted = this;
	}

	public void addLicenceToBeGiven(int licenceId)
	{
		licenceToBeGiven.Add(licenceId);
	}

	public void addToListToBeGiven(int itemId, int stackAmount)
	{
		itemsToBeGiven.Add(itemId);
		amountToBeGiven.Add(stackAmount);
	}

	public void addRecipeToUnlock(int itemId)
	{
		recipeLearnt.Add(itemId);
	}

	public void openWindowAndGiveItems(float delay = 0.5f)
	{
		if (!windowOpen)
		{
			windowOpen = true;
			titleText.gameObject.SetActive(false);
			descText.gameObject.SetActive(false);
			itemIcon.gameObject.SetActive(false);
			StartCoroutine(giveItemDelay(delay));
		}
	}

	public void setTextColor(Color newColor)
	{
		recipeOrReceivedText.color = newColor;
		titleText.color = newColor;
		descText.color = newColor;
	}

	public IEnumerator playSoundWithDelay(ASound soundToPlay, float delay)
	{
		SoundManager.manage.play2DSound(giftWindowPopupSound);
		yield return new WaitForSeconds(delay);
		SoundManager.manage.play2DSound(soundToPlay);
	}

	private IEnumerator giveItemDelay(float delay)
	{
		bool somethingWasMailed = false;
		blueprintIcon.SetActive(false);
		while (ConversationManager.manage.inConversation)
		{
			yield return null;
		}
		yield return new WaitForSeconds(delay);
		bool ready = false;
		giftBox.SetActive(false);
		licenceStamp.SetActive(true);
		for (int k = 0; k < licenceToBeGiven.Count; k++)
		{
			setTextColor(textColor);
			recipeOrReceivedText.text = "New Licence!";
			blueprintIcon.SetActive(false);
			titleText.text = LicenceManager.manage.getLicenceName((LicenceManager.LicenceTypes)licenceToBeGiven[k]) + " Level " + LicenceManager.manage.allLicences[licenceToBeGiven[k]].getCurrentLevel();
			descText.text = "On ya!";
			amountText.text = "";
			itemIcon.sprite = LicenceManager.manage.licenceIcons[licenceToBeGiven[k]];
			window.SetActive(true);
			StartCoroutine(playSoundWithDelay(licnecePopUpSound, 0.5f));
			itemIcon.gameObject.SetActive(true);
			titleText.gameObject.SetActive(true);
			descText.gameObject.SetActive(true);
			nextArrow.SetActive(true);
			yield return new WaitForSeconds(1f);
			while (!ready)
			{
				yield return null;
				if (InputMaster.input.UISelect())
				{
					ready = true;
					SoundManager.manage.play2DSound(ConversationManager.manage.nextTextSound);
					nextArrow.SetActive(false);
				}
			}
			window.SetActive(false);
			titleText.gameObject.SetActive(false);
			descText.gameObject.SetActive(false);
			itemIcon.gameObject.SetActive(false);
			ready = false;
			yield return new WaitForSeconds(delay);
		}
		licenceStamp.SetActive(false);
		giftBox.SetActive(true);
		for (int k = 0; k < itemsToBeGiven.Count; k++)
		{
			setTextColor(textColor);
			recipeOrReceivedText.text = "You received";
			titleText.text = Inventory.inv.allItems[itemsToBeGiven[k]].getInvItemName();
			descText.text = Inventory.inv.allItems[itemsToBeGiven[k]].getItemDescription(itemsToBeGiven[k]);
			itemIcon.sprite = Inventory.inv.allItems[itemsToBeGiven[k]].getSprite();
			if (amountToBeGiven[k] > 1)
			{
				amountText.text = amountToBeGiven[k].ToString("n0");
			}
			else
			{
				amountText.text = "";
			}
			window.SetActive(true);
			StartCoroutine(playSoundWithDelay(windowPopUpSound, 0.5f));
			itemIcon.gameObject.SetActive(true);
			titleText.gameObject.SetActive(true);
			descText.gameObject.SetActive(true);
			nextArrow.SetActive(true);
			yield return new WaitForSeconds(1f);
			while (!ready)
			{
				yield return null;
				if (!InputMaster.input.UISelect())
				{
					continue;
				}
				ready = true;
				if (itemsToBeGiven[k] == Inventory.inv.getInvItemId(mapItem))
				{
					TownManager.manage.mapUnlocked = true;
					RenderMap.map.changeMapWindow();
				}
				else if (itemsToBeGiven[k] == Inventory.inv.getInvItemId(journalItem))
				{
					TownManager.manage.unlockJournalAndStartTime();
				}
				else if (itemsToBeGiven[k] == Inventory.inv.getInvItemId(Inventory.inv.moneyItem))
				{
					Inventory.inv.changeWallet(amountToBeGiven[k]);
				}
				else if (Inventory.inv.allItems[itemsToBeGiven[k]].hasFuel)
				{
					if (!Inventory.inv.addItemToInventory(itemsToBeGiven[k], Inventory.inv.allItems[itemsToBeGiven[k]].fuelMax, false))
					{
						MailManager.manage.sendAnInvFullLetter(itemsToBeGiven[k], Inventory.inv.allItems[itemsToBeGiven[k]].fuelMax);
						somethingWasMailed = true;
					}
				}
				else if (!Inventory.inv.addItemToInventory(itemsToBeGiven[k], amountToBeGiven[k], false))
				{
					MailManager.manage.sendAnInvFullLetter(itemsToBeGiven[k], amountToBeGiven[k]);
					somethingWasMailed = true;
				}
				SoundManager.manage.play2DSound(ConversationManager.manage.nextTextSound);
				nextArrow.SetActive(false);
			}
			window.SetActive(false);
			titleText.gameObject.SetActive(false);
			descText.gameObject.SetActive(false);
			itemIcon.gameObject.SetActive(false);
			ready = false;
			yield return new WaitForSeconds(1f);
		}
		giftBox.SetActive(false);
		for (int k = 0; k < recipeLearnt.Count; k++)
		{
			setTextColor(recipeTextColor);
			recipeOrReceivedText.text = "New Crafting Recipe";
			blueprintIcon.SetActive(true);
			titleText.text = Inventory.inv.allItems[recipeLearnt[k]].getInvItemName();
			descText.text = Inventory.inv.allItems[recipeLearnt[k]].getItemDescription(recipeLearnt[k]);
			amountText.text = "";
			itemIcon.sprite = Inventory.inv.allItems[recipeLearnt[k]].getSprite();
			window.SetActive(true);
			StartCoroutine(playSoundWithDelay(recipePopUpSound, 0.5f));
			itemIcon.gameObject.SetActive(true);
			titleText.gameObject.SetActive(true);
			descText.gameObject.SetActive(true);
			nextArrow.SetActive(true);
			yield return new WaitForSeconds(1f);
			while (!ready)
			{
				yield return null;
				if (InputMaster.input.UISelect())
				{
					ready = true;
					CharLevelManager.manage.unlockRecipe(Inventory.inv.allItems[recipeLearnt[k]]);
					SoundManager.manage.play2DSound(ConversationManager.manage.nextTextSound);
					nextArrow.SetActive(false);
				}
			}
			window.SetActive(false);
			titleText.gameObject.SetActive(false);
			descText.gameObject.SetActive(false);
			itemIcon.gameObject.SetActive(false);
			ready = false;
			yield return new WaitForSeconds(delay);
		}
		itemsToBeGiven.Clear();
		amountToBeGiven.Clear();
		recipeLearnt.Clear();
		if (CharLevelManager.manage.unlockWindowOpen)
		{
			CharLevelManager.manage.refreshCurrentTier();
		}
		windowOpen = false;
		if (licenceToBeGiven.Count != 0)
		{
			licenceToBeGiven.Clear();
			LicenceManager.manage.openLicenceWindow();
		}
		if (somethingWasMailed)
		{
			NotificationManager.manage.makeTopNotification("An item was sent to your Mailbox", "Your pockets were full!");
		}
	}
}
