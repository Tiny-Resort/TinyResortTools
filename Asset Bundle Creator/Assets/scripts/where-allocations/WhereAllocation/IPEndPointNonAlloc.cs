using System;
using System.Net;
using System.Net.Sockets;

namespace WhereAllocation
{
	public class IPEndPointNonAlloc : IPEndPoint
	{
		public SocketAddress temp;

		public IPEndPointNonAlloc(long address, int port)
			: base(address, port)
		{
			temp = base.Serialize();
		}

		public IPEndPointNonAlloc(IPAddress address, int port)
			: base(address, port)
		{
			temp = base.Serialize();
		}

		public override SocketAddress Serialize()
		{
			return temp;
		}

		public override EndPoint Create(SocketAddress socketAddress)
		{
			if (socketAddress.Family != AddressFamily)
			{
				throw new ArgumentException(string.Format("Unsupported socketAddress.AddressFamily: {0}. Expected: {1}", socketAddress.Family, AddressFamily));
			}
			if (socketAddress.Size < 8)
			{
				throw new ArgumentException(string.Format("Unsupported socketAddress.Size: {0}. Expected: <8", socketAddress.Size));
			}
			if (socketAddress != temp)
			{
				temp = socketAddress;
				temp[0]++;
				temp[0]--;
				if (temp.GetHashCode() == 0)
				{
					throw new Exception("SocketAddress GetHashCode() is 0 after ReceiveFrom. Does the m_changed trick not work anymore?");
				}
			}
			return this;
		}

		public override int GetHashCode()
		{
			return temp.GetHashCode();
		}

		public IPEndPoint DeepCopyIPEndPoint()
		{
			IPAddress address;
			if (temp.Family == AddressFamily.InterNetworkV6)
			{
				address = IPAddress.IPv6Any;
			}
			else
			{
				if (temp.Family != AddressFamily.InterNetwork)
				{
					throw new Exception(string.Format("Unexpected SocketAddress family: {0}", temp.Family));
				}
				address = IPAddress.Any;
			}
			return (IPEndPoint)new IPEndPoint(address, 0).Create(temp);
		}
	}
}
