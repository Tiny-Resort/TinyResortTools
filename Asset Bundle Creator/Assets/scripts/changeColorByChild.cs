using UnityEngine;
using UnityEngine.UI;

public class changeColorByChild : MonoBehaviour
{
	public Color evenColor;

	public Color oddColor;

	public Image image;

	private void OnEnable()
	{
		if (base.transform.GetSiblingIndex() % 2 == 0)
		{
			image.color = evenColor;
		}
		else
		{
			image.color = oddColor;
		}
	}
}
