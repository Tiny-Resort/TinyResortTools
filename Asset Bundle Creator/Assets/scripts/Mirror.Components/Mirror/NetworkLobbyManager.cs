using System;
using UnityEngine;

namespace Mirror
{
	[AddComponentMenu("Network/NetworkLobbyManager")]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-room-manager")]
	[Obsolete("Use / inherit from NetworkRoomManager instead")]
	public class NetworkLobbyManager : NetworkRoomManager
	{
	}
}
