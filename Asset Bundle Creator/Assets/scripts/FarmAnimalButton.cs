using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FarmAnimalButton : MonoBehaviour
{
	public TextMeshProUGUI animalName;

	public Image animalIcon;

	public HeartContainer[] heartContainers;

	private FarmAnimalDetails showingDetails;

	public void setUpButton(FarmAnimalDetails details)
	{
		showingDetails = details;
		animalName.text = details.animalName;
		if (details.animalVariation == -1)
		{
			animalIcon.sprite = AnimalManager.manage.allAnimals[details.animalId].GetComponent<FarmAnimal>().defualtIcon;
		}
		else
		{
			animalIcon.sprite = AnimalManager.manage.allAnimals[details.animalId].GetComponent<AnimalVariation>().variationIcons[details.animalVariation];
		}
		for (int i = 0; i < heartContainers.Length; i++)
		{
			heartContainers[i].updateHealth(details.animalRelationShip);
		}
	}

	public void pressButton()
	{
		FarmAnimalMenu.menu.animalHouseMenu.fillData(showingDetails);
	}

	public int getAnimalIdForSorting()
	{
		if (showingDetails.animalId == 10)
		{
			return 0;
		}
		if (showingDetails.animalId == 9)
		{
			return 0;
		}
		if (showingDetails.animalId == 12)
		{
			return 1;
		}
		if (showingDetails.animalId == 11)
		{
			return 1;
		}
		if (showingDetails.animalId == 17)
		{
			return 2;
		}
		if (showingDetails.animalId == 19)
		{
			return 3;
		}
		return 200;
	}

	public int getAnimalAge()
	{
		return showingDetails.animalAge;
	}
}
