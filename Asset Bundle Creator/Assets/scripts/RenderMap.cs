using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RenderMap : MonoBehaviour
{
	public static RenderMap map;

	public static RenderMap undergroundMap;

	public GameObject mapIconPrefab;

	public GameObject mapSubWindow;

	private Texture2D noiseTex;

	private Texture2D undergroundTex;

	private Color[] pix;

	private Color[] undergroundPix;

	public RawImage mapImage;

	public RectTransform mapParent;

	public bool refreshMap;

	public Color water;

	public Color deepWater;

	public float scale = 5f;

	public float desiredScale = 5f;

	private float openedScale = 5f;

	public RectTransform charPointer;

	public RectTransform charDirPointer;

	public RectTransform compass;

	public RectTransform buttonPrompt;

	public Image turnOnMapButtonIcon;

	public RectTransform mapCircle;

	public Transform charToPointTo;

	public RectTransform mapWindow;

	public Transform mapMask;

	public Image mapWindowShape;

	public Sprite mapWindowCircle;

	public Sprite mapWindowSquare;

	public GameObject otherCharPointerPrefab;

	private List<Transform> otherPlayersToTrack = new List<Transform>();

	private List<RectTransform> otherPlayerIcons = new List<RectTransform>();

	public bool mapOpen;

	private bool mapChanged = true;

	private bool firstOpen = true;

	public TileObject[] tileObjectShowOnMap;

	public Color[] tileObjectsShownOnMapColor;

	public List<mapIcon> iconsOnMap = new List<mapIcon>();

	private float mapXPosDif;

	private float mapYPosDif;

	public Vector3 shopPosition;

	public Vector3 clothesShopPosition;

	public Vector3 crafterShopPosition;

	public Color mapBackColor;

	public Color heightLineColour;

	public Canvas myCanvas;

	public Material mapMaterial;

	public float mapScale;

	[Header("Icon Set Up --------")]
	public MapCursor mapCursor;

	public GameObject iconSelectorWindow;

	public bool iconSelectorOpen;

	public Sprite[] icons;

	public Image[] iconButtons;

	private int selectedIcon;

	public ASound placeMarkerSound;

	public ASound removeMarkerSound;

	public bool canTele;

	public bool debugTeleport;

	public bool selectTeleWindowOpen;

	public GameObject teleSelectWindow;

	private Color defaultMapMaskBackgroundColour;

	public TextMeshProUGUI biomeName;

	private string teleDir = "";

	private WaitForSeconds mapWait = new WaitForSeconds(0.25f);

	public GraphicRaycaster mapCaster;

	private List<int[]> bridgePositions = new List<int[]>();

	private void Awake()
	{
		map = this;
	}

	private void Start()
	{
		defaultMapMaskBackgroundColour = mapWindowShape.color;
		myCanvas = GetComponent<Canvas>();
		desiredScale = scale;
		noiseTex = new Texture2D(WorldManager.manageWorld.getMapSize(), WorldManager.manageWorld.getMapSize());
		noiseTex.filterMode = FilterMode.Point;
		undergroundTex = new Texture2D(WorldManager.manageWorld.getMapSize(), WorldManager.manageWorld.getMapSize());
		undergroundTex.filterMode = FilterMode.Point;
		pix = new Color[noiseTex.width * noiseTex.height];
		undergroundPix = new Color[noiseTex.width * noiseTex.height];
		mapScale = WorldManager.manageWorld.getMapSize() / 500;
	}

	public void Update()
	{
		if (!mapOpen)
		{
			return;
		}
		Vector2 localPoint = default(Vector2);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(mapImage.rectTransform, mapCursor.transform.position, null, out localPoint);
		biomeName.text = GenerateMap.generate.getBiomeNameUnderMapCursor((int)(localPoint.x * 2f), (int)(localPoint.y * 2f));
		if (selectTeleWindowOpen)
		{
			Cursor.lockState = CursorLockMode.None;
			if (InputMaster.input.UICancel() && iconSelectorOpen)
			{
				closeButton();
			}
		}
		else
		{
			if (iconSelectorOpen)
			{
				return;
			}
			Cursor.lockState = CursorLockMode.Locked;
			bool usingMouse = Inventory.inv.usingMouse;
			if (InputMaster.input.UISelect())
			{
				if ((bool)checkForIcon())
				{
					mapIcon componentInParent = checkForIcon().GetComponentInParent<mapIcon>();
					if (canTele)
					{
						mapCursor.pressDownOnButton();
						if ((bool)componentInParent && !Inventory.inv.usingMouse)
						{
							componentInParent.onPress();
						}
						return;
					}
					if ((bool)componentInParent && !componentInParent.isCustom())
					{
						bool flag = false;
						for (int i = 0; i < iconsOnMap.Count; i++)
						{
							if (iconsOnMap[i].placedByPlayer == NetworkMapSharer.share.localChar.netId)
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							NetworkMapSharer.share.localChar.CmdRemoveMyMarker();
							Vector2 localPoint2;
							RectTransformUtility.ScreenPointToLocalPointInRectangle(mapImage.rectTransform, componentInParent.transform.position, null, out localPoint2);
							NetworkMapSharer.share.localChar.CmdPlaceMarkerOnMap(componentInParent.transform.localPosition / 2f, componentInParent.iconId, (int)componentInParent.myIconType);
							SoundManager.manage.play2DSound(placeMarkerSound);
						}
						else
						{
							Vector2 localPoint3;
							RectTransformUtility.ScreenPointToLocalPointInRectangle(mapImage.rectTransform, componentInParent.transform.position, null, out localPoint3);
							NetworkMapSharer.share.localChar.CmdPlaceMarkerOnMap(componentInParent.transform.localPosition / 2f, componentInParent.iconId, (int)componentInParent.myIconType);
							SoundManager.manage.play2DSound(placeMarkerSound);
						}
					}
					else if ((bool)componentInParent && componentInParent.isCustom())
					{
						NetworkMapSharer.share.localChar.CmdRemoveMyMarker();
						SoundManager.manage.play2DSound(removeMarkerSound);
					}
					mapCursor.pressDownOnButton();
				}
				else
				{
					iconSelectorOpen = true;
					StartCoroutine(runIconSelector());
				}
			}
			if (!iconSelectorOpen && InputMaster.input.UIAlt() && (bool)checkForIcon())
			{
				mapIcon componentInParent2 = checkForIcon().GetComponentInParent<mapIcon>();
				if ((bool)componentInParent2 && componentInParent2.isNormal())
				{
					iconsOnMap.Remove(componentInParent2);
					Object.Destroy(componentInParent2.gameObject);
					SoundManager.manage.play2DSound(removeMarkerSound);
					mapCursor.placeButtonPing();
				}
				else if ((bool)componentInParent2 && componentInParent2.isCustom())
				{
					NetworkMapSharer.share.localChar.CmdRemoveMyMarker();
					SoundManager.manage.play2DSound(removeMarkerSound);
					mapCursor.placeButtonPing();
				}
			}
			float num = 0f - InputMaster.input.getLeftStick().x;
			float num2 = 0f - InputMaster.input.getLeftStick().y;
			if (Inventory.inv.usingMouse)
			{
				num = 0f - InputMaster.input.getMousePosOld().x;
				num2 = 0f - InputMaster.input.getMousePosOld().y;
			}
			if ((bool)checkForIcon())
			{
				mapCursor.setHovering(true, checkForIcon().GetComponentInParent<mapIcon>());
				num /= 2f;
				num2 /= 2f;
			}
			else
			{
				mapCursor.setHovering(false, null);
			}
			if ((!Inventory.inv.usingMouse && InputMaster.input.drop()) || (Inventory.inv.usingMouse && InputMaster.input.TriggerLook()))
			{
				recentre();
			}
			mapXPosDif += num * 2f / (scale / 5f);
			mapYPosDif += num2 * 2f / (scale / 5f);
			float scrollWheel = InputMaster.input.getScrollWheel();
			if (scrollWheel == 0f)
			{
				changeScale(InputMaster.input.getRightStick().y / 2f);
			}
			else
			{
				changeScale(scrollWheel * 3f);
			}
		}
	}

	public void openTheMap()
	{
		if (!mapOpen)
		{
			NetworkMapSharer.share.localChar.myEquip.setNewLookingAtMap(true);
			mapCaster.enabled = true;
		}
		mapOpen = true;
	}

	public void closeTheMap()
	{
		if (mapOpen)
		{
			NetworkMapSharer.share.localChar.myEquip.setNewLookingAtMap(false);
			mapCaster.enabled = false;
		}
		map.mapOpen = false;
	}

	private IEnumerator runIconSelector()
	{
		float changeTimer = 0.2f;
		selectedIcon = 0;
		changeSelectedIconSlot(0);
		iconSelectorWindow.SetActive(true);
		mapCursor.setPressing(true);
		while (iconSelectorOpen)
		{
			yield return null;
			if (changeTimer >= 0.2f)
			{
				if (!Inventory.inv.usingMouse)
				{
					float f = 0f - InputMaster.input.getLeftStick().y;
					float x = InputMaster.input.getLeftStick().x;
					if (Inventory.inv.usingMouse && Mathf.CeilToInt(x) != 0)
					{
						if ((Mathf.CeilToInt(x) == 1 && selectedIcon != 3 && selectedIcon != 7) || (Mathf.CeilToInt(x) == -1 && selectedIcon != 0 && selectedIcon != 4))
						{
							changeSelectedIconSlot(Mathf.CeilToInt(x));
							yield return new WaitForSeconds(0.15f);
						}
					}
					else if (Inventory.inv.usingMouse && Mathf.CeilToInt(f) != 0)
					{
						int num = Mathf.CeilToInt(f);
						if ((num == 1 && selectedIcon < 4) || (num == -1 && selectedIcon >= 4))
						{
							changeSelectedIconSlot(num * 4);
							yield return new WaitForSeconds(0.15f);
						}
					}
					else if (Mathf.RoundToInt(InputMaster.input.UINavigation().x) != 0)
					{
						if ((Mathf.RoundToInt(InputMaster.input.UINavigation().x) == 1 && selectedIcon != 3 && selectedIcon != 7) || (Mathf.RoundToInt(InputMaster.input.UINavigation().x) == -1 && selectedIcon != 0 && selectedIcon != 4))
						{
							changeSelectedIconSlot(Mathf.RoundToInt(InputMaster.input.UINavigation().x));
							yield return new WaitForSeconds(0.15f);
						}
					}
					else if (Mathf.RoundToInt(Mathf.RoundToInt(InputMaster.input.UINavigation().y)) != 0)
					{
						int num2 = -Mathf.RoundToInt(InputMaster.input.UINavigation().y);
						MonoBehaviour.print(num2 + " pushes " + selectedIcon);
						if ((num2 == 1 && selectedIcon < 4) || (num2 == -1 && selectedIcon >= 4))
						{
							changeSelectedIconSlot(num2 * 4);
							yield return new WaitForSeconds(0.15f);
						}
					}
				}
			}
			else
			{
				changeTimer += Time.deltaTime;
			}
			if (Inventory.inv.usingMouse)
			{
				if (InputMaster.input.getScrollWheel() / 20f > 0f)
				{
					changeSelectedIconSlot(-1);
					SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
				}
				if (InputMaster.input.getScrollWheel() / 20f < 0f)
				{
					changeSelectedIconSlot(1);
					SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
				}
				float y = InputMaster.input.getLeftStick().y;
				float x2 = InputMaster.input.getLeftStick().x;
				if (Mathf.CeilToInt(x2) != 0)
				{
					if ((Mathf.CeilToInt(x2) == 1 && selectedIcon != 3 && selectedIcon != 7) || (Mathf.CeilToInt(x2) == -1 && selectedIcon != 0 && selectedIcon != 4))
					{
						changeSelectedIconSlot(Mathf.CeilToInt(x2));
						yield return new WaitForSeconds(0.15f);
					}
				}
				else if (Mathf.CeilToInt(y) != 0)
				{
					int num3 = Mathf.CeilToInt(y);
					if ((num3 == 1 && selectedIcon < 4) || (num3 == -1 && selectedIcon >= 4))
					{
						changeSelectedIconSlot(num3 * 4);
						yield return new WaitForSeconds(0.15f);
					}
				}
			}
			if (InputMaster.input.UISelect())
			{
				Vector2 localPoint;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(mapImage.rectTransform, mapCursor.transform.position, null, out localPoint);
				createLocalCustomMarker(localPoint / 2f, selectedIcon);
				SoundManager.manage.play2DSound(placeMarkerSound);
				mapCursor.placeButtonPing();
				closeButton();
			}
			if ((InputMaster.input.UICancel() && iconSelectorOpen) || (Inventory.inv.usingMouse && InputMaster.input.Interact() && iconSelectorOpen))
			{
				closeButton();
			}
		}
		mapCursor.setPressing(false);
	}

	public void closeButton()
	{
		if (selectTeleWindowOpen)
		{
			closeTeleSelectWindow();
		}
		else if (iconSelectorOpen)
		{
			iconSelectorOpen = false;
			iconSelectorWindow.SetActive(false);
		}
		else
		{
			MenuButtonsTop.menu.closeWindow();
		}
	}

	public void changeSelectedIconSlot(int changeBy)
	{
		selectedIcon += changeBy;
		if (selectedIcon >= iconButtons.Length)
		{
			selectedIcon = 0;
		}
		if (selectedIcon < 0)
		{
			selectedIcon = iconButtons.Length - 1;
		}
		for (int i = 0; i < iconButtons.Length; i++)
		{
			iconButtons[i].enabled = false;
		}
		iconButtons[selectedIcon].enabled = true;
		SoundManager.manage.play2DSound(SoundManager.manage.inventorySound);
	}

	public void changeScale(float dif)
	{
		dif = Mathf.Clamp(dif, -2f, 2f);
		scale += dif * scale / 5f;
	}

	public void recentre()
	{
		mapXPosDif = 0f;
		mapYPosDif = 0f;
	}

	public void openTeleSelectWindow(string dirSelected)
	{
		teleDir = dirSelected;
		teleSelectWindow.SetActive(true);
		selectTeleWindowOpen = true;
	}

	public void closeTeleSelectWindow()
	{
		teleSelectWindow.SetActive(false);
		selectTeleWindowOpen = false;
	}

	public void confirmTele()
	{
		closeTeleSelectWindow();
		MenuButtonsTop.menu.closeWindow();
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.TeleportSomewhere);
		NetworkMapSharer.share.localChar.CmdTeleport(teleDir);
	}

	public void changeMapWindow()
	{
		if (mapOpen)
		{
			mapSubWindow.gameObject.SetActive(true);
			mapWindow.SetParent(mapSubWindow.transform.Find("MapPos"));
			mapWindow.SetSiblingIndex(0);
			mapWindow.anchoredPosition = Vector3.zero;
			Cursor.lockState = CursorLockMode.Locked;
			mapWindow.sizeDelta = mapWindow.GetComponentInParent<RectTransform>().sizeDelta;
			mapMask.localScale = new Vector3(1f, 1f, 1f);
			mapWindowShape.sprite = mapWindowSquare;
			mapWindowShape.type = Image.Type.Sliced;
			mapCircle.gameObject.SetActive(false);
			scale = openedScale;
			desiredScale = openedScale;
			mapMask.localRotation = Quaternion.Euler(0f, 0f, 0f - CameraController.control.transform.eulerAngles.y);
			compass.gameObject.SetActive(false);
			buttonPrompt.gameObject.SetActive(false);
			CurrencyWindows.currency.windowOn(false);
			return;
		}
		mapSubWindow.gameObject.SetActive(false);
		mapMask.localRotation = Quaternion.Euler(0f, 0f, CameraController.control.transform.eulerAngles.y);
		compass.localRotation = Quaternion.Euler(0f, 0f, CameraController.control.transform.eulerAngles.y);
		charPointer.localRotation = Quaternion.Euler(0f, 0f, 0f - CameraController.control.transform.eulerAngles.y);
		Cursor.lockState = CursorLockMode.None;
		mapMask.localScale = new Vector3(0.285f, 0.285f, 0.285f);
		mapWindowShape.sprite = mapWindowCircle;
		mapWindowShape.type = Image.Type.Simple;
		mapCircle.anchoredPosition = new Vector3(-100f, -100f, 0f);
		mapWindow.SetParent(mapCircle);
		mapWindow.anchoredPosition = Vector3.zero;
		mapWindow.sizeDelta = mapCircle.sizeDelta * 4f;
		compass.gameObject.SetActive(true);
		buttonPrompt.gameObject.SetActive(true);
		openedScale = scale;
		scale = 20f;
		desiredScale = 20f;
		if (!TownManager.manage.mapUnlocked)
		{
			mapCircle.gameObject.SetActive(false);
			CurrencyWindows.currency.windowOn(false);
		}
		else if (MenuButtonsTop.menu.subMenuOpen || WeatherManager.manage.isInside() || ChestWindow.chests.chestWindowOpen || CraftingManager.manage.craftMenuOpen || CheatScript.cheat.cheatMenuOpen)
		{
			mapCircle.gameObject.SetActive(false);
			CurrencyWindows.currency.windowOn(true);
		}
		else
		{
			mapCircle.gameObject.SetActive(true);
			CurrencyWindows.currency.windowOn(false);
		}
	}

	public void runMapFollow()
	{
		if ((bool)charToPointTo)
		{
			if (!mapOpen)
			{
				mapXPosDif = 0f;
				mapYPosDif = 0f;
			}
			if (firstOpen)
			{
				mapWindow.gameObject.SetActive(true);
				firstOpen = false;
			}
			scale = Mathf.Clamp(scale, 0.75f, 25f);
			if (Mathf.Abs(desiredScale - scale) < 0.005f)
			{
				desiredScale = scale;
			}
			else
			{
				desiredScale = Mathf.Lerp(desiredScale, scale, Time.deltaTime * 5f);
			}
			mapParent.localScale = new Vector3(desiredScale, desiredScale, 1f);
			float num = mapXPosDif + 250f * (0f - desiredScale) + (mapXPosDif + 250f - charPointer.localPosition.x) * desiredScale;
			float num2 = mapYPosDif + 250f * (0f - desiredScale) + (mapYPosDif + 250f - charPointer.localPosition.y) * desiredScale;
			mapParent.localPosition = new Vector3(num - mapXPosDif, num2 - mapYPosDif, 0f);
			if (!mapOpen && OptionsMenu.options.mapFacesNorth)
			{
				charDirPointer.localRotation = Quaternion.Euler(0f, 0f, 0f - charToPointTo.eulerAngles.y);
			}
			else if (mapOpen)
			{
				charDirPointer.localRotation = Quaternion.Euler(0f, 0f, 0f - charToPointTo.eulerAngles.y);
			}
			else
			{
				charDirPointer.localRotation = Quaternion.Euler(0f, 0f, CameraController.control.transform.eulerAngles.y - charToPointTo.eulerAngles.y);
			}
			Vector3 vector = new Vector3(charToPointTo.position.x / 2f / mapScale, charToPointTo.position.z / 2f / mapScale, 1f);
			charPointer.localPosition = new Vector3(vector.x, vector.y, 1f);
			trackOtherPlayers();
			if (!mapOpen && OptionsMenu.options.mapFacesNorth)
			{
				compass.localRotation = Quaternion.Euler(0f, 0f, 0f);
				mapMask.localRotation = Quaternion.Euler(0f, 0f, 0f);
				charPointer.localRotation = Quaternion.Euler(0f, 0f, 0f);
				charPointer.localRotation = Quaternion.Lerp(charPointer.localRotation, Quaternion.Euler(0f, 0f, 0f - CameraController.control.transform.eulerAngles.y), Time.deltaTime * 3f);
				charPointer.localScale = new Vector3(3f / desiredScale, 3f / desiredScale, 1f);
			}
			else if (!mapOpen)
			{
				mapMask.localRotation = Quaternion.Lerp(mapMask.localRotation, Quaternion.Euler(0f, 0f, CameraController.control.transform.eulerAngles.y), Time.deltaTime * 3f);
				compass.localRotation = Quaternion.Lerp(mapMask.localRotation, Quaternion.Euler(0f, 0f, CameraController.control.transform.eulerAngles.y), Time.deltaTime * 3f);
				charPointer.localRotation = Quaternion.Lerp(charPointer.localRotation, Quaternion.Euler(0f, 0f, 0f - CameraController.control.transform.eulerAngles.y), Time.deltaTime * 3f);
				charPointer.localScale = new Vector3(3f / desiredScale, 3f / desiredScale, 1f);
			}
			else
			{
				mapMask.localRotation = Quaternion.Euler(0f, 0f, 0f);
				charPointer.localRotation = Quaternion.Euler(0f, 0f, 0f);
				charPointer.localScale = new Vector3(1f / desiredScale, 1f / desiredScale, 1f);
			}
			if (refreshMap)
			{
				refreshMap = !refreshMap;
			}
		}
	}

	public IEnumerator clearMapForUnderground()
	{
		MonoBehaviour.print("Map being checked");
		mapWindowShape.color = Color.black;
		mapImage.texture = undergroundTex;
		int mapCounter = 0;
		int y = 0;
		yield return null;
		for (; y < WorldManager.manageWorld.getMapSize(); y++)
		{
			for (int i = 0; i < WorldManager.manageWorld.getMapSize(); i++)
			{
				undergroundPix[y * WorldManager.manageWorld.getMapSize() + i] = Color.black;
			}
			if ((float)mapCounter < 50f)
			{
				mapCounter++;
				continue;
			}
			mapCounter = 0;
			yield return null;
		}
		undergroundTex.SetPixels(undergroundPix);
		undergroundTex.Apply();
		mapMaterial.SetTexture("_MainTex", undergroundTex);
		hideAllAboveGroundIcons();
		while (RealWorldTimeLight.time.underGround)
		{
			checkIfChunkIsOnUndergroundMap(charToPointTo.position);
			yield return mapWait;
		}
	}

	public IEnumerator scanTheMap()
	{
		mapWindowShape.color = defaultMapMaskBackgroundColour;
		mapImage.texture = noiseTex;
		int mapCounter = 0;
		int y = 0;
		yield return null;
		for (; y < WorldManager.manageWorld.getMapSize(); y++)
		{
			for (int i = 0; i < WorldManager.manageWorld.getMapSize(); i++)
			{
				checkIfNeedsIcon(i, y);
				if (WorldManager.manageWorld.heightMap[i, y] < 1)
				{
					if (checkTileObject(WorldManager.manageWorld.onTileMap[i, y]) != -1)
					{
						pix[y * WorldManager.manageWorld.getMapSize() + i] = tileObjectsShownOnMapColor[checkTileObject(WorldManager.manageWorld.onTileMap[i, y])];
					}
					else if (WorldManager.manageWorld.heightMap[i, y] < -1)
					{
						pix[y * WorldManager.manageWorld.getMapSize() + i] = deepWater;
					}
					else
					{
						pix[y * WorldManager.manageWorld.getMapSize() + i] = water;
					}
				}
				else if (checkTileObject(WorldManager.manageWorld.onTileMap[i, y]) != -1)
				{
					pix[y * WorldManager.manageWorld.getMapSize() + i] = tileObjectsShownOnMapColor[checkTileObject(WorldManager.manageWorld.onTileMap[i, y])];
				}
				else
				{
					pix[y * WorldManager.manageWorld.getMapSize() + i] = Color.Lerp(WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[i, y]].tileColorOnMap, mapBackColor, (float)WorldManager.manageWorld.heightMap[i, y] / 12f);
					if (checkIfShouldBeHeightLine(i, y))
					{
						pix[y * WorldManager.manageWorld.getMapSize() + i] = Color.Lerp(pix[y * WorldManager.manageWorld.getMapSize() + i], heightLineColour, 0.075f);
					}
				}
			}
			if ((float)mapCounter < 50f)
			{
				mapCounter++;
				continue;
			}
			mapCounter = 0;
			yield return null;
		}
		drawBridgesOnMap();
		noiseTex.SetPixels(pix);
		noiseTex.Apply();
		mapMaterial.SetTexture("_MainTex", noiseTex);
	}

	private bool checkIfShouldBeHeightLine(int xPos, int yPos)
	{
		if (xPos < 2 || xPos >= WorldManager.manageWorld.getMapSize() - 2 || yPos < 2 || yPos >= WorldManager.manageWorld.getMapSize() - 2)
		{
			return false;
		}
		if (WorldManager.manageWorld.heightMap[xPos - 1, yPos] < WorldManager.manageWorld.heightMap[xPos, yPos] || WorldManager.manageWorld.heightMap[xPos + 1, yPos] < WorldManager.manageWorld.heightMap[xPos, yPos] || WorldManager.manageWorld.heightMap[xPos, yPos + 1] < WorldManager.manageWorld.heightMap[xPos, yPos] || WorldManager.manageWorld.heightMap[xPos, yPos - 1] < WorldManager.manageWorld.heightMap[xPos, yPos])
		{
			return true;
		}
		return false;
	}

	public void hideAllAboveGroundIcons()
	{
		for (int i = 0; i < iconsOnMap.Count; i++)
		{
			if (iconsOnMap[i].iconName == "Mine")
			{
				iconsOnMap[i].iconName = "Mine Exit";
			}
			else if (iconsOnMap[i].placedAboveGround)
			{
				iconsOnMap[i].gameObject.SetActive(false);
			}
			else
			{
				iconsOnMap[i].gameObject.SetActive(true);
			}
		}
	}

	public void showAllAboveGroundIcons()
	{
		for (int i = 0; i < iconsOnMap.Count; i++)
		{
			if (iconsOnMap[i].iconName == "Mine Exit")
			{
				iconsOnMap[i].iconName = "Mine";
			}
			else if (iconsOnMap[i].placedAboveGround)
			{
				iconsOnMap[i].gameObject.SetActive(true);
			}
			else
			{
				iconsOnMap[i].gameObject.SetActive(false);
			}
		}
	}

	public void updateMapOnPlaced()
	{
		StartCoroutine(updateMap());
	}

	public void checkIfChunkIsOnUndergroundMap(Vector3 playerPos)
	{
		int num = Mathf.RoundToInt(playerPos.x / 2f / (float)WorldManager.manageWorld.getChunkSize()) * WorldManager.manageWorld.getChunkSize();
		int num2 = Mathf.RoundToInt(playerPos.z / 2f / (float)WorldManager.manageWorld.getChunkSize()) * WorldManager.manageWorld.getChunkSize();
		int num3 = num;
		int num4 = num2;
		for (int i = -2; i < 3; i++)
		{
			for (int j = -2; j < 3; j++)
			{
				num3 = num + j * WorldManager.manageWorld.getChunkSize();
				num4 = num2 + i * WorldManager.manageWorld.getChunkSize();
				for (int k = 0; k < 10; k++)
				{
					for (int l = 0; l < 10; l++)
					{
						if (getDistanceFromCharacter(num3 + l, num4 + k) > 0f && WorldManager.manageWorld.isPositionOnMap(num3 + l, num4 + k))
						{
							Color color = ((WorldManager.manageWorld.heightMap[num3 + l, num4 + k] < 1) ? ((checkTileObject(WorldManager.manageWorld.onTileMap[num3 + l, num4 + k]) != -1) ? tileObjectsShownOnMapColor[checkTileObject(WorldManager.manageWorld.onTileMap[num3 + l, num4 + k])] : ((WorldManager.manageWorld.heightMap[num3 + l, num4 + k] >= -1) ? Color.Lerp(Color.blue, Color.white, 0.65f) : Color.Lerp(Color.blue, Color.white, 0.85f))) : ((WorldManager.manageWorld.onTileMap[num3 + l, num4 + k] == 29) ? Color.Lerp(Color.grey, Color.black, 0.65f) : ((checkTileObject(WorldManager.manageWorld.onTileMap[num3 + l, num4 + k]) <= -1) ? Color.Lerp(WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[num3 + l, num4 + k]].tileColorOnMap, mapBackColor, (float)WorldManager.manageWorld.heightMap[num3 + l, num4 + k] / 12f) : tileObjectsShownOnMapColor[checkTileObject(WorldManager.manageWorld.onTileMap[num3 + l, num4 + k])])));
							if (undergroundPix[(num4 + k) * WorldManager.manageWorld.getMapSize() + (num3 + l)] != color && (undergroundPix[(num4 + k) * WorldManager.manageWorld.getMapSize() + (num3 + l)] == Color.black || undergroundPix[(num4 + k) * WorldManager.manageWorld.getMapSize() + (num3 + l)].a < getDistanceFromCharacter(num3 + l, num4 + k)))
							{
								undergroundPix[(num4 + k) * WorldManager.manageWorld.getMapSize() + (num3 + l)] = color;
								undergroundPix[(num4 + k) * WorldManager.manageWorld.getMapSize() + (num3 + l)].a = getDistanceFromCharacter(num3 + l, num4 + k);
							}
						}
					}
				}
			}
		}
		undergroundTex.SetPixels(undergroundPix);
		undergroundTex.Apply();
		mapMaterial.SetTexture("_MainTex", undergroundTex);
	}

	public void moveMapBackUpFromMines()
	{
		mapImage.texture = noiseTex;
		mapWindowShape.color = defaultMapMaskBackgroundColour;
		noiseTex.SetPixels(pix);
		noiseTex.Apply();
		mapMaterial.SetTexture("_MainTex", noiseTex);
		showAllAboveGroundIcons();
		if (!NetworkMapSharer.share.isServer)
		{
			StartCoroutine(scanTheMap());
		}
	}

	public float getDistanceFromCharacter(int x, int y)
	{
		float num = Vector3.Distance(new Vector3(charToPointTo.transform.position.x, 0f, charToPointTo.transform.position.z), new Vector3(x * 2, 0f, y * 2));
		if (num <= 18f)
		{
			if (num <= 12f)
			{
				return 0.7f;
			}
			if (num <= 14f)
			{
				return 0.8f;
			}
			if (num <= 16f)
			{
				return 0.9f;
			}
			return 1f;
		}
		return 1f - Mathf.Clamp(num, 18f, 40f) / 40f;
	}

	public IEnumerator updateMap()
	{
		mapImage.texture = noiseTex;
		for (int y = 0; y < WorldManager.manageWorld.getMapSize() / WorldManager.manageWorld.getChunkSize(); y++)
		{
			for (int x = 0; x < WorldManager.manageWorld.getMapSize() / WorldManager.manageWorld.getChunkSize(); x++)
			{
				if (!WorldManager.manageWorld.chunkChangedMap[x, y])
				{
					continue;
				}
				for (int i = 0; i < 10; i++)
				{
					for (int j = 0; j < 10; j++)
					{
						checkIfNeedsIcon(x * 10 + j, y * 10 + i);
						if (WorldManager.manageWorld.heightMap[x * 10 + j, y * 10 + i] < 1)
						{
							if (checkTileObject(WorldManager.manageWorld.onTileMap[x * 10 + j, y * 10 + i]) != -1)
							{
								pix[(y * 10 + i) * WorldManager.manageWorld.getMapSize() + (x * 10 + j)] = tileObjectsShownOnMapColor[checkTileObject(WorldManager.manageWorld.onTileMap[x * 10 + j, y * 10 + i])];
							}
							else if (WorldManager.manageWorld.heightMap[x * 10 + j, y * 10 + i] < -1)
							{
								pix[(y * 10 + i) * WorldManager.manageWorld.getMapSize() + (x * 10 + j)] = deepWater;
							}
							else
							{
								pix[(y * 10 + i) * WorldManager.manageWorld.getMapSize() + (x * 10 + j)] = water;
							}
						}
						else if (checkTileObject(WorldManager.manageWorld.onTileMap[x * 10 + j, y * 10 + i]) > -1)
						{
							pix[(y * 10 + i) * WorldManager.manageWorld.getMapSize() + (x * 10 + j)] = tileObjectsShownOnMapColor[checkTileObject(WorldManager.manageWorld.onTileMap[x * 10 + j, y * 10 + i])];
						}
						else
						{
							pix[(y * 10 + i) * WorldManager.manageWorld.getMapSize() + (x * 10 + j)] = Color.Lerp(WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[x * 10 + j, y * 10 + i]].tileColorOnMap, mapBackColor, (float)WorldManager.manageWorld.heightMap[x * 10 + j, y * 10 + i] / 12f);
							if (checkIfShouldBeHeightLine(x * 10 + j, y * 10 + i))
							{
								pix[(y * 10 + i) * WorldManager.manageWorld.getMapSize() + (x * 10 + j)] = Color.Lerp(pix[(y * 10 + i) * WorldManager.manageWorld.getMapSize() + (x * 10 + j)], heightLineColour, 0.075f);
							}
						}
					}
				}
				yield return null;
			}
		}
		drawBridgesOnMap();
		noiseTex.SetPixels(pix);
		noiseTex.Apply();
		mapMaterial.SetTexture("_MainTex", noiseTex);
	}

	public void trackOtherPlayers(Transform trackMe)
	{
		otherPlayersToTrack.Add(trackMe);
		RectTransform component = Object.Instantiate(otherCharPointerPrefab, mapParent).GetComponent<RectTransform>();
		otherPlayerIcons.Add(component);
		component.GetComponent<OtherPlayerIcon>().setName(trackMe.GetComponent<EquipItemToChar>().playerName);
		playerMarkersOnTop();
	}

	public void changeMapIconName(Transform changeMe, string newName)
	{
		for (int i = 0; i < otherPlayersToTrack.Count; i++)
		{
			if (otherPlayersToTrack[i] == changeMe)
			{
				otherPlayerIcons[i].GetComponent<OtherPlayerIcon>().setName(newName);
			}
		}
	}

	public void unTrackOtherPlayers(Transform unTrackMe)
	{
		if (otherPlayersToTrack.Contains(unTrackMe))
		{
			int index = otherPlayersToTrack.IndexOf(unTrackMe);
			otherPlayersToTrack.RemoveAt(index);
			Object.Destroy(otherPlayerIcons[index].gameObject);
			otherPlayerIcons.RemoveAt(index);
		}
	}

	public void checkIfNeedsIcon(int xPos, int yPos)
	{
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] > -1 && (bool)WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].mapIcon)
		{
			for (int i = 0; i < iconsOnMap.Count; i++)
			{
				if (iconsOnMap[i].tileObjectId == WorldManager.manageWorld.onTileMap[xPos, yPos])
				{
					return;
				}
			}
			mapIcon component = Object.Instantiate(mapIconPrefab, mapParent).GetComponent<mapIcon>();
			component.setUp(WorldManager.manageWorld.onTileMap[xPos, yPos], new Vector3(xPos * 2, 0f, yPos * 2));
			iconsOnMap.Add(component);
			playerMarkersOnTop();
		}
		else if (WorldManager.manageWorld.onTileMap[xPos, yPos] > -1 && (bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectBridge)
		{
			bridgePositions.Add(new int[2] { xPos, yPos });
		}
	}

	public void createTeleIcons(string dir)
	{
		mapIcon component = Object.Instantiate(mapIconPrefab, mapParent).GetComponent<mapIcon>();
		component.setUpTelePoint(dir);
		iconsOnMap.Add(component);
		playerMarkersOnTop();
	}

	public void createTaskIcon(PostOnBoard postToTrack)
	{
		if (postToTrack.getRequiredLocation() != Vector3.zero && !taskAlreadyHasIcon(postToTrack))
		{
			mapIcon component = Object.Instantiate(mapIconPrefab, mapParent).GetComponent<mapIcon>();
			component.setUpQuestIcon(postToTrack);
			iconsOnMap.Add(component);
			playerMarkersOnTop();
		}
	}

	public void removeTaskIcon(mapIcon toRemove)
	{
		iconsOnMap.Remove(toRemove);
		Object.Destroy(toRemove.gameObject);
	}

	private bool taskAlreadyHasIcon(PostOnBoard postToTrack)
	{
		for (int i = 0; i < iconsOnMap.Count; i++)
		{
			if (iconsOnMap[i].isConnectedToTask(postToTrack))
			{
				return true;
			}
		}
		return false;
	}

	public void createCustomMarkerAtMouse()
	{
		if (Inventory.inv.usingMouse && InputMaster.input.Interact() && iconSelectorOpen)
		{
			closeButton();
		}
		else
		{
			if ((!Inventory.inv.usingMouse || !InputMaster.input.Interact()) && (Inventory.inv.usingMouse || !InputMaster.input.Other()))
			{
				return;
			}
			bool flag = false;
			for (int i = 0; i < iconsOnMap.Count; i++)
			{
				if (iconsOnMap[i].placedByPlayer == NetworkMapSharer.share.localChar.netId)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				NetworkMapSharer.share.localChar.CmdRemoveMyMarker();
			}
			else
			{
				Vector2 localPoint;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(mapImage.rectTransform, mapCursor.transform.position, null, out localPoint);
			}
		}
	}

	public void createCustomMarker(uint placedBy, Vector2 position, int iconNo, mapIcon.iconType myType)
	{
		mapIcon component = Object.Instantiate(mapIconPrefab, position, Quaternion.identity, mapParent).GetComponent<mapIcon>();
		component.transform.localScale = Vector3.zero;
		component.customMarkerSetup(new Vector3(position.x * 8f, 0f, position.y * 8f), placedBy, iconNo, myType);
		iconsOnMap.Add(component);
		playerMarkersOnTop();
	}

	public void createLocalCustomMarker(Vector2 position, int markerID)
	{
		mapIcon component = Object.Instantiate(mapIconPrefab, position, Quaternion.identity, mapParent).GetComponent<mapIcon>();
		component.transform.localScale = Vector3.zero;
		component.normalMarkerSetup(new Vector3(position.x * 8f, 0f, position.y * 8f), markerID);
		iconsOnMap.Add(component);
		playerMarkersOnTop();
	}

	public void removeMarkerByPlayer(uint markerBelongsTo)
	{
		for (int i = 0; i < iconsOnMap.Count; i++)
		{
			if (iconsOnMap[i].placedByPlayer == markerBelongsTo)
			{
				Object.Destroy(iconsOnMap[i].gameObject);
				iconsOnMap.RemoveAt(i);
			}
		}
	}

	private void playerMarkersOnTop()
	{
		for (int i = 0; i < otherPlayerIcons.Count; i++)
		{
			otherPlayerIcons[i].SetAsLastSibling();
		}
		charPointer.SetAsLastSibling();
	}

	public mapIcon createMapIconForVehicle(Transform toFollow, int vehicleSaveId)
	{
		if (vehicleSaveId < 0)
		{
			return null;
		}
		mapIcon component = Object.Instantiate(mapIconPrefab, mapParent).GetComponent<mapIcon>();
		component.setUpFollowVehicle(toFollow, SaveLoad.saveOrLoad.vehiclePrefabs[vehicleSaveId].GetComponent<Vehicle>().mapIconSprite);
		component.myIconType = mapIcon.iconType.Vehicle;
		component.iconId = vehicleSaveId;
		playerMarkersOnTop();
		component.iconName = "???";
		for (int i = 0; i < Inventory.inv.allItems.Length; i++)
		{
			if ((bool)Inventory.inv.allItems[i].spawnPlaceable && (bool)Inventory.inv.allItems[i].spawnPlaceable.GetComponent<Vehicle>() && Inventory.inv.allItems[i].spawnPlaceable.GetComponent<Vehicle>().saveId == vehicleSaveId)
			{
				component.iconName = Inventory.inv.allItems[i].getInvItemName();
				break;
			}
		}
		return component;
	}

	public void trackOtherPlayers()
	{
		for (int i = 0; i < otherPlayerIcons.Count; i++)
		{
			Vector3 vector = new Vector3(otherPlayersToTrack[i].position.x / 2f / mapScale, otherPlayersToTrack[i].position.z / 2f / mapScale, 1f);
			otherPlayerIcons[i].localPosition = new Vector3(vector.x, vector.y, 1f);
			if (!mapOpen)
			{
				otherPlayerIcons[i].localRotation = Quaternion.Lerp(otherPlayerIcons[i].localRotation, Quaternion.Euler(0f, 0f, 0f - CameraController.control.transform.eulerAngles.y), Time.deltaTime * 3f);
				otherPlayerIcons[i].localScale = new Vector3(2f / desiredScale, 2f / desiredScale, 1f);
			}
			else
			{
				otherPlayerIcons[i].localRotation = Quaternion.Euler(0f, 0f, 0f);
				otherPlayerIcons[i].localScale = new Vector3(1f / desiredScale, 1f / desiredScale, 1f);
			}
		}
	}

	public void connectMainChar(Transform mainChar)
	{
		charToPointTo = mainChar;
		map.changeMapWindow();
		if (!RealWorldTimeLight.time.underGround)
		{
			StartCoroutine(scanTheMap());
			return;
		}
		MonoBehaviour.print("Setting up map for underground");
		StartCoroutine(clearMapForUnderground());
	}

	public int checkTileObject(int onThisTile)
	{
		if (tileObjectShowOnMap.Length != 0)
		{
			for (int i = 0; i < tileObjectShowOnMap.Length; i++)
			{
				if (tileObjectShowOnMap[i].tileObjectId == onThisTile)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public void handlePointerSizeAndPos(RectTransform pointerName, Vector3 pointToPosition)
	{
		if (!(pointToPosition != Vector3.zero))
		{
			return;
		}
		if (mapOpen)
		{
			pointerName.localPosition = new Vector3(pointToPosition.x / 2f / mapScale, pointToPosition.z / 2f / mapScale, 1f);
			pointerName.localRotation = Quaternion.Euler(0f, 0f, 0f);
			pointerName.localScale = new Vector3(2f / desiredScale, 2f / desiredScale, 1f);
			return;
		}
		pointerName.localRotation = Quaternion.Lerp(pointerName.localRotation, Quaternion.Euler(0f, 0f, 0f - CameraController.control.transform.eulerAngles.y), Time.deltaTime * 3f);
		Vector3 a = new Vector3(charToPointTo.position.x, 0f, charToPointTo.position.z);
		Vector3 b = new Vector3(pointToPosition.x, 0f, pointToPosition.z);
		if (Vector3.Distance(a, b) < 100f)
		{
			pointerName.localPosition = Vector3.Lerp(pointerName.localPosition, new Vector3(pointToPosition.x / 2f / mapScale, pointToPosition.z / 2f / mapScale, 1f), Time.deltaTime * 3f);
			pointerName.localScale = Vector3.Lerp(pointerName.localScale, new Vector3(2f / desiredScale, 2f / desiredScale, 1f), Time.deltaTime * 2f);
		}
		else
		{
			Vector3 vector = charToPointTo.position + (pointToPosition - charToPointTo.position).normalized * 115f;
			pointerName.localPosition = Vector3.Lerp(pointerName.localPosition, new Vector3(vector.x / 2f / mapScale, vector.z / 2f / mapScale, 1f), Time.deltaTime * 3f);
			pointerName.localScale = Vector3.Lerp(pointerName.localScale, new Vector3(1.25f / desiredScale, 1.25f / desiredScale, 1f), Time.deltaTime * 2f);
		}
	}

	public InvButton checkForIcon()
	{
		PointerEventData pointerEventData = new PointerEventData(null);
		pointerEventData.position = mapCursor.transform.position;
		List<RaycastResult> list = new List<RaycastResult>();
		mapCaster.Raycast(pointerEventData, list);
		if (list.Count > 0)
		{
			return list[0].gameObject.GetComponent<InvButton>();
		}
		return null;
	}

	public void drawBridgesOnMap()
	{
		for (int i = 0; i < bridgePositions.Count; i++)
		{
			MonoBehaviour.print("Doing a bridge check here");
			int num = bridgePositions[i][0];
			int num2 = bridgePositions[i][1];
			Color bridgeColour = WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[num, num2]].tileObjectBridge.bridgeColour;
			pix[num2 * WorldManager.manageWorld.getMapSize() + num] = bridgeColour;
			for (int j = 1; WorldManager.manageWorld.onTileMap[num + j, num2] < -1; j++)
			{
				pix[num2 * WorldManager.manageWorld.getMapSize() + (num + j)] = bridgeColour;
			}
			for (int k = 1; WorldManager.manageWorld.onTileMap[num, num2 + k] < -1; k++)
			{
				pix[(num2 + k) * WorldManager.manageWorld.getMapSize() + num] = bridgeColour;
				for (int l = 1; WorldManager.manageWorld.onTileMap[num + l, num2 + k] < -1; l++)
				{
					pix[(num2 + k) * WorldManager.manageWorld.getMapSize() + (num + l)] = bridgeColour;
				}
			}
		}
		bridgePositions.Clear();
	}
}
