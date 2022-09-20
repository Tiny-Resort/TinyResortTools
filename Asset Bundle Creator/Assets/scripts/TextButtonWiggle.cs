using TMPro.Examples;
using UnityEngine;

public class TextButtonWiggle : MonoBehaviour
{
	public VertexJitter myJitter;

	public InvButton mybutton;

	private void Update()
	{
		if (mybutton.hovering)
		{
			if (!myJitter.enabled)
			{
				myJitter.enabled = true;
			}
		}
		else if (!mybutton.hovering && myJitter.enabled)
		{
			myJitter.enabled = false;
		}
	}
}
