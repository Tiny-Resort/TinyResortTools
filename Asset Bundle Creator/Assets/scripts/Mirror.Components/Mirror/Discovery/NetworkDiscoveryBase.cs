using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Mirror.Discovery
{
	[DisallowMultipleComponent]
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-discovery")]
	public abstract class NetworkDiscoveryBase<Request, Response> : MonoBehaviour where Request : NetworkMessage where Response : NetworkMessage
	{
		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003CServerListenAsync_003Ed__15 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder _003C_003Et__builder;

			public NetworkDiscoveryBase<Request, Response> _003C_003E4__this;

			private TaskAwaiter _003C_003Eu__1;

			private void MoveNext()
			{
				int num = _003C_003E1__state;
				NetworkDiscoveryBase<Request, Response> networkDiscoveryBase = _003C_003E4__this;
				try
				{
					while (true)
					{
						try
						{
							TaskAwaiter awaiter;
							if (num != 0)
							{
								awaiter = networkDiscoveryBase.ReceiveRequestAsync(networkDiscoveryBase.serverUdpClient).GetAwaiter();
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
								_003C_003Eu__1 = default(TaskAwaiter);
								num = (_003C_003E1__state = -1);
							}
							awaiter.GetResult();
						}
						catch (ObjectDisposedException)
						{
							break;
						}
						catch (Exception)
						{
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

		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003CReceiveRequestAsync_003Ed__16 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder _003C_003Et__builder;

			public UdpClient udpClient;

			public NetworkDiscoveryBase<Request, Response> _003C_003E4__this;

			private TaskAwaiter<UdpReceiveResult> _003C_003Eu__1;

			private void MoveNext()
			{
				int num = _003C_003E1__state;
				NetworkDiscoveryBase<Request, Response> networkDiscoveryBase = _003C_003E4__this;
				try
				{
					TaskAwaiter<UdpReceiveResult> awaiter;
					if (num != 0)
					{
						awaiter = udpClient.ReceiveAsync().GetAwaiter();
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
						_003C_003Eu__1 = default(TaskAwaiter<UdpReceiveResult>);
						num = (_003C_003E1__state = -1);
					}
					UdpReceiveResult result = awaiter.GetResult();
					PooledNetworkReader reader = NetworkReaderPool.GetReader(result.Buffer);
					try
					{
						if (reader.ReadLong() != networkDiscoveryBase.secretHandshake)
						{
							throw new ProtocolViolationException("Invalid handshake");
						}
						Request request = reader.Read<Request>();
						networkDiscoveryBase.ProcessClientRequest(request, result.RemoteEndPoint);
					}
					finally
					{
						if (num < 0 && reader != null)
						{
							((IDisposable)reader).Dispose();
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

		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003CClientListenAsync_003Ed__21 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder _003C_003Et__builder;

			public NetworkDiscoveryBase<Request, Response> _003C_003E4__this;

			private TaskAwaiter _003C_003Eu__1;

			private void MoveNext()
			{
				int num = _003C_003E1__state;
				NetworkDiscoveryBase<Request, Response> networkDiscoveryBase = _003C_003E4__this;
				try
				{
					while (true)
					{
						try
						{
							TaskAwaiter awaiter;
							if (num != 0)
							{
								awaiter = networkDiscoveryBase.ReceiveGameBroadcastAsync(networkDiscoveryBase.clientUdpClient).GetAwaiter();
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
								_003C_003Eu__1 = default(TaskAwaiter);
								num = (_003C_003E1__state = -1);
							}
							awaiter.GetResult();
						}
						catch (ObjectDisposedException)
						{
							break;
						}
						catch (Exception exception)
						{
							UnityEngine.Debug.LogException(exception);
						}
					}
				}
				catch (Exception exception2)
				{
					_003C_003E1__state = -2;
					_003C_003Et__builder.SetException(exception2);
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

		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003CReceiveGameBroadcastAsync_003Ed__24 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder _003C_003Et__builder;

			public UdpClient udpClient;

			public NetworkDiscoveryBase<Request, Response> _003C_003E4__this;

			private TaskAwaiter<UdpReceiveResult> _003C_003Eu__1;

			private void MoveNext()
			{
				int num = _003C_003E1__state;
				NetworkDiscoveryBase<Request, Response> networkDiscoveryBase = _003C_003E4__this;
				try
				{
					TaskAwaiter<UdpReceiveResult> awaiter;
					if (num != 0)
					{
						awaiter = udpClient.ReceiveAsync().GetAwaiter();
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
						_003C_003Eu__1 = default(TaskAwaiter<UdpReceiveResult>);
						num = (_003C_003E1__state = -1);
					}
					UdpReceiveResult result = awaiter.GetResult();
					PooledNetworkReader reader = NetworkReaderPool.GetReader(result.Buffer);
					try
					{
						if (reader.ReadLong() == networkDiscoveryBase.secretHandshake)
						{
							Response response = reader.Read<Response>();
							networkDiscoveryBase.ProcessResponse(response, result.RemoteEndPoint);
						}
					}
					finally
					{
						if (num < 0 && reader != null)
						{
							((IDisposable)reader).Dispose();
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

		[HideInInspector]
		public long secretHandshake;

		[SerializeField]
		[Tooltip("The UDP port the server will listen for multi-cast messages")]
		protected int serverBroadcastListenPort = 47777;

		[SerializeField]
		[Tooltip("If true, broadcasts a discovery request every ActiveDiscoveryInterval seconds")]
		public bool enableActiveDiscovery = true;

		[SerializeField]
		[Tooltip("Time in seconds between multi-cast messages")]
		[Range(1f, 60f)]
		private float ActiveDiscoveryInterval = 3f;

		protected UdpClient serverUdpClient;

		protected UdpClient clientUdpClient;

		public static bool SupportedOnThisPlatform
		{
			get
			{
				return Application.platform != RuntimePlatform.WebGLPlayer;
			}
		}

		public static long RandomLong()
		{
			int num = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			int num2 = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			return num + ((long)num2 << 32);
		}

		public virtual void Start()
		{
		}

		private void OnApplicationQuit()
		{
			Shutdown();
		}

		private void OnDisable()
		{
			Shutdown();
		}

		private void OnDestroy()
		{
			Shutdown();
		}

		private void Shutdown()
		{
			if (serverUdpClient != null)
			{
				try
				{
					serverUdpClient.Close();
				}
				catch (Exception)
				{
				}
				serverUdpClient = null;
			}
			if (clientUdpClient != null)
			{
				try
				{
					clientUdpClient.Close();
				}
				catch (Exception)
				{
				}
				clientUdpClient = null;
			}
			CancelInvoke();
		}

		public void AdvertiseServer()
		{
			if (!SupportedOnThisPlatform)
			{
				throw new PlatformNotSupportedException("Network discovery not supported in this platform");
			}
			StopDiscovery();
			serverUdpClient = new UdpClient(serverBroadcastListenPort)
			{
				EnableBroadcast = true,
				MulticastLoopback = false
			};
			ServerListenAsync();
		}

		[AsyncStateMachine(typeof(NetworkDiscoveryBase<, >._003CServerListenAsync_003Ed__15))]
		public Task ServerListenAsync()
		{
			_003CServerListenAsync_003Ed__15 stateMachine = default(_003CServerListenAsync_003Ed__15);
			stateMachine._003C_003E4__this = this;
			stateMachine._003C_003Et__builder = AsyncTaskMethodBuilder.Create();
			stateMachine._003C_003E1__state = -1;
			AsyncTaskMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
			_003C_003Et__builder.Start(ref stateMachine);
			return stateMachine._003C_003Et__builder.Task;
		}

		[AsyncStateMachine(typeof(NetworkDiscoveryBase<, >._003CReceiveRequestAsync_003Ed__16))]
		private Task ReceiveRequestAsync(UdpClient udpClient)
		{
			_003CReceiveRequestAsync_003Ed__16 stateMachine = default(_003CReceiveRequestAsync_003Ed__16);
			stateMachine._003C_003E4__this = this;
			stateMachine.udpClient = udpClient;
			stateMachine._003C_003Et__builder = AsyncTaskMethodBuilder.Create();
			stateMachine._003C_003E1__state = -1;
			AsyncTaskMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
			_003C_003Et__builder.Start(ref stateMachine);
			return stateMachine._003C_003Et__builder.Task;
		}

		protected virtual void ProcessClientRequest(Request request, IPEndPoint endpoint)
		{
			Response val = ProcessRequest(request, endpoint);
			if (val == null)
			{
				return;
			}
			using (PooledNetworkWriter pooledNetworkWriter = NetworkWriterPool.GetWriter())
			{
				try
				{
					pooledNetworkWriter.WriteLong(secretHandshake);
					pooledNetworkWriter.Write(val);
					ArraySegment<byte> arraySegment = pooledNetworkWriter.ToArraySegment();
					serverUdpClient.Send(arraySegment.Array, arraySegment.Count, endpoint);
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception, this);
				}
			}
		}

		protected abstract Response ProcessRequest(Request request, IPEndPoint endpoint);

		public void StartDiscovery()
		{
			if (!SupportedOnThisPlatform)
			{
				throw new PlatformNotSupportedException("Network discovery not supported in this platform");
			}
			StopDiscovery();
			try
			{
				clientUdpClient = new UdpClient(0)
				{
					EnableBroadcast = true,
					MulticastLoopback = false
				};
			}
			catch (Exception)
			{
				Shutdown();
				throw;
			}
			ClientListenAsync();
			if (enableActiveDiscovery)
			{
				InvokeRepeating("BroadcastDiscoveryRequest", 0f, ActiveDiscoveryInterval);
			}
		}

		public void StopDiscovery()
		{
			Shutdown();
		}

		[AsyncStateMachine(typeof(NetworkDiscoveryBase<, >._003CClientListenAsync_003Ed__21))]
		public Task ClientListenAsync()
		{
			_003CClientListenAsync_003Ed__21 stateMachine = default(_003CClientListenAsync_003Ed__21);
			stateMachine._003C_003E4__this = this;
			stateMachine._003C_003Et__builder = AsyncTaskMethodBuilder.Create();
			stateMachine._003C_003E1__state = -1;
			AsyncTaskMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
			_003C_003Et__builder.Start(ref stateMachine);
			return stateMachine._003C_003Et__builder.Task;
		}

		public void BroadcastDiscoveryRequest()
		{
			if (clientUdpClient == null)
			{
				return;
			}
			if (NetworkClient.isConnected)
			{
				StopDiscovery();
				return;
			}
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, serverBroadcastListenPort);
			using (PooledNetworkWriter pooledNetworkWriter = NetworkWriterPool.GetWriter())
			{
				pooledNetworkWriter.WriteLong(secretHandshake);
				try
				{
					Request request = GetRequest();
					pooledNetworkWriter.Write(request);
					ArraySegment<byte> arraySegment = pooledNetworkWriter.ToArraySegment();
					clientUdpClient.SendAsync(arraySegment.Array, arraySegment.Count, endPoint);
				}
				catch (Exception)
				{
				}
			}
		}

		protected virtual Request GetRequest()
		{
			return default(Request);
		}

		[AsyncStateMachine(typeof(NetworkDiscoveryBase<, >._003CReceiveGameBroadcastAsync_003Ed__24))]
		private Task ReceiveGameBroadcastAsync(UdpClient udpClient)
		{
			_003CReceiveGameBroadcastAsync_003Ed__24 stateMachine = default(_003CReceiveGameBroadcastAsync_003Ed__24);
			stateMachine._003C_003E4__this = this;
			stateMachine.udpClient = udpClient;
			stateMachine._003C_003Et__builder = AsyncTaskMethodBuilder.Create();
			stateMachine._003C_003E1__state = -1;
			AsyncTaskMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
			_003C_003Et__builder.Start(ref stateMachine);
			return stateMachine._003C_003Et__builder.Task;
		}

		protected abstract void ProcessResponse(Response response, IPEndPoint endpoint);
	}
}
