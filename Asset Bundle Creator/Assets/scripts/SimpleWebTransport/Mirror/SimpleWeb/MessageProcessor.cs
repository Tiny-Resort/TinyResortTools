using System.IO;
using System.Runtime.CompilerServices;

namespace Mirror.SimpleWeb
{
	public static class MessageProcessor
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static byte FirstLengthByte(byte[] buffer)
		{
			return (byte)(buffer[1] & 0x7Fu);
		}

		public static bool NeedToReadShortLength(byte[] buffer)
		{
			return FirstLengthByte(buffer) >= 126;
		}

		public static int GetOpcode(byte[] buffer)
		{
			return buffer[0] & 0xF;
		}

		public static int GetPayloadLength(byte[] buffer)
		{
			byte lenByte = FirstLengthByte(buffer);
			return GetMessageLength(buffer, 0, lenByte);
		}

		public static void ValidateHeader(byte[] buffer, int maxLength, bool expectMask)
		{
			bool finished = (buffer[0] & 0x80) != 0;
			bool hasMask = (buffer[1] & 0x80) != 0;
			int opcode = buffer[0] & 0xF;
			byte lenByte = FirstLengthByte(buffer);
			ThrowIfNotFinished(finished);
			ThrowIfMaskNotExpected(hasMask, expectMask);
			ThrowIfBadOpCode(opcode);
			int messageLength = GetMessageLength(buffer, 0, lenByte);
			ThrowIfLengthZero(messageLength);
			ThrowIfMsgLengthTooLong(messageLength, maxLength);
		}

		public static void ToggleMask(byte[] src, int sourceOffset, int messageLength, byte[] maskBuffer, int maskOffset)
		{
			ToggleMask(src, sourceOffset, src, sourceOffset, messageLength, maskBuffer, maskOffset);
		}

		public static void ToggleMask(byte[] src, int sourceOffset, ArrayBuffer dst, int messageLength, byte[] maskBuffer, int maskOffset)
		{
			ToggleMask(src, sourceOffset, dst.array, 0, messageLength, maskBuffer, maskOffset);
			dst.count = messageLength;
		}

		public static void ToggleMask(byte[] src, int srcOffset, byte[] dst, int dstOffset, int messageLength, byte[] maskBuffer, int maskOffset)
		{
			for (int i = 0; i < messageLength; i++)
			{
				byte b = maskBuffer[maskOffset + i % 4];
				dst[dstOffset + i] = (byte)(src[srcOffset + i] ^ b);
			}
		}

		private static int GetMessageLength(byte[] buffer, int offset, byte lenByte)
		{
			switch (lenByte)
			{
			case 126:
				return (ushort)((ushort)(0 | (ushort)(buffer[offset + 2] << 8)) | buffer[offset + 3]);
			case 127:
				throw new InvalidDataException("Max length is longer than allowed in a single message");
			default:
				return lenByte;
			}
		}

		private static void ThrowIfNotFinished(bool finished)
		{
			if (!finished)
			{
				throw new InvalidDataException("Full message should have been sent, if the full message wasn't sent it wasn't sent from this trasnport");
			}
		}

		private static void ThrowIfMaskNotExpected(bool hasMask, bool expectMask)
		{
			if (hasMask != expectMask)
			{
				throw new InvalidDataException(string.Format("Message expected mask to be {0} but was {1}", expectMask, hasMask));
			}
		}

		private static void ThrowIfBadOpCode(int opcode)
		{
			if (opcode != 2 && opcode != 8)
			{
				throw new InvalidDataException("Expected opcode to be binary or close");
			}
		}

		private static void ThrowIfLengthZero(int msglen)
		{
			if (msglen == 0)
			{
				throw new InvalidDataException("Message length was zero");
			}
		}

		private static void ThrowIfMsgLengthTooLong(int msglen, int maxLength)
		{
			if (msglen > maxLength)
			{
				throw new InvalidDataException("Message length is greater than max length");
			}
		}
	}
}
