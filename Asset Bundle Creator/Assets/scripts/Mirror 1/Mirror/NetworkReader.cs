using System;
using System.IO;
using UnityEngine;

namespace Mirror
{
	public class NetworkReader
	{
		private ArraySegment<byte> buffer;

		public int Position;

		public int Length
		{
			get
			{
				return buffer.Count;
			}
		}

		public int Remaining
		{
			get
			{
				return Length - Position;
			}
		}

		public NetworkReader(byte[] bytes)
		{
			buffer = new ArraySegment<byte>(bytes);
		}

		public NetworkReader(ArraySegment<byte> segment)
		{
			buffer = segment;
		}

		public void SetBuffer(byte[] bytes)
		{
			buffer = new ArraySegment<byte>(bytes);
			Position = 0;
		}

		public void SetBuffer(ArraySegment<byte> segment)
		{
			buffer = segment;
			Position = 0;
		}

		public byte ReadByte()
		{
			if (Position + 1 > buffer.Count)
			{
				throw new EndOfStreamException("ReadByte out of range:" + ToString());
			}
			return buffer.Array[buffer.Offset + Position++];
		}

		public byte[] ReadBytes(byte[] bytes, int count)
		{
			if (count > bytes.Length)
			{
				throw new EndOfStreamException("ReadBytes can't read " + count + " + bytes because the passed byte[] only has length " + bytes.Length);
			}
			ArraySegment<byte> arraySegment = ReadBytesSegment(count);
			Array.Copy(arraySegment.Array, arraySegment.Offset, bytes, 0, count);
			return bytes;
		}

		public ArraySegment<byte> ReadBytesSegment(int count)
		{
			if (Position + count > buffer.Count)
			{
				throw new EndOfStreamException("ReadBytesSegment can't read " + count + " bytes because it would read past the end of the stream. " + ToString());
			}
			ArraySegment<byte> result = new ArraySegment<byte>(buffer.Array, buffer.Offset + Position, count);
			Position += count;
			return result;
		}

		public override string ToString()
		{
			return string.Format("NetworkReader pos={0} len={1} buffer={2}", Position, Length, BitConverter.ToString(buffer.Array, buffer.Offset, buffer.Count));
		}

		public T Read<T>()
		{
			Func<NetworkReader, T> read = Reader<T>.read;
			if (read == null)
			{
				Debug.LogError(string.Format("No reader found for {0}. Use a type supported by Mirror or define a custom reader", typeof(T)));
				return default(T);
			}
			return read(this);
		}
	}
}
