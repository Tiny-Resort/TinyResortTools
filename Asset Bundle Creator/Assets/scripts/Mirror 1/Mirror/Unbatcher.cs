using System;
using System.Collections.Generic;

namespace Mirror
{
	public class Unbatcher
	{
		private Queue<PooledNetworkWriter> batches = new Queue<PooledNetworkWriter>();

		private NetworkReader reader = new NetworkReader(new byte[0]);

		private double readerRemoteTimeStamp;

		private void StartReadingBatch(PooledNetworkWriter batch)
		{
			reader.SetBuffer(batch.ToArraySegment());
			readerRemoteTimeStamp = reader.ReadDouble();
		}

		public bool AddBatch(ArraySegment<byte> batch)
		{
			if (batch.Count < 8)
			{
				return false;
			}
			PooledNetworkWriter writer = NetworkWriterPool.GetWriter();
			writer.WriteBytes(batch.Array, batch.Offset, batch.Count);
			if (batches.Count == 0)
			{
				StartReadingBatch(writer);
			}
			batches.Enqueue(writer);
			return true;
		}

		public bool GetNextMessage(out NetworkReader message, out double remoteTimeStamp)
		{
			message = null;
			if (batches.Count == 0)
			{
				remoteTimeStamp = 0.0;
				return false;
			}
			if (reader.Length == 0)
			{
				remoteTimeStamp = 0.0;
				return false;
			}
			if (reader.Remaining == 0)
			{
				NetworkWriterPool.Recycle(batches.Dequeue());
				if (batches.Count <= 0)
				{
					remoteTimeStamp = 0.0;
					return false;
				}
				PooledNetworkWriter batch = batches.Peek();
				StartReadingBatch(batch);
			}
			remoteTimeStamp = readerRemoteTimeStamp;
			message = reader;
			return true;
		}
	}
}
