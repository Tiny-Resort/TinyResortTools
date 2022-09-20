using UnityEngine;
using UnityEngine.UI;

public class HeartContainer : MonoBehaviour
{
	public Image heartCentre;

	public Animator heartAnim;

	public int showing20From;

	private int showingHealth = -200;

	public void updateHealth(int newHealth)
	{
		if (showingHealth == newHealth)
		{
			return;
		}
		showingHealth = newHealth;
		if (newHealth < showing20From)
		{
			if (heartCentre.fillAmount > 0f && (bool)heartAnim && !heartAnim.enabled)
			{
				heartAnim.SetTrigger("Bounce");
			}
			heartCentre.fillAmount = 0f;
			return;
		}
		if (newHealth > showing20From + 20)
		{
			if (heartCentre.fillAmount < 1f && (bool)heartAnim && !heartAnim.enabled)
			{
				heartAnim.SetTrigger("Bounce");
			}
			heartCentre.fillAmount = 1f;
			return;
		}
		float num = (float)(newHealth - showing20From) / 20f;
		if (heartCentre.fillAmount != (float)Mathf.RoundToInt(num * 4f) / 4f)
		{
			if ((bool)heartAnim && !heartAnim.enabled)
			{
				heartAnim.SetTrigger("Bounce");
			}
			heartCentre.fillAmount = (float)Mathf.RoundToInt(num * 4f) / 4f;
		}
	}
}
