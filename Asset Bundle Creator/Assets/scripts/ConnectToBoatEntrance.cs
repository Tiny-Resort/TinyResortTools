using System.Collections;
using UnityEngine;

public class ConnectToBoatEntrance : MonoBehaviour
{
	public static ConnectToBoatEntrance connect;

	public bool isMainConnect;

	public bool isBoat = true;

	public bool isInterior;

	public EntryExit myEntryExit;

	public EntryExit boatEntry;

	public EntryExit interiorEntryExit;

	public GameObject interiorToHideAndRotate;

	public NPCBuildingDoors myDoorAndFloor;

	public Transform cutOutWalls;

	private void Awake()
	{
		if (isMainConnect)
		{
			connect = this;
			StartCoroutine(interiorFollowsBoatRotation());
		}
	}

	private void OnEnable()
	{
		if (!isMainConnect)
		{
			if (isBoat)
			{
				connect.boatEntry = myEntryExit;
				myEntryExit.interiorToTurnOnOrOff = connect.interiorToHideAndRotate;
				myEntryExit.linkedTo = connect.interiorEntryExit.transform;
				connect.interiorEntryExit.linkedTo = myEntryExit.transform;
				connect.boatEntry.interiorToTurnOnOrOff = connect.interiorToHideAndRotate;
			}
			else
			{
				connect.interiorEntryExit = myEntryExit;
			}
		}
	}

	private IEnumerator interiorFollowsBoatRotation()
	{
		while (true)
		{
			yield return null;
			if ((bool)boatEntry)
			{
				interiorToHideAndRotate.transform.rotation = boatEntry.transform.rotation;
				interiorToHideAndRotate.transform.localEulerAngles = new Vector3(0f, interiorToHideAndRotate.transform.localEulerAngles.y, 0f);
				cutOutWalls.transform.rotation = interiorToHideAndRotate.transform.rotation;
			}
		}
	}

	public void setUpBoatFloor()
	{
		myDoorAndFloor.setConnectedToBuildingId(20);
		myDoorAndFloor.addMeshesToNavMesh();
	}
}
