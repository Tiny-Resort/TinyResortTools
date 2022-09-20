using UnityEngine;

public class Projectile : MonoBehaviour
{
	private int projectileNo = -1;

	public GameObject[] projectileTypes;

	public int[] projectileDamge;

	public float[] strengthBonusDivide;

	public int[] projectileDamageParticlesOnDeath;

	public bool[] fireDamage;

	public bool[] poisonDamage;

	public ASound[] projectileDestroySound;

	private Transform shotBy;

	public Rigidbody myRig;

	private float shotStrength = 1f;

	public Transform trail;

	private bool underwater;

	private Vector3 lastPos;

	private void OnTriggerEnter(Collider other)
	{
		Damageable componentInParent = other.GetComponentInParent<Damageable>();
		if (!componentInParent || !(componentInParent.transform != shotBy))
		{
			return;
		}
		if (componentInParent.isServer)
		{
			Mathf.Clamp(Mathf.RoundToInt((float)projectileDamge[projectileNo] * (shotStrength / strengthBonusDivide[projectileNo])), 1, 100);
			componentInParent.attackAndDoDamage(projectileDamge[projectileNo], shotBy, 0.5f);
			if (fireDamage[projectileNo])
			{
				componentInParent.setOnFire();
			}
		}
		if ((bool)componentInParent && shotBy == NetworkMapSharer.share.localChar.transform)
		{
			TileObjectHealthBar.tile.setCurrentlyHitting(componentInParent);
		}
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		Vector3 normalized = (base.transform.position - lastPos).normalized;
		base.transform.LookAt(base.transform.position + normalized);
		lastPos = base.transform.position;
		if (base.transform.position.y < 0.6f)
		{
			int num = Mathf.RoundToInt(base.transform.position.x / 2f);
			int num2 = Mathf.RoundToInt(base.transform.position.z / 2f);
			if (WorldManager.manageWorld.waterMap[num, num2] && !underwater)
			{
				ParticleManager.manage.waterSplash(base.transform.position);
				SoundManager.manage.playASoundAtPoint(SoundManager.manage.waterSplash, base.transform.position);
				underwater = true;
				shotStrength /= 2f;
				myRig.velocity /= 2f;
			}
		}
		if (base.transform.position.y < -10f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void SetUpProjectile(int newProjectileNo, Transform newShotBy, Vector3 forwardDir)
	{
		projectileNo = newProjectileNo;
		shotBy = newShotBy;
		onTypeChanged(newProjectileNo, forwardDir * 25f);
		lastPos = base.transform.position;
	}

	public void SetUpProjectile(int newProjectileNo, Transform newShotBy, Vector3 forwardDir, float strength)
	{
		projectileNo = newProjectileNo;
		shotBy = newShotBy;
		shotStrength = strength;
		onTypeChanged(newProjectileNo, forwardDir * (strength * 5f), strength);
		lastPos = base.transform.position;
	}

	private void onTypeChanged(int newType, Vector3 shotDir, float strength = 3f)
	{
		projectileNo = newType;
		for (int i = 0; i < projectileTypes.Length; i++)
		{
			if (i == newType)
			{
				projectileTypes[i].SetActive(true);
			}
			else
			{
				projectileTypes[i].SetActive(false);
			}
		}
		myRig.AddForce(shotDir + Vector3.up * strength, ForceMode.VelocityChange);
	}

	public void OnDisable()
	{
		int num = (int)base.transform.position.x / 2;
		int num2 = (int)base.transform.position.z / 2;
		if ((WorldManager.manageWorld.heightMap[num, num2] > 0 || !WorldManager.manageWorld.waterMap[num, num2]) && projectileNo >= 0)
		{
			ParticleManager.manage.emitAttackParticle(base.transform.position, 5);
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[projectileDamageParticlesOnDeath[projectileNo]], base.transform.position, 12);
			SoundManager.manage.playASoundAtPoint(projectileDestroySound[projectileNo], base.transform.position);
		}
		trail.parent = null;
		Object.Destroy(trail.gameObject, 1.4f);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!collision.transform.root.GetComponent<Projectile>())
		{
			Object.Destroy(base.gameObject);
			myRig.isKinematic = true;
		}
	}
}
