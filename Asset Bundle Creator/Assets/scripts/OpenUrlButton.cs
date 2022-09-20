using UnityEngine;

public class OpenUrlButton : MonoBehaviour
{
	public string URL;

	public void onPresOpenURL()
	{
		Application.OpenURL(URL);
	}
}
