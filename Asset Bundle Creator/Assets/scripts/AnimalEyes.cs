using System.Collections;
using UnityEngine;

public class AnimalEyes : MonoBehaviour
{
	private MeshRenderer myRen;

	private Material eyeLid;

	private AnimalAI_Sleep myAnimalSleep;

	private Damageable myDamage;

	private Coroutine myRoutine;

	private Coroutine normalEyes;

	private void Start()
	{
		myAnimalSleep = base.transform.root.GetComponent<AnimalAI_Sleep>();
		myDamage = base.transform.root.GetComponent<Damageable>();
	}

	private void OnEnable()
	{
		myRen = GetComponent<MeshRenderer>();
		eyeLid = myRen.material;
		if (myRoutine != null)
		{
			StopCoroutine(myRoutine);
			myRoutine = null;
		}
		myRoutine = StartCoroutine(eyeControl());
	}

	public void deadEyes()
	{
		if (myRoutine != null)
		{
			StopCoroutine(myRoutine);
			myRoutine = null;
		}
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0f, 0.5f));
	}

	private IEnumerator eyeControl()
	{
		normalEyes = StartCoroutine(eyesBlink());
		while (true)
		{
			if ((bool)myAnimalSleep && myAnimalSleep.checkIfSleeping())
			{
				StopCoroutine(normalEyes);
				while (myAnimalSleep.checkIfSleeping())
				{
					yield return null;
					eyeLid.SetTextureOffset("_MainTex", new Vector2(0.75f, 0.75f));
				}
				normalEyes = StartCoroutine(eyesBlink());
			}
			if ((bool)myDamage && myDamage.isStunned())
			{
				StopCoroutine(normalEyes);
				while ((bool)myDamage && myDamage.isStunned())
				{
					eyeLid.SetTextureOffset("_MainTex", new Vector2(0.25f, 0.5f));
					yield return null;
				}
				normalEyes = StartCoroutine(eyesBlink());
			}
			yield return null;
		}
	}

	private IEnumerator eyesBlink()
	{
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.75f, 0.75f));
		yield return null;
		yield return null;
		yield return null;
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.75f));
		yield return null;
		yield return null;
		yield return null;
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0.25f, 0.75f));
		yield return null;
		yield return null;
		yield return null;
		eyeLid.SetTextureOffset("_MainTex", new Vector2(0f, 0.75f));
		while (true)
		{
			float waitSeconds = Random.Range(3f, 7f);
			while (waitSeconds >= 0f)
			{
				waitSeconds -= Time.deltaTime;
				yield return null;
			}
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0f, 0.75f));
			yield return null;
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0.25f, 0.75f));
			yield return null;
			yield return null;
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.75f));
			yield return null;
			yield return null;
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0.75f, 0.75f));
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.75f));
			yield return null;
			yield return null;
			yield return null;
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0.25f, 0.75f));
			yield return null;
			yield return null;
			yield return null;
			eyeLid.SetTextureOffset("_MainTex", new Vector2(0f, 0.75f));
		}
	}
}
