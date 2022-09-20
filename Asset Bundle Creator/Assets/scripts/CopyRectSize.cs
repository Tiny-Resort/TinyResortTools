using UnityEngine;

public class CopyRectSize : MonoBehaviour
{
	public RectTransform copyRect;

	private RectTransform myRect;

	private void Start()
	{
		myRect = GetComponent<RectTransform>();
	}

	private void Update()
	{
		myRect.position = copyRect.position;
		myRect.sizeDelta = copyRect.sizeDelta;
	}
}
