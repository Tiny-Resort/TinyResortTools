using UnityEngine;

public class CameraHandHeld : MonoBehaviour
{
	private CharMovement myChar;

	private Quaternion startRot;

	private bool cameraOpen;

	public void Start()
	{
		myChar = GetComponentInParent<CharMovement>();
	}

	public void openCameraMenu()
	{
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			cameraOpen = true;
			PhotoManager.manage.openCameraView();
			Inventory.inv.quickBarLocked(true);
			Cursor.lockState = CursorLockMode.Locked;
			startRot = CameraController.control.transform.rotation;
			CameraController.control.transform.rotation = NetworkMapSharer.share.localChar.transform.rotation;
		}
	}

	public void closeCameraView()
	{
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			cameraOpen = false;
			PhotoManager.manage.closeCameraView();
			Inventory.inv.quickBarLocked(false);
			Cursor.lockState = CursorLockMode.None;
			CameraController.control.transform.rotation = startRot;
		}
	}

	private void OnDisable()
	{
		if (cameraOpen)
		{
			closeCameraView();
		}
	}
}
