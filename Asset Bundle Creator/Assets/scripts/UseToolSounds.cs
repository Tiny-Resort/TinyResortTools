using System.Collections;
using UnityEngine;

public class UseToolSounds : MonoBehaviour
{
	private CharMovement myChar;

	public AudioSource idleSource;

	public AudioSource useSource;

	public float idleVolume = 0.5f;

	public float useVolume = 0.5f;

	private void Start()
	{
		myChar = base.transform.root.GetComponent<CharMovement>();
		if (!myChar)
		{
			idleSource.Stop();
			Object.Destroy(this);
		}
		else
		{
			StartCoroutine(soundCheck());
		}
	}

	private IEnumerator soundCheck()
	{
		while (true)
		{
			if (myChar.myEquip.usingItem)
			{
				idleSource.volume = Mathf.Lerp(idleSource.volume, 0f, Time.deltaTime * 2f);
				useSource.volume = Mathf.Lerp(useSource.volume, useVolume * SoundManager.manage.getSoundVolume(), Time.deltaTime * 25f);
			}
			else
			{
				idleSource.volume = Mathf.Lerp(idleSource.volume, idleVolume * SoundManager.manage.getSoundVolume(), Time.deltaTime * 25f);
				useSource.volume = Mathf.Lerp(useSource.volume, 0f, Time.deltaTime * 2f);
			}
			yield return null;
		}
	}
}
