using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NewMilestoneNotification : MonoBehaviour
{
	public bool playingIcon;

	public Image icon;

	public ASound newMilestoneSound;

	public void setIconAndPlayAnimation(int milestoneNo)
	{
		base.transform.localPosition = new Vector3(0f, 55f, 0f);
		base.transform.localScale = Vector3.one;
		playingIcon = true;
		icon.sprite = DailyTaskGenerator.generate.taskSprites[milestoneNo];
		SoundManager.manage.play2DSound(newMilestoneSound);
		base.gameObject.SetActive(true);
		StartCoroutine(animate());
		MilestoneManager.manage.bookBounceAnimation.Rebind();
	}

	public void OnDisable()
	{
		base.gameObject.SetActive(false);
		playingIcon = false;
	}

	private IEnumerator animate()
	{
		float timer3 = 0f;
		while (timer3 < 1f)
		{
			base.transform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(0f, 65f, 0f), timer3);
			base.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.25f, timer3);
			timer3 += Time.deltaTime * 4.5f;
			yield return null;
		}
		timer3 = 0f;
		while (timer3 < 1f)
		{
			base.transform.localPosition = Vector3.Lerp(new Vector3(0f, 65f, 0f), new Vector3(0f, 55f, 0f), timer3);
			base.transform.localScale = Vector3.Lerp(Vector3.one * 1.25f, Vector3.one, timer3);
			timer3 += Time.deltaTime * 6.5f;
			yield return null;
		}
		yield return new WaitForSeconds(2f);
		timer3 = 0f;
		while (timer3 < 1f)
		{
			base.transform.localPosition = Vector3.Lerp(new Vector3(0f, 55f, 0f), Vector3.zero, timer3);
			base.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, timer3);
			timer3 += Time.deltaTime * 3.5f;
			yield return null;
		}
		playingIcon = false;
		base.gameObject.SetActive(false);
	}
}
