namespace kcp2k
{
	public enum KcpHeader : byte
	{
		Handshake = 1,
		Ping = 2,
		Data = 3,
		Disconnect = 4
	}
}
