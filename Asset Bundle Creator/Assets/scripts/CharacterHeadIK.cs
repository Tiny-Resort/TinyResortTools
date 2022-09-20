using System.Collections;
using UnityEngine;

public class CharacterHeadIK : MonoBehaviour
{
	private EquipItemToChar myEquip;

	private CharInteract myInteract;

	private NetworkFishingRod myRod;

	private CharPickUp myPickup;

	private Damageable myDamage;

	public Transform lookTrans;

	public Transform wantToLookAt;

	public Animator myAnim;

	public EyesScript eyes;

	private float lookingAtWeight;

	public LayerMask lookLayers;

	private float bodyWeight;

	private bool isEating;

	private WaitForSeconds lookWait;

	private string lookingAtTag;

	private void Start()
	{
		myEquip = GetComponent<EquipItemToChar>();
		myInteract = GetComponent<CharInteract>();
		myRod = GetComponent<NetworkFishingRod>();
		myPickup = GetComponent<CharPickUp>();
		myDamage = GetComponent<Damageable>();
		lookTrans.parent = null;
		lookWait = new WaitForSeconds(Random.Range(0.2f, 0.3f));
		StartCoroutine(randomLookAt());
	}

	public void setIsEating(bool newEating)
	{
		isEating = newEating;
	}

	private void Update()
	{
		if (myEquip.isInVehicle())
		{
			bodyWeight = 0f;
			lookingAtWeight = 0f;
			eyes.eyeLookAtTrans.localPosition = Vector3.zero;
		}
		else if (myEquip.checkIfDoingEmote() || isEating || myEquip.getDriving() || myPickup.isLayingDown() || myDamage.health <= 0)
		{
			bodyWeight = Mathf.Lerp(bodyWeight, 0f, Time.deltaTime * 15f);
			lookingAtWeight = Mathf.Lerp(lookingAtWeight, 0f, Time.deltaTime * 8f);
			eyes.eyeLookAtTrans.localPosition = Vector3.zero;
		}
		else if ((myEquip.usingItem && (bool)myEquip.currentlyHolding && myEquip.currentlyHolding.useRightHandAnim) || (myEquip.lookLock && (bool)myEquip.currentlyHolding && myEquip.currentlyHolding.useRightHandAnim))
		{
			bodyWeight = Mathf.Lerp(bodyWeight, 0f, Time.deltaTime * 15f);
			lookingAtWeight = Mathf.Lerp(lookingAtWeight, 0f, Time.deltaTime * 8f);
			eyes.eyeLookAtTrans.localPosition = eyes.eyeLookAtTrans.InverseTransformDirection((base.transform.position + base.transform.forward * 2f - (base.transform.position + Vector3.up * 1.25f)).normalized);
		}
		else if ((myEquip.usingItem && (bool)myEquip.lookable) || (myEquip.lookLock && (bool)myEquip.lookable))
		{
			bodyWeight = myEquip.lookingWeight;
			lookTrans.transform.position = myEquip.lookable.position;
			lookingAtWeight = Mathf.Lerp(lookingAtWeight, 0f, Time.deltaTime * 8f);
			eyes.eyeLookAtTrans.localPosition = eyes.eyeLookAtTrans.InverseTransformDirection((base.transform.position + base.transform.forward * 2f - (base.transform.position + Vector3.up * 1.25f)).normalized);
		}
		else if ((bool)wantToLookAt)
		{
			bodyWeight = Mathf.Lerp(bodyWeight, 0f, Time.deltaTime * 10f);
			if (lookingAtTag != "Drop")
			{
				moveLookTrans(wantToLookAt.position + Vector3.up * 1.5f, 5f);
			}
			else
			{
				moveLookTrans(wantToLookAt.position + Vector3.up * 0.75f, 5f);
			}
			eyes.eyeLookAtTrans.localPosition = eyes.eyeLookAtTrans.InverseTransformDirection((lookTrans.position - (base.transform.position + Vector3.up * 1.5f)).normalized);
			if (Vector3.Distance(wantToLookAt.position, base.transform.position) > 10f)
			{
				wantToLookAt = null;
				eyes.eyeLookAtTrans.localPosition = Vector2.zero;
				lookTrans.position = base.transform.position + base.transform.forward + Vector3.up * 1.5f;
			}
		}
		else
		{
			eyes.eyeLookAtTrans.localPosition = Vector2.zero;
			bodyWeight = Mathf.Lerp(bodyWeight, 0f, Time.deltaTime * 10f);
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
		if ((bool)wantToLookAt)
		{
			lookingAtWeight = Mathf.Lerp(lookingAtWeight, Mathf.Clamp01(Vector3.Dot(normalized, base.transform.forward)) / 2f, Time.deltaTime * 1.5f);
		}
		else
		{
			lookingAtWeight = Mathf.Lerp(lookingAtWeight, 0f, Time.deltaTime * 3f);
		}
	}

	private void OnAnimatorIK()
	{
		if ((double)lookingAtWeight >= 0.01 || (double)bodyWeight >= 0.01)
		{
			myAnim.SetLookAtPosition(lookTrans.position);
			myAnim.SetLookAtWeight(1f, bodyWeight, lookingAtWeight);
		}
	}

	private IEnumerator randomLookAt()
	{
		while (true)
		{
			yield return lookWait;
			if (BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen)
			{
				wantToLookAt = myEquip.holdPos.Find("BugInHand(Clone)/Animation/RightHandle");
				if ((bool)(wantToLookAt = null))
				{
					wantToLookAt = myEquip.holdPos.Find("FishHandItemLarge(Clone)/Animation/fish");
				}
				eyes.happyMouthUntilCelebrationOver();
				while (BugAndFishCelebration.bugAndFishCel.celebrationWindowOpen)
				{
					yield return null;
				}
			}
			if (Physics.CheckSphere(base.transform.position + base.transform.forward * 3f, 3f, lookLayers))
			{
				Collider[] array = Physics.OverlapSphere(base.transform.position + base.transform.forward * 3f, 3f, lookLayers);
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
						if (array[num2].transform != wantToLookAt)
						{
							wantToLookAt = array[num2].transform;
							lookingAtTag = wantToLookAt.tag;
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
				wantToLookAt = null;
			}
		}
	}
}
