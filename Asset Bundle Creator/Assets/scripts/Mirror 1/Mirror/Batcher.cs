using System;
using System.Collections.Generic;

namespace Mirror
{
	public class Batcher
	{
		private readonly int threshold;

		public const int HeaderSize = 8;

		private Queue<PooledNetworkWriter> messages = new Queue<PooledNetworkWriter>();

		public Batcher(int threshold)
		{
			this.threshold = threshold;
		}

		public void AddMessage(ArraySegment<byte> message)
		{
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteBytes(message.Array, message.Offset, message.Count);
			messages.Enqueue(writer);
		}

		public bool MakeNextBatch(NetworkWriter writer, double timeStamp)
		{
			if (messages.Count == 0)
			{
				return false;
			}
			if (writer.Position != 0)
			{
				throw new ArgumentException("MakeNextBatch needs a fresh writer!");
			}
			writer.WriteDouble(timeStamp);
			do
			{
				PooledNetworkWriter pooledNetworkWriter = messages.Dequeue();
				ArraySegment<byte> arraySegment = pooledNetworkWriter.ToArraySegment();
				writer.WriteBytes(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
				NetworkWriterPool.Recycle(pooledNetworkWriter);
			}
			while (messages.Count > 0 && writer.Position + messages.Peek().Position <= threshold);
			return true;
		}
	}
}
