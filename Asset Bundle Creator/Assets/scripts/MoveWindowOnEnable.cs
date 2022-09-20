using UnityEngine;

public class MoveWindowOnEnable : MonoBehaviour
{
	public RectTransform otherWindow;

	public Vector3 moveToPos;

	private Vector3 posToReturnTo;

	private void OnEnable()
	{
		posToReturnTo = otherWindow.GetComponent<WindowAnimator>().moveWindowAndReturnOriginalPos(moveToPos);
	}

	private void OnDisable()
	{
		otherWindow.GetComponent<WindowAnimator>().moveWindowAndReturnOriginalPos(posToReturnTo);
	}
}
