using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MilestoneManager : MonoBehaviour
{
	public static MilestoneManager manage;

	public GameObject milestoneButton;

	public Transform buttonWindow;

	public List<Milestone> milestones = new List<Milestone>();

	public List<MilestoneButton> milestoneButtons = new List<MilestoneButton>();

	public GameObject contentWindow;

	public TextMeshProUGUI title;

	public TextMeshProUGUI description;

	public TextMeshProUGUI fillText;

	public TextMeshProUGUI rewardButtonText;

	public Image milestonIcon;

	public Image fill;

	public GameObject redeemParticles;

	public GameObject redeemButton;

	public Sprite unknownMilestoneIcon;

	public ASound playSoundOnComplete;

	public GameObject[] notifications;

	public GameObject milestoneClaimScreen;

	public Image buttonIcon;

	public GameObject objectShineOnFull;

	public NewMilestoneNotification newMilestoneNotification;

	public Animator bookBounceAnimation;

	private Milestone lookingAt;

	public bool milestoneClaimWindowOpen;

	private void Awake()
	{
		manage = this;
	}

	public void refreshMilestoneAmounts()
	{
		milestones[6].changeAmountPerLevel(new int[7] { 5000, 20000, 100000, 250000, 500000, 1000000, 5000000 });
		milestones[6].changeRewardAmount(200);
		milestones[6].setSuffix("m");
		milestones[83].changeAmountPerLevel(new int[7] { 5000, 20000, 100000, 250000, 500000, 1000000, 5000000 });
		milestones[83].changeRewardAmount(200);
		milestones[83].setSuffix("m");
		milestones[79].changeAmountPerLevel(new int[7] { 5000, 20000, 100000, 250000, 500000, 1000000, 5000000 });
		milestones[79].changeRewardAmount(200);
		milestones[79].setSuffix("m");
		milestones[1].changeAmountPerLevel(new int[4] { 100, 500, 1000, 2000 });
		milestones[1].changeRewardAmount(50);
		milestones[20].changeAmountPerLevel(new int[4] { 50, 250, 1000, 5000 });
		milestones[20].changeRewardAmount(50);
		milestones[43].changeAmountPerLevel(new int[1] { 1 });
		milestones[44].changeAmountPerLevel(new int[3] { 50, 100, 500 });
		milestones[45].changeAmountPerLevel(new int[1] { 1 });
		milestones[46].changeAmountPerLevel(new int[3] { 1, 15, 50 });
		milestones[47].changeAmountPerLevel(new int[3] { 1, 2, 3 });
		milestones[47].changeRewardAmount(750);
		milestones[48].changeAmountPerLevel(new int[1] { 1 });
		milestones[49].changeAmountPerLevel(new int[1] { 1 });
		milestones[50].changeAmountPerLevel(new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
		milestones[50].changeRewardAmount(250);
		milestones[7].changeAmountPerLevel(new int[5] { 10000, 50000, 100000, 500000, 1500000 });
		milestones[7].changeRewardAmount(250);
		milestones[7].setPreffix("<sprite=11>");
		milestones[8].changeAmountPerLevel(new int[5] { 10000, 50000, 100000, 500000, 1500000 });
		milestones[8].changeRewardAmount(250);
		milestones[8].setPreffix("<sprite=11>");
		milestones[64].changeAmountPerLevel(new int[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
		milestones[64].changeRewardAmount(3000);
		milestones[64].setPreffix("<sprite=11>");
		milestones[64].setSuffix(",000,000");
		milestones[52].changeAmountPerLevel(new int[5] { 1, 25, 100, 500, 1000 });
		milestones[52].changeRewardAmount(100);
		milestones[51].changeAmountPerLevel(new int[5] { 5, 50, 250, 500, 1000 });
		milestones[51].changeRewardAmount(100);
		milestones[54].changeAmountPerLevel(new int[5] { 5, 10, 20, 25, 35 });
		milestones[54].changeRewardAmount(100);
		milestones[55].changeAmountPerLevel(new int[5] { 5, 10, 20, 25, 35 });
		milestones[55].changeRewardAmount(100);
		milestones[57].changeAmountPerLevel(new int[5] { 1, 10, 50, 100, 200 });
		milestones[57].changeRewardAmount(100);
		milestones[42].changeAmountPerLevel(new int[5] { 1, 10, 50, 100, 200 });
		milestones[42].changeRewardAmount(100);
		milestones[53].changeAmountPerLevel(new int[5] { 1, 10, 50, 100, 200 });
		milestones[53].changeRewardAmount(200);
		milestones[53].changeAmountPerLevel(new int[5] { 1, 10, 50, 100, 200 });
		milestones[53].changeRewardAmount(200);
		milestones[41].changeAmountPerLevel(new int[5] { 1, 10, 50, 100, 200 });
		milestones[41].changeRewardAmount(200);
		milestones[60].changeAmountPerLevel(new int[5] { 25, 100, 250, 1000, 5000 });
		milestones[60].changeRewardAmount(100);
		milestones[62].changeAmountPerLevel(new int[7] { 5000, 20000, 100000, 250000, 500000, 1000000, 5000000 });
		milestones[62].changeRewardAmount(200);
		milestones[62].setSuffix("m");
		milestones[63].changeAmountPerLevel(new int[1] { 1 });
		milestones[63].changeRewardAmount(200);
		milestones[65].changeAmountPerLevel(new int[3] { 1, 5, 10 });
		milestones[65].changeRewardAmount(200);
		milestones[66].changeAmountPerLevel(new int[5] { 1, 10, 50, 100, 200 });
		milestones[66].changeRewardAmount(200);
		milestones[38].changeAmountPerLevel(new int[5] { 1, 10, 50, 100, 200 });
		milestones[38].changeRewardAmount(200);
		milestones[78].changeAmountPerLevel(new int[5] { 10, 50, 200, 500, 1000 });
		milestones[78].changeRewardAmount(100);
		milestones[72].changeAmountPerLevel(new int[5] { 1, 10, 20, 50, 100 });
		milestones[72].changeRewardAmount(400);
		milestones[38].changeAmountPerLevel(new int[5] { 1, 10, 20, 50, 100 });
		milestones[38].changeRewardAmount(100);
		milestones[70].changeAmountPerLevel(new int[5] { 1, 10, 20, 50, 100 });
		milestones[70].changeRewardAmount(100);
		milestones[81].changeAmountPerLevel(new int[1] { 1 });
		milestones[81].changeRewardAmount(250);
		milestones[75].changeAmountPerLevel(new int[1] { 1 });
		milestones[75].changeRewardAmount(500);
		milestones[84].changeAmountPerLevel(new int[4] { 1, 10, 20, 50 });
		milestones[84].changeRewardAmount(200);
		milestones[88].changeAmountPerLevel(new int[4] { 1, 10, 20, 50 });
		milestones[88].changeRewardAmount(250);
		milestones[68].changeAmountPerLevel(new int[4] { 1, 10, 20, 50 });
		milestones[68].changeRewardAmount(200);
		milestones[67].changeAmountPerLevel(new int[4] { 1, 10, 20, 50 });
		milestones[67].changeRewardAmount(200);
		milestones[69].changeAmountPerLevel(new int[4] { 1, 10, 20, 50 });
		milestones[69].changeRewardAmount(200);
	}

	public void Start()
	{
		int num = Enum.GetNames(typeof(DailyTaskGenerator.genericTaskType)).Length;
		for (int i = 0; i < num; i++)
		{
			Milestone item = new Milestone((DailyTaskGenerator.genericTaskType)i);
			MilestoneButton component = UnityEngine.Object.Instantiate(milestoneButton, buttonWindow).GetComponent<MilestoneButton>();
			milestones.Add(item);
			milestoneButtons.Add(component);
			if (i == 0)
			{
				component.gameObject.SetActive(false);
			}
		}
		refreshMilestoneAmounts();
		updateMilestoneList();
	}

	public int getMilestonePointsInt(DailyTaskGenerator.genericTaskType milestoneToCheck)
	{
		return milestones[(int)milestoneToCheck].getPointsInt();
	}

	public void updateMilestoneList(bool firstOpen = false)
	{
		for (int i = 0; i < milestoneButtons.Count; i++)
		{
			milestoneButtons[i].fillButton(milestones[i]);
			if (!milestones[i].isVisibleOnList())
			{
				milestoneButtons[i].transform.SetAsLastSibling();
			}
			else if (firstOpen && milestones[i].checkIfLevelUpAvaliable())
			{
				milestoneButtons[i].transform.SetAsFirstSibling();
			}
		}
	}

	public void updateAfterSave()
	{
		refreshMilestoneAmounts();
	}

	public string getMilestoneName(Milestone m)
	{
		int myTaskType = (int)m.myTaskType;
		LocalizedString localizedString = "MilestoneTitles/Milestone_Name_" + myTaskType;
		if ((string)localizedString == null)
		{
			return "Milestone Name";
		}
		return localizedString;
	}

	public string getMilestoneDescription(Milestone m)
	{
		int myTaskType = (int)m.myTaskType;
		LocalizedString localizedString = "MilestoneDesc/MilestoneDesc_" + myTaskType;
		if ((string)localizedString == null)
		{
			return "Milestone Desc";
		}
		return localizedString;
	}

	public void fillMilestonDescription(Milestone toFill)
	{
		contentWindow.SetActive(false);
		lookingAt = toFill;
		title.text = getMilestoneName(toFill);
		description.text = getMilestoneDescription(toFill);
		milestonIcon.sprite = DailyTaskGenerator.generate.taskSprites[(int)lookingAt.myTaskType];
		fill.fillAmount = lookingAt.getCurrentLevelFill();
		rewardButtonText.text = lookingAt.getRewardAmount().ToString("n0");
		if (lookingAt.getRequiredPointsForLevelUp() == 0)
		{
			fillText.text = lookingAt.getPoints();
		}
		else
		{
			fillText.text = lookingAt.getPoints() + " / <b>" + lookingAt.getNextRequiredPointString();
		}
		redeemParticles.SetActive(lookingAt.checkIfLevelUpAvaliable());
		redeemButton.SetActive(lookingAt.checkIfLevelUpAvaliable());
		objectShineOnFull.SetActive(lookingAt.checkIfLevelUpAvaliable());
		contentWindow.SetActive(true);
		openMilestoneClaimWindow();
	}

	public void openMilestoneClaimWindow()
	{
		milestoneClaimScreen.gameObject.SetActive(true);
		milestoneClaimWindowOpen = true;
	}

	public void closeMilestoneClaimWindow()
	{
		milestoneClaimScreen.gameObject.SetActive(false);
		milestoneClaimWindowOpen = false;
	}

	public void fillUnknowMilestoneDescription()
	{
		contentWindow.SetActive(false);
		lookingAt = null;
		title.text = "????????";
		description.text = "";
		milestonIcon.sprite = unknownMilestoneIcon;
		fill.fillAmount = 0f;
		rewardButtonText.text = "0";
		fillText.text = "?? / <b>??";
		redeemParticles.SetActive(false);
		redeemButton.SetActive(false);
		contentWindow.SetActive(true);
	}

	public void pressRedeemButton()
	{
		SoundManager.manage.play2DSound(playSoundOnComplete);
		lookingAt.levelUp();
		Invoke("givePermitPointsDelay", 1.5f);
		fillMilestonDescription(lookingAt);
		updateMilestoneList();
		checkIfNeedNotification();
	}

	private void givePermitPointsDelay()
	{
		PermitPointsManager.manage.addPoints(lookingAt.getRewardAmount());
	}

	public void doATaskAndCountToMilestone(DailyTaskGenerator.genericTaskType type, int addAmount)
	{
		bool num = milestones[(int)type].checkIfLevelUpAvaliable();
		milestones[(int)type].addPoints(addAmount);
		if (!num && milestones[(int)type].checkIfLevelUpAvaliable())
		{
			StartCoroutine(newMilestoneDelay((int)type));
		}
		checkIfNeedNotification();
	}

	public void checkIfNeedNotification()
	{
		bool active = false;
		if (TownManager.manage.journalUnlocked)
		{
			for (int i = 0; i < milestones.Count; i++)
			{
				if (milestones[i].checkIfLevelUpAvaliable())
				{
					active = true;
					break;
				}
			}
		}
		for (int j = 0; j < notifications.Length; j++)
		{
			notifications[j].SetActive(active);
		}
	}

	private IEnumerator newMilestoneDelay(int milestoneId)
	{
		while (!newMilestoneNotification.transform.parent.gameObject.activeInHierarchy || newMilestoneNotification.playingIcon)
		{
			yield return null;
		}
		if (milestones[milestoneId].checkIfLevelUpAvaliable())
		{
			newMilestoneNotification.setIconAndPlayAnimation(milestoneId);
		}
	}
}
