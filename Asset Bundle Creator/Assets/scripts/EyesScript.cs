using System.Collections;
using UnityEngine;

public class EyesScript : MonoBehaviour
{
	public enum emotion
	{
		None = 0,
		Laughing = 1,
		Crying = 2,
		Thinking = 3,
		Glee = 4,
		Shocked = 5,
		Worried = 6,
		Proud = 7,
		Shy = 8,
		Sigh = 9,
		Question = 10,
		Pumped = 11
	}

	public Transform eyeLookAtTrans;

	public MeshRenderer eyeInside;

	public SkinnedMeshRenderer skinnedRen;

	private Material eyeMat;

	private Material eyeLid;

	private Material mouthMat;

	private Material mouthInsideMat;

	private Material noseMat;

	private Vector2 eyeOffset = Vector2.zero;

	private Coroutine currentBlink;

	private Coroutine currentEmotion;

	public MeshRenderer mouthInside;

	public MeshRenderer noseMesh;

	private emotion playingCurrentEmotion;

	private bool showingEmotion;

	public Transform HeadPos;

	private CharMovement myChar;

	private bool tired;

	public EmotionEffects effects;

	public Texture supriseEyes;

	private WaitForSeconds breathSeconds = new WaitForSeconds(1.5f);

	private Coroutine sayingWord;

	public FaceItemMoveWithMouth faceMove;

	public void Start()
	{
		myChar = GetComponentInParent<CharMovement>();
	}

	public void setMouthMesh(Mesh newMesh)
	{
		if ((bool)newMesh)
		{
			mouthInside.GetComponent<MeshFilter>().mesh = newMesh;
		}
		else
		{
			mouthInside.GetComponent<MeshFilter>().mesh = NPCManager.manage.defaultInsideMouth;
		}
	}

	private void Update()
	{
		eyeOffset = Vector2.Lerp(eyeOffset, new Vector2(Mathf.Clamp(eyeLookAtTrans.localPosition.x, -0.15f, 0.15f), Mathf.Clamp(eyeLookAtTrans.localPosition.y, -0.15f, 0.15f)), Time.deltaTime * 20f);
		eyeMat.SetTextureOffset("_MainTex", eyeOffset);
	}

	private void OnEnable()
	{
		tired = false;
		if (!eyeMat)
		{
			eyeMat = eyeInside.material;
			eyeLid = skinnedRen.materials[1];
			mouthMat = skinnedRen.materials[2];
			mouthInsideMat = mouthInside.material;
			noseMat = noseMesh.material;
		}
		stopBlinking();
		currentBlink = null;
		startBlinking();
	}

	public void startBlinking()
	{
		if (currentBlink == null)
		{
			currentBlink = StartCoroutine(charBlinks());
		}
	}

	public void changeMouthMat(Material newMouthMat, Color skinColor)
	{
		skinnedRen.materials = new Material[3]
		{
			skinnedRen.materials[0],
			skinnedRen.materials[1],
			newMouthMat
		};
		mouthMat = skinnedRen.materials[2];
		mouthMat.color = skinColor;
		setMouthTextures(new Vector2(0f, 0.75f));
	}

	public void changeSkinColor(Color newSkinColor)
	{
		if (!eyeMat)
		{
			eyeMat = eyeInside.material;
			eyeLid = skinnedRen.materials[1];
			mouthMat = skinnedRen.materials[2];
			mouthInsideMat = mouthInside.material;
			noseMat = noseMesh.material;
		}
		eyeLid.color = newSkinColor;
		mouthMat.color = newSkinColor;
		setMouthTextures(new Vector2(0f, 0.75f));
		noseMat.color = newSkinColor;
	}

