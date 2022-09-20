using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Mirror;
using UnityEngine;

public class FarmAnimalManager : NetworkBehaviour
{
	public static FarmAnimalManager manage;

	public List<AnimalHouse> animalHouses = new List<AnimalHouse>();

	public List<FarmAnimalDetails> farmAnimalDetails = new List<FarmAnimalDetails>();

	public List<FarmAnimal> activeAnimalAgents = new List<FarmAnimal>();

	public List<FarmAnimalHouseFloor> houseFloors = new List<FarmAnimalHouseFloor>();

	private int selectedNo;

	private void Awake()
	{
		manage = this;
	}

	public override void OnStartServer()
	{
		loadAnimalHouses();
		loadFarmAnimalDetails();
		loadAgentsForAllFarmAnimals();
		NetworkMapSharer.share.returnAgents.AddListener(loadAgentsOnMoveOverworld);
	}

	public void newDayCheck()
	{
		if (!base.isServer)
		{
			return;
		}
		foreach (FarmAnimalDetails farmAnimalDetail in farmAnimalDetails)
		{
			farmAnimalDetail.endOfDayCheck();
		}
	}

	public void loadAgentsOnMoveOverworld()
	{
		if (RealWorldTimeLight.time.underGround)
		{
			loadAgentsForAllFarmAnimals();
		}
		else
		{
			returnAllAgents();
		}
	}

	public void returnAllAgents()
	{
		for (int i = 0; i < farmAnimalDetails.Count; i++)
		{
			if (activeAnimalAgents[farmAnimalDetails[i].agentListId] != null)
			{
				farmAnimalDetails[i].setPosition(activeAnimalAgents[farmAnimalDetails[i].agentListId].transform.position);
			}
		}
		for (int j = 0; j < activeAnimalAgents.Count; j++)
		{
			if (activeAnimalAgents[j] != null)
			{
				NetworkNavMesh.nav.UnSpawnAnAnimal(activeAnimalAgents[j].GetComponent<AnimalAI>(), false);
				activeAnimalAgents[j] = null;
			}
		}
	}

	public void loadAgentsForAllFarmAnimals()
	{
		foreach (FarmAnimalDetails farmAnimalDetail in farmAnimalDetails)
		{
			NetworkNavMesh.nav.SpawnFarmAnimal(farmAnimalDetail);
		}
	}

	public int addActiveAgentAndReturnIndexId(FarmAnimal toAdd)
	{
		for (int i = 0; i < activeAnimalAgents.Count; i++)
		{
			if (activeAnimalAgents[i] == null)
			{
				activeAnimalAgents[i] = toAdd;
				return i;
			}
		}
		activeAnimalAgents.Add(toAdd);
		return activeAnimalAgents.Count - 1;
	}

	public void sellAnimal(FarmAnimalDetails sellAnimal)
	{
		sellAnimal.onAnimalDeath();
	}

	public void checkLiveAgentAfterFeed(FarmAnimalDetails details)
	{
		FarmAnimal farmAnimal = activeAnimalAgents[details.agentListId];
		if (details.ateYesterDay)
		{
			if ((bool)farmAnimal.dropper && !details.hasDoneDrop)
			{
				farmAnimal.dropper.dropTheDrop();
			}
			if ((bool)farmAnimal.canBeHarvested)
			{
				farmAnimal.canBeHarvested.setHarvestReadyServer();
			}
		}
	}

	public int getFreeAgentListId()
	{
		for (int i = 0; i < activeAnimalAgents.Count; i++)
		{
			if (activeAnimalAgents[i] == null)
			{
				return i;
			}
		}
		return activeAnimalAgents.Count;
	}

	public AnimalHouse findHouseById(int xPos, int yPos)
	{
		for (int i = 0; i < animalHouses.Count; i++)
		{
			if (animalHouses[i].isAtPos(xPos, yPos))
			{
				return animalHouses[i];
			}
		}
		return null;
	}

