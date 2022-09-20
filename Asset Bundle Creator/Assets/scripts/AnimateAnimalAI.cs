using System.Collections;
using UnityEngine;

public class AnimateAnimalAI : MonoBehaviour
{
	private AnimalAI myAi;

	private Animator myAnim;

	private Vector3 lastPos = Vector3.zero;

	private float travelSpeed;

	public bool animateDirection;

	public float animationSpeedMultiplier = 1f;

	private float facingDir;

	private Vector3 oldDirection = Vector3.zero;

	public bool flying;

	public bool canSwimAndWalk;

	private float flyingHeight;

	private AnimalMakeSounds hasSounds;

	public LayerMask groundLayers;

	private static int walkAnimationName;

	private static int swimAnimationName;

	private static int takeHitAnimationName;

	public float swimHeight = -1f;

	private bool useNormalPos;

	public Transform[] normalPositions;

	private bool fishInterested;

	private Quaternion lastRotation;

	public AnimalEyes myEyes;

	private bool inWater;

	private float currentFlyingHeight;

	public bool neverLands;

	public bool useFlightHeightFlux;

	public float heightMaxFluxMax = 6f;

	public float flyHeightFluxLow = 6f;

	private float followY;

	private Vector3 followVel;

	private float[] lastSpeeds = new float[4];

	private int lastSpeedSlot;

	private float vel;

	private RaycastHit hit;

	private float scaleSwimDif = 1f;

	private void Start()
	{
		currentFlyingHeight = heightMaxFluxMax;
		walkAnimationName = Animator.StringToHash("WalkingSpeed");
		swimAnimationName = Animator.StringToHash("Swimming");
		takeHitAnimationName = Animator.StringToHash("TakeHit");
		myAi = GetComponent<AnimalAI>();
		myAnim = GetComponent<Animator>();
		if ((bool)myAnim)
		{
			myAnim.SetFloat("Offset", Random.Range(0f, 1f));
			if (animationSpeedMultiplier != 1f)
			{
				myAnim.SetFloat("Speed", animationSpeedMultiplier);
			}
			hasSounds = GetComponent<AnimalMakeSounds>();
		}
		if (normalPositions.Length != 0)
		{
			useNormalPos = true;
		}
		followY = base.transform.position.y;
		if (useFlightHeightFlux)
		{
			StartCoroutine(flyingHeightVariation());
		}
	}

	private void OnEnable()
	{
		if ((bool)myAnim)
		{
			myAnim.SetFloat("Offset", Random.Range(0f, 1f));
		}
		lastPos = base.transform.position;
		lastPos.y = 0f;
		flyingHeight = 0f;
	}

	private void OnDisable()
	{
	}

	private void LateUpdate()
	{
		if (!myAi.isServer && !myAi.isInJump())
		{
			if (canSwimAndWalk && base.transform.position.y < 1f && WorldManager.manageWorld.waterMap[Mathf.RoundToInt(myAi.myAgent.transform.position.x / 2f), Mathf.RoundToInt(myAi.myAgent.transform.position.z / 2f)] && WorldManager.manageWorld.heightMap[Mathf.RoundToInt(myAi.myAgent.transform.position.x / 2f), Mathf.RoundToInt(myAi.myAgent.transform.position.z / 2f)] < 0)
			{
				followY = base.transform.position.y;
			}
			else if (Physics.Raycast(base.transform.position + Vector3.up / 2f, Vector3.down, out hit, 3f, groundLayers))
			{
				followY = Mathf.Lerp(followY, hit.point.y, Time.deltaTime * 8f);
				base.transform.position = new Vector3(base.transform.position.x, followY, base.transform.position.z);
				lastPos = base.transform.position;
				lastPos.y = 0f;
			}
			else
			{
				followY = base.transform.position.y;
			}
		}
	}

	private void Update()
	{
		if ((bool)myAnim)
		{
			animateBasedOnSpeed();
		}
	}

