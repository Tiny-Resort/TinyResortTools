using System;
using System.Net;
using System.Net.Sockets;
using WhereAllocation;

namespace kcp2k
{
	public class KcpServerNonAlloc : KcpServer
	{
		private IPEndPointNonAlloc reusableClientEP;

		public KcpServerNonAlloc(Action<int> OnConnected, Action<int, ArraySegment<byte>> OnData, Action<int> OnDisconnected, bool DualMode, bool NoDelay, uint Interval, int FastResend = 0, bool CongestionWindow = true, uint SendWindowSize = 32u, uint ReceiveWindowSize = 128u, int Timeout = 10000)
			: base(OnConnected, OnData, OnDisconnected, DualMode, NoDelay, Interval, FastResend, CongestionWindow, SendWindowSize, ReceiveWindowSize, Timeout)
		{
			reusableClientEP = (DualMode ? new IPEndPointNonAlloc(IPAddress.IPv6Any, 0) : new IPEndPointNonAlloc(IPAddress.Any, 0));
		}

		protected override int ReceiveFrom(byte[] buffer, out int connectionHash)
		{
			int result = socket.ReceiveFrom_NonAlloc(buffer, 0, buffer.Length, SocketFlags.None, reusableClientEP);
			SocketAddress temp = reusableClientEP.temp;
			connectionHash = temp.GetHashCode();
			return result;
		}

		protected override KcpServerConnection CreateConnection()
		{
			IPEndPoint iPEndPoint = reusableClientEP.DeepCopyIPEndPoint();
			IPEndPointNonAlloc reusableSendEndPoint = new IPEndPointNonAlloc(iPEndPoint.Address, iPEndPoint.Port);
			return new KcpServerConnectionNonAlloc(socket, iPEndPoint, reusableSendEndPoint, NoDelay, Interval, FastResend, CongestionWindow, SendWindowSize, ReceiveWindowSize, Timeout);
		}
	}
}
