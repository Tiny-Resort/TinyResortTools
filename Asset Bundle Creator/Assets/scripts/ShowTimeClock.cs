using UnityEngine;

public class ShowTimeClock : MonoBehaviour
{
	public Transform hourhand;

	public Transform minuteHand;

	public playLoopSoundOnTileObject tickingSound;

	private void OnEnable()
	{
		RealWorldTimeLight.time.clockTickEvent.AddListener(updateClockFace);
	}

	private void OnDisable()
	{
		RealWorldTimeLight.time.clockTickEvent.RemoveListener(updateClockFace);
	}

	public void updateClockFace()
	{
		Invoke("clockTick", Random.Range(0f, 0.15f));
	}

	public void clockTick()
	{
		float num = (float)RealWorldTimeLight.time.currentHour / 24f;
		float num2 = (float)RealWorldTimeLight.time.currentMinute / 60f;
		hourhand.localEulerAngles = new Vector3(0f, 0f, num * 720f);
		minuteHand.localEulerAngles = new Vector3(0f, 0f, num2 * 360f);
		if ((bool)tickingSound)
		{
			tickingSound.playSoundForAnimation();
		}
	}
}
