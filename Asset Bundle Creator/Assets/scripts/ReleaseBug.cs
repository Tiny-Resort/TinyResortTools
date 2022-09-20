using System.Collections;
using UnityEngine;

public class ReleaseBug : MonoBehaviour
{
	public Transform flyingBugTrans;

	public Transform crawlingBugTrans;

	[Header("Fish Stuff")]
	public Transform fishEscapeTrans;

	public GameObject fishDummy;

	private void Start()
	{
	}

	public void setUpForBug(int bugId)
	{
		GameObject gameObject;
		if (Inventory.inv.allItems[bugId].bug.insectType.name.Contains("Arach") || Inventory.inv.allItems[bugId].bug.insectType.name.Contains("Hopping"))
		{
			gameObject = Object.Instantiate(Inventory.inv.allItems[bugId].bug.insectType, crawlingBugTrans);
			StartCoroutine(crawlingBugRunAway());
		}
		else
		{
			gameObject = Object.Instantiate(Inventory.inv.allItems[bugId].bug.insectType, flyingBugTrans);
			StartCoroutine(flyingBugRunAway());
		}
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.GetComponentInChildren<Collider>().enabled = false;
		gameObject.GetComponent<BugAppearance>().setUpBug(Inventory.inv.allItems[bugId]);
		gameObject.GetComponent<Animator>().SetFloat("WalkingSpeed", 4f);
	}

	public void destroySelf()
	{
		Object.Destroy(base.gameObject);
	}

	public void setUpForFish(int fishId)
	{
		fishDummy.GetComponent<FishType>().setFishType(fishId, fishId);
		fishDummy.GetComponent<Animator>().SetFloat("WalkingSpeed", 2f);
		StartCoroutine(fishJumpsInWater());
	}

	private IEnumerator flyingBugRunAway()
	{
		yield return null;
		float timer2 = 0f;
		while (timer2 < 1f)
		{
			flyingBugTrans.transform.position += (Vector3.up * 2f + base.transform.forward * 3f) * Time.deltaTime;
			timer2 += Time.deltaTime;
			yield return null;
		}
		timer2 = 0f;
		while (timer2 < 1f)
		{
			flyingBugTrans.transform.position += (Vector3.up * 4f + base.transform.forward * 3f) * Time.deltaTime;
			flyingBugTrans.transform.localScale = new Vector3(1f - timer2, 1f - timer2, 1f - timer2);
			timer2 += Time.deltaTime;
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}

	private IEnumerator crawlingBugRunAway()
	{
		yield return null;
		float timer2 = 0f;
		while (timer2 < 1f)
		{
			Vector3 position = crawlingBugTrans.transform.position + base.transform.forward * 3f * Time.deltaTime;
			position.y = WorldManager.manageWorld.heightMap[(int)(position.x / 2f), (int)(position.z / 2f)];
			crawlingBugTrans.transform.position = position;
			timer2 += Time.deltaTime;
			yield return null;
		}
		timer2 = 0f;
		while (timer2 < 1f)
		{
			Vector3 position2 = crawlingBugTrans.transform.position + base.transform.forward * 3f * Time.deltaTime;
			position2.y = WorldManager.manageWorld.heightMap[(int)(position2.x / 2f), (int)(position2.z / 2f)];
			crawlingBugTrans.transform.position = position2;
			crawlingBugTrans.transform.localScale = new Vector3(1f - timer2, 1f - timer2, 1f - timer2);
			timer2 += Time.deltaTime;
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}

	private IEnumerator fishJumpsInWater()
	{
		Vector3 vector = base.transform.position + base.transform.forward;
		fishEscapeTrans.transform.position = base.transform.position + Vector3.up * 1.5f + base.transform.forward;
		while (fishEscapeTrans.transform.position.y < base.transform.position.y + 3.5f)
		{
			fishEscapeTrans.transform.position += Vector3.up * 8f * Time.deltaTime;
			fishEscapeTrans.transform.position += base.transform.forward * 2f * Time.deltaTime;
			fishEscapeTrans.transform.localRotation = Quaternion.Lerp(fishEscapeTrans.transform.localRotation, Quaternion.Euler(-85f, 0f, 0f), Time.deltaTime * 3f);
			yield return null;
		}
		while (fishEscapeTrans.transform.position.y > 0.6f)
		{
			fishEscapeTrans.transform.position += Vector3.down * 8f * Time.deltaTime;
			fishEscapeTrans.transform.position += base.transform.forward * 2f * Time.deltaTime;
			fishEscapeTrans.transform.localRotation = Quaternion.Lerp(fishEscapeTrans.transform.localRotation, Quaternion.Euler(85f, 0f, 0f), Time.deltaTime * 5f);
			yield return null;
		}
		ParticleManager.manage.bigSplash(fishEscapeTrans, 5);
		SoundManager.manage.playASoundAtPoint(SoundManager.manage.bigWaterSplash, fishEscapeTrans.position);
		float timer2 = 0f;
		while (timer2 < 1f)
		{
			fishEscapeTrans.transform.position += base.transform.forward * 4f * Time.deltaTime;
			if (fishEscapeTrans.transform.position.y > 0f)
			{
				fishEscapeTrans.transform.position += Vector3.down * 4f * Time.deltaTime;
			}
			fishEscapeTrans.transform.localRotation = Quaternion.Lerp(fishEscapeTrans.transform.localRotation, Quaternion.Euler(0f, 0f, 0f), Time.deltaTime * 4f);
			timer2 += Time.deltaTime;
			yield return null;
		}
		timer2 = 0f;
		while (timer2 < 1f)
		{
			fishEscapeTrans.transform.position += base.transform.forward * 2f * Time.deltaTime;
			fishEscapeTrans.transform.localRotation = Quaternion.Lerp(fishEscapeTrans.transform.localRotation, Quaternion.Euler(0f, 0f, 0f), Time.deltaTime * 2f);
			fishEscapeTrans.transform.localScale = new Vector3(1f - timer2, 1f - timer2, 1f - timer2);
			timer2 += Time.deltaTime;
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}
}
