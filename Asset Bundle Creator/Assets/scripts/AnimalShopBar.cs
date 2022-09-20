using UnityEngine;
using UnityEngine.UI;

public class AnimalShopBar : MonoBehaviour
{
	public int animalId = 9;

	public Sprite animalSprite;

	public Image animalIcon;

	public Image houseIcon;

	public Text houseAmountText;

	public Text moneyAmountText;

	public Text animalNameText;

	public GameObject buyButton;

	private Color defaultTextColor;

	private Color defaultHouseIconColor;

	private FarmAnimal myFarmAnimalDetails;

	public void Awake()
	{
		defaultHouseIconColor = houseIcon.color;
		defaultTextColor = houseAmountText.color;
		myFarmAnimalDetails = AnimalManager.manage.allAnimals[animalId].GetComponent<FarmAnimal>();
	}

	public void updateBarToShow(int avaliableHouses)
	{
		houseAmountText.text = avaliableHouses.ToString() ?? "";
		if (avaliableHouses == 0)
		{
			houseAmountText.color = Color.red;
			houseIcon.color = Color.red;
		}
		else
		{
			houseAmountText.color = defaultTextColor;
			houseIcon.color = defaultHouseIconColor;
		}
		moneyAmountText.text = "<sprite=11>" + myFarmAnimalDetails.baseValue;
		if ((bool)myFarmAnimalDetails.growsInto)
		{
			animalNameText.text = myFarmAnimalDetails.growsInto.GetComponent<AnimalAI>().animalName ?? "";
		}
		else
		{
			animalNameText.text = AnimalManager.manage.allAnimals[animalId].animalName ?? "";
		}
		if (Inventory.inv.wallet < myFarmAnimalDetails.baseValue)
		{
			moneyAmountText.color = Color.red;
		}
		else
		{
			moneyAmountText.color = defaultTextColor;
		}
		if (avaliableHouses > 0 && Inventory.inv.wallet >= AnimalManager.manage.allAnimals[animalId].GetComponent<FarmAnimal>().baseValue)
		{
			buyButton.SetActive(true);
		}
		else
		{
			buyButton.SetActive(false);
		}
	}
}
