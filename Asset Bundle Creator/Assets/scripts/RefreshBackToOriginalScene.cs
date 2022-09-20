using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RefreshBackToOriginalScene : MonoBehaviour
{
	private IEnumerator Start()
	{
		yield return null;
		SceneManager.LoadScene(0);
	}
}
