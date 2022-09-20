using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Mirror
{
	[DisallowMultipleComponent]
	public class MultiplexTransport : Transport
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass18_0
		{
			public int locali;

			public MultiplexTransport _003C_003E4__this;

			internal void _003CAddServerCallbacks_003Eb__0(int baseConnectionId)
			{
				_003C_003E4__this.OnServerConnected(_003C_003E4__this.FromBaseId(locali, baseConnectionId));
			}

			internal void _003CAddServerCallbacks_003Eb__1(int baseConnectionId, ArraySegment<byte> data, int channel)
			{
				_003C_003E4__this.OnServerDataReceived(_003C_003E4__this.FromBaseId(locali, baseConnectionId), data, channel);
			}

			internal void _003CAddServerCallbacks_003Eb__2(int baseConnectionId, Exception error)
			{
				_003C_003E4__this.OnServerError(_003C_003E4__this.FromBaseId(locali, baseConnectionId), error);
			}

			internal void _003CAddServerCallbacks_003Eb__3(int baseConnectionId)
			{
				_003C_003E4__this.OnServerDisconnected(_003C_003E4__this.FromBaseId(locali, baseConnectionId));
			}
		}

		public Transport[] transports;

		private Transport available;

		public void Awake()
		{
			if (transports == null || transports.Length == 0)
			{
				Debug.LogError("Multiplex transport requires at least 1 underlying transport");
			}
		}

		public override void ClientEarlyUpdate()
		{
			Transport[] array = transports;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ClientEarlyUpdate();
			}
		}

		public override void ServerEarlyUpdate()
		{
			Transport[] array = transports;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ServerEarlyUpdate();
			}
		}

		public override void ClientLateUpdate()
		{
			Transport[] array = transports;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ClientLateUpdate();
			}
		}

		public override void ServerLateUpdate()
		{
			Transport[] array = transports;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ServerLateUpdate();
			}
		}

		private void OnEnable()
		{
			Transport[] array = transports;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = true;
			}
		}

		private void OnDisable()
		{
			Transport[] array = transports;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}

		public override bool Available()
		{
			Transport[] array = transports;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Available())
				{
					return true;
				}
			}
			return false;
		}

		public override void ClientConnect(string address)
		{
			Transport[] array = transports;
			foreach (Transport transport in array)
			{
				if (transport.Available())
				{
					available = transport;
					transport.OnClientConnected = OnClientConnected;
					transport.OnClientDataReceived = OnClientDataReceived;
					transport.OnClientError = OnClientError;
					transport.OnClientDisconnected = OnClientDisconnected;
					transport.ClientConnect(address);
					return;
				}
			}
			throw new ArgumentException("No transport suitable for this platform");
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
						available = transport;
						transport.OnClientConnected = OnClientConnected;
						transport.OnClientDataReceived = OnClientDataReceived;
						transport.OnClientError = OnClientError;
						transport.OnClientDisconnected = OnClientDisconnected;
						transport.ClientConnect(uri);
						return;
					}
					catch (ArgumentException)
					{
					}
				}
			}
			throw new ArgumentException("No transport suitable for this platform");
		}

		public override bool ClientConnected()
		{
			if ((object)available != null)
			{
				return available.ClientConnected();
			}
			return false;
		}

		public override void ClientDisconnect()
		{
			if ((object)available != null)
			{
				available.ClientDisconnect();
			}
		}

		public override void ClientSend(ArraySegment<byte> segment, int channelId)
		{
			available.ClientSend(segment, channelId);
		}

		private int FromBaseId(int transportId, int connectionId)
		{
			return connectionId * transports.Length + transportId;
		}

		private int ToBaseId(int connectionId)
		{
			return connectionId / transports.Length;
		}

		private int ToTransportId(int connectionId)
		{
			return connectionId % transports.Length;
		}

		private void AddServerCallbacks()
		{
			for (int i = 0; i < transports.Length; i++)
			{
				_003C_003Ec__DisplayClass18_0 _003C_003Ec__DisplayClass18_ = new _003C_003Ec__DisplayClass18_0();
				_003C_003Ec__DisplayClass18_._003C_003E4__this = this;
				_003C_003Ec__DisplayClass18_.locali = i;
				Transport obj = transports[i];
				obj.OnServerConnected = _003C_003Ec__DisplayClass18_._003CAddServerCallbacks_003Eb__0;
				obj.OnServerDataReceived = _003C_003Ec__DisplayClass18_._003CAddServerCallbacks_003Eb__1;
				obj.OnServerError = _003C_003Ec__DisplayClass18_._003CAddServerCallbacks_003Eb__2;
				obj.OnServerDisconnected = _003C_003Ec__DisplayClass18_._003CAddServerCallbacks_003Eb__3;
			}
		}

		public override Uri ServerUri()
		{
			return transports[0].ServerUri();
		}

		public override bool ServerActive()
		{
			Transport[] array = transports;
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].ServerActive())
				{
					return false;
				}
			}
			return true;
		}

		public override string ServerGetClientAddress(int connectionId)
		{
			int connectionId2 = ToBaseId(connectionId);
			int num = ToTransportId(connectionId);
			return transports[num].ServerGetClientAddress(connectionId2);
		}

		public override void ServerDisconnect(int connectionId)
		{
			int connectionId2 = ToBaseId(connectionId);
			int num = ToTransportId(connectionId);
			transports[num].ServerDisconnect(connectionId2);
		}

		public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId)
		{
			int connectionId2 = ToBaseId(connectionId);
			int num = ToTransportId(connectionId);
			for (int i = 0; i < transports.Length; i++)
			{
				if (i == num)
				{
					transports[i].ServerSend(connectionId2, segment, channelId);
				}
			}
		}

		public override void ServerStart()
		{
			Transport[] array = transports;
			foreach (Transport obj in array)
			{
				AddServerCallbacks();
				obj.ServerStart();
			}
		}

		public override void ServerStop()
		{
			Transport[] array = transports;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ServerStop();
			}
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

		public override void Shutdown()
		{
			Transport[] array = transports;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Shutdown();
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			Transport[] array = transports;
			foreach (Transport transport in array)
			{
				stringBuilder.AppendLine(transport.ToString());
			}
			return stringBuilder.ToString().Trim();
		}
	}
}
