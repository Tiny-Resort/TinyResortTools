using System;
using UnityEngine.Events;

namespace Mirror.Discovery
{
	[Serializable]
	public class ServerFoundUnityEvent : UnityEvent<ServerResponse>
	{
	}
}
