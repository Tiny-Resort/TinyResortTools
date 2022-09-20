using UnityEngine;

public class UnderWaterCreature : MonoBehaviour
{
	public GameObject creatureModel;

	public float baseSpeed;

	public bool glows;

	public Mesh altBody;

	public AnimatorOverrideController control;

	public SeasonAndTime mySeason;

	public Sprite pediaPhoto;

	public bool turn90DegreesInHand = true;
}
