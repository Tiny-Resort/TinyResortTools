using UnityEngine;

public class ASound : MonoBehaviour
{
	public AudioClip[] myClips;

	public float volume = 0.05f;

	public float pitchLow = 1f;

	public float pitchHigh = 2f;

	public bool loop;

	public float maxDistance = 25f;

	public float getPitch()
	{
		return Random.Range(pitchLow, pitchHigh);
	}

	public AudioClip getSound()
	{
		return myClips[Random.Range(0, myClips.Length)];
	}

	public void playSoundForAnimator()
	{
		SoundManager.manage.playASoundAtPoint(this, base.transform.position);
	}

	public void play2DSoundForAnimator()
	{
		SoundManager.manage.play2DSound(this);
	}
}
