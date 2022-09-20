using UnityEngine;

public class camerFlys : MonoBehaviour
{
	public float HorizontalVerticalSensitivity = 0.1f;

	public float UpDownSensitivity = 0.015f;

	public bool InvertedY;

	public float SpeedMultiplyerOnShiftPressed = 4f;

	private float slowSpeedMax = 1f;

	private float moveUpDown;

	private float speedMultiplyer = 1f;

	private float inversion = -1f;

	public bool charFollows = true;

	private void Start()
	{
		if (InvertedY)
		{
			inversion = 1f;
		}
		else
		{
			inversion = -1f;
		}
	}

	private void Update()
	{
		if (Inventory.inv.usingMouse && !Input.GetKey(KeyCode.Mouse1))
		{
			return;
		}
		float x = InputMaster.input.getRightStick().x;
		float y = InputMaster.input.getRightStick().y;
		float x2 = InputMaster.input.getMousePosOld().x;
		float y2 = InputMaster.input.getMousePosOld().y;
		speedMultiplyer = Mathf.Lerp(speedMultiplyer, slowSpeedMax, 0.1f);
		if (!Inventory.inv.usingMouse)
		{
			x = InputMaster.input.getLeftStick().x;
			y = InputMaster.input.getLeftStick().y;
			x2 = InputMaster.input.getRightStick().x;
			y2 = InputMaster.input.getRightStick().y;
			if (InputMaster.input.VehicleAccelerate() > 0.5f)
			{
				speedMultiplyer += 0.2f;
				speedMultiplyer = Mathf.Clamp(speedMultiplyer, 1f, SpeedMultiplyerOnShiftPressed);
			}
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.P))
			{
				if (slowSpeedMax.Equals(1f))
				{
					slowSpeedMax = 0.45f;
				}
				else
				{
					slowSpeedMax = 1f;
				}
			}
			if (Input.GetKey(KeyCode.RightShift))
			{
				speedMultiplyer += 0.2f;
				speedMultiplyer = Mathf.Clamp(speedMultiplyer, 1f, SpeedMultiplyerOnShiftPressed);
			}
		}
		if (charFollows)
		{
			NetworkMapSharer.share.localChar.transform.position = base.transform.position - base.transform.forward * 2f;
			NetworkMapSharer.share.localChar.transform.rotation = base.transform.rotation;
		}
		moveUpDown = Mathf.Lerp(moveUpDown, 0f, 0.1f);
		if (Input.GetKey(KeyCode.N))
		{
			moveUpDown += UpDownSensitivity;
		}
		if (Input.GetKey(KeyCode.Q))
		{
			moveUpDown -= UpDownSensitivity;
		}
		base.transform.Rotate(Vector3.up * x2, Space.World);
		base.transform.Rotate(base.transform.right * -1f * y2, Space.World);
		base.transform.position += base.transform.right * x * HorizontalVerticalSensitivity * speedMultiplyer;
		base.transform.position += base.transform.forward * y * HorizontalVerticalSensitivity * speedMultiplyer;
		base.transform.position += base.transform.up * moveUpDown * speedMultiplyer;
	}
}
