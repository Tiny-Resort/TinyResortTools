using UnityEngine;

public class FallingProjectileAOE : MonoBehaviour
{
	public FallingProjectile[] falling;

	public void setUpAndFire(AnimalAI shotby, int randomSeed)
	{
		Random.InitState(randomSeed);
		for (int i = 0; i < falling.Length; i++)
		{
			falling[i].setShotByAnimal(shotby);
			falling[i].transform.parent = null;
		}
		Object.Destroy(base.gameObject);
	}
}
