using System.Net;
using System.Net.Sockets;

namespace WhereAllocation
{
	public static class Extensions
	{
		public static int ReceiveFrom_NonAlloc(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags, IPEndPointNonAlloc remoteEndPoint)
		{
			EndPoint remoteEP = remoteEndPoint;
			return socket.ReceiveFrom(buffer, offset, size, socketFlags, ref remoteEP);
		}

		public static int ReceiveFrom_NonAlloc(this Socket socket, byte[] buffer, IPEndPointNonAlloc remoteEndPoint)
		{
			EndPoint remoteEP = remoteEndPoint;
			return socket.ReceiveFrom(buffer, ref remoteEP);
		}

		public static int SendTo_NonAlloc(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags, IPEndPointNonAlloc remoteEndPoint)
		{
			return socket.SendTo(buffer, offset, size, socketFlags, remoteEndPoint);
		}
	}
}
