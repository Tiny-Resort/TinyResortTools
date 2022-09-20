using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WindowAnimator : MonoBehaviour
{
	public Image blurBorder;

	private Material mat;

	public RectTransform mask;

	public Image maskChild;

	public GameObject contents;

	public bool playOpenAndCloseSound;

	private Color maskChildColor;

	public Color fadedChildColor;

	private Vector2 transformStartingPos = Vector2.zero;

	public float openDelay;

	private RectTransform myTrans;

	public bool doubleSpeed;

	public bool halfSpeed;

	public bool dontChangeLocalPos;

	private void Awake()
	{
		myTrans = GetComponent<RectTransform>();
		if ((bool)maskChild)
		{
			maskChildColor = maskChild.color;
		}
		transformStartingPos = myTrans.anchoredPosition;
	}

	public Vector2 moveWindowAndReturnOriginalPos(Vector2 newPos)
	{
		Vector2 result = transformStartingPos;
		myTrans = GetComponent<RectTransform>();
		myTrans.anchoredPosition = newPos;
		transformStartingPos = newPos;
		return result;
	}

	private void OnEnable()
	{
		if ((bool)blurBorder && mat == null)
		{
			mat = new Material(Shader.Find("UI/Blurred"));
			blurBorder.material = mat;
		}
		if ((bool)blurBorder)
		{
			blurBorder.enabled = false;
			blurBorder.material.SetFloat("_Size", 0f);
		}
		StartCoroutine(AnimateOpen());
		if ((bool)blurBorder)
		{
			StartCoroutine(blurBackground());
		}
	}

	public void openAgain()
	{
		StartCoroutine(AnimateOpen());
	}

	private IEnumerator AnimateOpen()
	{
		if (playOpenAndCloseSound)
		{
			SoundManager.manage.soundOnOpenWindow();
		}
		if ((bool)mask)
		{
			contents.SetActive(false);
			if (openDelay > 0f)
			{
				maskChild.enabled = false;
				yield return new WaitForSeconds(openDelay);
			}
			float journey3 = 0f;
			float duration3 = 0.25f;
			if (doubleSpeed)
			{
				duration3 = 0.125f;
			}
			if (halfSpeed)
			{
				duration3 *= 2f;
			}
			maskChild.enabled = true;
			maskChild.color = maskChildColor;
			while (journey3 <= duration3)
			{
				journey3 += Time.deltaTime;
				float num = Mathf.Clamp01(journey3 / duration3);
				float num2 = Mathf.LerpUnclamped(0.01f, 1f, num);
				maskChild.transform.SetParent(null);
				mask.localScale = new Vector3(num2, num2, num2);
				maskChild.transform.SetParent(mask);
				if (!dontChangeLocalPos)
				{
					maskChild.transform.localPosition = Vector3.zero;
				}
				if (!dontChangeLocalPos)
				{
					if (num <= 0.5f)
					{
						myTrans.anchoredPosition = Vector2.Lerp(transformStartingPos - Vector2.up * 25f, transformStartingPos + Vector2.up * 15f, num * 2f);
					}
					else
					{
						myTrans.anchoredPosition = Vector2.Lerp(transformStartingPos + Vector2.up * 15f, transformStartingPos - Vector2.up * 10f, (num - 0.5f) * 2f);
					}
				}
				yield return null;
			}
			journey3 = 0f;
			duration3 = 0.15f;
			contents.gameObject.SetActive(true);
			while (journey3 <= duration3)
			{
				journey3 += Time.deltaTime;
				maskChild.color = Color.Lerp(maskChildColor, fadedChildColor, Mathf.Clamp01(journey3 / duration3));
				if (!dontChangeLocalPos)
				{
					myTrans.anchoredPosition = Vector3.Lerp(transformStartingPos - Vector2.up * 10f, transformStartingPos, Mathf.Clamp01(journey3 / duration3));
				}
				yield return null;
			}
			maskChild.enabled = false;
		}
		else
		{
			if (openDelay > 0f)
			{
				base.transform.localScale = new Vector3(0f, 0f, 0f);
				yield return new WaitForSeconds(openDelay);
			}
			float duration3 = 0f;
			float journey3 = 0.35f;
			while (duration3 <= journey3)
			{
				duration3 += Time.deltaTime;
				float time = Mathf.Clamp01(duration3 / journey3);
				float t = UIAnimationManager.manage.windowsOpenCurve.Evaluate(time);
				float num3 = Mathf.LerpUnclamped(0.75f, 1f, t);
				base.transform.localScale = new Vector3(num3, num3, num3);
				yield return null;
			}
		}
		Inventory.inv.checkAllClickableButtons();
	}

	private IEnumerator blurBackground()
	{
		if ((bool)blurBorder)
		{
			blurBorder.enabled = true;
			float blur = 0f;
			float blurTimer = 0f;
			while (blur < 1f)
			{
				blurTimer += Time.deltaTime * 2f;
				blurBorder.material.SetFloat("_Size", blur);
				blur = Mathf.Lerp(blur, 1f, blurTimer);
				yield return null;
			}
		}
	}

	public IEnumerator closeWithMask(float speedMod = 1f)
	{
		maskChild.enabled = true;
		float journey2 = 0f;
		float duration2 = 0.15f / speedMod;
		contents.gameObject.SetActive(true);
		while (journey2 <= duration2)
		{
			journey2 += Time.deltaTime;
			maskChild.color = Color.Lerp(fadedChildColor, maskChildColor, Mathf.Clamp01(journey2 / duration2));
			yield return null;
		}
		contents.SetActive(false);
		journey2 = 0f;
		duration2 = 0.25f / speedMod;
		maskChild.enabled = true;
		maskChild.color = maskChildColor;
		while (journey2 <= duration2)
		{
			journey2 += Time.deltaTime;
			float t = Mathf.Clamp01(journey2 / duration2);
			float num = Mathf.LerpUnclamped(1f, 0.01f, t);
			maskChild.transform.SetParent(null);
			mask.localScale = new Vector3(num, num, num);
			maskChild.transform.SetParent(mask);
			if (!dontChangeLocalPos)
			{
				maskChild.transform.localPosition = Vector3.zero;
			}
			yield return null;
		}
		maskChild.enabled = false;
		base.gameObject.SetActive(false);
	}

	public void bounceOnDamage()
	{
		if (base.gameObject.activeSelf)
		{
			StartCoroutine(statBounceOnDamage());
		}
	}

	private IEnumerator statBounceOnDamage()
	{
		float timer4 = 0f;
		while (timer4 <= 1f)
		{
			contents.transform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(0f, 10f, 0f), timer4);
			timer4 += Time.deltaTime * 20f;
			yield return null;
		}
		timer4 = 0f;
		while (timer4 <= 1f)
		{
			contents.transform.localPosition = Vector3.Lerp(new Vector3(0f, 10f, 0f), new Vector3(0f, -5f, 0f), timer4);
			timer4 += Time.deltaTime * 15f;
			yield return null;
		}
		timer4 = 0f;
		while (timer4 <= 1f)
		{
			contents.transform.localPosition = Vector3.Lerp(new Vector3(0f, -5f, 0f), new Vector3(0f, 2.5f, 0f), timer4);
			timer4 += Time.deltaTime * 10f;
			yield return null;
		}
		timer4 = 0f;
		while (timer4 <= 1f)
		{
			contents.transform.localPosition = Vector3.Lerp(new Vector3(0f, 2.5f, 0f), Vector3.zero, timer4);
			timer4 += Time.deltaTime * 10f;
			yield return null;
		}
	}

	private void OnDisable()
	{
		if ((bool)blurBorder)
		{
			blurBorder.enabled = false;
		}
		if (playOpenAndCloseSound)
		{
			SoundManager.manage.soundOnCloseWindow();
		}
		Inventory.inv.checkAllClickableButtons();
	}

	public void printMessage()
	{
		GameObject obj = base.gameObject;
		MonoBehaviour.print("Window Animator not here " + (((object)obj != null) ? obj.ToString() : null));
	}

	public void refreshAnimation()
	{
		if (base.isActiveAndEnabled)
		{
			StopCoroutine("AnimateOpen");
			StartCoroutine("AnimateOpen");
		}
	}
}
