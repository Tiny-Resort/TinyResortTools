using System.Collections;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;

public class ControlVehicle : NetworkBehaviour
{
	private Vehicle myVehicle;

	public Transform[] groundedCheckPos;

	public Transform collisionCheckPos;

	private bool grounded = true;

	[Header("Speed details -----------")]
	private float speed;

	public float acceleration = 1f;

	public float maxSpeed = 8f;

	public float turnSpeed = 10f;

	private float flyingSpeed;

	[Header("Slow down on mask")]
	public float slowDownSpeed = 1f;

	public float slowDownDividedBy = 10f;

	public bool useSlowDownMask;

	public bool instantSlowSpeedOnEnter;

	public LayerMask slowDownMask;

	public Transform[] slowDownGroundedCheck;

	private bool isOnSlowDownLayer;

	[Header("Other Options -----------")]
	public bool canJump = true;

	public bool canFly;

	public bool canReverse = true;

	public bool onlyTurnIfMovingForward;

	public bool turningReversedWhenBackwards;

	public float reverseSpeedClamp0to1 = -0.25f;

	public LayerMask canJumpLayer;

	private Vector3 forwardDir;

	public Transform wallChecker;

	public LayerMask wallCheckerLayers;

	public bool damagedObjectsOnCollision;

	public LayerMask doesDamageLayer;

	private float vel;

	private static WaitForFixedUpdate jumpWait;

	public float jumpUpHeight = 3f;

	public float fallSpeed = -1f;

	private bool inJump;

	private void Start()
	{
		myVehicle = GetComponent<Vehicle>();
	}

	private void OnDestroy()
	{
		for (int i = 0; i < groundedCheckPos.Length; i++)
		{
			ParticleManager.manage.emitParticleAtPosition(ParticleManager.manage.allParts[3], groundedCheckPos[i].position);
		}
	}

