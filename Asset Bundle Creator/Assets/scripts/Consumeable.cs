using UnityEngine;

public class Consumeable : MonoBehaviour
{
	[Header("Basic buffs-----")]
	public float staminaGain;

	public int healthGain;

	public bool givesTempPoints;

	public float tempStaminaGain;

	public int tempHealthGain;

	[Header("Special buffs-----")]
	public GivesBuffTypes[] myBuffs;

	[Header("Food details-----")]
	public bool isMeat;

	public bool isFruit;

	public bool isAnimalProduct;

	public bool isVegitable;

	public void giveBuffs()
	{
		for (int i = 0; i < myBuffs.Length; i++)
		{
			myBuffs[i].giveThisBuff();
		}
	}
}
