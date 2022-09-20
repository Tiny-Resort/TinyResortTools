using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Steamworks;
using UnityEngine;

namespace Mirror.FizzySteam
{
	public class NextServer : NextCommon, IServer
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass19_0
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

			internal void _003CCreateServer_003Eb__2(int id, byte[] data, int ch)
			{
				transport.OnServerDataReceived(id, new ArraySegment<byte>(data), ch);
			}

			internal void _003CCreateServer_003Eb__3(int id, Exception exception)
			{
				transport.OnServerError(id, exception);
			}
		}

		private BidirectionalDictionary<HSteamNetConnection, int> connToMirrorID;

		private BidirectionalDictionary<CSteamID, int> steamIDToMirrorID;

		private int maxConnections;

		private int nextConnectionID;

		private HSteamListenSocket listenSocket;

		private Callback<SteamNetConnectionStatusChangedCallback_t> c_onConnectionChange;

		private event Action<int> OnConnected;

		private event Action<int, byte[], int> OnReceivedData;

		private event Action<int> OnDisconnected;

		private event Action<int, Exception> OnReceivedError;

		private NextServer(int maxConnections)
		{
			this.maxConnections = maxConnections;
			connToMirrorID = new BidirectionalDictionary<HSteamNetConnection, int>();
			steamIDToMirrorID = new BidirectionalDictionary<CSteamID, int>();
			nextConnectionID = 1;
			c_onConnectionChange = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged);
		}

		public static NextServer CreateServer(FizzySteamworks transport, int maxConnections)
		{
			_003C_003Ec__DisplayClass19_0 _003C_003Ec__DisplayClass19_ = new _003C_003Ec__DisplayClass19_0();
			_003C_003Ec__DisplayClass19_.transport = transport;
			NextServer nextServer = new NextServer(maxConnections);
			nextServer.OnConnected += _003C_003Ec__DisplayClass19_._003CCreateServer_003Eb__0;
			nextServer.OnDisconnected += _003C_003Ec__DisplayClass19_._003CCreateServer_003Eb__1;
			nextServer.OnReceivedData += _003C_003Ec__DisplayClass19_._003CCreateServer_003Eb__2;
			nextServer.OnReceivedError += _003C_003Ec__DisplayClass19_._003CCreateServer_003Eb__3;
			if (!SteamManager.Initialized)
			{
				Debug.LogError("SteamWorks not initialized.");
			}
			nextServer.Host();
			return nextServer;
		}

		private void Host()
		{
			SteamNetworkingConfigValue_t[] array = new SteamNetworkingConfigValue_t[0];
			listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, array.Length, array);
		}

		private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t param)
		{
			ulong steamID = param.m_info.m_identityRemote.GetSteamID64();
			if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
			{
				EResult eResult;
				if (connToMirrorID.Count >= maxConnections)
				{
					Debug.Log(string.Format("Incoming connection {0} would exceed max connection count. Rejecting.", steamID));
					SteamNetworkingSockets.CloseConnection(param.m_hConn, 0, "Max Connection Count", false);
				}
				else if ((eResult = SteamNetworkingSockets.AcceptConnection(param.m_hConn)) == EResult.k_EResultOK)
				{
					Debug.Log(string.Format("Accepting connection {0}", steamID));
				}
				else
				{
					Debug.Log(string.Format("Connection {0} could not be accepted: {1}", steamID, eResult.ToString()));
				}
			}
			else if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
			{
				int num = nextConnectionID++;
				connToMirrorID.Add(param.m_hConn, num);
				steamIDToMirrorID.Add(param.m_info.m_identityRemote.GetSteamID(), num);
				this.OnConnected(num);
				Debug.Log(string.Format("Client with SteamID {0} connected. Assigning connection id {1}", steamID, num));
			}
			else if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer || param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
			{
				int value;
				if (connToMirrorID.TryGetValue(param.m_hConn, out value))
				{
					InternalDisconnect(value, param.m_hConn);
				}
			}
			else
			{
				Debug.Log(string.Format("Connection {0} state changed: {1}", steamID, param.m_info.m_eState.ToString()));
			}
		}

		private void InternalDisconnect(int connId, HSteamNetConnection socket)
		{
			this.OnDisconnected(connId);
			SteamNetworkingSockets.CloseConnection(socket, 0, "Graceful disconnect", false);
			connToMirrorID.Remove(connId);
			steamIDToMirrorID.Remove(connId);
			Debug.Log(string.Format("Client with ConnectionID {0} disconnected.", connId));
		}

		public void Disconnect(int connectionId)
		{
			HSteamNetConnection value;
			if (connToMirrorID.TryGetValue(connectionId, out value))
			{
				Debug.Log(string.Format("Connection id {0} disconnected.", connectionId));
				SteamNetworkingSockets.CloseConnection(value, 0, "Disconnected by server", false);
				steamIDToMirrorID.Remove(connectionId);
				connToMirrorID.Remove(connectionId);
				this.OnDisconnected(connectionId);
			}
			else
			{
				Debug.LogWarning("Trying to disconnect unknown connection id: " + connectionId);
			}
		}

		public void FlushData()
		{
			foreach (HSteamNetConnection firstType in connToMirrorID.FirstTypes)
			{
				SteamNetworkingSockets.FlushMessagesOnConnection(firstType);
			}
		}

		public void ReceiveData()
		{
			foreach (HSteamNetConnection item3 in connToMirrorID.FirstTypes.ToList())
			{
				int value;
				if (!connToMirrorID.TryGetValue(item3, out value))
				{
					continue;
				}
				IntPtr[] array = new IntPtr[256];
				int num;
				if ((num = SteamNetworkingSockets.ReceiveMessagesOnConnection(item3, array, 256)) > 0)
				{
					for (int i = 0; i < num; i++)
					{
						ValueTuple<byte[], int> valueTuple = ProcessMessage(array[i]);
						byte[] item = valueTuple.Item1;
						int item2 = valueTuple.Item2;
						this.OnReceivedData(value, item, item2);
					}
				}
			}
		}

		public void Send(int connectionId, byte[] data, int channelId)
		{
			HSteamNetConnection value;
			if (connToMirrorID.TryGetValue(connectionId, out value))
			{
				EResult eResult = SendSocket(value, data, channelId);
				switch (eResult)
				{
				case EResult.k_EResultNoConnection:
				case EResult.k_EResultInvalidParam:
					Debug.Log(string.Format("Connection to {0} was lost.", connectionId));
					InternalDisconnect(connectionId, value);
					break;
				default:
					Debug.LogError("Could not send: " + eResult);
					break;
				case EResult.k_EResultOK:
					break;
				}
			}
			else
			{
				Debug.LogError("Trying to send on unknown connection: " + connectionId);
				this.OnReceivedError(connectionId, new Exception("ERROR Unknown Connection"));
			}
		}

		public string ServerGetClientAddress(int connectionId)
		{
			CSteamID value;
			if (steamIDToMirrorID.TryGetValue(connectionId, out value))
			{
				return value.ToString();
			}
			Debug.LogError("Trying to get info on unknown connection: " + connectionId);
			this.OnReceivedError(connectionId, new Exception("ERROR Unknown Connection"));
			return string.Empty;
		}

		public void Shutdown()
		{
			SteamNetworkingSockets.CloseListenSocket(listenSocket);
			if (c_onConnectionChange != null)
			{
				c_onConnectionChange.Dispose();
				c_onConnectionChange = null;
			}
		}
	}
}
