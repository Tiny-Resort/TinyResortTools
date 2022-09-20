using UnityChan;
using UnityEngine;

public class CharNetworkAnimator : MonoBehaviour
{
	private float travelSpeed;

	private Vector3 lastPos = Vector3.zero;

	private Vector3 lastPosVehicle = Vector3.zero;

	private bool isOnVehicle;

	public CharMovement myMovement;

	public Animator myAnim;

	public static int groundedAnimName;

	public static int walkingAnimName;

	public static int swimmingAnimName;

	public static int underwaterAnimName;

	public static int usingAnimName;

	public static int blockingAnimName;

	public bool grounded;

	public SpringManager hairSpring;

	private float desiredTravelSpeed;

	public GameObject navMeshObsticle;

	private float hairVel;

	private void Start()
	{
		groundedAnimName = Animator.StringToHash("Grounded");
		walkingAnimName = Animator.StringToHash("WalkSpeed");
		swimmingAnimName = Animator.StringToHash("Swimming");
		underwaterAnimName = Animator.StringToHash("UnderWater");
		usingAnimName = Animator.StringToHash("Using");
		blockingAnimName = Animator.StringToHash("Blocking");
		lastPos = base.transform.position;
	}

	private void FixedUpdate()
	{
		grounded = Physics.CheckSphere(base.transform.position, 0.3f, myMovement.jumpLayers);
		if (grounded)
		{
			navMeshObsticle.SetActive(true);
		}
		else
		{
			navMeshObsticle.SetActive(false);
		}
		if (!myMovement.isLocalPlayer)
		{
			bool flag = Physics.CheckSphere(base.transform.position, 0.1f, myMovement.swimLayers);
			if (myMovement.standingOn != 0)
			{
				if (isOnVehicle)
				{
					desiredTravelSpeed = Vector3.Distance(myMovement.transform.localPosition, lastPosVehicle) / Time.fixedDeltaTime / 9f;
					travelSpeed = Mathf.Lerp(travelSpeed, desiredTravelSpeed, Time.fixedDeltaTime * 6f);
				}
				else
				{
					travelSpeed = Mathf.Lerp(travelSpeed, 0f, Time.fixedDeltaTime * 12f);
				}
				isOnVehicle = true;
				lastPosVehicle = myMovement.transform.localPosition;
			}
			else if (grounded || flag || myMovement.underWater)
			{
				isOnVehicle = false;
				desiredTravelSpeed = Vector3.Distance(base.transform.position, lastPos) / Time.fixedDeltaTime / 9f;
				travelSpeed = Mathf.Lerp(travelSpeed, desiredTravelSpeed, Time.fixedDeltaTime * 6f);
			}
			else
			{
				desiredTravelSpeed = 0.75f;
				isOnVehicle = false;
				travelSpeed = Mathf.Lerp(travelSpeed, 0f, Time.deltaTime * 2f);
			}
			lastPos = base.transform.position;
			myAnim.SetBool(groundedAnimName, grounded);
			myAnim.SetFloat(walkingAnimName, travelSpeed * 2f);
			myAnim.SetBool(swimmingAnimName, flag);
			myAnim.SetBool(underwaterAnimName, myMovement.underWater);
			if ((bool)hairSpring)
			{
				if (desiredTravelSpeed > 0.5f)
				{
					hairSpring.dynamicRatio = Mathf.SmoothDamp(hairSpring.dynamicRatio, Mathf.Clamp(1f - desiredTravelSpeed, 0.15f, 1f), ref hairVel, 0.05f);
				}
				else
				{
					hairSpring.dynamicRatio = Mathf.SmoothDamp(hairSpring.dynamicRatio, Mathf.Clamp(1f - desiredTravelSpeed, 0.15f, 1f), ref hairVel, 0.15f);
				}
			}
			return;
		}
		if ((bool)hairSpring || (myMovement.myPickUp.drivingVehicle && myMovement.myPickUp.currentlyDriving.animateCharAsWell))
		{
			if (myMovement.standingOn != 0)
			{
				if (isOnVehicle)
				{
					desiredTravelSpeed = Vector3.Distance(myMovement.transform.localPosition, lastPosVehicle) / Time.fixedDeltaTime / 9f;
					travelSpeed = Mathf.Lerp(travelSpeed, desiredTravelSpeed, Time.fixedDeltaTime * 6f);
				}
				else
				{
					travelSpeed = Mathf.Lerp(travelSpeed, 0f, Time.fixedDeltaTime * 12f);
				}
				isOnVehicle = true;
				lastPosVehicle = myMovement.transform.localPosition;
			}
			else if (myMovement.grounded || myMovement.swimming || myMovement.underWater)
			{
				isOnVehicle = false;
				desiredTravelSpeed = Vector3.Distance(base.transform.position, lastPos) / Time.fixedDeltaTime / 9f;
				travelSpeed = Mathf.Lerp(travelSpeed, desiredTravelSpeed, Time.fixedDeltaTime * 6f);
			}
			else
			{
				desiredTravelSpeed = 0.75f;
				isOnVehicle = false;
				travelSpeed = Mathf.Lerp(travelSpeed, 0f, Time.deltaTime * 2f);
			}
			if (myMovement.myPickUp.drivingVehicle && myMovement.myPickUp.currentlyDriving.animateCharAsWell)
			{
				myAnim.SetFloat(walkingAnimName, travelSpeed * 2f);
			}
			if ((bool)hairSpring)
			{
				if (desiredTravelSpeed > 0.5f)
				{
					hairSpring.dynamicRatio = Mathf.SmoothDamp(hairSpring.dynamicRatio, Mathf.Clamp(1f - desiredTravelSpeed, 0.15f, 1f), ref hairVel, 0.05f);
				}
				else
				{
					hairSpring.dynamicRatio = Mathf.SmoothDamp(hairSpring.dynamicRatio, Mathf.Clamp(1f - desiredTravelSpeed, 0.15f, 1f), ref hairVel, 0.15f);
				}
			}
		}
		lastPos = base.transform.position;
	}
}
