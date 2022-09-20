using UnityEngine;

public class MoonScript : MonoBehaviour
{
	public Transform moonObject;

	public Transform sun;

	public Material moonMat;

	public Vector2 fullMoon;

	public Vector2 noMoon;

	public Vector2[] otherDays;

	private void OnEnable()
	{
		int num = WorldManager.manageWorld.day + (WorldManager.manageWorld.week - 1) * 7;
		switch (num)
		{
		case 1:
			moonMat.SetTextureOffset("_MainTex", fullMoon);
			break;
		case 14:
			moonMat.SetTextureOffset("_MainTex", noMoon);
			break;
		default:
		{
			int num2 = Mathf.RoundToInt((float)num / 4.7f);
			moonMat.SetTextureOffset("_MainTex", otherDays[num2]);
			break;
		}
		}
	}

	private void Update()
	{
		base.transform.rotation = sun.rotation;
		moonObject.LookAt(CameraController.control.cameraTrans);
	}
}
