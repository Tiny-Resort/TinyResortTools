using System.Runtime.InteropServices;

namespace Mirror.Discovery
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ServerRequest : NetworkMessage
	{
	}
}
