using UnityEngine;

public class AnimateCharFace : MonoBehaviour
{
	public EmotionEffects myEmotionEffects;

	public bool emotionsLocked = true;

	private Animator bodyAnim;

	public EyesScript eyes;

	private void Start()
	{
		bodyAnim = base.transform.root.GetComponent<Animator>();
	}

	public void setFaceSleeping()
	{
		stopFaceEmotion();
		eyes.setSleeping();
	}

	public void stopFaceSleeping()
	{
		stopFaceEmotion();
		eyes.stopEmotion();
	}

	public void setFaceLaughing()
	{
		if (!emotionsLocked)
		{
			stopFaceEmotion();
			eyes.setFaceLaughing();
		}
	}

	public void stopFaceEmotion()
	{
		eyes.stopEmotion();
	}

	public void setHappyFace(int bypassLock = 0)
	{
		if (bypassLock != 0 || !emotionsLocked)
		{
			eyes.setHappyEyes();
		}
	}

	public void setEyesClosed(int bypassLock = 0)
	{
		if (bypassLock != 0 || !emotionsLocked)
		{
			eyes.setSleeping();
		}
	}

	public void setFaceAngry(int bypassLock = 0)
	{
		if (bypassLock != 0 || !emotionsLocked)
		{
			eyes.setAngryEyes();
		}
	}

	public void setFaceCrying(int bypassLock = 0)
	{
		if (bypassLock != 0 || !emotionsLocked)
		{
			eyes.setFaceCrying();
		}
	}

	public void setFaceThinking(int bypassLock = 0)
	{
		if (bypassLock != 0 || !emotionsLocked)
		{
			eyes.setFaceThinking();
		}
	}

	public void setFaceSigh(int bypassLock = 0)
	{
		if (bypassLock != 0 || !emotionsLocked)
		{
			eyes.setFaceSigh();
		}
	}

	public void setFacePumped(int bypassLock = 0)
	{
		if (bypassLock != 0 || !emotionsLocked)
		{
			eyes.setFacePumped();
		}
	}

	public void setFaceShocked(int bypassLock = 0)
	{
		if (bypassLock != 0 || !emotionsLocked)
		{
			eyes.setFaceShocked();
		}
	}

	public void setHappyGlee(int bypassLock = 0)
	{
		if (bypassLock != 0 || !emotionsLocked)
		{
			eyes.setFaceGlee();
		}
	}

	public void setFaceWorried(int bypassLock = 0)
	{
		if (bypassLock != 0 || !emotionsLocked)
		{
			eyes.setFaceWorried();
		}
	}

	public void setFaceProud(int bypassLock = 0)
	{
		if (bypassLock != 0 || !emotionsLocked)
		{
			eyes.setFaceProud();
		}
	}

	public void setFaceQuestion(int bypassLock = 0)
	{
		if (bypassLock != 0 || !emotionsLocked)
		{
			eyes.setFaceQuestion();
		}
	}

	public void setFaceShy(int bypassLock = 0)
	{
		if (bypassLock != 0 || !emotionsLocked)
		{
			eyes.setFaceShy();
		}
	}

	public void setTriggerTalk()
	{
		eyes.sayWord();
	}

	public void stopEmotions()
	{
		if ((bool)bodyAnim)
		{
			emotionsLocked = true;
			stopFaceEmotion();
			bodyAnim.SetInteger("Emotion", 0);
			stopFaceEmotion();
		}
	}

	public void setEmotionNo(int emotionNo)
	{
		if (bodyAnim.GetInteger("Emotion") != emotionNo)
		{
			stopEmotions();
		}
		emotionsLocked = false;
		bodyAnim.SetInteger("Emotion", emotionNo);
	}

	public void playClappingSound()
	{
		myEmotionEffects.playClappingSound();
	}
}
