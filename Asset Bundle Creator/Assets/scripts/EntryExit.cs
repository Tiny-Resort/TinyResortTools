using UnityEngine;

public class EntryExit : MonoBehaviour
{
	public Transform linkedTo;

	public bool isEntry = true;

	public bool isPlayerHouseDoor;

	public bool interiorDoor;

	public DisplayPlayerHouseTiles connectedPlayerHouse;

	public bool isMuseumDoor;

	public bool isShop;

	public bool noMusic;

	private int npcId = -1;

	public GameObject interiorToTurnOnOrOff;

	private TileObject thisBuilding;

	public void feedInNPCId(int newId)
	{
		npcId = newId;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!canEnter())
		{
			return;
		}
		CharInteract component = other.transform.GetComponent<CharInteract>();
		if (!component || !component.isLocalPlayer)
		{
			return;
		}
		if ((bool)interiorToTurnOnOrOff && isEntry)
		{
			interiorToTurnOnOrOff.SetActive(true);
		}
		if ((bool)interiorToTurnOnOrOff && !isEntry)
		{
			interiorToTurnOnOrOff.SetActive(false);
		}
		other.GetComponent<CharMovement>().forceNoStandingOn();
		other.transform.position = linkedTo.position + linkedTo.forward * 0.8f;
		component.GetComponent<Rigidbody>().velocity = Vector3.zero;
		CameraController.control.moveToFollowing();
		if (!interiorDoor)
		{
			if (isEntry)
			{
				NewChunkLoader.loader.inside = true;
				if (!isPlayerHouseDoor)
				{
					component.myEquip.setInsideOrOutside(true, false);
				}
				WeatherManager.manage.goInside(isShop, noMusic);
				RealWorldTimeLight.time.goInside();
				if ((bool)thisBuilding)
				{
					TownManager.manage.savedInside[0] = thisBuilding.xPos;
					TownManager.manage.savedInside[1] = thisBuilding.yPos;
				}
			}
			else
			{
				NewChunkLoader.loader.inside = false;
				WeatherManager.manage.goOutside();
				RealWorldTimeLight.time.goOutside();
				if (!isPlayerHouseDoor)
				{
					component.myEquip.setInsideOrOutside(false, false);
				}
				if ((bool)thisBuilding)
				{
					TownManager.manage.savedInside[0] = -1;
					TownManager.manage.savedInside[1] = -1;
				}
			}
		}
		CameraController.control.moveToFollowing();
		if (isPlayerHouseDoor)
		{
			if (isEntry && (bool)connectedPlayerHouse)
			{
				if ((bool)component)
				{
					component.changeInsideOut(isEntry, HouseManager.manage.getHouseInfo(connectedPlayerHouse.housePosX, connectedPlayerHouse.housePosY));
				}
			}
			else if ((bool)component)
			{
				component.changeInsideOut(false);
			}
		}
		if (isMuseumDoor && MuseumManager.manage.clientNeedsToRequest)
		{
			component.GetComponent<CharMovement>().CmdRequestMuseumInterior();
		}
	}

	public bool canEnter()
	{
		if (npcId != -1)
		{
			if (NPCManager.manage.NPCDetails[npcId].mySchedual.checkIfOpen())
			{
				return true;
			}
			return false;
		}
		return true;
	}

	private void OnEnable()
	{
		if (!thisBuilding)
		{
			thisBuilding = base.transform.root.GetComponent<TileObject>();
		}
		if ((bool)interiorToTurnOnOrOff && (bool)thisBuilding && thisBuilding.xPos == TownManager.manage.savedInside[0] && thisBuilding.yPos == TownManager.manage.savedInside[1])
		{
			interiorToTurnOnOrOff.SetActive(true);
		}
	}
}
