using UnityEngine;

public class AppearIfLicenceLevel : MonoBehaviour
{
	public LicenceManager.LicenceTypes licenceType;

	public int licenceLevel;

	public float percentChanceOfShowing;

	public GameObject toShow;

	private void OnEnable()
	{
		if (!NetworkMapSharer.share)
		{
			return;
		}
		if (LicenceManager.manage.allLicences[(int)licenceType].getCurrentLevel() >= licenceLevel && base.transform.position.y >= 0f)
		{
			Random.InitState((int)(base.transform.position.x + base.transform.position.z) + NetworkMapSharer.share.mineSeed + (int)base.transform.position.y);
			if (Random.Range(0f, 100f) <= percentChanceOfShowing)
			{
				toShow.SetActive(true);
			}
			else
			{
				toShow.SetActive(false);
			}
		}
		else
		{
			toShow.SetActive(false);
		}
	}
}
