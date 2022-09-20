using System;

namespace Mirror.SimpleWeb
{
	internal static class SimpleWebJSLib
	{
		internal static bool IsConnected(int index)
		{
			throw new NotSupportedException();
		}

		internal static int Connect(string address, Action<int> openCallback, Action<int> closeCallBack, Action<int, IntPtr, int> messageCallback, Action<int> errorCallback)
		{
			throw new NotSupportedException();
		}

		internal static void Disconnect(int index)
		{
			throw new NotSupportedException();
		}

		internal static bool Send(int index, byte[] array, int offset, int length)
		{
			throw new NotSupportedException();
		}
	}
}
