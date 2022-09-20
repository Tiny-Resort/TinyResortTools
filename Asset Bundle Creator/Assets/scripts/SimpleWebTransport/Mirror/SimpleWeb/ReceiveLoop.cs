using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Profiling;

namespace Mirror.SimpleWeb
{
	internal static class ReceiveLoop
	{
		public struct Config
		{
			public readonly Connection conn;

			public readonly int maxMessageSize;

			public readonly bool expectMask;

			public readonly ConcurrentQueue<Message> queue;

			public readonly BufferPool bufferPool;

			public Config(Connection conn, int maxMessageSize, bool expectMask, ConcurrentQueue<Message> queue, BufferPool bufferPool)
			{
				if (conn == null)
				{
					throw new ArgumentNullException("conn");
				}
				this.conn = conn;
				this.maxMessageSize = maxMessageSize;
				this.expectMask = expectMask;
				if (queue == null)
				{
					throw new ArgumentNullException("queue");
				}
				this.queue = queue;
				if (bufferPool == null)
				{
					throw new ArgumentNullException("bufferPool");
				}
				this.bufferPool = bufferPool;
			}

			public void Deconstruct(out Connection conn, out int maxMessageSize, out bool expectMask, out ConcurrentQueue<Message> queue, out BufferPool bufferPool)
			{
				conn = this.conn;
				maxMessageSize = this.maxMessageSize;
				expectMask = this.expectMask;
				queue = this.queue;
				bufferPool = this.bufferPool;
			}
		}

		public static void Loop(Config config)
		{
			Config config2 = config;
			Connection conn;
			int maxMessageSize;
			bool expectMask;
			ConcurrentQueue<Message> queue;
			BufferPool bufferPool;
			config2.Deconstruct(out conn, out maxMessageSize, out expectMask, out queue, out bufferPool);
			Connection connection = conn;
			int num = maxMessageSize;
			bool flag = expectMask;
			ConcurrentQueue<Message> concurrentQueue = queue;
			byte[] buffer = new byte[4 + (flag ? 4 : 0) + num];
			try
			{
				try
				{
					TcpClient client = connection.client;
					while (client.Connected)
					{
						ReadOneMessage(config, buffer);
					}
				}
				catch (Exception)
				{
					Utils.CheckForInterupt();
					throw;
				}
			}
			catch (ThreadInterruptedException)
			{
			}
			catch (ThreadAbortException)
			{
			}
			catch (ObjectDisposedException)
			{
			}
			catch (ReadHelperException)
			{
			}
			catch (SocketException exception)
			{
				concurrentQueue.Enqueue(new Message(connection.connId, exception));
			}
			catch (IOException exception2)
			{
				concurrentQueue.Enqueue(new Message(connection.connId, exception2));
			}
			catch (InvalidDataException exception3)
			{
				concurrentQueue.Enqueue(new Message(connection.connId, exception3));
			}
			catch (Exception ex6)
			{
				Log.Exception(ex6);
				concurrentQueue.Enqueue(new Message(connection.connId, ex6));
			}
			finally
			{
				Profiler.EndThreadProfiling();
				connection.Dispose();
			}
		}

		private static void ReadOneMessage(Config config, byte[] buffer)
		{
			Config config2 = config;
			Connection conn;
			int maxMessageSize;
			bool expectMask;
			ConcurrentQueue<Message> queue;
			BufferPool bufferPool;
			config2.Deconstruct(out conn, out maxMessageSize, out expectMask, out queue, out bufferPool);
			Connection connection = conn;
			int maxLength = maxMessageSize;
			bool flag = expectMask;
			Stream stream = connection.stream;
			int outOffset = 0;
			outOffset = ReadHelper.Read(stream, buffer, outOffset, 2);
			if (MessageProcessor.NeedToReadShortLength(buffer))
			{
				outOffset = ReadHelper.Read(stream, buffer, outOffset, 2);
			}
			MessageProcessor.ValidateHeader(buffer, maxLength, flag);
			if (flag)
			{
				outOffset = ReadHelper.Read(stream, buffer, outOffset, 4);
			}
			int opcode = MessageProcessor.GetOpcode(buffer);
			int payloadLength = MessageProcessor.GetPayloadLength(buffer);
			int msgOffset = outOffset;
			outOffset = ReadHelper.Read(stream, buffer, outOffset, payloadLength);
			switch (opcode)
			{
			case 2:
				HandleArrayMessage(config, buffer, msgOffset, payloadLength);
				break;
			case 8:
				HandleCloseMessage(config, buffer, msgOffset, payloadLength);
				break;
			}
		}

		private static void HandleArrayMessage(Config config, byte[] buffer, int msgOffset, int payloadLength)
		{
			Config config2 = config;
			Connection conn;
			int maxMessageSize;
			bool expectMask;
			ConcurrentQueue<Message> queue;
			BufferPool bufferPool;
			config2.Deconstruct(out conn, out maxMessageSize, out expectMask, out queue, out bufferPool);
			Connection connection = conn;
			bool num = expectMask;
			ConcurrentQueue<Message> concurrentQueue = queue;
			ArrayBuffer arrayBuffer = bufferPool.Take(payloadLength);
			if (num)
			{
				int maskOffset = msgOffset - 4;
				MessageProcessor.ToggleMask(buffer, msgOffset, arrayBuffer, payloadLength, buffer, maskOffset);
			}
			else
			{
				arrayBuffer.CopyFrom(buffer, msgOffset, payloadLength);
			}
			concurrentQueue.Enqueue(new Message(connection.connId, arrayBuffer));
		}

		private static void HandleCloseMessage(Config config, byte[] buffer, int msgOffset, int payloadLength)
		{
			Config config2 = config;
			Connection conn;
			int maxMessageSize;
			bool expectMask;
			ConcurrentQueue<Message> queue;
			BufferPool bufferPool;
			config2.Deconstruct(out conn, out maxMessageSize, out expectMask, out queue, out bufferPool);
			Connection connection = conn;
			if (expectMask)
			{
				int maskOffset = msgOffset - 4;
				MessageProcessor.ToggleMask(buffer, msgOffset, payloadLength, buffer, maskOffset);
			}
			connection.Dispose();
		}

		private static string GetCloseMessage(byte[] buffer, int msgOffset, int payloadLength)
		{
			return Encoding.UTF8.GetString(buffer, msgOffset + 2, payloadLength - 2);
		}

		private static int GetCloseCode(byte[] buffer, int msgOffset)
		{
			return (buffer[msgOffset] << 8) | buffer[msgOffset + 1];
		}
	}
}
