using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FarmAnimalMenu : MonoBehaviour
{
	public static FarmAnimalMenu menu;

	public GameObject animalBoxPrefab;

	public GameObject farmAnimalMenu;

	public bool farmAnimalMenuOpen;

	public GameObject nameAndBuyWindow;

	public TMP_InputField newAnimalName;

	public Image buyAnimalImage;

	public TextMeshProUGUI animalCostText;

	public Transform selectorTrans;

	public GameObject selectorTransPrefab;

	private int animalCurrentlyBuying = 9;

	private int setAnimalVariation = -1;

	public AnimalHouseMenu animalHouseMenu;

	public Transform spawnFarmAnimalPos;

	[Header("Farm Animal journal stuff")]
	public GameObject farmAnimalPage;

	public GameObject farmAnimalButton;

	public Transform buttonSpawnWindow;

	public FarmAnimalBoxChecker checkForAnimalPos;

	private List<FarmAnimalButton> animalButtons = new List<FarmAnimalButton>();

	private string[] randomNames = new string[0];

	private void Awake()
	{
		menu = this;
	}

	private void Start()
	{
		selectorTrans = Object.Instantiate(selectorTransPrefab).GetComponent<Transform>();
		setUpRandomNames();
	}

	public void openAnimalMenu(int animalId, int variationNo = -1)
	{
		farmAnimalMenuOpen = true;
		farmAnimalMenu.SetActive(true);
		nameAndBuyWindow.SetActive(true);
		Inventory.inv.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closed = false;
		animalCurrentlyBuying = animalId;
		newAnimalName.text = getRandomName();
		buyAnimalImage.sprite = AnimalManager.manage.allAnimals[animalId].GetComponent<FarmAnimal>().defualtIcon;
		animalCostText.text = "<sprite=11> " + AnimalManager.manage.allAnimals[animalId].GetComponent<FarmAnimal>().baseValue;
		if ((bool)AnimalManager.manage.allAnimals[animalId].hasVariation && variationNo == -1)
		{
			setAnimalVariation = AnimalManager.manage.allAnimals[animalId].getRandomVariationNo();
		}
	}

	public bool checkIfAnimalBoxIsInShop()
	{
		checkForAnimalPos.transform.position = NetworkMapSharer.share.localChar.transform.position;
		return checkForAnimalPos.checkIfAnimalIsInBuilding();
	}

	public void moveCameraToAnimalBox()
	{
		StartCoroutine(moveCameraToBoxWhileInConvo());
	}

	private IEnumerator moveCameraToBoxWhileInConvo()
	{
		CameraController.control.setFollowTransform(spawnFarmAnimalPos);
		yield return null;
		while (ConversationManager.manage.inConversation)
		{
			yield return null;
		}
		CameraController.control.setFollowTransform(NetworkMapSharer.share.localChar.transform);
	}

	public void openJournalTab()
	{
		farmAnimalMenuOpen = true;
		farmAnimalPage.gameObject.SetActive(true);
		refreshJournalButtons();
	}

	public void refreshJournalButtons()
	{
		for (int i = 0; i < animalButtons.Count; i++)
		{
			Object.Destroy(animalButtons[i].gameObject);
		}
		animalButtons.Clear();
		for (int j = 0; j < FarmAnimalManager.manage.farmAnimalDetails.Count; j++)
		{
			FarmAnimalButton component = Object.Instantiate(farmAnimalButton, buttonSpawnWindow).GetComponent<FarmAnimalButton>();
			component.setUpButton(FarmAnimalManager.manage.farmAnimalDetails[j]);
			animalButtons.Add(component);
		}
		animalButtons.Sort(sortButtons);
		for (int k = 0; k < animalButtons.Count; k++)
		{
			animalButtons[k].transform.SetSiblingIndex(k);
		}
	}

	public int sortButtons(FarmAnimalButton a, FarmAnimalButton b)
	{
		if (a.getAnimalIdForSorting() < b.getAnimalIdForSorting())
		{
			return -1;
		}
		if (a.getAnimalIdForSorting() > b.getAnimalIdForSorting())
		{
			return 1;
		}
		if (a.getAnimalIdForSorting() == b.getAnimalIdForSorting())
		{
			if (a.getAnimalAge() < b.getAnimalAge())
			{
				return -1;
			}
			if (a.getAnimalAge() > b.getAnimalAge())
			{
				return 1;
			}
		}
		return 0;
	}

	public void showAnimalsOnly(int animalSortId)
	{
		for (int i = 0; i < animalButtons.Count; i++)
		{
			if (animalButtons[i].getAnimalIdForSorting() == animalSortId || animalSortId == -1)
			{
				animalButtons[i].gameObject.SetActive(true);
			}
			else
			{
				animalButtons[i].gameObject.SetActive(false);
			}
		}
	}

	public void closeJournalTab()
	{
		farmAnimalMenuOpen = false;
		farmAnimalPage.gameObject.SetActive(false);
	}

	public void closeAnimalMenu()
	{
		farmAnimalMenuOpen = false;
		farmAnimalMenu.SetActive(false);
		nameAndBuyWindow.SetActive(false);
		Inventory.inv.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	public void cancleNameAndBuyButton()
	{
		closeAnimalMenu();
		ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, GiveNPC.give.cancleAnimalBuying);
	}

	public string getLastAnimalName()
	{
		return newAnimalName.text;
	}

	public void buyAnimalAndName()
	{
		if (newAnimalName.text != "")
		{
			Inventory.inv.changeWallet(-AnimalManager.manage.allAnimals[animalCurrentlyBuying].GetComponent<FarmAnimal>().baseValue);
			NPCManager.manage.npcStatus[NPCManager.manage.getVendorNPC(NPCSchedual.Locations.Animal_Shop).myId.NPCNo].moneySpentAtStore += AnimalManager.manage.allAnimals[animalCurrentlyBuying].GetComponent<FarmAnimal>().baseValue;
			if (AnimalManager.manage.allAnimals[animalCurrentlyBuying].GetComponent<FarmAnimal>().baseValue > 0)
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, GiveNPC.give.onBuyAnimalConvo);
				GiveNPC.give.dropToBuy.buyTheItem();
				DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.BuyAnAnimal);
				NetworkMapSharer.share.localChar.CmdSpawnAnimalBox(animalCurrentlyBuying, setAnimalVariation, newAnimalName.text);
				moveCameraToAnimalBox();
			}
			closeAnimalMenu();
		}
		else
		{
			SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
		}
	}

	public void closeNamePopUp()
	{
		cancleNameAndBuyButton();
	}

	public string getRandomName()
	{
		return randomNames[Random.Range(0, randomNames.Length)];
	}

	public void setUpRandomNames()
	{
		if (randomNames.Length == 0)
		{
			randomNames = new string[100]
			{
				"Rowan", "Tamia", "Janae", "Sloane", "Mareli", "Beatrice", "Marlie", "Lydia", "Macie", "Teresa",
				"Jaiden", "Karen", "Kaiya", "Rebecca", "Sophie", "Rachael", "Marin", "Karma", "Armani", "Anabella",
				"Eliana", "Lesly", "Ayanna", "Aileen", "Tania", "Cara", "Shiloh", "Alivia", "Madalynn", "Harmony",
				"Kimberly", "Kaila", "Ashlyn", "Julissa", "Haven", "Adelaide", "Emerson", "Kirsten", "Lia", "Madilyn",
				"Hazel", "Kinsley", "Valery", "Daniela", "Helen", "Nadia", "Macey", "Lilianna", "Abigayle", "Tamara",
				"Lainey", "Phoenix", "Rory", "Theresa", "Kamila", "Lilly", "Ryleigh", "Meredith", "Aspen", "Kali",
				"Ashleigh", "Andrea", "Stephany", "Scarlett", "Rhianna", "Shania", "Natalya", "Alani", "Samara", "Carley",
				"Layla", "Karla", "Alisson", "Kennedy", "Deanna", "Justine", "Daniella", "Hillary", "Gabriella", "Tia",
				"Heidy", "Kaliyah", "Jasmine", "Laurel", "Karley", "Elizabeth", "Rebekah", "Katrina", "Riley", "Gracie",
				"Lily", "Brianna", "Mya", "Savanna", "Delaney", "Amiyah", "Isla", "Estrella", "Belinda", "Sandra"
			};
		}
	}
}
