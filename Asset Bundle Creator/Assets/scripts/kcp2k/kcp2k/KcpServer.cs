using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace kcp2k
{
	public class KcpServer
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass24_0
		{
			public int connectionId;

			public KcpServer _003C_003E4__this;

			public Action<ArraySegment<byte>> _003C_003E9__1;

			public Action _003C_003E9__2;

			internal void _003CTickIncoming_003Eb__1(ArraySegment<byte> message)
			{
				_003C_003E4__this.OnData(connectionId, message);
			}

			internal void _003CTickIncoming_003Eb__2()
			{
				_003C_003E4__this.connectionsToRemove.Add(connectionId);
				Log.Info(string.Format("KCP: OnServerDisconnected({0})", connectionId));
				_003C_003E4__this.OnDisconnected(connectionId);
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass24_1
		{
			public KcpServerConnection connection;

			public _003C_003Ec__DisplayClass24_0 CS_0024_003C_003E8__locals1;

			internal void _003CTickIncoming_003Eb__0()
			{
				connection.SendHandshake();
				CS_0024_003C_003E8__locals1._003C_003E4__this.connections.Add(CS_0024_003C_003E8__locals1.connectionId, connection);
				Log.Info(string.Format("KCP: server added connection({0})", CS_0024_003C_003E8__locals1.connectionId));
				connection.OnData = CS_0024_003C_003E8__locals1._003C_003E9__1 ?? (CS_0024_003C_003E8__locals1._003C_003E9__1 = CS_0024_003C_003E8__locals1._003CTickIncoming_003Eb__1);
				connection.OnDisconnected = CS_0024_003C_003E8__locals1._003C_003E9__2 ?? (CS_0024_003C_003E8__locals1._003C_003E9__2 = CS_0024_003C_003E8__locals1._003CTickIncoming_003Eb__2);
				Log.Info(string.Format("KCP: OnServerConnected({0})", CS_0024_003C_003E8__locals1.connectionId));
				CS_0024_003C_003E8__locals1._003C_003E4__this.OnConnected(CS_0024_003C_003E8__locals1.connectionId);
			}
		}

		public Action<int> OnConnected;

		public Action<int, ArraySegment<byte>> OnData;

		public Action<int> OnDisconnected;

		public bool DualMode;

		public bool NoDelay;

		public uint Interval;

		public int FastResend;

		public bool CongestionWindow;

		public uint SendWindowSize;

		public uint ReceiveWindowSize;

		public int Timeout;

		protected Socket socket;

		private EndPoint newClientEP;

		private readonly byte[] rawReceiveBuffer = new byte[1200];

		public Dictionary<int, KcpServerConnection> connections = new Dictionary<int, KcpServerConnection>();

		private HashSet<int> connectionsToRemove = new HashSet<int>();

		public KcpServer(Action<int> OnConnected, Action<int, ArraySegment<byte>> OnData, Action<int> OnDisconnected, bool DualMode, bool NoDelay, uint Interval, int FastResend = 0, bool CongestionWindow = true, uint SendWindowSize = 32u, uint ReceiveWindowSize = 128u, int Timeout = 10000)
		{
			this.OnConnected = OnConnected;
			this.OnData = OnData;
			this.OnDisconnected = OnDisconnected;
			this.DualMode = DualMode;
			this.NoDelay = NoDelay;
			this.Interval = Interval;
			this.FastResend = FastResend;
			this.CongestionWindow = CongestionWindow;
			this.SendWindowSize = SendWindowSize;
			this.ReceiveWindowSize = ReceiveWindowSize;
			this.Timeout = Timeout;
			newClientEP = (DualMode ? new IPEndPoint(IPAddress.IPv6Any, 0) : new IPEndPoint(IPAddress.Any, 0));
		}

		public bool IsActive()
		{
			return socket != null;
		}

		public void Start(ushort port)
		{
			if (socket != null)
			{
				Log.Warning("KCP: server already started!");
			}
			if (DualMode)
			{
				socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
				socket.DualMode = true;
				socket.Bind(new IPEndPoint(IPAddress.IPv6Any, port));
			}
			else
			{
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				socket.Bind(new IPEndPoint(IPAddress.Any, port));
			}
		}

		public void Send(int connectionId, ArraySegment<byte> segment, KcpChannel channel)
		{
			KcpServerConnection value;
			if (connections.TryGetValue(connectionId, out value))
			{
				value.SendData(segment, channel);
			}
		}

		public void Disconnect(int connectionId)
		{
			KcpServerConnection value;
			if (connections.TryGetValue(connectionId, out value))
			{
				value.Disconnect();
			}
		}

		public string GetClientAddress(int connectionId)
		{
			KcpServerConnection value;
			if (connections.TryGetValue(connectionId, out value))
			{
				return (value.GetRemoteEndPoint() as IPEndPoint).Address.ToString();
			}
			return "";
		}

		protected virtual int ReceiveFrom(byte[] buffer, out int connectionHash)
		{
			int result = socket.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref newClientEP);
			connectionHash = newClientEP.GetHashCode();
			return result;
		}

		protected virtual KcpServerConnection CreateConnection()
		{
			return new KcpServerConnection(socket, newClientEP, NoDelay, Interval, FastResend, CongestionWindow, SendWindowSize, ReceiveWindowSize, Timeout);
		}

		public void TickIncoming()
		{
			while (socket != null && socket.Poll(0, SelectMode.SelectRead))
			{
				try
				{
					_003C_003Ec__DisplayClass24_0 _003C_003Ec__DisplayClass24_ = new _003C_003Ec__DisplayClass24_0();
					_003C_003Ec__DisplayClass24_._003C_003E4__this = this;
					int num = ReceiveFrom(rawReceiveBuffer, out _003C_003Ec__DisplayClass24_.connectionId);
					if (num <= rawReceiveBuffer.Length)
					{
						_003C_003Ec__DisplayClass24_1 _003C_003Ec__DisplayClass24_2 = new _003C_003Ec__DisplayClass24_1();
						_003C_003Ec__DisplayClass24_2.CS_0024_003C_003E8__locals1 = _003C_003Ec__DisplayClass24_;
						if (!connections.TryGetValue(_003C_003Ec__DisplayClass24_2.CS_0024_003C_003E8__locals1.connectionId, out _003C_003Ec__DisplayClass24_2.connection))
						{
							_003C_003Ec__DisplayClass24_2.connection = CreateConnection();
							_003C_003Ec__DisplayClass24_2.connection.OnAuthenticated = _003C_003Ec__DisplayClass24_2._003CTickIncoming_003Eb__0;
							_003C_003Ec__DisplayClass24_2.connection.RawInput(rawReceiveBuffer, num);
							_003C_003Ec__DisplayClass24_2.connection.TickIncoming();
						}
						else
						{
							_003C_003Ec__DisplayClass24_2.connection.RawInput(rawReceiveBuffer, num);
						}
					}
					else
					{
						Log.Error(string.Format("KCP Server: message of size {0} does not fit into buffer of size {1}. The excess was silently dropped. Disconnecting connectionId={2}.", num, rawReceiveBuffer.Length, _003C_003Ec__DisplayClass24_.connectionId));
						Disconnect(_003C_003Ec__DisplayClass24_.connectionId);
					}
				}
				catch (SocketException)
				{
				}
			}
			foreach (KcpServerConnection value in connections.Values)
			{
				value.TickIncoming();
			}
			foreach (int item in connectionsToRemove)
			{
				connections.Remove(item);
			}
			connectionsToRemove.Clear();
		}

		public void TickOutgoing()
		{
			foreach (KcpServerConnection value in connections.Values)
			{
				value.TickOutgoing();
			}
		}

		public void Tick()
		{
			TickIncoming();
			TickOutgoing();
		}

		public void Stop()
		{
			Socket obj = socket;
			if (obj != null)
			{
				obj.Close();
			}
			socket = null;
		}

		public void Pause()
		{
			foreach (KcpServerConnection value in connections.Values)
			{
				value.Pause();
			}
		}

		public void Unpause()
		{
			foreach (KcpServerConnection value in connections.Values)
			{
				value.Unpause();
			}
		}
	}
}
