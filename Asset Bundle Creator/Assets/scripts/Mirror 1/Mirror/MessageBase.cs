using System;

namespace Mirror
{
	[Obsolete("Implement NetworkMessage instead. Use extension methods instead of Serialize/Deserialize, see https://github.com/vis2k/Mirror/pull/2317", true)]
	public class MessageBase : IMessageBase
	{
	}
}
