using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CinematicEffects;

public class StatusManager : MonoBehaviour
{
	public enum BuffType
	{
		healthRegen = 0,
		staminaRegen = 1,
		fullBuff = 2,
		miningBuff = 3,
		loggingBuff = 4,
		huntingBuff = 5,
		farmingBuff = 6,
		fishingBuff = 7,
		defenceBuff = 8,
		speedBuff = 9,
		xPBuff = 10
	}

	public static StatusManager manage;

	public GameObject reviveWindow;

	public GameObject reviveButton;

	public Transform heartWindow;

	public GameObject heartContainerPrefab;

	public HeartContainer[] allContainers;

	public Transform statusWindow;

	public InvSlotAnimator healthBubbleBounce;

	public InvSlotAnimator statusBubbleBounce;

	public ASound faintSound;

	public Image healthBar;

	public Image staminaBar;

	public Damageable connectedDamge;

	public ASound heartHealSound;

	public TonemappingColorGrading worldColor;

	private float stamina = 50f;

	private float staminaMax = 50f;

	public bool tired;

	public bool dead;

	private Animator playerAnim;

	private float changespeed = 1f;

	private TonemappingColorGrading.ColorGradingSettings worldColorSettings;

	public ASound lowHealthSound;

	public GameObject healthBarToHide;

	public GameObject staminaBarToHide;

	public RectTransform staminaBarRect;

	public RectTransform healthBarRect;

	public Conversation faintedOnFirstDayConvo;

	public GameObject lateTiredStatusIcon;

	public Image damageVignette;

	private Coroutine damageRoutine;

	[Header("Fullness -------------")]
	public RectTransform fullnessIconRect;

	public Image fullnessIcon;

	public Sprite[] fullnessSpriteStages;

	public int currentFullness;

	public GameObject[] buffIcons;

	public TextMeshProUGUI[] buffTextBox;

	public Image[] buffLevels;

	public Sprite buffLevel2Sprite;

	public Sprite buffLevel3Sprite;

	private float damageAmountShown;

	private float addFullnessAmount = 35f;

	private float fullnessTimer;

	private Coroutine fillExtraBarRoutine;

	private Buff[] currentBuffs = new Buff[5];

	private static WaitForSeconds sec = new WaitForSeconds(1f);

	private void Awake()
	{
		worldColorSettings = worldColor.colorGrading;
		worldColor.colorGrading = worldColorSettings;
		manage = this;
	}

	private void Start()
	{
		currentBuffs = new Buff[Enum.GetValues(typeof(BuffType)).Length];
		StartCoroutine(setTiredColours());
		StartCoroutine(lateTiredStatus());
		StartCoroutine(fullnessStatus());
	}

	private void Update()
	{
		if ((bool)connectedDamge)
		{
			if (!dead && connectedDamge.health <= 0)
			{
				die();
			}
			changeFillAmount(healthBar, (float)connectedDamge.health / (float)connectedDamge.maxHealth);
			changeFillAmount(staminaBar, stamina / staminaMax);
		}
	}

	public void changeFillAmount(Image toFill, float fillToShow)
	{
		if (toFill.fillAmount != fillToShow)
		{
			if (toFill.fillAmount < fillToShow)
			{
				toFill.fillAmount = Mathf.Clamp(toFill.fillAmount + Time.deltaTime, 0f, fillToShow);
			}
			else
			{
				toFill.fillAmount = Mathf.Clamp(toFill.fillAmount - Time.deltaTime, fillToShow, 1f);
			}
		}
	}

