using UnityEngine;

public class MusicSource
{
	public bool isPaused;

	private float localVolume = 1f;

	private float fadeMax = 1f;

	private AudioSource myAudioSource;

	public Coroutine runningCoroutine;

	public MusicSource(AudioSource myAudio)
	{
		myAudioSource = myAudio;
	}

	public void changeMusicClip(AudioClip musicClip)
	{
		myAudioSource.clip = musicClip;
	}

	public void adjustLocalVolume(float difference)
	{
		localVolume = Mathf.Clamp(localVolume + difference, 0f, 1f);
		myAudioSource.volume = Mathf.Clamp(localVolume, 0f, fadeMax) * (MusicManager.manage.musicMasterVolume * SoundManager.manage.getMasterVolume() / 4f);
	}

	public void setLocalVolume(float newLocalVolume)
	{
		localVolume = Mathf.Clamp(newLocalVolume, 0f, 1f);
		myAudioSource.volume = Mathf.Clamp(localVolume, 0f, fadeMax) * (MusicManager.manage.musicMasterVolume * SoundManager.manage.getMasterVolume() / 4f);
	}

	public void updateVolumeToMaster()
	{
		myAudioSource.volume = Mathf.Clamp(localVolume, 0f, fadeMax) * (MusicManager.manage.musicMasterVolume * SoundManager.manage.getMasterVolume() / 4f);
	}

	public float getLocalVolume()
	{
		return localVolume;
	}

	public void play()
	{
		myAudioSource.time = 0f;
		myAudioSource.Play();
		isPaused = false;
	}

	public void pause(bool newPaused)
	{
		isPaused = newPaused;
		if (isPaused)
		{
			myAudioSource.Pause();
		}
		else
		{
			myAudioSource.UnPause();
		}
	}

	public void Stop()
	{
		if (runningCoroutine != null)
		{
			MusicManager.manage.StopCoroutine(runningCoroutine);
		}
		myAudioSource.Stop();
	}

	public void fadeOut(float fadeOutSpeed = 3f)
	{
		if (runningCoroutine != null)
		{
			MusicManager.manage.StopCoroutine(runningCoroutine);
		}
		runningCoroutine = MusicManager.manage.StartCoroutine(MusicManager.manage.fadeOut(this, fadeOutSpeed));
	}

	public void fadeIn(float fadeInSpeed = 3f)
	{
		if (runningCoroutine != null)
		{
			MusicManager.manage.StopCoroutine(runningCoroutine);
		}
		runningCoroutine = MusicManager.manage.StartCoroutine(MusicManager.manage.fadeIn(this, fadeInSpeed));
	}

	public bool isPlaying()
	{
		if (isPaused)
		{
			return true;
		}
		return myAudioSource.isPlaying;
	}

	public void checkForLastSecondsFade()
	{
		if (myAudioSource.time < 20f)
		{
			fadeMax = myAudioSource.time / 20f;
			updateVolumeToMaster();
		}
		else if (myAudioSource.time > myAudioSource.clip.length - 20f)
		{
			fadeMax = (myAudioSource.clip.length - myAudioSource.time) / 20f;
			updateVolumeToMaster();
		}
		else
		{
			fadeMax = 1f;
		}
	}
}
