using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Telepathy;
using UnityEngine;

namespace Mirror
{
	[HelpURL("https://github.com/vis2k/Telepathy/blob/master/README.md")]
	[DisallowMultipleComponent]
	public class TelepathyTransport : Transport
	{
		public const string Scheme = "tcp4";

		public ushort port = 7777;

		[Header("Common")]
		[Tooltip("Nagle Algorithm can be disabled by enabling NoDelay")]
		public bool NoDelay = true;

		[Tooltip("Send timeout in milliseconds.")]
		public int SendTimeout = 5000;

		[Tooltip("Receive timeout in milliseconds. High by default so users don't time out during scene changes.")]
		public int ReceiveTimeout = 30000;

		[Header("Server")]
		[Tooltip("Protect against allocation attacks by keeping the max message size small. Otherwise an attacker might send multiple fake packets with 2GB headers, causing the server to run out of memory after allocating multiple large packets.")]
		public int serverMaxMessageSize = 16384;

		[Tooltip("Server processes a limit amount of messages per tick to avoid a deadlock where it might end up processing forever if messages come in faster than we can process them.")]
		public int serverMaxReceivesPerTick = 10000;

		[Tooltip("Server send queue limit per connection for pending messages. Telepathy will disconnect a connection's queues reach that limit for load balancing. Better to kick one slow client than slowing down the whole server.")]
		public int serverSendQueueLimitPerConnection = 10000;

		[Tooltip("Server receive queue limit per connection for pending messages. Telepathy will disconnect a connection's queues reach that limit for load balancing. Better to kick one slow client than slowing down the whole server.")]
		public int serverReceiveQueueLimitPerConnection = 10000;

		[Header("Client")]
		[Tooltip("Protect against allocation attacks by keeping the max message size small. Otherwise an attacker host might send multiple fake packets with 2GB headers, causing the connected clients to run out of memory after allocating multiple large packets.")]
		public int clientMaxMessageSize = 16384;

		[Tooltip("Client processes a limit amount of messages per tick to avoid a deadlock where it might end up processing forever if messages come in faster than we can process them.")]
		public int clientMaxReceivesPerTick = 1000;

		[Tooltip("Client send queue limit for pending messages. Telepathy will disconnect if the connection's queues reach that limit in order to avoid ever growing latencies.")]
		public int clientSendQueueLimit = 10000;

		[Tooltip("Client receive queue limit for pending messages. Telepathy will disconnect if the connection's queues reach that limit in order to avoid ever growing latencies.")]
		public int clientReceiveQueueLimit = 10000;

		private Client client;

		private Server server;

		private Func<bool> enabledCheck;

		private void Awake()
		{
			Log.Info = Debug.Log;
			Log.Warning = Debug.LogWarning;
			Log.Error = Debug.LogError;
			enabledCheck = _003CAwake_003Eb__16_0;
			Debug.Log("TelepathyTransport initialized!");
		}

		public override bool Available()
		{
			return Application.platform != RuntimePlatform.WebGLPlayer;
		}

		private void CreateClient()
		{
			client = new Client(clientMaxMessageSize);
			client.OnConnected = _003CCreateClient_003Eb__18_0;
			client.OnData = _003CCreateClient_003Eb__18_1;
			client.OnDisconnected = _003CCreateClient_003Eb__18_2;
			client.NoDelay = NoDelay;
			client.SendTimeout = SendTimeout;
			client.ReceiveTimeout = ReceiveTimeout;
			client.SendQueueLimit = clientSendQueueLimit;
			client.ReceiveQueueLimit = clientReceiveQueueLimit;
		}

		public override bool ClientConnected()
		{
			if (client != null)
			{
				return client.Connected;
			}
			return false;
		}

		public override void ClientConnect(string address)
		{
			CreateClient();
			client.Connect(address, port);
		}

		public override void ClientConnect(Uri uri)
		{
			CreateClient();
			if (uri.Scheme != "tcp4")
			{
				throw new ArgumentException(string.Format("Invalid url {0}, use {1}://host:port instead", uri, "tcp4"), "uri");
			}
			int num = (uri.IsDefaultPort ? port : uri.Port);
			client.Connect(uri.Host, num);
		}

