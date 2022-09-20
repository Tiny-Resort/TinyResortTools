using System.Net;
using System.Net.Sockets;

namespace kcp2k
{
	public class KcpClientConnection : KcpConnection
	{
		private readonly byte[] rawReceiveBuffer = new byte[1200];

		public static bool ResolveHostname(string hostname, out IPAddress[] addresses)
		{
			try
			{
				addresses = Dns.GetHostAddresses(hostname);
				return addresses.Length >= 1;
			}
			catch (SocketException)
			{
				Log.Info("Failed to resolve host: " + hostname);
				addresses = null;
				return false;
			}
		}

		protected virtual void CreateRemoteEndPoint(IPAddress[] addresses, ushort port)
		{
			remoteEndPoint = new IPEndPoint(addresses[0], port);
		}

		protected virtual int ReceiveFrom(byte[] buffer)
		{
			return socket.ReceiveFrom(buffer, ref remoteEndPoint);
		}

		public void Connect(string host, ushort port, bool noDelay, uint interval = 100u, int fastResend = 0, bool congestionWindow = true, uint sendWindowSize = 32u, uint receiveWindowSize = 128u, int timeout = 10000)
		{
			Log.Info(string.Format("KcpClient: connect to {0}:{1}", host, port));
			IPAddress[] addresses;
			if (ResolveHostname(host, out addresses))
			{
				CreateRemoteEndPoint(addresses, port);
				socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
				socket.Connect(remoteEndPoint);
				SetupKcp(noDelay, interval, fastResend, congestionWindow, sendWindowSize, receiveWindowSize, timeout);
				SendHandshake();
				RawReceive();
			}
			else
			{
				OnDisconnected();
			}
		}

		public void RawReceive()
		{
			try
			{
				if (socket == null)
				{
					return;
				}
				while (socket.Poll(0, SelectMode.SelectRead))
				{
					int num = ReceiveFrom(rawReceiveBuffer);
					if (num <= rawReceiveBuffer.Length)
					{
						RawInput(rawReceiveBuffer, num);
						continue;
					}
					Log.Error(string.Format("KCP ClientConnection: message of size {0} does not fit into buffer of size {1}. The excess was silently dropped. Disconnecting.", num, rawReceiveBuffer.Length));
					Disconnect();
				}
			}
			catch (SocketException)
			{
			}
		}

		protected override void Dispose()
		{
			socket.Close();
			socket = null;
		}

		protected override void RawSend(byte[] data, int length)
		{
			socket.Send(data, length, SocketFlags.None);
		}
	}
}
