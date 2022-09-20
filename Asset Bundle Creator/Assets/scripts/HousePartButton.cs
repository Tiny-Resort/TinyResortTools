using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HousePartButton : MonoBehaviour
{
	public HouseExterior displayHouse;

	public int partId;

	public TextMeshProUGUI buttonText;

	public PlayerHouseExterior.houseParts changingPart;

	public bool isPaintButton;

	public Image colorImage;

	public RawImage textureImage;

	private Color myColor;

	private bool textureButton;

	private void Start()
	{
		if (isPaintButton)
		{
			myColor = colorImage.color;
			colorImage.raycastTarget = false;
		}
	}

	public void setUpButton(HouseExterior display, int myPartId, PlayerHouseExterior.houseParts part)
	{
		displayHouse = display;
		partId = myPartId;
		changingPart = part;
		buttonText.text = partId.ToString() ?? "";
	}

	public void setUpTextureButton(HouseExterior display, int myPartId, PlayerHouseExterior.houseParts part, Material textureToShow)
	{
		displayHouse = display;
		partId = myPartId;
		changingPart = part;
		buttonText.text = partId.ToString() ?? "";
		textureButton = true;
		textureImage.texture = textureToShow.GetTexture("_MainTex");
		textureImage.gameObject.SetActive(true);
	}

	public void press()
	{
		if (textureButton)
		{
			if (changingPart == PlayerHouseExterior.houseParts.houseBase)
			{
				displayHouse.wallMat = partId;
			}
			else if (changingPart == PlayerHouseExterior.houseParts.roof)
			{
				displayHouse.roofMat = partId;
			}
			else if (changingPart == PlayerHouseExterior.houseParts.houseDetailsColor)
			{
				displayHouse.houseMat = partId;
			}
			HouseEditor.edit.updateDummy();
			return;
		}
		if (isPaintButton)
		{
			pressPaintButton();
			return;
		}
		if (changingPart == PlayerHouseExterior.houseParts.houseBase)
		{
			displayHouse.houseBase = partId;
		}
		else if (changingPart == PlayerHouseExterior.houseParts.roof)
		{
			displayHouse.roof = partId;
		}
		else if (changingPart == PlayerHouseExterior.houseParts.window)
		{
			displayHouse.windows = partId;
		}
		else if (changingPart == PlayerHouseExterior.houseParts.door)
		{
			displayHouse.door = partId;
		}
		HouseEditor.edit.updateDummy();
	}

	public void pressPaintButton()
	{
		if (HouseEditor.edit.currentlyPainting == PlayerHouseExterior.houseParts.houseBase)
		{
			HouseEditor.edit.dummyExterior.wallColor = "#" + ColorUtility.ToHtmlStringRGB(myColor);
		}
		else if (HouseEditor.edit.currentlyPainting == PlayerHouseExterior.houseParts.roof)
		{
			HouseEditor.edit.dummyExterior.roofColor = "#" + ColorUtility.ToHtmlStringRGB(myColor);
		}
		else if (HouseEditor.edit.currentlyPainting == PlayerHouseExterior.houseParts.houseDetailsColor)
		{
			HouseEditor.edit.dummyExterior.houseColor = "#" + ColorUtility.ToHtmlStringRGB(myColor);
		}
		HouseEditor.edit.tintColorButtons(myColor);
		HouseEditor.edit.updateDummy();
	}
}
