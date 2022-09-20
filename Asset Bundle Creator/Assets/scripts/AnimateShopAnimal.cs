using System.Collections;
using UnityEngine;

public class AnimateShopAnimal : MonoBehaviour
{
	public Vector2 walkDistance;

	public Animator walkAnim;

	public float speed = 1f;

	private float minWait = 0.5f;

	private float maxWait = 2f;

	private float maxHeight;

	public float turnspeed = 2.5f;

	public bool printpos;

	private float closePos = 0.25f;

	private Vector3 dampSpeed;

	private float posDistance = 1f;

	private Vector3 randomWalkPos = Vector3.zero;

	private void OnEnable()
	{
		StartCoroutine(wonderAround());
	}

	private IEnumerator wonderAround()
	{
		if (!walkAnim)
		{
			yield return null;
		}
		setNewRandomPositionAndGetDistance();
		while (true)
		{
			yield return null;
			while (Vector3.Distance(randomWalkPos, base.transform.localPosition) > closePos)
			{
				base.transform.localPosition = Vector3.SmoothDamp(base.transform.localPosition, randomWalkPos, ref dampSpeed, posDistance);
				base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, Quaternion.LookRotation((randomWalkPos - base.transform.localPosition).normalized), Time.deltaTime * turnspeed);
				walkAnim.SetFloat("WalkingSpeed", Mathf.Clamp01(Vector3.Distance(randomWalkPos, base.transform.localPosition) * 4f));
				yield return null;
			}
			while (walkAnim.GetFloat("WalkingSpeed") > 0f)
			{
				base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, Quaternion.LookRotation((randomWalkPos - base.transform.localPosition).normalized), Time.deltaTime * turnspeed);
				walkAnim.SetFloat("WalkingSpeed", Mathf.Clamp(walkAnim.GetFloat("WalkingSpeed") - 0.05f, 0f, 2f));
				yield return null;
			}
			walkAnim.SetFloat("WalkingSpeed", 0f);
			yield return new WaitForSeconds(Random.Range(minWait, maxWait));
			setNewRandomPositionAndGetDistance();
			if (!(minWait <= 0.11f) || !(maxWait <= 0.11f))
			{
				continue;
			}
			int num = 0;
			while (Vector3.Distance(base.transform.position, randomWalkPos) < 1.5f)
			{
				setNewRandomPositionAndGetDistance();
				num++;
				if (num >= 30)
				{
					break;
				}
			}
		}
	}

	public void resetStartingPosAndRandomisePos(float setWalkSpeed = 1f)
	{
		speed = setWalkSpeed;
		base.transform.localPosition = new Vector3(Random.Range(0f - walkDistance.x, walkDistance.x), Random.Range(0f, maxHeight), Random.Range(0f - walkDistance.y, walkDistance.y));
	}

	public void setMinWaitTimeFish()
	{
		turnspeed = 1f;
		minWait = 0.1f;
		maxWait = 0.1f;
		maxHeight = 2.5f;
		closePos = 0.45f;
	}

	public void setNewRandomPositionAndGetDistance()
	{
		randomWalkPos = new Vector3(Random.Range(0f - walkDistance.x, walkDistance.x), Random.Range(0f, maxHeight), Random.Range(0f - walkDistance.y, walkDistance.y));
		if (printpos)
		{
			MonoBehaviour.print(randomWalkPos);
		}
		posDistance = Vector3.Distance(base.transform.localPosition, randomWalkPos) / 2f / speed;
	}
}