	public void takeDamageUIChanges(int amountTaken)
	{
		healthBubbleBounce.UpdateSlotContents();
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.TakeDamage, amountTaken);
		addToDamageVignette((float)amountTaken * 3f);
		InputMaster.input.doRumble(Mathf.Clamp((float)amountTaken / 10f, 0f, 0.75f));
	}

	public bool isTooFull()
	{
		return currentFullness == 3;
	}

	private IEnumerator lateTiredStatus()
	{
		while (true)
		{
			yield return null;
			if (RealWorldTimeLight.time.currentHour == 0 && NetworkMapSharer.share.nextDayIsReady)
			{
				lateTiredStatusIcon.SetActive(true);
				while (RealWorldTimeLight.time.currentHour == 0 && NetworkMapSharer.share.nextDayIsReady)
				{
					lateTiredStatusIcon.transform.localPosition = staminaBarRect.localPosition + new Vector3(staminaBarRect.sizeDelta.x + 25f, 0f, 0f);
					if (staminaMax > 10f)
					{
						addTempPoints(0, 0f);
					}
					yield return null;
					if (stamina > 10f)
					{
						manage.changeStamina(-0.05f);
					}
				}
				lateTiredStatusIcon.SetActive(false);
			}
			if (!NetworkMapSharer.share.nextDayIsReady)
			{
				lateTiredStatusIcon.SetActive(false);
				while (!NetworkMapSharer.share.nextDayIsReady)
				{
					yield return null;
				}
				lateTiredStatusIcon.SetActive(false);
			}
		}
	}

	private IEnumerator takeDamageRoutine()
	{
		Color colourToSet = damageVignette.color;
		damageVignette.enabled = true;
		while (damageAmountShown > 0f)
		{
			damageAmountShown = Mathf.Clamp(damageAmountShown - Time.deltaTime * 25f, 0f, 100f);
			colourToSet.a = damageAmountShown / 100f;
			damageVignette.color = colourToSet;
			yield return null;
		}
		damageVignette.enabled = false;
		damageRoutine = null;
	}

	public void addToDamageVignette(float amount)
	{
		damageAmountShown += amount;
		if (damageRoutine == null)
		{
			StartCoroutine(takeDamageRoutine());
		}
	}

	public void addToFullness()
	{
		fullnessTimer = addFullnessAmount;
		currentFullness++;
		currentFullness = Mathf.Clamp(currentFullness, 0, 3);
		addBuff(BuffType.fullBuff, (int)fullnessTimer, currentFullness);
	}

	private IEnumerator fullnessStatus()
	{
		while (true)
		{
			yield return null;
			fullnessIconRect.transform.localPosition = staminaBarRect.localPosition + new Vector3(staminaBarRect.sizeDelta.x, 0f, 0f);
			fullnessIcon.fillAmount = (float)currentFullness / 3f;
			if (currentFullness <= 0)
			{
				continue;
			}
			fullnessTimer -= Time.deltaTime;
			changeStamina(0.001f * (float)currentFullness);
			if (fullnessTimer <= 0f)
			{
				currentFullness--;
				if (currentFullness != 0)
				{
					fullnessTimer = addFullnessAmount;
				}
				addBuff(BuffType.fullBuff, (int)fullnessTimer, currentFullness);
			}
		}
	}

	private void die()
	{
		InputMaster.input.doRumble(0.85f);
		takeDamageUIChanges(100);
		MusicManager.manage.stopMusic();
		SoundManager.manage.play2DSound(faintSound);
		DailyTaskGenerator.generate.doATask(DailyTaskGenerator.genericTaskType.Faint);
		dead = true;
		NetworkMapSharer.share.localChar.myEquip.equipNewItem(-1);
		stamina = 0f;
		NetworkMapSharer.share.localChar.CmdCharFaints();
		reviveWindow.gameObject.SetActive(true);
		Inventory.inv.checkIfWindowIsNeeded();
	}

	public void changeStamina(float takeOrPlus)
	{
		statusBubbleBounce.UpdateSlotContents();
		if (takeOrPlus < 0f && fullnessTimer > 0f)
		{
			fullnessTimer -= Time.deltaTime;
		}
		stamina = Mathf.Clamp(stamina + takeOrPlus, 0f, staminaMax);
		if (Mathf.Floor(stamina) != (float)connectedDamge.myChar.stamina)
		{
			connectedDamge.myChar.CmdSetNewStamina((int)Mathf.Floor(stamina));
		}
		if (!dead)
		{
			if (stamina <= 0f)
			{
				die();
			}
			else if (stamina < 10f && takeOrPlus < 0f)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.sweatParticles, connectedDamge.transform.root.position + Vector3.up * 1.5f, UnityEngine.Random.Range(5, 10));
			}
			if (stamina < 10f)
			{
				tired = true;
			}
			else
			{
				tired = false;
			}
		}
	}

	public void sweatParticlesNotLocal(Vector3 pos)
	{
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.sweatParticles, pos + Vector3.up * 1.5f, UnityEngine.Random.Range(5, 10));
	}

	public void addTempPoints(int tempHealthDif, float tempStaminaDif)
	{
		if (RealWorldTimeLight.time.currentHour != 0)
		{
			staminaMax = Mathf.Clamp(staminaMax + tempStaminaDif, 50f, 100f);
		}
		else
		{
			staminaMax = 10f;
			if (stamina > staminaMax)
			{
				stamina = staminaMax;
			}
		}
		if (tempHealthDif != 0)
		{
			connectedDamge.maxHealth = Mathf.Clamp(connectedDamge.maxHealth + tempHealthDif, 50, 100);
			connectedDamge.CmdChangeMaxHealth(Mathf.Clamp(connectedDamge.maxHealth + tempHealthDif, 50, 100));
		}
		changeStatus(tempHealthDif, tempStaminaDif);
		if (fillExtraBarRoutine != null)
		{
			StopCoroutine(fillExtraBarRoutine);
		}
		fillExtraBarRoutine = StartCoroutine(fillExtraBar());
	}

	public void loadNewMaxStaminaAndHealth(float newMaxStam, int newMaxHealth)
	{
		connectedDamge.maxHealth = newMaxHealth;
		connectedDamge.CmdChangeMaxHealth(newMaxHealth);
		staminaMax = newMaxStam;
		if (fillExtraBarRoutine != null)
		{
			StopCoroutine(fillExtraBarRoutine);
		}
		fillExtraBarRoutine = StartCoroutine(fillExtraBar());
	}

	public void changeStatus(int healthChange, float staminaChange)
	{
		changeStamina(staminaChange);
		if (healthChange != 0)
		{
			connectedDamge.CmdChangeHealth(healthChange);
		}
	}

	public void changeHealthTo(int newHealth)
	{
		connectedDamge.CmdChangeHealthTo(newHealth);
	}

	public void nextDayReset()
	{
		connectedDamge.CmdStopStatusEffects();
		staminaMax = 50f;
		connectedDamge.CmdChangeMaxHealth(Mathf.Clamp(50, 50, 100));
		connectedDamge.maxHealth = Mathf.Clamp(50, 50, 100);
		changeStamina(50f);
		changeHealthTo(50);
		healthBarRect.sizeDelta = Vector2.Lerp(healthBarRect.sizeDelta, new Vector2(20 + connectedDamge.maxHealth * 2, 18f), 1f);
		staminaBarRect.sizeDelta = Vector2.Lerp(staminaBarRect.sizeDelta, new Vector2(20f + staminaMax * 2f, 18f), 1f);
	}

	public void connectPlayer(Damageable mainPlayerDamage)
	{
		connectedDamge = mainPlayerDamage;
		StartCoroutine(lowHealthCheck());
		statusWindow.gameObject.SetActive(true);
		playerAnim = mainPlayerDamage.gameObject.GetComponent<Animator>();
	}

	public void revive()
	{
		StartCoroutine(reviveSelfButton());
	}

	public IEnumerator reviveSelfButton()
	{
		reviveWindow.gameObject.SetActive(false);
		NetworkMapSharer.share.canUseMineControls = true;
		NetworkMapSharer.share.localChar.CmdReviveSelf();
		MusicManager.manage.startMusic();
		Inventory.inv.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
		yield return StartCoroutine(reviveDelay());
		if ((WorldManager.manageWorld.year != 1 || WorldManager.manageWorld.month != 1 || WorldManager.manageWorld.week != 1 || WorldManager.manageWorld.day != 1) && NetworkNavMesh.nav.getPlayerCount() <= 1 && !RealWorldTimeLight.time.underGround)
		{
			WorldManager.manageWorld.nextDay();
		}
	}

	public void getRevivedByOtherChar()
	{
		dead = false;
		playerAnim.SetBool("Fainted", false);
		MusicManager.manage.startMusic();
		reviveWindow.gameObject.SetActive(false);
		Inventory.inv.checkIfWindowIsNeeded();
		MenuButtonsTop.menu.closeButtonDelay();
	}

	private IEnumerator reviveDelay()
	{
		connectedDamge.GetComponent<Rigidbody>().isKinematic = true;
		while (connectedDamge.health <= 0)
		{
			yield return null;
		}
		if (!RealWorldTimeLight.time.underGround && TownManager.manage.lastSleptPos != Vector3.zero)
		{
			if (TownManager.manage.sleepInsideHouse != null)
			{
				NetworkMapSharer.share.localChar.myInteract.changeInsideOut(true, TownManager.manage.sleepInsideHouse);
				TownManager.manage.savedInside[0] = TownManager.manage.sleepInsideHouse.xPos;
				TownManager.manage.savedInside[1] = TownManager.manage.sleepInsideHouse.yPos;
				WeatherManager.manage.goInside(false);
				RealWorldTimeLight.time.goInside();
				MusicManager.manage.changeInside(true, false);
				NetworkMapSharer.share.localChar.myEquip.setInsideOrOutside(true, true);
			}
			else
			{
				NetworkMapSharer.share.localChar.myInteract.changeInsideOut(false);
				WeatherManager.manage.goOutside();
				RealWorldTimeLight.time.goOutside();
			}
			NetworkMapSharer.share.localChar.transform.position = TownManager.manage.lastSleptPos;
		}
		else if (!RealWorldTimeLight.time.underGround && NetworkMapSharer.share.personalSpawnPoint != Vector3.zero)
		{
			NetworkMapSharer.share.localChar.transform.position = NetworkMapSharer.share.personalSpawnPoint;
			if (WorldManager.manageWorld.spawnPos.position.y <= -12f)
			{
				NewChunkLoader.loader.inside = false;
				WeatherManager.manage.goOutside();
				RealWorldTimeLight.time.goOutside();
				NetworkMapSharer.share.localChar.myEquip.setInsideOrOutside(false, false);
				MusicManager.manage.changeInside(true, false);
			}
		}
		else
		{
			NetworkMapSharer.share.localChar.transform.position = WorldManager.manageWorld.spawnPos.position;
		}
		CameraController.control.transform.position = NetworkMapSharer.share.localChar.transform.position;
		NewChunkLoader.loader.forceInstantUpdateAtPos();
		if (WorldManager.manageWorld.year == 1 && WorldManager.manageWorld.month == 1 && WorldManager.manageWorld.week == 1 && WorldManager.manageWorld.day == 1)
		{
			NetworkNavMesh.nav.InstantNavMeshRefresh();
			NetworkMapSharer.share.fadeToBlack.setBlack();
			while (!NetworkNavMesh.nav.doesPositionHaveNavChunk(Mathf.RoundToInt(NetworkMapSharer.share.localChar.transform.position.x / 2f), Mathf.RoundToInt(NetworkMapSharer.share.localChar.transform.position.z / 2f)))
			{
				yield return null;
			}
			NPCManager.manage.moveNpcToPlayerAndStartTalking(6, false, faintedOnFirstDayConvo);
			if (WorldManager.manageWorld.spawnPos.position.y <= -12f)
			{
				NPCManager.manage.setNPCInSideBuilding(6, NPCSchedual.Locations.Post_Office);
			}
		}
		if (stamina == 0f)
		{
			stamina = 10f;
			tired = false;
		}
		playerAnim.SetBool("Tired", false);
		Inventory.inv.changeWallet(-(Inventory.inv.wallet / 100 * 20));
		Inventory.inv.damageAllTools();
		dead = false;
		Inventory.inv.equipNewSelectedSlot();
		Inventory.inv.checkIfWindowIsNeeded();
		playerAnim.SetBool("Fainted", false);
		yield return null;
		connectedDamge.GetComponent<Rigidbody>().isKinematic = false;
	}

	public bool staminaAboveNo(float aboveThis)
	{
		return stamina > aboveThis;
	}

	public float getStamina()
	{
		return stamina;
	}

	public float getStaminaMax()
	{
		return staminaMax;
	}

	public void loadStatus(int loadHealth, int loadHealthMax, float loadStamina, float loadStaminaMax)
	{
		StartCoroutine(loadStaminaAndHealth(loadHealth, loadHealthMax, loadStamina, loadStaminaMax));
	}

	public IEnumerator loadStaminaAndHealth(int loadHealth, int loadHealthMax, float loadStamina, float loadStaminaMax)
	{
		while (connectedDamge == null)
		{
			yield return null;
		}
		loadNewMaxStaminaAndHealth(loadStaminaMax, loadHealthMax);
		stamina = loadStamina;
		connectedDamge.CmdChangeHealthTo(loadHealth);
	}

	public void staminaAndHealthBarOn(bool isOn)
	{
		healthBarToHide.SetActive(isOn);
		staminaBarToHide.SetActive(isOn);
		fullnessIconRect.gameObject.SetActive(isOn);
		if (isOn)
		{
			lateTiredStatusIcon.SetActive(isOn && RealWorldTimeLight.time.currentHour == 0);
		}
		else
		{
			lateTiredStatusIcon.SetActive(false);
		}
		QuestTracker.track.pinnedMissionTextOn = isOn;
		QuestTracker.track.updatePinnedTask();
	}

	private IEnumerator lowHealthCheck()
	{
		while (true)
		{
			if ((float)connectedDamge.health <= 10f && !dead)
			{
				float noiseTimer = 2f;
				while ((float)connectedDamge.health <= 10f && !dead)
				{
					noiseTimer += Time.deltaTime;
					if (noiseTimer >= 2f)
					{
						SoundManager.manage.play2DSound(lowHealthSound);
						healthBubbleBounce.UpdateSlotContents();
						noiseTimer = 0f;
					}
					yield return null;
				}
			}
			yield return null;
		}
	}

	private IEnumerator setTiredColours()
	{
		while (true)
		{
			if (stamina < 10f)
			{
				if (!worldColorSettings.basics.saturation.Equals(stamina / 10f))
				{
					float timer2 = 0f;
					while (!worldColorSettings.basics.saturation.Equals(stamina / 10f) && stamina < 10f)
					{
						worldColorSettings.basics.saturation = Mathf.Lerp(worldColorSettings.basics.saturation, stamina / 10f, timer2);
						worldColor.colorGrading = worldColorSettings;
						timer2 += Time.deltaTime / 2f;
						yield return null;
					}
				}
			}
			else if (stamina >= 10f && !worldColorSettings.basics.saturation.Equals(1f))
			{
				float timer2 = 0f;
				while (!worldColorSettings.basics.saturation.Equals(1f) && stamina >= 10f)
				{
					worldColorSettings.basics.saturation = Mathf.Lerp(worldColorSettings.basics.saturation, 1f, timer2);
					worldColor.colorGrading = worldColorSettings;
					timer2 += Time.deltaTime / 4f;
					yield return null;
				}
			}
			yield return null;
		}
	}

	private IEnumerator fillExtraBar()
	{
		float timer = 0f;
		while (timer < 1f)
		{
			timer += Time.deltaTime * 2f;
			healthBarRect.sizeDelta = Vector2.Lerp(healthBarRect.sizeDelta, new Vector2(20 + connectedDamge.maxHealth * 2, 18f), timer);
			staminaBarRect.sizeDelta = Vector2.Lerp(staminaBarRect.sizeDelta, new Vector2(20f + staminaMax * 2f, 18f), timer);
			yield return null;
		}
		healthBarRect.sizeDelta = Vector2.Lerp(healthBarRect.sizeDelta, new Vector2(20 + connectedDamge.maxHealth * 2, 18f), 1f);
		staminaBarRect.sizeDelta = Vector2.Lerp(staminaBarRect.sizeDelta, new Vector2(20f + staminaMax * 2f, 18f), 1f);
		fillExtraBarRoutine = null;
	}

	public void addBuff(BuffType typeToAdd, int time, int level)
	{
		if (currentBuffs[(int)typeToAdd] == null)
		{
			currentBuffs[(int)typeToAdd] = new Buff(time, level);
			showBuffLevel(typeToAdd);
			StartCoroutine(countDownBuff((int)typeToAdd, currentBuffs[(int)typeToAdd]));
		}
		else
		{
			currentBuffs[(int)typeToAdd].stackBuff(time, level, typeToAdd == BuffType.fullBuff);
			showBuffLevel(typeToAdd);
		}
		checkIfBuffNeedsCommand(typeToAdd, level, time);
	}

	private IEnumerator countDownBuff(int buffId, Buff myBuff)
	{
		buffIcons[buffId].gameObject.SetActive(true);
		while (currentBuffs[buffId] != null)
		{
			if (Mathf.RoundToInt((float)myBuff.getTimeRemaining() / 60f) > 1)
			{
				buffTextBox[buffId].text = Mathf.RoundToInt((float)myBuff.getTimeRemaining() / 60f) + "m";
			}
			else
			{
				buffTextBox[buffId].text = myBuff.getTimeRemaining() + "s";
			}
			yield return sec;
			if (myBuff.takeTick())
			{
				checkIfBuffNeedsCommand((BuffType)buffId, 0, 0);
				currentBuffs[buffId] = null;
				break;
			}
		}
		buffIcons[buffId].gameObject.SetActive(false);
	}

	public int getBuffLevel(BuffType buffType)
	{
		if (currentBuffs[(int)buffType] == null)
		{
			return 0;
		}
		return currentBuffs[(int)buffType].getLevel();
	}

	private void showBuffLevel(BuffType buffType)
	{
		buffLevels[(int)buffType].enabled = getBuffLevel(buffType) > 1;
		if (getBuffLevel(buffType) == 2)
		{
			buffLevels[(int)buffType].sprite = buffLevel2Sprite;
		}
		else
		{
			buffLevels[(int)buffType].sprite = buffLevel3Sprite;
		}
	}

	public void checkIfBuffNeedsCommand(BuffType buffType, int level, int timer)
	{
		switch (buffType)
		{
		case BuffType.defenceBuff:
			NetworkMapSharer.share.localChar.CmdSetDefenceBuff(1f + 0.5f * (float)level);
			return;
		case BuffType.healthRegen:
			if (level > 0)
			{
				NetworkMapSharer.share.localChar.CmdSetHealthRegen(timer, level);
				return;
			}
			break;
		}
		if (buffType == BuffType.speedBuff)
		{
			NetworkMapSharer.share.localChar.setSpeedDif(level / 2);
		}
	}
}
