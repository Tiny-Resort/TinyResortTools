using UnityEngine;

public class InvisibleWallsDissapear : MonoBehaviour
{
	private void OnEnable()
	{
		if (TownManager.manage.journalUnlocked)
		{
			base.gameObject.SetActive(false);
		}
	}
}