	private void FixedUpdate()
	{
		if (!myAi.isServer)
		{
			return;
		}
		bool flag = (bool)myAi.myAgent;
		Vector3 vector = myAi.myAgent.transform.position;
		if (canSwimAndWalk)
		{
			if (myAi.myAgent.transform.position.y < 1f && WorldManager.manageWorld.waterMap[Mathf.RoundToInt(myAi.myAgent.transform.position.x / 2f), Mathf.RoundToInt(myAi.myAgent.transform.position.z / 2f)] && WorldManager.manageWorld.heightMap[Mathf.RoundToInt(myAi.myAgent.transform.position.x / 2f), Mathf.RoundToInt(myAi.myAgent.transform.position.z / 2f)] < 0)
			{
				vector.y = swimHeight;
				inWater = true;
			}
			else
			{
				inWater = false;
			}
		}
		if (myAi.waterOnly)
		{
			vector = (fishInterested ? new Vector3(myAi.myAgent.transform.position.x, 0f - scaleSwimDif, myAi.myAgent.transform.position.z) : ((!myAi.currentlyAttacking()) ? new Vector3(myAi.myAgent.transform.position.x, Mathf.Clamp(WorldManager.manageWorld.heightMap[(int)myAi.myAgent.transform.position.x / 2, (int)myAi.myAgent.transform.position.z / 2], -20f, 0f - scaleSwimDif), myAi.myAgent.transform.position.z) : new Vector3(myAi.myAgent.transform.position.x, Mathf.Clamp(myAi.currentlyAttacking().position.y, -20f, 0f - scaleSwimDif), myAi.myAgent.transform.position.z)));
		}
		if (flying)
		{
			if (myAi.isDead())
			{
				flyingHeight = Mathf.Lerp(flyingHeight, 0f, Time.deltaTime * 15f);
			}
			else if ((bool)myAi.currentlyAttacking() && Vector3.Distance(new Vector3(myAi.currentlyAttacking().position.x, 0f, myAi.currentlyAttacking().position.z), new Vector3(base.transform.position.x, 0f, base.transform.position.z)) <= 4f)
			{
				flyingHeight = Mathf.Lerp(flyingHeight, 2f, Time.deltaTime * 5f);
			}
			else if (neverLands || (myAi.myAgent.speed > myAi.getBaseSpeed() && !myAi.checkIfHasArrivedAtDestination()))
			{
				flyingHeight = Mathf.Lerp(flyingHeight, currentFlyingHeight, Time.deltaTime * 5f);
			}
			else
			{
				flyingHeight = Mathf.Lerp(flyingHeight, 0f, Time.deltaTime * 10f);
			}
		}
		if (!inWater && !myAi.waterOnly && Physics.Raycast(vector + Vector3.up / 2f, Vector3.down, out hit, 4f, groundLayers))
		{
			vector.y = hit.point.y;
		}
		if (!myAi.isInJump())
		{
			base.transform.position = Vector3.SmoothDamp(base.transform.position, vector + Vector3.up * flyingHeight, ref followVel, 0.25f);
		}
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, myAi.myAgent.transform.rotation, Time.deltaTime * myAi.myAgent.angularSpeed);
	}

	public IEnumerator flyingHeightVariation()
	{
		bool goingDown = false;
		float variationChangeTimer = 0f;
		while (true)
		{
			yield return null;
			while (variationChangeTimer < 2f)
			{
				if (goingDown)
				{
					currentFlyingHeight = Mathf.Lerp(heightMaxFluxMax, flyHeightFluxLow, variationChangeTimer / 2f);
				}
				else
				{
					currentFlyingHeight = Mathf.Lerp(flyHeightFluxLow, heightMaxFluxMax, variationChangeTimer / 2f);
				}
				variationChangeTimer += Time.deltaTime;
				yield return null;
			}
			goingDown = !goingDown;
			variationChangeTimer = 0f;
		}
	}

	private float getAverageSpeed(float latestSpeed)
	{
		lastSpeeds[lastSpeedSlot] = latestSpeed;
		lastSpeedSlot++;
		if (lastSpeedSlot > 3)
		{
			lastSpeedSlot = 0;
		}
		float num = 0f;
		for (int i = 0; i < 4; i++)
		{
			num += lastSpeeds[i];
		}
		return num / 4f;
	}

	private void animateBasedOnSpeed()
	{
		float num = Vector3.Distance(new Vector3(base.transform.position.x, 0f, base.transform.position.z), lastPos) / Time.deltaTime / myAi.getBaseSpeed();
		if (num > 0.15f)
		{
			travelSpeed = Mathf.SmoothDamp(travelSpeed, num, ref vel, 0.1f);
		}
		else
		{
			if (Quaternion.Angle(base.transform.rotation, lastRotation) > 0.25f)
			{
				num = 1f;
			}
			if (num <= 0.1f)
			{
				num = 0f;
			}
			travelSpeed = Mathf.SmoothDamp(travelSpeed, num, ref vel, 0.1f);
		}
		if (neverLands)
		{
			travelSpeed = Mathf.Clamp(travelSpeed, 1.5f, 2f);
		}
		lastRotation = base.transform.rotation;
		lastPos = base.transform.position;
		lastPos.y = 0f;
		myAnim.SetFloat(walkAnimationName, travelSpeed);
		if (canSwimAndWalk)
		{
			if (myAi.myAgent.transform.position.y < 1f && WorldManager.manageWorld.waterMap[Mathf.RoundToInt(myAi.myAgent.transform.position.x / 2f), Mathf.RoundToInt(myAi.myAgent.transform.position.z / 2f)] && WorldManager.manageWorld.heightMap[Mathf.RoundToInt(myAi.myAgent.transform.position.x / 2f), Mathf.RoundToInt(myAi.myAgent.transform.position.z / 2f)] < 0)
			{
				myAnim.SetBool(swimAnimationName, true);
			}
			else
			{
				myAnim.SetBool(swimAnimationName, false);
			}
		}
		if (animateDirection)
		{
			facingDir = Mathf.Lerp(facingDir, leftOrRight(oldDirection, base.transform.forward, Vector3.up), Time.deltaTime * 1.5f);
			oldDirection = base.transform.forward;
			myAnim.SetFloat("Direction", facingDir);
		}
	}

	public float leftOrRight(Vector3 fwd, Vector3 targetDir, Vector3 up)
	{
		float num = Vector3.Dot(Vector3.Cross(fwd, targetDir), up);
		if (num > 0.005f)
		{
			if (num > 0.01f)
			{
				return 1f;
			}
			return 0.5f;
		}
		if (num < -0.005f)
		{
			if (num < -0.01f)
			{
				return -1f;
			}
			return -0.5f;
		}
		return 0f;
	}

	public void takeAHitLocal()
	{
		if ((bool)hasSounds)
		{
			hasSounds.playDamageSound();
		}
		if ((bool)myAnim)
		{
			myAnim.SetTrigger(takeHitAnimationName);
		}
	}

	public void playDeathAnimation()
	{
		if ((bool)myEyes)
		{
			myEyes.deadEyes();
		}
		if ((bool)GetComponent<AnimalMakeSounds>())
		{
			GetComponent<AnimalMakeSounds>().playDamageSound();
		}
		if ((bool)GetComponent<Ragdoll>())
		{
			Invoke("activateRagdoll", 0.35f);
		}
		myAnim.SetTrigger("Die");
	}

	public void activateRagdoll()
	{
		GetComponent<Ragdoll>().enableRagDoll();
	}

	public void setScaleSwimDif()
	{
		scaleSwimDif = Mathf.Clamp(base.transform.localScale.y, 1f, 3f);
	}

	public void makeFishInterested(bool newInterested)
	{
		fishInterested = newInterested;
	}

	public void setAnimator(Animator anim)
	{
		myAnim = anim;
		myAnim.SetFloat("Offset", Random.Range(0f, 1f));
		if ((bool)GetComponent<AnimalAI_Attack>())
		{
			GetComponent<AnimalAI_Attack>().setAnimator(myAnim);
		}
	}
}
