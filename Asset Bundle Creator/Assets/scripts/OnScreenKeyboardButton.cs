using TMPro;
using UnityEngine;

public class OnScreenKeyboardButton : MonoBehaviour
{
	public string keyboardKey;

	public TextMeshProUGUI keyboardText;

	public void updateButtonTextCase()
	{
		if ((bool)keyboardText)
		{
			if (ControllerKeyboard.keyboard.isUpperCase())
			{
				keyboardText.text = keyboardKey.ToUpper();
			}
			else
			{
				keyboardText.text = keyboardKey;
			}
		}
	}

	public void onPress()
	{
		ControllerKeyboard.keyboard.pressKeyboardButton(keyboardKey);
	}

	public void swapCase()
	{
		ControllerKeyboard.keyboard.swapUpperCase();
	}
}
