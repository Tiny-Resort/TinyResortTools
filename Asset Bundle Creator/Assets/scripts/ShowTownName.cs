using TMPro;
using UnityEngine;

public class ShowTownName : MonoBehaviour
{
	private void OnEnable()
	{
		GetComponent<TextMeshPro>().text = NetworkMapSharer.share.islandName;
	}
}
