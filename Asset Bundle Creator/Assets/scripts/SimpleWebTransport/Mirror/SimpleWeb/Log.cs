using System;
using System.Diagnostics;
using UnityEngine;

namespace Mirror.SimpleWeb
{
	public static class Log
	{
		public enum Levels
		{
			none = 0,
			error = 1,
			warn = 2,
			info = 3,
			verbose = 4
		}

		private const string SIMPLEWEB_LOG_ENABLED = "SIMPLEWEB_LOG_ENABLED";

		private const string DEBUG = "DEBUG";

		public static Levels level;

		public static string BufferToString(byte[] buffer, int offset = 0, int? length = null)
		{
			return BitConverter.ToString(buffer, offset, length ?? buffer.Length);
		}

		[Conditional("SIMPLEWEB_LOG_ENABLED")]
		public static void DumpBuffer(string label, byte[] buffer, int offset, int length)
		{
			if (level >= Levels.verbose)
			{
				UnityEngine.Debug.Log("VERBOSE: <color=blue>" + label + ": " + BufferToString(buffer, offset, length) + "</color>");
			}
		}

		[Conditional("SIMPLEWEB_LOG_ENABLED")]
		public static void DumpBuffer(string label, ArrayBuffer arrayBuffer)
		{
			if (level >= Levels.verbose)
			{
				UnityEngine.Debug.Log("VERBOSE: <color=blue>" + label + ": " + BufferToString(arrayBuffer.array, 0, arrayBuffer.count) + "</color>");
			}
		}

		[Conditional("SIMPLEWEB_LOG_ENABLED")]
		public static void Verbose(string msg, bool showColor = true)
		{
			if (level >= Levels.verbose)
			{
				if (showColor)
				{
					UnityEngine.Debug.Log("VERBOSE: <color=blue>" + msg + "</color>");
				}
				else
				{
					UnityEngine.Debug.Log("VERBOSE: " + msg);
				}
			}
		}

		[Conditional("SIMPLEWEB_LOG_ENABLED")]
		public static void Info(string msg, bool showColor = true)
		{
			if (level >= Levels.info)
			{
				if (showColor)
				{
					UnityEngine.Debug.Log("INFO: <color=blue>" + msg + "</color>");
				}
				else
				{
					UnityEngine.Debug.Log("INFO: " + msg);
				}
			}
		}

		[Conditional("SIMPLEWEB_LOG_ENABLED")]
		public static void InfoException(Exception e)
		{
			if (level >= Levels.info)
			{
				UnityEngine.Debug.Log("INFO_EXCEPTION: <color=blue>" + e.GetType().Name + "</color> Message: " + e.Message);
			}
		}

		[Conditional("SIMPLEWEB_LOG_ENABLED")]
		[Conditional("DEBUG")]
		public static void Warn(string msg, bool showColor = true)
		{
			if (level >= Levels.warn)
			{
				if (showColor)
				{
					UnityEngine.Debug.LogWarning("WARN: <color=orange>" + msg + "</color>");
				}
				else
				{
					UnityEngine.Debug.LogWarning("WARN: " + msg);
				}
			}
		}

		[Conditional("SIMPLEWEB_LOG_ENABLED")]
		[Conditional("DEBUG")]
		public static void Error(string msg, bool showColor = true)
		{
			if (level >= Levels.error)
			{
				if (showColor)
				{
					UnityEngine.Debug.LogError("ERROR: <color=red>" + msg + "</color>");
				}
				else
				{
					UnityEngine.Debug.LogError("ERROR: " + msg);
				}
			}
		}

		public static void Exception(Exception e)
		{
			UnityEngine.Debug.LogError("EXCEPTION: <color=red>" + e.GetType().Name + "</color> Message: " + e.Message);
		}
	}
}
