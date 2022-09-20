using System.Collections;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
	public bool destroyAndDropItemAfterTime;

	public bool destroyAndDropBeforeTime;

	public float destroyTime;

	public Transform[] dropPositions;

	public float health = 100f;

	public InventoryItem dropsItemOnDestroyed;

	public InventoryItemLootTable spawnBugOnDrop;

	public Transform[] bugPositions;

	private int itemDrop = -1;

	[Header("Spawn Carryable")]
	public PickUpAndCarry carryableId;

	public float chance;

	private float randomChance;

	public GameObject dummyItem;

	public void Start()
	{
		if ((bool)dropsItemOnDestroyed)
		{
			itemDrop = Inventory.inv.getInvItemId(dropsItemOnDestroyed);
		}
		if (destroyAndDropItemAfterTime)
		{
			StartCoroutine("runClock");
		}
		if (destroyAndDropBeforeTime)
		{
			doDrop();
		}
		if ((bool)carryableId)
		{
			Random.InitState((int)(base.transform.position.x * base.transform.position.y) * NetworkMapSharer.share.mineSeed + (int)base.transform.position.z + RealWorldTimeLight.time.currentHour);
			randomChance = Random.Range(0f, 100f);
			dummyItem.SetActive(chance > randomChance);
		}
	}

	private IEnumerator runClock()
	{
		yield return new WaitForSeconds(destroyTime);
		if (!destroyAndDropBeforeTime)
		{
			doDrop();
		}
		Transform[] array = dropPositions;
		foreach (Transform transform in array)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], transform.position);
		}
		Object.Destroy(base.gameObject);
	}

	public void doDrop()
	{
		if (NetworkMapSharer.share.isServer)
		{
			Transform[] array = dropPositions;
			foreach (Transform transform in array)
			{
				NetworkMapSharer.share.spawnAServerDrop(itemDrop, 1, transform.position, null, true, 1);
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], transform.position);
			}
			array = bugPositions;
			foreach (Transform transform2 in array)
			{
				NetworkNavMesh.nav.spawnSpecificBug(spawnBugOnDrop.getRandomDropFromTable().getItemId(), transform2.position);
			}
			if (dropPositions.Length == 0)
			{
				ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], base.transform.position);
			}
			if (chance > randomChance && NetworkMapSharer.share.isServer)
			{
				NetworkMapSharer.share.spawnACarryable(carryableId.gameObject, dummyItem.transform.position);
			}
		}
	}
}
