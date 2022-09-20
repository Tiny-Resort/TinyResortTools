using System.Net;
using System.Net.Sockets;

namespace kcp2k
{
	public class KcpServerConnection : KcpConnection
	{
		public KcpServerConnection(Socket socket, EndPoint remoteEndPoint, bool noDelay, uint interval = 100u, int fastResend = 0, bool congestionWindow = true, uint sendWindowSize = 32u, uint receiveWindowSize = 128u, int timeout = 10000)
		{
			base.socket = socket;
			base.remoteEndPoint = remoteEndPoint;
			SetupKcp(noDelay, interval, fastResend, congestionWindow, sendWindowSize, receiveWindowSize, timeout);
		}

		protected override void RawSend(byte[] data, int length)
		{
			socket.SendTo(data, 0, length, SocketFlags.None, remoteEndPoint);
		}
	}
}
