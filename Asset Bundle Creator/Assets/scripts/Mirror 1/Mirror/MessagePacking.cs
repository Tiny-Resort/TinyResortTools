using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirror
{
	public static class MessagePacking
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass6_0<T, C> where T : struct, NetworkMessage where C : NetworkConnection
		{
			public bool requireAuthentication;

			public Action<C, T> handler;

			internal void _003CWrapHandler_003Eb__0(NetworkConnection conn, NetworkReader reader, int channelId)
			{
				T val = default(T);
				int position = reader.Position;
				try
				{
					if (requireAuthentication && !conn.isAuthenticated)
					{
						Debug.LogWarning(string.Format("Closing connection: {0}. Received message {1} that required authentication, but the user has not authenticated yet", conn, typeof(T)));
						conn.Disconnect();
						return;
					}
					val = reader.Read<T>();
				}
				catch (Exception arg)
				{
					Debug.LogError(string.Format("Closed connection: {0}. This can happen if the other side accidentally (or an attacker intentionally) sent invalid data. Reason: {1}", conn, arg));
					conn.Disconnect();
					return;
				}
				finally
				{
					int position2 = reader.Position;
					NetworkDiagnostics.OnReceive(val, channelId, position2 - position);
				}
				try
				{
					handler((C)conn, val);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Disconnecting connId={0} to prevent exploits from an Exception in MessageHandler: {1} {2}\n{3}", conn.connectionId, ex.GetType().Name, ex.Message, ex.StackTrace));
				}
			}
		}

		public const int HeaderSize = 2;

		public static int MaxContentSize
		{
			get
			{
				return Transport.activeTransport.GetMaxPacketSize() - 2 - 8;
			}
		}

		public static ushort GetId<T>() where T : struct, NetworkMessage
		{
			return (ushort)((uint)typeof(T).FullName.GetStableHashCode() & 0xFFFFu);
		}

		public static void Pack<T>(T message, NetworkWriter writer) where T : struct, NetworkMessage
		{
			ushort id = GetId<T>();
			writer.WriteUShort(id);
			writer.Write(message);
		}

		public static bool Unpack(NetworkReader messageReader, out ushort msgType)
		{
			try
			{
				msgType = messageReader.ReadUShort();
				return true;
			}
			catch (EndOfStreamException)
			{
				msgType = 0;
				return false;
			}
		}

		internal static NetworkMessageDelegate WrapHandler<T, C>(Action<C, T> handler, bool requireAuthentication) where T : struct, NetworkMessage where C : NetworkConnection
		{
			_003C_003Ec__DisplayClass6_0<T, C> _003C_003Ec__DisplayClass6_ = new _003C_003Ec__DisplayClass6_0<T, C>();
			_003C_003Ec__DisplayClass6_.requireAuthentication = requireAuthentication;
			_003C_003Ec__DisplayClass6_.handler = handler;
			return _003C_003Ec__DisplayClass6_._003CWrapHandler_003Eb__0;
		}
	}
}
