using System;
using System.IO;
using Steamworks;
using UnityEngine;

namespace Mirror.FizzySteam
{
	[HelpURL("https://github.com/Chykary/FizzySteamworks")]
	public class FizzySteamworks : Transport
	{
		private const string STEAM_SCHEME = "steam";

		private static IClient client;

		private static IServer server;

		[SerializeField]
		public EP2PSend[] Channels = new EP2PSend[2]
		{
			EP2PSend.k_EP2PSendReliable,
			EP2PSend.k_EP2PSendUnreliableNoDelay
		};

		[Tooltip("Timeout for connecting in seconds.")]
		public int Timeout = 25;

		[Tooltip("The Steam ID for your application.")]
		public string SteamAppID = "480";

		[Tooltip("Allow or disallow P2P connections to fall back to being relayed through the Steam servers if a direct connection or NAT-traversal cannot be established.")]
		public bool AllowSteamRelay = true;

		[Tooltip("Use SteamSockets instead of the (deprecated) SteamNetworking. This will always use Relay.")]
		public bool UseNextGenSteamNetworking = true;

		[Header("Info")]
		[Tooltip("This will display your Steam User ID when you start or connect to a server.")]
		public ulong SteamUserID;

		private void Awake()
		{
			if (File.Exists("steam_appid.txt"))
			{
				string text = File.ReadAllText("steam_appid.txt");
				if (text != SteamAppID)
				{
					File.WriteAllText("steam_appid.txt", SteamAppID.ToString());
					Debug.Log("Updating steam_appid.txt. Previous: " + text + ", new SteamAppID " + SteamAppID);
				}
			}
			else
			{
				File.WriteAllText("steam_appid.txt", SteamAppID.ToString());
				Debug.Log("New steam_appid.txt written with SteamAppID " + SteamAppID);
			}
		}

		private void OnEnable()
		{
			Invoke("FetchSteamID", 1f);
		}

		public override void ClientEarlyUpdate()
		{
			if (base.enabled)
			{
				IClient obj = client;
				if (obj != null)
				{
					obj.ReceiveData();
				}
			}
		}

		public override void ServerEarlyUpdate()
		{
			if (base.enabled)
			{
				IServer obj = server;
				if (obj != null)
				{
					obj.ReceiveData();
				}
			}
		}

		public override void ClientLateUpdate()
		{
			if (base.enabled)
			{
				IClient obj = client;
				if (obj != null)
				{
					obj.FlushData();
				}
			}
		}

		public override void ServerLateUpdate()
		{
			if (base.enabled)
			{
				IServer obj = server;
				if (obj != null)
				{
					obj.FlushData();
				}
			}
		}

		public override bool ClientConnected()
		{
			if (ClientActive())
			{
				return client.Connected;
			}
			return false;
		}

		public override void ClientConnect(string address)
		{
			if (!SteamManager.Initialized)
			{
				Debug.LogError("SteamWorks not initialized. Client could not be started.");
				OnClientDisconnected();
				return;
			}
			FetchSteamID();
			if (ServerActive())
			{
				Debug.LogError("Transport already running as server!");
			}
			else if (!ClientActive() || client.Error)
			{
				if (UseNextGenSteamNetworking)
				{
					Debug.Log("Starting client [SteamSockets], target address " + address + ".");
					client = NextClient.CreateClient(this, address);
				}
				else
				{
					Debug.Log(string.Format("Starting client [DEPRECATED SteamNetworking], target address {0}. Relay enabled: {1}", address, AllowSteamRelay));
					SteamNetworking.AllowP2PPacketRelay(AllowSteamRelay);
					client = LegacyClient.CreateClient(this, address);
				}
			}
			else
			{
				Debug.LogError("Client already running!");
			}
		}

		public override void ClientConnect(Uri uri)
		{
			if (uri.Scheme != "steam")
			{
				throw new ArgumentException(string.Format("Invalid url {0}, use {1}://SteamID instead", uri, "steam"), "uri");
			}
			ClientConnect(uri.Host);
		}

