using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
	public static LoadingScreen load;

	public FadeBlackness blackness;

	public TextMeshProUGUI loadingText;

	public Image loadingBar;

	public Animator loadingAnim;

	public LoadingScreenImageAndTips loadingScreenImages;

	public TopNotification saveGameConfirmed;

	public void appear(string screenText, bool loadingTipsOn = false)
	{
		loadingAnim.SetBool("Completed", false);
		loadingAnim.gameObject.SetActive(true);
		base.gameObject.SetActive(true);
		loadingBar.fillAmount = 0f;
		loadingText.text = screenText;
		blackness.fadeIn();
		loadingScreenImages.gameObject.SetActive(loadingTipsOn);
	}

	public void disappear()
	{
		loadingAnim.gameObject.SetActive(false);
		loadingScreenImages.fadeAway();
		blackness.fadeOut();
		Invoke("hideAfterUse", 1f);
	}

	private void hideAfterUse()
	{
		base.gameObject.SetActive(false);
	}

	public void showPercentage(float currentPercent)
	{
		loadingBar.fillAmount = currentPercent;
	}

	public void loadingBarOnlyAppear()
	{
		loadingAnim.SetBool("Completed", false);
		loadingAnim.gameObject.SetActive(true);
		loadingBar.fillAmount = 0f;
	}

	public void completed()
	{
		loadingAnim.SetBool("Completed", true);
	}
}
