using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;

namespace Mirror.FizzySteam
{
	public class NextClient : NextCommon, IClient
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass25_0
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

			internal void _003CCreateClient_003Eb__2(byte[] data, int ch)
			{
				transport.OnClientDataReceived(new ArraySegment<byte>(data), ch);
			}
		}

		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003CConnect_003Ed__26 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncVoidMethodBuilder _003C_003Et__builder;

			public NextClient _003C_003E4__this;

			public string host;

			private Task _003CconnectedCompleteTask_003E5__2;

			private Task _003CtimeOutTask_003E5__3;

			private TaskAwaiter<Task> _003C_003Eu__1;

			private void MoveNext()
			{
				int num = _003C_003E1__state;
				NextClient nextClient = _003C_003E4__this;
				try
				{
					if (num != 0)
					{
						nextClient.cancelToken = new CancellationTokenSource();
						nextClient.c_onConnectionChange = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(nextClient.OnConnectionStatusChanged);
					}
					try
					{
						TaskAwaiter<Task> awaiter;
						if (num != 0)
						{
							nextClient.hostSteamID = new CSteamID(ulong.Parse(host));
							nextClient.connectedComplete = new TaskCompletionSource<Task>();
							nextClient.OnConnected += nextClient.SetConnectedComplete;
							SteamNetworkingIdentity identityRemote = default(SteamNetworkingIdentity);
							identityRemote.SetSteamID(nextClient.hostSteamID);
							SteamNetworkingConfigValue_t[] array = new SteamNetworkingConfigValue_t[0];
							nextClient.HostConnection = SteamNetworkingSockets.ConnectP2P(ref identityRemote, 0, array.Length, array);
							_003CconnectedCompleteTask_003E5__2 = nextClient.connectedComplete.Task;
							_003CtimeOutTask_003E5__3 = Task.Delay(nextClient.ConnectionTimeout, nextClient.cancelToken.Token);
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
							if (nextClient.cancelToken.IsCancellationRequested)
							{
								UnityEngine.Debug.LogError("The connection attempt was cancelled.");
							}
							else if (_003CtimeOutTask_003E5__3.IsCompleted)
							{
								UnityEngine.Debug.LogError("Connection to " + host + " timed out.");
							}
							nextClient.OnConnected -= nextClient.SetConnectedComplete;
							nextClient.OnConnectionFailed();
						}
						nextClient.OnConnected -= nextClient.SetConnectedComplete;
						_003CconnectedCompleteTask_003E5__2 = null;
						_003CtimeOutTask_003E5__3 = null;
					}
					catch (FormatException)
					{
						UnityEngine.Debug.LogError("Connection string was not in the right format. Did you enter a SteamId?");
						nextClient.Error = true;
						nextClient.OnConnectionFailed();
					}
					catch (Exception ex2)
					{
						UnityEngine.Debug.LogError(ex2.Message);
						nextClient.Error = true;
						nextClient.OnConnectionFailed();
					}
					finally
					{
						if (num < 0 && nextClient.Error)
						{
							UnityEngine.Debug.LogError("Connection failed.");
							nextClient.OnConnectionFailed();
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

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass31_0
		{
			public byte[] data;

			public int ch;

			public NextClient _003C_003E4__this;

			internal void _003CReceiveData_003Eb__0()
			{
				_003C_003E4__this.OnReceivedData(data, ch);
			}
		}

		private TimeSpan ConnectionTimeout;

		private Callback<SteamNetConnectionStatusChangedCallback_t> c_onConnectionChange;

		private CancellationTokenSource cancelToken;

		private TaskCompletionSource<Task> connectedComplete;

		private CSteamID hostSteamID = CSteamID.Nil;

		private HSteamNetConnection HostConnection;

		private List<Action> BufferedData;

		public bool Connected { get; private set; }

		public bool Error { get; private set; }

		private event Action<byte[], int> OnReceivedData;

		private event Action OnConnected;

		private event Action OnDisconnected;

		private NextClient(FizzySteamworks transport)
		{
			ConnectionTimeout = TimeSpan.FromSeconds(Math.Max(1, transport.Timeout));
			BufferedData = new List<Action>();
		}

		public static NextClient CreateClient(FizzySteamworks transport, string host)
		{
			_003C_003Ec__DisplayClass25_0 _003C_003Ec__DisplayClass25_ = new _003C_003Ec__DisplayClass25_0();
			_003C_003Ec__DisplayClass25_.transport = transport;
			NextClient nextClient = new NextClient(_003C_003Ec__DisplayClass25_.transport);
			nextClient.OnConnected += _003C_003Ec__DisplayClass25_._003CCreateClient_003Eb__0;
			nextClient.OnDisconnected += _003C_003Ec__DisplayClass25_._003CCreateClient_003Eb__1;
			nextClient.OnReceivedData += _003C_003Ec__DisplayClass25_._003CCreateClient_003Eb__2;
			if (SteamManager.Initialized)
			{
				nextClient.Connect(host);
			}
			else
			{
				UnityEngine.Debug.LogError("SteamWorks not initialized");
				nextClient.OnConnectionFailed();
			}
			return nextClient;
		}

		[AsyncStateMachine(typeof(_003CConnect_003Ed__26))]
		private void Connect(string host)
		{
			_003CConnect_003Ed__26 stateMachine = default(_003CConnect_003Ed__26);
			stateMachine._003C_003E4__this = this;
			stateMachine.host = host;
			stateMachine._003C_003Et__builder = AsyncVoidMethodBuilder.Create();
			stateMachine._003C_003E1__state = -1;
			AsyncVoidMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
			_003C_003Et__builder.Start(ref stateMachine);
		}

		private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t param)
		{
			param.m_info.m_identityRemote.GetSteamID64();
			if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
			{
				Connected = true;
				this.OnConnected();
				UnityEngine.Debug.Log("Connection established.");
				if (BufferedData.Count <= 0)
				{
					return;
				}
				UnityEngine.Debug.Log(string.Format("{0} received before connection was established. Processing now.", BufferedData.Count));
				{
					foreach (Action bufferedDatum in BufferedData)
					{
						bufferedDatum();
					}
					return;
				}
			}
			if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer || param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
			{
				UnityEngine.Debug.Log("Connection was closed by peer, " + param.m_info.m_szEndDebug);
				Disconnect();
			}
			else
			{
				UnityEngine.Debug.Log("Connection state changed: " + param.m_info.m_eState.ToString() + " - " + param.m_info.m_szEndDebug);
			}
		}

		public void Disconnect()
		{
			CancellationTokenSource cancellationTokenSource = cancelToken;
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			Dispose();
			if (HostConnection.m_HSteamNetConnection != 0)
			{
				UnityEngine.Debug.Log("Sending Disconnect message");
				SteamNetworkingSockets.CloseConnection(HostConnection, 0, "Graceful disconnect", false);
				HostConnection.m_HSteamNetConnection = 0u;
			}
		}

		protected void Dispose()
		{
			if (c_onConnectionChange != null)
			{
				c_onConnectionChange.Dispose();
				c_onConnectionChange = null;
			}
		}

		private void InternalDisconnect()
		{
			Connected = false;
			this.OnDisconnected();
			UnityEngine.Debug.Log("Disconnected.");
			SteamNetworkingSockets.CloseConnection(HostConnection, 0, "Disconnected", false);
		}

		public void ReceiveData()
		{
			IntPtr[] array = new IntPtr[256];
			int num;
			if ((num = SteamNetworkingSockets.ReceiveMessagesOnConnection(HostConnection, array, 256)) <= 0)
			{
				return;
			}
			for (int i = 0; i < num; i++)
			{
				_003C_003Ec__DisplayClass31_0 _003C_003Ec__DisplayClass31_ = new _003C_003Ec__DisplayClass31_0();
				_003C_003Ec__DisplayClass31_._003C_003E4__this = this;
				ValueTuple<byte[], int> valueTuple = ProcessMessage(array[i]);
				_003C_003Ec__DisplayClass31_.data = valueTuple.Item1;
				_003C_003Ec__DisplayClass31_.ch = valueTuple.Item2;
				if (Connected)
				{
					this.OnReceivedData(_003C_003Ec__DisplayClass31_.data, _003C_003Ec__DisplayClass31_.ch);
				}
				else
				{
					BufferedData.Add(_003C_003Ec__DisplayClass31_._003CReceiveData_003Eb__0);
				}
			}
		}

		public void Send(byte[] data, int channelId)
		{
			EResult eResult = SendSocket(HostConnection, data, channelId);
			switch (eResult)
			{
			case EResult.k_EResultNoConnection:
			case EResult.k_EResultInvalidParam:
				UnityEngine.Debug.Log("Connection to server was lost.");
				InternalDisconnect();
				break;
			default:
				UnityEngine.Debug.LogError("Could not send: " + eResult);
				break;
			case EResult.k_EResultOK:
				break;
			}
		}

		private void SetConnectedComplete()
		{
			connectedComplete.SetResult(connectedComplete.Task);
		}

		private void OnConnectionFailed()
		{
			this.OnDisconnected();
		}

		public void FlushData()
		{
			SteamNetworkingSockets.FlushMessagesOnConnection(HostConnection);
		}
	}
}
