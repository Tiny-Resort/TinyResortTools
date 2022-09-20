using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Mirror.SimpleWeb
{
	public abstract class SimpleWebClient
	{
		private readonly int maxMessagesPerTick;

		protected readonly int maxMessageSize;

		protected readonly ConcurrentQueue<Message> receiveQueue = new ConcurrentQueue<Message>();

		protected readonly BufferPool bufferPool;

		protected ClientState state;

		public ClientState ConnectionState
		{
			get
			{
				return state;
			}
		}

		public event Action onConnect;

		public event Action onDisconnect;

		public event Action<ArraySegment<byte>> onData;

		public event Action<Exception> onError;

		public static SimpleWebClient Create(int maxMessageSize, int maxMessagesPerTick, TcpConfig tcpConfig)
		{
			return new WebSocketClientStandAlone(maxMessageSize, maxMessagesPerTick, tcpConfig);
		}

		protected SimpleWebClient(int maxMessageSize, int maxMessagesPerTick)
		{
			this.maxMessageSize = maxMessageSize;
			this.maxMessagesPerTick = maxMessagesPerTick;
			bufferPool = new BufferPool(5, 20, maxMessageSize);
		}

		public void ProcessMessageQueue(MonoBehaviour behaviour)
		{
			int num = 0;
			Message result;
			while (behaviour.enabled && num < maxMessagesPerTick && receiveQueue.TryDequeue(out result))
			{
				num++;
				switch (result.type)
				{
				case EventType.Connected:
				{
					Action action4 = this.onConnect;
					if (action4 != null)
					{
						action4();
					}
					break;
				}
				case EventType.Data:
				{
					Action<ArraySegment<byte>> action2 = this.onData;
					if (action2 != null)
					{
						action2(result.data.ToSegment());
					}
					result.data.Release();
					break;
				}
				case EventType.Disconnected:
				{
					Action action3 = this.onDisconnect;
					if (action3 != null)
					{
						action3();
					}
					break;
				}
				case EventType.Error:
				{
					Action<Exception> action = this.onError;
					if (action != null)
					{
						action(result.exception);
					}
					break;
				}
				}
			}
		}

		public abstract void Connect(Uri serverAddress);

		public abstract void Disconnect();

		public abstract void Send(ArraySegment<byte> segment);
	}
}
