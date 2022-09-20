using System.Collections;
using UnityEngine;

public class RangedWeapon : MonoBehaviour
{
	private EquipItemToChar attachedChar;

	private CharMovement connectedChar;

	public InventoryItem useAsAmmo;

	public Transform lookable;

	public Transform faceCam;

	public int projectileId;

	public float projectileDelay = 0.5f;

	public float holdToFireAgainDelay = 0.5f;

	public float strengthMax = 5f;

	public AudioSource stretchSound;

	private float stretchSoundMax = 0.1f;

	public ASound fireSound;

	private int ammoId;

	private bool lookingDownSights;

	public LayerMask canHitLayer;

	private void OnEnable()
	{
		Crossheir.cross.crossheirIcon.enabled = true;
		ammoId = Inventory.inv.getInvItemId(useAsAmmo);
		attachedChar = base.transform.GetComponentInParent<EquipItemToChar>();
		connectedChar = base.transform.GetComponentInParent<CharMovement>();
		if ((bool)attachedChar)
		{
			StartCoroutine(aimDownSights());
			StartCoroutine(attachLeftHand());
		}
	}

	private IEnumerator aimDownSights()
	{
		updateAmmoCounter();
		if (attachedChar.isLocalPlayer)
		{
			StartCoroutine(updateLookableLocation());
		}
		if (attachedChar.isLocalPlayer)
		{
			StartCoroutine(lookDownSights());
		}
		while (true)
		{
			if (attachedChar.usingItem)
			{
				float strength = 1f;
				stretchSound.volume = 0f;
				stretchSound.pitch = 1f + Random.Range(-0.1f, 0.1f);
				stretchSound.Play();
				while (attachedChar.usingItem)
				{
					strength = Mathf.Clamp(strength + Time.deltaTime * 3f, 1f, strengthMax);
					if (attachedChar.isLocalPlayer)
					{
						Crossheir.cross.setPower(strength, strengthMax);
					}
					if (strength < strengthMax)
					{
						stretchSound.volume = Mathf.Clamp(stretchSound.volume + Time.deltaTime, 0f, stretchSoundMax * SoundManager.manage.getSoundVolume());
						stretchSound.pitch = Mathf.Clamp(stretchSound.pitch + Time.deltaTime, 0f, strengthMax / 2f);
					}
					else
					{
						stretchSound.volume = Mathf.Clamp(stretchSound.volume - Time.deltaTime * 5f, 0f, stretchSoundMax * SoundManager.manage.getSoundVolume());
					}
					yield return null;
				}
				if (attachedChar.isLocalPlayer)
				{
					Crossheir.cross.fadeOut();
				}
				stretchSound.Pause();
				yield return new WaitForSeconds(projectileDelay);
				if ((bool)fireSound)
				{
					SoundManager.manage.playASoundAtPoint(fireSound, base.transform.position);
				}
				if (attachedChar.isLocalPlayer)
				{
					if (canFire())
					{
						Vector3 direction = CameraController.control.cameraTrans.position + CameraController.control.cameraTrans.forward * (strength * 5f) - base.transform.position;
						RaycastHit hitInfo;
						if (Physics.Raycast(CameraController.control.cameraTrans.position, CameraController.control.cameraTrans.forward, out hitInfo, 25f, canHitLayer))
						{
							direction = hitInfo.point - base.transform.position;
						}
						direction.Normalize();
						attachedChar.CmdFireProjectileAtDir(attachedChar.holdPos.position + attachedChar.holdPos.forward, direction, strength, projectileId);
						consumeAmmo();
						Inventory.inv.useItemWithFuel();
					}
					updateAmmoCounter();
				}
				yield return new WaitForSeconds(holdToFireAgainDelay);
			}
			if (attachedChar.isLocalPlayer && !attachedChar.usingItem)
			{
				while (!attachedChar.usingItem)
				{
					yield return null;
				}
			}
			yield return null;
		}
	}

	private IEnumerator updateLookableLocation()
	{
		while (true)
		{
			if (attachedChar.usingItem && lookingDownSights)
			{
				faceCam.LookAt(CameraController.control.Camera_Y.position + CameraController.control.Camera_Y.forward * 5f);
				lookable.transform.position = faceCam.position + faceCam.forward * 2f;
			}
			else
			{
				lookable.transform.localPosition = Vector3.zero + Vector3.up;
			}
			yield return null;
		}
	}

	public void exitSights()
	{
		CameraController.control.exitAimCamera();
		connectedChar.lockRotation(false);
		lookingDownSights = false;
		Cursor.lockState = CursorLockMode.Confined;
		Crossheir.cross.turnOffCrossheir();
	}

	private IEnumerator lookDownSights()
	{
		while (true)
		{
			if (Inventory.inv.canMoveChar() && InputMaster.input.Use())
			{
				bool lookingDownSights = true;
				CameraController.control.enterAimCamera();
				connectedChar.lockRotation(true);
				Crossheir.cross.turnOnCrossheir();
				while (lookingDownSights)
				{
					Cursor.lockState = CursorLockMode.Locked;
					if (!Inventory.inv.canMoveChar())
					{
						lookingDownSights = false;
						exitSights();
					}
					if (!InputMaster.input.Use() && (InputMaster.input.UICancel() || (Inventory.inv.usingMouse && InputMaster.input.Interact())))
					{
						lookingDownSights = false;
						exitSights();
					}
					yield return null;
				}
			}
			yield return null;
		}
	}

	private void OnDisable()
	{
		if ((bool)attachedChar && attachedChar.isLocalPlayer)
		{
			CameraController.control.exitAimCamera();
			Crossheir.cross.turnOffCrossheir();
		}
	}

	public void updateAmmoCounter()
	{
		Crossheir.cross.setAmmo(useAsAmmo.itemSprite, Inventory.inv.getAmountOfItemInAllSlots(ammoId));
	}

	public void consumeAmmo()
	{
		Inventory.inv.removeAmountOfItem(ammoId, 1);
	}

	public bool canFire()
	{
		return Inventory.inv.getAmountOfItemInAllSlots(ammoId) > 0;
	}

	private IEnumerator attachLeftHand()
	{
		while (true)
		{
			if (attachedChar.usingItem)
			{
				attachedChar.attachLeftHand();
			}
			else
			{
				attachedChar.removeLeftHand();
			}
			yield return null;
		}
	}
}
