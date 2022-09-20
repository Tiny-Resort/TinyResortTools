using UnityEngine;
using UnityEngine.UI;

public class FadeImagesAndText : MonoBehaviour
{
	public Image[] imagesToFade;

	public Color[] imageColorToChangeTo;

	private Color[] imageColorOrig;

	public Text[] textToFade;

	private Color[] textColorOrig;

	public Color[] textColorToChangeTo;

	private void Awake()
	{
		imageColorOrig = new Color[imagesToFade.Length];
		for (int i = 0; i < imagesToFade.Length; i++)
		{
			imageColorOrig[i] = imagesToFade[i].color;
		}
		textColorOrig = new Color[textToFade.Length];
		for (int j = 0; j < textToFade.Length; j++)
		{
			textColorOrig[j] = textToFade[j].color;
		}
	}

	public void isFaded(bool isFaded)
	{
		for (int i = 0; i < imagesToFade.Length; i++)
		{
			if (isFaded)
			{
				imagesToFade[i].color = imageColorToChangeTo[i];
			}
			else
			{
				imagesToFade[i].color = imageColorOrig[i];
			}
		}
		for (int j = 0; j < textToFade.Length; j++)
		{
			if (textToFade[j] != null)
			{
				if (isFaded)
				{
					textToFade[j].color = textColorToChangeTo[j];
				}
				else if (textColorOrig != null && textColorOrig.Length > j)
				{
					textToFade[j].color = textColorOrig[j];
				}
			}
		}
	}
}
