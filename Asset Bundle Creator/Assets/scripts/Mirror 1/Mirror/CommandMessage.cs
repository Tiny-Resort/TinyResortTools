using System;

namespace Mirror
{
	public struct CommandMessage : NetworkMessage
	{
		public uint netId;

		public int componentIndex;

		public int functionHash;

		public ArraySegment<byte> payload;
	}
}
