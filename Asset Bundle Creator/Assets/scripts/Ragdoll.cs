using System.Collections;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
	public Rigidbody[] ragdollRigs;

	public CapsuleCollider[] ragdollColiders;

	public CharacterJoint[] characterJoints;

	public Collider[] livingColiders;

	public bool turnOnRagDoll;

	private bool ragDollOn;

	public Animator myAnim;

	private Vector3[] anchorPositions;

	public Transform[] allBones;

	private Quaternion[] allBonesRot;

	private Vector3[] allBonesLoc;

	private void Awake()
	{
		anchorPositions = new Vector3[characterJoints.Length];
		for (int i = 0; i < anchorPositions.Length; i++)
		{
			anchorPositions[i] = characterJoints[i].connectedAnchor;
		}
		allBonesRot = new Quaternion[allBones.Length];
		allBonesLoc = new Vector3[allBones.Length];
		for (int j = 0; j < allBones.Length; j++)
		{
			allBonesRot[j] = allBones[j].localRotation;
			allBonesLoc[j] = allBones[j].localPosition;
		}
	}

	private void Update()
	{
		if (turnOnRagDoll)
		{
			turnOnRagDoll = false;
			enableRagDoll();
		}
	}

	private void OnEnable()
	{
		disableRagDoll();
	}

	private void OnDisable()
	{
		resetBonePos();
	}

	public void enableRagDoll()
	{
		areRigsOn(true);
		areColidersOn(true);
		areLivingColidersOn(false);
		ragDollOn = true;
		myAnim.enabled = false;
	}

	public void disableRagDoll()
	{
		areRigsOn(false);
		areColidersOn(false);
		ragdollRigs[0].transform.localRotation = Quaternion.identity;
		ragdollRigs[0].transform.localPosition = Vector3.zero;
		areLivingColidersOn(true);
		ragDollOn = false;
		myAnim.enabled = true;
	}

	private void areRigsOn(bool onOrOff)
	{
		for (int i = 0; i < ragdollRigs.Length; i++)
		{
			if (!ragdollRigs[i].isKinematic)
			{
				ragdollRigs[i].velocity = new Vector3(0f, 0f, 0f);
				ragdollRigs[i].angularVelocity = new Vector3(0f, 0f, 0f);
			}
			ragdollRigs[i].isKinematic = !onOrOff;
		}
	}

	private void areColidersOn(bool onOrOff)
	{
		for (int i = 0; i < ragdollColiders.Length; i++)
		{
			ragdollColiders[i].enabled = onOrOff;
		}
	}

	private void areLivingColidersOn(bool onOrOff)
	{
		for (int i = 0; i < livingColiders.Length; i++)
		{
			livingColiders[i].enabled = onOrOff;
		}
	}

	private IEnumerator keepBodyCloseToAgent()
	{
		while (ragDollOn)
		{
			yield return null;
			ragdollRigs[0].transform.localPosition = new Vector3(Mathf.Clamp(ragdollRigs[0].transform.localPosition.x, -2f, 2f), ragdollRigs[0].transform.localPosition.y, Mathf.Clamp(ragdollRigs[0].transform.localPosition.z, -2f, 2f));
		}
	}

	public void resetBonePos()
	{
		for (int i = 0; i < allBones.Length; i++)
		{
			allBones[i].localRotation = allBonesRot[i];
			allBones[i].localPosition = allBonesLoc[i];
		}
		for (int j = 0; j < anchorPositions.Length; j++)
		{
			characterJoints[j].connectedAnchor = anchorPositions[j];
		}
	}
}
