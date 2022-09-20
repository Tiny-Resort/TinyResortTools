using System.Net;
using WhereAllocation;

namespace kcp2k
{
	public class KcpClientConnectionNonAlloc : KcpClientConnection
	{
		private IPEndPointNonAlloc reusableEP;

		protected override void CreateRemoteEndPoint(IPAddress[] addresses, ushort port)
		{
			reusableEP = new IPEndPointNonAlloc(addresses[0], port);
			base.CreateRemoteEndPoint(addresses, port);
		}

		protected override int ReceiveFrom(byte[] buffer)
		{
			return socket.ReceiveFrom_NonAlloc(buffer, reusableEP);
		}
	}
}
