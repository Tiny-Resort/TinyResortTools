using UnityEngine;

public class UIAnimationManager : MonoBehaviour
{
	public static UIAnimationManager manage;

	[Header("Main Window --------------")]
	public AnimationCurve windowsOpenCurve;

	[Header("Invslots --------------")]
	public AnimationCurve invSlotUpdateCurve;

	public AnimationCurve invSlotSelectedCurve;

	public AnimationCurve invSlotDeselectCurve;

	[Header("Button Animators --------------")]
	public AnimationCurve buttonPressCurve;

	public AnimationCurve buttonHoverCurve;

	public AnimationCurve buttonRollOutCurve;

	[Header("Character Animators --------------")]
	public AnimationCurve hairChangeBounce;

	[Header("TextColors --------------")]
	public Color itemNameColor;

	public Color characterNameColor;

	public Color moneyAmountColor;

	public Color pointsAmountColor;

	public Color fadedColor;

	[Header("UITextColors --------------")]
	public Color plantableText;

	public Color consumableText;

	[Header("UI Yes No Colors-----")]
	public Color yesColor;

	public Color noColor;

	[Header("Invslot Types-----")]
	public Sprite baseSlot;

	public Sprite toolSlot;

	public Sprite eatableSlot;

	public Sprite placeableSlot;

	public Sprite bugOrFishSlot;

	public Sprite clothSlot;

	public Sprite vehicleSlot;

	public Sprite relicSlot;

	public Color colourBaseSlot;

	public Color colourToolSlot;

	public Color colourEatableSlot;

	public Color colourPlaceableSlot;

	public Color colourBugOrFishSlot;

	public Color colourClothSlot;

	public Color colourVehicleSlot;

	public Color colourRelicSlot;

	private void Awake()
	{
		manage = this;
	}

	public string getItemColorTag(string inString)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(itemNameColor) + ">" + inString + "</color>";
	}

	public string getCharacterNameTag(string inString)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(characterNameColor) + ">" + inString + "</color>";
	}

	public string moneyAmountColorTag(string inString)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(moneyAmountColor) + ">" + inString + "</color>";
	}

	public string pointsAmountColorTag(string inString)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(pointsAmountColor) + ">" + inString + "</color>";
	}

	public string wrapStringInColor(string text, Color color)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + text + "</color>";
	}

	public string wrapStringInYesColor(string text)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(yesColor) + ">" + text + "</color>";
	}

	public string wrapStringInNoColor(string text)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(noColor) + ">" + text + "</color>";
	}

	public string wrapStringInNotEnoughColor(string text)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(fadedColor) + ">" + text + "</color>";
	}

	public Sprite getSlotSprite(int itemId)
	{
		if ((bool)Inventory.inv.allItems[itemId].spawnPlaceable)
		{
			if ((bool)Inventory.inv.allItems[itemId].spawnPlaceable.GetComponent<Vehicle>())
			{
				return vehicleSlot;
			}
			return placeableSlot;
		}
		if ((bool)Inventory.inv.allItems[itemId].bug || (bool)Inventory.inv.allItems[itemId].fish || (bool)Inventory.inv.allItems[itemId].underwaterCreature)
		{
			return bugOrFishSlot;
		}
		if (Inventory.inv.allItems[itemId].isATool)
		{
			return toolSlot;
		}
		if ((bool)Inventory.inv.allItems[itemId].relic)
		{
			return relicSlot;
		}
		if ((bool)Inventory.inv.allItems[itemId].equipable && Inventory.inv.allItems[itemId].equipable.cloths)
		{
			return clothSlot;
		}
		if ((bool)Inventory.inv.allItems[itemId].consumeable)
		{
			return eatableSlot;
		}
		if (((bool)Inventory.inv.allItems[itemId].placeable && !Inventory.inv.allItems[itemId].burriedPlaceable) || (Inventory.inv.allItems[itemId].placeableTileType > -1 && WorldManager.manageWorld.tileTypes[Inventory.inv.allItems[itemId].placeableTileType].isPath))
		{
			return placeableSlot;
		}
		if ((bool)Inventory.inv.allItems[itemId].itemChange && (bool)Inventory.inv.allItems[itemId].itemChange.changesAndTheirChanger[0].changesWhenComplete && (bool)Inventory.inv.allItems[itemId].itemChange.changesAndTheirChanger[0].changesWhenComplete.consumeable)
		{
			return eatableSlot;
		}
		return baseSlot;
	}

	public Color getSlotColour(int itemId)
	{
		if (itemId > -1)
		{
			if ((bool)Inventory.inv.allItems[itemId].spawnPlaceable)
			{
				if ((bool)Inventory.inv.allItems[itemId].spawnPlaceable.GetComponent<Vehicle>())
				{
					return colourVehicleSlot;
				}
				return colourPlaceableSlot;
			}
			if ((bool)Inventory.inv.allItems[itemId].bug || (bool)Inventory.inv.allItems[itemId].fish || (bool)Inventory.inv.allItems[itemId].underwaterCreature)
			{
				return colourBugOrFishSlot;
			}
			if (Inventory.inv.allItems[itemId].isATool)
			{
				return colourToolSlot;
			}
			if ((bool)Inventory.inv.allItems[itemId].relic)
			{
				return colourRelicSlot;
			}
			if ((bool)Inventory.inv.allItems[itemId].equipable && Inventory.inv.allItems[itemId].equipable.cloths)
			{
				return colourClothSlot;
			}
			if ((bool)Inventory.inv.allItems[itemId].consumeable)
			{
				return colourEatableSlot;
			}
			if (((bool)Inventory.inv.allItems[itemId].placeable && !Inventory.inv.allItems[itemId].burriedPlaceable) || (Inventory.inv.allItems[itemId].placeableTileType > -1 && WorldManager.manageWorld.tileTypes[Inventory.inv.allItems[itemId].placeableTileType].isPath))
			{
				return colourPlaceableSlot;
			}
			if ((bool)Inventory.inv.allItems[itemId].itemChange && (bool)Inventory.inv.allItems[itemId].itemChange.changesAndTheirChanger[0].changesWhenComplete && (bool)Inventory.inv.allItems[itemId].itemChange.changesAndTheirChanger[0].changesWhenComplete.consumeable)
			{
				return colourEatableSlot;
			}
		}
		return colourBaseSlot;
	}
}