	public void createNewAnimalHouseWithHouseId(int xPos, int yPos, int newHouseId, int rotation)
	{
		for (int i = 0; i < animalHouses.Count; i++)
		{
			if (animalHouses[i].isAtPos(xPos, yPos))
			{
				animalHouses[i] = new AnimalHouse(xPos, yPos);
				placeNavmeshOnHouseFloor(xPos, yPos, newHouseId, rotation);
				return;
			}
		}
		AnimalHouse item = new AnimalHouse(xPos, yPos);
		placeNavmeshOnHouseFloor(xPos, yPos, newHouseId, rotation);
		animalHouses.Add(item);
		for (int j = 0; j < farmAnimalDetails.Count; j++)
		{
			if (!farmAnimalDetails[j].hasHouse() && (bool)activeAnimalAgents[farmAnimalDetails[j].agentListId])
			{
				activeAnimalAgents[farmAnimalDetails[j].agentListId].animalFindsSleepSpot(farmAnimalDetails[j]);
			}
		}
	}

	public void removeAnimalHouse(int xPos, int yPos)
	{
		for (int i = 0; i < farmAnimalDetails.Count; i++)
		{
			if (farmAnimalDetails[i].isHouseAtPos(xPos, yPos))
			{
				farmAnimalDetails[i].clearFromHouse();
			}
		}
		for (int j = 0; j < animalHouses.Count; j++)
		{
			if (animalHouses[j].isAtPos(xPos, yPos))
			{
				animalHouses[j] = null;
				animalHouses.RemoveAt(j);
				removeNavmeshOnHouseRemoval(xPos, yPos);
				break;
			}
		}
	}

