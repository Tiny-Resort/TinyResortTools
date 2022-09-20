using System.Collections;
using UnityEngine;

public class deletePlayerPrefsOnPress : MonoBehaviour
{
	private void OnEnable()
	{
		StartCoroutine(listenForButton());
	}

	private IEnumerator listenForButton()
	{
		while (true)
		{
			yield return null;
			if (Input.GetKeyDown(KeyCode.F12))
			{
				PlayerPrefs.DeleteAll();
			}
		}
	}
}
