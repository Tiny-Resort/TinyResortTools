using System;

namespace Mirror
{
	[AttributeUsage(AttributeTargets.Method)]
	public class TargetRpcAttribute : Attribute
	{
		public int channel;
	}
}
