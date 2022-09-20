using System.Security.Authentication;

namespace Mirror.SimpleWeb
{
	public struct SslConfig
	{
		public readonly bool enabled;

		public readonly string certPath;

		public readonly string certPassword;

		public readonly SslProtocols sslProtocols;

		public SslConfig(bool enabled, string certPath, string certPassword, SslProtocols sslProtocols)
		{
			this.enabled = enabled;
			this.certPath = certPath;
			this.certPassword = certPassword;
			this.sslProtocols = sslProtocols;
		}
	}
}
