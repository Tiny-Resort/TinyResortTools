using System;

namespace Mirror
{
	public static class Reader<T>
	{
		public static Func<NetworkReader, T> read;
	}
}
