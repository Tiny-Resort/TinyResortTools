namespace Mirror
{
	public struct NetworkPingMessage : NetworkMessage
	{
		public double clientTime;

		public NetworkPingMessage(double value)
		{
			clientTime = value;
		}
	}
}
