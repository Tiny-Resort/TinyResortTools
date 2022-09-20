using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
	public class GUIConsole : MonoBehaviour
	{
		public int height = 150;

		public int maxLogCount = 50;

		private Queue<LogEntry> log = new Queue<LogEntry>();

		public KeyCode hotKey = KeyCode.F12;

		private bool visible;

		private Vector2 scroll = Vector2.zero;

		private void Awake()
		{
			Application.logMessageReceived += OnLog;
		}

		private void OnLog(string message, string stackTrace, LogType type)
		{
			int num;
			if (type != 0)
			{
				num = ((type == LogType.Exception) ? 1 : 0);
				if (num == 0)
				{
					goto IL_0023;
				}
			}
			else
			{
				num = 1;
			}
			if (!string.IsNullOrWhiteSpace(stackTrace))
			{
				message = message + "\n" + stackTrace;
			}
			goto IL_0023;
			IL_0023:
			log.Enqueue(new LogEntry(message, type));
			if (log.Count > maxLogCount)
			{
				log.Dequeue();
			}
			if (num != 0)
			{
				visible = true;
			}
			scroll.y = float.MaxValue;
		}

		private void Update()
		{
			if (Input.GetKeyDown(hotKey))
			{
				visible = !visible;
			}
		}

		private void OnGUI()
		{
			if (!visible)
			{
				return;
			}
			scroll = GUILayout.BeginScrollView(scroll, "Box", GUILayout.Width(Screen.width), GUILayout.Height(height));
			foreach (LogEntry item in log)
			{
				if (item.type == LogType.Error || item.type == LogType.Exception)
				{
					GUI.color = Color.red;
				}
				else if (item.type == LogType.Warning)
				{
					GUI.color = Color.yellow;
				}
				GUILayout.Label(item.message);
				GUI.color = Color.white;
			}
			GUILayout.EndScrollView();
		}
	}
}
