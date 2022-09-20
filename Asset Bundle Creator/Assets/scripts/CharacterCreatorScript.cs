using System.Collections;
using System.IO;
using TMPro;
using UnityChan;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterCreatorScript : MonoBehaviour
{
	public int hairStyleNo;

	public int hairColourNo;

	public int eyeStyleNo;

	public int eyeColorNo;

	public int skinToneNo = 1;

	public Mesh[] noseMeshes;

	public SkinnedMeshRenderer charRen;

	public MeshRenderer noseRen;

	public Animator charAnim;

	public ParticleSystem hairPart;

	public ParticleSystemRenderer hairPartRen;

	public GameObject createCharacterScreen;

	public Transform onHeadPosition;

	public GameObject[] allHairStyles;

	public Material[] allHairColours;

	public Renderer[] eyeRenderers;

	public Material[] allEyeTypes;

	public Material[] eyeColours;

	public Material[] skinTones;

	public Material[] mouthTypes;

	public InventoryItem[] allShirts;

	public InventoryItem shorts;

	public EyesScript eyes;

	public GameObject hairOnHead;

	public Transform CharacterRotateTransform;

	public Transform myCamera;

	private Camera charCam;

	public Text hairStyleText;

	public TMP_InputField nameBox;

	public TMP_InputField islandNameBox;

	public Text eyeStyleText;

	private int desiredRotation;

	private bool autoSpin = true;

	public ASound confirmSound;

	public ASound backSound;

	private GameObject itemOnHead;

	private GameObject itemOnFace;

	public static CharacterCreatorScript create;

	public RenderTexture charCreatorRenderText;

	private Vector3 orignalPos;

	private Quaternion originalRot;

	private bool isOpen;

	public UnityEvent changeEyeColourEvent = new UnityEvent();

	private Coroutine animateCharOnChangeRoutine;

	public GameObject[] hairButtons;

	public Texture2D noFilePhoto;

	public SkinnedMeshRenderer shirtRen;

	private int shirtNo;

	private int noseNo;

	private int mouthNo;

	private int showingHeadItem = -1;

	private float turnSpeed;

	private float lookWeight;

	private bool takingPhoto;

	private bool playingLookAtHairAnimation;

	private bool playingLookAtShirtAnimation;

	private bool lookingAtCamera;

	private float lookTimer;

	private void Awake()
	{
		create = this;
	}

	private void OnDisable()
	{
		playingLookAtHairAnimation = false;
		lookingAtCamera = false;
		RealWorldTimeLight.time.theSun.gameObject.SetActive(true);
		RealWorldTimeLight.time.theMoon.gameObject.SetActive(true);
	}

	private void OnEnable()
	{
		StartCoroutine(characterLooksAroundRandom());
		RealWorldTimeLight.time.theSun.gameObject.SetActive(false);
		RealWorldTimeLight.time.theMoon.gameObject.SetActive(false);
		randomiseHairButtons();
	}

	public void randomiseHairButtons()
	{
		for (int i = 0; i < hairButtons.Length; i++)
		{
			hairButtons[i].transform.SetSiblingIndex(Random.Range(0, hairButtons.Length));
		}
	}

	private void Start()
	{
		myCamera.parent = null;
		orignalPos = myCamera.position;
		originalRot = myCamera.rotation;
		changeHairColour(hairColourNo);
		equipHeadItem(hairStyleNo);
		createCharacterScreen.SetActive(false);
		base.gameObject.SetActive(false);
		myCamera.gameObject.SetActive(false);
		charCam = myCamera.GetComponent<Camera>();
	}

	public void openCharCreator()
	{
		showingHeadItem = -1;
		createCharacterScreen.SetActive(true);
		base.gameObject.SetActive(true);
		myCamera.gameObject.SetActive(true);
		changeEyeColourOnModel(0);
		equipHeadItem(0);
		changeHairColour(0);
		equipToFace(-1);
		changeSkinTone(1);
		onChangeShirt(-1);
		myCamera.transform.position = orignalPos;
		myCamera.transform.rotation = originalRot;
		isOpen = true;
	}

	public void closeCharCreator()
	{
		createCharacterScreen.SetActive(false);
		base.gameObject.SetActive(false);
		myCamera.gameObject.SetActive(false);
		isOpen = false;
	}

	private void equipHeadItem(int itemNoToEquip, int headNo = -1)
	{
		if (hairOnHead != null)
		{
			Object.Destroy(hairOnHead);
		}
		hairOnHead = Object.Instantiate(allHairStyles[itemNoToEquip], onHeadPosition);
		foreach (Transform item in hairOnHead.transform)
		{
			item.gameObject.layer = 26;
		}
		hairOnHead.transform.localPosition = Vector3.zero;
		hairOnHead.transform.localRotation = Quaternion.Euler(Vector3.zero);
		changeHairColour(hairColourNo);
		if (headNo >= 0)
		{
			if (headNo >= 0 && (bool)Inventory.inv.allItems[headNo].equipable && Inventory.inv.allItems[headNo].equipable.useHelmetHair)
			{
				Object.Destroy(hairOnHead);
				hairOnHead = Object.Instantiate(allHairStyles[0], onHeadPosition);
			}
			if (headNo >= 0 && (bool)Inventory.inv.allItems[headNo].equipable && Inventory.inv.allItems[headNo].equipable.hideHair)
			{
				Object.Destroy(hairOnHead);
			}
			if ((bool)itemOnHead)
			{
				Object.Destroy(itemOnHead);
			}
			if ((bool)hairOnHead)
			{
				if (!Inventory.inv.allItems[headNo].equipable.useRegularHair)
				{
					hairOnHead.transform.Find("Hair").gameObject.SetActive(false);
					hairOnHead.transform.Find("Hair_Hat").gameObject.SetActive(true);
				}
				hairOnHead.transform.localPosition = Vector3.zero;
				hairOnHead.transform.localRotation = Quaternion.Euler(Vector3.zero);
			}
			itemOnHead = Object.Instantiate(Inventory.inv.allItems[headNo].equipable.hatPrefab, onHeadPosition);
			foreach (Transform item2 in itemOnHead.transform)
			{
				item2.gameObject.layer = 26;
			}
			if ((bool)itemOnHead.GetComponent<SetItemTexture>())
			{
				itemOnHead.GetComponent<SetItemTexture>().setTexture(Inventory.inv.allItems[headNo]);
				if ((bool)itemOnHead.GetComponent<SetItemTexture>().changeSize)
				{
					itemOnHead.GetComponent<SetItemTexture>().changeSizeOfTrans(Inventory.inv.allItems[headNo].transform.localScale);
				}
			}
			Light[] componentsInChildren = itemOnHead.GetComponentsInChildren<Light>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			itemOnHead.transform.localPosition = Vector3.zero;
			itemOnHead.transform.localRotation = Quaternion.Euler(Vector3.zero);
		}
		else if ((bool)itemOnHead)
		{
			Object.Destroy(itemOnHead);
		}
	}

	private void equipToFace(int newId)
	{
		if ((bool)itemOnFace)
		{
			Object.Destroy(itemOnFace);
		}
		if (newId <= -1)
		{
			return;
		}
		itemOnFace = Object.Instantiate(Inventory.inv.allItems[newId].equipable.hatPrefab, onHeadPosition);
		foreach (Transform item in itemOnFace.transform)
		{
			item.gameObject.layer = 26;
		}
		itemOnFace.transform.localPosition = Vector3.zero;
		itemOnFace.transform.localRotation = Quaternion.Euler(Vector3.zero);
		if ((bool)itemOnFace.GetComponent<SetItemTexture>())
		{
			itemOnFace.GetComponent<SetItemTexture>().setTexture(Inventory.inv.allItems[newId]);
			if ((bool)itemOnFace.GetComponent<SetItemTexture>().changeSize)
			{
				itemOnFace.GetComponent<SetItemTexture>().changeSizeOfTrans(Inventory.inv.allItems[newId].transform.localScale);
			}
		}
	}

	private void onChangeShirt(int newId)
	{
		if (newId != -1 && (bool)Inventory.inv.allItems[newId].equipable.shirtMesh)
		{
			shirtRen.sharedMesh = Inventory.inv.allItems[newId].equipable.shirtMesh;
		}
		else if (newId == -1)
		{
			shirtRen.sharedMesh = EquipWindow.equip.tShirtMesh;
		}
		else
		{
			shirtRen.sharedMesh = EquipWindow.equip.defaultShirtMesh;
		}
		if (newId == -1)
		{
			shirtRen.material = EquipWindow.equip.underClothes;
		}
		else
		{
			shirtRen.material = Inventory.inv.allItems[newId].equipable.material;
		}
	}

	public void changeEyeStyle(int newEye)
	{
		lookingAtCamera = true;
		lookTimer = 8f;
		eyeStyleNo = newEye;
		changeEyeColourOnModel(eyeStyleNo);
	}

	public void playBackSound()
	{
	}

	public void playNextSound()
	{
	}

	public void playConfirmSound()
	{
	}

	public void changeShirt(int newNo)
	{
		shirtRen.material = allShirts[newNo].equipable.material;
		int num = Random.Range(0, 3);
		playingLookAtHairAnimation = false;
		lookingAtCamera = false;
		lookTimer = 9f;
		playingLookAtShirtAnimation = true;
		Invoke("stopLookatShirt", 2f);
		charAnim.SetTrigger("ChangeShirt");
		switch (num)
		{
		case 0:
			eyes.supriseMouth();
			break;
		case 1:
			eyes.happyMouth();
			break;
		}
		shirtNo = newNo;
	}

	public void stopLookatShirt()
	{
		playingLookAtShirtAnimation = false;
	}

	public void changeEyeColor(int newNo)
	{
		eyeColorNo = newNo;
		changeEyeColourOnModel(eyeStyleNo);
		eyes.changeEyeColor(eyeColours[newNo]);
		changeEyeColourEvent.Invoke();
	}

	public void changeNose(int newNose)
	{
		noseNo = newNose;
		noseRen.GetComponent<MeshFilter>().sharedMesh = noseMeshes[noseNo];
	}

	public void changeMouth(int newMouth)
	{
		mouthNo = newMouth;
		eyes.changeMouthMat(mouthTypes[mouthNo], skinTones[skinToneNo].color);
	}

	private void changeEyeColourOnModel(int newNo)
	{
		if (isOpen)
		{
			switch (Random.Range(0, 3))
			{
			case 0:
				eyes.supriseMouth();
				break;
			case 1:
				eyes.happyMouth();
				break;
			}
		}
		eyes.changeEyeMat(allEyeTypes[newNo], skinTones[skinToneNo].color);
	}

	public Color getHairColour(int id)
	{
		return allHairColours[id].color;
	}

	private void changeHairColour(int newNo)
	{
		hairPartRen.material.color = getHairColour(hairColourNo);
		if (isOpen)
		{
			hairPart.Emit(25);
		}
		StartCoroutine(jumpOnHairChange());
		if ((bool)hairOnHead)
		{
			if ((bool)hairOnHead.GetComponentInChildren<MeshRenderer>())
			{
				hairOnHead.GetComponentInChildren<MeshRenderer>().material.color = getHairColour(newNo);
			}
			if ((bool)hairOnHead.GetComponentInChildren<SkinnedMeshRenderer>())
			{
				hairOnHead.GetComponentInChildren<SkinnedMeshRenderer>().material.color = getHairColour(newNo);
			}
		}
	}

	public void changeHairColourNo(int newNo)
	{
		hairColourNo = newNo;
		changeHairColour(hairColourNo);
	}

	public void changeHairStyle(int changeTo)
	{
		hairStyleNo = changeTo;
		equipHeadItem(hairStyleNo, showingHeadItem);
	}

	public void changeSkinTone(int changeTo)
	{
		skinToneNo = changeTo;
		charRen.material = skinTones[skinToneNo];
		eyes.changeSkinColor(skinTones[skinToneNo].color);
	}

	public void saveCharSetUp()
	{
		Inventory.inv.playerHair = hairStyleNo;
		Inventory.inv.playerHairColour = hairColourNo;
		Inventory.inv.playerEyes = eyeStyleNo;
		Inventory.inv.playerEyeColor = eyeColorNo;
		Inventory.inv.skinTone = skinToneNo;
		Inventory.inv.nose = noseNo;
		Inventory.inv.mouth = mouthNo;
		EquipWindow.equip.pantsSlot.updateSlotContentsAndRefresh(Inventory.inv.getInvItemId(shorts), 1);
		EquipWindow.equip.shirtSlot.updateSlotContentsAndRefresh(Inventory.inv.getInvItemId(allShirts[shirtNo]), 1);
		if (nameBox.text == "")
		{
			Inventory.inv.playerName = "Bazza";
		}
		else
		{
			Inventory.inv.playerName = nameBox.text;
		}
		if (islandNameBox.text == "")
		{
			Inventory.inv.islandName = "Dinkum";
		}
		else
		{
			Inventory.inv.islandName = islandNameBox.text;
		}
		createCharacterScreen.SetActive(false);
		myCamera.gameObject.SetActive(false);
		base.gameObject.SetActive(false);
		TownManager.manage.firstConnect = true;
		DeedManager.manage.unlockStartingDeeds();
		DeedManager.manage.loadDeedIngredients();
		CharLevelManager.manage.recipesAlwaysUnlocked();
	}

	public void Update()
	{
		float num = 0f;
		if (Inventory.inv.usingMouse && InputMaster.input.UISelectHeld())
		{
			num = (0f - InputMaster.input.getMousePosOld().x) * 2f;
		}
		else if (!Inventory.inv.usingMouse)
		{
			num = InputMaster.input.getRightStick().x * 3f;
		}
		float b = 2f * num;
		turnSpeed = Mathf.Lerp(turnSpeed, b, Time.deltaTime * 8f);
		if (takingPhoto)
		{
			eyes.eyeLookAtTrans.localPosition = new Vector3(0f, 0f, 0f);
		}
		else if (!playingLookAtHairAnimation && lookingAtCamera)
		{
			eyes.eyeLookAtTrans.localPosition = eyes.eyeLookAtTrans.InverseTransformDirection((myCamera.transform.position - eyes.eyeLookAtTrans.position).normalized);
		}
		else if (playingLookAtHairAnimation)
		{
			eyes.eyeLookAtTrans.localPosition = new Vector3(0f, -0.75f, 0f);
		}
		else if (playingLookAtShirtAnimation)
		{
			eyes.eyeLookAtTrans.localPosition = new Vector3(0f, 0.75f, 0f);
		}
		else
		{
			eyes.eyeLookAtTrans.localPosition = new Vector3(0f, 0f, 0f);
		}
		if (turnSpeed > 0.01f || turnSpeed < -0.01f)
		{
			CharacterRotateTransform.Rotate(0f, turnSpeed, 0f);
		}
	}

	private void OnAnimatorIK(int layerIndex)
	{
		if (!takingPhoto)
		{
			charAnim.SetLookAtPosition(myCamera.position);
			Vector3 normalized = (myCamera.transform.position - base.transform.position).normalized;
			if (!playingLookAtHairAnimation && lookingAtCamera)
			{
				lookWeight = Mathf.Lerp(lookWeight, Mathf.Clamp01(Vector3.Dot(normalized, base.transform.forward)), Time.deltaTime);
			}
			else
			{
				lookWeight = Mathf.Lerp(lookWeight, Mathf.Clamp(Vector3.Dot(normalized, base.transform.forward), 0f, 0.2f), Time.deltaTime);
			}
			charAnim.SetLookAtWeight(lookWeight);
		}
		else
		{
			charAnim.SetLookAtPosition(myCamera.position);
			Vector3 normalized2 = (myCamera.transform.position - base.transform.position).normalized;
			lookWeight = Mathf.Lerp(lookWeight, Mathf.Clamp01(Vector3.Dot(normalized2, base.transform.forward)), Time.deltaTime);
			charAnim.SetLookAtWeight(0.1f);
		}
	}

	public void rotate(int dif)
	{
		if (autoSpin)
		{
			autoSpin = false;
			desiredRotation = 180;
		}
		else
		{
			desiredRotation -= dif;
		}
	}

	public void setCharForPicture()
	{
		hairPart.gameObject.SetActive(false);
		base.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
		myCamera.position = new Vector3(247.7f, 375.56f, 1112.45f);
		myCamera.eulerAngles = new Vector3(0f, 42.7f, 0f);
		base.gameObject.SetActive(true);
		charCam.gameObject.SetActive(true);
		changeEyeColourOnModel(Inventory.inv.playerEyes);
		showingHeadItem = EquipWindow.equip.hatSlot.itemNo;
		changeHairStyle(Inventory.inv.playerHair);
		if ((bool)hairOnHead.GetComponent<SpringManager>())
		{
			hairOnHead.GetComponent<SpringManager>().enabled = false;
		}
		changeMouth(Inventory.inv.mouth);
		changeNose(Inventory.inv.nose);
		changeSkinTone(Inventory.inv.skinTone);
		equipToFace(EquipWindow.equip.faceSlot.itemNo);
		changeHairColour(Inventory.inv.playerHairColour);
		onChangeShirt(EquipWindow.equip.shirtSlot.itemNo);
		eyes.setEyesOpen();
		myCamera.gameObject.SetActive(true);
		Vector3 position = myCamera.position;
		Quaternion rotation = myCamera.rotation;
	}

	public void setForNPCPicture(int npcId)
	{
		hairPart.gameObject.SetActive(false);
		base.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
		myCamera.position = new Vector3(247.7f, 375.56f, 1112.45f);
		myCamera.eulerAngles = new Vector3(0f, 42.7f, 0f);
		base.gameObject.SetActive(true);
		charCam.gameObject.SetActive(true);
		if (NPCManager.manage.NPCDetails[npcId].isAVillager)
		{
			eyes.changeEyeMat(allEyeTypes[NPCManager.manage.npcInvs[npcId].eyesId], skinTones[NPCManager.manage.npcInvs[npcId].skinId].color);
			eyes.changeEyeColor(eyeColours[NPCManager.manage.npcInvs[npcId].eyeColorId]);
			eyes.mouthInside.GetComponent<MeshFilter>().sharedMesh = NPCManager.manage.defaultInsideMouth;
			eyes.changeMouthMat(mouthTypes[NPCManager.manage.npcInvs[npcId].mouthId], skinTones[NPCManager.manage.npcInvs[npcId].skinId].color);
			changeNose(NPCManager.manage.npcInvs[npcId].noseId);
			eyes.changeSkinColor(skinTones[NPCManager.manage.npcInvs[npcId].skinId].color);
			charRen.material = skinTones[NPCManager.manage.npcInvs[npcId].skinId];
			if (hairOnHead != null)
			{
				Object.Destroy(hairOnHead);
			}
			hairOnHead = Object.Instantiate(allHairStyles[NPCManager.manage.npcInvs[npcId].hairId], onHeadPosition);
			if ((bool)hairOnHead.GetComponentInChildren<MeshRenderer>())
			{
				hairOnHead.GetComponentInChildren<MeshRenderer>().material.color = getHairColour(NPCManager.manage.npcInvs[npcId].hairColorId);
			}
			if ((bool)hairOnHead.GetComponentInChildren<SkinnedMeshRenderer>())
			{
				hairOnHead.GetComponentInChildren<SkinnedMeshRenderer>().material.color = getHairColour(NPCManager.manage.npcInvs[npcId].hairColorId);
			}
			shirtRen.material = Inventory.inv.allItems[NPCManager.manage.npcInvs[npcId].shirtId].equipable.material;
			if ((bool)Inventory.inv.allItems[NPCManager.manage.npcInvs[npcId].shirtId].equipable.shirtMesh)
			{
				shirtRen.sharedMesh = Inventory.inv.allItems[NPCManager.manage.npcInvs[npcId].shirtId].equipable.shirtMesh;
			}
			else
			{
				shirtRen.sharedMesh = EquipWindow.equip.defaultShirtMesh;
			}
			Object.Destroy(itemOnHead);
			Object.Destroy(itemOnFace);
		}
		else
		{
			eyes.changeEyeMat(NPCManager.manage.NPCDetails[npcId].NpcEyes, NPCManager.manage.NPCDetails[npcId].NpcSkin.color);
			eyes.changeEyeColor(NPCManager.manage.NPCDetails[npcId].NpcEyesColor);
			eyes.mouthInside.GetComponent<MeshFilter>().sharedMesh = NPCManager.manage.NPCDetails[npcId].insideMouthMesh;
			eyes.changeMouthMat(NPCManager.manage.NPCDetails[npcId].NPCMouth, NPCManager.manage.NPCDetails[npcId].NpcSkin.color);
			eyes.changeSkinColor(NPCManager.manage.NPCDetails[npcId].NpcSkin.color);
			charRen.material = NPCManager.manage.NPCDetails[npcId].NpcSkin;
			changeNose(NPCManager.manage.NPCDetails[npcId].nose);
			if (hairOnHead != null)
			{
				Object.Destroy(hairOnHead);
			}
			hairOnHead = Object.Instantiate(NPCManager.manage.NPCDetails[npcId].NpcHair, onHeadPosition);
			shirtRen.material = NPCManager.manage.NPCDetails[npcId].NPCShirt.equipable.material;
			if ((bool)NPCManager.manage.NPCDetails[npcId].NPCShirt.equipable.shirtMesh)
			{
				shirtRen.sharedMesh = NPCManager.manage.NPCDetails[npcId].NPCShirt.equipable.shirtMesh;
			}
			else
			{
				shirtRen.sharedMesh = EquipWindow.equip.defaultShirtMesh;
			}
		}
		hairOnHead.transform.localPosition = Vector3.zero;
		hairOnHead.transform.localRotation = Quaternion.identity;
		Transform[] componentsInChildren = hairOnHead.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 26;
		}
		if ((bool)hairOnHead.GetComponentInChildren<SpringManager>())
		{
			hairOnHead.GetComponentInChildren<SpringManager>().enabled = false;
		}
		if (!NPCManager.manage.NPCDetails[npcId].isAVillager && (bool)NPCManager.manage.NPCDetails[npcId].npcMesh)
		{
			charRen.sharedMesh = NPCManager.manage.NPCDetails[npcId].npcMesh;
		}
		else
		{
			charRen.sharedMesh = NPCManager.manage.defaultNpcMesh;
		}
		eyes.setEyesOpen();
		myCamera.gameObject.SetActive(true);
		Vector3 position = myCamera.position;
		Quaternion rotation = myCamera.rotation;
	}

	public void takeSlotPhotoAndSave()
	{
		base.gameObject.SetActive(true);
		StartCoroutine(photoDelay());
	}

	private IEnumerator photoDelay()
	{
		takingPhoto = true;
		setCharForPicture();
		charAnim.Rebind();
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		eyes.setEyesOpen();
		takeAndSaveSlotPhoto();
		base.gameObject.SetActive(false);
		takingPhoto = false;
	}

	public void takeAndSaveSlotPhoto()
	{
		myCamera.gameObject.SetActive(true);
		int num = 512;
		RenderTexture renderTexture = new RenderTexture(num, num, 32);
		renderTexture.filterMode = FilterMode.Trilinear;
		renderTexture.antiAliasing = 3;
		charCam.targetTexture = renderTexture;
		Texture2D texture2D = new Texture2D(num, num, TextureFormat.ARGB32, false);
		charCam.Render();
		RenderTexture.active = renderTexture;
		texture2D.ReadPixels(new Rect(0f, 0f, num, num), 0, 0);
		byte[] bytes = texture2D.EncodeToPNG();
		File.WriteAllBytes(CharIconName(), bytes);
		myCamera.gameObject.SetActive(false);
	}

	public void takeNPCAndSave(int npcId)
	{
		myCamera.gameObject.SetActive(true);
		int num = 512;
		RenderTexture renderTexture = new RenderTexture(num, num, 32);
		renderTexture.filterMode = FilterMode.Trilinear;
		renderTexture.antiAliasing = 3;
		charCam.targetTexture = renderTexture;
		Texture2D texture2D = new Texture2D(num, num, TextureFormat.ARGB32, false);
		charCam.Render();
		RenderTexture.active = renderTexture;
		texture2D.ReadPixels(new Rect(0f, 0f, num, num), 0, 0);
		byte[] bytes = texture2D.EncodeToPNG();
		File.WriteAllBytes(NPCIconName(npcId), bytes);
		myCamera.gameObject.SetActive(false);
	}

	public void takeNPCPhoto(int npcId)
	{
		base.gameObject.SetActive(true);
		StartCoroutine(npcPhotoDelay(npcId));
	}

	private IEnumerator takeAllNPCName()
	{
		for (int i = 0; i < 12; i++)
		{
			yield return StartCoroutine(npcPhotoDelay(i));
		}
		base.gameObject.SetActive(false);
	}

	private IEnumerator npcPhotoDelay(int npcId)
	{
		takingPhoto = true;
		setForNPCPicture(npcId);
		charAnim.Rebind();
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		eyes.setEyesOpen();
		takeNPCAndSave(npcId);
		takingPhoto = false;
	}

	public Texture2D loadSlotPhoto()
	{
		Texture2D texture2D = null;
		if (File.Exists(CharIconName()))
		{
			byte[] data = File.ReadAllBytes(CharIconName());
			texture2D = new Texture2D(512, 512);
			texture2D.LoadImage(data);
			texture2D.wrapMode = TextureWrapMode.Clamp;
			return texture2D;
		}
		MonoBehaviour.print("Couldn't find the file for the saved photo. Maybe delete it from the list of photos saved?");
		return noFilePhoto;
	}

	public Sprite loadNPCPhoto(int npcId)
	{
		Texture2D texture2D = null;
		if (File.Exists(NPCIconName(npcId)))
		{
			byte[] data = File.ReadAllBytes(NPCIconName(npcId));
			texture2D = new Texture2D(512, 512);
			texture2D.LoadImage(data);
			texture2D.wrapMode = TextureWrapMode.Clamp;
			return Sprite.Create(texture2D, new Rect(0f, 0f, 512f, 512f), new Vector2(0.5f, 0.5f));
		}
		MonoBehaviour.print("Couldn't find the file for the saved photo. Maybe delete it from the list of photos saved?");
		return null;
	}

	public static string CharIconName()
	{
		return string.Format("{0}/SaveIcon.png", SaveLoad.saveOrLoad.saveSlot());
	}

	public static string NPCIconName(int npcId)
	{
		return string.Format("{0}/npc{1}Image.png", SaveLoad.saveOrLoad.saveSlot(), npcId);
	}

	private IEnumerator characterLooksAroundRandom()
	{
		while (true)
		{
			if (lookTimer > 0f)
			{
				yield return null;
				lookTimer -= Time.deltaTime;
			}
			else
			{
				lookingAtCamera = !lookingAtCamera;
				lookTimer = Random.Range(8f, 12f);
				yield return null;
			}
		}
	}

	private IEnumerator jumpOnHairChange()
	{
		if (isOpen && !playingLookAtHairAnimation)
		{
			playingLookAtHairAnimation = true;
			lookingAtCamera = false;
			lookTimer = 9f;
			charAnim.SetTrigger("ChangeClothes");
			switch (Random.Range(0, 3))
			{
			case 0:
				eyes.supriseMouth();
				break;
			case 1:
				eyes.happyMouth();
				break;
			}
			yield return new WaitForSeconds(1.8f);
		}
		playingLookAtHairAnimation = false;
	}

	public void randomiseCharacter()
	{
		int num = hairStyleNo;
		while (num == hairStyleNo)
		{
			changeHairStyle(Random.Range(0, 20));
		}
		int num2 = hairColourNo;
		while (num2 == hairColourNo)
		{
			changeHairColourNo(Random.Range(0, 6));
		}
		int num3 = eyeStyleNo;
		while (num3 == eyeStyleNo)
		{
			changeEyeStyle(Random.Range(0, 17));
		}
		int num4 = eyeColorNo;
		while (num4 == eyeColorNo)
		{
			changeEyeColor(Random.Range(0, 5));
		}
		int num5 = skinToneNo;
		while (num5 == skinToneNo)
		{
			changeSkinTone(Random.Range(0, 6));
		}
		int num6 = shirtNo;
		while (num6 == shirtNo)
		{
			changeShirt(Random.Range(0, allShirts.Length));
		}
		int num7 = noseNo;
		while (num7 == noseNo)
		{
			changeNose(Random.Range(0, noseMeshes.Length));
		}
		int num8 = mouthNo;
		while (num8 == mouthNo)
		{
			changeMouth(Random.Range(0, mouthTypes.Length));
		}
	}
}
