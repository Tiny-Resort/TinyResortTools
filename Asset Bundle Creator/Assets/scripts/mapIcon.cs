using UnityEngine;
using UnityEngine.UI;

public class mapIcon : MonoBehaviour
{
	public enum iconType
	{
		PlayerPlaced = 0,
		TileObject = 1,
		Vehicle = 2,
		Teletower = 3,
		CameraQuest = 4,
		HuntingQuest = 5,
		InvestigationQuest = 6
	}

	public int tileObjectId;

	public Image myIcon;

	public Vector3 pointingAtPosition;

	public RectTransform myTrans;

	private uint playersIcon;

	public Sprite playerCustomIcon;

	public Sprite vehicleIcon;

	public Sprite teleTowerIcon;

	public Sprite cameraIcon;

	public Sprite cameraCompleteIcon;

	public Sprite huntingIcon;

	public Sprite huntingCompleIcon;

	public Sprite investigationIcon;

	private Transform followTransform;

	public uint placedByPlayer;

	private bool normalMarker;

	private bool customMarker;

	public GameObject ping;

	private int mySprite;

	public string telePointName = "";

	public string iconName = "";

	public iconType myIconType;

	public int iconId;

	public bool placedAboveGround = true;

	private PostOnBoard myPost;

	private void Start()
	{
		myTrans.pivot = new Vector2(0.5f, 0.5f);
	}

	public bool isNormal()
	{
		return normalMarker;
	}

	public bool isCustom()
	{
		return customMarker;
	}

	public int getIconSpriteNo()
	{
		return mySprite;
	}

	public void setUp(int showingTileObjectId, Vector3 pointingPosition)
	{
		tileObjectId = showingTileObjectId;
		pointingAtPosition = pointingPosition;
		myIcon.sprite = WorldManager.manageWorld.allObjectSettings[showingTileObjectId].mapIcon;
		myIcon.color = WorldManager.manageWorld.allObjectSettings[showingTileObjectId].mapIconColor;
		if ((bool)WorldManager.manageWorld.allObjectSettings[showingTileObjectId].tileObjectLoadInside)
		{
			iconName = WorldManager.manageWorld.allObjectSettings[showingTileObjectId].tileObjectLoadInside.buildingName;
		}
		else if ((bool)WorldManager.manageWorld.allObjects[showingTileObjectId].displayPlayerHouseTiles && WorldManager.manageWorld.allObjects[showingTileObjectId].displayPlayerHouseTiles.isPlayerHouse)
		{
			iconName = Inventory.inv.playerName + "'s House";
		}
		myIconType = iconType.TileObject;
		iconId = showingTileObjectId;
	}

	public void customMarkerSetup(Vector3 pointingPosition, uint placedBy, int iconNo, iconType myType)
	{
		customMarker = true;
		placedByPlayer = placedBy;
		pointingAtPosition = pointingPosition;
		switch (myType)
		{
		case iconType.PlayerPlaced:
			myIcon.sprite = RenderMap.map.icons[iconNo];
			break;
		case iconType.Vehicle:
			myIcon.sprite = SaveLoad.saveOrLoad.vehiclePrefabs[iconNo].GetComponent<Vehicle>().mapIconSprite;
			break;
		case iconType.TileObject:
			myIcon.sprite = WorldManager.manageWorld.allObjectSettings[iconNo].mapIcon;
			myIcon.color = WorldManager.manageWorld.allObjectSettings[iconNo].mapIconColor;
			break;
		case iconType.CameraQuest:
			myIcon.sprite = cameraIcon;
			break;
		case iconType.HuntingQuest:
			myIcon.sprite = huntingIcon;
			break;
		case iconType.InvestigationQuest:
			myIcon.sprite = investigationIcon;
			break;
		case iconType.Teletower:
			myIcon.sprite = teleTowerIcon;
			break;
		}
		ping.SetActive(true);
	}

	public void normalMarkerSetup(Vector3 pointingPosition, int markerSprite)
	{
		normalMarker = true;
		pointingAtPosition = pointingPosition;
		myIcon.sprite = playerCustomIcon;
		myIcon.sprite = RenderMap.map.icons[markerSprite];
		mySprite = markerSprite;
		myIconType = iconType.PlayerPlaced;
		iconId = markerSprite;
	}

	public void setUpTelePoint(string dir)
	{
		telePointName = dir;
		switch (dir)
		{
		case "private":
			pointingAtPosition = new Vector3(NetworkMapSharer.share.privateTowerPos.x * 2f, 1f, NetworkMapSharer.share.privateTowerPos.y * 2f);
			break;
		case "north":
			pointingAtPosition = new Vector3(TownManager.manage.northTowerPos[0] * 2, 1f, TownManager.manage.northTowerPos[1] * 2);
			break;
		case "east":
			pointingAtPosition = new Vector3(TownManager.manage.eastTowerPos[0] * 2, 1f, TownManager.manage.eastTowerPos[1] * 2);
			break;
		case "south":
			pointingAtPosition = new Vector3(TownManager.manage.southTowerPos[0] * 2, 1f, TownManager.manage.southTowerPos[1] * 2);
			break;
		case "west":
			pointingAtPosition = new Vector3(TownManager.manage.westTowerPos[0] * 2, 1f, TownManager.manage.westTowerPos[1] * 2);
			break;
		}
		myIcon.sprite = teleTowerIcon;
		myIcon.color = Color.Lerp(Color.yellow, Color.red, 0.35f);
		if (dir == "private")
		{
			iconName = "Tele Pad";
		}
		else
		{
			iconName = "Tele Tower";
		}
		myIconType = iconType.Teletower;
	}

