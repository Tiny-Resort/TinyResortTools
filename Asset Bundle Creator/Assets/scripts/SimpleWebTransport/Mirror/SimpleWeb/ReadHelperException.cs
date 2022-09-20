using System;
using System.Runtime.Serialization;

namespace Mirror.SimpleWeb
{
	[Serializable]
	public class ReadHelperException : Exception
	{
		public ReadHelperException(string message)
			: base(message)
		{
		}

		protected ReadHelperException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
