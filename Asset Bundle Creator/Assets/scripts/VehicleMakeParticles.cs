using System.Collections;
using UnityEngine;

public class VehicleMakeParticles : MonoBehaviour
{
	[Header("Animation ----------")]
	public Animator myAnim;

	public bool hasMinSpeedAnim;

	public bool hasDirectionAnimation;

	public bool animateDriver;

	public bool animateMovement;

	public bool animateGrounded;

	public bool animateBackwards;

	public string moveVariableName = "MoveSpeed";

	public float animationMultiplier = 1f;

	public float turnSpeedAnimSpeed = 10f;

	public bool turningEffectsMovementSpeed;

	[Header("Other Grounded Animations")]
	public bool animateOtherGrounded;

	[Header("Particles ----------")]
	public bool idleParticles;

	public bool particlesOnMove;

	public Transform[] particlePositions;

	public LayerMask groundLayers;

	public bool useTireTracks;

	[Header("Sound Stuff ----------")]
	public AudioSource myAudio;

	public float engineMaxVolume = 0.1f;

	public bool playSoundEvenWhenNotMoving = true;

	public float numberOfGears = 6f;

	public float baseLinePitch = 1.5f;

	private Vehicle myVehicle;

	private ControlVehicle myControl;

	private Vector3 lasPos;

	private float animationSpeed;

	private float animationSpeedGoal;

	private float movementSpeed;

	private float dir;

	private Vector3 oldDirection;

	private float desiredPitch = 1f;

	private float currentPitch = 1f;

	[Header("Grounded Sounds ----------")]
	public bool useGroundedSounds;

	public ASound onNotGroundedSound;

	public ASound onGroundedSound;

	private bool lastGrounded = true;

	public bool splashWhenGrouned;

	[Header("Drag Sounds ----------")]
	public AudioSource dragSound;

	public AudioSource dragWhenGrounded;

	private float dragVolumeMax = 0.075f;

	private float dragGroundedMax = 0.1f;

	private bool lastSlowSurface;

	private Animator characterAnimator;

	public bool hasRumble = true;

	private WaitForSeconds wait = new WaitForSeconds(0.05f);

	private void Start()
	{
		myVehicle = GetComponent<Vehicle>();
		myControl = GetComponent<ControlVehicle>();
		if ((bool)myAudio)
		{
			StartCoroutine(makeVehicleSounds());
		}
		if ((bool)dragSound)
		{
			dragVolumeMax = dragSound.volume;
			dragSound.volume = 0f;
		}
		if ((bool)dragWhenGrounded)
		{
			dragGroundedMax = dragWhenGrounded.volume;
			dragWhenGrounded.volume = 0f;
		}
	}

	private void OnEnable()
	{
		lasPos = base.transform.position;
		if (particlesOnMove)
		{
			StartCoroutine(movementParticles());
		}
		if ((bool)myAudio)
		{
			myAudio.pitch = baseLinePitch / 1.5f;
		}
	}

	private void FixedUpdate()
	{
		movementSpeed = Mathf.Lerp(movementSpeed, Vector3.Distance(base.transform.position, lasPos) / Time.fixedDeltaTime, Time.deltaTime * 2f);
		if (animateBackwards)
		{
			Vector3 rhs = lasPos - base.transform.position;
			myAnim.SetBool("MovingBackwards", Vector3.Dot(base.transform.forward, rhs) > 0f);
		}
		lasPos = base.transform.position;
		if (hasDirectionAnimation)
		{
			dir = Mathf.Lerp(dir, leftOrRight(oldDirection, base.transform.forward, Vector3.up), Time.deltaTime * turnSpeedAnimSpeed);
			oldDirection = base.transform.forward;
			myAnim.SetFloat("Direction", dir);
		}
		if (useGroundedSounds)
		{
			if (lastGrounded != myControl.isGrounded())
			{
				if (myControl.isGrounded())
				{
					SoundManager.manage.playASoundAtPoint(onGroundedSound, base.transform.position);
					if (splashWhenGrouned)
					{
						Transform[] array = particlePositions;
						foreach (Transform splashPos in array)
						{
							ParticleManager.manage.bigSplash(splashPos);
						}
					}
				}
				else
				{
					SoundManager.manage.playASoundAtPoint(onNotGroundedSound, base.transform.position);
					if (splashWhenGrouned)
					{
						Transform[] array = particlePositions;
						foreach (Transform splashPos2 in array)
						{
							ParticleManager.manage.bigSplash(splashPos2);
						}
					}
				}
			}
			lastGrounded = myControl.isGrounded();
		}
		if ((bool)dragSound)
		{
			if (myControl.isGroundedOnSlowSurface() && !myControl.isGrounded())
			{
				dragSound.volume = Mathf.Lerp(dragSound.volume, movementSpeed / myControl.maxSpeed * dragVolumeMax * SoundManager.manage.getSoundVolume(), Time.deltaTime * 10f);
			}
			else
			{
				dragSound.volume = Mathf.Lerp(dragSound.volume, 0f, Time.deltaTime * 4f);
			}
		}
		if ((bool)dragWhenGrounded)
		{
			if (myControl.isGrounded())
			{
				dragWhenGrounded.volume = Mathf.Lerp(dragWhenGrounded.volume, movementSpeed / myControl.maxSpeed * dragGroundedMax * SoundManager.manage.getSoundVolume(), Time.deltaTime * 10f);
			}
			else
			{
				dragWhenGrounded.volume = Mathf.Lerp(dragWhenGrounded.volume, 0f, Time.deltaTime * 4f);
			}
		}
	}

