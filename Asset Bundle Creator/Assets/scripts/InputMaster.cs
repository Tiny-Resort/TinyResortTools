using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputMaster : MonoBehaviour
{
	public static InputMaster input;

	private ControllerInputActions controls;

	private Coroutine currentRubleRoutine;

	private void Awake()
	{
		input = this;
		controls = new ControllerInputActions();
		controls.Enable();
		controls.UI.Enable();
		stopRumble();
	}

	public Vector3 getMousePos()
	{
		return controls.UI.Point.ReadValue<Vector2>();
	}

	public bool Other()
	{
		return controls.Controls.Other.triggered;
	}

	public bool OtherKeyboard()
	{
		if (controls.Controls.OtherKeyboard.phase == InputActionPhase.Started)
		{
			return UISelect();
		}
		return false;
	}

	public bool Interact()
	{
		return controls.Controls.Interact.triggered;
	}

	public bool InteractHeld()
	{
		return controls.Controls.Interact.phase == InputActionPhase.Started;
	}

	public bool Jump()
	{
		return controls.Controls.Jump.triggered;
	}

	public bool JumpHeld()
	{
		return controls.Controls.Jump.phase == InputActionPhase.Started;
	}

	public bool Use()
	{
		return controls.Controls.Use.triggered;
	}

	public bool UseHeld()
	{
		return controls.Controls.Use.phase == InputActionPhase.Started;
	}

	public bool UISelect()
	{
		if (controls.UI.Select.triggered)
		{
			return true;
		}
		if (Inventory.inv.usingMouse)
		{
			return Input.GetMouseButtonDown(0);
		}
		return false;
	}

	public bool UISelectHeld()
	{
		return controls.UI.Select.phase == InputActionPhase.Started;
	}

	public bool UICancel()
	{
		return controls.UI.Cancel.triggered;
	}

	public bool UICancelHeld()
	{
		return controls.UI.Cancel.phase == InputActionPhase.Started;
	}

	public bool UIAlt()
	{
		return controls.UI.UIAlt.triggered;
	}

	public bool UIAltHeld()
	{
		return controls.UI.UIAlt.phase == InputActionPhase.Started;
	}

	public Vector2 UINavigation()
	{
		return controls.UI.Navigate.ReadValue<Vector2>();
	}

	public Vector2 getLeftStick()
	{
		return controls.Controls.Move.ReadValue<Vector2>();
	}

	public Vector2 getRightStick()
	{
		return controls.Controls.Look.ReadValue<Vector2>();
	}

	public bool drop()
	{
		return controls.Controls.DropItem.triggered;
	}

	public float getScrollWheel()
	{
		return controls.UI.ScrollWheel.ReadValue<Vector2>().y;
	}

	public bool UISelectActiveConfirmButton()
	{
		return controls.UI.SelectActiveConfirmButton.triggered;
	}

	public bool RB()
	{
		return controls.Controls.RB.triggered;
	}

	public bool RBKeyBoard()
	{
		return controls.Controls.RBKeyBoard.triggered;
	}

	public bool LBKeyBoard()
	{
		return controls.Controls.LBKeyBoard.triggered;
	}

	public bool LB()
	{
		return controls.Controls.LB.triggered;
	}

	public bool RBHeld()
	{
		return controls.Controls.RB.phase == InputActionPhase.Started;
	}

	public bool LBHeld()
	{
		return controls.Controls.LB.phase == InputActionPhase.Started;
	}

	public bool OpenInventory()
	{
		return controls.Controls.Inventory.triggered;
	}

	public bool Journal()
	{
		return controls.Controls.Journal.triggered;
	}

	public bool SwapCamera()
	{
		return controls.Controls.SwapCamera.triggered;
	}

	public bool TriggerLook()
	{
		return controls.Controls.TriggerLook.triggered;
	}

	public bool TriggerLookHeld()
	{
		return controls.Controls.TriggerLook.phase == InputActionPhase.Started;
	}

	public bool ChangeToKeyboard()
	{
		if (!controls.Controls.SwapToKeyboard.triggered)
		{
			return Input.GetMouseButtonDown(0);
		}
		return controls.Controls.SwapToKeyboard.triggered;
	}

	public bool ChangeToController()
	{
		return controls.Controls.SwapToController.triggered;
	}

	public bool OpenMap()
	{
		return controls.UI.OpenMap.triggered;
	}

	public bool OpenChat()
	{
		return controls.UI.OpenChat.triggered;
	}

	public bool KeyboardUpperCase()
	{
		return controls.UI.KeyboardUpperCase.triggered;
	}

	public float VehicleAccelerate()
	{
		return controls.Controls.VehicleAccelerate.ReadValue<float>();
	}

	public bool VehicleUse()
	{
		return controls.Controls.VehicleUse.triggered;
	}

	public bool VehicleUseHeld()
	{
		return controls.Controls.VehicleUse.phase == InputActionPhase.Started;
	}

	public bool VehicleInteract()
	{
		return controls.Controls.VehicleInteract.triggered;
	}

	public bool VehicleInteractHeld()
	{
		return controls.Controls.VehicleInteract.phase == InputActionPhase.Started;
	}

	public bool invSlotNumberPressed()
	{
		return controls.Controls.NumKeys.phase == InputActionPhase.Started;
	}

	public int getInvSlotNumber()
	{
		return (int)controls.Controls.NumKeys.ReadValue<float>() - 1;
	}

	public Vector2 getMousePosOld()
	{
		return new Vector2(Input.GetAxis("X_Mouse"), Input.GetAxis("Y_Mouse"));
	}

	public void doRumble(float rumbleMax, float fadeSpeed = 3f)
	{
		if (OptionsMenu.options.rumbleOn && !Inventory.inv.usingMouse)
		{
			if (currentRubleRoutine != null)
			{
				Gamepad.current.SetMotorSpeeds(0f, 0f);
				StopCoroutine(currentRubleRoutine);
			}
			currentRubleRoutine = StartCoroutine(smallRumbleRoutine(rumbleMax, fadeSpeed));
		}
	}

	public void connectRumbleToVehicle(VehicleMakeParticles connected)
	{
		if (OptionsMenu.options.rumbleOn && !Inventory.inv.usingMouse)
		{
			if (currentRubleRoutine != null)
			{
				Gamepad.current.SetMotorSpeeds(0f, 0f);
				StopCoroutine(currentRubleRoutine);
			}
			currentRubleRoutine = StartCoroutine(vehicleRumble(connected));
		}
	}

	public void stopRumble()
	{
		if (Gamepad.current != null)
		{
			Gamepad.current.SetMotorSpeeds(0f, 0f);
		}
	}

	private IEnumerator smallRumbleRoutine(float currentRumble, float fadeSpeed)
	{
		float timer = 0f;
		while (timer < 1f)
		{
			currentRumble = Mathf.Lerp(currentRumble, 0f, timer);
			Gamepad.current.SetMotorSpeeds(currentRumble, currentRumble);
			timer += Time.deltaTime * fadeSpeed;
			yield return null;
		}
		Gamepad.current.SetMotorSpeeds(0f, 0f);
		currentRubleRoutine = null;
	}

	private IEnumerator vehicleRumble(VehicleMakeParticles vehicle)
	{
		if (!vehicle.hasRumble)
		{
			yield break;
		}
		if ((bool)vehicle)
		{
			while (NetworkMapSharer.share.localChar.myPickUp.drivingVehicle)
			{
				yield return true;
				if (Inventory.inv.usingMouse)
				{
					if (Gamepad.current != null)
					{
						Gamepad.current.SetMotorSpeeds(0f, 0f);
					}
					continue;
				}
				float num = Mathf.Clamp(VehicleAccelerate() / 20f, 0.01f, 0.05f);
				if (!vehicle.getGrounded())
				{
					num /= 2f;
				}
				Gamepad.current.SetMotorSpeeds(num, num);
			}
		}
		Gamepad.current.SetMotorSpeeds(0f, 0f);
		currentRubleRoutine = null;
	}

	public void harvestRumble()
	{
		doRumble(0.05f, 4f);
	}
}
