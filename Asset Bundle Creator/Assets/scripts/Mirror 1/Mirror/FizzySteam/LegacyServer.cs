using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Steamworks;
using UnityEngine;

namespace Mirror.FizzySteam
{
	public class LegacyServer : LegacyCommon, IServer
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass15_0
		{
			public FizzySteamworks transport;

			internal void _003CCreateServer_003Eb__0(int id)
			{
				transport.OnServerConnected(id);
			}

			internal void _003CCreateServer_003Eb__1(int id)
			{
				transport.OnServerDisconnected(id);
			}

			internal void _003CCreateServer_003Eb__2(int id, byte[] data, int channel)
			{
				transport.OnServerDataReceived(id, new ArraySegment<byte>(data), channel);
			}

			internal void _003CCreateServer_003Eb__3(int id, Exception exception)
			{
				transport.OnServerError(id, exception);
			}
		}

		private BidirectionalDictionary<CSteamID, int> steamToMirrorIds;

		private int maxConnections;

		private int nextConnectionID;

		private event Action<int> OnConnected;

		private event Action<int, byte[], int> OnReceivedData;

		private event Action<int> OnDisconnected;

		private event Action<int, Exception> OnReceivedError;

		public static LegacyServer CreateServer(FizzySteamworks transport, int maxConnections)
		{
			_003C_003Ec__DisplayClass15_0 _003C_003Ec__DisplayClass15_ = new _003C_003Ec__DisplayClass15_0();
			_003C_003Ec__DisplayClass15_.transport = transport;
			LegacyServer legacyServer = new LegacyServer(_003C_003Ec__DisplayClass15_.transport, maxConnections);
			legacyServer.OnConnected += _003C_003Ec__DisplayClass15_._003CCreateServer_003Eb__0;
			legacyServer.OnDisconnected += _003C_003Ec__DisplayClass15_._003CCreateServer_003Eb__1;
			legacyServer.OnReceivedData += _003C_003Ec__DisplayClass15_._003CCreateServer_003Eb__2;
			legacyServer.OnReceivedError += _003C_003Ec__DisplayClass15_._003CCreateServer_003Eb__3;
			if (!SteamManager.Initialized)
			{
				Debug.LogError("SteamWorks not initialized.");
			}
			return legacyServer;
		}

		private LegacyServer(FizzySteamworks transport, int maxConnections)
			: base(transport)
		{
			this.maxConnections = maxConnections;
			steamToMirrorIds = new BidirectionalDictionary<CSteamID, int>();
			nextConnectionID = 1;
		}

		protected override void OnNewConnection(P2PSessionRequest_t result)
		{
			SteamNetworking.AcceptP2PSessionWithUser(result.m_steamIDRemote);
		}

		protected override void OnReceiveInternalData(InternalMessages type, CSteamID clientSteamID)
		{
			switch (type)
			{
			case InternalMessages.CONNECT:
			{
				if (steamToMirrorIds.Count >= maxConnections)
				{
					SendInternal(clientSteamID, InternalMessages.DISCONNECT);
					break;
				}
				SendInternal(clientSteamID, InternalMessages.ACCEPT_CONNECT);
				int num = nextConnectionID++;
				steamToMirrorIds.Add(clientSteamID, num);
				this.OnConnected(num);
				Debug.Log(string.Format("Client with SteamID {0} connected. Assigning connection id {1}", clientSteamID, num));
				break;
			}
			case InternalMessages.DISCONNECT:
			{
				int value;
				if (steamToMirrorIds.TryGetValue(clientSteamID, out value))
				{
					this.OnDisconnected(value);
					CloseP2PSessionWithUser(clientSteamID);
					steamToMirrorIds.Remove(clientSteamID);
					Debug.Log(string.Format("Client with SteamID {0} disconnected.", clientSteamID));
				}
				break;
			}
			default:
				Debug.Log("Received unknown message type");
				break;
			}
		}

		protected override void OnReceiveData(byte[] data, CSteamID clientSteamID, int channel)
		{
			int value;
			if (steamToMirrorIds.TryGetValue(clientSteamID, out value))
			{
				this.OnReceivedData(value, data, channel);
				return;
			}
			CloseP2PSessionWithUser(clientSteamID);
			CSteamID cSteamID = clientSteamID;
			Debug.LogError("Data received from steam client thats not known " + cSteamID.ToString());
			this.OnReceivedError(-1, new Exception("ERROR Unknown SteamID"));
		}

		public void Disconnect(int connectionId)
		{
			CSteamID value;
			if (steamToMirrorIds.TryGetValue(connectionId, out value))
			{
				SendInternal(value, InternalMessages.DISCONNECT);
				steamToMirrorIds.Remove(connectionId);
			}
			else
			{
				Debug.LogWarning("Trying to disconnect unknown connection id: " + connectionId);
			}
		}

		public void Shutdown()
		{
			foreach (KeyValuePair<CSteamID, int> steamToMirrorId in steamToMirrorIds)
			{
				Disconnect(steamToMirrorId.Value);
				WaitForClose(steamToMirrorId.Key);
			}
			Dispose();
		}

		public void Send(int connectionId, byte[] data, int channelId)
		{
			CSteamID value;
			if (steamToMirrorIds.TryGetValue(connectionId, out value))
			{
				Send(value, data, channelId);
				return;
			}
			Debug.LogError("Trying to send on unknown connection: " + connectionId);
			this.OnReceivedError(connectionId, new Exception("ERROR Unknown Connection"));
		}

		public string ServerGetClientAddress(int connectionId)
		{
			CSteamID value;
			if (steamToMirrorIds.TryGetValue(connectionId, out value))
			{
				return value.ToString();
			}
			Debug.LogError("Trying to get info on unknown connection: " + connectionId);
			this.OnReceivedError(connectionId, new Exception("ERROR Unknown Connection"));
			return string.Empty;
		}

		protected override void OnConnectionFailed(CSteamID remoteId)
		{
			int value;
			int obj = (steamToMirrorIds.TryGetValue(remoteId, out value) ? value : nextConnectionID++);
			this.OnDisconnected(obj);
			steamToMirrorIds.Remove(remoteId);
		}

		public void FlushData()
		{
		}
	}
}
