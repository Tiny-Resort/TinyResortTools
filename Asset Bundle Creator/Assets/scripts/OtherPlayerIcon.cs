using TMPro;
using UnityEngine;

public class OtherPlayerIcon : MonoBehaviour
{
	public TextMeshProUGUI nameTag;

	public void setName(string playerName)
	{
		nameTag.text = playerName;
	}
}
