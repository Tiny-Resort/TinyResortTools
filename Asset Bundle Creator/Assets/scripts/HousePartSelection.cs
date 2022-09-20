using UnityEngine;

public class HousePartSelection : MonoBehaviour
{
	public GameObject[] baseParts;

	public GameObject[] frames;

	public HousePartRenderers[] baseRen;

	public HousePartRenderers[] frameRen;

	public Material wallMaterial;

	public Material houseMaterial;

	public Material roofMaterial;

	public void setUpRenderers()
	{
		baseRen = new HousePartRenderers[baseParts.Length];
		frameRen = new HousePartRenderers[frames.Length];
	}

	public void setPart(int windowId, Material newWall, Material newHouse, Material newRoof)
	{
		for (int i = 0; i < baseParts.Length; i++)
		{
			if (i == windowId)
			{
				baseParts[i].SetActive(true);
				if (baseRen[i] == null)
				{
					baseRen[i] = new HousePartRenderers(baseParts[i], roofMaterial, houseMaterial, wallMaterial);
				}
				baseRen[i].setRendererColors(newRoof, newHouse, newWall);
			}
			else
			{
				baseParts[i].SetActive(false);
			}
		}
		for (int j = 0; j < frames.Length; j++)
		{
			if (j == windowId)
			{
				frames[j].SetActive(true);
				if (frameRen[j] == null)
				{
					frameRen[j] = new HousePartRenderers(frames[j], roofMaterial, houseMaterial, wallMaterial);
				}
				frameRen[j].setRendererColors(newRoof, newHouse, newWall);
			}
			else
			{
				frames[j].SetActive(false);
			}
		}
	}
}
