using System;

namespace Mirror
{
	public sealed class PooledNetworkWriter : NetworkWriter, IDisposable
	{
		public void Dispose()
		{
			NetworkWriterPool.Recycle(this);
		}
	}
}
