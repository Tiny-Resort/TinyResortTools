using System;
using UnityEngine;

namespace Mirror
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Network/NetworkPingDisplay")]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-ping-display")]
	public class NetworkPingDisplay : MonoBehaviour
	{
		public Color color = Color.white;

		public int padding = 2;

		private int width = 150;

		private int height = 25;

		private void OnGUI()
		{
			if (NetworkClient.active)
			{
				GUI.color = color;
				Rect position = new Rect(Screen.width - width - padding, Screen.height - height - padding, width, height);
				GUIStyle style = GUI.skin.GetStyle("Label");
				style.alignment = TextAnchor.MiddleRight;
				GUI.Label(position, string.Format("RTT: {0}ms", Math.Round(NetworkTime.rtt * 1000.0)), style);
				GUI.color = Color.white;
			}
		}
	}
}
