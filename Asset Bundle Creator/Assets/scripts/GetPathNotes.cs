using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class GetPathNotes : MonoBehaviour
{
	public TextMeshProUGUI textBox;

	private void Start()
	{
		StartCoroutine(GetText());
	}

	private IEnumerator GetText()
	{
		UnityWebRequest www = UnityWebRequest.Get("https://www.playdinkum.com/patchnotes");
		yield return www.SendWebRequest();
		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
			yield break;
		}
		Debug.Log(www.downloadHandler.text);
		textBox.text = www.downloadHandler.text.Substring(www.downloadHandler.text.IndexOf("Patch"));
		byte[] datum = www.downloadHandler.data;
	}
}
