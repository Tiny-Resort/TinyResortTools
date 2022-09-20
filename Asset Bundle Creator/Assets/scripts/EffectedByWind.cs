using UnityEngine;

public class EffectedByWind : MonoBehaviour
{
	public Transform faceWindDir;

	public Animator animatorWindSpeed;

	private void Start()
	{
		if ((bool)animatorWindSpeed)
		{
			animatorWindSpeed.SetFloat("Offset", Random.Range(0f, 1f));
		}
	}

	private void Update()
	{
		if ((bool)animatorWindSpeed)
		{
			animatorWindSpeed.SetFloat("WindSpeed", 1f + WeatherManager.manage.currentWindSpeed * 5f);
		}
	}

	public void newDayWeatherCheck()
	{
		if ((bool)faceWindDir)
		{
			faceWindDir.LookAt(faceWindDir.transform.position + WeatherManager.manage.windDir);
		}
	}

	private void OnEnable()
	{
		if ((bool)faceWindDir)
		{
			WorldManager.manageWorld.changeDayEvent.AddListener(newDayWeatherCheck);
		}
		newDayWeatherCheck();
	}

	private void OnDisable()
	{
		WorldManager.manageWorld.changeDayEvent.RemoveListener(newDayWeatherCheck);
	}
}
