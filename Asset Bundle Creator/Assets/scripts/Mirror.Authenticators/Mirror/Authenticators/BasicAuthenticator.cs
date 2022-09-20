using System;
using System.Collections;
using UnityEngine;

namespace Mirror.Authenticators
{
	[AddComponentMenu("Network/Authenticators/BasicAuthenticator")]
	public class BasicAuthenticator : NetworkAuthenticator
	{
		public struct AuthRequestMessage : NetworkMessage
		{
			public string authUsername;

			public string authPassword;
		}

		public struct AuthResponseMessage : NetworkMessage
		{
			public byte code;

			public string message;
		}

		[Header("Custom Properties")]
		public string username;

		public string password;

		public override void OnStartServer()
		{
			NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
		}

		public override void OnStopServer()
		{
			NetworkServer.UnregisterHandler<AuthRequestMessage>();
		}

		public override void OnServerAuthenticate(NetworkConnection conn)
		{
		}

		public void OnAuthRequestMessage(NetworkConnection conn, AuthRequestMessage msg)
		{
			if (msg.authUsername == username && msg.authPassword == password)
			{
				AuthResponseMessage authResponseMessage = default(AuthResponseMessage);
				authResponseMessage.code = 100;
				authResponseMessage.message = "Success";
				AuthResponseMessage message = authResponseMessage;
				conn.Send(message);
				ServerAccept(conn);
			}
			else
			{
				AuthResponseMessage authResponseMessage = default(AuthResponseMessage);
				authResponseMessage.code = 200;
				authResponseMessage.message = "Invalid Credentials";
				AuthResponseMessage message2 = authResponseMessage;
				conn.Send(message2);
				conn.isAuthenticated = false;
				StartCoroutine(DelayedDisconnect(conn, 1f));
			}
		}

		private IEnumerator DelayedDisconnect(NetworkConnection conn, float waitTime)
		{
			yield return new WaitForSeconds(waitTime);
			ServerReject(conn);
		}

		public override void OnStartClient()
		{
			NetworkClient.RegisterHandler((Action<AuthResponseMessage>)OnAuthResponseMessage, false);
		}

		public override void OnStopClient()
		{
			NetworkClient.UnregisterHandler<AuthResponseMessage>();
		}

		public override void OnClientAuthenticate()
		{
			AuthRequestMessage authRequestMessage = default(AuthRequestMessage);
			authRequestMessage.authUsername = username;
			authRequestMessage.authPassword = password;
			AuthRequestMessage message = authRequestMessage;
			NetworkClient.connection.Send(message);
		}

		[Obsolete("Call OnAuthResponseMessage without the NetworkConnection parameter. It always points to NetworkClient.connection anyway.")]
		public void OnAuthResponseMessage(NetworkConnection conn, AuthResponseMessage msg)
		{
			OnAuthResponseMessage(msg);
		}

		public void OnAuthResponseMessage(AuthResponseMessage msg)
		{
			if (msg.code == 100)
			{
				ClientAccept();
				return;
			}
			Debug.LogError("Authentication Response: " + msg.message);
			ClientReject();
		}
	}
}
