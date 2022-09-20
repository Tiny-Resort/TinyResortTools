using System;

namespace Mirror.RemoteCalls
{
	internal class Invoker
	{
		public Type invokeClass;

		public MirrorInvokeType invokeType;

		public CmdDelegate invokeFunction;

		public bool cmdRequiresAuthority;

		public bool AreEqual(Type invokeClass, MirrorInvokeType invokeType, CmdDelegate invokeFunction)
		{
			if (this.invokeClass == invokeClass && this.invokeType == invokeType)
			{
				return this.invokeFunction == invokeFunction;
			}
			return false;
		}
	}
}
