using UnityEngine;

public class BiomSpawnTable : MonoBehaviour
{
	public TileObject[] objectsInBiom;

	public float[] rarityPercentage;

	public int getBiomObject(MapRand useGenerator = null)
	{
		float num = 0f;
		for (int i = 0; i < objectsInBiom.Length; i++)
		{
			num += rarityPercentage[i];
		}
		float num2 = ((useGenerator != null) ? useGenerator.Range(0f, num) : Random.Range(0f, num));
		float num3 = 0f;
		for (int j = 0; j < objectsInBiom.Length; j++)
		{
			num3 += rarityPercentage[j];
			if (num2 < num3)
			{
				if (objectsInBiom[j] == null)
				{
					return -1;
				}
				return objectsInBiom[j].tileObjectId;
			}
		}
		return -1;
	}

	public void getRandomObjectAndPlaceWithGrowth(int xPos, int yPos)
	{
		WorldManager.manageWorld.onTileMap[xPos, yPos] = getBiomObject();
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] != -1 && (bool)WorldManager.manageWorld.allObjects[WorldManager.manageWorld.onTileMap[xPos, yPos]].tileObjectGrowthStages)
		{
			WorldManager.manageWorld.onTileStatusMap[xPos, yPos] = 0;
		}
	}
}
