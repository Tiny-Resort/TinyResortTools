using UnityEngine;
using UnityEngine.UI;

public class UIScrollBar : MonoBehaviour
{
	public RectTransform scrollBar;

	public RectTransform scroolBack;

	public RectTransform limitWindow;

	public RectTransform windowThatScrolls;

	private bool holdingClick;

	private float windowThatScrollsHeight;

	private Image scrollBackgroundImage;

	private Vector2 desiredScrollPos = Vector2.zero;

	private void Start()
	{
		scrollBackgroundImage = scroolBack.GetComponent<Image>();
	}

	private void Update()
	{
		if (scrollBar.sizeDelta.y >= scroolBack.sizeDelta.y)
		{
			scrollBar.gameObject.SetActive(false);
			scrollBackgroundImage.enabled = false;
		}
		else
		{
			scrollBar.gameObject.SetActive(true);
			scrollBackgroundImage.enabled = true;
		}
		if (windowThatScrollsHeight != windowThatScrolls.sizeDelta.y)
		{
			windowThatScrollsHeight = windowThatScrolls.sizeDelta.y;
			if (windowThatScrolls.sizeDelta.y < limitWindow.sizeDelta.y - 5f)
			{
				desiredScrollPos = Vector2.zero;
			}
			else
			{
				desiredScrollPos = new Vector2(windowThatScrolls.anchoredPosition.x, Mathf.Clamp(desiredScrollPos.y, 0f, windowThatScrolls.sizeDelta.y - limitWindow.sizeDelta.y));
			}
		}
		scrollBar.sizeDelta = new Vector2(scrollBar.sizeDelta.x, Mathf.Clamp(scroolBack.sizeDelta.y / (windowThatScrolls.sizeDelta.y / limitWindow.sizeDelta.y), 15f, scroolBack.sizeDelta.y));
		windowThatScrolls.anchoredPosition = Vector2.Lerp(windowThatScrolls.anchoredPosition, desiredScrollPos, Time.deltaTime * 10f);
		scrollBar.anchoredPosition = new Vector2(scrollBar.anchoredPosition.x, (0f - desiredScrollPos.y) / scroolBack.sizeDelta.y * scrollBar.sizeDelta.y);
		if (holdingClick)
		{
			if ((Inventory.inv.usingMouse && InputMaster.input.UISelectHeld()) || (Inventory.inv.usingMouse && InputMaster.input.UIAltHeld()))
			{
				pressScrollBackAndMoveToPoint();
			}
			else
			{
				holdingClick = false;
			}
		}
	}

	private void OnEnable()
	{
		Inventory.inv.activeScrollBar = this;
	}

	private void OnDisable()
	{
		if (Inventory.inv.activeScrollBar == this)
		{
			Inventory.inv.activeScrollBar = null;
		}
	}

	public void resetToTop()
	{
		windowThatScrolls.anchoredPosition = Vector2.zero;
		desiredScrollPos = Vector2.zero;
	}

	public void sendToBottom()
	{
		if (windowThatScrolls.sizeDelta.y < limitWindow.sizeDelta.y - 5f)
		{
			desiredScrollPos = Vector2.zero;
		}
		else
		{
			desiredScrollPos = new Vector2(windowThatScrolls.anchoredPosition.x, windowThatScrolls.sizeDelta.y - limitWindow.sizeDelta.y + 5f);
		}
		windowThatScrolls.anchoredPosition = Vector2.Lerp(windowThatScrolls.anchoredPosition, desiredScrollPos, 1f);
	}

	public void pressScrollBackAndMoveToPoint()
	{
		holdingClick = true;
		float num = Mathf.Clamp((scroolBack.transform.position - Inventory.inv.cursor.position).y - scrollBar.sizeDelta.y / 2f, 0f, scroolBack.sizeDelta.y);
		if (windowThatScrolls.sizeDelta.y < limitWindow.sizeDelta.y - 5f)
		{
			desiredScrollPos = Vector2.zero;
		}
		else
		{
			desiredScrollPos = new Vector2(windowThatScrolls.anchoredPosition.x, Mathf.Clamp(num / scroolBack.sizeDelta.y * windowThatScrolls.sizeDelta.y, 0f, windowThatScrolls.sizeDelta.y - limitWindow.sizeDelta.y));
		}
	}

	public bool ifInScrollBarIsVisible(RectTransform check)
	{
		if (check.IsChildOf(windowThatScrolls))
		{
			if (check.transform.position.y + check.sizeDelta.y * 2.7f >= limitWindow.transform.position.y && check.position.y - check.sizeDelta.y * 2.7f <= limitWindow.transform.position.y + limitWindow.sizeDelta.y)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public void moveScrollBarForInvSnapping(float scrollAmount)
	{
		if (Inventory.inv.currentlySelected.IsChildOf(windowThatScrolls))
		{
			scrollUpOrDown(scrollAmount);
		}
	}

	public void moveDirectlyToSelectedButton()
	{
		if (Inventory.inv.snapCursorOn && !Inventory.inv.usingMouse && scrollBar.gameObject.activeInHierarchy && Inventory.inv.currentlySelected.IsChildOf(windowThatScrolls))
		{
			desiredScrollPos = new Vector2(windowThatScrolls.anchoredPosition.x, Mathf.Clamp(0f - (Inventory.inv.currentlySelected.localPosition.y + Inventory.inv.currentlySelected.sizeDelta.y), 0f, windowThatScrolls.sizeDelta.y - limitWindow.sizeDelta.y));
		}
	}

	public void scrollUpOrDown(float amount)
	{
		if (amount != 0f)
		{
			if (windowThatScrolls.sizeDelta.y < limitWindow.sizeDelta.y - 5f)
			{
				desiredScrollPos = Vector2.zero;
			}
			else
			{
				desiredScrollPos = new Vector2(windowThatScrolls.anchoredPosition.x, Mathf.Clamp(desiredScrollPos.y + amount, 0f, windowThatScrolls.sizeDelta.y - limitWindow.sizeDelta.y));
			}
		}
	}
}
