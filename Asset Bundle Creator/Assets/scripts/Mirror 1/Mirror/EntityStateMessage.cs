using System;

namespace Mirror
{
	public struct EntityStateMessage : NetworkMessage
	{
		public uint netId;

		public ArraySegment<byte> payload;
	}
}
