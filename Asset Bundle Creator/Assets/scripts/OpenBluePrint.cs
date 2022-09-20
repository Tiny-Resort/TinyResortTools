using UnityEngine;

public class OpenBluePrint : MonoBehaviour
{
	private CharMovement myChar;

	private void Start()
	{
		myChar = GetComponentInParent<CharMovement>();
	}

	public void openBluePrintMenu()
	{
		if ((bool)myChar && myChar.isLocalPlayer)
		{
			CharLevelManager.manage.openUnlockScreen();
		}
	}
}
