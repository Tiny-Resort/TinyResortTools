using UnityEngine;

public class PhotoChallengeManager : MonoBehaviour
{
	public enum photoSubject
	{
		Npc = 0,
		Animal = 1,
		Carryable = 2,
		Location = 3,
		Biome = 4
	}

	public enum subjectActions
	{
		None = 0,
		Sleeping = 1,
		Drinking = 2,
		Running = 3,
		Hunting = 4,
		Attacking = 5,
		Howling = 6
	}

	public static PhotoChallengeManager manage;

	public bool checkPhotos;

	public SellByWeight carryablesToPhotograph;

	public void Awake()
	{
		manage = this;
	}

	public PhotoChallenge createRandomPhotoChallengeAndAttachToPost()
	{
		return new PhotoChallenge((photoSubject)Random.Range(0, 4));
	}
}
