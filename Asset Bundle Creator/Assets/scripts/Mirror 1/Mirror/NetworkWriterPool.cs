using System;
using System.Runtime.CompilerServices;

namespace Mirror
{
	public static class NetworkWriterPool
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			internal PooledNetworkWriter _003C_002Ecctor_003Eb__3_0()
			{
				return new PooledNetworkWriter();
			}
		}

		private static readonly Pool<PooledNetworkWriter> Pool = new Pool<PooledNetworkWriter>(_003C_003Ec._003C_003E9._003C_002Ecctor_003Eb__3_0, 1000);

		public static PooledNetworkWriter GetWriter()
		{
			PooledNetworkWriter pooledNetworkWriter = Pool.Take();
			pooledNetworkWriter.Reset();
			return pooledNetworkWriter;
		}

		public static void Recycle(PooledNetworkWriter writer)
		{
			Pool.Return(writer);
		}
	}
}
