using System;

namespace Mirror
{
	public class NetworkConnectionToServer : NetworkConnection
	{
		public override string address
		{
			get
			{
				return "";
			}
		}

		protected override void SendToTransport(ArraySegment<byte> segment, int channelId = 0)
		{
			Transport.activeTransport.ClientSend(segment, channelId);
		}

		public override void Disconnect()
		{
			isReady = false;
			NetworkClient.ready = false;
			Transport.activeTransport.ClientDisconnect();
		}
	}
}
