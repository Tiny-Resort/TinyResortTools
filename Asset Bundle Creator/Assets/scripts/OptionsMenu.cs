using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
	public static OptionsMenu options;

	public Image fullscreenToggle;

	private List<Resolution> resolutions = new List<Resolution>();

	private bool firstSetUp = true;

	[Header("Quality Buttons")]
	public InvButton[] qualityButtons;

	public Color selectedColor;

	public Color baseColor;

	public InvButton[] chunkViewDistanceButtons;

	[Header("Sound Options")]
	public Image masterVolumeSlider;

	public Image masterVolumeBack;

	public Image volumeSlider;

	public Image volumeBack;

	public Image UIVolumeSlider;

	public Image UIVolumeBack;

	public Image musicVolumeSlider;

	public Image musicVolumeBack;

	public TextMeshProUGUI resolutionText;

	[Header("Control Options")]
	public GameObject invertXTick;

	public GameObject invertYTick;

	public GameObject cameraToggleTick;

	public GameObject cameraButtonTick;

	public GameObject rumbleTick;

	public GameObject autoDetectControllerTick;

	public GameObject mapNorthTick;

	[Header("Nametag Options")]
	public GameObject nameTagTick;

	[Header("Other Options")]
	public GameObject menuParent;

	public GameObject menuButtons;

	public GameObject optionWindow;

	[Header("Voice Options")]
	public Image voiceOnTick;

	public InvButton[] voiceSpeedButtons;

	public InvButton journalCloseButton;

	public bool openedInGame;

	public bool nameTagsOn;

	public UnityEvent nameTagSwitch = new UnityEvent();

	public int chunkViewDistance = 4;

	public int voiceSpeed;

	public bool voiceOn = true;

	public int textSpeed;

	public bool optionWindowOpen;

	public bool rumbleOn = true;

	public CanvasScaler[] canvases;

	public Image canvasWideButton;

	public Image canvasNormalButton;

	public bool autoDetectOn = true;

	public int refreshRate;

	public bool mapFacesNorth;

	public Image snapCursorCheckBox;

	private bool isUsingSnapCursor;

	private int showingNo;

	public static bool FullScreenMode
	{
		get
		{
			return Screen.fullScreen;
		}
		set
		{
			FullScreenMode fullscreenMode = ((!value) ? UnityEngine.FullScreenMode.Windowed : UnityEngine.FullScreenMode.ExclusiveFullScreen);
			Screen.fullScreen = value;
			Screen.SetResolution(Screen.width, Screen.height, fullscreenMode, options.refreshRate);
			MonoBehaviour.print("Refresh rate set to: " + options.refreshRate + " actually set to " + Screen.currentResolution.refreshRate);
		}
	}

	private void Awake()
	{
		options = this;
	}

	private void Start()
	{
		nameTagSwitch.AddListener(setNameTagOnOff);
		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			if (!resolutions.Contains(Screen.resolutions[i]))
			{
				resolutions.Add(Screen.resolutions[i]);
			}
			if (Screen.resolutions[i].ToString() == Screen.currentResolution.ToString())
			{
				showingNo = i;
			}
		}
		resolutionText.text = resolutions[showingNo].width + " x " + resolutions[showingNo].height;
		setUpVolumeSliders();
		fullscreenToggle.enabled = Screen.fullScreen;
		if (PlayerPrefs.HasKey("chunkDistance"))
		{
			chunkViewDistance = PlayerPrefs.GetInt("chunkDistance");
			NewChunkLoader.loader.setChunkDistance(chunkViewDistance);
		}
		if (PlayerPrefs.HasKey("snapCursorOn"))
		{
			if (PlayerPrefs.GetInt("snapCursorOn") == 1)
			{
				isUsingSnapCursor = false;
				Inventory.inv.turnSnapCursorOnOff(false);
			}
			else
			{
				isUsingSnapCursor = true;
				Inventory.inv.turnSnapCursorOnOff(true);
			}
		}
		else
		{
			isUsingSnapCursor = true;
			Inventory.inv.turnSnapCursorOnOff(true);
		}
		snapCursorCheckBox.enabled = isUsingSnapCursor;
		for (int j = 0; j < qualityButtons.Length; j++)
		{
			if (j == QualitySettings.GetQualityLevel())
			{
				qualityButtons[j].GetComponent<Image>().color = selectedColor;
			}
			else
			{
				qualityButtons[j].GetComponent<Image>().color = baseColor;
			}
		}
		selectChunkViewDistanceButton();
		if (PlayerPrefs.HasKey("invertXFree"))
		{
			setInvertX(PlayerPrefs.GetInt("invertXFree") == 1);
		}
		if (PlayerPrefs.HasKey("invertYFree"))
		{
			setInvertY(PlayerPrefs.GetInt("invertYFree") == 1);
		}
		if (PlayerPrefs.HasKey("toggleLook"))
		{
			setCameraToggle(PlayerPrefs.GetInt("toggleLook") == 1);
		}
		if (PlayerPrefs.HasKey("toggleNameTags"))
		{
			nameTagsOn = PlayerPrefs.GetInt("toggleNameTags") == 1;
			setNameTagOnOff();
		}
		if (PlayerPrefs.HasKey("voiceOn") && PlayerPrefs.GetInt("voiceOn") == 1)
		{
			turnOnOffVoice();
		}
		if (PlayerPrefs.HasKey("textSpeed"))
		{
			ChangeTextSpeed(PlayerPrefs.GetInt("textSpeed"));
		}
		else
		{
			voiceSpeedButtons[0].GetComponent<Image>().color = selectedColor;
		}
		if (PlayerPrefs.HasKey("rumbleOn") && PlayerPrefs.GetInt("rumbleOn") == 1)
		{
			swapRumble();
		}
		rumbleTick.SetActive(rumbleOn);
		LocalizationManager.CurrentLanguage = "English";
		if (PlayerPrefs.HasKey("canvasScale"))
		{
			if (PlayerPrefs.GetInt("canvasScale") == 1)
			{
				makeCanvasWide();
			}
			else
			{
				canvasWideButton.color = baseColor;
				canvasNormalButton.color = selectedColor;
			}
		}
		else
		{
			canvasWideButton.color = baseColor;
			canvasNormalButton.color = selectedColor;
		}
		if (PlayerPrefs.HasKey("autoDetectController"))
		{
			if (PlayerPrefs.GetInt("autoDetectController") == 0)
			{
				autoDetectControllerTick.SetActive(true);
				autoDetectOn = true;
			}
			else
			{
				autoDetectControllerTick.SetActive(false);
				autoDetectOn = false;
			}
		}
		else
		{
			autoDetectControllerTick.SetActive(true);
		}
		if (PlayerPrefs.HasKey("mapFacesNorth") && PlayerPrefs.GetInt("mapFacesNorth") == 1)
		{
			mapFacesNorth = true;
			mapNorthTick.SetActive(mapFacesNorth);
		}
	}

	public void autoDetectOnOff()
	{
		autoDetectOn = !autoDetectOn;
		autoDetectControllerTick.SetActive(autoDetectOn);
		if (autoDetectOn)
		{
			PlayerPrefs.SetInt("autoDetectController", 0);
		}
		else
		{
			PlayerPrefs.SetInt("autoDetectController", 1);
		}
	}

	public void mapFaceNorth()
	{
		mapFacesNorth = !mapFacesNorth;
		mapNorthTick.SetActive(mapFacesNorth);
		if (mapFacesNorth)
		{
			PlayerPrefs.SetInt("mapFacesNorth", 1);
		}
		else
		{
			PlayerPrefs.SetInt("mapFacesNorth", 0);
		}
	}

	public void makeCanvasWide()
	{
		PlayerPrefs.SetInt("canvasScale", 1);
		for (int i = 0; i < canvases.Length; i++)
		{
			canvases[i].matchWidthOrHeight = 0.7f;
		}
		canvasWideButton.color = selectedColor;
		canvasNormalButton.color = baseColor;
	}

	public void makeCanvasNormal()
	{
		PlayerPrefs.SetInt("canvasScale", 0);
		for (int i = 0; i < canvases.Length; i++)
		{
			canvases[i].matchWidthOrHeight = 0.4f;
		}
		canvasWideButton.color = baseColor;
		canvasNormalButton.color = selectedColor;
	}

	public void clearAllPlayerPrefs()
	{
		PlayerPrefs.DeleteAll();
	}

	public void swapRumble()
	{
		rumbleOn = !rumbleOn;
		if (rumbleOn)
		{
			InputMaster.input.doRumble(0.5f, 10f);
			PlayerPrefs.SetInt("rumbleOn", 0);
		}
		else
		{
			InputMaster.input.stopRumble();
			PlayerPrefs.SetInt("rumbleOn", 1);
		}
		rumbleTick.SetActive(rumbleOn);
	}

	public void openOptionsMenu()
	{
		optionWindowOpen = true;
	}

	public void pressNameTagTogggle()
	{
		nameTagsOn = !nameTagsOn;
		if (nameTagsOn)
		{
			PlayerPrefs.SetInt("toggleNameTags", 1);
		}
		else
		{
			PlayerPrefs.SetInt("toggleNameTags", 0);
		}
		nameTagSwitch.Invoke();
	}

	private void setNameTagOnOff()
	{
		nameTagTick.SetActive(nameTagsOn);
	}

	private void setCameraToggle(bool isOn)
	{
		cameraToggleTick.SetActive(isOn);
		cameraButtonTick.SetActive(!isOn);
		if (isOn)
		{
			PlayerPrefs.SetInt("toggleLook", 1);
			CameraController.control.toggle = true;
		}
		else
		{
			PlayerPrefs.SetInt("toggleLook", 0);
			CameraController.control.toggle = false;
		}
	}

	private void setInvertX(bool isOn)
	{
		invertXTick.SetActive(isOn);
		if (isOn)
		{
			PlayerPrefs.SetInt("invertXFree", 1);
			CameraController.control.xMod = -1;
		}
		else
		{
			PlayerPrefs.SetInt("invertXFree", 0);
			CameraController.control.xMod = 1;
		}
	}

	private void setInvertY(bool isOn)
	{
		invertYTick.SetActive(isOn);
		if (isOn)
		{
			PlayerPrefs.SetInt("invertYFree", 1);
			CameraController.control.YMod = -1;
		}
		else
		{
			PlayerPrefs.SetInt("invertYFree", 0);
			CameraController.control.YMod = 1;
		}
	}

	public void switchToggle(bool onOrOff)
	{
		if (cameraToggleTick.activeInHierarchy)
		{
			setCameraToggle(false);
		}
		else
		{
			setCameraToggle(true);
		}
	}

	public void SwitchInvertX()
	{
		if (invertXTick.activeInHierarchy)
		{
			setInvertX(false);
		}
		else
		{
			setInvertX(true);
		}
	}

	public void SwitchInvertY()
	{
		if (invertYTick.activeInHierarchy)
		{
			setInvertY(false);
		}
		else
		{
			setInvertY(true);
		}
	}

	public void turnOnOffVoice()
	{
		voiceOn = !voiceOn;
		voiceOnTick.enabled = voiceOn;
		if (!voiceOn)
		{
			PlayerPrefs.SetInt("voiceOn", 1);
		}
		else
		{
			PlayerPrefs.SetInt("voiceOn", 0);
		}
	}

	public void ChangeTextSpeed(int newSpeed)
	{
		textSpeed = newSpeed;
		PlayerPrefs.SetInt("textSpeed", newSpeed);
		for (int i = 0; i < voiceSpeedButtons.Length; i++)
		{
			if (i == textSpeed)
			{
				voiceSpeedButtons[i].GetComponent<Image>().color = selectedColor;
			}
			else
			{
				voiceSpeedButtons[i].GetComponent<Image>().color = baseColor;
			}
		}
	}

	public void setChunkDistanceButton(int newChunkDistance)
	{
		PlayerPrefs.SetInt("chunkDistance", newChunkDistance);
		chunkViewDistance = newChunkDistance;
		NewChunkLoader.loader.setChunkDistance(chunkViewDistance);
		selectChunkViewDistanceButton();
	}

	public void openOptionsMenuInGame()
	{
		openOptionsMenu();
		optionWindow.SetActive(true);
		menuParent.SetActive(true);
		openedInGame = true;
	}

	public void closeOptionsMenuInGame()
	{
		optionWindowOpen = false;
		optionWindow.SetActive(false);
		if (openedInGame)
		{
			Inventory.inv.setAsActiveCloseButton(journalCloseButton);
			menuParent.SetActive(false);
		}
		else
		{
			menuParent.SetActive(true);
			menuButtons.SetActive(true);
		}
		openedInGame = false;
	}

	private void OnEnable()
	{
	}

	public void changeFullScreenMode()
	{
		FullScreenMode = !FullScreenMode;
		StartCoroutine(changeToggleDelay());
	}

	private IEnumerator changeToggleDelay()
	{
		yield return null;
		fullscreenToggle.enabled = Screen.fullScreen;
	}

	public void changeRefreshRate(int target)
	{
		refreshRate = target;
		Screen.SetResolution(resolutions[showingNo].width, resolutions[showingNo].height, Screen.fullScreen, refreshRate);
		MonoBehaviour.print("Refresh rate set to: " + target + " actually set to " + Screen.currentResolution.refreshRate);
	}

	public void changeSnapCursorOnOff()
	{
		isUsingSnapCursor = !isUsingSnapCursor;
		Inventory.inv.turnSnapCursorOnOff(isUsingSnapCursor);
		snapCursorCheckBox.enabled = isUsingSnapCursor;
	}

	public void ChangeResolutions(int dif)
	{
		showingNo += dif;
		if (showingNo > resolutions.Count - 1)
		{
			showingNo = 0;
		}
		if (showingNo < 0)
		{
			showingNo = resolutions.Count - 1;
		}
		resolutionText.text = resolutions[showingNo].width + " x " + resolutions[showingNo].height;
	}

	public void ApplyResolution()
	{
		Debug.Log("called here");
		Screen.SetResolution(resolutions[showingNo].width, resolutions[showingNo].height, Screen.fullScreen, refreshRate);
	}

	private string ResToString(Resolution res)
	{
		return res.width + " x " + res.height;
	}

	public void changeLanguageEnglish()
	{
		LocalizationManager.CurrentLanguage = "English";
	}

	public void changeLanguageFrench()
	{
		LocalizationManager.CurrentLanguage = "French";
	}

	public void changeLanguageChinese()
	{
		LocalizationManager.CurrentLanguage = "Chinese";
	}

	public void changeLanguageRussian()
	{
		LocalizationManager.CurrentLanguage = "Russian";
	}

	public void setQuality(int newQuality)
	{
		QualitySettings.SetQualityLevel(newQuality, true);
		for (int i = 0; i < qualityButtons.Length; i++)
		{
			if (i == newQuality)
			{
				qualityButtons[i].GetComponent<Image>().color = selectedColor;
			}
			else
			{
				qualityButtons[i].GetComponent<Image>().color = baseColor;
			}
		}
	}

	public void changeLanguage(string languageName)
	{
		LocalizationManager.CurrentLanguage = languageName;
	}

	public void changeSoundVolume(float dif)
	{
		changeSoundVolumeAmounts(dif);
		if (InputMaster.input.UISelectHeld())
		{
			StartCoroutine(holdVolumeButton(true, dif));
		}
	}

	private void changeSoundVolumeAmounts(float dif)
	{
		float num = Mathf.Clamp(SoundManager.manage.getSoundVolumeForChange() + dif, 0f, 4f);
		volumeSlider.fillAmount = num / 4f;
		SoundManager.manage.setSoundEffectVolume(num);
		PlayerPrefs.SetFloat("soundVolume", num);
		PlayerPrefs.Save();
	}

	public void changeMasterVolume(float dif)
	{
		changeMasterVolumeAmounts(dif);
	}

	public void pressMasterVolumeBack()
	{
		float num = Mathf.Abs((masterVolumeBack.transform.position.x - masterVolumeBack.rectTransform.sizeDelta.x / 2f - Inventory.inv.cursor.transform.position.x) / masterVolumeBack.rectTransform.sizeDelta.x);
		MonoBehaviour.print(num);
		masterVolumeSlider.fillAmount = num;
		SoundManager.manage.setMasterVolume(num * 4f);
		PlayerPrefs.SetFloat("masterVolume", num * 4f);
		PlayerPrefs.Save();
	}

	private void changeMasterVolumeAmounts(float dif)
	{
		float num = Mathf.Clamp(SoundManager.manage.getMasterVolume() + dif, 0f, 4f);
		masterVolumeSlider.fillAmount = num / 4f;
		SoundManager.manage.setMasterVolume(num);
		PlayerPrefs.SetFloat("masterVolume", num);
		PlayerPrefs.Save();
	}

	public void changeUIVolume(float dif)
	{
		changeUIVolumeAmounts(dif);
	}

	public void pressVolumeBack()
	{
		float num = Mathf.Abs((volumeBack.transform.position.x - volumeBack.rectTransform.sizeDelta.x / 2f - Inventory.inv.cursor.transform.position.x) / volumeBack.rectTransform.sizeDelta.x);
		volumeSlider.fillAmount = num;
		SoundManager.manage.setSoundEffectVolume(num * 4f);
		PlayerPrefs.SetFloat("soundVolume", num * 4f);
		PlayerPrefs.Save();
	}

	public void pressUIVolumeBack()
	{
		float num = Mathf.Abs((UIVolumeBack.transform.position.x - UIVolumeBack.rectTransform.sizeDelta.x / 2f - Inventory.inv.cursor.transform.position.x) / UIVolumeBack.rectTransform.sizeDelta.x);
		UIVolumeSlider.fillAmount = num;
		SoundManager.manage.setUiVolume(num * 4f);
		PlayerPrefs.SetFloat("uiVolume", num * 4f);
		PlayerPrefs.Save();
	}

	public void pressMusicVolumeBack()
	{
		float num = Mathf.Abs((musicVolumeBack.transform.position.x - musicVolumeBack.rectTransform.sizeDelta.x / 2f - Inventory.inv.cursor.transform.position.x) / musicVolumeBack.rectTransform.sizeDelta.x);
		musicVolumeSlider.fillAmount = num;
		MusicManager.manage.changeVolume(num * 4f);
		PlayerPrefs.SetFloat("musicVolume", num * 4f);
		PlayerPrefs.Save();
	}

	private void changeUIVolumeAmounts(float dif)
	{
		float num = Mathf.Clamp(SoundManager.manage.getUiVolumeForChange() + dif, 0f, 4f);
		UIVolumeSlider.fillAmount = num / 4f;
		SoundManager.manage.setUiVolume(num);
		PlayerPrefs.SetFloat("uiVolume", num);
		PlayerPrefs.Save();
	}

	public void changeMusicVolume(float dif)
	{
		changeMusicVolumeAmounts(dif);
		if (InputMaster.input.UISelectHeld())
		{
			StartCoroutine(holdVolumeButton(false, dif));
		}
	}

	private void changeMusicVolumeAmounts(float dif)
	{
		float num = Mathf.Clamp(MusicManager.manage.musicMasterVolume + dif, 0f, 4f);
		musicVolumeSlider.fillAmount = num / 4f;
		MusicManager.manage.changeVolume(num);
		PlayerPrefs.SetFloat("musicVolume", num);
		PlayerPrefs.Save();
	}

	private IEnumerator holdVolumeButton(bool isVolume, float change)
	{
		float changeTimer = 0f;
		while (InputMaster.input.UISelectHeld())
		{
			if (changeTimer < 0.1f)
			{
				changeTimer += Time.deltaTime;
			}
			else
			{
				changeTimer = 0f;
				if (isVolume)
				{
					SoundManager.manage.play2DSound(SoundManager.manage.buttonSound);
					changeSoundVolumeAmounts(change);
				}
				else
				{
					changeMusicVolumeAmounts(change);
				}
			}
			yield return null;
		}
	}

	private void selectChunkViewDistanceButton()
	{
		for (int i = 0; i < chunkViewDistanceButtons.Length; i++)
		{
			if (i == chunkViewDistance - 4)
			{
				chunkViewDistanceButtons[i].GetComponent<Image>().color = selectedColor;
			}
			else
			{
				chunkViewDistanceButtons[i].GetComponent<Image>().color = baseColor;
			}
		}
	}

	public void setUpVolumeSliders()
	{
		if (PlayerPrefs.HasKey("masterVolume"))
		{
			float @float = PlayerPrefs.GetFloat("masterVolume");
			SoundManager.manage.setMasterVolume(@float);
		}
		masterVolumeSlider.fillAmount = SoundManager.manage.getMasterVolume() / 4f;
		if (PlayerPrefs.HasKey("soundVolume"))
		{
			float float2 = PlayerPrefs.GetFloat("soundVolume");
			SoundManager.manage.setSoundEffectVolume(float2);
		}
		volumeSlider.fillAmount = SoundManager.manage.getSoundVolumeForChange() / 4f;
		if (PlayerPrefs.HasKey("musicVolume"))
		{
			float float3 = PlayerPrefs.GetFloat("musicVolume");
			MusicManager.manage.changeVolume(float3);
		}
		musicVolumeSlider.fillAmount = MusicManager.manage.musicMasterVolume / 4f;
		if (PlayerPrefs.HasKey("uiVolume"))
		{
			float float4 = PlayerPrefs.GetFloat("uiVolume");
			SoundManager.manage.setUiVolume(float4);
		}
		UIVolumeSlider.fillAmount = SoundManager.manage.getUiVolumeForChange() / 4f;
	}
}
