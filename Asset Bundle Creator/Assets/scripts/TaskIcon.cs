using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskIcon : MonoBehaviour
{
	public TextMeshProUGUI TaskText;

	public TextMeshProUGUI rewardText;

	public Image IconImage;

	public Image TaskCircle;

	public RectTransform rewardBox;

	public GameObject taskDescBox;

	public ASound completeSound;

	private int points;

	private int requiredPoints;

	private Coroutine fillRoutine;

	public GameObject completedTick;

	private Task trackingTask;

	private void Start()
	{
	}

	public void OnEnable()
	{
		if (trackingTask == null)
		{
			fillRoutine = null;
			TaskCircle.fillAmount = 0f;
		}
		else
		{
			fillRoutine = null;
			fillWithDetails(trackingTask);
		}
		if (trackingTask.completed)
		{
			completedTick.SetActive(true);
		}
		else
		{
			completedTick.SetActive(false);
		}
	}

	public void fillWithDetails(Task task)
	{
		if (task != trackingTask)
		{
			completedTick.SetActive(false);
		}
		trackingTask = task;
		TaskText.text = task.missionText;
		points = task.points;
		requiredPoints = task.requiredPoints;
		if (fillRoutine == null && base.isActiveAndEnabled)
		{
			fillRoutine = StartCoroutine(fillTheBar());
		}
		rewardText.text = task.reward.ToString();
		if (task.taskTypeId == 1)
		{
			if ((bool)task.tileObjectToInteract.tileObjectGrowthStages.harvestDrop)
			{
				IconImage.sprite = task.tileObjectToInteract.tileObjectGrowthStages.harvestDrop.getSprite();
			}
		}
		else
		{
			IconImage.sprite = DailyTaskGenerator.generate.taskSprites[task.taskTypeId];
		}
	}

	private IEnumerator fillTheBar()
	{
		float desiredFill = (float)points / ((float)requiredPoints * 1f);
		while (TaskCircle.fillAmount < desiredFill)
		{
			desiredFill = (float)points / ((float)requiredPoints * 1f);
			TaskCircle.fillAmount = Mathf.Lerp(TaskCircle.fillAmount, desiredFill + 0.1f, Time.deltaTime * 2f);
			yield return null;
		}
		TaskCircle.fillAmount = desiredFill;
		fillRoutine = null;
	}

	public void completeTask()
	{
		completedTick.SetActive(true);
		SoundManager.manage.play2DSound(completeSound);
		PermitPointsManager.manage.addPoints(trackingTask.reward);
	}

	public void makeSmall()
	{
		taskDescBox.SetActive(false);
		rewardBox.gameObject.SetActive(false);
		GetComponent<RectTransform>().sizeDelta = new Vector2(40f, 40f);
	}

	public void makeBig()
	{
		taskDescBox.SetActive(true);
		rewardBox.gameObject.SetActive(true);
		GetComponent<RectTransform>().sizeDelta = new Vector2(75f, 75f);
	}

	private IEnumerator completeAnimation(Task currentTask)
	{
		yield return null;
	}
}
