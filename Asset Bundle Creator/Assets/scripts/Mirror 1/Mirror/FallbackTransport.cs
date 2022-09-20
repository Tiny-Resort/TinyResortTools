using System;
using UnityEngine;

namespace Mirror
{
	[HelpURL("https://mirror-networking.gitbook.io/docs/transports/fallback-transport")]
	[DisallowMultipleComponent]
	[Obsolete("Fallback Transport will be retired. It was only needed for Apathy/Libuv. Use kcp or Telepathy instead, those run everywhere.")]
	public class FallbackTransport : Transport
	{
		public Transport[] transports;

		private Transport available;

		public void Awake()
		{
			if (transports == null || transports.Length == 0)
			{
				throw new Exception("FallbackTransport requires at least 1 underlying transport");
			}
			available = GetAvailableTransport();
			Type type = available.GetType();
			Debug.Log("FallbackTransport available: " + (((object)type != null) ? type.ToString() : null));
		}

		private void OnEnable()
		{
			available.enabled = true;
		}

		private void OnDisable()
		{
			available.enabled = false;
		}

		private Transport GetAvailableTransport()
		{
			Transport[] array = transports;
			foreach (Transport transport in array)
			{
				if (transport.Available())
				{
					return transport;
				}
			}
			throw new Exception("No transport suitable for this platform");
		}

		public override bool Available()
		{
			return available.Available();
		}

		public override void ClientConnect(string address)
		{
			available.OnClientConnected = OnClientConnected;
			available.OnClientDataReceived = OnClientDataReceived;
			available.OnClientError = OnClientError;
			available.OnClientDisconnected = OnClientDisconnected;
			available.ClientConnect(address);
		}

		public override void ClientConnect(Uri uri)
		{
			Transport[] array = transports;
			foreach (Transport transport in array)
			{
				if (transport.Available())
				{
					try
					{
						transport.ClientConnect(uri);
						available = transport;
					}
					catch (ArgumentException)
					{
					}
				}
			}
			throw new Exception("No transport suitable for this platform");
		}

		public override bool ClientConnected()
		{
			return available.ClientConnected();
		}

		public override void ClientDisconnect()
		{
			available.ClientDisconnect();
		}

		public override void ClientSend(ArraySegment<byte> segment, int channelId)
		{
			available.ClientSend(segment, channelId);
		}

		public override Uri ServerUri()
		{
			return available.ServerUri();
		}

		public override bool ServerActive()
		{
			return available.ServerActive();
		}

		public override string ServerGetClientAddress(int connectionId)
		{
			return available.ServerGetClientAddress(connectionId);
		}

		public override void ServerDisconnect(int connectionId)
		{
			available.ServerDisconnect(connectionId);
		}

		public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId)
		{
			available.ServerSend(connectionId, segment, channelId);
		}

		public override void ServerStart()
		{
			available.OnServerConnected = OnServerConnected;
			available.OnServerDataReceived = OnServerDataReceived;
			available.OnServerError = OnServerError;
			available.OnServerDisconnected = OnServerDisconnected;
			available.ServerStart();
		}

		public override void ServerStop()
		{
			available.ServerStop();
		}

		public override void ClientEarlyUpdate()
		{
			available.ClientEarlyUpdate();
		}

		public override void ServerEarlyUpdate()
		{
			available.ServerEarlyUpdate();
		}

		public override void ClientLateUpdate()
		{
			available.ClientLateUpdate();
		}

		public override void ServerLateUpdate()
		{
			available.ServerLateUpdate();
		}

		public override void Shutdown()
		{
			available.Shutdown();
		}

		public override int GetMaxPacketSize(int channelId = 0)
		{
			int num = int.MaxValue;
			Transport[] array = transports;
			for (int i = 0; i < array.Length; i++)
			{
				num = Mathf.Min(array[i].GetMaxPacketSize(channelId), num);
			}
			return num;
		}

		public override string ToString()
		{
			return available.ToString();
		}
	}
}
