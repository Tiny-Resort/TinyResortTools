using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

namespace kcp2k
{
	[DisallowMultipleComponent]
	public class KcpTransport : Transport
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static Action<string> _003C_003E9__16_0;

			public static Func<KcpServerConnection, long> _003C_003E9__38_0;

			public static Func<KcpServerConnection, long> _003C_003E9__39_0;

			public static Func<KcpServerConnection, int> _003C_003E9__40_0;

			public static Func<KcpServerConnection, int> _003C_003E9__41_0;

			public static Func<KcpServerConnection, int> _003C_003E9__42_0;

			public static Func<KcpServerConnection, int> _003C_003E9__43_0;

			internal void _003CAwake_003Eb__16_0(string _)
			{
			}

			internal long _003CGetAverageMaxSendRate_003Eb__38_0(KcpServerConnection conn)
			{
				return conn.MaxSendRate;
			}

			internal long _003CGetAverageMaxReceiveRate_003Eb__39_0(KcpServerConnection conn)
			{
				return conn.MaxReceiveRate;
			}

			internal int _003CGetTotalSendQueue_003Eb__40_0(KcpServerConnection conn)
			{
				return conn.SendQueueCount;
			}

			internal int _003CGetTotalReceiveQueue_003Eb__41_0(KcpServerConnection conn)
			{
				return conn.ReceiveQueueCount;
			}

			internal int _003CGetTotalSendBuffer_003Eb__42_0(KcpServerConnection conn)
			{
				return conn.SendBufferCount;
			}

