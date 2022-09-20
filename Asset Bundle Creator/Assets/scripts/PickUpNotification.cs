using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickUpNotification : MonoBehaviour
{
	public Image itemImage;

	public TextMeshProUGUI itemText;

	public Sprite chatSprite;

	public Sprite controllerB;

	public Sprite controllerX;

	public Sprite controllerRightClick;

	public Sprite controllerLeftClick;

	public Sprite controllerY;

	public Sprite controllerE;

	public Sprite controllerZ;

	public Sprite controllerRightStick;

	public Sprite cantSprite;

	public Animator promptAnim;

	public int showingStack = -1;

	public int showingItemId = -1;

	public Image backgroundBox;

	private Color endColour;

	private string lastButtonPromptText;

	private void OnEnable()
	{
		if ((bool)backgroundBox)
		{
			if (endColour == Color.clear)
			{
				endColour = backgroundBox.color;
			}
			StartCoroutine(backgroundDelay());
		}
	}

	private IEnumerator backgroundDelay()
	{
		Color backgroundFadeFrom = backgroundBox.color;
		backgroundFadeFrom.a = 0f;
		backgroundBox.color = backgroundFadeFrom;
		itemText.maxVisibleCharacters = 0;
		float timer2 = 0f;
		float lastVisibleCharacters = itemText.text.Length;
		while (timer2 <= 1f)
		{
			yield return null;
			timer2 += Time.deltaTime * 5f;
			backgroundBox.color = Color.Lerp(backgroundFadeFrom, endColour, timer2);
		}
		timer2 = 0f;
		while (timer2 <= 1f)
		{
			yield return null;
			timer2 += Time.deltaTime * 5f;
			itemText.maxVisibleCharacters = Mathf.RoundToInt(Mathf.Lerp(0f, lastVisibleCharacters, timer2));
		}
		backgroundBox.color = endColour;
		itemText.maxVisibleCharacters = 200;
	}

	public void fillItemDetails(int itemId, int stackAmount)
	{
		itemImage.sprite = Inventory.inv.allItems[itemId].getSprite();
		if (stackAmount > 1 && !Inventory.inv.allItems[itemId].hasFuel && !Inventory.inv.allItems[itemId].hasColourVariation)
		{
			itemText.text = Inventory.inv.allItems[itemId].getInvItemName() + " <size=9>X</size> " + stackAmount;
		}
		else
		{
			itemText.text = Inventory.inv.allItems[itemId].getInvItemName();
		}
		itemImage.rectTransform.localPosition = new Vector3(0f - (itemText.preferredWidth + 40f), 0f, 0f);
		showingStack = stackAmount;
		showingItemId = itemId;
	}

	public void fillChatDetails(string chatString)
	{
		itemText.fontSize += 1f;
		itemImage.sprite = chatSprite;
		itemText.text = chatString;
		itemImage.rectTransform.localPosition = new Vector3(0f - (itemText.preferredWidth + 35f), 0f, 0f);
	}

	public void fillButtonPrompt(string buttonPromptText, Sprite buttonSprite)
	{
		if (promptAnim.isActiveAndEnabled && buttonPromptText != lastButtonPromptText)
		{
			promptAnim.SetTrigger("Bounce");
			lastButtonPromptText = buttonPromptText;
		}
		if (buttonSprite == null)
		{
			itemImage.enabled = false;
		}
		else
		{
			itemImage.enabled = true;
		}
		itemImage.sprite = buttonSprite;
		itemText.text = buttonPromptText;
		itemImage.rectTransform.localPosition = new Vector3(0f - (itemText.preferredWidth + 40f), 0f, 0f);
	}
}