		public override void ClientSend(ArraySegment<byte> segment, int channelId)
		{
			byte[] array = new byte[segment.Count];
			Array.Copy(segment.Array, segment.Offset, array, 0, segment.Count);
			client.Send(array, channelId);
		}

		public override void ClientDisconnect()
		{
			if (ClientActive())
			{
				Shutdown();
			}
		}

		public bool ClientActive()
		{
			return client != null;
		}

		public override bool ServerActive()
		{
			return server != null;
		}

		public override void ServerStart()
		{
			if (!SteamManager.Initialized)
			{
				Debug.LogError("SteamWorks not initialized. Server could not be started.");
				return;
			}
			FetchSteamID();
			if (ClientActive())
			{
				Debug.LogError("Transport already running as client!");
			}
			else if (!ServerActive())
			{
				if (UseNextGenSteamNetworking)
				{
					Debug.Log("Starting server [SteamSockets].");
					server = NextServer.CreateServer(this, NetworkManager.singleton.maxConnections);
				}
				else
				{
					Debug.Log(string.Format("Starting server [DEPRECATED SteamNetworking]. Relay enabled: {0}", AllowSteamRelay));
					SteamNetworking.AllowP2PPacketRelay(AllowSteamRelay);
					server = LegacyServer.CreateServer(this, NetworkManager.singleton.maxConnections);
				}
			}
			else
			{
				Debug.LogError("Server already started!");
			}
		}

		public override Uri ServerUri()
		{
			return new UriBuilder
			{
				Scheme = "steam",
				Host = SteamUser.GetSteamID().m_SteamID.ToString()
			}.Uri;
		}

		public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId)
		{
			if (ServerActive())
			{
				byte[] array = new byte[segment.Count];
				Array.Copy(segment.Array, segment.Offset, array, 0, segment.Count);
				server.Send(connectionId, array, channelId);
			}
		}

		public override void ServerDisconnect(int connectionId)
		{
			if (ServerActive())
			{
				server.Disconnect(connectionId);
			}
		}

		public override string ServerGetClientAddress(int connectionId)
		{
			if (!ServerActive())
			{
				return string.Empty;
			}
			return server.ServerGetClientAddress(connectionId);
		}

		public override void ServerStop()
		{
			if (ServerActive())
			{
				Shutdown();
			}
		}

		public override void Shutdown()
		{
			if (server != null)
			{
				server.Shutdown();
				server = null;
				Debug.Log("Transport shut down - was server.");
			}
			if (client != null)
			{
				client.Disconnect();
				client = null;
				Debug.Log("Transport shut down - was client.");
			}
		}

		public override int GetMaxPacketSize(int channelId)
		{
			if (UseNextGenSteamNetworking)
			{
				return 524288;
			}
			if (channelId >= Channels.Length)
			{
				Debug.LogError("Channel Id exceeded configured channels! Please configure more channels.");
				return 1200;
			}
			switch (Channels[channelId])
			{
			case EP2PSend.k_EP2PSendUnreliable:
			case EP2PSend.k_EP2PSendUnreliableNoDelay:
				return 1200;
			case EP2PSend.k_EP2PSendReliable:
			case EP2PSend.k_EP2PSendReliableWithBuffering:
				return 1048576;
			default:
				throw new NotSupportedException();
			}
		}

		public override bool Available()
		{
			try
			{
				return SteamManager.Initialized;
			}
			catch
			{
				return false;
			}
		}

		private void FetchSteamID()
		{
			if (SteamManager.Initialized)
			{
				if (UseNextGenSteamNetworking)
				{
					SteamNetworkingUtils.InitRelayNetworkAccess();
				}
				SteamUserID = SteamUser.GetSteamID().m_SteamID;
			}
		}

		private void OnDestroy()
		{
			Shutdown();
		}
	}
}
