using System.Collections;
using UnityEngine;

public class NPCIk : MonoBehaviour
{
	public Transform lookTrans;

	public Transform wantToLookAt;

	public Animator myAnim;

	public EyesScript eyes;

	public float lookingAtWeight;

	public LayerMask lookLayers;

	private NPCAI myAi;

	private NPCHoldsItems holdingItem;

	private bool usingItem;

	private float bodyWeight;

	private WaitForSeconds lookWait = new WaitForSeconds(1f);

	public void Start()
	{
	}

	private void OnEnable()
	{
		myAi = GetComponent<NPCAI>();
		lookTrans.parent = null;
		lookWait = new WaitForSeconds(Random.Range(0.8f, 1f));
		holdingItem = GetComponent<NPCHoldsItems>();
		StartCoroutine(randomLookAt());
	}

	private void FixedUpdate()
	{
		if (myAi.doesTask.isScared)
		{
			eyes.eyeLookAtTrans.localPosition = Vector2.zero;
			bodyWeight = Mathf.Lerp(bodyWeight, 0f, Time.deltaTime);
			lookingAtWeight = Mathf.Lerp(bodyWeight, 0f, Time.deltaTime);
			moveLookTrans(base.transform.position + base.transform.forward + Vector3.up * 1.5f, 2f);
		}
		else if (holdingItem.usingItem && (bool)holdingItem.currentlyHolding && holdingItem.currentlyHolding.useRightHandAnim)
		{
			bodyWeight = Mathf.Lerp(bodyWeight, 0f, Time.deltaTime * 15f);
			lookingAtWeight = Mathf.Lerp(lookingAtWeight, 0f, Time.deltaTime * 8f);
			eyes.eyeLookAtTrans.localPosition = eyes.eyeLookAtTrans.InverseTransformDirection((base.transform.position + base.transform.forward * 2f - (base.transform.position + Vector3.up * 1.25f)).normalized);
		}
		else if (holdingItem.usingItem && (bool)holdingItem.lookable)
		{
			bodyWeight = Mathf.Lerp(bodyWeight, 0f, Time.deltaTime * 8f);
			moveLookTrans(holdingItem.lookable.position, 10f);
			lookingAtWeight = Mathf.Lerp(lookingAtWeight, holdingItem.lookingWeight, Time.deltaTime * 5f);
			eyes.eyeLookAtTrans.localPosition = Vector2.zero;
		}
		else if ((bool)wantToLookAt)
		{
			bodyWeight = Mathf.Lerp(bodyWeight, 0f, Time.deltaTime);
			moveLookTrans(wantToLookAt.position + Vector3.up * 1.5f, 5f);
			eyes.eyeLookAtTrans.localPosition = eyes.eyeLookAtTrans.InverseTransformDirection((lookTrans.position - (base.transform.position + Vector3.up * 1.5f)).normalized);
			if (Vector3.Distance(wantToLookAt.position, base.transform.position) > 10f)
			{
				wantToLookAt = null;
			}
		}
		else
		{
			eyes.eyeLookAtTrans.localPosition = Vector2.zero;
			bodyWeight = Mathf.Lerp(bodyWeight, 0f, Time.deltaTime);
			moveLookTrans(base.transform.position + base.transform.forward + Vector3.up * 1.5f, 2f);
		}
	}

	public void moveLookTrans(Vector3 desiredPos, float multiplier)
	{
		desiredPos.y = Mathf.Clamp(desiredPos.y, base.transform.position.y, (base.transform.position + Vector3.up * 3f).y);
		lookTrans.position = Vector3.Lerp(lookTrans.position, desiredPos, Time.deltaTime * multiplier);
		Vector3 vector = lookTrans.position - base.transform.position;
		Vector3 forward = base.transform.forward;
		Vector3 normalized = (lookTrans.position - base.transform.position).normalized;
		if (!wantToLookAt)
		{
			lookingAtWeight = Mathf.Lerp(lookingAtWeight, 0f, Time.deltaTime * 1.5f);
		}
		else if (myAi.talkingTo == 0)
		{
			lookingAtWeight = Mathf.Lerp(lookingAtWeight, Mathf.Clamp01(Vector3.Dot(normalized, base.transform.forward)), Time.deltaTime * 1.5f);
		}
		else
		{
			lookingAtWeight = Mathf.Lerp(lookingAtWeight, 0.15f, Time.deltaTime * 1.5f);
		}
	}

	private void OnAnimatorIK(int layerIndex)
	{
		myAnim.SetLookAtPosition(lookTrans.position);
		myAnim.SetLookAtWeight(1f, bodyWeight, lookingAtWeight);
	}

	private IEnumerator randomLookAt()
	{
		while (true)
		{
			yield return lookWait;
			if (myAi.talkingTo == 0 && !myAi.doesTask.isScared)
			{
				if (Physics.CheckSphere(base.transform.position + base.transform.forward * 5f, 5f, lookLayers))
				{
					Collider[] array = Physics.OverlapSphere(base.transform.position + base.transform.forward * 5f, 5f, lookLayers);
					if (array.Length != 0)
					{
						float num = 10f;
						int num2 = 0;
						for (int i = 0; i < array.Length; i++)
						{
							if (array[i].transform != base.transform)
							{
								float num3 = Vector3.Distance(array[i].transform.position, base.transform.position + base.transform.forward);
								if (num3 < num)
								{
									num = num3;
									num2 = i;
								}
							}
						}
						if (array[num2].transform != base.transform)
						{
							wantToLookAt = array[num2].transform;
						}
						else
						{
							wantToLookAt = null;
						}
					}
					else
					{
						wantToLookAt = null;
					}
				}
				else
				{
					wantToLookAt = null;
				}
			}
			else
			{
				wantToLookAt = myAi.getTalkingToTransform();
			}
		}
	}
}
