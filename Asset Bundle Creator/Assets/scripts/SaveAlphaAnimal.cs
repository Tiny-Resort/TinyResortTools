using UnityEngine;

public class SaveAlphaAnimal : MonoBehaviour
{
	public Vector3 mySpawnPoint;

	public int daysRemaining;

	public bool canSpawnInWater;

	private void OnEnable()
	{
		WorldManager.manageWorld.changeDayEvent.AddListener(takeADayAway);
	}

	private void OnDisable()
	{
		WorldManager.manageWorld.changeDayEvent.RemoveListener(takeADayAway);
	}

	public void takeADayAway()
	{
		daysRemaining--;
	}
}
