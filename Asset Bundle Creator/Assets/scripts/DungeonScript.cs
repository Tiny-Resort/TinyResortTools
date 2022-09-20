using UnityEngine;

public class DungeonScript : MonoBehaviour
{
	public DungeonMap thisMap;

	public int[,] convertTo2dArray()
	{
		int[,] array = new int[16, 16];
		for (int i = 0; i < thisMap.rows2.Length; i++)
		{
			for (int j = 0; j < thisMap.rows2.Length; j++)
			{
				array[j, i] = thisMap.rows2[j].row[i];
			}
		}
		return array;
	}
}
