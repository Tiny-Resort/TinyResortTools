using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace kcp2k
{
	public abstract class KcpConnection
	{
		protected Socket socket;

		protected EndPoint remoteEndPoint;

		internal Kcp kcp;

		private KcpState state = KcpState.Disconnected;

		public Action OnAuthenticated;

		public Action<ArraySegment<byte>> OnData;

		public Action OnDisconnected;

		private bool paused;

		public const int DEFAULT_TIMEOUT = 10000;

		public int timeout = 10000;

		private uint lastReceiveTime;

		private readonly Stopwatch refTime = new Stopwatch();

		private const int CHANNEL_HEADER_SIZE = 1;

		public const int ReliableMaxMessageSize = 149224;

		public const int UnreliableMaxMessageSize = 1199;

		private byte[] kcpMessageBuffer = new byte[149225];

		private byte[] kcpSendBuffer = new byte[149225];

		private byte[] rawSendBuffer = new byte[1200];

		public const int PING_INTERVAL = 1000;

		private uint lastPingTime;

		internal const int QueueDisconnectThreshold = 10000;

		public int SendQueueCount
		{
			get
			{
				return kcp.snd_queue.Count;
			}
		}

		public int ReceiveQueueCount
		{
			get
			{
				return kcp.rcv_queue.Count;
			}
		}

		public int SendBufferCount
		{
			get
			{
				return kcp.snd_buf.Count;
			}
		}

		public int ReceiveBufferCount
		{
			get
			{
				return kcp.rcv_buf.Count;
			}
		}

		public uint MaxSendRate
		{
			get
			{
				return kcp.snd_wnd * kcp.mtu * 1000 / kcp.interval;
			}
		}

		public uint MaxReceiveRate
		{
			get
			{
				return kcp.rcv_wnd * kcp.mtu * 1000 / kcp.interval;
			}
		}

		protected void SetupKcp(bool noDelay, uint interval = 100u, int fastResend = 0, bool congestionWindow = true, uint sendWindowSize = 32u, uint receiveWindowSize = 128u, int timeout = 10000)
		{
			kcp = new Kcp(0u, RawSendReliable);
			kcp.SetNoDelay(noDelay ? 1u : 0u, interval, fastResend, !congestionWindow);
			kcp.SetWindowSize(sendWindowSize, receiveWindowSize);
			kcp.SetMtu(1199u);
			this.timeout = timeout;
			state = KcpState.Connected;
			refTime.Start();
		}

		private void HandleTimeout(uint time)
		{
			if (time >= lastReceiveTime + timeout)
			{
				Log.Warning(string.Format("KCP: Connection timed out after not receiving any message for {0}ms. Disconnecting.", timeout));
				Disconnect();
			}
		}

		private void HandleDeadLink()
		{
			if (kcp.state == -1)
			{
				Log.Warning("KCP Connection dead_link detected. Disconnecting.");
				Disconnect();
			}
		}

		private void HandlePing(uint time)
		{
			if (time >= lastPingTime + 1000)
			{
				SendPing();
				lastPingTime = time;
			}
		}

		private void HandleChoked()
		{
			int num = kcp.rcv_queue.Count + kcp.snd_queue.Count + kcp.rcv_buf.Count + kcp.snd_buf.Count;
			if (num >= 10000)
			{
				Log.Warning("KCP: disconnecting connection because it can't process data fast enough.\n" + string.Format("Queue total {0}>{1}. rcv_queue={2} snd_queue={3} rcv_buf={4} snd_buf={5}\n", num, 10000, kcp.rcv_queue.Count, kcp.snd_queue.Count, kcp.rcv_buf.Count, kcp.snd_buf.Count) + "* Try to Enable NoDelay, decrease INTERVAL, disable Congestion Window (= enable NOCWND!), increase SEND/RECV WINDOW or compress data.\n* Or perhaps the network is simply too slow on our end, or on the other end.\n");
				kcp.snd_queue.Clear();
				Disconnect();
			}
		}

		private bool ReceiveNextReliable(out KcpHeader header, out ArraySegment<byte> message)
		{
			int num = kcp.PeekSize();
			if (num > 0)
			{
				if (num <= kcpMessageBuffer.Length)
				{
					int num2 = kcp.Receive(kcpMessageBuffer, num);
					if (num2 >= 0)
					{
						header = (KcpHeader)kcpMessageBuffer[0];
						message = new ArraySegment<byte>(kcpMessageBuffer, 1, num - 1);
						lastReceiveTime = (uint)refTime.ElapsedMilliseconds;
						return true;
					}
					Log.Warning(string.Format("Receive failed with error={0}. closing connection.", num2));
					Disconnect();
				}
				else
				{
					Log.Warning(string.Format("KCP: possible allocation attack for msgSize {0} > buffer {1}. Disconnecting the connection.", num, kcpMessageBuffer.Length));
					Disconnect();
				}
			}
			message = default(ArraySegment<byte>);
			header = KcpHeader.Disconnect;
			return false;
		}

		private void TickIncoming_Connected(uint time)
		{
			HandleTimeout(time);
			HandleDeadLink();
			HandlePing(time);
			HandleChoked();
			KcpHeader header;
			ArraySegment<byte> message;
			if (!ReceiveNextReliable(out header, out message))
			{
				return;
			}
			switch (header)
			{
			case KcpHeader.Handshake:
			{
				Log.Info("KCP: received handshake");
				state = KcpState.Authenticated;
				Action onAuthenticated = OnAuthenticated;
				if (onAuthenticated != null)
				{
					onAuthenticated();
				}
				break;
			}
			case KcpHeader.Data:
			case KcpHeader.Disconnect:
				Log.Warning(string.Format("KCP: received invalid header {0} while Connected. Disconnecting the connection.", header));
				Disconnect();
				break;
			case KcpHeader.Ping:
				break;
			}
		}

		private void TickIncoming_Authenticated(uint time)
		{
			HandleTimeout(time);
			HandleDeadLink();
			HandlePing(time);
			HandleChoked();
			KcpHeader header;
			ArraySegment<byte> message;
			while (!paused && ReceiveNextReliable(out header, out message))
			{
				switch (header)
				{
				case KcpHeader.Handshake:
					Log.Warning(string.Format("KCP: received invalid header {0} while Authenticated. Disconnecting the connection.", header));
					Disconnect();
					break;
				case KcpHeader.Data:
					if (message.Count > 0)
					{
						Action<ArraySegment<byte>> onData = OnData;
						if (onData != null)
						{
							onData(message);
						}
					}
					else
					{
						Log.Warning("KCP: received empty Data message while Authenticated. Disconnecting the connection.");
						Disconnect();
					}
					break;
				case KcpHeader.Disconnect:
					Log.Info("KCP: received disconnect message");
					Disconnect();
					break;
				}
			}
		}

		public void TickIncoming()
		{
			uint time = (uint)refTime.ElapsedMilliseconds;
			try
			{
				switch (state)
				{
				case KcpState.Connected:
					TickIncoming_Connected(time);
					break;
				case KcpState.Authenticated:
					TickIncoming_Authenticated(time);
					break;
				case KcpState.Disconnected:
					break;
				}
			}
			catch (SocketException arg)
			{
				Log.Info(string.Format("KCP Connection: Disconnecting because {0}. This is fine.", arg));
				Disconnect();
			}
			catch (ObjectDisposedException arg2)
			{
				Log.Info(string.Format("KCP Connection: Disconnecting because {0}. This is fine.", arg2));
				Disconnect();
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
				Disconnect();
			}
		}

		public void TickOutgoing()
		{
			uint currentTimeMilliSeconds = (uint)refTime.ElapsedMilliseconds;
			try
			{
				switch (state)
				{
				case KcpState.Connected:
				case KcpState.Authenticated:
					kcp.Update(currentTimeMilliSeconds);
					break;
				}
			}
			catch (SocketException arg)
			{
				Log.Info(string.Format("KCP Connection: Disconnecting because {0}. This is fine.", arg));
				Disconnect();
			}
			catch (ObjectDisposedException arg2)
			{
				Log.Info(string.Format("KCP Connection: Disconnecting because {0}. This is fine.", arg2));
				Disconnect();
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
				Disconnect();
			}
		}

		public void RawInput(byte[] buffer, int msgLength)
		{
			if (msgLength <= 0)
			{
				return;
			}
			byte b = buffer[0];
			switch (b)
			{
			case 1:
			{
				int num = kcp.Input(buffer, 1, msgLength - 1);
				if (num != 0)
				{
					Log.Warning(string.Format("Input failed with error={0} for buffer with length={1}", num, msgLength - 1));
				}
				break;
			}
			case 2:
				if (state == KcpState.Authenticated)
				{
					if (!paused)
					{
						ArraySegment<byte> obj = new ArraySegment<byte>(buffer, 1, msgLength - 1);
						Action<ArraySegment<byte>> onData = OnData;
						if (onData != null)
						{
							onData(obj);
						}
					}
					lastReceiveTime = (uint)refTime.ElapsedMilliseconds;
				}
				else
				{
					Log.Warning(string.Format("KCP: received unreliable message in state {0}. Disconnecting the connection.", state));
					Disconnect();
				}
				break;
			default:
				Log.Info(string.Format("Disconnecting connection because of invalid channel header: {0}", b));
				Disconnect();
				break;
			}
		}

		protected abstract void RawSend(byte[] data, int length);

		private void RawSendReliable(byte[] data, int length)
		{
			rawSendBuffer[0] = 1;
			Buffer.BlockCopy(data, 0, rawSendBuffer, 1, length);
			RawSend(rawSendBuffer, length + 1);
		}

		private void SendReliable(KcpHeader header, ArraySegment<byte> content)
		{
			if (1 + content.Count <= kcpSendBuffer.Length)
			{
				kcpSendBuffer[0] = (byte)header;
				if (content.Count > 0)
				{
					Buffer.BlockCopy(content.Array, content.Offset, kcpSendBuffer, 1, content.Count);
				}
				int num = kcp.Send(kcpSendBuffer, 0, 1 + content.Count);
				if (num < 0)
				{
					Log.Warning(string.Format("Send failed with error={0} for content with length={1}", num, content.Count));
				}
			}
			else
			{
				Log.Error(string.Format("Failed to send reliable message of size {0} because it's larger than ReliableMaxMessageSize={1}", content.Count, 149224));
			}
		}

		private void SendUnreliable(ArraySegment<byte> message)
		{
			if (message.Count <= 1199)
			{
				rawSendBuffer[0] = 2;
				Buffer.BlockCopy(message.Array, 0, rawSendBuffer, 1, message.Count);
				RawSend(rawSendBuffer, message.Count + 1);
			}
			else
			{
				Log.Error(string.Format("Failed to send unreliable message of size {0} because it's larger than UnreliableMaxMessageSize={1}", message.Count, 1199));
			}
		}

		public void SendHandshake()
		{
			Log.Info("KcpConnection: sending Handshake to other end!");
			SendReliable(KcpHeader.Handshake, default(ArraySegment<byte>));
		}

		public void SendData(ArraySegment<byte> data, KcpChannel channel)
		{
			if (data.Count == 0)
			{
				Log.Warning("KcpConnection: tried sending empty message. This should never happen. Disconnecting.");
				Disconnect();
				return;
			}
			switch (channel)
			{
			case KcpChannel.Reliable:
				SendReliable(KcpHeader.Data, data);
				break;
			case KcpChannel.Unreliable:
				SendUnreliable(data);
				break;
			}
		}

		private void SendPing()
		{
			SendReliable(KcpHeader.Ping, default(ArraySegment<byte>));
		}

		private void SendDisconnect()
		{
			SendReliable(KcpHeader.Disconnect, default(ArraySegment<byte>));
		}

		protected virtual void Dispose()
		{
		}

		public void Disconnect()
		{
			if (state == KcpState.Disconnected)
			{
				return;
			}
			if (socket.Connected)
			{
				try
				{
					SendDisconnect();
					kcp.Flush();
				}
				catch (SocketException)
				{
				}
				catch (ObjectDisposedException)
				{
				}
			}
			Log.Info("KCP Connection: Disconnected.");
			state = KcpState.Disconnected;
			Action onDisconnected = OnDisconnected;
			if (onDisconnected != null)
			{
				onDisconnected();
			}
		}

		public EndPoint GetRemoteEndPoint()
		{
			return remoteEndPoint;
		}

		public void Pause()
		{
			paused = true;
		}

		public void Unpause()
		{
			paused = false;
			lastReceiveTime = (uint)refTime.ElapsedMilliseconds;
		}
	}
}
