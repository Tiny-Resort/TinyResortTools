using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControllerKeyboard : MonoBehaviour
{
	public static ControllerKeyboard keyboard;

	public TMP_InputField keyboardField;

	public GameObject window;

	private TMP_InputField toPlaceInto;

	public OnScreenKeyboardButton[] allButtons;

	public Image shiftKey;

	public Color shiftKeyDown;

	public Color shiftKeyNormal;

	private bool uppercase;

	private void Awake()
	{
		keyboard = this;
	}

	private void Start()
	{
		updateAllButtons();
	}

	public void openKeyboard(TMP_InputField fieldToEnter)
	{
		if (!Inventory.inv.usingMouse)
		{
			keyboardField.text = fieldToEnter.text;
			updateAllButtons();
			toPlaceInto = fieldToEnter;
			window.SetActive(true);
			if (!isUpperCase())
			{
				swapUpperCase();
			}
			StartCoroutine(controllerShortcuts());
			keyboardField.Select();
		}
		else
		{
			fieldToEnter.Select();
		}
	}

	public void closeKeyboard()
	{
		toPlaceInto.text = keyboardField.text;
		window.SetActive(false);
	}

	public void pressKeyboardButton(string buttonKey)
	{
		if (isUpperCase())
		{
			keyboardField.text += buttonKey.ToUpper();
		}
		else
		{
			keyboardField.text += buttonKey;
		}
		if (isUpperCase())
		{
			swapUpperCase();
		}
		keyboardField.caretPosition = keyboardField.text.Length;
	}

	public void backSpace()
	{
		string text = "";
		for (int i = 0; i < keyboardField.text.Length - 1; i++)
		{
			text += keyboardField.text[i];
		}
		keyboardField.text = text;
	}

	public void swapUpperCase()
	{
		uppercase = !uppercase;
		updateAllButtons();
	}

	public bool isUpperCase()
	{
		return uppercase;
	}

	public void updateAllButtons()
	{
		OnScreenKeyboardButton[] array = allButtons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].updateButtonTextCase();
		}
		if (isUpperCase())
		{
			shiftKey.color = shiftKeyDown;
		}
		else
		{
			shiftKey.color = shiftKeyNormal;
		}
	}

	private IEnumerator controllerShortcuts()
	{
		bool capslockOn = false;
		float backspaceHeldTimer = 0f;
		float extrawaitTimer = 0.2f;
		while (window.activeInHierarchy)
		{
			if (Inventory.inv.usingMouse)
			{
				closeKeyboard();
			}
			yield return true;
			if (InputMaster.input.KeyboardUpperCase())
			{
				swapUpperCase();
				capslockOn = isUpperCase();
				SoundManager.manage.play2DSound(SoundManager.manage.cameraSwitch);
			}
			if (capslockOn && !isUpperCase())
			{
				swapUpperCase();
			}
			if (InputMaster.input.Use() && keyboardField.text.Length > 0)
			{
				backSpace();
				SoundManager.manage.play2DSound(SoundManager.manage.deselectSlotForGive);
			}
			if (InputMaster.input.Other())
			{
				pressKeyboardButton(" ");
				SoundManager.manage.play2DSound(SoundManager.manage.buttonSound);
			}
			if (InputMaster.input.UseHeld() && keyboardField.text.Length > 0)
			{
				if (backspaceHeldTimer > extrawaitTimer)
				{
					backspaceHeldTimer = 0f;
					extrawaitTimer = Mathf.Clamp(extrawaitTimer - 0.05f, 0.05f, 1f);
					backSpace();
					SoundManager.manage.play2DSound(SoundManager.manage.deselectSlotForGive);
				}
				else
				{
					backspaceHeldTimer += Time.deltaTime;
				}
			}
			else
			{
				extrawaitTimer = 0.2f;
				backspaceHeldTimer = 0f;
			}
		}
	}
}
