using System.Collections;
using UnityEngine;

public class playLoopSoundOnTileObject : MonoBehaviour
{
	public AudioSource myAudio;

	private float defualtVolume;

	public bool useCoroutine;

	public ASound soundForCoroutine;

	public ParticleSystem particlesToMatch;

	public float waitTimeMin = 2f;

	public float waitTimeMax = 8f;

	private void Start()
	{
		if ((bool)myAudio)
		{
			myAudio.rolloffMode = AudioRolloffMode.Linear;
			myAudio.minDistance = 1f;
			myAudio.maxDistance = 10f;
		}
	}

	private void OnEnable()
	{
		if ((bool)myAudio)
		{
			SoundManager.manage.onMasterChange.AddListener(changeVolume);
			if (defualtVolume == 0f)
			{
				defualtVolume = myAudio.volume;
			}
			changeVolume();
		}
		if (useCoroutine)
		{
			StartCoroutine(playSoundWithDelay());
		}
	}

	private void OnDisable()
	{
		if ((bool)myAudio)
		{
			SoundManager.manage.onMasterChange.RemoveListener(changeVolume);
			myAudio.Stop();
		}
	}

	public void changeVolume()
	{
		if ((bool)myAudio)
		{
			myAudio.volume = defualtVolume * SoundManager.manage.getSoundVolume();
		}
	}

	public void playSoundForAnimation()
	{
		if ((bool)myAudio && myAudio.enabled)
		{
			myAudio.Play();
		}
	}

	private IEnumerator playSoundWithDelay()
	{
		while (true)
		{
			yield return new WaitForSeconds(Random.Range(waitTimeMin, waitTimeMax));
			if ((bool)particlesToMatch)
			{
				particlesToMatch.Emit(Random.Range(25, 35));
			}
			SoundManager.manage.playASoundAtPoint(soundForCoroutine, base.transform.position);
		}
	}
}
