using UnityEngine;

public class ParticlePositions : MonoBehaviour
{
	public TileObject myObject;

	public Transform[] dropPositions;

	private void OnEnable()
	{
		myObject.damageParticlePositions = dropPositions;
		myObject.particlePositions = dropPositions;
	}
}
