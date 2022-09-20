using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Mirror.SimpleWeb
{
	internal class BufferBucket : IBufferOwner
	{
		public readonly int arraySize;

		private readonly ConcurrentQueue<ArrayBuffer> buffers;

		internal int _current;

		public BufferBucket(int arraySize)
		{
			this.arraySize = arraySize;
			buffers = new ConcurrentQueue<ArrayBuffer>();
		}

		public ArrayBuffer Take()
		{
			ArrayBuffer result;
			if (buffers.TryDequeue(out result))
			{
				return result;
			}
			return new ArrayBuffer(this, arraySize);
		}

		public void Return(ArrayBuffer buffer)
		{
			buffers.Enqueue(buffer);
		}

		[Conditional("DEBUG")]
		private void IncrementCreated()
		{
			Interlocked.Increment(ref _current);
		}

		[Conditional("DEBUG")]
		private void DecrementCreated()
		{
			Interlocked.Decrement(ref _current);
		}
	}
}
