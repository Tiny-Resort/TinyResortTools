using UnityEngine;

public class HideMenuButtonsWhenNoFile : MonoBehaviour
{
	public GameObject loadButton;

	public GameObject loadButtonDummy;

	public GameObject multiplayerErrorMessage;

	private void OnEnable()
	{
		if (SaveLoad.saveOrLoad.isASaveSlot())
		{
			loadButton.SetActive(true);
			multiplayerErrorMessage.SetActive(false);
			loadButtonDummy.SetActive(false);
		}
		else
		{
			loadButton.SetActive(false);
			multiplayerErrorMessage.SetActive(true);
			loadButtonDummy.SetActive(true);
		}
	}
}
