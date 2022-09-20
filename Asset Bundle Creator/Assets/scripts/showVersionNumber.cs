using TMPro;
using UnityEngine;

public class showVersionNumber : MonoBehaviour
{
	public TextMeshProUGUI showVersionNumberTextBox;

	private void OnEnable()
	{
		showVersionNumberTextBox.text = "v.0." + WorldManager.manageWorld.masterVersionNumber + "." + WorldManager.manageWorld.versionNumber;
	}
}
