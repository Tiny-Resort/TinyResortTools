using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileObjectHealthBar : MonoBehaviour
{
	public static TileObjectHealthBar tile;

	public Image healthBar;

	public TextMeshProUGUI nameText;

	private bool currentlyCanShow;

	public TileObject currentlyTracking;

	public Damageable currentlyHitting;

	public GameObject toHide;

	private float currentlyShowingHealth;

	private float lerpedHealth;

	private float speed;

	public WindowAnimator win;

	private void Awake()
	{
		tile = this;
	}

	private void Update()
	{
		if (currentlyCanShow)
		{
			if ((bool)currentlyHitting)
			{
				base.transform.position = CameraController.control.mainCamera.WorldToScreenPoint(currentlyHitting.transform.position + Vector3.up * 4f);
				if (currentlyHitting.health < currentlyHitting.maxHealth && currentlyHitting.health > 0)
				{
					float num = (float)currentlyHitting.health / (float)currentlyHitting.maxHealth;
					currentlyShowingHealth = (float)currentlyHitting.health / (float)currentlyHitting.maxHealth;
					if (num != currentlyShowingHealth)
					{
						win.bounceOnDamage();
					}
					if (lerpedHealth != currentlyShowingHealth && Mathf.Abs(lerpedHealth - currentlyShowingHealth) < 0.01f)
					{
						win.bounceOnDamage();
						healthBar.fillAmount = currentlyShowingHealth;
						lerpedHealth = currentlyShowingHealth;
					}
					else
					{
						lerpedHealth = Mathf.SmoothDamp(lerpedHealth, currentlyShowingHealth, ref speed, 0.15f);
					}
					healthBar.fillAmount = lerpedHealth;
					toHide.SetActive(true);
				}
				else
				{
					if (currentlyHitting.health <= 0)
					{
						currentlyHitting = null;
					}
					toHide.SetActive(false);
				}
				if (!currentlyHitting || !currentlyHitting.gameObject.activeInHierarchy || (!CameraController.control.isInAimCam() && Vector3.Distance(CameraController.control.transform.position, currentlyHitting.transform.position) > 10f) || (CameraController.control.isInAimCam() && Vector3.Distance(CameraController.control.transform.position, currentlyHitting.transform.position) > 50f))
				{
					currentlyHitting = null;
					toHide.SetActive(false);
				}
			}
			else if ((bool)currentlyTracking)
			{
				base.transform.position = CameraController.control.mainCamera.WorldToScreenPoint(currentlyTracking.transform.position + Vector3.up * 2f);
				if (currentlyTracking.currentHealth < WorldManager.manageWorld.allObjectSettings[currentlyTracking.tileObjectId].fullHealth && currentlyTracking.currentHealth > 0f)
				{
					float num2 = currentlyShowingHealth;
					currentlyShowingHealth = currentlyTracking.currentHealth / WorldManager.manageWorld.allObjectSettings[currentlyTracking.tileObjectId].fullHealth;
					if (num2 != currentlyShowingHealth)
					{
						win.bounceOnDamage();
					}
					if (lerpedHealth != currentlyShowingHealth && Mathf.Abs(lerpedHealth - currentlyShowingHealth) < 0.01f)
					{
						healthBar.fillAmount = currentlyShowingHealth;
						lerpedHealth = currentlyShowingHealth;
					}
					else
					{
						lerpedHealth = Mathf.SmoothDamp(lerpedHealth, currentlyShowingHealth, ref speed, 0.15f);
					}
					healthBar.fillAmount = lerpedHealth;
					toHide.SetActive(true);
				}
				else
				{
					toHide.SetActive(false);
				}
			}
			else
			{
				toHide.SetActive(false);
			}
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}

	public void canBeShown(bool show)
	{
		currentlyCanShow = show;
		if (!show)
		{
			base.gameObject.SetActive(false);
		}
	}

	public void setCurrentlyAttacking(TileObject currentlyAttacking)
	{
		if (currentlyCanShow && currentlyAttacking != currentlyTracking)
		{
			currentlyTracking = currentlyAttacking;
			if (toHide.activeInHierarchy)
			{
				win.openAgain();
			}
			if ((bool)currentlyAttacking && !currentlyHitting)
			{
				currentlyShowingHealth = currentlyTracking.currentHealth / WorldManager.manageWorld.allObjectSettings[currentlyTracking.tileObjectId].fullHealth;
				lerpedHealth = currentlyShowingHealth;
				nameText.text = currentlyAttacking.name;
				healthBar.fillAmount = currentlyShowingHealth;
				base.gameObject.SetActive(true);
			}
			else if (!currentlyHitting)
			{
				base.gameObject.SetActive(false);
			}
		}
	}

	public void setCurrentlyHitting(Damageable newCurrentlyHitting)
	{
		if (currentlyCanShow)
		{
			currentlyHitting = newCurrentlyHitting;
			if ((bool)currentlyHitting)
			{
				currentlyShowingHealth = (float)currentlyHitting.health / (float)currentlyHitting.maxHealth;
				lerpedHealth = currentlyShowingHealth;
				nameText.text = currentlyHitting.name;
				healthBar.fillAmount = currentlyShowingHealth;
				base.gameObject.SetActive(true);
			}
			else
			{
				base.gameObject.SetActive(false);
			}
		}
	}
}
