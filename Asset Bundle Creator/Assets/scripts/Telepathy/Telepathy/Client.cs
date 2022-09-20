using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Telepathy
{
	public class Client : Common
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass13_0
		{
			public ClientConnectionState state;

			internal void _003CReceiveThreadFunction_003Eb__0()
			{
				ThreadFunctions.SendLoop(0, state.client, state.sendPipe, state.sendPending);
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass14_0
		{
			public Client _003C_003E4__this;

			public string ip;

			public int port;

			internal void _003CConnect_003Eb__0()
			{
				ReceiveThreadFunction(_003C_003E4__this.state, ip, port, _003C_003E4__this.MaxMessageSize, _003C_003E4__this.NoDelay, _003C_003E4__this.SendTimeout, _003C_003E4__this.ReceiveTimeout, _003C_003E4__this.ReceiveQueueLimit);
			}
		}

		public Action OnConnected;

		public Action<ArraySegment<byte>> OnData;

		public Action OnDisconnected;

		public int SendQueueLimit = 10000;

		public int ReceiveQueueLimit = 10000;

		private ClientConnectionState state;

		public bool Connected
		{
			get
			{
				if (state != null)
				{
					return state.Connected;
				}
				return false;
			}
		}

		public bool Connecting
		{
			get
			{
				if (state != null)
				{
					return state.Connecting;
				}
				return false;
			}
		}

		public int ReceivePipeCount
		{
			get
			{
				if (state == null)
				{
					return 0;
				}
				return state.receivePipe.TotalCount;
			}
		}

		public Client(int MaxMessageSize)
			: base(MaxMessageSize)
		{
		}

		private static void ReceiveThreadFunction(ClientConnectionState state, string ip, int port, int MaxMessageSize, bool NoDelay, int SendTimeout, int ReceiveTimeout, int ReceiveQueueLimit)
		{
			_003C_003Ec__DisplayClass13_0 _003C_003Ec__DisplayClass13_ = new _003C_003Ec__DisplayClass13_0();
			_003C_003Ec__DisplayClass13_.state = state;
			Thread thread = null;
			try
			{
				_003C_003Ec__DisplayClass13_.state.client.Connect(ip, port);
				_003C_003Ec__DisplayClass13_.state.Connecting = false;
				_003C_003Ec__DisplayClass13_.state.client.NoDelay = NoDelay;
				_003C_003Ec__DisplayClass13_.state.client.SendTimeout = SendTimeout;
				_003C_003Ec__DisplayClass13_.state.client.ReceiveTimeout = ReceiveTimeout;
				thread = new Thread(_003C_003Ec__DisplayClass13_._003CReceiveThreadFunction_003Eb__0);
				thread.IsBackground = true;
				thread.Start();
				ThreadFunctions.ReceiveLoop(0, _003C_003Ec__DisplayClass13_.state.client, MaxMessageSize, _003C_003Ec__DisplayClass13_.state.receivePipe, ReceiveQueueLimit);
			}
			catch (SocketException ex)
			{
				Action<string> info = Log.Info;
				string[] obj = new string[6]
				{
					"Client Recv: failed to connect to ip=",
					ip,
					" port=",
					port.ToString(),
					" reason=",
					null
				};
				obj[5] = ((ex != null) ? ex.ToString() : null);
				info(string.Concat(obj));
				_003C_003Ec__DisplayClass13_.state.receivePipe.Enqueue(0, EventType.Disconnected, default(ArraySegment<byte>));
			}
			catch (ThreadInterruptedException)
			{
			}
			catch (ThreadAbortException)
			{
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex5)
			{
				Log.Error("Client Recv Exception: " + ((ex5 != null) ? ex5.ToString() : null));
			}
			if (thread != null)
			{
				thread.Interrupt();
			}
			_003C_003Ec__DisplayClass13_.state.Connecting = false;
			TcpClient client = _003C_003Ec__DisplayClass13_.state.client;
			if (client != null)
			{
				client.Close();
			}
		}

		public void Connect(string ip, int port)
		{
			_003C_003Ec__DisplayClass14_0 _003C_003Ec__DisplayClass14_ = new _003C_003Ec__DisplayClass14_0();
			_003C_003Ec__DisplayClass14_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass14_.ip = ip;
			_003C_003Ec__DisplayClass14_.port = port;
			if (Connecting || Connected)
			{
				Log.Warning("Telepathy Client can not create connection because an existing connection is connecting or connected");
				return;
			}
			state = new ClientConnectionState(MaxMessageSize);
			state.Connecting = true;
			state.client.Client = null;
			state.receiveThread = new Thread(_003C_003Ec__DisplayClass14_._003CConnect_003Eb__0);
			state.receiveThread.IsBackground = true;
			state.receiveThread.Start();
		}

		public void Disconnect()
		{
			if (Connecting || Connected)
			{
				state.Dispose();
			}
		}

		public bool Send(ArraySegment<byte> message)
		{
			if (Connected)
			{
				if (message.Count <= MaxMessageSize)
				{
					if (state.sendPipe.Count < SendQueueLimit)
					{
						state.sendPipe.Enqueue(message);
						state.sendPending.Set();
						return true;
					}
					Log.Warning(string.Format("Client.Send: sendPipe reached limit of {0}. This can happen if we call send faster than the network can process messages. Disconnecting to avoid ever growing memory & latency.", SendQueueLimit));
					state.client.Close();
					return false;
				}
				Log.Error("Client.Send: message too big: " + message.Count + ". Limit: " + MaxMessageSize);
				return false;
			}
			Log.Warning("Client.Send: not connected!");
			return false;
		}

		public int Tick(int processLimit, Func<bool> checkEnabled = null)
		{
			if (state == null)
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
				if (!state.receivePipe.TryPeek(out connectionId, out eventType, out data))
				{
					break;
				}
				switch (eventType)
				{
				case EventType.Connected:
				{
					Action onConnected = OnConnected;
					if (onConnected != null)
					{
						onConnected();
					}
					break;
				}
				case EventType.Data:
				{
					Action<ArraySegment<byte>> onData = OnData;
					if (onData != null)
					{
						onData(data);
					}
					break;
				}
				case EventType.Disconnected:
				{
					Action onDisconnected = OnDisconnected;
					if (onDisconnected != null)
					{
						onDisconnected();
					}
					break;
				}
				}
				state.receivePipe.TryDequeue();
			}
			return state.receivePipe.TotalCount;
		}
	}
}
