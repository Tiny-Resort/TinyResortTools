using System;
using UnityEngine;

namespace Mirror
{
	[HelpURL("https://mirror-networking.gitbook.io/docs/components/network-authenticators")]
	public abstract class NetworkAuthenticator : MonoBehaviour
	{
		[Header("Event Listeners (optional)")]
		[Tooltip("Mirror has an internal subscriber to this event. You can add your own here.")]
		public UnityEventNetworkConnection OnServerAuthenticated = new UnityEventNetworkConnection();

		[Tooltip("Mirror has an internal subscriber to this event. You can add your own here.")]
		public UnityEventNetworkConnection OnClientAuthenticated = new UnityEventNetworkConnection();

		public virtual void OnStartServer()
		{
		}

		public virtual void OnStopServer()
		{
		}

		public abstract void OnServerAuthenticate(NetworkConnection conn);

		protected void ServerAccept(NetworkConnection conn)
		{
			OnServerAuthenticated.Invoke(conn);
		}

		protected void ServerReject(NetworkConnection conn)
		{
			conn.Disconnect();
		}

		public virtual void OnStartClient()
		{
		}

		public virtual void OnStopClient()
		{
		}

		[Obsolete("Remove the NetworkConnection parameter from your override and use NetworkClient.connection instead")]
		public virtual void OnClientAuthenticate(NetworkConnection conn)
		{
			OnClientAuthenticate();
		}

		public abstract void OnClientAuthenticate();

		[Obsolete("Remove the NetworkConnection parameter from your override and use NetworkClient.connection instead")]
		protected void ClientAccept(NetworkConnection conn)
		{
			ClientAccept();
		}

		protected void ClientAccept()
		{
			OnClientAuthenticated.Invoke(NetworkClient.connection);
		}

		[Obsolete("Remove the NetworkConnection parameter from your override and use NetworkClient.connection instead")]
		protected void ClientReject(NetworkConnection conn)
		{
			ClientReject();
		}

		protected void ClientReject()
		{
			NetworkClient.connection.isAuthenticated = false;
			NetworkClient.connection.Disconnect();
		}

		private void OnValidate()
		{
		}
	}
}
