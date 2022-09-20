using System;

namespace Mirror
{
	[AttributeUsage(AttributeTargets.Method)]
	public class CommandAttribute : Attribute
	{
		public int channel;

		public bool requiresAuthority = true;
	}
}
