using System.Collections;
using UnityEngine;
using UnityStandardAssets.CinematicEffects;
using UnityStandardAssets.ImageEffects;

public class CameraController : MonoBehaviour
{
	public static CameraController control;

	public int freeCamYLimitMax = 140;

	public int freeCamYLimitMin = -30;

	public Camera mainCamera;

	public CameraShake myShake;

	public int playerNo = 1;

	private Transform followTransform;

	public Transform Camera_Y;

	public Transform cameraTrans;

	private float Yrotation = 35f;

	public UnityStandardAssets.CinematicEffects.DepthOfField def;

	private float camDistance = 12f;

	public NewChunkLoader newChunkLoader;

	public cameraWonderOnMenu wondering;

	private bool conversationCam;

	private float followSpeed = 0.15f;

	public bool inCar;

	private bool cameraLocked = true;

	public AudioSource riverNoiseAudio;

	public AudioSource oceanNoiseAudio;

	public AudioSource landAmbientAudio;

	public AudioClip riverSound;

	public AudioClip oceanSound;

	public FadeBlackness blackFadeAnim;

	private bool lockCameraForTransition;

	private bool freeCamOn;

	private bool aimCamOn;

	private Vector3 freeCamPos = Vector3.zero;

	private float turnSpeed;

	private bool freeCam;

	private bool zoomOnChar;

	private float savedCamDistance = 12f;

	private bool distanceLock;

	private CharMovement followingChar;

	public bool flyCamOn;

	public GameObject undergroundSoundZone;

	private Transform targetfacing;

	public GlobalFog fogScript;

	public PickUpNotification cameraHint;

	public int xMod = 1;

	public int YMod = 1;

	public bool toggle;

	public LayerMask cameraCollisions;

	public Vector3 freePosition;

	public Quaternion freeRotation;

	public RectTransform waterEffect;

	public RectTransform underWaterEffect;

	public RectTransform waterCanvas;

	public GameObject waterUnderSide;

	private Vector3 cameraLocalPosition;

	private float farPlaneNo = 7.6f;

	private Vector3 followVel = Vector3.one;

	public float landAmbienceMax = 0.25f;

	public AudioClip dayTimeAmbience;

	public AudioClip undergroundAmbience;

	public AudioClip nightTimeAmbience;

	private RaycastHit cameraHit;

	private float distance = 7f;

	private float maxDistance = 12f;

	private float maxSpeed = 10f;

	public bool cameraShowingSomething;

	public float checkDistance = 0.3f;

	private void Awake()
	{
		control = this;
	}

	private void Start()
	{
		StartCoroutine(cameraSwitchControl());
		StartCoroutine(playAmbientSounds());
		StartCoroutine(cameraUnderWaterEffects());
	}

	private IEnumerator cameraSwitchControl()
	{
		while (Inventory.inv.menuOpen || TownManager.manage.firstConnect)
		{
			yield return null;
		}
		while (true)
		{
			if (!ChatBox.chat.chatOpen && !Inventory.inv.isMenuOpen())
			{
				cameraHint.gameObject.SetActive(true);
				updateCameraSwitchPrompt(Inventory.inv.usingMouse);
				if (InputMaster.input.SwapCamera() && !ConversationManager.manage.inConversation)
				{
					SoundManager.manage.play2DSound(SoundManager.manage.cameraSwitch);
					swapFreeCam();
					updateCameraSwitchPrompt(Inventory.inv.usingMouse);
				}
			}
			else
			{
				cameraHint.gameObject.SetActive(false);
			}
			yield return null;
		}
	}

	public void setCamDistanceForDeedPlacement()
	{
		camDistance = 12f;
	}

	public void swapFlyCam(bool followCam = true)
	{
		if (flyCamOn)
		{
			stopFlyCam();
		}
		else
		{
			startFlyCam(followCam);
		}
	}

	private void startFlyCam(bool follow)
	{
		flyCamOn = true;
		cameraLocalPosition = cameraTrans.localPosition;
		cameraTrans.parent = null;
		cameraTrans.GetComponent<camerFlys>().charFollows = follow;
		cameraTrans.GetComponent<camerFlys>().enabled = true;
		def.focus.transform = null;
		myShake.enabled = false;
		if (follow)
		{
			NetworkMapSharer.share.localChar.lockCharOnFreeCam();
		}
		setFollowTransform(cameraTrans);
		if (freePosition != Vector3.zero)
		{
			cameraTrans.position = freePosition;
			cameraTrans.rotation = freeRotation;
		}
	}

	private void stopFlyCam()
	{
		flyCamOn = false;
		freePosition = cameraTrans.transform.position;
		freeRotation = cameraTrans.transform.rotation;
		myShake.enabled = true;
		cameraTrans.parent = Camera_Y;
		cameraTrans.localRotation = Quaternion.Euler(new Vector3(50f, 0f, 0f));
		cameraTrans.localPosition = cameraLocalPosition;
		cameraTrans.GetComponent<camerFlys>().enabled = false;
		def.focus.transform = base.transform;
		setFollowTransform(NetworkMapSharer.share.localChar.transform);
		NetworkMapSharer.share.localChar.unlocklockCharOnFreeCam();
	}

