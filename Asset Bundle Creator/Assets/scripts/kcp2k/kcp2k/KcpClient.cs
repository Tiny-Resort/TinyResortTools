using System;
using System.Runtime.CompilerServices;

namespace kcp2k
{
	public class KcpClient
	{
		public Action OnConnected;

		public Action<ArraySegment<byte>> OnData;

		public Action OnDisconnected;

		public KcpClientConnection connection;

		public bool connected;

		public KcpClient(Action OnConnected, Action<ArraySegment<byte>> OnData, Action OnDisconnected)
		{
			this.OnConnected = OnConnected;
			this.OnData = OnData;
			this.OnDisconnected = OnDisconnected;
		}

		protected virtual KcpClientConnection CreateConnection()
		{
			return new KcpClientConnection();
		}

		public void Connect(string address, ushort port, bool noDelay, uint interval, int fastResend = 0, bool congestionWindow = true, uint sendWindowSize = 32u, uint receiveWindowSize = 128u, int timeout = 10000)
		{
			if (connected)
			{
				Log.Warning("KCP: client already connected!");
				return;
			}
			connection = CreateConnection();
			connection.OnAuthenticated = _003CConnect_003Eb__7_0;
			connection.OnData = _003CConnect_003Eb__7_1;
			connection.OnDisconnected = _003CConnect_003Eb__7_2;
			connection.Connect(address, port, noDelay, interval, fastResend, congestionWindow, sendWindowSize, receiveWindowSize, timeout);
		}

		public void Send(ArraySegment<byte> segment, KcpChannel channel)
		{
			if (connected)
			{
				connection.SendData(segment, channel);
			}
			else
			{
				Log.Warning("KCP: can't send because client not connected!");
			}
		}

		public void Disconnect()
		{
			if (connected)
			{
				KcpClientConnection kcpClientConnection = connection;
				if (kcpClientConnection != null)
				{
					kcpClientConnection.Disconnect();
				}
			}
		}

		public void TickIncoming()
		{
			KcpClientConnection kcpClientConnection = connection;
			if (kcpClientConnection != null)
			{
				kcpClientConnection.RawReceive();
			}
			KcpClientConnection kcpClientConnection2 = connection;
			if (kcpClientConnection2 != null)
			{
				kcpClientConnection2.TickIncoming();
			}
		}

		public void TickOutgoing()
		{
			KcpClientConnection kcpClientConnection = connection;
			if (kcpClientConnection != null)
			{
				kcpClientConnection.TickOutgoing();
			}
		}

		public void Tick()
		{
			TickIncoming();
			TickOutgoing();
		}

		public void Pause()
		{
			KcpClientConnection kcpClientConnection = connection;
			if (kcpClientConnection != null)
			{
				kcpClientConnection.Pause();
			}
		}

		public void Unpause()
		{
			KcpClientConnection kcpClientConnection = connection;
			if (kcpClientConnection != null)
			{
				kcpClientConnection.Unpause();
			}
		}

		[CompilerGenerated]
		private void _003CConnect_003Eb__7_0()
		{
			Log.Info("KCP: OnClientConnected");
			connected = true;
			OnConnected();
		}

		[CompilerGenerated]
		private void _003CConnect_003Eb__7_1(ArraySegment<byte> message)
		{
			OnData(message);
		}

		[CompilerGenerated]
		private void _003CConnect_003Eb__7_2()
		{
			Log.Info("KCP: OnClientDisconnected");
			connected = false;
			connection = null;
			OnDisconnected();
		}
	}
}
