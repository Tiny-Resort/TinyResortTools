using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;

namespace Mirror.FizzySteam
{
	public class LegacyClient : LegacyCommon, IClient
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass22_0
		{
			public FizzySteamworks transport;

			internal void _003CCreateClient_003Eb__0()
			{
				transport.OnClientConnected();
			}

			internal void _003CCreateClient_003Eb__1()
			{
				transport.OnClientDisconnected();
			}

			internal void _003CCreateClient_003Eb__2(byte[] data, int channel)
			{
				transport.OnClientDataReceived(new ArraySegment<byte>(data), channel);
			}
		}

		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003CConnect_003Ed__23 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncVoidMethodBuilder _003C_003Et__builder;

			public LegacyClient _003C_003E4__this;

			public string host;

			private Task _003CconnectedCompleteTask_003E5__2;

			private Task _003CtimeOutTask_003E5__3;

			private TaskAwaiter<Task> _003C_003Eu__1;

			private void MoveNext()
			{
				int num = _003C_003E1__state;
				LegacyClient legacyClient = _003C_003E4__this;
				try
				{
					if (num != 0)
					{
						legacyClient.cancelToken = new CancellationTokenSource();
					}
					try
					{
						TaskAwaiter<Task> awaiter;
						if (num != 0)
						{
							legacyClient.hostSteamID = new CSteamID(ulong.Parse(host));
							legacyClient.connectedComplete = new TaskCompletionSource<Task>();
							legacyClient.OnConnected += legacyClient.SetConnectedComplete;
							legacyClient.SendInternal(legacyClient.hostSteamID, InternalMessages.CONNECT);
							_003CconnectedCompleteTask_003E5__2 = legacyClient.connectedComplete.Task;
							_003CtimeOutTask_003E5__3 = Task.Delay(legacyClient.ConnectionTimeout, legacyClient.cancelToken.Token);
							awaiter = Task.WhenAny(_003CconnectedCompleteTask_003E5__2, _003CtimeOutTask_003E5__3).GetAwaiter();
							if (!awaiter.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = awaiter;
								_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
								return;
							}
						}
						else
						{
							awaiter = _003C_003Eu__1;
							_003C_003Eu__1 = default(TaskAwaiter<Task>);
							num = (_003C_003E1__state = -1);
						}
						if (awaiter.GetResult() != _003CconnectedCompleteTask_003E5__2)
						{
							if (legacyClient.cancelToken.IsCancellationRequested)
							{
								UnityEngine.Debug.LogError("The connection attempt was cancelled.");
							}
							else if (_003CtimeOutTask_003E5__3.IsCompleted)
							{
								UnityEngine.Debug.LogError("Connection to " + host + " timed out.");
							}
							legacyClient.OnConnected -= legacyClient.SetConnectedComplete;
							legacyClient.OnConnectionFailed(legacyClient.hostSteamID);
						}
						legacyClient.OnConnected -= legacyClient.SetConnectedComplete;
						_003CconnectedCompleteTask_003E5__2 = null;
						_003CtimeOutTask_003E5__3 = null;
					}
					catch (FormatException)
					{
						UnityEngine.Debug.LogError("Connection string was not in the right format. Did you enter a SteamId?");
						legacyClient.Error = true;
						legacyClient.OnConnectionFailed(legacyClient.hostSteamID);
					}
					catch (Exception ex2)
					{
						UnityEngine.Debug.LogError(ex2.Message);
						legacyClient.Error = true;
						legacyClient.OnConnectionFailed(legacyClient.hostSteamID);
					}
					finally
					{
						if (num < 0 && legacyClient.Error)
						{
							legacyClient.OnConnectionFailed(CSteamID.Nil);
						}
					}
				}
				catch (Exception exception)
				{
					_003C_003E1__state = -2;
					_003C_003Et__builder.SetException(exception);
					return;
				}
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetResult();
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
				_003C_003Et__builder.SetStateMachine(stateMachine);
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		private TimeSpan ConnectionTimeout;

		private CSteamID hostSteamID = CSteamID.Nil;

		private TaskCompletionSource<Task> connectedComplete;

		private CancellationTokenSource cancelToken;

		public bool Connected { get; private set; }

		public bool Error { get; private set; }

		private event Action<byte[], int> OnReceivedData;

		private event Action OnConnected;

		private event Action OnDisconnected;

		private LegacyClient(FizzySteamworks transport)
			: base(transport)
		{
			ConnectionTimeout = TimeSpan.FromSeconds(Math.Max(1, transport.Timeout));
		}

		public static LegacyClient CreateClient(FizzySteamworks transport, string host)
		{
			_003C_003Ec__DisplayClass22_0 _003C_003Ec__DisplayClass22_ = new _003C_003Ec__DisplayClass22_0();
			_003C_003Ec__DisplayClass22_.transport = transport;
			LegacyClient legacyClient = new LegacyClient(_003C_003Ec__DisplayClass22_.transport);
			legacyClient.OnConnected += _003C_003Ec__DisplayClass22_._003CCreateClient_003Eb__0;
			legacyClient.OnDisconnected += _003C_003Ec__DisplayClass22_._003CCreateClient_003Eb__1;
			legacyClient.OnReceivedData += _003C_003Ec__DisplayClass22_._003CCreateClient_003Eb__2;
			if (SteamManager.Initialized)
			{
				legacyClient.Connect(host);
			}
			else
			{
				UnityEngine.Debug.LogError("SteamWorks not initialized");
				legacyClient.OnConnectionFailed(CSteamID.Nil);
			}
			return legacyClient;
		}

		[AsyncStateMachine(typeof(_003CConnect_003Ed__23))]
		private void Connect(string host)
		{
			_003CConnect_003Ed__23 stateMachine = default(_003CConnect_003Ed__23);
			stateMachine._003C_003E4__this = this;
			stateMachine.host = host;
			stateMachine._003C_003Et__builder = AsyncVoidMethodBuilder.Create();
			stateMachine._003C_003E1__state = -1;
			AsyncVoidMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
			_003C_003Et__builder.Start(ref stateMachine);
		}

		public void Disconnect()
		{
			UnityEngine.Debug.Log("Sending Disconnect message");
			SendInternal(hostSteamID, InternalMessages.DISCONNECT);
			Dispose();
			CancellationTokenSource cancellationTokenSource = cancelToken;
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			WaitForClose(hostSteamID);
		}

		private void SetConnectedComplete()
		{
			connectedComplete.SetResult(connectedComplete.Task);
		}

		protected override void OnReceiveData(byte[] data, CSteamID clientSteamID, int channel)
		{
			if (clientSteamID != hostSteamID)
			{
				UnityEngine.Debug.LogError("Received a message from an unknown");
			}
			else
			{
				this.OnReceivedData(data, channel);
			}
		}

		protected override void OnNewConnection(P2PSessionRequest_t result)
		{
			if (hostSteamID == result.m_steamIDRemote)
			{
				SteamNetworking.AcceptP2PSessionWithUser(result.m_steamIDRemote);
			}
			else
			{
				UnityEngine.Debug.LogError("P2P Acceptance Request from unknown host ID.");
			}
		}

		protected override void OnReceiveInternalData(InternalMessages type, CSteamID clientSteamID)
		{
			switch (type)
			{
			case InternalMessages.ACCEPT_CONNECT:
				if (!Connected)
				{
					Connected = true;
					this.OnConnected();
					UnityEngine.Debug.Log("Connection established.");
				}
				break;
			case InternalMessages.DISCONNECT:
				if (Connected)
				{
					Connected = false;
					UnityEngine.Debug.Log("Disconnected.");
					this.OnDisconnected();
				}
				break;
			default:
				UnityEngine.Debug.Log("Received unknown message type");
				break;
			}
		}

		public void Send(byte[] data, int channelId)
		{
			Send(hostSteamID, data, channelId);
		}

		protected override void OnConnectionFailed(CSteamID remoteId)
		{
			this.OnDisconnected();
		}

		public void FlushData()
		{
		}
	}
}
