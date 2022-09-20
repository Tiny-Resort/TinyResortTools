using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirror
{
	public abstract class Transport : MonoBehaviour
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static Action _003C_003E9__35_0;

			public static Action<ArraySegment<byte>, int> _003C_003E9__35_1;

			public static Action<Exception> _003C_003E9__35_2;

			public static Action _003C_003E9__35_3;

			public static Action<int> _003C_003E9__35_4;

			public static Action<int, ArraySegment<byte>, int> _003C_003E9__35_5;

			public static Action<int, Exception> _003C_003E9__35_6;

			public static Action<int> _003C_003E9__35_7;

			internal void _003C_002Ector_003Eb__35_0()
			{
				Debug.LogWarning("OnClientConnected called with no handler");
			}

			internal void _003C_002Ector_003Eb__35_1(ArraySegment<byte> data, int channel)
			{
				Debug.LogWarning("OnClientDataReceived called with no handler");
			}

			internal void _003C_002Ector_003Eb__35_2(Exception error)
			{
				Debug.LogWarning("OnClientError called with no handler");
			}

			internal void _003C_002Ector_003Eb__35_3()
			{
				Debug.LogWarning("OnClientDisconnected called with no handler");
			}

			internal void _003C_002Ector_003Eb__35_4(int connId)
			{
				Debug.LogWarning("OnServerConnected called with no handler");
			}

			internal void _003C_002Ector_003Eb__35_5(int connId, ArraySegment<byte> data, int channel)
			{
				Debug.LogWarning("OnServerDataReceived called with no handler");
			}

			internal void _003C_002Ector_003Eb__35_6(int connId, Exception error)
			{
				Debug.LogWarning("OnServerError called with no handler");
			}

			internal void _003C_002Ector_003Eb__35_7(int connId)
			{
				Debug.LogWarning("OnServerDisconnected called with no handler");
			}
		}

		public static Transport activeTransport;

		public Action OnClientConnected = _003C_003Ec._003C_003E9__35_0 ?? (_003C_003Ec._003C_003E9__35_0 = _003C_003Ec._003C_003E9._003C_002Ector_003Eb__35_0);

		public Action<ArraySegment<byte>, int> OnClientDataReceived = _003C_003Ec._003C_003E9__35_1 ?? (_003C_003Ec._003C_003E9__35_1 = _003C_003Ec._003C_003E9._003C_002Ector_003Eb__35_1);

		public Action<Exception> OnClientError = _003C_003Ec._003C_003E9__35_2 ?? (_003C_003Ec._003C_003E9__35_2 = _003C_003Ec._003C_003E9._003C_002Ector_003Eb__35_2);

		public Action OnClientDisconnected = _003C_003Ec._003C_003E9__35_3 ?? (_003C_003Ec._003C_003E9__35_3 = _003C_003Ec._003C_003E9._003C_002Ector_003Eb__35_3);

		public Action<int> OnServerConnected = _003C_003Ec._003C_003E9__35_4 ?? (_003C_003Ec._003C_003E9__35_4 = _003C_003Ec._003C_003E9._003C_002Ector_003Eb__35_4);

		public Action<int, ArraySegment<byte>, int> OnServerDataReceived = _003C_003Ec._003C_003E9__35_5 ?? (_003C_003Ec._003C_003E9__35_5 = _003C_003Ec._003C_003E9._003C_002Ector_003Eb__35_5);

		public Action<int, Exception> OnServerError = _003C_003Ec._003C_003E9__35_6 ?? (_003C_003Ec._003C_003E9__35_6 = _003C_003Ec._003C_003E9._003C_002Ector_003Eb__35_6);

		public Action<int> OnServerDisconnected = _003C_003Ec._003C_003E9__35_7 ?? (_003C_003Ec._003C_003E9__35_7 = _003C_003Ec._003C_003E9._003C_002Ector_003Eb__35_7);

		public abstract bool Available();

		public abstract bool ClientConnected();

		public abstract void ClientConnect(string address);

		public virtual void ClientConnect(Uri uri)
		{
			ClientConnect(uri.Host);
		}

		[Obsolete("Use ClientSend(segment, channelId) instead. channelId is now the last parameter.")]
		public virtual void ClientSend(int channelId, ArraySegment<byte> segment)
		{
		}

		public virtual void ClientSend(ArraySegment<byte> segment, int channelId)
		{
			ClientSend(channelId, segment);
		}

		public abstract void ClientDisconnect();

		public abstract Uri ServerUri();

		public abstract bool ServerActive();

		public abstract void ServerStart();

		[Obsolete("Use ServerSend(connectionId, segment, channelId) instead. channelId is now the last parameter.")]
		public virtual void ServerSend(int connectionId, int channelId, ArraySegment<byte> segment)
		{
		}

		public virtual void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId)
		{
			ServerSend(connectionId, channelId, segment);
		}

		public abstract void ServerDisconnect(int connectionId);

		public abstract string ServerGetClientAddress(int connectionId);

		public abstract void ServerStop();

		public abstract int GetMaxPacketSize(int channelId = 0);

		public virtual int GetBatchThreshold(int channelId)
		{
			return GetMaxBatchSize(channelId);
		}

		[Obsolete("GetMaxBatchSize was renamed to GetBatchThreshold.")]
		public virtual int GetMaxBatchSize(int channelId)
		{
			return GetMaxPacketSize(channelId);
		}

		public void Update()
		{
		}

		public void LateUpdate()
		{
		}

		public virtual void ClientEarlyUpdate()
		{
		}

		public virtual void ServerEarlyUpdate()
		{
		}

		public virtual void ClientLateUpdate()
		{
		}

		public virtual void ServerLateUpdate()
		{
		}

		public abstract void Shutdown();

		public virtual void OnApplicationQuit()
		{
			Shutdown();
		}
	}
}
