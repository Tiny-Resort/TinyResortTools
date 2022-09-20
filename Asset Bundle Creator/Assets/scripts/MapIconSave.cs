using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
internal class MapIconSave
{
	public float[] iconXPos;

	public float[] iconYPos;

	public int[] iconId;

	public void saveIcons()
	{
		List<float> list = new List<float>();
		List<float> list2 = new List<float>();
		List<int> list3 = new List<int>();
		for (int i = 0; i < RenderMap.map.iconsOnMap.Count; i++)
		{
			if (RenderMap.map.iconsOnMap[i].isNormal())
			{
				list.Add(RenderMap.map.iconsOnMap[i].pointingAtPosition.x);
				list2.Add(RenderMap.map.iconsOnMap[i].pointingAtPosition.z);
				list3.Add(RenderMap.map.iconsOnMap[i].getIconSpriteNo());
			}
		}
		iconXPos = list.ToArray();
		iconYPos = list2.ToArray();
		iconId = list3.ToArray();
	}

	public void loadIcons()
	{
		for (int i = 0; i < iconId.Length; i++)
		{
			RenderMap.map.createLocalCustomMarker(new Vector2(iconXPos[i] / 8f, iconYPos[i] / 8f), iconId[i]);
		}
	}
}
