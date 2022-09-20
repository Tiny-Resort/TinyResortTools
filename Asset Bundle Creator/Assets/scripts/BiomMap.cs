using UnityEngine;

public class BiomMap
{
	public float biomWidth = 4f;

	public float biomScale = 1f;

	public float varianceX;

	public float varianceY;

	public MapRand generator;

	public BiomMap(MapRand useGenerator)
	{
		generator = useGenerator;
	}

	public BiomMap()
	{
	}

	private void Start()
	{
		randomisePosition();
	}

	public float getNoise(float tileX, float tileY)
	{
		return Mathf.PerlinNoise((tileX + varianceX) / biomWidth * biomScale, (tileY + varianceY) / biomWidth * biomScale);
	}

	public void randomisePosition()
	{
		if (generator != null)
		{
			while (varianceX == 0f || varianceY == 0f)
			{
				varianceX = generator.Range(-1000, 1000) * generator.Range(1, 50);
				varianceY = generator.Range(-1000, 1000) * generator.Range(1, 50);
			}
		}
		else
		{
			while (varianceX == 0f || varianceY == 0f)
			{
				varianceX = Random.Range(-1000, 1000) * Random.Range(1, 50);
				varianceY = Random.Range(-1000, 1000) * Random.Range(1, 50);
			}
		}
	}
}