			internal int _003CGetTotalReceiveBuffer_003Eb__43_0(KcpServerConnection conn)
			{
				return conn.ReceiveBufferCount;
			}
		}

		public const string Scheme = "kcp";

		[Header("Transport Configuration")]
		public ushort Port = 7777;

		[Tooltip("DualMode listens to IPv6 and IPv4 simultaneously. Disable if the platform only supports IPv4.")]
		public bool DualMode = true;

		[Tooltip("NoDelay is recommended to reduce latency. This also scales better without buffers getting full.")]
		public bool NoDelay = true;

		[Tooltip("KCP internal update interval. 100ms is KCP default, but a lower interval is recommended to minimize latency and to scale to more networked entities.")]
		public uint Interval = 10u;

		[Tooltip("KCP timeout in milliseconds. Note that KCP sends a ping automatically.")]
		public int Timeout = 10000;

		[Header("Advanced")]
		[Tooltip("KCP fastresend parameter. Faster resend for the cost of higher bandwidth. 0 in normal mode, 2 in turbo mode.")]
		public int FastResend = 2;

		[Tooltip("KCP congestion window. Enabled in normal mode, disabled in turbo mode. Disable this for high scale games if connections get choked regularly.")]
		public bool CongestionWindow;

		[Tooltip("KCP window size can be modified to support higher loads.")]
		public uint SendWindowSize = 4096u;

		[Tooltip("KCP window size can be modified to support higher loads.")]
		public uint ReceiveWindowSize = 4096u;

		[Tooltip("Enable to use where-allocation NonAlloc KcpServer/Client/Connection versions. Highly recommended on all Unity platforms.")]
		public bool NonAlloc = true;

		private KcpServer server;

		private KcpClient client;

		[Header("Debug")]
		public bool debugLog;

		public bool statisticsGUI;

		public bool statisticsLog;

		private void Awake()
		{
			if (debugLog)
			{
				Log.Info = Debug.Log;
			}
			else
			{
				Log.Info = _003C_003Ec._003C_003E9__16_0 ?? (_003C_003Ec._003C_003E9__16_0 = _003C_003Ec._003C_003E9._003CAwake_003Eb__16_0);
			}
			Log.Warning = Debug.LogWarning;
			Log.Error = Debug.LogError;
			client = (NonAlloc ? new KcpClientNonAlloc(_003CAwake_003Eb__16_1, _003CAwake_003Eb__16_2, _003CAwake_003Eb__16_3) : new KcpClient(_003CAwake_003Eb__16_4, _003CAwake_003Eb__16_5, _003CAwake_003Eb__16_6));
			server = (NonAlloc ? new KcpServerNonAlloc(_003CAwake_003Eb__16_7, _003CAwake_003Eb__16_8, _003CAwake_003Eb__16_9, DualMode, NoDelay, Interval, FastResend, CongestionWindow, SendWindowSize, ReceiveWindowSize, Timeout) : new KcpServer(_003CAwake_003Eb__16_10, _003CAwake_003Eb__16_11, _003CAwake_003Eb__16_12, DualMode, NoDelay, Interval, FastResend, CongestionWindow, SendWindowSize, ReceiveWindowSize, Timeout));
			if (statisticsLog)
			{
				InvokeRepeating("OnLogStatistics", 1f, 1f);
			}
			Debug.Log("KcpTransport initialized!");
		}

		public override bool Available()
		{
			return Application.platform != RuntimePlatform.WebGLPlayer;
		}

		public override bool ClientConnected()
		{
			return client.connected;
		}

		public override void ClientConnect(string address)
		{
			client.Connect(address, Port, NoDelay, Interval, FastResend, CongestionWindow, SendWindowSize, ReceiveWindowSize, Timeout);
		}

		public override void ClientSend(ArraySegment<byte> segment, int channelId)
		{
			if (channelId == 1)
			{
				client.Send(segment, KcpChannel.Unreliable);
			}
			else
			{
				client.Send(segment, KcpChannel.Reliable);
			}
		}

		public override void ClientDisconnect()
		{
			client.Disconnect();
		}

		public override void ClientEarlyUpdate()
		{
			if (base.enabled)
			{
				client.TickIncoming();
			}
		}

		public override void ClientLateUpdate()
		{
			client.TickOutgoing();
		}

		private void OnEnable()
		{
			KcpClient kcpClient = client;
			if (kcpClient != null)
			{
				kcpClient.Unpause();
			}
			KcpServer kcpServer = server;
			if (kcpServer != null)
			{
				kcpServer.Unpause();
			}
		}

		private void OnDisable()
		{
			KcpClient kcpClient = client;
			if (kcpClient != null)
			{
				kcpClient.Pause();
			}
			KcpServer kcpServer = server;
			if (kcpServer != null)
			{
				kcpServer.Pause();
			}
		}

		public override Uri ServerUri()
		{
			return new UriBuilder
			{
				Scheme = "kcp",
				Host = Dns.GetHostName(),
				Port = Port
			}.Uri;
		}

		public override bool ServerActive()
		{
			return server.IsActive();
		}

		public override void ServerStart()
		{
			server.Start(Port);
		}

		public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId)
		{
			if (channelId == 1)
			{
				server.Send(connectionId, segment, KcpChannel.Unreliable);
			}
			else
			{
				server.Send(connectionId, segment, KcpChannel.Reliable);
			}
		}

		public override void ServerDisconnect(int connectionId)
		{
			server.Disconnect(connectionId);
		}

		public override string ServerGetClientAddress(int connectionId)
		{
			return server.GetClientAddress(connectionId);
		}

		public override void ServerStop()
		{
			server.Stop();
		}

		public override void ServerEarlyUpdate()
		{
			if (base.enabled)
			{
				server.TickIncoming();
			}
		}

		public override void ServerLateUpdate()
		{
			server.TickOutgoing();
		}

		public override void Shutdown()
		{
		}

		public override int GetMaxPacketSize(int channelId = 0)
		{
			if (channelId == 1)
			{
				return 1199;
			}
			return 149224;
		}

		public override int GetBatchThreshold(int channelId)
		{
			return 1199;
		}

		public long GetAverageMaxSendRate()
		{
			if (server.connections.Count <= 0)
			{
				return 0L;
			}
			return server.connections.Values.Sum(_003C_003Ec._003C_003E9__38_0 ?? (_003C_003Ec._003C_003E9__38_0 = _003C_003Ec._003C_003E9._003CGetAverageMaxSendRate_003Eb__38_0)) / server.connections.Count;
		}

		public long GetAverageMaxReceiveRate()
		{
			if (server.connections.Count <= 0)
			{
				return 0L;
			}
			return server.connections.Values.Sum(_003C_003Ec._003C_003E9__39_0 ?? (_003C_003Ec._003C_003E9__39_0 = _003C_003Ec._003C_003E9._003CGetAverageMaxReceiveRate_003Eb__39_0)) / server.connections.Count;
		}

		private long GetTotalSendQueue()
		{
			return server.connections.Values.Sum(_003C_003Ec._003C_003E9__40_0 ?? (_003C_003Ec._003C_003E9__40_0 = _003C_003Ec._003C_003E9._003CGetTotalSendQueue_003Eb__40_0));
		}

		private long GetTotalReceiveQueue()
		{
			return server.connections.Values.Sum(_003C_003Ec._003C_003E9__41_0 ?? (_003C_003Ec._003C_003E9__41_0 = _003C_003Ec._003C_003E9._003CGetTotalReceiveQueue_003Eb__41_0));
		}

		private long GetTotalSendBuffer()
		{
			return server.connections.Values.Sum(_003C_003Ec._003C_003E9__42_0 ?? (_003C_003Ec._003C_003E9__42_0 = _003C_003Ec._003C_003E9._003CGetTotalSendBuffer_003Eb__42_0));
		}

		private long GetTotalReceiveBuffer()
		{
			return server.connections.Values.Sum(_003C_003Ec._003C_003E9__43_0 ?? (_003C_003Ec._003C_003E9__43_0 = _003C_003Ec._003C_003E9._003CGetTotalReceiveBuffer_003Eb__43_0));
		}

		public static string PrettyBytes(long bytes)
		{
			if (bytes < 1024)
			{
				return string.Format("{0} B", bytes);
			}
			if (bytes < 1048576)
			{
				return string.Format("{0:F2} KB", (float)bytes / 1024f);
			}
			if (bytes < 1073741824)
			{
				return string.Format("{0:F2} MB", (float)bytes / 1048576f);
			}
			return string.Format("{0:F2} GB", (float)bytes / 1.0737418E+09f);
		}

		private void OnGUI()
		{
			if (statisticsGUI)
			{
				GUILayout.BeginArea(new Rect(5f, 110f, 300f, 300f));
				if (ServerActive())
				{
					GUILayout.BeginVertical("Box");
					GUILayout.Label("SERVER");
					GUILayout.Label(string.Format("  connections: {0}", server.connections.Count));
					GUILayout.Label("  MaxSendRate (avg): " + PrettyBytes(GetAverageMaxSendRate()) + "/s");
					GUILayout.Label("  MaxRecvRate (avg): " + PrettyBytes(GetAverageMaxReceiveRate()) + "/s");
					GUILayout.Label(string.Format("  SendQueue: {0}", GetTotalSendQueue()));
					GUILayout.Label(string.Format("  ReceiveQueue: {0}", GetTotalReceiveQueue()));
					GUILayout.Label(string.Format("  SendBuffer: {0}", GetTotalSendBuffer()));
					GUILayout.Label(string.Format("  ReceiveBuffer: {0}", GetTotalReceiveBuffer()));
					GUILayout.EndVertical();
				}
				if (ClientConnected())
				{
					GUILayout.BeginVertical("Box");
					GUILayout.Label("CLIENT");
					GUILayout.Label("  MaxSendRate: " + PrettyBytes(client.connection.MaxSendRate) + "/s");
					GUILayout.Label("  MaxRecvRate: " + PrettyBytes(client.connection.MaxReceiveRate) + "/s");
					GUILayout.Label(string.Format("  SendQueue: {0}", client.connection.SendQueueCount));
					GUILayout.Label(string.Format("  ReceiveQueue: {0}", client.connection.ReceiveQueueCount));
					GUILayout.Label(string.Format("  SendBuffer: {0}", client.connection.SendBufferCount));
					GUILayout.Label(string.Format("  ReceiveBuffer: {0}", client.connection.ReceiveBufferCount));
					GUILayout.EndVertical();
				}
				GUILayout.EndArea();
			}
		}

		private void OnLogStatistics()
		{
			if (ServerActive())
			{
				Debug.Log(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("kcp SERVER @ time: " + NetworkTime.localTime + "\n", string.Format("  connections: {0}\n", server.connections.Count)), "  MaxSendRate (avg): ", PrettyBytes(GetAverageMaxSendRate()), "/s\n"), "  MaxRecvRate (avg): ", PrettyBytes(GetAverageMaxReceiveRate()), "/s\n"), string.Format("  SendQueue: {0}\n", GetTotalSendQueue())), string.Format("  ReceiveQueue: {0}\n", GetTotalReceiveQueue())), string.Format("  SendBuffer: {0}\n", GetTotalSendBuffer())), string.Format("  ReceiveBuffer: {0}\n\n", GetTotalReceiveBuffer())));
			}
			if (ClientConnected())
			{
				Debug.Log(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("kcp CLIENT @ time: " + NetworkTime.localTime + "\n", "  MaxSendRate: ", PrettyBytes(client.connection.MaxSendRate), "/s\n"), "  MaxRecvRate: ", PrettyBytes(client.connection.MaxReceiveRate), "/s\n"), string.Format("  SendQueue: {0}\n", client.connection.SendQueueCount)), string.Format("  ReceiveQueue: {0}\n", client.connection.ReceiveQueueCount)), string.Format("  SendBuffer: {0}\n", client.connection.SendBufferCount)), string.Format("  ReceiveBuffer: {0}\n\n", client.connection.ReceiveBufferCount)));
			}
		}

		public override string ToString()
		{
			return "KCP";
		}

		[CompilerGenerated]
		private void _003CAwake_003Eb__16_1()
		{
			OnClientConnected();
		}

		[CompilerGenerated]
		private void _003CAwake_003Eb__16_2(ArraySegment<byte> message)
		{
			OnClientDataReceived(message, 0);
		}

		[CompilerGenerated]
		private void _003CAwake_003Eb__16_3()
		{
			OnClientDisconnected();
		}

		[CompilerGenerated]
		private void _003CAwake_003Eb__16_4()
		{
			OnClientConnected();
		}

		[CompilerGenerated]
		private void _003CAwake_003Eb__16_5(ArraySegment<byte> message)
		{
			OnClientDataReceived(message, 0);
		}

		[CompilerGenerated]
		private void _003CAwake_003Eb__16_6()
		{
			OnClientDisconnected();
		}

		[CompilerGenerated]
		private void _003CAwake_003Eb__16_7(int connectionId)
		{
			OnServerConnected(connectionId);
		}

		[CompilerGenerated]
		private void _003CAwake_003Eb__16_8(int connectionId, ArraySegment<byte> message)
		{
			OnServerDataReceived(connectionId, message, 0);
		}

		[CompilerGenerated]
		private void _003CAwake_003Eb__16_9(int connectionId)
		{
			OnServerDisconnected(connectionId);
		}

		[CompilerGenerated]
		private void _003CAwake_003Eb__16_10(int connectionId)
		{
			OnServerConnected(connectionId);
		}

		[CompilerGenerated]
		private void _003CAwake_003Eb__16_11(int connectionId, ArraySegment<byte> message)
		{
			OnServerDataReceived(connectionId, message, 0);
		}

		[CompilerGenerated]
		private void _003CAwake_003Eb__16_12(int connectionId)
		{
			OnServerDisconnected(connectionId);
		}
	}
}
