using System;
using System.Runtime.CompilerServices;

namespace Mirror
{
	public static class NetworkDiagnostics
	{
		public struct MessageInfo
		{
			public readonly NetworkMessage message;

			public readonly int channel;

			public readonly int bytes;

			public readonly int count;

			internal MessageInfo(NetworkMessage message, int channel, int bytes, int count)
			{
				this.message = message;
				this.channel = channel;
				this.bytes = bytes;
				this.count = count;
			}
		}

		public static event Action<MessageInfo> OutMessageEvent;

		public static event Action<MessageInfo> InMessageEvent;

		internal static void OnSend<T>(T message, int channel, int bytes, int count) where T : struct, NetworkMessage
		{
			if (count > 0 && NetworkDiagnostics.OutMessageEvent != null)
			{
				MessageInfo obj = new MessageInfo(message, channel, bytes, count);
				Action<MessageInfo> outMessageEvent = NetworkDiagnostics.OutMessageEvent;
				if (outMessageEvent != null)
				{
					outMessageEvent(obj);
				}
			}
		}

		internal static void OnReceive<T>(T message, int channel, int bytes) where T : struct, NetworkMessage
		{
			if (NetworkDiagnostics.InMessageEvent != null)
			{
				MessageInfo obj = new MessageInfo(message, channel, bytes, 1);
				Action<MessageInfo> inMessageEvent = NetworkDiagnostics.InMessageEvent;
				if (inMessageEvent != null)
				{
					inMessageEvent(obj);
				}
			}
		}
	}
}
