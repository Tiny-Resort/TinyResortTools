using UnityEngine;

public class PlayerHouseExterior : MonoBehaviour
{
	public enum houseParts
	{
		houseBase = 0,
		door = 1,
		window = 2,
		roof = 3,
		houseDetailsColor = 4
	}

	public int levelNo = 1;

	public GameObject[] houseLevels;

	public HousePartSelection[] roofs;

	public HousePartSelection[] bases;

	public HousePartSelection[] windows;

	public HousePartSelection[] doors;

	public Material[] houseMaterials;

	public Material[] roofMaterials;

	public Material[] wallMaterials;

	private Material houseColor;

	private Material wallColor;

	private Material roofColor;

	private bool renderersSet;

	private void Start()
	{
		setUpRenderers();
	}

	public void setExterior(HouseExterior houseDetails)
	{
		setUpRenderers();
		Color color;
		ColorUtility.TryParseHtmlString(houseDetails.houseColor, out color);
		Color color2;
		ColorUtility.TryParseHtmlString(houseDetails.wallColor, out color2);
		Color color3;
		ColorUtility.TryParseHtmlString(houseDetails.roofColor, out color3);
		Object.Destroy(houseColor);
		Object.Destroy(wallColor);
		Object.Destroy(roofColor);
		houseColor = Object.Instantiate(houseMaterials[houseDetails.houseMat]);
		houseColor.color = color;
		wallColor = Object.Instantiate(houseMaterials[houseDetails.wallMat]);
		wallColor.color = color2;
		roofColor = Object.Instantiate(roofMaterials[Mathf.Clamp(houseDetails.roofMat, 0, roofMaterials.Length)]);
		roofColor.color = color3;
		for (int i = 0; i < houseLevels.Length; i++)
		{
			if (i == houseDetails.houseLevel)
			{
				houseLevels[i].gameObject.SetActive(true);
			}
			else
			{
				houseLevels[i].gameObject.SetActive(false);
			}
		}
		for (int j = 0; j < roofs.Length; j++)
		{
			roofs[j].setPart(houseDetails.roof, wallColor, houseColor, roofColor);
		}
		for (int k = 0; k < bases.Length; k++)
		{
			bases[k].setPart(houseDetails.houseBase, wallColor, houseColor, roofColor);
		}
		for (int l = 0; l < windows.Length; l++)
		{
			windows[l].setPart(houseDetails.windows, wallColor, houseColor, roofColor);
		}
		for (int m = 0; m < doors.Length; m++)
		{
			doors[m].setPart(houseDetails.door, wallColor, houseColor, roofColor);
		}
	}

	public void setUpRenderers()
	{
		if (!renderersSet)
		{
			for (int i = 0; i < roofs.Length; i++)
			{
				roofs[i].setUpRenderers();
			}
			for (int j = 0; j < bases.Length; j++)
			{
				bases[j].setUpRenderers();
			}
			for (int k = 0; k < windows.Length; k++)
			{
				windows[k].setUpRenderers();
			}
			for (int l = 0; l < doors.Length; l++)
			{
				doors[l].setUpRenderers();
			}
			renderersSet = true;
		}
	}
}
