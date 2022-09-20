using System;

[Serializable]
public class DungeonMap
{
	[Serializable]
	public struct rowData
	{
		public int[] row;
	}

	public rowData[] rows2 = new rowData[16];
}
