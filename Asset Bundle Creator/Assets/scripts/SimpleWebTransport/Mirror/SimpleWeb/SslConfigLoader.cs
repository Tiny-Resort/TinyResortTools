using System.IO;
using UnityEngine;

namespace Mirror.SimpleWeb
{
	internal class SslConfigLoader
	{
		internal struct Cert
		{
			public string path;

			public string password;
		}

		internal static SslConfig Load(SimpleWebTransport transport)
		{
			if (!transport.sslEnabled)
			{
				return default(SslConfig);
			}
			Cert cert = LoadCertJson(transport.sslCertJson);
			return new SslConfig(transport.sslEnabled, sslProtocols: transport.sslProtocols, certPath: cert.path, certPassword: cert.password);
		}

		internal static Cert LoadCertJson(string certJsonPath)
		{
			Cert result = JsonUtility.FromJson<Cert>(File.ReadAllText(certJsonPath));
			if (string.IsNullOrEmpty(result.path))
			{
				throw new InvalidDataException("Cert Json didn't not contain \"path\"");
			}
			if (string.IsNullOrEmpty(result.password))
			{
				result.password = string.Empty;
			}
			return result;
		}
	}
}
