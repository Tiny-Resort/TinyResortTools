using System;
using System.Runtime.CompilerServices;

namespace Mirror
{
	public static class NetworkReaderPool
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			internal PooledNetworkReader _003C_002Ecctor_003Eb__4_0()
			{
				return new PooledNetworkReader(new byte[0]);
			}
		}

		private static readonly Pool<PooledNetworkReader> Pool = new Pool<PooledNetworkReader>(_003C_003Ec._003C_003E9._003C_002Ecctor_003Eb__4_0, 1000);

		public static PooledNetworkReader GetReader(byte[] bytes)
		{
			PooledNetworkReader pooledNetworkReader = Pool.Take();
			pooledNetworkReader.SetBuffer(bytes);
			return pooledNetworkReader;
		}

		public static PooledNetworkReader GetReader(ArraySegment<byte> segment)
		{
			PooledNetworkReader pooledNetworkReader = Pool.Take();
			pooledNetworkReader.SetBuffer(segment);
			return pooledNetworkReader;
		}

		public static void Recycle(PooledNetworkReader reader)
		{
			Pool.Return(reader);
		}
	}
}
