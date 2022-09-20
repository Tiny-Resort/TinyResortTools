using System;
using System.Net;
using UnityEngine;

namespace Mirror.Discovery
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Network/NetworkDiscovery")]
	public class NetworkDiscovery : NetworkDiscoveryBase<ServerRequest, ServerResponse>
	{
		[Tooltip("Transport to be advertised during discovery")]
		public Transport transport;

		[Tooltip("Invoked when a server is found")]
		public ServerFoundUnityEvent OnServerFound;

		public long ServerId { get; private set; }

		public override void Start()
		{
			ServerId = NetworkDiscoveryBase<ServerRequest, ServerResponse>.RandomLong();
			if (transport == null)
			{
				transport = Transport.activeTransport;
			}
			base.Start();
		}

		protected override ServerResponse ProcessRequest(ServerRequest request, IPEndPoint endpoint)
		{
			try
			{
				ServerResponse result = default(ServerResponse);
				result.serverId = ServerId;
				result.uri = transport.ServerUri();
				return result;
			}
			catch (NotImplementedException)
			{
				Debug.LogError(string.Format("Transport {0} does not support network discovery", transport));
				throw;
			}
		}

		protected override ServerRequest GetRequest()
		{
			return default(ServerRequest);
		}

		protected override void ProcessResponse(ServerResponse response, IPEndPoint endpoint)
		{
			response.EndPoint = endpoint;
			UriBuilder uriBuilder = new UriBuilder(response.uri)
			{
				Host = response.EndPoint.Address.ToString()
			};
			response.uri = uriBuilder.Uri;
			OnServerFound.Invoke(response);
		}
	}
}
