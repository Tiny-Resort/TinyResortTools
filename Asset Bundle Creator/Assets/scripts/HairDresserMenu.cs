using System.Collections;
using UnityEngine;

public class HairDresserMenu : MonoBehaviour
{
	public static HairDresserMenu menu;

	public GameObject hairDresserMenu;

	public bool hairMenuOpen;

	private int startingHair;

	private int showingHair;

	private int startingColour;

	private int showingColour;

	private bool selectingColor;

	private WaitForSeconds wait = new WaitForSeconds(0.15f);

	private void Awake()
	{
		menu = this;
	}

	public void openHairCutMenu(bool colorSelector)
	{
		hairMenuOpen = true;
		hairDresserMenu.SetActive(true);
		startingHair = NetworkMapSharer.share.localChar.myEquip.hairId;
		showingHair = startingHair;
		startingColour = NetworkMapSharer.share.localChar.myEquip.hairColor;
		showingColour = startingColour;
		selectingColor = colorSelector;
		Inventory.inv.checkIfWindowIsNeeded();
		NetworkMapSharer.share.localChar.myEquip.CmdChangeHeadId(-1);
		NetworkMapSharer.share.localChar.myEquip.CmdChangeFaceId(-1);
		CameraController.control.zoomInOnCharForChair();
	}

	public void closeHairCutMenu()
	{
		hairDresserMenu.SetActive(false);
		hairMenuOpen = false;
		NetworkMapSharer.share.localChar.myEquip.CmdChangeHeadId(EquipWindow.equip.hatSlot.itemNo);
		NetworkMapSharer.share.localChar.myEquip.CmdChangeFaceId(EquipWindow.equip.faceSlot.itemNo);
		NetworkMapSharer.share.localChar.myPickUp.pickUp();
		ConversationManager.manage.checkIfYouWereTalkingToNPCAndStopTalkingAfterMenuCloses();
		Inventory.inv.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	public void lastSelection()
	{
		if (!selectingColor)
		{
			showingHair--;
			clampShowingHair();
		}
		else
		{
			showingColour--;
			clampShowingHair();
		}
		NetworkMapSharer.share.localChar.myEquip.CmdMakeHairDresserSpin();
		StartCoroutine("delayHairChange");
	}

	public void nextSelection()
	{
		if (!selectingColor)
		{
			showingHair++;
			clampShowingHair();
		}
		else
		{
			showingColour++;
			clampShowingHair();
		}
		NetworkMapSharer.share.localChar.myEquip.CmdMakeHairDresserSpin();
		StartCoroutine("delayHairChange");
	}

	public void selectAndChargeForHairCut()
	{
		int num = 5000;
		if (selectingColor)
		{
			num = (int)((float)num * 1.5f);
		}
		if (Inventory.inv.wallet >= num)
		{
			Inventory.inv.changeWallet(-num);
			Inventory.inv.playerHair = showingHair;
			Inventory.inv.playerHairColour = showingColour;
			DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.GetAHairCut);
		}
		else
		{
			NetworkMapSharer.share.localChar.myEquip.CmdChangeHairId(showingHair);
			NetworkMapSharer.share.localChar.myEquip.CmdChangeHairColour(showingColour);
		}
		closeHairCutMenu();
	}

	public void cancleHairCut()
	{
		NetworkMapSharer.share.localChar.myEquip.CmdChangeHairId(startingHair);
		NetworkMapSharer.share.localChar.myEquip.CmdChangeHairColour(startingColour);
		closeHairCutMenu();
	}

	private void clampShowingHair()
	{
		if (showingHair < 0)
		{
			showingHair = CharacterCreatorScript.create.allHairStyles.Length - 1;
		}
		if (showingHair >= CharacterCreatorScript.create.allHairStyles.Length)
		{
			showingHair = 0;
		}
		if (showingColour < 0)
		{
			showingColour = CharacterCreatorScript.create.allHairColours.Length - 1;
		}
		if (showingColour >= CharacterCreatorScript.create.allHairColours.Length)
		{
			showingColour = 0;
		}
	}

	private IEnumerator delayHairChange()
	{
		yield return wait;
		if (!selectingColor)
		{
			NetworkMapSharer.share.localChar.myEquip.CmdChangeHairId(showingHair);
		}
		else
		{
			NetworkMapSharer.share.localChar.myEquip.CmdChangeHairColour(showingColour);
		}
	}
}
