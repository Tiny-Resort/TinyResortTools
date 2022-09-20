using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Telepathy
{
	public class Server : Common
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass16_0
		{
			public int connectionId;

			public TcpClient client;

			public ConnectionState connection;

			public Thread sendThread;

			public Server _003C_003E4__this;

			internal void _003CListen_003Eb__0()
			{
				try
				{
					ThreadFunctions.SendLoop(connectionId, client, connection.sendPipe, connection.sendPending);
				}
				catch (ThreadAbortException)
				{
				}
				catch (Exception ex2)
				{
					Log.Error("Server send thread exception: " + ((ex2 != null) ? ex2.ToString() : null));
				}
			}

			internal void _003CListen_003Eb__1()
			{
				try
				{
					ThreadFunctions.ReceiveLoop(connectionId, client, _003C_003E4__this.MaxMessageSize, _003C_003E4__this.receivePipe, _003C_003E4__this.ReceiveQueueLimit);
					sendThread.Interrupt();
				}
				catch (Exception ex)
				{
					Log.Error("Server client thread exception: " + ((ex != null) ? ex.ToString() : null));
				}
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass17_0
		{
			public Server _003C_003E4__this;

			public int port;

			internal void _003CStart_003Eb__0()
			{
				_003C_003E4__this.Listen(port);
			}
		}

		public Action<int> OnConnected;

		public Action<int, ArraySegment<byte>> OnData;

		public Action<int> OnDisconnected;

		public TcpListener listener;

		private Thread listenerThread;

		public int SendQueueLimit = 10000;

		public int ReceiveQueueLimit = 10000;

		protected MagnificentReceivePipe receivePipe;

		private readonly ConcurrentDictionary<int, ConnectionState> clients = new ConcurrentDictionary<int, ConnectionState>();

		private int counter;

		public int ReceivePipeTotalCount
		{
			get
			{
				return receivePipe.TotalCount;
			}
		}

		public bool Active
		{
			get
			{
				if (listenerThread != null)
				{
					return listenerThread.IsAlive;
				}
				return false;
			}
		}

		public int NextConnectionId()
		{
			int num = Interlocked.Increment(ref counter);
			if (num == int.MaxValue)
			{
				throw new Exception("connection id limit reached: " + num);
			}
			return num;
		}

		public Server(int MaxMessageSize)
			: base(MaxMessageSize)
		{
		}

		private void Listen(int port)
		{
			try
			{
				listener = TcpListener.Create(port);
				listener.Server.NoDelay = NoDelay;
				listener.Start();
				Log.Info("Server: listening port=" + port);
				while (true)
				{
					_003C_003Ec__DisplayClass16_0 _003C_003Ec__DisplayClass16_ = new _003C_003Ec__DisplayClass16_0();
					_003C_003Ec__DisplayClass16_._003C_003E4__this = this;
					_003C_003Ec__DisplayClass16_.client = listener.AcceptTcpClient();
					_003C_003Ec__DisplayClass16_.client.NoDelay = NoDelay;
					_003C_003Ec__DisplayClass16_.client.SendTimeout = SendTimeout;
					_003C_003Ec__DisplayClass16_.client.ReceiveTimeout = ReceiveTimeout;
					_003C_003Ec__DisplayClass16_.connectionId = NextConnectionId();
					_003C_003Ec__DisplayClass16_.connection = new ConnectionState(_003C_003Ec__DisplayClass16_.client, MaxMessageSize);
					clients[_003C_003Ec__DisplayClass16_.connectionId] = _003C_003Ec__DisplayClass16_.connection;
					_003C_003Ec__DisplayClass16_.sendThread = new Thread(_003C_003Ec__DisplayClass16_._003CListen_003Eb__0);
					_003C_003Ec__DisplayClass16_.sendThread.IsBackground = true;
					_003C_003Ec__DisplayClass16_.sendThread.Start();
					Thread thread = new Thread(_003C_003Ec__DisplayClass16_._003CListen_003Eb__1);
					thread.IsBackground = true;
					thread.Start();
				}
			}
			catch (ThreadAbortException ex)
			{
				Log.Info("Server thread aborted. That's okay. " + ((ex != null) ? ex.ToString() : null));
			}
			catch (SocketException ex2)
			{
				Log.Info("Server Thread stopped. That's okay. " + ((ex2 != null) ? ex2.ToString() : null));
			}
			catch (Exception ex3)
			{
				Log.Error("Server Exception: " + ((ex3 != null) ? ex3.ToString() : null));
			}
		}

		public bool Start(int port)
		{
			_003C_003Ec__DisplayClass17_0 _003C_003Ec__DisplayClass17_ = new _003C_003Ec__DisplayClass17_0();
			_003C_003Ec__DisplayClass17_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass17_.port = port;
			if (Active)
			{
				return false;
			}
			receivePipe = new MagnificentReceivePipe(MaxMessageSize);
			Log.Info("Server: Start port=" + _003C_003Ec__DisplayClass17_.port);
			listenerThread = new Thread(_003C_003Ec__DisplayClass17_._003CStart_003Eb__0);
			listenerThread.IsBackground = true;
			listenerThread.Priority = ThreadPriority.BelowNormal;
			listenerThread.Start();
			return true;
		}

		public void Stop()
		{
			if (!Active)
			{
				return;
			}
			Log.Info("Server: stopping...");
			TcpListener tcpListener = listener;
			if (tcpListener != null)
			{
				tcpListener.Stop();
			}
			Thread thread = listenerThread;
			if (thread != null)
			{
				thread.Interrupt();
			}
			listenerThread = null;
			foreach (KeyValuePair<int, ConnectionState> client2 in clients)
			{
				TcpClient client = client2.Value.client;
				try
				{
					client.GetStream().Close();
				}
				catch
				{
				}
				client.Close();
			}
			clients.Clear();
			counter = 0;
		}

		public bool Send(int connectionId, ArraySegment<byte> message)
		{
			if (message.Count <= MaxMessageSize)
			{
				ConnectionState value;
				if (clients.TryGetValue(connectionId, out value))
				{
					if (value.sendPipe.Count < SendQueueLimit)
					{
						value.sendPipe.Enqueue(message);
						value.sendPending.Set();
						return true;
					}
					Log.Warning(string.Format("Server.Send: sendPipe for connection {0} reached limit of {1}. This can happen if we call send faster than the network can process messages. Disconnecting this connection for load balancing.", connectionId, SendQueueLimit));
					value.client.Close();
					return false;
				}
				return false;
			}
			Log.Error("Server.Send: message too big: " + message.Count + ". Limit: " + MaxMessageSize);
			return false;
		}

		public string GetClientAddress(int connectionId)
		{
			ConnectionState value;
			if (clients.TryGetValue(connectionId, out value))
			{
				return ((IPEndPoint)value.client.Client.RemoteEndPoint).Address.ToString();
			}
			return "";
		}

		public bool Disconnect(int connectionId)
		{
			ConnectionState value;
			if (clients.TryGetValue(connectionId, out value))
			{
				value.client.Close();
				Log.Info("Server.Disconnect connectionId:" + connectionId);
				return true;
			}
			return false;
		}

		public int Tick(int processLimit, Func<bool> checkEnabled = null)
		{
			if (receivePipe == null)
			{
				return 0;
			}
			for (int i = 0; i < processLimit; i++)
			{
				if (checkEnabled != null && !checkEnabled())
				{
					break;
				}
				int connectionId;
				EventType eventType;
				ArraySegment<byte> data;
				if (!receivePipe.TryPeek(out connectionId, out eventType, out data))
				{
					break;
				}
				switch (eventType)
				{
				case EventType.Connected:
				{
					Action<int> onConnected = OnConnected;
					if (onConnected != null)
					{
						onConnected(connectionId);
					}
					break;
				}
				case EventType.Data:
				{
					Action<int, ArraySegment<byte>> onData = OnData;
					if (onData != null)
					{
						onData(connectionId, data);
					}
					break;
				}
				case EventType.Disconnected:
				{
					Action<int> onDisconnected = OnDisconnected;
					if (onDisconnected != null)
					{
						onDisconnected(connectionId);
					}
					ConnectionState value;
					clients.TryRemove(connectionId, out value);
					break;
				}
				}
				receivePipe.TryDequeue();
			}
			return receivePipe.TotalCount;
		}
	}
}
