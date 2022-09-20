using System;

[Serializable]
public class PhotographedObject
{
	public int objectType;

	public int id;

	public int variation;

	public int animationPlaying;

	public int xPos;

	public int yPos;

	public string otherObjectName = "";

	public PhotographedObject(AnimalAI animalDetails)
	{
		objectType = 0;
		id = animalDetails.animalId;
		if (id == 1)
		{
			variation = animalDetails.GetComponent<FishType>().getFishTypeId();
		}
		else if (id == 2)
		{
			variation = animalDetails.GetComponent<BugTypes>().getBugTypeId();
		}
		else
		{
			variation = animalDetails.getVariationNo();
		}
		xPos = (int)animalDetails.transform.position.x / 2;
		yPos = (int)animalDetails.transform.position.z / 2;
	}

	public PhotographedObject(EquipItemToChar charDetails)
	{
		objectType = 2;
		otherObjectName = charDetails.playerName;
	}

	public PhotographedObject(NPCIdentity npcDetails)
	{
		objectType = 1;
		id = npcDetails.NPCNo;
	}

	public PhotographedObject(PickUpAndCarry carryableDetails)
	{
		objectType = 3;
		id = carryableDetails.prefabId;
	}
}