	public float getCurrentSpeed()
	{
		return movementSpeed / myControl.maxSpeed;
	}

	public bool getGrounded()
	{
		return myControl.isGrounded();
	}

	private void Update()
	{
		if (idleParticles)
		{
			makeParticles();
		}
		if ((bool)myAnim && animateDriver && (bool)characterAnimator && hasDirectionAnimation)
		{
			characterAnimator.SetFloat("VehicleDir", dir);
		}
		if (!animateMovement)
		{
			return;
		}
		if (myControl.canFly && myVehicle.hasDriver())
		{
			if (!myControl.isGrounded())
			{
				animationSpeedGoal = Mathf.Lerp(animationSpeed, 2f, Time.deltaTime * 2f);
			}
			else
			{
				animationSpeedGoal = Mathf.Lerp(animationSpeed, 0.35f, Time.deltaTime);
			}
		}
		else if (hasMinSpeedAnim)
		{
			animationSpeedGoal = movementSpeed / myControl.maxSpeed * animationMultiplier;
			if (animationSpeedGoal > 0.1f)
			{
				animationSpeedGoal = Mathf.Clamp(animationSpeedGoal, 0.35f, 2f);
			}
		}
		else
		{
			animationSpeedGoal = movementSpeed / myControl.maxSpeed * animationMultiplier;
		}
		if (turningEffectsMovementSpeed && (dir > 0.2f || dir < -0.2f))
		{
			animationSpeedGoal = Mathf.Clamp(animationSpeedGoal, 0.35f, 2f);
		}
		if (animateGrounded)
		{
			if (myControl.isGrounded() || myControl.isGroundedOnSlowSurface())
			{
				myAnim.SetBool("Grounded", true);
			}
			else
			{
				myAnim.SetBool("Grounded", false);
			}
			if (myControl.isGrounded() || myControl.canFly)
			{
				animationSpeed = Mathf.Lerp(animationSpeed, animationSpeedGoal, Time.deltaTime * 25f);
			}
			else if (!animateOtherGrounded)
			{
				animationSpeed = Mathf.Lerp(animationSpeed, 0f, Time.deltaTime * 2f);
			}
		}
		else
		{
			animationSpeed = Mathf.Lerp(animationSpeed, animationSpeedGoal, Time.deltaTime * 25f);
		}
		if (animateOtherGrounded)
		{
			if (!myControl.isGrounded() && myControl.isGroundedOnSlowSurface())
			{
				myAnim.SetBool("OtherGrounded", true);
				animationSpeedGoal = movementSpeed / myControl.maxSpeed / myControl.slowDownDividedBy * animationMultiplier;
				animationSpeed = Mathf.Lerp(animationSpeed, animationSpeedGoal, Time.deltaTime * 25f);
			}
			else
			{
				myAnim.SetBool("OtherGrounded", false);
			}
		}
		myAnim.SetFloat(moveVariableName, animationSpeed);
	}

	public void setCharacterAnimator(Animator newAnim)
	{
		characterAnimator = newAnim;
	}

	public void makeParticles()
	{
		Transform[] array = particlePositions;
		foreach (Transform transform in array)
		{
			ParticleManager.manage.waterWakePart(transform.position, 1);
		}
	}

	public void makeMoveParticles()
	{
		Transform[] array = particlePositions;
		foreach (Transform transform in array)
		{
			ParticleManager.manage.emitFeetDust(WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2]].footStepParticle, transform.position, transform.rotation);
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

