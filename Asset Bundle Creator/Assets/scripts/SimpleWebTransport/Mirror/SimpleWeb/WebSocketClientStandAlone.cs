using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mirror.SimpleWeb
{
	public class WebSocketClientStandAlone : SimpleWebClient
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass5_0
		{
			public WebSocketClientStandAlone _003C_003E4__this;

			public Uri serverAddress;

			internal void _003CConnect_003Eb__0()
			{
				_003C_003E4__this.ConnectAndReceiveLoop(serverAddress);
			}
		}

		private readonly ClientSslHelper sslHelper;

		private readonly ClientHandshake handshake;

		private readonly TcpConfig tcpConfig;

		private Connection conn;

		internal WebSocketClientStandAlone(int maxMessageSize, int maxMessagesPerTick, TcpConfig tcpConfig)
			: base(maxMessageSize, maxMessagesPerTick)
		{
			sslHelper = new ClientSslHelper();
			handshake = new ClientHandshake();
			this.tcpConfig = tcpConfig;
		}

		public override void Connect(Uri serverAddress)
		{
			_003C_003Ec__DisplayClass5_0 _003C_003Ec__DisplayClass5_ = new _003C_003Ec__DisplayClass5_0();
			_003C_003Ec__DisplayClass5_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass5_.serverAddress = serverAddress;
			state = ClientState.Connecting;
			Thread thread = new Thread(_003C_003Ec__DisplayClass5_._003CConnect_003Eb__0);
			thread.IsBackground = true;
			thread.Start();
		}

		private void ConnectAndReceiveLoop(Uri serverAddress)
		{
			try
			{
				TcpClient tcpClient = new TcpClient();
				tcpConfig.ApplyTo(tcpClient);
				conn = new Connection(tcpClient, AfterConnectionDisposed);
				conn.receiveThread = Thread.CurrentThread;
				try
				{
					tcpClient.Connect(serverAddress.Host, serverAddress.Port);
				}
				catch (SocketException)
				{
					tcpClient.Dispose();
					throw;
				}
				if (!sslHelper.TryCreateStream(conn, serverAddress))
				{
					conn.Dispose();
					return;
				}
				if (!handshake.TryHandshake(conn, serverAddress))
				{
					conn.Dispose();
					return;
				}
				state = ClientState.Connected;
				receiveQueue.Enqueue(new Message(EventType.Connected));
				Thread thread = new Thread(_003CConnectAndReceiveLoop_003Eb__6_0);
				conn.sendThread = thread;
				thread.IsBackground = true;
				thread.Start();
				ReceiveLoop.Loop(new ReceiveLoop.Config(conn, maxMessageSize, false, receiveQueue, bufferPool));
			}
			catch (ThreadInterruptedException)
			{
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
			finally
			{
				Connection connection = conn;
				if (connection != null)
				{
					connection.Dispose();
				}
			}
		}

		private void AfterConnectionDisposed(Connection conn)
		{
			state = ClientState.NotConnected;
			receiveQueue.Enqueue(new Message(EventType.Disconnected));
		}

		public override void Disconnect()
		{
			state = ClientState.Disconnecting;
			if (conn == null)
			{
				state = ClientState.NotConnected;
				return;
			}
			Connection connection = conn;
			if (connection != null)
			{
				connection.Dispose();
			}
		}

		public override void Send(ArraySegment<byte> segment)
		{
			ArrayBuffer arrayBuffer = bufferPool.Take(segment.Count);
			arrayBuffer.CopyFrom(segment);
			conn.sendQueue.Enqueue(arrayBuffer);
			conn.sendPending.Set();
		}

		[CompilerGenerated]
		private void _003CConnectAndReceiveLoop_003Eb__6_0()
		{
			SendLoop.Loop(new SendLoop.Config(conn, 8 + maxMessageSize, true));
		}
	}
}
