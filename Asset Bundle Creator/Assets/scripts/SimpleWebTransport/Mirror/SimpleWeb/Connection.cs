using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Mirror.SimpleWeb
{
	internal sealed class Connection : IDisposable
	{
		public const int IdNotSet = -1;

		private readonly object disposedLock = new object();

		public TcpClient client;

		public int connId = -1;

		public Stream stream;

		public Thread receiveThread;

		public Thread sendThread;

		public ManualResetEventSlim sendPending = new ManualResetEventSlim(false);

		public ConcurrentQueue<ArrayBuffer> sendQueue = new ConcurrentQueue<ArrayBuffer>();

		public Action<Connection> onDispose;

		private volatile bool hasDisposed;

		public Connection(TcpClient client, Action<Connection> onDispose)
		{
			if (client == null)
			{
				throw new ArgumentNullException("client");
			}
			this.client = client;
			this.onDispose = onDispose;
		}

		public void Dispose()
		{
			if (hasDisposed)
			{
				return;
			}
			lock (disposedLock)
			{
				if (hasDisposed)
				{
					return;
				}
				hasDisposed = true;
				receiveThread.Interrupt();
				Thread thread = sendThread;
				if (thread != null)
				{
					thread.Interrupt();
				}
				try
				{
					Stream obj = stream;
					if (obj != null)
					{
						obj.Dispose();
					}
					stream = null;
					client.Dispose();
					client = null;
				}
				catch (Exception e)
				{
					Log.Exception(e);
				}
				sendPending.Dispose();
				ArrayBuffer result;
				while (sendQueue.TryDequeue(out result))
				{
					result.Release();
				}
				onDispose(this);
			}
		}

		public override string ToString()
		{
			TcpClient tcpClient = client;
			object obj;
			if (tcpClient == null)
			{
				obj = null;
			}
			else
			{
				Socket socket = tcpClient.Client;
				obj = ((socket != null) ? socket.RemoteEndPoint : null);
			}
			EndPoint arg = (EndPoint)obj;
			return string.Format("[Conn:{0}, endPoint:{1}]", connId, arg);
		}
	}
}
