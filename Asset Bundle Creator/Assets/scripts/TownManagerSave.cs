using System;
using UnityEngine;

[Serializable]
public class TownManagerSave
{
	public bool hasBaseTentBeenPlaced;

	public bool firstConnect = true;

	public bool journalUnlock;

	public int movingBuilding = -1;

	public bool freeCamOn;

	public int[] privateTowerPos = new int[2];

	public float[] lastSleptPos = new float[3];

	public float[] savedPos = new float[4];

	public float[] savedRotation = new float[5];

	public int[] savedInside = new int[2];

	public int lastSleptHouseX = -1;

	public int lastSleptHouseY = -1;

	public Nest[] allNests = new Nest[0];

	public int lastMarketplaceVisitor;

	public void saveTown()
	{
		firstConnect = TownManager.manage.firstConnect;
		movingBuilding = BuildingManager.manage.currentlyMoving;
		hasBaseTentBeenPlaced = TownManager.manage.baseTentFirstPlace;
		lastSleptPos[0] = TownManager.manage.lastSleptPos.x;
		lastSleptPos[1] = TownManager.manage.lastSleptPos.y;
		lastSleptPos[2] = TownManager.manage.lastSleptPos.z;
		savedPos[0] = NetworkMapSharer.share.localChar.transform.position.x;
		savedPos[1] = NetworkMapSharer.share.localChar.transform.position.y;
		savedPos[2] = NetworkMapSharer.share.localChar.transform.position.z;
		savedInside[0] = TownManager.manage.savedInside[0];
		savedInside[1] = TownManager.manage.savedInside[1];
		savedRotation[0] = NetworkMapSharer.share.localChar.transform.eulerAngles.y;
		savedRotation[1] = CameraController.control.transform.eulerAngles.y;
		savedRotation[2] = CameraController.control.Camera_Y.localEulerAngles.x;
		savedRotation[3] = CameraController.control.Camera_Y.localEulerAngles.y;
		savedRotation[4] = CameraController.control.Camera_Y.localEulerAngles.z;
		freeCamOn = CameraController.control.isFreeCamOn();
		privateTowerPos = new int[2]
		{
			(int)NetworkMapSharer.share.privateTowerPos.x,
			(int)NetworkMapSharer.share.privateTowerPos.y
		};
		allNests = AnimalManager.manage.allNests.ToArray();
		if (TownManager.manage.sleepInsideHouse != null)
		{
			lastSleptHouseX = TownManager.manage.sleepInsideHouse.xPos;
			lastSleptHouseY = TownManager.manage.sleepInsideHouse.yPos;
		}
		else
		{
			lastSleptHouseX = -1;
			lastSleptHouseY = -1;
		}
		lastMarketplaceVisitor = MarketPlaceManager.manage.lastVisitor;
	}

	public void load()
	{
		TownManager.manage.baseTentFirstPlace = hasBaseTentBeenPlaced;
		TownManager.manage.firstConnect = firstConnect;
		TownManager.manage.mapUnlocked = !TownManager.manage.firstConnect;
		TownManager.manage.journalUnlocked = !TownManager.manage.firstConnect;
		if (movingBuilding > 0)
		{
			BuildingManager.manage.loadCurrentlyMoving(movingBuilding);
		}
		else
		{
			BuildingManager.manage.currentlyMoving = -1;
			NetworkMapSharer.share.NetworkmovingBuilding = -1;
		}
		if (TownManager.manage.journalUnlocked)
		{
			CurrencyWindows.currency.sideTaskBarSmall.gameObject.SetActive(true);
		}
		if (savedPos != null)
		{
			TownManager.manage.lastSavedPos = new Vector3(savedPos[0], savedPos[1], savedPos[2]);
			WorldManager.manageWorld.spawnPos.position = TownManager.manage.lastSleptPos;
		}
		if (lastSleptPos != null)
		{
			TownManager.manage.lastSleptPos = new Vector3(lastSleptPos[0], lastSleptPos[1], lastSleptPos[2]);
			if (TownManager.manage.lastSavedPos == Vector3.zero)
			{
				WorldManager.manageWorld.spawnPos.position = TownManager.manage.lastSleptPos;
			}
		}
		if (privateTowerPos != null)
		{
			NetworkMapSharer.share.NetworkprivateTowerPos = new Vector2(privateTowerPos[0], privateTowerPos[1]);
		}
		if (lastSleptHouseX != -1 && lastSleptHouseY != -1)
		{
			TownManager.manage.sleepInsideHouse = HouseManager.manage.getHouseInfo(lastSleptHouseX, lastSleptHouseY);
		}
		TownManager.manage.savedInside[0] = savedInside[0];
		TownManager.manage.savedInside[1] = savedInside[1];
		if (freeCamOn)
		{
			CameraController.control.swapFreeCam();
		}
		if (savedRotation != null)
		{
			TownManager.manage.savedRot = savedRotation[0];
			CameraController.control.transform.eulerAngles = new Vector3(0f, savedRotation[1], 0f);
			CameraController.control.Camera_Y.transform.localEulerAngles = new Vector3(savedRotation[2], savedRotation[3], savedRotation[4]);
		}
		if (allNests != null)
		{
			for (int i = 0; i < allNests.Length; i++)
			{
				AnimalManager.manage.allNests.Add(allNests[i]);
			}
		}
		MarketPlaceManager.manage.lastVisitor = lastMarketplaceVisitor;
	}
}
