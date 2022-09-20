using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TemperatureManager : MonoBehaviour
{
	public Image thermometreIcon;

	public TextMeshProUGUI tempText;

	private int lastTemp;

	private void FixedUpdate()
	{
		int placeTemperature = GenerateMap.generate.getPlaceTemperature(CameraController.control.transform.position);
		if (lastTemp != placeTemperature)
		{
			tempText.text = placeTemperature + "°";
			lastTemp = placeTemperature;
		}
	}
}
