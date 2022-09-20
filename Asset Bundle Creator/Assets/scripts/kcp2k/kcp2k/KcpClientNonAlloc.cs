using System;

namespace kcp2k
{
	public class KcpClientNonAlloc : KcpClient
	{
		public KcpClientNonAlloc(Action OnConnected, Action<ArraySegment<byte>> OnData, Action OnDisconnected)
			: base(OnConnected, OnData, OnDisconnected)
		{
		}

		protected override KcpClientConnection CreateConnection()
		{
			return new KcpClientConnectionNonAlloc();
		}
	}
}
