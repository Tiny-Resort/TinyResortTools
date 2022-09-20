using System.Collections;
using UnityEngine;

public class FallingProjectile : MonoBehaviour
{
	public LayerMask smashWhenHitLayer;

	public LayerMask damageLayer;

	public ASound smashSound;

	public Transform rockTransform;

	private AnimalAI shotBy;

	public int damageOnHit = 4;

	public bool setFireOnHit;

	public float stayOnGroundTime = 2f;

	private bool hitGround;

	public GameObject skyObject;

	public GameObject hitGroundObject;

	private void OnEnable()
	{
		base.transform.parent = null;
	}

	public void setShotByAnimal(AnimalAI newShotBy)
	{
		shotBy = newShotBy;
		StartCoroutine(flyUpAndHitGround());
		StartCoroutine(rotateRandomly());
	}

	private IEnumerator rotateRandomly()
	{
		Vector3 randomDir = new Vector3(Random.Range(-1f, -1f), Random.Range(-1f, -1f), Random.Range(-1f, -1f));
		float randomSpeed = Random.Range(8f, 16f);
		rockTransform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
		while (true)
		{
			yield return null;
			rockTransform.Rotate(randomDir, randomSpeed);
		}
	}

	private IEnumerator flyUpAndHitGround()
	{
		Vector3 startPos = base.transform.position;
		float x = Random.Range(-15f, 15f);
		float z = Random.Range(-15f, 15f);
		float maxHeight = Random.Range(7f, 10f);
		Vector3 flyToPos = base.transform.position + new Vector3(x, 0f, z);
		float riseSpeed = 8f;
		while (Mathf.Abs(startPos.y - base.transform.position.y) < maxHeight)
		{
			yield return null;
			base.transform.position += Vector3.up * riseSpeed * Time.deltaTime;
			riseSpeed = Mathf.Clamp(riseSpeed - Time.deltaTime * 2f, 2f, 8f);
			flyToPos.y = base.transform.position.y;
			if (Mathf.Abs(startPos.y - base.transform.position.y) > maxHeight / 4f)
			{
				base.transform.position = Vector3.Lerp(base.transform.position, flyToPos, Time.deltaTime * 2f);
			}
		}
		while (Vector3.Distance(flyToPos, base.transform.position) > 0.25f)
		{
			yield return null;
			flyToPos.y = base.transform.position.y + Time.deltaTime * 2f;
			base.transform.position = Vector3.Lerp(base.transform.position, flyToPos, Time.deltaTime * 3f);
		}
		float fallspeed = 0f;
		while (base.transform.position.y > startPos.y - 15f)
		{
			base.transform.position += Vector3.down * fallspeed * Time.deltaTime;
			fallspeed = Mathf.Clamp(fallspeed + Time.deltaTime * 8f, 0f, 18f);
			yield return null;
			checkForDamage();
			if (hitGround)
			{
				break;
			}
		}
		skyObject.SetActive(false);
		SoundManager.manage.playASoundAtPoint(smashSound, base.transform.position);
		hitGroundObject.SetActive(true);
		float groundDelayTimer = 0f;
		while (groundDelayTimer < stayOnGroundTime)
		{
			checkForDamage();
			groundDelayTimer += Time.deltaTime;
			yield return null;
		}
		destoyEffects();
		Object.Destroy(base.gameObject);
	}

	public void checkForDamage()
	{
		if (NetworkMapSharer.share.isServer)
		{
			Collider[] array = Physics.OverlapSphere(base.transform.position, 0.5f, damageLayer);
			for (int i = 0; i < array.Length; i++)
			{
				Damageable componentInParent = array[i].GetComponentInParent<Damageable>();
				if (((bool)componentInParent && !componentInParent.isAnAnimal()) || ((bool)componentInParent && (bool)componentInParent.isAnAnimal() && componentInParent.isAnAnimal().animalId != shotBy.animalId))
				{
					componentInParent.attackAndDoDamage(damageOnHit, shotBy.transform, 0f);
					if (setFireOnHit)
					{
						componentInParent.setOnFire();
						stayOnGroundTime = 0f;
					}
				}
			}
		}
		if (hitGround && WorldManager.manageWorld.isPositionOnMap(base.transform.position) && WorldManager.manageWorld.waterMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)])
		{
			stayOnGroundTime = 0f;
		}
		else if (!hitGround && Physics.Raycast(base.transform.position, Vector3.down, 0.35f, smashWhenHitLayer))
		{
			hitGround = true;
		}
		else if (!hitGround && WorldManager.manageWorld.isPositionOnMap(base.transform.position) && base.transform.position.y <= (float)WorldManager.manageWorld.heightMap[Mathf.RoundToInt(base.transform.position.x / 2f), Mathf.RoundToInt(base.transform.position.z / 2f)])
		{
			hitGround = true;
		}
	}

	public void destoyEffects()
	{
	}
}