		public override void ClientSend(ArraySegment<byte> segment, int channelId)
		{
			Client obj = client;
			if (obj != null)
			{
				obj.Send(segment);
			}
		}

		public override void ClientDisconnect()
		{
			Client obj = client;
			if (obj != null)
			{
				obj.Disconnect();
			}
			client = null;
		}

		public override void ClientEarlyUpdate()
		{
			if (base.enabled)
			{
				Client obj = client;
				if (obj != null)
				{
					obj.Tick(clientMaxReceivesPerTick, enabledCheck);
				}
			}
		}

		public override Uri ServerUri()
		{
			return new UriBuilder
			{
				Scheme = "tcp4",
				Host = Dns.GetHostName(),
				Port = port
			}.Uri;
		}

		public override bool ServerActive()
		{
			if (server != null)
			{
				return server.Active;
			}
			return false;
		}

		public override void ServerStart()
		{
			server = new Server(serverMaxMessageSize);
			server.OnConnected = _003CServerStart_003Eb__27_0;
			server.OnData = _003CServerStart_003Eb__27_1;
			server.OnDisconnected = _003CServerStart_003Eb__27_2;
			server.NoDelay = NoDelay;
			server.SendTimeout = SendTimeout;
			server.ReceiveTimeout = ReceiveTimeout;
			server.SendQueueLimit = serverSendQueueLimitPerConnection;
			server.ReceiveQueueLimit = serverReceiveQueueLimitPerConnection;
			server.Start(port);
		}

		public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId)
		{
			Server obj = server;
			if (obj != null)
			{
				obj.Send(connectionId, segment);
			}
		}

		public override void ServerDisconnect(int connectionId)
		{
			Server obj = server;
			if (obj != null)
			{
				obj.Disconnect(connectionId);
			}
		}

		public override string ServerGetClientAddress(int connectionId)
		{
			try
			{
				Server obj = server;
				return (obj != null) ? obj.GetClientAddress(connectionId) : null;
			}
			catch (SocketException)
			{
				return "unknown";
			}
		}

		public override void ServerStop()
		{
			Server obj = server;
			if (obj != null)
			{
				obj.Stop();
			}
			server = null;
		}

		public override void ServerEarlyUpdate()
		{
			if (base.enabled)
			{
				Server obj = server;
				if (obj != null)
				{
					obj.Tick(serverMaxReceivesPerTick, enabledCheck);
				}
			}
		}

		public override void Shutdown()
		{
			Debug.Log("TelepathyTransport Shutdown()");
			Client obj = client;
			if (obj != null)
			{
				obj.Disconnect();
			}
			client = null;
			Server obj2 = server;
			if (obj2 != null)
			{
				obj2.Stop();
			}
			server = null;
		}

		public override int GetMaxPacketSize(int channelId)
		{
			return serverMaxMessageSize;
		}

		public override string ToString()
		{
			if (server != null && server.Active && server.listener != null)
			{
				return "Telepathy Server port: " + port;
			}
			if (client != null && (client.Connecting || client.Connected))
			{
				return "Telepathy Client port: " + port;
			}
			return "Telepathy (inactive/disconnected)";
		}

		[CompilerGenerated]
		private bool _003CAwake_003Eb__16_0()
		{
			return base.enabled;
		}

		[CompilerGenerated]
		private void _003CCreateClient_003Eb__18_0()
		{
			OnClientConnected();
		}

		[CompilerGenerated]
		private void _003CCreateClient_003Eb__18_1(ArraySegment<byte> segment)
		{
			OnClientDataReceived(segment, 0);
		}

		[CompilerGenerated]
		private void _003CCreateClient_003Eb__18_2()
		{
			OnClientDisconnected();
		}

		[CompilerGenerated]
		private void _003CServerStart_003Eb__27_0(int connectionId)
		{
			OnServerConnected(connectionId);
		}

		[CompilerGenerated]
		private void _003CServerStart_003Eb__27_1(int connectionId, ArraySegment<byte> segment)
		{
			OnServerDataReceived(connectionId, segment, 0);
		}

		[CompilerGenerated]
		private void _003CServerStart_003Eb__27_2(int connectionId)
		{
			OnServerDisconnected(connectionId);
		}
	}
}
