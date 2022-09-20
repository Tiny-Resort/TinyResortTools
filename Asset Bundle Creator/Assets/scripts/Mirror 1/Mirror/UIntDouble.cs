using System.Runtime.InteropServices;

namespace Mirror
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct UIntDouble
	{
		[FieldOffset(0)]
		public double doubleValue;

		[FieldOffset(0)]
		public ulong longValue;
	}
}
