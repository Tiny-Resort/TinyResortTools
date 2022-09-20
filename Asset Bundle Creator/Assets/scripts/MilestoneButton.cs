using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MilestoneButton : MonoBehaviour
{
	public TextMeshProUGUI title;

	public Image taskIcon;

	public Milestone myMilestone;

	public Image backing;

	public Sprite milestoneUnlocked;

	public Sprite milestoneLocked;

	public RectTransform milestoneShine;

	private Coroutine runningShine;

	public static WaitForSeconds shineWait = new WaitForSeconds(1f);

	public void fillButton(Milestone newMilestone)
	{
		myMilestone = newMilestone;
		if (!myMilestone.isVisibleOnList())
		{
			title.text = "???????????";
			taskIcon.sprite = MilestoneManager.manage.unknownMilestoneIcon;
			backing.sprite = milestoneLocked;
			return;
		}
		title.text = MilestoneManager.manage.getMilestoneName(myMilestone);
		if ((float)MilestoneManager.manage.getMilestoneName(myMilestone).Length >= 12f)
		{
			title.enableAutoSizing = true;
		}
		else
		{
			title.enableAutoSizing = false;
		}
		taskIcon.sprite = DailyTaskGenerator.generate.taskSprites[(int)myMilestone.myTaskType];
		backing.sprite = milestoneUnlocked;
		if (!myMilestone.checkIfLevelUpAvaliable() && runningShine != null)
		{
			StopCoroutine(runningShine);
			runningShine = null;
			milestoneShine.anchoredPosition = new Vector2(0f, 0f);
		}
	}

	private void OnEnable()
	{
		if (myMilestone == null || !myMilestone.isVisibleOnList())
		{
			return;
		}
		if (myMilestone.checkIfLevelUpAvaliable())
		{
			if (runningShine != null)
			{
				StopCoroutine(runningShine);
				runningShine = null;
			}
			runningShine = StartCoroutine(iconDance());
		}
		else if (runningShine != null)
		{
			StopCoroutine(runningShine);
			runningShine = null;
		}
	}

	private void OnDisable()
	{
		milestoneShine.anchoredPosition = new Vector2(0f, 0f);
	}

	public void pressButton()
	{
		if (myMilestone == null)
		{
			MilestoneManager.manage.fillUnknowMilestoneDescription();
		}
		else if (myMilestone.isVisibleOnList() || (Inventory.inv.usingMouse && InputMaster.input.JumpHeld()))
		{
			MilestoneManager.manage.fillMilestonDescription(myMilestone);
		}
		else
		{
			MilestoneManager.manage.fillUnknowMilestoneDescription();
		}
	}

	private IEnumerator iconDance()
	{
		while (true)
		{
			float shineTimer = 0f;
			while (shineTimer < 1.5f)
			{
				yield return null;
				shineTimer += Time.deltaTime;
				milestoneShine.anchoredPosition = Vector2.Lerp(new Vector2(0f, 0f), new Vector2(100f, -100f), shineTimer);
			}
			yield return shineWait;
		}
	}
}
