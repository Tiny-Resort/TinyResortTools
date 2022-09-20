using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimalHouseMenu : MonoBehaviour
{
	public TextMeshProUGUI shelterTitleText;

	public Image animalIcon;

	public HeartContainer[] hearts;

	public TextMeshProUGUI sellAmount;

	public TextMeshProUGUI sellButtonText;

	private FarmAnimalDetails showingAnimal;

	public GameObject sellButton;

	public GameObject moveButton;

	public GameObject confirmPopUp;

	public Image confirmImage;

	public TextMeshProUGUI confirmText;

	public TextMeshProUGUI animalAge;

	public GameObject openWindow;

	public TextMeshProUGUI pettedText;

	public TextMeshProUGUI shelterText;

	public TextMeshProUGUI eatenText;

	private void OnEnable()
	{
		openWindow.SetActive(false);
		confirmPopUp.SetActive(false);
	}

	public void openConfirm()
	{
		confirmPopUp.SetActive(true);
		confirmText.text = "Sell " + showingAnimal.animalName + " for <sprite=11>" + getSellValue().ToString("n0") + "?";
	}

	public void sellAnimalInHouse()
	{
		int sellValue = getSellValue();
		Inventory.inv.changeWallet(sellValue);
		showingAnimal.sell();
		FarmAnimalMenu.menu.refreshJournalButtons();
		confirmPopUp.SetActive(false);
		openWindow.SetActive(false);
	}

	public int getSellValue()
	{
		int num = (int)((float)AnimalManager.manage.allAnimals[showingAnimal.animalId].GetComponent<FarmAnimal>().baseValue / 2f);
		int num2 = 3;
		if (showingAnimal.animalRelationShip > 50)
		{
			num2 = 6;
		}
		else if (showingAnimal.animalRelationShip > 85)
		{
			num2 = 8;
		}
		return num + (int)((float)num * ((float)num2 * ((float)showingAnimal.animalRelationShip / 100f)));
	}

	public void fillData(FarmAnimalDetails animalToShow)
	{
		confirmPopUp.SetActive(false);
		openWindow.SetActive(false);
		openWindow.SetActive(true);
		showingAnimal = animalToShow;
		shelterTitleText.text = showingAnimal.animalName;
		animalIcon.enabled = true;
		if (animalToShow.hasEaten)
		{
			eatenText.text = eatenText.text.Replace("<sprite=16>", "<sprite=17>");
		}
		else
		{
			eatenText.text = eatenText.text.Replace("<sprite=17>", "<sprite=16>");
		}
		if (animalToShow.hasHouse())
		{
			shelterText.text = shelterText.text.Replace("<sprite=16>", "<sprite=17>");
		}
		else
		{
			shelterText.text = shelterText.text.Replace("<sprite=17>", "<sprite=16>");
		}
		if (animalToShow.hasBeenPatted)
		{
			pettedText.text = pettedText.text.Replace("<sprite=16>", "<sprite=17>");
		}
		else
		{
			pettedText.text = pettedText.text.Replace("<sprite=17>", "<sprite=16>");
		}
		HeartContainer[] array = hearts;
		foreach (HeartContainer obj in array)
		{
			obj.updateHealth(showingAnimal.animalRelationShip);
			obj.gameObject.SetActive(true);
		}
		if (showingAnimal.animalVariation == -1)
		{
			animalIcon.sprite = AnimalManager.manage.allAnimals[showingAnimal.animalId].GetComponent<FarmAnimal>().defualtIcon;
		}
		else
		{
			animalIcon.sprite = AnimalManager.manage.allAnimals[showingAnimal.animalId].GetComponent<AnimalVariation>().variationIcons[showingAnimal.animalVariation];
		}
		if (showingAnimal.animalAge >= 28)
		{
			if (showingAnimal.animalAge / 28 / 4 >= 1)
			{
				animalAge.text = showingAnimal.animalAge / 28 + " Year";
				if (showingAnimal.animalAge / 28 / 4 > 1)
				{
					animalAge.text += "s";
				}
			}
			else
			{
				animalAge.text = showingAnimal.animalAge / 28 + " Month";
				if (showingAnimal.animalAge / 28 > 1)
				{
					animalAge.text += "s";
				}
			}
		}
		else
		{
			animalAge.text = showingAnimal.animalAge + " Day";
			if (showingAnimal.animalAge > 1)
			{
				animalAge.text += "s";
			}
		}
		sellAmount.text = "<sprite=11>" + getSellValue().ToString("n0");
		sellButtonText.text = "SELL " + showingAnimal.animalName;
		sellButton.SetActive(true);
		if (!NetworkMapSharer.share.isServer)
		{
			sellButton.SetActive(false);
		}
	}
}
