using System.Collections;
using Mirror;
using UnityEngine;

public class SpawnVehicleOnOpen : NetworkBehaviour
{
	public GameObject vehicleToSpawn;

	public Transform[] boxSides;

	public ASound openBoxSounds;

	public ASound openBoxQuick;

	public NetworkConnection ownerConn;

	public int variation;

	public override void OnStartServer()
	{
		StartCoroutine(openBoxAndSpawn());
	}

	public void fillDetails(GameObject vehicleSpawning, int variationToSend, NetworkConnection ownerConnToSpawn)
	{
		vehicleToSpawn = vehicleSpawning;
		variation = variationToSend;
		ownerConn = ownerConnToSpawn;
	}

	private IEnumerator openBoxAndSpawn()
	{
		yield return new WaitForSeconds(1f);
		Vehicle component = Object.Instantiate(vehicleToSpawn, base.transform.position, base.transform.rotation).GetComponent<Vehicle>();
		component.setVariation(variation);
		NetworkServer.Spawn(component.gameObject, ownerConn);
		yield return new WaitForSeconds(1f);
		NetworkServer.Destroy(base.gameObject);
	}

	public void playSmokeParticles()
	{
		for (int i = 0; i < boxSides.Length; i++)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], boxSides[i].position);
		}
	}

	public void playPlacedDownSound()
	{
		SoundManager.manage.playASoundAtPoint(openBoxSounds, base.transform.position);
	}

	public void openBoxSound()
	{
		SoundManager.manage.playASoundAtPoint(openBoxQuick, base.transform.position);
		for (int i = 0; i < boxSides.Length; i++)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], boxSides[i].position);
		}
	}

	private void MirrorProcessed()
	{
	}
}
