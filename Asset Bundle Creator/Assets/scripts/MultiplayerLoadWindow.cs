using UnityEngine;

public class MultiplayerLoadWindow : MonoBehaviour
{
	public static MultiplayerLoadWindow load;

	public Transform joinGameMenu;

	public SaveSlotButton hostGameSaveSlot;

	public bool characterSelectedOnWindow;

	public bool joiningInvite;

	private void OnEnable()
	{
		load = this;
	}

	public void onCharSelectedForMultiplayer(int slotDataToLoad)
	{
		joinGameMenu.gameObject.SetActive(true);
		hostGameSaveSlot.setSlotNo(slotDataToLoad, SaveLoad.saveOrLoad.getSaveDetailsForFileButton(slotDataToLoad), SaveLoad.saveOrLoad.getSaveDateDetailsForButton(slotDataToLoad));
		base.gameObject.SetActive(false);
		characterSelectedOnWindow = true;
		if (joiningInvite)
		{
			SteamLobby.instance.joinButton.onButtonPress.Invoke();
			joiningInvite = false;
		}
	}

	public void closeWindow()
	{
		base.gameObject.SetActive(false);
	}

	public void cancelCharacterSelected()
	{
		characterSelectedOnWindow = false;
		joiningInvite = false;
	}

	public void openForInvite()
	{
		base.gameObject.SetActive(true);
		joiningInvite = true;
	}
}