	public bool isConnectedToTask(PostOnBoard newPost)
	{
		if (myPost == newPost)
		{
			return true;
		}
		return false;
	}

	public void setUpQuestIcon(PostOnBoard newPost)
	{
		myPost = newPost;
		QuestTracker.track.updateTasksEvent.AddListener(updateTaskIcon);
		updateTaskIcon();
	}

	public void updateTaskIcon()
	{
		if (BulletinBoard.board.attachedPosts.Contains(myPost) && !myPost.checkIfExpired() && !myPost.completed)
		{
			pointingAtPosition = myPost.getRequiredLocation();
			if (myPost.isPhotoTask)
			{
				if (myPost.readyForNPC)
				{
					iconName = "Completed " + myPost.getTitleText(myPost.getPostIdOnBoard());
					myIcon.sprite = cameraCompleteIcon;
					iconId = 1;
				}
				else
				{
					iconName = myPost.getTitleText(myPost.getPostIdOnBoard());
					myIcon.sprite = cameraIcon;
					iconId = 0;
				}
				myIconType = iconType.CameraQuest;
			}
			else if (myPost.isHuntingTask)
			{
				if (myPost.readyForNPC)
				{
					iconName = "Completed " + myPost.getTitleText(myPost.getPostIdOnBoard());
					myIcon.sprite = huntingCompleIcon;
					iconId = 3;
				}
				else
				{
					iconName = myPost.getTitleText(myPost.getPostIdOnBoard());
					myIcon.sprite = huntingIcon;
					iconId = 2;
				}
				myIconType = iconType.HuntingQuest;
			}
			else if (myPost.isInvestigation)
			{
				iconName = myPost.getTitleText(myPost.getPostIdOnBoard());
				myIcon.sprite = investigationIcon;
				iconId = 4;
				myIconType = iconType.InvestigationQuest;
			}
		}
		else
		{
			RenderMap.map.removeTaskIcon(this);
		}
	}

	public void setUpFollowVehicle(Transform newFollowTransform, Sprite vehiclesIconSprite)
	{
		followTransform = newFollowTransform;
		myIcon.sprite = vehiclesIconSprite;
		myIcon.color = Color.Lerp(Color.gray, Color.white, 0.45f);
	}

	public void onPress()
	{
		if (RenderMap.map.debugTeleport)
		{
			NetworkMapSharer.share.localChar.transform.position = new Vector3(pointingAtPosition.x, (float)WorldManager.manageWorld.heightMap[(int)pointingAtPosition.x / 2, (int)pointingAtPosition.z / 2] + 2f, pointingAtPosition.z);
			CameraController.control.moveToFollowing();
			NewChunkLoader.loader.forceInstantUpdateAtPos();
			RenderMap.map.debugTeleport = false;
		}
		if (telePointName != "" && Vector3.Distance(NetworkMapSharer.share.localChar.transform.position, pointingAtPosition) > 25f && !RenderMap.map.selectTeleWindowOpen && RenderMap.map.canTele)
		{
			RenderMap.map.openTeleSelectWindow(telePointName);
		}
	}

	private void Update()
	{
		if ((bool)followTransform)
		{
			pointingAtPosition = followTransform.position;
		}
		if (RenderMap.map.mapOpen)
		{
			myTrans.localPosition = new Vector3(pointingAtPosition.x / 2f / RenderMap.map.mapScale, pointingAtPosition.z / 2f / RenderMap.map.mapScale, 1f);
			myTrans.localRotation = Quaternion.Euler(0f, 0f, 0f);
			myTrans.localScale = new Vector3(2f / RenderMap.map.desiredScale, 2f / RenderMap.map.desiredScale, 1f);
			return;
		}
		if (!OptionsMenu.options.mapFacesNorth)
		{
			myTrans.localRotation = Quaternion.Lerp(myTrans.localRotation, Quaternion.Euler(0f, 0f, 0f - CameraController.control.transform.eulerAngles.y), Time.deltaTime * 3f);
		}
		else
		{
			myTrans.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
		if (!customMarker)
		{
			myTrans.localPosition = new Vector3(pointingAtPosition.x / 2f / RenderMap.map.mapScale, pointingAtPosition.z / 2f / RenderMap.map.mapScale, 1f);
			myTrans.localScale = new Vector3(4.5f / RenderMap.map.desiredScale, 4.5f / RenderMap.map.desiredScale, 1f);
		}
		else if (Vector3.Distance(RenderMap.map.charToPointTo.position, pointingAtPosition) < 45f)
		{
			myTrans.localPosition = new Vector3(pointingAtPosition.x / 2f / RenderMap.map.mapScale, pointingAtPosition.z / 2f / RenderMap.map.mapScale, 1f);
			myTrans.localScale = new Vector3(5.5f / RenderMap.map.desiredScale, 5.5f / RenderMap.map.desiredScale, 1f);
		}
		else
		{
			Vector3 vector = RenderMap.map.charToPointTo.position + (pointingAtPosition - RenderMap.map.charToPointTo.position).normalized * 45f;
			myTrans.localPosition = new Vector3(vector.x / 2f / RenderMap.map.mapScale, vector.z / 2f / RenderMap.map.mapScale, 1f);
			myTrans.localScale = new Vector3(5.5f / RenderMap.map.desiredScale, 5.5f / RenderMap.map.desiredScale, 1f);
		}
	}
}
