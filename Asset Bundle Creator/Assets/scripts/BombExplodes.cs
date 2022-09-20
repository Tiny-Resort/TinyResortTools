using System.Collections;
using Mirror;
using UnityEngine;

public class BombExplodes : NetworkBehaviour
{
	public GameObject hideOnExplode;

	public ASound bombExplodesSound;

	public LayerMask landOnMask;

	public LayerMask damageLayer;

	public Light explosionLight;

	public override void OnStartClient()
	{
		StartCoroutine(explodeTimer());
	}

	private IEnumerator explodeTimer()
	{
		float timeBeforeExplode = 2f;
		int xPos = Mathf.RoundToInt(base.transform.position.x / 2f);
		int yPos = Mathf.RoundToInt(base.transform.position.z / 2f);
		int fallToHeight = -2;
		if (WorldManager.manageWorld.isPositionOnMap(xPos, yPos))
		{
			fallToHeight = WorldManager.manageWorld.heightMap[xPos, yPos];
		}
		new Vector3(base.transform.position.x, fallToHeight, base.transform.position.z);
		float fallSpeed = 9f;
		while (timeBeforeExplode > 0f)
		{
			timeBeforeExplode -= Time.deltaTime;
			RaycastHit hitInfo;
			if (base.transform.position.y > (float)fallToHeight && !Physics.Raycast(base.transform.position - Vector3.up / 10f, Vector3.down, out hitInfo, 0.12f, landOnMask))
			{
				base.transform.position += Vector3.down * Time.deltaTime * fallSpeed;
				fallSpeed = Mathf.Lerp(fallSpeed, 15f, Time.deltaTime * 2f);
			}
			yield return null;
		}
		Vector3 particlePos = base.transform.position;
		SoundManager.manage.playASoundAtPoint(bombExplodesSound, particlePos);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.explosion, particlePos, 60);
		float num = Vector3.Distance(CameraController.control.transform.position, base.transform.position);
		float num2 = 0.75f - Mathf.Clamp(num / 25f, 0f, 0.75f);
		if (num2 > 0f)
		{
			CameraController.control.shakeScreen(num2);
		}
		StartCoroutine(explosionLightFlash());
		hideOnExplode.SetActive(false);
		if (base.isServer)
		{
			blowUpPos(xPos, yPos, 0, 0);
			Collider[] array = Physics.OverlapSphere(base.transform.position - Vector3.up * 1.5f, 4f, damageLayer);
			for (int i = 0; i < array.Length; i++)
			{
				Damageable component = array[i].transform.root.GetComponent<Damageable>();
				if ((bool)component)
				{
					component.attackAndDoDamage(25, base.transform);
					component.setOnFire();
				}
			}
		}
		ParticleManager.manage.emitAttackParticle(particlePos, 50);
		ParticleManager.manage.emitRedAttackParticle(particlePos, 25);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos, 50);
		yield return new WaitForSeconds(0.05f);
		ParticleManager.manage.emitAttackParticle(particlePos + Vector3.left * 2f, 25);
		ParticleManager.manage.emitAttackParticle(particlePos + Vector3.right * 2f, 25);
		ParticleManager.manage.emitAttackParticle(particlePos + Vector3.forward * 2f, 25);
		ParticleManager.manage.emitAttackParticle(particlePos + Vector3.back * 2f, 25);
		ParticleManager.manage.emitRedAttackParticle(particlePos + Vector3.left * 2f, 25);
		ParticleManager.manage.emitRedAttackParticle(particlePos + Vector3.right * 2f, 25);
		ParticleManager.manage.emitRedAttackParticle(particlePos + Vector3.forward * 2f, 25);
		ParticleManager.manage.emitRedAttackParticle(particlePos + Vector3.back * 2f, 25);
		if (base.isServer)
		{
			blowUpPos(xPos, yPos, 1, 0);
			blowUpPos(xPos, yPos, -1, 0);
			blowUpPos(xPos, yPos, 0, 1);
			blowUpPos(xPos, yPos, 0, -1);
			yield return null;
			blowUpPos(xPos, yPos, 1, 1);
			blowUpPos(xPos, yPos, -1, -1);
			blowUpPos(xPos, yPos, -1, 1);
			blowUpPos(xPos, yPos, 1, -1);
			yield return null;
			blowUpPos(xPos, yPos, 2, 2, true);
			blowUpPos(xPos, yPos, -2, -2, true);
			blowUpPos(xPos, yPos, -2, 1, true);
			blowUpPos(xPos, yPos, 2, -2, true);
		}
		yield return new WaitForSeconds(0.05f);
		hideOnExplode.gameObject.SetActive(false);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.left * 2f, 5);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.right * 2f, 5);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.forward * 2f, 5);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.back * 2f, 5);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.left * 2f, 5);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.right * 2f, 5);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.forward * 2f, 5);
		ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], particlePos + Vector3.back * 2f, 5);
		if (base.isServer)
		{
			yield return new WaitForSeconds(0.5f);
			NetworkServer.Destroy(base.gameObject);
		}
	}

	public void blowUpPos(int xPos, int yPos, int xdif, int yDif, bool ignoreHeight = false)
	{
		if (WorldManager.manageWorld.isPositionOnMap(xPos + xdif, yPos + yDif) && (shouldDestroyOnTile(xPos + xdif, yPos + yDif) || WorldManager.manageWorld.onTileMap[xPos + xdif, yPos + yDif] == -1))
		{
			if (WorldManager.manageWorld.onTileMap[xPos + xdif, yPos + yDif] != -1)
			{
				NetworkMapSharer.share.RpcUpdateOnTileObject(-1, xPos + xdif, yPos + yDif);
			}
			if (!ignoreHeight && (WorldManager.manageWorld.heightMap[xPos, yPos] == WorldManager.manageWorld.heightMap[xPos + xdif, yPos + yDif] || WorldManager.manageWorld.heightMap[xPos, yPos] - 1 == WorldManager.manageWorld.heightMap[xPos + xdif, yPos + yDif] || WorldManager.manageWorld.heightMap[xPos, yPos] + 1 == WorldManager.manageWorld.heightMap[xPos + xdif, yPos + yDif]))
			{
				NetworkMapSharer.share.changeTileHeight(-1, xPos + xdif, yPos + yDif);
			}
		}
	}

	public bool shouldDestroyOnTile(int xPos, int yPos)
	{
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] == -1)
		{
			return false;
		}
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] < -1)
		{
			return false;
		}
		if (WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].isMultiTileObject)
		{
			return false;
		}
		if (WorldManager.manageWorld.onTileMap[xPos, yPos] > -1 && (WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].isWood || WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].isHardWood || WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].isSmallPlant || WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].isStone || WorldManager.manageWorld.allObjectSettings[WorldManager.manageWorld.onTileMap[xPos, yPos]].isHardStone))
		{
			return true;
		}
		return false;
	}

	private IEnumerator explosionLightFlash()
	{
		explosionLight.gameObject.SetActive(true);
		while (explosionLight.intensity > 0f)
		{
			yield return null;
			explosionLight.intensity -= Time.deltaTime * 15f;
		}
	}

	private void MirrorProcessed()
	{
	}
}
