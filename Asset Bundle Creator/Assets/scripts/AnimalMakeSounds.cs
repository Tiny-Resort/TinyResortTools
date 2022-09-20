using System.Collections;
using UnityEngine;

public class AnimalMakeSounds : MonoBehaviour
{
	public ASound animalSoundPool;

	public ASound animalDamageSound;

	public ASound attackSound;

	public float soundCheckTime = 1f;

	private WaitForSeconds animalSoundWait = new WaitForSeconds(1f);

	private Damageable myDamage;

	private AnimalAI_Sleep mySleep;

	private bool isPlayingOtherSound;

	private float timeToWait;

	private AudioSource currentlyPlaying;

	public bool animateOnSound;

	private Animator myAnim;

	private AnimalVariation hasVariation;

	private float pitchDif = 1f;

	private void Start()
	{
		animalSoundWait = new WaitForSeconds(soundCheckTime);
	}

	private void OnEnable()
	{
		currentlyPlaying = null;
		isPlayingOtherSound = false;
		hasVariation = GetComponent<AnimalVariation>();
		if ((bool)hasVariation && hasVariation.sizeVariation.Length != 0)
		{
			pitchDif = 1f + (1f - hasVariation.sizeVariation[hasVariation.getVaritationNo()].y) * 2f;
		}
		if (animateOnSound)
		{
			myAnim = GetComponent<Animator>();
		}
		myDamage = GetComponent<Damageable>();
		mySleep = GetComponent<AnimalAI_Sleep>();
		if ((bool)animalSoundPool)
		{
			StartCoroutine(animalMakeSounds());
		}
	}

	public IEnumerator makeSoundForBox()
	{
		yield return new WaitForSeconds(Random.Range(0f, 3f));
		while (true)
		{
			yield return animalSoundWait;
			if (!isPlayingOtherSound && Random.Range(0, 8) == 2)
			{
				isPlayingOtherSound = true;
				if ((bool)hasVariation)
				{
					Invoke("whenSoundFinished", SoundManager.manage.playASoundAtPointAndReturnLength(animalSoundPool, base.transform.position, out currentlyPlaying, 1f, pitchDif));
				}
				else
				{
					Invoke("whenSoundFinished", SoundManager.manage.playASoundAtPointAndReturnLength(animalSoundPool, base.transform.position, out currentlyPlaying));
				}
			}
		}
	}

	private IEnumerator animalMakeSounds()
	{
		while (true)
		{
			yield return animalSoundWait;
			if (((!mySleep && myDamage.health > 0) || ((bool)mySleep && !mySleep.checkIfSleeping() && myDamage.health > 0)) && !isPlayingOtherSound && Random.Range(0, 8) == 2)
			{
				isPlayingOtherSound = true;
				if ((bool)hasVariation)
				{
					Invoke("whenSoundFinished", SoundManager.manage.playASoundAtPointAndReturnLength(animalSoundPool, base.transform.position, out currentlyPlaying, 1f, pitchDif));
				}
				else
				{
					Invoke("whenSoundFinished", SoundManager.manage.playASoundAtPointAndReturnLength(animalSoundPool, base.transform.position, out currentlyPlaying));
				}
				if (animateOnSound)
				{
					myAnim.SetTrigger("MakeSound");
				}
			}
		}
	}

	public void playDamageSound()
	{
		if (!animalDamageSound)
		{
			return;
		}
		if ((bool)currentlyPlaying)
		{
			if (currentlyPlaying.isPlaying)
			{
				currentlyPlaying.Stop();
			}
			currentlyPlaying = null;
		}
		isPlayingOtherSound = true;
		if ((bool)hasVariation)
		{
			Invoke("whenSoundFinished", SoundManager.manage.playASoundAtPointAndReturnLength(animalDamageSound, base.transform.position, out currentlyPlaying, 1f, pitchDif));
		}
		else
		{
			Invoke("whenSoundFinished", SoundManager.manage.playASoundAtPointAndReturnLength(animalDamageSound, base.transform.position, out currentlyPlaying));
		}
	}

	private void whenSoundFinished()
	{
		isPlayingOtherSound = false;
	}

	public void playAnimalSoundOnAttack()
	{
		if (!attackSound)
		{
			return;
		}
		if ((bool)currentlyPlaying)
		{
			if (currentlyPlaying.isPlaying)
			{
				currentlyPlaying.Stop();
			}
			currentlyPlaying = null;
		}
		isPlayingOtherSound = true;
		if ((bool)hasVariation)
		{
			Invoke("whenSoundFinished", SoundManager.manage.playASoundAtPointAndReturnLength(attackSound, base.transform.position, out currentlyPlaying, 1f, pitchDif));
		}
		else
		{
			Invoke("whenSoundFinished", SoundManager.manage.playASoundAtPointAndReturnLength(attackSound, base.transform.position, out currentlyPlaying));
		}
	}
}
