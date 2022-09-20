using System;

namespace Mirror
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ClientRpcAttribute : Attribute
	{
		public int channel;

		public bool includeOwner = true;
	}
}
