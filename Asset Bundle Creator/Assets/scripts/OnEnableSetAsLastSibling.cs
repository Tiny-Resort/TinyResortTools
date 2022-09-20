using UnityEngine;

public class OnEnableSetAsLastSibling : MonoBehaviour
{
	private void OnEnable()
	{
		base.transform.SetAsLastSibling();
	}

	public void onPress()
	{
		base.transform.SetAsLastSibling();
	}
}
