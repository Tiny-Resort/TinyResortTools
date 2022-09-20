using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mirror.SimpleWeb
{
	public class WebSocketServer
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass14_0
		{
			public Connection conn;

			public WebSocketServer _003C_003E4__this;

			internal void _003CacceptLoop_003Eb__0()
			{
				_003C_003E4__this.HandshakeAndReceiveLoop(conn);
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass15_0
		{
			public Connection conn;

			public WebSocketServer _003C_003E4__this;

			internal void _003CHandshakeAndReceiveLoop_003Eb__0()
			{
				SendLoop.Loop(new SendLoop.Config(conn, 4 + _003C_003E4__this.maxMessageSize, false));
			}
		}

		public readonly ConcurrentQueue<Message> receiveQueue = new ConcurrentQueue<Message>();

		private readonly TcpConfig tcpConfig;

		private readonly int maxMessageSize;

		private TcpListener listener;

		private Thread acceptThread;

		private bool serverStopped;

		private readonly ServerHandshake handShake;

		private readonly ServerSslHelper sslHelper;

		private readonly BufferPool bufferPool;

		private readonly ConcurrentDictionary<int, Connection> connections = new ConcurrentDictionary<int, Connection>();

		private int _idCounter;

		public WebSocketServer(TcpConfig tcpConfig, int maxMessageSize, int handshakeMaxSize, SslConfig sslConfig, BufferPool bufferPool)
		{
			this.tcpConfig = tcpConfig;
			this.maxMessageSize = maxMessageSize;
			sslHelper = new ServerSslHelper(sslConfig);
			this.bufferPool = bufferPool;
			handShake = new ServerHandshake(this.bufferPool, handshakeMaxSize);
		}

		public void Listen(int port)
		{
			listener = TcpListener.Create(port);
			listener.Start();
			acceptThread = new Thread(acceptLoop);
			acceptThread.IsBackground = true;
			acceptThread.Start();
		}

		public void Stop()
		{
			serverStopped = true;
			Thread thread = acceptThread;
			if (thread != null)
			{
				thread.Interrupt();
			}
			TcpListener tcpListener = listener;
			if (tcpListener != null)
			{
				tcpListener.Stop();
			}
			acceptThread = null;
			Connection[] array = connections.Values.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Dispose();
			}
			connections.Clear();
		}

		private void acceptLoop()
		{
			try
			{
				try
				{
					while (true)
					{
						_003C_003Ec__DisplayClass14_0 _003C_003Ec__DisplayClass14_ = new _003C_003Ec__DisplayClass14_0();
						_003C_003Ec__DisplayClass14_._003C_003E4__this = this;
						TcpClient client = listener.AcceptTcpClient();
						tcpConfig.ApplyTo(client);
						_003C_003Ec__DisplayClass14_.conn = new Connection(client, AfterConnectionDisposed);
						Thread thread = new Thread(_003C_003Ec__DisplayClass14_._003CacceptLoop_003Eb__0);
						_003C_003Ec__DisplayClass14_.conn.receiveThread = thread;
						thread.IsBackground = true;
						thread.Start();
					}
				}
				catch (SocketException)
				{
					Utils.CheckForInterupt();
					throw;
				}
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
		}

		private void HandshakeAndReceiveLoop(Connection conn)
		{
			_003C_003Ec__DisplayClass15_0 _003C_003Ec__DisplayClass15_ = new _003C_003Ec__DisplayClass15_0();
			_003C_003Ec__DisplayClass15_.conn = conn;
			_003C_003Ec__DisplayClass15_._003C_003E4__this = this;
			try
			{
				if (!sslHelper.TryCreateStream(_003C_003Ec__DisplayClass15_.conn))
				{
					_003C_003Ec__DisplayClass15_.conn.Dispose();
				}
				else if (!handShake.TryHandshake(_003C_003Ec__DisplayClass15_.conn))
				{
					_003C_003Ec__DisplayClass15_.conn.Dispose();
				}
				else if (!serverStopped)
				{
					_003C_003Ec__DisplayClass15_.conn.connId = Interlocked.Increment(ref _idCounter);
					connections.TryAdd(_003C_003Ec__DisplayClass15_.conn.connId, _003C_003Ec__DisplayClass15_.conn);
					receiveQueue.Enqueue(new Message(_003C_003Ec__DisplayClass15_.conn.connId, EventType.Connected));
					Thread thread = new Thread(_003C_003Ec__DisplayClass15_._003CHandshakeAndReceiveLoop_003Eb__0);
					_003C_003Ec__DisplayClass15_.conn.sendThread = thread;
					thread.IsBackground = true;
					thread.Name = string.Format("SendLoop {0}", _003C_003Ec__DisplayClass15_.conn.connId);
					thread.Start();
					ReceiveLoop.Loop(new ReceiveLoop.Config(_003C_003Ec__DisplayClass15_.conn, maxMessageSize, true, receiveQueue, bufferPool));
				}
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
				_003C_003Ec__DisplayClass15_.conn.Dispose();
			}
		}

		private void AfterConnectionDisposed(Connection conn)
		{
			if (conn.connId != -1)
			{
				receiveQueue.Enqueue(new Message(conn.connId, EventType.Disconnected));
				Connection value;
				connections.TryRemove(conn.connId, out value);
			}
		}

		public void Send(int id, ArrayBuffer buffer)
		{
			Connection value;
			if (connections.TryGetValue(id, out value))
			{
				value.sendQueue.Enqueue(buffer);
				value.sendPending.Set();
			}
		}

		public bool CloseConnection(int id)
		{
			Connection value;
			if (connections.TryGetValue(id, out value))
			{
				value.Dispose();
				return true;
			}
			return false;
		}

		public string GetClientAddress(int id)
		{
			Connection value;
			if (connections.TryGetValue(id, out value))
			{
				return value.client.Client.RemoteEndPoint.ToString();
			}
			return null;
		}
	}
}
