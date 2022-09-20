using System;

namespace Mirror
{
	public sealed class PooledNetworkReader : NetworkReader, IDisposable
	{
		internal PooledNetworkReader(byte[] bytes)
			: base(bytes)
		{
		}

		internal PooledNetworkReader(ArraySegment<byte> segment)
			: base(segment)
		{
		}

		public void Dispose()
		{
			NetworkReaderPool.Recycle(this);
		}
	}
}
