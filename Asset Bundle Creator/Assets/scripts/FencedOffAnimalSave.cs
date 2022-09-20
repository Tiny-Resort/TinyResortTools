using System;
using UnityEngine;

[Serializable]
internal class FencedOffAnimalSave
{
	public FencedOffAnimal[] fencedOffAnimals;

	public FencedOffAnimal[] alphas;

	public void saveAnimals(bool endOfDaySave)
	{
		if (!endOfDaySave)
		{
			AnimalManager.manage.getAllFencedOffAnimals();
		}
		fencedOffAnimals = AnimalManager.manage.fencedOffAnimals.ToArray();
		alphas = AnimalManager.manage.alphaAnimals.ToArray();
	}

	public void loadAnimals()
	{
		if (fencedOffAnimals != null)
		{
			for (int i = 0; i < fencedOffAnimals.Length; i++)
			{
				AnimalManager.manage.placeFencedOffAnimalIntoChunk(fencedOffAnimals[i]);
			}
		}
		if (alphas != null)
		{
			Debug.Log("Loading Alpha animals");
			for (int j = 0; j < alphas.Length; j++)
			{
				Debug.Log("Loading Alpha animal id " + j);
				AnimalManager.manage.placeHuntingTargetOnMap(alphas[j].animalId, alphas[j].xPos, alphas[j].yPos, alphas[j].daysRemaining);
			}
		}
	}
}
