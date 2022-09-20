using System;

namespace Mirror
{
	public static class Channels
	{
		public const int Reliable = 0;

		public const int Unreliable = 1;

		[Obsolete("Use Channels.Reliable instead")]
		public const int DefaultReliable = 0;

		[Obsolete("Use Channels.Unreliable instead")]
		public const int DefaultUnreliable = 1;
	}
}
