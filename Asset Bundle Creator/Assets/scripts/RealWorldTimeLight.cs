using System;
using System.Collections;
using System.Runtime.InteropServices;
using I2.Loc;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class RealWorldTimeLight : NetworkBehaviour
{
	public static RealWorldTimeLight time;

	public int seasonAverageTemp = 25;

	public Light theSun;

	public Light theMoon;

	public Light insideLight;

	private Vector3 desiredSunPos;

	public UnityEvent onDayNightChange;

	public UnityEvent onLightPlaced;

	public UnityEvent clockTickEvent;

	public UnityEvent taskChecker;

	[SyncVar(hook = "onTimeChange")]
	public int currentHour;

	[SyncVar(hook = "onUnderGround")]
	public bool underGround;

	public int currentMinute;

	public bool isNightTime;

	public TextMeshProUGUI DateText;

	public TextMeshProUGUI TimeText;

	public TextMeshProUGUI DayText;

	public TextMeshProUGUI SeasonText;

	public bool useTime = true;

	private float startingTime;

	private DateTime RealDate;

	private int startTime;

	private float desiredDegrees;

	private float SunRotationDegrees;

	private float vel;

	public int[] seasonAverageTemps;

	private bool inside;

	public Material skyCloudMaterials;

	public Material glassLightOff;

	public Material glassLightOn;

	public Color LightOnEmission;

	public Color LightOffColor;

	public Color lightOnColour;

	public Material outsideWindowMaterial;

	public Material insideWindowMaterial;

	public Material seeThroughGlassOutside;

	public Material doorBlackness;

	public Color doorBlacknessAtNight;

	private Coroutine clientSecondsCount;

	[Header("Night sky stuff")]
	public GameObject moonObject;

	public GameObject stars;

	public Material starMat;

	private Color starColor;

	[Header("Morning Colours -----")]
	public Color sunRiseSetColor;

	public Color fogRiseColor;

	public Color sunRiseSetGroundColor;

	[Header("Daytime Colours -----")]
	public Color defaultSunColor;

	public Color fogDayColor;

	public Color dayTimeGroundColor;

	[Header("Nighttime Colours -----")]
	public Color nightTimeColor;

	public Color fogNightColor;

	public Color nightTimeGroundColor;

	[Header("Different Skys -----")]
	public Color daySky;

	public Color rainSky;

	[Header("Cloud Colours -----")]
	public Material cloudMat;

	public Color defaultCloudColour;

	public Color cloudSunRiseColor;

	public Color cloudNightColor;

	[Header("Other Colours -----")]
	public Color rainingDif;

	public Color rainFogColour;

	[Header("Water Settings----")]
	public Material waterMat;

	private Coroutine sunAndLightRoutine;

	public GameObject mineRoof;

	private Coroutine clockRoutine;

	public GameObject clouds;

	private WaitForSeconds wait = new WaitForSeconds(2f);

	private float currentAtmosphere = 1f;

	[Header("Atmosphere--------")]
	public float dayAtmosphere = 0.65f;

	public float nightAtmosphere = 2f;

	public float nightIntensityLight = 0.5f;

	public float nightIntensityAmbient = 0.2f;

	public float nightIntensityReflection = 0.4f;

	public float dayTimeIntensity = 1f;

	private float currentSpeed = 2f;

	public int NetworkcurrentHour
	{
		get
		{
			return currentHour;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref currentHour))
			{
				int oldHour = currentHour;
				SetSyncVar(value, ref currentHour, 1uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(1uL))
				{
					setSyncVarHookGuard(1uL, true);
					onTimeChange(oldHour, value);
					setSyncVarHookGuard(1uL, false);
				}
			}
		}
	}

	public bool NetworkunderGround
	{
		get
		{
			return underGround;
		}
		[param: In]
		set
		{
			if (!SyncVarEqual(value, ref underGround))
			{
				bool old = underGround;
				SetSyncVar(value, ref underGround, 2uL);
				if (NetworkServer.localClientActive && !getSyncVarHookGuard(2uL))
				{
					setSyncVarHookGuard(2uL, true);
					onUnderGround(old, value);
					setSyncVarHookGuard(2uL, false);
				}
			}
		}
	}

	private void Awake()
	{
		time = this;
	}

	public override void OnStartServer()
	{
		if (WorldManager.manageWorld.day == 1 && WorldManager.manageWorld.week == 1 && WorldManager.manageWorld.month == 1 && WorldManager.manageWorld.year == 1)
		{
			NetworkcurrentHour = 10;
		}
		else
		{
			clockRoutine = StartCoroutine(runClock());
		}
	}

	public void startTimeOnFirstDay()
	{
		clockRoutine = StartCoroutine(runClock());
	}

	private void Start()
	{
		starColor = starMat.color;
		glassLightOn.EnableKeyword("_EMISSION");
		outsideWindowMaterial.EnableKeyword("_EMISSION");
		insideWindowMaterial.EnableKeyword("_EMISSION");
		seeThroughGlassOutside.EnableKeyword("_EMISSION");
		clockTick();
		if (sunAndLightRoutine == null)
		{
			sunAndLightRoutine = StartCoroutine(moveSunAndLighting());
		}
		onDayNightChange = new UnityEvent();
		onDayNightChange.Invoke();
		onLightPlaced = new UnityEvent();
		int num = currentHour * 3600 + currentMinute * 60 + 60;
		SunRotationDegrees = (float)num * 0.0041667f - 90f;
		desiredDegrees = SunRotationDegrees;
		setUpDayAndDate();
		StartCoroutine(fadeInWindows());
	}

	private IEnumerator runClock()
	{
		while (true)
		{
			clockTick();
			clockTickEvent.Invoke();
			yield return wait;
			if (currentHour != 0)
			{
				currentMinute++;
			}
			if (currentMinute >= 60)
			{
				currentMinute = 0;
				if (currentHour != 0)
				{
					NetworkcurrentHour = currentHour + 1;
				}
			}
			if (currentMinute == 0 || currentMinute == 15 || currentMinute == 30 || currentMinute == 45)
			{
				taskChecker.Invoke();
			}
		}
	}

	private IEnumerator clientRunClock()
	{
		currentMinute = 0;
		while (currentMinute < 60)
		{
			clockTick();
			clockTickEvent.Invoke();
			yield return wait;
			if (currentHour != 0)
			{
				currentMinute++;
			}
		}
	}

	public IEnumerator startNewDay()
	{
		NetworkcurrentHour = 7;
		StopCoroutine(clockRoutine);
		clockRoutine = null;
		while (!NetworkMapSharer.share.nextDayIsReady)
		{
			yield return null;
		}
		clockRoutine = StartCoroutine(runClock());
	}

	public IEnumerator startNewDayClient()
	{
		while (!NetworkMapSharer.share.nextDayIsReady)
		{
			yield return null;
		}
		StopCoroutine(clientSecondsCount);
		clientSecondsCount = null;
		currentMinute = 0;
		clientSecondsCount = StartCoroutine(clientRunClock());
	}

	public void goInside()
	{
		inside = true;
		if (sunAndLightRoutine != null)
		{
			StopCoroutine(sunAndLightRoutine);
			sunAndLightRoutine = null;
		}
		sunAndLightRoutine = StartCoroutine(moveSunAndLighting());
		theSun.gameObject.SetActive(false);
		theMoon.gameObject.SetActive(false);
		insideLight.enabled = true;
	}

	public void goOutside()
	{
		inside = false;
		if (underGround)
		{
			changeColor(Color.black, Color.black, 1f);
			cloudMat.color = Color.clear;
			setFogColor(Color.black);
		}
		else
		{
			theSun.gameObject.SetActive(true);
			theMoon.gameObject.SetActive(true);
		}
		if (sunAndLightRoutine != null)
		{
			StopCoroutine(sunAndLightRoutine);
			sunAndLightRoutine = null;
		}
		sunAndLightRoutine = StartCoroutine(moveSunAndLighting());
		insideLight.enabled = false;
	}

	public void nextDay()
	{
		WorldManager.manageWorld.day++;
		if (WorldManager.manageWorld.day > 7)
		{
			WorldManager.manageWorld.week++;
			WorldManager.manageWorld.day = 1;
		}
		if (WorldManager.manageWorld.week > 4)
		{
			WorldManager.manageWorld.week = 1;
			WorldManager.manageWorld.month++;
		}
		if (WorldManager.manageWorld.month > 4)
		{
			WorldManager.manageWorld.month = 1;
			WorldManager.manageWorld.year++;
		}
		setUpDayAndDate();
		RenderSettings.skybox.SetColor("_SkyTint", getSkyDayColor());
	}

	public void getDaySkyBox()
	{
		RenderSettings.skybox.SetColor("_SkyTint", getSkyDayColor());
	}

	public void setUpDayAndDate()
	{
		seasonAverageTemp = seasonAverageTemps[WorldManager.manageWorld.month - 1];
		DayText.text = getDayName(WorldManager.manageWorld.day - 1).Substring(0, 3).ToUpper();
		DateText.text = (WorldManager.manageWorld.day + (WorldManager.manageWorld.week - 1) * 7).ToString("00");
		SeasonText.text = getSeasonName(WorldManager.manageWorld.month - 1);
		if (SeasonText.text.Length >= 3)
		{
			SeasonText.text = SeasonText.text.Substring(0, 3).ToUpper();
		}
	}

	public void Update()
	{
		desiredDegrees = Mathf.SmoothDamp(desiredDegrees, SunRotationDegrees, ref vel, 1f);
		if (!underGround && currentHour != 0)
		{
			theSun.transform.eulerAngles = new Vector3(desiredDegrees, -135f, 0f);
		}
		else
		{
			theSun.transform.eulerAngles = new Vector3(270f, -135f, 0f);
		}
		RenderSettings.skybox.SetFloat("_AtmosphereThickness", currentAtmosphere);
	}

	private Color getSkyDayColor()
	{
		if (WeatherManager.manage.raining)
		{
			return rainSky;
		}
		return daySky;
	}

	private Color getDayCloudColor()
	{
		if (WeatherManager.manage.raining)
		{
			return Color.Lerp(Color.grey, Color.clear, 0.75f);
		}
		return Color.Lerp(Color.clear, Color.white, 0.15f);
	}

	public void setDegreesNewDay()
	{
		desiredDegrees = SunRotationDegrees;
	}

	private string returnWith0(int value)
	{
		if (value < 10)
		{
			return "0" + value;
		}
		return value.ToString() ?? "";
	}

	public void clockTick()
	{
		int num = currentHour * 3600 + currentMinute * 60 + 60;
		SunRotationDegrees = (float)num * 0.0041667f - 90f;
		SunRotationDegrees %= 360f;
		showTimeOnClock();
	}

	private void showTimeOnClock()
	{
		string text = "<size=10>PM</size>";
		int num = currentHour;
		if (currentHour < 12)
		{
			text = "<size=10>AM</size>";
		}
		else if (currentHour > 12)
		{
			num -= 12;
		}
		if (currentHour != 0)
		{
			TimeText.text = num.ToString("00") + ":" + currentMinute.ToString("00") + text;
		}
		else
		{
			TimeText.text = "Late";
		}
	}

	public void changeColor(Color changeFrom, Color changeTo, float amount)
	{
		if (underGround)
		{
			theSun.color = changeTo;
		}
		else if (WeatherManager.manage.raining)
		{
			theSun.color = Color.Lerp(Color.Lerp(changeFrom, changeTo, amount), rainingDif, 0.6f);
		}
		else
		{
			theSun.color = Color.Lerp(changeFrom, changeTo, amount);
		}
	}

	public void changeCloudColour(Color changeFrom, Color changeTo, float amount)
	{
		cloudMat.color = Color.Lerp(changeFrom, changeTo, amount);
	}

	public void changeSpeed(float newSpeed)
	{
		currentSpeed = newSpeed;
		wait = new WaitForSeconds(newSpeed);
	}

	public float getCurrentSpeed()
	{
		return currentSpeed;
	}

	private void onTimeChange(int oldHour, int newHour)
	{
		currentMinute = 0;
		if (!base.isServer)
		{
			if (clientSecondsCount != null)
			{
				StopCoroutine(clientSecondsCount);
			}
			clientSecondsCount = StartCoroutine(clientRunClock());
		}
		if (newHour == 24)
		{
			NetworkcurrentHour = 0;
		}
		else
		{
			NetworkcurrentHour = newHour;
		}
		StartCoroutine(fadeInWindows());
		if (sunAndLightRoutine != null)
		{
			StopCoroutine(sunAndLightRoutine);
			sunAndLightRoutine = null;
		}
		sunAndLightRoutine = StartCoroutine(moveSunAndLighting());
	}

	private IEnumerator fadeInWindows()
	{
		if (currentHour >= 17 || currentHour == 0)
		{
			while (currentHour == 17)
			{
				yield return null;
				glassLightOn.color = Color.Lerp(LightOffColor, Color.white, (float)currentMinute / 60f);
				glassLightOn.SetColor("_EmissionColor", Color.Lerp(Color.black, LightOnEmission, (float)currentMinute / 60f));
				glassLightOff.color = Color.Lerp(Color.white, LightOffColor, (float)currentMinute / 60f);
				glassLightOff.SetColor("_EmissionColor", Color.Lerp(LightOnEmission, Color.black, (float)currentMinute / 60f));
				outsideWindowMaterial.color = Color.Lerp(LightOffColor, lightOnColour, (float)currentMinute / 60f);
				outsideWindowMaterial.SetColor("_EmissionColor", Color.Lerp(Color.black, LightOnEmission, (float)currentMinute / 60f));
				seeThroughGlassOutside.SetColor("_EmissionColor", Color.Lerp(Color.black, Color.Lerp(LightOnEmission, Color.black, 0.3f), (float)currentMinute / 60f));
				insideWindowMaterial.color = Color.Lerp(lightOnColour, Color.Lerp(LightOffColor, Color.black, 0.5f), (float)currentMinute / 60f);
				insideWindowMaterial.SetColor("_EmissionColor", Color.Lerp(LightOnEmission, Color.black, (float)currentMinute / 60f));
				doorBlackness.SetColor("_Color", Color.Lerp(Color.black, doorBlacknessAtNight, (float)currentMinute / 60f));
			}
			glassLightOff.color = LightOffColor;
			glassLightOff.SetColor("_EmissionColor", Color.black);
			glassLightOn.color = Color.white;
			glassLightOn.SetColor("_EmissionColor", LightOnEmission);
			doorBlackness.SetColor("_Color", doorBlacknessAtNight);
			insideWindowMaterial.color = Color.Lerp(LightOffColor, Color.black, 0.5f);
			insideWindowMaterial.SetColor("_EmissionColor", Color.black);
			outsideWindowMaterial.color = lightOnColour;
			outsideWindowMaterial.SetColor("_EmissionColor", LightOnEmission);
			seeThroughGlassOutside.SetColor("_EmissionColor", Color.Lerp(LightOnEmission, Color.black, 0.3f));
		}
		else
		{
			outsideWindowMaterial.color = LightOffColor;
			outsideWindowMaterial.SetColor("_EmissionColor", Color.black);
			seeThroughGlassOutside.SetColor("_EmissionColor", Color.black);
			doorBlackness.SetColor("_Color", Color.black);
			insideWindowMaterial.color = lightOnColour;
			insideWindowMaterial.SetColor("_EmissionColor", LightOnEmission);
		}
	}

	private void onRain(bool rain)
	{
		WeatherManager.manage.isRaining(rain);
	}

	private void onUnderGround(bool old, bool newUnderground)
	{
		NetworkunderGround = newUnderground;
		if (underGround)
		{
			mineRoof.SetActive(true);
			WeatherManager.manage.isRaining(WeatherManager.manage.raining);
			setFogColor(Color.black);
			clouds.SetActive(false);
			if (CameraController.control.landAmbientAudio.clip != CameraController.control.undergroundAmbience)
			{
				CameraController.control.landAmbientAudio.clip = CameraController.control.undergroundAmbience;
				CameraController.control.landAmbientAudio.Play();
			}
			CameraController.control.landAmbienceMax = 0.25f;
		}
		else
		{
			if (WeatherManager.manage.raining)
			{
				clouds.SetActive(true);
			}
			mineRoof.SetActive(false);
		}
		onDayNightChange.Invoke();
		CameraController.control.updateDepthOfFieldAndFog(NewChunkLoader.loader.getChunkDistance());
		CameraController.control.undergroundSoundZone.SetActive(newUnderground);
	}

	private IEnumerator moveSunAndLighting()
	{
		if (inside)
		{
			RenderSettings.ambientIntensity = dayTimeIntensity;
			RenderSettings.reflectionIntensity = 1f;
		}
		else if (underGround)
		{
			RenderSettings.ambientIntensity = 0.15f;
			RenderSettings.reflectionIntensity = 0.25f;
			setFogColor(Color.black);
		}
		else
		{
			bool flag = isNightTime;
			if (currentHour >= 17 || currentHour <= 6)
			{
				isNightTime = true;
			}
			else
			{
				isNightTime = false;
			}
			if (isNightTime != flag)
			{
				onDayNightChange.Invoke();
			}
			if (currentHour == 17)
			{
				theMoon.enabled = true;
				theSun.enabled = true;
				stars.SetActive(true);
				moonObject.SetActive(true);
				CameraController.control.landAmbienceMax = 0f;
				while (currentHour == 17)
				{
					yield return null;
					float num = (float)currentMinute / 60f;
					RenderSettings.ambientIntensity = Mathf.Lerp(dayTimeIntensity, nightIntensityAmbient, num);
					RenderSettings.reflectionIntensity = Mathf.Lerp(1f, nightIntensityReflection, num);
					currentAtmosphere = Mathf.Lerp(currentAtmosphere, Mathf.Lerp(dayAtmosphere, nightAtmosphere, num), Time.deltaTime);
					theSun.intensity = Mathf.Lerp(theSun.intensity, Mathf.Lerp(1f, 0f, num), Time.deltaTime);
					theMoon.intensity = Mathf.Lerp(theMoon.intensity, Mathf.Lerp(0f, 0.25f, num), Time.deltaTime);
					changeColor(sunRiseSetColor, nightTimeColor, num);
					changeCloudColour(cloudSunRiseColor, cloudNightColor, num);
					setFogColor(Color.Lerp(getMorningFogColour(), fogNightColor, num));
					starColor.a = Mathf.Lerp(0f, 1f, num);
					starMat.color = starColor;
				}
			}
			else if (currentHour == 6)
			{
				theMoon.enabled = true;
				theSun.enabled = true;
				CameraController.control.landAmbienceMax = 0f;
				stars.SetActive(true);
				moonObject.SetActive(true);
				while (currentHour == 6)
				{
					yield return null;
					float num2 = (float)currentMinute / 60f;
					RenderSettings.ambientIntensity = Mathf.Lerp(nightIntensityAmbient, dayTimeIntensity, num2);
					RenderSettings.reflectionIntensity = Mathf.Lerp(nightIntensityReflection, 1f, num2);
					currentAtmosphere = Mathf.Lerp(currentAtmosphere, Mathf.Lerp(nightAtmosphere, dayAtmosphere, num2), Time.deltaTime);
					theSun.intensity = Mathf.Lerp(theSun.intensity, Mathf.Lerp(0f, 1f, num2), Time.deltaTime);
					theMoon.intensity = Mathf.Lerp(theMoon.intensity, Mathf.Lerp(0.25f, 0f, num2), Time.deltaTime);
					changeColor(nightTimeColor, sunRiseSetColor, num2);
					changeCloudColour(cloudNightColor, cloudSunRiseColor, num2);
					setFogColor(Color.Lerp(fogNightColor, getMorningFogColour(), num2));
					starColor.a = Mathf.Lerp(1f, 0f, num2);
					starMat.color = starColor;
				}
			}
			else if (currentHour < 6 || currentHour > 17)
			{
				stars.SetActive(true);
				moonObject.SetActive(true);
				starColor.a = 1f;
				starMat.color = starColor;
				theMoon.enabled = true;
				theSun.enabled = false;
				theSun.intensity = 0f;
				theMoon.intensity = 0.25f;
				changeColor(nightTimeColor, nightTimeColor, 1f);
				changeCloudColour(cloudNightColor, cloudNightColor, 1f);
				currentAtmosphere = nightAtmosphere;
				RenderSettings.ambientIntensity = nightIntensityAmbient;
				RenderSettings.reflectionIntensity = nightIntensityReflection;
				setFogColor(fogNightColor);
				if (CameraController.control.landAmbientAudio.clip != CameraController.control.nightTimeAmbience)
				{
					CameraController.control.landAmbientAudio.clip = CameraController.control.nightTimeAmbience;
					CameraController.control.landAmbientAudio.Play();
				}
				CameraController.control.landAmbienceMax = 0.25f;
				while (currentHour == 18)
				{
					yield return null;
					CameraController.control.landAmbienceMax = Mathf.Lerp(0f, 0.25f, (float)currentMinute / 60f);
				}
				while (currentHour == 5)
				{
					yield return null;
					CameraController.control.landAmbienceMax = Mathf.Lerp(0.25f, 0f, (float)currentMinute / 60f);
				}
			}
			else
			{
				stars.SetActive(false);
				moonObject.SetActive(false);
				theMoon.enabled = false;
				theSun.enabled = true;
				theSun.intensity = 1f;
				theMoon.intensity = 0f;
				currentAtmosphere = dayAtmosphere;
				RenderSettings.ambientIntensity = dayTimeIntensity;
				RenderSettings.reflectionIntensity = 1f;
				if (CameraController.control.landAmbientAudio.clip != CameraController.control.dayTimeAmbience)
				{
					CameraController.control.landAmbientAudio.clip = CameraController.control.dayTimeAmbience;
					CameraController.control.landAmbientAudio.Play();
				}
				CameraController.control.landAmbienceMax = 0.25f;
				if (currentHour == 15)
				{
					setFogColor(getDayTimeFogColour());
					while (currentHour == 15)
					{
						yield return null;
						changeColor(defaultSunColor, sunRiseSetColor, (float)currentMinute / 60f);
						changeCloudColour(defaultCloudColour, cloudSunRiseColor, (float)currentMinute / 60f);
					}
				}
				else if (currentHour == 16 || currentHour == 7)
				{
					changeColor(sunRiseSetColor, sunRiseSetColor, 1f);
					changeCloudColour(cloudSunRiseColor, cloudSunRiseColor, 1f);
					while (currentHour == 7)
					{
						yield return null;
						changeColor(sunRiseSetColor, sunRiseSetColor, 1f);
						changeCloudColour(cloudSunRiseColor, cloudSunRiseColor, 1f);
						setFogColor(Color.Lerp(getMorningFogColour(), getDayTimeFogColour(), (float)currentMinute / 60f));
						CameraController.control.landAmbienceMax = Mathf.Lerp(0f, 0.25f, (float)currentMinute / 60f);
					}
					while (currentHour == 16)
					{
						yield return null;
						setFogColor(Color.Lerp(getDayTimeFogColour(), getMorningFogColour(), (float)currentMinute / 60f));
					}
				}
				else if (currentHour == 8)
				{
					setFogColor(getDayTimeFogColour());
					while (currentHour == 8)
					{
						yield return null;
						changeColor(sunRiseSetColor, defaultSunColor, (float)currentMinute / 60f);
						changeCloudColour(cloudSunRiseColor, defaultCloudColour, (float)currentMinute / 60f);
					}
				}
				else
				{
					changeColor(defaultSunColor, defaultSunColor, 1f);
					changeCloudColour(defaultCloudColour, defaultCloudColour, 1f);
					setFogColor(getDayTimeFogColour());
				}
			}
		}
		sunAndLightRoutine = null;
	}

	public string getDayName(int dayId)
	{
		return (LocalizedString)("Time/dayName_" + dayId);
	}

	public string getSeasonName(int dayId)
	{
		return (LocalizedString)("Time/seasonName_" + dayId);
	}

	public void setFogColor(Color set)
	{
		RenderSettings.fogColor = set;
		RenderSettings.skybox.SetColor("_GroundColor", RenderSettings.fogColor);
		waterMat.SetColor("_FogColor", set);
	}

	public Color getDayTimeFogColour()
	{
		if (WeatherManager.manage.raining)
		{
			return rainFogColour;
		}
		return fogDayColor;
	}

	public Color getMorningFogColour()
	{
		if (WeatherManager.manage.raining)
		{
			return rainFogColour;
		}
		return fogRiseColor;
	}

	private void MirrorProcessed()
	{
	}

	protected override bool SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		bool result = base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteInt(currentHour);
			writer.WriteBool(underGround);
			return true;
		}
		writer.WriteULong(base.syncVarDirtyBits);
		if ((base.syncVarDirtyBits & 1L) != 0L)
		{
			writer.WriteInt(currentHour);
			result = true;
		}
		if ((base.syncVarDirtyBits & 2L) != 0L)
		{
			writer.WriteBool(underGround);
			result = true;
		}
		return result;
	}

	protected override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			int num = currentHour;
			NetworkcurrentHour = reader.ReadInt();
			if (!SyncVarEqual(num, ref currentHour))
			{
				onTimeChange(num, currentHour);
			}
			bool flag = underGround;
			NetworkunderGround = reader.ReadBool();
			if (!SyncVarEqual(flag, ref underGround))
			{
				onUnderGround(flag, underGround);
			}
			return;
		}
		long num2 = (long)reader.ReadULong();
		if ((num2 & 1L) != 0L)
		{
			int num3 = currentHour;
			NetworkcurrentHour = reader.ReadInt();
			if (!SyncVarEqual(num3, ref currentHour))
			{
				onTimeChange(num3, currentHour);
			}
		}
		if ((num2 & 2L) != 0L)
		{
			bool flag2 = underGround;
			NetworkunderGround = reader.ReadBool();
			if (!SyncVarEqual(flag2, ref underGround))
			{
				onUnderGround(flag2, underGround);
			}
		}
	}
}
