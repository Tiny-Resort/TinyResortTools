using System;
using System.Diagnostics;

namespace Mirror
{
	public static class NetworkTime
	{
		public static float PingFrequency;

		public static int PingWindowSize;

		private static double lastPingTime;

		private static readonly Stopwatch stopwatch;

		private static ExponentialMovingAverage _rtt;

		private static ExponentialMovingAverage _offset;

		private static double offsetMin;

		private static double offsetMax;

		public static double localTime
		{
			get
			{
				return stopwatch.Elapsed.TotalSeconds;
			}
		}

		public static double time
		{
			get
			{
				return localTime - _offset.Value;
			}
		}

		public static double timeVariance
		{
			get
			{
				return _offset.Var;
			}
		}

		[Obsolete("NetworkTime.timeVar was renamed to timeVariance")]
		public static double timeVar
		{
			get
			{
				return timeVariance;
			}
		}

		public static double timeStandardDeviation
		{
			get
			{
				return Math.Sqrt(timeVariance);
			}
		}

		[Obsolete("NetworkTime.timeSd was renamed to timeStandardDeviation")]
		public static double timeSd
		{
			get
			{
				return timeStandardDeviation;
			}
		}

		public static double offset
		{
			get
			{
				return _offset.Value;
			}
		}

		public static double rtt
		{
			get
			{
				return _rtt.Value;
			}
		}

		public static double rttVariance
		{
			get
			{
				return _rtt.Var;
			}
		}

		[Obsolete("NetworkTime.rttVar was renamed to rttVariance")]
		public static double rttVar
		{
			get
			{
				return rttVariance;
			}
		}

		public static double rttStandardDeviation
		{
			get
			{
				return Math.Sqrt(rttVariance);
			}
		}

		[Obsolete("NetworkTime.rttSd was renamed to rttStandardDeviation")]
		public static double rttSd
		{
			get
			{
				return rttStandardDeviation;
			}
		}

		static NetworkTime()
		{
			PingFrequency = 2f;
			PingWindowSize = 10;
			stopwatch = new Stopwatch();
			_rtt = new ExponentialMovingAverage(10);
			_offset = new ExponentialMovingAverage(10);
			offsetMin = double.MinValue;
			offsetMax = double.MaxValue;
			stopwatch.Start();
		}

		public static void Reset()
		{
			stopwatch.Restart();
			_rtt = new ExponentialMovingAverage(PingWindowSize);
			_offset = new ExponentialMovingAverage(PingWindowSize);
			offsetMin = double.MinValue;
			offsetMax = double.MaxValue;
			lastPingTime = 0.0;
		}

		internal static void UpdateClient()
		{
			if (localTime - lastPingTime >= (double)PingFrequency)
			{
				NetworkClient.Send(new NetworkPingMessage(localTime), 1);
				lastPingTime = localTime;
			}
		}

		internal static void OnServerPing(NetworkConnection conn, NetworkPingMessage message)
		{
			NetworkPongMessage networkPongMessage = default(NetworkPongMessage);
			networkPongMessage.clientTime = message.clientTime;
			networkPongMessage.serverTime = localTime;
			NetworkPongMessage message2 = networkPongMessage;
			conn.Send(message2, 1);
		}

		internal static void OnClientPong(NetworkPongMessage message)
		{
			double num = localTime;
			double num2 = num - message.clientTime;
			_rtt.Add(num2);
			double num3 = num - num2 * 0.5 - message.serverTime;
			double val = num - num2 - message.serverTime;
			double val2 = num - message.serverTime;
			offsetMin = Math.Max(offsetMin, val);
			offsetMax = Math.Min(offsetMax, val2);
			if (_offset.Value < offsetMin || _offset.Value > offsetMax)
			{
				_offset = new ExponentialMovingAverage(PingWindowSize);
				_offset.Add(num3);
			}
			else if (num3 >= offsetMin || num3 <= offsetMax)
			{
				_offset.Add(num3);
			}
		}
	}
}
