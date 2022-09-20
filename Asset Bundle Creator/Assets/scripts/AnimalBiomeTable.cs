using UnityEngine;

public class AnimalBiomeTable : MonoBehaviour
{
	public AnimalAI[] animalsInBiome;

	public float[] rarityPercentage;

	public int getBiomeAnimal(MapRand useGenerator = null)
	{
		float num = 0f;
		for (int i = 0; i < animalsInBiome.Length; i++)
		{
			num += rarityPercentage[i];
		}
		float num2 = ((useGenerator != null) ? useGenerator.Range(0f, num) : Random.Range(0f, num));
		float num3 = 0f;
		for (int j = 0; j < animalsInBiome.Length; j++)
		{
			num3 += rarityPercentage[j];
			if (num2 < num3)
			{
				if (animalsInBiome[j] == null)
				{
					return -1;
				}
				return animalsInBiome[j].animalId * 10 + animalsInBiome[j].getRandomVariationNo();
			}
		}
		return -1;
	}
}
