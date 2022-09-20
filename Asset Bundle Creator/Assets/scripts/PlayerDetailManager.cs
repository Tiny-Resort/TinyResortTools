using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDetailManager : MonoBehaviour
{
	public static PlayerDetailManager manage;

	public bool windowOpen;

	public GameObject tabsPage;

	public GameObject licenceWindow;

	public GameObject levelWindow;

	public SkillBox[] levelBoxes;

	public RawImage playerImage;

	public TextMeshProUGUI nameTextBox;

	public TextMeshProUGUI islandNameTextBox;

	public TextMeshProUGUI moneyTextBox;

	public TextMeshProUGUI bankAccountTextBox;

	public TextMeshProUGUI residentAgeText;

	private void Awake()
	{
		manage = this;
	}

	public void openTab()
	{
		windowOpen = true;
		switchToLevelWindow();
		tabsPage.SetActive(true);
	}

	public void closeTab()
	{
		windowOpen = false;
		tabsPage.SetActive(false);
	}

	public void switchToLevelWindow()
	{
		levelWindow.SetActive(true);
		licenceWindow.SetActive(false);
		nameTextBox.text = Inventory.inv.playerName;
		islandNameTextBox.text = Inventory.inv.islandName;
		moneyTextBox.text = "<sprite=11>" + Inventory.inv.wallet.ToString("n0");
		if (BankMenu.menu.accountBalance > 0)
		{
			bankAccountTextBox.text = "<sprite=11>" + BankMenu.menu.accountBalance.ToString("n0");
		}
		else
		{
			bankAccountTextBox.text = "";
		}
		residentAgeText.text = "Resident for: ";
		TextMeshProUGUI textMeshProUGUI = residentAgeText;
		textMeshProUGUI.text = textMeshProUGUI.text + (WorldManager.manageWorld.day + (WorldManager.manageWorld.week - 1) * 7) + " days";
		if (WorldManager.manageWorld.month > 1)
		{
			TextMeshProUGUI textMeshProUGUI2 = residentAgeText;
			textMeshProUGUI2.text = textMeshProUGUI2.text + ", " + WorldManager.manageWorld.month + " months";
		}
		if (WorldManager.manageWorld.year > 1)
		{
			TextMeshProUGUI textMeshProUGUI3 = residentAgeText;
			textMeshProUGUI3.text = textMeshProUGUI3.text + ", " + WorldManager.manageWorld.year + " years";
		}
		playerImage.texture = CharacterCreatorScript.create.loadSlotPhoto();
		updateLevelDetails();
	}

	public void switchToLicenceWindow()
	{
		levelWindow.SetActive(false);
		licenceWindow.SetActive(true);
		LicenceManager.manage.refreshCharacterJournalTab();
	}

	private void updateLevelDetails()
	{
		for (int i = 0; i < levelBoxes.Length; i++)
		{
			levelBoxes[i].setToCurrent(i, CharLevelManager.manage.currentXp[i]);
		}
	}
}