	public void checkForInvalidHousesAndRemove()
	{
		for (int i = 0; i < animalHouses.Count; i++)
		{
			if (WorldManager.manageWorld.onTileMap[animalHouses[i].xPos, animalHouses[i].yPos] < 0 || !WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[animalHouses[i].xPos, animalHouses[i].yPos]].tileObjectAnimalHouse)
			{
				removeAnimalHouse(animalHouses[i].xPos, animalHouses[i].yPos);
				i--;
			}
		}
	}

	public void spawnNewFarmAnimalWithDetails(int animalId, int variation, string animalName, Vector3 spawnPos)
	{
		FarmAnimalDetails farmAnimalDetails = new FarmAnimalDetails(animalId, variation, animalName);
		farmAnimalDetails.setPosition(spawnPos);
		this.farmAnimalDetails.Add(farmAnimalDetails);
		NetworkNavMesh.nav.SpawnFarmAnimal(farmAnimalDetails);
	}

	public bool isThereAtleastOneActiveAgent()
	{
		for (int i = 0; i < activeAnimalAgents.Count; i++)
		{
			if (activeAnimalAgents[i] != null)
			{
				return true;
			}
		}
		return false;
	}

	public void EasySaveFarmAnimals()
	{
		FarmAnimalSave farmAnimalSave = new FarmAnimalSave();
		for (int i = 0; i < farmAnimalDetails.Count; i++)
		{
			if (activeAnimalAgents[farmAnimalDetails[i].agentListId] != null)
			{
				farmAnimalDetails[i].setPosition(activeAnimalAgents[farmAnimalDetails[i].agentListId].transform.position);
			}
		}
		farmAnimalSave.farmAnimalsToSave = farmAnimalDetails.ToArray();
		try
		{
			ES3.Save("farmAnimalSave", farmAnimalSave, SaveLoad.saveOrLoad.saveSlot() + "/farmAnimalSave.es3");
		}
		catch
		{
			ES3.DeleteFile(SaveLoad.saveOrLoad.saveSlot() + "/farmAnimalSave.es3");
			ES3.Save("farmAnimalSave", farmAnimalSave, SaveLoad.saveOrLoad.saveSlot() + "/farmAnimalSave.es3");
		}
	}

	public bool EasyLoadFarmAnimals()
	{
		try
		{
			if (ES3.KeyExists("farmAnimalSave", SaveLoad.saveOrLoad.saveSlot() + "/farmAnimalSave.es3"))
			{
				FarmAnimalSave farmAnimalSave = new FarmAnimalSave();
				ES3.LoadInto("farmAnimalSave", SaveLoad.saveOrLoad.saveSlot() + "/farmAnimalSave.es3", farmAnimalSave);
				for (int i = 0; i < farmAnimalSave.farmAnimalsToSave.Length; i++)
				{
					farmAnimalDetails.Add(farmAnimalSave.farmAnimalsToSave[i]);
				}
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public void saveFarmAnimalDetails()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(SaveLoad.saveOrLoad.saveSlot() + "/farmAnimalSave.dat");
			FarmAnimalSave farmAnimalSave = new FarmAnimalSave();
			for (int i = 0; i < farmAnimalDetails.Count; i++)
			{
				if (activeAnimalAgents[farmAnimalDetails[i].agentListId] != null)
				{
					farmAnimalDetails[i].setPosition(activeAnimalAgents[farmAnimalDetails[i].agentListId].transform.position);
				}
			}
			farmAnimalSave.farmAnimalsToSave = farmAnimalDetails.ToArray();
			binaryFormatter.Serialize(fileStream, farmAnimalSave);
			fileStream.Close();
			SaveLoad.saveOrLoad.makeABackUp(SaveLoad.saveOrLoad.saveSlot() + "/farmAnimalSave.dat", SaveLoad.saveOrLoad.saveSlot() + "/farmAnimalSave.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving farm animals");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void loadFarmAnimalDetails()
	{
		if (File.Exists(SaveLoad.saveOrLoad.saveSlot() + "/farmAnimalSave.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(SaveLoad.saveOrLoad.saveSlot() + "/farmAnimalSave.dat", FileMode.Open);
				FarmAnimalSave farmAnimalSave = (FarmAnimalSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				for (int i = 0; i < farmAnimalSave.farmAnimalsToSave.Length; i++)
				{
					farmAnimalDetails.Add(farmAnimalSave.farmAnimalsToSave[i]);
				}
				fileStream.Close();
				SaveLoad.saveOrLoad.makeABackUp(SaveLoad.saveOrLoad.saveSlot() + "/farmAnimalSave.dat", SaveLoad.saveOrLoad.saveSlot() + "/farmAnimalSave.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading farm animals");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadFarmAnimalBackup();
				return;
			}
		}
		loadFarmAnimalBackup();
	}

	public void loadFarmAnimalBackup()
	{
		Debug.LogWarning("Loading farm animals backup");
		if (File.Exists(SaveLoad.saveOrLoad.saveSlot() + "/farmAnimalSave.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(SaveLoad.saveOrLoad.saveSlot() + "/farmAnimalSave.bak", FileMode.Open);
				FarmAnimalSave farmAnimalSave = (FarmAnimalSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				for (int i = 0; i < farmAnimalSave.farmAnimalsToSave.Length; i++)
				{
					farmAnimalDetails.Add(farmAnimalSave.farmAnimalsToSave[i]);
				}
				fileStream.Close();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading farm animals backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadFarmAnimals();
				return;
			}
		}
		EasyLoadFarmAnimals();
	}

	public void EasySaveAnimalHouses()
	{
		AnimalHouseSave animalHouseSave = new AnimalHouseSave();
		animalHouseSave.animalHouses = new AnimalHouse[animalHouses.Count];
		for (int i = 0; i < animalHouses.Count; i++)
		{
			animalHouseSave.animalHouses[i] = animalHouses[i];
		}
		try
		{
			ES3.Save("animalHouseSave", animalHouseSave, SaveLoad.saveOrLoad.saveSlot() + "/animalHouseSave.es3");
		}
		catch
		{
			ES3.DeleteFile(SaveLoad.saveOrLoad.saveSlot() + "/animalHouseSave.es3");
			ES3.Save("animalHouseSave", animalHouseSave, SaveLoad.saveOrLoad.saveSlot() + "/animalHouseSave.es3");
		}
	}

	public bool EasyLoadAnimalHouses()
	{
		try
		{
			if (ES3.KeyExists("animalHouseSave", SaveLoad.saveOrLoad.saveSlot() + "/animalHouseSave.es3"))
			{
				AnimalHouseSave animalHouseSave = new AnimalHouseSave();
				ES3.LoadInto("animalHouseSave", SaveLoad.saveOrLoad.saveSlot() + "/animalHouseSave.es3", animalHouseSave);
				for (int i = 0; i < animalHouseSave.animalHouses.Length; i++)
				{
					animalHouses.Add(animalHouseSave.animalHouses[i]);
					placeNavmeshOnHouseFloor(animalHouseSave.animalHouses[i].xPos, animalHouseSave.animalHouses[i].yPos);
				}
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public void saveAnimalHouses()
	{
		FileStream fileStream = null;
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			fileStream = File.Create(SaveLoad.saveOrLoad.saveSlot() + "/animalHouseSave.dat");
			AnimalHouseSave animalHouseSave = new AnimalHouseSave();
			animalHouseSave.animalHouses = new AnimalHouse[animalHouses.Count];
			for (int i = 0; i < animalHouses.Count; i++)
			{
				animalHouseSave.animalHouses[i] = animalHouses[i];
			}
			binaryFormatter.Serialize(fileStream, animalHouseSave);
			fileStream.Close();
			SaveLoad.saveOrLoad.makeABackUp(SaveLoad.saveOrLoad.saveSlot() + "/animalHouseSave.dat", SaveLoad.saveOrLoad.saveSlot() + "/animalHouseSave.bak");
		}
		catch (Exception)
		{
			Debug.LogWarning("Error saving farm animal houses");
			if (fileStream != null)
			{
				fileStream.Close();
			}
		}
	}

	public void loadAnimalHouses()
	{
		if (File.Exists(SaveLoad.saveOrLoad.saveSlot() + "/animalHouseSave.dat"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(SaveLoad.saveOrLoad.saveSlot() + "/animalHouseSave.dat", FileMode.Open);
				AnimalHouseSave animalHouseSave = (AnimalHouseSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				for (int i = 0; i < animalHouseSave.animalHouses.Length; i++)
				{
					animalHouses.Add(animalHouseSave.animalHouses[i]);
					placeNavmeshOnHouseFloor(animalHouseSave.animalHouses[i].xPos, animalHouseSave.animalHouses[i].yPos);
				}
				fileStream.Close();
				checkForInvalidHousesAndRemove();
				SaveLoad.saveOrLoad.makeABackUp(SaveLoad.saveOrLoad.saveSlot() + "/animalHouseSave.dat", SaveLoad.saveOrLoad.saveSlot() + "/animalHouseSave.bak");
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading farm animal houses");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				loadAnimalHousesBackup();
				return;
			}
		}
		loadAnimalHousesBackup();
	}

	public void loadAnimalHousesBackup()
	{
		Debug.LogWarning("Loading animal houses backup slot id");
		if (File.Exists(SaveLoad.saveOrLoad.saveSlot() + "/animalHouseSave.bak"))
		{
			FileStream fileStream = null;
			try
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				fileStream = File.Open(SaveLoad.saveOrLoad.saveSlot() + "/animalHouseSave.bak", FileMode.Open);
				AnimalHouseSave animalHouseSave = (AnimalHouseSave)binaryFormatter.Deserialize(new BufferedStream(fileStream));
				for (int i = 0; i < animalHouseSave.animalHouses.Length; i++)
				{
					animalHouses.Add(animalHouseSave.animalHouses[i]);
					placeNavmeshOnHouseFloor(animalHouseSave.animalHouses[i].xPos, animalHouseSave.animalHouses[i].yPos);
				}
				fileStream.Close();
				checkForInvalidHousesAndRemove();
				return;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error loading farm animal houses backup");
				if (fileStream != null)
				{
					fileStream.Close();
				}
				EasyLoadAnimalHouses();
				return;
			}
		}
		EasyLoadAnimalHouses();
	}

	public void clearSaveForClient()
	{
		animalHouses = new List<AnimalHouse>();
		activeAnimalAgents = new List<FarmAnimal>();
		farmAnimalDetails = new List<FarmAnimalDetails>();
	}

	private void placeNavmeshOnHouseFloor(int xPos, int yPos, int houseId = -1, int rotation = -1)
	{
		if (houseId == -1)
		{
			houseId = WorldManager.manageWorld.onTileMap[xPos, yPos];
		}
		if (houseId < 0)
		{
			return;
		}
		if (rotation != -1)
		{
			WorldManager.manageWorld.rotationMap[xPos, yPos] = rotation;
		}
		TileObject tileObjectForServerDrop = WorldManager.manageWorld.getTileObjectForServerDrop(houseId, new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2));
		if ((bool)tileObjectForServerDrop.tileObjectAnimalHouse && (bool)tileObjectForServerDrop.tileObjectAnimalHouse.houseNavTileFloor)
		{
			GameObject obj = UnityEngine.Object.Instantiate(tileObjectForServerDrop.tileObjectAnimalHouse.houseNavTileFloor, tileObjectForServerDrop._transform.position, tileObjectForServerDrop._transform.rotation);
			FarmAnimalHouseFloor component = obj.GetComponent<FarmAnimalHouseFloor>();
			component.setBridge(xPos, yPos);
			component.GetComponent<FarmAnimalHouseFloor>().setXY(xPos, yPos);
			FarmAnimalHouseFloor component2 = obj.GetComponent<FarmAnimalHouseFloor>();
			houseFloors.Add(component2);
			NavMeshSourceTag[] componentsInChildren = obj.GetComponentsInChildren<NavMeshSourceTag>();
			foreach (NavMeshSourceTag navMeshSourceTag in componentsInChildren)
			{
				navMeshSourceTag.name = xPos.ToString() + yPos;
				if (component2.smallAnimalsOnly)
				{
					navMeshSourceTag.forceStartForAnimalFloor(7);
				}
				else if (component2.mediumAnimalsOnly)
				{
					navMeshSourceTag.forceStartForAnimalFloor(8);
				}
				else
				{
					navMeshSourceTag.forceStartForAnimalFloor(0);
				}
				NetworkNavMesh.nav.otherMeshes.Add(navMeshSourceTag);
			}
			NetworkNavMesh.nav.forceRebuild();
		}
		WorldManager.manageWorld.returnTileObject(tileObjectForServerDrop);
	}

	private void removeNavmeshOnHouseRemoval(int xPos, int yPos)
	{
		List<NavMeshSourceTag> list = new List<NavMeshSourceTag>();
		foreach (NavMeshSourceTag otherMesh in NetworkNavMesh.nav.otherMeshes)
		{
			if (otherMesh.name == xPos.ToString() + yPos)
			{
				list.Add(otherMesh);
			}
		}
		foreach (NavMeshSourceTag item in list)
		{
			NetworkNavMesh.nav.otherMeshes.Remove(item);
		}
		foreach (FarmAnimalHouseFloor houseFloor in houseFloors)
		{
			if (houseFloor.xPos == xPos && houseFloor.yPos == yPos)
			{
				UnityEngine.Object.Destroy(houseFloor.gameObject);
				houseFloors.Remove(houseFloor);
				break;
			}
		}
	}

	public FarmAnimalHouseFloor returnFarmAnimalHouseFloor(int xPos, int yPos)
	{
		foreach (FarmAnimalHouseFloor houseFloor in houseFloors)
		{
			if (houseFloor.xPos == xPos && houseFloor.yPos == yPos)
			{
				return houseFloor;
			}
		}
		return null;
	}

	public FarmAnimalHouseFloor findHouseForAnimal(FarmAnimalDetails details, Vector3 position)
	{
		FarmAnimal component = AnimalManager.manage.allAnimals[details.animalId].GetComponent<FarmAnimal>();
		if (details.hasHouse())
		{
			Vector2 housePos = details.getHousePos();
			return returnFarmAnimalHouseFloor((int)housePos.x, (int)housePos.y);
		}
		for (int i = 0; i < animalHouses.Count; i++)
		{
			if (!animalHouses[i].belongsToAnimal && animalHouses[i].returnTileId() == component.animalHouse.tileObjectId && Vector3.Distance(position, animalHouses[i].returnWorldPos()) < 30f && WorldManager.manageWorld.fencedOffMap[(int)position.x / 2, (int)position.z / 2] == WorldManager.manageWorld.fencedOffMap[animalHouses[i].xPos, animalHouses[i].yPos])
			{
				details.moveIntoAnimalHouse(animalHouses[i].xPos, animalHouses[i].yPos);
				return returnFarmAnimalHouseFloor(animalHouses[i].xPos, animalHouses[i].yPos);
			}
		}
		if (details.hasHouse())
		{
			details.clearFromHouse();
		}
		return null;
	}

	private void MirrorProcessed()
	{
	}
}
