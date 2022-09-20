using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace Mirror.Discovery
{
	public struct ServerResponse : NetworkMessage
	{
		public Uri uri;

		public long serverId;

		public IPEndPoint EndPoint
		{
			get;
			set; }
	}
}
