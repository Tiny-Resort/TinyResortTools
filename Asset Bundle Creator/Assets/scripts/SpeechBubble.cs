using System.Collections;
using TMPro;
using UnityEngine;

public class SpeechBubble : MonoBehaviour
{
	public TextMeshProUGUI bubbleText;

	public Transform characterSaying;

	private Renderer charRen;

	public void setUpBubble(string setBubbleText, Transform followChar)
	{
		bubbleText.text = setBubbleText;
		characterSaying = followChar;
		StartCoroutine(moveChatBubble());
	}

	private IEnumerator moveChatBubble()
	{
		moveInstantly();
		charRen = characterSaying.GetComponentInChildren<Renderer>();
		while (true)
		{
			yield return null;
			if (!isGameObjectVisible())
			{
				base.transform.position = new Vector3(-500f, -500f, 0f);
				while (!isGameObjectVisible())
				{
					yield return null;
				}
				moveInstantly();
			}
			moveAndScale();
		}
	}

	public void moveAndScale()
	{
		base.transform.position = Vector3.Lerp(base.transform.position, CameraController.control.mainCamera.WorldToScreenPoint(characterSaying.position + Vector3.up * 2f + CameraController.control.mainCamera.transform.right), Time.deltaTime * 25f);
		float value = Vector3.Distance(CameraController.control.transform.position, characterSaying.transform.position);
		value = Mathf.Clamp(value, 8f, 32f);
		float num = 1f / (value / 8f);
		base.transform.localScale = new Vector3(num, num, num);
	}

	public void moveInstantly()
	{
		base.transform.position = CameraController.control.mainCamera.WorldToScreenPoint(characterSaying.position + Vector3.up * 2f + CameraController.control.mainCamera.transform.right);
		float value = Vector3.Distance(CameraController.control.transform.position, characterSaying.transform.position);
		value = Mathf.Clamp(value, 8f, 32f);
		float num = 1f / (value / 8f);
		base.transform.localScale = new Vector3(num, num, num);
	}

	public bool isGameObjectVisible()
	{
		if (Vector3.Distance(characterSaying.position, CameraController.control.transform.position) > 35f)
		{
			return false;
		}
		if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(CameraController.control.mainCamera), charRen.bounds))
		{
			return true;
		}
		return false;
	}
}
