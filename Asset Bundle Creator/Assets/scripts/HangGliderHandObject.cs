using System.Collections;
using UnityEngine;

public class HangGliderHandObject : MonoBehaviour
{
	private Rigidbody charRig;

	private CharMovement myMove;

	private CharNetworkAnimator networkAnimator;

	private bool givenJumpIncrease;

	public ASound openSound;

	private int speedDif = 2;

	private int jumpDif = 4;

	private float swimDif = 1.25f;

	public bool isBoogieBoard;

	public Transform boardTransform;

	public Animator gliderAnimator;

	public float fallSpeedSlowdown = 1.5f;

	public AudioSource flyingAudio;

	public ParticleSystem[] parts;

	private float boardLocalZ;

	private WaitForSeconds staminaWait = new WaitForSeconds(0.25f);

	private void Start()
	{
		charRig = base.transform.GetComponentInParent<Rigidbody>();
		myMove = base.transform.GetComponentInParent<CharMovement>();
		networkAnimator = base.transform.GetComponentInParent<CharNetworkAnimator>();
		if (isBoogieBoard && (bool)myMove)
		{
			boardLocalZ = boardTransform.localPosition.z;
			myMove.addOrRemoveSwimDif(swimDif);
			myMove.usingBoogieBoard = true;
			if ((bool)myMove)
			{
				StartCoroutine(keepBoardFlatInWater());
			}
		}
		else
		{
			getDaysDif();
			if ((bool)myMove && myMove.isLocalPlayer)
			{
				StartCoroutine(useStamina());
			}
		}
		base.transform.localRotation = Quaternion.Euler(4f, 4f, 0f);
	}

	private void FixedUpdate()
	{
		if (!isBoogieBoard && (bool)charRig)
		{
			if (myMove.myEquip.usingItem)
			{
				myMove.usingHangGlider = true;
				charRig.velocity = new Vector3(charRig.velocity.x, charRig.velocity.y / fallSpeedSlowdown, charRig.velocity.z);
				if (!myMove.grounded)
				{
					charRig.MovePosition(charRig.position + myMove.transform.forward * speedDif * Time.fixedDeltaTime);
				}
			}
			else
			{
				myMove.usingHangGlider = false;
			}
		}
		if (!gliderAnimator || !myMove)
		{
			return;
		}
		gliderAnimator.SetBool("Using", myMove.myEquip.usingItem);
		if (myMove.myEquip.usingItem)
		{
			emitParts();
		}
		if (myMove.isLocalPlayer)
		{
			gliderAnimator.SetBool("Grounded", myMove.grounded);
		}
		else
		{
			gliderAnimator.SetBool("Grounded", networkAnimator.grounded);
		}
		if ((bool)flyingAudio && myMove.myEquip.usingItem && !gliderAnimator.GetBool("Grounded"))
		{
			if ((bool)flyingAudio)
			{
				if (!flyingAudio.isPlaying)
				{
					flyingAudio.Play();
				}
				flyingAudio.volume = 0.35f * SoundManager.manage.getSoundVolume();
			}
		}
		else if ((bool)flyingAudio && flyingAudio.isPlaying)
		{
			if (flyingAudio.volume > 0f)
			{
				flyingAudio.volume -= 0.05f;
			}
			else
			{
				flyingAudio.Pause();
			}
		}
	}

	public void OnDestroy()
	{
		if (isBoogieBoard)
		{
			if ((bool)myMove)
			{
				myMove.addOrRemoveSwimDif(0f - swimDif);
				myMove.usingBoogieBoard = false;
			}
			return;
		}
		if ((bool)myMove && givenJumpIncrease)
		{
			myMove.addOrRemoveJumpDif(-jumpDif);
		}
		if ((bool)myMove)
		{
			myMove.usingHangGlider = false;
		}
	}

	public void playOpenSound()
	{
		SoundManager.manage.playASoundAtPoint(openSound, base.transform.position);
	}

	public void getDaysDif()
	{
		if ((bool)myMove && givenJumpIncrease)
		{
			myMove.addOrRemoveJumpDif(-jumpDif);
			givenJumpIncrease = false;
		}
		if (WeatherManager.manage.windy)
		{
			speedDif = 3;
			jumpDif = 4;
		}
		else
		{
			speedDif = 2;
			jumpDif = 2;
		}
		if ((bool)myMove)
		{
			myMove.addOrRemoveJumpDif(jumpDif);
			givenJumpIncrease = true;
		}
	}

	private IEnumerator useStamina()
	{
		while (true)
		{
			yield return null;
			if (myMove.myEquip.usingItem && !myMove.grounded)
			{
				StatusManager.manage.changeStamina(-0.1f);
			}
			yield return staminaWait;
		}
	}

	private IEnumerator keepBoardFlatInWater()
	{
		while (true)
		{
			if (myMove.myAnim.GetBool("Swimming"))
			{
				while (myMove.myAnim.GetBool("Swimming") && !myMove.underWater)
				{
					boardTransform.rotation = Quaternion.Euler(0f, myMove.transform.eulerAngles.y, 0f);
					boardTransform.localPosition = Vector3.Lerp(boardTransform.localPosition, new Vector3(boardTransform.localPosition.x, boardTransform.localPosition.y, 0f + myMove.myAnim.GetFloat("WalkSpeed") / 2.3f), Time.deltaTime * 5f);
					boardTransform.position = new Vector3(boardTransform.position.x, 0.6f, boardTransform.position.z);
					yield return null;
				}
				boardTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
				boardTransform.localPosition = Vector3.zero;
				while (myMove.myAnim.GetBool("Swimming") && myMove.underWater)
				{
					boardTransform.rotation = Quaternion.Euler(0f, myMove.transform.eulerAngles.y, 0f);
					yield return null;
				}
				boardTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
				boardTransform.localPosition = Vector3.zero;
			}
			yield return null;
		}
	}

	public void emitParts()
	{
		for (int i = 0; i < parts.Length; i++)
		{
			parts[i].Emit(1);
		}
	}
}
