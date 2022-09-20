using System.Collections;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class WeatherManager : MonoBehaviour
{
	public static WeatherManager manage;

	public bool raining;

	private bool inside;

	public ParticleSystem rainParts;

	public ParticleSystem otherRainParts;

	public GlobalFog fogScript;

	public AudioSource rainSound;

	public AudioSource windSound;

	public AudioSource otherWeatherSounds;

	public Material[] windyMaterials;

	public ParticleSystem windParticles;

	public AudioClip[] windSoundClips;

	public AudioClip[] thunderSounds;

	public bool windBlowing;

	public bool windy;

	public bool storming;

	public bool foggy;

	public Light weatherLight;

	private ParticleSystem.VelocityOverLifetimeModule windPart;

	private ParticleSystem.VelocityOverLifetimeModule rainWind;

	private ParticleSystem.VelocityOverLifetimeModule rainWind2;

	private ParticleSystem.ShapeModule rainShape;

	private ParticleSystem.ShapeModule rain2Shape;

	public ParticleSystem lightingPart;

	public AudioClip[] lightningCrack;

	public Vector3 windDir;

	public ItemHitBox lightningDamageBox;

	private float desiredRainVolume = 0.6f;

	private Coroutine windRoutine;

	private WaitForSeconds thunderWait = new WaitForSeconds(3.5f);

	public float currentWindSpeed;

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
		setMaterialsToDefault();
		windPart = windParticles.velocityOverLifetime;
		rainWind = rainParts.velocityOverLifetime;
		rainWind2 = otherRainParts.velocityOverLifetime;
		rainShape = rainParts.shape;
		rain2Shape = otherRainParts.shape;
		SoundManager.manage.onMasterChange.AddListener(onChangeVolume);
	}

	public void startFog()
	{
		foggy = true;
	}

	public void stopFog()
	{
		foggy = false;
	}

	public void startWind()
	{
		windy = true;
		if (windRoutine != null)
		{
			StopCoroutine(windRoutine);
			windRoutine = null;
		}
		windRoutine = StartCoroutine(handleWind());
	}

	public void stopWind()
	{
		setMaterialsToDefault();
		windSound.Stop();
		setWindOnAllPart(0f, 0f);
		windParticles.gameObject.SetActive(false);
		currentWindSpeed = 0f;
		if (windRoutine != null)
		{
			StopCoroutine(windRoutine);
			windRoutine = null;
		}
		windy = false;
	}

	public void startStorm()
	{
		startWind();
		rainShape.scale = new Vector3(55f, 55f, 0f);
		rain2Shape.scale = new Vector3(45f, 45f, 0f);
		isRaining(true);
		if (!storming)
		{
			if (NetworkMapSharer.share.isServer)
			{
				StartCoroutine("handleStorm");
			}
			storming = true;
		}
	}

	public void stopStorm()
	{
		rainShape.scale = new Vector3(80f, 80f, 0f);
		rain2Shape.scale = new Vector3(75f, 75f, 0f);
		stopWind();
		otherWeatherSounds.Stop();
		if (NetworkMapSharer.share.isServer)
		{
			StopCoroutine("handleStorm");
		}
		storming = false;
	}

	public void isRaining(bool isItRaining)
	{
		if (foggy || storming)
		{
			RealWorldTimeLight.time.theSun.intensity = 0.5f;
		}
		else
		{
			RealWorldTimeLight.time.theSun.intensity = 1f;
		}
		raining = isItRaining;
		CameraController.control.updateDepthOfFieldAndFog(NewChunkLoader.loader.getChunkDistance());
		RealWorldTimeLight.time.clouds.SetActive(!isItRaining && !RealWorldTimeLight.time.underGround);
		if (isItRaining && !RealWorldTimeLight.time.underGround)
		{
			if (inside)
			{
				changeRainForInside();
			}
			else
			{
				changeRainForOutSide();
			}
		}
		else
		{
			rainParts.gameObject.SetActive(false);
			otherRainParts.gameObject.SetActive(false);
			rainSound.enabled = false;
		}
		if (!isItRaining && foggy)
		{
			if (inside)
			{
				PhotoManager.manage.photoFog.enabled = false;
			}
			else
			{
				PhotoManager.manage.photoFog.enabled = true;
			}
		}
		else
		{
			bool foggy2 = foggy;
		}
	}

	public void changeRainForInside()
	{
		rainSound.enabled = true;
		rainSound.pitch = 0.5f;
		desiredRainVolume = 0.25f;
		rainSound.volume = desiredRainVolume * SoundManager.manage.getSoundVolume();
		rainParts.gameObject.SetActive(false);
		otherRainParts.gameObject.SetActive(false);
		PhotoManager.manage.photoFog.enabled = false;
	}

	public void changeRainForOutSide()
	{
		rainSound.enabled = true;
		rainSound.pitch = 1f;
		desiredRainVolume = 0.6f;
		rainSound.volume = desiredRainVolume * SoundManager.manage.getSoundVolume();
		rainParts.gameObject.SetActive(true);
		otherRainParts.gameObject.SetActive(true);
		PhotoManager.manage.photoFog.enabled = true;
	}

	private void onChangeVolume()
	{
		rainSound.volume = desiredRainVolume * SoundManager.manage.getSoundVolume();
	}

	public bool isInside()
	{
		return inside;
	}

	public void goInside(bool isShop, bool noMusic = false)
	{
		inside = true;
		isRaining(raining);
		if (windy)
		{
			windParticles.gameObject.SetActive(false);
		}
		if (storming)
		{
			otherWeatherSounds.volume = Random.Range(0.15f, 0.25f) * SoundManager.manage.getSoundVolume();
		}
		fogScript.enabled = false;
		MusicManager.manage.changeInside(true, isShop, noMusic);
		RenderMap.map.changeMapWindow();
	}

	public void goOutside()
	{
		inside = false;
		isRaining(raining);
		if (windy && !RealWorldTimeLight.time.underGround)
		{
			windParticles.gameObject.SetActive(true);
			setMaterialsToWindy();
		}
		if (storming)
		{
			otherWeatherSounds.volume = Random.Range(0.35f, 0.5f) * SoundManager.manage.getSoundVolume();
		}
		fogScript.enabled = true;
		MusicManager.manage.changeInside(false, false);
		RenderMap.map.changeMapWindow();
	}

	public void strikeThunder(Vector2 lightningPos)
	{
		StartCoroutine(lightningStrike(lightningPos));
	}

	public void playJustThunderSound()
	{
		StartCoroutine(lightningStrike(new Vector2(-500f, -500f)));
	}

	private IEnumerator handleStorm()
	{
		storming = true;
		int maxLightningStrikesToday = Random.Range(1, 4);
		int strikesThatHappened = 0;
		while (storming)
		{
			if (Random.Range(0, 7) == 3 && !otherWeatherSounds.isPlaying)
			{
				if (strikesThatHappened < maxLightningStrikesToday && RealWorldTimeLight.time.currentHour != 0 && Random.Range(0, 130) == 1)
				{
					Vector3 position = NetworkNavMesh.nav.charsConnected[Random.Range(0, NetworkNavMesh.nav.charsConnected.Count)].position;
					NetworkMapSharer.share.RpcThunderStrike(new Vector2(position.x + (float)Random.Range(-15, 15), position.z + (float)Random.Range(-15, 15)));
					strikesThatHappened++;
				}
				else
				{
					NetworkMapSharer.share.RpcThunderSound();
				}
			}
			yield return thunderWait;
		}
	}

	private IEnumerator lightningStrike(Vector2 lightningXY)
	{
		if (!RealWorldTimeLight.time.underGround)
		{
			weatherLight.intensity = 0f;
			weatherLight.enabled = true;
		}
		Vector3 vector = new Vector3(lightningXY.x, 35f, lightningXY.y);
		if ((int)lightningXY.x / 2 >= 0 && (int)lightningXY.x < WorldManager.manageWorld.getMapSize() && (int)lightningXY.y / 2 >= 0 && (int)lightningXY.x / 2 < WorldManager.manageWorld.getMapSize())
		{
			vector = new Vector3(lightningXY.x, WorldManager.manageWorld.heightMap[(int)lightningXY.x / 2, (int)lightningXY.y / 2], lightningXY.y);
		}
		if (Vector3.Distance(vector, CameraController.control.transform.position) < 50f)
		{
			otherWeatherSounds.pitch = Random.Range(0.85f, 1.1f);
			otherWeatherSounds.volume = Random.Range(0.45f, 0.5f) * SoundManager.manage.getSoundVolume();
			if (inside || RealWorldTimeLight.time.underGround)
			{
				otherWeatherSounds.volume = Random.Range(0.15f, 0.25f) * SoundManager.manage.getSoundVolume();
			}
			otherWeatherSounds.PlayOneShot(lightningCrack[Random.Range(0, lightningCrack.Length)]);
			lightingPart.transform.position = vector + Vector3.up * 15f;
			lightingPart.Emit(1);
			yield return null;
			lightingPart.Emit(1);
		}
		else
		{
			otherWeatherSounds.pitch = Random.Range(0.85f, 1.1f);
			otherWeatherSounds.volume = Random.Range(0.45f, 0.5f) * SoundManager.manage.getSoundVolume();
			if (inside || RealWorldTimeLight.time.underGround)
			{
				otherWeatherSounds.volume = Random.Range(0.15f, 0.25f) * SoundManager.manage.getSoundVolume();
			}
			otherWeatherSounds.PlayOneShot(thunderSounds[Random.Range(0, thunderSounds.Length)]);
		}
		yield return new WaitForSeconds(0.15f);
		if (RealWorldTimeLight.time.underGround)
		{
			yield break;
		}
		while (weatherLight.intensity <= 1f)
		{
			weatherLight.intensity += Time.deltaTime * 16f;
			yield return null;
		}
		while (weatherLight.intensity >= 0.5f)
		{
			weatherLight.intensity -= Time.deltaTime * 16f;
			yield return null;
		}
		while (weatherLight.intensity <= 1f)
		{
			weatherLight.intensity += Time.deltaTime * 16f;
			yield return null;
		}
		while (weatherLight.intensity > 0.05f)
		{
			weatherLight.intensity -= Time.deltaTime * 16f;
			yield return null;
		}
		weatherLight.intensity = 0f;
		weatherLight.enabled = false;
		int xPos = (int)lightningXY.x / 2;
		int yPos = (int)lightningXY.y / 2;
		if (WorldManager.manageWorld.isPositionOnMap(xPos, yPos))
		{
			lightningDamageBox.transform.position = new Vector3((float)xPos * 2f, WorldManager.manageWorld.heightMap[xPos, yPos], (float)yPos * 2f);
		}
		else
		{
			lightningDamageBox.transform.position = new Vector3((float)xPos * 2f, 0.6f, (float)yPos * 2f);
		}
		lightningDamageBox.startAttack();
		yield return new WaitForSeconds(0.15f);
		yield return new WaitForSeconds(0.15f);
		lightningDamageBox.endAttack();
		if (xPos >= 0 && xPos < WorldManager.manageWorld.getMapSize() && yPos >= 0 && yPos < WorldManager.manageWorld.getMapSize() && (WorldManager.manageWorld.onTileMap[xPos, yPos] == -1 || WorldManager.manageWorld.onTileMap[xPos, yPos] == 0 || WorldManager.manageWorld.onTileMap[xPos, yPos] == 6 || WorldManager.manageWorld.onTileMap[xPos, yPos] == 38 || WorldManager.manageWorld.onTileMap[xPos, yPos] == 136 || (WorldManager.manageWorld.onTileMap[xPos, yPos] >= 0 && WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].isGrass)))
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], new Vector3((float)xPos * 2f, WorldManager.manageWorld.heightMap[xPos, yPos], (float)yPos * 2f));
			if (NetworkMapSharer.share.isServer)
			{
				NetworkMapSharer.share.spawnACarryable(SaveLoad.saveOrLoad.carryablePrefabs[8], new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2));
			}
		}
	}

	private IEnumerator handleWind()
	{
		windSound.enabled = true;
		setMaterialsToWindy();
		currentWindSpeed = 0f;
		if (!inside && !RealWorldTimeLight.time.underGround)
		{
			windParticles.gameObject.SetActive(true);
		}
		while (windy)
		{
			yield return null;
			if (inside || RealWorldTimeLight.time.underGround)
			{
				windSound.Stop();
				setMaterialsToDefault();
				while (inside || RealWorldTimeLight.time.underGround)
				{
					yield return null;
				}
				setMaterialsToWindy();
			}
			else if (windBlowing)
			{
				currentWindSpeed = Mathf.Lerp(currentWindSpeed, 1f, Time.deltaTime / 5f);
				setWindOnAllPart(8f * currentWindSpeed + 0.25f, 19f * currentWindSpeed + 0.25f);
				setWindSpeedToMaterials(currentWindSpeed);
				yield return null;
				windSound.volume = currentWindSpeed * SoundManager.manage.getSoundVolume();
				if (!windSound.isPlaying)
				{
					windSound.pitch = Random.Range(0.95f, 1.25f) * SoundManager.manage.masterPitch;
					windSound.PlayOneShot(windSoundClips[Random.Range(0, windSoundClips.Length)]);
				}
				if (currentWindSpeed >= 0.95f)
				{
					yield return new WaitForSeconds(Random.Range(6f, 7f));
					windBlowing = false;
				}
			}
			else
			{
				currentWindSpeed = Mathf.Lerp(currentWindSpeed, 0f, Time.deltaTime / 5f);
				setWindOnAllPart(8f * currentWindSpeed + 0.1f, 19f * currentWindSpeed + 0.1f);
				setWindSpeedToMaterials(currentWindSpeed);
				windSound.volume = currentWindSpeed * 0.75f * SoundManager.manage.getSoundVolume();
				if (currentWindSpeed <= 0.1f)
				{
					windBlowing = true;
				}
			}
		}
		windRoutine = null;
	}

	private void setMaterialsToWindy()
	{
		for (int i = 0; i < windyMaterials.Length; i++)
		{
			if ((bool)windyMaterials[i])
			{
				windyMaterials[i].SetFloat("_ShakeTime", 1f);
				windyMaterials[i].SetFloat("_ShakeBending", 0.5f);
				windyMaterials[i].SetFloat("_WeatherWind", 0.25f);
			}
		}
	}

	private void setMaterialsToDefault()
	{
		for (int i = 0; i < windyMaterials.Length; i++)
		{
			if ((bool)windyMaterials[i])
			{
				windyMaterials[i].SetFloat("_ShakeTime", 0.5f);
				windyMaterials[i].SetFloat("_ShakeBending", 0.2f);
				windyMaterials[i].SetFloat("_WeatherWind", 1f);
			}
		}
	}

	private void setWindSpeedToMaterials(float currentWindSpeed)
	{
		for (int i = 0; i < windyMaterials.Length; i++)
		{
			if ((bool)windyMaterials[i])
			{
				windyMaterials[i].SetFloat("_WeatherWind", currentWindSpeed * 1.25f);
			}
		}
	}

	private void setWindOnAllPart(float min, float max)
	{
		windPart.x = new ParticleSystem.MinMaxCurve(min * windDir.x, max * windDir.x);
		rainWind.x = new ParticleSystem.MinMaxCurve(min * windDir.x, max * windDir.x);
		rainWind2.x = new ParticleSystem.MinMaxCurve(min * windDir.x, max * windDir.x);
		windPart.z = new ParticleSystem.MinMaxCurve(min * windDir.z, max * windDir.z);
		rainWind.z = new ParticleSystem.MinMaxCurve(min * windDir.z, max * windDir.z);
		rainWind2.z = new ParticleSystem.MinMaxCurve(min * windDir.z, max * windDir.z);
		windPart.y = new ParticleSystem.MinMaxCurve(0f, Mathf.Clamp(max, 0f, 5f));
	}

	public string currentWeather()
	{
		string text = "";
		int seasonAverageTemp = RealWorldTimeLight.time.seasonAverageTemp;
		float num = GenerateMap.generate.getDistanceToCentre((int)CameraController.control.transform.position.x / 2, (int)CameraController.control.transform.position.z / 2, 500f, 825f) * -18f + 18f;
		float num2 = GenerateMap.generate.getDistanceToCentre((int)CameraController.control.transform.position.x / 2, (int)CameraController.control.transform.position.z / 2, 200f, 200f) * 18f + -18f;
		text = text + "It is currently " + (int)((float)seasonAverageTemp + num + num2) + "° and ";
		text = (storming ? (text + UIAnimationManager.manage.wrapStringInYesColor("Storming") + ". With a") : (raining ? (text + UIAnimationManager.manage.wrapStringInYesColor("Raining") + ". With a") : ((!foggy) ? (text + UIAnimationManager.manage.wrapStringInYesColor("Fine") + ". With a") : (text + UIAnimationManager.manage.wrapStringInYesColor("Foggy") + ". With a"))));
		text = ((!windy) ? (text + " Light") : (text + " Strong"));
		if (windDir.z < -0.1f)
		{
			text += " Northern ";
		}
		else if (windDir.z > 0.1f)
		{
			text += " Southern ";
		}
		if (windDir.x < -0.1f)
		{
			text += " Westernly ";
		}
		else if (windDir.x > 0.1f)
		{
			text += " Easternly ";
		}
		return text + " Wind.";
	}

	public string tomorrowsWeather(int seedToCheck)
	{
		Random.InitState(seedToCheck);
		float num = 0f;
		float num2 = 0f;
		while (num == 0f && num2 == 0f)
		{
			num = Random.Range(-1, 1);
			num2 = Random.Range(-1, 1);
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		int num3 = WorldManager.manageWorld.day + 1;
		int num4 = WorldManager.manageWorld.week;
		int num5 = WorldManager.manageWorld.month;
		if (num3 > 7)
		{
			num4++;
			num3 = 1;
		}
		if (num4 > 4)
		{
			num4 = 1;
			num5++;
		}
		if (num5 > 4)
		{
			num5 = 1;
		}
		switch (num5)
		{
		case 1:
			if (num3 < 15)
			{
				if (Random.Range(0, 5) == 1)
				{
					flag = true;
					if (Random.Range(0, 3) == 1)
					{
						flag2 = true;
					}
				}
				else
				{
					flag = false;
				}
			}
			else if (Random.Range(0, 2) == 1)
			{
				flag = true;
				if (Random.Range(0, 2) == 1)
				{
					flag2 = true;
				}
			}
			else
			{
				flag = false;
			}
			break;
		case 2:
			if (Random.Range(0, 6) == 1)
			{
				flag = true;
				if (Random.Range(0, 4) == 1)
				{
					flag2 = true;
				}
			}
			if (Random.Range(0, 2) == 1)
			{
				flag3 = true;
			}
			break;
		case 3:
			if (Random.Range(0, 2) == 1)
			{
				flag4 = true;
			}
			if (Random.Range(0, 10) == 1)
			{
				flag = true;
			}
			if (Random.Range(0, 4) == 1)
			{
				flag3 = true;
			}
			break;
		default:
			if (Random.Range(0, 8) == 1)
			{
				flag = true;
			}
			if (Random.Range(0, 4) == 1)
			{
				flag3 = true;
			}
			break;
		}
		string text = "Tomorrow expect ";
		text = (flag2 ? (text + UIAnimationManager.manage.wrapStringInYesColor("Storms") + ". With") : (flag ? (text + UIAnimationManager.manage.wrapStringInYesColor("Rain") + ". With") : ((!flag4) ? (text + UIAnimationManager.manage.wrapStringInYesColor("Fine Weather") + ". With") : (text + UIAnimationManager.manage.wrapStringInYesColor("Fog") + ". With"))));
		text = ((!flag3) ? (text + " Light") : (text + " Strong"));
		if (num2 < -0.1f)
		{
			text += " Northern ";
		}
		else if (num2 > 0.1f)
		{
			text += " Southern ";
		}
		if (num < -0.1f)
		{
			text += " Westernly ";
		}
		else if (num > 0.1f)
		{
			text += " Easternly ";
		}
		int seasonAverageTemp = RealWorldTimeLight.time.seasonAverageTemp;
		float num6 = GenerateMap.generate.getDistanceToCentre((int)CameraController.control.transform.position.x / 2, (int)CameraController.control.transform.position.z / 2, 500f, 825f) * -18f + 18f;
		float num7 = GenerateMap.generate.getDistanceToCentre((int)CameraController.control.transform.position.x / 2, (int)CameraController.control.transform.position.z / 2, 200f, 200f) * 18f + -18f;
		return text + "Wind. With temperatures around " + (int)((float)seasonAverageTemp + num6 + num7) + "°.";
	}

	public void newDaWeatherCheck(int dayMineSeed)
	{
		Random.InitState(dayMineSeed);
		float num = 0f;
		float num2 = 0f;
		while (num == 0f && num2 == 0f)
		{
			num = Random.Range(-1, 1);
			num2 = Random.Range(-1, 1);
		}
		windDir = new Vector3(num, 0f, num2);
		stopWind();
		stopFog();
		stopStorm();
		raining = false;
		if (WorldManager.manageWorld.month == 1)
		{
			if (WorldManager.manageWorld.day < 15)
			{
				if (Random.Range(0, 5) == 1)
				{
					raining = true;
					if (Random.Range(0, 3) == 1)
					{
						startStorm();
					}
				}
				else
				{
					raining = false;
				}
			}
			else if (Random.Range(0, 2) == 1)
			{
				raining = true;
				if (Random.Range(0, 2) == 1)
				{
					startStorm();
				}
			}
			else
			{
				raining = false;
			}
		}
		else if (WorldManager.manageWorld.month == 2)
		{
			if (Random.Range(0, 6) == 1)
			{
				raining = true;
				if (Random.Range(0, 4) == 1)
				{
					startStorm();
				}
			}
			if (Random.Range(0, 2) == 1)
			{
				startWind();
			}
		}
		else if (WorldManager.manageWorld.month == 3)
		{
			if (Random.Range(0, 2) == 1)
			{
				startFog();
			}
			if (Random.Range(0, 10) == 1)
			{
				raining = true;
			}
			if (Random.Range(0, 4) == 1)
			{
				startWind();
			}
		}
		else
		{
			if (Random.Range(0, 8) == 1)
			{
				raining = true;
			}
			if (Random.Range(0, 4) == 1)
			{
				startWind();
			}
		}
		if (WorldManager.manageWorld.day <= 2 && WorldManager.manageWorld.week <= 1 && WorldManager.manageWorld.month <= 1 && WorldManager.manageWorld.year <= 1)
		{
			stopWind();
			windy = false;
			stopFog();
			foggy = false;
			stopStorm();
			storming = false;
			raining = false;
		}
		isRaining(raining);
	}
}
