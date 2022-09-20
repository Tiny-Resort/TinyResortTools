using System;

namespace Mirror
{
	public static class Writer<T>
	{
		public static Action<NetworkWriter, T> write;
	}
}
