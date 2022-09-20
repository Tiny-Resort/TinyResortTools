using UnityEngine;

namespace Mirror
{
	internal struct LogEntry
	{
		public string message;

		public LogType type;

		public LogEntry(string message, LogType type)
		{
			this.message = message;
			this.type = type;
		}
	}
}
