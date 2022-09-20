using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHitBox : MonoBehaviour
{
	public MeleeAttacks myAttacks;

	public Collider myCollider;

	private List<Damageable> damageablesToAttackOnSwing = new List<Damageable>();

	private bool hasSplashed;

	public int attackDamageAmount = 10;

	private bool attackOn;

	public bool fireDamage;

	public bool poisonDamage;

	public bool canDamageFriendly;

	public float chanceToStun;

	public bool stunWithLight;

	private AnimalAI isAnimal;

	public ASound playSoundIfHit;

	public float specialKnockback;

	private void Start()
	{
		myCollider.enabled = false;
		if (!isAnimal)
		{
			isAnimal = GetComponentInParent<AnimalAI>();
		}
	}

	public void setIsAnimal(AnimalAI newAnimal)
	{
		isAnimal = newAnimal;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!attackOn)
		{
			return;
		}
		BugTypes component = other.transform.root.GetComponent<BugTypes>();
		if ((bool)component && (bool)myAttacks)
		{
			myAttacks.catchBug(component);
		}
		Damageable component2 = other.transform.root.GetComponent<Damageable>();
		if (((!component2 || component2.isFriendly) && (!component2 || !component2.isFriendly || !canDamageFriendly)) || damageablesToAttackOnSwing.Contains(component2))
		{
			return;
		}
		damageablesToAttackOnSwing.Add(component2);
		if ((bool)playSoundIfHit && damageablesToAttackOnSwing.Count == 1)
		{
			SoundManager.manage.playASoundAtPoint(playSoundIfHit, base.transform.position);
		}
		if ((bool)myAttacks)
		{
			myAttacks.attackAndDealDamage(component2);
		}
		else
		{
			if (!NetworkMapSharer.share.isServer || ((bool)isAnimal && (bool)component2.isAnAnimal() && (!isAnimal || !component2.isAnAnimal() || isAnimal.animalId == component2.isAnAnimal().animalId)))
			{
				return;
			}
			if (attackDamageAmount > 0)
			{
				if (specialKnockback == 0f)
				{
					component2.attackAndDoDamage(attackDamageAmount, base.transform);
				}
				else
				{
					component2.attackAndDoDamage(attackDamageAmount, base.transform, specialKnockback);
				}
			}
			if (checkForStun())
			{
				if (stunWithLight)
				{
					component2.stunWithLight();
				}
				else
				{
					component2.stun();
				}
			}
			if (fireDamage)
			{
				component2.setOnFire();
			}
			if (poisonDamage)
			{
				component2.poison();
			}
		}
	}

	public bool checkForStun()
	{
		if (!chanceToStun.Equals(0f) && Random.Range(0f, 100f) <= chanceToStun)
		{
			return true;
		}
		return false;
	}

	private IEnumerator splash()
	{
		while (attackOn && (bool)myAttacks && !hasSplashed)
		{
			if (base.transform.position.y < 0.6f && WorldManager.manageWorld.waterMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2])
			{
				ParticleManager.manage.waterSplash(base.transform.position, 6);
				SoundManager.manage.playASoundAtPoint(SoundManager.manage.bigWaterSplash, base.transform.position);
				ParticleManager.manage.waterWakePart(base.transform.position, 8);
				hasSplashed = true;
			}
			yield return null;
		}
	}

	public void startAttack()
	{
		attackOn = true;
		damageablesToAttackOnSwing.Clear();
		myCollider.enabled = true;
		StartCoroutine(splash());
	}

	public void endAttack()
	{
		attackOn = false;
		damageablesToAttackOnSwing.Clear();
		myCollider.enabled = false;
		hasSplashed = false;
	}
}
