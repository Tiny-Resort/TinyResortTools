using System.Runtime.InteropServices;

namespace Mirror
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct NotReadyMessage : NetworkMessage
	{
	}
}
