using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionWindow : MonoBehaviour
{
	public TextMeshProUGUI OptionText;

	public Image OptionBox;

	public Color selectedColor;

	private Color origColor;

	private void Start()
	{
		origColor = OptionBox.color;
	}

	public void setOptionText(string newOptionText)
	{
		OptionText.text = newOptionText;
	}

	public void selectedOrNot(bool selected)
	{
		if (selected)
		{
			OptionBox.color = selectedColor;
		}
		else
		{
			OptionBox.color = origColor;
		}
	}

	public void showOptionBox()
	{
		OptionBox.enabled = true;
		OptionText.enabled = true;
	}
}
