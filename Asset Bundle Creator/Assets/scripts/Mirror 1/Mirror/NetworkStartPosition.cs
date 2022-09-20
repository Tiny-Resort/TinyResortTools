using UnityEngine;

namespace Mirror
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Network/NetworkStartPosition")]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-start-position")]
	public class NetworkStartPosition : MonoBehaviour
	{
		public void Awake()
		{
			NetworkManager.RegisterStartPosition(base.transform);
		}

		public void OnDestroy()
		{
			NetworkManager.UnRegisterStartPosition(base.transform);
		}
	}
}
