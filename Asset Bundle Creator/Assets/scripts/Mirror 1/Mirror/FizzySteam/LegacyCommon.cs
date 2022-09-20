using System;
using System.Collections;
using Steamworks;
using UnityEngine;

namespace Mirror.FizzySteam
{
	public abstract class LegacyCommon
	{
		protected enum InternalMessages : byte
		{
			CONNECT = 0,
			ACCEPT_CONNECT = 1,
			DISCONNECT = 2
		}

		private EP2PSend[] channels;

		private Callback<P2PSessionRequest_t> callback_OnNewConnection;

		private Callback<P2PSessionConnectFail_t> callback_OnConnectFail;

		protected readonly FizzySteamworks transport;

		private int internal_ch
		{
			get
			{
				return channels.Length;
			}
		}

		protected LegacyCommon(FizzySteamworks transport)
		{
			channels = transport.Channels;
			callback_OnNewConnection = Callback<P2PSessionRequest_t>.Create(OnNewConnection);
			callback_OnConnectFail = Callback<P2PSessionConnectFail_t>.Create(OnConnectFail);
			this.transport = transport;
		}

		protected void Dispose()
		{
			if (callback_OnNewConnection != null)
			{
				callback_OnNewConnection.Dispose();
				callback_OnNewConnection = null;
			}
			if (callback_OnConnectFail != null)
			{
				callback_OnConnectFail.Dispose();
				callback_OnConnectFail = null;
			}
		}

		protected abstract void OnNewConnection(P2PSessionRequest_t result);

		private void OnConnectFail(P2PSessionConnectFail_t result)
		{
			OnConnectionFailed(result.m_steamIDRemote);
			CloseP2PSessionWithUser(result.m_steamIDRemote);
			switch (result.m_eP2PSessionError)
			{
			case 1:
				Debug.LogError("Connection failed: The target user is not running the same game.");
				break;
			case 2:
				Debug.LogError("Connection failed: The local user doesn't own the app that is running.");
				break;
			case 3:
				Debug.LogError("Connection failed: Target user isn't connected to Steam.");
				break;
			case 4:
				Debug.LogError("Connection failed: The connection timed out because the target user didn't respond.");
				break;
			default:
				Debug.LogError("Connection failed: Unknown error.");
				break;
			}
		}

		protected void SendInternal(CSteamID target, InternalMessages type)
		{
			SteamNetworking.SendP2PPacket(target, new byte[1] { (byte)type }, 1u, EP2PSend.k_EP2PSendReliable, internal_ch);
		}

		protected void Send(CSteamID host, byte[] msgBuffer, int channel)
		{
			SteamNetworking.SendP2PPacket(host, msgBuffer, (uint)msgBuffer.Length, channels[Mathf.Min(channel, channels.Length - 1)], channel);
		}

		private bool Receive(out CSteamID clientSteamID, out byte[] receiveBuffer, int channel)
		{
			uint pcubMsgSize;
			if (SteamNetworking.IsP2PPacketAvailable(out pcubMsgSize, channel))
			{
				receiveBuffer = new byte[pcubMsgSize];
				uint pcubMsgSize2;
				return SteamNetworking.ReadP2PPacket(receiveBuffer, pcubMsgSize, out pcubMsgSize2, out clientSteamID, channel);
			}
			receiveBuffer = null;
			clientSteamID = CSteamID.Nil;
			return false;
		}

		protected void CloseP2PSessionWithUser(CSteamID clientSteamID)
		{
			SteamNetworking.CloseP2PSessionWithUser(clientSteamID);
		}

		protected void WaitForClose(CSteamID cSteamID)
		{
			if (transport.enabled)
			{
				transport.StartCoroutine(DelayedClose(cSteamID));
			}
			else
			{
				CloseP2PSessionWithUser(cSteamID);
			}
		}

		private IEnumerator DelayedClose(CSteamID cSteamID)
		{
			yield return null;
			CloseP2PSessionWithUser(cSteamID);
		}

		public void ReceiveData()
		{
			try
			{
				CSteamID clientSteamID;
				byte[] receiveBuffer;
				while (transport.enabled && Receive(out clientSteamID, out receiveBuffer, internal_ch))
				{
					if (receiveBuffer.Length == 1)
					{
						OnReceiveInternalData((InternalMessages)receiveBuffer[0], clientSteamID);
						return;
					}
					Debug.Log("Incorrect package length on internal channel.");
				}
				for (int i = 0; i < channels.Length; i++)
				{
					CSteamID clientSteamID2;
					byte[] receiveBuffer2;
					while (transport.enabled && Receive(out clientSteamID2, out receiveBuffer2, i))
					{
						OnReceiveData(receiveBuffer2, clientSteamID2, i);
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		protected abstract void OnReceiveInternalData(InternalMessages type, CSteamID clientSteamID);

		protected abstract void OnReceiveData(byte[] data, CSteamID clientSteamID, int channel);

		protected abstract void OnConnectionFailed(CSteamID remoteId);
	}
}
