using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InvButton : MonoBehaviour
{
	public bool isSellButton;

	public bool isCraftButton;

	public bool isCraftRecipeButton;

	public bool isConverstationOption;

	public int craftRecipeNumber;

	public ASound playOnPress;

	public ASound rollOverSound;

	public UnityEvent onButtonPress = new UnityEvent();

	private Animator anim;

	private ButtonAnimation buttonAnim;

	private bool soundPlayed;

	public Color hoverColor;

	public Color defaultColor;

	public Image imageToTint;

	private bool hasBeenPressed;

	public bool isACloseButton;

	public bool isAConfirmButton;

	public bool hovering;

	public bool disabled;

	public bool isSnappable = true;

	public bool currentlySelectedOnEnabled;

	private RectTransform myTrans;

	public void Awake()
	{
		if ((bool)imageToTint)
		{
			defaultColor = imageToTint.color;
		}
		anim = GetComponent<Animator>();
		buttonAnim = GetComponent<ButtonAnimation>();
		myTrans = GetComponent<RectTransform>();
	}

	public void PressButton()
	{
		if (!disabled && !hasBeenPressed)
		{
			hasBeenPressed = true;
			StartCoroutine(pressDelay());
		}
	}

	public void PressButtonDelay()
	{
		if (!base.isActiveAndEnabled || disabled)
		{
			return;
		}
		if (isConverstationOption)
		{
			ConversationManager.manage.buttonClick();
			playPressSound();
		}
		else if (isSellButton)
		{
			GiveNPC.give.CloseAndMakeOffer();
			playPressSound();
		}
		else if (isCraftRecipeButton)
		{
			playPressSound();
			CraftingManager.manage.showRecipeForItem(craftRecipeNumber);
		}
		else
		{
			if (isCraftButton)
			{
				if (CraftingManager.manage.canBeCrafted(CraftingManager.manage.craftableItemId))
				{
					playPressSound();
				}
				else
				{
					SoundManager.manage.play2DSound(SoundManager.manage.buttonCantPressSound);
				}
			}
			else
			{
				playPressSound();
			}
			onButtonPress.Invoke();
		}
		if ((bool)buttonAnim && (!isCraftButton || CraftingManager.manage.canBeCrafted(CraftingManager.manage.craftableItemId)) && base.gameObject.activeSelf)
		{
			buttonAnim.press();
		}
	}

	public void RollOver()
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (disabled)
		{
			hovering = false;
			return;
		}
		hovering = true;
		if ((bool)buttonAnim)
		{
			buttonAnim.hoverOver();
		}
		if (isConverstationOption)
		{
			ConversationManager.manage.selectWithButton(craftRecipeNumber);
		}
		if ((bool)rollOverSound)
		{
			if (!soundPlayed)
			{
				soundPlayed = true;
				SoundManager.manage.play2DSound(rollOverSound);
			}
		}
		else if (!soundPlayed)
		{
			soundPlayed = true;
			SoundManager.manage.play2DSound(SoundManager.manage.rollOverButton);
		}
		changeFadeColor(hoverColor);
	}

	public void RollOut()
	{
		if (disabled)
		{
			hovering = false;
		}
		hovering = false;
		if (soundPlayed)
		{
			soundPlayed = false;
		}
		if ((bool)buttonAnim)
		{
			buttonAnim.rollOut();
		}
		changeFadeColor(defaultColor);
	}

	private void playPressSound()
	{
		if (!playOnPress)
		{
			SoundManager.manage.play2DSound(SoundManager.manage.buttonSound);
		}
		else
		{
			SoundManager.manage.play2DSound(playOnPress);
		}
	}

	private void OnEnable()
	{
		if (isACloseButton)
		{
			Inventory.inv.setAsActiveCloseButton(this);
		}
		if (isAConfirmButton)
		{
			Inventory.inv.setAsActiveConfirmButton(this);
		}
		if ((bool)imageToTint)
		{
			imageToTint.color = defaultColor;
		}
		bool flag = (bool)buttonAnim;
		if (!isACloseButton && isSnappable)
		{
			Inventory.inv.buttonsToSnapTo.Add(myTrans);
		}
	}

	private void OnDisable()
	{
		Inventory.inv.buttonsToSnapTo.Remove(myTrans);
		if (isACloseButton)
		{
			Inventory.inv.removeAsActiveCloseButton(this);
		}
		if (isAConfirmButton)
		{
			Inventory.inv.removeAsActiveConfirmButton(this);
		}
		hasBeenPressed = false;
		hovering = false;
	}

	private void changeFadeColor(Color fadeTo)
	{
		if ((bool)imageToTint)
		{
			StopCoroutine("fadeTintToColor");
			if (base.gameObject.activeInHierarchy)
			{
				StartCoroutine(fadeTintToColor(fadeTo));
			}
		}
	}

	private IEnumerator fadeTintToColor(Color fadeTo)
	{
		float fadeSpeed = 0f;
		while (fadeSpeed <= 1f)
		{
			imageToTint.color = Color.Lerp(imageToTint.color, fadeTo, fadeSpeed);
			fadeSpeed += Time.deltaTime * 5f;
			yield return null;
		}
		imageToTint.color = fadeTo;
	}

	private IEnumerator pressDelay()
	{
		bool flag = (bool)buttonAnim;
		yield return null;
		PressButtonDelay();
		hasBeenPressed = false;
	}
}
