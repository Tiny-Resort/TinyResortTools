using UnityEngine;

public class SetAnimalVariationTrap : MonoBehaviour
{
	public SkinnedMeshRenderer skinMesh;

	public void setVariationNo(int animalId, int variation)
	{
		skinMesh.material = AnimalManager.manage.allAnimals[animalId].hasVariation.variations[variation];
	}
}
