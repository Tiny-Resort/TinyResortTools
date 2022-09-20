using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotButton : MonoBehaviour
{
	public int slotNoToLoad;

	public RawImage charSlotPic;

	public TextMeshProUGUI slotName;

	public TextMeshProUGUI islandName;

	public TextMeshProUGUI slotMoney;

	public TextMeshProUGUI slotDate;

	public GameObject deleteButton;

	private bool playerOnly;

	public bool createMultiPlayerGame;

	private PlayerInv showingInvSave;

	public long savedTime;

	public void setSlotNo(int slotNo, PlayerInv invSave, DateSave saveDate, bool loadPlayerOnly = false)
	{
		slotNoToLoad = slotNo;
		fillFromSaveSlot(invSave, saveDate);
		playerOnly = loadPlayerOnly;
		if (loadPlayerOnly)
		{
			deleteButton.gameObject.SetActive(false);
		}
	}

	private void fillFromSaveSlot(PlayerInv invSave, DateSave saveDate)
	{
		if (invSave != null)
		{
			slotName.text = invSave.playerName;
			islandName.text = invSave.islandName;
			slotMoney.text = "<sprite=11> " + invSave.money.ToString("n0");
			showingInvSave = invSave;
			savedTime = invSave.savedTime;
			getPhotoDelay();
		}
		if (saveDate != null)
		{
			slotDate.text = "Year " + saveDate.year + ", " + RealWorldTimeLight.time.getDayName(saveDate.day - 1) + " " + (saveDate.day + (saveDate.week - 1) * 7) + " " + RealWorldTimeLight.time.getSeasonName(saveDate.month - 1);
		}
		else
		{
			slotDate.text = "";
		}
	}

	public void getPhotoDelay()
	{
		charSlotPic.texture = CharacterCreatorScript.create.loadSlotPhoto();
	}

	public void onPress()
	{
		MonoBehaviour.print("Loading slot " + slotNoToLoad);
		TownManager.manage.firstConnect = false;
		CharacterCreatorScript.create.gameObject.SetActive(false);
		saveLastLoadOrder();
		SaveLoad.saveOrLoad.setSlotToLoad(slotNoToLoad);
		if (!playerOnly)
		{
			MultiplayerLoadWindow.load.characterSelectedOnWindow = true;
			SaveLoad.saveOrLoad.StartCoroutine(startDelay());
			MultiplayerLoadWindow.load.closeWindow();
		}
		else
		{
			MultiplayerLoadWindow.load.onCharSelectedForMultiplayer(slotNoToLoad);
		}
	}

	public void saveLastLoadOrder()
	{
	}

	private IEnumerator startDelay()
	{
		yield return SaveLoad.saveOrLoad.StartCoroutine(SaveLoad.saveOrLoad.loadOverFrames());
		if (createMultiPlayerGame)
		{
			CustomNetworkManager.manage.createLobbyBeforeConnection();
		}
		CustomNetworkManager.manage.StartUpHost();
		cameraWonderOnMenu.wonder.enabled = false;
		CharacterCreatorScript.create.myCamera.gameObject.SetActive(false);
		SaveLoad.saveOrLoad.loadingScreen.completed();
		yield return new WaitForSeconds(2f);
		SaveLoad.saveOrLoad.loadingScreen.disappear();
	}

	public void deleteSave()
	{
		SaveLoad.saveOrLoad.DeleteSave(slotNoToLoad);
		base.gameObject.SetActive(false);
	}
}
