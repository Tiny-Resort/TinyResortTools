using UnityEngine;

public class MakeSoundOnEnabled : MonoBehaviour
{
	public ASound soundToMake;

	private void OnEnable()
	{
		SoundManager.manage.play2DSound(soundToMake);
	}
}
