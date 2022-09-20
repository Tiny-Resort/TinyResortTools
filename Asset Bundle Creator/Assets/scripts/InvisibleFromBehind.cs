using System.Collections;
using UnityEngine;

public class InvisibleFromBehind : MonoBehaviour
{
	public GameObject[] gameObjectsToHide;

	private void OnEnable()
	{
		for (int i = 0; i < gameObjectsToHide.Length; i++)
		{
			gameObjectsToHide[i].SetActive(true);
		}
		StartCoroutine(hideWhenCamClose());
	}

	private IEnumerator hideWhenCamClose()
	{
		while (true)
		{
			yield return null;
			float num = Vector3.Dot((CameraController.control.cameraTrans.position - base.transform.position).normalized, -base.transform.forward);
			if (num > 0f)
			{
				for (int i = 0; i < gameObjectsToHide.Length; i++)
				{
					gameObjectsToHide[i].SetActive(false);
				}
				while (num > 0f)
				{
					yield return null;
					num = Vector3.Dot((CameraController.control.cameraTrans.position - base.transform.position).normalized, -base.transform.forward);
				}
				for (int j = 0; j < gameObjectsToHide.Length; j++)
				{
					gameObjectsToHide[j].SetActive(true);
				}
			}
		}
	}
}
