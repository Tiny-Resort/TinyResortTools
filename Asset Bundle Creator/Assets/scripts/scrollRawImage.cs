using UnityEngine;
using UnityEngine.UI;

public class scrollRawImage : MonoBehaviour
{
	public RawImage myimage;

	public InvButton button;

	private Rect animatedRec;

	public float speed = 0.001f;

	public Image matchColor;

	public bool vertToo;

	public bool changeWithWindowSize;

	private void Awake()
	{
		animatedRec = myimage.uvRect;
	}

	private void Update()
	{
		if (!button || button.hovering)
		{
			animatedRec.x += speed;
			if (vertToo)
			{
				animatedRec.y += speed;
			}
			myimage.uvRect = animatedRec;
		}
		if ((bool)matchColor && matchColor.color != myimage.color)
		{
			myimage.color = matchColor.color;
		}
	}

	private void OnEnable()
	{
		if (changeWithWindowSize)
		{
			animatedRec.height = animatedRec.width / Camera.main.aspect;
		}
	}

	public void setColour(Color newColor)
	{
		myimage.color = newColor;
	}
}
