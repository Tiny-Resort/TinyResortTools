using UnityEngine;
using UnityStandardAssets.CinematicEffects;

public class blurOnOpen : MonoBehaviour
{
	public DepthOfField blurOnMenu;

	private void OnEnable()
	{
		blurOnMenu.enabled = true;
	}

	private void OnDisable()
	{
		blurOnMenu.enabled = false;
	}
}
