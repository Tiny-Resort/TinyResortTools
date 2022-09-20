using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileIdInMat
{
	public List<CombineInstance> idsWithMyMat = new List<CombineInstance>();

	public TileIdInMat()
	{
		idsWithMyMat = new List<CombineInstance>(0);
	}
}
