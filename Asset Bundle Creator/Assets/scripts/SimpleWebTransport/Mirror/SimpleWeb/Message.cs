using System;

namespace Mirror.SimpleWeb
{
	public struct Message
	{
		public readonly int connId;

		public readonly EventType type;

		public readonly ArrayBuffer data;

		public readonly Exception exception;

		public Message(EventType type)
		{
			this = default(Message);
			this.type = type;
		}

		public Message(ArrayBuffer data)
		{
			this = default(Message);
			type = EventType.Data;
			this.data = data;
		}

		public Message(Exception exception)
		{
			this = default(Message);
			type = EventType.Error;
			this.exception = exception;
		}

		public Message(int connId, EventType type)
		{
			this = default(Message);
			this.connId = connId;
			this.type = type;
		}

		public Message(int connId, ArrayBuffer data)
		{
			this = default(Message);
			this.connId = connId;
			type = EventType.Data;
			this.data = data;
		}

		public Message(int connId, Exception exception)
		{
			this = default(Message);
			this.connId = connId;
			type = EventType.Error;
			this.exception = exception;
		}
	}
}
