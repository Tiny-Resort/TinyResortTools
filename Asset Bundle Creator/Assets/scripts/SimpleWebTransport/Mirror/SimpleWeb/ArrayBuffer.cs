using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Mirror.SimpleWeb
{
	public sealed class ArrayBuffer : IDisposable
	{
		private readonly IBufferOwner owner;

		public readonly byte[] array;

		internal int count;

		private int releasesRequired;

		public void SetReleasesRequired(int required)
		{
			releasesRequired = required;
		}

		public ArrayBuffer(IBufferOwner owner, int size)
		{
			this.owner = owner;
			array = new byte[size];
		}

		public void Release()
		{
			if (Interlocked.Decrement(ref releasesRequired) <= 0)
			{
				count = 0;
				owner.Return(this);
			}
		}

		public void Dispose()
		{
			Release();
		}

		public void CopyTo(byte[] target, int offset)
		{
			if (count > target.Length + offset)
			{
				throw new ArgumentException("count was greater than target.length", "target");
			}
			Buffer.BlockCopy(array, 0, target, offset, count);
		}

		public void CopyFrom(ArraySegment<byte> segment)
		{
			CopyFrom(segment.Array, segment.Offset, segment.Count);
		}

		public void CopyFrom(byte[] source, int offset, int length)
		{
			if (length > array.Length)
			{
				throw new ArgumentException("length was greater than array.length", "length");
			}
			count = length;
			Buffer.BlockCopy(source, offset, array, 0, length);
		}

		public void CopyFrom(IntPtr bufferPtr, int length)
		{
			if (length > array.Length)
			{
				throw new ArgumentException("length was greater than array.length", "length");
			}
			count = length;
			Marshal.Copy(bufferPtr, array, 0, length);
		}

		public ArraySegment<byte> ToSegment()
		{
			return new ArraySegment<byte>(array, 0, count);
		}

		[Conditional("UNITY_ASSERTIONS")]
		internal void Validate(int arraySize)
		{
			int num = array.Length;
		}
	}
}
