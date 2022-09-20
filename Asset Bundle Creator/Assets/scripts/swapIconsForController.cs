using UnityEngine;
using UnityEngine.UI;

public class swapIconsForController : MonoBehaviour
{
	public Sprite controllerSprite;

	public Sprite keyboardSprite;

	public Image toBeReplaced;

	private void Start()
	{
		setCorrectSprite();
		Inventory.inv.changeControlsEvent.AddListener(setCorrectSprite);
	}

	private void OnEnable()
	{
		setCorrectSprite();
	}

	public void setCorrectSprite()
	{
		if (Inventory.inv.usingMouse)
		{
			toBeReplaced.sprite = keyboardSprite;
		}
		else
		{
			toBeReplaced.sprite = controllerSprite;
		}
	}
}
