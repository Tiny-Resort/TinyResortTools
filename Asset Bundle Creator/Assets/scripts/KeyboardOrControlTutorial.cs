using UnityEngine;

public class KeyboardOrControlTutorial : MonoBehaviour
{
	public GameObject keyboard;

	public GameObject controller;

	private void OnEnable()
	{
		if (Inventory.inv.usingMouse)
		{
			keyboard.SetActive(true);
			controller.SetActive(false);
		}
		else
		{
			keyboard.SetActive(false);
			controller.SetActive(true);
		}
	}
}
