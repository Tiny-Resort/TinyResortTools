using System;
using UnityEngine;

[Serializable]
public class HousePartRenderers
{
	public MeshRenderer[] renderersInObject;

	public int[] houseColor;

	public int[] wallColor;

	public int[] roofColor;

	public bool hasBeenSetUp = true;

	public HousePartRenderers()
	{
		hasBeenSetUp = false;
	}

	public HousePartRenderers(GameObject gameObject, Material roofMat, Material houseMat, Material wallsMat)
	{
		hasBeenSetUp = true;
		renderersInObject = gameObject.GetComponentsInChildren<MeshRenderer>();
		houseColor = new int[renderersInObject.Length];
		wallColor = new int[renderersInObject.Length];
		roofColor = new int[renderersInObject.Length];
		for (int i = 0; i < renderersInObject.Length; i++)
		{
			houseColor[i] = -1;
			wallColor[i] = -1;
			roofColor[i] = -1;
			for (int j = 0; j < renderersInObject[i].materials.Length; j++)
			{
				if (renderersInObject[i].materials[j].name.Contains(roofMat.name))
				{
					roofColor[i] = j;
				}
				if (renderersInObject[i].materials[j].name.Contains(houseMat.name))
				{
					houseColor[i] = j;
				}
				if (renderersInObject[i].materials[j].name.Contains(wallsMat.name))
				{
					wallColor[i] = j;
				}
			}
		}
	}

	public void setRendererColors(Material newRoofMat, Material newHouseMat, Material newWallsMat)
	{
		for (int i = 0; i < renderersInObject.Length; i++)
		{
			if (houseColor[i] != -1 || wallColor[i] != -1 || roofColor[i] != -1)
			{
				Material[] materials = renderersInObject[i].materials;
				if (houseColor[i] != -1)
				{
					materials[houseColor[i]] = newHouseMat;
				}
				if (wallColor[i] != -1)
				{
					materials[wallColor[i]] = newWallsMat;
				}
				if (roofColor[i] != -1)
				{
					materials[roofColor[i]] = newRoofMat;
				}
				renderersInObject[i].materials = materials;
			}
		}
	}
}