	public void clearFreeCam()
	{
		freePosition = Vector3.zero;
	}

	public void saveFreeCam()
	{
		PlayerPrefs.SetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamPosX", cameraTrans.transform.position.x);
		PlayerPrefs.SetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamPosY", cameraTrans.transform.position.y);
		PlayerPrefs.SetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamPosZ", cameraTrans.transform.position.z);
		PlayerPrefs.SetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamRotX", cameraTrans.transform.eulerAngles.x);
		PlayerPrefs.SetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamRotY", cameraTrans.transform.eulerAngles.y);
		PlayerPrefs.SetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamRotZ", cameraTrans.transform.eulerAngles.z);
	}

	public void loadFreeCam()
	{
		freePosition = new Vector3(PlayerPrefs.GetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamPosX"), PlayerPrefs.GetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamPosY"), PlayerPrefs.GetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamPosZ"));
		freeRotation = Quaternion.Euler(PlayerPrefs.GetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamRotX"), PlayerPrefs.GetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamRotY"), PlayerPrefs.GetFloat(SaveLoad.saveOrLoad.currentSaveSlotNo() + "freeCamRotZ"));
		if (flyCamOn)
		{
			cameraTrans.position = freePosition;
			cameraTrans.rotation = freeRotation;
		}
	}

	public void updateCameraSwitchPrompt(bool usingMouse)
	{
		string buttonPromptText = "Free Camera";
		if (freeCam)
		{
			buttonPromptText = "Build Camera";
		}
		if (usingMouse)
		{
			cameraHint.fillButtonPrompt(buttonPromptText, cameraHint.controllerZ);
		}
		else
		{
			cameraHint.fillButtonPrompt(buttonPromptText, cameraHint.controllerRightStick);
		}
	}

	public void updateDepthOfFieldAndFog(int newChunkDistance)
	{
		RenderSettings.fogStartDistance = 0f;
		farPlaneNo = 7.6f - (float)(newChunkDistance - 4) * 1.9f;
		if (RealWorldTimeLight.time.underGround)
		{
			fogScript.startDistance = 15f;
			RenderSettings.fogEndDistance = 40f;
			RealWorldTimeLight.time.waterMat.SetFloat("_FogDistance", 25f);
		}
		else
		{
			if (WeatherManager.manage.raining)
			{
				fogScript.startDistance = 10f;
				RenderSettings.fog = true;
			}
			else
			{
				fogScript.startDistance = (float)(20 * newChunkDistance) - 20f;
				RenderSettings.fog = false;
			}
			RenderSettings.fogEndDistance = (float)(20 * newChunkDistance) - 20f + 25f;
			RealWorldTimeLight.time.waterMat.SetFloat("_FogDistance", 25 * newChunkDistance);
		}
		def.focus.farPlane = farPlaneNo;
	}

	public bool isFreeCamOn()
	{
		return freeCam;
	}

	public void swapFreeCam()
	{
		freeCam = !freeCam;
		if (freeCam)
		{
			cameraTrans.localPosition = new Vector3(0f, 8f, -8f);
			Camera_Y.localRotation = Quaternion.Euler(0f, 0f, 0f);
			Camera_Y.localPosition = new Vector3(0f, 1f, 0f);
			def.focus.farPlane = farPlaneNo;
			def.focus.fStops = 5.2f;
			def.focus.nearPlane = 0.05f;
			newChunkLoader.transform.localPosition = new Vector3(0f, 0f, 20f);
			fogScript.enabled = true;
		}
		else
		{
			Camera_Y.localPosition = new Vector3(0f, 0f, 0f);
			Camera_Y.localRotation = Quaternion.Euler(0f, 0f, 0f);
			newChunkLoader.transform.localPosition = Vector3.zero;
			def.focus.fStops = 32f;
		}
	}

	public void faceTarget(Transform target)
	{
		targetfacing = target;
	}

	private void LateUpdate()
	{
		if ((bool)followTransform && followTransform.parent != null)
		{
			if (aimCamOn)
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, followTransform.position, ref followVel, followSpeed);
			}
			else
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, followTransform.position, ref followVel, followSpeed * 2f);
			}
		}
	}

	private void FixedUpdate()
	{
		if (!followTransform || lockCameraForTransition)
		{
			return;
		}
		if (followTransform.parent == null)
		{
			if (aimCamOn)
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, followTransform.position, ref followVel, followSpeed / 10f);
			}
			else
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, followTransform.position, ref followVel, followSpeed);
			}
		}
		controlCam();
	}

	public void setFollowTransform(Transform followthis, float newFollowSpeed = 0.15f)
	{
		followSpeed = newFollowSpeed;
		followTransform = followthis;
		followingChar = followthis.GetComponent<CharMovement>();
	}

	public void moveToFollowing(bool slowFade = false)
	{
		base.transform.position = followTransform.position;
		if (slowFade)
		{
			blackFadeAnim.fadeTime = 1f;
			blackFadeAnim.fadeOut();
		}
		else
		{
			blackFadeAnim.fadeTime = 0.5f;
			blackFadeAnim.fadeOut();
		}
	}

	public void shakeScreen(float trauma0to1)
	{
		myShake.addToTrauma(trauma0to1);
	}

	public void shakeScreenMax(float trauma0to1, float max)
	{
		myShake.addToTraumaMax(trauma0to1, max);
	}

	public bool isInAimCam()
	{
		return aimCamOn;
	}

	public void enterAimCamera()
	{
		if (!aimCamOn)
		{
			Crossheir.cross.turnOnCrossheir();
		}
		aimCamOn = true;
	}

	public void exitAimCamera()
	{
		if (aimCamOn)
		{
			Crossheir.cross.turnOnCrossheir();
			aimCamOn = false;
			freeCam = !freeCam;
			swapFreeCam();
			Cursor.lockState = CursorLockMode.Confined;
		}
	}

	private void controlCam()
	{
		if (aimCamOn)
		{
			Camera_Y.localPosition = new Vector3(0f, 1f, 0f);
			cameraTrans.transform.localPosition = new Vector3(2f, 3f, -1f);
			Vector3 forward = cameraTrans.position + cameraTrans.forward * 100f - base.transform.position;
			forward.y = 0f;
			Quaternion b = Quaternion.LookRotation(forward);
			followTransform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * 250f);
			float x = InputMaster.input.getMousePosOld().x;
			float num = InputMaster.input.getMousePosOld().y;
			if (!Inventory.inv.usingMouse)
			{
				x = InputMaster.input.getRightStick().x;
				num = InputMaster.input.getRightStick().y * (float)YMod;
			}
			Yrotation += num * Time.deltaTime * 55f;
			Yrotation = Mathf.Clamp(Yrotation, -30f, 140f);
			Camera_Y.localEulerAngles = Vector3.left * Yrotation;
			x *= (float)xMod;
			float num2 = 2f * x;
			turnSpeed = Mathf.Lerp(turnSpeed, num2, Time.deltaTime * 15f);
			if (!targetfacing || num2 != 0f)
			{
				base.transform.Rotate(0f, turnSpeed, 0f);
			}
			return;
		}
		if (flyCamOn)
		{
			base.transform.rotation = Quaternion.Euler(0f, cameraTrans.eulerAngles.y, 0f);
			return;
		}
		if (Inventory.inv.canMoveChar() && !conversationCam && !zoomOnChar && !RenderMap.map.mapOpen)
		{
			float num3 = InputMaster.input.getMousePosOld().x;
			float num4 = 0f - InputMaster.input.getMousePosOld().y;
			if (!Inventory.inv.usingMouse)
			{
				num3 = InputMaster.input.getRightStick().x;
				num4 = 0f - InputMaster.input.getRightStick().y;
			}
			if (InputMaster.input.TriggerLookHeld() || PhotoManager.manage.cameraViewOpen || toggle)
			{
				if (PhotoManager.manage.cameraViewOpen && !PhotoManager.manage.canMoveCam)
				{
					num3 = 0f;
					num4 = 0f;
				}
			}
			else if (Inventory.inv.usingMouse)
			{
				num3 = 0f;
				num4 = 0f;
			}
			if (!cameraLocked && !Inventory.inv.invOpen)
			{
				num3 = InputMaster.input.getMousePosOld().x;
				num4 = InputMaster.input.getMousePosOld().y;
			}
			if (freeCam)
			{
				num4 *= (float)YMod;
				checkCameraForCollisions();
			}
			num3 *= (float)xMod;
			float num5 = 2f * num3;
			turnSpeed = Mathf.Lerp(turnSpeed, num5, Time.deltaTime * 15f);
			if (!targetfacing || num5 != 0f)
			{
				base.transform.Rotate(0f, turnSpeed, 0f);
			}
			else
			{
				Quaternion b2 = Quaternion.LookRotation((base.transform.position - new Vector3(targetfacing.position.x, base.transform.position.y, targetfacing.position.z)).normalized);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b2, Time.deltaTime * 8f);
			}
			if (freeCam)
			{
				Yrotation += num4 * Time.deltaTime * 55f;
				Yrotation = Mathf.Clamp(Yrotation, freeCamYLimitMin, freeCamYLimitMax);
				Camera_Y.localEulerAngles = Vector3.left * Yrotation;
			}
			else if (!distanceLock)
			{
				if (!Physics.Raycast(cameraTrans.position, cameraTrans.forward * num4, out cameraHit, 1f, cameraCollisions))
				{
					camDistance = Mathf.Clamp(camDistance + num4 / 5f, 5f, 15f);
				}
				cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Vector3.up * camDistance - Vector3.forward * camDistance, Time.deltaTime * 10f);
			}
			else
			{
				cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Vector3.up * camDistance - Vector3.forward * camDistance, Time.deltaTime);
			}
		}
		if (ConversationManager.manage.inConversation && !conversationCam && !ConversationManager.manage.lastTalkTo.isSign)
		{
			StartCoroutine(zoomInConvo());
		}
	}

	public void zoomInOnCharForChair()
	{
		StartCoroutine(ZoomInOnChair());
	}

	public void checkCameraForCollisionsZoom(Vector3 desiredLocalPosition, float amount)
	{
		Vector3 vector = Vector3.Lerp(cameraTrans.localPosition, desiredLocalPosition, amount);
		Vector3 normalized = (Camera_Y.TransformPoint(vector) - Camera_Y.position).normalized;
		float num = Vector3.Distance(Camera_Y.TransformPoint(vector), Camera_Y.position);
		Debug.DrawLine(Camera_Y.position, Camera_Y.position + normalized * num, Color.white);
		RaycastHit hitInfo;
		if (Physics.Linecast(Camera_Y.position, Camera_Y.position + normalized * num, out hitInfo, cameraCollisions))
		{
			distance = Mathf.Clamp(hitInfo.distance * 0.87f, 0.5f, num);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Camera_Y.InverseTransformDirection(normalized * distance), amount);
		}
		else
		{
			cameraTrans.localPosition = vector;
		}
	}

	private IEnumerator ZoomInOnChair()
	{
		zoomOnChar = true;
		Vector3 originalLocalPosition = cameraTrans.localPosition;
		Quaternion originalRotation = base.transform.rotation;
		Vector3 eulerAngle = followTransform.eulerAngles;
		Vector3 desiredPos = base.transform.position - followTransform.forward - base.transform.position;
		Vector3 camYPos = Camera_Y.transform.localPosition;
		Quaternion camYRot = Camera_Y.localRotation;
		if (!freeCam)
		{
			def.focus.fStops = 32f;
		}
		def.enabled = true;
		float velocity = 0f;
		float amount = 0f;
		float smoother = 1f;
		while (HairDresserMenu.menu.hairMenuOpen)
		{
			yield return null;
			Camera_Y.localRotation = Quaternion.Lerp(Camera_Y.localRotation, Quaternion.Euler(-20f, 0f, 0f), amount);
			Camera_Y.localPosition = Vector3.Lerp(Camera_Y.localPosition, new Vector3(0f, 1f, 0f), amount);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(desiredPos), amount);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Vector3.up * 5f - Vector3.forward * 5f, amount);
			def.focus.fStops = Mathf.Lerp(def.focus.fStops, 2.9f, amount);
			def.focus.farPlane = Mathf.Lerp(def.focus.farPlane, 14.6f, amount);
			def.focus.nearPlane = Mathf.Lerp(def.focus.nearPlane, 4.4f, amount);
			amount = Mathf.SmoothDamp(amount, 1f, ref velocity, smoother);
			float value = smoother + Time.deltaTime * 10f;
			smoother = Mathf.Clamp(value, 1f, 25f);
		}
		for (float returnTo = 0f; returnTo < 1f; returnTo += Mathf.Clamp01(Time.deltaTime * 2f))
		{
			if (HairDresserMenu.menu.hairMenuOpen)
			{
				break;
			}
			yield return null;
			Camera_Y.localRotation = Quaternion.Lerp(Camera_Y.localRotation, camYRot, returnTo);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, originalRotation, returnTo);
			Camera_Y.localPosition = Vector3.Lerp(Camera_Y.localPosition, camYPos, returnTo);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, originalLocalPosition, returnTo);
			if (!freeCam)
			{
				def.focus.fStops = Mathf.Lerp(def.focus.fStops, 32f, returnTo);
				continue;
			}
			def.focus.fStops = Mathf.Lerp(def.focus.fStops, 5.2f, returnTo);
			def.focus.farPlane = Mathf.Lerp(def.focus.farPlane, farPlaneNo, returnTo);
			def.focus.nearPlane = Mathf.Lerp(def.focus.nearPlane, 0.05f, returnTo);
		}
		if (!freeCam)
		{
			def.enabled = false;
		}
		Camera_Y.localRotation = camYRot;
		Camera_Y.localPosition = camYPos;
		zoomOnChar = false;
	}

	public IEnumerator zoomOnCharacterInventory()
	{
		zoomOnChar = true;
		Vector3 originalLocalPosition = cameraTrans.localPosition;
		Quaternion originalRotation = base.transform.rotation;
		Vector3 eulerAngle = followTransform.eulerAngles;
		Vector3 desiredPos = base.transform.position - followTransform.forward - base.transform.position;
		while (Inventory.inv.invOpen || HairDresserMenu.menu.hairMenuOpen || BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen)
		{
			Camera_Y.localRotation = Quaternion.Lerp(Camera_Y.localRotation, Quaternion.Euler(-20f, 0f, 0f), Time.deltaTime * 4f);
			Camera_Y.localPosition = Vector3.Lerp(Camera_Y.localPosition, new Vector3(0f, 1f, 2f), Time.deltaTime * 3f);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(desiredPos), Time.deltaTime * 6f);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Vector3.up * 5f - Vector3.forward * 5f, Time.deltaTime * 6f);
			yield return null;
		}
		for (float returnTo = 0f; returnTo < 1f; returnTo += Time.deltaTime)
		{
			if (Inventory.inv.invOpen)
			{
				break;
			}
			if (HairDresserMenu.menu.hairMenuOpen)
			{
				break;
			}
			if (BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen)
			{
				break;
			}
			yield return null;
			Camera_Y.localRotation = Quaternion.Lerp(Quaternion.Euler(-20f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f), returnTo);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, originalRotation, returnTo);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, originalLocalPosition, returnTo);
			Camera_Y.localPosition = Vector3.Lerp(new Vector3(-3f, 1f, 2f), new Vector3(0f, 0f, 0f), returnTo);
		}
		Camera_Y.localRotation = Quaternion.Euler(0f, 0f, 0f);
		zoomOnChar = false;
	}

	public IEnumerator zoomInFishOrBug()
	{
		zoomOnChar = true;
		Vector3 originalLocalPosition = cameraTrans.localPosition;
		Quaternion originalRotation = base.transform.rotation;
		Vector3 eulerAngle = followTransform.eulerAngles;
		Vector3 desiredPos = base.transform.position - followTransform.forward - base.transform.position;
		Vector3 camYPos = Camera_Y.transform.localPosition;
		Quaternion camYRot = Camera_Y.localRotation;
		if (!freeCam)
		{
			def.focus.fStops = 32f;
		}
		def.enabled = true;
		float velocity = 0f;
		float amount = 0f;
		float smoother = 1f;
		while (BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen || (ConversationManager.manage.inConversation && ConversationManager.manage.lastTalkTo.isSign))
		{
			yield return null;
			Camera_Y.localRotation = Quaternion.Lerp(Camera_Y.localRotation, Quaternion.Euler(-20f, 0f, 0f), amount);
			Camera_Y.localPosition = Vector3.Lerp(Camera_Y.localPosition, new Vector3(0f, 1f, 0f), amount);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(desiredPos), amount);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Vector3.up * 5f - Vector3.forward * 5f, amount);
			def.focus.fStops = Mathf.Lerp(def.focus.fStops, 2.9f, amount);
			def.focus.farPlane = Mathf.Lerp(def.focus.farPlane, 14.6f, amount);
			def.focus.nearPlane = Mathf.Lerp(def.focus.nearPlane, 4.4f, amount);
			amount = Mathf.SmoothDamp(amount, 1f, ref velocity, smoother);
			float value = smoother + Time.deltaTime * 10f;
			smoother = Mathf.Clamp(value, 1f, 25f);
		}
		for (float returnTo = 0f; returnTo < 1f; returnTo += Mathf.Clamp01(Time.deltaTime * 2f))
		{
			if (BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen)
			{
				break;
			}
			yield return null;
			Camera_Y.localRotation = Quaternion.Lerp(Camera_Y.localRotation, camYRot, returnTo);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, originalRotation, returnTo);
			Camera_Y.localPosition = Vector3.Lerp(Camera_Y.localPosition, camYPos, returnTo);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, originalLocalPosition, returnTo);
			if (!freeCam)
			{
				def.focus.fStops = Mathf.Lerp(def.focus.fStops, 32f, returnTo);
				continue;
			}
			def.focus.fStops = Mathf.Lerp(def.focus.fStops, 5.2f, returnTo);
			def.focus.farPlane = Mathf.Lerp(def.focus.farPlane, farPlaneNo, returnTo);
			def.focus.nearPlane = Mathf.Lerp(def.focus.nearPlane, 0.05f, returnTo);
		}
		if (!freeCam)
		{
			def.enabled = false;
		}
		Camera_Y.localRotation = camYRot;
		Camera_Y.localPosition = camYPos;
		zoomOnChar = false;
	}

	private IEnumerator zoomInConvo()
	{
		conversationCam = true;
		Vector3 originalLocalPosition = cameraTrans.localPosition;
		Quaternion originalRotation = base.transform.rotation;
		Vector3 eulerAngle = followTransform.eulerAngles;
		Vector3 zero = Vector3.zero;
		Vector3 camYPos = Camera_Y.transform.localPosition;
		Quaternion camYRot = Camera_Y.localRotation;
		Vector3 desiredLocalPosition = Vector3.up * 4f - Vector3.forward * 3f;
		Vector3 desiredPos2;
		if (Vector3.Distance(followTransform.position + followTransform.right + followTransform.forward, cameraTrans.position) < Vector3.Distance(followTransform.position - followTransform.right + followTransform.forward, cameraTrans.position))
		{
			desiredPos2 = base.transform.position + followTransform.forward - followTransform.right - base.transform.position;
			desiredLocalPosition += Vector3.right * Mathf.Clamp(Vector3.Distance(base.transform.position, ConversationManager.manage.lastTalkTo.transform.position), 0f, 0f);
		}
		else
		{
			desiredPos2 = base.transform.position + followTransform.forward + followTransform.right - base.transform.position;
			desiredLocalPosition -= Vector3.right * Mathf.Clamp(Vector3.Distance(base.transform.position, ConversationManager.manage.lastTalkTo.transform.position), 0f, 0f);
		}
		desiredPos2 = new Vector3(desiredPos2.x, 0f, desiredPos2.z);
		if (!freeCam)
		{
			def.focus.fStops = 32f;
		}
		def.enabled = true;
		float velocity = 0f;
		float amount = 0f;
		float smoother = 3f;
		while ((ConversationManager.manage.inConversation || GiveNPC.give.giveWindowOpen) && !zoomOnChar)
		{
			yield return null;
			amount = Mathf.SmoothDamp(amount, 1f, ref velocity, smoother);
			Camera_Y.localRotation = Quaternion.Lerp(Camera_Y.localRotation, Quaternion.Euler(-20f, 0f, 0f), amount);
			Camera_Y.localPosition = Vector3.Lerp(Camera_Y.localPosition, new Vector3(0f, 1f, 0f), amount);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(desiredPos2), amount);
			checkCameraForCollisionsZoom(desiredLocalPosition, amount);
			def.focus.fStops = Mathf.Lerp(def.focus.fStops, 2.9f, amount);
			def.focus.farPlane = Mathf.Lerp(def.focus.farPlane, 14.6f, amount);
			def.focus.nearPlane = Mathf.Lerp(def.focus.nearPlane, 4.4f, amount);
			float value = smoother + Time.deltaTime * 10f;
			smoother = Mathf.Clamp(value, 3f, 35f);
		}
		float returnTo = 0f;
		while (returnTo < 1f && !ConversationManager.manage.inConversation && !GiveNPC.give.giveWindowOpen && !zoomOnChar)
		{
			yield return null;
			Camera_Y.localRotation = Quaternion.Lerp(Camera_Y.localRotation, camYRot, returnTo);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, originalRotation, returnTo);
			Camera_Y.localPosition = Vector3.Lerp(Camera_Y.localPosition, camYPos, returnTo);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, originalLocalPosition, returnTo);
			returnTo += Time.deltaTime / 1.5f;
			if (!freeCam)
			{
				def.focus.fStops = Mathf.Lerp(def.focus.fStops, 32f, returnTo);
				continue;
			}
			def.focus.fStops = Mathf.Lerp(def.focus.fStops, 5.2f, returnTo);
			def.focus.farPlane = Mathf.Lerp(def.focus.farPlane, farPlaneNo, returnTo);
			def.focus.nearPlane = Mathf.Lerp(def.focus.nearPlane, 0.05f, returnTo);
		}
		if (!freeCam)
		{
			def.enabled = false;
		}
		Camera_Y.localRotation = camYRot;
		Camera_Y.localPosition = camYPos;
		conversationCam = false;
	}

	public void startFishing()
	{
		distanceLock = true;
		savedCamDistance = camDistance;
		StartCoroutine("fishingZoomIn");
	}

	public void stopFishing()
	{
		if (distanceLock)
		{
			camDistance = savedCamDistance;
			distanceLock = false;
		}
	}

	public void zoomInOnAnimalHouse()
	{
		if (!distanceLock)
		{
			savedCamDistance = camDistance;
			distanceLock = true;
		}
		camDistance = 6f;
		cameraTrans.localPosition = Vector3.up * 9f - Vector3.forward * 9f;
	}

	public void stopZoomInOnAnimalHouse()
	{
		if (distanceLock)
		{
			camDistance = savedCamDistance;
			distanceLock = false;
		}
	}

	private IEnumerator fishingZoomIn()
	{
		while (distanceLock && Vector3.Distance(base.transform.position, followTransform.position) > 6f)
		{
			yield return null;
		}
		if (distanceLock)
		{
			camDistance = 5f;
		}
	}

	public void showOffPos(int xPos, int yPos)
	{
		StartCoroutine(moveCameraToShowPos(xPos, yPos));
	}

	public void moveCameraPointerToTileObject(int xPos, int yPos)
	{
		int num = WorldManager.manageWorld.rotationMap[xPos, yPos];
		TileObject tileObject = WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]];
		switch (num)
		{
		case 1:
			base.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
			FarmAnimalMenu.menu.selectorTrans.position = new Vector3(xPos * 2 + 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2 + 2);
			break;
		case 2:
			base.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
			FarmAnimalMenu.menu.selectorTrans.position = new Vector3(xPos * 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2 + tileObject.getXSize());
			break;
		case 3:
			base.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
			FarmAnimalMenu.menu.selectorTrans.position = new Vector3(xPos * 2 + tileObject.getXSize(), WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2 + tileObject.getYSize());
			break;
		case 4:
			base.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
			FarmAnimalMenu.menu.selectorTrans.position = new Vector3(xPos * 2 + tileObject.getYSize() + 2, WorldManager.manageWorld.heightMap[xPos, yPos], yPos * 2 + 2);
			break;
		}
	}

	public void checkCameraForCollisions()
	{
		Vector3 vector = -mainCamera.transform.forward;
		Debug.DrawLine(Camera_Y.position, Camera_Y.position + vector * maxDistance, Color.blue);
		if (Physics.Linecast(Camera_Y.position, Camera_Y.position + vector * maxDistance, out cameraHit, cameraCollisions))
		{
			distance = Mathf.Clamp(cameraHit.distance * 0.87f, 0.5f, maxDistance);
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Camera_Y.InverseTransformDirection(vector * distance), Time.deltaTime * maxSpeed * 2f);
		}
		else
		{
			distance = maxDistance;
			cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Camera_Y.InverseTransformDirection(vector * distance), Time.deltaTime * maxSpeed);
		}
	}

	public void checkCameraForCollisionsOld()
	{
		Vector3 normalized = (Vector3.up / 1.5f - Camera_Y.forward).normalized;
		Debug.DrawLine(Camera_Y.position, Camera_Y.position + normalized * maxDistance, Color.blue);
		if (Physics.Linecast(Camera_Y.position, Camera_Y.position + normalized * maxDistance, out cameraHit, cameraCollisions))
		{
			distance = Mathf.Clamp(cameraHit.distance * 0.87f, 0.5f, maxDistance);
		}
		else
		{
			distance = maxDistance;
		}
		cameraTrans.localPosition = Vector3.Lerp(cameraTrans.localPosition, Camera_Y.InverseTransformDirection(normalized * distance), Time.deltaTime * 15f);
	}

	private IEnumerator moveCameraToShowPos(int xpos, int yPos)
	{
		if (flyCamOn)
		{
			yield break;
		}
		while (!NetworkMapSharer.share.nextDayIsReady)
		{
			yield return null;
		}
		cameraShowingSomething = true;
		bool isFreeCam = freeCam;
		Quaternion startingRot = base.transform.rotation;
		float startingZoom = camDistance;
		if (isFreeCam)
		{
			swapFreeCam();
		}
		if (WeatherManager.manage.isInside())
		{
			RealWorldTimeLight.time.goOutside();
		}
		NetworkMapSharer.share.localChar.attackLockOn(true);
		bool isKinimeaticNow = NetworkMapSharer.share.localChar.GetComponent<Rigidbody>().isKinematic;
		NetworkMapSharer.share.localChar.GetComponent<Rigidbody>().isKinematic = true;
		moveCameraPointerToTileObject(xpos, yPos);
		setFollowTransform(FarmAnimalMenu.menu.selectorTrans);
		base.transform.position = followTransform.position;
		NewChunkLoader.loader.forceInstantUpdateAtPos();
		float timer = 4f;
		camDistance = 12f;
		while (CharLevelManager.manage.levelUpWindowOpen)
		{
			yield return null;
		}
		if (WorldManager.manageWorld.onTileStatusMap[xpos, yPos] > 0)
		{
			if (NPCManager.manage.npcStatus[WorldManager.manageWorld.onTileStatusMap[xpos, yPos] - 1].hasMet)
			{
				NotificationManager.manage.makeTopNotification(NPCManager.manage.NPCDetails[WorldManager.manageWorld.onTileStatusMap[xpos, yPos] - 1].NPCName + " is visiting the island!");
			}
			else
			{
				NotificationManager.manage.makeTopNotification("Someone is visiting the island!");
			}
		}
		else
		{
			NotificationManager.manage.makeTopNotification("No one is visiting today...");
		}
		while (timer > 0f)
		{
			base.transform.Rotate(Vector3.up, 0.05f);
			camDistance += 0.01f;
			timer -= Time.deltaTime;
			yield return null;
		}
		if (WeatherManager.manage.isInside())
		{
			RealWorldTimeLight.time.goInside();
		}
		setFollowTransform(NetworkMapSharer.share.localChar.transform);
		NetworkMapSharer.share.localChar.attackLockOn(false);
		control.moveToFollowing(true);
		NewChunkLoader.loader.forceInstantUpdateAtPos();
		yield return null;
		NetworkMapSharer.share.localChar.GetComponent<Rigidbody>().isKinematic = isKinimeaticNow;
		base.transform.rotation = startingRot;
		camDistance = startingZoom;
		cameraTrans.localPosition = Vector3.up * camDistance - Vector3.forward * camDistance;
		if (isFreeCam)
		{
			swapFreeCam();
		}
		cameraShowingSomething = false;
	}

	public void lockCamera(bool locked)
	{
		lockCameraForTransition = locked;
	}

	private IEnumerator playAmbientSounds()
	{
		while (Inventory.inv.menuOpen)
		{
			yield return null;
		}
		while (true)
		{
			if ((bool)RealWorldTimeLight.time && !RealWorldTimeLight.time.underGround)
			{
				if (base.transform.position.y < -5f)
				{
					riverNoiseAudio.volume = 0f;
					oceanNoiseAudio.volume = 0f;
				}
				else
				{
					if ((bool)followingChar && followingChar.underWater)
					{
						riverNoiseAudio.pitch = Mathf.Lerp(riverNoiseAudio.pitch, 0.1f, Time.deltaTime * 3f);
						oceanNoiseAudio.pitch = Mathf.Lerp(riverNoiseAudio.pitch, 0.1f, Time.deltaTime * 3f);
						landAmbientAudio.pitch = Mathf.Lerp(riverNoiseAudio.pitch, 0.1f, Time.deltaTime * 3f);
					}
					else
					{
						riverNoiseAudio.pitch = Mathf.Lerp(riverNoiseAudio.pitch, 0.8f, Time.deltaTime * 3f);
						oceanNoiseAudio.pitch = Mathf.Lerp(riverNoiseAudio.pitch, 0.8f, Time.deltaTime * 3f);
						landAmbientAudio.pitch = Mathf.Lerp(riverNoiseAudio.pitch, 1f, Time.deltaTime * 3f);
					}
					if ((float)NewChunkLoader.loader.riverTilesInCharChunk >= 45f)
					{
						riverNoiseAudio.volume = Mathf.Lerp(riverNoiseAudio.volume, 0.07f * Mathf.Clamp(NewChunkLoader.loader.waterTilesNearChar, 0f, 500f) / 500f * SoundManager.manage.getSoundVolume(), Time.deltaTime * 3f);
					}
					else
					{
						riverNoiseAudio.volume = Mathf.Lerp(riverNoiseAudio.volume, 0.035f * Mathf.Clamp(NewChunkLoader.loader.waterTilesNearChar, 0f, 500f) / 500f * SoundManager.manage.getSoundVolume(), Time.deltaTime * 3f);
					}
					oceanNoiseAudio.volume = Mathf.Lerp(oceanNoiseAudio.volume, 0.1f * Mathf.Clamp(NewChunkLoader.loader.oceanTilesNearChar, 0f, 800f) / 800f * SoundManager.manage.getSoundVolume(), Time.deltaTime * 3f);
					landAmbientAudio.volume = Mathf.Lerp(landAmbientAudio.volume, landAmbienceMax * (1f - Mathf.Clamp(NewChunkLoader.loader.oceanTilesNearChar + NewChunkLoader.loader.waterTilesNearChar, 0f, 1000f) / 1000f) * SoundManager.manage.getSoundVolume(), Time.deltaTime * 3f);
				}
			}
			else
			{
				landAmbientAudio.volume = Mathf.Lerp(landAmbientAudio.volume, landAmbienceMax * SoundManager.manage.getSoundVolume(), Time.deltaTime * 3f);
				riverNoiseAudio.volume = 0f;
				oceanNoiseAudio.volume = 0f;
			}
			yield return null;
		}
	}

	private IEnumerator cameraUnderWaterEffects()
	{
		Fisheye wobble = mainCamera.GetComponent<Fisheye>();
		while (true)
		{
			if (!WeatherManager.manage.isInside() && cameraTrans.position.y <= 1f && WorldManager.manageWorld.waterMap[Mathf.RoundToInt(cameraTrans.position.x / 2f), Mathf.RoundToInt(cameraTrans.position.z / 2f)])
			{
				waterEffect.gameObject.SetActive(true);
				wobble.enabled = true;
				waterUnderSide.SetActive(true);
				float randomWobbleX = 0f;
				float randomWobbleY = 0f;
				while (!WeatherManager.manage.isInside() && cameraTrans.position.y <= 1f && WorldManager.manageWorld.waterMap[Mathf.RoundToInt(cameraTrans.position.x / 2f), Mathf.RoundToInt(cameraTrans.position.z / 2f)])
				{
					yield return null;
					Vector3 position = mainCamera.transform.position + mainCamera.transform.forward * 0.3f;
					position.y = 0.61f;
					Vector2 vector = mainCamera.WorldToScreenPoint(position);
					waterEffect.sizeDelta = new Vector2(waterEffect.sizeDelta.x, vector.y / (float)Screen.height * waterCanvas.sizeDelta.y);
					wobble.strengthX = Mathf.Lerp(wobble.strengthX, randomWobbleX, Time.deltaTime);
					wobble.strengthY = Mathf.Lerp(wobble.strengthY, randomWobbleY, Time.deltaTime);
					if (Mathf.Abs(wobble.strengthX - randomWobbleX) <= 0.01f)
					{
						randomWobbleX = Random.Range(0.05f, 0.35f);
					}
					if (Mathf.Abs(wobble.strengthY - randomWobbleY) <= 0.01f)
					{
						randomWobbleY = Random.Range(0.05f, 0.35f);
					}
				}
				wobble.enabled = false;
				waterEffect.gameObject.SetActive(false);
				waterUnderSide.SetActive(false);
			}
			yield return null;
		}
	}
}
