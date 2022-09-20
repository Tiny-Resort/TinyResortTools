using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirror.Authenticators
{
	[AddComponentMenu("Network/Authenticators/TimeoutAuthenticator")]
	public class TimeoutAuthenticator : NetworkAuthenticator
	{
		public NetworkAuthenticator authenticator;

		[Range(0f, 600f)]
		[Tooltip("Timeout to auto-disconnect in seconds. Set to 0 for no timeout.")]
		public float timeout = 60f;

		public void Awake()
		{
			authenticator.OnServerAuthenticated.AddListener(_003CAwake_003Eb__2_0);
			authenticator.OnClientAuthenticated.AddListener(_003CAwake_003Eb__2_1);
		}

		public override void OnStartServer()
		{
			authenticator.OnStartServer();
		}

		public override void OnStopServer()
		{
			authenticator.OnStopServer();
		}

		public override void OnStartClient()
		{
			authenticator.OnStartClient();
		}

		public override void OnStopClient()
		{
			authenticator.OnStopClient();
		}

		public override void OnServerAuthenticate(NetworkConnection conn)
		{
			authenticator.OnServerAuthenticate(conn);
			if (timeout > 0f)
			{
				StartCoroutine(BeginAuthentication(conn));
			}
		}

		public override void OnClientAuthenticate()
		{
			authenticator.OnClientAuthenticate();
			if (timeout > 0f)
			{
				StartCoroutine(BeginAuthentication(NetworkClient.connection));
			}
		}

		private IEnumerator BeginAuthentication(NetworkConnection conn)
		{
			yield return new WaitForSecondsRealtime(timeout);
			if (!conn.isAuthenticated)
			{
				conn.Disconnect();
			}
		}

		[CompilerGenerated]
		private void _003CAwake_003Eb__2_0(NetworkConnection connection)
		{
			OnServerAuthenticated.Invoke(connection);
		}

		[CompilerGenerated]
		private void _003CAwake_003Eb__2_1(NetworkConnection connection)
		{
			OnClientAuthenticated.Invoke(connection);
		}
	}
}
