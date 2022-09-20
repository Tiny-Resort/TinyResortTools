using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace Aevus
{
	public static class EmergencyExit
	{
		private static Thread mainThread;

		private static Thread emergencyThread;

		private static bool lastESC;

		static EmergencyExit()
		{
			Start();
		}

		[RuntimeInitializeOnLoadMethod]
		private static void Start()
		{
			Application.logMessageReceived -= CatchThreadAbort;
			Application.logMessageReceived += CatchThreadAbort;
			ResetAbortThreadFlag();
			SpawnEmergencyThreadIfItDoesNotAlreadyExist();
		}

		private static void CatchThreadAbort(string condition, string stackTrace, LogType type)
		{
			if (type == LogType.Exception && condition == "ThreadAbortException")
			{
				ResetAbortThreadFlag();
			}
		}

		public static void ResetAbortThreadFlag()
		{
			if ((Thread.CurrentThread.ThreadState & (ThreadState.AbortRequested | ThreadState.Aborted)) != 0)
			{
				Thread.ResetAbort();
			}
		}

		private static void SpawnEmergencyThreadIfItDoesNotAlreadyExist()
		{
			if (mainThread == null)
			{
				mainThread = Thread.CurrentThread;
			}
			if (emergencyThread == null || !emergencyThread.IsAlive)
			{
				emergencyThread = new Thread(EmergencyTerminationThread);
				emergencyThread.Name = "Aevus Emergency Exit Thread";
				emergencyThread.Start();
			}
		}

		private static void EmergencyTerminationThread()
		{
			while (true)
			{
				if (ShowEmergencyThreadActivity())
				{
					Debug.Log("Aevus emergencyThreadId " + Thread.CurrentThread.ManagedThreadId);
				}
				if (EmergencyStopCode())
				{
					try
					{
						mainThread.Abort();
					}
					catch (Exception message)
					{
						Debug.LogError(message);
					}
				}
				Thread.Sleep(100);
			}
		}

		[DllImport("user32.dll")]
		public static extern short GetAsyncKeyState(int keycode);

		private static bool EmergencyStopCode()
		{
			bool flag = GetAsyncKeyState(16) < 0;
			bool flag2 = GetAsyncKeyState(17) < 0;
			bool flag3 = GetAsyncKeyState(81) < 0;
			bool flag4 = GetAsyncKeyState(69) < 0;
			bool flag5 = flag && flag2 && flag3;
			bool num = flag5 && !lastESC;
			lastESC = flag5;
			if (!num)
			{
				return flag2 && flag && flag4;
			}
			return true;
		}

		private static bool ShowEmergencyThreadActivity()
		{
			bool num = GetAsyncKeyState(16) < 0;
			bool flag = GetAsyncKeyState(17) < 0;
			bool flag2 = GetAsyncKeyState(72) < 0;
			return num && flag && flag2;
		}
	}
}
