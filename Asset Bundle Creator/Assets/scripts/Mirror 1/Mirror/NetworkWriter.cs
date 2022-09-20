using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirror
{
	public class NetworkWriter
	{
		public const int MaxStringLength = 32768;

		private byte[] buffer = new byte[1500];

		public int Position;

		public void Reset()
		{
			Position = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureCapacity(int value)
		{
			if (buffer.Length < value)
			{
				int newSize = Math.Max(value, buffer.Length * 2);
				Array.Resize(ref buffer, newSize);
			}
		}

		public byte[] ToArray()
		{
			byte[] array = new byte[Position];
			Array.ConstrainedCopy(buffer, 0, array, 0, Position);
			return array;
		}

		public ArraySegment<byte> ToArraySegment()
		{
			return new ArraySegment<byte>(buffer, 0, Position);
		}

		public void WriteByte(byte value)
		{
			EnsureCapacity(Position + 1);
			buffer[Position++] = value;
		}

		public void WriteBytes(byte[] buffer, int offset, int count)
		{
			EnsureCapacity(Position + count);
			Array.ConstrainedCopy(buffer, offset, this.buffer, Position, count);
			Position += count;
		}

		public void Write<T>(T value)
		{
			Action<NetworkWriter, T> write = Writer<T>.write;
			if (write == null)
			{
				Debug.LogError(string.Format("No writer found for {0}. Use a type supported by Mirror or define a custom writer", typeof(T)));
			}
			else
			{
				write(this, value);
			}
		}
	}
}
