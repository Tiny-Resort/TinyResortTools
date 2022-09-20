using System.Collections;
using UnityEngine;

public class EnableBookAnimation : MonoBehaviour
{
	public Animator myAnim;

	public GameObject bookTabs;

	private WaitForSeconds tabWait = new WaitForSeconds(1f);

	private void OnEnable()
	{
		if (TownManager.manage.journalUnlocked)
		{
			myAnim.enabled = true;
			StartCoroutine(tabCheck());
		}
	}

	public void disableAnimation()
	{
		myAnim.enabled = false;
	}

	private IEnumerator tabCheck()
	{
		yield return tabWait;
		bookTabs.SetActive(true);
	}

	private void OnDisable()
	{
	}
}