	public void changeEyeMat(Material newEyeMat, Color skinColor)
	{
		skinnedRen.materials = new Material[3]
		{
			skinnedRen.materials[0],
			newEyeMat,
			skinnedRen.materials[2]
		};
		eyeLid = skinnedRen.materials[1];
		stopBlinking();
		if (base.isActiveAndEnabled)
		{
			stopBlinking();
			startBlinking();
		}
		setMouthTextures(new Vector2(0f, 0.75f));
		changeSkinColor(skinColor);
	}

	public void changeEyeColor(Material newColour)
	{
		eyeInside.material = newColour;
		eyeMat = eyeInside.material;
	}

	public void stopEmotion()
	{
		playingCurrentEmotion = emotion.None;
		startBlinking();
		if (currentEmotion != null)
		{
			StopCoroutine(currentEmotion);
			currentEmotion = null;
		}
		setMouthTextures(new Vector2(0f, 0.75f));
		eyeOffset = Vector2.zero;
	}

	public void setEyesOpen()
	{
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0f, 0.75f));
	}

	public bool needsTiredFace()
	{
		if ((bool)myChar && (float)myChar.stamina <= 10f)
		{
			if (!tired)
			{
				tired = true;
				StartCoroutine(tiredMouth());
			}
			return true;
		}
		if (tired)
		{
			tired = false;
			StartCoroutine(quickHappyMouth());
		}
		return false;
	}

	private IEnumerator charBlinks()
	{
		if (needsTiredFace())
		{
			stopBlinking();
			currentBlink = StartCoroutine(charBlinksTiredFace());
			yield break;
		}
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.75f, 0.75f));
		yield return null;
		yield return null;
		yield return null;
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.75f));
		yield return null;
		yield return null;
		yield return null;
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.25f, 0.75f));
		yield return null;
		yield return null;
		yield return null;
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0f, 0.75f));
		while (true)
		{
			float waitSeconds = Random.Range(3f, 7f);
			while (waitSeconds >= 0f)
			{
				waitSeconds -= Time.deltaTime;
				yield return null;
			}
			if (needsTiredFace())
			{
				break;
			}
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0f, 0.75f));
			yield return null;
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0.25f, 0.75f));
			yield return null;
			yield return null;
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.75f));
			yield return null;
			yield return null;
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0.75f, 0.75f));
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.75f));
			yield return null;
			yield return null;
			yield return null;
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0.25f, 0.75f));
			yield return null;
			yield return null;
			yield return null;
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0f, 0.75f));
		}
		stopBlinking();
		currentBlink = StartCoroutine(charBlinksTiredFace());
	}

	private IEnumerator charBlinksTiredFace()
	{
		if (!needsTiredFace())
		{
			stopBlinking();
			currentBlink = StartCoroutine(charBlinks());
			yield break;
		}
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.75f, 0.75f));
		yield return null;
		yield return null;
		yield return null;
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.75f));
		while (true)
		{
			float waitSeconds = Random.Range(3f, 7f);
			while (waitSeconds >= 0f)
			{
				waitSeconds -= Time.deltaTime;
				yield return null;
			}
			if (!needsTiredFace())
			{
				break;
			}
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0.75f, 0.75f));
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.75f));
		}
		stopBlinking();
		currentBlink = StartCoroutine(charBlinks());
	}

	private IEnumerator tiredMouth()
	{
		while (tired)
		{
			setMouthTextures(new Vector2(0.25f, 0.5f));
			yield return breathSeconds;
			setMouthTextures(new Vector2(0f, 0.5f));
			yield return breathSeconds;
		}
	}

	public void setSleeping()
	{
		stopBlinking();
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.75f, 0.75f));
	}

	public void setHappyEyes()
	{
		stopBlinking();
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0f, 0.5f));
	}

	public void setFaceLaughing()
	{
		if (playingCurrentEmotion != emotion.Laughing)
		{
			playingCurrentEmotion = emotion.Laughing;
			setHappyEyes();
			if (currentEmotion != null)
			{
				StopCoroutine(currentEmotion);
			}
			currentEmotion = StartCoroutine(mouthLaugh());
		}
	}

	public void setFaceCrying()
	{
		if (playingCurrentEmotion != emotion.Crying)
		{
			playingCurrentEmotion = emotion.Crying;
			setSadEyes();
			if (currentEmotion != null)
			{
				StopCoroutine(currentEmotion);
			}
			currentEmotion = StartCoroutine(faceCrying());
		}
	}

	public void setFaceGlee()
	{
		if (playingCurrentEmotion != emotion.Glee)
		{
			playingCurrentEmotion = emotion.Glee;
			setHappyEyes();
			if (currentEmotion != null)
			{
				StopCoroutine(currentEmotion);
			}
			currentEmotion = StartCoroutine(faceGlee());
		}
	}

	public void setFaceThinking()
	{
		if (playingCurrentEmotion != emotion.Thinking)
		{
			playingCurrentEmotion = emotion.Thinking;
			setThinkingEyes();
			if (currentEmotion != null)
			{
				StopCoroutine(currentEmotion);
			}
			currentEmotion = StartCoroutine(faceThinking());
		}
	}

	public void setFaceProud()
	{
		if (playingCurrentEmotion != emotion.Proud)
		{
			playingCurrentEmotion = emotion.Proud;
			setAngryEyes();
			if (currentEmotion != null)
			{
				StopCoroutine(currentEmotion);
			}
			currentEmotion = StartCoroutine(faceProud());
		}
	}

	public void setFaceSigh()
	{
		if (playingCurrentEmotion != emotion.Sigh)
		{
			playingCurrentEmotion = emotion.Sigh;
			setSadEyes();
			StartCoroutine(quickSupriseMouth());
			if (currentEmotion != null)
			{
				StopCoroutine(currentEmotion);
			}
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.sighSound, base.transform.root.position);
			ParticleManager.manage.sighPart.transform.rotation = base.transform.root.rotation;
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.sighPart, base.transform.position + base.transform.forward * 0.25f, 1);
		}
	}

	public void setFaceShy()
	{
		if (playingCurrentEmotion != emotion.Shy)
		{
			playingCurrentEmotion = emotion.Shy;
			setShyEyes();
			if (currentEmotion != null)
			{
				StopCoroutine(currentEmotion);
			}
			StartCoroutine(faceShy());
		}
	}

	public void setFaceWorried()
	{
		if (playingCurrentEmotion != emotion.Worried)
		{
			playingCurrentEmotion = emotion.Worried;
			setSadEyes();
			if (currentEmotion != null)
			{
				StopCoroutine(currentEmotion);
			}
			currentEmotion = StartCoroutine(faceWorried());
		}
	}

	public void setFaceQuestion()
	{
		if (playingCurrentEmotion != emotion.Question)
		{
			playingCurrentEmotion = emotion.Question;
			if (currentEmotion != null)
			{
				StopCoroutine(currentEmotion);
			}
			MonoBehaviour.print("Setting Question");
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.questionSound, base.transform.root.position);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.questionPart, base.transform.position + Vector3.up, 1);
		}
	}

	public void setFaceShocked()
	{
		if (playingCurrentEmotion != emotion.Shocked)
		{
			playingCurrentEmotion = emotion.Shocked;
			setShockedEyes();
			if (currentEmotion != null)
			{
				StopCoroutine(currentEmotion);
			}
			StartCoroutine(faceShocked());
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.shockedSound, base.transform.root.position);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.shockedPart, base.transform.position + CameraController.control.mainCamera.transform.forward, 10);
		}
	}

	public void setFacePumped()
	{
		if (playingCurrentEmotion != emotion.Pumped)
		{
			playingCurrentEmotion = emotion.Pumped;
			setAngryEyes();
			StartCoroutine(quickHappyMouth());
			if (currentEmotion != null)
			{
				StopCoroutine(currentEmotion);
			}
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.pumpedParticle, base.transform.position + CameraController.control.mainCamera.transform.forward, 10);
		}
	}

	public void setAngryEyes()
	{
		stopBlinking();
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.25f, 0.5f));
	}

	public void setSadEyes()
	{
		stopBlinking();
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.5f));
	}

	public void setThinkingEyes()
	{
		stopBlinking();
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.75f));
	}

	public void setShockedEyes()
	{
		stopBlinking();
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.75f, 0.5f));
		setMouthTextures(new Vector2(0.25f, 0.5f));
	}

	public void setShyEyes()
	{
		stopBlinking();
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.75f));
		setMouthTextures(new Vector2(0.5f, 0.75f));
	}

	public void setPumpedEyes()
	{
		stopBlinking();
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.25f, 0.5f));
		setMouthTextures(new Vector2(0.75f, 0.75f));
	}

	public void setSupriseEyes()
	{
		stopBlinking();
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.75f, 0.5f));
	}

	public void sayWord()
	{
		if (sayingWord == null)
		{
			sayingWord = StartCoroutine(moveMouth());
		}
	}

	public void supriseMouth()
	{
		if (sayingWord == null)
		{
			sayingWord = StartCoroutine(quickSupriseMouth());
		}
	}

	private IEnumerator quickSupriseMouth()
	{
		setMouthTextures(new Vector2(0f, 0.75f));
		setMouthTextures(new Vector2(0.25f, 0.75f));
		yield return new WaitForSeconds(Random.Range(1f, 1.8f));
		setMouthTextures(new Vector2(0f, 0.75f));
		yield return null;
		yield return null;
		sayingWord = null;
	}

	public void happyMouth()
	{
		if (sayingWord == null)
		{
			sayingWord = StartCoroutine(quickHappyMouth());
		}
	}

	public void happyMouthUntilCelebrationOver()
	{
		StartCoroutine(happyMouthCelebrationOver());
	}

	private IEnumerator happyMouthCelebrationOver()
	{
		stopBlinking();
		setMouthTextures(new Vector2(0.25f, 0.75f));
		setHappyEyes();
		while (BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		setMouthTextures(new Vector2(0f, 0.75f));
		startBlinking();
		sayingWord = null;
	}

	private IEnumerator quickHappyMouth()
	{
		setMouthTextures(new Vector2(0f, 0.75f));
		setMouthTextures(new Vector2(0f, 0.5f));
		yield return new WaitForSeconds(Random.Range(1f, 1.8f));
		setMouthTextures(new Vector2(0f, 0.75f));
		sayingWord = null;
	}

	public void lookForFaceMoveItems()
	{
		faceMove = HeadPos.GetComponentInChildren<FaceItemMoveWithMouth>();
	}

	private IEnumerator moveMouth()
	{
		if ((bool)faceMove)
		{
			faceMove.openMouth();
		}
		if (Random.Range(0, 3) == 2)
		{
			setMouthTextures(new Vector2(0f, 0.75f));
			yield return null;
			setMouthTextures(new Vector2(0f, 0.5f));
			yield return null;
			yield return null;
			setMouthTextures(new Vector2(0.25f, 0.5f));
			yield return null;
			setMouthTextures(new Vector2(0.5f, 0.5f));
			yield return null;
			yield return null;
			yield return null;
			setMouthTextures(new Vector2(0.25f, 0.5f));
			yield return null;
			setMouthTextures(new Vector2(0f, 0.5f));
			yield return null;
			yield return null;
			setMouthTextures(new Vector2(0f, 0.75f));
		}
		else
		{
			setMouthTextures(new Vector2(0f, 0.75f));
			yield return null;
			setMouthTextures(new Vector2(0.25f, 0.75f));
			yield return null;
			yield return null;
			setMouthTextures(new Vector2(0.5f, 0.75f));
			yield return null;
			setMouthTextures(new Vector2(0.75f, 0.75f));
			yield return null;
			yield return null;
			yield return null;
			setMouthTextures(new Vector2(0.5f, 0.75f));
			yield return null;
			setMouthTextures(new Vector2(0.25f, 0.75f));
			yield return null;
			yield return null;
			setMouthTextures(new Vector2(0f, 0.75f));
		}
		sayingWord = null;
	}

	private IEnumerator mouthLaugh()
	{
		while (playingCurrentEmotion == emotion.Laughing)
		{
			setMouthTextures(new Vector2(0f, 0.75f));
			yield return null;
			yield return null;
			yield return null;
			setMouthTextures(new Vector2(0f, 0.5f));
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			setMouthTextures(new Vector2(0.25f, 0.5f));
			yield return null;
			yield return null;
			yield return null;
			setMouthTextures(new Vector2(0.5f, 0.5f));
			effects.LaughEffect();
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			setMouthTextures(new Vector2(0.25f, 0.5f));
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			setMouthTextures(new Vector2(0f, 0.5f));
			yield return null;
			yield return null;
			yield return null;
			yield return null;
		}
	}

	private IEnumerator faceCrying()
	{
		while (playingCurrentEmotion == emotion.Crying)
		{
			effects.cryingEffect();
			yield return new WaitForSeconds(0.15f);
		}
	}

	private IEnumerator faceShy()
	{
		ParticleManager.manage.shyPart.transform.rotation = base.transform.rotation;
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.shyPart, base.transform.position - CameraController.control.mainCamera.transform.forward, 4);
		while (playingCurrentEmotion == emotion.Shy)
		{
			yield return new WaitForSeconds(0.15f);
			ParticleManager.manage.shyPart.transform.rotation = base.transform.rotation;
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.shyPart, base.transform.position + base.transform.forward * 0.5f - CameraController.control.mainCamera.transform.forward, 1);
		}
		quickHappyMouth();
	}

	private IEnumerator faceGlee()
	{
		while (playingCurrentEmotion == emotion.Glee)
		{
			ParticleManager.manage.gleePart.transform.GetChild(0).rotation = Quaternion.Euler(-90f, 0f, CameraController.control.transform.eulerAngles.y);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.gleePart, base.transform.position + Vector3.up + CameraController.control.mainCamera.transform.forward, 2);
			SoundManager.manage.playASoundAtPoint(SoundManager.manage.gleeSound, base.transform.root.position);
			yield return new WaitForSeconds(1f);
		}
	}

	private IEnumerator faceWorried()
	{
		SoundManager.manage.playASoundAtPoint(SoundManager.manage.worriedSound, base.transform.root.position);
		while (playingCurrentEmotion == emotion.Worried)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.worriedPart, base.transform.position + Vector3.up + CameraController.control.mainCamera.transform.forward, 2);
			yield return new WaitForSeconds(1f);
		}
	}

	private IEnumerator faceThinking()
	{
		effects.thinkingEffect();
		yield return null;
		currentEmotion = null;
	}

	private IEnumerator faceProud()
	{
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.proudPart, base.transform.position - base.transform.root.right / 1.5f + Vector3.up / 1.5f, 1);
		yield return new WaitForSeconds(0.7f);
		SoundManager.manage.play2DSound(SoundManager.manage.proudSound);
		currentEmotion = null;
	}

	private IEnumerator faceShocked()
	{
		Texture myEyes = eyeMat.mainTexture;
		eyeMat.mainTexture = supriseEyes;
		while (playingCurrentEmotion == emotion.Shocked)
		{
			yield return null;
		}
		eyeMat.mainTexture = myEyes;
		quickSupriseMouth();
	}

	public void setMouthTextures(Vector2 mouthPos)
	{
		mouthMat.SetTextureOffset("_MainTex", mouthPos);
		mouthInsideMat.SetTextureOffset("_MainTex", mouthPos);
	}

	public void stopBlinking()
	{
		if (currentBlink != null)
		{
			StopCoroutine(currentBlink);
			currentBlink = null;
		}
	}
}
