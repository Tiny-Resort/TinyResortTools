using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemDescription : MonoBehaviour
{
	[Header("Weapon Details")]
	public GameObject weaponDamage;

	public TextMeshProUGUI weaponDamageText;

	[Header("Farming Details")]
	public GameObject farmingDetails;

	public TextMeshProUGUI farmingDetailText;

	[Header("ConsumableDetails")]
	public GameObject consumeableWindow;

	public GameObject health;

	public TextMeshProUGUI healthText;

	public GameObject stamina;

	public TextMeshProUGUI staminaText;

	public GameObject healthPlus;

	public TextMeshProUGUI healthPlusText;

	public GameObject staminaPlus;

	public TextMeshProUGUI staminaPlusText;

	[Header("Buff Details")]
	public GameObject buffwindow;

	public GameObject[] buffObjects;

	public Image[] buffLevel;

	public TextMeshProUGUI[] buffSeconds;

	public Sprite buffLevel2;

	public Sprite buffLevel3;

	[Header("WindMill And Sprinklers")]
	public GameObject windmillCompatible;

	public GameObject reachTiles;

	public TextMeshProUGUI reachTileText;

	public void fillItemDescription(InventoryItem item)
	{
		if (item == null)
		{
			return;
		}
		if ((bool)item.itemPrefab && (bool)item.itemPrefab.GetComponent<MeleeAttacks>() && item.itemPrefab.GetComponent<MeleeAttacks>().isWeapon)
		{
			weaponDamage.SetActive(true);
			weaponDamageText.text = item.weaponDamage.ToString();
		}
		else
		{
			weaponDamage.SetActive(false);
		}
		if ((bool)item.placeable && (bool)item.placeable.tileObjectGrowthStages && item.placeable.tileObjectGrowthStages.isPlant)
		{
			farmingDetails.SetActive(true);
			if (item.placeable.tileObjectGrowthStages.needsTilledSoil)
			{
				string text = "";
				if (item.placeable.tileObjectGrowthStages.growsInSummer && item.placeable.tileObjectGrowthStages.growsInWinter && item.placeable.tileObjectGrowthStages.growsInSpring && item.placeable.tileObjectGrowthStages.growsInAutum)
				{
					text = "All year";
				}
				else
				{
					if (item.placeable.tileObjectGrowthStages.growsInSummer)
					{
						text += "Summer";
					}
					if (item.placeable.tileObjectGrowthStages.growsInAutum)
					{
						if (text != "")
						{
							text += " & ";
						}
						text += "Autum";
					}
					if (item.placeable.tileObjectGrowthStages.growsInWinter)
					{
						if (text != "")
						{
							text += " & ";
						}
						text += "Winter";
					}
					if (item.placeable.tileObjectGrowthStages.growsInSpring)
					{
						if (text != "")
						{
							text += " & ";
						}
						text += "Spring";
					}
				}
				farmingDetailText.text = text;
			}
			else
			{
				farmingDetailText.text = "Bury";
			}
		}
		else
		{
			farmingDetails.SetActive(false);
		}
		if ((bool)item.consumeable)
		{
			consumeableWindow.SetActive(true);
			if (item.consumeable.healthGain != 0)
			{
				health.SetActive(true);
				healthText.text = item.consumeable.healthGain.ToString();
			}
			else
			{
				health.SetActive(false);
			}
			if (item.consumeable.staminaGain != 0f)
			{
				stamina.SetActive(true);
				staminaText.text = item.consumeable.staminaGain.ToString();
			}
			else
			{
				stamina.SetActive(false);
			}
			if (item.consumeable.givesTempPoints)
			{
				if (item.consumeable.tempHealthGain != 0)
				{
					healthPlus.SetActive(true);
					healthPlusText.text = item.consumeable.tempHealthGain.ToString();
				}
				else
				{
					healthPlus.SetActive(false);
				}
				if (item.consumeable.tempStaminaGain != 0f)
				{
					staminaPlus.SetActive(true);
					staminaPlusText.text = item.consumeable.tempStaminaGain.ToString();
				}
				else
				{
					staminaPlus.SetActive(false);
				}
			}
			else
			{
				healthPlus.SetActive(false);
				staminaPlus.SetActive(false);
			}
			if (item.consumeable.myBuffs.Length != 0)
			{
				buffwindow.SetActive(true);
				for (int i = 0; i < buffObjects.Length; i++)
				{
					buffObjects[i].SetActive(false);
				}
				for (int j = 0; j < item.consumeable.myBuffs.Length; j++)
				{
					buffObjects[(int)item.consumeable.myBuffs[j].myType].SetActive(true);
					buffLevel[(int)item.consumeable.myBuffs[j].myType].enabled = item.consumeable.myBuffs[j].myLevel > 1;
					if (item.consumeable.myBuffs[j].myLevel == 2)
					{
						buffLevel[(int)item.consumeable.myBuffs[j].myType].sprite = buffLevel2;
					}
					else
					{
						buffLevel[(int)item.consumeable.myBuffs[j].myType].sprite = buffLevel3;
					}
					if (item.consumeable.myBuffs[j].seconds > 60)
					{
						buffSeconds[(int)item.consumeable.myBuffs[j].myType].text = Mathf.RoundToInt(item.consumeable.myBuffs[j].seconds / 60) + "m";
					}
					else
					{
						buffSeconds[(int)item.consumeable.myBuffs[j].myType].text = item.consumeable.myBuffs[j].seconds + "s";
					}
				}
			}
			else
			{
				buffwindow.SetActive(false);
			}
		}
		else
		{
			consumeableWindow.SetActive(false);
			buffwindow.SetActive(false);
		}
		windmillCompatible.SetActive((bool)item.placeable && (bool)item.placeable.tileObjectItemChanger && item.placeable.tileObjectItemChanger.useWindMill);
		if (((bool)item.placeable && (bool)item.placeable.sprinklerTile) || ((bool)item.placeable && item.placeable.tileObjectId == 16))
		{
			reachTiles.SetActive(true);
			if (item.placeable.tileObjectId == 16)
			{
				reachTileText.text = "Speeds up certain production devices for up to 12 tiles";
			}
			else if (!item.placeable.sprinklerTile.isTank && !item.placeable.sprinklerTile.isSilo)
			{
				reachTileText.text = "Reaches " + item.placeable.sprinklerTile.verticlSize + " tiles out.\n<color=red>Requires Water Tank</color>";
			}
			else if (item.placeable.sprinklerTile.isTank)
			{
				reachTileText.text = "Provides water to sprinklers " + item.placeable.sprinklerTile.verticlSize + " tiles out.";
			}
			else if (item.placeable.sprinklerTile.isSilo)
			{
				reachTileText.text = "Fills animal feeders " + item.placeable.sprinklerTile.verticlSize + " tiles out.\n<color=red>Requires Animal Food</color>";
			}
		}
		else
		{
			reachTiles.SetActive(false);
		}
	}
}
