using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FillRecipeSlot : MonoBehaviour
{
	public TextMeshProUGUI itemName;

	public TextMeshProUGUI itemAmounts;

	public Image itemImage;

	public InventoryItem itemInSlot;

	public Image lockedIcon;

	public Image background;

	public Color defualtColor;

	public GameObject canBeCraftedImage;

	public Image itemBackgroundImage;

	private int showingItemId;

	public void fillRecipeSlot(int itemId)
	{
		showingItemId = itemId;
		itemInSlot = Inventory.inv.allItems[itemId];
		itemName.text = Inventory.inv.allItems[itemId].getInvItemName();
		itemImage.sprite = Inventory.inv.allItems[itemId].getSprite();
		if (CraftingManager.manage.canBeCraftedInAVariation(itemId))
		{
			canBeCraftedImage.SetActive(true);
		}
		else
		{
			canBeCraftedImage.SetActive(false);
		}
		if ((bool)itemBackgroundImage)
		{
			itemBackgroundImage.sprite = UIAnimationManager.manage.getSlotSprite(itemId);
		}
	}

	public void updateIfCanBeCrafted()
	{
		if (CraftingManager.manage.canBeCraftedInAVariation(showingItemId))
		{
			canBeCraftedImage.SetActive(true);
		}
		else
		{
			canBeCraftedImage.SetActive(false);
		}
	}

	public void refreshRecipeSlot()
	{
		fillRecipeSlot(Inventory.inv.getInvItemId(itemInSlot));
	}

	public void fillRecipeSlotWithCraftAmount(int itemId, int amountBeingCrafted)
	{
		itemInSlot = Inventory.inv.allItems[itemId];
		itemName.text = Inventory.inv.allItems[itemId].getInvItemName();
		itemImage.sprite = Inventory.inv.allItems[itemId].getSprite();
		itemAmounts.text = amountBeingCrafted.ToString() ?? "";
		if ((bool)itemBackgroundImage)
		{
			itemBackgroundImage.sprite = UIAnimationManager.manage.getSlotSprite(itemId);
		}
	}

	public void fillRecipeSlotWithAmounts(int itemId, int amountHave, int amountNeed)
	{
		if (itemId >= 0)
		{
			itemInSlot = Inventory.inv.allItems[itemId];
			itemName.text = Inventory.inv.allItems[itemId].getInvItemName();
			itemImage.sprite = Inventory.inv.allItems[itemId].getSprite();
			if (amountHave < amountNeed)
			{
				itemAmounts.text = UIAnimationManager.manage.wrapStringInNotEnoughColor(amountHave.ToString() ?? "") + " / " + amountNeed;
			}
			else
			{
				itemAmounts.text = "<b>" + amountHave + "</b> / " + amountNeed;
			}
			if ((bool)itemBackgroundImage)
			{
				itemBackgroundImage.sprite = UIAnimationManager.manage.getSlotSprite(itemId);
			}
		}
	}

	public void fillRecipeSlotForQuestReward(int itemId, int stackAmount)
	{
		itemInSlot = Inventory.inv.allItems[itemId];
		itemName.text = Inventory.inv.allItems[itemId].getInvItemName();
		itemImage.sprite = Inventory.inv.allItems[itemId].getSprite();
		itemAmounts.text = stackAmount.ToString("n0");
		if ((bool)itemBackgroundImage)
		{
			itemBackgroundImage.sprite = UIAnimationManager.manage.getSlotSprite(itemId);
		}
	}

	public void fillRecipeSlotForCraftUnlock(int itemId, bool isUnlocked)
	{
		itemInSlot = Inventory.inv.allItems[itemId];
		itemName.text = Inventory.inv.allItems[itemId].getInvItemName();
		itemImage.sprite = Inventory.inv.allItems[itemId].getSprite();
		if (!isUnlocked)
		{
			lockedIcon.enabled = true;
			background.color = Color.Lerp(defualtColor, Color.black, 0.25f);
			itemImage.color = Color.Lerp(Color.white, Color.black, 0.25f);
		}
		else
		{
			background.color = defualtColor;
			lockedIcon.enabled = false;
			itemImage.color = Color.white;
		}
		if ((bool)itemBackgroundImage)
		{
			itemBackgroundImage.sprite = UIAnimationManager.manage.getSlotSprite(itemId);
		}
	}

	public void fillDeedBuySlot(int itemId)
	{
		itemInSlot = Inventory.inv.allItems[itemId];
		itemName.text = Inventory.inv.allItems[itemId].getInvItemName();
		itemImage.sprite = Inventory.inv.allItems[itemId].getSprite();
		if ((bool)itemBackgroundImage)
		{
			itemBackgroundImage.sprite = UIAnimationManager.manage.getSlotSprite(itemId);
		}
	}

	private void OnEnable()
	{
		Inventory.inv.buttonsToSnapTo.Add(GetComponent<RectTransform>());
	}

	private void OnDisable()
	{
		Inventory.inv.buttonsToSnapTo.Remove(GetComponent<RectTransform>());
	}

	public void setSlotFull()
	{
		itemBackgroundImage.color = Color.Lerp(Color.white, Color.black, 0.35f);
		itemImage.color = Color.Lerp(Color.white, Color.black, 0.35f);
		itemAmounts.text = "";
	}

	public void setSlotEmpty()
	{
		itemBackgroundImage.color = Color.white;
		itemImage.color = Color.white;
	}
}
