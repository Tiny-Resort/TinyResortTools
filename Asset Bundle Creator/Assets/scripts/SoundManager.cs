using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SoundManager : MonoBehaviour
{
	public static SoundManager manage;

	private float masterVolume = 1f;

	private float UIVolume = 1f;

	private float savedMasterVolume = 1f;

	public float masterPitch = 1f;

	private float soundeffectVolume = 1f;

	public UnityEvent onMasterChange = new UnityEvent();

	public AudioSource[] myAudios;

	public AudioSource[] myTalkAudios;

	public bool[] audiosInUse;

	public AudioSource[] my2DAudios;

	public ASound footStepGrass;

	public ASound footStepDirt;

	public ASound footStepWater;

	public ASound footStepWood;

	public ASound footStepStone;

	public ASound footStepSand;

	public ASound wetFootStep;

	public ASound signTalk;

	public ASound chargeUpPitch;

	public ASound waterSplash;

	public ASound bigWaterSplash;

	public ASound treadWater;

	public ASound coinsChange;

	public ASound pickUpItem;

	public ASound dropItem;

	public ASound rollOverButton;

	public ASound buttonSound;

	public ASound buttonCantPressSound;

	public ASound inventorySound;

	public ASound placeItem;

	public ASound plantSeed;

	public ASound doorClose;

	public ASound doorOpen;

	public ASound pocketsFull;

	public ASound genericFootStep;

	public ASound impactDamageSound;

	public ASound finalImpactSound;

	public ASound nonOrganicHitSound;

	public ASound nonOrganicFinalHitSound;

	public ASound statusDamageSound;

	public ASound windowOpenSound;

	public ASound animalDiesSound;

	public ASound goToSleepSound;

	public ASound onFireStatusSound;

	public ASound craftingComplete;

	public ASound levelUpSound;

	public ASound fishFakeBite;

	public ASound fishBite;

	public ASound dropInBoxSound;

	public ASound deepVoice;

	public ASound medVoice;

	public ASound highVoice;

	public ASound toolBreaks;

	public ASound cameraSwitch;

	public ASound notificationSound;

	public ASound taskAcceptedSound;

	public ASound selectSlotForGive;

	public ASound deselectSlotForGive;

	public ASound stunnedByLightSound;

	public ASound rotationSound;

	public ASound paintSound;

	public ASound trapperSound;

	public AudioClip[] alphabet;

	public AudioSource windowOpenSource;

	public AudioSource windowCloseSource;

	public ASound teleportCharge;

	public ASound teleportSound;

	public ASound placeInScaleSound;

	public ASound vehicleKnockBack;

	public ASound pickUpUnderwaterCreature;

	public ASound placeItemInChanger;

	public ASound digUpBurriedItem;

	public ASound thinkingSound;

	public ASound questionSound;

	public ASound shockedSound;

	public ASound gleeSound;

	public ASound playerLaugh;

	public ASound worriedSound;

	public ASound sighSound;

	public ASound proudSound;

	private bool sayingAWord;

	private bool underWater;

	private void Awake()
	{
		manage = this;
	}

	private void Start()
	{
		audiosInUse = new bool[myAudios.Length];
		for (int i = 0; i < audiosInUse.Length; i++)
		{
			audiosInUse[i] = false;
		}
	}

	public void soundOnOpenWindow()
	{
		windowOpenSource.volume = 0.1f * getUiVolume();
		if (!windowOpenSource.isPlaying)
		{
			if (windowCloseSource.isPlaying)
			{
				windowCloseSource.Stop();
			}
			windowOpenSource.Play();
		}
	}

	public void soundOnCloseWindow()
	{
		windowCloseSource.volume = 0.1f * getUiVolume();
		if (!windowCloseSource.isPlaying)
		{
			windowCloseSource.Play();
		}
	}

	public void playASoundAtPoint(ASound soundToPlay, Vector3 position, float volumePercent = 1f, float pitchDif = 1f)
	{
		if (Vector3.Distance(CameraController.control.transform.position, position) > 22f)
		{
			return;
		}
		AudioSource[] array = myAudios;
		foreach (AudioSource audioSource in array)
		{
			if (!audioSource.isPlaying)
			{
				audioSource.pitch = soundToPlay.getPitch() * pitchDif * masterPitch;
				audioSource.transform.position = position;
				audioSource.PlayOneShot(soundToPlay.getSound(), (soundToPlay.volume + Random.Range((0f - soundToPlay.volume) / 10f, soundToPlay.volume / 10f)) * volumePercent * getSoundVolume());
				break;
			}
		}
	}

	public float playASoundAtPointAndReturnLength(ASound soundToPlay, Vector3 position, out AudioSource currentlyPlaying, float volumePercent = 1f, float pitchDif = 1f)
	{
		currentlyPlaying = null;
		if (Vector3.Distance(CameraController.control.transform.position, position) > 22f)
		{
			return 0f;
		}
		AudioSource[] array = myAudios;
		foreach (AudioSource audioSource in array)
		{
			if (!audioSource.isPlaying)
			{
				audioSource.pitch = soundToPlay.getPitch() * pitchDif * masterPitch;
				audioSource.transform.position = position;
				AudioClip sound = soundToPlay.getSound();
				audioSource.PlayOneShot(sound, (soundToPlay.volume + Random.Range((0f - soundToPlay.volume) / 10f, soundToPlay.volume / 10f)) * volumePercent * getSoundVolume());
				currentlyPlaying = audioSource;
				return sound.length;
			}
		}
		return 0f;
	}

	public void playASoundAtPointWithPitch(ASound soundToPlay, Vector3 position, float pitch)
	{
		AudioSource[] array = myAudios;
		foreach (AudioSource audioSource in array)
		{
			if (!audioSource.isPlaying)
			{
				audioSource.transform.position = position;
				audioSource.pitch = pitch * masterPitch;
				audioSource.PlayOneShot(soundToPlay.getSound(), soundToPlay.volume * getSoundVolume());
				break;
			}
		}
	}

	public void play2DSound(ASound soundToPlay)
	{
		AudioSource[] array = my2DAudios;
		foreach (AudioSource audioSource in array)
		{
			if (!audioSource.isPlaying)
			{
				audioSource.pitch = soundToPlay.getPitch();
				audioSource.PlayOneShot(soundToPlay.getSound(), soundToPlay.volume * getUiVolume());
				break;
			}
		}
	}

	public float getUiVolume()
	{
		return UIVolume * masterVolume;
	}

	public float getMasterVolume()
	{
		return masterVolume;
	}

	public float getSoundVolumeForChange()
	{
		return soundeffectVolume;
	}

	public float getUiVolumeForChange()
	{
		return UIVolume;
	}

	public float getSoundVolume()
	{
		return soundeffectVolume * masterVolume;
	}

	public void setMasterVolume(float newVolume)
	{
		masterVolume = newVolume;
		MusicManager.manage.changeVolume(MusicManager.manage.musicMasterVolume);
		onMasterChange.Invoke();
	}

	public void setSoundEffectVolume(float newVolume)
	{
		soundeffectVolume = newVolume;
		onMasterChange.Invoke();
	}

	public void setUiVolume(float newVolume)
	{
		UIVolume = newVolume;
	}

	private IEnumerator playSoundWithDelay(int audio, ASound soundToPlay)
	{
		yield return null;
		myAudios[audio].PlayOneShot(soundToPlay.getSound(), soundToPlay.volume * getSoundVolume());
		while (myAudios[audio].isPlaying)
		{
			yield return null;
		}
		yield return null;
		audiosInUse[audio] = false;
	}

	public void sayWord(string wordToSay, Vector3 sayPos, float pitch = 1.5f)
	{
		StartCoroutine(playWord(wordToSay, sayPos, pitch));
	}

	private AudioSource getFreeTalkerForLetter(Vector3 position, float pitch)
	{
		AudioSource[] array = myTalkAudios;
		foreach (AudioSource audioSource in array)
		{
			if (!audioSource.isPlaying)
			{
				audioSource.pitch = pitch + Random.Range(-0.15f, 0.15f);
				audioSource.transform.position = position;
				return audioSource;
			}
		}
		return null;
	}

	public float getWordTime(string wordToSay, float sayingPitch = 1.5f)
	{
		float num = 0f;
		wordToSay = wordToSay.ToUpper();
		for (int i = 0; i < wordToSay.Length; i++)
		{
			int num2 = wordToSay[i] - 65;
			if (num2 >= 0 && num2 <= 25)
			{
				num += alphabet[num2].length / sayingPitch / 4f;
			}
		}
		return num + Random.Range(0.01f, 0.07f);
	}

	private IEnumerator playWord(string wordToSay, Vector3 sayPos, float pitch)
	{
		while (sayingAWord)
		{
			yield return null;
		}
		sayingAWord = true;
		wordToSay = wordToSay.ToUpper();
		for (int i = 0; i < wordToSay.Length; i++)
		{
			int num = wordToSay[i] - 65;
			if (num >= 0 && num <= 25)
			{
				getFreeTalkerForLetter(sayPos, pitch).PlayOneShot(alphabet[num]);
				yield return new WaitForSeconds(alphabet[num].length / pitch / 4f);
			}
		}
		sayingAWord = false;
	}

	public void switchUnderWater(bool newUnderWater)
	{
	}
}
