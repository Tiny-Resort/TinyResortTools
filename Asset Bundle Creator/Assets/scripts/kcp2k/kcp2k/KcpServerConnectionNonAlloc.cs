using System.Net;
using System.Net.Sockets;
using WhereAllocation;

namespace kcp2k
{
	public class KcpServerConnectionNonAlloc : KcpServerConnection
	{
		private IPEndPointNonAlloc reusableSendEndPoint;

		public KcpServerConnectionNonAlloc(Socket socket, EndPoint remoteEndpoint, IPEndPointNonAlloc reusableSendEndPoint, bool noDelay, uint interval = 100u, int fastResend = 0, bool congestionWindow = true, uint sendWindowSize = 32u, uint receiveWindowSize = 128u, int timeout = 10000)
			: base(socket, remoteEndpoint, noDelay, interval, fastResend, congestionWindow, sendWindowSize, receiveWindowSize, timeout)
		{
			this.reusableSendEndPoint = reusableSendEndPoint;
		}

		protected override void RawSend(byte[] data, int length)
		{
			socket.SendTo_NonAlloc(data, 0, length, SocketFlags.None, reusableSendEndPoint);
		}
	}
}
