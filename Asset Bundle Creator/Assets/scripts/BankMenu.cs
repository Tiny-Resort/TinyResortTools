using TMPro;
using UnityEngine;

public class BankMenu : MonoBehaviour
{
	public static BankMenu menu;

	public GameObject window;

	public GameObject amountButtons;

	public GameObject confirmConversionWindow;

	public TextMeshProUGUI AccountTypeTitle;

	public TextMeshProUGUI amountInAccount;

	public TextMeshProUGUI amountChanging;

	public TextMeshProUGUI titleText;

	public TextMeshProUGUI dinkConversionTotal;

	public TextMeshProUGUI permitPointConversionTotal;

	public bool bankOpen;

	public int accountBalance;

	public int difference;

	private bool depositing = true;

	private bool donating;

	public bool converting;

	private string amount = "";

	public InvButton closeWindowButton;

	private void Awake()
	{
		menu = this;
	}

	public void open()
	{
		AccountTypeTitle.text = "Account Balance";
		window.gameObject.SetActive(true);
		bankOpen = true;
		Inventory.inv.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closed = false;
		updateAccountAmounts();
		converting = false;
	}

	public void openAsDonations()
	{
		donating = true;
		open();
		clear();
		amountButtons.gameObject.SetActive(true);
		AccountTypeTitle.text = "Town Debt";
		titleText.text = "Donate";
	}

	public void close()
	{
		window.gameObject.SetActive(false);
		bankOpen = false;
		difference = 0;
		amount = "";
		donating = false;
		converting = false;
		amountButtons.SetActive(false);
		checkBalanceForMilestones();
		Inventory.inv.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	public void withdrawButton()
	{
		depositing = false;
		titleText.text = "Withdraw";
		checkBalanceForMilestones();
		clear();
	}

	public void depositButton()
	{
		depositing = true;
		titleText.text = "Deposit";
		checkBalanceForMilestones();
		clear();
	}

	public void convertButton()
	{
		converting = true;
		titleText.text = "Convert [<sprite=11> 500 for <sprite=15> 1]";
		clear();
	}

	public void confirmButton()
	{
		if (converting)
		{
			confirmConversionWindow.SetActive(true);
			difference = Mathf.RoundToInt((float)difference / 500f) * 500;
			dinkConversionTotal.text = difference.ToString("n0");
			permitPointConversionTotal.text = Mathf.RoundToInt((float)difference / 500f).ToString("n0");
			return;
		}
		if (donating)
		{
			difference = Mathf.Clamp(difference, 0, Inventory.inv.wallet);
			difference = Mathf.Clamp(difference, 0, NetworkMapSharer.share.townDebt);
			amountInAccount.text = NetworkMapSharer.share.townDebt.ToString("n0");
			amountChanging.text = difference.ToString("n0");
			NetworkMapSharer.share.localChar.CmdPayTownDebt(difference);
			Inventory.inv.changeWallet(-difference, false);
			close();
			if (NetworkMapSharer.share.townDebt == 0)
			{
				ConversationManager.manage.talkToNPC(NPCManager.manage.sign, TownManager.manage.debtComplete);
			}
		}
		else if (depositing)
		{
			accountBalance += difference;
			Inventory.inv.changeWallet(-difference, false);
		}
		else
		{
			accountBalance -= difference;
			Inventory.inv.changeWallet(difference, false);
		}
		updateAccountAmounts();
		checkBalanceForMilestones();
		clear();
	}

	public void confirmConversionButton()
	{
		MonoBehaviour.print("Giving Conversion");
		accountBalance -= difference;
		PermitPointsManager.manage.addPoints((int)((float)difference / 500f));
		confirmConversionWindow.SetActive(false);
		Inventory.inv.setAsActiveCloseButton(closeWindowButton);
		converting = false;
		updateAccountAmounts();
		checkBalanceForMilestones();
		clear();
	}

	public void cancelConversionButton()
	{
		confirmConversionWindow.SetActive(false);
		Inventory.inv.setAsActiveCloseButton(closeWindowButton);
	}

	public void cancelButton()
	{
		if (donating)
		{
			close();
		}
		converting = false;
	}

	public void toAccountButton(int addToDifference)
	{
		amount += addToDifference;
		difference = int.Parse(amount);
		if (converting)
		{
			difference = Mathf.Clamp(difference, 0, accountBalance);
		}
		else if (donating)
		{
			difference = Mathf.Clamp(difference, 0, Inventory.inv.wallet);
			difference = Mathf.Clamp(difference, 0, NetworkMapSharer.share.townDebt);
		}
		else if (depositing)
		{
			difference = Mathf.Clamp(difference, 0, Inventory.inv.wallet);
		}
		else
		{
			difference = Mathf.Clamp(difference, 0, accountBalance);
		}
		amount = difference.ToString() ?? "";
		updateAccountAmounts();
	}

	public void clear()
	{
		amount = "";
		difference = 0;
		updateAccountAmounts();
	}

	public void updateAccountAmounts()
	{
		if (!donating)
		{
			amountInAccount.text = accountBalance.ToString("n0");
			amountChanging.text = difference.ToString("n0");
		}
		else
		{
			amountInAccount.text = NetworkMapSharer.share.townDebt.ToString("n0");
			amountChanging.text = difference.ToString("n0");
		}
	}

	public void addDailyInterest()
	{
		if (NetworkMapSharer.share.isServer)
		{
			accountBalance += Mathf.RoundToInt((float)accountBalance / 100f * 8f / 112f);
		}
	}

	public void checkBalanceForMilestones()
	{
		if (accountBalance >= 1000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 0)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 2000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 1)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 3000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 2)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 4000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 3)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 5000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 4)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 6000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 5)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 6000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 6)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 7000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 7)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 8000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 8)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 9000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 9)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
		if (accountBalance >= 10000000 && MilestoneManager.manage.getMilestonePointsInt(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank) == 10)
		{
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.DepositMoneyIntoBank);
		}
	}
}