	private IEnumerator makeVehicleSounds()
	{
		int currentGear = 1;
		float num = movementSpeed / myControl.maxSpeed;
		float gearTimer = 0f;
		int num3 = (int)(num * numberOfGears);
		while (true)
		{
			if (myControl.canFly && myVehicle.hasDriver())
			{
				animationSpeedGoal = Mathf.Lerp(animationSpeed, 2f, Time.deltaTime * 2f);
				myAudio.volume = Mathf.Lerp(myAudio.volume, engineMaxVolume * SoundManager.manage.getSoundVolume(), Time.deltaTime);
				if (myControl.isGrounded())
				{
					myAudio.pitch = Mathf.Lerp(myAudio.pitch, baseLinePitch / 1.5f, Time.deltaTime * 2f);
				}
				else
				{
					myAudio.pitch = Mathf.Lerp(myAudio.pitch, baseLinePitch, Time.deltaTime * 2f);
				}
			}
			else if ((myVehicle.currentlyHasDriver() && playSoundEvenWhenNotMoving) || (myVehicle.currentlyHasDriver() && !playSoundEvenWhenNotMoving && movementSpeed > 0.1f))
			{
				myAudio.volume = Mathf.Lerp(myAudio.volume, engineMaxVolume * SoundManager.manage.getSoundVolume(), Time.deltaTime);
				num = movementSpeed / myControl.maxSpeed;
				if (gearTimer < 3f + num)
				{
					gearTimer += Time.deltaTime;
				}
				else
				{
					int num2 = (int)(num * numberOfGears);
					if (currentGear < num2)
					{
						currentGear = Mathf.Clamp(currentGear + 1, 0, (int)numberOfGears);
						myAudio.pitch = baseLinePitch;
					}
					else if (currentGear > num2)
					{
						currentGear = Mathf.Clamp(currentGear - 1, 0, (int)numberOfGears);
						myAudio.pitch = baseLinePitch;
					}
					gearTimer = 0f;
				}
				if (numberOfGears != 0f)
				{
					if (myControl.isGrounded())
					{
						desiredPitch = baseLinePitch + num + (float)currentGear / numberOfGears;
					}
					else
					{
						desiredPitch = (baseLinePitch + num + (float)currentGear / numberOfGears) * 1.15f;
					}
				}
				myAudio.pitch = Mathf.Lerp(myAudio.pitch, desiredPitch, Time.deltaTime * 2f);
			}
			else
			{
				myAudio.volume = Mathf.Lerp(myAudio.volume, 0f, Time.deltaTime * 2f);
				myAudio.pitch = Mathf.Lerp(myAudio.pitch, baseLinePitch, Time.deltaTime * 2f);
			}
			yield return null;
		}
	}

	private IEnumerator movementParticles()
	{
		float footparticleTimer = 0.03f;
		while (true)
		{
			yield return null;
			if (!(movementSpeed / myControl.maxSpeed > 0.18f))
			{
				continue;
			}
			for (int i = 0; i < particlePositions.Length; i++)
			{
				if (!Physics.CheckSphere(particlePositions[i].position, 0.2f, groundLayers))
				{
					continue;
				}
				if (footparticleTimer.Equals(0.03f))
				{
					if (WorldManager.manageWorld.waterMap[(int)particlePositions[i].position.x / 2, (int)particlePositions[i].position.z / 2] && particlePositions[i].position.y <= 0.6f)
					{
						ParticleManager.manage.waterSplash(particlePositions[i].position, 4);
					}
					else
					{
						ParticleManager.manage.emitFeetDust(WorldManager.manageWorld.tileTypes[WorldManager.manageWorld.tileTypeMap[(int)particlePositions[i].position.x / 2, (int)particlePositions[i].position.z / 2]].footStepParticle, particlePositions[i].position, particlePositions[i].rotation);
					}
				}
				if (WorldManager.manageWorld.waterMap[(int)particlePositions[i].position.x / 2, (int)particlePositions[i].position.z / 2] && particlePositions[i].position.y <= 0.6f)
				{
					ParticleManager.manage.waterWakePart(particlePositions[i].position, 1);
				}
				if (useTireTracks && movementSpeed / myControl.maxSpeed > 0.25f)
				{
					ParticleManager.manage.makeMotorbikeTrack(particlePositions[i]);
				}
			}
			footparticleTimer = ((!(footparticleTimer > 0f)) ? 0.03f : (footparticleTimer - Time.deltaTime));
		}
	}
}
