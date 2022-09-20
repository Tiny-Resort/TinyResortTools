using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoad : MonoBehaviour
{
	public static SaveLoad saveOrLoad;

	public bool loadingComplete;

	private bool quitAfterSave;

	public List<Vehicle> vehiclesToSave = new List<Vehicle>();

	public GameObject[] vehiclePrefabs;

	public GameObject[] carryablePrefabs;

	private int saveSlotToLoad = 1;

	public LoadingScreen loadingScreen;

	public List<NPCInventory> localInvs = new List<NPCInventory>();

	private Coroutine returnToMenuCoroutine;

	private int[] sittingPosOriginals;

	private void Awake()
	{
		saveOrLoad = this;
	}

	public string saveSlot()
	{
		return Path.Combine(Application.persistentDataPath + "\\Slot" + saveSlotToLoad);
	}

	public int currentSaveSlotNo()
	{
		return saveSlotToLoad;
	}

	public void findAFreeSlotForNewSave()
	{
		new DirectoryInfo(Application.persistentDataPath + "/Slot" + 0);
		for (int i = 0; i < 100; i++)
		{
			if (!new DirectoryInfo(Application.persistentDataPath + "/Slot" + i).Exists)
			{
				MonoBehaviour.print("Found an empy slot at " + i);
				saveSlotToLoad = i;
				break;
			}
		}
	}

	public bool isASaveSlot()
	{
		new DirectoryInfo(Application.persistentDataPath + "/Slot" + 0);
		for (int i = 0; i < 100; i++)
		{
			if (new DirectoryInfo(Application.persistentDataPath + "/Slot" + i).Exists)
			{
				return true;
			}
		}
		return false;
	}

	public void quitGame()
	{
		if ((bool)SteamLobby.instance)
		{
			SteamLobby.instance.LeaveGameLobby();
		}
		Application.Quit();
	}

	public void returnToMenu()
	{
		if (returnToMenuCoroutine == null)
		{
			returnToMenuCoroutine = StartCoroutine(returnToMenuDelay());
		}
	}

	public IEnumerator returnToMenuDelay()
	{
		if ((bool)SteamLobby.instance)
		{
			SteamLobby.instance.LeaveGameLobbyButton();
			yield return null;
		}
		if (NetworkMapSharer.share.isServer)
		{
			CustomNetworkManager.manage.StopHost();
		}
		else if (!NetworkMapSharer.share.isServer)
		{
			yield return StartCoroutine(saveRoutine(false, false, false, true));
			CustomNetworkManager.manage.StopClient();
			yield return null;
			SceneManager.LoadScene(1);
		}
	}

	public void SaveChests()
	{
		if (!NetworkMapSharer.share.isServer)
		{
			return;
		}
		foreach (Chest activeChest in ContainerManager.manage.activeChests)
		{
			ContainerManager.manage.saveChest(activeChest);
		}
	}

	public void SaveGame(bool isServer, bool takePhoto = true, bool endOfDaySave = true)
	{
		StartCoroutine(saveRoutine(isServer, takePhoto, endOfDaySave));
	}

	public IEnumerator saveRoutine(bool isServer, bool takePhoto = true, bool endOfDaySave = true, bool logOutSave = false)
	{
		if (isServer && RealWorldTimeLight.time.underGround)
		{
			yield return null;
		}
		else
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(saveSlot());
			if (!directoryInfo.Exists)
			{
				Debug.Log("Creating Slot Folder");
				directoryInfo.Create();
			}
			saveVersionNumber();
			SaveInv();
			saveLicences();
			saveRecipesUnlocked();
			saveLevels();
			savePedia();
			saveNpcRelations();
			saveMail();
			yield return null;
			EasySaveInv();
			EasySaveLicences();
			EasySaveRecipes();
			EasySaveLevels();
			EasySavePedia();
			EasySaveNPCRelations();
			EasySaveMail();
			yield return null;
			if (takePhoto)
			{
				CharacterCreatorScript.create.takeSlotPhotoAndSave();
			}
			if (isServer)
			{
				sittingPosOriginals = new int[NetworkNavMesh.nav.charsConnected.Count];
				for (int i = 0; i < NetworkNavMesh.nav.charsConnected.Count; i++)
				{
					CharPickUp component = NetworkNavMesh.nav.charsConnected[i].GetComponent<CharPickUp>();
					if (!component)
					{
						continue;
					}
					if (component.sitting)
					{
						if (component.myInteract.insideHouseDetails != null)
						{
							sittingPosOriginals[i] = component.myInteract.insideHouseDetails.houseMapOnTileStatus[component.sittingXpos, component.sittingYPos];
							component.myInteract.insideHouseDetails.houseMapOnTileStatus[component.sittingXpos, component.sittingYPos] = 0;
						}
						else
						{
							sittingPosOriginals[i] = WorldManager.manageWorld.onTileStatusMap[component.sittingXpos, component.sittingYPos];
							WorldManager.manageWorld.onTileStatusMap[component.sittingXpos, component.sittingYPos] = 0;
						}
					}
					if (NetworkMapSharer.share.isServer && NetworkMapSharer.share.localChar.myInteract.insidePlayerHouse)
					{
						NetworkMapSharer.share.localChar.myInteract.insideHouseDetails.clearFurnitureStatus();
					}
				}
				saveQuests();
				saveDate();
				saveTownManager();
				SaveChests();
				saveHouse();
				saveTownStatus();
				saveBulletinBoard();
				saveMuseum();
				saveVehicles();
				saveDeeds();
				saveMapIcons();
				saveDrops();
				saveCarriables();
				saveItemsOnTop();
				yield return StartCoroutine(saveOverFrames(endOfDaySave));
				saveFencedOffAnimals(endOfDaySave);
				FarmAnimalManager.manage.saveAnimalHouses();
				FarmAnimalManager.manage.saveFarmAnimalDetails();
				savePhotos(false);
				yield return null;
				EasySaveQuests();
				EasySaveDate();
				EasySaveTownManager();
				EasySaveHouses();
				EasySaveTownStatus();
				EasySaveBulletinBoard();
				EasySaveMuseum();
				EasySaveVehicles();
				EasySaveDeeds();
				EasySaveMapIcons();
				EasySaveDrops();
				EasySaveCarriables();
				EasySaveOnTop();
				FarmAnimalManager.manage.EasySaveAnimalHouses();
				FarmAnimalManager.manage.EasySaveFarmAnimals();
			}
			else
			{
				savePhotos(true);
				if (NetworkMapSharer.share.nextDayIsReady)
				{
					StartCoroutine(nonServerSave());
				}
			}
		}
		if (!logOutSave)
		{
			saveOrLoad.loadingScreen.saveGameConfirmed.gameObject.SetActive(true);
			yield return new WaitForSeconds(1f);
			StartCoroutine(saveOrLoad.loadingScreen.saveGameConfirmed.GetComponent<WindowAnimator>().closeWithMask());
		}
	}

	private IEnumerator nonServerSave()
	{
		loadingScreen.loadingBarOnlyAppear();
		yield return new WaitForSeconds(1f);
		loadingScreen.completed();
	}

	public void saveVehiclesForUnderGround()
	{
		for (int i = 0; i < vehiclesToSave.Count; i++)
		{
			NetworkMapSharer.share.unSpawnGameObject(vehiclesToSave[i].gameObject);
			vehiclesToSave[i].gameObject.SetActive(false);
		}
	}

	public void loadVehiclesForAboveGround()
	{
		for (int i = 0; i < vehiclesToSave.Count; i++)
		{
			vehiclesToSave[i].gameObject.SetActive(true);
			NetworkMapSharer.share.spawnGameObject(vehiclesToSave[i].gameObject);
		}
	}

	public void EasySaveVehicles()
	{
		List<VehicleSavable> list = new List<VehicleSavable>();
		for (int i = 0; i < vehiclesToSave.Count; i++)
		{
			if (vehiclesToSave[i] != null)
			{
				list.Add(new VehicleSavable(vehiclesToSave[i]));
			}
		}
		VehicleSave vehicleSave = new VehicleSave();
		vehicleSave.allVehicles = list.ToArray();
		try
		{
			ES3.Save("vehicleInfo", vehicleSave, saveSlot() + "/vehicleInfo.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/vehicleInfo.es3");
			ES3.Save("vehicleInfo", vehicleSave, saveSlot() + "/vehicleInfo.es3");
		}
	}

	public bool EasyLoadVehicles()
	{
		try
		{
			if (ES3.KeyExists("vehicleInfo", saveSlot() + "/vehicleInfo.es3"))
			{
				VehicleSave vehicleSave = new VehicleSave();
				ES3.LoadInto("vehicleInfo", saveSlot() + "/vehicleInfo.es3", vehicleSave);
				VehicleSavable[] allVehicles = vehicleSave.allVehicles;
				foreach (VehicleSavable vehicleSavable in allVehicles)
				{
					if (vehicleSavable.vehicleId < vehiclePrefabs.Length)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(vehiclePrefabs[vehicleSavable.vehicleId], vehicleSavable.getPosition(), vehicleSavable.getRotation());
						gameObject.GetComponent<Vehicle>().setVariation(vehicleSavable.colourVaration);
						NetworkMapSharer.share.spawnGameObject(gameObject);
					}
				}
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveVehicles()
	{
		List<VehicleSavable> list = new List<VehicleSavable>();
		for (int i = 0; i < vehiclesToSave.Count; i++)
		{
			if (vehiclesToSave[i] != null)
			{
				list.Add(new VehicleSavable(vehiclesToSave[i]));
			}
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			VehicleSave graph = new VehicleSave
			{
				allVehicles = list.ToArray()
			};
			fileStream = File.Create(saveSlot() + "/vehicleInfo.dat");
			binaryFormatter.Serialize(fileStream, graph);
			fileStream.Close();
			makeABackUp(saveSlot() + "/vehicleInfo.dat", saveSlot() + "/vehicleInfo.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving vehicles");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void savePhotos(bool isClient)
	{
		PhotoSave graph = new PhotoSave(PhotoManager.manage.savedPhotos, null);
		if (isClient)
		{
			if (File.Exists(saveSlot() + "/photoDetails.dat"))
			{
				FileStream fileStream = null;
				try
				{
					MonoBehaviour.print("Loading Photos For Client Save");
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					fileStream = File.Open(saveSlot() + "/photoDetails.dat", FileMode.Open);
					PhotoSave photoSave = (PhotoSave)binaryFormatter.Deserialize(fileStream);
					fileStream.Close();
					graph = new PhotoSave(PhotoManager.manage.savedPhotos, photoSave.displayedPhotosSave);
				}
				catch (Exception)
				{
					Debug.LogWarning("error saving photos");
					if (fileStream != null)
					{
						fileStream.Close();
					}
				}
			}
			else
			{
				graph = new PhotoSave(PhotoManager.manage.savedPhotos, null);
			}
		}
		else
		{
			graph = new PhotoSave(PhotoManager.manage.savedPhotos, PhotoManager.manage.displayedPhotos);
		}
		FileStream fileStream2 = null;
		try
		{
			BinaryFormatter binaryFormatter2 = new BinaryFormatter();
			fileStream2 = File.Create(saveSlot() + "/photoDetails.dat");
			binaryFormatter2.Serialize(fileStream2, graph);
			fileStream2.Close();
			makeABackUp(saveSlot() + "/photoDetails.dat", saveSlot() + "/photoDetails.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving photos");
			if (fileStream2 != null)
			{
				fileStream2.Close();
			}
		}
	}

	public void loadPhotos(bool isClient = false)
	{
		if (File.Exists(saveSlot() + "/photoDetails.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/photoDetails.dat", FileMode.Open);
				((PhotoSave)binaryFormatter.Deserialize(fileStream)).loadPhotos(isClient);
				StartCoroutine(populateJournalPhotos());
				fileStream.Close();
				makeABackUp(saveSlot() + "/photoDetails.dat", saveSlot() + "/photoDetails.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading photo save.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadBackupPhotos();
				return;
			}
		}
		loadBackupPhotos();
	}

	public void loadBackupPhotos(bool isClient = false)
	{
		if (!File.Exists(saveSlot() + "/photoDetails.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/photoDetails.bak", FileMode.Open);
			((PhotoSave)binaryFormatter.Deserialize(fileStream)).loadPhotos(isClient);
			StartCoroutine(populateJournalPhotos());
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error reading photo backup.");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	private IEnumerator populateJournalPhotos()
	{
		yield return null;
		PhotoManager.manage.populatePhotoButtons();
	}

	public void makeABackUp(string fileName, string backupName)
	{
		try
		{
			File.Copy(fileName, backupName, true);
		}
		catch (Exception)
		{
			Debug.LogWarning("Error backing up file: " + fileName);
		}
	}

	public void loadVehicles()
	{
		if (File.Exists(saveSlot() + "/vehicleInfo.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/vehicleInfo.dat", FileMode.Open);
				VehicleSavable[] allVehicles = ((VehicleSave)binaryFormatter.Deserialize(fileStream)).allVehicles;
				foreach (VehicleSavable vehicleSavable in allVehicles)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(vehiclePrefabs[vehicleSavable.vehicleId], vehicleSavable.getPosition(), vehicleSavable.getRotation());
					gameObject.GetComponent<Vehicle>().setVariation(vehicleSavable.colourVaration);
					NetworkMapSharer.share.spawnGameObject(gameObject);
				}
				fileStream.Close();
				makeABackUp(saveSlot() + "/vehicleInfo.dat", saveSlot() + "/vehicleInfo.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading vehicles");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadBackupVehicles();
				return;
			}
		}
		loadBackupVehicles();
	}

	public void loadBackupVehicles()
	{
		MonoBehaviour.print("Loading backup vehicles");
		if (File.Exists(saveSlot() + "/vehicleInfo.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/vehicleInfo.bak", FileMode.Open);
				VehicleSavable[] allVehicles = ((VehicleSave)binaryFormatter.Deserialize(fileStream)).allVehicles;
				foreach (VehicleSavable vehicleSavable in allVehicles)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(vehiclePrefabs[vehicleSavable.vehicleId], vehicleSavable.getPosition(), vehicleSavable.getRotation());
					gameObject.GetComponent<Vehicle>().setVariation(vehicleSavable.colourVaration);
					NetworkMapSharer.share.spawnGameObject(gameObject);
				}
				fileStream.Close();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading vehicles backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadVehicles();
				return;
			}
		}
		EasyLoadVehicles();
	}

	public void EasySaveInv()
	{
		PlayerInv playerInv = new PlayerInv();
		playerInv.money = Inventory.inv.wallet;
		playerInv.bankBalance = BankMenu.menu.accountBalance;
		playerInv.playerName = Inventory.inv.playerName;
		playerInv.islandName = Inventory.inv.islandName;
		MonoBehaviour.print("Name saved as " + Inventory.inv.playerName + "in slot No" + saveSlotToLoad);
		playerInv.eyeStyle = Inventory.inv.playerEyes;
		playerInv.eyeColour = Inventory.inv.playerEyeColor;
		playerInv.skinTone = Inventory.inv.skinTone;
		playerInv.nose = Inventory.inv.nose;
		playerInv.mouth = Inventory.inv.mouth;
		playerInv.face = EquipWindow.equip.faceSlot.itemNo;
		playerInv.hair = Inventory.inv.playerHair;
		playerInv.hairColour = Inventory.inv.playerHairColour;
		playerInv.head = EquipWindow.equip.hatSlot.itemNo;
		playerInv.body = EquipWindow.equip.shirtSlot.itemNo;
		playerInv.pants = EquipWindow.equip.pantsSlot.itemNo;
		playerInv.shoes = EquipWindow.equip.shoeSlot.itemNo;
		playerInv.health = StatusManager.manage.connectedDamge.health;
		playerInv.healthMax = StatusManager.manage.connectedDamge.maxHealth;
		playerInv.stamina = StatusManager.manage.getStamina();
		playerInv.staminaMax = StatusManager.manage.getStaminaMax();
		playerInv.catalogue = CatalogueManager.manage.collectedItem;
		playerInv.savedTime = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
		playerInv.itemsInInvSlots = new int[Inventory.inv.invSlots.Length];
		playerInv.stacksInSlots = new int[Inventory.inv.invSlots.Length];
		for (int i = 0; i < Inventory.inv.invSlots.Length; i++)
		{
			playerInv.itemsInInvSlots[i] = Inventory.inv.invSlots[i].itemNo;
			playerInv.stacksInSlots[i] = Inventory.inv.invSlots[i].stack;
		}
		try
		{
			ES3.Save("playerInfo", playerInv, saveSlot() + "/playerInfo.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/playerInfo.es3");
			ES3.Save("playerInfo", playerInv, saveSlot() + "/playerInfo.es3");
		}
	}

	public bool EasyInvExists()
	{
		if (ES3.KeyExists("playerInfo", saveSlot() + "/playerInfo.es3"))
		{
			return true;
		}
		return false;
	}

	public PlayerInv EasyInvForLoadSlot()
	{
		try
		{
			if (ES3.KeyExists("playerInfo", saveSlot() + "/playerInfo.es3"))
			{
				PlayerInv playerInv = new PlayerInv();
				ES3.LoadInto("playerInfo", saveSlot() + "/playerInfo.es3", playerInv);
				return playerInv;
			}
		}
		catch
		{
		}
		return null;
	}

	public bool EasyLoadInv()
	{
		try
		{
			if (ES3.KeyExists("playerInfo", saveSlot() + "/playerInfo.es3"))
			{
				PlayerInv playerInv = new PlayerInv();
				ES3.LoadInto("playerInfo", saveSlot() + "/playerInfo.es3", playerInv);
				Inventory.inv.changeWalletToLoad(playerInv.money);
				BankMenu.menu.accountBalance = playerInv.bankBalance;
				if (playerInv.hair < 0)
				{
					playerInv.hair = Mathf.Abs(playerInv.hair + 1);
				}
				Inventory.inv.playerHair = playerInv.hair;
				Inventory.inv.playerHairColour = playerInv.hairColour;
				Inventory.inv.playerEyes = playerInv.eyeStyle;
				Inventory.inv.nose = playerInv.nose;
				Inventory.inv.mouth = playerInv.mouth;
				Inventory.inv.playerEyeColor = playerInv.eyeColour;
				Inventory.inv.skinTone = playerInv.skinTone;
				Inventory.inv.playerName = playerInv.playerName;
				Inventory.inv.islandName = playerInv.islandName;
				EquipWindow.equip.hatSlot.updateSlotContentsAndRefresh(playerInv.head, 1);
				EquipWindow.equip.faceSlot.updateSlotContentsAndRefresh(playerInv.face, 1);
				EquipWindow.equip.shirtSlot.updateSlotContentsAndRefresh(playerInv.body, 1);
				EquipWindow.equip.pantsSlot.updateSlotContentsAndRefresh(playerInv.pants, 1);
				EquipWindow.equip.shoeSlot.updateSlotContentsAndRefresh(playerInv.shoes, 1);
				StartCoroutine(EquipWindow.equip.wearingMinersHelmet());
				if (playerInv.catalogue != null)
				{
					for (int i = 0; i < playerInv.catalogue.Length; i++)
					{
						CatalogueManager.manage.collectedItem[i] = playerInv.catalogue[i];
					}
				}
				StatusManager.manage.loadStatus(playerInv.health, playerInv.healthMax, playerInv.stamina, playerInv.staminaMax);
				for (int j = 0; j < playerInv.itemsInInvSlots.Length; j++)
				{
					Inventory.inv.invSlots[j].itemNo = playerInv.itemsInInvSlots[j];
					Inventory.inv.invSlots[j].stack = playerInv.stacksInSlots[j];
					Inventory.inv.invSlots[j].updateSlotContentsAndRefresh(playerInv.itemsInInvSlots[j], playerInv.stacksInSlots[j]);
				}
				loadNpcRelations();
				loadQuests();
				loadRecipesUnlocked();
				loadLicences();
				loadPedia();
				loadLevels();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public void SaveInv()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/playerInfo.dat");
			PlayerInv playerInv = new PlayerInv();
			playerInv.money = Inventory.inv.wallet;
			playerInv.bankBalance = BankMenu.menu.accountBalance;
			playerInv.playerName = Inventory.inv.playerName;
			playerInv.islandName = Inventory.inv.islandName;
			MonoBehaviour.print("Name saved as " + Inventory.inv.playerName + "in slot No" + saveSlotToLoad);
			playerInv.eyeStyle = Inventory.inv.playerEyes;
			playerInv.eyeColour = Inventory.inv.playerEyeColor;
			playerInv.skinTone = Inventory.inv.skinTone;
			playerInv.nose = Inventory.inv.nose;
			playerInv.mouth = Inventory.inv.mouth;
			playerInv.face = EquipWindow.equip.faceSlot.itemNo;
			playerInv.hair = Inventory.inv.playerHair;
			playerInv.hairColour = Inventory.inv.playerHairColour;
			playerInv.head = EquipWindow.equip.hatSlot.itemNo;
			playerInv.body = EquipWindow.equip.shirtSlot.itemNo;
			playerInv.pants = EquipWindow.equip.pantsSlot.itemNo;
			playerInv.shoes = EquipWindow.equip.shoeSlot.itemNo;
			playerInv.health = StatusManager.manage.connectedDamge.health;
			playerInv.healthMax = StatusManager.manage.connectedDamge.maxHealth;
			playerInv.stamina = StatusManager.manage.getStamina();
			playerInv.staminaMax = StatusManager.manage.getStaminaMax();
			playerInv.catalogue = CatalogueManager.manage.collectedItem;
			playerInv.savedTime = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
			playerInv.itemsInInvSlots = new int[Inventory.inv.invSlots.Length];
			playerInv.stacksInSlots = new int[Inventory.inv.invSlots.Length];
			for (int i = 0; i < Inventory.inv.invSlots.Length; i++)
			{
				playerInv.itemsInInvSlots[i] = Inventory.inv.invSlots[i].itemNo;
				playerInv.stacksInSlots[i] = Inventory.inv.invSlots[i].stack;
			}
			binaryFormatter.Serialize(fileStream, playerInv);
			fileStream.Close();
			makeABackUp(saveSlot() + "/playerInfo.dat", saveSlot() + "/playerInfo.bak");
			ContainerManager.manage.saveStashes();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving player ");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void EasySaveLicences()
	{
		LicenceAndPermitPointSave licenceAndPermitPointSave = new LicenceAndPermitPointSave();
		licenceAndPermitPointSave.saveLicencesAndPoints();
		try
		{
			ES3.Save("licences", licenceAndPermitPointSave, saveSlot() + "/licences.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/licences.es3");
			ES3.Save("licences", licenceAndPermitPointSave, saveSlot() + "/licences.es3");
		}
	}

	public bool EasyLoadLicences()
	{
		try
		{
			if (ES3.KeyExists("licences", saveSlot() + "/licences.es3"))
			{
				LicenceAndPermitPointSave licenceAndPermitPointSave = new LicenceAndPermitPointSave();
				ES3.LoadInto("licences", saveSlot() + "/licences.es3", licenceAndPermitPointSave);
				licenceAndPermitPointSave.loadLicencesAndPoints();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public void saveLicences()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/licences.dat");
			LicenceAndPermitPointSave licenceAndPermitPointSave = new LicenceAndPermitPointSave();
			licenceAndPermitPointSave.saveLicencesAndPoints();
			binaryFormatter.Serialize(fileStream, licenceAndPermitPointSave);
			fileStream.Close();
			makeABackUp(saveSlot() + "/licences.dat", saveSlot() + "/licences.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving licences.");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void loadLicences()
	{
		if (File.Exists(saveSlot() + "/licences.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/licences.dat", FileMode.Open);
				LicenceAndPermitPointSave obj = (LicenceAndPermitPointSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadLicencesAndPoints();
				makeABackUp(saveSlot() + "/licences.dat", saveSlot() + "/licences.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading licences.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadLicencesBackup();
				return;
			}
		}
		loadLicencesBackup();
	}

	public void loadLicencesBackup()
	{
		Debug.LogWarning("Loading licences backup");
		if (File.Exists(saveSlot() + "/licences.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/licences.bak", FileMode.Open);
				LicenceAndPermitPointSave obj = (LicenceAndPermitPointSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadLicencesAndPoints();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error saving licences backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadLicences();
				return;
			}
		}
		EasyLoadLicences();
	}

	public void EasySaveQuests()
	{
		QuestSave questSave = new QuestSave();
		questSave.accepted = QuestManager.manage.isQuestAccepted;
		questSave.completed = QuestManager.manage.isQuestCompleted;
		try
		{
			ES3.Save("quests", questSave, saveSlot() + "/quests.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/quests.es3");
			ES3.Save("quests", questSave, saveSlot() + "/quests.es3");
		}
	}

	public bool EasyLoadQuests()
	{
		try
		{
			if (ES3.KeyExists("quests", saveSlot() + "/quests.es3"))
			{
				QuestSave questSave = new QuestSave();
				ES3.LoadInto("quests", saveSlot() + "/quests.es3", questSave);
				for (int i = 0; i < questSave.completed.Length; i++)
				{
					QuestManager.manage.isQuestCompleted[i] = questSave.completed[i];
					QuestManager.manage.isQuestAccepted[i] = questSave.accepted[i];
				}
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveQuests()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/quests.dat");
			binaryFormatter.Serialize(fileStream, new QuestSave
			{
				accepted = QuestManager.manage.isQuestAccepted,
				completed = QuestManager.manage.isQuestCompleted
			});
			fileStream.Close();
			makeABackUp(saveSlot() + "/quests.dat", saveSlot() + "/quests.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving quests");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	private void loadQuests()
	{
		if (File.Exists(saveSlot() + "/quests.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/quests.dat", FileMode.Open);
				QuestSave questSave = (QuestSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				makeABackUp(saveSlot() + "/quests.dat", saveSlot() + "/quests.bak");
				for (int i = 0; i < questSave.completed.Length; i++)
				{
					QuestManager.manage.isQuestCompleted[i] = questSave.completed[i];
					QuestManager.manage.isQuestAccepted[i] = questSave.accepted[i];
				}
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading quest save.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadQuestsBackup();
				return;
			}
		}
		loadQuestsBackup();
	}

	private void loadQuestsBackup()
	{
		Debug.LogWarning("Loading quest save backup");
		if (File.Exists(saveSlot() + "/quests.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/quests.bak", FileMode.Open);
				QuestSave questSave = (QuestSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				for (int i = 0; i < questSave.completed.Length; i++)
				{
					QuestManager.manage.isQuestCompleted[i] = questSave.completed[i];
					QuestManager.manage.isQuestAccepted[i] = questSave.accepted[i];
				}
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading quest save backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadQuests();
				return;
			}
		}
		EasyLoadQuests();
	}

	public void clearSavedStatsForClient()
	{
	}

	public void EasySaveTownManager()
	{
		TownManagerSave townManagerSave = new TownManagerSave();
		townManagerSave.saveTown();
		try
		{
			ES3.Save("townSave", townManagerSave, saveSlot() + "/townSave.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/townSave.es3");
			ES3.Save("townSave", townManagerSave, saveSlot() + "/townSave.es3");
		}
	}

	public bool EasyLoadTownManager()
	{
		try
		{
			if (ES3.KeyExists("townSave", saveSlot() + "/townSave.es3"))
			{
				TownManagerSave townManagerSave = new TownManagerSave();
				ES3.LoadInto("townSave", saveSlot() + "/townSave.es3", townManagerSave);
				townManagerSave.load();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveTownManager()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/townSave.dat");
			TownManagerSave townManagerSave = new TownManagerSave();
			townManagerSave.saveTown();
			binaryFormatter.Serialize(fileStream, townManagerSave);
			fileStream.Close();
			makeABackUp(saveSlot() + "/townSave.dat", saveSlot() + "/townSave.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving quests");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void EasySaveDate()
	{
		DateSave dateSave = new DateSave();
		dateSave = WorldManager.manageWorld.getDateSave();
		dateSave.todaysMineSeed = NetworkMapSharer.share.mineSeed;
		dateSave.tomorrowsMineSeed = NetworkMapSharer.share.tomorrowsMineSeed;
		dateSave.hour = RealWorldTimeLight.time.currentHour;
		try
		{
			ES3.Save("date", dateSave, saveSlot() + "/date.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/date.es3");
			ES3.Save("date", dateSave, saveSlot() + "/date.es3");
		}
	}

	public bool EasyLoadDate()
	{
		try
		{
			if (ES3.KeyExists("date", saveSlot() + "/date.es3"))
			{
				DateSave dateSave = new DateSave();
				ES3.LoadInto("date", saveSlot() + "/date.es3", dateSave);
				WorldManager.manageWorld.loadDateFromSave(dateSave);
				NetworkMapSharer.share.NetworkmineSeed = dateSave.todaysMineSeed;
				NetworkMapSharer.share.tomorrowsMineSeed = dateSave.tomorrowsMineSeed;
				RealWorldTimeLight.time.NetworkcurrentHour = dateSave.hour;
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveDate()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/date.dat");
			DateSave dateSave = new DateSave();
			dateSave = WorldManager.manageWorld.getDateSave();
			dateSave.todaysMineSeed = NetworkMapSharer.share.mineSeed;
			dateSave.tomorrowsMineSeed = NetworkMapSharer.share.tomorrowsMineSeed;
			dateSave.hour = RealWorldTimeLight.time.currentHour;
			binaryFormatter.Serialize(fileStream, dateSave);
			fileStream.Close();
			makeABackUp(saveSlot() + "/date.dat", saveSlot() + "/date.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving quests");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	private void loadDate()
	{
		if (DoesSaveExist() && File.Exists(saveSlot() + "/date.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/date.dat", FileMode.Open);
				DateSave dateSave = (DateSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				WorldManager.manageWorld.loadDateFromSave(dateSave);
				NetworkMapSharer.share.NetworkmineSeed = dateSave.todaysMineSeed;
				NetworkMapSharer.share.tomorrowsMineSeed = dateSave.tomorrowsMineSeed;
				RealWorldTimeLight.time.NetworkcurrentHour = dateSave.hour;
				makeABackUp(saveSlot() + "/date.dat", saveSlot() + "/date.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading date file.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadDateBackup();
				return;
			}
		}
		loadDateBackup();
	}

	private void loadDateBackup()
	{
		Debug.LogWarning("Loading date backup file.");
		if (DoesSaveExist() && File.Exists(saveSlot() + "/date.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/date.bak", FileMode.Open);
				DateSave dateSave = (DateSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				WorldManager.manageWorld.loadDateFromSave(dateSave);
				NetworkMapSharer.share.NetworkmineSeed = dateSave.todaysMineSeed;
				NetworkMapSharer.share.tomorrowsMineSeed = dateSave.tomorrowsMineSeed;
				RealWorldTimeLight.time.NetworkcurrentHour = dateSave.hour;
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading date backup file.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadDate();
				return;
			}
		}
		EasyLoadDate();
	}

	private void loadTown()
	{
		if (DoesSaveExist() && File.Exists(saveSlot() + "/townSave.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/townSave.dat", FileMode.Open);
				((TownManagerSave)binaryFormatter.Deserialize(fileStream)).load();
				fileStream.Close();
				makeABackUp(saveSlot() + "/townSave.dat", saveSlot() + "/townSave.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading town save");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadTownBackup();
				return;
			}
		}
		loadTownBackup();
	}

	private void loadTownBackup()
	{
		Debug.LogWarning("Loading town save backup");
		if (DoesSaveExist() && File.Exists(saveSlot() + "/townSave.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/townSave.bak", FileMode.Open);
				((TownManagerSave)binaryFormatter.Deserialize(fileStream)).load();
				fileStream.Close();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading town save backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadTownManager();
				return;
			}
		}
		EasyLoadTownManager();
	}

	public void EasySaveOnTop()
	{
		ItemOnTopSave itemOnTopSave = new ItemOnTopSave();
		itemOnTopSave.saveObjectsOnTop();
		try
		{
			ES3.Save("onTop", itemOnTopSave, saveSlot() + "/onTop.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/onTop.es3");
			ES3.Save("onTop", itemOnTopSave, saveSlot() + "/onTop.es3");
		}
	}

	public bool EasyLoadOnTop()
	{
		try
		{
			if (ES3.KeyExists("onTop", saveSlot() + "/onTop.es3"))
			{
				ItemOnTopSave itemOnTopSave = new ItemOnTopSave();
				ES3.LoadInto("onTop", saveSlot() + "/onTop.es3", itemOnTopSave);
				itemOnTopSave.loadObjectsOnTop();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void loadItemsOnTop()
	{
		if (DoesSaveExist() && File.Exists(saveSlot() + "/onTop.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/onTop.dat", FileMode.Open);
				((ItemOnTopSave)binaryFormatter.Deserialize(fileStream)).loadObjectsOnTop();
				fileStream.Close();
				makeABackUp(saveSlot() + "/onTop.dat", saveSlot() + "/onTop.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading on top files.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadItemsOnTopBackup();
				return;
			}
		}
		loadItemsOnTopBackup();
	}

	private void loadItemsOnTopBackup()
	{
		Debug.LogWarning("Loading on top files backup");
		if (DoesSaveExist() && File.Exists(saveSlot() + "/onTop.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/onTop.bak", FileMode.Open);
				((ItemOnTopSave)binaryFormatter.Deserialize(fileStream)).loadObjectsOnTop();
				fileStream.Close();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading on top files backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadOnTop();
				return;
			}
		}
		EasyLoadOnTop();
	}

	private void saveItemsOnTop()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/onTop.dat");
			ItemOnTopSave itemOnTopSave = new ItemOnTopSave();
			itemOnTopSave.saveObjectsOnTop();
			binaryFormatter.Serialize(fileStream, itemOnTopSave);
			fileStream.Close();
			makeABackUp(saveSlot() + "/onTop.dat", saveSlot() + "/onTop.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving items on top");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	private void saveNpcRelations()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/npc.dat");
			NPCsave nPCsave = new NPCsave
			{
				savedStatuses = NPCManager.manage.npcStatus.ToArray()
			};
			if (NetworkMapSharer.share.isServer)
			{
				nPCsave.saveInvs = NPCManager.manage.npcInvs.ToArray();
			}
			else
			{
				nPCsave.saveInvs = localInvs.ToArray();
			}
			binaryFormatter.Serialize(fileStream, nPCsave);
			fileStream.Close();
			makeABackUp(saveSlot() + "/npc.dat", saveSlot() + "/npc.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving NPC relationship");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void EasySaveNPCRelations()
	{
		NPCsave nPCsave = new NPCsave();
		nPCsave.savedStatuses = NPCManager.manage.npcStatus.ToArray();
		if (NetworkMapSharer.share.isServer)
		{
			nPCsave.saveInvs = NPCManager.manage.npcInvs.ToArray();
		}
		else
		{
			nPCsave.saveInvs = localInvs.ToArray();
		}
		try
		{
			ES3.Save("npc", nPCsave, saveSlot() + "/npc.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/npc.es3");
			ES3.Save("npc", nPCsave, saveSlot() + "/npc.es3");
		}
	}

	public bool EasyLoadNPCRelations()
	{
		try
		{
			if (ES3.KeyExists("npc", saveSlot() + "/npc.es3"))
			{
				NPCsave nPCsave = new NPCsave();
				ES3.LoadInto("npc", saveSlot() + "/npc.es3", nPCsave);
				nPCsave.loadNpcs();
				localInvs = NPCManager.manage.npcInvs;
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void loadNpcRelations()
	{
		if (File.Exists(saveSlot() + "/npc.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/npc.dat", FileMode.Open);
				NPCsave obj = (NPCsave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadNpcs();
				localInvs = NPCManager.manage.npcInvs;
				makeABackUp(saveSlot() + "/npc.dat", saveSlot() + "/npc.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading on NPC status.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadNpcRelationsBackup();
				return;
			}
		}
		loadNpcRelationsBackup();
	}

	private void loadNpcRelationsBackup()
	{
		Debug.LogWarning("Loading on NPC status backup");
		if (File.Exists(saveSlot() + "/npc.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/npc.bak", FileMode.Open);
				NPCsave obj = (NPCsave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadNpcs();
				localInvs = NPCManager.manage.npcInvs;
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading on NPC status backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadNPCRelations();
				return;
			}
		}
		EasyLoadNPCRelations();
	}

	private void EasySaveRecipes()
	{
		RecipesUnlockedSave recipesUnlockedSave = new RecipesUnlockedSave();
		recipesUnlockedSave.crafterLevel = CraftsmanManager.manage.currentLevel;
		recipesUnlockedSave.currentPoints = CraftsmanManager.manage.currentPoints;
		recipesUnlockedSave.crafterWorkingOnItemId = CraftsmanManager.manage.itemCurrentlyCrafting;
		recipesUnlockedSave.crafterCurrentlyWorking = NetworkMapSharer.share.craftsmanWorking;
		recipesUnlockedSave.recipesUnlocked = CharLevelManager.manage.recipes.ToArray();
		try
		{
			ES3.Save("unlocked", recipesUnlockedSave, saveSlot() + "/unlocked.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/unlocked.es3");
			ES3.Save("unlocked", recipesUnlockedSave, saveSlot() + "/unlocked.es3");
		}
	}

	private void saveRecipesUnlocked()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/unlocked.dat");
			binaryFormatter.Serialize(fileStream, new RecipesUnlockedSave
			{
				crafterLevel = CraftsmanManager.manage.currentLevel,
				currentPoints = CraftsmanManager.manage.currentPoints,
				crafterWorkingOnItemId = CraftsmanManager.manage.itemCurrentlyCrafting,
				crafterCurrentlyWorking = NetworkMapSharer.share.craftsmanWorking,
				recipesUnlocked = CharLevelManager.manage.recipes.ToArray()
			});
			fileStream.Close();
			makeABackUp(saveSlot() + "/unlocked.dat", saveSlot() + "/unlocked.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving recipes");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public bool EasyLoadRecipes()
	{
		try
		{
			if (ES3.KeyExists("unlocked", saveSlot() + "/unlocked.es3"))
			{
				RecipesUnlockedSave recipesUnlockedSave = new RecipesUnlockedSave();
				ES3.LoadInto("unlocked", saveSlot() + "/unlocked.es3", recipesUnlockedSave);
				recipesUnlockedSave.loadRecipes();
				CraftsmanManager.manage.currentLevel = recipesUnlockedSave.crafterLevel;
				CraftsmanManager.manage.currentPoints = recipesUnlockedSave.currentPoints;
				CraftsmanManager.manage.itemCurrentlyCrafting = recipesUnlockedSave.crafterWorkingOnItemId;
				NetworkMapSharer.share.NetworkcraftsmanWorking = recipesUnlockedSave.crafterCurrentlyWorking;
				CharLevelManager.manage.recipesAlwaysUnlocked();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void loadRecipesUnlocked()
	{
		if (File.Exists(saveSlot() + "/unlocked.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/unlocked.dat", FileMode.Open);
				RecipesUnlockedSave recipesUnlockedSave = (RecipesUnlockedSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				recipesUnlockedSave.loadRecipes();
				CraftsmanManager.manage.currentLevel = recipesUnlockedSave.crafterLevel;
				CraftsmanManager.manage.currentPoints = recipesUnlockedSave.currentPoints;
				CraftsmanManager.manage.itemCurrentlyCrafting = recipesUnlockedSave.crafterWorkingOnItemId;
				NetworkMapSharer.share.NetworkcraftsmanWorking = recipesUnlockedSave.crafterCurrentlyWorking;
				CharLevelManager.manage.recipesAlwaysUnlocked();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading recipes");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadRecipes();
				return;
			}
		}
		EasyLoadRecipes();
	}

	private void EasySaveDeeds()
	{
		DeedSave deedSave = new DeedSave();
		deedSave.saveDeeds(DeedManager.manage.deedDetails.ToArray());
		try
		{
			ES3.Save("deeds", deedSave, saveSlot() + "/deeds.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/deeds.es3");
			ES3.Save("deeds", deedSave, saveSlot() + "/deeds.es3");
		}
	}

	public bool EasyLoadDeeds()
	{
		try
		{
			if (ES3.KeyExists("deeds", saveSlot() + "/deeds.es3"))
			{
				DeedSave deedSave = new DeedSave();
				ES3.LoadInto("deeds", saveSlot() + "/deeds.es3", deedSave);
				deedSave.loadDeeds();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveDeeds()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/deeds.dat");
			DeedSave deedSave = new DeedSave();
			deedSave.saveDeeds(DeedManager.manage.deedDetails.ToArray());
			binaryFormatter.Serialize(fileStream, deedSave);
			fileStream.Close();
			makeABackUp(saveSlot() + "/deeds.dat", saveSlot() + "/deeds.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving deeds.");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	private void loadDeeds()
	{
		if (File.Exists(saveSlot() + "/deeds.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/deeds.dat", FileMode.Open);
				DeedSave obj = (DeedSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadDeeds();
				makeABackUp(saveSlot() + "/deeds.dat", saveSlot() + "/deeds.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading deeds.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadDeedsBackUp();
				return;
			}
		}
		loadDeedsBackUp();
	}

	private void loadDeedsBackUp()
	{
		Debug.LogWarning("Loading backup deeds");
		if (File.Exists(saveSlot() + "/deeds.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/deeds.bak", FileMode.Open);
				DeedSave obj = (DeedSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadDeeds();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading deeds backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadDeeds();
				return;
			}
		}
		EasyLoadDeeds();
	}

	private void EasySavePedia()
	{
		PediaSave pediaSave = new PediaSave();
		pediaSave.saveEntries(PediaManager.manage.allEntries.ToArray());
		try
		{
			ES3.Save("pedia", pediaSave, saveSlot() + "/pedia.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/pedia.es3");
			ES3.Save("pedia", pediaSave, saveSlot() + "/pedia.es3");
		}
	}

	public bool EasyLoadPedia()
	{
		try
		{
			if (ES3.KeyExists("pedia", saveSlot() + "/pedia.es3"))
			{
				PediaSave pediaSave = new PediaSave();
				ES3.LoadInto("pedia", saveSlot() + "/pedia.es3", pediaSave);
				pediaSave.loadEntries();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void savePedia()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/pedia.dat");
			PediaSave pediaSave = new PediaSave();
			pediaSave.saveEntries(PediaManager.manage.allEntries.ToArray());
			binaryFormatter.Serialize(fileStream, pediaSave);
			fileStream.Close();
			makeABackUp(saveSlot() + "/pedia.dat", saveSlot() + "/pedia.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving pedia");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	private void loadPedia()
	{
		if (File.Exists(saveSlot() + "/pedia.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/pedia.dat", FileMode.Open);
				PediaSave obj = (PediaSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadEntries();
				makeABackUp(saveSlot() + "/pedia.dat", saveSlot() + "/pedia.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading Pedia");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadPediaBackup();
				return;
			}
		}
		loadPediaBackup();
	}

	private void loadPediaBackup()
	{
		Debug.LogWarning("Loading Pedia backup");
		if (File.Exists(saveSlot() + "/pedia.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/pedia.bak", FileMode.Open);
				PediaSave obj = (PediaSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadEntries();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading Pedia backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadPedia();
				return;
			}
		}
		EasyLoadPedia();
	}

	private void EasySaveTownStatus()
	{
		TownStatusSave townStatusSave = new TownStatusSave();
		townStatusSave.saveTownStatus();
		try
		{
			ES3.Save("townStatus", townStatusSave, saveSlot() + "/townStatus.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/townStatus.es3");
			ES3.Save("townStatus", townStatusSave, saveSlot() + "/townStatus.es3");
		}
	}

	private bool EasyLoadTownStatus()
	{
		try
		{
			if (ES3.KeyExists("townStatus", saveSlot() + "/townStatus.es3"))
			{
				TownStatusSave townStatusSave = new TownStatusSave();
				ES3.LoadInto("townStatus", saveSlot() + "/townStatus.es3", townStatusSave);
				townStatusSave.loadTownStatus();
				MonoBehaviour.print("Loaded town status with easy save");
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveTownStatus()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/townStatus.dat");
			TownStatusSave townStatusSave = new TownStatusSave();
			townStatusSave.saveTownStatus();
			binaryFormatter.Serialize(fileStream, townStatusSave);
			fileStream.Close();
			makeABackUp(saveSlot() + "/townStatus.dat", saveSlot() + "/townStatus.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving town status.");
			if (fileStream != null)
			{
				fileStream.Close();
			}
			loadTownStatusBackup();
		}
	}

	private void loadTownStatus()
	{
		if (File.Exists(saveSlot() + "/townStatus.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/townStatus.dat", FileMode.Open);
				TownStatusSave obj = (TownStatusSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadTownStatus();
				makeABackUp(saveSlot() + "/townStatus.dat", saveSlot() + "/townStatus.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading town status.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadTownStatusBackup();
				return;
			}
		}
		loadTownStatusBackup();
	}

	private void loadTownStatusBackup()
	{
		Debug.LogWarning("Loading town status backup");
		if (File.Exists(saveSlot() + "/townStatus.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/townStatus.bak", FileMode.Open);
				TownStatusSave obj = (TownStatusSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadTownStatus();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading town status backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadTownStatus();
				return;
			}
		}
		EasyLoadTownStatus();
	}

	public void EasySaveChangers()
	{
		ChangerSave changerSave = new ChangerSave();
		changerSave.saveChangers();
		try
		{
			ES3.Save("changers", changerSave, saveSlot() + "/changers.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/changers.es3");
			ES3.Save("changers", changerSave, saveSlot() + "/changers.es3");
		}
	}

	public bool EasyLoadChangers()
	{
		try
		{
			if (ES3.KeyExists("changers", saveSlot() + "/changers.es3"))
			{
				ChangerSave changerSave = new ChangerSave();
				ES3.LoadInto("changers", saveSlot() + "/changers.es3", changerSave);
				changerSave.loadChangers();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveChangers()
	{
		EasySaveChangers();
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/changers.dat");
			ChangerSave changerSave = new ChangerSave();
			changerSave.saveChangers();
			binaryFormatter.Serialize(fileStream, changerSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving changers");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	private void loadChangers()
	{
		if (File.Exists(saveSlot() + "/changers.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/changers.dat", FileMode.Open);
				ChangerSave obj = (ChangerSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadChangers();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading changers from file.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadChangers();
				return;
			}
		}
		EasyLoadChangers();
	}

	private void saveLevels()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/levels.dat");
			LevelSave levelSave = new LevelSave();
			levelSave.saveLevels(CharLevelManager.manage.todaysXp, CharLevelManager.manage.currentXp, CharLevelManager.manage.currentLevels);
			binaryFormatter.Serialize(fileStream, levelSave);
			fileStream.Close();
			makeABackUp(saveSlot() + "/levels.dat", saveSlot() + "/levels.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving levels");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void EasySaveLevels()
	{
		LevelSave levelSave = new LevelSave();
		levelSave.saveLevels(CharLevelManager.manage.todaysXp, CharLevelManager.manage.currentXp, CharLevelManager.manage.currentLevels);
		try
		{
			ES3.Save("levels", levelSave, saveSlot() + "/levels.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/levels.es3");
			ES3.Save("levels", levelSave, saveSlot() + "/levels.es3");
		}
	}

	public bool EasyLoadLevels()
	{
		try
		{
			if (ES3.KeyExists("levels", saveSlot() + "/levels.es3"))
			{
				LevelSave levelSave = new LevelSave();
				ES3.LoadInto("levels", saveSlot() + "/levels.es3", levelSave);
				levelSave.loadLevels();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void loadLevels()
	{
		if (File.Exists(saveSlot() + "/levels.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/levels.dat", FileMode.Open);
				LevelSave obj = (LevelSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadLevels();
				makeABackUp(saveSlot() + "/levels.dat", saveSlot() + "/levels.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading levels");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadLevelsBackup();
				return;
			}
		}
		loadLevelsBackup();
	}

	private void loadLevelsBackup()
	{
		Debug.LogWarning("Loading levels backup");
		if (File.Exists(saveSlot() + "/levels.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/levels.bak", FileMode.Open);
				LevelSave obj = (LevelSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				obj.loadLevels();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading levels backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadLevels();
				return;
			}
		}
		EasyLoadLevels();
	}

	public void EasySaveMuseum()
	{
		MuseumSave museumSave = new MuseumSave();
		museumSave.fishDonated = MuseumManager.manage.fishDonated;
		museumSave.bugDonated = MuseumManager.manage.bugsDonated;
		museumSave.underWaterCreatures = MuseumManager.manage.underWaterCreaturesDonated;
		try
		{
			ES3.Save("museumSave", museumSave, saveSlot() + "/museumSave.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/museumSave.es3");
			ES3.Save("museumSave", museumSave, saveSlot() + "/museumSave.es3");
		}
	}

	public bool EasyLoadMuseum()
	{
		try
		{
			if (ES3.KeyExists("museumSave", saveSlot() + "/museumSave.es3"))
			{
				MuseumSave museumSave = new MuseumSave();
				ES3.LoadInto("museumSave", saveSlot() + "/museumSave.es3", museumSave);
				for (int i = 0; i < museumSave.fishDonated.Length; i++)
				{
					MuseumManager.manage.fishDonated[i] = museumSave.fishDonated[i];
				}
				for (int j = 0; j < museumSave.bugDonated.Length; j++)
				{
					MuseumManager.manage.bugsDonated[j] = museumSave.bugDonated[j];
				}
				if (museumSave.underWaterCreatures != null)
				{
					for (int k = 0; k < museumSave.underWaterCreatures.Length; k++)
					{
						MuseumManager.manage.underWaterCreaturesDonated[k] = museumSave.underWaterCreatures[k];
					}
				}
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveMuseum()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/museumSave.dat");
			binaryFormatter.Serialize(fileStream, new MuseumSave
			{
				fishDonated = MuseumManager.manage.fishDonated,
				bugDonated = MuseumManager.manage.bugsDonated,
				underWaterCreatures = MuseumManager.manage.underWaterCreaturesDonated
			});
			fileStream.Close();
			makeABackUp(saveSlot() + "/museumSave.dat", saveSlot() + "/museumSave.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving museum");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void loadMuseum()
	{
		if (File.Exists(saveSlot() + "/museumSave.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/museumSave.dat", FileMode.Open);
				MuseumSave museumSave = (MuseumSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				fileStream.Close();
				makeABackUp(saveSlot() + "/museumSave.dat", saveSlot() + "/museumSave.bak");
				for (int i = 0; i < museumSave.fishDonated.Length; i++)
				{
					MuseumManager.manage.fishDonated[i] = museumSave.fishDonated[i];
				}
				for (int j = 0; j < museumSave.bugDonated.Length; j++)
				{
					MuseumManager.manage.bugsDonated[j] = museumSave.bugDonated[j];
				}
				if (museumSave.underWaterCreatures != null)
				{
					for (int k = 0; k < museumSave.underWaterCreatures.Length; k++)
					{
						MuseumManager.manage.underWaterCreaturesDonated[k] = museumSave.underWaterCreatures[k];
					}
				}
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading museum save.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadMuseumBackup();
				return;
			}
		}
		loadMuseumBackup();
	}

	public void loadMuseumBackup()
	{
		if (File.Exists(saveSlot() + "/museumSave.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/museumSave.bak", FileMode.Open);
				MuseumSave museumSave = (MuseumSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				for (int i = 0; i < museumSave.fishDonated.Length; i++)
				{
					MuseumManager.manage.fishDonated[i] = museumSave.fishDonated[i];
				}
				for (int j = 0; j < museumSave.bugDonated.Length; j++)
				{
					MuseumManager.manage.bugsDonated[j] = museumSave.bugDonated[j];
				}
				if (museumSave.underWaterCreatures != null)
				{
					for (int k = 0; k < museumSave.underWaterCreatures.Length; k++)
					{
						MuseumManager.manage.underWaterCreaturesDonated[k] = museumSave.underWaterCreatures[k];
					}
				}
				fileStream.Close();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading museum backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadMuseum();
				return;
			}
		}
		EasyLoadMuseum();
	}

	public void EasySaveBulletinBoard()
	{
		BulletinBoardSave bulletinBoardSave = new BulletinBoardSave();
		bulletinBoardSave.allPosts = BulletinBoard.board.attachedPosts.ToArray();
		try
		{
			ES3.Save("bboard", bulletinBoardSave, saveSlot() + "/bboard.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/bboard.es3");
			ES3.Save("bboard", bulletinBoardSave, saveSlot() + "/bboard.es3");
		}
	}

	public bool EasyLoadBulletinBoard()
	{
		try
		{
			if (ES3.KeyExists("bboard", saveSlot() + "/bboard.es3"))
			{
				BulletinBoardSave bulletinBoardSave = new BulletinBoardSave();
				ES3.LoadInto("bboard", saveSlot() + "/bboard.es3", bulletinBoardSave);
				fillBoardSave(bulletinBoardSave);
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveBulletinBoard()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/bboard.dat");
			binaryFormatter.Serialize(fileStream, new BulletinBoardSave
			{
				allPosts = BulletinBoard.board.attachedPosts.ToArray()
			});
			fileStream.Close();
			makeABackUp(saveSlot() + "/bboard.dat", saveSlot() + "/bboard.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error reading bulletinboard save.");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void loadBulletinBoard()
	{
		if (File.Exists(saveSlot() + "/bboard.dat"))
		{
			BulletinBoard.board.onLocalConnect();
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/bboard.dat", FileMode.Open);
				BulletinBoardSave boardSave = (BulletinBoardSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				fileStream.Close();
				fillBoardSave(boardSave);
				makeABackUp(saveSlot() + "/bboard.dat", saveSlot() + "/bboard.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading bulletinboard save.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadBulletinBoardBackUp();
				return;
			}
		}
		loadBulletinBoardBackUp();
	}

	public void loadBulletinBoardBackUp()
	{
		Debug.LogWarning("Looking for bulletinboard backup.");
		if (File.Exists(saveSlot() + "/bboard.bak"))
		{
			BulletinBoard.board.onLocalConnect();
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/bboard.bak", FileMode.Open);
				BulletinBoardSave boardSave = (BulletinBoardSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				fileStream.Close();
				fillBoardSave(boardSave);
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading backup bulletinboard save.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadBulletinBoard();
				return;
			}
		}
		EasyLoadBulletinBoard();
	}

	private void fillBoardSave(BulletinBoardSave boardSave)
	{
		for (int i = 0; i < boardSave.allPosts.Length; i++)
		{
			boardSave.allPosts[i].populateOnLoad();
			BulletinBoard.board.attachedPosts.Add(boardSave.allPosts[i]);
		}
		if (BulletinBoard.board.attachedPosts.Count > 4)
		{
			List<PostOnBoard> list = new List<PostOnBoard>();
			for (int j = 0; j < 4; j++)
			{
				list.Add(BulletinBoard.board.attachedPosts[BulletinBoard.board.attachedPosts.Count - 1 - j]);
			}
			BulletinBoard.board.attachedPosts = list;
		}
		for (int k = 0; k < BulletinBoard.board.attachedPosts.Count; k++)
		{
			if (BulletinBoard.board.attachedPosts[k].checkIfAccepted() && !BulletinBoard.board.attachedPosts[k].checkIfExpired() && !BulletinBoard.board.attachedPosts[k].completed)
			{
				RenderMap.map.createTaskIcon(BulletinBoard.board.attachedPosts[k]);
			}
		}
	}

	private void EasySaveMail()
	{
		MailSave mailSave = new MailSave();
		mailSave.allMail = MailManager.manage.mailInBox.ToArray();
		mailSave.tomorrowsMail = MailManager.manage.tomorrowsLetters.ToArray();
		try
		{
			ES3.Save("mail", mailSave, saveSlot() + "/mail.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/mail.es3");
			ES3.Save("mail", mailSave, saveSlot() + "/mail.es3");
		}
	}

	public bool EasyLoadMail()
	{
		try
		{
			if (ES3.KeyExists("mail", saveSlot() + "/mail.es3"))
			{
				MailSave mailSave = new MailSave();
				ES3.LoadInto("mail", saveSlot() + "/mail.es3", mailSave);
				for (int i = 0; i < mailSave.allMail.Length; i++)
				{
					MailManager.manage.mailInBox.Add(mailSave.allMail[i]);
				}
				if (mailSave.tomorrowsMail != null)
				{
					for (int j = 0; j < mailSave.tomorrowsMail.Length; j++)
					{
						MailManager.manage.tomorrowsLetters.Add(mailSave.tomorrowsMail[j]);
					}
				}
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveMail()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/mail.dat");
			binaryFormatter.Serialize(fileStream, new MailSave
			{
				allMail = MailManager.manage.mailInBox.ToArray(),
				tomorrowsMail = MailManager.manage.tomorrowsLetters.ToArray()
			});
			fileStream.Close();
			makeABackUp(saveSlot() + "/mail.dat", saveSlot() + "/mail.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("error saving mail");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void loadMail()
	{
		if (File.Exists(saveSlot() + "/mail.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/mail.dat", FileMode.Open);
				MailSave mailSave = (MailSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				for (int i = 0; i < mailSave.allMail.Length; i++)
				{
					MailManager.manage.mailInBox.Add(mailSave.allMail[i]);
				}
				if (mailSave.tomorrowsMail != null)
				{
					for (int j = 0; j < mailSave.tomorrowsMail.Length; j++)
					{
						MailManager.manage.tomorrowsLetters.Add(mailSave.tomorrowsMail[j]);
					}
				}
				fileStream.Close();
				makeABackUp(saveSlot() + "/mail.dat", saveSlot() + "/mail.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("error loading mail");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadBackupMail();
				return;
			}
		}
		loadBackupMail();
	}

	public void loadBackupMail()
	{
		if (!File.Exists(saveSlot() + "/mail.bak"))
		{
			return;
		}
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Open(saveSlot() + "/mail.bak", FileMode.Open);
			MailSave mailSave = (MailSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
			for (int i = 0; i < mailSave.allMail.Length; i++)
			{
				MailManager.manage.mailInBox.Add(mailSave.allMail[i]);
			}
			if (mailSave.tomorrowsMail != null)
			{
				for (int j = 0; j < mailSave.tomorrowsMail.Length; j++)
				{
					MailManager.manage.tomorrowsLetters.Add(mailSave.tomorrowsMail[j]);
				}
			}
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("error loading backup mail");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	private void EasySaveHouses()
	{
		HouseListSave houseListSave = new HouseListSave();
		houseListSave.save();
		try
		{
			ES3.Save("houseSave", houseListSave, saveSlot() + "/houseSave.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/houseSave.es3");
			ES3.Save("houseSave", houseListSave, saveSlot() + "/houseSave.es3");
		}
	}

	public bool EasyLoadHouses()
	{
		try
		{
			if (ES3.KeyExists("houseSave", saveSlot() + "/houseSave.es3"))
			{
				HouseListSave houseListSave = new HouseListSave();
				ES3.LoadInto("houseSave", saveSlot() + "/houseSave.es3", houseListSave);
				houseListSave.load();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public void saveHouse()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/houseSave.dat");
			HouseListSave houseListSave = new HouseListSave();
			houseListSave.save();
			binaryFormatter.Serialize(fileStream, houseListSave);
			fileStream.Close();
			makeABackUp(saveSlot() + "/houseSave.dat", saveSlot() + "/houseSave.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving house ");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void loadHouse()
	{
		if (File.Exists(saveSlot() + "/houseSave.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/houseSave.dat", FileMode.Open);
				((HouseListSave)binaryFormatter.Deserialize(new BufferedStream(fileStream))).load();
				fileStream.Close();
				makeABackUp(saveSlot() + "/houseSave.dat", saveSlot() + "/houseSave.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading house from file.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadHouseBackup();
				return;
			}
		}
		loadHouseBackup();
	}

	public void loadHouseBackup()
	{
		Debug.LogWarning("Loading house backup");
		if (File.Exists(saveSlot() + "/houseSave.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/houseSave.bak", FileMode.Open);
				((HouseListSave)binaryFormatter.Deserialize(new BufferedStream(fileStream))).load();
				fileStream.Close();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading house backup.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadHouses();
				return;
			}
		}
		EasyLoadHouses();
	}

	private void EasySaveMapIcons()
	{
		MapIconSave mapIconSave = new MapIconSave();
		mapIconSave.saveIcons();
		try
		{
			ES3.Save("mapIcons", mapIconSave, saveSlot() + "/mapIcons.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/mapIcons.es3");
			ES3.Save("mapIcons", mapIconSave, saveSlot() + "/mapIcons.es3");
		}
	}

	public bool EasyLoadMapIcons()
	{
		try
		{
			if (ES3.KeyExists("mapIcons", saveSlot() + "/mapIcons.es3"))
			{
				MapIconSave mapIconSave = new MapIconSave();
				ES3.LoadInto("mapIcons", saveSlot() + "/mapIcons.es3", mapIconSave);
				mapIconSave.loadIcons();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveMapIcons()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/mapIcons.dat");
			MapIconSave mapIconSave = new MapIconSave();
			mapIconSave.saveIcons();
			binaryFormatter.Serialize(fileStream, mapIconSave);
			fileStream.Close();
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving map icons ");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void loadMapIcons()
	{
		FileStream fileStream = null;
		try
		{
			if (File.Exists(saveSlot() + "/mapIcons.dat"))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/mapIcons.dat", FileMode.Open);
				((MapIconSave)binaryFormatter.Deserialize(new BufferedStream(fileStream))).loadIcons();
				fileStream.Close();
			}
		}
		catch (Exception)
		{
			Debug.LogWarning("Error loading map icons ");
			if (fileStream != null)
			{
				fileStream.Close();
			}
			EasyLoadMapIcons();
		}
	}

	private void EasySaveDrops()
	{
		DropSaves dropSaves = new DropSaves();
		dropSaves.saveDrops();
		try
		{
			ES3.Save("drops", dropSaves, saveSlot() + "/drops.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/drops.es3");
			ES3.Save("drops", dropSaves, saveSlot() + "/drops.es3");
		}
	}

	public bool EasyLoadDrops()
	{
		try
		{
			if (ES3.KeyExists("drops", saveSlot() + "/drops.es3"))
			{
				DropSaves dropSaves = new DropSaves();
				ES3.LoadInto("drops", saveSlot() + "/drops.es3", dropSaves);
				dropSaves.loadDrops();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveDrops()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/drops.dat");
			DropSaves dropSaves = new DropSaves();
			dropSaves.saveDrops();
			binaryFormatter.Serialize(fileStream, dropSaves);
			fileStream.Close();
			makeABackUp(saveSlot() + "/drops.dat", saveSlot() + "/drops.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error Saving Drops");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void loadDrops()
	{
		if (File.Exists(saveSlot() + "/drops.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/drops.dat", FileMode.Open);
				((DropSaves)binaryFormatter.Deserialize(new BufferedStream(fileStream))).loadDrops();
				fileStream.Close();
				makeABackUp(saveSlot() + "/drops.dat", saveSlot() + "/drops.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading drops");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadDropsBackup();
				return;
			}
		}
		loadDropsBackup();
	}

	public void loadDropsBackup()
	{
		Debug.LogWarning("Loading backup Drops");
		if (File.Exists(saveSlot() + "/drops.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/drops.bak", FileMode.Open);
				((DropSaves)binaryFormatter.Deserialize(new BufferedStream(fileStream))).loadDrops();
				fileStream.Close();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading drops backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadDrops();
				return;
			}
		}
		EasyLoadDrops();
	}

	private void EasySaveCarriables()
	{
		CarrySave carrySave = new CarrySave();
		carrySave.saveAllCarryable();
		try
		{
			ES3.Save("carry", carrySave, saveSlot() + "/carry.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/carry.es3");
			ES3.Save("carry", carrySave, saveSlot() + "/carry.es3");
		}
	}

	public bool EasyLoadCarriables()
	{
		try
		{
			if (ES3.KeyExists("carry", saveSlot() + "/carry.es3"))
			{
				CarrySave carrySave = new CarrySave();
				ES3.LoadInto("carry", saveSlot() + "/carry.es3", carrySave);
				carrySave.loadAllCarryable();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void saveCarriables()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/carry.dat");
			CarrySave carrySave = new CarrySave();
			carrySave.saveAllCarryable();
			binaryFormatter.Serialize(fileStream, carrySave);
			fileStream.Close();
			makeABackUp(saveSlot() + "/carry.dat", saveSlot() + "/carry.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving carrables");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void loadCarriables()
	{
		if (File.Exists(saveSlot() + "/carry.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/carry.dat", FileMode.Open);
				CarrySave obj = (CarrySave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				fileStream.Close();
				obj.loadAllCarryable();
				makeABackUp(saveSlot() + "/carry.dat", saveSlot() + "/carry.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading carryables");
				if (fileStream != null)
				{
					Debug.LogWarning("File closed");
					fileStream.Close();
				}
				loadCarriablesBackup();
				return;
			}
		}
		loadCarriablesBackup();
	}

	public void loadCarriablesBackup()
	{
		Debug.LogWarning("Loading carryables backup");
		if (File.Exists(saveSlot() + "/carry.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/carry.bak", FileMode.Open);
				CarrySave obj = (CarrySave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				fileStream.Close();
				obj.loadAllCarryable();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading carryables backup");
				if (fileStream != null)
				{
					Debug.LogWarning("File closed");
					fileStream.Close();
				}
				EasyLoadCarriables();
				return;
			}
		}
		EasyLoadCarriables();
	}

	public void EasySaveFencedOffAnimals(bool endOfDaySave)
	{
		FencedOffAnimalSave fencedOffAnimalSave = new FencedOffAnimalSave();
		fencedOffAnimalSave.saveAnimals(endOfDaySave);
		try
		{
			ES3.Save("animalDetails", fencedOffAnimalSave, saveSlot() + "/animalDetails.es3");
		}
		catch
		{
			ES3.DeleteFile(saveSlot() + "/animalDetails.es3");
			ES3.Save("animalDetails", fencedOffAnimalSave, saveSlot() + "/animalDetails.es3");
		}
	}

	public bool EasyLoadFencedOffAnimals()
	{
		try
		{
			if (ES3.KeyExists("animalDetails", saveSlot() + "/animalDetails.es3"))
			{
				FencedOffAnimalSave fencedOffAnimalSave = new FencedOffAnimalSave();
				ES3.LoadInto("animalDetails", saveSlot() + "/animalDetails.es3", fencedOffAnimalSave);
				fencedOffAnimalSave.loadAnimals();
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public void saveFencedOffAnimals(bool endOfDaySave)
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(saveSlot() + "/animalDetails.dat");
			FencedOffAnimalSave fencedOffAnimalSave = new FencedOffAnimalSave();
			fencedOffAnimalSave.saveAnimals(endOfDaySave);
			binaryFormatter.Serialize(fileStream, fencedOffAnimalSave);
			fileStream.Close();
			makeABackUp(saveSlot() + "/animalDetails.dat", saveSlot() + "/animalDetails.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving animals");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void loadFencedOffAnimals()
	{
		if (File.Exists(saveSlot() + "/animalDetails.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/animalDetails.dat", FileMode.Open);
				((FencedOffAnimalSave)binaryFormatter.Deserialize(new BufferedStream(fileStream))).loadAnimals();
				fileStream.Close();
				makeABackUp(saveSlot() + "/animalDetails.dat", saveSlot() + "/animalDetails.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading animal details.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadFencedOffAnimalsBackup();
				return;
			}
		}
		loadFencedOffAnimalsBackup();
	}

	public void loadFencedOffAnimalsBackup()
	{
		if (File.Exists(saveSlot() + "/animalDetails.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/animalDetails.bak", FileMode.Open);
				((FencedOffAnimalSave)binaryFormatter.Deserialize(new BufferedStream(fileStream))).loadAnimals();
				fileStream.Close();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error reading animal details backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadFencedOffAnimals();
				return;
			}
		}
		EasyLoadFencedOffAnimals();
	}

	public void LoadInv()
	{
		if (File.Exists(saveSlot() + "/playerInfo.dat"))
		{
			FileStream fileStream = null;
			PlayerInv playerInv;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/playerInfo.dat", FileMode.Open);
				playerInv = (PlayerInv)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				makeABackUp(saveSlot() + "/playerInfo.dat", saveSlot() + "/playerInfo.bak");
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading player details.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadInvBackup();
				return;
			}
			Inventory.inv.changeWalletToLoad(playerInv.money);
			BankMenu.menu.accountBalance = playerInv.bankBalance;
			if (playerInv.hair < 0)
			{
				playerInv.hair = Mathf.Abs(playerInv.hair + 1);
			}
			Inventory.inv.playerHair = playerInv.hair;
			Inventory.inv.playerHairColour = playerInv.hairColour;
			Inventory.inv.playerEyes = playerInv.eyeStyle;
			Inventory.inv.nose = playerInv.nose;
			Inventory.inv.mouth = playerInv.mouth;
			Inventory.inv.playerEyeColor = playerInv.eyeColour;
			Inventory.inv.skinTone = playerInv.skinTone;
			Inventory.inv.playerName = playerInv.playerName;
			Inventory.inv.islandName = playerInv.islandName;
			EquipWindow.equip.hatSlot.updateSlotContentsAndRefresh(playerInv.head, 1);
			EquipWindow.equip.faceSlot.updateSlotContentsAndRefresh(playerInv.face, 1);
			EquipWindow.equip.shirtSlot.updateSlotContentsAndRefresh(playerInv.body, 1);
			EquipWindow.equip.pantsSlot.updateSlotContentsAndRefresh(playerInv.pants, 1);
			EquipWindow.equip.shoeSlot.updateSlotContentsAndRefresh(playerInv.shoes, 1);
			StartCoroutine(EquipWindow.equip.wearingMinersHelmet());
			if (playerInv.catalogue != null)
			{
				for (int i = 0; i < playerInv.catalogue.Length; i++)
				{
					CatalogueManager.manage.collectedItem[i] = playerInv.catalogue[i];
				}
			}
			StatusManager.manage.loadStatus(playerInv.health, playerInv.healthMax, playerInv.stamina, playerInv.staminaMax);
			for (int j = 0; j < playerInv.itemsInInvSlots.Length; j++)
			{
				Inventory.inv.invSlots[j].itemNo = playerInv.itemsInInvSlots[j];
				Inventory.inv.invSlots[j].stack = playerInv.stacksInSlots[j];
				Inventory.inv.invSlots[j].updateSlotContentsAndRefresh(playerInv.itemsInInvSlots[j], playerInv.stacksInSlots[j]);
			}
			loadNpcRelations();
			loadQuests();
			loadRecipesUnlocked();
			loadLicences();
			loadPedia();
			loadLevels();
		}
		else
		{
			MonoBehaviour.print("Load backup here.");
			loadInvBackup();
		}
		MonoBehaviour.print("loaded inv");
	}

	public void loadInvBackup()
	{
		MonoBehaviour.print("looking for inv backup");
		if (File.Exists(saveSlot() + "/playerInfo.bak"))
		{
			FileStream fileStream = null;
			PlayerInv playerInv;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/playerInfo.bak", FileMode.Open);
				playerInv = (PlayerInv)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading player backup.");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadInv();
				return;
			}
			Inventory.inv.changeWalletToLoad(playerInv.money);
			BankMenu.menu.accountBalance = playerInv.bankBalance;
			if (playerInv.hair < 0)
			{
				playerInv.hair = Mathf.Abs(playerInv.hair + 1);
			}
			Inventory.inv.playerHair = playerInv.hair;
			Inventory.inv.playerHairColour = playerInv.hairColour;
			Inventory.inv.playerEyes = playerInv.eyeStyle;
			Inventory.inv.nose = playerInv.nose;
			Inventory.inv.mouth = playerInv.mouth;
			Inventory.inv.playerEyeColor = playerInv.eyeColour;
			Inventory.inv.skinTone = playerInv.skinTone;
			Inventory.inv.playerName = playerInv.playerName;
			Inventory.inv.islandName = playerInv.islandName;
			EquipWindow.equip.hatSlot.updateSlotContentsAndRefresh(playerInv.head, 1);
			EquipWindow.equip.faceSlot.updateSlotContentsAndRefresh(playerInv.face, 1);
			EquipWindow.equip.shirtSlot.updateSlotContentsAndRefresh(playerInv.body, 1);
			EquipWindow.equip.pantsSlot.updateSlotContentsAndRefresh(playerInv.pants, 1);
			EquipWindow.equip.shoeSlot.updateSlotContentsAndRefresh(playerInv.shoes, 1);
			StartCoroutine(EquipWindow.equip.wearingMinersHelmet());
			if (playerInv.catalogue != null)
			{
				for (int i = 0; i < playerInv.catalogue.Length; i++)
				{
					CatalogueManager.manage.collectedItem[i] = playerInv.catalogue[i];
				}
			}
			StatusManager.manage.loadStatus(playerInv.health, playerInv.healthMax, playerInv.stamina, playerInv.staminaMax);
			for (int j = 0; j < playerInv.itemsInInvSlots.Length; j++)
			{
				Inventory.inv.invSlots[j].itemNo = playerInv.itemsInInvSlots[j];
				Inventory.inv.invSlots[j].stack = playerInv.stacksInSlots[j];
				Inventory.inv.invSlots[j].updateSlotContentsAndRefresh(playerInv.itemsInInvSlots[j], playerInv.stacksInSlots[j]);
			}
			loadNpcRelations();
			loadQuests();
			loadRecipesUnlocked();
			loadLicences();
			loadPedia();
			loadLevels();
		}
		else
		{
			EasyLoadInv();
		}
	}

	private IEnumerator saveOverFrames(bool isEndOfDaySave)
	{
		if (NetworkMapSharer.share.nextDayIsReady)
		{
			loadingScreen.loadingBarOnlyAppear();
		}
		string path = saveSlot() + "/savefile.dat";
		int num = 0;
		using (BinaryWriter w = new BinaryWriter(File.OpenWrite(path)))
		{
			w.Write(GenerateMap.generate.seed);
			for (int y = 0; y < 100; y++)
			{
				for (int x = 0; x < 100; x++)
				{
					w.Write(WorldManager.manageWorld.chunkChangedMap[x, y]);
					if (!WorldManager.manageWorld.chunkChangedMap[x, y])
					{
						continue;
					}
					w.Write(WorldManager.manageWorld.changedMapHeight[x, y]);
					w.Write(WorldManager.manageWorld.changedMapWater[x, y]);
					w.Write(WorldManager.manageWorld.changedMapOnTile[x, y]);
					w.Write(WorldManager.manageWorld.changedMapTileType[x, y]);
					for (int ydif = 0; ydif < 10; ydif++)
					{
						for (int i = 0; i < 10; i++)
						{
							if (WorldManager.manageWorld.changedMapOnTile[x, y])
							{
								w.Write((short)WorldManager.manageWorld.onTileMap[x * 10 + i, y * 10 + ydif]);
								if (WorldManager.manageWorld.onTileMap[x * 10 + i, y * 10 + ydif] > 1 && (bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[x * 10 + i, y * 10 + ydif]].tileObjectFurniture)
								{
									WorldManager.manageWorld.onTileStatusMap[x * 10 + i, y * 10 + ydif] = 0;
								}
								w.Write((short)WorldManager.manageWorld.onTileStatusMap[x * 10 + i, y * 10 + ydif]);
								w.Write((short)WorldManager.manageWorld.rotationMap[x * 10 + i, y * 10 + ydif]);
							}
							if (WorldManager.manageWorld.changedMapHeight[x, y])
							{
								w.Write((short)WorldManager.manageWorld.heightMap[x * 10 + i, y * 10 + ydif]);
							}
							if (WorldManager.manageWorld.changedMapTileType[x, y])
							{
								w.Write((short)WorldManager.manageWorld.tileTypeMap[x * 10 + i, y * 10 + ydif]);
								w.Write((short)WorldManager.manageWorld.tileTypeStatusMap[x * 10 + i, y * 10 + ydif]);
							}
							if (WorldManager.manageWorld.changedMapWater[x, y])
							{
								w.Write(WorldManager.manageWorld.waterMap[x * 10 + i, y * 10 + ydif]);
							}
						}
						if (num <= 0)
						{
							loadingScreen.showPercentage((float)(y * 10) + (float)ydif / 1000f);
							yield return null;
							num = 50;
						}
					}
				}
				if (num <= 0)
				{
					loadingScreen.showPercentage((float)(y * 10) / 1000f);
					yield return null;
					num = 50;
				}
			}
		}
		saveChangers();
		if (!isEndOfDaySave)
		{
			MonoBehaviour.print("Doing fence check on save");
			yield return StartCoroutine(WorldManager.manageWorld.fenceCheck());
		}
		for (int j = 0; j < NetworkNavMesh.nav.charsConnected.Count; j++)
		{
			CharPickUp component = NetworkNavMesh.nav.charsConnected[j].GetComponent<CharPickUp>();
			if ((bool)component)
			{
				if (component.myInteract.insideHouseDetails != null)
				{
					component.myInteract.insideHouseDetails.houseMapOnTileStatus[component.sittingXpos, component.sittingYPos] = sittingPosOriginals[j];
					MonoBehaviour.print("Setting sleeping pos inside house back to " + sittingPosOriginals[j]);
				}
				else
				{
					WorldManager.manageWorld.onTileStatusMap[component.sittingXpos, component.sittingYPos] = sittingPosOriginals[j];
					MonoBehaviour.print("Setting sleeping pos back to " + sittingPosOriginals[j]);
				}
			}
		}
		yield return new WaitForSeconds(2f);
		if (NetworkMapSharer.share.nextDayIsReady)
		{
			loadingScreen.completed();
		}
		NetworkMapSharer.share.NetworknextDayIsReady = true;
		if (quitAfterSave)
		{
			Application.Quit();
		}
	}

	public void saveVersionNumber()
	{
		using (BinaryWriter binaryWriter = new BinaryWriter(File.OpenWrite(saveSlot() + "/versionCheck.dat")))
		{
			binaryWriter.Write((short)WorldManager.manageWorld.versionNumber);
		}
	}

	public void loadVersionNumber()
	{
		using (BinaryReader binaryReader = new BinaryReader(File.OpenRead(saveSlot() + "/versionCheck.dat")))
		{
			MonoBehaviour.print("Lasted version saved " + (int)binaryReader.ReadInt16());
			int versionNumber = WorldManager.manageWorld.versionNumber;
		}
	}

	public IEnumerator loadOverFrames()
	{
		loadingScreen.appear("loading", true);
		yield return new WaitForSeconds(1f);
		if (!new DirectoryInfo(saveSlot()).Exists)
		{
			yield break;
		}
		loadDate();
		loadTown();
		loadItemsOnTop();
		yield return null;
		string path = saveSlot() + "/savefile.dat";
		int frameCounter = 0;
		using (BinaryReader r = new BinaryReader(File.OpenRead(path)))
		{
			GenerateMap.generate.seed = r.ReadInt32();
			yield return StartCoroutine(GenerateMap.generate.generateNewMap(GenerateMap.generate.seed));
			yield return null;
			for (int y = 0; y < 100; y++)
			{
				for (int x = 0; x < 100; x++)
				{
					frameCounter--;
					WorldManager.manageWorld.chunkChangedMap[x, y] = r.ReadBoolean();
					if (!WorldManager.manageWorld.chunkChangedMap[x, y])
					{
						continue;
					}
					WorldManager.manageWorld.changedMapHeight[x, y] = r.ReadBoolean();
					WorldManager.manageWorld.changedMapWater[x, y] = r.ReadBoolean();
					WorldManager.manageWorld.changedMapOnTile[x, y] = r.ReadBoolean();
					WorldManager.manageWorld.changedMapTileType[x, y] = r.ReadBoolean();
					for (int ydif = 0; ydif < 10; ydif++)
					{
						for (int i = 0; i < 10; i++)
						{
							if (WorldManager.manageWorld.changedMapOnTile[x, y])
							{
								WorldManager.manageWorld.onTileMap[x * 10 + i, y * 10 + ydif] = r.ReadInt16();
								WorldManager.manageWorld.onTileStatusMap[x * 10 + i, y * 10 + ydif] = r.ReadInt16();
								WorldManager.manageWorld.rotationMap[x * 10 + i, y * 10 + ydif] = r.ReadInt16();
								if (WorldManager.manageWorld.onTileMap[x * 10 + i, y * 10 + ydif] > -1 && (bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[x * 10 + i, y * 10 + ydif]].tileObjectConnect && WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[x * 10 + i, y * 10 + ydif]].tileObjectConnect.isFence)
								{
									WorldManager.manageWorld.placeFenceInChunk(x * 10 + i, y * 10 + ydif);
								}
								if (WorldManager.manageWorld.onTileMap[x * 10 + i, y * 10 + ydif] == 132 || WorldManager.manageWorld.onTileMap[x * 10 + i, y * 10 + ydif] == 318)
								{
									WorldManager.manageWorld.sprinkerContinuesToWater(x * 10 + i, y * 10 + ydif);
								}
							}
							if (WorldManager.manageWorld.changedMapHeight[x, y])
							{
								WorldManager.manageWorld.heightMap[x * 10 + i, y * 10 + ydif] = r.ReadInt16();
							}
							if (WorldManager.manageWorld.changedMapTileType[x, y])
							{
								WorldManager.manageWorld.tileTypeMap[x * 10 + i, y * 10 + ydif] = r.ReadInt16();
								WorldManager.manageWorld.tileTypeStatusMap[x * 10 + i, y * 10 + ydif] = r.ReadInt16();
							}
							if (WorldManager.manageWorld.changedMapWater[x, y])
							{
								WorldManager.manageWorld.waterMap[x * 10 + i, y * 10 + ydif] = r.ReadBoolean();
								if (WorldManager.manageWorld.heightMap[x * 10 + i, y * 10 + ydif] > 0)
								{
									WorldManager.manageWorld.waterMap[x * 10 + i, y * 10 + ydif] = false;
								}
							}
						}
						if (frameCounter <= 0)
						{
							loadingScreen.showPercentage(0.5f + (float)(y * 10 + ydif) / 1000f / 2f);
							yield return null;
							frameCounter = 3;
						}
					}
				}
				if (frameCounter <= 0)
				{
					loadingScreen.showPercentage(0.5f + (float)y / 100f / 2f);
					yield return null;
					frameCounter = 5;
				}
			}
		}
		loadHouse();
		MuseumManager.manage.loadMuseum();
		yield return StartCoroutine(WorldManager.manageWorld.fenceCheck());
		GenerateMap.generate.onFileLoaded();
		if (saveOrLoad.DoesInvSaveExist())
		{
			saveOrLoad.LoadInv();
		}
		loadDeeds();
		loadTownStatus();
		loadMapIcons();
		loadFencedOffAnimals();
		loadVersionNumber();
		loadingComplete = true;
		loadChangers();
	}

	public void createPhotoDir()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(saveSlot() + "/Photos");
		if (!directoryInfo.Exists)
		{
			Debug.Log("Creating subdirectory");
			directoryInfo.Create();
		}
	}

	public void DeleteSave(int saveSlotToDelete)
	{
		setSlotToLoad(saveSlotToDelete);
		if (new DirectoryInfo(saveSlot()).Exists)
		{
			Debug.Log("Folder Deleted");
			Directory.Delete(saveSlot(), true);
		}
	}

	public bool DoesSaveExist()
	{
		if (new DirectoryInfo(saveSlot()).Exists)
		{
			return true;
		}
		MonoBehaviour.print("NO slot for folder found");
		return false;
	}

	public void setSlotToLoad(int slotId)
	{
		saveSlotToLoad = slotId;
	}

	public PlayerInv getSaveDetailsForFileButton(int slotToCheck)
	{
		FileStream fileStream = null;
		try
		{
			saveSlotToLoad = slotToCheck;
			if (new DirectoryInfo(saveSlot()).Exists && File.Exists(saveSlot() + "/playerInfo.dat"))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/playerInfo.dat", FileMode.Open);
				PlayerInv result = (PlayerInv)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				return result;
			}
		}
		catch (Exception)
		{
			Debug.LogWarning("Error getting player details for save button. Trying backup.");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
		try
		{
			saveSlotToLoad = slotToCheck;
			if (new DirectoryInfo(saveSlot()).Exists && File.Exists(saveSlot() + "/playerInfo.bak"))
			{
				BinaryFormatter binaryFormatter2 = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/playerInfo.bak", FileMode.Open);
				PlayerInv result2 = (PlayerInv)binaryFormatter2.Deserialize(fileStream);
				fileStream.Close();
				return result2;
			}
		}
		catch (Exception)
		{
			Debug.LogWarning("Error getting player details for save button");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
		return EasyInvForLoadSlot();
	}

	public DateSave getSaveDateDetailsForButton(int slotToCheck)
	{
		FileStream fileStream = null;
		try
		{
			saveSlotToLoad = slotToCheck;
			if (new DirectoryInfo(saveSlot()).Exists && File.Exists(saveSlot() + "/date.dat"))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(saveSlot() + "/date.dat", FileMode.Open);
				DateSave result = (DateSave)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				return result;
			}
		}
		catch (Exception)
		{
			Debug.LogWarning("Error getting date file.");
			if (fileStream != null)
			{
				fileStream.Close();
			}
			return null;
		}
		return null;
	}

	public bool DoesHouseSaveExist()
	{
		if (File.Exists(saveSlot() + "/houseSave.dat"))
		{
			return true;
		}
		return false;
	}

	public bool DoesInvSaveExist()
	{
		if (File.Exists(saveSlot() + "/playerInfo.dat"))
		{
			return true;
		}
		if (File.Exists(saveSlot() + "/playerInfo.bak"))
		{
			return true;
		}
		if (File.Exists(saveSlot() + "/playerInfo.es3"))
		{
			return true;
		}
		return false;
	}
}
