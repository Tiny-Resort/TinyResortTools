using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class PhotoManager : MonoBehaviour
{
	public enum photographObjects
	{
		Animal = 0,
		NPC = 1,
		Player = 2,
		Carryable = 3,
		Location = 4
	}

	public static PhotoManager manage;

	public GameObject cameraWindow;

	private AudioSource cameraZoomSound;

	public Camera photoCamera;

	private int resWidth = 640;

	private int resHeight = 512;

	public RawImage showPhotoPos;

	public Image takePhotoFlashEffect;

	private RenderTexture photoTexture;

	public bool cameraViewOpen;

	public bool photoTabOpen;

	public GlobalFog photoFog;

	public RawImage previousPhotoFrame;

	public List<PhotoDetails> savedPhotos = new List<PhotoDetails>();

	public PhotoDetails[] displayedPhotos;

	public Transform photoTabWindow;

	public Transform previewPhotoWindows;

	public List<InvPhoto> invPhotoFrames = new List<InvPhoto>();

	public InvPhoto blownUpPhoto;

	public LayerMask myPhotosInterest;

	public RectTransform photoFrameMovement;

	public GameObject showPhotoButton;

	public Conversation noPhotoGiven;

	[Header("Photo Journal Tab----------")]
	public InvButton tabCloseButton;

	public GameObject photoPreviewPrefab;

	public Transform photoButtonSpawnPos;

	public GameObject blownUpWindow;

	public TextMeshProUGUI blownUpPhotoText;

	public TMP_InputField photoNameField;

	public TextMeshProUGUI photoNamePlaceHolder;

	[Header("Give Photo things----------")]
	public Animator animToDisableOnGive;

	public GameObject[] objectsToHideOnGive;

	public Transform windowToMove;

	public Transform moveToWhenGive;

	public Transform moveBackAfterGive;

	private bool givingToNpc;

	private PostOnBoard givingPost;

	private int placingInFrameNo = -1;

	public bool canMoveCam = true;

	private int showingPhotosFrom;

	private int showingBlowUp;

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
		displayedPhotos = new PhotoDetails[MuseumManager.manage.paintingsOnDisplay.Length];
		RenderTexture renderTexture = new RenderTexture(resWidth, resHeight, 24);
		photoCamera.targetTexture = renderTexture;
		showPhotoPos.texture = renderTexture;
		photoFog = photoCamera.GetComponent<GlobalFog>();
		cameraZoomSound = photoCamera.GetComponent<AudioSource>();
		resetMaterialsToSeeThrough();
	}

	public void resetMaterialsToSeeThrough()
	{
		for (int i = 0; i < WeatherManager.manage.windyMaterials.Length; i++)
		{
			if (i != 4 && i != 1)
			{
				WeatherManager.manage.windyMaterials[i].SetFloat("_MaxDistance", 6f);
			}
		}
	}

	public void openCameraView()
	{
		takePhotoFlashEffect.enabled = false;
		cameraViewOpen = true;
		cameraWindow.SetActive(cameraViewOpen);
		photoCamera.gameObject.SetActive(cameraViewOpen);
		CameraController.control.mainCamera.gameObject.SetActive(!cameraViewOpen);
		showPhotoPos.gameObject.SetActive(cameraViewOpen);
		StartCoroutine(cameraOpen());
	}

	public void closeCameraView()
	{
		takePhotoFlashEffect.enabled = false;
		cameraViewOpen = false;
		cameraWindow.SetActive(cameraViewOpen);
		photoCamera.gameObject.SetActive(cameraViewOpen);
		CameraController.control.mainCamera.gameObject.SetActive(!cameraViewOpen);
		showPhotoPos.gameObject.SetActive(cameraViewOpen);
	}

	public bool isGivingToNPC()
	{
		return givingToNpc;
	}

	public void openPhotoTab(bool showingNPC = false, int selectingForFrame = -1)
	{
		placingInFrameNo = selectingForFrame;
		givingToNpc = showingNPC;
		if (showingNPC)
		{
			hideWindowOnGive();
			MenuButtonsTop.menu.subMenuButtonsWindow.gameObject.SetActive(false);
			showPhotoButton.gameObject.SetActive(true);
			if (selectingForFrame == -1)
			{
				givingPost = BulletinBoard.board.checkMissionsCompletedForNPC(ConversationManager.manage.lastTalkTo.GetComponent<NPCIdentity>().NPCNo);
				GiveNPC.give.givingPost = givingPost;
			}
		}
		else
		{
			showPhotoButton.gameObject.SetActive(false);
			givingPost = null;
		}
		photoTabOpen = true;
		photoTabWindow.gameObject.SetActive(photoTabOpen);
		closeBlownUpWindow();
	}

	public void populatePhotoButtons()
	{
		for (int i = 0; i < savedPhotos.Count; i++)
		{
			InvPhoto component = Object.Instantiate(photoPreviewPrefab, photoButtonSpawnPos).GetComponent<InvPhoto>();
			component.fillPhotoImage(loadPhoto(savedPhotos[i].photoName), i);
			invPhotoFrames.Add(component);
			component.transform.SetAsFirstSibling();
		}
	}

	public void createNewButtonForPhoto()
	{
		InvPhoto component = Object.Instantiate(photoPreviewPrefab, photoButtonSpawnPos).GetComponent<InvPhoto>();
		component.fillPhotoImage(loadPhoto(savedPhotos[savedPhotos.Count - 1].photoName), savedPhotos.Count - 1);
		invPhotoFrames.Add(component);
		component.transform.SetAsFirstSibling();
	}

	public void giveButtonPress()
	{
		if (placingInFrameNo == -1)
		{
			if (givingPost.checkIfPhotoShownIsCorrect(savedPhotos[showingBlowUp]))
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, givingPost.getPostPostsById().onGivenItem);
			}
			else
			{
				ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, givingPost.getPostPostsById().onGivenWrongPhoto);
			}
			givingPost = null;
		}
		else
		{
			MuseumManager.manage.donatePhoto(placingInFrameNo, showingBlowUp);
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, ConversationManager.manage.lastTalkTo.getVendorConversation().GetComponent<MuseumConvoGroup>().thankForPhoto);
		}
		closeWhenGive();
		showPhotoButton.gameObject.SetActive(false);
		closePhotoTab();
		MenuButtonsTop.menu.closeSubMenu();
	}

	public void closeWhenGive()
	{
		if (givingToNpc)
		{
			givingToNpc = false;
			windowToMove.SetParent(moveBackAfterGive);
			windowToMove.localPosition = Vector2.zero;
			moveToWhenGive.gameObject.SetActive(false);
		}
	}

	public void hideWindowOnGive()
	{
		windowToMove.SetParent(moveToWhenGive);
		windowToMove.localPosition = Vector2.zero;
		moveToWhenGive.gameObject.SetActive(true);
	}

	public void letNPCKeepPhoto()
	{
		savedPhotos.RemoveAt(showingBlowUp);
	}

	public void closePhotoTab()
	{
		if (givingToNpc)
		{
			ConversationManager.manage.talkToNPC(ConversationManager.manage.lastTalkTo, noPhotoGiven);
			closeWhenGive();
			givingPost = null;
			showPhotoButton.gameObject.SetActive(false);
		}
		photoTabOpen = false;
		closeBlownUpWindow();
		photoTabWindow.gameObject.SetActive(photoTabOpen);
		blownUpWindow.gameObject.SetActive(false);
	}

	public static string ScreenShotName(int width, int height, string name)
	{
		SaveLoad.saveOrLoad.createPhotoDir();
		return string.Format("{0}/Photos/{1}.png", SaveLoad.saveOrLoad.saveSlot(), name);
	}

	public string getPhotoDateAndTime()
	{
		return WorldManager.manageWorld.year + "." + WorldManager.manageWorld.month + "." + WorldManager.manageWorld.week + "." + WorldManager.manageWorld.day + "." + RealWorldTimeLight.time.currentHour + "." + RealWorldTimeLight.time.currentMinute;
	}

	private void waitAndReactivateViewfinder()
	{
		photoCamera.targetTexture = photoTexture;
		showPhotoPos.texture = photoTexture;
		RenderTexture.active = photoTexture;
		photoCamera.enabled = false;
		photoCamera.enabled = true;
		canMoveCam = true;
	}

	private IEnumerator cameraOpen()
	{
		float yRot = 10f;
		float zoom = 0f;
		float desiredZoom = 0f;
		for (int i = 0; i < WeatherManager.manage.windyMaterials.Length; i++)
		{
			if (i != 4)
			{
				WeatherManager.manage.windyMaterials[i].SetFloat("_MaxDistance", 0f);
			}
		}
		while (cameraViewOpen)
		{
			if (InputMaster.input.Use())
			{
				takePhoto();
				canMoveCam = false;
				yield return StartCoroutine(takePhotoEffects());
				waitAndReactivateViewfinder();
			}
			if (canMoveCam)
			{
				float num = 0f - InputMaster.input.getRightStick().y;
				float num2 = 0f - InputMaster.input.getMousePosOld().y;
				if (InputMaster.input.RBHeld())
				{
					desiredZoom = Mathf.Clamp(desiredZoom + 50f * Time.deltaTime, 0f, 60f);
				}
				if (InputMaster.input.LBHeld())
				{
					desiredZoom = Mathf.Clamp(desiredZoom - 50f * Time.deltaTime, 0f, 60f);
				}
				desiredZoom = Mathf.Clamp(desiredZoom + InputMaster.input.getScrollWheel() * Time.deltaTime, 0f, 60f);
				zoom = Mathf.Lerp(zoom, desiredZoom, Time.deltaTime * 10f);
				if (Inventory.inv.usingMouse)
				{
					num = num2;
				}
				yRot = Mathf.Clamp(yRot + num, -25f, 25f);
				photoCamera.fieldOfView = 70f - zoom;
				photoCamera.transform.position = new Vector3(CameraController.control.transform.position.x, NetworkMapSharer.share.localChar.myEquip.holdPos.position.y, CameraController.control.transform.position.z) + Vector3.up * 0.85f + CameraController.control.transform.forward * 1.75f;
				photoCamera.transform.rotation = CameraController.control.transform.rotation * Quaternion.Euler(yRot, 0f, 0f);
				if (Mathf.Abs(desiredZoom - zoom) > 1.5f)
				{
					AudioSource audioSource = cameraZoomSound;
					float volume = (cameraZoomSound.volume = Mathf.Lerp(0.5f * SoundManager.manage.getSoundVolume(), 0f, Time.deltaTime * 50f));
					audioSource.volume = volume;
					cameraZoomSound.pitch = Mathf.Clamp(zoom / 30f + 2f, 2f, 4f);
					cameraZoomSound.Play();
				}
				else
				{
					cameraZoomSound.volume = Mathf.Lerp(cameraZoomSound.volume, 0f, Time.deltaTime * 10f);
					if (cameraZoomSound.volume <= 0.05f)
					{
						cameraZoomSound.Stop();
					}
				}
			}
			float y = InputMaster.input.getLeftStick().y;
			float x = InputMaster.input.getLeftStick().x;
			if (y == 0f && x == 0f)
			{
				NetworkMapSharer.share.localChar.transform.rotation = Quaternion.Lerp(NetworkMapSharer.share.localChar.transform.rotation, CameraController.control.transform.rotation, Time.deltaTime * 3f);
			}
			yield return null;
		}
		resetMaterialsToSeeThrough();
		cameraZoomSound.Stop();
	}

	public bool isGameObjectVisible(GameObject target)
	{
		if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(photoCamera), target.GetComponentInChildren<Renderer>().bounds))
		{
			return true;
		}
		return false;
	}

	public List<PhotographedObject> getPhotosContents()
	{
		List<PhotographedObject> list = new List<PhotographedObject>();
		List<Transform> list2 = new List<Transform>();
		RaycastHit hitInfo;
		if (Physics.Raycast(photoCamera.transform.position, photoCamera.transform.forward, out hitInfo, 12f) && !list2.Contains(hitInfo.transform.root) && isGameObjectVisible(hitInfo.transform.root.gameObject))
		{
			list2.Add(hitInfo.transform.root);
			checkTransform(hitInfo.transform.root, list);
		}
		Collider[] array = Physics.OverlapSphere(photoCamera.transform.position + photoCamera.transform.forward * 25.5f, 25f, myPhotosInterest);
		for (int i = 0; i < array.Length; i++)
		{
			if (!list2.Contains(array[i].transform.root) && isGameObjectVisible(array[i].transform.root.gameObject))
			{
				list2.Add(array[i].transform.root);
				checkTransform(array[i].transform.root, list);
			}
		}
		return list;
	}

	public void takePhoto()
	{
		photoTexture = new RenderTexture(resWidth, resHeight, 24);
		photoCamera.targetTexture = photoTexture;
		Texture2D texture2D = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
		photoCamera.Render();
		RenderTexture.active = photoTexture;
		texture2D.ReadPixels(new Rect(0f, 0f, resWidth, resHeight), 0, 0);
		string photoDateAndTime = getPhotoDateAndTime();
		PhotoDetails photoDetails = new PhotoDetails(photoDateAndTime, getPhotosContents(), NetworkMapSharer.share.localChar.transform.position, 1);
		savedPhotos.Add(photoDetails);
		byte[] bytes = texture2D.EncodeToJPG();
		File.WriteAllBytes(ScreenShotName(resWidth, resHeight, photoDateAndTime), bytes);
		BulletinBoard.board.checkAllMissionsForPhotoRequestsOnNewPhoto(photoDetails);
		createNewButtonForPhoto();
	}

	private IEnumerator takePhotoEffects()
	{
		float progress = 0f;
		takePhotoFlashEffect.enabled = true;
		takePhotoFlashEffect.color = Color.white;
		while (progress <= 1f)
		{
			takePhotoFlashEffect.color = Color.Lerp(Color.white, Color.clear, progress);
			progress += Time.deltaTime;
			yield return null;
		}
		takePhotoFlashEffect.enabled = false;
	}

	public Texture2D loadPhoto(string photoName)
	{
		Texture2D texture2D = null;
		if (File.Exists(ScreenShotName(resWidth, resHeight, photoName)))
		{
			byte[] data = File.ReadAllBytes(ScreenShotName(resWidth, resHeight, photoName));
			texture2D = new Texture2D(resWidth, resHeight);
			texture2D.LoadImage(data);
			previousPhotoFrame.texture = texture2D;
		}
		return texture2D;
	}

	public Texture2D loadPhotoFromByteArray(byte[] array)
	{
		Texture2D texture2D = new Texture2D(resWidth, resHeight);
		texture2D.LoadImage(array);
		return texture2D;
	}

	public byte[] getByteArrayForTransfer(string photoName)
	{
		if (File.Exists(ScreenShotName(resWidth, resHeight, photoName)))
		{
			return File.ReadAllBytes(ScreenShotName(resWidth, resHeight, photoName));
		}
		return null;
	}

	public void setPhotoTabCloseButtonAsLastCloseButton()
	{
		Inventory.inv.setAsLastActiveCloseButton(tabCloseButton);
	}

	public void blowUpImage(int frameNo)
	{
		photoButtonSpawnPos.gameObject.SetActive(false);
		showingBlowUp = frameNo;
		blownUpPhoto.fillPhotoImage(loadPhoto(savedPhotos[showingBlowUp].photoName), showingBlowUp);
		blownUpWindow.SetActive(true);
		blownUpPhotoText.text = manage.savedPhotos[frameNo].getIslandName() + "\n";
		TextMeshProUGUI textMeshProUGUI = blownUpPhotoText;
		textMeshProUGUI.text = textMeshProUGUI.text + manage.savedPhotos[frameNo].getDateString() + "\n" + manage.savedPhotos[frameNo].getTimeString();
		if (savedPhotos[showingBlowUp].photoNickname == "" || savedPhotos[showingBlowUp].photoNickname == null)
		{
			savedPhotos[showingBlowUp].photoNickname = "Untitled";
		}
		photoNameField.text = savedPhotos[showingBlowUp].photoNickname;
		photoNamePlaceHolder.text = savedPhotos[showingBlowUp].photoNickname;
	}

	public void renamePhoto()
	{
		savedPhotos[showingBlowUp].photoNickname = photoNameField.text;
		invPhotoFrames[showingBlowUp].updatePhotoId(showingBlowUp);
	}

	public void closeBlownUpWindow()
	{
		setPhotoTabCloseButtonAsLastCloseButton();
		photoButtonSpawnPos.gameObject.SetActive(true);
		blownUpWindow.SetActive(false);
	}

	public void closeBlownUpWindowAndSaveName()
	{
		renamePhoto();
		closeBlownUpWindow();
	}

	public void deleteBlownUpImage()
	{
		savedPhotos.RemoveAt(showingBlowUp);
		Object.Destroy(invPhotoFrames[showingBlowUp].gameObject);
		invPhotoFrames.RemoveAt(showingBlowUp);
		for (int i = 0; i < invPhotoFrames.Count; i++)
		{
			invPhotoFrames[i].updatePhotoId(i);
		}
		closeBlownUpWindow();
	}

	public void donatePhoto(int photoId)
	{
		savedPhotos.RemoveAt(photoId);
		Object.Destroy(invPhotoFrames[photoId].gameObject);
		invPhotoFrames.RemoveAt(photoId);
		for (int i = 0; i < invPhotoFrames.Count; i++)
		{
			invPhotoFrames[i].updatePhotoId(i);
		}
	}

	public void checkTransform(Transform transformToCheck, List<PhotographedObject> inPicture)
	{
		AnimalAI componentInParent = transformToCheck.GetComponentInParent<AnimalAI>();
		if ((bool)componentInParent)
		{
			inPicture.Add(new PhotographedObject(componentInParent));
		}
		NPCIdentity componentInParent2 = transformToCheck.GetComponentInParent<NPCIdentity>();
		if ((bool)componentInParent2)
		{
			inPicture.Add(new PhotographedObject(componentInParent2));
		}
		EquipItemToChar componentInParent3 = transformToCheck.GetComponentInParent<EquipItemToChar>();
		if ((bool)componentInParent3)
		{
			inPicture.Add(new PhotographedObject(componentInParent3));
		}
		PickUpAndCarry componentInParent4 = transformToCheck.GetComponentInParent<PickUpAndCarry>();
		if ((bool)componentInParent4)
		{
			inPicture.Add(new PhotographedObject(componentInParent4));
		}
	}
}
