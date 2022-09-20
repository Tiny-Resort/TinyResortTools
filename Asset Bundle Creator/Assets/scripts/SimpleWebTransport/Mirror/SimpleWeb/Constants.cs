using System.Text;

namespace Mirror.SimpleWeb
{
	internal static class Constants
	{
		public const int HeaderSize = 4;

		public const int HeaderMinSize = 2;

		public const int ShortLength = 2;

		public const int MaskSize = 4;

		public const int BytePayloadLength = 125;

		public const int UshortPayloadLength = 126;

		public const int UlongPayloadLength = 127;

		public const string HandshakeGUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		public static readonly int HandshakeGUIDLength = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11".Length;

		public static readonly byte[] HandshakeGUIDBytes = Encoding.ASCII.GetBytes("258EAFA5-E914-47DA-95CA-C5AB0DC85B11");

		public static readonly byte[] endOfHandshake = new byte[4] { 13, 10, 13, 10 };
	}
}
