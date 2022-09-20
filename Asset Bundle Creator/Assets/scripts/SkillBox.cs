using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillBox : MonoBehaviour
{
	public Image barFill;

	public TextMeshProUGUI levelNo;

	public bool completed = true;

	public AudioSource fillSound;

	public int frameCount = 1;

	public void setToCurrent(int skill, int current)
	{
		levelNo.text = "Lvl " + CharLevelManager.manage.currentLevels[skill];
		barFill.fillAmount = (float)current / (float)CharLevelManager.manage.getLevelRequiredXP(skill);
	}

	public IEnumerator fillProgressBar(int skill, int starting, int finish)
	{
		levelNo.text = "Lvl " + CharLevelManager.manage.currentLevels[skill];
		float num = (float)starting / (float)CharLevelManager.manage.getLevelRequiredXP(skill);
		float finishPos = (float)finish / (float)CharLevelManager.manage.getLevelRequiredXP(skill);
		float fillAmount2 = num;
		fillSound.Play();
		while (fillAmount2 < finishPos)
		{
			barFill.fillAmount = fillAmount2;
			fillAmount2 = Mathf.Clamp(fillAmount2 + 0.01f, 0f, finishPos);
			fillSound.volume = 0.35f * SoundManager.manage.getUiVolume();
			fillSound.pitch = 2f + fillAmount2 * 6f;
			if (InputMaster.input.UISelectHeld() || InputMaster.input.UIAltHeld())
			{
				if (frameCount == 1)
				{
					yield return null;
					frameCount = 0;
				}
				else
				{
					frameCount = 1;
				}
			}
			else
			{
				yield return null;
			}
		}
		fillSound.Stop();
		fillAmount2 = finishPos;
		barFill.fillAmount = fillAmount2;
	}

	public IEnumerator levelUp(int newLevel)
	{
		levelNo.text = "Lvl " + newLevel;
		levelNo.GetComponent<InvSlotAnimator>().UpdateSlotContents();
		SoundManager.manage.play2DSound(SoundManager.manage.levelUpSound);
		yield return null;
		yield return null;
		yield return null;
		yield return null;
	}
}