	private void FixedUpdate()
	{
		if ((bool)myVehicle.damageWhenUnderWater && base.transform.position.y <= -1f)
		{
			int xPos = (int)base.transform.position.x / 2;
			int yPos = (int)base.transform.position.z / 2;
			if (!WorldManager.manageWorld.isPositionOnMap(xPos, yPos) || WorldManager.manageWorld.waterMap[(int)base.transform.position.x / 2, (int)base.transform.position.z / 2])
			{
				if (base.isServer)
				{
					myVehicle.damageWhenUnderWater.attackAndDoDamage(100, null, 0f);
				}
				ParticleManager.manage.bigSplash(base.transform, 5);
			}
		}
		if (groundedCheckPos.Length != 0)
		{
			for (int i = 0; i < groundedCheckPos.Length; i++)
			{
				grounded = Physics.CheckSphere(groundedCheckPos[i].position, 0.2f, canJumpLayer);
				if (grounded)
				{
					break;
				}
			}
		}
		else
		{
			grounded = Physics.CheckSphere(base.transform.position, 0.2f, canJumpLayer);
		}
		if (useSlowDownMask)
		{
			bool flag = isOnSlowDownLayer;
			if (slowDownGroundedCheck.Length != 0)
			{
				for (int j = 0; j < slowDownGroundedCheck.Length; j++)
				{
					isOnSlowDownLayer = Physics.CheckSphere(slowDownGroundedCheck[j].position, 0.2f, slowDownMask);
					if (isOnSlowDownLayer)
					{
						break;
					}
				}
			}
			else
			{
				isOnSlowDownLayer = Physics.CheckSphere(base.transform.position, 0.2f, slowDownMask);
			}
			if (instantSlowSpeedOnEnter && !flag && isOnSlowDownLayer)
			{
				speed = Mathf.Clamp(speed, 0f, maxSpeed / slowDownDividedBy);
			}
		}
		if (!base.hasAuthority)
		{
			return;
		}
		if (myVehicle.hasDriver() && myVehicle.mountingAnimationComplete)
		{
			float num = 0f - InputMaster.input.getLeftStick().x;
			float value = InputMaster.input.VehicleAccelerate();
			value = (canReverse ? Mathf.Clamp(value, reverseSpeedClamp0to1, 1f) : Mathf.Clamp01(value));
			bool isOnSlowDownLayer2 = isOnSlowDownLayer;
			RaycastHit hitInfo;
			if (Physics.Raycast(collisionCheckPos.position, collisionCheckPos.forward, out hitInfo, 0.25f, canJumpLayer))
			{
				value = Mathf.Clamp(value, -2f, 0f);
			}
			if ((grounded && !canFly) || (!grounded && canFly))
			{
				if (value < 0f && speed > 0.1f)
				{
					speed = Mathf.SmoothDamp(speed, value * maxSpeed, ref vel, 2f / (acceleration * 3f));
				}
				else
				{
					speed = Mathf.SmoothDamp(speed, value * maxSpeed, ref vel, 2f / acceleration);
				}
				forwardDir = Vector3.Lerp(forwardDir, base.transform.forward, Time.deltaTime * 3f);
			}
			else if (isOnSlowDownLayer)
			{
				forwardDir = Vector3.Lerp(forwardDir, base.transform.forward, Time.deltaTime);
				if (instantSlowSpeedOnEnter)
				{
					speed = Mathf.SmoothDamp(speed, value * maxSpeed / slowDownDividedBy, ref vel, 2f / acceleration);
				}
				else
				{
					speed = Mathf.SmoothDamp(speed, value * maxSpeed / slowDownDividedBy, ref vel, 2f / slowDownSpeed);
				}
			}
			else
			{
				speed = Mathf.SmoothDamp(speed, value * 0.25f, ref vel, 2f);
			}
			if ((bool)wallChecker)
			{
				checkForDamage();
			}
			if (wallCheckerTouching() && !beingPushedUp() && speed > 0f)
			{
				if (speed >= maxSpeed / 2f)
				{
					SoundManager.manage.playASoundAtPoint(SoundManager.manage.vehicleKnockBack, base.transform.position);
				}
				speed = (0f - speed) / 2f;
			}
			if (!grounded && isOnSlowDownLayer)
			{
				if (!onlyTurnIfMovingForward)
				{
					base.transform.Rotate(0f, (0f - num) * (turnSpeed / 4f) * Time.deltaTime, 0f);
				}
				else
				{
					base.transform.Rotate(0f, (0f - num) * (turnSpeed * (Mathf.Abs(speed) / maxSpeed) / 4f) * Time.deltaTime, 0f);
				}
			}
			else if (!onlyTurnIfMovingForward)
			{
				base.transform.Rotate(0f, (0f - num) * turnSpeed * Time.deltaTime, 0f);
			}
			else
			{
				base.transform.Rotate(0f, (0f - num) * (turnSpeed * (Mathf.Abs(speed) / maxSpeed)) * Time.deltaTime, 0f);
			}
			if (canFly)
			{
				if (grounded)
				{
					myVehicle.myRig.useGravity = true;
				}
				else
				{
					myVehicle.myRig.useGravity = false;
					myVehicle.myRig.velocity = Vector3.zero;
				}
				if (InputMaster.input.JumpHeld() || InputMaster.input.VehicleUseHeld())
				{
					flyingSpeed = Mathf.Lerp(flyingSpeed, 6f, Time.deltaTime);
				}
				else if (InputMaster.input.VehicleInteractHeld())
				{
					flyingSpeed = Mathf.Lerp(flyingSpeed, -6f, Time.deltaTime);
				}
				else
				{
					flyingSpeed = Mathf.Lerp(flyingSpeed, 0f, Time.deltaTime);
				}
				if (flyingSpeed < 0.05f && flyingSpeed > 0f)
				{
					flyingSpeed = 0f;
				}
				if (flyingSpeed > -0.05f && flyingSpeed < 0f)
				{
					flyingSpeed = 0f;
				}
				if (flyingSpeed > 0f)
				{
					if (myVehicle.transform.position.y < 18f)
					{
						myVehicle.myRig.MovePosition(myVehicle.myRig.position + Vector3.up * flyingSpeed * Time.fixedDeltaTime);
					}
				}
				else
				{
					myVehicle.myRig.MovePosition(myVehicle.myRig.position + Vector3.up * flyingSpeed * Time.fixedDeltaTime);
				}
			}
			else if (canJump && InputMaster.input.JumpHeld() && !inJump && grounded)
			{
				StartCoroutine(jumpFeel());
			}
		}
		else
		{
			if (wallCheckerTouching() && !beingPushedUp() && speed > 0f)
			{
				if (speed >= maxSpeed / 2f)
				{
					SoundManager.manage.playASoundAtPoint(SoundManager.manage.vehicleKnockBack, base.transform.position);
				}
				speed = (0f - speed) / 2f;
			}
			speed = Mathf.SmoothDamp(speed, 0f, ref vel, 1.5f / acceleration);
		}
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(0f, base.transform.eulerAngles.y, 0f), Time.deltaTime * 50f);
		Vector3 vector = speed * forwardDir;
		if (!myVehicle.myRig.isKinematic)
		{
			myVehicle.myRig.MovePosition(myVehicle.myRig.position + vector * Time.fixedDeltaTime);
		}
		if (beingPushedUp())
		{
			myVehicle.myRig.MovePosition(myVehicle.myRig.position + Vector3.up * 8f * Time.fixedDeltaTime);
		}
	}

	public bool wallCheckerTouching()
	{
		RaycastHit hitInfo;
		if ((bool)wallChecker && Physics.Raycast(wallChecker.position, wallChecker.forward, out hitInfo, 0.25f, wallCheckerLayers) && hitInfo.transform != myVehicle.myHitBox)
		{
			return true;
		}
		return false;
	}

	public void checkForDamage()
	{
		RaycastHit hitInfo;
		if (!damagedObjectsOnCollision || !Physics.Raycast(wallChecker.position, wallChecker.forward, out hitInfo, 0.35f, doesDamageLayer))
		{
			return;
		}
		Damageable componentInParent = hitInfo.transform.GetComponentInParent<Damageable>();
		if ((bool)componentInParent)
		{
			if (speed >= maxSpeed / 2f + maxSpeed / 4f)
			{
				CmdDoDamageWhenHit(componentInParent.netId, 3);
			}
			else
			{
				CmdDoDamageWhenHit(componentInParent.netId, 0);
			}
		}
	}

	public bool beingPushedUp()
	{
		if (WorldManager.manageWorld.isPositionOnMap(Mathf.RoundToInt((base.transform.position.x + base.transform.forward.x * 2f) / 2f), Mathf.RoundToInt((base.transform.position.z + base.transform.forward.z * 2f) / 2f)))
		{
			float num = WorldManager.manageWorld.heightMap[Mathf.RoundToInt((base.transform.position.x + base.transform.forward.x * 2f) / 2f), Mathf.RoundToInt((base.transform.position.z + base.transform.forward.z * 2f) / 2f)];
			if (!inJump && speed >= maxSpeed / 8f && base.transform.position.y < num && num - base.transform.position.y > 0f && num - base.transform.position.y < 1f)
			{
				return true;
			}
		}
		return false;
	}

	private void rotateVehicleToDir(float x, float y, float turnSpeed)
	{
		if (x != 0f || y != 0f)
		{
			Vector3 normalized = new Vector3(x, 0f, y).normalized;
			normalized = CameraController.control.transform.TransformDirection(normalized);
			if (base.transform.parent != null)
			{
				normalized = CameraController.control.transform.InverseTransformDirection(normalized);
			}
			Quaternion b = Quaternion.LookRotation(normalized);
			base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, b, Time.deltaTime * turnSpeed);
		}
	}

	public bool isGrounded()
	{
		return grounded;
	}

	public bool isGroundedOnSlowSurface()
	{
		return isOnSlowDownLayer;
	}

	private IEnumerator jumpFeel()
	{
		float desiredHeight = 0f;
		float multi = 25f;
		while (desiredHeight < jumpUpHeight)
		{
			yield return jumpWait;
			myVehicle.myRig.MovePosition(myVehicle.myRig.position + Vector3.up * desiredHeight * Time.fixedDeltaTime);
			desiredHeight = Mathf.Lerp(desiredHeight, jumpUpHeight + 1f, Time.fixedDeltaTime * multi);
			multi = Mathf.Lerp(multi, 10f, Time.deltaTime * 25f);
		}
		while (desiredHeight > 0f && !grounded)
		{
			yield return jumpWait;
			myVehicle.myRig.MovePosition(myVehicle.myRig.position + Vector3.up * desiredHeight * Time.fixedDeltaTime);
			desiredHeight = Mathf.Lerp(desiredHeight, -1f, Time.deltaTime * 2f);
		}
		while (!grounded)
		{
			yield return null;
		}
		inJump = false;
	}

	[Command]
	private void CmdDoDamageWhenHit(uint netId, int damageAmount)
	{
		PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
		writer.WriteUInt(netId);
		writer.WriteInt(damageAmount);
		SendCommandInternal(typeof(ControlVehicle), "CmdDoDamageWhenHit", writer, 0);
		NetworkWriterPool.Recycle(writer);
	}

	static ControlVehicle()
	{
		jumpWait = new WaitForFixedUpdate();
		RemoteCallHelper.RegisterCommandDelegate(typeof(ControlVehicle), "CmdDoDamageWhenHit", InvokeUserCode_CmdDoDamageWhenHit, true);
	}

	private void MirrorProcessed()
	{
	}

	protected void UserCode_CmdDoDamageWhenHit(uint netId, int damageAmount)
	{
		NetworkIdentity.spawned[netId].GetComponent<Damageable>().attackAndDoDamage(damageAmount, base.transform, 8f);
	}

	protected static void InvokeUserCode_CmdDoDamageWhenHit(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDoDamageWhenHit called on client.");
		}
		else
		{
			((ControlVehicle)obj).UserCode_CmdDoDamageWhenHit(reader.ReadUInt(), reader.ReadInt());
		}
	}
}
