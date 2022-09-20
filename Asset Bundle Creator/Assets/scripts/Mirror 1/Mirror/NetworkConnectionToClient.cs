using System;

namespace Mirror
{
	public class NetworkConnectionToClient : NetworkConnection
	{
		public Unbatcher unbatcher = new Unbatcher();

		public override string address
		{
			get
			{
				return Transport.activeTransport.ServerGetClientAddress(connectionId);
			}
		}

		public NetworkConnectionToClient(int networkConnectionId)
			: base(networkConnectionId)
		{
		}

		protected override void SendToTransport(ArraySegment<byte> segment, int channelId = 0)
		{
			Transport.activeTransport.ServerSend(connectionId, segment, channelId);
		}

		public override void Disconnect()
		{
			isReady = false;
			Transport.activeTransport.ServerDisconnect(connectionId);
		}
	}
}
